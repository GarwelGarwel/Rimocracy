using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class Dialog_Decision : Window
    {
        DecisionDef decisionDef;

        public Dialog_Decision(DecisionDef decisionDef)
        {
            this.decisionDef = decisionDef;
            doCloseButton = false;
            doCloseX = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (decisionDef == null)
                return;

            Listing_Standard content = new Listing_Standard();
            content.Begin(inRect);
            content.Label($"Activate {decisionDef.LabelCap}?");
            content.Label(decisionDef.description);
            if (decisionDef.governanceCost != 0)
                content.Label($"Will reduce Governance Quality by {decisionDef.governanceCost:P0}.");
            content.Gap();
            if (content.ButtonText("Accept"))
                OnAcceptKeyPressed();
            if (content.ButtonText("Reject"))
                Close();
            content.End();
        }

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();
            Utility.Log($"Activating {decisionDef?.defName}.");
            if (decisionDef == null)
                return;
            decisionDef.Activate();
        }
    }
}
