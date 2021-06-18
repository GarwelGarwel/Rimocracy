using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class DecisionVoteResults : HashSet<PawnDecisionOpinion>
    {
        public PawnDecisionOpinion this[Pawn pawn] => this.FirstOrDefault(opinion => opinion.voter == pawn);

        public int Yea => GetVotesNum(DecisionVote.Yea);

        public int Nay => GetVotesNum(DecisionVote.Nay);

        public int Abstentions => GetVotesNum(DecisionVote.Abstain);

        public bool Passed => Yea > Nay;

        public bool Vetoed => Utility.RimocracyComp.HasLeader && this[Utility.RimocracyComp.Leader].Vote == DecisionVote.Nay;

        public DecisionVoteResults()
        { }

        public DecisionVoteResults(IEnumerable<PawnDecisionOpinion> opinions)
            : base(opinions)
        { }

        public int GetVotesNum(DecisionVote vote) => this.Count(pdo => pdo.Vote == vote);

        public override string ToString() => this.Select(opinion => $"{opinion.voter}: {opinion.support.ToStringWithSign("0")}").ToLineList();
    }
}
