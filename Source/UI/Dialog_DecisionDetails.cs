using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class Dialog_DecisionDetails : Window
    {
        Vector2 scrollPosition = new Vector2();
        Rect viewRect;
        bool showVoteDetails;

        DecisionDef def;

        public Dialog_DecisionDetails(DecisionDef def)
            : base()
        {
            doCloseX = true;
            closeOnClickedOutside = true;
            draggable = true;
            this.def = def;
        }

        public override void DoWindowContents(Rect rect)
        {
            if (viewRect.height < rect.height)
            {
                viewRect.width = rect.width - GenUI.ScrollBarWidth - 4;
                viewRect.height = 500;
            }
            Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, viewRect);
            Listing_Standard content = new Listing_Standard();
            content.Begin(viewRect);

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            content.Label(def.LabelTitleCase);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            content.Label(def.description);
            if (def.governanceCost != 0)
                content.Label($"Will {(def.governanceCost > 0 ? "reduce" : "increase")} Governance by {Math.Abs(def.GovernanceCost).ToStringPercent()}.");
            if (def.regimeEffect != 0)
                content.Label($"Will move the regime {Math.Abs(def.regimeEffect).ToStringPercent()} towards {(def.regimeEffect > 0 ? "democracy" : "authoritarianism")}.");
            if (!def.effectRequirements.IsTrivial)
                content.Label($"Requirements:\n{def.effectRequirements.ToString(target: Utility.RimocracyComp.Leader?.NameShortColored)}");

            DecisionVoteResults votingResult = def.GetVotingResults();
            switch (def.enactment)
            {
                case DecisionEnactmentRule.Decree:
                    if (votingResult.Any())
                        content.Label($"{Utility.LeaderTitle}'s support: {votingResult.First().support.ToStringWithSign("0")}", tooltip: votingResult.First().explanation);
                    break;

                case DecisionEnactmentRule.Law:
                case DecisionEnactmentRule.Referendum:
                    if (content.ButtonTextLabeled($"Support: {votingResult.Yea.ToStringCached()} - {votingResult.Nay.ToStringCached()}", showVoteDetails ? "Hide Details" : "Show Details"))
                        showVoteDetails = !showVoteDetails;
                    if (showVoteDetails)
                        foreach (PawnDecisionOpinion opinion in votingResult)
                            content.Label($"  {opinion.voter.NameShortColored}: {opinion.support.ToStringWithSign("0")}", tooltip: opinion.explanation);
                    break;
            }

            // Display Activate button for valid decisions
            if (def.IsValid && def.IsPassed(votingResult))
            {
                if (content.ButtonText("Activate"))
                {
                    Utility.Log($"Activating {def.defName}.");
                    if (def.allCitizensReact && def.enactment != DecisionEnactmentRule.Referendum)
                        votingResult = def.GetVotingResults(Utility.Citizens.ToList());
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

            if (content.ButtonText("Close"))
                Close();

            viewRect.height = content.CurHeight;
            content.End();
            Widgets.EndScrollView();
        }
    }
}
