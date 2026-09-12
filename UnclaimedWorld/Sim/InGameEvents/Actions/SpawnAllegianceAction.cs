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
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;

namespace UWGame.SimSide.InGameEvents.Actions
{
    /// <summary>
    /// doesn't spawn if allegiance or expedition with the same key already exists.
    /// </summary>
    public class SpawnAllegianceAction : EventActionType
    {
        /// <summary>
        /// If set this action will also create an expedition for the new allegiance.
        /// </summary>
        public ExpeditionData ExpeditionData = null;

        public string AllegianceDataKey;

        public AllegianceData AllegianceData;

       
      //  public string AllegianceKeyName;

        public string Site;

        public SpawnAllegianceAction(string keyName): base(keyName)
        {

        }

        public SpawnAllegianceAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
            AllegianceData data;
            if (AllegianceDataKey != null)
            {
                data = GameData.Instance.AllAllegianceData[AllegianceDataKey];
            }
            else
            {
                data = AllegianceData;
            }

            //Look for and deny allegiance with duplicate key.
            if (The.Sim.World.GetAllegianceFromKey(data.KeyName) != null)
            {
                failReason = "Allegiance already exists.";
                return false;
            }

            Allegiance allegiance;
            if (data != null)
            {
               // AllegianceData.Site = Site; 
                Site site = null;
                if (Site != null)
                {
                    site = The.Sim.World.AllSites[Site];
                }

                allegiance = Allegiance.CreateFromAllegianceData(data, site); //, allegianceKeyName: AllegianceKeyName);
                if (allegiance == null)
                {
                    failReason = "Failed to create allegiance.";
                    return false;
                }
            }
            else
            {
                return false;
            }

            if (ExpeditionData != null)
            {
                return Expedition.CreateFromExpeditionData(ExpeditionData, allegiance, action, null, null, out failReason);
            }

            return true;               
            
        }
            

        public override string ToString()
        {
            if (AllegianceData != null)
            {
                if (this.AllegianceData.Site != null)
                    return "Spawn " + this.AllegianceData.KeyName + " in " + this.AllegianceData.Site;
                else
                    return "Spawn " + this.AllegianceData.KeyName;
            }
            else
            {
                return "Spawn " + this.AllegianceDataKey;
            }
        }
    }
}
