﻿using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class Dialog_PoliticalAction : Window
    {
        static bool showOpinions;

        Listing_Standard content = new Listing_Standard();
        RimocracyComp comp = Utility.RimocracyComp;
        PoliticalActionDef action;
        DecisionVoteResults opinions;
        bool actionTaken;
        float scaleFactor;

        public Dialog_PoliticalAction(PoliticalActionDef action, DecisionVoteResults opinions, bool actionTaken, float scaleFactor = 1)
            : base()
        {
            this.action = action;
            this.opinions = opinions;
            this.actionTaken = actionTaken;
            this.scaleFactor = scaleFactor;
            optionalTitle = $"{action.LabelTitleCase} Action {(actionTaken ? "Effects" : "Vetoed")}";
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            forcePause = true;
            draggable = true;
        }

        public override Vector2 InitialSize => new Vector2(500, Mathf.Min(220 + (showOpinions ? Text.LineHeight * Utility.CitizensCount : 0), UI.screenHeight));

        public override void DoWindowContents(Rect inRect)
        {
            content.Begin(inRect);

            if (content.ButtonTextLabeledPct(
                $"{opinions.Yea.ToStringCached()} citizens {"support".Colorize(Color.green)} the action, {opinions.Nay.ToStringCached()} {"oppose".Colorize(Color.red)} it, and {opinions.Tolerates.ToStringCached()} are unhappy but {"tolerate".Colorize(Color.yellow)} it.",
                showOpinions ? "Hide Details" : "Show Details",
                0.70f))
            {
                showOpinions = !showOpinions;
                SetInitialSizeAndPosition();
            }

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
                if (showOpinions)
                    content.Label($"{Utility.LeaderTitle.CapitalizeFirst()} {comp.Leader.NameShortColored}: {opinion.VoteStringColored}", tooltip: opinion.explanation);
            }

            if (showOpinions)
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
