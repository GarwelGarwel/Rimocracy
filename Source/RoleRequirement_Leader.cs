using RimWorld;
using Verse;

namespace Rimocracy
{
    public class RoleRequirement_Leader : RoleRequirement
    {
        public override bool Met(Pawn p, Precept_Role role) => !role.def.leaderRole || !p.HomeFaction.IsPlayer || (Utility.PoliticsEnabled && p.IsLeader());

        public override string GetLabel(Precept_Role role) => "Chosen according to succession law";
    }
}
