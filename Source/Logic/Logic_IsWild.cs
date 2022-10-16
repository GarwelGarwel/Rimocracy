using Verse;

namespace Rimocracy
{
    public class Logic_IsWild : Logic_Simple
    {
        bool isWild;

        public override string SlateRef
        {
            get => isWild.ToString();
            set => isWild = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "{PAWN} is wild" : "{PAWM} is not wild";

        public override string DefaultLabel => GetLabel(isWild);

        public override string LabelInverted => GetLabel(!isWild);

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => pawn.IsWildMan();
    }
}
