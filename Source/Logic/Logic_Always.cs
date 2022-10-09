using Verse;

namespace Rimocracy
{
    public class Logic_Always : Logic
    {
        public override string DefaultLabel => "always";

        public override string LabelInverted => "never";

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => true;
    }
}
