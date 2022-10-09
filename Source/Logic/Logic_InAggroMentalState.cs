using Verse;

namespace Rimocracy
{
    public class Logic_InAggroMentalState : Logic_Simple
    {
        bool inAggroMentalState;

        public override string SlateRef
        {
            get => inAggroMentalState.ToString();
            set => inAggroMentalState = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "{PAWN} is in an aggressive mental break" : "{PAWM} is not in an aggressive mental break";

        public override string DefaultLabel => GetLabel(inAggroMentalState);

        public override string LabelInverted => GetLabel(!inAggroMentalState);

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => pawn.InAggroMentalState;
    }
}
