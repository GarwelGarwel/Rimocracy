using Verse;

namespace Rimocracy.Requirements
{
    public class Requirement_Succession : Requirement
    {
        SuccessionType succession = SuccessionType.Undefined;

        //protected override bool GetBaseValue() => Utility.RimocracyComp.SuccessionType == succession;

        //public override void ExposeData()
        //{
        //    base.ExposeData();
        //    Scribe_Values.Look(ref succession, "succession");
        //}
    }
}
