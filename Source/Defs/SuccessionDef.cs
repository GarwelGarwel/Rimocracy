using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class SuccessionDef : Def
    {
        public string noun = "succession";

        public Type workerClass;

        public float weight = 1;

        List<StatModifier> memes = new List<StatModifier>();

        public float regimeEffect;

        public float loyaltyEffect;

        public string newLeaderMessageTitle = "New {LEADERTITLE}";

        public string newLeaderMessageText = "{NATIONNAME} has a new {LEADERTITLE}: {PAWN}. May {PAWN_possessive} reign be long and prosperous!";

        public string sameLeaderMessageTitle = "{LEADERTITLE} Stays in Power";

        public string sameLeaderMessageText = "{PAWN} remains the {LEADERTITLE} of {NATIONNAME}.";

        private SuccessionWorker worker;

        public SuccessionWorker Worker
        {
            get
            {
                if (worker == null && workerClass != null)
                {
                    worker = (SuccessionWorker)Activator.CreateInstance(workerClass);
                    worker.def = this;
                }
                return worker;
            }
        }

        public float GetWeight(Ideo ideo)
        {
            float res = weight;
            if (!memes.NullOrEmpty() && ideo != null)
                foreach (StatModifier meme in memes.Where(meme => ideo.memes.Exists(m => m.defName == meme.name)))
                    meme.TransformValue(ref res);
            return res;
        }
    }
}
