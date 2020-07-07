using RimWorld;
using Verse;
using System.Linq;

namespace Rimocracy
{
    public class ThoughtWorker_Competitor : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
            => Utility.RimocracyComp?.Candidates != null && Utility.RimocracyComp.Candidates.Contains(p) && Utility.RimocracyComp.Candidates.Contains(otherPawn);
    }
}
