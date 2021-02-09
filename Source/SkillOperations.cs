using RimWorld;
using Verse;

namespace Rimocracy
{
    public class SkillOperations : ValueOperations
    {
        SkillDef skill;

        public bool Compare(Pawn pawn) => base.Compare(pawn.skills.GetSkill(skill).Level);

        public void TransformValue(Pawn pawn, ref float valueToTransform) =>
            TransformValue(pawn.skills.GetSkill(skill).Level, ref valueToTransform);
    }
}
