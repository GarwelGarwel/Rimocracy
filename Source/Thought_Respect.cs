using RimWorld;

namespace Rimocracy
{
    class Thought_Respect : Thought_SituationalSocial
    {
        protected override ThoughtState CurrentStateInternal()
        {
            if (otherPawn.IsLeader() && pawn.ageTracker.AgeBiologicalYears >= 16)
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
