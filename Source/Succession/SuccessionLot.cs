using Verse;

namespace Rimocracy.Succession
{
    /// <summary>
    /// Choose a random leader
    /// </summary>
    class SuccessionLot : SuccessionBase
    {
        public override SuccessionType SuccessionType => SuccessionType.Lot;

        public override string Title => "Lot";

        public override string SuccessionLabel => "lottery";

        public override float RegimeEffect => -0.05f;

        public override string NewLeaderMessage(Pawn leader)
            => $"{{PAWN_nameFullDef}} drew the short straw and will rule {Utility.NationName} from now on!".Formatted(leader.Named("PAWN"));

        public override string SameLeaderMessage(Pawn leader)
            => $"{{PAWN_nameFullDef}} has a lucky hand! {{PAWN_pronoun}} won the {Utility.LeaderTitle} lottery again.".Formatted(leader.Named("PAWN"));

        public override Pawn ChooseLeader() => Candidates.RandomElementWithFallback();
    }
}
