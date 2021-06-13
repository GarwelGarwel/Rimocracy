using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

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

            foreach (PoliticalActionDef def in DefDatabase<PoliticalActionDef>.AllDefs.Where(def => def.preActionPatch?.patchClass != null && def.preActionPatch?.patchMethod != null))
            {
                Utility.Log($"Patching prefix for {def.preActionPatch.patchClass}.{def.preActionPatch.patchMethod} for {def.defName} ({def.label}).");
                if (def.preActionPatch.targetArgument < 0 || def.preActionPatch.targetArgument > 3)
                {
                    Utility.Log($"Incorrect targetArgument in preActionPatch of {def.defName}: only values between 0 and 3 are supported.", LogLevel.Error);
                    def.preActionPatch.targetArgument = 0;
                }
                harmony.Patch(def.preActionPatch.patchClass.GetMethod(def.preActionPatch.patchMethod), prefix: new HarmonyMethod(type.GetMethod($"Action_Prefix{def.preActionPatch.targetArgument}")));
            }

            foreach (PoliticalActionDef def in DefDatabase<PoliticalActionDef>.AllDefs.Where(def => def.postActionPatch?.patchClass != null && def.postActionPatch?.patchMethod != null))
            {
                Utility.Log($"Patching postfix for {def.postActionPatch.patchClass}.{def.postActionPatch.patchMethod} for {def.defName} ({def.label}).");
                if (def.postActionPatch.targetArgument < 0 || def.postActionPatch.targetArgument > 3)
                {
                    Utility.Log($"Incorrect targetArgument in actionPatch of {def.defName}: only values between 0 and 3 are supported.", LogLevel.Error);
                    def.postActionPatch.targetArgument = 0;
                }
                harmony.Patch(def.postActionPatch.patchClass.GetMethod(def.postActionPatch.patchMethod), postfix: new HarmonyMethod(type.GetMethod($"Action_Postfix{def.postActionPatch.targetArgument}")));
            }

            //harmony.Patch(typeof(PawnBanishUtility).GetMethod("GetBanishButtonTip"), postfix: new HarmonyMethod(type.GetMethod($"PawnBanishUtility_GetBanishButtonTip_Postfix")));

            Utility.Log($"{harmony.GetPatchedMethods().EnumerableCount()} methods patched with Harmony.");
            initialized = true;
        }

        #region Prefix Methods

        static bool Action_Prefix(MethodBase __originalMethod, out DecisionVoteResults __state, object __0 = null, object __1 = null, object __2 = null)
        {
            Utility.Log($"Action_Prefix for {__originalMethod.DeclaringType}.{__originalMethod.Name}");
            if (!Utility.PoliticsEnabled || !Utility.RimocracyComp.IsEnabled)
            {
                Utility.Log("Rimocracy is not active.");
                __state = null;
                return true;
            }

            PoliticalActionDef politicalAction = DefDatabase<PoliticalActionDef>.AllDefs
                .FirstOrDefault(def => def.preActionPatch?.patchClass == __originalMethod.DeclaringType && def.preActionPatch?.patchMethod == __originalMethod.Name);

            if (politicalAction != null)
            {
                Pawn target = null;
                switch (politicalAction.preActionPatch.targetArgument)
                {
                    case 1:
                        target = __0 as Pawn;
                        break;

                    case 2:
                        target = __1 as Pawn;
                        break;

                    case 3:
                        target = __2 as Pawn;
                        break;
                }

                __state = politicalAction.GetOpinions(target);
                Pawn leader = Utility.RimocracyComp.Leader;

                if (leader != null && Utility.RimocracyComp.ActionsNeedApproval)
                {
                    PawnDecisionOpinion leaderSupport = __state[leader]; //new PawnDecisionOpinion(leader, politicalAction.considerations, target);
                    Utility.Log($"Leader {leader} opinion of {politicalAction.defName} action against {target} is {leaderSupport.support}:\r\n{leaderSupport.explanation}");
                    if (leaderSupport.Vote == DecisionVote.Nay)
                    {
                        Utility.Log($"Action {politicalAction.defName} vetoed by {Utility.RimocracyComp.Leader}.");
                        Messages.Message(
                            $"{Utility.LeaderTitle} {leader.NameShortColored} has vetoed {politicalAction.label}{(target != null ? $" of {target.NameShortColored}" : "")} (support: {leaderSupport.support.ToStringWithSign("0")}).}",
                            new LookTargets(target),
                            MessageTypeDefOf.NegativeEvent);
                        return false;
                    }
                    else
                    {
                        Utility.Log($"Action {politicalAction.defName} approved by {leader}.");
                        return true;
                    }
                }
                else Utility.Log("There is no leader or his/her approval for the action is not needed.");
            }
            else
            {
                Utility.Log($"PoliticalActionDef for method {__originalMethod.DeclaringType}.{__originalMethod.Name} not found.", LogLevel.Error);
                __state = null;
            }
            return true;
        }

        public static bool Action_Prefix0(MethodBase __originalMethod, out DecisionVoteResults __state) => Action_Prefix(__originalMethod, out __state);

        public static bool Action_Prefix1(MethodBase __originalMethod, out DecisionVoteResults __state, object __0) => Action_Prefix(__originalMethod, out __state, __0);

        public static bool Action_Prefix2(MethodBase __originalMethod, out DecisionVoteResults __state, object __0, object __1) => Action_Prefix(__originalMethod, out __state, __0, __1);

        public static bool Action_Prefix3(MethodBase __originalMethod, out DecisionVoteResults __state, object __0, object __1, object __2) => Action_Prefix(__originalMethod, out __state, __0, __1, __2);
        #endregion

        #region Postfix Methods

        static void Action_Postfix(MethodBase __originalMethod, DecisionVoteResults __state, object __0 = null, object __1 = null, object __2 = null)
        {
            Utility.Log($"Postfix for {__originalMethod.DeclaringType}.{__originalMethod.Name}");
            if (!Utility.PoliticsEnabled || !Utility.RimocracyComp.IsEnabled)
            {
                Utility.Log("Rimocracy is not active.");
                return;
            }

            PoliticalActionDef politicalAction = DefDatabase<PoliticalActionDef>.AllDefs
                .FirstOrDefault(def => def.postActionPatch?.patchClass == __originalMethod.DeclaringType && def.postActionPatch?.patchMethod == __originalMethod.Name);
            if (politicalAction != null)
            {
                Pawn target = null;
                switch (politicalAction.postActionPatch.targetArgument)
                {
                    case 1:
                        target = __0 as Pawn;
                        break;

                    case 2:
                        target = __1 as Pawn;
                        break;

                    case 3:
                        target = __2 as Pawn;
                        break;
                }

                if (__state != null)
                    Utility.Log($"Prefix opinions:\r\n{__state}");

                if (!Utility.RimocracyComp.ActionsNeedApproval || __state == null || __state[Utility.RimocracyComp.Leader].Vote != DecisionVote.Nay)
                    politicalAction.Activate(__state ?? politicalAction.GetOpinions(target));
            }
            else Utility.Log($"PoliticalActionDef for method {__originalMethod.DeclaringType}.{__originalMethod.Name} not found.", LogLevel.Error);
        }

        public static void Action_Postfix0(MethodBase __originalMethod, DecisionVoteResults __state) => Action_Postfix(__originalMethod, __state);

        public static void Action_Postfix1(MethodBase __originalMethod, DecisionVoteResults __state, object __0) => Action_Postfix(__originalMethod, __state, __0);

        public static void Action_Postfix2(MethodBase __originalMethod, DecisionVoteResults __state, object __0, object __1) => Action_Postfix(__originalMethod, __state, __0, __1);

        public static void Action_Postfix3(MethodBase __originalMethod, DecisionVoteResults __state, object __0, object __1, object __2) => Action_Postfix(__originalMethod, __state, __0, __1, __2);
        #endregion

        //public static void PawnBanishUtility_GetBanishButtonTip_Postfix(MethodBase __originalMethod, ref string __result, object __0)
        //{
        //    PoliticalActionDef politicalAction = DefDatabase<PoliticalActionDef>.AllDefs
        //        .FirstOrDefault(def => def.actionPatch?.patchClass == __originalMethod.DeclaringType);
        //    __result = $"{{ORIGINAL}}\n{politicalAction.PreviewString(__0 as Pawn)}".Formatted(new NamedArgument(__result, "ORIGINAL"));
        //}
    }
}
