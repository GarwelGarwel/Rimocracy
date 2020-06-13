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
            Widgets.Label(inRect, "Leader: " + (Rimocracy.Instance.Leader?.NameFullColored ?? "none"));
            if (Rimocracy.Instance.Leader == null)
                return;
            if (Rimocracy.Instance.TermExpiration >= 0)
                Widgets.Label(inRect, "Term expires in " + GenDate.ToStringTicksToPeriod(Rimocracy.Instance.TermExpiration - Find.TickManager.TicksAbs, false));
            Widgets.Label(inRect, "Authority: " + Rimocracy.Instance.AuthorityPercentage.ToString("N1") + "%");
        }
    }
}
