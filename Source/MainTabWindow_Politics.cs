using RimWorld;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    class MainTabWindow_Politics : MainTabWindow
    {
        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            string label = "";
            label += "Leader: " + (Rimocracy.Instance.Leader?.NameFullColored ?? "none");
            if (Rimocracy.Instance.Leader != null)
            {
                if (Rimocracy.Instance.TermExpiration >= 0)
                    label += "\nTerm expires in " + GenDate.ToStringTicksToPeriod(Rimocracy.Instance.TermExpiration - Find.TickManager.TicksAbs, false);
                label += "\nAuthority: " + Rimocracy.Instance.AuthorityPercentage.ToString("N1") + "%";
            }
            Widgets.Label(inRect, label);
        }
    }
}
