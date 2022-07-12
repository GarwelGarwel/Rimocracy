using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    class MainTabWindow_Politics : MainTabWindow
    {
        public override Vector2 InitialSize => new Vector2(455, 320);

        public override void DoWindowContents(Rect rect)
        {
            RimocracyComp comp = Utility.RimocracyComp;
            if (comp == null)
            {
                Utility.Log("RimocracyComp is null.", LogLevel.Error);
                return;
            }

            // Politics Disabled
            if (!comp.IsEnabled)
            {
                Widgets.Label(rect, $"You need at least {Settings.MinPopulation.ToStringCached()} free, adult colonists{(ModsConfig.IdeologyActive && Utility.RimocracyComp.DecisionActive(DecisionDef.StateIdeoligion) ? $" following {Utility.NationPrimaryIdeo?.name ?? "your primary ideoligion"}" : "")} and a potential leader for politics.");
                return;
            }

            Listing_Standard content = new Listing_Standard();
            content.Begin(rect);

            string leaderTitle = Utility.LeaderTitle.CapitalizeFirst(comp.LeaderTitleDef);

            // Current Leader
            content.Label($"{leaderTitle}: {comp.Leader?.NameFullColored ?? "none"}");

            // Governance target, leader skills and next succession
            if (comp.HasLeader)
            {
                content.Label($"Governance quality: {comp.Governance.ToStringPercent("F1")}. Falls by {comp.GovernanceDecayPerDay.ToStringPercent()} per day.");

                content.Label($"Governance target: {comp.GovernanceTarget.ToStringPercent()}");
                comp.GovernanceTarget = GenMath.RoundedHundredth(content.Slider(comp.GovernanceTarget, 0, 1));

                if (comp.FocusSkill != null)
                    content.Label($"Focus skill: {comp.FocusSkill.LabelCap}.");

                if (comp.TermDuration != TermDuration.Indefinite)
                    content.Label($"Next {comp.SuccessionType.noun} in {(comp.TermExpiration - Find.TickManager.TicksAbs).ToStringTicksToPeriod(false)}.", tooltip: Utility.DateFullStringWithHourAtHome(comp.TermExpiration));
            }
            // Next election
            else if (comp.ElectionTick > Find.TickManager.TicksAbs)
            {
                if (comp.ElectionCalled)
                    content.Label($"{leaderTitle} will be elected in {(comp.ElectionTick - Find.TickManager.TicksAbs).ToStringTicksToPeriod(false)}.", tooltip: Utility.DateFullStringWithHourAtHome(comp.ElectionTick));
                else content.Label($"Election of a new {leaderTitle} not yet called for.");
            }
            else content.Label($"Choosing a new {Utility.LeaderTitle}...");

            // Election candidates
            if (comp.IsCampaigning)
            {
                content.Gap();
                content.Label($"Candidates:\r\n{comp.Campaigns.Select(ec => $"- {ec.ToTaggedString()}").ToLineList()}");
            }

            content.Gap();
            if (content.ButtonText("View Decisions"))
                Find.WindowStack.Add(new Dialog_DecisionList());

            content.End();
        }
    }
}
