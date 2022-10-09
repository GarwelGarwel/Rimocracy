using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public enum DecisionVote
    {
        Abstain = 0,
        Tolerate,
        Yea,
        Nay
    }

    public struct PawnDecisionOpinion
    {
        public Pawn voter;
        public float support;
        public float loyaltyOffset;
        public string explanation;

        public DecisionVote Vote
        {
            get
            {
                if (support > 0.5f)
                    return DecisionVote.Yea;
                if (support < -0.5f)
                    if (support + loyaltyOffset < -0.5f)
                        return DecisionVote.Nay;
                    else return DecisionVote.Tolerate;
                return DecisionVote.Abstain;
            }
        }

        public string VoteStringColored
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
                (float support, TaggedString explanation) supportExplanation = consideration.GetValueAndExplanation(voter, target);
                if (supportExplanation.support != 0)
                {
                    support += supportExplanation.support;
                    explanations.Add(supportExplanation.explanation.Resolve());
                }
            }
            explanations.Add($"Overall support: {Utility.ColorizeOpinion(support)}");
            if (support < -0.5f)
            {
                loyaltyOffset = voter.GetLoyaltySupportOffset();
                explanations.Add($"Loyalty {voter.GetLoyaltyLevel().ToStringPercent()}: {Utility.ColorizeOpinion(loyaltyOffset)}");
            }
            else loyaltyOffset = 0;
            explanation = explanations.ToLineList();
        }
    }
}
