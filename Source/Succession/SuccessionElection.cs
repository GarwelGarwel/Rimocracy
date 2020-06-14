using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Rimocracy.Succession
{
    class SuccessionElection : SuccessionBase
    {
        public override string Title => "Election";

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
            return votes.MaxByWithFallback(kvp => kvp.Value).Key;
        }

        Pawn Vote(Pawn voter)
        {
            Dictionary<Pawn, float> weights = new Dictionary<Pawn, float>();
            foreach (Pawn p in Candidates)
                weights[p] = voter.relations.OpinionOf(p);
            Pawn choice = weights.MaxByWithFallback(kvp => kvp.Value).Key;
            Utility.Log(voter + " votes for " + choice);
            return choice;
        }
    }
}
