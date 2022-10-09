using RimWorld;
using Verse;

namespace Rimocracy
{
    public class Logic_Skill : Logic_Value
    {
        public SkillDef skill;

        public override string DefaultLabel => skill.label;

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => pawn?.skills != null && skill != null;

        public override float UnderlyingValue(Pawn pawn, Pawn target) => pawn.skills.GetSkill(skill).Level;
    }
}
