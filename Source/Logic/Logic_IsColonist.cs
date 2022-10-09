using Verse;

namespace Rimocracy
{
    public class Logic_IsColonist : Logic_Simple
    {
        bool isColonist;

        public override string SlateRef
        {
            get => isColonist.ToString();
            set => isColonist = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "{PAWN} is a colonist" : "{PAWM} is not a colonist";

        public override string DefaultLabel => GetLabel(isColonist);

        public override string LabelInverted => GetLabel(!isColonist);

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => pawn.IsColonist == isColonist;
    }
}
