using Verse;

namespace Rimocracy
{
    public class Logic_Age : Logic_Value
    {
        public override string DefaultLabel => "{PAWN}'s age";

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => pawn?.ageTracker != null;

        public override float UnderlyingValue(Pawn pawn, Pawn target) => pawn.ageTracker.AgeBiologicalYears;
    }
}
