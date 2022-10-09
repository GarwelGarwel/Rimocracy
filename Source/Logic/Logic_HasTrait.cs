using RimWorld;
using Verse;

namespace Rimocracy
{
    public class Logic_HasTrait : Logic_Simple
    {
        int degree;

        public override string SlateRef
        {
            get => $"{base.SlateRef}:{degree}";
            set
            {
                string[] parts = value.Split(':');
                if (parts.Length > 1)
                {
                    base.SlateRef = parts[0];
                    degree = int.Parse(parts[1]);
                }
            }
        }

        TraitDef Trait => DefDatabase<TraitDef>.GetNamedSilentFail(SlateRef);

        public override string DefaultLabel => $"{{PAWN}} has trait {Trait.LabelCap}{(degree != 0 ? $" of {Trait.DataAtDegree(degree)?.LabelCap}" : "")}";

        public override string LabelInverted => $"{{PAWN}} doesn't have trait {Trait.LabelCap}{(degree != 0 ? $" of {Trait.DataAtDegree(degree)?.LabelCap}" : "")}";

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => Trait != null && pawn?.story?.traits != null;

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => pawn.story.traits.HasTrait(Trait, degree);
    }
}
