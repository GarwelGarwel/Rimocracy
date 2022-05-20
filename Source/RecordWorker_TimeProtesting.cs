using RimWorld;
using Verse;

namespace Rimocracy
{
    class RecordWorker_TimeProtesting : RecordWorker
    {
        public override bool ShouldMeasureTimeNow(Pawn pawn)
        {
            Need_Loyalty loyalty = pawn.GetLoyalty();
            return loyalty != null && loyalty.IsProtesting;
        }
    }
}
