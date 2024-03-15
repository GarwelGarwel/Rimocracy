using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public enum TermDuration
    { 
        Undefined = 0, 
        Quadrum, 
        Halfyear, 
        Year, 
        Indefinite 
    };

    enum LogLevel 
    {
        Message = 0, 
        Warning, 
        Error 
    };

    public static class Utility
    {
        static int rimocracyCompIndex = -1;

        public static RimocracyComp RimocracyComp
        {
            get
            {
                RimocracyComp comp;
                if (rimocracyCompIndex >= 0 && rimocracyCompIndex < Find.World.components.Count)
                {
                    comp = Find.World.components[rimocracyCompIndex] as RimocracyComp;
                    if (comp != null)
                        return comp;
                }
                for (rimocracyCompIndex = Find.World.components.Count - 1; rimocracyCompIndex >= 0; rimocracyCompIndex--)
                {
                    comp = Find.World.components[rimocracyCompIndex] as RimocracyComp;
                    if (comp != null)
                        return comp;
                }
                Log($"Could not find RimocracyComp among {Find.World.components.Count} world components.", LogLevel.Error);
                return null;
            }
        }

        public static bool PoliticsEnabled => RimocracyComp != null && RimocracyComp.IsEnabled;

        public static bool IsFreeAdultColonist(this Pawn pawn) =>
            pawn != null
            && !pawn.Dead
            && pawn.IsFreeNonSlaveColonist
            && pawn.HomeFaction.IsPlayer
            && pawn.ageTracker.AgeBiologicalYears >= Settings.CitizenshipAge;

        public static bool IsCitizen(this Pawn pawn) =>
            pawn.IsFreeAdultColonist() && (!ModsConfig.IdeologyActive || pawn?.Ideo == NationPrimaryIdeo || !RimocracyComp.DecisionActive(DecisionDef.StateIdeoligion));

        public static IEnumerable<Pawn> Citizens =>
            PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Where(p => p.IsCitizen());

        public static int CitizensCount => Citizens.Count();

        public static int Population => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep.Count();

        public static float CitizenGovernanceWeight(Pawn pawn)
        {
            if (!Settings.LoyaltyEnabled)
                return 1;
            Need_Loyalty loyalty = pawn.GetLoyalty();
            if (loyalty == null)
                return 1;
            if (loyalty.IsProtesting)
                return 2;
            return 1.5f - loyalty.CurLevel;
        }

        public static Need_Loyalty GetLoyalty(this Pawn pawn) => pawn.needs.TryGetNeed<Need_Loyalty>();

        public static float GetLoyaltyLevel(this Pawn pawn)
        {
            Need_Loyalty loyalty = pawn.GetLoyalty();
            return loyalty != null ? loyalty.CurLevel : Need_Loyalty.DefaultLevel;
        }

        public static void ChangeLoyalty(this Pawn pawn, float value)
        {
            Need_Loyalty loyalty = pawn?.GetLoyalty();
            if (loyalty != null)
                loyalty.CurLevel += value;
            else Log($"ChangeLoyalty: {pawn} has no Need_Loyalty.", LogLevel.Error);
        }

        public static float GetLoyaltySupportOffset(this Pawn pawn) => pawn.GetLoyaltyLevel() * 200 - 100;

        public static float TotalNutrition => Find.Maps.Where(map => map.IsPlayerHome).Sum(map => map.resourceCounter.TotalHumanEdibleNutrition);

        public static float FoodConsumptionPerDay =>
            PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep
            .Sum(pawn => pawn.needs.food.FoodFallPerTick) * GenDate.TicksPerDay;

        public static int GetTotalSilver() => Find.Maps.Where(map => map.IsPlayerHome).Sum(map => map.resourceCounter.Silver);

        public static int RemoveSilver(this Map map, int amount)
        {
            int removed = 0;
            foreach (Thing thing in map.spawnedThings.Where(thing => thing.Faction.IsPlayer && thing.def == ThingDefOf.Silver).ToList())
            {
                int count = Math.Min(amount - removed, thing.stackCount);
                Log($"Removing {count} silver from {thing}...");
                thing.SplitOff(count);
                removed += count;
                if (removed >= amount)
                    break;
            }
            Log($"{removed} units of silver removed from {map} out of {amount}.");
            return removed;
        }

        public static int RemoveSilver(int amount)
        {
            int removed = 0;
            foreach (Map map in Find.Maps.Where(map => map.IsPlayerHome))
            {
                removed += map.RemoveSilver(amount - removed);
                if (removed >= amount)
                    break;
            }
            Log($"{removed} silver removed from list maps out of {amount}.");
            return removed;
        }

        public static float DaysOfFood => TotalNutrition / FoodConsumptionPerDay;

        public static string NationName => Find.FactionManager.OfPlayer.Name;

        public static Ideo NationPrimaryIdeo => Find.FactionManager.OfPlayer.ideos.PrimaryIdeo;

        public static IEnumerable<LeaderTitleDef> ApplicableLeaderTitles => DefDatabase<LeaderTitleDef>.AllDefs.Where(def => def.IsApplicable);

        public static string LeaderTitle =>
            (ModsConfig.IdeologyActive && !RimocracyComp.DecisionActive(DecisionDef.Multiculturalism)
            ? IdeologyLeaderPrecept()?.Label
            : RimocracyComp?.LeaderTitleDef?.GetTitle(RimocracyComp.Leader)) ?? "leader";

        public static int TermDurationTicks => RimocracyComp.TermDuration.GetDurationTicks();

        public static string DateFullStringWithHourAtHome(long tick)
        {
            Map playerMap = Find.AnyPlayerHomeMap;
            return GenDate.DateFullStringWithHourAt(tick, playerMap != null ? Find.WorldGrid.LongLatOf(playerMap.Tile) : default);
        }

        public static Precept_RoleSingle IdeologyLeaderPrecept(Ideo ideo = null) =>
            (ideo ?? NationPrimaryIdeo).GetAllPreceptsOfType<Precept_RoleSingle>().FirstOrDefault(p => p.def == PreceptDefOf.IdeoRole_Leader);

        public static bool RoleRequirementsMetPotentially(Pawn pawn, Precept_Role role)
        {
            if (role?.def?.roleRequirements == null)
                return false;
            for (int i = 0; i < role.def.roleRequirements.Count; i++)
                if (!(role.def.roleRequirements[i] is RoleRequirement_Leader) && !role.def.roleRequirements[i].Met(pawn, role))
                    return false;
            return true;
        }

        public static bool CanBeLeader(this Pawn p) =>
            p.IsCitizen()
            && !p.GetDisabledWorkTypes(true).Contains(RimocracyDefOf.Governing)
            && (!ModsConfig.IdeologyActive || RimocracyComp.DecisionActive(DecisionDef.Multiculturalism) || RoleRequirementsMetPotentially(p, IdeologyLeaderPrecept()));

        public static bool IsLeader(this Pawn p) => p != null && RimocracyComp?.Leader == p;
        
        /// <summary>
        /// Returns pawn's most senior title's seniority, with no titles at list being -100
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static int GetTitleSeniority(this Pawn pawn)
        {
            RoyalTitle royalTitle = pawn.royalty.MostSeniorTitle;
            return royalTitle != null ? royalTitle.def.seniority : -100;
        }

        public static bool IsPowerStarved(this Building building)
        {
            CompPowerTrader comp = building?.GetComp<CompPowerTrader>();
            return comp != null && !comp.PowerOn;
        }

        public static float GovernanceImprovementSpeed(Pawn pawn, Thing worktable) =>
            pawn.GetStatValue(RimocracyDefOf.GovernEfficiency) * worktable.GetStatValue(RimocracyDefOf.GovernEfficiencyFactor);

        public static int GetDurationTicks(this TermDuration termDuration)
        {
            switch (termDuration)
            {
                case TermDuration.Quadrum:
                    return GenDate.TicksPerQuadrum;

                case TermDuration.Halfyear:
                    return GenDate.TicksPerYear / 2;

                case TermDuration.Year:
                    return GenDate.TicksPerYear;

                default:
                    return int.MaxValue;
            }
        }

        public static float GetOpinionOf(this Pawn pawn, Pawn target)
        {
            if (pawn == null || target == null)
                return 0;
            if (pawn == target)
                return 100;
            return Mathf.Clamp(pawn.needs.mood.thoughts.TotalOpinionOffset(target), -100, 100);
        }

        public static float MedianCitizensOpinion(this Pawn pawn) =>
            Citizens.Where(p => p != pawn).Select(p => (float)p.needs.mood.thoughts.TotalOpinionOffset(pawn)).Median();

        public static float MedianMood => Citizens.Select(pawn => pawn.needs.mood.CurLevelPercentage).Median();

        public static float Median(this List<float> values)
        {
            if (values.EnumerableNullOrEmpty())
                return 0;
            values.Sort();
            return values.Count % 2 == 0 ? (values[values.Count / 2 - 1] + values[values.Count / 2]) / 2 : values[values.Count / 2];
        }

        public static float Median(this IEnumerable<float> values) => Median(values.ToList());

        public static string ColorizeByValue(this string text, float value, Color color1, Color color2, Color color3, float lowValue = 0, float highValue = 0) =>
            text.Colorize(value < lowValue ? color1 : (value <= highValue ? color2 : color3));

        public static string ColorizeByValue(this string text, float value, float lowValue = 0, float highValue = 0) =>
            text.ColorizeByValue(value, Color.red, Color.gray, Color.green, lowValue, highValue);

        public static string ColorizeOpinion(this string text, float support) => text.ColorizeByValue(support, -0.5f, 0.5f);

        public static string ColorizeOpinion(this float support) => support.ToStringWithSign("0").ColorizeOpinion(support);

        internal static void Log(string message, LogLevel logLevel = LogLevel.Message)
        {
            message = $"[Rimocracy] {message}";
            switch (logLevel)
            {
                case LogLevel.Message:
                    if (Settings.DebugLogging || Prefs.LogVerbose)
                        Verse.Log.Message(message);
                    break;

                case LogLevel.Warning:
                    Verse.Log.Warning(message);
                    break;

                case LogLevel.Error:
                    Verse.Log.Error(message);
                    break;
            }
        }
    }
}
