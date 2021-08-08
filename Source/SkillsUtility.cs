using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public static class SkillsUtility
    {
        /// <summary>
        /// Returns a list of skills that affect the given stat
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public static List<SkillDef> GetSkills(StatDef stat)
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
        public static SkillDef GetRandomSkill(List<SkillRecord> skills, SkillDef luckySkill) =>
            skills.RandomElementByWeight(sr => (sr.Level + 1) * (sr.def == luckySkill ? 2 : 1)).def;
    }
}
