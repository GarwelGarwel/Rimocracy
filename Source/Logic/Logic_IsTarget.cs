using Verse;

namespace Rimocracy
{
    internal class Logic_IsTarget : Logic_Simple
    {
        bool isTarget;

        public override string SlateRef
        {
            get => isTarget.ToString();
            set => isTarget = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "leader exists" : "leader does not exist";

        public override string DefaultLabel => GetLabel(isTarget);

        public override string LabelInverted => GetLabel(!isTarget);

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => (pawn == target) == isTarget;
    }
}
