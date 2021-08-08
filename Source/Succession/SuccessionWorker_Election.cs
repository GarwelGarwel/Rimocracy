using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class SuccessionWorker_Election : SuccessionWorker
    {
        int lastVotesForWinner = 0;
        public const int campaignsNumber = 2;

        public override IEnumerable<Pawn> Candidates => Utility.RimocracyComp.Candidates ?? base.Candidates;

        string VotesNumString => lastVotesForWinner > 1 ? $"{lastVotesForWinner.ToStringCached()} votes" : "just one vote";

        public override string NewLeaderMessageText(Pawn leader) =>
            def.newLeaderMessageText.Formatted(
                Utility.NationName.Named("NATIONNAME"),
                Utility.LeaderTitle.Named("LEADERTITLE"),
                leader.Named("PAWN"),
                VotesNumString.Named("VOTESNUM"));

        public override string SameLeaderMessageText(Pawn leader) =>
            def.sameLeaderMessageText.Formatted(
                Utility.NationName.Named("NATIONNAME"),
                Utility.LeaderTitle.Named("LEADERTITLE"),
                leader.Named("PAWN"),
                VotesNumString.Named("VOTESNUM"));

        public override Pawn ChooseLeader()
        {
            Dictionary<Pawn, int> votes = GetVotes();

            if (Settings.DebugLogging)
                Utility.Log(votes.Select(kvp => $"- {kvp.Key}: {kvp.Value.ToStringCached()} votes").ToLineList());

            // Determining the winner
            KeyValuePair<Pawn, int> winner = votes.MaxByWithFallback(kvp => kvp.Value);
            winner.Key.records.Increment(RimocracyDefOf.TimesElected);
            lastVotesForWinner = winner.Value;
            return winner.Key;
        }

        /// <summary>
        /// Returns the given number of candidates with the most votes
        /// </summary>
        public IEnumerable<Pawn> ChooseLeaders(int num = campaignsNumber) => GetVotes().OrderByDescending(kvp => kvp.Value).Take(num).Select(kvp => kvp.Key);

        Dictionary<Pawn, int> GetVotes()
        {
            Dictionary<Pawn, int> votes = new Dictionary<Pawn, int>();

            foreach (Pawn p in Utility.Citizens.ToList())
            {
                Pawn votedFor = Vote(p);
                if (votedFor == null)
                    continue;
                votes.Increment(votedFor);
            }
            return votes;
        }

        Pawn Vote(Pawn voter)
        {
            Dictionary<Pawn, float> weights = new Dictionary<Pawn, float>();
            foreach (Pawn p in Candidates.Where(p => voter != p))
                weights[p] = ElectionUtility.VoteWeight(voter, p);
            Pawn choice = weights.MaxByWithFallback(kvp => kvp.Value, new KeyValuePair<Pawn, float>(null, float.MinValue)).Key;
            if (choice != null)
                Utility.Log($"{voter} votes for {choice}.");
            else Utility.Log($"{voter} has no suitable candidates to vote for.");
            return choice;
        }
    }
}
