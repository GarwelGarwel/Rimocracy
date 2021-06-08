using System;
using System.Collections.Generic;
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
            foreach (PoliticalActionDef def in DefDatabase<PoliticalActionDef>.AllDefs.Where(def => def.actionClass != null && def.actionMethod != null))
            {
                Utility.Log($"Patching {def.actionClass}.{def.actionMethod} for {def.defName} ({def.label}).");
                if (def.targetArgument < 0 || def.targetArgument > 2)
                {
                    Utility.Log($"Incorrect targetArgument in PoliticalActionDef {def.defName}: only values between 0 and 2 are supported.", LogLevel.Error);
                    def.targetArgument = 0;
                }
                harmony.Patch(def.actionClass.GetMethod(def.actionMethod), postfix: new HarmonyMethod(type.GetMethod($"Postfix{def.targetArgument}")));
            }
            Utility.Log($"{harmony.GetPatchedMethods().EnumerableCount()} methods patched with Harmony.");
            initialized = true;
        }

        static void Postfix(MethodBase __originalMethod, object __0 = null, object __1 = null, object __2 = null)
        {
            Utility.Log($"Postfix for {__originalMethod.DeclaringType}.{__originalMethod.Name}");
            PoliticalActionDef politicalAction = DefDatabase<PoliticalActionDef>.AllDefs
                .FirstOrDefault(def => def.actionClass == __originalMethod.DeclaringType && def.actionMethod == __originalMethod.Name);
            if (politicalAction != null)
            {
                Pawn target = null;
                switch (politicalAction.targetArgument)
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
                politicalAction.Activate(target);
            }
            else Utility.Log($"PoliticalActionDef for method {__originalMethod.DeclaringType}.{__originalMethod.Name} not found.", LogLevel.Error);
        }

        public static void Postfix0(MethodBase __originalMethod) => Postfix(__originalMethod);

        public static void Postfix1(MethodBase __originalMethod, object __0) => Postfix(__originalMethod, __0);

        public static void Postfix2(MethodBase __originalMethod, object __0, object __1) => Postfix(__originalMethod, __0, __1);

        public static void Postfix3(MethodBase __originalMethod, object __0, object __1, object __2) => Postfix(__originalMethod, __0, __1, __2);

        public static void PawnBanishUtility_GetBanishButtonTip_Postfix(Pawn pawn, ref string __result)
        {
            __result += "\r\nThis can anger your leader and citizens!";
        }
    }
}
