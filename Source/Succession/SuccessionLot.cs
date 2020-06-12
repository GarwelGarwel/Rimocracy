using Verse;

namespace Rimocracy.Succession
{
    /// <summary>
    /// Choose a random leader
    /// </summary>
    class SuccessionLot : SuccessionBase
    {
        public override string Title => "Lot";

        public override Pawn ChooseLeader() => Candidates.RandomElementWithFallback();
    }
}
