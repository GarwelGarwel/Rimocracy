using Verse;

namespace Rimocracy.Succession
{
    /// <summary>
    /// Chooses the eligible pawn with the highest biological age
    /// </summary>
    class SuccessionOldest : SuccessionBase
    {
        public override SuccessionType SuccessionType => SuccessionType.Seniority;

        public override string Title => "Seniority";

        public override float RegimeEffect => -0.10f;

        public override string NewLeaderMessage(Pawn leader)
            => $"{{PAWN_nameFullDef}} will rule {Utility.NationName} now as the oldest colonist.".Formatted(leader.Named("PAWN"));

        public override string SameLeaderMessage(Pawn leader)
            => $"{{PAWN_nameFullDef}} is still the oldest colonist in {Utility.NationName}. {{PAWN_pronoun}} remains our {Utility.LeaderTitle}."
            .Formatted(leader.Named("PAWN"));

        public override Pawn ChooseLeader() => Candidates.MaxByWithFallback(p => p.ageTracker.AgeBiologicalTicks);
    }
}
