using Verse;

namespace Rimocracy.Succession
{
    /// <summary>
    /// Choose a random leader
    /// </summary>
    class SuccessionLot : SuccessionBase
    {
        public override SuccessionType SuccessionType => SuccessionType.Lot;

        //public override string Title => "Lot";

        //public override string SuccessionLabel => "lottery";

        //public override float RegimeEffect => -0.05f;

        //public override string NewLeaderMessage(Pawn leader)
        //    => $"{leader.Name} drew the short straw and will rule {Utility.NationName} from now on!";

        //public override string SameLeaderMessage(Pawn leader)
        //    => $"{leader.Name} has a lucky hand! {leader.gender.GetPronoun()} won the {Utility.LeaderTitle} lottery again.";

        public override Pawn ChooseLeader() => Candidates.RandomElementWithFallback();
    }
}
