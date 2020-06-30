using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    enum LogLevel { Message = 0, Warning, Error };

    public static class Utility
    {
        // Min number of colonists to enable the mod
        public const int MinColonistsRequirement = 3;

        // How much vote weight every point of political sympathy translates to
        const float PoliticalSympathyWeightFactor = 25;

        static bool? simpleSlaveryInstalled = null;

        public static Rimocracy Rimocracy => Find.World?.GetComponent<Rimocracy>();

        public static bool PoliticsEnabled => Rimocracy != null && Rimocracy.IsEnabled;

        public static bool CampaigningEnabled => Citizens.Count() >= 6;

        public static bool IsSimpleSlaveryInstalled => (bool)(simpleSlaveryInstalled ?? (simpleSlaveryInstalled = DefDatabase<HediffDef>.GetNamedSilentFail("Enslaved") != null));

        public static IEnumerable<Pawn> Citizens => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep
                    .Where(p => p.IsCitizen());

        /// <summary>
        /// Returns a list of skills that affect the given stat
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public static List<SkillDef> GetSkills(StatDef stat)
        {
            List<SkillDef> skills = stat.skillNeedFactors.NullOrEmpty()
                ? new List<SkillDef>()
                : stat.skillNeedFactors.Select(sn => sn.skill).ToList();
            if (!stat.skillNeedOffsets.NullOrEmpty())
                skills.AddRange(stat.skillNeedOffsets.Select(sn => sn.skill));
            return skills;
        }

        /// <summary>
        /// Returns a skill from the list weighed by squares of skill levels + 1 (except those completely disabled)
        /// </summary>
        /// <param name="skills">List of SkillRecords of the pawn</param>
        /// <param name="luckySkill">SkillDef whose chances are doubled</param>
        /// <returns></returns>
        public static SkillDef GetRandomSkill(List<SkillRecord> skills, SkillDef luckySkill)
            => skills
            .Where(sr => !sr.TotallyDisabled)
            .Select(sr => new KeyValuePair<SkillDef, float>(sr.def, Rand.Range(0, (sr.Level + 1) * (sr.def == luckySkill ? 2 : 1))))
            .MaxBy(kvp => kvp.Value)
            .Key;

        public static bool IsCitizen(this Pawn pawn)
            => pawn != null
            && !pawn.Dead
            && pawn.IsFreeColonist
            && pawn.ageTracker.AgeBiologicalYears >= 16
            && (!IsSimpleSlaveryInstalled || !pawn.health.hediffSet.hediffs.Any(hediff => hediff.def.defName == "Enslaved"));

        public static bool CanBeLeader(this Pawn p) => p.IsCitizen() && !p.GetDisabledWorkTypes(true).Contains(RimocracyDefOf.Ruling);

        public static bool IsLeader(this Pawn p) => PoliticsEnabled && Rimocracy.Leader == p;

        public static ElectionCampaign SupportsCampaign(this Pawn p)
            => Rimocracy.Campaigns?.FirstOrDefault(ec => ec.Supporters.Contains(p));

        public static float VoteWeight(Pawn voter, Pawn candidate)
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
                Log(voter + " and " + candidate + " have " + sameBackstories + " backstories in common.");
            weight += sameBackstories * 10;

            // Taking into account political sympathy (built during campaigning)
            float sympathy = voter.needs.mood.thoughts.memories.Memories
                .OfType<Thought_MemorySocial>()
                .Where(m => m.def == RimocracyDefOf.PoliticalSympathy && m.otherPawn == candidate)
                .Sum(m => m.OpinionOffset())
                * PoliticalSympathyWeightFactor;
            if (sympathy != 0)
                Log(voter + " has " + sympathy.ToString("N1") + " of sympathy for " + candidate);
            weight += sympathy;

            // Adding a random factor of -5 to +5
            weight += Rand.Range(-5, 5);

            Log(voter + " vote weight for " + candidate + ": " + weight);
            return weight;
        }

        internal static void Log(string message, LogLevel logLevel = LogLevel.Message)
        {
            message = "[Rimocracy] " + message;
            switch (logLevel)
            {
                case LogLevel.Message:
                    Verse.Log.Message(message);
                    break;

                case LogLevel.Warning:
                    Verse.Log.Warning(message);
                    break;

                case LogLevel.Error:
                    Verse.Log.Error(message);
                    break;
            }
        }
    }
}
