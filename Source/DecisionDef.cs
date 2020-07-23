using Rimocracy.Requirements;
using Verse;

namespace Rimocracy
{
    public class DecisionDef : Def
    {
        public Requirement requirements = new Requirement();
        public float governanceCost = 0;
        public SuccessionType setSuccession = SuccessionType.Undefined;
        public TermDuration setTermDuration = TermDuration.Undefined;

        public bool IsValid => (requirements == null || requirements) && Utility.RimocracyComp.Governance >= governanceCost;

        public bool Activate()
        {
            if (!IsValid)
            {
                Utility.Log(defName + " decision is invalid.", LogLevel.Warning);
                return false;
            }

            Utility.RimocracyComp.Governance -= governanceCost;
            if (setSuccession != SuccessionType.Undefined)
            {
                Utility.Log("Setting succession to " + setSuccession);
                Utility.RimocracyComp.SuccessionType = setSuccession;
            }

            if (setTermDuration != TermDuration.Undefined)
            {
                Utility.Log("Setting term duration to " + setTermDuration);
                Utility.RimocracyComp.TermDuration = setTermDuration;
            }

            return true;
        }
    }
}
