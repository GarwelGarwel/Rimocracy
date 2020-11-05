using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ThoughtWorker_Supporter: ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (Utility.RimocracyComp?.Campaigns == null)
                return false;

            Pawn myCandidate = p.GetSupportedCandidate();
            if (myCandidate == null || myCandidate != otherPawn.GetSupportedCandidate())
                return false;

            // The current pawn is a candidate; the other pawn is their supporter
            if (p == myCandidate)
                return ThoughtState.ActiveAtStage(1);

            // The current pawn is a supporter of the other pawn
            if (otherPawn == myCandidate)
                return ThoughtState.ActiveAtStage(2);

            // Both pawns support the same candidate
            return ThoughtState.ActiveAtStage(0);
        }
    }
}
