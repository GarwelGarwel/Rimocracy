using Verse;

namespace Rimocracy
{
    class Decision : IExposable
    {
        public DecisionDef def;
        public int expiration;

        public string Tag => def?.Tag;

        public bool HasExpired => Find.TickManager.TicksAbs >= expiration;

        public bool ShouldBeRemoved => HasExpired || !def.effectRequirements;

        public Decision()
        { }

        public Decision(DecisionDef def)
        {
            this.def = def;
            expiration = def.Expiration;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref expiration, "expiration", int.MaxValue);
        }
    }
}
