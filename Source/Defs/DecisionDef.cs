using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public enum DecisionEnactmentRule
    { 
        None = 0, 
        Decree, 
        Law, 
        Referendum 
    };

    public class DecisionDef : Def
    {
        public const string CultOfPersonality = "CultOfPersonality";
        public const string Egalitarianism = "Egalitarianism";
        public const string Elitism = "Elitism";
        public const string Meritocracy = "Meritocracy";
        public const string Multiculturalism = "Multiculturalism";
        public const string ResPublica = "ResPublica";
        public const string Stability = "Stability";
        public const string StateIdeoligion = "StateIdeoligion";
        public const string StateOfEmergency = "StateOfEmergency";

        public DecisionCategoryDef category;
        public int displayPriorityInCategory;

        public Consideration displayRequirements = Consideration.always;
        public Consideration effectRequirements = Consideration.always;
        public DecisionEnactmentRule enactment = DecisionEnactmentRule.None;
        public bool allCitizensReact = true;
        public List<Consideration> considerations = new List<Consideration>();

        public string tag;
        public int durationDays;
        public int durationTicks;

        public float governanceCost;
        public int silverCostPerCitizen;
        public SuccessionDef setSuccession;
        public TermDuration setTermDuration = TermDuration.Undefined;
        public bool impeachLeader;
        public bool? actionsNeedApproval;
        public string cancelDecision;
        public float changeLoyalty;
        public float loyaltyEffect = 0.10f;

        public string LabelTitleCase => GenText.ToTitleCaseSmart(label.Formatted(new NamedArgument(Utility.RimocracyComp.Leader, "TARGET")));

        public bool IsDisplayable =>
            (!IsPersistent || !Utility.RimocracyComp.DecisionActive(Tag)) && (displayRequirements == null || displayRequirements);

        public bool IsValid =>
            IsDisplayable
            && (effectRequirements == null || effectRequirements)
            && Utility.RimocracyComp.Governance >= GovernanceCost
            && (silverCostPerCitizen <= 0 || Utility.GetMaxSilver().silver >= SilverCost);

        /// <summary>
        /// Tells if this decision tag should be stored
        /// </summary>
        public bool IsPersistent => Duration != 0 || tag != null;

        /// <summary>
        /// Returns tag (for timers) or defName by default
        /// </summary>
        public string Tag => tag.NullOrEmpty() ? defName : tag;

        public float GovernanceCost => governanceCost * Settings.GovernanceCostFactor;

        public int SilverCost => silverCostPerCitizen * Utility.CitizensCount;

        public int Duration => durationDays * GenDate.TicksPerDay + durationTicks;

        /// <summary>
        /// Returns expiration tick or MaxValue if only tag is set
        /// </summary>
        public int Expiration => Duration != 0 ? Find.TickManager.TicksAbs + Duration : (tag == null ? 0 : int.MaxValue);

        public List<Pawn> Stakeholders =>
            allCitizensReact || enactment == DecisionEnactmentRule.Law || enactment == DecisionEnactmentRule.Referendum
            ? Utility.Citizens.ToList()
            : (Utility.RimocracyComp.HasLeader ? new List<Pawn>(1) { Utility.RimocracyComp.Leader } : new List<Pawn>());

        public DecisionVoteResults GetVotingResults(List<Pawn> voters) => new DecisionVoteResults(voters.Select(pawn => new PawnDecisionOpinion(pawn, considerations, Utility.RimocracyComp.Leader)));

        public DecisionVoteResults GetVotingResults() => GetVotingResults(Stakeholders);

        public bool IsPassed(DecisionVoteResults votingResult)
        {
            switch (enactment)
            {
                case DecisionEnactmentRule.Decree:
                    return Utility.RimocracyComp.HasLeader && votingResult[Utility.RimocracyComp.Leader].Vote == DecisionVote.Yea;

                case DecisionEnactmentRule.Law:
                case DecisionEnactmentRule.Referendum:
                    return votingResult.MajoritySupport;
            }
            return true;
        }

        public bool Activate(bool cheat = false)
        {
            if (!IsValid && !cheat)
            {
                Utility.Log($"{defName} decision is invalid.", LogLevel.Warning);
                return false;
            }

            if (IsPersistent)
                Utility.RimocracyComp.Decisions.Add(new Decision(this));

            if (!cheat)
            {
                Utility.RimocracyComp.Governance -= GovernanceCost;
                if (silverCostPerCitizen > 0)
                {
                    int cost = SilverCost;
                    (Map map, int silver) = Utility.GetMaxSilver();
                    Utility.Log($"Available silver: {silver}. Cost: {cost}.");
                    Utility.RemoveSilver(map, cost);
                }
            }

            if (setSuccession != null)
            {
                Utility.Log($"Setting succession to {setSuccession}.");
                Utility.RimocracyComp.SuccessionType = setSuccession;
            }

            if (setTermDuration != TermDuration.Undefined)
            {
                Utility.Log($"Setting term duration to {setTermDuration}.");
                Utility.RimocracyComp.TermDuration = setTermDuration;
                if (!cheat)
                    Utility.RimocracyComp.TermExpiration = Math.Min(Utility.RimocracyComp.TermExpiration, Utility.RimocracyComp.UpdatedTermExpiration());
            }

            if (impeachLeader && Utility.RimocracyComp.HasLeader)
            {
                Utility.Log($"Impeaching {Utility.RimocracyComp.Leader}.");
                Utility.RimocracyComp.Leader.needs.mood.thoughts.memories.TryGainMemory(RimocracyDefOf.ImpeachedMemory);
                Utility.RimocracyComp.Leader = null;
            }

            if (actionsNeedApproval != null)
            {
                Utility.Log($"Setting ActionsNeeedApproval to {actionsNeedApproval}.");
                Utility.RimocracyComp.ActionsNeedApproval = (bool)actionsNeedApproval;
            }

            if (!cancelDecision.NullOrEmpty())
            {
                Utility.Log($"Canceling decision tag {cancelDecision}.");
                Utility.RimocracyComp.CancelDecision(cancelDecision);
            }

            if (changeLoyalty != 0)
            {
                Utility.Log($"Changing all pawns' loyalty by {changeLoyalty.ToStringPercent()}.");
                foreach (Pawn pawn in Utility.Citizens)
                    pawn.ChangeLoyalty(changeLoyalty);
            }

            return true;
        }

        public void Cancel()
        { }
    }
}
