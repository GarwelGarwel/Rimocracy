using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            StringBuilder res = new StringBuilder();

            void AddLine(string str) => res.Append(indent).AppendLine(str);

            void AddLogic(Logic logic) => AddLine(logic.LabelAdjusted(checkmark, pawn, target));

            void AddLogicTarget(Logic logic) => AddLine(logic.LabelAdjusted(checkmark, target));

            if (inverted)
            {
                AddLine(base.LabelAdjusted(checkmark, pawn, target));
                AddIndent();
                checkmark = false;
            }

            if (succession != null)
                AddLogic(succession);
            if (leaderExists != null)
                AddLogic(leaderExists);
            if (termDuration != null)
                AddLogic(termDuration);
            if (campaigning != null)
                AddLogic(campaigning);
            if (governance != null)
                AddLogic(governance);
            if (population != null)
                AddLogic(population);
            if (daysOfFood != null)
                AddLogic(daysOfFood);
            if (techLevel != null)
                AddLogic(techLevel);
            if (modActive != null)
                AddLogic(modActive);
            if (decision != null)
                AddLogic(decision);

            if (isLeader != null)
                AddLogic(isLeader);
            if (isTarget != null)
                AddLogic(isTarget);
            if (trait != null && trait.ValidFor(pawn))
                AddLogic(trait);
            if (!skills.EnumerableNullOrEmpty())
                foreach (Logic_Skill skill in skills)
                    AddLogic(skill);
            if (isCapableOfViolence != null)
                AddLogic(isCapableOfViolence);
            if (medianOpinionOfMe != null)
                AddLogic(medianOpinionOfMe);
            if (age != null && age.ValidFor(pawn))
                AddLogic(age);
            if (backstory != null && backstory.ValidFor(pawn))
                AddLogic(backstory);
            if (titleSeniority != null && ModsConfig.RoyaltyActive)
                AddLogic(titleSeniority);

            if (ModsConfig.IdeologyActive)
            {
                if (meme != null && meme.ValidFor(pawn))
                    AddLogic(meme);
                if (precept != null && precept.ValidFor(pawn))
                    AddLogic(precept);
                if (ideoCertainty != null && ideoCertainty.ValidFor(pawn))
                    AddLogic(ideoCertainty);
                if (primaryIdeoligion != null && primaryIdeoligion.ValidFor(pawn))
                    AddLogic(primaryIdeoligion);
            }

            if (targetIsColonist != null)
                AddLogicTarget(targetIsColonist);
            if (targetIsLeader != null)
                AddLogicTarget(targetIsLeader);
            if (targetInAggroMentalState != null)
                AddLogicTarget(targetInAggroMentalState);
            if (targetIsHostile != null && targetIsHostile.ValidFor(pawn, target))
                AddLogic(targetIsHostile);
            if (targetIsGuilty != null)
                AddLogicTarget(targetIsGuilty);
            if (targetIsWild != null)
                AddLogicTarget(targetIsWild);
            if (targetTrait != null && targetTrait.ValidFor(target))
                AddLogicTarget(targetTrait);
            if (opinionOfTarget != null)
                AddLogic(opinionOfTarget);
            if (medianOpinionOfTarget != null)
                AddLogicTarget(medianOpinionOfTarget);
            if (targetAge != null)
                AddLogicTarget(targetAge);
            if (targetFactionGoodwill != null)
                AddLogicTarget(targetFactionGoodwill);

            if (all != null)
            {
                AddIndent();
                AddLogic(all);
                RemoveIndent();
            }
            if (any != null)
            {
                AddIndent();
                AddLogic(any);
                RemoveIndent();
            }

            if (inverted)
                RemoveIndent();
            
            return res.ToString().TrimEndNewlines();
        }
    }
}
