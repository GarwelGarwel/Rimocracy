using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ThoughtWorker_Supporter: ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (Utility.RimocracyComp?.Candidates == null)
                return false;
            Pawn myCandidate = Utility.RimocracyComp.GetSupportedCampaign(p)?.Candidate;
            if (myCandidate == null || myCandidate != Utility.RimocracyComp.GetSupportedCampaign(otherPawn)?.Candidate)
                return false;

            if (p == myCandidate)
                return ThoughtState.ActiveAtStage(1);
            if (otherPawn == myCandidate)
                return ThoughtState.ActiveAtStage(2);
            return ThoughtState.ActiveAtStage(0);
        }
    }
}
