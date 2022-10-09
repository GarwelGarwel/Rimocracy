using Verse;

namespace Rimocracy
{
    internal class Logic_LeaderExists : Logic_Simple
    {
        bool leaderExists;

        public override string SlateRef
        {
            get => leaderExists.ToString();
            set => leaderExists = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "leader exists" : "leader does not exist";

        public override string DefaultLabel => GetLabel(leaderExists);

        public override string LabelInverted => GetLabel(!leaderExists);

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => Utility.RimocracyComp.HasLeader == leaderExists;
    }
}
