using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
