using HarmonyLib;
using Verse;

namespace Rimocracy
{
    [StaticConstructorOnStartup]
    public static class HarmonyManager
    {
        static Harmony harmony;

        static HarmonyManager()
        {
            harmony = new Harmony("Garwel.Rimocracy");
        }
    }
}
