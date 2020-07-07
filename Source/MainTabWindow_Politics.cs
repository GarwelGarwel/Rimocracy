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
                label += "Leader: " + (Utility.RimocracyComp.Leader?.NameFullColored ?? "none");
                if (Utility.RimocracyComp.Leader != null)
                {
                    label += "\nGovernance quality: " + Utility.RimocracyComp.GovernancePercentage.ToString("N1") + "%. Decays at " + (100 * Utility.RimocracyComp.GovernanceDecayPerDay).ToString("N1") + "% per day.";
                    if (Utility.RimocracyComp.FocusSkill != null)
                        label += "\nFocus skill: " + Utility.RimocracyComp.FocusSkill.LabelCap + ".";
                    if (Utility.RimocracyComp.TermExpiration < int.MaxValue)
                        label += "\nNext " + Utility.RimocracyComp.Succession.SuccessionLabel + " in " + GenDate.ToStringTicksToPeriod(Utility.RimocracyComp.TermExpiration - Find.TickManager.TicksAbs, false) + ".";
                }
                else if (Utility.RimocracyComp.ElectionTick > Find.TickManager.TicksAbs)
                    label += "\nLeader will be elected in " + GenDate.ToStringTicksToPeriod(Utility.RimocracyComp.ElectionTick - Find.TickManager.TicksAbs) + ".";
                else label += "\nChoosing the new leader...";

                if (!Utility.RimocracyComp.Campaigns.NullOrEmpty())
                {
                    label += "\n\nCandidates:";
                    foreach (ElectionCampaign ec in Utility.RimocracyComp.Campaigns)
                        label += "\n- " + ec;
                }
            }
            else label = "You need at least " + Settings.MinPopulation + " free, adult colonists for politics.";
            Widgets.Label(inRect, label);
        }
    }
}
