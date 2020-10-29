using RimWorld;
using Verse;

namespace Rimocracy
{
    public class StatPart_Elitism : StatPart_Mood
    {
        public override string ExplanationPart(StatRequest req) => IsApplicable ? base.ExplanationPart(req) : null;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (IsApplicable)
                base.TransformValue(req, ref val);
        }

        bool IsApplicable => Utility.PoliticsEnabled && Utility.RimocracyComp.DecisionActive("Elitism");
    }
}
