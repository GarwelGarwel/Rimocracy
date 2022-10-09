using Verse;

namespace Rimocracy
{
    internal class Logic_Population : Logic_Value
    {
        public override string DefaultLabel => "population";

        public override float UnderlyingValue(Pawn pawn, Pawn target) => Utility.Population;
    }
}
