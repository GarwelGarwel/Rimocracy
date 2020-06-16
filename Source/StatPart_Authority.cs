using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class StatPart_Authority : StatPart
    {
        public float minValue = 1;
        public float maxValue = 1;
        public bool focusOnly = false;

        public override string ExplanationPart(StatRequest req)
        {
            float mult = Multiplier(req);
            return mult != 1
                ? "Authority " + Rimocracy.Instance.AuthorityPercentage.ToString("N0") + "%: x" + mult.ToString("P0")
                : null;
        }

        public override void TransformValue(StatRequest req, ref float val) => val *= Multiplier(req);

        static List<SkillDef> GetRelevantSkills(StatDef stat)
        {
            List<SkillDef> skills = stat.skillNeedFactors.NullOrEmpty()
                ? new List<SkillDef>()
                : stat.skillNeedFactors.Select(sn => sn.skill).ToList();
            if (!stat.skillNeedOffsets.NullOrEmpty())
                skills.AddRange(stat.skillNeedOffsets.Select(sn => sn.skill));
            return skills;
        }

        float Multiplier(StatRequest req)
        {
            // Only applies to free colonists
            if (!(Rimocracy.Instance.IsEnabled && req.Thing is Pawn p && p.IsFreeColonist))
                return 1;
            float effect = Rimocracy.Instance.Authority;

            // If the effect is skill-based, check if the parent stat's skills include the current focus skill
            if (focusOnly)
            {
                List<SkillDef> skills = GetRelevantSkills(parentStat);
                if (!skills.Contains(Rimocracy.Instance.FocusSkill))
                    return 1;
                effect /= skills.Count();
            }

            return Mathf.Lerp(minValue, maxValue, effect);
        }
    }
}
