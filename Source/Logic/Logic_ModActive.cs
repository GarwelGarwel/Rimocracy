using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Logic_ModActive : Logic_Simple
    {
        public override string DefaultLabel => $"{SlateRef} is active";

        public override string LabelInverted => $"{SlateRef} is not active";

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => ModsConfig.IsActive(SlateRef.Contains('.') ? SlateRef : $"Ludeon.RimWorld.{SlateRef}");
    }
}
