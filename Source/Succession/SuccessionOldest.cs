using Verse;

namespace Rimocracy.Succession
{
    /// <summary>
    /// Chooses the eligible pawn with the highest biological age
    /// </summary>
    class SuccessionOldest : SuccessionBase
    {
        public override SuccessionType SuccessionType => SuccessionType.Seniority;

        //public override string Title => "Seniority";

        //public override float RegimeEffect => -0.10f;

        //public override string NewLeaderMessage(Pawn leader)
        //    => $"{leader.Name} will rule {Utility.NationName} now as the oldest colonist.";

        //public override string SameLeaderMessage(Pawn leader)
        //    => $"{leader.Name} is still the oldest colonist in {Utility.NationName}. {leader.gender.GetPronoun()} remains our {Utility.LeaderTitle}.";

        public override Pawn ChooseLeader() => Candidates.MaxByWithFallback(p => p.ageTracker.AgeBiologicalTicks);
    }
}
