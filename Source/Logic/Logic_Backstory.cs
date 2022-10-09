using RimWorld;
using Verse;

namespace Rimocracy
{
    public class Logic_Backstory : Logic_Simple
    {
        BackstoryDef backstory;

        BackstoryDef Backstory
        {
            get
            {
                if (backstory == null)
                    backstory = DefDatabase<BackstoryDef>.GetNamedSilentFail(SlateRef);
                return backstory;
            }
        }

        string BackstoryLabel => Backstory?.title ?? $"{SlateRef} (backstory)";

        public override string DefaultLabel => BackstoryLabel;

        public override string LabelInverted => $"not {BackstoryLabel}";

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => pawn?.story?.Childhood != null || pawn?.story?.Adulthood != null;

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => Backstory != null && (pawn.story.Childhood == Backstory || pawn.story.Adulthood == Backstory);
    }
}
