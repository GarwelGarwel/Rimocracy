using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    class MainTabWindow_Politics : MainTabWindow
    {
        static List<float> governanceBarBandPercentages = new List<float>(1) { 0.5f };

        RimocracyComp comp = Utility.RimocracyComp;
        bool draggingBar;

        public override Vector2 InitialSize => new Vector2(420, comp.IsCampaigning ? 360 : 280);

        public override void DoWindowContents(Rect rect)
        {
            // Politics disabled
            if (!comp.IsEnabled)
            {
                if (Faction.OfPlayer.def.techLevel < Settings.MinTechLevel)
                    Widgets.Label(rect, $"Your faction's tech level needs to be {Settings.MinTechLevel.ToStringHuman().CapitalizeFirst()} or higher for politics. Currently it is {Faction.OfPlayer.def.techLevel.ToStringHuman().CapitalizeFirst()}.");
                else Widgets.Label(rect, $"You need at least {Settings.MinPopulation.ToStringCached()} free, adult colonists{(ModsConfig.IdeologyActive && Utility.RimocracyComp.DecisionActive(DecisionDef.StateIdeoligion) ? $" following {Utility.NationPrimaryIdeo?.name ?? "your primary ideoligion"}" : "")} and a potential leader for politics.");
                return;
            }

            string leaderTitle = Utility.LeaderTitle.CapitalizeFirst(comp.LeaderTitleDef);

            Listing_Standard content = new Listing_Standard();
            content.Begin(rect);

            // Governance target, leader skills and next succession
            if (comp.HasLeader)
            {
                // Current Leader
                if (Widgets.ButtonInvisible(content.Label($"{leaderTitle}: {comp.Leader.NameFullColored}")))
                {
                    Close();
                    CameraJumper.TryJumpAndSelect(comp.Leader);
                    return;
                }

                content.Gap();
                content.Label($"Governance quality: {comp.Governance.ToStringPercent("F1")}. Target: {comp.GovernanceTarget.ToStringPercent()}.");

                float governanceTarget = comp.GovernanceTarget;
                Widgets.DraggableBar(content.GetRect(22), Texture2D.grayTexture, Texture2D.whiteTexture, Texture2D.blackTexture, Texture2D.whiteTexture, ref draggingBar, comp.Governance, ref governanceTarget, governanceBarBandPercentages);
                comp.GovernanceTarget = GenMath.RoundedHundredth(governanceTarget);

                content.Label($"Falls at {comp.GovernanceDecayPerDay.ToStringPercent()} per day.");
                content.Gap();

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
                else content.Label($"Election of a new {Utility.LeaderTitle} not yet called for.");
            }
            else content.Label($"Choosing a new {Utility.LeaderTitle}...");

            // Election candidates
            if (comp.IsCampaigning)
            {
                content.Gap();
                content.Label("Candidates:");
                foreach (ElectionCampaign campaign in comp.Campaigns)
                    if (Widgets.ButtonInvisible(content.Label($"- {campaign.Candidate.NameShortColored}, {campaign.FocusSkill?.LabelCap ?? "no"} focus", tooltip: campaign.Supporters.Count > 1 ? $"{(campaign.Supporters.Count - 1).ToStringCached()} core supporters" : null)))
                    {
                        Close();
                        CameraJumper.TryJumpAndSelect(campaign.Candidate);
                        return;
                    }
            }

            content.Gap();
            if (content.ButtonText("View Decisions"))
                Find.WindowStack.Add(new Dialog_DecisionList());

            content.End();
        }
    }
}
