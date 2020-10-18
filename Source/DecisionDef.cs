using RimWorld;
using Verse;

namespace Rimocracy
{
    public class DecisionDef : Def
    {
        public Requirement displayRequirements = Requirement.always;
        public Requirement effectRequirements = Requirement.always;
        public string tag;
        public int durationTicks;
        public int durationDays;

        public float governanceCost;
        public SuccessionType setSuccession = SuccessionType.Undefined;
        public TermDuration setTermDuration = TermDuration.Undefined;
        public bool impeachLeader;
        public string cancelDecision;

        public bool IsDisplayable =>
            (!IsUnique || !Utility.RimocracyComp.DecisionActive(Tag))
            && (displayRequirements == null || displayRequirements);

        public bool IsValid =>
            IsDisplayable
            && (effectRequirements == null || effectRequirements)
            && Utility.RimocracyComp.Governance >= governanceCost;

        /// <summary>
        /// Tells if this decision tag should be stored
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
                Utility.Log($"{defName} decision is invalid.", LogLevel.Warning);
                return false;
            }

            if (IsUnique)
                Utility.RimocracyComp.Decisions.Add(new Decision(this));

            Utility.RimocracyComp.Governance -= governanceCost;
            if (setSuccession != SuccessionType.Undefined)
            {
                Utility.Log($"Setting succession to {setSuccession}.");
                Utility.RimocracyComp.SuccessionType = setSuccession;
            }

            if (setTermDuration != TermDuration.Undefined)
            {
                Utility.Log($"Setting term duration to {setTermDuration}.");
                Utility.RimocracyComp.TermDuration = setTermDuration;
            }

            if (impeachLeader && Utility.RimocracyComp.Leader != null)
            {
                Utility.Log($"Impeaching {Utility.RimocracyComp.Leader}.");
                Utility.RimocracyComp.Leader = null;
            }

            if (!cancelDecision.NullOrEmpty())
            {
                Utility.Log($"Canceling decision tag {cancelDecision}.");
                if (Utility.RimocracyComp.Decisions.RemoveAll(decision => decision.Tag == cancelDecision || decision.def.defName == cancelDecision) == 0)
                    Utility.Log("Decision not found.", LogLevel.Warning);
            }

            return true;
        }
    }
}
