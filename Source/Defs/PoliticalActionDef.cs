using RimWorld;
using System;
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

        public Type actionClass;
        public string actionMethod;
        public int targetArgument;

        public void Activate(Pawn target = null)
        {
            Utility.Log($"{defName} activated for target {target}.");
            DecisionVoteResults opinions = null;
            if (allCitizensReact)
                opinions = new DecisionVoteResults(Utility.Citizens.Select(pawn => new PawnDecisionOpinion(pawn, considerations, target)));
            else if (Utility.RimocracyComp.Leader != null)
                opinions = new DecisionVoteResults() { new PawnDecisionOpinion(Utility.RimocracyComp.Leader, considerations, target) };

            foreach (PawnDecisionOpinion opinion in opinions.Where(opinion => opinion.support != 0))
            {
                opinion.voter.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RimocracyDefOf.DecisionMade, opinion.support > 0 ? 1 : 0));
                if (opinion.voter == Utility.RimocracyComp.Leader)
                    Utility.RimocracyComp.Governance = Mathf.Clamp(Utility.RimocracyComp.Governance + opinion.support > 0 ? governanceChangeIfSupported : governanceChangeIfOpposed, 0, 1);
            }

            if (!opinions.EnumerableNullOrEmpty())
                Find.WindowStack.Add(new Dialog_PoliticalAction(this, opinions));
        }
    }
}
