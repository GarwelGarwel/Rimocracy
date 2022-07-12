using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
        public float loyaltyEffect = 0.04f;

        public string LabelTitleCase => GenText.ToTitleCaseSmart(label);

        public DecisionVoteResults GetOpinions(Pawn target) =>
            allCitizensReact
            ? new DecisionVoteResults(Utility.Citizens.Select(pawn => new PawnDecisionOpinion(pawn, considerations, target)))
            : (Utility.RimocracyComp.HasLeader ? new DecisionVoteResults() { new PawnDecisionOpinion(Utility.RimocracyComp.Leader, considerations, target) } : new DecisionVoteResults());

        public void Activate(DecisionVoteResults opinions, float scaleFactor = 1)
        {
            Utility.Log($"{defName} activated.");
            foreach (PawnDecisionOpinion opinion in opinions)
            {
                Utility.Log($"{opinion.voter}'s opinion is {opinion.support.ToStringWithSign()}.");
                switch (opinion.Vote)
                {
                    case DecisionVote.Yea:
                        if (supportThought != null)
                            opinion.voter.needs.mood.thoughts.memories.TryGainMemory(supportThought);
                        opinion.voter.ChangeLoyalty(loyaltyEffect * scaleFactor);
                        if (opinion.voter == Utility.RimocracyComp.Leader)
                            Utility.RimocracyComp.ChangeGovernance(governanceChangeIfSupported * scaleFactor);
                        break;

                    case DecisionVote.Nay:
                        if (opposeThought != null)
                            opinion.voter.needs.mood.thoughts.memories.TryGainMemory(opposeThought);
                        opinion.voter.ChangeLoyalty(-loyaltyEffect * scaleFactor);
                        if (opinion.voter == Utility.RimocracyComp.Leader)
                            Utility.RimocracyComp.ChangeGovernance(governanceChangeIfSupported * scaleFactor);
                        break;

                    case DecisionVote.Tolerate:
                        opinion.voter.ChangeLoyalty(-loyaltyEffect * scaleFactor * Need_Loyalty.ToleratedDecisionLoyaltyFactor);
                        break;
                }
            }

            if (Settings.ShowActionSupportDetails)
                Dialog_PoliticalAction.Show(this, opinions, true, scaleFactor);
        }

        public void Activate(Pawn target = null, float scaleFactor = 1) => Activate(GetOpinions(target), scaleFactor);
    }
}
