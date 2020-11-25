using System;
using Verse;

namespace Rimocracy
{
    public class Consideration : Requirement
    {
        string label;
        float support;

        bool? isLeader;
        ValueOperations opinionOfLeader;
        ValueOperations medianOpinionOfLeader;
        ValueOperations medianOpinionOfMe;

        public override bool IsTrivial => base.IsTrivial && isLeader == null;

        public bool IsActive(Pawn pawn)
        {
            bool res = base.GetValueBeforeInversion();
            if (isLeader != null)
                res &= pawn.IsLeader() == isLeader;
            if (opinionOfLeader != null)
                res &= opinionOfLeader.Compare(Utility.GetOpinionOfLeader(pawn));
            Pawn leader = Utility.RimocracyComp.Leader;
            if (medianOpinionOfLeader != null)
                res &= medianOpinionOfLeader.Compare(leader != null ? leader.MedianCitizensOpinion() : 0);
            if (medianOpinionOfMe != null)
                res &= medianOpinionOfMe.Compare(pawn.MedianCitizensOpinion());
            return res ^ inverted;
        }

        public float GetSupportValue(Pawn pawn) => IsActive(pawn) ? GetSupportValue_NoCheck(pawn) : 0;

        public string ExplanationPart(Pawn pawn) => IsActive(pawn) ? ExplanationPart_NoCheck(pawn) : null;

        public Tuple<float, string> GetSupportAndExplanation(Pawn pawn) => IsActive(pawn)
                ? new Tuple<float, string>(GetSupportValue_NoCheck(pawn), ExplanationPart_NoCheck(pawn))
                : new Tuple<float, string>(0, null);

        float GetSupportValue_NoCheck(Pawn pawn)
        {
            float s = support;
            if (opinionOfLeader != null)
                opinionOfLeader.TransformValue(Utility.GetOpinionOfLeader(pawn), ref s);
            if (medianOpinionOfLeader != null)
                medianOpinionOfLeader.TransformValue(Utility.GetOpinionOfLeader(pawn), ref s);
            if (medianOpinionOfMe != null)
                medianOpinionOfMe.TransformValue(Utility.GetOpinionOfLeader(pawn), ref s);
            return s;
        }

        string ExplanationPart_NoCheck(Pawn pawn) => $"{label}: {support.ToStringWithSign()}";
    }
}
