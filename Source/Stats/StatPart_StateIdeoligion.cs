using RimWorld;
using Verse;

namespace Rimocracy
{
    public class StatPart_StateIdeoligion : StatPart
    {
        public float factor = 1;

        public float offset;

        public override string ExplanationPart(StatRequest req)
        {
            if (AppliesTo(req))
            {
                string res = "";
                if (factor != 1)
                    res = $"State ideoligion: x{factor.ToStringPercent()}\n";
                if (offset != 0)
                    res = $"{res}State ideoligion: {offset.ToStringWithSign()}";
                return res.TrimEndNewlines();
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (AppliesTo(req))
                val = val * factor + offset;
        }

        protected bool AppliesTo(StatRequest req) =>
            Utility.PoliticsEnabled && Utility.RimocracyComp.DecisionActive(DecisionDef.StateIdeoligion) && req.Pawn?.Ideo != null && req.Pawn.Ideo == Utility.NationPrimaryIdeo;
    }
}
