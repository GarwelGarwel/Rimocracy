using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class Dialog_DecisionList : Window
    {
        const int LineHeight = 60;
        List<DecisionDef> availableDecisions = DefDatabase<DecisionDef>.AllDefs.Where(def => def.IsDisplayable).ToList();
        Vector2 scrollPosition = new Vector2();
        Rect viewRect = new Rect();

        public Dialog_DecisionList()
        {
            viewRect.height = 100000;
            //doCloseButton = true;
            doCloseX = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            //int contentHeight = possibleDecisions.Count * LineHeight;
            viewRect.width = inRect.width - 20;
            Listing_Standard content = new Listing_Standard();
            content.BeginScrollView(inRect.AtZero(), ref scrollPosition, ref viewRect);
            foreach (DecisionDef d in availableDecisions)
            {
                content.Label(d.LabelCap);
                content.Label(d.description);
                if (d.governanceCost != 0)
                    content.Label($"Will reduce Governance by {d.governanceCost:P0}.");
                if (!d.effectRequirements.IsTrivial)
                    content.Label($"Conditions:\n{d.effectRequirements}");
                if (d.IsValid)
                    if (content.ButtonText("Activate"))
                    {
                        Utility.Log($"Activating {d.defName}.");
                        d.Activate();
                        Messages.Message($"{d.LabelCap} decision taken.", MessageTypeDefOf.NeutralEvent);
                        Close();
                    }
                //Find.WindowStack.Add(new Dialog_Decision(d));
                content.GapLine();
            }
            content.EndScrollView(ref viewRect);
        }
    }
}
