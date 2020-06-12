using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    abstract class SuccessionBase
    {
        public abstract string Title { get; }

        public virtual IEnumerable<Pawn> Candidates => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Where(p => CanBeLeader(p));

        public abstract Pawn ChooseLeader();

        public virtual bool CanBeLeader(Pawn pawn) => Rimocracy.CanBeLeader(pawn);
    }
}
