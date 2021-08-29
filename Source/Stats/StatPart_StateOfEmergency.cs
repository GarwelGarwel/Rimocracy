using RimWorld;
using Verse;

namespace Rimocracy
{
    public class StatPart_StateOfEmergency : StatPart
    {
        public float value = 0.25f;

        public override string ExplanationPart(StatRequest req)
        {
            float mult = Multiplier(req);
            return mult != 1
                ? $"State of Emergency: x{mult.ToStringPercent()}"
                : null;
        }

        public override void TransformValue(StatRequest req, ref float val) => val *= Multiplier(req);

        float Multiplier(StatRequest req) =>
            Utility.PoliticsEnabled
            && req.HasThing
            && (req.Thing as Pawn).IsCitizen()
            && Utility.RimocracyComp.DecisionActive(DecisionDef.StateOfEmergency)
            ? 0.25f : 1;
    }
}
