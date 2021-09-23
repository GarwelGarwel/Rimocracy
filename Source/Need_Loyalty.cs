using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Rimocracy
{
    public class Need_Loyalty : Need
    {
        public const float MoodChangeBase = 150 * 0.1f / 60000;
        public const float LeaderChangeLoyaltyResetBase = 0.25f;

        float lastChange;

        public override int GUIChangeArrow => IsFrozen ? 0 : Math.Sign(lastChange);

        protected override bool IsFrozen => base.IsFrozen || !Utility.PoliticsEnabled || !pawn.IsCitizen();

        public override bool ShowOnNeedList => base.ShowOnNeedList && Utility.PoliticsEnabled && pawn.IsCitizen();

        public Need_Loyalty(Pawn pawn)
            : base(pawn) =>
            threshPercents = new List<float>() { 0.5f };

        public override void NeedInterval()
        {
            if (!IsFrozen)
            {
                lastChange = (pawn.needs.mood.CurLevelPercentage - 0.5f) * MoodChangeBase * Settings.LoyaltyChangeSpeed;
                CurLevel += lastChange;
            }
        }
    }
}
