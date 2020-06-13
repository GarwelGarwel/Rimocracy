using RimWorld;
using Verse;

namespace Rimocracy
{
    public class StatPart_Authority : StatPart
    {
        float AuthorityMultiplier => 0.9f + Rimocracy.Instance.Authority * 0.2f;

        public override string ExplanationPart(StatRequest req)
        {
            if (AppliesTo(req))
                return "Aurhority " + Rimocracy.Instance.AuthorityPercentage.ToString("N0") + "%: x" + AuthorityMultiplier.ToString("F2");
            return "StatPart_Authority doesn't apply to " + (req.Thing?.ToString() ?? "nothing") + ".";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (AppliesTo(req))
                val *= AuthorityMultiplier;
        }

        bool AppliesTo(StatRequest req) => req.Thing is Pawn p && p.IsFreeColonist;
    }
}
