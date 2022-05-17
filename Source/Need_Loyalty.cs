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

        public const float MoodChangeBase = 0.1f;
        public const float OpinionOfLeaderChangeBase = 0.0005f;
        public const float LoyaltyResetOnLeaderChange = 0.25f;
        public const float ProtestLevelBase = 0.10f;
        public const float DefaultLevel = 0.50f;

        static List<float> threshPercentsCommon = new List<float>(2)
        {
            ProtestLevelBase,
            DefaultLevel
        };

        float lastChange;
        bool isProtesting;

        public bool IsProtesting => isProtesting;

        public override int GUIChangeArrow => IsFrozen ? 0 : Math.Sign(lastChange);

        protected override bool IsFrozen => base.IsFrozen || !Utility.PoliticsEnabled || !pawn.IsCitizen();

        public override bool ShowOnNeedList => base.ShowOnNeedList && Utility.PoliticsEnabled && pawn.IsCitizen();

        public static float ProtestLevel
        {
            get
            {
                int pop = Utility.Population;
                if (pop <= 0)
                    return ProtestLevelBase;
                return ProtestLevelBase + (DefaultLevel - ProtestLevelBase) * Utility.RimocracyComp.Protesters.Count / pop;
            }
        }

        public float StartProtestMTB => Prefs.DevMode? 3 : GenDate.HoursPerDay * 5 * (1 + CurLevel / ProtestLevel) * (1 + Utility.RimocracyComp.Governance);

        public Need_Loyalty(Pawn pawn) : base(pawn) => threshPercents = threshPercentsCommon;

        public static void RecalculateThreshPercents() => threshPercentsCommon[0] = ProtestLevel;

        static ProtestDef RandomProtestDefFor(Pawn pawn) =>
            DefDatabase<ProtestDef>.AllDefs.Where(def => def.AppliesTo(pawn)).RandomElementByWeightWithFallback(def => def.weight.GetValue(pawn));

        public void StartProtest()
        {
            if (pawn.InMentalState || QuestUtility.AnyQuestDisablesRandomMoodCausedMentalBreaksFor(pawn))
                return;
            Utility.Log($"Trying to start a protest for {pawn}.");

            foreach (ProtestDef p in DefDatabase<ProtestDef>.AllDefs.Where(def => def.AppliesTo(pawn)))
                Utility.Log($"Possible protest: {p.defName}, weight = {p.weight.GetValue(pawn):F1}");

            ProtestDef protest = RandomProtestDefFor(pawn);
            if (protest == null)
            {
                Utility.Log($"Could not find a suitable ProtestDef for {pawn} out of {DefDatabase<ProtestDef>.DefCount.ToStringCached()} defs.", LogLevel.Warning);
                return;
            }
            Utility.Log($"{pawn} initiates {protest}.");

            if (pawn.mindState.mentalStateHandler.TryStartMentalState(protest.mentalState, transitionSilently: true))
            {
                isProtesting = true;
                Utility.RimocracyComp.Protesters.Add(pawn);
                if (protest.mentalState.IsExtreme)
                    Find.LetterStack.ReceiveLetter($"Political Protest", protest.DescriptionFor(pawn), LetterDefOf.NegativeEvent, new LookTargets(pawn));
                else Messages.Message(protest.DescriptionFor(pawn), new LookTargets(pawn), MessageTypeDefOf.NegativeEvent);
                RecalculateThreshPercents();
            }
        }

        public void StopProtest()
        {
            Utility.Log($"{pawn} stops protesting.");
            pawn.mindState.mentalStateHandler.Reset();
            isProtesting = false;
            Utility.RimocracyComp.Protesters.Remove(pawn);
            RecalculateThreshPercents();
            Messages.Message($"{pawn} is no longer protesting.", pawn, MessageTypeDefOf.PositiveEvent);
        }

        public override void NeedInterval()
        {
            if (isProtesting && !pawn.InMentalState)
                StopProtest();
            if (!IsFrozen)
            {
                lastChange = ((pawn.needs.mood.CurLevelPercentage - 0.50f) * MoodChangeBase + pawn.GetOpinionOf(Utility.RimocracyComp.Leader) * OpinionOfLeaderChangeBase) * Settings.LoyaltyChangeSpeed;
                CurLevel += lastChange * TicksPerInterval / GenDate.TicksPerDay;

                if (!isProtesting && CurLevel < ProtestLevel)
                {
                    // Ready to protest
                    if ((Find.TickManager.TicksAbs / TicksPerInterval) % 20 == 0)
                        Utility.Log($"{pawn} starts protesting in ~{StartProtestMTB:N0} hours.");
                    if (Rand.MTBEventOccurs(StartProtestMTB, GenDate.TicksPerHour, TicksPerInterval))
                        StartProtest();
                }
                else if (isProtesting && CurLevel > ProtestLevel)
                    StopProtest();
            }
        }

        public override string GetTipString()
        {
            string tip = base.GetTipString();
            if (lastChange != 0)
                tip += $"\n\nChange: {lastChange.ToStringWithSign("0.#%")} per day";
            if (CurLevel < ProtestLevel)
                tip += $"\n\n<color=red>Expected to protest in {GenDate.ToStringTicksToPeriodVague((int)(StartProtestMTB * GenDate.TicksPerHour), false)}.</color>";
            else tip += $"\n\nIf loyalty falls below {ProtestLevel.ToStringPercent()}, {pawn.NameShortColored} may start protesting.";
            return tip;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isProtesting, "protesting");
        }
    }
}
