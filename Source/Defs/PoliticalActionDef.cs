using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    /// <summary>
    /// Game action (like arresting a pawn or attacking a settlement) that citizens can have their opinions about and that can have political repercursions
    /// </summary>
    class PoliticalActionDef : Def
    {
        public bool allCitizensReact = true;
        public List<Consideration> considerations = new List<Consideration>();
        public float governanceChangeIfSupported;
        public float governanceChangeIfOpposed;

        public Type actionClass;
        public string actionMethod;
        public int targetArgument;

        public void Activate(Pawn target = null)
        {
            Utility.Log($"{defName} activated for target {target}.");
            float support;
            string msg = $"{label} happened.";

            if (Utility.RimocracyComp.Leader != null)
            {
                support = Consideration.GetSupportValue(considerations, Utility.RimocracyComp.Leader, target);
                    Utility.Log($"The leader's support for action {defName} is {support}.");
                if (support != 0)
                {
                    Utility.RimocracyComp.Governance = Mathf.Clamp(Utility.RimocracyComp.Governance + support > 0 ? governanceChangeIfSupported : governanceChangeIfOpposed, 0, 1);
                    msg += $" {Utility.LeaderTitle} {Utility.RimocracyComp.Leader} {(support > 0 ? "supported" : "opposed")} the decision. Governance is now {Utility.RimocracyComp.Governance:P1}.";
                }
            }

            if (allCitizensReact)
            {
                msg += $" Citizens' reactions:";
                foreach (Pawn pawn in Utility.Citizens)
                {
                    support = Consideration.GetSupportValue(considerations, pawn, target);
                    Utility.Log($"{pawn}'s support for action {defName} is {support}.");
                    if (support != 0)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RimocracyDefOf.DecisionMade, support > 0 ? 1 : 0));
                        msg += $"\n- {pawn} {(support > 0 ? "supported" : "opposed")} the decision.";
                    }
                    else msg += $"\n- {pawn} is indifferent.";
                }
            }

            Find.LetterStack.ReceiveLetter("Political Repercursions", msg, LetterDefOf.NeutralEvent);
        }
    }
}
