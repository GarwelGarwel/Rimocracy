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

        float scaleFactor;

        public Dialog_PoliticalAction(PoliticalActionDef action, DecisionVoteResults opinions, bool actionTaken, float scaleFactor = 1)
        {
            this.action = action;
            this.opinions = opinions;
            this.actionTaken = actionTaken;
            this.scaleFactor = scaleFactor;
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
            content.Label($"{action.LabelTitleCase} Action {(actionTaken ? "Effects" : "Vetoed")}");
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            if (opinions.Count > 1)
                content.Label($"{opinions.Yea.ToStringCached()} citizens {"support".Colorize(Color.green)} the action, {opinions.Nay.ToStringCached()} {"oppose".Colorize(Color.red)} it, and {opinions.Tolerates.ToStringCached()} are unhappy but {"tolerate".Colorize(Color.yellow)} it.");

            RimocracyComp comp = Utility.RimocracyComp;

            if (comp.HasLeader)
            {
                PawnDecisionOpinion opinion = opinions[comp.Leader];
                if (actionTaken)
                {
                    float govChange = 0;
                    if (opinion.Vote == DecisionVote.Yea)
                        govChange = action.governanceChangeIfSupported * scaleFactor;
                    else if (opinion.Vote == DecisionVote.Nay)
                        govChange = action.governanceChangeIfOpposed * scaleFactor;
                    if (Mathf.Abs(govChange) >= 0.001f)
                        content.Label($"Governance changed by {govChange.ToStringPercent().ColorizeByValue(govChange)}, because the {Utility.LeaderTitle} {(opinion.Vote == DecisionVote.Yea ? "spearheaded" : "protested")} the action.");
                }
                content.Label($"{Utility.LeaderTitle.CapitalizeFirst()} {comp.Leader.NameShortColored}: {opinion.VoteStringColored}", tooltip: opinion.explanation);
            }

            foreach (PawnDecisionOpinion opinion in opinions.Where(opinion => opinion.voter != comp.Leader))
                content.Label($"{opinion.voter.NameShortColored}: {opinion.VoteStringColored}", tooltip: opinion.explanation);

            content.Gap();
            content.CheckboxLabeled("Always show this dialog", ref Settings.ShowActionSupportDetails, "Uncheck to disable action details from popping up. You may re-enable it in the Settings.");

            content.End();
        }

        public static void Show(PoliticalActionDef action, DecisionVoteResults opinions, bool actionTaken, float scaleFactor = 1)
        {
            if (opinions.Any(opinion => opinion.Vote != DecisionVote.Abstain))
                Find.WindowStack.Add(new Dialog_PoliticalAction(action, opinions, actionTaken, scaleFactor));
        }
    }
}
