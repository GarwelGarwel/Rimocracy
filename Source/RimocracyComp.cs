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
    public class RimocracyComp : WorldComponent
    {
        // How often mod enabled/disabled check, succession, authority decay etc. are updated
        private const int UpdateInterval = 500;

        bool isEnabled = false;

        Pawn leader;
        SuccessionBase succession;
        List<ElectionCampaign> campaigns;
        int termExpiration = int.MaxValue;
        int electionTick = int.MaxValue;
        float authority = 0.5f;
        SkillDef focusSkill;

        public RimocracyComp()
            : this(Find.World)
        { }

        public RimocracyComp(World world)
            : base(world)
        { }

        public bool IsEnabled => isEnabled;

        public Pawn Leader
        {
            get => leader;
            set => leader = value;
        }

        public SuccessionBase Succession
        {
            get
            {
                switch (Settings.SuccessionType)
                {
                    case SuccessionType.Election:
                        if (!(succession is SuccessionElection))
                            succession = new SuccessionElection();
                        break;
                    case SuccessionType.Lot:
                        if (!(succession is SuccessionLot))
                            succession = new SuccessionLot();
                        break;
                    case SuccessionType.Seniority:
                        if (!(succession is SuccessionOldest))
                            succession = new SuccessionOldest();
                        break;
                    default:
                        Utility.Log("Succession type not set! Reverting to election.", LogLevel.Error);
                        Settings.SuccessionType = SuccessionType.Election;
                        succession = new SuccessionElection();
                        break;
                }
                return succession;
            }
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
                        campaigns.Add(new ElectionCampaign(p, SkillsUtility.GetRandomSkill(p.skills.skills, p.IsLeader() ? FocusSkill : null)));
                }
                else campaigns = null;
            }
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
            => (0.03f + authority * 0.1f - (0.06f + authority * 0.25f) / Utility.Citizens.Count()) * Settings.AuthorityDecaySpeed;

        public float AuthorityDecayPerDay
            => Math.Max(BaseAuthorityDecayPerDay * (leader != null ? leader.GetStatValue(RimocracyDefOf.AuthorityDecay) : 1), 0);

        public bool ElectionCalled => electionTick != int.MaxValue;

        string FocusSkillMessage => "The focus skill is " + focusSkill.LabelCap + ".";

        public ElectionCampaign GetCampaignOf(Pawn candidate) => Campaigns?.FirstOrDefault(ec => ec.Candidate == candidate);

        public override void ExposeData()
        {
            Scribe_Values.Look(ref isEnabled, "isEnabled");
            Scribe_References.Look(ref leader, "leader");
            Scribe_Collections.Look(ref campaigns, "campaigns", LookMode.Deep);
            Scribe_Values.Look(ref termExpiration, "termExpiration", int.MaxValue);
            Scribe_Values.Look(ref electionTick, "electionTick", int.MaxValue);
            Scribe_Values.Look(ref authority, "authority", 0.5f);
            Scribe_Defs.Look(ref focusSkill, "focusSkill");
        }

        public override void WorldComponentTick()
        {
            int ticks = Find.TickManager.TicksAbs;

            if (ticks % UpdateInterval != 0)
                return;

            // If population is less than 3, temporarily disable the mod
            if (Utility.Citizens.Count() < Settings.MinPopulation)
            {
                isEnabled = false;
                leader = null;
                authority = 0.5f;
                electionTick = int.MaxValue;
                return;
            }
            isEnabled = true;

            if (Succession is SuccessionElection)
            {
                if (ticks >= termExpiration - Settings.CampaignDurationTicks || !leader.CanBeLeader())
                    // If term is about to expire or there is no (valid) leader, call a new election
                    if (!ElectionCalled)
                        CallElection();
                    else if (!campaigns.NullOrEmpty())
                    {
                        // If at least one of the candidates is no longer eligible, campaign starts over
                        if (campaigns.Any(p => !Succession.CanBeCandidate(p.Candidate)))
                        {
                            Utility.Log("Campaign restarted because one of the candidates is ineligible.");
                            campaigns = null;
                            CallElection();
                        }

                        foreach (ElectionCampaign campaign in campaigns.InRandomOrder())
                            campaign.RareTick();
                    }

                // If election is due, choose new leader
                if (ticks >= electionTick)
                    ChooseLeader();
            }

            // If no valid leader, initiate succession (non-electoral)
            else if (ticks >= termExpiration || !leader.CanBeLeader())
                ChooseLeader();

            // Authority decay
            authority = Math.Max(authority - AuthorityDecayPerDay / GenDate.TicksPerDay * UpdateInterval, 0);
        }

        public void BuildAuthority(float amount) => authority = Math.Min(authority + amount, 1);

        void CallElection()
        {
            electionTick = Find.TickManager.TicksAbs + Settings.CampaignDurationTicks;

            // Adjust term expiration to the time of election
            if (termExpiration < int.MaxValue)
                termExpiration = electionTick;

            // Launch campaigns
            if (ElectionUtility.CampaigningEnabled)
            {
                Candidates = ((SuccessionElection)Succession).ChooseLeaders();
                Utility.Log("Candidates in the campaign: ");
                foreach (ElectionCampaign ec in campaigns)
                    Utility.Log("- " + ec);
                Messages.Message("The election campaign is on! " + Utility.ListString(Candidates.Select(p => p.LabelShortCap).ToList()) + " are competing to be the leader of the nation.", new LookTargets(Candidates), MessageTypeDefOf.NeutralEvent);
            }
            Utility.Log("Election has been called on " + GenDate.DateFullStringWithHourAt(electionTick, Find.WorldGrid.LongLatOf(Find.AnyPlayerHomeMap.Tile)));
        }

        void ChooseLeader()
        {
            Pawn oldLeader = leader;
            leader = Succession.ChooseLeader();

            if (leader != null)
            {
                Utility.Log(leader + " was chosen to be the leader.");

                // Election was successful
                if (Settings.TermDuration != TermDuration.Indefinite)
                    termExpiration = Find.TickManager.TicksAbs + Settings.TermDurationTicks;
                else termExpiration = int.MaxValue;
                electionTick = int.MaxValue;
                focusSkill = GetCampaignOf(leader)?.FocusSkill ?? SkillsUtility.GetRandomSkill(leader.skills.skills, leader == oldLeader ? focusSkill : null);

                // Candidates gain positive or negative thoughts of the election outcome + opinion memories of each other
                if (Candidates != null)
                    foreach (Pawn p in Candidates)
                    {
                        p.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RimocracyDefOf.ElectionOutcome, p.IsLeader() ? 1 : 0));
                        foreach (Pawn p2 in Candidates.Where(p2 => p2 != p))
                            p.needs.mood.thoughts.memories.TryGainMemory(RimocracyDefOf.ElectionCompetitorMemory, p2);
                    }

                // If the leader has changed, partially reset Authority; show message
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
