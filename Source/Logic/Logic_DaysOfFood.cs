using Verse;

namespace Rimocracy
{
    internal class Logic_DaysOfFood : Logic_Value
    {
        public override string DefaultLabel => "days worth of food";

        protected override string NumberFormat => "F1";

        public override float UnderlyingValue(Pawn pawn, Pawn target) => Utility.DaysOfFood;
    }
}
