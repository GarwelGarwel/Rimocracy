using Verse;

namespace Rimocracy
{
    /// <summary>
    /// Choose a random leader
    /// </summary>
    class SuccessionWorker_Lot : SuccessionWorker
    {
        public override Pawn ChooseLeader() => Candidates.RandomElementWithFallback();
    }
}
