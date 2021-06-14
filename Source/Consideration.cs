using RimWorld;
using System;
using System.Collections.Generic;
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

        ValueOperations opinionOfLeader;
        ValueOperations medianOpinionOfLeader;
        ValueOperations medianOpinionOfMe;
        ValueOperations age;
        ValueOperations titleSeniority;

        TraitDef targetTrait;
        ValueOperations opinionOfTarget;
        ValueOperations targetAge;

        protected override bool IsSatisfied_Internal(Pawn pawn, Pawn target = null)
        {
            bool res = base.IsSatisfied_Internal(pawn, target);
            if (pawn == null)
                return res;
            Pawn leader = Utility.RimocracyComp.Leader;
            if (opinionOfLeader != null && leader != null)
                res &= opinionOfLeader.Compare(Utility.GetOpinionOfLeader(pawn));
            if (medianOpinionOfLeader != null && leader != null)
                res &= medianOpinionOfLeader.Compare(leader.MedianCitizensOpinion());
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
                if (targetAge != null && target.ageTracker != null)
                    res &= targetAge.Compare(target.ageTracker.AgeBiologicalYears);
            }
            return res;
        }

        public Tuple<float, string> GetSupportAndExplanation(Pawn pawn, Pawn target = null)
        {
            float s = GetSupport(pawn, target);
            return new Tuple<float, string>(s, s != 0 ? $"{label}: {s.ToStringWithSign("0")}".Formatted(pawn.Named("PAWN"), Utility.RimocracyComp.Leader.Named("LEADER"), target.Named("TARGET")) : null);
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
            Pawn leader = Utility.RimocracyComp.Leader;
            if (opinionOfLeader != null && leader != null)
                opinionOfLeader.TransformValue(Utility.GetOpinionOfLeader(pawn), ref s);
            if (medianOpinionOfLeader != null && leader != null)
                medianOpinionOfLeader.TransformValue(leader.MedianCitizensOpinion(), ref s);
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
                if (targetAge != null && target.ageTracker != null)
                    targetAge.TransformValue(target.ageTracker.AgeBiologicalYears, ref s);
            }
            return s;
        }
    }
}
