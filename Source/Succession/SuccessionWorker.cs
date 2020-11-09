using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public enum SuccessionType { Undefined = 0, Election, Lot, Seniority, Nobility, Martial };

    public abstract class SuccessionWorker
    {
        public SuccessionDef def;

        public virtual string NewLeaderMessageTitle(Pawn leader) =>
            def.newLeaderMessageTitle.Formatted(Utility.LeaderTitle.Named("LEADERTITLE"), leader.Named("PAWN"));

        public virtual string NewLeaderMessageText(Pawn leader) =>
            def.newLeaderMessageText.Formatted(Utility.NationName.Named("NATIONNAME"), Utility.LeaderTitle.Named("LEADERTITLE"), leader.Named("PAWN"));

        public virtual string SameLeaderMessageTitle(Pawn leader) =>
            def.sameLeaderMessageTitle.Formatted(Utility.LeaderTitle.Named("LEADERTITLE"), leader.Named("PAWN"));

        public virtual string SameLeaderMessageText(Pawn leader) =>
            def.sameLeaderMessageText.Formatted(Utility.NationName.Named("NATIONNAME"), Utility.LeaderTitle.Named("LEADERTITLE"), leader.Named("PAWN"));

        public abstract SuccessionType SuccessionType { get; }

        public virtual IEnumerable<Pawn> Candidates => Utility.Citizens.Where(p => CanBeCandidate(p));

        public virtual bool IsValid => true;

        public abstract Pawn ChooseLeader();

        public virtual bool CanBeCandidate(Pawn pawn) => pawn.CanBeLeader();
    }
}
