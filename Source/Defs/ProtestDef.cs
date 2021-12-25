using Verse;

namespace Rimocracy
{
    public class ProtestDef : Def
    {
        public string description;
        public float weight = 1;
        public MentalStateDef mentalState;
        public bool mustBeCritical;

        public bool AppliesTo(Pawn pawn)
        {
            Need_Loyalty loyalty = pawn.needs.TryGetNeed<Need_Loyalty>();
            if (loyalty == null || loyalty.CurLevel >= loyalty.JoinProtestLevel)
                return false;
            if (mustBeCritical && loyalty.CurLevel >= loyalty.StartProtestLevel)
                return false;
            return mentalState.Worker.StateCanOccur(pawn);
        }

        public TaggedString DescriptionFor(Pawn pawn) => description.Formatted(new NamedArgument(pawn, "PAWN"), new NamedArgument(Utility.NationName, "NATION"));
    }
}
