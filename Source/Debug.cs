using Verse;

namespace Rimocracy
{
    internal static class Debug
    {
        [DebugAction(category = "General", name = "Lower Governance", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        static void LowerGovernance()
        {
            if (Utility.PoliticsEnabled)
                Utility.RimocracyComp.ChangeGovernance(-0.1f);
        }

        [DebugAction(category = "General", name = "Raise Governance", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        static void RaiseGovernance()
        {
            if (Utility.PoliticsEnabled)
                Utility.RimocracyComp.ChangeGovernance(0.1f);
        }

        [DebugAction("Pawns", "Appoint Leader", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void AppointLeader(Pawn pawn)
        {
            if (Utility.PoliticsEnabled && pawn.CanBeLeader())
                Utility.RimocracyComp.Leader = pawn;
        }

        [DebugAction("Pawns", "Lower Loyalty", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void LowerLoyalty(Pawn pawn) => pawn.ChangeLoyalty(-0.1f);

        [DebugAction("Pawns", "Raise Loyalty", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void RaiseLoyalty(Pawn pawn) => pawn.ChangeLoyalty(0.1f);

        [DebugAction("Pawns", "Start Protest", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void StartProtest(Pawn pawn)
        {
            Need_Loyalty loyalty = pawn.needs.TryGetNeed<Need_Loyalty>();
            if (loyalty != null && !loyalty.IsProtesting)
                loyalty.StartProtest();
        }

        [DebugAction("Pawns", "Stop Protest", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void StopProtest(Pawn pawn)
        {
            Need_Loyalty loyalty = pawn.needs.TryGetNeed<Need_Loyalty>();
            if (loyalty != null && loyalty.IsProtesting)
                loyalty.StopProtest();
        }
    }
}
