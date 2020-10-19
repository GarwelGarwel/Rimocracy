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
            optionalTitle = "Political Decisions";
            doCloseX = true;
            closeOnClickedOutside = true;
            draggable = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            viewRect.width = inRect.width - 20;
            Listing_Standard content = new Listing_Standard();
            content.BeginScrollView(inRect.AtZero(), ref scrollPosition, ref viewRect);
            if (Utility.RimocracyComp.Regime != 0)
                content.Label($"The current regime is {Math.Abs(Utility.RimocracyComp.Regime) * 100:N0}% towards {(Utility.RimocracyComp.Regime > 0 ? "democracy" : "dictatorship")}.");
            else content.Label("The current regime is neither a democracy nor a dictatorship.");
            foreach (DecisionDef d in DefDatabase<DecisionDef>.AllDefs.Where(def => def.IsDisplayable))
            {
                content.GapLine();
                content.Label(d.LabelCap);
                content.Label(d.description);
                if (d.governanceCost != 0)
                    content.Label($"Will reduce Governance by {d.governanceCost * 100:N0}%.");
                if (d.regimeEffect != 0)
                    content.Label($"Will move the regime {Math.Abs(d.regimeEffect)* 100:N0}% towards {(d.regimeEffect > 0 ? "democracy" : "dictatorship")}.");
                if (!d.effectRequirements.IsTrivial)
                    content.Label($"Requirements:\n{d.effectRequirements}");
                if (d.IsValid)
                    if (content.ButtonText("Activate"))
                    {
                        Utility.Log($"Activating {d.defName}.");
                        d.Activate();
                        Messages.Message($"{d.LabelCap} decision taken.", MessageTypeDefOf.NeutralEvent);
                        Close();
                    }
            }
            content.EndScrollView(ref viewRect);
        }
    }
}
