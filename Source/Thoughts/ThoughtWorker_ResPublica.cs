using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ThoughtWorker_ResPublica : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!Utility.RimocracyComp.DecisionActive("ResPublica") || !p.IsCitizen())
                return ThoughtState.Inactive;

            float g = Utility.RimocracyComp.Governance;
            if (g < 0.05f)
                return ThoughtState.ActiveAtStage(0);
            else if (g < 0.25f)
                return ThoughtState.ActiveAtStage(1);
            else if (g < 0.75f)
                return ThoughtState.Inactive;
            else if (g < 0.95f)
                return ThoughtState.ActiveAtStage(2);
            return ThoughtState.ActiveAtStage(3);
        }
    }
}
