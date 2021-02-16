using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public enum DecisionEnactmentRule { None = 0, Decree, Law, Referendum };

    public class DecisionDef : Def
    {
        public DecisionCategoryDef category;
        public int displayPriorityInCategory;

        public Requirement displayRequirements = Requirement.always;
        public Requirement effectRequirements = Requirement.always;
        public DecisionEnactmentRule enactment = DecisionEnactmentRule.None;
        public bool allCitizensReact = true;
        public List<Consideration> considerations = new List<Consideration>();

        public string tag;
        public int durationTicks;
        public int durationDays;

        public float governanceCost;
        public SuccessionDef setSuccession;
        public TermDuration setTermDuration = TermDuration.Undefined;
        public bool impeachLeader;
        public string cancelDecision;
        public float regimeEffect;

        public bool IsDisplayable =>
            (!IsUnique || !Utility.RimocracyComp.DecisionActive(Tag))
            && (displayRequirements == null || displayRequirements);

        public bool IsValid =>
            IsDisplayable
            && (effectRequirements == null || effectRequirements)
            && Utility.RimocracyComp.Governance >= GovernanceCost;

        /// <summary>
        /// Tells if this decision tag should be stored
        /// </summary>
        public bool IsUnique => Duration != 0 || tag != null;

        /// <summary>
        /// Returns tag (for timers) or defName by default
        /// </summary>
        public string Tag => tag.NullOrEmpty() ? defName : tag;

        public float GovernanceCost => governanceCost * Settings.GovernanceCostFactor;

        public int Duration => durationDays * GenDate.TicksPerDay + durationTicks;

        /// <summary>
        /// Returns expiration tick or MaxValue if only tag is set
        /// </summary>
        public int Expiration => Duration != 0 ? Find.TickManager.TicksAbs + Duration : (tag == null ? 0 : int.MaxValue);

        public List<Pawn> Decisionmakers
        {
            get
            {
                switch (enactment)
                {
                    case DecisionEnactmentRule.Decree:
                        Pawn leader = Utility.RimocracyComp.Leader;
                        if (leader != null)
                            return new List<Pawn>(1) { leader };
                        break;

                    case DecisionEnactmentRule.Law:
                    case DecisionEnactmentRule.Referendum:
                        return Utility.Citizens.ToList();
                }

                return new List<Pawn>();
            }
        }

        public DecisionVoteResults GetVotingResults(List<Pawn> voters) => new DecisionVoteResults(voters.Select(pawn => GetPawnOpinion(pawn)));

        public DecisionVoteResults GetVotingResults() => GetVotingResults(Decisionmakers);

        public PawnDecisionOpinion GetPawnOpinion(Pawn pawn)
        {
            float support = 0;
            List<string> explanations = new List<string>();
            foreach (Consideration consideration in considerations)
            {
                Tuple<float, string> supportExplanation = consideration.GetSupportAndExplanation(pawn);
                if (supportExplanation.Item1 != 0)
                {
                    support += supportExplanation.Item1;
                    explanations.Add(supportExplanation.Item2);
                }
            }
            return new PawnDecisionOpinion(pawn, support, explanations.ToLineList());
        }

        public bool IsPassed(DecisionVoteResults votingResult) => enactment == DecisionEnactmentRule.None || votingResult.IsPassed;

        public bool Activate()
        {
            if (!IsValid)
            {
                Utility.Log($"{defName} decision is invalid.", LogLevel.Warning);
                return false;
            }

            if (IsUnique)
                Utility.RimocracyComp.Decisions.Add(new Decision(this));

            Utility.RimocracyComp.Governance -= GovernanceCost;

            if (setSuccession != null)
            {
                Utility.Log($"Setting succession to {setSuccession}.");
                Utility.RimocracyComp.SuccessionType = setSuccession;
            }

            if (setTermDuration != TermDuration.Undefined)
            {
                Utility.Log($"Setting term duration to {setTermDuration}.");
                Utility.RimocracyComp.TermDuration = setTermDuration;
            }

            if (impeachLeader && Utility.RimocracyComp.Leader != null)
            {
                Utility.Log($"Impeaching {Utility.RimocracyComp.Leader}.");
                Utility.RimocracyComp.Leader = null;
            }

            if (!cancelDecision.NullOrEmpty())
            {
                Utility.Log($"Canceling decision tag {cancelDecision}.");
                Utility.RimocracyComp.CancelDecision(cancelDecision);
            }

            if (regimeEffect != 0)
            {
                Utility.Log($"Changing regime by {regimeEffect}.");
                Utility.RimocracyComp.RegimeBase += regimeEffect;
            }

            return true;
        }

        public void Cancel()
        {
            if (regimeEffect != 0)
            {
                Utility.Log($"Changing regime by {(-regimeEffect).ToStringWithSign()}.");
                Utility.RimocracyComp.RegimeBase -= regimeEffect;
            }
        }
    }
}
