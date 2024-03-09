using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Logic_TechLevel : Logic_Simple
    {
        TechLevel level = TechLevel.Undefined;
        TechLevel higherOrEqual = TechLevel.Undefined;
        List<TechLevel> any = new List<TechLevel>();

        string LevelCommaList =>
            level != TechLevel.Undefined ? level.ToStringHuman()
            : (higherOrEqual != TechLevel.Undefined ? $"at least {higherOrEqual.ToStringHuman()}"
            : any.Select(techLevel => techLevel.ToStringHuman()).ToCommaListOr());

        public override string DefaultLabel => $"tech level is {LevelCommaList}";

        public override string LabelInverted => $"tech level is not {LevelCommaList}";

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target)
        {
            if (Faction.OfPlayer?.def == null)
                return false;
            TechLevel techLevel = Faction.OfPlayer.def.techLevel;
            return techLevel == level
                || (higherOrEqual != TechLevel.Undefined && techLevel >= higherOrEqual)
                || any.Contains(techLevel);
        }
    }
}
