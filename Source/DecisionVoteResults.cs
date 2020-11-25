using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class DecisionVoteResults : HashSet<PawnDecisionOpinion>
    {
        public int Yea => GetVotesNum(DecisionVote.Yea);

        public int Nay => GetVotesNum(DecisionVote.Nay);

        public int Abstentions => GetVotesNum(DecisionVote.Abstain);

        public bool IsPassed => Yea > Nay;

        public string Explanations => this.Select(opinion => $"{opinion.voter}: {opinion.support.ToStringWithSign()}").ToLineList();

        public DecisionVoteResults()
        { }

        public DecisionVoteResults(IEnumerable<PawnDecisionOpinion> opinions)
            : base(opinions)
        { }

        public int GetVotesNum(DecisionVote vote) => this.Count(pdo => pdo.Vote == vote);
    }
}
