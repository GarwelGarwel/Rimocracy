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

        bool initialized = false;

        public Rimocracy()
            : base(Find.World)
        { }

        public Rimocracy(World world)
            : base(world)
        { }

        public Pawn Leader
        {
            get => leader;
            set => leader = value;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref leader, "leader");
        }

        public override void WorldComponentTick()
        {
            if (initialized)
                return;
            Utility.Log("WorldComponentTick: initializing...");
            if (leader == null)
                ChooseLeader();
            else Utility.Log("Current leader is " + leader);
            initialized = true;
        }

        void ChooseLeader()
        {
            Utility.Log("ChooseLeader");
            List<Pawn> candidates = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep;
            leader = candidates.FirstOrDefault();
            if (leader != null)
                Utility.Log("New leader is " + leader);
            else
            {
                Utility.Log("No suitable leader found. " + candidates.Count + " total candidate pawns:");
                foreach (Pawn p in candidates)
                    Utility.Log("- " + (p.IsColonist ? "COLONIST " : "") + p);
            }
        }
    }
}
