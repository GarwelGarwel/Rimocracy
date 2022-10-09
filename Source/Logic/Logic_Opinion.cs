using Verse;

namespace Rimocracy
{
    public class Logic_Opinion : Logic_Value
    {
        public override string DefaultLabel => "{PAWN}'s opinion of {TARGET}";

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => pawn != null && target != null;

        public override float UnderlyingValue(Pawn pawn, Pawn target) => pawn.GetOpinionOf(target);
    }
}
