using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
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
            harmony.Patch(AccessTools.Method("RimWorld.JobDriver_TakeToBed:MakeNewToils"),
                prefix: new HarmonyMethod(type.GetMethod("Arrest_Prefix")),
                postfix: new HarmonyMethod(type.GetMethod("Arrest_Postfix")));
            harmony.Patch(AccessTools.Method("RimWorld.JobDriver_Execute:MakeNewToils"), prefix: new HarmonyMethod(type.GetMethod("Execution_Prefix")));
            harmony.Patch(AccessTools.Method("RimWorld.ExecutionUtility:DoExecutionByCut"), postfix: new HarmonyMethod(type.GetMethod("Execution_Postfix")));
            harmony.Patch(AccessTools.Method("Verse.AI.JobDriver_ReleasePrisoner:MakeNewToils"), prefix: new HarmonyMethod(type.GetMethod("Release_Prefix")));
            harmony.Patch(AccessTools.Method("RimWorld.GenGuest:PrisonerRelease"), postfix: new HarmonyMethod(type.GetMethod("Release_Postfix")));
            harmony.Patch(AccessTools.Method("RimWorld.PawnBanishUtility:Banish"),
                prefix: new HarmonyMethod(type.GetMethod("Banishment_Prefix")),
                postfix: new HarmonyMethod(type.GetMethod("Banishment_Postfix")));
            harmony.Patch(AccessTools.Method("RimWorld.Planet.SettlementUtility:Attack"),
                prefix: new HarmonyMethod(type.GetMethod("SettlementAttack_Prefix")),
                postfix: new HarmonyMethod(type.GetMethod("SettlementAttack_Postfix")));
            harmony.Patch(AccessTools.Method("RimWorld.Dialog_Trade:PostOpen"), postfix: new HarmonyMethod(type.GetMethod("Trade_Prefix")));
            harmony.Patch(AccessTools.Method("RimWorld.Faction:Notify_PlayerTraded"), postfix: new HarmonyMethod(type.GetMethod("Trade_Postfix")));

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
                if (Settings.ShowActionSupportDetails)
                    Dialog_PoliticalAction.Show(politicalAction, opinions, false);
                else Messages.Message(
                    $"{Utility.LeaderTitle} {Utility.RimocracyComp.Leader.NameShortColored} vetoed {politicalAction.label}{(target != null ? $" of {target.NameShortColored}" : "")} (support: {opinions[Utility.RimocracyComp.Leader].support.ToStringWithSign("0")}).",
                    new LookTargets(target),
                    MessageTypeDefOf.NegativeEvent);

                return true;
            }
            Utility.Log($"Action {politicalAction.defName} was approved or leader's approval is not required.");
            return false;
        }

        static bool Vetoed(PoliticalActionDef politicalAction, Pawn target = null) => Vetoed(politicalAction, out DecisionVoteResults _, target);

        #region ARREST

        // Check is the TakeToBed job is in fact to arrest a non-prisoner for the colony (to prevent it from firing for relocating prisoners etc.)
        static bool IsActualArrestJob(JobDriver_TakeToBed jobDriver)
            => jobDriver.job.def.makeTargetPrisoner && jobDriver.pawn.IsColonist && !jobDriver.job.targetA.Pawn.IsPrisonerOfColony;

        public static void Arrest_Prefix(JobDriver_TakeToBed __instance, out DecisionVoteResults __state)
        {
            if (!IsActualArrestJob(__instance))
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
            if (!Utility.RimocracyComp.IsEnabled || !IsActualArrestJob(__instance))
                return;
            Pawn target = __instance.job.targetA.Pawn;
            Utility.Log($"Arrest_Postfix for {target}");
            if (!Utility.RimocracyComp.ActionsNeedApproval || __state == null || !__state.Vetoed)
                RimocracyDefOf.Arrest.Activate(__state ?? RimocracyDefOf.Arrest.GetOpinions(target));
        }

        #endregion ARREST

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

        #endregion EXECUTION

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

        #endregion RELEASE

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

        #endregion BANISHMENT

        #region SETTLEMENT ATTACK

        public static bool SettlementAttack_Prefix(Caravan caravan, Settlement settlement, out DecisionVoteResults __state)
        {
            if (!caravan.Faction.IsPlayer)
            {
                __state = null;
                return true;
            }
            Utility.Log($"SettlementAttack_Prefix({caravan}, {settlement})");
            return !Vetoed(RimocracyDefOf.SettlementAttack, out __state, settlement.Faction.leader);
        }

        public static void SettlementAttack_Postfix(Caravan caravan, Settlement settlement, DecisionVoteResults __state)
        {
            if (!caravan.Faction.IsPlayer)
                return;
            Utility.Log($"SettlementAttack_Postfix({caravan}, {settlement})");
            if (!Utility.RimocracyComp.ActionsNeedApproval || (__state != null && !__state.Vetoed))
                RimocracyDefOf.SettlementAttack.Activate(__state);
        }

        #endregion SETTLEMENT ATTACK

        #region TRADE

        // This is technically a postfix that does the job of a prefix, i.e. checks if the PoliticalAction (Trade in this case) is vetoed
        public static void Trade_Prefix(Dialog_Trade __instance)
        {
            Utility.Log($"Trade_Prefix for trader {TradeSession.trader} ({TradeSession.trader?.Faction})");
            if (TradeSession.trader == null)
                return;
            if (Vetoed(RimocracyDefOf.Trade, TradeSession.trader.Faction?.leader))
                __instance.Close();
        }

        public static void Trade_Postfix(float marketValueSentByPlayer, Faction __instance)
        {
            if (marketValueSentByPlayer <= 0)
                return;
            Utility.Log($"Trade_Postfix({marketValueSentByPlayer}) for {__instance}");
            // Governance is changed in direct proportion to the amount traded and reverse proportion to the total items' wealth of the player
            RimocracyDefOf.Trade.Activate(__instance?.leader, marketValueSentByPlayer / Math.Max(Find.Maps.Where(map => map.IsPlayerHome).Sum(map => map.wealthWatcher.WealthItems), 1000));
        }

        #endregion TRADE
    }
}
