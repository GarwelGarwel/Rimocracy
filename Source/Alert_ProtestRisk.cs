using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Alert_ProtestRisk : Alert
    {
        List<Pawn> potentialProtesters = new List<Pawn>();

        public Alert_ProtestRisk()
        {
            defaultPriority = AlertPriority.High;
            defaultLabel = "Protest risk";
        }

        public override AlertReport GetReport()
        {
            if (Utility.RimocracyComp.Protesters.Any())
                return false;
            potentialProtesters = Utility.Citizens.Where(pawn => pawn.GetLoyalty() < Need_Loyalty.ProtestLevel).ToList();
            if (!potentialProtesters.Any())
                return false;
            if (potentialProtesters.Count == 1)
                defaultExplanation = $"{potentialProtesters[0]?.NameShortColored} is disloyal and may start a protest at any time.";
            else defaultExplanation = $"{potentialProtesters.Count.ToStringCached()} citizens are disloyal and may start a protest at any time: {potentialProtesters.Select(pawn => pawn.NameShortColored.RawText).ToCommaList()}.";
            return AlertReport.CulpritsAre(potentialProtesters);
        }
    }
}