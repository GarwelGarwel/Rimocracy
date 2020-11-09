using Verse;

namespace Rimocracy.Succession
{
    /// <summary>
    /// Chooses the eligible pawn with the highest biological age
    /// </summary>
    class SuccessionWorker_Oldest : SuccessionWorker
    {
        public override SuccessionType SuccessionType => SuccessionType.Seniority;

        public override Pawn ChooseLeader() => Candidates.MaxByWithFallback(p => p.ageTracker.AgeBiologicalTicks);
    }
}
