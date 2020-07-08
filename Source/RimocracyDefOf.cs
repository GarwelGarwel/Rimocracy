using RimWorld;
using Verse;

namespace Rimocracy
{
    [DefOf]
    public static class RimocracyDefOf
    {
        public static HediffDef Enslaved = DefDatabase<HediffDef>.GetNamedSilentFail("Enslaved");

        public static JobDef Govern;

        public static RecordDef TimesElected;
        public static RecordDef VotersSwayed;
        public static RecordDef SupportersRecruited;

        public static StatDef GovernanceDecay;
        public static StatDef GovernEfficiency;
        public static StatDef GovernEfficiencyFactor;

        public static ThoughtDef ElectionOutcome;
        public static ThoughtDef ElectionCompetitorMemory;
        public static ThoughtDef PoliticalSympathy;

        public static WorkTypeDef Governing;
    }
}
