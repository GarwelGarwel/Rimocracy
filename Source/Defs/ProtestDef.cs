using Verse;

namespace Rimocracy
{
    public class ProtestDef : Def
    {
        public Consideration weight = new Consideration(1);
        public MentalStateDef mentalState;
        public bool critical;

        public bool AppliesTo(Pawn pawn)
        {
            Need_Loyalty loyalty = pawn.needs.TryGetNeed<Need_Loyalty>();
            if (loyalty == null || loyalty.CurLevel >= loyalty.JoinProtestLevel || (critical && loyalty.CurLevel >= loyalty.StartProtestLevel))
                return false;
            return mentalState.Worker.StateCanOccur(pawn) && weight.IsSatisfied(pawn);
        }

        public TaggedString DescriptionFor(Pawn pawn) => description.Formatted(new NamedArgument(pawn, "PAWN"), new NamedArgument(Utility.NationName, "NATION"));
    }
}
