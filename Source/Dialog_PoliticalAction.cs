using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class Dialog_PoliticalAction : Window
    {
        PoliticalActionDef action;

        DecisionVoteResults opinions;

        public bool? result;

        public Dialog_PoliticalAction(PoliticalActionDef action, DecisionVoteResults opinions)
        {
            this.action = action;
            this.opinions = opinions;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard content = new Listing_Standard();
            content.Begin(inRect);

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            content.Label(action.LabelCap);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            if (Utility.RimocracyComp.Leader != null)
            {
                PawnDecisionOpinion opinion = opinions[Utility.RimocracyComp.Leader];
                content.Label($"{Utility.LeaderTitle.CapitalizeFirst()} {Utility.RimocracyComp.Leader.NameShortColored}: {opinion.support.ToStringWithSign("0")}", tooltip: opinion.explanation);
                float governanceChange = 0;
                if (opinion.Vote == DecisionVote.Yea)
                    governanceChange = action.governanceChangeIfSupported;
                else if (opinion.Vote == DecisionVote.Nay)
                    governanceChange = action.governanceChangeIfOpposed;
                if (governanceChange != 0)
                    content.Label($"Governance changed by {governanceChange.ToStringPercent()}.");
            }

            foreach (PawnDecisionOpinion opinion in opinions.Where(opinion => opinion.voter != Utility.RimocracyComp.Leader))
                content.Label($"{opinion.voter.NameShortColored}: {opinion.support.ToStringWithSign("0")}", tooltip: opinion.explanation);

            content.End();
        }
    }
}
