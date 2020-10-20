using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class Dialog_DecisionList : Window
    {
        Vector2 scrollPosition = new Vector2();
        Rect viewRect = new Rect();

        public Dialog_DecisionList()
        {
            doCloseX = true;
            closeOnClickedOutside = true;
            draggable = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            viewRect.width = inRect.width - 20;
            Listing_Standard content = new Listing_Standard();
            content.BeginScrollView(inRect.AtZero(), ref scrollPosition, ref viewRect);

            // Display regime type
            if (Utility.RimocracyComp.RegimeFinal != 0)
                content.Label($"The current regime is {Math.Abs(Utility.RimocracyComp.RegimeFinal) * 100:N0}% {(Utility.RimocracyComp.RegimeFinal > 0 ? "democratic" : "authoritarian")}.");
            else content.Label("The current regime is neither democratic nor authoritarian.");

            // Display decision categories and available decisions
            foreach (IGrouping<DecisionCategoryDef, DecisionDef> group in DefDatabase<DecisionDef>.AllDefs.Where(def => def.IsDisplayable).GroupBy(def => def.category).OrderBy(group => group.Key.displayOrder))
            {
                content.Gap();
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Medium;
                content.Label(group.Key.label);
                Text.Font = GameFont.Small;
                foreach (DecisionDef d in group.OrderBy(def => def.displayPriorityInCategory))
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    content.Label(d.LabelCap);
                    Text.Anchor = TextAnchor.UpperLeft;
                    content.Label(d.description);
                    if (d.governanceCost != 0)
                        content.Label($"Will reduce Governance by {d.governanceCost * 100:N0}%.");
                    if (d.regimeEffect != 0)
                        content.Label($"Will move the regime {Math.Abs(d.regimeEffect) * 100:N0}% towards {(d.regimeEffect > 0 ? "democracy" : "authoritarianism")}.");
                    if (!d.effectRequirements.IsTrivial)
                        content.Label($"Requirements:\n{d.effectRequirements}");

                    // Display Activate button for valid decisions
                    if (d.IsValid)
                    {
                        if (content.ButtonText("Activate"))
                        {
                            Utility.Log($"Activating {d.defName}.");
                            d.Activate();
                            Messages.Message($"{d.LabelCap} decision taken.", MessageTypeDefOf.NeutralEvent);
                            Close();
                        }
                    }
                    else content.Label("Requirements are not met.");
                    content.GapLine();
                }
            }
            content.EndScrollView(ref viewRect);
        }
    }
}
