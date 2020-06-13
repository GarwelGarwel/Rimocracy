using Rimocracy.Succession;
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
        // Base amount of authority built while doing Rule job
        public const float BaseAuthorityBuildPerTick = 0.000004f;

        // Default duration of a leader's turn (1 day for testing purposes)
        public const int DefaultTerm = 60000;

        Pawn leader;
        SuccessionBase succession;
        int termExpiration = -1;
        float authority;

        public Rimocracy()
            : this(Find.World)
        { }

        public Rimocracy(World world)
            : base(world)
        {
            succession = new SuccessionLot();
        }

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

        public int TermExpiration
        {
            get => termExpiration;
            set => termExpiration = value;
        }

        //public static List<Pawn> Citizens = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep;

        //public static int Population = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count;

        public float AuthorityPercentage => 100 * Authority;

        float AuthorityDecayPerTick => 0.1f - 0.2f / PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count / 60000;

        public static bool CanBeLeader(Pawn p)
            => p != null && !p.Dead && p.IsFreeColonist && !p.WorkTypeIsDisabled(DefDatabase<WorkTypeDef>.GetNamed("Ruling"));

        public static bool IsEnabled => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count >= 3;

        public override void ExposeData()
        {
            Scribe_References.Look(ref leader, "leader");
            Scribe_Values.Look(ref termExpiration, "termExpiration", -1);
            Scribe_Values.Look(ref authority, "authority");
        }

        public override void WorldComponentTick()
        {
            int ticks = Find.TickManager.TicksAbs;

            // If no leader, choose a new one and reset term
            if (leader == null || !CanBeLeader(leader) || (termExpiration >= 0 && ticks >= termExpiration))
            {
                if (succession == null)
                {
                    Utility.Log("Succession is null!", LogLevel.Error);
                    succession = new SuccessionLot();
                }
                leader = succession.ChooseLeader();
                if (leader != null)
                {
                    termExpiration = ticks + DefaultTerm;
                    Utility.Log("New leader is " + leader + " (chosen from " + succession.Candidates.Count() + " candidates). Their term expires on " + GenDate.DateFullStringAt(termExpiration, Find.WorldGrid.LongLatOf(leader.Tile)));
                }
                authority /= 2;
            }

            if (ticks % 600 == 0)
            {
                // Authority decay
                float oldAuthority = authority;
                authority = Math.Max(authority - Math.Min(AuthorityDecayPerTick * (leader != null ? leader.GetStatValue(DefDatabase<StatDef>.GetNamed("AuthorityDecay")) : 1) * 600, 0.0003f), 0);
                Utility.Log("Authority decay from " + oldAuthority.ToString("P2") + " to " + authority.ToString("P2") + " for " + PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count + " colonists.");
            }
        }

        public void BuildAuthority(float amount) => authority = Math.Min(Authority + amount, 1);
    }
}
