using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Requirement
    {
        static string indent = "";

        public static readonly Requirement always = new Requirement();
        public static readonly Requirement never = new Requirement() { inverted = true };

        bool inverted = false;

        List<Requirement> all = new List<Requirement>();
        List<Requirement> any = new List<Requirement>();

        SuccessionType succession = SuccessionType.Undefined;
        TermDuration termDuration = TermDuration.Undefined;
        bool leaderExists;
        bool notCampaigning;
        float minGovernance;
        float minRegime = -1;
        float maxRegime = 1;
        string decision;

        /// <summary>
        /// Returns true if this requirement is default
        /// </summary>
        public bool IsTrivial =>
            !inverted
            && all.NullOrEmpty()
            && any.NullOrEmpty()
            && succession == SuccessionType.Undefined
            && termDuration == TermDuration.Undefined
            && !leaderExists
            && !notCampaigning
            && minGovernance == 0
            && minRegime == -1
            && maxRegime == 1
            && decision == null;

        public static implicit operator bool(Requirement requirement) => requirement.GetValue();

        public bool GetValue()
        {
            bool res = true;
            if (!all.NullOrEmpty())
                res &= all.All(r => r);
            if (res && !any.NullOrEmpty())
                res &= any.Any(r => r);
            if (res && succession != SuccessionType.Undefined)
                res &= Utility.RimocracyComp.SuccessionType == succession;
            if (res && termDuration != TermDuration.Undefined)
                res &= Utility.RimocracyComp.TermDuration == termDuration;
            if (res && leaderExists)
                res &= Utility.RimocracyComp.Leader != null;
            if (res && notCampaigning)
                res &= Utility.RimocracyComp.Campaigns.NullOrEmpty();
            if (res && minGovernance > 0)
                res &= Utility.RimocracyComp.Governance >= minGovernance;
            if (res && minRegime > -1)
                res &= Utility.RimocracyComp.RegimeFinal >= minRegime;
            if (res && maxRegime < 1)
                res &= Utility.RimocracyComp.RegimeFinal <= maxRegime;
            if (res && !decision.NullOrEmpty())
                res &= Utility.RimocracyComp.DecisionActive(decision);
            return res ^ inverted;
        }

        public override string ToString()
        {
            string res = "";
            if (inverted)
            {
                res = $"{indent}The following must be FALSE:\n";
                indent += "\t";
            }
            if (succession != SuccessionType.Undefined)
                res += $"{indent}Succession law: {succession}\n";
            if (termDuration != TermDuration.Undefined)
                res += $"{indent}Term duration: {termDuration}\n";
            if (notCampaigning)
                res += $"{indent}Not campaigning\n";
            if (minGovernance > 0)
                res += $"{indent}Governance is at least {minGovernance * 100}%\n";
            if (minRegime > -1)
                if (minRegime > 0)
                    res += $"{indent}Regime is at least {minRegime * 100}% democratic\n";
                else res += $"{indent}Regime is at most {-minRegime * 100}% authoritarian\n";
            if (maxRegime < 1)
                if (maxRegime > 0)
                    res += $"{indent}Regime is at most {maxRegime * 100}% democratic\n";
                else res += $"{indent}Regime is at least {-maxRegime * 100}% authoritarian\n";
            if (!decision.NullOrEmpty())
                res += $"{indent}{decision} is active";
            if (!all.NullOrEmpty())
            {
                res += $"{indent}All of the following:\n";
                indent += "\t";
                foreach (Requirement r in all)
                    res += $"{r}\n";
                indent = indent.Remove(0, 1);
            }
            if (!any.NullOrEmpty())
            {
                res += $"{indent}Any of the following:\n";
                indent += "\t";
                foreach (Requirement r in any)
                    res += $"{r}\n";
                indent = indent.Remove(0, 1);
            }
            if (inverted)
                indent = indent.Remove(0, 1);
            return res.TrimEndNewlines();
        }
    }
}
