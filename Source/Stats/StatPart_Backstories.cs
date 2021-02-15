using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Rimocracy
{
    public class StatPart_Backstories : StatPart
    {
        public List<StatModifierString> modifiers;

        public override string ExplanationPart(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;

            if (modifiers == null || pawn?.story == null)
                return null;

            string res = "";
            StatModifierString mod = GetStatModifierString(pawn.story.childhood);
            if (mod != null)
                res = mod.ToString();
            mod = GetStatModifierString(pawn.story.adulthood);
            if (mod != null)
                res += $"\n{mod}";
            return res == "" ? null : res.Trim();
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            Pawn pawn = req.Thing as Pawn;
            
            if (modifiers == null || pawn?.story == null)
                return;

            BackstoryEffect(pawn.story.childhood, ref val);
            BackstoryEffect(pawn.story.adulthood, ref val);
        }

        StatModifierString GetStatModifierString(Backstory backstory) => backstory != null ? modifiers.Find(m => m.name == backstory.identifier) : null;

        void BackstoryEffect(Backstory backstory, ref float val)
        {
            if (backstory?.identifier == null)
                return;

            StatModifierString mod = GetStatModifierString(backstory);
            if (mod != null)
            {
                val *= mod.factor;
                val += mod.offset;
            }
        }
    }
}
