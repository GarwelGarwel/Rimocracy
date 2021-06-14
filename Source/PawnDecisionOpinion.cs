using System;
using System.Collections.Generic;
using Verse;

namespace Rimocracy
{
    public struct PawnDecisionOpinion
    {
        public Pawn voter;
        public float support;
        public string explanation;

        public DecisionVote Vote => support > 0.5f ? DecisionVote.Yea : (support < -0.5f ? DecisionVote.Nay : DecisionVote.Abstain);

        public PawnDecisionOpinion(Pawn voter, float support, string explanation)
        {
            this.voter = voter;
            this.support = support;
            this.explanation = explanation;
        }

        public PawnDecisionOpinion(Pawn voter, IEnumerable<Consideration> considerations, Pawn target = null)
        {
            this.voter = voter;
            support = 0;
            List<string> explanations = new List<string>();
            foreach (Consideration consideration in considerations)
            {
                Tuple<float, string> supportExplanation = consideration.GetSupportAndExplanation(voter, target);
                if (supportExplanation.Item1 != 0)
                {
                    support += supportExplanation.Item1;
                    explanations.Add(supportExplanation.Item2);
                }
            }
            explanation = explanations.ToLineList();
        }
    }

    public enum DecisionVote
    {
        Abstain = 0,
        Yea,
        Nay
    }
}
