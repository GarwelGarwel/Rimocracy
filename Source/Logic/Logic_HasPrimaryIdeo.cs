using Verse;

namespace Rimocracy
{
    public class Logic_HasPrimaryIdeo : Logic_Simple
    {
        bool hasPrimaryIdeo;

        public override string SlateRef
        {
            get => hasPrimaryIdeo.ToString();
            set => hasPrimaryIdeo = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "{PAWN} has colony's primary ideoligion" : "{PAWN} has a non-primary ideoligion";

        public override string DefaultLabel => GetLabel(hasPrimaryIdeo);

        public override string LabelInverted => GetLabel(!hasPrimaryIdeo);

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => pawn?.Ideo != null && Utility.NationPrimaryIdeo != null;

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => (pawn?.Ideo == Utility.NationPrimaryIdeo) == hasPrimaryIdeo;
    }
}
