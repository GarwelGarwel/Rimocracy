using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Rimocracy
{
    public class StatPart_Backstories : StatPart
    {
        public List<StatModifier> modifiers;

        public override string ExplanationPart(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;
            if (modifiers == null || pawn?.story == null)
                return null;

            string res = "";
            StatModifier mod = GetStatModifier(pawn.story.Childhood);
            if (mod != null)
                res = mod.ToString();
            mod = GetStatModifier(pawn.story.Adulthood);
            if (mod != null)
                res += $"\n{mod}";
            return res.Length == 0 ? null : res.Trim();
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            Pawn pawn = req.Thing as Pawn;
            if (modifiers == null || pawn?.story == null)
                return;
            BackstoryEffect(pawn.story.Childhood, ref val);
            BackstoryEffect(pawn.story.Adulthood, ref val);
        }

        StatModifier GetStatModifier(BackstoryDef backstory) => backstory != null ? modifiers.Find(m => m.name == backstory.identifier) : null;

        void BackstoryEffect(BackstoryDef backstory, ref float val)
        {
            if (backstory?.identifier != null)
                GetStatModifier(backstory)?.TransformValue(ref val);
        }
    }
}
