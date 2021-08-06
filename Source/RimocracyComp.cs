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
        public const int UpdateInterval = 500;

        bool isEnabled = false;
        int updateTick = Rand.Range(0, UpdateInterval);

        Pawn leader;
        LeaderTitleDef leaderTitle;
        float governance = 0.5f;
        float governanceTarget = 1;

        float regime;

        SkillDef focusSkill;
        TermDuration termDuration = TermDuration.Halfyear;
        SuccessionDef successionType = RimocracyDefOf.Election;
        List<ElectionCampaign> campaigns;
        int termExpiration = int.MaxValue;
        int electionTick = int.MaxValue;
        List<Decision> decisions = new List<Decision>();
        bool actionsNeedApproval;

        public bool IsEnabled
        {
            get => isEnabled;
            private set => isEnabled = value;
        }

        public int UpdateTick
        {
            get => updateTick;
            set => updateTick = value;
        }

        public bool IsUpdateTick => Find.TickManager.TicksAbs % UpdateInterval == UpdateTick;

        public Pawn Leader
        {
            get => leader;
            set
            {
                if (leader == value)
                    return;
                leader = value;
                if (ModsConfig.IdeologyActive)
                    if (value != null)
                        Utility.IdeologyLeaderPrecept.Assign(value, true);
                    else Utility.IdeologyLeaderPrecept.Unassign(Find.FactionManager.OfPlayer.leader, true);
                else Find.FactionManager.OfPlayer.leader = value;
            }
        }

        public bool HasLeader => Leader != null;

        public LeaderTitleDef LeaderTitleDef
        {
            get => leaderTitle;
            set => leaderTitle = value;
        }

        public SuccessionDef SuccessionType
        {
            get
            {
                if (successionType == null)
                    successionType = RimocracyDefOf.Election;
                return successionType;
            }
            set => successionType = value;
        }

        public SuccessionWorker SuccessionWorker => SuccessionType.Worker;

        public List<ElectionCampaign> Campaigns
        {
            get => campaigns;
            set => campaigns = value;
        }

        public bool IsCampaigning => !Campaigns.NullOrEmpty();

        public IEnumerable<Pawn> Candidates
        {
            get => campaigns?.Select(c => c.Candidate);
            set
            {
                if (!value.EnumerableNullOrEmpty())
                {
                    Campaigns = new List<ElectionCampaign>();
                    foreach (Pawn p in value)
                        Campaigns.Add(new ElectionCampaign(p, SkillsUtility.GetRandomSkill(p.skills.skills, p.IsLeader() ? FocusSkill : null)));
                }
                else Campaigns = null;
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
            Mathf.Clamp(RegimeBase + (SuccessionWorker != null ? SuccessionWorker.def.regimeEffect : 0) + TermDuration.GetRegimeEffect(), -1, 1);

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
            (0.03f + Governance * 0.1f - (0.06f + Governance * 0.25f) / Utility.CitizensCount) * Settings.GovernanceDecaySpeed;

        public float GovernanceDecayPerDay =>
            Math.Max(0,
                BaseGovernanceDecayPerDay
                * (HasLeader ? Leader.GetStatValue(RimocracyDefOf.GovernanceDecay) : 1)
                * (DecisionActive("Egalitarianism") ? 1.5f - Utility.MedianMood : 1)
                * (DecisionActive("Stability") ? 0.75f : 1));

        public bool ElectionCalled => ElectionTick != int.MaxValue;

        public bool ActionsNeedApproval
        {
            get => actionsNeedApproval;
            set => actionsNeedApproval = value;
        }

        internal List<Decision> Decisions
        {
            get => decisions;
            set => decisions = value;
        }

        string FocusSkillMessage => $"The focus skill is {FocusSkill.LabelCap}.";

        public RimocracyComp()
            : this(Find.World)
        { }

        public RimocracyComp(World world)
            : base(world)
        { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (Decisions == null)
                Decisions = new List<Decision>();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref isEnabled, "isEnabled");
            Scribe_Values.Look(ref updateTick, "updateTick", Rand.Range(0, UpdateInterval), true);
            Scribe_References.Look(ref leader, "leader");
            Scribe_Defs.Look(ref leaderTitle, "leaderTitle");
            Scribe_Defs.Look(ref successionType, "successionType");
            Scribe_Values.Look(ref termDuration, "termDuration", TermDuration.Quadrum, true);
            Scribe_Collections.Look(ref campaigns, "campaigns", LookMode.Deep);
            Scribe_Values.Look(ref termExpiration, "termExpiration", int.MaxValue);
            Scribe_Values.Look(ref electionTick, "electionTick", int.MaxValue);
            Scribe_Values.Look(ref governance, "governance", 0.5f);
            Scribe_Values.Look(ref governanceTarget, "governanceTarget", 1);
            Scribe_Values.Look(ref regime, "regime");
            Scribe_Defs.Look(ref focusSkill, "focusSkill");
            Scribe_Collections.Look(ref decisions, "decisions", LookMode.Deep);
            Scribe_Values.Look(ref actionsNeedApproval, "actionsNeedApproval");
        }

        public override void WorldComponentTick()
        {
            if (!IsUpdateTick)
                return;

            if (Utility.CitizensCount < Settings.MinPopulation)
            {
                if (IsEnabled)
                {
                    IsEnabled = false;
                    Leader = null;
                    Governance = 0.5f;
                    ElectionTick = int.MaxValue;
                }
                return;
            }
            IsEnabled = true;
            int ticks = Find.TickManager.TicksAbs;

            if (!ModsConfig.IdeologyActive && LeaderTitleDef == null)
                ChooseLeaderTitle();

            // Remove expired or invalid decisions
            for (int i = Decisions.Count - 1; i >= 0; i--)
                if (Decisions[i].ShouldBeRemoved)
                {
                    Utility.Log($"Canceling expired or invalid decision '{Decisions[i].def.label}'.");
                    Decisions[i].def.Cancel();
                    Decisions.RemoveAt(i);
                }

            if (SuccessionType == null || !SuccessionWorker.IsValid)
            {
                Utility.Log($"Succession type is {SuccessionType}. SuccessionWorker is {SuccessionWorker}. Resetting to election.");
                SuccessionType = RimocracyDefOf.Election;
            }

            if (SuccessionType == RimocracyDefOf.Election)
            {
                if (ticks >= TermExpiration - Settings.CampaignDurationTicks || !Leader.CanBeLeader())
                    // If term is about to expire or there is no (valid) leader, call a new election
                    if (!ElectionCalled)
                        CallElection();
                    else if (IsCampaigning)
                    {
                        // If at least one of the candidates is no longer eligible, campaign starts over
                        ElectionCampaign invalidCampaign = Campaigns.Find(p => !SuccessionWorker.CanBeCandidate(p.Candidate));
                        if (invalidCampaign != null)
                        {
                            Utility.Log($"Campaign restarted because {invalidCampaign.Candidate} is ineligible to be a candidate.");
                            Messages.Message($"{(invalidCampaign.Candidate != null ? invalidCampaign.Candidate.NameShortColored : new TaggedString("One of the candidates"))} can't be a candidate, so the election is started over.", MessageTypeDefOf.NegativeEvent);
                            Campaigns = null;
                            CallElection();
                        }

                        foreach (ElectionCampaign campaign in Campaigns.InRandomOrder())
                            campaign.RareTick();
                    }

                // If election is due, choose new leader
                if (ticks >= ElectionTick)
                    ChooseLeader();
            }

            // If no valid leader, initiate succession (non-electoral)
            else if (ticks >= TermExpiration || !Leader.CanBeLeader())
                ChooseLeader();

            // Governance decay
            governance = Math.Max(Governance - GovernanceDecayPerDay / GenDate.TicksPerDay * UpdateInterval, 0);
        }

        public void ImproveGovernance(float amount) => Governance = Math.Min(governance + amount, 1);

        public bool DecisionActive(string tag) => Decisions.Any(d => d.Tag == tag);

        internal void CancelDecision(string tag)
        {
            foreach (Decision d in Decisions.Where(decision => decision.Tag == tag || decision.def.defName == tag))
                d.def.Cancel();
            Decisions.RemoveAll(decision => decision.Tag == tag || decision.def.defName == tag);
        }

        void ChooseLeaderTitle()
        {
            string oldLeaderTitle = LeaderTitleDef?.defName;
            LeaderTitleDef = Utility.ApplicableLeaderTitles.RandomElement();
            Utility.Log($"Selected leader title: {LeaderTitleDef?.defName}.");
            if (oldLeaderTitle != LeaderTitleDef.defName)
                Messages.Message($"Our leader is now called {LeaderTitleDef.GetTitle(Leader)}.", MessageTypeDefOf.NeutralEvent);
        }

        void CallElection()
        {
            if (DecisionActive("StateOfEmergency"))
            {
                Utility.Log("No election called because State of Emergency is active.");
                return;
            }

            ElectionTick = Find.TickManager.TicksAbs + Settings.CampaignDurationTicks;
            Utility.Log($"Election has been called on {Utility.DateFullStringWithHourAtHome(ElectionTick)}.");

            // Adjust term expiration to the time of election
            if (TermExpiration < int.MaxValue)
                TermExpiration = ElectionTick;

            // Launch campaigns
            if (ElectionUtility.CampaigningEnabled)
            {
                Candidates = ((SuccessionWorker_Election)SuccessionWorker).ChooseLeaders();
                Utility.Log($"Candidates in the campaign: {campaigns.Select(ec => $"- {ec}").ToCommaList(true, true)}");
                Messages.Message($"The election campaign is on! {Candidates.Select(p => p.LabelShortCap).ToCommaList(true)} are competing to be the {Utility.LeaderTitle} of {Utility.NationName}.", new LookTargets(Candidates), MessageTypeDefOf.NeutralEvent);
            }
        }

        void ChooseLeader()
        {
            Pawn oldLeader = Leader;
            Leader = SuccessionWorker.ChooseLeader();

            if (Leader != null)
            {
                Utility.Log($"{Leader} was chosen to be the leader.");

                if (!ModsConfig.IdeologyActive && (LeaderTitleDef == null || !LeaderTitleDef.IsApplicable || (Leader != oldLeader && Rand.Chance(0.2f))))
                    ChooseLeaderTitle();

                if (TermDuration != TermDuration.Indefinite)
                    TermExpiration = Find.TickManager.TicksAbs + Utility.TermDurationTicks;
                else TermExpiration = int.MaxValue;
                ElectionTick = int.MaxValue;
                FocusSkill = Leader.GetCampaign()?.FocusSkill ?? SkillsUtility.GetRandomSkill(Leader.skills.skills, Leader == oldLeader ? FocusSkill : null);

                // Candidates gain positive or negative thoughts of the election outcome + opinion memories of each other
                if (Candidates != null)
                    foreach (Pawn p in Candidates)
                    {
                        p.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RimocracyDefOf.ElectionOutcome, p.IsLeader() ? 1 : 0));
                        foreach (Pawn p2 in Candidates.Where(p2 => p2 != p))
                            p.needs.mood.thoughts.memories.TryGainMemory(RimocracyDefOf.ElectionCompetitorMemory, p2);
                    }

                // Campaign supporters gain their thoughts too
                if (IsCampaigning)
                    foreach (ElectionCampaign campaign in Campaigns)
                    {
                        int stage = campaign.Candidate.IsLeader() ? 3 : 2;
                        foreach (Pawn p in campaign.Supporters.Where(p => p != campaign.Candidate))
                            p.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RimocracyDefOf.ElectionOutcome, stage));
                    }

                // If the leader has changed, partially reset Governance; show message
                if (Leader != oldLeader)
                {
                    Governance = Mathf.Lerp(DecisionActive("Stability") ? 0 : 0.5f, Governance, 0.5f);
                    Find.LetterStack.ReceiveLetter(SuccessionWorker.NewLeaderMessageTitle(Leader), $"{SuccessionWorker.NewLeaderMessageText(Leader)}\n\n{FocusSkillMessage}", LetterDefOf.NeutralEvent);
                    Tale tale = TaleRecorder.RecordTale(RimocracyDefOf.BecameLeader, Leader);
                    if (tale != null)
                        Utility.Log($"Tale recorded: {tale}");
                }
                else Find.LetterStack.ReceiveLetter(SuccessionWorker.SameLeaderMessageTitle(Leader), $"{SuccessionWorker.SameLeaderMessageText(Leader)}\n\n{FocusSkillMessage}", LetterDefOf.NeutralEvent);
                Utility.Log($"New leader is {Leader} (chosen from {SuccessionWorker.Candidates.Count().ToStringCached()} candidates). Their term expires on {GenDate.DateFullStringAt(termExpiration, Find.WorldGrid.LongLatOf(leader.Tile))}. The focus skill is {focusSkill.defName}.");
            }
            else Utility.Log("Could not choose a new leader.", LogLevel.Warning);
            Campaigns = null;
        }
    }
}
