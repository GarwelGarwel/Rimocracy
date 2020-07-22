using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Rimocracy
{
    public class LeaderTitleDef : Def
    {
        public string labelMale;

        public string labelFemale;

        public bool appliesToFixedTerm = true;

        public bool appliesToIndefiniteTerm = true;

        public List<SuccessionType> successionTypes;

        public int minPopulation = 0;

        public int maxPopulation = int.MaxValue;

        public List<TechLevel> techLevels;

        public bool IsApplicable =>
            ((Settings.TermDuration == TermDuration.Indefinite && appliesToIndefiniteTerm)
            || (Settings.TermDuration != TermDuration.Indefinite && appliesToFixedTerm))
            && (successionTypes.NullOrEmpty() || successionTypes.Contains(Settings.SuccessionType))
            && (techLevels.NullOrEmpty() || techLevels.Contains(Find.FactionManager.OfPlayer.def.techLevel))
            && Utility.CitizensCount >= minPopulation
            && Utility.CitizensCount <= maxPopulation;

        public string GetTitle(Pawn pawn = null)
        {
            if (!IsApplicable)
                return null;
            if (pawn == null)
                return label ?? labelMale;
            return (pawn.gender == Gender.Female ? labelFemale : labelMale) ?? label;
        }
    }
}
