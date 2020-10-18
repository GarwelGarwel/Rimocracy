using RimWorld;
using Verse;

namespace Rimocracy
{
    public class StatPart_StateOfEmergency : StatPart
    {
        public float value = 0.25f;

        public override string ExplanationPart(StatRequest req)
        {
            float mult = Multiplier(req) * 100;
            return mult != 100
                ? $"State of Emergency: x{mult:N0}%"
                : null;
        }

        public override void TransformValue(StatRequest req, ref float val) => val *= Multiplier(req);

        float Multiplier(StatRequest req) =>
            Utility.PoliticsEnabled
            && req.HasThing
            && (req.Thing as Pawn).IsCitizen()
            && Utility.RimocracyComp.DecisionActive("StateOfEmergency")
            ? 0.25f : 1;
    }
}
