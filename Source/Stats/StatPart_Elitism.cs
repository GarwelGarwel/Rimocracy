using RimWorld;
using Verse;

namespace Rimocracy
{
    public class StatPart_Elitism : StatPart_Mood
    {
        public override string ExplanationPart(StatRequest req) => AppliesTo(req) ? base.ExplanationPart(req) : null;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (AppliesTo(req))
                base.TransformValue(req, ref val);
        }

        bool AppliesTo(StatRequest req) =>
            Utility.PoliticsEnabled && (req.Thing is Pawn && (req.Thing as Pawn).IsLeader()) && Utility.RimocracyComp.DecisionActive("Elitism");
    }
}
