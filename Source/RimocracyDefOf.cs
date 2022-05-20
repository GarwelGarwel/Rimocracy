using RimWorld;
using Verse;

namespace Rimocracy
{
    [DefOf]
    public static class RimocracyDefOf
    {
        public static DecisionDef Impeachment;

        internal static HediffDef Enslaved = DefDatabase<HediffDef>.GetNamedSilentFail("Enslaved");

        public static JobDef Govern;

        public static NeedDef Rimocracy_Loyalty;

        public static PoliticalActionDef Arrest;
        public static PoliticalActionDef Execution;
        public static PoliticalActionDef Release;
        public static PoliticalActionDef Banishment;
        public static PoliticalActionDef SettlementAttack;
        public static PoliticalActionDef Trade;

        public static RecordDef TimesElected;
        public static RecordDef VotersSwayed;
        public static RecordDef SupportersRecruited;

        public static StatDef GovernanceDecay;
        public static StatDef GovernEfficiency;
        public static StatDef GovernEfficiencyFactor;

        public static SuccessionDef Election;

        public static TaleDef BecameLeader;

        public static ThoughtDef LikeDecision;
        public static ThoughtDef DislikeDecision;
        public static ThoughtDef LikeTrade;
        public static ThoughtDef DislikeTrade;
        public static ThoughtDef ElectionOutcome;
        public static ThoughtDef ElectionCompetitorMemory;
        public static ThoughtDef ImpeachedMemory;
        public static ThoughtDef PoliticalSympathy;

        public static WorkTypeDef Governing;

        static RimocracyDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(RimocracyDefOf));
    }
}
