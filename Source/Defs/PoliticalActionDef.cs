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
        public ThoughtDef supportThought;
        public ThoughtDef opposeThought;
        public float governanceChangeIfSupported;
        public float governanceChangeIfOpposed;
        public float loyaltyEffect = 3;

        public string LabelTitleCase => GenText.ToTitleCaseSmart(label);

        public DecisionVoteResults GetOpinions(Pawn target = null) =>
            allCitizensReact
            ? new DecisionVoteResults(Utility.Citizens.Select(pawn => new PawnDecisionOpinion(pawn, considerations, target)))
            : (Utility.RimocracyComp.HasLeader ? new DecisionVoteResults() { new PawnDecisionOpinion(Utility.RimocracyComp.Leader, considerations, target) } : new DecisionVoteResults());

        public void Activate(DecisionVoteResults opinions, float governanceChangeFactor = 1)
        {
            Utility.Log($"{defName} activated.");
            foreach (PawnDecisionOpinion opinion in opinions.Where(opinion => opinion.Vote != DecisionVote.Abstain))
            {
                if (opinion.Vote == DecisionVote.Yea)
                {
                    if (supportThought != null)
                        opinion.voter.needs.mood.thoughts.memories.TryGainMemory(supportThought);
                    opinion.voter.ChangeLoyalty(loyaltyEffect);
                }
                else if (opinion.Vote == DecisionVote.Nay || opinion.Vote == DecisionVote.Tolerate)
                {
                    if (opposeThought != null)
                        opinion.voter.needs.mood.thoughts.memories.TryGainMemory(opposeThought);
                    opinion.voter.ChangeLoyalty(-loyaltyEffect);
                }
                if (opinion.voter == Utility.RimocracyComp.Leader)
                    Utility.RimocracyComp.Governance = Mathf.Clamp(
                        Utility.RimocracyComp.Governance + (opinion.Vote == DecisionVote.Yea ? governanceChangeIfSupported : governanceChangeIfOpposed) * governanceChangeFactor,
                        0,
                        1);
            }

            if (Settings.ShowActionSupportDetails)
                Dialog_PoliticalAction.Show(this, opinions, true, governanceChangeFactor);
        }

        public void Activate(Pawn target = null, float governanceChangeFactor = 1) => Activate(GetOpinions(target), governanceChangeFactor);
    }
}
