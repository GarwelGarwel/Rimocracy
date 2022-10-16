using System.Collections.Generic;
using Verse;

namespace Rimocracy
{
    public abstract class Logic_Value : Logic
    {
        public float lessThan = float.MaxValue;
        public float lessOrEqual = float.MaxValue;
        public float greaterThan = float.MinValue;
        public float greaterOrEqual = float.MinValue;
        public float equals = float.NaN;

        // Used for changing support value based on input value
        SimpleCurve factor;
        SimpleCurve offset;

        public abstract float UnderlyingValue(Pawn pawn, Pawn target);

        protected virtual string NumberFormat => null;

        protected override bool IsSatisfiedInternal(Pawn pawn = null, Pawn target = null)
        {
            float v = UnderlyingValue(pawn, target);
            return v < lessThan
                && v <= lessOrEqual
                && v > greaterThan
                && v >= greaterOrEqual
                && (float.IsNaN(equals) || v == equals);
        }

        float GetFactor(float value) => factor.EnumerableNullOrEmpty() ? 1 : factor.Evaluate(value);

        float GetOffset(float value) => offset.EnumerableNullOrEmpty() ? 0 : offset.Evaluate(value);

        public override float GetValue(Pawn pawn = null, Pawn target = null)
        {
            if (!IsSatisfied(pawn, target))
                return 0;
            float v = UnderlyingValue(pawn, target), res = value;
            res *= GetFactor(v);
            res += GetOffset(v);
            return res;
        }

        public void TransformValue(ref float valueToTransform, Pawn pawn = null, Pawn target = null) => valueToTransform += GetValue(pawn, target);

        public override string LabelAdjusted(bool checkmark, Pawn pawn = null, Pawn target = null)
        {
            if (!ValidFor(pawn, target))
                return "";
            float v = UnderlyingValue(pawn, target);
            string format = NumberFormat;
            List<string> res = new List<string>();
            if (lessThan != float.MaxValue)
                res.Add($"{LabelCap} is less than {lessThan.ToString(format)} ({v.ToString(format)})");
            if (lessOrEqual != float.MaxValue)
                res.Add($"{LabelCap} is at most {lessOrEqual.ToString(format)} ({v.ToString(format)})");
            if (greaterThan != float.MinValue)
                res.Add($"{LabelCap} is greater than {greaterThan.ToString(format)} ({v.ToString(format)})");
            if (greaterOrEqual != float.MinValue)
                res.Add($"{LabelCap} is at least {greaterOrEqual.ToString(format)} ({v.ToString(format)})");
            if (!float.IsNaN(equals))
                res.Add($"{LabelCap} is {equals.ToString(format)} ({v.ToString(format)})");
            return res.ToLineList();
        }
    }
}
