using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    abstract class SuccessionBase
    {
        public abstract string Title { get; }

        public virtual IEnumerable<Pawn> Candidates => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Where(p => CanBeCandidate(p));

        public abstract Pawn ChooseLeader();

        public virtual bool CanBeCandidate(Pawn pawn) => Rimocracy.CanBeLeader(pawn) && pawn.ageTracker.AgeBiologicalYears >= 16;
    }
}
