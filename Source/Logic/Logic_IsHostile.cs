using RimWorld;
using Verse;

namespace Rimocracy
{
    internal class Logic_IsHostile : Logic_Simple
    {
        bool isHostile;

        public override string SlateRef
        {
            get => isHostile.ToString();
            set => isHostile = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "{TARGET} is hostile to {PAWN}" : "{TARGET} is not hostile to {PAWN}";

        public override string DefaultLabel => GetLabel(isHostile);

        public override string LabelInverted => GetLabel(!isHostile);

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => pawn != null && target != null;

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => target.HostileTo(pawn) == isHostile;
    }
}
