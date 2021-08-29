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

        DecisionDef decisionToShowDetails;
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
                viewRect.height = 500 + availableDecisions.Count * 100;
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
                Text.Anchor = TextAnchor.UpperLeft;
                
                foreach (DecisionDef def in group.OrderBy(def => def.displayPriorityInCategory))
                {
                    if (!def.IsValid)
                        GUI.color = Color.gray;
                    if (content.ButtonTextLabeled(def.LabelTitleCase, "Details"))
                        decisionToShowDetails = decisionToShowDetails == def ? null : def;
                    GUI.color = Color.white;
                    if (decisionToShowDetails == def)
                    {
                        content.Label(def.description);
                        if (def.governanceCost != 0)
                            content.Label($"Will {(def.governanceCost > 0 ? "reduce" : "increase")} Governance by {Math.Abs(def.GovernanceCost).ToStringPercent()}.");
                        if (def.regimeEffect != 0)
                            content.Label($"Will move the regime {Math.Abs(def.regimeEffect).ToStringPercent()} towards {(def.regimeEffect > 0 ? "democracy" : "authoritarianism")}.");
                        if (!def.effectRequirements.IsTrivial)
                            content.Label($"Requirements:\n{def.effectRequirements.ToString(target: Utility.RimocracyComp.Leader?.NameShortColored)}");

                        switch (def.enactment)
                        {
                            case DecisionEnactmentRule.Decree:
                                content.Label($"Requires {Utility.LeaderTitle}'s approval.");
                                break;

                            case DecisionEnactmentRule.Law:
                            case DecisionEnactmentRule.Referendum:
                                content.Label($"Requires approval of a majority of citizens.");
                                break;
                        }
                        
                        DecisionVoteResults votingResult = def.GetVotingResults(Utility.Citizens.ToList());
                        if (def.enactment == DecisionEnactmentRule.Decree && Utility.RimocracyComp.HasLeader)
                        {
                            PawnDecisionOpinion leaderOpinion = votingResult[Utility.RimocracyComp.Leader];
                            content.Label($"{Utility.LeaderTitle.CapitalizeFirst()}'s support: {leaderOpinion.support.ToStringWithSign("0")}", tooltip: leaderOpinion.explanation);
                        }

                        if ((def.allCitizensReact || def.enactment == DecisionEnactmentRule.Law || def.enactment == DecisionEnactmentRule.Referendum) && votingResult.Any(opinion => opinion.Vote != DecisionVote.Abstain))
                        {
                            content.Label($"Citizens' support: {votingResult.Yea.ToStringCached()} - {votingResult.Nay.ToStringCached()}");
                            foreach (PawnDecisionOpinion opinion in votingResult)
                                content.Label($"  {opinion.voter.NameShortColored}: {opinion.support.ToStringWithSign("0")}", tooltip: opinion.explanation);
                        }

                        // Display Activate button for valid decisions
                        if (def.IsValid && def.IsPassed(votingResult))
                        {
                            if (content.ButtonText("Activate"))
                            {
                                Utility.Log($"Activating {def.defName}.");
                                if (def.Activate())
                                {
                                    foreach (PawnDecisionOpinion opinion in votingResult.Where(opinion => opinion.Vote != DecisionVote.Abstain))
                                    {
                                        Utility.Log($"{opinion.voter}'s opinion is {opinion.support.ToStringWithSign()}.");
                                        opinion.voter.needs.mood.thoughts.memories.TryGainMemory(opinion.Vote == DecisionVote.Yea ? RimocracyDefOf.LikeDecision : RimocracyDefOf.DislikeDecision);
                                    }
                                    Find.LetterStack.ReceiveLetter($"{def.LabelTitleCase} Decision Taken", def.description, LetterDefOf.NeutralEvent, null);
                                }
                                else Messages.Message($"Could not take {def.LabelTitleCase} decision: requirements are not met.", MessageTypeDefOf.NegativeEvent, false);
                                Close();
                            }
                        }
                        else content.Label("Requirements are not met.");

                        // Display devmode (cheat) Activate button
                        if (Prefs.DevMode && content.ButtonText("Activate (DevMode)"))
                        {
                            def.Activate(true);
                            Close();
                        }

                        content.GapLine();
                    }
                }
            }

            viewRect.height = content.CurHeight;
            content.End();
            Widgets.EndScrollView();
        }
    }
}
