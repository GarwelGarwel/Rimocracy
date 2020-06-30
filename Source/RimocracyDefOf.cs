using RimWorld;
using Verse;

namespace Rimocracy
{
    [DefOf]
    public static class RimocracyDefOf
    {
        public static JobDef DoRuling;

        public static StatDef AuthorityDecay;
        public static StatDef RulingEfficiency;
        public static StatDef RulingEfficiencyFactor;

        public static ThoughtDef ElectionOutcome;
        public static ThoughtDef ElectionCompetitorMemory;
        public static ThoughtDef PoliticalSympathy;

        public static WorkTypeDef Ruling;
    }
}
