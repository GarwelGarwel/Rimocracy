using RimWorld;
using Verse;

namespace Rimocracy
{
    class RecordWorker_TimeAsLeader : RecordWorker
    {
        public override bool ShouldMeasureTimeNow(Pawn pawn) => pawn.IsLeader();
    }
}
