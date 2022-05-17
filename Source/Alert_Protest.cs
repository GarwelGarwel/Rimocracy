using RimWorld;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Alert_Protest : Alert
    {
        public Alert_Protest()
        {
            defaultPriority = AlertPriority.High;
            defaultLabel = "Ongoing protest";
        }

        public override Verse.TaggedString GetExplanation()
        {
            return Utility.RimocracyComp.Protesters.Count == 1
                ? $"{Utility.RimocracyComp.Protesters[0]?.NameShortColored} is protesting. Raise {Utility.RimocracyComp.Protesters[0]?.Possessive()} loyalty to stop it."
                : $"{Utility.RimocracyComp.Protesters.Count.ToStringCached()} citizens are protesting: {Utility.RimocracyComp.Protesters.Select(pawn => pawn.NameShortColored.RawText).ToCommaList()}. Raise their loyalty to stop it.";
        }

        public override AlertReport GetReport() =>
            Utility.RimocracyComp.Protesters.Any() ? AlertReport.CulpritsAre(Utility.RimocracyComp.Protesters) : false;
    }
}
