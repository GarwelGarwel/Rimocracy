using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimocracy
{
    public static class RitualUtility
    {
        public static void ShowDecisionRitualDialog(DecisionDef decisionDef)
        {
            Utility.Log($"ShowDecisionRitualDialog({decisionDef.defName})");
            RimocracyComp comp = Utility.RimocracyComp;
            Find.WindowStack.Add(new Dialog_BeginRitual(
                $"Take {decisionDef.LabelTitleCase} Decision",
                "Decision adoption",
                (Precept_Ritual)PreceptMaker.MakePrecept(RimocracyDefOf.Rimocracy_DecisionAnnouncement),
                comp.Leader.Map.GetRandomValidGoverningBench(),
                comp.Leader.Map,
                (ritualRoleAssignments) =>
                {
                    Utility.Log($"Decision ritual dialog action. ritualRoleAssignments has {ritualRoleAssignments.AllPawns.Count} pawns and {ritualRoleAssignments.AllRolesForReading.Count} roles.");
                    if (decisionDef.Activate(decisionDef.GetVotingResults(Utility.Citizens.ToList())))
                        Find.LetterStack.ReceiveLetter($"{decisionDef.LabelTitleCase} Decision Taken", decisionDef.description, LetterDefOf.PositiveEvent);
                    else Messages.Message($"Could not take {decisionDef.LabelTitleCase} decision: requirements are not met.", MessageTypeDefOf.NegativeEvent, false);
                    return true;
                },
                comp.Leader,
                null,
                (pawn, voluntary, allowOtherIdeos) => pawn.IsCitizen() && !pawn.Downed));
        }
    }
}
