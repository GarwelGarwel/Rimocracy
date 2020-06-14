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
                ? "Authority " + Rimocracy.Instance.AuthorityPercentage.ToString("N0") + "%: x" + mult.ToString("F2")
                : null;
        }

        public override void TransformValue(StatRequest req, ref float val) => val *= Multiplier(req);

        float Multiplier(StatRequest req)
        {
            // Only applies to free colonists
            if (!(Rimocracy.Instance.IsEnabled && req.Thing is Pawn p && p.IsFreeColonist))
                return 1;
            float effect = Rimocracy.Instance.Authority;

            // If the effect is skill-based, check if the parent stat's skills include the current focus skill
            if (focusOnly)
            {
                List<SkillDef> skills = parentStat.skillNeedFactors.NullOrEmpty() ? new List<SkillDef>() : parentStat.skillNeedFactors.Select(sn => sn.skill).ToList();
                if (!parentStat.skillNeedOffsets.NullOrEmpty())
                    skills.AddRange(parentStat.skillNeedOffsets.Select(sn => sn.skill));
                if (skills.EnumerableNullOrEmpty())
                    Utility.Log("Stat " + parentStat + " is not skill-based, but has a skill-based StatPart_Authority.", LogLevel.Warning);
                if (!skills.Contains(Rimocracy.Instance.FocusSkill))
                    return 1;
                effect /= skills.Count();
            }

            return Mathf.Lerp(minValue, maxValue, effect);
        }
    }
}
