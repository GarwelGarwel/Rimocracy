using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    enum LogLevel { Message = 0, Warning, Error };

    public static class Utility
    {
        static bool? simpleSlaveryInstalled = null;

        public static RimocracyComp Rimocracy => Find.World?.GetComponent<RimocracyComp>();

        public static bool PoliticsEnabled => Rimocracy != null && Rimocracy.IsEnabled;

        public static bool IsSimpleSlaveryInstalled =>
            (bool)(simpleSlaveryInstalled ?? (simpleSlaveryInstalled = RimocracyDefOf.Enslaved != null));

        public static IEnumerable<Pawn> Citizens =>
            PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Where(p => p.IsCitizen());

        public static bool IsCitizen(this Pawn pawn)
            => pawn != null
            && !pawn.Dead
            && pawn.IsFreeColonist
            && pawn.ageTracker.AgeBiologicalYears >= Settings.CitizenshipAge
            && (!IsSimpleSlaveryInstalled || !pawn.health.hediffSet.hediffs.Any(hediff => hediff.def == RimocracyDefOf.Enslaved));

        public static bool CanBeLeader(this Pawn p) => p.IsCitizen() && !p.GetDisabledWorkTypes(true).Contains(RimocracyDefOf.Ruling);

        public static bool IsLeader(this Pawn p) => PoliticsEnabled && Rimocracy.Leader == p;

        public static string ListString(List<string> list)
        {
            if (list.NullOrEmpty())
                return "";
            if (list.Count == 2)
                return list[0] + " and " + list[1];
            string res = list[0];
            for (int i = 1; i < list.Count; i++)
                res += (list.Count == i + 1 ? ", and " : ", ") + list[i];
            return res;
        }

        internal static void Log(string message, LogLevel logLevel = LogLevel.Message)
        {
            message = "[Rimocracy] " + message;
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
