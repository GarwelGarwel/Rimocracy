using System;
using Verse;

namespace Rimocracy
{
    internal class Logic_TermDuration : Logic_Simple
    {
        public TermDuration TermDuration
        {
            get => Enum.TryParse(SlateRef, out TermDuration result) ? result : TermDuration.Undefined;
            set => SlateRef = value.ToString();
        }

        public override string DefaultLabel => $"term duration is {GenText.SplitCamelCase(TermDuration.ToString()).ToLower()}";

        public override string LabelInverted => $"term duration is not {GenText.SplitCamelCase(TermDuration.ToString()).ToLower()}";

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => Utility.RimocracyComp.TermDuration == TermDuration;
    }
}
