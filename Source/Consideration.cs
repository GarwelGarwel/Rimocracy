using Verse;

namespace Rimocracy
{
    public class Consideration : Requirement
    {
        string label;
        float support;

        bool? isLeader;

        public override bool IsTrivial => base.IsTrivial && isLeader == null;

        public bool IsActive(Pawn pawn)
        {
            bool res = base.GetValueBeforeInversion();
            if (isLeader != null)
                res &= pawn.IsLeader() == isLeader;
            return res ^ inverted;
        }

        public float GetSupportValue(Pawn pawn) => IsActive(pawn) ? support : 0;

        public string ExplanationPart(Pawn pawn) => IsActive(pawn) ? $"{label}: {support.ToStringWithSign()}" : null;

        //Requirement_Personal requirement = Requirement_Personal.always;

        //public float GetSupportValue(Pawn pawn) => requirement.GetValue(pawn) ? support : 0;

        //public string ExplanationPart(Pawn pawn) => requirement.GetValue(pawn) ? $"{explanation}: {support.ToStringWithSign()}" : null;
    }
}
