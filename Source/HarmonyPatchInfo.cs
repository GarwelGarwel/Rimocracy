using System;

namespace Rimocracy
{
    public class HarmonyPatchInfo
    {
        public Type patchClass;
        public string patchMethod;
        public int targetArgument;
    }
}
