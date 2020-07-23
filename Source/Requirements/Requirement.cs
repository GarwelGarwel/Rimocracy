using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Requirement
    {
        static string indent = "";

        bool inverted = false;

        List<Requirement> all = new List<Requirement>();
        List<Requirement> any = new List<Requirement>();

        SuccessionType succession = SuccessionType.Undefined;
        TermDuration termDuration = TermDuration.Undefined;
        bool notCampaigning;

        public bool GetValue()
        {
            bool res = true;
            Utility.Log("\n" + indent + "Calculating value for this requirement:\n" + this);
            if (!all.NullOrEmpty())
            {
                indent += "\t";
                res &= all.All(r => r);
                indent = indent.Remove(0, 1);
            }
            if (res && !any.NullOrEmpty())
            {
                indent += "\t";
                res &= any.Any(r => r);
                indent = indent.Remove(0, 1);
            }
            if (res && succession != SuccessionType.Undefined)
                res &= Utility.RimocracyComp.SuccessionType == succession;
            if (res && termDuration != TermDuration.Undefined)
                res &= Utility.RimocracyComp.TermDuration == termDuration;
            if (res && notCampaigning)
                res &= Utility.RimocracyComp.Campaigns.NullOrEmpty();
            Utility.Log("\n" + indent + "The condition is " + res + " (inverted: " + inverted + ")");
            return res ^ inverted;
        }

        public static implicit operator bool(Requirement requirement) => requirement.GetValue();

        public override string ToString()
        {
            string res = indent;
            if (inverted)
                res += "The following must be FALSE:\n";
            if (succession != SuccessionType.Undefined)
                res += indent + "Succession law: " + succession + "\n";
            if (termDuration != TermDuration.Undefined)
                res += indent + "Term duration: " + termDuration + "\n";
            if (notCampaigning)
                res += indent + "Not campaigning\n";
            if (!all.NullOrEmpty())
            {
                res += indent + "All of the following:\n";
                indent += "\t";
                foreach (Requirement r in all)
                    res += r + "\n";
                indent = indent.Remove(0, 1);
            }
            if (!any.NullOrEmpty())
            {
                res += indent + "Any of the following:\n";
                indent += "\t";
                foreach (Requirement r in any)
                    res += r + "\n";
                indent = indent.Remove(0, 1);
            }
            return res;
        }
    }
}
