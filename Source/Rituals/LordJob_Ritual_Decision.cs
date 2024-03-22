using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimocracy
{
    public class LordJob_Ritual_Decision : LordJob_Joinable_Speech
    {
        DecisionDef decisionDef;

        public LordJob_Ritual_Decision()
        { }

        public LordJob_Ritual_Decision(TargetInfo selectedTarget, RitualRoleAssignments assignments, DecisionDef decisionDef)
            : base(selectedTarget, Utility.RimocracyComp.Leader, (Precept_Ritual)PreceptMaker.MakePrecept(RimocracyDefOf.Rimocracy_DecisionAnnouncement), new List<RitualStage>() { new RitualStage_GoverningBenchSpeech() }, assignments, false)
        {
            this.decisionDef = decisionDef;
        }
    }
}
