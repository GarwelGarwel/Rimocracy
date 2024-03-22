using RimWorld;
using Verse;

namespace Rimocracy
{
    public class RitualStage_GoverningBenchSpeech : RitualStage
    {
        public override TargetInfo GetSecondFocus(LordJob_Ritual ritual) => ritual.Map.GetRandomValidGoverningBench();
    }
}
