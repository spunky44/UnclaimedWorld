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
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;

namespace UWGame.SimSide.InGameEvents.Actions
{
    public class SpawnStockpileAction : EventActionType
    {
       // public Stockpile.TypesOfStockpiles TypeOfStockpile;

        //public long EntityGroupID;

        public AllegianceAndExpedition OwnedBy = null;

      //  public long Structure;
       // public ZoneCommand ZoneCommand;

        public Point[] /* List<TilePos>*/ CoveredArea;
        public Point StartDragTilePosition;

        

        public SerializableDictionary<string, bool> MayStockpileCategory; // = new SerializableDictionary<EntityCategory, bool>();

        /// <summary>
        /// item setting will override category setting
        /// </summary>
        public SerializableDictionary<string, int> MayStockpileItem; 
      //  public SerializableDictionary<string, bool> MayStockpileItem; // = new Dictionary<EntityType, bool>();


      //  public string DefaultStorageSettings;

         public SpawnStockpileAction(string keyName): base(keyName)
        {

        }

         public SpawnStockpileAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EventAction eventAction /*EntityID? triggeringEntity, EntityID? targetEntity*/, ref string failReason)
        {
           /* if (!The.Sim.World.AllSites.ContainsKey(RouteData.FromSite))
            {
                failReason = "Site: " + RouteData.FromSite + " not found.";
                return false;
            }*/

            MapArea mapArea = new MapArea();
            foreach (var tilepos in CoveredArea)
            {
                mapArea.Add(The.Map.GetTile(tilepos));    
            }
            mapArea.StartDragTile = StartDragTilePosition; 

            Allegiance allegiance; 
            Expedition expedition;
            if (!OwnedBy.Resolve(action, out allegiance, out expedition, ref failReason))
            {
                return false;
            }


            CreateStockpile command = new CreateStockpile(MayStockpileCategory, MayStockpileItem, expedition.OwnedEntities.ID, null, mapArea, true);

            command.Execute(false); // execute outside CommandInvoker

            /*
            if (RouteData != null)
            {
                Route.CreateFromRouteData(RouteData);
            }
            else
            {
                return false;
            }*/

            return true;               
            
        }


        public override void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (MayStockpileCategory != null)
            {
                foreach (var item in MayStockpileCategory)
                {
                    EntityCategory catgeory;
                    EntityType.ValidateGameDataTypeExists(ref listOfErrors, item.Key, GameData.Instance.AllEntityCategories, out catgeory);
                }
            }

            if (MayStockpileItem != null)
            {
                foreach (var item in MayStockpileItem)
                {
                    EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, item.Key);
                }
            }
        }

        public override void PreInitValidate(ref List<string> listOfErrors)
        {
            base.PreInitValidate(ref listOfErrors);

            if (!CoveredArea.Contains(StartDragTilePosition))
            {
                EntityType.CreateValidationError(ref listOfErrors, "StartDragTilePosition is not in the list of Covered tiles");
            }
        
        }

       


        public override string ToString()
        {
            return "Spawn stockpile: " + OwnedBy.ToString();
        }
    }
}
