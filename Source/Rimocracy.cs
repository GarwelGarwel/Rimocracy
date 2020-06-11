using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Rimocracy : WorldComponent
    {
        Pawn leader;
        float authority = 0.1f;
        bool initialized = false;

        public Rimocracy()
            : base(Find.World)
        { }

        public Rimocracy(World world)
            : base(world)
        { }

        public static Rimocracy Instance => Find.World.GetComponent<Rimocracy>();

        public Pawn Leader
        {
            get => leader;
            set => leader = value;
        }

        public float Authority
        {
            get => authority;
            set => authority = value;
        }

        public static bool CanBeLeader(Pawn p) => p != null && !p.Dead && p.IsFreeColonist;

        public override void ExposeData()
        {
            Scribe_References.Look(ref leader, "leader");
            Scribe_Values.Look(ref authority, "authority");
        }

        public override void WorldComponentTick()
        {
            if (initialized)
                return;
            Utility.Log("WorldComponentTick: initializing...");
            if (leader == null || !CanBeLeader(leader))
                ChooseLeader();
            else Utility.Log("Current leader is " + leader);
            initialized = true;
        }

        void ChooseLeader()
        {
            Utility.Log("ChooseLeader");
            List<Pawn> candidates = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep;
            leader = candidates.FirstOrDefault(p => CanBeLeader(p));
            if (leader != null)
            {
                Utility.Log("New leader is " + leader + " (chosen from " + candidates.Count + " candidates).");
                authority = (float)(leader.skills.GetSkill(SkillDefOf.Intellectual).Level + leader.skills.GetSkill(SkillDefOf.Social).Level) / 40;
                Utility.Log("Authority level is " + authority.ToString("P0"));
            }
            else Utility.Log("No suitable leader found. " + candidates.Count + " total candidate pawns:");
        }
    }
}
