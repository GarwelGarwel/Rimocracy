using RimWorld;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public static class ElectionUtility
    {
        public static bool CampaigningEnabled => Utility.Citizens.Count() >= Settings.MinPopulationForCampaigning;

        public static ElectionCampaign SupportsCampaign(this Pawn p) => Utility.RimocracyComp.Campaigns?.FirstOrDefault(ec => ec.Supporters.Contains(p));

        public static float VoteWeight(Pawn voter, Pawn candidate)
        {
            float weight = voter.relations.OpinionOf(candidate);

            // If the candidate is currently in a mental state, it's not good for an election
            if (candidate.InAggroMentalState)
                weight -= 2 * Settings.MentalStateVoteWeightPenalty;
            else if (candidate.InMentalState)
                weight -= Settings.MentalStateVoteWeightPenalty;

            // For every backstory the two pawns have in common, 10 points are added
            int sameBackstories = voter.story.AllBackstories.Count(bs => candidate.story.AllBackstories.Contains(bs));
            if (sameBackstories > 0)
                Utility.Log(voter + " and " + candidate + " have " + sameBackstories + " backstories in common.");
            weight += sameBackstories * Settings.SameBackstoryVoteWeightBonus;

            // Taking into account political sympathy (built during campaigning)
            float sympathy = voter.needs.mood.thoughts.memories.Memories
                .OfType<Thought_MemorySocial>()
                .Where(m => m.def == RimocracyDefOf.PoliticalSympathy && m.otherPawn == candidate)
                .Sum(m => m.OpinionOffset())
                * Settings.PoliticalSympathyWeightFactor;
            if (sympathy != 0)
                Utility.Log(voter + " has " + sympathy.ToString("N1") + " of sympathy for " + candidate);
            weight += sympathy;

            // Adding a random factor of -5 to +5
            weight += Rand.Range(-Settings.RandomVoteWeightRadius, Settings.RandomVoteWeightRadius);

            Utility.Log(voter + " vote weight for " + candidate + ": " + weight);
            return weight;
        }
    }
}
