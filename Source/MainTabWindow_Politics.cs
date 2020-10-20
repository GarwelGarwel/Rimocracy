using RimWorld;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    class MainTabWindow_Politics : MainTabWindow
    {
        public override Vector2 InitialSize => new Vector2(455, 320);

        public override void DoWindowContents(Rect inRect)
        {
            // Politics Disabled
            base.DoWindowContents(inRect);
            if (!Utility.PoliticsEnabled)
            {
                Widgets.Label(inRect, $"You need at least {Settings.MinPopulation} free, adult colonists for politics.");
                return;
            }

            Listing_Standard content = new Listing_Standard();
            content.Begin(inRect);

            // Current Leader
            content.Label($"{Utility.LeaderTitle.CapitalizeFirst(Utility.RimocracyComp.LeaderTitleDef)}: {(Utility.RimocracyComp.Leader?.Name?.ToStringFull ?? "none")}");

            // Governance target, leader skills and next succession
            if (Utility.RimocracyComp.Leader != null)
            {
                content.Label($"Governance quality: {Utility.RimocracyComp.GovernancePercentage:N1}%. Decays at {Utility.RimocracyComp.GovernanceDecayPerDay * 100:N1}% per day.");

                content.Label($"Governance target: {Utility.RimocracyComp.GovernanceTarget * 100:N0}%");
                Utility.RimocracyComp.GovernanceTarget = content.Slider(Utility.RimocracyComp.GovernanceTarget, 0, 1);

                if (Utility.RimocracyComp.FocusSkill != null)
                    content.Label($"Focus skill: {Utility.RimocracyComp.FocusSkill.LabelCap}.");
                if (Utility.RimocracyComp.TermDuration != TermDuration.Indefinite)
                    content.Label($"Next {Utility.RimocracyComp.Succession.SuccessionLabel} in {(Utility.RimocracyComp.TermExpiration - Find.TickManager.TicksAbs).ToStringTicksToPeriod(false)}.");
            }
            // Next election
            else if (Utility.RimocracyComp.ElectionTick > Find.TickManager.TicksAbs)
                content.Label($"{Utility.LeaderTitle.CapitalizeFirst(Utility.RimocracyComp.LeaderTitleDef)} will be elected in {(Utility.RimocracyComp.ElectionTick - Find.TickManager.TicksAbs).ToStringTicksToPeriod()}.");
            else content.Label($"Choosing a new {Utility.LeaderTitle}...");

            // Election candidates
            if (!Utility.RimocracyComp.Campaigns.NullOrEmpty())
            {
                content.Gap();
                content.Label("Candidates:");
                foreach (ElectionCampaign ec in Utility.RimocracyComp.Campaigns)
                    content.Label($"- {ec}");
            }

            content.Gap();
            if (content.ButtonText("View Available Decisions"))
                Find.WindowStack.Add(new Dialog_DecisionList());

            content.End();
        }
    }
}
