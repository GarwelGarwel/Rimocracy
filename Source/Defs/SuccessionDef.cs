using System;
using Verse;

namespace Rimocracy
{
    public class SuccessionDef : Def
    {
        public string noun = "succession";

        public Type workerClass;

        public float regimeEffect;

        public string newLeaderMessageTitle = "New [LEADERTITLE]";

        public string newLeaderMessageText = "[NATIONNAME] has a new [LEADERTITLE]: [PAWN]. Let [PAWN_possessive] reign be long and prosperous!";

        public string sameLeaderMessageTitle = "[LEADERTITLE] Stays in Power";

        public string sameLeaderMessageText = "[PAWN] remains the [LEADERTITLE] of [NATIONNAME].";

        private SuccessionWorker worker;

        public static SuccessionDef Named(string defName) => DefDatabase<SuccessionDef>.GetNamed(defName);

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
    }
}
