using RimWorld;
using Verse;

namespace Rimocracy
{
    public class RoleRequirement_Leader : RoleRequirement
    {
        public override bool Met(Pawn p, Precept_Role role) => !p.HomeFaction.IsPlayer || (role.def.leaderRole == p.IsLeader());

        public override string GetLabel(Precept_Role role) =>
            role.def.leaderRole ? "Chosen according to succession law" : $"Can't be the current {Utility.IdeologyLeaderPrecept(role.ideo)?.Label ?? "leader"}";
    }
}
