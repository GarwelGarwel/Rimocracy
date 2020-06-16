using Rimocracy.Succession;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class Rimocracy : WorldComponent
    {
        // Default duration of a leader's turn
        public const int DefaultTerm = GenDate.TicksPerQuadrum;

        // Delay first election succession by a day to let colonists know each other
        public const int FirstElectionDelay = GenDate.TicksPerDay;

        // Min number of colonists to enable the mod
        public const int MinColonistsRequirement = 3;

        private const int RareTicksPeriod = 600;

        bool isEnabled = false;

        Pawn leader;
        SuccessionBase succession;
        int termExpiration = -1;
        int electionTick = -1;
        float authority = 0.5f;
        SkillDef focusSkill;

        public Rimocracy()
            : this(Find.World)
        { }

        public Rimocracy(World world)
            : base(world)
        { }

        public static Rimocracy Instance => Find.World.GetComponent<Rimocracy>();

        public bool IsEnabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }

        public Pawn Leader
        {
            get => leader;
            set => leader = value;
        }

        public SuccessionBase Succession
        {
            get => succession ?? (succession = new SuccessionElection());
            set => succession = value;
        }

        public float Authority
        {
            get => authority;
            set => authority = value;
        }

        public float AuthorityPercentage => 100 * Authority;

        public int TermExpiration
        {
            get => termExpiration;
            set => termExpiration = value;
        }

        public int ElectionTick
        {
            get => electionTick;
            set => electionTick = value;
        }

        public SkillDef FocusSkill
        {
            get => focusSkill;
            set => focusSkill = value;
        }

        public float BaseAuthorityDecayPerDay
            => 0.03f + authority * 0.1f - (0.06f + authority * 0.25f) / PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count;

        public float AuthorityDecayPerDay
            => Math.Max(BaseAuthorityDecayPerDay * (leader != null ? leader.GetStatValue(DefDatabase<StatDef>.GetNamed("AuthorityDecay")) : 1), 0);

        public static bool CanBeLeader(Pawn p)
            => p != null && !p.Dead && p.IsFreeColonist && !p.WorkTypeIsDisabled(DefDatabase<WorkTypeDef>.GetNamed("Ruling"));

        public override void ExposeData()
        {
            Scribe_Values.Look(ref isEnabled, "isEnabled");
            Scribe_References.Look(ref leader, "leader");
            Scribe_Values.Look(ref termExpiration, "termExpiration", -1);
            Scribe_Values.Look(ref electionTick, "electionTick", -1);
            Scribe_Values.Look(ref authority, "authority", 0.5f);
            Scribe_Defs.Look(ref focusSkill, "focusSkill");
        }

        public override void WorldComponentTick()
        {
            int ticks = Find.TickManager.TicksAbs;

            if (ticks % RareTicksPeriod != 0)
                return;

            // If population is less than 3, temporarily disable the mod
            if (PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count < MinColonistsRequirement)
            {
                isEnabled = false;
                leader = null;
                authority = 0.5f;
                electionTick = -1;
                return;
            }
            isEnabled = true;

            // If no valid leader, initiate succession
            if (!CanBeLeader(leader) || (termExpiration >= 0 && ticks >= termExpiration))
            {
                if (Succession == null)
                {
                    Utility.Log("Succession is null!");
                    Succession = new SuccessionElection();
                }

                // Delay first election
                if (Succession is SuccessionElection && electionTick < 0)
                {
                    electionTick = ticks + FirstElectionDelay;
                    Utility.Log("Election will take place on " + GenDate.DateFullStringWithHourAt(ticks, Find.WorldGrid.LongLatOf(Find.AnyPlayerHomeMap.Tile)));
                }

                // Choose new leader
                if (ticks >= electionTick || !(Succession is SuccessionElection))
                {
                    Pawn oldLeader = leader;
                    leader = Succession.ChooseLeader();
                    if (leader != null)
                    {
                        termExpiration = ticks + DefaultTerm;
                        if (leader != oldLeader)
                        {
                            authority = Mathf.Lerp(0.5f, authority, 0.5f);
                            Find.LetterStack.ReceiveLetter(
                                Succession.NewLeaderTitle,
                                Succession.NewLeaderMessage(leader),
                                LetterDefOf.NeutralEvent);
                        }
                        else if (Succession is SuccessionElection)
                            Find.LetterStack.ReceiveLetter(
                                Succession.SameLeaderTitle,
                                Succession.SameLeaderMessage(leader),
                                LetterDefOf.NeutralEvent);
                        focusSkill = leader.skills.skills.MaxBy(sr => sr.Level).def;
                        Utility.Log("New leader is " + leader + " (chosen from " + Succession.Candidates.Count() + " candidates). Their term expires on " + GenDate.DateFullStringAt(termExpiration, Find.WorldGrid.LongLatOf(leader.Tile)) + ". The focus skill is " + focusSkill.defName);
                    }
                }
            }

            // Authority decay
            authority = Math.Max(authority - AuthorityDecayPerDay / GenDate.TicksPerDay * RareTicksPeriod, 0);
        }

        public void BuildAuthority(float amount) => authority = Math.Min(authority + amount, 1);
    }
}
