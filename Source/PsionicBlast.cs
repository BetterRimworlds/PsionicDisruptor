using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
namespace BetterRimworlds
{
    public class PsionicBlast
    {
        private static bool isWearingPsychicFoilHelmet(Pawn pawn)
        {
            return pawn?.apparel?.WornApparel?.Any(a => a.def.defName == "Apparel_PsychicFoilHelmet") ?? false;
        }

        private static bool isWearingMarineHelmet(Pawn pawn)
        {
            return pawn?.apparel?.WornApparel?.Any(
                a => a.def.defName == "Apparel_PowerArmorHelmet" ||
                            a.def.defName == "Apparel_ArmorHelmetRecon"
            ) ?? false;
        }

        public static int calculateHelmetProtection(Pawn pawn)
        {
            if (pawn.apparel == null)
            {
                return 0;
            }

            if (isWearingPsychicFoilHelmet(pawn))
            {
                return 5;
            }

            // Add protection for any helmet.
            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
                {
                    //Log.Warning("Helmet name: " + apparel.def.defName);
                    return isWearingMarineHelmet(pawn) ? 2 : 1;
                }
            }

            return 0;
        }

        public static void DoPsionicBlast()
        {
            foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawns.ToArray())
            {
                if (pawn.RaceProps.FleshType != FleshTypeDefOf.Normal)
                {
                    continue;
                }

                (new PsionicBlast()).AddPsionicShock(pawn);
            }
        }

        private void CauseHeartAttack(Pawn pawn)
        {
            HediffDef heartAttack = HediffDef.Named("HeartAttack");

            BodyPartRecord heart = pawn.RaceProps.body.AllParts.Find(bpr => bpr.def.defName == "Heart");

            pawn.health.AddHediff(heartAttack, heart, null);
        }

        private static void CauseSedation(Pawn pawn)
        {
            pawn.health.AddHediff(HediffDefOf.Anesthetic, null, null);
        }

        private void CauseDisorientation(Pawn pawn)
        {
            pawn.health.AddHediff(HediffDefOf.PsychicHangover, null, null);
            pawn.health.AddHediff(HediffDefOf.CryptosleepSickness, null, null);
        }

        private static void CauseCatatonia(Pawn pawn)
        {
            pawn.health.AddHediff(HediffDefOf.CatatonicBreakdown, null, null);
        }

        private void AddPsionicShock(Pawn pawn)
        {
            System.Random rand = new System.Random();

            int psychicSensitivity = 0;
            bool? shouldGiveHeartAttack = null;
            bool? shouldSedate = null;

            if (pawn.story?.traits != null && pawn.story.traits.HasTrait(TraitDef.Named("PsychicSensitivity")))
            {
                Trait psychicSensitivityTrait = pawn.story.traits.GetTrait(TraitDef.Named("PsychicSensitivity"));
                psychicSensitivity = psychicSensitivityTrait.Degree;
            }

            psychicSensitivity -= calculateHelmetProtection(pawn);

            // If they're Psychically Deaf, do nothing:
            if (psychicSensitivity <= -2)
            {
                return;
            }
            // If they're Psychically Dull, don't give them a heart attack.
            else if (psychicSensitivity == -1)
            {
                shouldGiveHeartAttack = false;
                CauseDisorientation(pawn);
            }
            // If they're Psychically Sensitive, make sure they're passed out for a few hours.
            else if (psychicSensitivity == 1)
            {
                shouldSedate = true;
            }
            // If they're Psychically Hypersensitive, unfortunately, it will mean instant death or catatonia :-(
            else if (psychicSensitivity >= 2)
            {
                bool shouldKill = rand.Next(1, 13) >= 6;
                if (shouldKill == true)
                {
                    if (pawn.IsColonist)
                    {
                        Messages.Message(pawn.Name.ToStringShort + " was psychically supersensitive and died because of the psionic blast.", MessageTypeDefOf.ThreatSmall);
                    }

                    HealthUtility.DamageUntilDead(pawn);
                }
                else
                {
                    CauseCatatonia(pawn);
                }
            }

            // If they have a Bionic Heart, then skip but give them bad food poisoning instead.
            // bool hasBionicHeart = pawn.RaceProps.body.AllParts.Exists(bpr =>
            // {
            //     return bpr.def.defName == "Heart" && (
            //         bpr.def.label.Contains("bionic") ||
            //         bpr.def.label.Contains("arcotech")
            //     );
            // });
            bool hasBionicHeart =
                pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamedSilentFail("BionicHeart"));

            if (shouldGiveHeartAttack != false && hasBionicHeart)
            {
                shouldGiveHeartAttack = false;
            }

            // If they aren't wearing a Marine helmet, give them a psychic shock 100% of the time.
            // If they are wearing a helmet, only shock them 50% of the time.
            if (isWearingMarineHelmet(pawn) == false || rand.Next(0, 2) == 0)
            {
                Hediff shock = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn, null);
                pawn.health.AddHediff(shock, null, null);
            }

            if (shouldGiveHeartAttack == null)
            {
                shouldGiveHeartAttack = rand.Next(1, 11) <= 4;

                if (psychicSensitivity > 0 && shouldGiveHeartAttack == false)
                {
                    // Flip a coin again to see if they should have a heart attack.
                    shouldGiveHeartAttack = rand.Next(0, 2) == 1;
                }
            }

            if (shouldGiveHeartAttack == true)
            {
                this.CauseHeartAttack(pawn);
            }

            if (shouldSedate == null)
            {
                int likelihood = rand.Next(1, 11);
                shouldSedate = likelihood >= 6 + (isWearingMarineHelmet(pawn) ? 2 : 0);
            }

            if (shouldSedate == true)
            {
                CauseSedation(pawn);
            }

            DamageInfo psionicIntensity = new DamageInfo(DamageDefOf.Stun, 50 + (10 * psychicSensitivity));
            pawn.TakeDamage(psionicIntensity);
        }

    }
}
