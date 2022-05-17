using System;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class RimocracyMod : Mod
    {
        Vector2 scrollPosition = new Vector2();
        Rect viewRect = new Rect();

        public RimocracyMod(ModContentPack content)
            : base(content)
        {
            GetSettings<Settings>();
            HarmonyManager.Initialize();
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            if (viewRect.height <= 0)
            {
                viewRect.width = rect.width - 16;
                viewRect.height = 1000;
            }
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            Listing_Standard content = new Listing_Standard();
            content.ColumnWidth = rect.width - 20;
            content.Begin(viewRect);

            content.Label($"Min Population for Politics: {Settings.MinPopulation.ToStringCached()}", tooltip: "Most mod effects are disabled if you have fewer citizens than this number");
            Settings.MinPopulation = (int)content.Slider(Settings.MinPopulation, 1, 20);

            content.Label($"Citizenship Age: {Settings.CitizenshipAge.ToStringCached()}", tooltip: "Biological age for a colonist to have citizenship rights");
            Settings.CitizenshipAge = (int)content.Slider(Settings.CitizenshipAge, 0, 18);

            content.Label($"Governance Decay Speed: {Settings.GovernanceDecaySpeed.ToStringPercent()}", tooltip: "Relative speed of governance deterioration over time");
            Settings.GovernanceDecaySpeed = (float)Math.Round(content.Slider(Settings.GovernanceDecaySpeed, 0, 2), 2);

            content.Label($"Min Population for Campaigning: {Settings.MinPopulationForCampaigning.ToStringCached()}", tooltip: "Min number of voters to have elections between only two candidates, who actively seek supporters");
            Settings.MinPopulationForCampaigning = (int)content.Slider(Settings.MinPopulationForCampaigning, Settings.MinPopulation, 20);

            content.Label($"Campaign Duration: {Settings.CampaignDurationDays} days", tooltip: "How many days a campaign lasts (also applies to the delay before the first election)");
            Settings.CampaignDurationDays = (float)Math.Round(content.Slider(Settings.CampaignDurationDays, 0, 15), 1);

            content.Label($"Mental State Vote Weight Penalty: {Settings.MentalStateVoteWeightPenalty.ToStringCached()}", tooltip: "How much vote weight is reduced for candidates in a mental state (the value is doubled for aggressive mental states)");
            Settings.MentalStateVoteWeightPenalty = (int)content.Slider(Settings.MentalStateVoteWeightPenalty, 0, 20);

            content.Label($"Same Backstory Vote Weight Penalty: {Settings.SameBackstoryVoteWeightBonus.ToStringCached()}", tooltip: "How much vote weight is increased for candidates with similar backstories as the voter");
            Settings.SameBackstoryVoteWeightBonus = (int)content.Slider(Settings.SameBackstoryVoteWeightBonus, 0, 50);

            content.Label($"Political Sympathy Factor: {Settings.PoliticalSympathyWeightFactor.ToStringCached()}", tooltip: "The importance of political sympathy for voting");
            Settings.PoliticalSympathyWeightFactor = (int)content.Slider(Settings.PoliticalSympathyWeightFactor, 0, 50);

            content.Label($"Random Vote Weight Element Radius: ±{Settings.RandomVoteWeightRadius.ToStringCached()}", tooltip: "Max random number that is added or subtracted from the vote weight");
            Settings.RandomVoteWeightRadius = (int)content.Slider(Settings.RandomVoteWeightRadius, 0, 10);

            content.Label($"Sway Chance Factor: {Settings.SwayChanceFactor.ToStringPercent()}", tooltip: "Relative likelyhood of a candidate or supporter successfully swaying a voter during a campaign");
            Settings.SwayChanceFactor = (float)Math.Round(content.Slider(Settings.SwayChanceFactor, 0, 2), 2);

            content.Label($"Recruitment Chance Factor: {Settings.RecruitmentChanceFactor.ToStringPercent()}", tooltip: "Relative likelyhood of a successfully recruitment of a supporter during a campaign");
            Settings.RecruitmentChanceFactor = (float)Math.Round(content.Slider(Settings.RecruitmentChanceFactor, 0, 2), 2);

            content.Label($"Governance Cost Factor: {Settings.GovernanceCostFactor.ToStringPercent()}", tooltip: "Adjust the Governance cost of decisions");
            Settings.GovernanceCostFactor = (float)Math.Round(content.Slider(Settings.GovernanceCostFactor, 0, 2), 2);

            content.CheckboxLabeled("Show Action Support Details", ref Settings.ShowActionSupportDetails, "Show a dialog with info about who supported or opposed various actions (e.g. arrest) and why");

            content.CheckboxLabeled("Debug Logging", ref Settings.DebugLogging, "Check to enable verbose logging; it is super useful for catching bugs");

            viewRect.height = content.CurHeight;
            content.End();
            Widgets.EndScrollView();
        }

        public override string SettingsCategory() => "Rimocracy";
    }
}
