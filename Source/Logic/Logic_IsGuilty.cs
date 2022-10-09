using Verse;

namespace Rimocracy
{
    public class Logic_IsGuilty : Logic_Simple
    {
        bool isGuilty;

        public override string SlateRef
        {
            get => isGuilty.ToString();
            set => isGuilty = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "{PAWN} is guilty" : "{PAWM} is not guilty";

        public override string DefaultLabel => GetLabel(isGuilty);

        public override string LabelInverted => GetLabel(!isGuilty);

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => pawn.guilt.IsGuilty;
    }
}
