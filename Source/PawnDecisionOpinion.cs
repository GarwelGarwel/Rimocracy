using Verse;

namespace Rimocracy
{
    public struct PawnDecisionOpinion
    {
        public Pawn voter;
        public float support;
        public string explanation;

        public DecisionVote Vote => support >= 0 ? DecisionVote.Yea : DecisionVote.Nay;

        public PawnDecisionOpinion(Pawn voter, float support, string explanation = null)
        {
            this.voter = voter;
            this.support = support;
            this.explanation = explanation;
        }
    }

    public enum DecisionVote { Abstain = 0, Yea, Nay };
}
