using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Rimocracy
{
    class JobDriver_Govern : JobDriver
    {
        const int JobDuration = GenDate.TicksPerHour * 2;

        bool isSitting = false;
        RimocracyComp comp = Utility.RimocracyComp;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(TargetThingA, job);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
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
                defaultDuration = JobDuration,
                activeSkill = () => SkillDefOf.Social
            };
            governToil.FailOn(() => comp.Governance >= comp.GovernanceTarget);
            governToil.FailOn(() => !pawn.IsLeader());
            governToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            governToil.WithProgressBar(TargetIndex.A, () => comp.Governance / comp.GovernanceTarget);
            yield return governToil;
            yield return Toils_General.Wait(2, TargetIndex.None);
        }

        public override string GetReport() => $"{base.GetReport()}\r\nImproving governance at {GovernanceImprovementSpeed.ToStringPercent()} per hour.";

        float GovernanceImprovementSpeed => Utility.GovernanceImprovementSpeed(pawn, TargetA.Thing);

        void Govern_TickAction()
        {
            if (isSitting)
                rotateToFace = TargetIndex.B;
            comp.ChangeGovernance(GovernanceImprovementSpeed / GenDate.TicksPerHour);
            pawn.skills.Learn(SkillDefOf.Intellectual, 0.05f);
            pawn.skills.Learn(SkillDefOf.Social, 0.05f);
            pawn.GainComfortFromCellIfPossible(true);
        }
    }
}
