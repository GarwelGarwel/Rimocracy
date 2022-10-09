using RimWorld;
using Verse;

namespace Rimocracy
{
    public class RoleRequirement_Leader : RoleRequirement
    {
        public override bool Met(Pawn p, Precept_Role role) =>
            Utility.RimocracyComp == null
            || p.HomeFaction == null
            || !p.HomeFaction.IsPlayer
            || (role.def.leaderRole == (p.IsLeader() && !Utility.RimocracyComp.DecisionActive(DecisionDef.Multiculturalism)));

        public override string GetLabel(Precept_Role role) =>
            role.def.leaderRole
            ? ((Utility.PoliticsEnabled && Utility.RimocracyComp.DecisionActive(DecisionDef.Multiculturalism)) ? "Disabled when Multiculturalism is active" : "Chosen according to SuccessionDef law")
            : $"Can't be the current {Utility.IdeologyLeaderPrecept(role.ideo)?.Label ?? "leader"}";
    }
}
