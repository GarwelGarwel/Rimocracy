using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Rimocracy : WorldComponent
    {
        public const float BaseAuthorityBuildPerTick = 0.000004f;

        Pawn leader;
        float authority;

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

        float AuthorityDecayPerTick
            => (0.1f - 0.2f / PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count) / 60000;

        public static bool CanBeLeader(Pawn p)
            => p != null && !p.Dead && p.IsFreeColonist && !p.WorkTypeIsDisabled(DefDatabase<WorkTypeDef>.GetNamed("Ruling"));

        public override void ExposeData()
        {
            Scribe_References.Look(ref leader, "leader");
            Scribe_Values.Look(ref authority, "authority");
        }

        public override void WorldComponentTick()
        {
            if (Find.TickManager.TicksAbs % 600 == 0)
                Utility.Log("Authority: " + authority.ToString("P4"));

            if (Find.TickManager.TicksAbs % 600 == 0)
            {
                // Authority decay
                Utility.Log("Authority decay for " + PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count + " colonists.");
                Authority = Math.Max(Authority - AuthorityDecayPerTick * 600, 0);
            }

            // If no leader, choose a new one
            if (leader == null || !CanBeLeader(leader))
                ChooseLeader();
        }

        void ChooseLeader()
        {
            Utility.Log("ChooseLeader");
            List<Pawn> candidates = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep;
            leader = candidates.FirstOrDefault(p => CanBeLeader(p));
            if (leader != null)
            {
                Messages.Message(leader + " is the new leader of " + Find.FactionManager.OfPlayer.Name + ".", MessageTypeDefOf.NeutralEvent);
                Utility.Log("New leader is " + leader + " (chosen from " + candidates.Count + " candidates).");
            }
            else Utility.Log("No suitable leader found. " + candidates.Count + " total candidate pawns:");
            authority = 0;
        }

        public void BuildAuthority(float amount)
        {
            Authority = Math.Min(Authority + amount, 1);
        }
    }
}
