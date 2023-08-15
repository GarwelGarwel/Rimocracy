using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using Verse;
using Verse.AI;

using static Rimocracy.Utility;

namespace Rimocracy
{
    static class HarmonyManager
    {
        internal static Harmony harmony;

        public static void Initialize()
        {
            if (harmony != null)
                return;

            harmony = new Harmony("Garwel.Rimocracy");
            Type type = typeof(HarmonyManager);

            void Patch(string methodToPatch, string prefix = null, string postfix = null) =>
                harmony.Patch(
                    AccessTools.Method(methodToPatch),
                    prefix != null ? new HarmonyMethod(type.GetMethod(prefix)) : null,
                    postfix != null ? new HarmonyMethod(type.GetMethod(postfix)) : null);

            Log($"Applying Harmony patches...");

            // Patches for political actions
            Patch("RimWorld.JobDriver_TakeToBed:MakeNewToils", prefix: "Arrest_Prefix");
            Patch("RimWorld.Pawn_GuestTracker:CapturedBy", postfix: "Arrest_Postfix");
            Patch("RimWorld.JobDriver_Execute:MakeNewToils", prefix: "Execution_Prefix");
            Patch("RimWorld.ExecutionUtility:DoExecutionByCut", postfix: "Execution_Postfix");
            Patch("Verse.AI.JobDriver_ReleasePrisoner:MakeNewToils", prefix: "Release_Prefix");
            Patch("RimWorld.GenGuest:PrisonerRelease", postfix: "Release_Postfix");
            Patch("RimWorld.PawnBanishUtility:Banish", "Banishment_Prefix", "Banishment_Postfix");
            Patch("RimWorld.Planet.SettlementUtility:Attack", "SettlementAttack_Prefix", "SettlementAttack_Postfix");
            Patch("RimWorld.Dialog_Trade:PostOpen", postfix: "Trade_Prefix");
            Patch("RimWorld.Faction:Notify_PlayerTraded", postfix: "Trade_Postfix");

            // Ideology compatibility patch
            Patch("RimWorld.Precept_RoleSingle:Unassign", "RoleUnassign_Prefix");
            Patch("RimWorld.Ideo:Notify_NotPrimaryAnymore", postfix: "PrimaryIdeoChange_Postfix");

            Log($"{harmony.GetPatchedMethods().EnumerableCount()} methods patched with Harmony.");
        }

        static bool Vetoed(PoliticalActionDef politicalAction, out DecisionVoteResults opinions, Pawn target = null)
        {
            if (!PoliticsEnabled)
            {
                opinions = null;
                return false;
            }

            opinions = politicalAction.GetOpinions(target);
            if (Utility.RimocracyComp.ActionsNeedApproval && opinions.Vetoed)
            {
                Log($"Action {politicalAction.defName} was vetoed by {Utility.RimocracyComp.Leader}.");
                if (Settings.ShowActionSupportDetails)
                    Dialog_PoliticalAction.Show(politicalAction, opinions, false);
                else Messages.Message(
                    $"{LeaderTitle} {Utility.RimocracyComp.Leader.NameShortColored} vetoed {politicalAction.label}{(target != null ? $" of {target.NameShortColored}" : "")} (support: {opinions[Utility.RimocracyComp.Leader].support.ToStringWithSign("0")}).",
                    new LookTargets(target),
                    MessageTypeDefOf.NegativeEvent);

                return true;
            }
            Log($"Action {politicalAction.defName} was approved or leader's approval is not required.");
            return false;
        }

        static bool Vetoed(PoliticalActionDef politicalAction, Pawn target = null) => Vetoed(politicalAction, out DecisionVoteResults _, target);

        #region ARREST

        public static void Arrest_Prefix(JobDriver_TakeToBed __instance, ref DecisionVoteResults __state)
        {
            if (!__instance.job.def.makeTargetPrisoner || !__instance.pawn.IsColonist || __instance.job.targetA.Pawn.IsPrisonerOfColony)
                return;
            Pawn target = __instance.job.targetA.Pawn;
            Log($"Arrest_Prefix for {target} (from {target.Faction}) by {__instance.pawn.Faction}");
            if (Vetoed(RimocracyDefOf.Arrest, out __state, target))
                __instance.EndJobWith(JobCondition.Incompletable);
        }

        public static void Arrest_Postfix(Pawn_GuestTracker __instance, Faction by, Pawn ___pawn, DecisionVoteResults __state)
        {
            if (!PoliticsEnabled || !__instance.IsPrisoner || !by.IsPlayer)
                return;
            Log($"Arrest_Postfix for {___pawn} (from {___pawn.Faction}) by {by}");
            if (__state != null)
                RimocracyDefOf.Arrest.Activate(__state);
            else RimocracyDefOf.Arrest.Activate(___pawn);
        }

        #endregion ARREST

        #region EXECUTION

        public static void Execution_Prefix(JobDriver_Execute __instance)
        {
            Pawn target = __instance.job.targetA.Pawn;
            Log($"Execution_Prefix for {target}");
            if (Vetoed(RimocracyDefOf.Execution, target))
                target.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
        }

        public static void Execution_Postfix(Pawn executioner, Pawn victim)
        {
            if (!PoliticsEnabled || victim.AnimalOrWildMan())
                return;
            Log($"Execution_Postfix('{executioner}', '{victim}')");
            RimocracyDefOf.Execution.Activate(victim);
        }

        #endregion EXECUTION

        #region RELEASE

        public static void Release_Prefix(JobDriver_ReleasePrisoner __instance)
        {
            Pawn target = __instance.job.targetA.Pawn;
            Log($"Release_Prefix for {target}");
            if (Vetoed(RimocracyDefOf.Release, target))
                target.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
        }

        public static void Release_Postfix(Pawn p)
        {
            if (!PoliticsEnabled || p.AnimalOrWildMan())
                return;
            Log($"Release_Postfix('{p}')");
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
            Log($"Banishment_Prefix for {pawn}");
            return !Vetoed(RimocracyDefOf.Banishment, out __state, pawn);
        }

        public static void Banishment_Postfix(Pawn pawn, DecisionVoteResults __state)
        {
            if (!PoliticsEnabled)
                return;
            Log($"Banishment_Postfix for {pawn}");
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
            Log($"SettlementAttack_Prefix({caravan}, {settlement})");
            return !Vetoed(RimocracyDefOf.SettlementAttack, out __state, settlement.Faction.leader);
        }

        public static void SettlementAttack_Postfix(Caravan caravan, Settlement settlement, DecisionVoteResults __state)
        {
            if (!caravan.Faction.IsPlayer || !PoliticsEnabled)
                return;
            Log($"SettlementAttack_Postfix({caravan}, {settlement})");
            if (!Utility.RimocracyComp.ActionsNeedApproval || (__state != null && !__state.Vetoed))
                RimocracyDefOf.SettlementAttack.Activate(__state);
        }

        #endregion SETTLEMENT ATTACK

        #region TRADE

        // This is technically a postfix that does the job of a prefix, i.e. checks if the PoliticalAction (Trade in this case) is vetoed
        public static void Trade_Prefix(Dialog_Trade __instance)
        {
            Log($"Trade_Prefix for trader {TradeSession.trader} ({TradeSession.trader?.Faction})");
            if (TradeSession.trader == null)
                return;
            if (Vetoed(RimocracyDefOf.Trade, TradeSession.trader.Faction?.leader))
                __instance.Close();
        }

        public static void Trade_Postfix(float marketValueSentByPlayer, Faction __instance)
        {
            if (marketValueSentByPlayer <= 0 || !PoliticsEnabled)
                return;
            Log($"Trade_Postfix({marketValueSentByPlayer}) for {__instance}");
            // Governance is changed in direct proportion to the amount traded and reverse proportion to the total items' wealth of the player
            RimocracyDefOf.Trade.Activate(__instance?.leader, marketValueSentByPlayer / Math.Max(Find.Maps.Where(map => map.IsPlayerHome).Sum(map => map.wealthWatcher.WealthItems), 1000));
        }

        #endregion TRADE

        #region IDEOLOGY PATCHES

        public static bool RoleUnassign_Prefix(Precept_RoleSingle __instance, Pawn p)
        {
            Log($"RoleUnassign_Prefix({__instance.def}, {p})");
            if (__instance.def.leaderRole && p != null && p.IsLeader())
            {
                Log($"Blocked unassignment of role {__instance.def} from {p}.");
                Messages.Message($"{LeaderTitle} can only be unassigned via Impeachment decision.", MessageTypeDefOf.RejectInput);
                if (RimocracyDefOf.Impeachment.IsDisplayable)
                    Find.WindowStack.Add(new Dialog_DecisionList());
                return false;
            }
            return true;
        }

        // Resets SuccessionDef type to a random one on primary ideoligion change
        public static void PrimaryIdeoChange_Postfix(Ideo __instance, Ideo newIdeo)
        {
            Log($"PrimaryIdeoChange_Postfix('{__instance.name}', '{newIdeo.name}')");
            if (Utility.RimocracyComp == null || Utility.RimocracyComp.DecisionActive(DecisionDef.Multiculturalism))
                return;
            SuccessionDef newSuccession = Utility.RimocracyComp.GetRandomSuccessionDef(newIdeo);
            if (newSuccession != Utility.RimocracyComp.SuccessionType)
            {
                Log($"SuccessionDef type changed from {Utility.RimocracyComp.SuccessionType.LabelCap} to {newSuccession.LabelCap}.");
                Find.LetterStack.ReceiveLetter("SuccessionDef type changed", $"SuccessionDef type changed to {newSuccession.LabelCap} due to change of primary ideoligion.\n\n{newSuccession.description}", LetterDefOf.NeutralEvent);
                Utility.RimocracyComp.SuccessionType = newSuccession;
            }
        }

        #endregion IDEOLOGY PATCHES
    }
}
