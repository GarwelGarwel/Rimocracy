using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Consideration
    {
        public static readonly Consideration always = new Consideration();

        public static readonly Consideration never = new Consideration() { inverted = true };

        static string indent = "";

        const string indentSymbol = "  ";

        public string label;
        public float support;

        bool inverted = false;

        List<Consideration> all = new List<Consideration>();
        List<Consideration> any = new List<Consideration>();

        SuccessionDef succession;
        bool? leaderExists;
        TermDuration termDuration = TermDuration.Undefined;
        bool? campaigning;
        ValueOperations governance;
        ValueOperations regime;
        ValueOperations population;
        ValueOperations daysOfFood;
        string decision;

        bool? isLeader;
        bool? isTarget;
        TraitDef trait;
        List<SkillOperations> skills = new List<SkillOperations>();
        ValueOperations medianOpinionOfMe;
        ValueOperations age;
        ValueOperations titleSeniority;

        bool? targetIsColonist;
        bool? targetIsLeader;
        bool? targetInAggroMentalState;
        TraitDef targetTrait;
        ValueOperations opinionOfTarget;
        ValueOperations medianOpinionOfTarget;
        ValueOperations targetAge;

        /// <summary>
        /// Returns true if this requirement is default
        /// </summary>
        public bool IsTrivial =>
            !inverted
            && all.NullOrEmpty()
            && any.NullOrEmpty()
            && succession == null
            && leaderExists == null
            && termDuration == TermDuration.Undefined
            && campaigning == null
            && governance == null
            && regime == null
            && population == null
            && daysOfFood == null
            && decision == null
            && isLeader == null
            && isTarget == null
            && trait == null
            && skills.NullOrEmpty()
            && medianOpinionOfMe == null
            && age == null
            && titleSeniority == null
            && targetIsColonist == null
            && targetIsLeader == null
            && targetInAggroMentalState == null
            && targetTrait == null
            && medianOpinionOfTarget == null
            && medianOpinionOfTarget == null
            && targetAge == null;

        public static implicit operator bool(Consideration consideration) => consideration.IsSatisfied();

        public bool IsSatisfied(Pawn pawn = null, Pawn target = null)
        {
            bool res = true;
            if (succession != null)
                res &= Utility.RimocracyComp.SuccessionType.defName == succession.defName;
            if (res && termDuration != TermDuration.Undefined)
                res &= Utility.RimocracyComp.TermDuration == termDuration;
            if (res && leaderExists != null)
                res &= (Utility.RimocracyComp.Leader != null) == leaderExists;
            if (res && campaigning != null)
                res &= !Utility.RimocracyComp.Campaigns.NullOrEmpty() == campaigning;
            if (res && governance != null)
                res &= governance.Compare(Utility.RimocracyComp.Governance);
            if (res && regime != null)
                res &= regime.Compare(Utility.RimocracyComp.RegimeFinal);
            if (res && population != null)
                res &= population.Compare(Utility.Population);
            if (res && daysOfFood != null)
                res &= daysOfFood.Compare(Utility.DaysOfFood);
            if (res && !decision.NullOrEmpty())
                res &= Utility.RimocracyComp.DecisionActive(decision);

            if (res && pawn != null)
            {
                if (isLeader != null)
                    res &= pawn.IsLeader() == isLeader;
                if (res && isTarget != null)
                    res &= (pawn == target) == isTarget;
                if (res && trait != null && pawn?.story?.traits != null)
                    res &= pawn.story.traits.HasTrait(trait);
                if (res && !skills.NullOrEmpty())
                    res &= skills.TrueForAll(so => so.Compare(pawn));
                if (res && medianOpinionOfMe != null)
                    res &= medianOpinionOfMe.Compare(pawn.MedianCitizensOpinion());
                if (res && age != null && pawn?.ageTracker != null)
                    res &= age.Compare(pawn.ageTracker.AgeBiologicalYears);
                if (res && titleSeniority != null && pawn?.royalty != null)
                    res &= titleSeniority.Compare(pawn.GetTitleSeniority());
            }

            if (res && target != null)
            {
                if (targetIsColonist != null)
                    res &= target.IsColonist == targetIsColonist;
                if (res && targetIsLeader != null)
                    res &= target.IsLeader() == targetIsLeader;
                if (res && targetInAggroMentalState != null)
                    res &= target.InAggroMentalState == targetInAggroMentalState;
                if (res && targetTrait != null && target.story?.traits != null)
                    res &= target.story.traits.HasTrait(targetTrait);
                if (res && opinionOfTarget != null && pawn != null)
                    res &= opinionOfTarget.Compare(pawn.GetOpinionOf(target));
                if (res && medianOpinionOfTarget != null)
                    res &= medianOpinionOfTarget.Compare(target.MedianCitizensOpinion());
                if (res && targetAge != null && target.ageTracker != null)
                    res &= targetAge.Compare(target.ageTracker.AgeBiologicalYears);
            }

            if (res && !all.NullOrEmpty())
                res &= all.All(r => r.IsSatisfied(pawn, target));
            if (res && !any.NullOrEmpty())
                res &= any.Any(r => r.IsSatisfied(pawn, target));
            return res ^ inverted; ;
        }

        public (float support, TaggedString explanation) GetSupportAndExplanation(Pawn pawn, Pawn target)
        {
            float s = GetSupport(pawn, target);
            return (s, s != 0 ? $"{label.Formatted(pawn.Named("PAWN"), target.Named("TARGET")).Resolve().CapitalizeFirst()}: {s.ToStringWithSign("0")}" : null);
        }

        public float GetSupport(Pawn pawn, Pawn target = null)
        {
            if (!IsSatisfied(pawn, target))
                return 0;
            float s = support;
            if (governance != null)
                governance.TransformValue(Utility.RimocracyComp.Governance, ref s);
            if (regime != null)
                regime.TransformValue(Utility.RimocracyComp.RegimeFinal, ref s);
            if (population != null)
                population.TransformValue(Utility.Population, ref s);
            if (daysOfFood != null)
                daysOfFood.TransformValue(Utility.DaysOfFood, ref s);
            foreach (SkillOperations so in skills)
                so.TransformValue(pawn, ref s);
            if (medianOpinionOfMe != null)
                medianOpinionOfMe.TransformValue(pawn.MedianCitizensOpinion(), ref s);
            if (age != null && pawn?.ageTracker != null)
                age.TransformValue(pawn.ageTracker.AgeBiologicalYears, ref s);
            if (titleSeniority != null && pawn?.royalty != null)
                titleSeniority.TransformValue(pawn.GetTitleSeniority(), ref s);
            if (target != null)
            {
                if (opinionOfTarget != null)
                    opinionOfTarget.TransformValue(pawn.GetOpinionOf(target), ref s);
                if (medianOpinionOfTarget != null && target != null)
                    medianOpinionOfTarget.TransformValue(target.MedianCitizensOpinion(), ref s);
                if (targetAge != null && target.ageTracker != null)
                    targetAge.TransformValue(target.ageTracker.AgeBiologicalYears, ref s);
            }
            return s;
        }

        public string ToString(string pawn = null, string target = null)
        {
            if (pawn.NullOrEmpty())
                pawn = "the pawn";
            if (target.NullOrEmpty())
                target = "the target";

            string res = "";
            if (inverted)
            {
                res = $"{indent}The following must be FALSE:\n";
                indent += indentSymbol;
            }

            if (succession != null)
                res += $"{indent}Succession law: {succession.LabelCap}\n";
            if (leaderExists != null)
                res += $"{indent}Leader {((bool)leaderExists ? "exists" : "doesn't exist")}\n";
            if (termDuration != TermDuration.Undefined)
                res += $"{indent}Term duration: {termDuration}\n";
            if (campaigning != null)
                res += $"{indent}Campaign is {((bool)campaigning ? "on" : "off")}\n";
            if (governance != null)
                res += $"{indent}{governance.ToString("Governance", "P0")}\n";
            if (regime != null)
                res += $"{indent}{regime.ToString("Regime (democracy)", "P0")}\n";
            if (population != null)
                res += $"{indent}{population.ToString("Population")}\n";
            if (daysOfFood != null)
                res += $"{indent}{daysOfFood.ToString("Days worth of food")}\n";
            if (!decision.NullOrEmpty())
                res += $"{indent}{GenText.SplitCamelCase(decision)} is active\n";

            if (isLeader != null)
                res += $"{indent}{pawn.CapitalizeFirst()} is {((bool)isLeader ? $"" :"not ")}the leader\n";
            if (isTarget != null)
                res += $"{indent}{pawn.CapitalizeFirst()} is {((bool)isTarget ? $"" : $"not ")}the target\n";
            if (trait != null)
                res += $"{indent}{pawn.CapitalizeFirst()} has trait {trait}\n";
            if (!skills.EnumerableNullOrEmpty())
                foreach (SkillOperations skill in skills)
                    res += $"{indent}{skill.ToString(skill.skill.LabelCap)}\n";
            if (medianOpinionOfMe != null)
                res += $"{indent}{medianOpinionOfMe.ToString($"Median citizens' opinion of {pawn}")}\n";
            if (age != null)
                res += $"{indent}{age.ToString($"{pawn.CapitalizeFirst()}'s age")}\n";
            if (titleSeniority != null)
                res += $"{indent}{titleSeniority.ToString($"{pawn.CapitalizeFirst()}'s title seniority")}\n";

            if (targetIsColonist != null)
                res += $"{indent}{target.CapitalizeFirst()} is {((bool)targetIsColonist ? "" : "not ")}a colonist\n";
            if (targetIsLeader != null)
                res += $"{indent}{target.CapitalizeFirst()} is {((bool)targetIsLeader ? "" : "not ")}the leader\n";
            if (targetInAggroMentalState != null)
                res += $"{indent}{target.CapitalizeFirst()} is {((bool)targetInAggroMentalState ? "" : "not ")}in an aggressive mental break\n";
            if (targetTrait != null)
                res += $"{indent}{target.CapitalizeFirst()} has trait {targetTrait}\n";
            if (opinionOfTarget != null)
                res += $"{indent}{opinionOfTarget.ToString($"{pawn.CapitalizeFirst()}'s opinion of {target}")}\n";
            if (medianOpinionOfTarget != null)
                res += $"{indent}{medianOpinionOfTarget.ToString($"Median citizens' opinion of {target}")}\n";
            if (targetAge != null)
                res += $"{indent}{targetAge.ToString($"{target.CapitalizeFirst()}'s age")}\n";

            if (!all.NullOrEmpty())
            {
                res += $"{indent}All of the following:\n";
                indent += indentSymbol;
                foreach (Consideration r in all)
                    res += $"{r.ToString(pawn, target)}\n";
                indent = indent.Remove(0, indentSymbol.Length);
            }
            if (!any.NullOrEmpty())
            {
                res += $"{indent}Any of the following:\n";
                indent += indentSymbol;
                foreach (Consideration r in any)
                    res += $"{r.ToString(pawn, target)}\n";
                indent = indent.Remove(0, indentSymbol.Length);
            }
            if (inverted)
                indent = indent.Remove(0, indentSymbol.Length);
            return res.TrimEndNewlines();
        }
    }
}
