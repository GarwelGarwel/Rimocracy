using Verse;

namespace Rimocracy
{
    public class StatModifierString : IExposable
    {
        public string name;
        public float factor;
        public float offset;

        public override string ToString()
        {
            string res = "";
            if (factor != 1)
                res = $"{name}: x{factor.ToStringPercent()}";
            if (offset != 0)
                res += $"\n{name} {offset.ToStringSign()}";
            if (res == "")
                return null;
            return res.Trim();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref factor, "factor", 1);
            Scribe_Values.Look(ref offset, "offset");
        }
    }
}
