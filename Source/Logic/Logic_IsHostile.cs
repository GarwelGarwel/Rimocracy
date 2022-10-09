using RimWorld;
using Verse;

namespace Rimocracy
{
    internal class Logic_IsHostile : Logic_Simple
    {
        bool inAggroMentalState;

        public override string SlateRef
        {
            get => inAggroMentalState.ToString();
            set => inAggroMentalState = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "{TARGET} is hostile to {PAWN}" : "{TARGET} is not hostile to {PAWN}";

        public override string DefaultLabel => GetLabel(inAggroMentalState);

        public override string LabelInverted => GetLabel(!inAggroMentalState);

        public override bool ValidFor(Pawn pawn = null, Pawn target = null) => pawn != null && target != null;

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => target.HostileTo(pawn);
    }
}
