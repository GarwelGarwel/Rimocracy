using UnityEngine;
using Verse;

namespace Rimocracy
{
    public abstract class Logic
    {
        protected string label;
        protected float value;
        protected bool inverted;

        public abstract string DefaultLabel { get; }

        public virtual string LabelInverted => $"the following is false: {label ?? DefaultLabel}";

        public string Label => inverted ? LabelInverted : (label ?? DefaultLabel);

        public string LabelCap => Label.CapitalizeFirst();

        public virtual string LabelAdjusted(Pawn pawn = null, Pawn target = null) => (IsSatisfied(pawn, target) ? "✔ ".Colorize(Color.green) : "✖ ".Colorize(Color.red)) + LabelCap;

        public virtual bool ValidFor(Pawn pawn = null, Pawn target = null) => true;

        public bool IsSatisfied(Pawn pawn = null, Pawn target = null) => IsSatisfiedInternal(pawn, target) ^ inverted;

        protected abstract bool IsSatisfiedInternal(Pawn pawn, Pawn target);

        public virtual float GetValue(Pawn pawn = null, Pawn target = null) => IsSatisfied(pawn, target) ? value : 0;

        public (float value, TaggedString explanation) GetValueAndExplanation(Pawn pawn, Pawn target)
        {
            float s = GetValue(pawn, target);
            return (s, s != 0 ? $"{Label.Formatted(pawn.Named("PAWN"), target.Named("TARGET"), Utility.LeaderTitle.Named("LEADERTITLE")).Resolve().CapitalizeFirst()}: {s.ToStringWithSign("0").ColorizeOpinion(s)}" : null);
        }
    }
}
