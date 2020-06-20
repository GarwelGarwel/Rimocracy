using Rimocracy.Succession;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class Rimocracy : WorldComponent
    {
        // Default duration of a leader's turn
        public const int DefaultTerm = GenDate.TicksPerQuadrum;

        // Campaign duration (also applies to first elections)
        public const int ElectionDelay = GenDate.TicksPerDay * 3;

        // Min number of colonists to enable the mod
        public const int MinColonistsRequirement = 3;

        // How often mod enabled/disabled check, succession, authority decay etc. are updated
        private const int RareTicksPeriod = 600;

        bool isEnabled = false;

        Pawn leader;
        SuccessionBase succession;
        List<ElectionCampaign> campaigns;
        int termExpiration = int.MaxValue;
        int electionTick = int.MaxValue;
        float authority = 0.5f;
        SkillDef focusSkill;

        public Rimocracy()
            : this(Find.World)
        { }

        public Rimocracy(World world)
            : base(world)
        { }

        public static Rimocracy Instance => Find.World?.GetComponent<Rimocracy>();

        public static bool IsEnabled => Instance != null && Instance.isEnabled;

        public bool CampaigningEnabled => true;

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

        public List<ElectionCampaign> Campaigns
        {
            get => campaigns;
            set => campaigns = value;
        }

        public IEnumerable<Pawn> Candidates
        {
            get => campaigns?.Select(c => c.Candidate);
            set
            {
                if (!value.EnumerableNullOrEmpty())
                {
                    campaigns = new List<ElectionCampaign>();
                    foreach (Pawn p in value)
                        campaigns.Add(new ElectionCampaign(p, Utility.GetRandomSkill(p.skills.skills, p.IsLeader() ? focusSkill : null)));
                }
                else campaigns = null;
            }
        }

        public ElectionCampaign GetCampaignOf(Pawn candidate) => campaigns?.FirstOrDefault(ec => ec.Candidate == candidate);

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
            => Math.Max(BaseAuthorityDecayPerDay * (leader != null ? leader.GetStatValue(DefOf.AuthorityDecay) : 1), 0);

        string FocusSkillMessage => "The focus skill is " + focusSkill.LabelCap + ".";

        public static bool CanBeLeader(Pawn p) =>
            p != null && !p.Dead && p.IsFreeColonist && !p.WorkTypeIsDisabled(DefOf.Ruling);

        public override void ExposeData()
        {
            Scribe_Values.Look(ref isEnabled, "isEnabled");
            Scribe_References.Look(ref leader, "leader");
            Scribe_Deep.Look(ref campaigns, "campaigns");
            Scribe_Values.Look(ref termExpiration, "termExpiration", int.MaxValue);
            Scribe_Values.Look(ref electionTick, "electionTick", int.MaxValue);
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

            if (Succession is SuccessionElection)
            {
                if (ticks >= termExpiration - ElectionDelay || !CanBeLeader(leader))
                    // If term is about to expire or there is no (valid) leader, call a new election
                    if (campaigns.NullOrEmpty())
                        CallElection();
                    else
                    {
                        // If at least one of the candidates is no longer eligible, campaign starts over
                        if (campaigns.Any(p => !Succession.CanBeCandidate(p.Candidate)))
                        {
                            Utility.Log("Campaign restarted because one of the candidates is ineligible.");
                            CallElection();
                        }
                        // Do campaigning stuff
                    }

                // If election is due, choose new leader
                if (ticks >= electionTick)
                    ChooseLeader();
            }

            // If no valid leader, initiate succession (non-electoral)
            else if (ticks >= termExpiration || !CanBeLeader(leader))
                ChooseLeader();

            // Authority decay
            authority = Math.Max(authority - AuthorityDecayPerDay / GenDate.TicksPerDay * RareTicksPeriod, 0);
        }

        public void BuildAuthority(float amount) => authority = Math.Min(authority + amount, 1);

        void CallElection()
        {
            electionTick = Find.TickManager.TicksAbs + ElectionDelay;
            if (CampaigningEnabled)
            {
                Candidates = ((SuccessionElection)Succession).ChooseLeaders();
                Utility.Log("Candidates in the campaign: ");
                foreach (ElectionCampaign ec in campaigns)
                    Utility.Log("- " + ec);
            }
            Utility.Log("Election has been called on " + GenDate.DateFullStringWithHourAt(electionTick, Find.WorldGrid.LongLatOf(Find.AnyPlayerHomeMap.Tile)));
        }

        void ChooseLeader()
        {
            Pawn oldLeader = leader;
            leader = Succession.ChooseLeader();
            if (leader != null)
            {
                termExpiration = Find.TickManager.TicksAbs + DefaultTerm;
                electionTick = int.MaxValue;
                focusSkill = GetCampaignOf(leader)?.FocusSkill ?? Utility.GetRandomSkill(leader.skills.skills, leader == oldLeader ? focusSkill : null);
                if (leader != oldLeader)
                {
                    authority = Mathf.Lerp(0.5f, authority, 0.5f);
                    Find.LetterStack.ReceiveLetter(Succession.NewLeaderTitle, Succession.NewLeaderMessage(leader) + "\n\n" + FocusSkillMessage, LetterDefOf.NeutralEvent);
                }
                else Find.LetterStack.ReceiveLetter(Succession.SameLeaderTitle, Succession.SameLeaderMessage(leader) + "\n\n" + FocusSkillMessage, LetterDefOf.NeutralEvent);
                Utility.Log("New leader is " + leader + " (chosen from " + Succession.Candidates.Count() + " candidates). Their term expires on " + GenDate.DateFullStringAt(termExpiration, Find.WorldGrid.LongLatOf(leader.Tile)) + ". The focus skill is " + focusSkill.defName);
            }
            campaigns = null;
        }
    }
}
