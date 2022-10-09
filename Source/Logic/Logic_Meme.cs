using RimWorld;
using Verse;

namespace Rimocracy
{
    public class Logic_Meme : Logic_Simple
    {
        public override string DefaultLabel => $"ideoligion has meme {MemeDef?.LabelCap ?? SlateRef}";

        public override string LabelInverted => $"ideoligion doesn't have meme {MemeDef?.LabelCap ?? SlateRef}";

        MemeDef MemeDef => DefDatabase<MemeDef>.GetNamedSilentFail(SlateRef);

        Ideo Ideo(Pawn pawn) => pawn != null ? pawn.Ideo : Utility.NationPrimaryIdeo;

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => Ideo(pawn) != null;

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target)
        {
            Ideo ideo = Ideo(pawn);
            for (int i = 0; i < ideo.memes.Count; i++)
                if (ideo.memes[i].defName == SlateRef)
                    return true;
            return false;
        }
    }
}
