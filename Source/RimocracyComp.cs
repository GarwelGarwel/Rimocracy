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
        // How often mod enabled/disabled check, succession, governance decay etc. are updated
        const int UpdateInterval = 500;

        bool isEnabled = false;

        Pawn leader;
        LeaderTitleDef leaderTitle;
        float governance = 0.5f;
        float governanceTarget = 1;

        float regime;

        SkillDef focusSkill;
        TermDuration termDuration = TermDuration.Halfyear;
        List<ElectionCampaign> campaigns;
        int termExpiration = int.MaxValue;
        int electionTick = int.MaxValue;
        List<Decision> decisions = new List<Decision>();

        public bool IsEnabled => isEnabled;

        public Pawn Leader
        {
            get => leader;
            set => leader = value;
        }

        public LeaderTitleDef LeaderTitleDef
        {
            get => leaderTitle;
            set => leaderTitle = value;
        }

        public SuccessionType SuccessionType
        {
            get => Succession != null ? Succession.SuccessionType : SuccessionType.Undefined;
            set
            {
                switch (value)
                {
                    case SuccessionType.Election:
                        Succession = new SuccessionElection();
                        break;

                    case SuccessionType.Lot:
                        Succession = new SuccessionLot();
                        break;

                    case SuccessionType.Seniority:
                        Succession = new SuccessionOldest();
                        break;

                    case SuccessionType.Nobility:
                        Succession = new SuccessionNobility();
                        break;

                    case SuccessionType.Martial:
                        Succession = new SuccessionMartial();
                        break;

                    default:
                        Utility.Log("Succession type not set! Reverting to election.", LogLevel.Error);
                        Succession = new SuccessionElection();
                        break;
                }
            }
        }

        public SuccessionBase Succession { get; set; }

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

        public float Governance
        {
            get => governance;
            set => governance = value;
        }

        public float GovernanceTarget
        {
            get => governanceTarget;
            set => governanceTarget = value;
        }

        public float RegimeBase
        {
            get => regime;
            set => regime = value;
        }

        public float RegimeFinal =>
            Mathf.Clamp(regime + (Succession != null ? Succession.def.regimeEffect : 0) + TermDuration.GetRegimeEffect(), -1, 1);

        public TermDuration TermDuration
        {
            get => termDuration;
            set
            {
                if (termDuration != value)
                {
                    termDuration = value;
                    if (termDuration == TermDuration.Indefinite)
                        TermExpiration = int.MaxValue;
                    else TermExpiration = Math.Min(TermExpiration, Find.TickManager.TicksAbs + Utility.TermDurationTicks);
                }
            }
        }

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

        public float BaseGovernanceDecayPerDay =>
            (0.03f + governance * 0.1f - (0.06f + governance * 0.25f) / Utility.CitizensCount) * Settings.GovernanceDecaySpeed;

        public float GovernanceDecayPerDay =>
            Math.Max(0,
                BaseGovernanceDecayPerDay
                * (leader != null ? leader.GetStatValue(RimocracyDefOf.GovernanceDecay) : 1)
                * (DecisionActive("Egalitarianism") ? 1.5f - MedianMood : 1)
                * (DecisionActive("Stability") ? 0.75f : 1));

        public bool ElectionCalled => electionTick != int.MaxValue;

        internal List<Decision> Decisions
        {
            get => decisions;
            set => decisions = value;
        }

        float MedianMood => Utility.Citizens.Select(pawn => pawn.needs.mood.CurLevelPercentage).Median();

        string FocusSkillMessage => $"The focus skill is {focusSkill.LabelCap}.";

        public RimocracyComp()
            : this(Find.World)
        { }

        public RimocracyComp(World world)
            : base(world)
        { }

        public bool DecisionActive(string tag) => decisions.Any(d => d.Tag == tag);

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (decisions == null)
                decisions = new List<Decision>();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref isEnabled, "isEnabled");
            Scribe_References.Look(ref leader, "leader");
            Scribe_Defs.Look(ref leaderTitle, "leaderTitle");
            SuccessionType successionType = SuccessionType;
            Scribe_Values.Look(ref successionType, "successionType", SuccessionType.Undefined);
            SuccessionType = successionType;
            Scribe_Values.Look(ref termDuration, "termDuration", TermDuration.Quadrum, true);
            Scribe_Collections.Look(ref campaigns, "campaigns", LookMode.Deep);
            Scribe_Values.Look(ref termExpiration, "termExpiration", int.MaxValue);
            Scribe_Values.Look(ref electionTick, "electionTick", int.MaxValue);
            Scribe_Values.Look(ref governance, "governance", 0.5f);
            Scribe_Values.Look(ref governanceTarget, "governanceTarget", 1);
            Scribe_Values.Look(ref regime, "regime");
            Scribe_Defs.Look(ref focusSkill, "focusSkill");
            Scribe_Collections.Look(ref decisions, "decisions", LookMode.Deep);
        }

        public override void WorldComponentTick()
        {
            int ticks = Find.TickManager.TicksAbs;

            if (ticks % UpdateInterval != 0)
                return;

            if (Utility.CitizensCount < Settings.MinPopulation)
            {
                isEnabled = false;
                leader = null;
                governance = 0.5f;
                electionTick = int.MaxValue;
                return;
            }
            isEnabled = true;

            if (leaderTitle == null)
                ChooseLeaderTitle();

            // Remove expired or invalid decisions
            for (int i = Decisions.Count - 1; i >= 0; i--)
                if (Decisions[i].ShouldBeRemoved)
                {
                    Utility.Log($"Canceling expired or invalid decision '{Decisions[i].def.label}'.");
                    Decisions[i].def.Cancel();
                    Decisions.RemoveAt(i);
                }

            if (Succession == null || !Succession.IsValid)
                SuccessionType = SuccessionType.Election;

            if (SuccessionType == SuccessionType.Election)
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

            // Governance decay
            governance = Math.Max(governance - GovernanceDecayPerDay / GenDate.TicksPerDay * UpdateInterval, 0);
        }

        public void ImproveGovernance(float amount) => governance = Math.Min(governance + amount, 1);

        internal void CancelDecision(string tag)
        {
            foreach (Decision d in Decisions.Where(decision => decision.Tag == tag || decision.def.defName == tag))
                d.def.Cancel();
            Decisions.RemoveAll(decision => decision.Tag == tag || decision.def.defName == tag);
        }

        void ChooseLeaderTitle()
        {
            string oldLeaderTitle = leaderTitle?.defName;
            leaderTitle = Utility.ApplicableLeaderTitles.RandomElement();
            Utility.Log($"Selected leader title: {leaderTitle?.defName}.");
            if (oldLeaderTitle != leaderTitle.defName)
                Messages.Message($"Our leader is now called {leaderTitle.GetTitle(leader)}.", MessageTypeDefOf.NeutralEvent);
        }

        void CallElection()
        {
            if (DecisionActive("StateOfEmergency"))
            {
                Utility.Log("No election called because State of Emergency is active.");
                return;
            }

            electionTick = Find.TickManager.TicksAbs + Settings.CampaignDurationTicks;
            Utility.Log($"Election has been called on {GenDate.DateFullStringWithHourAt(electionTick, Find.WorldGrid.LongLatOf(Find.AnyPlayerHomeMap.Tile))}");

            // Adjust term expiration to the time of election
            if (termExpiration < int.MaxValue)
                termExpiration = electionTick;

            // Launch campaigns
            if (ElectionUtility.CampaigningEnabled)
            {
                Candidates = ((SuccessionElection)Succession).ChooseLeaders();
                Utility.Log("Candidates in the campaign: ");
                foreach (ElectionCampaign ec in campaigns)
                    Utility.Log($"- {ec}");
                Messages.Message($"The election campaign is on! {Candidates.Select(p => p.LabelShortCap).ToCommaList(true)} are competing to be the {Utility.LeaderTitle} of {Utility.NationName}.", new LookTargets(Candidates), MessageTypeDefOf.NeutralEvent);
            }
        }

        void ChooseLeader()
        {
            Pawn oldLeader = leader;
            leader = Succession.ChooseLeader();

            if (leader != null)
            {
                Utility.Log($"{leader} was chosen to be the leader.");

                if (leaderTitle == null || !leaderTitle.IsApplicable || (leader != oldLeader && Rand.Chance(0.2f)))
                    ChooseLeaderTitle();

                if (TermDuration != TermDuration.Indefinite)
                    termExpiration = Find.TickManager.TicksAbs + Utility.TermDurationTicks;
                else termExpiration = int.MaxValue;
                electionTick = int.MaxValue;
                focusSkill = leader.GetCampaign()?.FocusSkill ?? SkillsUtility.GetRandomSkill(leader.skills.skills, leader == oldLeader ? focusSkill : null);

                // Candidates gain positive or negative thoughts of the election outcome + opinion memories of each other
                if (Candidates != null)
                    foreach (Pawn p in Candidates)
                    {
                        p.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RimocracyDefOf.ElectionOutcome, p.IsLeader() ? 1 : 0));
                        foreach (Pawn p2 in Candidates.Where(p2 => p2 != p))
                            p.needs.mood.thoughts.memories.TryGainMemory(RimocracyDefOf.ElectionCompetitorMemory, p2);
                    }

                // Campaign supporters gain their thoughts too
                if (Campaigns != null)
                    foreach (ElectionCampaign campaign in Campaigns)
                    {
                        int stage = campaign.Candidate.IsLeader() ? 3 : 2;
                        foreach (Pawn p in campaign.Supporters.Where(p => p != campaign.Candidate))
                            p.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RimocracyDefOf.ElectionOutcome, stage));
                    }

                // If the leader has changed, partially reset Governance; show message
                if (leader != oldLeader)
                {
                    governance = Mathf.Lerp(DecisionActive("Stability") ? 0 : 0.5f, governance, 0.5f);
                    Find.LetterStack.ReceiveLetter(Succession.NewLeaderMessageTitle(leader), $"{Succession.NewLeaderMessageText(leader)}\n\n{FocusSkillMessage}", LetterDefOf.NeutralEvent);
                    Tale tale = TaleRecorder.RecordTale(RimocracyDefOf.BecameLeader, leader);
                    if (tale != null)
                        Utility.Log($"Tale recorded: {tale}");
                }
                else Find.LetterStack.ReceiveLetter(Succession.SameLeaderMessageTitle(leader), $"{Succession.SameLeaderMessageText(leader)}\n\n{FocusSkillMessage}", LetterDefOf.NeutralEvent);
                Utility.Log($"New leader is {leader} (chosen from {Succession.Candidates.Count()} candidates). Their term expires on {GenDate.DateFullStringAt(termExpiration, Find.WorldGrid.LongLatOf(leader.Tile))}. The focus skill is {focusSkill.defName}.");
            }
            else Utility.Log("Could not choose a new leader.", LogLevel.Warning);
            campaigns = null;
        }
    }
}
