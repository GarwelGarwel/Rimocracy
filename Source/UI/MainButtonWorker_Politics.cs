using RimWorld;

namespace Rimocracy
{
    public class MainButtonWorker_Politics : MainButtonWorker_ToggleTab
    {
        public override float ButtonBarPercent => Utility.PoliticsEnabled ? Utility.RimocracyComp.Governance : 0;
    }
}
