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
            if (Utility.RimocracyComp == null)
            {
                Utility.Log("RimocracyComp is null.", LogLevel.Error);
                return;
            }

            // Politics Disabled
            if (!Utility.RimocracyComp.IsEnabled)
            {
                Widgets.Label(rect, $"You need at least {Settings.MinPopulation.ToStringCached()} free, adult colonists{(ModsConfig.IdeologyActive && Utility.RimocracyComp.DecisionActive(DecisionDef.StateIdeoligion) ? $" following {Utility.NationPrimaryIdeo?.name ?? "your primary ideoligion"}" : "")} and a potential leader for politics.");
                return;
            }

            Listing_Standard content = new Listing_Standard();
            content.Begin(rect);

            string leaderTitle = Utility.LeaderTitle.CapitalizeFirst(Utility.RimocracyComp.LeaderTitleDef);

            // Current Leader
            content.Label($"{leaderTitle}: {Utility.RimocracyComp.Leader?.NameFullColored ?? "none"}");

            // Governance target, leader skills and next succession
            if (Utility.RimocracyComp.HasLeader)
            {
                content.Label($"Governance quality: {Utility.RimocracyComp.Governance.ToStringPercent("F1")}. Falls by {Utility.RimocracyComp.GovernanceDecayPerDay.ToStringPercent()} per day.");

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
                content.Label($"Candidates:\r\n{Utility.RimocracyComp.Campaigns.Select(ec => $"- {ec.ToTaggedString()}").ToLineList()}");
            }

            // Protests
            if (Utility.RimocracyComp.Protesters.Count > 0)
            {
                content.Gap();
                if (Utility.RimocracyComp.Protesters.Count == 1)
                    content.Label($"Protester: {Utility.RimocracyComp.Protesters[0].NameShortColored}");
                else content.Label($"Protesters: {Utility.RimocracyComp.Protesters.Count}", tooltip: Utility.RimocracyComp.Protesters.Select(pawn => pawn.LabelCap).ToCommaList());
            }

            content.Gap();
            if (content.ButtonText("View Decisions"))
                Find.WindowStack.Add(new Dialog_DecisionList());

            content.End();
        }
    }
}
