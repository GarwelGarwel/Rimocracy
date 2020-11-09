using RimWorld;
using Verse;

namespace Rimocracy.Succession
{
    class SuccessionNobility : SuccessionBase
    {
        public override SuccessionType SuccessionType => SuccessionType.Nobility;

        //public override string Title => "Nobility";

        //public override float RegimeEffect => -0.10f;

        public override bool IsValid => !Candidates.EnumerableNullOrEmpty();

        public override bool CanBeCandidate(Pawn pawn) =>
            base.CanBeCandidate(pawn) && pawn.royalty != null && !pawn.royalty.AllTitlesForReading.NullOrEmpty();

        public override Pawn ChooseLeader() => Candidates.MaxByWithFallback(pawn => GetExtendedSeniority(pawn.royalty.MostSeniorTitle));

        int GetExtendedSeniority(RoyalTitle title) => title.def.seniority * GenDate.TicksPerYear - title.receivedTick;
    }
}
