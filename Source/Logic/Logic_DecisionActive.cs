using Verse;

namespace Rimocracy
{
    public class Logic_DecisionActive : Logic_Simple
    {
        public override string DefaultLabel => $"{GenText.SplitCamelCase(SlateRef)} is active";

        public override string LabelInverted => $"{GenText.SplitCamelCase(SlateRef)} is not active";

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => Utility.RimocracyComp.DecisionActive(SlateRef);
    }
}
