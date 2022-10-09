using Verse;

namespace Rimocracy
{
    internal class Logic_TitleSeniority : Logic_Value
    {
        public override string DefaultLabel => "{PAWN}'s title seniority";

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => pawn?.royalty != null;

        public override float UnderlyingValue(Pawn pawn, Pawn target) => pawn.GetTitleSeniority();
    }
}
