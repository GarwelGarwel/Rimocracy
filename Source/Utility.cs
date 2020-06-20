using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    enum LogLevel { Message = 0, Warning, Error };

    static class Utility
    {
        /// <summary>
        /// Returns a list of skills that affect the given stat
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public static List<SkillDef> GetRelevantSkills(StatDef stat)
        {
            List<SkillDef> skills = stat.skillNeedFactors.NullOrEmpty()
                ? new List<SkillDef>()
                : stat.skillNeedFactors.Select(sn => sn.skill).ToList();
            if (!stat.skillNeedOffsets.NullOrEmpty())
                skills.AddRange(stat.skillNeedOffsets.Select(sn => sn.skill));
            return skills;
        }

        /// <summary>
        /// Returns a skill from the list weighed by squares of skill levels + 1 (except those completely disabled)
        /// </summary>
        /// <param name="skills">List of SkillRecords of the pawn</param>
        /// <param name="luckySkill">SkillDef whose chances are doubled</param>
        /// <returns></returns>
        public static SkillDef GetRandomSkill(List<SkillRecord> skills, SkillDef luckySkill)
            => skills
            .Where(sr => !sr.TotallyDisabled)
            .Select(sr => new KeyValuePair<SkillDef, float>(sr.def, Rand.Range(0, (sr.Level + 1) * (sr.def == luckySkill ? 2 : 1))))
            .MaxBy(kvp => kvp.Value)
            .Key;

        public static bool IsLeader(this Pawn p) => Rimocracy.IsEnabled && Rimocracy.Instance.Leader == p;

        public static void Log(string message, LogLevel logLevel = LogLevel.Message)
        {
            message = "[Rimocracy] " + message;
            switch (logLevel)
            {
                case LogLevel.Message:
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
