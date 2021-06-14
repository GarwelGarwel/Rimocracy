using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimocracy
{
    static class HarmonyManager
    {
        internal static Harmony harmony;

        static bool initialized = false;

        public static void Initialize()
        {
            if (initialized)
                return;
            harmony = new Harmony("Garwel.Rimocracy");
            Type type = typeof(HarmonyManager);

            Utility.Log($"Applying Harmony patches...");
            harmony.Patch(AccessTools.Method("RimWorld.JobDriver_TakeToBed:MakeNewToils"), prefix: new HarmonyMethod(type.GetMethod("Arrest_Prefix")), postfix: new HarmonyMethod(type.GetMethod("Arrest_Postfix")));
            harmony.Patch(AccessTools.Method("RimWorld.JobDriver_Execute:MakeNewToils"), prefix: new HarmonyMethod(type.GetMethod("Execution_Prefix")));
            harmony.Patch(AccessTools.Method("RimWorld.ExecutionUtility:DoExecutionByCut"), postfix: new HarmonyMethod(type.GetMethod("Execution_Postfix")));
            harmony.Patch(AccessTools.Method("Verse.AI.JobDriver_ReleasePrisoner:MakeNewToils"), prefix: new HarmonyMethod(type.GetMethod("Release_Prefix")));
            harmony.Patch(AccessTools.Method("RimWorld.GenGuest:PrisonerRelease"), postfix: new HarmonyMethod(type.GetMethod("Release_Postfix")));
            harmony.Patch(AccessTools.Method("RimWorld.PawnBanishUtility:Banish"), prefix: new HarmonyMethod(type.GetMethod("Banishment_Prefix")), postfix: new HarmonyMethod(type.GetMethod("Banishment_Postfix")));

            Utility.Log($"{harmony.GetPatchedMethods().EnumerableCount()} methods patched with Harmony.");
            initialized = true;
        }

        static bool Vetoed(PoliticalActionDef politicalAction, out DecisionVoteResults opinions, Pawn target = null)
        {
            if (!Utility.RimocracyComp.IsEnabled)
            {
                opinions = null;
                return false;
            }

            opinions = politicalAction.GetOpinions(target);
            if (Utility.RimocracyComp.ActionsNeedApproval && opinions.Vetoed)
            {
                Utility.Log($"Action {politicalAction.defName} was vetoed by {Utility.RimocracyComp.Leader}.");
                DisplayVetoMessage(politicalAction, target, opinions[Utility.RimocracyComp.Leader]);
                return true;
            }
            Utility.Log($"Action {politicalAction.defName} was approved or leader's approval is not required.");
            return false;
        }

        static bool Vetoed(PoliticalActionDef politicalAction, Pawn target = null) => Vetoed(politicalAction, out DecisionVoteResults _, target);

        static void DisplayVetoMessage(PoliticalActionDef politicalAction, Pawn target, PawnDecisionOpinion leaderOpinion) =>
            Messages.Message(
                $"{Utility.LeaderTitle} {Utility.RimocracyComp.Leader.NameShortColored} vetoed {politicalAction.label}{(target != null ? $" of {target.NameShortColored}" : "")} (support: {leaderOpinion.support.ToStringWithSign("0")}).",
                new LookTargets(target),
                MessageTypeDefOf.NegativeEvent);


        #region ARREST

        public static void Arrest_Prefix(JobDriver_TakeToBed __instance, out DecisionVoteResults __state)
        {
            if (!__instance.job.def.makeTargetPrisoner)
            {
                __state = null;
                return;
            }
            Pawn target = __instance.job.targetA.Pawn;
            Utility.Log($"Arrest_Prefix for {target}");
            if (Vetoed(RimocracyDefOf.Arrest, out __state, target))
                __instance.EndJobWith(JobCondition.Incompletable);
        }

        public static void Arrest_Postfix(JobDriver_TakeToBed __instance, DecisionVoteResults __state)
        {
            if (!Utility.RimocracyComp.IsEnabled || !__instance.job.def.makeTargetPrisoner)
                return;
            Pawn target = __instance.job.targetA.Pawn;
            Utility.Log($"Arrest_Postfix for {target}");
            if (!Utility.RimocracyComp.ActionsNeedApproval || __state == null || !__state.Vetoed)
                RimocracyDefOf.Arrest.Activate(__state ?? RimocracyDefOf.Arrest.GetOpinions(target));
        }

        #endregion

        #region EXECUTION

        public static void Execution_Prefix(JobDriver_Execute __instance)
        {
            Pawn target = __instance.job.targetA.Pawn;
            Utility.Log($"Execution_Prefix for {target}");
            if (Vetoed(RimocracyDefOf.Execution, target))
                target.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
        }

        public static void Execution_Postfix(Pawn executioner, Pawn victim)
        {
            if (!Utility.RimocracyComp.IsEnabled || victim.AnimalOrWildMan())
                return;
            Utility.Log($"Execution_Postfix('{executioner}', '{victim}')");
            RimocracyDefOf.Execution.Activate(victim);
        }

        #endregion

        #region RELEASE

        public static void Release_Prefix(JobDriver_ReleasePrisoner __instance)
        {
            Pawn target = __instance.job.targetA.Pawn;
            Utility.Log($"Release_Prefix for {target}");
            if (Vetoed(RimocracyDefOf.Release, target))
                target.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
        }

        public static void Release_Postfix(Pawn p)
        {
            if (!Utility.RimocracyComp.IsEnabled || p.AnimalOrWildMan())
                return;
            Utility.Log($"Release_Postfix('{p}')");
            RimocracyDefOf.Release.Activate(p);
        }

        #endregion

        #region BANISHMENT

        public static bool Banishment_Prefix(Pawn pawn, out DecisionVoteResults __state)
        {
            if (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
            {
                __state = null;
                return true;
            }
            Utility.Log($"Banishment_Prefix for {pawn}");
            return !Vetoed(RimocracyDefOf.Banishment, out __state, pawn);
        }

        public static void Banishment_Postfix(Pawn pawn, DecisionVoteResults __state)
        {
            if (!Utility.RimocracyComp.IsEnabled)
                return;
            Utility.Log($"Banishment_Postfix for {pawn}");
            if (!Utility.RimocracyComp.ActionsNeedApproval || (__state != null && !__state.Vetoed))
                RimocracyDefOf.Banishment.Activate(__state);
        }

        #endregion

    }
}
