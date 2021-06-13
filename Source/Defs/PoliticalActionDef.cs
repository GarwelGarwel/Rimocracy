using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    /// <summary>
    /// Game action (like arresting a pawn or attacking a settlement) that citizens can have their opinions about and that can have political repercursions
    /// </summary>
    public class PoliticalActionDef : Def
    {
        public bool allCitizensReact = true;
        public List<Consideration> considerations = new List<Consideration>();
        public float governanceChangeIfSupported;
        public float governanceChangeIfOpposed;

        public HarmonyPatchInfo preActionPatch;
        public HarmonyPatchInfo postActionPatch;

        public string previewMethod;

        public DecisionVoteResults GetOpinions(Pawn target = null) =>
            allCitizensReact
            ? new DecisionVoteResults(Utility.Citizens.Select(pawn => new PawnDecisionOpinion(pawn, considerations, target)))
            : new DecisionVoteResults() { new PawnDecisionOpinion(Utility.RimocracyComp.Leader, considerations, target) };

        public bool Approved(Pawn target = null)

        public void Activate(DecisionVoteResults opinions)
        {
            Utility.Log($"{defName} activated.");
            foreach (PawnDecisionOpinion opinion in opinions.Where(opinion => opinion.Vote != DecisionVote.Abstain))
            {
                opinion.voter.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RimocracyDefOf.DecisionMade, opinion.Vote == DecisionVote.Yea ? 1 : 0));
                if (opinion.voter == Utility.RimocracyComp.Leader)
                    Utility.RimocracyComp.Governance = Mathf.Clamp(Utility.RimocracyComp.Governance + (opinion.Vote == DecisionVote.Yea ? governanceChangeIfSupported : governanceChangeIfOpposed), 0, 1);
            }

            if (!opinions.EnumerableNullOrEmpty())
                Find.WindowStack.Add(new Dialog_PoliticalAction(this, opinions));
        }

        public void Activate(Pawn target = null) => Activate(GetOpinions(target));

        //public string PreviewString(Pawn target = null)
        //{
        //    string s = "";
        //    if (Utility.RimocracyComp.Leader != null)
        //    {
        //        PawnDecisionOpinion opinion = new PawnDecisionOpinion(Utility.RimocracyComp.Leader, considerations, target);
        //        if (opinion.Vote != DecisionVote.Abstain)
        //            s = $"{Utility.RimocracyComp.Leader.NameShortColored} {(opinion.Vote == DecisionVote.Yea ? "supports" : "opposes")} this action.\r\n";
        //    }
        //    if (allCitizensReact)
        //        s += $"{Utility.Citizens.Count(pawn => considerations.Sum(consideration => consideration.GetSupport(pawn, target)) > 0)} / {Utility.CitizensCount} citizens support this action.";
        //    return s.TrimEndNewlines();
        //}
    }
}
