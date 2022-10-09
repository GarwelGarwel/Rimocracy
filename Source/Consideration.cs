using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class Consideration : Logic
    {
        public static readonly Consideration always = new Consideration();

        public static readonly Consideration never = new Consideration() { inverted = true };

        internal static string indent = "";

        internal const string indentSymbol = "  ";

        Logic_All all;
        Logic_Any any;

        List<Consideration> factors = new List<Consideration>();
        List<Consideration> offsets = new List<Consideration>();

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

        public Consideration()
        { }

        public Consideration(float value) => this.value = value;

        public static implicit operator bool(Consideration consideration) => consideration.IsSatisfied(target: Utility.RimocracyComp.Leader);

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

            if (res && primaryIdeoligion != null && primaryIdeoligion.ValidFor(pawn))
                res &= primaryIdeoligion.IsSatisfied(pawn);
            if (res && meme != null && meme.ValidFor(pawn))
                res &= meme.IsSatisfied(pawn);
            if (res && precept != null && precept.ValidFor(pawn))
                res &= precept.IsSatisfied(pawn);

            if (res && pawn != null)
            {
                if (isLeader != null)
                    res &= isLeader.IsSatisfied(pawn);
                if (res && isTarget != null)
                    res &= isTarget.IsSatisfied(pawn, target);
                if (res && trait != null && trait.ValidFor(pawn))
                    res &= trait.IsSatisfied(pawn);
                if (res && !skills.NullOrEmpty())
                    res &= skills.TrueForAll(so => so.IsSatisfied(pawn));
                if (res && isCapableOfViolence != null)
                    res &= isCapableOfViolence.IsSatisfied(pawn);
                if (res && medianOpinionOfMe != null)
                    res &= medianOpinionOfMe.IsSatisfied(pawn);
                if (res && age != null && age.ValidFor(pawn))
                    res &= age.IsSatisfied(pawn);
                if (res && titleSeniority != null && titleSeniority.ValidFor(pawn))
                    res &= titleSeniority.IsSatisfied(pawn);
                if (res && ideoCertainty != null && ideoCertainty.ValidFor(pawn))
                    res &= ideoCertainty.IsSatisfied(pawn);
            }

            if (res && target != null)
            {
                if (targetIsColonist != null)
                    res &= targetIsColonist.IsSatisfied(target);
                if (res && targetIsLeader != null)
                    res &= targetIsLeader.IsSatisfied(target);
                if (res && targetInAggroMentalState != null)
                    res &= targetInAggroMentalState.IsSatisfied(target);
                if (res && targetIsHostile != null && targetIsHostile.ValidFor(pawn, target))
                    res &= targetIsHostile.IsSatisfied(pawn, target);
                if (res && targetIsGuilty != null)
                    res &= targetIsGuilty.IsSatisfied(target);
                if (res && targetIsWild != null)
                    res &= targetIsWild.IsSatisfied(target);
                if (res && targetTrait != null && targetTrait.ValidFor(target))
                    res &= targetTrait.IsSatisfied(target);
                if (res && opinionOfTarget != null)
                    res &= opinionOfTarget.IsSatisfied(pawn, target);
                if (res && medianOpinionOfTarget != null)
                    res &= medianOpinionOfTarget.IsSatisfied(target);
                if (res && targetAge != null && targetAge.ValidFor(target))
                    res &= targetAge.IsSatisfied(target);
                if (res && targetFactionGoodwill != null && targetFactionGoodwill.ValidFor(target))
                    res &= targetFactionGoodwill.IsSatisfied(target);
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

            foreach (Consideration factor in factors.Where(factor => factor.IsSatisfied(pawn, target)))
                s *= factor.GetValue(pawn, target);
            foreach (Consideration offset in offsets)
                s += offset.GetValue(pawn, target);
            return s;
        }

        internal static void AddIndent() => indent += indentSymbol;

        internal static void RemoveIndent() => indent = indent.Remove(0, indentSymbol.Length);

        public override string LabelAdjusted(Pawn pawn = null, Pawn target = null)
        {
            string res = "";

            void AddLine(string text) => res += $"{indent}{text}\n";

            if (inverted)
            {
                res = $"{base.LabelAdjusted(pawn, target)}\n";
                AddIndent();
            }

            if (succession != null)
                AddLine(succession.LabelAdjusted());
            if (leaderExists != null)
                AddLine(leaderExists.LabelAdjusted());
            if (termDuration != null)
                AddLine(termDuration.LabelAdjusted());
            if (campaigning != null)
                AddLine(campaigning.LabelAdjusted());
            if (governance != null)
                AddLine(governance.LabelAdjusted());
            if (population != null)
                AddLine(population.LabelAdjusted());
            if (daysOfFood != null)
                AddLine(daysOfFood.LabelAdjusted());
            if (techLevel != null)
                AddLine(techLevel.LabelAdjusted());
            if (modActive != null)
                AddLine(modActive.LabelAdjusted());
            if (decision != null)
                AddLine(decision.LabelAdjusted());

            if (isLeader != null)
                AddLine(isLeader.LabelAdjusted(pawn));
            if (isTarget != null)
                AddLine(isTarget.LabelAdjusted(pawn, target));
            if (trait != null && trait.ValidFor(pawn))
                AddLine(trait.LabelAdjusted(pawn));
            if (!skills.EnumerableNullOrEmpty())
                foreach (Logic_Skill skill in skills)
                    AddLine(skill.LabelAdjusted(pawn));
            if (isCapableOfViolence != null)
                AddLine(isCapableOfViolence.LabelAdjusted(pawn));
            if (medianOpinionOfMe != null)
                AddLine(medianOpinionOfMe.LabelAdjusted(pawn));
            if (age != null && age.ValidFor(pawn))
                AddLine(age.LabelAdjusted(pawn));
            if (titleSeniority != null && ModsConfig.RoyaltyActive)
                AddLine(titleSeniority.LabelAdjusted(pawn));

            if (ModsConfig.IdeologyActive)
            {
                if (meme != null && meme.ValidFor(pawn))
                    AddLine(meme.LabelAdjusted(pawn));
                if (precept != null && precept.ValidFor(pawn))
                    AddLine(precept.LabelAdjusted(pawn));
                if (ideoCertainty != null && ideoCertainty.ValidFor(pawn))
                    AddLine(ideoCertainty.LabelAdjusted(pawn));
                if (primaryIdeoligion != null && primaryIdeoligion.ValidFor(pawn))
                    AddLine(primaryIdeoligion.LabelAdjusted(pawn));
            }

            if (targetIsColonist != null)
                AddLine(targetIsColonist.LabelAdjusted(target));
            if (targetIsLeader != null)
                AddLine(targetIsLeader.LabelAdjusted(target));
            if (targetInAggroMentalState != null)
                AddLine(targetInAggroMentalState.LabelAdjusted(target));
            if (targetIsHostile != null && targetIsHostile.ValidFor(pawn, target))
                AddLine(targetIsHostile.LabelAdjusted(pawn, target));
            if (targetIsGuilty != null)
                AddLine(targetIsGuilty.LabelAdjusted(target));
            if (targetIsWild != null)
                AddLine(targetIsWild.LabelAdjusted(target));
            if (targetTrait != null && targetTrait.ValidFor(target))
                AddLine(targetTrait.LabelAdjusted(target));
            if (opinionOfTarget != null)
                AddLine(opinionOfTarget.LabelAdjusted(pawn, target));
            if (medianOpinionOfTarget != null)
                AddLine(medianOpinionOfTarget.LabelAdjusted(target));
            if (targetAge != null)
                AddLine(targetAge.LabelAdjusted(target));
            if (targetFactionGoodwill != null)
                AddLine(targetFactionGoodwill.LabelAdjusted(target));

            if (all != null)
                AddLine(GenText.Indented(all.LabelAdjusted(pawn, target), indent));
            if (any != null)
                AddLine(GenText.Indented(any.LabelAdjusted(pawn, target), indent));

            if (inverted)
                RemoveIndent();
            
            return res.TrimEndNewlines();
        }
    }
}
