﻿using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class Dialog_PoliticalAction : Window
    {
        PoliticalActionDef action;

        DecisionVoteResults opinions;

        bool actionTaken;

        float governanceChangeFactor;

        public Dialog_PoliticalAction(PoliticalActionDef action, DecisionVoteResults opinions, bool actionTaken, float governanceChangeFactor = 1)
        {
            this.action = action;
            this.opinions = opinions;
            this.actionTaken = actionTaken;
            this.governanceChangeFactor = governanceChangeFactor;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            forcePause = true;
            Utility.Log($"Opinions of {action.defName}: {opinions}");
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
                content.Label($"{opinions.Yea.ToStringCached()} citizens support the action, {opinions.Nay.ToStringCached()} oppose it, and {opinions.Abstentions.ToStringCached()} are indiffirent.");

            if (Utility.RimocracyComp.HasLeader)
            {
                PawnDecisionOpinion opinion = opinions[Utility.RimocracyComp.Leader];
                if (actionTaken)
                {
                    float govChange = 0;
                    if (opinion.Vote == DecisionVote.Yea && action.governanceChangeIfSupported != 0)
                        govChange = action.governanceChangeIfSupported * governanceChangeFactor;
                    else if (opinion.Vote == DecisionVote.Nay && action.governanceChangeIfOpposed != 0)
                        govChange = action.governanceChangeIfOpposed * governanceChangeFactor;
                    if (Mathf.Abs(govChange) >= 0.001f)
                        content.Label($"Governance changed by {govChange.ToStringPercent()}, because the {Utility.LeaderTitle} {(opinion.Vote == DecisionVote.Yea ? "spearheaded" : "protested")} the action.");
                }
                content.Label($"{Utility.LeaderTitle.CapitalizeFirst()} {Utility.RimocracyComp.Leader.NameShortColored}: {opinion.support.ToStringWithSign("0")}", tooltip: opinion.explanation);
            }

            foreach (PawnDecisionOpinion opinion in opinions.Where(opinion => opinion.voter != Utility.RimocracyComp.Leader))
                content.Label($"{opinion.voter.NameShortColored}: {opinion.support.ToStringWithSign("0")}", tooltip: opinion.explanation);

            content.Gap();
            content.CheckboxLabeled("Always show action details", ref Settings.ShowActionSupportDetails, "Uncheck to disable this dialog from popping up. You may re-enable it in the Settings.");

            content.End();
        }

        public static void Show(PoliticalActionDef action, DecisionVoteResults opinions, bool actionTaken, float governanceChangeFactor = 1)
        {
            if (opinions.Any(opinion => opinion.Vote != DecisionVote.Abstain))
                Find.WindowStack.Add(new Dialog_PoliticalAction(action, opinions, actionTaken, governanceChangeFactor));
        }
    }
}
