using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public enum SuccessionType { Undefined = 0, Election, Lot, Seniority, Nobility, Martial };

    public abstract class SuccessionBase
    {
        public abstract SuccessionType SuccessionType { get; }

        public abstract string Title { get; }

        public virtual string SuccessionLabel => "succession";

        public virtual float RegimeEffect => 0;

        public virtual string NewLeaderTitle => $"New {Utility.LeaderTitle.CapitalizeFirst()}";

        public virtual string SameLeaderTitle => $"{Utility.LeaderTitle.CapitalizeFirst()} Stays in Power";

        public virtual IEnumerable<Pawn> Candidates => Utility.Citizens.Where(p => CanBeCandidate(p));

        public virtual bool IsValid => true;

        public virtual string NewLeaderMessage(Pawn leader) =>
            $"{Utility.NationName} has a new {Utility.LeaderTitle}: {leader.Name}. Let {leader.gender.GetPossessive()} reign be long and prosperous!";

        public virtual string SameLeaderMessage(Pawn leader) => $"{leader.Name} remains the {Utility.LeaderTitle} of {Utility.NationName}.";

        public abstract Pawn ChooseLeader();

        public virtual bool CanBeCandidate(Pawn pawn) => pawn.CanBeLeader();
    }
}
