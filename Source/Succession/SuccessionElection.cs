using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy.Succession
{
    public class SuccessionElection : SuccessionBase
    {
        int votesForWinner = 0;

        public override string Title => "Election";

        public override SuccessionType SuccessionType => SuccessionType.Election;

        public override string SuccessionLabel => "election";

        public override string SameLeaderTitle => "Leader Reelected";

        public override IEnumerable<Pawn> Candidates => Utility.Rimocracy.Candidates ?? base.Candidates;

        public override string NewLeaderMessage(Pawn leader) => 
            ("{PAWN_nameFullDef} has been elected as our new leader with " + (votesForWinner != 1 ? votesForWinner + " votes" : "just one vote") + ". Vox populi, vox dei!").Formatted(leader.Named("PAWN"));

        public override string SameLeaderMessage(Pawn leader) => 
            ("{PAWN_nameFullDef} has been reelected as the leader of our nation" + (votesForWinner > 1 ? " with " + votesForWinner + " votes." : (votesForWinner == 1 ? " with just one vote." : "."))).Formatted(leader.Named("PAWN"));

        public override Pawn ChooseLeader()
        {
            Dictionary<Pawn, int> votes = GetVotes();

            // Logging votes
            foreach (KeyValuePair<Pawn, int> kvp in votes)
                Utility.Log("- " + kvp.Key + ": " + kvp.Value + " votes");

            // Returning the winner
            KeyValuePair<Pawn, int> winner = votes.MaxByWithFallback(kvp => kvp.Value);
            winner.Key.records.Increment(RimocracyDefOf.TimesElected);
            votesForWinner = winner.Value;
            return winner.Key;
        }

        /// <summary>
        /// Returns the given number of most candidates with the most votes;
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public IEnumerable<Pawn> ChooseLeaders(int num = 2) => GetVotes().OrderByDescending(kvp => kvp.Value).Take(num).Select(kvp => kvp.Key);

        Dictionary<Pawn, int> GetVotes()
        {
            Dictionary<Pawn, int> votes = new Dictionary<Pawn, int>();

            foreach (Pawn p in Utility.Citizens.Where(p => !p.Dead).ToList())
            {
                Pawn votedFor = Vote(p);
                if (votes.ContainsKey(votedFor))
                    votes[votedFor]++;
                else votes[votedFor] = 1;
            }
            return votes;
        }

        Pawn Vote(Pawn voter)
        {
            Dictionary<Pawn, float> weights = new Dictionary<Pawn, float>();
            foreach (Pawn p in Candidates.Where(p => voter != p))
                weights[p] = ElectionUtility.VoteWeight(voter, p);
            Pawn choice = weights.MaxByWithFallback(kvp => kvp.Value).Key;
            Utility.Log(voter + " votes for " + choice);
            return choice;
        }
    }
}
