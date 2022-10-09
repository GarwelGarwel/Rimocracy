using Verse;

namespace Rimocracy
{
    public class Logic_IsCapableOfViolence : Logic_Simple
    {
        bool isCapableOfViolence;

        public override string SlateRef
        {
            get => isCapableOfViolence.ToString();
            set => isCapableOfViolence = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "{PAWN} is capable of violence" : "{PAWN} is incapable of violence";

        public override string DefaultLabel => GetLabel(isCapableOfViolence);

        public override string LabelInverted => GetLabel(!isCapableOfViolence);

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => pawn.WorkTagIsDisabled(WorkTags.Violent) != isCapableOfViolence;
    }
}
