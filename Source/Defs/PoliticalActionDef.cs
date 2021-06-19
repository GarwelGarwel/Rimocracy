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

        public string LabelTitleCase => GenText.ToTitleCaseSmart(label);

        public DecisionVoteResults GetOpinions(Pawn target = null) =>
            allCitizensReact
            ? new DecisionVoteResults(Utility.Citizens.Select(pawn => new PawnDecisionOpinion(pawn, considerations, target)))
            : (Utility.RimocracyComp.HasLeader ? new DecisionVoteResults() { new PawnDecisionOpinion(Utility.RimocracyComp.Leader, considerations, target) } : new DecisionVoteResults());

        public void Activate(DecisionVoteResults opinions)
        {
            Utility.Log($"{defName} activated.");
            foreach (PawnDecisionOpinion opinion in opinions.Where(opinion => opinion.Vote != DecisionVote.Abstain))
            {
                opinion.voter.needs.mood.thoughts.memories.TryGainMemory(opinion.Vote == DecisionVote.Yea ? RimocracyDefOf.LikeDecision : RimocracyDefOf.DislikeDecision);
                if (opinion.voter == Utility.RimocracyComp.Leader)
                    Utility.RimocracyComp.Governance = Mathf.Clamp(Utility.RimocracyComp.Governance + (opinion.Vote == DecisionVote.Yea ? governanceChangeIfSupported : governanceChangeIfOpposed), 0, 1);
            }

            Dialog_PoliticalAction.Show(this, opinions, true);
        }

        public void Activate(Pawn target = null) => Activate(GetOpinions(target));
    }
}
