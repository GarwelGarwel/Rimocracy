using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Rimocracy
{
    class WorkGiver_Govern : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => pawn.Map.listerBuildings.allBuildingsColonist;

        public override bool ShouldSkip(Pawn pawn, bool forced = false) => !pawn.IsLeader();

        // Prefer more efficient stations
        public override float GetPriority(Pawn pawn, TargetInfo t) => t.Thing.GetStatValue(RimocracyDefOf.GovernEfficiencyFactor);

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!t.def.StatBaseDefined(RimocracyDefOf.GovernEfficiencyFactor)
                || !pawn.CanReserve(t, ignoreOtherReservations: forced)
                || Utility.IsPowerStarved(t as Building))
                return false;

            ThingComp_GoverningBench comp = t.TryGetComp<ThingComp_GoverningBench>();
            if (comp != null && !comp.AllowGoverning)
                return false;

            if (!forced && Utility.RimocracyComp.Governance >= (Utility.RimocracyComp.GovernanceTarget - Utility.GovernanceImprovementSpeed(pawn, t)))
            {
                JobFailReason.Is("Governance is already high enough.");
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => JobMaker.MakeJob(RimocracyDefOf.Govern, t);
    }
}
