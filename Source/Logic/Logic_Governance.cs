using Verse;

namespace Rimocracy
{
    internal class Logic_Governance : Logic_Value
    {
        public override string DefaultLabel => "governance";

        protected override string NumberFormat => "P0";

        public override float UnderlyingValue(Pawn pawn, Pawn target) => Utility.RimocracyComp.Governance;
    }
}
