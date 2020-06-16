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

        public override string NewLeaderMessage(Pawn leader)
            => ("{PAWN_nameFullDef} has been elected as our new leader" + (votesForWinner > 1 ? " with " + votesForWinner + " votes" : (votesForWinner == 1 ? " with just one vote" : "")) + ". Vox populi, vox dei!").Formatted(leader.Named("PAWN"));

        public override string SameLeaderTitle => "Leader Reelected";

        public override string SameLeaderMessage(Pawn leader)
            => ("{PAWN_nameFullDef} has been reelected as the leader of our nation" + (votesForWinner > 1 ? " with " + votesForWinner + " votes." : (votesForWinner == 1 ? " with just one vote." : "."))).Formatted(leader.Named("PAWN"));

        public override Pawn ChooseLeader()
        {
            Dictionary<Pawn, int> votes = new Dictionary<Pawn, int>();

            // Collecting votes
            foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.FindAll(p => p.ageTracker.AgeBiologicalYears >= 16))
            {
                Pawn votedFor = Vote(p);
                if (votes.ContainsKey(votedFor))
                    votes[votedFor]++;
                else votes[votedFor] = 1;
            }

            // Logging vote tabulation
            foreach (KeyValuePair<Pawn, int> kvp in votes)
                Utility.Log("- " + kvp.Key + ": " + kvp.Value + " votes");

            // Returning the winner
            KeyValuePair<Pawn, int> winner = votes.MaxByWithFallback(kvp => kvp.Value);
            votesForWinner = winner.Value;
            return winner.Key;
        }

        float VoteWeight(Pawn voter, Pawn candidate)
        {
            float weight = voter.relations.OpinionOf(candidate);
            if (candidate.InAggroMentalState)
                weight -= 10;
            else if (candidate.InMentalState)
                weight -= 5;
            int sameBackstories = voter.story.AllBackstories.Count(bs => candidate.story.AllBackstories.Contains(bs));
            if (sameBackstories > 0)
                Utility.Log(voter.LabelShort + " and " + candidate.LabelShort + " have " + sameBackstories + " backstories in common.");
            weight += sameBackstories * 5;
            Utility.Log(voter.LabelShort + " vote weight for " + candidate.LabelShort + ": " + weight);
            return weight;
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
    }
}
