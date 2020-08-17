using Verse;

namespace Rimocracy
{
    class Decision : IExposable
    {
        public string tag;
        public int expiration;

        public bool HasExpired => Find.TickManager.TicksAbs >= expiration;

        public Decision(DecisionDef decisionDef)
            : this(decisionDef.Tag, decisionDef.Expiration)
        { }

        public Decision(string tag, int expiration = int.MaxValue)
        {
            this.tag = tag;
            this.expiration = expiration;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref tag, "tag");
            Scribe_Values.Look(ref expiration, "expiration", int.MaxValue);
        }
    }
}
