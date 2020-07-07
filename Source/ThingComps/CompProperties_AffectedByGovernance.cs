using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Rimocracy
{
    public class CompProperties_AffectedByGovernance : CompProperties
    {
        List<SkillDef> skills;

        public CompProperties_AffectedByGovernance() => compClass = typeof(ThingComp_AffectedByGovernance);

        public List<SkillDef> Skills
        {
            get => skills;
            set => skills = value;
        }
    }
}
