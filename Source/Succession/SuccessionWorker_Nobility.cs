using RimWorld;
using Verse;

namespace Rimocracy
{
    class SuccessionWorker_Nobility : SuccessionWorker
    {
        public override bool IsValid => !Candidates.EnumerableNullOrEmpty();

        public override bool CanBeCandidate(Pawn pawn) =>
            base.CanBeCandidate(pawn) && pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Any();

        public override Pawn ChooseLeader() => Candidates.MaxByWithFallback(pawn => GetExtendedSeniority(pawn.royalty.MostSeniorTitle));

        int GetExtendedSeniority(RoyalTitle title) => title.def.seniority * GenDate.TicksPerYear - title.receivedTick;
    }
}
