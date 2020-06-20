using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy.Succession
{
    public class SuccessionElection : SuccessionBase
    {
        int votesForWinner = 0;

        public override string Title => "Election";

        public override string SuccessionLabel => "election";

        public override string SameLeaderTitle => "Leader Reelected";

        public override IEnumerable<Pawn> Candidates => Rimocracy.Instance.Candidates ?? base.Candidates;

        public override string NewLeaderMessage(Pawn leader)
            => ("{PAWN_nameFullDef} has been elected as our new leader" + (votesForWinner > 1 ? " with " + votesForWinner + " votes" : (votesForWinner == 1 ? " with just one vote" : "")) + ". Vox populi, vox dei!").Formatted(leader.Named("PAWN"));

        public override string SameLeaderMessage(Pawn leader)
            => ("{PAWN_nameFullDef} has been reelected as the leader of our nation" + (votesForWinner > 1 ? " with " + votesForWinner + " votes." : (votesForWinner == 1 ? " with just one vote." : "."))).Formatted(leader.Named("PAWN"));

        public override Pawn ChooseLeader()
        {
            Dictionary<Pawn, int> votes = GetVotes();

            // Logging votes
            foreach (KeyValuePair<Pawn, int> kvp in votes)
                Utility.Log("- " + kvp.Key + ": " + kvp.Value + " votes");

            // Returning the winner
            KeyValuePair<Pawn, int> winner = votes.MaxByWithFallback(kvp => kvp.Value);
            winner.Key.records.Increment(DefDatabase<RecordDef>.GetNamed("TimesElected"));
            votesForWinner = winner.Value;
            return winner.Key;
        }

        /// <summary>
        /// Returns the given number of most candidates with the most votes;
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public IEnumerable<Pawn> ChooseLeaders(int num = 2)
            => (IEnumerable<Pawn>)GetVotes()
            .OrderByDescending(kvp => kvp.Value)
            .Take(num)
            .Select(kvp => kvp.Key);

        Dictionary<Pawn, int> GetVotes()
        {
            Dictionary<Pawn, int> votes = new Dictionary<Pawn, int>();

            foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.FindAll(p => p.ageTracker.AgeBiologicalYears >= 16))
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
                weights[p] = VoteWeight(voter, p);
            Pawn choice = weights.MaxByWithFallback(kvp => kvp.Value).Key;
            Utility.Log(voter + " votes for " + choice);
            return choice;
        }

        float VoteWeight(Pawn voter, Pawn candidate)
        {
            float weight = voter.relations.OpinionOf(candidate);
            
            // If the candidate is currently in a mental state, it's not good for an election
            if (candidate.InAggroMentalState)
                weight -= 10;
            else if (candidate.InMentalState)
                weight -= 5;

            // For every backstory the two pawns have in common, 10 points are added
            int sameBackstories = voter.story.AllBackstories.Count(bs => candidate.story.AllBackstories.Contains(bs));
            if (sameBackstories > 0)
                Utility.Log(voter.LabelShort + " and " + candidate.LabelShort + " have " + sameBackstories + " backstories in common.");
            weight += sameBackstories * 10;
            
            Utility.Log(voter.LabelShort + " vote weight for " + candidate.LabelShort + ": " + weight);
            return weight;
        }
    }
}
