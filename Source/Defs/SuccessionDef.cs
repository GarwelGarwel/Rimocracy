using System;
using Verse;

namespace Rimocracy
{
    public class SuccessionDef : Def
    {
        public string noun = "succession";

        public float regimeEffect;

        public string newLeaderMessageTitle = "New [LEADERTITLE]";

        public string newLeaderMessageText = "[NATIONNAME] has a new [LEADERTITLE]: [PAWN]. Let [PAWN_possessive] reign be long and prosperous!";

        public string sameLeaderMessageTitle = "[LEADERTITLE] Stays in Power";

        public string sameLeaderMessageText = "[PAWN] remains the [LEADERTITLE] of [NATIONNAME].";

        public virtual string NewLeaderMessageTitle(Pawn pawn) =>
            newLeaderMessageTitle.Formatted(Utility.LeaderTitle.Named("LEADERTITLE"), pawn.Named("PAWN"));

        public virtual string NewLeaderMessageText(Pawn pawn)
            => newLeaderMessageText.Formatted(Utility.NationName.Named("NATIONNAME"), Utility.LeaderTitle.Named("LEADERTITLE"), pawn.Named("PAWN"));

        public virtual string SameLeaderMessageTitle(Pawn pawn) =>
            sameLeaderMessageTitle.Formatted(Utility.LeaderTitle.Named("LEADERTITLE"), pawn.Named("PAWN"));

        public virtual string SameLeaderMessageText(Pawn pawn)
            => sameLeaderMessageText.Formatted(Utility.NationName.Named("NATIONNAME"), Utility.LeaderTitle.Named("LEADERTITLE"), pawn.Named("PAWN"));

        public SuccessionDef Named(string defName) => DefDatabase<SuccessionDef>.GetNamed(defName);
    }
}
