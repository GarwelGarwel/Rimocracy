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

            Utility.Log($"Applying Harmony patches...");

            // Patches for political actions
            Patch("RimWorld.JobDriver_TakeToBed:MakeNewToils", "Arrest_Prefix", "Arrest_Postfix");
            Patch("RimWorld.JobDriver_Execute:MakeNewToils", "Execution_Prefix");
            Patch("RimWorld.ExecutionUtility:DoExecutionByCut", postfix: "Execution_Postfix");
            Patch("Verse.AI.JobDriver_ReleasePrisoner:MakeNewToils", "Release_Prefix");
            Patch("RimWorld.GenGuest:PrisonerRelease", postfix: "Release_Postfix");
            Patch("RimWorld.PawnBanishUtility:Banish", "Banishment_Prefix", "Banishment_Postfix");
            Patch("RimWorld.Planet.SettlementUtility:Attack", "SettlementAttack_Prefix", "SettlementAttack_Postfix");
            Patch("RimWorld.Dialog_Trade:PostOpen", postfix: "Trade_Prefix");
            Patch("RimWorld.Faction:Notify_PlayerTraded", postfix: "Trade_Postfix");

            // Ideology compatibility patch
            Patch("RimWorld.Precept_RoleSingle:Unassign", "RoleUnassign_Prefix");
            Patch("RimWorld.Ideo:Notify_NotPrimaryAnymore", postfix: "PrimaryIdeoChange_Postfix");

            Utility.Log($"{harmony.GetPatchedMethods().EnumerableCount()} methods patched with Harmony.");
        }

        static bool Vetoed(PoliticalActionDef politicalAction, out DecisionVoteResults opinions, Pawn target = null)
        {
            if (!Utility.PoliticsEnabled)
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
        static bool IsActualArrestJob(JobDriver_TakeToBed jobDriver) =>
            jobDriver.job.def.makeTargetPrisoner && jobDriver.pawn.IsColonist && !jobDriver.job.targetA.Pawn.IsPrisonerOfColony;

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
            if (!Utility.PoliticsEnabled || !IsActualArrestJob(__instance))
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
            if (!Utility.PoliticsEnabled || victim.AnimalOrWildMan())
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
            if (!Utility.PoliticsEnabled || p.AnimalOrWildMan())
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
            if (!Utility.PoliticsEnabled)
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
            if (!caravan.Faction.IsPlayer || !Utility.PoliticsEnabled)
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
            if (marketValueSentByPlayer <= 0 || !Utility.PoliticsEnabled)
                return;
            Utility.Log($"Trade_Postfix({marketValueSentByPlayer}) for {__instance}");
            // Governance is changed in direct proportion to the amount traded and reverse proportion to the total items' wealth of the player
            RimocracyDefOf.Trade.Activate(__instance?.leader, marketValueSentByPlayer / Math.Max(Find.Maps.Where(map => map.IsPlayerHome).Sum(map => map.wealthWatcher.WealthItems), 1000));
        }

        #endregion TRADE

        #region IDEOLOGY PATCHES

        public static bool RoleUnassign_Prefix(Precept_RoleSingle __instance, Pawn p)
        {
            Utility.Log($"RoleUnassign_Prefix({__instance.def}, {p})");
            if (__instance.def.leaderRole && p != null && p.IsLeader())
            {
                Utility.Log($"Blocked unassignment of role {__instance.def} from {p}.");
                Messages.Message($"{Utility.LeaderTitle} can only be unassigned via Impeachment decision.", MessageTypeDefOf.RejectInput);
                Find.WindowStack.Add(new Dialog_DecisionList());
                return false;
            }
            return true;
        }

        // Resets succession type to a random one on primary ideologion change
        public static void PrimaryIdeoChange_Postfix(Ideo __instance, Ideo newIdeo)
        {
            Utility.Log($"PrimaryIdeoChange_Postfix('{__instance.name}', '{newIdeo.name}')");
            SuccessionDef newSuccession = Utility.RimocracyComp?.GetRandomSuccessionDef();
            if (Utility.RimocracyComp != null && newSuccession != Utility.RimocracyComp.SuccessionType)
            {
                Utility.Log($"Succession type changed from {Utility.RimocracyComp.SuccessionType.LabelCap} to {newSuccession.LabelCap}.");
                Find.LetterStack.ReceiveLetter("Succession type changed", $"Succession type changed to {newSuccession.LabelCap} due to change of primary ideologion.\n\n{newSuccession.description}", LetterDefOf.NeutralEvent);
                Utility.RimocracyComp.SuccessionType = newSuccession;
            }
        }

        #endregion IDEOLOGY PATCHES
    }
}
