using Verse;

namespace Rimocracy.Succession
{
    /// <summary>
    /// Chooses the eligible pawn with the highest biological age
    /// </summary>
    class SuccessionOldest : SuccessionBase
    {
        public override string Title => "Seniority";

        public override Pawn ChooseLeader() => Candidates.MaxByWithFallback(p => p.ageTracker.AgeBiologicalTicks);
    }
}
