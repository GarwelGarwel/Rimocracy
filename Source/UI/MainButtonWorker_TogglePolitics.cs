namespace Rimocracy
{
    public class MainButtonWorker_TogglePolitics : RimWorld.MainButtonWorker_ToggleTab
    {
        public override float ButtonBarPercent
        {
            get
            {
                RimocracyComp comp = Utility.RimocracyComp;
                return comp != null && comp.IsEnabled ? comp.Governance : 0;
            }
        }
    }
}
