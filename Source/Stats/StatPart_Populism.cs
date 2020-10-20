using RimWorld;
using Verse;

namespace Rimocracy
{
    public class StatPart_Populism : StatPart_Curve
    {
        protected override bool AppliesTo(StatRequest req) => Utility.PoliticsEnabled && Utility.RimocracyComp.DecisionActive("Populism");

        protected override float CurveXGetter(StatRequest req) => (req.Thing as Pawn).MedianCitizensOpinion();

        protected override string ExplanationLabel(StatRequest req) => $"Multiplier for median opinion {CurveXGetter(req):N0} (from Populism)";
    }
}
