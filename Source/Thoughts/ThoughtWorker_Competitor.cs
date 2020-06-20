using RimWorld;
using Verse;
using System.Linq;

namespace Rimocracy
{
    public class ThoughtWorker_Competitor : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
            => Rimocracy.Instance?.Candidates != null && Rimocracy.Instance.Candidates.Contains(p) && Rimocracy.Instance.Candidates.Contains(otherPawn);
    }
}
