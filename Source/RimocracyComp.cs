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
        SuccessionDef successionType = RimocracyDefOf.Election;
        List<ElectionCampaign> campaigns;
        int termExpiration = int.MaxValue;
        int electionTick = int.MaxValue;
        List<Decision> decisions = new List<Decision>();

        List<Pawn> council = new List<Pawn>();

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
            Mathf.Clamp(regime + (SuccessionWorker != null ? SuccessionWorker.def.regimeEffect : 0) + TermDuration.GetRegimeEffect(), -1, 1);

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
                * (DecisionActive("Egalitarianism") ? 1.5f - Utility.GetMedianMood() : 1)
                * (DecisionActive("Stability") ? 0.75f : 1));

        public bool ElectionCalled => electionTick != int.MaxValue;

        internal List<Decision> Decisions
        {
            get => decisions;
            set => decisions = value;
        }

        public List<Pawn> Council
        {
            get => council;
            set => council = value;
        }

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
            if (council == null)
                council = new List<Pawn>();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref isEnabled, "isEnabled");
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
            Scribe_Collections.Look(ref council, "council", LookMode.Reference);
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

            if (SuccessionType == null || !SuccessionWorker.IsValid)
            {
                Utility.Log($"Succession type is {SuccessionType}. SuccessionWorker is {SuccessionWorker}. Resetting to election.");
                SuccessionType = RimocracyDefOf.Election;
            }

            if (SuccessionType == RimocracyDefOf.Election)
            {
                if (ticks >= termExpiration - Settings.CampaignDurationTicks || !leader.CanBeLeader())
                    // If term is about to expire or there is no (valid) leader, call a new election
                    if (!ElectionCalled)
                        CallElection();
                    else if (!campaigns.NullOrEmpty())
                    {
                        // If at least one of the candidates is no longer eligible, campaign starts over
                        ElectionCampaign invalidCampaign = campaigns.Find(p => !SuccessionWorker.CanBeCandidate(p.Candidate));
                        if (invalidCampaign != null)
                        {
                            Utility.Log($"Campaign restarted because {invalidCampaign.Candidate} is ineligible to be a candidate.");
                            Messages.Message($"{(invalidCampaign.Candidate.Name.ToStringShort ?? "One of the candidates")} can't be a candidate, so the election is started over.", MessageTypeDefOf.NegativeEvent);
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

            ElectionTick = Find.TickManager.TicksAbs + Settings.CampaignDurationTicks;
            Utility.Log($"Election has been called on {Utility.DateFullStringWithHourAtHome(ElectionTick)}.");

            // Adjust term expiration to the time of election
            if (TermExpiration < int.MaxValue)
                TermExpiration = ElectionTick;

            // Launch campaigns
            if (ElectionUtility.CampaigningEnabled)
            {
                Candidates = ((SuccessionWorker_Election)SuccessionWorker).ChooseLeaders();
                Utility.Log("Candidates in the campaign: ");
                foreach (ElectionCampaign ec in campaigns)
                    Utility.Log($"- {ec}");
                Messages.Message($"The election campaign is on! {Candidates.Select(p => p.LabelShortCap).ToCommaList(true)} are competing to be the {Utility.LeaderTitle} of {Utility.NationName}.", new LookTargets(Candidates), MessageTypeDefOf.NeutralEvent);
            }
        }

        void ChooseLeader()
        {
            Pawn oldLeader = leader;
            leader = SuccessionWorker.ChooseLeader();

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

                string councilMessage = "";
                Council.Clear();
                if (Utility.CitizensCount >= 4)
                {
                    Utility.Log("Choosing Council...");
                    ElectCouncil(3);
                    councilMessage = $"\n\nNew Council chosen:\n{Council.Select(pawn => pawn.NameShortColored.RawText).ToLineList("- ")}";
                }
                else Utility.Log($"Too few citizens for a council ({Utility.CitizensCount}.");

                // If the leader has changed, partially reset Governance; show message
                if (leader != oldLeader)
                {
                    governance = Mathf.Lerp(DecisionActive("Stability") ? 0 : 0.5f, governance, 0.5f);
                    Find.LetterStack.ReceiveLetter(SuccessionWorker.NewLeaderMessageTitle(leader), $"{SuccessionWorker.NewLeaderMessageText(leader)}\n\n{FocusSkillMessage}{councilMessage}", LetterDefOf.NeutralEvent);
                    Tale tale = TaleRecorder.RecordTale(RimocracyDefOf.BecameLeader, leader);
                    if (tale != null)
                        Utility.Log($"Tale recorded: {tale}");
                }
                else Find.LetterStack.ReceiveLetter(SuccessionWorker.SameLeaderMessageTitle(leader), $"{SuccessionWorker.SameLeaderMessageText(leader)}\n\n{FocusSkillMessage}{councilMessage}", LetterDefOf.NeutralEvent);
                Utility.Log($"New leader is {leader} (chosen from {SuccessionWorker.Candidates.Count()} candidates). Their term expires on {GenDate.DateFullStringAt(termExpiration, Find.WorldGrid.LongLatOf(leader.Tile))}. The focus skill is {focusSkill.defName}.");
            }
            else Utility.Log("Could not choose a new leader.", LogLevel.Warning);
            campaigns = null;
        }

        void ElectCouncil(int size)
        {
            Dictionary<Pawn, int> votes = new Dictionary<Pawn, int>();
            foreach (Pawn voter in Utility.Citizens.ToList())
            {
                Dictionary<Pawn, float> weights = new Dictionary<Pawn, float>();
                foreach (Pawn p in Utility.Citizens.Where(p => voter != p && !p.IsLeader()))
                    weights[p] = ElectionUtility.VoteWeight(voter, p);
                foreach (Pawn p in weights.OrderByDescending(kvp => kvp.Value).Take(size).Select(kvp => kvp.Key))
                {
                    Utility.Log($"{voter} votes for {p} for Council.");
                    if (votes.ContainsKey(p))
                        votes[p]++;
                    else votes[p] = 1;
                }
            }

            foreach (KeyValuePair<Pawn, int> vote in votes.OrderByDescending(kvp => kvp.Value).Take(size))
            {
                Utility.Log($"{vote.Key} elected into Council with {vote.Value} votes.");
                Council.Add(vote.Key);
            }
        }
    }
}
