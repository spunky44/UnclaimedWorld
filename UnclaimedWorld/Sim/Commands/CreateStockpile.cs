using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Jobs;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Commands
{

    public class CreateStockpile : Control.Commands.Command
    {
       
        public Stockpile.TypesOfStockpiles TypeOfStockpile;

        public long EntityGroupID;

       // public long ExpeditionID;

        public bool GiveClientFeedback;

        /// <summary>
        /// fill in one of the below:
        /// </summary>
        public long Structure;
        public ZoneCommand ZoneCommand;

      

        public SerializableDictionary<string, bool> MayStockpileCategory; // = new SerializableDictionary<EntityCategory, bool>();

        /// <summary>
        /// item setting will override category setting
        /// -1 means no limit
        /// </summary>
        public SerializableDictionary<string, int> MayStockpileItem; 
      //  public SerializableDictionary<string, bool> MayStockpileItem; 


        public string DefaultStorageSettings;

     
        //public class StockpileAmount


        public CreateStockpile()
        {
        }

        public CreateStockpile(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, int> mayStockpileItem,
           EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings,
           ZoneID zoneID, bool giveClientFeedback)
        {
            ZoneCommand = new Commands.ZoneCommand(zoneID);
            TypeOfStockpile = Stockpile.TypesOfStockpiles.Normal;

            Init(mayStockpileCategory, mayStockpileItem, entityGroupID, defaultStorageSettings, giveClientFeedback);

        }


        public CreateStockpile(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, int> mayStockpileItem,
            EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings,
            MapArea mapArea, bool giveClientFeedback)
        {
            ZoneCommand = new Commands.ZoneCommand(mapArea);
            TypeOfStockpile = Stockpile.TypesOfStockpiles.Normal;

            Init(mayStockpileCategory, mayStockpileItem, entityGroupID, defaultStorageSettings, giveClientFeedback);

        }

        public CreateStockpile(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, int> mayStockpileItem,
            EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings,
            EntityID structure, Stockpile.TypesOfStockpiles typeOfStockpile, bool giveClientFeedback)
        {

            this.Structure = (long)structure;
            this.TypeOfStockpile = typeOfStockpile;

            Init(mayStockpileCategory, mayStockpileItem, entityGroupID, defaultStorageSettings, giveClientFeedback);
        }

        private void Init(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, int> mayStockpileItem, EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings, bool giveClientFeedback)
        {
            this.MayStockpileCategory = mayStockpileCategory;
            this.MayStockpileItem = mayStockpileItem;

            this.GiveClientFeedback = giveClientFeedback;

            EntityGroupID = (long)entityGroupID;
            if (defaultStorageSettings != null)
            {
                DefaultStorageSettings = defaultStorageSettings.KeyName;
            }
        }

      /*  public CreateStockpile(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, bool> mayStockpileItem,
            EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings, 
            ZoneID zoneID)
        {
            ZoneCommand = new Commands.ZoneCommand(zoneID);
            TypeOfStockpile = Stockpile.TypesOfStockpiles.Normal;

            Init(mayStockpileCategory, mayStockpileItem, entityGroupID, defaultStorageSettings);
           
        }


        public CreateStockpile(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, bool> mayStockpileItem,
            EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings, 
            MapArea mapArea)
        {
            ZoneCommand = new Commands.ZoneCommand(mapArea);
            TypeOfStockpile = Stockpile.TypesOfStockpiles.Normal;

            Init(mayStockpileCategory, mayStockpileItem, entityGroupID, defaultStorageSettings);

        }

        public CreateStockpile(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, bool> mayStockpileItem,
            EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings,
            EntityID structure, Stockpile.TypesOfStockpiles typeOfStockpile)
        {

            this.Structure = (long)structure;
            this.TypeOfStockpile = typeOfStockpile;

            Init(mayStockpileCategory, mayStockpileItem, entityGroupID, defaultStorageSettings);
        }

        private void Init(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, bool> mayStockpileItem, EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings)
        {
            this.MayStockpileCategory = mayStockpileCategory;
            this.MayStockpileItem = mayStockpileItem;

            EntityGroupID = (long)entityGroupID;
            if (defaultStorageSettings != null)
            {
                DefaultStorageSettings = defaultStorageSettings.KeyName;
            }
        }*/


        public override void Execute(bool giveClientFeedback)
        {    
            
            EntityGroup entityGroupToUse;

            Zone zone = null;

            if (ZoneCommand != null)
            {
                zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);
            }
            else
            {
                entityGroupToUse = LookUp<EntityGroup, EntityGroupID>.FindByID((EntityGroupID)EntityGroupID);
            }

            bool success = DoCreateStockpile(zone, entityGroupToUse);

            if (giveClientFeedback && GiveClientFeedback)
            {
                // call a method in the CLIENT that gives feedback to the player (blinking/beep) IF NEEDED! Not needed when the command is passed from an AI
                // call Client method
                if (success)
                {
                    The.Client.OnCreateStockpile(zone);
                }
            }

          /*  if (giveClientFeedback)
            {
                if (success)
                {
                    The.Client.OnForageArea(zoneToScout);
                }
            }*/

        }


        private bool DoCreateStockpile(Zone zone, EntityGroup entityGroupToUse) // MapArea mapArea, EntityGroup owner, out Stockpile stockpile)
        {
            DefaultStorageSettings defaultStorage = null;
            if (DefaultStorageSettings != null)
            {
                defaultStorage = GameData.Instance.AllDefaultStorageSettings[DefaultStorageSettings];
            }


            Dictionary<EntityCategory, bool> mayStockpileCategory;
            if (MayStockpileCategory != null)
            {
                mayStockpileCategory = MayStockpileCategory.ToDictionary(k => GameData.Instance.AllEntityCategories[k.Key], k => k.Value);
            }
            else
            {
                mayStockpileCategory = new Dictionary<EntityCategory, bool>();
            }

            /*
            Dictionary<EntityType, bool> mayStockpileItem;
            if (MayStockpileItem != null)
            {
                mayStockpileItem = MayStockpileItem.ToDictionary(k => GameData.Instance.AllEntityTypes[k.Key], k => k.Value);
            }
            else
            {
                mayStockpileItem = new Dictionary<EntityType, bool>();
            }*/

            Dictionary<EntityType, int> mayStockpileItem;
            if (MayStockpileItem != null)
            {
                mayStockpileItem = MayStockpileItem.ToDictionary(k => GameData.Instance.AllEntityTypes[k.Key], k => k.Value);
            }
            else
            {
                mayStockpileItem = new Dictionary<EntityType, int>();
            }

            Stockpile stockpile = new Stockpile(TypeOfStockpile, defaultStorage,
                mayStockpileCategory,
                mayStockpileItem);

            if (zone != null)
            {
                zone.Stockpile = stockpile;
            }
            else
            {          
                // structure 'pile
                if (TypeOfStockpile == Stockpile.TypesOfStockpiles.Normal)
                {
                    entityGroupToUse.StructureStockpiles[(EntityID)Structure] = stockpile;
                }
                else
                {
                    entityGroupToUse.TerminalTradeOffers[(EntityID)Structure] = stockpile;
                }
               
            }

            return true;
        }
    }
}
