using UnityEngine;
using Verse;

namespace BetterRimworlds.PsionicDisruptor
{
    public class Settings: ModSettings
    {
        public int countdownSpeed = 4;
        public int requiredCapacitorCharge = 20000;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref countdownSpeed,          "brw.psionicdisruptor.countdownspeed", 4);
            Scribe_Values.Look(ref requiredCapacitorCharge, "brw.cryoregenesis.requiredCapacitorCharge", 20000);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);

            string[] labels = {
                "Countdown speed (lower is faster; 1=250 ticks):",
                "Required capacitor charge? ",
            };

            // targetAge = listing_Standard.TextEntryLabeled(labels[0], targetAge.ToString());
            string buffer = null;
            string buffer2 = null;
            listing_Standard.TextFieldNumericLabeled<int>(labels[0], ref countdownSpeed, ref buffer);
            listing_Standard.TextFieldNumericLabeled<int>(labels[1], ref requiredCapacitorCharge, ref buffer2);

            listing_Standard.End();
        }
    }
}
