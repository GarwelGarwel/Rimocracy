using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

using static Rimocracy.Utility;

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

        SkillDef focusSkill;
        TermDuration termDuration = TermDuration.Halfyear;
        SuccessionDef successionType;
        List<ElectionCampaign> campaigns;
        int termExpiration = int.MaxValue;
        int electionTick = int.MaxValue;
        List<Decision> decisions = new List<Decision>();
        bool actionsNeedApproval;
        List<Pawn> protesters = new List<Pawn>();

        bool justLoaded = true;

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
                if (ModsConfig.IdeologyActive && !DecisionActive(DecisionDef.Multiculturalism))
                    if (value != null)
                        IdeologyLeaderPrecept().Assign(value, true);
                    else IdeologyLeaderPrecept().Unassign(Find.FactionManager.OfPlayer.leader, true);
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
                    successionType = GetRandomSuccessionDef(NationPrimaryIdeo);
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

        public IEnumerable<Pawn> CampaigningCandidates
        {
            get => campaigns?.Select(ec => ec.Candidate);
            set
            {
                if (!value.EnumerableNullOrEmpty())
                {
                    Campaigns = new List<ElectionCampaign>();
                    foreach (Pawn pawn in value)
                        Campaigns.Add(new ElectionCampaign(pawn, SkillsUtility.GetRandomSkill(pawn.skills.skills, pawn.IsLeader() ? FocusSkill : null)));
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

        public TermDuration TermDuration
        {
            get => termDuration;
            set => termDuration = value;
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
            Settings.GovernanceDecaySpeed *
            (0.03f + Governance * 0.1f - (0.06f + Governance * 0.25f) / Citizens.Sum(pawn => CitizenGovernanceWeight(pawn)));

        public float GovernanceDecayPerDay =>
            Math.Max(0,
                BaseGovernanceDecayPerDay
                * (HasLeader ? Leader.GetStatValue(RimocracyDefOf.GovernanceDecay) : 1)
                * (DecisionActive(DecisionDef.Egalitarianism) ? 1.5f - MedianMood : 1)
                * (DecisionActive(DecisionDef.Stability) ? 0.75f : 1));

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

        internal List<Pawn> Protesters
        {
            get => protesters;
            set => protesters = value;
        }

        string FocusSkillMessage => $"The focus skill is {FocusSkill.LabelCap}.";

        public RimocracyComp()
            : this(Find.World)
        { }

        public RimocracyComp(World world)
            : base(world)
        { }

        public SuccessionDef GetRandomSuccessionDef(Ideo ideo) =>
            DefDatabase<SuccessionDef>.AllDefs.Where(def => def.Worker.IsValid).RandomElementByWeight(def => def.GetWeight(ideo));

        public int UpdatedTermExpiration() =>
            TermDuration == TermDuration.Indefinite ? int.MaxValue : Find.TickManager.TicksAbs + TermDurationTicks;

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (Decisions == null)
                Decisions = new List<Decision>();
            else Decisions.RemoveAll(d => d.def == null);
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
            Scribe_Defs.Look(ref focusSkill, "focusSkill");
            Scribe_Collections.Look(ref decisions, "decisions", LookMode.Deep);
            Scribe_Values.Look(ref actionsNeedApproval, "actionsNeedApproval");
            Scribe_Collections.Look(ref protesters, "protesters", LookMode.Reference);
        }

        public override void WorldComponentTick()
        {
            if (justLoaded)
            {
                justLoaded = false;
                if (Settings.DebugLogging || Prefs.LogVerbose)
                {
                    Log($"Politics: {(IsEnabled ? "enabled" : "disabled")}");
                    Log($"Leader: {(HasLeader ? Leader.Name.ToStringShort : "none")}");
                    Log($"Succession: {SuccessionType.defName} @ {TermExpiration} (in {(TermExpiration - Find.TickManager.TicksAbs).ToStringTicksToPeriod(false, true)})");
                    Log($"Election tick: {ElectionTick} (in {(ElectionTick - Find.TickManager.TicksAbs).ToStringTicksToPeriod(false, true)})");
                    Log($"Term duration: {TermDuration}");
                    if (IsCampaigning)
                        Log($"Campaigns:\r\n{Campaigns.Select(campaign => $"- {campaign}").ToLineList()}");
                    Log($"Governance: {Governance.ToStringPercent()}");
                    Log($"Governance decay: {GovernanceDecayPerDay.ToStringPercent()}/day");
                    Log($"Focus skill: {FocusSkill}");
                    Log($"Decisions: {Decisions.Select(decision => decision?.Tag).ToCommaList()}");
                    Log($"Protesters: {Protesters.Select(pawn => pawn.Name.ToStringShort).ToCommaList()}");
                }
            }

            if (!IsUpdateTick)
                return;

            if (CitizensCount < Settings.MinPopulation || (!HasLeader && !Citizens.Any(pawn => pawn.CanBeLeader())))
            {
                // If there are too few citizens or no potential leaders, politics is disabled
                if (IsEnabled)
                {
                    IsEnabled = false;
                    Leader = null;
                    Governance = 0.50f;
                    ElectionTick = int.MaxValue;
                    Protesters.Clear();
                }
                return;
            }
            IsEnabled = true;
            int ticks = Find.TickManager.TicksAbs;

            if ((!ModsConfig.IdeologyActive || DecisionActive(DecisionDef.Multiculturalism)) && LeaderTitleDef == null)
                ChooseLeaderTitle();

            // Clean up protesters list
            if (Settings.LoyaltyEnabled)
            {
                int oldProtestersCount = protesters.Count;
                for (int i = oldProtestersCount - 1; i >= 0; i--)
                {
                    if (protesters[i] == null || !protesters[i].IsCitizen())
                    {
                        Log($"Removing invalid protester record {(protesters[i]?.Name.ToStringShort ?? "null")} (index {i}).");
                        protesters.RemoveAt(i);
                        continue;
                    }
                    Need_Loyalty loyalty = protesters[i].GetLoyalty();
                    if (loyalty == null || !loyalty.IsProtesting)
                    {
                        Log($"Removing invalid protester {protesters[i]} (loyalty {protesters[i].GetLoyaltyLevel().ToStringPercent()}.");
                        protesters.RemoveAt(i);
                    }
                }
                if (protesters.Count != oldProtestersCount)
                    Need_Loyalty.RecalculateThreshPercents();
            }
            else protesters.Clear();

            // Remove expired or invalid decisions
            for (int i = Decisions.Count - 1; i >= 0; i--)
                if (Decisions[i].ShouldBeRemoved)
                {
                    Log($"Canceling expired or invalid decision '{Decisions[i].def.label}'.");
                    Decisions[i].def.Cancel();
                    Decisions.RemoveAt(i);
                }

            if (SuccessionType == RimocracyDefOf.Election)
            {
                if (ticks >= TermExpiration - Settings.CampaignDurationTicks || !Leader.CanBeLeader())
                    // If term is about to expire or there is no (valid) leader, call a new election
                    if (!ElectionCalled)
                        CallElection();
                    else if (IsCampaigning)
                    {
                        // If at least one of the candidates is no longer eligible, the entire campaign starts over
                        if (Campaigns.Any(campaign => campaign == null || !SuccessionWorker.CanBeCandidate(campaign.Candidate)))
                        {
                            Log($"Campaign restarted because a candidate is ineligible.");
                            Messages.Message($"One or more candidates are ineligible, so the election is starting over.", MessageTypeDefOf.NegativeEvent);
                            Campaigns = null;
                            CallElection();
                        }

                        int offset = Rand.Range(0, Campaigns.Count);
                        for (int i = 0; i < Campaigns.Count; i++)
                            Campaigns[(i + offset) % Campaigns.Count].RareTick();
                    }

                // If election is due, choose new leader
                if (ticks >= ElectionTick)
                    ChooseLeader();
            }

            // If no valid leader, initiate succession (non-electoral)
            else if (ticks >= TermExpiration || !Leader.CanBeLeader())
                ChooseLeader();

            // Governance decay
            ChangeGovernance(-GovernanceDecayPerDay / GenDate.TicksPerDay * UpdateInterval);
        }

        public void ChangeGovernance(float amount) => Governance = Mathf.Clamp01(Governance + amount);

        public bool DecisionActive(string tag)
        {
            for (int i = 0; i < Decisions.Count; i++)
                if (Decisions[i].Tag == tag)
                    return true;
            return false;
        }

        internal void CancelDecision(string tag)
        {
            for (int i = Decisions.Count - 1; i >= 0; i--)
                if (Decisions[i].Tag == tag || Decisions[i].def.defName == tag)
                {
                    Decisions[i].def.Cancel();
                    Decisions.RemoveAt(i);
                }
        }

        void ChooseLeaderTitle()
        {
            string oldLeaderTitle = LeaderTitleDef?.defName;
            LeaderTitleDef = ApplicableLeaderTitles.RandomElement();
            Log($"Selected leader title: {LeaderTitleDef?.defName}.");
            if (oldLeaderTitle != LeaderTitleDef.defName)
                Messages.Message($"Our leader is now called {LeaderTitleDef.GetTitle(Leader)}.", MessageTypeDefOf.NeutralEvent);
        }

        void CallElection()
        {
            if (DecisionActive(DecisionDef.StateOfEmergency))
            {
                Log("No election called because State of Emergency is active.");
                return;
            }

            ElectionTick = Find.TickManager.TicksAbs + Settings.CampaignDurationTicks;
            Log($"Election has been called on {DateFullStringWithHourAtHome(ElectionTick)}.");

            // Adjust term expiration to the time of election
            if (TermExpiration < int.MaxValue)
                TermExpiration = ElectionTick;

            // Launch campaigns
            if (ElectionUtility.CampaigningEnabled)
            {
                CampaigningCandidates = ((SuccessionWorker_Election)SuccessionWorker).ChooseLeaders();
                Log($"Campaigns:\n{Campaigns.Select(campaign => $"- {campaign.ToString()}").ToLineList()}");
                Messages.Message($"The election campaign is on! {CampaigningCandidates.Select(p => p.NameShortColored.RawText).ToCommaList(true)} are competing to be the {LeaderTitle} of {NationName}.",
                    new LookTargets(CampaigningCandidates),
                    MessageTypeDefOf.NeutralEvent);
            }
        }

        void ChooseLeader()
        {
            Pawn oldLeader = Leader;
            Leader = SuccessionWorker.ChooseLeader();

            if (HasLeader)
            {
                Log($"{Leader} was chosen to be the leader.");

                // Chance to choose a new leader title
                if ((!ModsConfig.IdeologyActive || DecisionActive(DecisionDef.Multiculturalism))
                    && (LeaderTitleDef == null || !LeaderTitleDef.IsApplicable || (Leader != oldLeader && Rand.Chance(0.2f))))
                    ChooseLeaderTitle();

                TermExpiration = UpdatedTermExpiration();
                ElectionTick = int.MaxValue;
                FocusSkill = Leader.GetCampaign()?.FocusSkill ?? SkillsUtility.GetRandomSkill(Leader.skills.skills, Leader == oldLeader ? FocusSkill : null);
                Log($"New leader is {Leader} (chosen from {SuccessionWorker.Candidates.Count().ToStringCached()} candidates). Their term expires on {GenDate.DateFullStringAt(TermExpiration, Find.WorldGrid.LongLatOf(Leader.Tile))}. The focus skill is {FocusSkill.defName}.");

                // Campaigning candidates and their supporters gain their thoughts
                if (IsCampaigning)
                    foreach (ElectionCampaign campaign in Campaigns)
                    {
                        campaign.Candidate.needs.mood.thoughts.memories.TryGainMemory(
                            ThoughtMaker.MakeThought(RimocracyDefOf.ElectionOutcome,
                            campaign.Candidate.IsLeader() ? 1 : 0));
                        foreach (Pawn p2 in CampaigningCandidates.Where(p2 => p2 != campaign.Candidate))
                            campaign.Candidate.needs.mood.thoughts.memories.TryGainMemory(RimocracyDefOf.ElectionCompetitorMemory, p2);
                        int stage = campaign.Candidate.IsLeader() ? 3 : 2;
                        foreach (Pawn p in campaign.Supporters.Where(p => p != campaign.Candidate))
                            p.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RimocracyDefOf.ElectionOutcome, stage));
                    }

                // If the leader has changed, partially reset Governance and loyalty; show message; record tale
                if (Leader != oldLeader)
                {
                    Governance = Mathf.Lerp(DecisionActive(DecisionDef.Stability) ? 0 : 0.5f, Governance, 0.5f);
                    foreach (Pawn pawn in Citizens)
                        pawn.ChangeLoyalty((Need_Loyalty.DefaultLevel - pawn.GetLoyaltyLevel()) * Need_Loyalty.LoyaltyResetOnLeaderChange * (DecisionActive(DecisionDef.Stability) ? 2 : 1));
                    Find.LetterStack.ReceiveLetter(SuccessionWorker.NewLeaderMessageTitle(Leader), $"{SuccessionWorker.NewLeaderMessageText(Leader)}\n\n{FocusSkillMessage}", LetterDefOf.NeutralEvent);
                    Tale tale = TaleRecorder.RecordTale(RimocracyDefOf.BecameLeader, Leader);
                }
                else Find.LetterStack.ReceiveLetter(SuccessionWorker.SameLeaderMessageTitle(Leader), $"{SuccessionWorker.SameLeaderMessageText(Leader)}\n\n{FocusSkillMessage}", LetterDefOf.NeutralEvent);

                // Apply succession's loyalty effect
                float loyaltyEffect = SuccessionWorker.LoyaltyEffect;
                if (loyaltyEffect != 0)
                {
                    Log($"All citizens gain {loyaltyEffect:N0} loyalty due to the succession.");
                    foreach (Pawn pawn in Citizens)
                        pawn.ChangeLoyalty(loyaltyEffect);
                }
            }
            else Log("Could not choose a new leader.", LogLevel.Warning);
            Campaigns = null;
        }
    }
}
