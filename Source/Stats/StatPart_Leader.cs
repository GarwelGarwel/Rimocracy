using RimWorld;
using Verse;

namespace Rimocracy
{
    class StatPart_Leader : StatPart
    {
        public float factor = 1;

        public float offset;

        public override string ExplanationPart(StatRequest req)
        {
            if (AppliesTo(req))
            {
                string res = "";
                if (factor != 1)
                    res = $"Leader: x{factor.ToStringPercent()}\n";
                if (offset != 0)
                    res = $"{res}Leader: {offset.ToStringWithSign()}";
                return res.TrimEndNewlines();
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (AppliesTo(req))
                val = val * factor + offset;
        }

        protected bool AppliesTo(StatRequest req) => Utility.PoliticsEnabled && (req.Thing as Pawn).IsLeader();
    }
}
