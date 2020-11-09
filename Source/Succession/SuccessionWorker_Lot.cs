using Verse;

namespace Rimocracy.Succession
{
    /// <summary>
    /// Choose a random leader
    /// </summary>
    class SuccessionWorker_Lot : SuccessionWorker
    {
        public override SuccessionType SuccessionType => SuccessionType.Lot;

        public override Pawn ChooseLeader() => Candidates.RandomElementWithFallback();
    }
}
