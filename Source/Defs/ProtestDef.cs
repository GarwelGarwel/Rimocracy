using Verse;

namespace Rimocracy
{
    public class ProtestDef : Def
    {
        public Logic_Consideration weight = new Logic_Consideration(1);
        public MentalStateDef mentalState;

        public bool AppliesTo(Pawn pawn) => mentalState.Worker.StateCanOccur(pawn) && weight.IsSatisfied(pawn);

        public TaggedString DescriptionFor(Pawn pawn) =>
            description.Formatted(
                new NamedArgument(pawn, "PAWN"),
                new NamedArgument(Utility.NationName, "NATION"));
    }
}
