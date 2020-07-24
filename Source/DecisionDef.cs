using RimWorld;
using Verse;

namespace Rimocracy
{
    public class DecisionDef : Def
    {
        public string tag;
        public int durationTicks;
        public int durationDays;
        public Requirement requirements = new Requirement();
        public float governanceCost;
        public SuccessionType setSuccession = SuccessionType.Undefined;
        public TermDuration setTermDuration = TermDuration.Undefined;

        public bool IsValid =>
            (!IsUnique || !Utility.RimocracyComp.DecisionActive(Tag))
            && (requirements == null || requirements)
            && Utility.RimocracyComp.Governance >= governanceCost;

        /// <summary>
        /// Tells if this decision should be recorded
        /// </summary>
        public bool IsUnique => Duration != 0 || tag != null;

        /// <summary>
        /// Returns tag (for timers) or defName by default
        /// </summary>
        public string Tag => tag.NullOrEmpty() ? defName : tag;

        public int Duration => durationDays * GenDate.TicksPerDay + durationTicks;

        /// <summary>
        /// Returns expiration tick or MaxValue if only tag is set
        /// </summary>
        public int Expiration => Duration != 0 ? Find.TickManager.TicksAbs + Duration : (tag == null ? 0 : int.MaxValue);

        public bool Activate()
        {
            if (!IsValid)
            {
                Utility.Log(defName + " decision is invalid.", LogLevel.Warning);
                return false;
            }

            if (IsUnique)
                Utility.RimocracyComp.Decisions.Add(new Decision(this));

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
