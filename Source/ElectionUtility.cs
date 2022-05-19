using RimWorld;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public static class ElectionUtility
    {
        public static bool CampaigningEnabled =>
            Utility.CitizensCount >= Settings.MinPopulationForCampaigning
            && Utility.RimocracyComp.SuccessionWorker.Candidates.Count() >= SuccessionWorker_Election.campaignsNumber;

        public static ElectionCampaign GetCampaign(this Pawn candidate) => Utility.RimocracyComp?.Campaigns?.FirstOrDefault(ec => ec?.Candidate == candidate);

        public static Pawn GetSupportedCandidate(this Pawn pawn) =>
            Utility.RimocracyComp?.Campaigns?.FirstOrDefault(ec => ec?.Supporters != null && ec.Supporters.Contains(pawn))?.Candidate;

        public static float VoteWeight(Pawn voter, Pawn candidate)
        {
            float weight = voter.relations.OpinionOf(candidate);

            // If the candidate is currently in a mental state, it's not good for an election. Penalty is doubled for aggressive mental states
            if (candidate.InMentalState)
                weight -= Settings.MentalStateVoteWeightPenalty * (candidate.InAggroMentalState ? 2 : 1);

            // For every backstory the two pawns have in common, a bonus is added
            int sameBackstories = voter.story.AllBackstories.Count(bs => candidate.story.AllBackstories.Contains(bs));
            if (sameBackstories > 0)
                Utility.Log($"{voter} and {candidate} have {sameBackstories.ToStringCached()} backstories in common.");
            weight += sameBackstories * Settings.SameBackstoryVoteWeightBonus;

            // If the candidate has a royal title, their vote weight is increased according to seniority
            RoyalTitleDef title = candidate.royalty?.MostSeniorTitle?.def;
            if (title != null)
            {
                Utility.Log($"{candidate} has royal title {title.label} (seniority {title.seniority.ToStringCached()}).");
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

            // If Meritocracy is in effect, sum of candidate's skills is taken into account
            if (Utility.RimocracyComp.DecisionActive(DecisionDef.Meritocracy))
            {
                float sumSkills = candidate.skills.skills.Sum(sr => sr.Level);
                Utility.Log($"{candidate} has a total level of skills of {sumSkills}, affecting Meritocracy.");
                weight += sumSkills * 0.25f;
            }

            // Adding a random factor of -5 to +5
            weight += Rand.Range(-Settings.RandomVoteWeightRadius, Settings.RandomVoteWeightRadius);

            Utility.Log($"{voter} vote weight for {candidate}: {weight:N0}.");
            return weight;
        }
    }
}
