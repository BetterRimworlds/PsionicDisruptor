﻿/*
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

using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace BetterRimworlds
{
    public class Utilities
    {
        public static IEnumerable<Pawn> findPawnsInArea(Area_Allowed teleportArea)
        {
            IEnumerable<Pawn> pawns = Find.CurrentMap.mapPawns.AllPawns.Where(c => c is Pawn && teleportArea.ActiveCells.Contains(c.Position));

            return pawns;
        }

        public static IEnumerable<Pawn> findPawnsInColony(IntVec3 position, float radius)
        {
            //IEnumerable<Pawn> pawns = Find.ListerPawns.ColonistsAndPrisoners;
            //IEnumerable<Pawn> pawns = Find.ListerPawns.FreeColonists;
            //IEnumerable<Pawn> pawns = Find.ListerPawns.AllPawns.Where(item => item.IsColonistPlayerControlled || item.IsColonistPlayerControlled);

            IEnumerable<Pawn> pawns = Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer);
            IEnumerable<Pawn> closePawns;

            if (pawns != null)
            {
                closePawns = pawns.Where<Pawn>(t => t.Position.InHorDistOf(position, radius));
                return closePawns;
            }
            return null;
        }


        static public List<Thing> FindItemThingsNearBuilding(Thing centerBuilding, int radius, Map map)
        {
            IEnumerable<Thing> closeThings = GenRadial.RadialDistinctThingsAround(centerBuilding.Position, map, radius, true);

            var closeItems = new List<Thing>(closeThings.Count());

            foreach (Thing tempThing in closeThings)
            {
                if (tempThing.def.category == ThingCategory.Item)
                {
                    closeItems.Add(tempThing);
                }
            }

            return closeItems;
        }
    }
}

