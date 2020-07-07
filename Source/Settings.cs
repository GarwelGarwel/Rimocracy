using RimWorld;
using Verse;

namespace Rimocracy
{
    public enum TermDuration { Indefinite = 0, Quadrum, Halfyear, Year };

    public class Settings : ModSettings
    {
        public static int MinPopulation = MinPopulation_Default;
        public static int CitizenshipAge = CitizenshipAge_Default;
        public static float GovernanceDecaySpeed = 1;
        public static TermDuration TermDuration = TermDuration.Quadrum;
        public static SuccessionType SuccessionType = SuccessionType.Election;
        public static int MinPopulationForCampaigning = MinPopulationForCampaigning_Default;
        public static float CampaignDurationDays = CampaignDurationDays_Default;
        public static int MentalStateVoteWeightPenalty = MentalStateVoteWeightPenalty_Default;
        public static int SameBackstoryVoteWeightBonus = SameBackstoryVoteWeightBonus_Default;
        public static int PoliticalSympathyWeightFactor = PoliticalSympathyWeightFactor_Default;
        public static int RandomVoteWeightRadius = RandomVoteWeightRadius_Default;
        public static float SwayChanceFactor = 1;
        public static float RecruitmentChanceFactor = 1;
        public static bool DebugLogging = true;

        const int MinPopulation_Default = 3;
        const int CitizenshipAge_Default = 16;
        const int MinPopulationForCampaigning_Default = 6;
        const float CampaignDurationDays_Default = 3;
        const int MentalStateVoteWeightPenalty_Default = 10;
        const int SameBackstoryVoteWeightBonus_Default = 20;
        const int PoliticalSympathyWeightFactor_Default = 25;
        const int RandomVoteWeightRadius_Default = 5;

        public static int TermDurationTicks
        {
            get
            {
                switch (TermDuration)
                {
                    case TermDuration.Quadrum:
                        return GenDate.TicksPerQuadrum;

                    case TermDuration.Halfyear:
                        return GenDate.TicksPerYear / 2;

                    case TermDuration.Year:
                        return GenDate.TicksPerYear;

                    default:
                        return int.MaxValue;
                }
            }
        }

        public static int CampaignDurationTicks => (int)(CampaignDurationDays * GenDate.TicksPerDay);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MinPopulation, "MinPopulation", MinPopulation_Default);
            Scribe_Values.Look(ref CitizenshipAge, "CitizenshipAge", CitizenshipAge_Default);
            Scribe_Values.Look(ref GovernanceDecaySpeed, "GovernanceDecaySpeed", 1);
            Scribe_Values.Look(ref TermDuration, "TermDuration", TermDuration.Quadrum);
            Scribe_Values.Look(ref SuccessionType, "SuccessionType");
            Scribe_Values.Look(ref MinPopulationForCampaigning, "MinPopulationForCampaigning", MinPopulationForCampaigning_Default);
            Scribe_Values.Look(ref CampaignDurationDays, "CampaignDurationDays", CampaignDurationDays_Default);
            Scribe_Values.Look(ref MentalStateVoteWeightPenalty, "MentalStateVoteWeightPenalty", MentalStateVoteWeightPenalty_Default);
            Scribe_Values.Look(ref SameBackstoryVoteWeightBonus, "SameBackstoryVoteWeightBonus", SameBackstoryVoteWeightBonus_Default);
            Scribe_Values.Look(ref PoliticalSympathyWeightFactor, "PoliticalSympathyWeightFactor", PoliticalSympathyWeightFactor_Default);
            Scribe_Values.Look(ref RandomVoteWeightRadius, "RandomVoteWeightRadius", RandomVoteWeightRadius_Default);
            Scribe_Values.Look(ref SwayChanceFactor, "SwayChanceFactor", 1);
            Scribe_Values.Look(ref RecruitmentChanceFactor, "RecruitmentChanceFactor", 1);
            Scribe_Values.Look(ref DebugLogging, "DebugLogging");
        }
    }
}
