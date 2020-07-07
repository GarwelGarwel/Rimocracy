using RimWorld;
using Verse;

namespace Rimocracy
{
    [DefOf]
    public static class RimocracyDefOf
    {
        public static HediffDef Enslaved = DefDatabase<HediffDef>.GetNamedSilentFail("Enslaved");

        public static JobDef DoRuling;

        public static RecordDef TimesElected;
        public static RecordDef VotersSwayed;
        public static RecordDef SupportersRecruited;

        public static StatDef AuthorityDecay;
        public static StatDef RulingEfficiency;
        public static StatDef RulingEfficiencyFactor;

        public static ThoughtDef ElectionOutcome;
        public static ThoughtDef ElectionCompetitorMemory;
        public static ThoughtDef PoliticalSympathy;

        public static WorkTypeDef Ruling;
    }
}
