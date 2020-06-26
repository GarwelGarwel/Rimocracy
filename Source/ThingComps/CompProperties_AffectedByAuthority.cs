using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Rimocracy
{
    public class CompProperties_AffectedByAuthority : CompProperties
    {
        List<SkillDef> skills;

        public CompProperties_AffectedByAuthority() => compClass = typeof(ThingComp_AffectedByAuthority);

        public List<SkillDef> Skills
        {
            get => skills;
            set => skills = value;
        }
    }
}
