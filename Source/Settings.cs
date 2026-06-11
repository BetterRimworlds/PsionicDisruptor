/*
 * This file is part of Psionic Disruptor, a Better Rimworlds Project.
 *
 * Copyright © 2024 Theodore R. Smith
 * Author: Theodore R. Smith <hopeseekr@gmail.com>
 *   GPG Fingerprint: D8EA 6E4D 5952 159D 7759  2BB4 EEB6 CE72 F441 EC41
 *   https://github.com/BetterRimworlds/PsionicDisruptor
 *
 * This file is licensed under the Creative Commons No-Derivations v4.0 License.
 * Most rights are reserved.
 */

using UnityEngine;
using Verse;

namespace BetterRimworlds.PsionicDisruptor;

public class Settings: ModSettings
{
    public int countdown = 5;
    public int countdownSpeed = 4;
    public int requiredCapacitorCharge = 2000;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref countdown,               "brw.psionicdisruptor.count", 5);
        Scribe_Values.Look(ref countdownSpeed,          "brw.psionicdisruptor.countdownspeed", 4);
        Scribe_Values.Look(ref requiredCapacitorCharge, "brw.cryoregenesis.requiredCapacitorCharge", 2000);
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard listing_Standard = new Listing_Standard();
        listing_Standard.Begin(inRect);

        string[] labels = {
            "Countdown Count",
            "Countdown speed (lower is faster; 1=250 ticks):",
            "Required capacitor charge? ",
        };

        // targetAge = listing_Standard.TextEntryLabeled(labels[0], targetAge.ToString());
        string buffer = null;
        string buffer2 = null;
        string buffer3 = null;
        listing_Standard.TextFieldNumericLabeled<int>(labels[0], ref countdown, ref buffer);
        listing_Standard.TextFieldNumericLabeled<int>(labels[1], ref countdownSpeed, ref buffer2);
        listing_Standard.TextFieldNumericLabeled<int>(labels[2], ref requiredCapacitorCharge, ref buffer3);

        listing_Standard.End();
    }
}
