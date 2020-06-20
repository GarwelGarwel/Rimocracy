using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ThoughtWorker_Respect : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
            => otherPawn.IsLeader() && p.ageTracker.AgeBiologicalYears >= 16;
    }
}
