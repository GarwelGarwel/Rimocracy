using RimWorld;
using Verse;

namespace Rimocracy
{
    public class StatWorker_GovernanceDecay : StatWorker
    {
        public override bool IsDisabledFor(Thing thing) => !(thing as Pawn).IsLeader();

        public override bool ShouldShowFor(StatRequest req) => base.ShouldShowFor(req) && req.Pawn.IsLeader();
    }
}
