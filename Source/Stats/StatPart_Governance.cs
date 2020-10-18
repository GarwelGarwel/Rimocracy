using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class StatPart_Governance : StatPart
    {
        public float minValue = 1;
        public float maxValue = 1;
        public bool focusOnly = false;

        public override string ExplanationPart(StatRequest req)
        {
            float mult = Multiplier(req) * 100;
            return mult != 100
                ? $"Governance {Utility.RimocracyComp.GovernancePercentage:N0}%: x{mult:N0}%"
                : null;
        }

        public override void TransformValue(StatRequest req, ref float val) => val *= Multiplier(req);

        float Multiplier(StatRequest req)
        {
            // Only applies to player's buildings and free colonists
            if (!req.HasThing || !Utility.PoliticsEnabled || !((req.Thing is Pawn && ((Pawn)req.Thing).IsCitizen()) || (req.Thing is Building && req.Thing.Faction != null && req.Thing.Faction.IsPlayer)))
                return 1;
            float effect = Utility.RimocracyComp.Governance;

            // If the effect is skill-based, check if the parent stat's skills include the current focus skill
            if (focusOnly)
            {
                HashSet<SkillDef> skills = new HashSet<SkillDef>(SkillsUtility.GetSkills(parentStat));
                CompProperties_AffectedByGovernance rs = (req.Thing as ThingWithComps).GetComp<ThingComp_AffectedByGovernance>()?.Props;
                if (rs?.Skills != null)
                    skills.AddRange(rs.Skills);
                if (!skills.Contains(Utility.RimocracyComp.FocusSkill))
                    return 1;
                effect /= skills.Count();
            }

            return Mathf.Lerp(minValue, maxValue, effect);
        }
    }
}
