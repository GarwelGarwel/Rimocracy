using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Need_Loyalty : Need
    {
        const int TicksPerInterval = 150;

        public const float MoodChangeBase = TicksPerInterval * 0.1f / 60000;
        public const float LoyaltyResetOnLeaderChange = 0.25f;
        public const float CriticalLevel = 0.10f;
        public const float DisloyalLevel = 0.30f;
        public const float DefaultLevel = 0.50f;

        float lastChange;

        public override int GUIChangeArrow => IsFrozen ? 0 : Math.Sign(lastChange);

        protected override bool IsFrozen => base.IsFrozen || !Utility.PoliticsEnabled || !pawn.IsCitizen();

        public override bool ShowOnNeedList => base.ShowOnNeedList && Utility.PoliticsEnabled && pawn.IsCitizen();

        public float StartProtestLevel => Prefs.DevMode ? 0.40f : CriticalLevel;

        public float JoinProtestLevel => Prefs.DevMode ? DefaultLevel : DisloyalLevel;

        public float StartProtestMTB => Prefs.DevMode? 3 : 24 * 5 * (1 + CurLevel / StartProtestLevel) * (1 + Utility.RimocracyComp.Governance);

        public Need_Loyalty(Pawn pawn)
            : base(pawn) =>
            threshPercents = new List<float>()
            {
                StartProtestLevel,
                DisloyalLevel,
                DefaultLevel
            };

        static ProtestDef RandomProtestDefFor(Pawn pawn) => DefDatabase<ProtestDef>.AllDefs.Where(def => def.AppliesTo(pawn)).RandomElementByWeightWithFallback(def => def.weight);

        public void StartProtest()
        {
            if (pawn.InMentalState || QuestUtility.AnyQuestDisablesRandomMoodCausedMentalBreaksFor(pawn))
                return;
            Utility.Log($"Trying to start a protest for {pawn}.");

            ProtestDef protest = RandomProtestDefFor(pawn);

            if (protest == null)
            {
                Utility.Log($"Could not find a suitable ProtestDef for {pawn} out of {DefDatabase<ProtestDef>.DefCount} defs.", LogLevel.Warning);
                return;
            }
            Utility.Log($"{pawn} initiates {protest}.");

            List<Pawn> protesters = new List<Pawn>() { pawn };
            TaggedString message = protest.DescriptionFor(pawn);

            if (pawn.mindState.mentalStateHandler.TryStartMentalState(protest.mentalState, transitionSilently: true))
            {
                Need_Loyalty need = null;
                foreach (Pawn pawn2 in Utility.Citizens.Where(pawn2 =>
                    pawn.MapHeld == pawn2.MapHeld
                    && (need = pawn2.needs.AllNeeds.OfType<Need_Loyalty>().FirstOrDefault()) != null
                    && need.CurLevel < need.JoinProtestLevel))
                {
                    Utility.Log($"{pawn2} joins the protest with {protest}.");
                    protest = RandomProtestDefFor(pawn2);
                    pawn2.mindState.mentalStateHandler.TryStartMentalState(protest.mentalState, transitionSilently: true);
                    protesters.Add(pawn2);
                    message += $"\n{protest.DescriptionFor(pawn2)}";
                }
            }

            message = $"{protesters.Count} citizen{(protesters.Count > 1 ? "s are" : " is")} participating in a protest started by {pawn.NameShortColored}.\n{message}";
            Find.LetterStack.ReceiveLetter($"Political Protest", message, LetterDefOf.NegativeEvent, new LookTargets(protesters));
        }

        public override void NeedInterval()
        {
            if (!IsFrozen)
            {
                lastChange = (pawn.needs.mood.CurLevelPercentage - 0.50f) * MoodChangeBase * Settings.LoyaltyChangeSpeed;
                CurLevel += lastChange;

                if (CurLevel < StartProtestLevel)
                {
                    if ((Find.TickManager.TicksAbs / TicksPerInterval) % 20 == 0)
                        Utility.Log($"{pawn} starts protesting in ~{StartProtestMTB:N0} hours.");
                    if (Rand.MTBEventOccurs(StartProtestMTB, GenDate.TicksPerHour, TicksPerInterval))
                        StartProtest();
                }
            }
        }

        public override string GetTipString() =>
            CurLevel >= StartProtestLevel
            ? base.GetTipString()
            : $"{base.GetTipString()}\n<color=red>Will protest in {GenDate.ToStringTicksToPeriodVague((int)(StartProtestMTB * GenDate.TicksPerHour))}.</color>";
    }
}
