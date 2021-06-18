using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class Dialog_PoliticalAction : Window
    {
        PoliticalActionDef action;

        DecisionVoteResults opinions;

        bool actionTaken;

        public Dialog_PoliticalAction(PoliticalActionDef action, DecisionVoteResults opinions, bool actionTaken)
        {
            this.action = action;
            this.opinions = opinions;
            this.actionTaken = actionTaken;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            forcePause = true;
            Utility.Log($"Opinions of {action.defName}:{opinions}");
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard content = new Listing_Standard();
            content.Begin(inRect);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            content.Label($"{action.LabelTitleCase} Action {(actionTaken ? "Effects" : "Vetoed")}");
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            if (opinions.Count > 1)
                content.Label($"{opinions.Yea} citizens support the action, {opinions.Nay} oppose it, and {opinions.Abstentions} are indiffirent.");

            if (Utility.RimocracyComp.HasLeader)
            {
                PawnDecisionOpinion opinion = opinions[Utility.RimocracyComp.Leader];
                if (actionTaken)
                    if (opinion.Vote == DecisionVote.Yea && action.governanceChangeIfSupported != 0)
                        content.Label($"Governance changed by {action.governanceChangeIfSupported.ToStringPercent()}, because the {Utility.LeaderTitle} spearheaded the action.");
                    else if (opinion.Vote == DecisionVote.Nay && action.governanceChangeIfOpposed != 0)
                        content.Label($"Governance changed by {action.governanceChangeIfOpposed.ToStringPercent()}, because the action was taken despite {Utility.LeaderTitle}'s opposition.");
                content.Label($"{Utility.LeaderTitle.CapitalizeFirst()} {Utility.RimocracyComp.Leader.NameShortColored}: {opinion.support.ToStringWithSign("0")}", tooltip: opinion.explanation);
            }

            foreach (PawnDecisionOpinion opinion in opinions.Where(opinion => opinion.voter != Utility.RimocracyComp.Leader))
                content.Label($"{opinion.voter.NameShortColored}: {opinion.support.ToStringWithSign("0")}", tooltip: opinion.explanation);

            content.End();
        }

        public static void Show(PoliticalActionDef action, DecisionVoteResults opinions, bool actionTaken)
        {
            if (!opinions.EnumerableNullOrEmpty() && Settings.ShowActionSupportDetails)
                Find.WindowStack.Add(new Dialog_PoliticalAction(action, opinions, actionTaken));
        }
    }
}
