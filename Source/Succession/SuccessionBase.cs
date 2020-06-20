using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public abstract class SuccessionBase
    {
        public abstract string Title { get; }

        public virtual string SuccessionLabel => "succession";

        public virtual string NewLeaderTitle => "New Leader";

        public virtual string SameLeaderTitle => "Leader Stays in Power";

        public virtual IEnumerable<Pawn> Candidates
            => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Where(p => CanBeCandidate(p));

        public virtual string NewLeaderMessage(Pawn leader)
            => "Our nation has a new leader: {PAWN_nameFullDef}. Let {PAWN_possessive} reign be long and prosperous!".Formatted(leader.Named("PAWN"));

        public virtual string SameLeaderMessage(Pawn leader)
            => "{PAWN_nameFullDef} remains our nation's leader.".Formatted(leader.Named("PAWN"));

        public abstract Pawn ChooseLeader();

        public virtual bool CanBeCandidate(Pawn pawn)
            => Rimocracy.CanBeLeader(pawn) && pawn.ageTracker.AgeBiologicalYears >= 16 && pawn.health.capacities.CanBeAwake;
    }
}
