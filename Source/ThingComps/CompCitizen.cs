using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class CompCitizen : ThingComp
    {
        public const float LoyaltyChangeBase = 0.05f;
        public const float LeaderChangeLoyaltyResetBase = 0.25f;

        float loyalty;

        public CompCitizen()
        { }

        public CompCitizen(Pawn parent) => Pawn = parent;

        public float Loyalty
        { 
            get => loyalty;
            set => loyalty = Mathf.Clamp(value, -100, 100);
        }

        public Pawn Pawn
        {
            get => parent as Pawn;
            set => parent = value;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref loyalty, "loyalty");
        }

        public override string CompInspectStringExtra() => Pawn.IsCitizen() ? $"Loyalty: {Loyalty:N1}" : null;

        public override void CompTickRare()
        {
            // Don't update Loyalty if politics is disabled, for non-citizens, unconscious and sleeping pawns
            if (!Utility.PoliticsEnabled || !Pawn.IsCitizen() || !Pawn.health.capacities.CanBeAwake || (Pawn.jobs?.curDriver != null && Pawn.jobs.curDriver.asleep))
                return;

            if (Pawn.needs?.mood == null)
            {
                Utility.Log($"{Pawn}'s mood is null.", LogLevel.Error);
                return;
            }

            loyalty += (Pawn.needs.mood.CurLevelPercentage - 0.5f) * LoyaltyChangeBase * Settings.LoyaltyChangeSpeed;
        }
    }
}
