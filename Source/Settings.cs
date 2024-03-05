using RimWorld;
using Verse;

using static Rimocracy.Utility;

namespace Rimocracy
{
    public class Settings : ModSettings
    {
        public static TechLevel MinTechLevel;
        public static int MinPopulation;
        public static int CitizenshipAge;
        public static float GovernanceDecaySpeed;
        public static int MinPopulationForCampaigning;
        public static float CampaignDurationDays;
        public static float GovernanceCostFactor;
        public static bool LoyaltyEnabled;
        public static bool ShowActionSupportDetails;
        public static bool DebugLogging = false;

        const TechLevel MinTechLevel_Default = TechLevel.Neolithic;
        const int MinPopulation_Default = 3;
        const int CitizenshipAge_Default = 16;
        const int MinPopulationForCampaigning_Default = 8;
        const float CampaignDurationDays_Default = 3;

        public const int MentalStateVoteWeightPenalty = 10;
        public const int SameBackstoryVoteWeightBonus = 20;
        public const int PoliticalSympathyWeightFactor = 25;
        public const int RandomVoteWeightRadius = 5;

        public static int CampaignDurationTicks => (int)(CampaignDurationDays * GenDate.TicksPerDay);

        public Settings() => Reset();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MinTechLevel, "MinTechLevel", MinTechLevel_Default);
            Scribe_Values.Look(ref MinPopulation, "MinPopulation", MinPopulation_Default);
            Scribe_Values.Look(ref CitizenshipAge, "CitizenshipAge", CitizenshipAge_Default);
            Scribe_Values.Look(ref GovernanceDecaySpeed, "GovernanceDecaySpeed", 1);
            Scribe_Values.Look(ref MinPopulationForCampaigning, "MinPopulationForCampaigning", MinPopulationForCampaigning_Default);
            Scribe_Values.Look(ref CampaignDurationDays, "CampaignDurationDays", CampaignDurationDays_Default);
            Scribe_Values.Look(ref GovernanceCostFactor, "GovernanceCostFactor", 1);
            Scribe_Values.Look(ref LoyaltyEnabled, "LoyaltyEnabled", true);
            Scribe_Values.Look(ref ShowActionSupportDetails, "ShowActionSupportDetails", true);
            Scribe_Values.Look(ref DebugLogging, "DebugLogging");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                Print();
        }

        public static void Reset()
        {
            MinTechLevel = MinTechLevel_Default;
            MinPopulation = MinPopulation_Default;
            CitizenshipAge = CitizenshipAge_Default;
            GovernanceDecaySpeed = 1;
            MinPopulationForCampaigning = MinPopulationForCampaigning_Default;
            CampaignDurationDays = CampaignDurationDays_Default;
            GovernanceCostFactor = 1;
            LoyaltyEnabled = true;
            ShowActionSupportDetails = true;
        }

        public static void Print()
        {
            if (!DebugLogging)
                return;
            Log($"MinTechLevel: {MinTechLevel}");
            Log($"MinPopulation: {MinPopulation.ToStringCached()}");
            Log($"CitizenshipAge: {CitizenshipAge.ToStringCached()}");
            Log($"GovernanceDecaySpeed: {GovernanceDecaySpeed:P0}");
            Log($"MinPopulationForCampaigning: {MinPopulationForCampaigning.ToStringCached()}");
            Log($"CampaignDurationDays: {CampaignDurationDays}");
            Log($"GovernanceCostFactor: {GovernanceCostFactor:P0}");
            Log($"LoyaltyEnabled: {LoyaltyEnabled}");
            Log($"ShowActionSupportDetails: {ShowActionSupportDetails}");
        }
    }
}
