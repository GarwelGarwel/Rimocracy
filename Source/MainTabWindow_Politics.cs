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
            if (Rimocracy.IsEnabled)
            {
                label += "Leader: " + (Rimocracy.Instance.Leader?.NameFullColored ?? "none");
                if (Rimocracy.Instance.Leader != null)
                {
                    label += "\nAuthority: " + Rimocracy.Instance.AuthorityPercentage.ToString("N1") + "%";
                    label += "\nAuthority decay: " + (100 * Rimocracy.Instance.AuthorityDecayPerDay).ToString("N1") + "% per day";
                    if (Rimocracy.Instance.FocusSkill != null)
                        label += "\nFocus skill: " + Rimocracy.Instance.FocusSkill.LabelCap;
                    if (Rimocracy.Instance.TermExpiration < int.MaxValue)
                        label += "\nNext " + Rimocracy.Instance.Succession.SuccessionLabel + " in " + GenDate.ToStringTicksToPeriod(Rimocracy.Instance.TermExpiration - Find.TickManager.TicksAbs, false) + ".";
                }
                else if (Rimocracy.Instance.ElectionTick > Find.TickManager.TicksAbs)
                    label += "\nLeader will be elected in " + GenDate.ToStringTicksToPeriod(Rimocracy.Instance.ElectionTick - Find.TickManager.TicksAbs);
                else label += "\nChoosing the new leader...";

                if (!Rimocracy.Instance.Campaigns.NullOrEmpty())
                {
                    label += "\nCandidates:";
                    foreach (ElectionCampaign ec in Rimocracy.Instance.Campaigns)
                        label += "\n" + ec;
                }
            }
            else label = "You need at least " + Rimocracy.MinColonistsRequirement + " free colonists for politics.";
            Widgets.Label(inRect, label);
        }
    }
}
