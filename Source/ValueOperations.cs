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
        public SimpleCurve factor;

        public SimpleCurve offset;

        public ValueOperations()
        { }

        public bool Compare(float value) =>
            value < lessThan
            && value <= lessOrEqual
            && value > greaterThan
            && value >= greaterOrEqual
            && (float.IsNaN(equals) || value == equals);

        public float GetFactor(float value) => factor.EnumerableNullOrEmpty() ? 1 : factor.Evaluate(value);

        public float GetOffset(float value) => offset.EnumerableNullOrEmpty() ? 0 : offset.Evaluate(value);

        public void TransformValue(float parameter, ref float valueToTransform)
        {
            if (Compare(parameter))
            {
                valueToTransform *= GetFactor(parameter);
                valueToTransform += GetOffset(parameter);
            }
        }
    }
}
