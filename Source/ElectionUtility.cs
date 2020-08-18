using RimWorld;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public static class ElectionUtility
    {
        public static bool CampaigningEnabled => Utility.CitizensCount >= Settings.MinPopulationForCampaigning;

        public static ElectionCampaign SupportsCampaign(this Pawn p) => Utility.RimocracyComp.Campaigns?.FirstOrDefault(ec => ec.Supporters.Contains(p));

        public static float VoteWeight(Pawn voter, Pawn candidate)
        {
            float weight = voter.relations.OpinionOf(candidate);

            // If the candidate is currently in a mental state, it's not good for an election
            if (candidate.InMentalState)
            {
                weight -= Settings.MentalStateVoteWeightPenalty;
                // Penalty is doubled for aggressive mental states
                if (candidate.InAggroMentalState)
                    weight -= Settings.MentalStateVoteWeightPenalty;
            }

            // For every backstory the two pawns have in common, a bonus is added
            int sameBackstories = voter.story.AllBackstories.Count(bs => candidate.story.AllBackstories.Contains(bs));
            if (sameBackstories > 0)
                Utility.Log($"{voter} and {candidate} have {sameBackstories} backstories in common.");
            weight += sameBackstories * Settings.SameBackstoryVoteWeightBonus;

            // If the candidate has a royal title, their vote weight is increased according to seniority
            RoyalTitleDef title = candidate.royalty?.MostSeniorTitle?.def;
            if (title != null)
            {
                Utility.Log($"{candidate} has royal title {title.label} (seniority {title.seniority}).");
                weight += 5 + title.seniority / 10;
            }

            // Taking into account political sympathy (built during campaigning)
            float sympathy = voter.needs.mood.thoughts.memories.Memories
                .OfType<Thought_MemorySocial>()
                .Where(m => m.def == RimocracyDefOf.PoliticalSympathy && m.otherPawn == candidate)
                .Sum(m => m.OpinionOffset())
                * Settings.PoliticalSympathyWeightFactor;
            if (sympathy != 0)
                Utility.Log($"{voter} has {sympathy:N1} of sympathy for {candidate}.");
            weight += sympathy;

            // Adding a random factor of -5 to +5
            weight += Rand.Range(-Settings.RandomVoteWeightRadius, Settings.RandomVoteWeightRadius);

            Utility.Log($"{voter} vote weight for {candidate}: {weight:N0}.");
            return weight;
        }
    }
}
