using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ThoughtWorker_Multiculturalism : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p) =>
            Utility.PoliticsEnabled && Utility.RimocracyComp.DecisionActive(DecisionDef.Multiculturalism) && p.IsFreeAdultColonist() && p?.Ideo != null && p.Ideo != Utility.NationPrimaryIdeo;
    }
}
