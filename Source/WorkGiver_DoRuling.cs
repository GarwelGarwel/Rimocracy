using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Rimocracy
{
    class WorkGiver_DoRuling : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => pawn.Map.listerBuildings.allBuildingsColonist;

        public override bool ShouldSkip(Pawn pawn, bool forced = false) => Rimocracy.Instance.Leader != pawn || Rimocracy.Instance.Authority >= 1;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) 
            => (new string[]
            {
                "RimWorld.Building_ResearchBench",
                "FluffyManager.Building_ManagerStation"
            }.Contains(t.def.thingClass.FullName))
            && pawn.CanReserve(t, ignoreOtherReservations: forced);

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("DoRuling"), t);
    }
}
