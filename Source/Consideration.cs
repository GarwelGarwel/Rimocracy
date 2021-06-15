using Verse;

namespace Rimocracy
{
    /// <summary>
    /// Defines conditions for supporting or opposing something (e.g. Decision) by a pawn, and the amount of that support
    /// </summary>
    public class Consideration : Requirement
    {
        string label;
        float support;

        ValueOperations medianOpinionOfMe;
        ValueOperations age;
        ValueOperations titleSeniority;

        ValueOperations opinionOfTarget;
        ValueOperations medianOpinionOfTarget;
        ValueOperations targetAge;

        public (float support, string explanation) GetSupportAndExplanation(Pawn pawn, Pawn target = null)
        {
            float s = GetSupport(pawn, target);
            return (s, s != 0 ? $"{label.Formatted(pawn.Named("PAWN"), target.Named("TARGET")).CapitalizeFirst()}: {s.ToStringWithSign("0")}" : null);
        }

        public float GetSupport(Pawn pawn, Pawn target = null)
        {
            if (!IsSatisfied(pawn, target))
                return 0;
            float s = support;
            if (governance != null)
                governance.TransformValue(Utility.RimocracyComp.Governance, ref s);
            if (regime != null)
                regime.TransformValue(Utility.RimocracyComp.RegimeFinal, ref s);
            foreach (SkillOperations so in skills)
                so.TransformValue(pawn, ref s);
            if (medianOpinionOfMe != null)
                medianOpinionOfMe.TransformValue(pawn.MedianCitizensOpinion(), ref s);
            if (age != null && pawn?.ageTracker != null)
                age.TransformValue(pawn.ageTracker.AgeBiologicalYears, ref s);
            if (titleSeniority != null && pawn?.royalty != null)
                titleSeniority.TransformValue(pawn.GetTitleSeniority(), ref s);
            if (target != null)
            {
                if (opinionOfTarget != null)
                    opinionOfTarget.TransformValue(pawn.GetOpinionOfPawn(target), ref s);
                if (medianOpinionOfTarget != null && target != null)
                    medianOpinionOfTarget.TransformValue(target.MedianCitizensOpinion(), ref s);
                if (targetAge != null && target.ageTracker != null)
                    targetAge.TransformValue(target.ageTracker.AgeBiologicalYears, ref s);
            }
            return s;
        }

        protected override bool IsSatisfied_Internal(Pawn pawn, Pawn target = null)
        {
            bool res = base.IsSatisfied_Internal(pawn, target);
            if (pawn == null)
                return res;
            if (medianOpinionOfMe != null)
                res &= medianOpinionOfMe.Compare(pawn.MedianCitizensOpinion());
            if (age != null && pawn?.ageTracker != null)
                res &= age.Compare(pawn.ageTracker.AgeBiologicalYears);
            if (titleSeniority != null && pawn?.royalty != null)
                res &= titleSeniority.Compare(pawn.GetTitleSeniority());
            if (target != null)
            {
                if (opinionOfTarget != null)
                    res &= opinionOfTarget.Compare(pawn.GetOpinionOfPawn(target));
                if (medianOpinionOfTarget != null && target != null)
                    res &= medianOpinionOfTarget.Compare(target.MedianCitizensOpinion());
                if (targetAge != null && target.ageTracker != null)
                    res &= targetAge.Compare(target.ageTracker.AgeBiologicalYears);
            }
            return res;
        }
    }
}
