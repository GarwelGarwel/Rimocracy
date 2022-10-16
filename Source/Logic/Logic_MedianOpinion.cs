using Verse;

namespace Rimocracy
{
    public class Logic_MedianOpinion : Logic_Value
    {
        public override string DefaultLabel => "citizens' median opinion of {PAWN}";

        protected override string NumberFormat => base.NumberFormat;

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => pawn != null;

        public override float UnderlyingValue(Pawn pawn, Pawn target) => pawn.MedianCitizensOpinion();
    }
}
