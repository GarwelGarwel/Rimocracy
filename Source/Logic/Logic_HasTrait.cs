using RimWorld;
using Verse;

namespace Rimocracy
{
    public class Logic_HasTrait : Logic_Simple
    {
        TraitDef trait;
        int degree;

        TraitDef Trait
        {
            get
            {
                if (trait == null)
                {
                    string[] parts = SlateRef.Split(':');
                    trait = DefDatabase<TraitDef>.GetNamedSilentFail(parts[0]);
                    if (trait == null)
                        Utility.Log($"Trait {parts[0]} not found.", LogLevel.Warning);
                    if (parts.Length > 1)
                        degree = int.Parse(parts[1]);
                }
                return trait;
            }
        }

        string TraitLabel => Trait?.LabelCap ?? $"{SlateRef} (trait)";

        public override string DefaultLabel => $"{{PAWN}} is {TraitLabel}{(degree != 0 ? $" of {Trait.DataAtDegree(degree)?.LabelCap}" : "")}";

        public override string LabelInverted => $"{{PAWN}} is not {TraitLabel}{(degree != 0 ? $" of {Trait.DataAtDegree(degree)?.LabelCap}" : "")}";

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => Trait != null && pawn?.story?.traits != null;

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => pawn.story.traits.HasTrait(Trait, degree);
    }
}
