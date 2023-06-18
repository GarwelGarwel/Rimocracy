using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Rimocracy
{
    class JobDriver_Govern : JobDriver
    {
        public const float JobDurationHours = 2;
        public const int JobDurationTicks = (int)(GenDate.TicksPerHour * JobDurationHours);

        bool isSitting = false;
        RimocracyComp comp = Utility.RimocracyComp;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(TargetThingA, job);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            AddFailCondition(() => !pawn.IsLeader());
            ThingComp_GoverningBench benchComp = TargetThingA?.TryGetComp<ThingComp_GoverningBench>();
            if (benchComp == null)
            {
                if (TargetThingA?.def != null)
                    Utility.Log($"Can't govern at {TargetThingA.def.defName}: it has no ThingComp_GoverningBench.", LogLevel.Error);
                else Utility.Log($"Tried to govern from a nonexistent governing bench.", LogLevel.Error);
                yield return null;
            }
            AddFailCondition(() => !benchComp.AllowGoverning);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            if (TargetThingA.def.building != null && TargetThingA.def.building.isSittable)
            {
                isSitting = true;
                yield return Toils_General.Do(() => job.SetTarget(TargetIndex.B, TargetThingA.InteractionCell + TargetThingA.Rotation.FacingCell));
            }
            Toil governToil = new Toil
            {
                tickAction = Govern_TickAction,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = JobDurationTicks,
                activeSkill = () => SkillDefOf.Social
            };
            if (!job.playerForced)
                governToil.AddEndCondition(() => comp.Governance >= comp.GovernanceTarget ? JobCondition.Succeeded : JobCondition.Ongoing);
            governToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            governToil.WithProgressBar(TargetIndex.A, () => comp.Governance / comp.GovernanceTarget);
            yield return governToil;
            yield return Toils_General.Wait(2);
        }

        public override string GetReport() => $"{base.GetReport()}\r\nImproving governance at {GovernanceImprovementSpeed.ToStringPercent()} per hour.";

        float GovernanceImprovementSpeed => Utility.GovernanceImprovementSpeed(pawn, TargetA.Thing);

        void Govern_TickAction()
        {
            if (isSitting)
                rotateToFace = TargetIndex.B;
            comp.ChangeGovernance(GovernanceImprovementSpeed / GenDate.TicksPerHour);
            pawn.skills.Learn(SkillDefOf.Social, 0.1f);
            pawn.GainComfortFromCellIfPossible(true);
        }
    }
}
