using RimWorld;
using Verse;
using System.Linq;

namespace Rimocracy
{
    public class ThoughtWorker_Competitor : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
            => Utility.Rimocracy?.Candidates != null && Utility.Rimocracy.Candidates.Contains(p) && Utility.Rimocracy.Candidates.Contains(otherPawn);
    }
}
