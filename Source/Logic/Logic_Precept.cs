using RimWorld;
using Verse;

namespace Rimocracy
{
    public class Logic_Precept : Logic_Simple
    {
        public override string DefaultLabel => $"ideoligion has precept {PreceptDef?.LabelCap ?? SlateRef}";

        public override string LabelInverted => $"ideoligion doesn't have precept {PreceptDef?.LabelCap ?? SlateRef}";

        PreceptDef PreceptDef => DefDatabase<PreceptDef>.GetNamedSilentFail(SlateRef);

        Ideo Ideo(Pawn pawn) => pawn != null ? pawn.Ideo : Utility.NationPrimaryIdeo;

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => Ideo(pawn) != null;

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target)
        {
            Ideo ideo = Ideo(pawn);
            for (int i = 0; i < ideo.PreceptsListForReading.Count; i++)
                if (ideo.PreceptsListForReading[i].def.defName == SlateRef)
                    return true;
            return false;
        }
    }
}
