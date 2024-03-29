/*
 * This file is part of Dematerializer, a Better Rimworlds Project.
 *
 * Copyright © 2024 Theodore R. Smith
 * Author: Theodore R. Smith <hopeseekr@gmail.com>
 *   GPG Fingerprint: D8EA 6E4D 5952 159D 7759  2BB4 EEB6 CE72 F441 EC41
 *   https://github.com/BetterRimworlds/Dematerializer
 *
 * This file is licensed under the Creative Commons No-Derivations v4.0 License.
 * Most rights are reserved.
 */

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BetterRimworlds.Dematerializer
{
    public class ITab_DematerializerBuffer : ITab_ContentsBase
    {
        public override IList<Thing> container
        {
            get
            {
                var stargate = base.SelThing as Building_Dematerializer;

                return stargate.GetDirectlyHeldThings();
            }
        }

        public ITab_DematerializerBuffer()
        {
            labelKey = "TabCasketContents";
            containedItemsKey = "ContainedItems";
            canRemoveThings = false;
        }
    }
}
