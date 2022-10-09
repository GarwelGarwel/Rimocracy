using System;
using UnityEngine;
using Verse;

using static Rimocracy.Settings;

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

            content.Label($"Min Population for Politics: {MinPopulation.ToStringCached()}", tooltip: "Most mod effects are disabled if you have fewer citizens than this number");
            MinPopulation = (int)content.Slider(MinPopulation, 1, 20);

            content.Label($"Citizenship Age: {CitizenshipAge.ToStringCached()}", tooltip: "Biological age for a colonist to have citizenship rights");
            CitizenshipAge = (int)content.Slider(CitizenshipAge, 0, 18);

            content.Label($"Governance Decay Speed: {GovernanceDecaySpeed.ToStringPercent()}", tooltip: "Relative speed of governance deterioration over time");
            GovernanceDecaySpeed = (float)Math.Round(content.Slider(GovernanceDecaySpeed, 0, 2), 2);

            content.Label($"Min Population for Campaigning: {MinPopulationForCampaigning.ToStringCached()}", tooltip: "Min number of voters to have elections between only two candidates, who actively seek supporters");
            MinPopulationForCampaigning = (int)content.Slider(MinPopulationForCampaigning, MinPopulation, 20);

            content.Label($"Campaign Duration: {CampaignDurationDays} days", tooltip: "How many days a campaign lasts (also applies to the delay before the first election)");
            CampaignDurationDays = (float)Math.Round(content.Slider(CampaignDurationDays, 0, 15), 1);

            content.Label($"Mental State Vote Weight Penalty: {MentalStateVoteWeightPenalty.ToStringCached()}", tooltip: "How much vote weight is reduced for candidates in a mental state (the value is doubled for aggressive mental states)");
            MentalStateVoteWeightPenalty = (int)content.Slider(MentalStateVoteWeightPenalty, 0, 20);

            content.Label($"Same Backstory Vote Weight Penalty: {SameBackstoryVoteWeightBonus.ToStringCached()}", tooltip: "How much vote weight is increased for candidates with similar backstories as the voter");
            SameBackstoryVoteWeightBonus = (int)content.Slider(SameBackstoryVoteWeightBonus, 0, 50);

            content.Label($"Political Sympathy Factor: {PoliticalSympathyWeightFactor.ToStringCached()}", tooltip: "The importance of political sympathy for voting");
            PoliticalSympathyWeightFactor = (int)content.Slider(PoliticalSympathyWeightFactor, 0, 50);

            content.Label($"Random Vote Weight Element Radius: ±{RandomVoteWeightRadius.ToStringCached()}", tooltip: "Max random number that is added or subtracted from the vote weight");
            RandomVoteWeightRadius = (int)content.Slider(RandomVoteWeightRadius, 0, 10);

            content.Label($"Sway Chance Factor: {SwayChanceFactor.ToStringPercent()}", tooltip: "Relative likelyhood of a candidate or supporter successfully swaying a voter during a campaign");
            SwayChanceFactor = (float)Math.Round(content.Slider(SwayChanceFactor, 0, 2), 2);

            content.Label($"Recruitment Chance Factor: {RecruitmentChanceFactor.ToStringPercent()}", tooltip: "Relative likelyhood of a successfully recruitment of a supporter during a campaign");
            RecruitmentChanceFactor = (float)Math.Round(content.Slider(RecruitmentChanceFactor, 0, 2), 2);

            content.Label($"Governance Cost Factor: {GovernanceCostFactor.ToStringPercent()}", tooltip: "Adjust the Governance cost of decisions");
            GovernanceCostFactor = (float)Math.Round(content.Slider(GovernanceCostFactor, 0, 2), 2);

            content.CheckboxLabeled("Loyalty Enabled", ref LoyaltyEnabled, $"Pawns have a Loyalty need that affects their reaction to decisions and may cause protests. {(LoyaltyEnabled ? "Note: If you uncheck this but then change your mind, list pawns' loyalties will be reset to 50%.".Colorize(Color.red) : "")}");

            content.CheckboxLabeled("Show Action Support Details", ref ShowActionSupportDetails, "Show a dialog with info about who supported or opposed various actions (e.g. arrest) and why");

            content.CheckboxLabeled("Debug Logging", ref DebugLogging, "Check to enable verbose logging; it is necessary to report bugs");

            viewRect.height = content.CurHeight;
            content.End();
            Widgets.EndScrollView();
        }

        public override string SettingsCategory() => "Rimocracy";
    }
}
