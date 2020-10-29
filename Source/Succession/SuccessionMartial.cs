using RimWorld;
using Verse;

namespace Rimocracy.Succession
{
    class SuccessionMartial : SuccessionBase
    {
        const float MechanoidValue = 1;
        const float AnimalValue = 0.05f;
        const float DownedValue = 0.2f;
        const float DamageDealtValue = 0.001f;

        public override SuccessionType SuccessionType => SuccessionType.Martial;

        public override string Title => "Best Warrior";

        public override float RegimeEffect => -0.10f;

        public override Pawn ChooseLeader() => Candidates.MaxByWithFallback(pawn => GetMartialRenown(pawn));

        public override bool CanBeCandidate(Pawn pawn) => base.CanBeCandidate(pawn) && !pawn.WorkTagIsDisabled(WorkTags.Violent);

        public override string NewLeaderMessage(Pawn leader) =>
            $"{leader.Name} will now lead {Utility.NationName} to glory as its most renowned warrior.";

        public override string SameLeaderMessage(Pawn leader) =>
            $"{leader.Name} is still the most able worrior of {Utility.NationName}. Friends and foes alike bow before {leader.gender.GetObjective()}!";

        float GetMartialRenown(Pawn pawn) =>
            pawn.records.GetValue(RecordDefOf.KillsHumanlikes)
            + pawn.records.GetValue(RecordDefOf.KillsMechanoids) * MechanoidValue
            + pawn.records.GetValue(RecordDefOf.KillsAnimals) * AnimalValue
            + pawn.records.GetValue(RecordDefOf.PawnsDownedHumanlikes) * DownedValue
            + pawn.records.GetValue(RecordDefOf.PawnsDownedMechanoids) * MechanoidValue * DownedValue
            + pawn.records.GetValue(RecordDefOf.PawnsDownedAnimals) * AnimalValue * DownedValue
            + pawn.records.GetValue(RecordDefOf.DamageDealt) * DamageDealtValue;
    }
}
