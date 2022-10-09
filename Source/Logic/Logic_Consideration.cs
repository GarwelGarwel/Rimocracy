using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Logic_Consideration : Logic
    {
        public static readonly Logic_Consideration always = new Logic_Consideration();

        public static readonly Logic_Consideration never = new Logic_Consideration() { inverted = true };

        internal static string indent = "";

        internal const string indentSymbol = "  ";

        Logic_All all;
        Logic_Any any;

        List<Logic_Consideration> factors = new List<Logic_Consideration>();
        List<Logic_Consideration> offsets = new List<Logic_Consideration>();

        Logic_Succession succession;
        Logic_LeaderExists leaderExists;
        Logic_TermDuration termDuration;
        Logic_IsCampaigning campaigning;
        Logic_Governance governance;
        Logic_Population population;
        Logic_DaysOfFood daysOfFood;
        Logic_TechLevel techLevel;
        Logic_ModActive modActive;
        Logic_DecisionActive decision;

        Logic_IsLeader isLeader;
        Logic_IsTarget isTarget;
        Logic_HasTrait trait;
        List<Logic_Skill> skills = new List<Logic_Skill>();
        Logic_IsCapableOfViolence isCapableOfViolence;
        Logic_MedianOpinion medianOpinionOfMe;
        Logic_Age age;
        Logic_Backstory backstory;
        Logic_TitleSeniority titleSeniority;
        Logic_HasPrimaryIdeo primaryIdeoligion;
        Logic_Meme meme;
        Logic_Precept precept;
        Logic_IdeoCertainty ideoCertainty;

        Logic_IsColonist targetIsColonist;
        Logic_IsLeader targetIsLeader;
        Logic_InAggroMentalState targetInAggroMentalState;
        Logic_IsHostile targetIsHostile;
        Logic_IsGuilty targetIsGuilty;
        Logic_IsWild targetIsWild;
        Logic_HasTrait targetTrait;
        Logic_Opinion opinionOfTarget;
        Logic_MedianOpinion medianOpinionOfTarget;
        Logic_Age targetAge;
        Logic_FactionGoodwill targetFactionGoodwill;

        /// <summary>
        /// Returns true if this requirement is default
        /// </summary>
        public bool IsTrivial =>
            !inverted
            && all == null
            && any == null
            && factors.NullOrEmpty()
            && offsets.NullOrEmpty()
            && succession == null
            && leaderExists == null
            && termDuration == null
            && campaigning == null
            && governance == null
            && population == null
            && daysOfFood == null
            && techLevel == null
            && modActive == null
            && decision == null
            && isLeader == null
            && isTarget == null
            && trait == null
            && skills.NullOrEmpty()
            && isCapableOfViolence == null
            && medianOpinionOfMe == null
            && age == null
            && backstory == null
            && titleSeniority == null
            && targetFactionGoodwill == null
            && primaryIdeoligion == null
            && meme == null
            && precept == null
            && ideoCertainty == null
            && targetIsColonist == null
            && targetIsLeader == null
            && targetInAggroMentalState == null
            && targetIsHostile == null
            && targetIsGuilty == null
            && targetIsWild == null
            && targetTrait == null
            && opinionOfTarget == null
            && medianOpinionOfTarget == null
            && targetAge == null;

        public override string DefaultLabel => "";

        public Logic_Consideration()
        { }

        public Logic_Consideration(float value) => this.value = value;

        public static implicit operator bool(Logic_Consideration consideration) => consideration.IsSatisfied(target: Utility.RimocracyComp.Leader);

        protected override bool IsSatisfiedInternal(Pawn pawn = null, Pawn target = null)
        {
            bool res = true;

            if (succession != null)
                res &= succession.IsSatisfied();
            if (res && leaderExists != null)
                res &= leaderExists.IsSatisfied();
            if (res && termDuration != null)
                res &= termDuration.IsSatisfied();
            if (res && campaigning != null)
                res &= campaigning.IsSatisfied();
            if (res && governance != null)
                res &= governance.IsSatisfied();
            if (res && population != null)
                res &= population.IsSatisfied();
            if (res && daysOfFood != null)
                res &= daysOfFood.IsSatisfied();
            if (res && techLevel != null)
                res &= techLevel.IsSatisfied();
            if (res && modActive != null)
                res &= modActive.IsSatisfied();
            if (res && decision != null)
                res &= decision.IsSatisfied();

            if (res && primaryIdeoligion != null)
                res &= primaryIdeoligion.ValidFor(pawn) && primaryIdeoligion.IsSatisfied(pawn);
            if (res && meme != null)
                res &= meme.ValidFor(pawn) && meme.IsSatisfied(pawn);
            if (res && precept != null)
                res &= precept.ValidFor(pawn) && precept.IsSatisfied(pawn);

            if (res && pawn != null)
            {
                if (isLeader != null)
                    res &= isLeader.IsSatisfied(pawn);
                if (res && isTarget != null)
                    res &= isTarget.IsSatisfied(pawn, target);
                if (res && trait != null)
                    res &= trait.ValidFor(pawn) && trait.IsSatisfied(pawn);
                if (res && !skills.NullOrEmpty())
                    res &= skills.TrueForAll(so => so.IsSatisfied(pawn));
                if (res && isCapableOfViolence != null)
                    res &= isCapableOfViolence.IsSatisfied(pawn);
                if (res && medianOpinionOfMe != null)
                    res &= medianOpinionOfMe.IsSatisfied(pawn);
                if (res && age != null && age.ValidFor(pawn))
                    res &= age.IsSatisfied(pawn);
                if (res && backstory != null)
                    res &= backstory.ValidFor(pawn) && backstory.IsSatisfied(pawn);
                if (res && titleSeniority != null)
                    res &= titleSeniority.ValidFor(pawn) && titleSeniority.IsSatisfied(pawn);
                if (res && ideoCertainty != null)
                    res &= ideoCertainty.ValidFor(pawn) && ideoCertainty.IsSatisfied(pawn);
            }

            if (res && target != null)
            {
                if (targetIsColonist != null)
                    res &= targetIsColonist.IsSatisfied(target);
                if (res && targetIsLeader != null)
                    res &= targetIsLeader.IsSatisfied(target);
                if (res && targetInAggroMentalState != null)
                    res &= targetInAggroMentalState.IsSatisfied(target);
                if (res && targetIsHostile != null)
                    res &= targetIsHostile.ValidFor(pawn, target) && targetIsHostile.IsSatisfied(pawn, target);
                if (res && targetIsGuilty != null)
                    res &= targetIsGuilty.IsSatisfied(target);
                if (res && targetIsWild != null)
                    res &= targetIsWild.IsSatisfied(target);
                if (res && targetTrait != null)
                    res &= targetTrait.ValidFor(target) && targetTrait.IsSatisfied(target);
                if (res && opinionOfTarget != null)
                    res &= opinionOfTarget.IsSatisfied(pawn, target);
                if (res && medianOpinionOfTarget != null)
                    res &= medianOpinionOfTarget.IsSatisfied(target);
                if (res && targetAge != null)
                    res &= targetAge.ValidFor(target) && targetAge.IsSatisfied(target);
                if (res && targetFactionGoodwill != null)
                    res &= targetFactionGoodwill.ValidFor(target) && targetFactionGoodwill.IsSatisfied(target);
            }

            if (res && all != null)
                res &= all.IsSatisfied(pawn, target);
            if (res && any != null)
                res &= any.IsSatisfied(pawn, target);
            return res;
        }

        public override float GetValue(Pawn pawn = null, Pawn target = null)
        {
            if (!IsSatisfied(pawn, target))
                return 0;
            float s = value;
            governance?.TransformValue(ref s);
            population?.TransformValue(ref s);
            daysOfFood?.TransformValue(ref s);
            for (int i = 0; i < skills.Count; i++)
                skills[i]?.TransformValue(ref s, pawn);
            medianOpinionOfMe?.TransformValue(ref s, pawn);
            age?.TransformValue(ref s, pawn);
            titleSeniority?.TransformValue(ref s, pawn);
            ideoCertainty?.TransformValue(ref s, pawn);
            if (target != null)
            {
                opinionOfTarget?.TransformValue(ref s, pawn, target);
                medianOpinionOfTarget?.TransformValue(ref s, target);
                targetAge?.TransformValue(ref s, target);
                targetFactionGoodwill?.TransformValue(ref s, target);
            }

            foreach (Logic_Consideration factor in factors.Where(factor => factor.IsSatisfied(pawn, target)))
                s *= factor.GetValue(pawn, target);
            foreach (Logic_Consideration offset in offsets)
                s += offset.GetValue(pawn, target);
            return s;
        }

        internal static void AddIndent() => indent += indentSymbol;

        internal static void RemoveIndent() => indent = indent.Remove(0, indentSymbol.Length);

        public override string LabelAdjusted(bool checkmark, Pawn pawn = null, Pawn target = null)
        {
            if (label != null)
                return base.LabelAdjusted(checkmark, pawn, target);

            string res = "";

            void AddLine(string text) => res += $"{indent}{text}\n";

            if (inverted)
            {
                res = $"{base.LabelAdjusted(checkmark, pawn, target)}\n";
                AddIndent();
            }

            if (succession != null)
                AddLine(succession.LabelAdjusted(checkmark));
            if (leaderExists != null)
                AddLine(leaderExists.LabelAdjusted(checkmark));
            if (termDuration != null)
                AddLine(termDuration.LabelAdjusted(checkmark));
            if (campaigning != null)
                AddLine(campaigning.LabelAdjusted(checkmark));
            if (governance != null)
                AddLine(governance.LabelAdjusted(checkmark));
            if (population != null)
                AddLine(population.LabelAdjusted(checkmark));
            if (daysOfFood != null)
                AddLine(daysOfFood.LabelAdjusted(checkmark));
            if (techLevel != null)
                AddLine(techLevel.LabelAdjusted(checkmark));
            if (modActive != null)
                AddLine(modActive.LabelAdjusted(checkmark));
            if (decision != null)
                AddLine(decision.LabelAdjusted(checkmark));

            if (isLeader != null)
                AddLine(isLeader.LabelAdjusted(checkmark, pawn));
            if (isTarget != null)
                AddLine(isTarget.LabelAdjusted(checkmark, pawn, target));
            if (trait != null && trait.ValidFor(pawn))
                AddLine(trait.LabelAdjusted(checkmark, pawn));
            if (!skills.EnumerableNullOrEmpty())
                foreach (Logic_Skill skill in skills)
                    AddLine(skill.LabelAdjusted(checkmark, pawn));
            if (isCapableOfViolence != null)
                AddLine(isCapableOfViolence.LabelAdjusted(checkmark, pawn));
            if (medianOpinionOfMe != null)
                AddLine(medianOpinionOfMe.LabelAdjusted(checkmark, pawn));
            if (age != null && age.ValidFor(pawn))
                AddLine(age.LabelAdjusted(checkmark, pawn));
            if (backstory != null && backstory.ValidFor(pawn))
                AddLine(backstory.LabelAdjusted(checkmark, pawn));
            if (titleSeniority != null && ModsConfig.RoyaltyActive)
                AddLine(titleSeniority.LabelAdjusted(checkmark, pawn));

            if (ModsConfig.IdeologyActive)
            {
                if (meme != null && meme.ValidFor(pawn))
                    AddLine(meme.LabelAdjusted(checkmark, pawn));
                if (precept != null && precept.ValidFor(pawn))
                    AddLine(precept.LabelAdjusted(checkmark, pawn));
                if (ideoCertainty != null && ideoCertainty.ValidFor(pawn))
                    AddLine(ideoCertainty.LabelAdjusted(checkmark, pawn));
                if (primaryIdeoligion != null && primaryIdeoligion.ValidFor(pawn))
                    AddLine(primaryIdeoligion.LabelAdjusted(checkmark, pawn));
            }

            if (targetIsColonist != null)
                AddLine(targetIsColonist.LabelAdjusted(checkmark, target));
            if (targetIsLeader != null)
                AddLine(targetIsLeader.LabelAdjusted(checkmark, target));
            if (targetInAggroMentalState != null)
                AddLine(targetInAggroMentalState.LabelAdjusted(checkmark, target));
            if (targetIsHostile != null && targetIsHostile.ValidFor(pawn, target))
                AddLine(targetIsHostile.LabelAdjusted(checkmark, pawn, target));
            if (targetIsGuilty != null)
                AddLine(targetIsGuilty.LabelAdjusted(checkmark, target));
            if (targetIsWild != null)
                AddLine(targetIsWild.LabelAdjusted(checkmark, target));
            if (targetTrait != null && targetTrait.ValidFor(target))
                AddLine(targetTrait.LabelAdjusted(checkmark, target));
            if (opinionOfTarget != null)
                AddLine(opinionOfTarget.LabelAdjusted(checkmark, pawn, target));
            if (medianOpinionOfTarget != null)
                AddLine(medianOpinionOfTarget.LabelAdjusted(checkmark, target));
            if (targetAge != null)
                AddLine(targetAge.LabelAdjusted(checkmark, target));
            if (targetFactionGoodwill != null)
                AddLine(targetFactionGoodwill.LabelAdjusted(checkmark, target));

            if (all != null)
                AddLine(GenText.Indented(all.LabelAdjusted(checkmark, pawn, target), indent));
            if (any != null)
                AddLine(GenText.Indented(any.LabelAdjusted(checkmark, pawn, target), indent));

            if (inverted)
                RemoveIndent();
            
            return res.TrimEndNewlines();
        }
    }
}
