using Verse;

namespace Rimocracy
{
    internal class Logic_IdeoCertainty : Logic_Value
    {
        public override string DefaultLabel => "{PAWN}'s certainty in {PAWN_possessive} ideoligion";

        protected override string NumberFormat => "P0";

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => pawn?.ideo != null;

        public override float UnderlyingValue(Pawn pawn, Pawn target) => pawn.ideo.Certainty;
    }
}
