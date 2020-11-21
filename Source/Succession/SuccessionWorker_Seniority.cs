using Verse;

namespace Rimocracy
{
    /// <summary>
    /// Chooses the eligible pawn with the highest biological age
    /// </summary>
    class SuccessionWorker_Seniority : SuccessionWorker
    {
        public override Pawn ChooseLeader() => Candidates.MaxByWithFallback(p => p.ageTracker.AgeBiologicalTicks);
    }
}
