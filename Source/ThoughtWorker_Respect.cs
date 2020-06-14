using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ThoughtWorker_Respect : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
            => otherPawn == Rimocracy.Instance.Leader && p.ageTracker.AgeBiologicalYears >= 16;
    }
}
