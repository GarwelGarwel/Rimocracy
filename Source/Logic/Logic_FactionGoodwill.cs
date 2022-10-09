using Verse;

namespace Rimocracy
{
    internal class Logic_FactionGoodwill : Logic_Value
    {
        public override string DefaultLabel => "Goodwill of {PAWN}'s faction";

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => pawn.HomeFaction != null && !pawn.HomeFaction.IsPlayer;

        public override float UnderlyingValue(Pawn pawn, Pawn target) => pawn.HomeFaction.PlayerGoodwill;
    }
}
