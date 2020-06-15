using RimWorld;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    class MainTabWindow_Politics : MainTabWindow
    {
        public override Vector2 InitialSize => new Vector2(455, 180);

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            string label = "";
            if (Rimocracy.Instance.IsEnabled)
            {
                label += "Leader: " + (Rimocracy.Instance.Leader?.NameFullColored ?? "none");
                if (Rimocracy.Instance.Leader != null)
                {
                    if (Rimocracy.Instance.TermExpiration >= 0)
                        label += "\nTerm expires in " + GenDate.ToStringTicksToPeriod(Rimocracy.Instance.TermExpiration - Find.TickManager.TicksAbs, false) + ".";
                    label += "\nAuthority: " + Rimocracy.Instance.AuthorityPercentage.ToString("N1") + "%";
                    if (Rimocracy.Instance.FocusSkill != null)
                        label += "\nFocus skill: " + Rimocracy.Instance.FocusSkill.LabelCap;
                }
                else if (Rimocracy.Instance.ElectionTick > Find.TickManager.TicksAbs)
                    label += "\nLeader will be elected in " + GenDate.ToStringTicksToPeriod(Rimocracy.Instance.ElectionTick - Find.TickManager.TicksAbs) + ".";
                else label += "\nChoosing the new leader...";
            }
            else label = "You need at least " + Rimocracy.MinColonistsRequirement + " free colonists for politics.";
            Widgets.Label(inRect, label);
        }
    }
}
