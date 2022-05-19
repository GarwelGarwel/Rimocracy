using RimWorld;
using Verse;

namespace Rimocracy
{
    class RecordWorker_TimeProtesting : RecordWorker
    {
        public override bool ShouldMeasureTimeNow(Pawn pawn) => pawn.GetLoyalty().IsProtesting;
    }
}
