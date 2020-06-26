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
            float mult = Multiplier(req) * 100;
            return mult != 100
                ? "Authority " + Rimocracy.Instance.AuthorityPercentage.ToString("N0") + "%: x" + mult.ToString("N0") + "%"
                : null;
        }

        public override void TransformValue(StatRequest req, ref float val) => val *= Multiplier(req);

        float Multiplier(StatRequest req)
        {
            // Only applies to buildings and free colonists
            if (!req.HasThing || !Rimocracy.IsEnabled || !((req.Thing is Pawn && (req.Thing as Pawn).IsCitizen()) || (req.Thing is Building && req.Thing.Faction.IsPlayer)))
                return 1;
            float effect = Rimocracy.Instance.Authority;

            // If the effect is skill-based, check if the parent stat's skills include the current focus skill
            if (focusOnly)
            {
                HashSet<SkillDef> skills = new HashSet<SkillDef>(Utility.GetRelevantSkills(parentStat));
                CompProperties_AffectedByAuthority rs = (req.Thing as ThingWithComps).GetComp<ThingComp_AffectedByAuthority>()?.Props;
                if (rs?.Skills != null)
                    skills.AddRange(rs.Skills);
                if (!skills.Contains(Rimocracy.Instance.FocusSkill))
                    return 1;
                effect /= skills.Count();
            }

            return Mathf.Lerp(minValue, maxValue, effect);
        }
    }
}
