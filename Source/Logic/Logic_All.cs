﻿using System.Collections.Generic;
using Verse;

namespace Rimocracy
{
    public class Logic_All : Logic
    {
        List<Logic_Consideration> list = new List<Logic_Consideration>();

        public override string DefaultLabel => "all of the following:";

        public override string LabelAdjusted(bool checkmark, Pawn pawn = null, Pawn target = null)
        {
            string res = base.LabelAdjusted(checkmark, pawn, target);
            for (int i = 0; i < list.Count; i++)
                res += $"\n{list[i].LabelAdjusted(checkmark, pawn, target)}".Indented(Logic_Consideration.indentSymbol);
            return res;
        }

        public override string LabelInverted => $"not all of the following:";

        protected override bool IsSatisfiedInternal(Pawn pawn, Pawn target)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null && list[i].ValidFor(pawn, target) && !list[i].IsSatisfied(pawn, target))
                    return false;
            return true;
        }
    }
}
