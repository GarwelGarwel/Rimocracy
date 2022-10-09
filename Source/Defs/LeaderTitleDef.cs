using Verse;

namespace Rimocracy
{
    public class LeaderTitleDef : Def
    {
        public string labelMale;

        public string labelFemale;

        public Logic_Consideration requirements = Logic_Consideration.always;

        public bool IsApplicable => requirements.IsSatisfied();

        public string GetTitle(Pawn pawn)
        {
            if (pawn == null)
                return label ?? labelMale;
            return (pawn.gender == Gender.Female ? labelFemale : labelMale) ?? label;
        }
    }
}
