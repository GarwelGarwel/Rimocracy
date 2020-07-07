using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public enum SuccessionType { Undefined = 0, Election, Lot, Seniority };

    public abstract class SuccessionBase
    {
        public abstract string Title { get; }

        public virtual string SuccessionLabel => "succession";

        public abstract SuccessionType SuccessionType { get; }

        public virtual string NewLeaderTitle => "New " + Utility.LeaderTitle.CapitalizeFirst();

        public virtual string SameLeaderTitle => Utility.LeaderTitle.CapitalizeFirst() + " Stays in Power";

        public virtual IEnumerable<Pawn> Candidates
            => Utility.Citizens.Where(p => CanBeCandidate(p));

        public virtual string NewLeaderMessage(Pawn leader)
            => (Utility.NationName + " has a new " + Utility.LeaderTitle + ": {PAWN_nameFullDef}. Let {PAWN_possessive} reign be long and prosperous!").Formatted(leader.Named("PAWN"));

        public virtual string SameLeaderMessage(Pawn leader)
            => ("{PAWN_nameFullDef} remains the " + Utility.LeaderTitle + " of " + Utility.NationName + ".").Formatted(leader.Named("PAWN"));

        public abstract Pawn ChooseLeader();

        public virtual bool CanBeCandidate(Pawn pawn)
            => pawn.CanBeLeader();
    }
}
