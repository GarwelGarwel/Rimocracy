using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Requirement
    {
        public static readonly Requirement always = new Requirement();

        public static readonly Requirement never = new Requirement() { inverted = true };

        static string indent = "";

        const string indentSymbol = "  ";

        protected bool inverted = false;

        List<Requirement> all = new List<Requirement>();
        List<Requirement> any = new List<Requirement>();

        protected SuccessionDef succession;
        protected TermDuration termDuration = TermDuration.Undefined;
        protected bool? leaderExists;
        protected bool? campaigning;
        protected ValueOperations governance;
        protected ValueOperations regime;
        protected string decision;

        /// <summary>
        /// Returns true if this requirement is default
        /// </summary>
        public virtual bool IsTrivial =>
            !inverted
            && all.NullOrEmpty()
            && any.NullOrEmpty()
            && succession == null
            && termDuration == TermDuration.Undefined
            && leaderExists == null
            && campaigning == null
            && governance == null
            && regime == null
            && decision == null;

        public static implicit operator bool(Requirement requirement) => requirement.IsSatisfied();

        protected virtual bool IsSatisfied_Internal(Pawn pawn = null, Pawn target = null)
        {
            bool res = true;
            if (!all.NullOrEmpty())
                res &= all.All(r => r.IsSatisfied(pawn));
            if (res && !any.NullOrEmpty())
                res &= any.Any(r => r.IsSatisfied(pawn));
            if (res && succession != null)
                res &= Utility.RimocracyComp.SuccessionType.defName == succession.defName;
            if (res && termDuration != TermDuration.Undefined)
                res &= Utility.RimocracyComp.TermDuration == termDuration;
            if (res && leaderExists != null)
                res &= (Utility.RimocracyComp.Leader != null) == leaderExists;
            if (res && campaigning != null)
                res &= !Utility.RimocracyComp.Campaigns.NullOrEmpty() == campaigning;
            if (res && governance != null)
                res &= governance.Compare(Utility.RimocracyComp.Governance);
            if (regime != null)
                res &= regime.Compare(Utility.RimocracyComp.RegimeFinal);
            if (res && !decision.NullOrEmpty())
                res &= Utility.RimocracyComp.DecisionActive(decision);
            return res;
        }

        public bool IsSatisfied(Pawn pawn = null, Pawn target = null) => IsSatisfied_Internal(pawn, target) ^ inverted;

        public override string ToString()
        {
            string res = "";
            if (inverted)
            {
                res = $"{indent}The following must be FALSE:\n";
                indent += indentSymbol;
            }
            if (succession != null)
                res += $"{indent}Succession law: {succession.label}\n";
            if (leaderExists != null)
                res += $"{indent}Leader {((bool)leaderExists ? "exists" : "doesn't exist")}";
            if (termDuration != TermDuration.Undefined)
                res += $"{indent}Term duration: {termDuration}\n";
            if (campaigning != null)
                res += $"{indent}Campaign is {((bool)campaigning ? "on" : "off")}\n";
            if (governance != null)
                res += $"{indent}{governance.ToString("Governance", "P0")}\n";
            if (regime != null)
                res += $"{indent}{regime.ToString("Regime (democracy)", "P0")}\n";
            if (!decision.NullOrEmpty())
                res += $"{indent}{decision} is active";
            if (!all.NullOrEmpty())
            {
                res += $"{indent}All of the following:\n";
                indent += indentSymbol;
                foreach (Requirement r in all)
                    res += $"{r}\n";
                indent = indent.Remove(0, indentSymbol.Length);
            }
            if (!any.NullOrEmpty())
            {
                res += $"{indent}Any of the following:\n";
                indent += indentSymbol;
                foreach (Requirement r in any)
                    res += $"{r}\n";
                indent = indent.Remove(0, indentSymbol.Length);
            }
            if (inverted)
                indent = indent.Remove(0, indentSymbol.Length);
            return res.TrimEndNewlines();
        }
    }
}
