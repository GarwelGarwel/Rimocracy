using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Rimocracy
{
    class JobDriver_DoRuling : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Utility.Log("JobDriver_DoRuling.TryMakePreToilReservations for " + pawn + ". TargetA: " + TargetThingA);
            return pawn.Reserve(TargetThingA, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
			Utility.Log("JobDriver_DoRuling.MakeNewToils for " + pawn + ". TargetA: " + TargetThingA);
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil rulingToil = new Toil();
            rulingToil.tickAction = Ruling_TickAction;
            rulingToil.FailOn(() => Rimocracy.Instance.Authority >= 1);
            rulingToil.FailOn(() => Rimocracy.Instance.Leader != pawn);
            rulingToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            rulingToil.defaultCompleteMode = ToilCompleteMode.Delay;
            rulingToil.defaultDuration = 4000;
            rulingToil.activeSkill = () => SkillDefOf.Intellectual;
            yield return rulingToil;
            yield return Toils_General.Wait(2, TargetIndex.None);
            yield break;
		}

        void Ruling_TickAction()
        {
            Rimocracy.Instance.BuildAuthority(Rimocracy.BaseAuthorityBuildPerTick * pawn.GetStatValue(StatDefOf.WorkSpeedGlobal, true));
            pawn.skills.Learn(SkillDefOf.Intellectual, 0.05f, false);
            pawn.skills.Learn(SkillDefOf.Social, 0.05f, false);
            pawn.GainComfortFromCellIfPossible(true);
        }
    }
}
