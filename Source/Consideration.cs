using Verse;

namespace Rimocracy
{
    public class Consideration : Requirement
    {
        string label;
        float support;

        bool? isLeader;
        CompareFloat opinionOfLeader;

        public override bool IsTrivial => base.IsTrivial && isLeader == null;

        public bool IsActive(Pawn pawn)
        {
            bool res = base.GetValueBeforeInversion();
            if (isLeader != null)
                res &= pawn.IsLeader() == isLeader;
            if (opinionOfLeader != null)
                res &= opinionOfLeader.Compare(Utility.GetOpinionOfLeader(pawn));
            return res ^ inverted;
        }

        public float GetSupportValue(Pawn pawn) => IsActive(pawn) ? support : 0;

        public string ExplanationPart(Pawn pawn) => IsActive(pawn) ? $"{label}: {support.ToStringWithSign()}" : null;
    }
}
