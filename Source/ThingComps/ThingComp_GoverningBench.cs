using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class ThingComp_GoverningBench : ThingComp
    {
        bool allowGoverning = true;

        public bool AllowGoverning => allowGoverning;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction == Faction.OfPlayer)
                yield return new Command_Toggle
                {
                    defaultLabel = "allow governing",
                    defaultDesc = "When active, this workbench can be used by your leader to govern the colony.",
                    icon = ContentFinder<Texture2D>.Get("AllowGoverning"),
                    isActive = () => allowGoverning,
                    toggleAction = () => allowGoverning = !allowGoverning
                };
        }

        public override void PostExposeData() => Scribe_Values.Look(ref allowGoverning, "allowGoverning", true);
    }
}
