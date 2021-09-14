using RimWorld;

namespace Rimocracy
{
    public class GoodwillSituationWorker_Multiculturalism : GoodwillSituationWorker
    {
        public override int GetNaturalGoodwillOffset(Faction other) =>
            Utility.PoliticsEnabled && Utility.RimocracyComp.DecisionActive(DecisionDef.Multiculturalism) && other?.ideos?.PrimaryIdeo != null && other.ideos.PrimaryIdeo != Utility.NationPrimaryIdeo
            ? def.naturalGoodwillOffset
            : 0;
    }
}
