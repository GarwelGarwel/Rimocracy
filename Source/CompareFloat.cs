using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rimocracy
{
    public class CompareFloat
    {
        public float lessThan = float.MaxValue;
        public float greaterThan = float.MinValue;
        public float equals = float.NaN;

        public bool Compare(float value) => (value < lessThan) && (value > greaterThan) && (float.IsNaN(equals) || value == equals);

        public CompareFloat()
        { }
    }
}
