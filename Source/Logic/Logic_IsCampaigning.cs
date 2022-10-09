using Verse;

namespace Rimocracy
{
    internal class Logic_IsCampaigning : Logic_Simple
    {
        bool isCampaigning;

        public override string SlateRef
        {
            get => isCampaigning.ToString();
            set => isCampaigning = bool.Parse(value);
        }

        string GetLabel(bool condition) => condition ? "campaign is on" : "campaign is not on";

        public override string DefaultLabel => GetLabel(isCampaigning);

        public override string LabelInverted => GetLabel(!isCampaigning);

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target) => Utility.RimocracyComp.IsCampaigning == isCampaigning;
    }
}
