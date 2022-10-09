using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Logic_TechLevel : Logic_Simple
    {
        TechLevel level = TechLevel.Undefined;
        List<TechLevel> any = new List<TechLevel>();

        string LevelCommaList => level != TechLevel.Undefined ? level.ToStringHuman() : any.Select(techLevel => techLevel.ToStringHuman()).ToCommaListOr();

        public override string DefaultLabel => $"tech level is {LevelCommaList}";

        public override string LabelInverted => $"tech level is not {LevelCommaList}";

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => Faction.OfPlayer.def.techLevel == level || any.Contains(Faction.OfPlayer.def.techLevel);
    }
}
