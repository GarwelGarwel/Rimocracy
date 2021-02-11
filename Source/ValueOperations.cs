using System.Collections.Generic;
using Verse;

namespace Rimocracy
{
    public class ValueOperations
    {
        public float lessThan = float.MaxValue;
        public float lessOrEqual = float.MaxValue;
        public float greaterThan = float.MinValue;
        public float greaterOrEqual = float.MinValue;
        public float equals = float.NaN;

        // Used for changing support value based on input value
        SimpleCurve factor;
        SimpleCurve offset;

        public ValueOperations()
        { }

        public bool Compare(float value) =>
            value < lessThan
            && value <= lessOrEqual
            && value > greaterThan
            && value >= greaterOrEqual
            && (float.IsNaN(equals) || value == equals);

        float GetFactor(float value) => factor.EnumerableNullOrEmpty() ? 1 : factor.Evaluate(value);

        float GetOffset(float value) => offset.EnumerableNullOrEmpty() ? 0 : offset.Evaluate(value);

        public void TransformValue(float parameter, ref float valueToTransform)
        {
            if (Compare(parameter))
            {
                valueToTransform *= GetFactor(parameter);
                valueToTransform += GetOffset(parameter);
            }
        }

        public string ToString(string valueLabel = "Value", string format = null)
        {
            List<string> res = new List<string>();
            if (lessThan != float.MaxValue)
                res.Add($"{valueLabel} < {lessThan.ToString(format)}");
            if (lessOrEqual != float.MaxValue)
                res.Add($"{valueLabel} <= {lessOrEqual.ToString(format)}");
            if (greaterThan != float.MinValue)
                res.Add($"{valueLabel} > {greaterThan.ToString(format)}");
            if (greaterOrEqual != float.MinValue)
                res.Add($"{valueLabel} >= {greaterOrEqual.ToString(format)}");
            if (!float.IsNaN(equals))
                res.Add($"{valueLabel} = {equals.ToString(format)}");
            return res.ToLineList();
        }
    }
}
