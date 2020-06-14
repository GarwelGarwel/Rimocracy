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
        // Default duration of a leader's turn (1 day for testing purposes)
        public const int DefaultTerm = GenDate.TicksPerDay / 12;

        bool isEnabled = false;

        Pawn leader;
        SuccessionBase succession;
        int termExpiration = -1;
        float authority = 0.5f;
        SkillDef focusSkill;

        public Rimocracy()
            : this(Find.World)
        { }

        public Rimocracy(World world)
            : base(world)
        {
        }

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

        public SkillDef FocusSkill
        {
            get => focusSkill;
            set => focusSkill = value;
        }

        float AuthorityDecayPerTick
            => (0.05f + authority * 0.1f - (0.1f + authority * 0.2f) / PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count) / GenDate.TicksPerDay;

        public static bool CanBeLeader(Pawn p)
            => p != null && !p.Dead && p.IsFreeColonist && !p.WorkTypeIsDisabled(DefDatabase<WorkTypeDef>.GetNamed("Ruling"));

        public override void ExposeData()
        {
            Scribe_Values.Look(ref isEnabled, "isEnabled");
            Scribe_References.Look(ref leader, "leader");
            Scribe_Values.Look(ref termExpiration, "termExpiration", -1);
            Scribe_Values.Look(ref authority, "authority", 0.5f);
            Scribe_Defs.Look(ref focusSkill, "focusSkill");
        }

        public override void WorldComponentTick()
        {
            int ticks = Find.TickManager.TicksAbs;

            if (ticks % 600 != 0)
                return;

            // If population is less than 3, temporarily disable the mod
            if (PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count < 3)
            {
                isEnabled = false;
                leader = null;
                authority = 0.5f;
                return;
            }
            isEnabled = true;

            // If no valid leader, choose a new one and reset term
            if (!CanBeLeader(leader) || (termExpiration >= 0 && ticks >= termExpiration))
            {
                if (succession == null)
                {
                    Utility.Log("Succession is null!");
                    succession = new SuccessionElection();
                }
                Pawn oldLeader = leader;
                leader = succession.ChooseLeader();
                if (leader != null)
                {
                    termExpiration = ticks + DefaultTerm;
                    if (leader != oldLeader)
                        authority = Mathf.Lerp(0.5f, authority, 0.5f);
                    focusSkill = leader.skills.skills.MaxBy(sr => sr.Level).def;
                    Utility.Log("New leader is " + leader + " (chosen from " + succession.Candidates.Count() + " candidates). Their term expires on " + GenDate.DateFullStringAt(termExpiration, Find.WorldGrid.LongLatOf(leader.Tile)) + ". The focus skill is " + focusSkill.defName);
                }
            }

            // Authority decay
            float oldAuthority = authority;
            authority = Math.Max(authority - Math.Min(AuthorityDecayPerTick * (leader != null ? leader.GetStatValue(DefDatabase<StatDef>.GetNamed("AuthorityDecay")) : 1) * 600, 0.0001f), 0);
            Utility.Log("Authority decay from " + oldAuthority.ToString("P2") + " to " + authority.ToString("P2") + " for " + PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count + " colonists.");
        }

        public void BuildAuthority(float amount) => authority = Math.Min(Authority + amount, 1);
    }
}
