using RimWorld;
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

        public override string SettingsCategory() => "Rimocracy";

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

            TechLevel curTechLevel = Faction.OfPlayerSilentFail?.def != null ? Faction.OfPlayerSilentFail.def.techLevel : TechLevel.Undefined;
            content.Label($"Min tech level for politics: {MinTechLevel.ToStringHuman().CapitalizeFirst().Colorize(Color.yellow)}{(curTechLevel != TechLevel.Undefined ? $" (current level: {curTechLevel.ToStringHuman().CapitalizeFirst().Colorize(Color.yellow)})" : "")}", tooltip: "Most mod effects are disabled unless player's faction is at this tech level or higher.");
            MinTechLevel = (TechLevel)content.Slider((int)MinTechLevel, (int)TechLevel.Animal, (int)TechLevel.Archotech);

            content.Label($"Min population for politics: {MinPopulation.ToStringCached().Colorize(Color.yellow)}", tooltip: "Most mod effects are disabled if you have fewer citizens than this number.");
            MinPopulation = (int)content.Slider(MinPopulation, 2, 20);

            content.Label($"Citizenship age: {CitizenshipAge.ToStringCached().Colorize(Color.yellow)}", tooltip: "Biological age for a colonist to have citizenship rights.");
            CitizenshipAge = (int)content.Slider(CitizenshipAge, 0, 18);

            content.Label($"Governance decay speed: {GovernanceDecaySpeed.ToStringPercent().Colorize(Color.yellow)}", tooltip: "Relative speed of governance deterioration over time.");
            GovernanceDecaySpeed = (float)Math.Round(content.Slider(GovernanceDecaySpeed, 0, 2), 2);

            content.Label($"Min Population for campaigning: {MinPopulationForCampaigning.ToStringCached().Colorize(Color.yellow)}", tooltip: "Min number of voters to have elections between only two candidates, who actively seek supporters.");
            MinPopulationForCampaigning = (int)content.Slider(MinPopulationForCampaigning, MinPopulation, 20);

            content.Label($"Campaign duration: {CampaignDurationDays.ToString().Colorize(Color.yellow)} days", tooltip: "How many days a campaign lasts (also applies to the delay before the first election).");
            CampaignDurationDays = (float)Math.Round(content.Slider(CampaignDurationDays, 0, 15), 1);

            content.Label($"Governance cost factor: {GovernanceCostFactor.ToStringPercent().Colorize(Color.yellow)}", tooltip: "Adjust the Governance cost of decisions.");
            GovernanceCostFactor = (float)Math.Round(content.Slider(GovernanceCostFactor, 0, 2), 2);

            content.CheckboxLabeled("Loyalty enabled", ref LoyaltyEnabled, $"Pawns have a Loyalty need that affects their reaction to decisions and may cause protests. {(LoyaltyEnabled ? "Note: If you uncheck this but then change your mind, list pawns' loyalties will be reset to 50%.".Colorize(Color.red) : "")}");

            content.CheckboxLabeled("Show action support details", ref ShowActionSupportDetails, "Show a dialog with info about who supported or opposed various actions (e.g. arrest) and why.");

            content.CheckboxLabeled("Debug logging", ref DebugLogging, "Check to enable verbose logging. It is necessary to report bugs.");

            if (content.ButtonText("Reset to default"))
                Reset();

            viewRect.height = content.CurHeight;
            content.End();
            Widgets.EndScrollView();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            Print();
        }
    }
}
