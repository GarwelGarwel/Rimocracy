using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimocracy
{
    public class ElectionCampaign : IExposable
    {
        // How often campaign updates take place; must be multiple of 250
        internal const int CampaignInterval = 2000;

        Pawn candidate;
        SkillDef focusSkill;
        List<Pawn> supporters;

        public ElectionCampaign()
            : this(null)
        { }

        public ElectionCampaign(Pawn candidate, SkillDef focusSkill = null)
        {
            this.candidate = candidate;
            this.focusSkill = focusSkill;
            supporters = new List<Pawn>();
            if (candidate != null)
                supporters.Add(candidate);
        }

        public Pawn Candidate => candidate;

        public SkillDef FocusSkill
        {
            get => focusSkill;
            set => focusSkill = value;
        }

        public List<Pawn> Supporters
        {
            get => supporters;
            set => supporters = value;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref candidate, "candidate");
            Scribe_Defs.Look(ref focusSkill, "focusSkill");
            Scribe_Collections.Look(ref supporters, "supporters");
        }

        public void RareTick()
        {
            if (Find.TickManager.TicksAbs % CampaignInterval != 0)
                return;

            // List of pawns that will leave the campaign
            List<Pawn> defectors = new List<Pawn>();
            // List of pawns that will join the campaign
            HashSet<Pawn> recruits = new HashSet<Pawn>();

            foreach (Pawn pawn in Supporters.Where(pawn => !pawn.InMentalState && !pawn.Downed))
            {
                // Checking if this pawn should become a defector
                if (pawn != candidate)
                {
                    float defectionChance = 1 - Utility.VoteWeight(pawn, candidate) / 100;
                    if (!pawn.IsCitizen() || Rand.Chance(defectionChance) || Utility.Rimocracy.Candidates.MaxBy(p => Utility.VoteWeight(pawn, p)) != candidate)
                    {
                        Utility.Log(pawn + " is no longer a core supporter for " + candidate + ". Their defection chance was " + defectionChance.ToString("P1"));
                        defectors.Add(pawn);
                        continue;
                    }
                }

                // Picking a random target pawn to sway
                Pawn targetPawn = Utility.Citizens
                    .Where(p => p != pawn && !Utility.Rimocracy.Candidates.Contains(p) && !p.InMentalState && !p.Downed && p.MapHeld == pawn.MapHeld)
                    .RandomElementWithFallback();
                if (targetPawn == null)
                {
                    Utility.Log("No one left to sway.");
                    break;
                }
                Utility.Log(pawn + " is trying to sway " + targetPawn);
                float swayChance = pawn.GetStatValue(StatDefOf.SocialImpact) * 0.1f;
                Utility.Log("Sway chance: " + swayChance.ToString("P1"));
                if (Rand.Chance(swayChance))
                {
                    Utility.Log("Sway successful!");
                    targetPawn.needs.mood.thoughts.memories.TryGainMemory(RimocracyDefOf.PoliticalSympathy, candidate);

                    if (!Utility.Rimocracy.Campaigns.Any(ec => ec.Supporters.Contains(targetPawn)))
                    {
                        // If the target pawn is not already a core supporter of any candidate, try to recruit them to the campaign
                        float recruitChance = (Utility.VoteWeight(targetPawn, candidate) / 100 - 1) * pawn.GetStatValue(StatDefOf.NegotiationAbility);
                        Utility.Log("Chance of recruitment: " + recruitChance.ToString("P1"));
                        if (Rand.Chance(recruitChance))
                        {
                            Utility.Log(pawn + " successfully recruited " + targetPawn + " to support " + candidate);
                            recruits.Add(targetPawn);
                        }
                    }
                }
            }

            // Removing campaign defectors and adding new recruits
            foreach (Pawn p in defectors)
                Supporters.Remove(p);
            Supporters.AddRange(recruits);
        }

        public override string ToString()
            => Candidate + ", " + (focusSkill?.LabelCap.RawText ?? "no") + " focus" + (Supporters.Count > 1 ? ", " + (Supporters.Count - 1) + " core supporters" : "");
    }
}
