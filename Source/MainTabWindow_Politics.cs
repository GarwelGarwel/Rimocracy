using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    class MainTabWindow_Politics : MainTabWindow
    {
        Vector2 scrollPosition = new Vector2();

        public override Vector2 InitialSize => new Vector2(455, Utility.PoliticsEnabled ? 400 : 180);

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
            Rect viewRect = new Rect(0, 0, inRect.width, 360);
            content.BeginScrollView(inRect, ref scrollPosition, ref viewRect);

            // Current Leader
            content.Label($"{Utility.LeaderTitle.CapitalizeFirst(Utility.RimocracyComp.LeaderTitleDef)}: {(Utility.RimocracyComp.Leader?.Name?.ToStringFull ?? "none")}");

            // Leader Skills and next leader
            if (Utility.RimocracyComp.Leader != null)
            {
                content.Label($"Governance quality: {Utility.RimocracyComp.GovernancePercentage:N1}%. Decays at {100 * Utility.RimocracyComp.GovernanceDecayPerDay:N1}% per day.");
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

            // Governance Target
            content.GapLine();
            Widgets.FloatRange(content.GetRect(28f), 975643279, ref Utility.RimocracyComp.GovernanceTarget, 0f, 1f, "Rimocracy_GovernanceTargetLabel", ToStringStyle.PercentZero);

            // Decisions
            content.GapLine();
            content.Label("Decisions:");
            foreach (DecisionDef def in DefDatabase<DecisionDef>.AllDefs.Where(def => def.IsValid))
                if (content.ButtonText(def.label))
                    Find.WindowStack.Add(new Dialog_Decision(def));

            content.EndScrollView(ref viewRect);
            content.End();
        }
    }
}
