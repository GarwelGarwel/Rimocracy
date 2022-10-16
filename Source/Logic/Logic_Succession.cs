using Verse;

namespace Rimocracy
{
    internal class Logic_Succession : Logic_Simple
    {
        public SuccessionDef SuccessionDef
        {
            get => DefDatabase<SuccessionDef>.GetNamed(SlateRef);
            set => SlateRef = value.defName;
        }

        public override string DefaultLabel => $"succession law is {SuccessionDef?.LabelCap}";

        public override string LabelInverted => $"succession law is not {SuccessionDef?.LabelCap}";

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => Utility.RimocracyComp?.SuccessionType?.defName == SlateRef;
    }
}
