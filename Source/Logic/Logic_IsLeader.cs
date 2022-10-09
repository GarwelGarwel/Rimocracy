using Verse;

namespace Rimocracy
{
    public class Logic_IsLeader : Logic_Simple
    {
        bool isLeader;

        public override string SlateRef
        {
            get => isLeader.ToString();
            set => isLeader = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "leader exists" : "leader does not exist";

        public override string DefaultLabel => GetLabel(isLeader);

        public override string LabelInverted => GetLabel(!isLeader);

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => pawn.IsLeader() == isLeader;
    }
}
