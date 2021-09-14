using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ThoughtWorker_StateIdeoligion : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p) =>
            Utility.PoliticsEnabled && Utility.RimocracyComp.DecisionActive(DecisionDef.StateIdeoligion) && p.IsFreeAdultColonist() && p?.Ideo != null && p.Ideo != Utility.NationPrimaryIdeo;
    }
}
