﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
        static bool? simpleSlaveryInstalled = null;

        public static RimocracyComp RimocracyComp => Find.World?.GetComponent<RimocracyComp>();

        public static bool PoliticsEnabled => RimocracyComp != null && RimocracyComp.IsEnabled;

        public static bool IsSimpleSlaveryInstalled => (bool)(simpleSlaveryInstalled ?? (simpleSlaveryInstalled = RimocracyDefOf.Enslaved != null));

        public static IEnumerable<Pawn> Citizens =>
            PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Where(p => p.IsCitizen());

        public static int CitizensCount => Citizens.Count();

        public static int Population => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep.Count();

        public static float TotalNutrition => Find.Maps.Where(map => map.mapPawns.AnyColonistSpawned).Sum(map => map.resourceCounter.TotalHumanEdibleNutrition);

        public static float FoodConsumptionPerDay =>
            PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep.Sum(pawn => pawn.needs.food.FoodFallPerTick) * GenDate.TicksPerDay;

        public static float DaysOfFood => TotalNutrition / FoodConsumptionPerDay;

        public static string NationName => Find.FactionManager.OfPlayer.Name;

        public static IEnumerable<LeaderTitleDef> ApplicableLeaderTitles => DefDatabase<LeaderTitleDef>.AllDefs.Where(def => def.IsApplicable);

        public static string LeaderTitle => RimocracyComp?.LeaderTitleDef?.GetTitle(RimocracyComp.Leader) ?? "leader";

        public static int TermDurationTicks => RimocracyComp.TermDuration.GetDurationTicks();

        public static string DateFullStringWithHourAtHome(long tick) =>
            GenDate.DateFullStringWithHourAt(tick, Find.WorldGrid.LongLatOf(Find.AnyPlayerHomeMap.Tile));

        public static bool IsCitizen(this Pawn pawn) =>
            pawn != null
            && !pawn.Dead
            && pawn.IsFreeColonist
            && pawn.ageTracker.AgeBiologicalYears >= Settings.CitizenshipAge
            && (!IsSimpleSlaveryInstalled || !pawn.health.hediffSet.hediffs.Any(hediff => hediff.def == RimocracyDefOf.Enslaved));

        public static bool CanBeLeader(this Pawn p) => p.IsCitizen() && !p.GetDisabledWorkTypes(true).Contains(RimocracyDefOf.Governing);

        public static bool IsLeader(this Pawn p) => PoliticsEnabled && RimocracyComp.Leader == p;

        /// <summary>
        /// Returns pawn's most senior title's seniority, with no titles at all being -100
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

        public static float GetRegimeEffect(this TermDuration termDuration)
        {
            switch (termDuration)
            {
                case TermDuration.Quadrum:
                    return 0.05f;

                case TermDuration.Halfyear:
                    return 0;

                case TermDuration.Year:
                    return -0.05f;

                default:
                    return -0.10f;
            }
        }

        public static float GetOpinionOf(this Pawn pawn, Pawn target)
        {
            if (pawn == null || target == null)
                return 0;
            if (pawn == target)
                return 100;
            return pawn.needs.mood.thoughts.TotalOpinionOffset(target);
        }

        public static float MedianCitizensOpinion(this Pawn pawn) =>
             Citizens.Where(p => p != pawn).Select(p => (float)p.needs.mood.thoughts.TotalOpinionOffset(pawn)).Median();

        public static float Median(this IEnumerable<float> values)
        {
            if (values.EnumerableNullOrEmpty())
                return 0;
            List<float> list = values.OrderBy(v => v).ToList();
            int count = list.Count;
            return count % 2 == 0 ? (list[count / 2 - 1] + list[count / 2]) / 2 : list[count / 2];
        }

        internal static void Log(string message, LogLevel logLevel = LogLevel.Message)
        {
            message = $"[Rimocracy] {message}";
            switch (logLevel)
            {
                case LogLevel.Message:
                    if (Settings.DebugLogging)
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
