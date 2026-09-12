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
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.InGameEvents.Actions
{
    public class SpawnUpgradeAction: EventActionType 
    {
       // EvalNode UpgradeTarget;
        public TargetObject UpgradeTargetObject;
        public string UpgradeTargetEntityName;
             
        public AllegianceAndExpedition OwnedBy = null;

        public string UpgradeCategoryKey;

        /// <summary>
        /// set this to null to clear the order
        /// </summary>
        public string EntityTypeKey;

      
        public override bool Execute(EventAction action, ref string failReason)
        {
            Entity entity = null;
            if (!EventActionType.GetEntity(UpgradeTargetEntityName, UpgradeTargetObject, action, out entity, ref failReason))
            {
                return false;
            }

            Expedition expedition;
            Allegiance allegiance;
            if (OwnedBy != null)
            {
                
                if (!OwnedBy.Resolve(action, out allegiance, out expedition, ref failReason))
                {
                    failReason = "Failed to resolve owner";
                    return false;
                }
            }
            else
            {
                // else use the structure owner
                // entity
                expedition = entity.GetOwner();
                if (expedition == null)
                {
                    failReason = "Upgrade target is not owned by an expedition";
                    return false;
                }
                else
                {
                    allegiance = expedition.Allegiance;
                }
            }

            SetUpgrade command = new SetUpgrade(entity.ID, allegiance.ID,
                expedition.OwnedEntities.ID, false, GameData.Instance.AllUpgradeCategories[UpgradeCategoryKey], EntityTypeKey);
           
            command.Execute(false); // execute outside CommandInvoker


            return true;               
            
        }


        public override void PostDataCompleteValidate(ref List<string> listOfErrors)
        {

            UpgradeCategory category;
            EntityType.ValidateGameDataTypeExists(ref listOfErrors, UpgradeCategoryKey, GameData.Instance.AllUpgradeCategories, out category);

            EntityType entityType;
            EntityType.ValidateGameDataTypeExists(ref listOfErrors, EntityTypeKey, GameData.Instance.AllEntityTypes, out entityType);
            

        }

       


        public override string ToString()
        {
            return "Set upgrade " + UpgradeCategoryKey + " to " + EntityTypeKey;
        }
    }
}
