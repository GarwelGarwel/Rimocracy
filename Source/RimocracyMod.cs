using System;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class RimocracyMod : Mod
    {
        Vector2 scrollPosition = new Vector2();

        public RimocracyMod(ModContentPack content)
            : base(content) => GetSettings<Settings>();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            Rect viewRect = new Rect(0, 0, inRect.width - 50, 1000);
            listingStandard.BeginScrollView(inRect, ref scrollPosition, ref viewRect);
            listingStandard.Begin(viewRect);

            listingStandard.Label("Min Population for Politics: " + Settings.MinPopulation, tooltip: "Most mod effects are disabled if you have fewer citizens than this number");
            Settings.MinPopulation = (int)listingStandard.Slider(Settings.MinPopulation, 1, 20);

            listingStandard.Label("Citizenship Age: " + Settings.CitizenshipAge, tooltip: "Biological age for a colonist to have citizenship rights");
            Settings.CitizenshipAge = (int)listingStandard.Slider(Settings.CitizenshipAge, 0, 18);

            listingStandard.Label("Governance Decay Speed: " + Settings.GovernanceDecaySpeed.ToString("P0"), tooltip: "Relative speed of governance deterioration over time");
            Settings.GovernanceDecaySpeed = (float)Math.Round(listingStandard.Slider(Settings.GovernanceDecaySpeed, 0, 2), 2);

            listingStandard.Label("Min Population for Campaigning: " + Settings.MinPopulationForCampaigning, tooltip: "Min number of voters to have elections between only two candidates, who actively seek supporters");
            Settings.MinPopulationForCampaigning = (int)listingStandard.Slider(Settings.MinPopulationForCampaigning, Settings.MinPopulation, 20);

            listingStandard.Label("Campaign Duration: " + Settings.CampaignDurationDays + " days", tooltip: "How many days a campaign lasts (also applies to the delay before the first election)");
            Settings.CampaignDurationDays = (float)Math.Round(listingStandard.Slider(Settings.CampaignDurationDays, 0, 15), 1);

            listingStandard.Label("Mental State Vote Weight Penalty: " + Settings.MentalStateVoteWeightPenalty, tooltip: "How much vote weight is reduced for candidates in a mental state (the value is doubled for aggressive mental states)");
            Settings.MentalStateVoteWeightPenalty = (int)listingStandard.Slider(Settings.MentalStateVoteWeightPenalty, 0, 20);

            listingStandard.Label("Same Backstory Vote Weight Penalty: " + Settings.SameBackstoryVoteWeightBonus, tooltip: "How much vote weight is increased for candidates with similar backstories as the voter");
            Settings.SameBackstoryVoteWeightBonus = (int)listingStandard.Slider(Settings.SameBackstoryVoteWeightBonus, 0, 50);

            listingStandard.Label("Political Sympathy Factor: " + Settings.PoliticalSympathyWeightFactor, tooltip: "The importance of political sympathy for voting");
            Settings.PoliticalSympathyWeightFactor = (int)listingStandard.Slider(Settings.PoliticalSympathyWeightFactor, 0, 50);

            listingStandard.Label("Random Vote Weight Element Radius: ±" + Settings.RandomVoteWeightRadius, tooltip: "Max random number that is added or subtracted from the vote weight");
            Settings.RandomVoteWeightRadius = (int)listingStandard.Slider(Settings.RandomVoteWeightRadius, 0, 10);

            listingStandard.Label("Sway Chance Factor: " + Settings.SwayChanceFactor.ToString("P0"), tooltip: "Relative likelyhood of a candidate or supporter successfully swaying a voter during a campaign");
            Settings.SwayChanceFactor = (float)Math.Round(listingStandard.Slider(Settings.SwayChanceFactor, 0, 2), 2);

            listingStandard.Label("Recruitment Chance Factor: " + Settings.RecruitmentChanceFactor.ToString("P0"), tooltip: "Relative likelyhood of a successfully recruitment of a supporter during a campaign");
            Settings.RecruitmentChanceFactor = (float)Math.Round(listingStandard.Slider(Settings.RecruitmentChanceFactor, 0, 2), 2);

            listingStandard.CheckboxLabeled("Debug Logging", ref Settings.DebugLogging, "Check to enable verbose logging; it is super useful for catching bugs");

            listingStandard.EndScrollView(ref viewRect);
            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "Rimocracy";
    }
}
