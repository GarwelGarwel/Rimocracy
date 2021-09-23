using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public struct PawnDecisionOpinion
    {
        public Pawn voter;
        public float support;
        public string explanation;

        public DecisionVote Vote =>
            support > 0.5f ? DecisionVote.Yea : (support + voter.GetLoyaltySupportOffset() < -0.5f ? DecisionVote.Nay : (support < -0.5f ? DecisionVote.Tolerate : DecisionVote.Abstain));

        public string VoteStringColor
        {
            get
            {
                switch (Vote)
                {
                    case DecisionVote.Abstain:
                        return "abstains".Colorize(Color.gray);

                    case DecisionVote.Tolerate:
                        return "tolerates".Colorize(Color.yellow);

                    case DecisionVote.Yea:
                        return "supports".Colorize(Color.green);

                    case DecisionVote.Nay:
                        return "opposes".Colorize(Color.red);
                }
                return null;
            }
        }

        public PawnDecisionOpinion(Pawn voter, IEnumerable<Consideration> considerations, Pawn target)
        {
            this.voter = voter;
            support = 0;
            List<string> explanations = new List<string>();
            foreach (Consideration consideration in considerations)
            {
                (float support, TaggedString explanation) supportExplanation = consideration.GetSupportAndExplanation(voter, target);
                if (supportExplanation.support != 0)
                {
                    support += supportExplanation.support;
                    explanations.Add(supportExplanation.explanation.Resolve());
                }
            }
            explanations.Add($"Overall support: {Utility.ColorizeOpinion(support)}");
            if (support < -0.5f)
                explanations.Add($"Loyalty {voter.GetLoyalty().ToStringPercent()}: {Utility.ColorizeOpinion(voter.GetLoyaltySupportOffset())}");
            explanation = explanations.ToLineList();
        }
    }

    public enum DecisionVote
    {
        Abstain = 0,
        Tolerate,
        Yea,
        Nay
    }
}
