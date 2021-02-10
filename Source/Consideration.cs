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

        bool? isLeader;
        TraitDef trait;
        List<SkillOperations> skills = new List<SkillOperations>();
        ValueOperations opinionOfLeader;
        ValueOperations medianOpinionOfLeader;
        ValueOperations medianOpinionOfMe;
        ValueOperations age;
        ValueOperations titleSeniority;

        public override bool IsTrivial =>
            base.IsTrivial 
            && isLeader == null
            && trait == null
            && skills.NullOrEmpty()
            && opinionOfLeader == null
            && medianOpinionOfLeader == null
            && medianOpinionOfMe == null
            && age == null
            && titleSeniority == null;

        protected override bool IsSatisfied_Internal(Pawn pawn)
        {
            bool res = base.IsSatisfied_Internal();
            if (pawn == null)
                return res;
            if (isLeader != null)
                res &= pawn.IsLeader() == isLeader;
            if (trait != null && pawn?.story?.traits != null)
                res &= pawn.story.traits.HasTrait(trait);
            if (!skills.NullOrEmpty())
                res &= skills.TrueForAll(so => so.Compare(pawn));
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
            return res;
        }

        public Tuple<float, string> GetSupportAndExplanation(Pawn pawn) => IsSatisfied(pawn)
                ? new Tuple<float, string>(GetSupportValue(pawn), ExplanationPart(pawn))
                : new Tuple<float, string>(0, null);

        float GetSupportValue(Pawn pawn)
        {
            float s = support;
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
            return s;
        }

        string ExplanationPart(Pawn pawn) =>
            $"{label}: {GetSupportValue(pawn).ToStringWithSign("0")}".Formatted(pawn.Named("PAWN"), Utility.RimocracyComp.Leader.Named("LEADER"));
    }
}
