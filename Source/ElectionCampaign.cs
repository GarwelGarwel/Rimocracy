using RimWorld;
using Verse;

namespace Rimocracy
{
    public class ElectionCampaign : IExposable
    {
        Pawn candidate;
        SkillDef focusSkill;

        public ElectionCampaign()
        { }

        public ElectionCampaign(Pawn candidate, SkillDef focusSkill = null)
        {
            this.candidate = candidate;
            this.focusSkill = focusSkill;
        }

        public Pawn Candidate => candidate;

        public SkillDef FocusSkill
        {
            get => focusSkill;
            set => focusSkill = value;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref candidate, "candidate");
            Scribe_Defs.Look(ref focusSkill, "focusSkill");
        }

        public override string ToString() => Candidate + (" (focus: " + focusSkill.label + ")" ?? " (no focus)");
    }
}
