using RimWorld;
using Verse;

namespace Rimocracy
{
    public static class DefOf
    {
        public static JobDef DoRuling = DefDatabase<JobDef>.GetNamed("DoRuling");

        public static StatDef AuthorityDecay = DefDatabase<StatDef>.GetNamed("AuthorityDecay");
        public static StatDef RulingEfficiency = DefDatabase<StatDef>.GetNamed("RulingEfficiency");
        public static StatDef RulingEfficiencyFactor = DefDatabase<StatDef>.GetNamed("RulingEfficiencyFactor");

        public static ThoughtDef ElectionOutcome = DefDatabase<ThoughtDef>.GetNamed("Rimocracy_ElectionOutcome");
        public static ThoughtDef PoliticalSympathy = DefDatabase<ThoughtDef>.GetNamed("Rimocracy_PoliticalSympathy");

        public static WorkTypeDef Ruling = DefDatabase<WorkTypeDef>.GetNamed("Ruling");
    }
}
