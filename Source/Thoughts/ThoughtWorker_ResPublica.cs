using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ThoughtWorker_ResPublica : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!Utility.RimocracyComp.DecisionActive("ResPublica"))
                return ThoughtState.Inactive;

            float g = Utility.RimocracyComp.Governance;
            if (g < 0.10)
                return ThoughtState.ActiveAtStage(0);
            else if (g < 0.25)
                return ThoughtState.ActiveAtStage(1);
            else if (g < 0.75)
                return ThoughtState.Inactive;
            else if (g < 0.95)
                return ThoughtState.ActiveAtStage(3);
            return ThoughtState.ActiveAtStage(4);
        }
    }
}
