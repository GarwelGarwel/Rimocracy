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
            if (Utility.PoliticsEnabled)
            {
                label += "Leader: " + (Utility.Rimocracy.Leader?.NameFullColored ?? "none");
                if (Utility.Rimocracy.Leader != null)
                {
                    label += "\nAuthority: " + Utility.Rimocracy.AuthorityPercentage.ToString("N1") + "%. Decays at " + (100 * Utility.Rimocracy.AuthorityDecayPerDay).ToString("N1") + "% per day.";
                    if (Utility.Rimocracy.FocusSkill != null)
                        label += "\nFocus skill: " + Utility.Rimocracy.FocusSkill.LabelCap + ".";
                    if (Utility.Rimocracy.TermExpiration < int.MaxValue)
                        label += "\nNext " + Utility.Rimocracy.Succession.SuccessionLabel + " in " + GenDate.ToStringTicksToPeriod(Utility.Rimocracy.TermExpiration - Find.TickManager.TicksAbs, false) + ".";
                }
                else if (Utility.Rimocracy.ElectionTick > Find.TickManager.TicksAbs)
                    label += "\nLeader will be elected in " + GenDate.ToStringTicksToPeriod(Utility.Rimocracy.ElectionTick - Find.TickManager.TicksAbs) + ".";
                else label += "\nChoosing the new leader...";

                if (!Utility.Rimocracy.Campaigns.NullOrEmpty())
                {
                    label += "\n\nCandidates:";
                    foreach (ElectionCampaign ec in Utility.Rimocracy.Campaigns)
                        label += "\n- " + ec;
                }
            }
            else label = "You need at least " + Utility.MinColonistsRequirement + " free colonists for politics.";
            Widgets.Label(inRect, label);
        }
    }
}
