﻿using RimWorld;
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
        public SuccessionDef setSuccession;
        public TermDuration setTermDuration = TermDuration.Undefined;
        public bool impeachLeader;
        public bool? actionsNeedApproval;
        public string cancelDecision;
        public float regimeEffect;

        public string LabelTitleCase => GenText.ToTitleCaseSmart(label.Formatted(new NamedArgument(Utility.RimocracyComp.Leader, "TARGET")));

        public bool IsDisplayable =>
            (!IsPersistent || !Utility.RimocracyComp.DecisionActive(Tag)) && (displayRequirements == null || displayRequirements);

        public bool IsValid =>
            IsDisplayable && (effectRequirements == null || effectRequirements) && Utility.RimocracyComp.Governance >= GovernanceCost;

        /// <summary>
        /// Tells if this decision tag should be stored
        /// </summary>
        public bool IsPersistent => Duration != 0 || tag != null;

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

        public DecisionVoteResults GetVotingResults(List<Pawn> voters) => new DecisionVoteResults(voters.Select(pawn => new PawnDecisionOpinion(pawn, considerations, Utility.RimocracyComp.Leader)));

        public DecisionVoteResults GetVotingResults() => GetVotingResults(Decisionmakers);

        public bool IsPassed(DecisionVoteResults votingResult) => enactment == DecisionEnactmentRule.None || votingResult.Passed;

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
                Utility.RimocracyComp.Governance -= GovernanceCost;

            if (setSuccession != null)
            {
                Utility.Log($"Setting succession to {setSuccession}.");
                Utility.RimocracyComp.SuccessionType = setSuccession;
            }

            if (setTermDuration != TermDuration.Undefined && !cheat)
            {
                Utility.Log($"Setting term duration to {setTermDuration}.");
                Utility.RimocracyComp.TermDuration = setTermDuration;
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
