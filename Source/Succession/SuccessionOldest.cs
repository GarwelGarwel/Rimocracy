﻿using Verse;

namespace Rimocracy.Succession
{
    /// <summary>
    /// Chooses the eligible pawn with the highest biological age
    /// </summary>
    class SuccessionOldest : SuccessionBase
    {
        public override string Title => "Seniority";

        public override string NewLeaderMessage(Pawn leader)
            => "{PAWN_nameFullDef} will rule our nation now as the oldest colonist.".Formatted(leader.Named("PAWN"));

        public override string SameLeaderMessage(Pawn leader)
            => "{PAWN_nameFullDef} is still the oldest colonist in our nation. {PAWN_pronoun} remains our leader.".Formatted(leader.Named("PAWN"));

        public override Pawn ChooseLeader() => Candidates.MaxByWithFallback(p => p.ageTracker.AgeBiologicalTicks);
    }
}
