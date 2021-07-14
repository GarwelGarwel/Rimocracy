using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    /// <summary>
    /// Dialog window with the list of possible decisions
    /// </summary>
    public class Dialog_DecisionList : Window
    {
        Vector2 scrollPosition = new Vector2();
        Rect viewRect;

        DecisionDef decisionToShowVoteDetails;
        List<DecisionDef> availableDecisions;

        public Dialog_DecisionList()
        {
            doCloseX = true;
            closeOnClickedOutside = true;
            draggable = true;
            UpdateAvailableDecisions();
        }

        void UpdateAvailableDecisions() => availableDecisions = DefDatabase<DecisionDef>.AllDefs.Where(def => def.IsDisplayable).ToList();

        public override void DoWindowContents(Rect rect)
        {
            if (Utility.RimocracyComp.IsUpdateTick)
            {
                UpdateAvailableDecisions();
                viewRect = new Rect();
            }
            if (viewRect.height < rect.height)
            {
                viewRect.width = rect.width - GenUI.ScrollBarWidth - 4;
                viewRect.height = 500 + availableDecisions.Count * 300;
            }
            Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, viewRect);
            Listing_Standard content = new Listing_Standard();
            content.Begin(viewRect);

            content.Label($"Succession type: {Utility.RimocracyComp.SuccessionType.LabelCap}", tooltip: Utility.RimocracyComp.SuccessionType.description);
            content.Label($"Leader's term: {Utility.RimocracyComp.TermDuration}");

            if (Utility.RimocracyComp.Decisions.Any())
            {
                content.Label("Active decisions:");
                foreach (Decision decision in Utility.RimocracyComp.Decisions)
                    content.Label($"- {decision.def.LabelTitleCase}{(decision.def.Expiration != int.MaxValue ? $" (expires in {(decision.expiration - Find.TickManager.TicksAbs).ToStringTicksToPeriod()})" : "")}", tooltip: decision.def.description);
            }

            // Display regime type
            if (Utility.RimocracyComp.RegimeFinal != 0)
                content.Label($"The current regime is {Math.Abs(Utility.RimocracyComp.RegimeFinal).ToStringPercent()} {(Utility.RimocracyComp.RegimeFinal > 0 ? "democratic" : "authoritarian")}.");
            else content.Label("The current regime is neither democratic nor authoritarian.");

            content.GapLine();

            // Display decision categories and available decisions
            foreach (IGrouping<DecisionCategoryDef, DecisionDef> group in availableDecisions
                .GroupBy(def => def.category)
                .OrderBy(group => group.Key.displayOrder))
            {
                content.Gap();
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Medium;
                content.Label(group.Key.LabelCap);
                Text.Font = GameFont.Small;

                foreach (DecisionDef d in group.OrderBy(def => def.displayPriorityInCategory))
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    content.Label(d.LabelTitleCase);
                    Text.Anchor = TextAnchor.UpperLeft;
                    content.Label(d.description);
                    if (d.governanceCost != 0)
                        content.Label($"Will {(d.governanceCost > 0 ? "reduce" : "increase")} Governance by {Math.Abs(d.GovernanceCost).ToStringPercent()}.");
                    if (d.regimeEffect != 0)
                        content.Label($"Will move the regime {Math.Abs(d.regimeEffect).ToStringPercent()} towards {(d.regimeEffect > 0 ? "democracy" : "authoritarianism")}.");
                    if (!d.effectRequirements.IsTrivial)
                        content.Label($"Requirements:\n{d.effectRequirements.ToString(target: Utility.RimocracyComp.Leader?.NameShortColored)}");

                    DecisionVoteResults votingResult = d.GetVotingResults();
                    switch (d.enactment)
                    {
                        case DecisionEnactmentRule.Decree:
                            if (!votingResult.EnumerableNullOrEmpty())
                                content.Label($"{Utility.LeaderTitle}'s support: {votingResult.First().support.ToStringWithSign("0")}", tooltip: votingResult.First().explanation);
                            break;

                        case DecisionEnactmentRule.Law:
                        case DecisionEnactmentRule.Referendum:
                            if (content.ButtonTextLabeled($"Support: {votingResult.Yea} - {votingResult.Nay}", decisionToShowVoteDetails == d ? "Hide Details" : "Show Details"))
                                decisionToShowVoteDetails = decisionToShowVoteDetails != d ? d : null;
                            if (decisionToShowVoteDetails == d)
                                foreach (PawnDecisionOpinion opinion in votingResult)
                                    content.Label($"  {opinion.voter.NameShortColored}: {opinion.support.ToStringWithSign("0")}", tooltip: opinion.explanation);
                            break;
                    }

                    // Display Activate button for valid decisions
                    if (d.IsValid && d.IsPassed(votingResult))
                    {
                        if (content.ButtonText("Activate"))
                        {
                            Utility.Log($"Activating {d.defName}.");
                            if (d.allCitizensReact && d.enactment != DecisionEnactmentRule.Referendum)
                                votingResult = d.GetVotingResults(Utility.Citizens.ToList());
                            if (d.Activate())
                            {
                                foreach (PawnDecisionOpinion opinion in votingResult.Where(opinion => opinion.Vote != DecisionVote.Abstain))
                                {
                                    Utility.Log($"{opinion.voter}'s opinion is {opinion.support.ToStringWithSign()}.");
                                    opinion.voter.needs.mood.thoughts.memories.TryGainMemory(opinion.Vote == DecisionVote.Yea ? RimocracyDefOf.LikeDecision : RimocracyDefOf.DislikeDecision);
                                }
                                Find.LetterStack.ReceiveLetter($"{d.LabelTitleCase} Decision Taken", d.description, LetterDefOf.NeutralEvent, null);
                            }
                            else Messages.Message($"Could not take {d.LabelTitleCase} decision: requirements are not met.", MessageTypeDefOf.NegativeEvent, false);
                            Close();
                        }
                    }
                    else content.Label("Requirements are not met.");

                    // Display devmode (cheat) Activate button
                    if (Prefs.DevMode && content.ButtonText("Activate (DevMode)"))
                    {
                        d.Activate(true);
                        Close();
                    }

                    content.GapLine();
                }
            }

            viewRect.height = content.CurHeight;
            content.End();
            Widgets.EndScrollView();
        }
    }
}
