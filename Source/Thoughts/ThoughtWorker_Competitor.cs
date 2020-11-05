using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ThoughtWorker_Competitor : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (Utility.RimocracyComp?.Candidates == null)
                return false;

            Pawn myCandidate = p.GetSupportedCandidate();
            Pawn otherCandidate = otherPawn.GetSupportedCandidate();
            if (myCandidate == null || otherCandidate == null || myCandidate == otherCandidate)
                return false;

            // The other pawn is a candidate
            if (otherCandidate == otherPawn)
                if (myCandidate == p)
                    return ThoughtState.ActiveAtStage(2);
                else return ThoughtState.ActiveAtStage(1);

            // The other pawn is just a supporter of another candidate
            return ThoughtState.ActiveAtStage(0);
        }
    }
}
