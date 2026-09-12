using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;

namespace UWGame.SimSide.InGameEvents.Actions
{

    /// <summary>
    /// should this spawn nested allegiances and expeditions too? created from templates... only for othersites?
    /// </summary>
    public class SpawnSiteAction : EventActionType
    {
        /// <summary>
        /// in km.
        /// only reliable for short distances, otherwise the distortion becomes great...
        /// </summary>
        public float? DistanceFromPlaySite;

        /// <summary>
        /// in radians!
        /// </summary>
        public float? BearingFromPlaySite;


        public string SiteDataKey;

        public SiteData SiteData;

         public SpawnSiteAction(string keyName): base(keyName)
        {

        }

         public SpawnSiteAction()           
        {

        }


        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, ref string failReason)
        {
            SiteData siteData;
            if (SiteDataKey != null)
            {
                siteData = GameData.Instance.AllSiteData[SiteDataKey];
            }
            else
            {
                siteData = SiteData;
            }

            if (siteData != null)
            {
                if (The.Sim.World.AllSites.ContainsKey(siteData.KeyName)) // SiteData.Key))
                {
                    return false;
                }
            
                GeodeticCoordinate coords;
                if (DistanceFromPlaySite.HasValue && BearingFromPlaySite.HasValue) // DirectionFromPlaySite.HasValue)
                {
                    if (The.Sim.PlaySite != null)
                    {
                        coords = DistanceCalculator.CoordFromDistance(The.Sim.PlaySite.Coords, BearingFromPlaySite.Value, DistanceFromPlaySite.Value, The.Sim.World.WorldRadius);
                        
                        // probably not accurate... but better than nothing
                       /* double latitude = The.Sim.PlaySite.Coords.Latitude + DirectionFromPlaySite.Value.X * DistanceFromPlaySite.Value;
                        double longitude = The.Sim.PlaySite.Coords.Longitude + DirectionFromPlaySite.Value.Y * DistanceFromPlaySite.Value;

                        coords = new GeodeticCoordinate(longitude, latitude);*/
                    }
                    else
                    {
                        failReason = "Playsite not spawned yet";
                        return false;
                    }
                }
                else
                {
                    coords = siteData.Coords;
                }

                Site.CreateFromSiteData(siteData, coords);
            }
            else
            {
                return false;
            }

            return true;               
            
        }

      

        public void PreInitValidate(List<string> listOfErrors)
        { 
        
        }

        public void Initialize()
        { 
        
        }

        public void PostInitValidate(List<string> listOfErrors)
        {
            if (SiteData == null)
            {
            }
           
        }

        public void PostLoadContentValidate(List<string> listOfErrors)
        {
        }


        public override string ToString()
        {
            if (SiteDataKey != null)
            {
                return "Spawn " + SiteDataKey;
            }
            else
            {
                return "Spawn " + this.SiteData.KeyName;
            }
        }
    }
}
