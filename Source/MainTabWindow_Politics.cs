using RimWorld;
using System.Linq;
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
            if (!Utility.PoliticsEnabled)
            {
                Widgets.Label(inRect, $"You need at least {Settings.MinPopulation.ToStringCached()} free, adult colonists and a potential leader for politics.");
                return;
            }

            Listing_Standard content = new Listing_Standard();
            content.Begin(inRect);

            string leaderTitle = Utility.LeaderTitle.CapitalizeFirst(Utility.RimocracyComp.LeaderTitleDef);

            // Current Leader
            content.Label($"{leaderTitle}: {Utility.RimocracyComp.Leader?.NameFullColored ?? "none"}");

            // Governance target, leader skills and next succession
            if (Utility.RimocracyComp.HasLeader)
            {
                content.Label($"Governance quality: {Utility.RimocracyComp.Governance.ToStringPercent("F1")}. Decays at {Utility.RimocracyComp.GovernanceDecayPerDay.ToStringPercent()} per day.");

                content.Label($"Governance target: {Utility.RimocracyComp.GovernanceTarget.ToStringPercent()}");
                Utility.RimocracyComp.GovernanceTarget = GenMath.RoundedHundredth(content.Slider(Utility.RimocracyComp.GovernanceTarget, 0, 1));

                if (Utility.RimocracyComp.FocusSkill != null)
                    content.Label($"Focus skill: {Utility.RimocracyComp.FocusSkill.LabelCap}.");

                if (Utility.RimocracyComp.TermDuration != TermDuration.Indefinite)
                    content.Label($"Next {Utility.RimocracyComp.SuccessionType.noun} in {(Utility.RimocracyComp.TermExpiration - Find.TickManager.TicksAbs).ToStringTicksToPeriod(false)}.", tooltip: Utility.DateFullStringWithHourAtHome(Utility.RimocracyComp.TermExpiration));
            }
            // Next election
            else if (Utility.RimocracyComp.ElectionTick > Find.TickManager.TicksAbs)
            {
                if (Utility.RimocracyComp.ElectionTick != int.MaxValue)
                    content.Label($"{leaderTitle} will be elected in {(Utility.RimocracyComp.ElectionTick - Find.TickManager.TicksAbs).ToStringTicksToPeriod(false)}.", tooltip: Utility.DateFullStringWithHourAtHome(Utility.RimocracyComp.ElectionTick));
                else content.Label($"Election of a new {leaderTitle} not yet called for.");
            }
            else content.Label($"Choosing a new {Utility.LeaderTitle}...");

            // Election candidates
            if (Utility.RimocracyComp.IsCampaigning)
            {
                content.Gap();
                content.Label($"Candidates:\r\n{Utility.RimocracyComp.Campaigns.Select(ec => ec.ToString()).ToLineList("- ")}");
            }

            content.Gap();
            if (content.ButtonText("View Decisions"))
                Find.WindowStack.Add(new Dialog_DecisionList());

            content.End();
        }
    }
}
