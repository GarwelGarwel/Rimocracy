using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

using static Rimocracy.Utility;

namespace Rimocracy
{
    public class Need_Loyalty : Need_Seeker
    {
        const int TicksPerInterval = 150;
        const int IntervalsBetweenPersistentUpdates = 100;

        public const float MoodWeight = 1;
        public const float OpinionOfLeaderWeight = 1;
        public const float MentalStateDebuff = 0.20f;
        public const float LoyaltyResetOnLeaderChange = 0.25f;
        public const float ToleratedDecisionLoyaltyFactor = 0.75f;
        public const float ProtestLevelBase = 0.15f;
        public const float DefaultLevel = 0.50f;
        public const int BaseProtestMTB = GenDate.HoursPerDay * 5;

        static List<float> threshPercentsCommon = new List<float>(2)
        {
            ProtestLevelBase,
            DefaultLevel
        };

        float persistentOffset;
        ProtestDef protest;

        public bool IsProtesting => protest != null;

        bool InGenuineMentalState => pawn.InMentalState && !IsProtesting;

        protected override bool IsFrozen => base.IsFrozen || !PoliticsEnabled || !pawn.IsCitizen();

        public override bool ShowOnNeedList => base.ShowOnNeedList && Settings.LoyaltyEnabled && PoliticsEnabled && pawn.IsCitizen();

        public static float ProtestLevel =>
            ProtestLevelBase + (DefaultLevel - ProtestLevelBase) * Utility.RimocracyComp.Protesters.Count / Math.Max(CitizensCount, 1);

        public override float CurInstantLevel =>
            Mathf.Clamp01(GenMath.WeightedAverage(pawn.needs.mood.CurLevelPercentage, MoodWeight, (pawn.GetOpinionOf(Utility.RimocracyComp.Leader) + 100) / 200, OpinionOfLeaderWeight)
                - (InGenuineMentalState ? MentalStateDebuff : 0)
                + persistentOffset);

        public float StartProtestMTB => (1 + CurLevel / ProtestLevel) * (1 + Utility.RimocracyComp.Governance) * BaseProtestMTB;

        public Need_Loyalty(Pawn pawn)
            : base(pawn) =>
            threshPercents = threshPercentsCommon;

        public static void RecalculateThreshPercents() => threshPercentsCommon[0] = ProtestLevel;

        public void RecalculatePersistentEffects()
        {
            Log($"Need_Loyalty.RecalculatePersistentEffects for {pawn}");
            persistentOffset = 0;
            for (int i = 0; i < Utility.RimocracyComp.Decisions.Count; i++)
            {
                DecisionDef decision = Utility.RimocracyComp.Decisions[i].def;
                if (decision.loyaltyEffect == 0)
                    continue;
                PawnDecisionOpinion opinion = new PawnDecisionOpinion(pawn, decision.considerations, Utility.RimocracyComp.Leader);
                Log($"{pawn}'s support for {decision.defName}: {opinion.support.ToStringWithSign()}. Loyalty effect: {decision.loyaltyEffect}.");
                switch (opinion.Vote)
                {
                    case DecisionVote.Yea:
                        persistentOffset += decision.loyaltyEffect;
                        break;

                    case DecisionVote.Nay:
                        persistentOffset -= decision.loyaltyEffect;
                        break;

                    case DecisionVote.Tolerate:
                        persistentOffset -= decision.loyaltyEffect * ToleratedDecisionLoyaltyFactor;
                        break;
                }
            }
            if (persistentOffset != 0)
                Log($"Persistent offset due to decisions: {persistentOffset.ToStringPercent()}");
        }

        static ProtestDef RandomProtestDefFor(Pawn pawn) =>
            DefDatabase<ProtestDef>.AllDefs.Where(def => def.AppliesTo(pawn)).RandomElementByWeightWithFallback(def => def.weight.GetValue(pawn));

        public void StartProtest()
        {
            if (pawn.InMentalState || QuestUtility.AnyQuestDisablesRandomMoodCausedMentalBreaksFor(pawn))
                return;
            Log($"Trying to start a protest for {pawn}.");

            if (Settings.DebugLogging)
                foreach (ProtestDef p in DefDatabase<ProtestDef>.AllDefs.Where(def => def.AppliesTo(pawn)))
                    Log($"Possible protest: {p.defName}, weight = {p.weight.GetValue(pawn):N1}");

            ProtestDef chosenProtest = RandomProtestDefFor(pawn);
            if (chosenProtest == null)
            {
                Log($"Could not find a suitable ProtestDef for {pawn} out of {DefDatabase<ProtestDef>.DefCount.ToStringCached()} defs.", LogLevel.Warning);
                return;
            }
            Log($"{pawn} initiates {chosenProtest}.");

            if (pawn.mindState.mentalStateHandler.TryStartMentalState(chosenProtest.mentalState, transitionSilently: true))
            {
                protest = chosenProtest;
                Utility.RimocracyComp.Protesters.Add(pawn);
                if (protest.mentalState.IsExtreme)
                    Find.LetterStack.ReceiveLetter($"Political Protest", protest.DescriptionFor(pawn), LetterDefOf.NegativeEvent, new LookTargets(pawn));
                else Messages.Message(protest.DescriptionFor(pawn), new LookTargets(pawn), MessageTypeDefOf.NegativeEvent);
                RecalculateThreshPercents();
            }
        }

        public void StopProtest()
        {
            Log($"{pawn} stops protesting.");
            pawn.mindState.mentalStateHandler.Reset();
            protest = null;
            Utility.RimocracyComp.Protesters.Remove(pawn);
            RecalculateThreshPercents();
            Messages.Message($"{pawn} is no longer protesting.", pawn, MessageTypeDefOf.PositiveEvent);
        }

        public override void NeedInterval()
        {
            if (!Settings.LoyaltyEnabled)
            {
                CurLevel = DefaultLevel;
                protest = null;
                return;
            }
            base.NeedInterval();
            if (IsProtesting && !pawn.InMentalState)
                StopProtest();
            if (!IsFrozen)
            {
                if (pawn.IsHashIntervalTick(TicksPerInterval * IntervalsBetweenPersistentUpdates))
                    RecalculatePersistentEffects();
                if (IsProtesting && CurLevel > ProtestLevel)
                    StopProtest();
                else if (!IsProtesting && CurLevel < ProtestLevel && Rand.MTBEventOccurs(StartProtestMTB, GenDate.TicksPerHour, TicksPerInterval))
                    StartProtest();
            }
        }

        public override string GetTipString()
        {
            StringBuilder tip = new StringBuilder(base.GetTipString());
            Pawn leader = Utility.RimocracyComp.Leader;
            float opinionOfLeader = pawn.GetOpinionOf(leader);
            tip.AppendInNewLine($"\nLoyalty of {pawn.NameShortColored} is affected by {pawn.Possessive()} mood ({pawn.needs.mood.CurLevelPercentage.ToStringPercent().ColorizeByValue(pawn.needs.mood.CurLevelPercentage, 0.5f, 0.5f)}){(leader != null ? $" and opinion of {LeaderTitle} {leader.NameShortColored} ({opinionOfLeader.ToStringWithSign("0").ColorizeByValue(opinionOfLeader)})." : ".")}");
            if (persistentOffset != 0)
                tip.Append($" Active decisions change loyalty by {persistentOffset.ToStringWithSign("0.#%").ColorizeByValue(persistentOffset)}.");
            if (InGenuineMentalState)
                tip.Append($" Mental state lowers loyalty by {MentalStateDebuff.ToStringPercent().Colorize(Color.red)}.");
            if (CurLevel > DefaultLevel)
            {
                int supportOffset = (int)Math.Floor(pawn.GetLoyaltySupportOffset());
                if (supportOffset > 0)
                    tip.AppendInNewLine($"\nLoyalty adds +{supportOffset.ToStringCached().Colorize(Color.green)} support for decisions.");
            }
            if (IsProtesting)
                tip.AppendInNewLine($"\n{pawn.NameShortColored} is protesting. {pawn.ProSubjCap()} will stop protesting when {pawn.Possessive()} loyalty goes over {ProtestLevel.ToStringPercent()}.".Colorize(Color.red));
            else if (CurLevel < ProtestLevel)
                tip.AppendInNewLine($"\nExpected to protest in {GenDate.ToStringTicksToPeriodVague((int)(StartProtestMTB * GenDate.TicksPerHour), false)}.".Colorize(Color.red));
            else if (CurInstantLevel < ProtestLevel)
                tip.AppendInNewLine($"\nIf {pawn.Possessive()} loyalty falls below {ProtestLevel.ToStringPercent()}, {pawn.NameShortColored} may start protesting.".Colorize(Color.yellow));
            return tip.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref persistentOffset, "persistentOffset");
            Scribe_Defs.Look(ref protest, "protest");
        }
    }
}
