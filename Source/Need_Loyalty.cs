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

        public const float MoodChangeBase = 0.1f * TicksPerInterval / GenDate.TicksPerDay;
        public const float OpinionOfLeaderChangeBase = 0.0005f * TicksPerInterval / GenDate.TicksPerDay;
        public const float LoyaltyResetOnLeaderChange = 0.25f;
        public const float CriticalLevel = 0.10f;
        public const float DisloyalLevel = 0.30f;
        public const float DefaultLevel = 0.50f;

        float lastChange;
        bool isProtesting;

        public override int GUIChangeArrow => IsFrozen ? 0 : Math.Sign(lastChange);

        protected override bool IsFrozen => base.IsFrozen || !Utility.PoliticsEnabled || !pawn.IsCitizen();

        public override bool ShowOnNeedList => base.ShowOnNeedList && Utility.PoliticsEnabled && pawn.IsCitizen();

        public float StartProtestLevel => Prefs.DevMode ? 0.40f : CriticalLevel;

        public float JoinProtestLevel => Prefs.DevMode ? DefaultLevel : DisloyalLevel;

        public float StartProtestMTB => Prefs.DevMode? 3 : GenDate.HoursPerDay * 5 * (1 + CurLevel / StartProtestLevel) * (1 + Utility.RimocracyComp.Governance);

        public Need_Loyalty(Pawn pawn)
            : base(pawn) =>
            threshPercents = new List<float>()
            {
                StartProtestLevel,
                DisloyalLevel,
                DefaultLevel
            };

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
                List<Pawn> protesters = new List<Pawn>() { pawn };
                TaggedString message = protest.DescriptionFor(pawn);

                Need_Loyalty need = null;
                foreach (Pawn pawn2 in Utility.Citizens.Where(pawn2 =>
                    pawn.MapHeld == pawn2.MapHeld
                    && (need = pawn2.needs.AllNeeds.OfType<Need_Loyalty>().FirstOrDefault()) != null
                    && need.CurLevel < need.JoinProtestLevel))
                {
                    Utility.Log($"{pawn2} joins the protest with {protest}.");
                    protest = RandomProtestDefFor(pawn2);
                    if (pawn2.mindState.mentalStateHandler.TryStartMentalState(protest.mentalState, transitionSilently: true))
                    {
                        need.isProtesting = true;
                        protesters.Add(pawn2);
                        message += $"\n{protest.DescriptionFor(pawn2)}";
                    }
                }

                message = $"{protesters.Count.ToStringCached()} citizen{(protesters.Count > 1 ? "s are" : " is")} participating in a protest started by {pawn.NameShortColored}.\n{message}";
                Find.LetterStack.ReceiveLetter($"Political Protest", message, LetterDefOf.NegativeEvent, new LookTargets(protesters));
            }
        }

        public void StopProtest()
        {
            Utility.Log($"{pawn} stops protesting.");
            pawn.mindState.mentalStateHandler.Reset();
            isProtesting = false;
            Messages.Message($"{pawn} is no longer protesting.", pawn, MessageTypeDefOf.PositiveEvent);
        }

        public override void NeedInterval()
        {
            if (isProtesting && !pawn.InMentalState)
                isProtesting = false;
            if (!IsFrozen)
            {
                lastChange = (pawn.needs.mood.CurLevelPercentage - 0.50f) * MoodChangeBase + pawn.GetOpinionOf(Utility.RimocracyComp.Leader) * OpinionOfLeaderChangeBase;
                CurLevel += lastChange * Settings.LoyaltyChangeSpeed;

                if (!isProtesting && CurLevel < StartProtestLevel)
                {
                    // Ready to protest
                    if ((Find.TickManager.TicksAbs / TicksPerInterval) % 20 == 0)
                        Utility.Log($"{pawn} starts protesting in ~{StartProtestMTB:N0} hours.");
                    if (Rand.MTBEventOccurs(StartProtestMTB, GenDate.TicksPerHour, TicksPerInterval))
                        StartProtest();
                }
                else if (isProtesting && CurLevel > JoinProtestLevel)
                    StopProtest();
            }
        }

        public override string GetTipString() =>
            CurLevel >= StartProtestLevel
            ? base.GetTipString()
            : $"{base.GetTipString()}\n<color=red>Will protest in {GenDate.ToStringTicksToPeriodVague((int)(StartProtestMTB * GenDate.TicksPerHour))}.</color>";
    }
}
