using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ThoughtWorker_Respect : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (otherPawn.IsLeader() && p.ageTracker.AgeBiologicalYears >= 16)
            {
                if (Rimocracy.Instance.Authority < 0.10)
                    return ThoughtState.ActiveAtStage(0);
                else if (Rimocracy.Instance.Authority < 0.25)
                    return ThoughtState.ActiveAtStage(1);
                else if (Rimocracy.Instance.Authority < 0.75)
                    return ThoughtState.ActiveAtStage(2);
                else if (Rimocracy.Instance.Authority < 0.95)
                    return ThoughtState.ActiveAtStage(3);
                return ThoughtState.ActiveAtStage(4);
            }
            return false;
        }
    }
}
