using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Commands
{
    public class ZoneCommand
    {
        public long? ZoneID;

        public List<TilePos> CoveredArea;
        public Point StartDragTilePosition;


        public ZoneCommand() { }

        public ZoneCommand(ZoneID zoneID)
        {
            this.ZoneID = (long)zoneID;
        }

        public ZoneCommand(List<TilePos> coveredArea, Point startDragTilePosition)
        {
            this.CoveredArea = coveredArea;
            this.StartDragTilePosition = startDragTilePosition;

        }

        public ZoneCommand(MapArea mapArea)
            : this(mapArea.GetTileLocations(), mapArea.StartDragTile.Value)
        {
            
        } 

        /// <summary>
        /// either looks up an existing zone or creates a new zone for the executing command to use.
        /// </summary>
        /// <param name="zoneID"></param>
        /// <param name="entityGroupID"></param>
        /// <param name="tileLocations"></param>
        /// <param name="startDragTileLocation"></param>
        /// <param name="entityGroupToUse"></param>
        /// <returns></returns>
        public Zone RetrieveOrCreateZone(long entityGroupID, out EntityGroup entityGroupToUse) //, bool giveClientFeedback)
        {
            entityGroupToUse = LookUp<EntityGroup, EntityGroupID>.FindByID((EntityGroupID)entityGroupID);

            Zone zoneToUse = null;
            if (ZoneID.HasValue)
            {
                zoneToUse = LookUp<Zone, ZoneID>.FindByID((ZoneID)ZoneID);

                /*
                foreach (var zone in entityGroupToUse.Zones)
                {
                    if (zone.ID == (ZoneID)ZoneID)
                    {
                        zoneToUse = zone;
                        break;
                    }
                }*/

                if (zoneToUse == null)
                {
                    throw new Exception("Zone not found");
                }

            }
            else
            {
                MapArea coveredArea = new MapArea();
                foreach (var tileLocation in CoveredArea)
                {
                    coveredArea.Add(The.Map.GetTile(tileLocation.X, tileLocation.Y));
                }
                coveredArea.StartDragTile = StartDragTilePosition;

                zoneToUse = new Zone(entityGroupToUse, coveredArea);
               
            }

         /*   if (giveClientFeedback)
            {
                // set selected zone?
                The.InGameUI.SelectedZone = zoneToUse;
            }*/

            return zoneToUse;
        }
    }
}
