using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ThoughtWorker_StateIdeologion : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
            => Utility.PoliticsEnabled && Utility.RimocracyComp.DecisionActive("StateIdeologion") && p.IsFreeAdultColonist() && p?.Ideo != null && p.Ideo != Utility.NationPrimaryIdea;
    }
}
