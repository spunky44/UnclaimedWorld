using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;
using System.Xml.Serialization;
using UWGame.SimSide.Processes;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Allegiances;

namespace UWGame.SimSide.InGameEvents.Actions
{
    public class SpawnEntityAction : EventActionType
    {
        public EntityData EntityData;

        /// <summary>
        /// if true, the spawned entity will be counted as Produced in the statistics
        /// </summary>
        public bool LogAsProduction = false;


        public bool SuppressSpawnEvents = false;

        #region Dynamic fields, will override EntityData, if present
        // remeber to add them to the GetEntityDataToUse() method
        public EvalNode EntityType = null;
        public AllegianceAndExpedition OwnedBy = null;

        //  public Ownership? OwnershipType;
        //  public TargetObject OwnedBy;

        public EvalNode EntityDataKey = null;

        // etc. add more when needed...

        #endregion

        /// <summary>
        /// for build port, farm plot...
        /// </summary>
        public string ActingOnEntityName;
        public TargetObject ActingOnEntityObject;

        public string ProductionProcessToUse;

        /// <summary>
        /// if not filled, will use allegiance's site
        /// </summary>
        public EvalNode Site;

        /// <summary>
        /// if set, the spawn location will be treated as an offset to the starting location
        /// (is this overriding, or adding..?)
        /// </summary>       
        public DynamicLocation DynamicLocation;

        public ContainerLocation AddToContainer;

        //public PropertyResult[] Properties;

        /// <summary>
        /// for display in the debug panel
        /// </summary>
        [XmlIgnore]
        private string lastSpawnInfo;

        /// <summary>
        /// optional amount. default is 1
        /// </summary>
        public EvalNode Amount;

        /*  private int? amount;

          private Vector3? spawnLocation = null;*/
        //private Entity container = null;


        public SpawnEntityAction(string keyName): base(keyName)
        {

        }

        public SpawnEntityAction()
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
           
            EntityData entityDataToUse = null;
            if (EntityData != null)
            {
                entityDataToUse = EntityData;
            }
            else if (EntityDataKey != null)
            {                
                PropertyResult? entityDataKeyResult = EntityDataKey.Evaluate(action);
                if (entityDataKeyResult.HasValue)
                {
                    string entityDataKey = entityDataKeyResult.Value.StringResult;
                    entityDataToUse = GameData.Instance.AllEntityData[entityDataKey];
                }
            }

         
            string keyToUse;
            if (EntityType != null)
            {

                PropertyResult? entityKey = EntityType.Evaluate(action);
                if (entityKey.HasValue)
                {
                    keyToUse = entityKey.Value.StringResult;
                }
                else
                {
                    lastSpawnInfo = ComposeInfoString(null, null, null);

                    failReason = "Failed to find entity type key";
                    return false;
                }
            }
            else if (entityDataToUse != null)
            {
                keyToUse = entityDataToUse.EntityKey;
            }           
            else //if (ProductionProcessToUse == null)
            {
                return false;
            }

            Entity container = null;
            StorageCondition storageCondition = null;
            bool offerForSale = false;
            bool isProductionOutput = false;
            UpgradeCategory upgradeCategory = null;
            Vector3? spawnLocation = null;
            int? amount = null;

            EntityType entityType = GameData.Instance.AllEntityTypes[keyToUse];

            // will use either a container or a location:          
            if (AddToContainer != null)
            {
                // get the container
                container = AddToContainer.GetContainer(action);
                offerForSale = AddToContainer.OfferForSale;
                isProductionOutput = AddToContainer.IsProductionOutput;

                if (AddToContainer.UpgradeCategory != null)
                {
                    upgradeCategory = GameData.Instance.AllUpgradeCategories[AddToContainer.UpgradeCategory];
                }

                if (container == null)
                {
                    failReason = AddToContainer.TargetObject + " container not found.";
                    lastSpawnInfo = ComposeInfoString(amount, container, spawnLocation);

                    return false;
                }

                if (AddToContainer.StorageCondition != null)
                {
                    storageCondition = GameData.Instance.AllStorageConditions[AddToContainer.StorageCondition];
                }
            }
            else
            {
                // get the location
                if (entityDataToUse != null)
                {
                    spawnLocation = entityDataToUse.Location;
                }
                else
                {
                    spawnLocation = new Vector3(0f, 0f, 0f);
                }


                if (DynamicLocation != null)
                {
                    // add a dynamic offset, such as a starting location, the location of a triggering entity etc.
                    Vector2? offsetLocation = DynamicLocation.GetLocation(action);
                    if (offsetLocation.HasValue)
                    {
                        spawnLocation += offsetLocation.Value.ToVector3();
                    }
                    else
                    {
                        // cancel the spawn if the offset was not available...
                        failReason = DynamicLocation.PropertyKey + " dynamic offset location was null.";
                        lastSpawnInfo = ComposeInfoString(amount, container, spawnLocation);

                        return false;
                    }
                }

                if (spawnLocation.HasValue && !The.Map.WorldLocationIsInsideMap(spawnLocation.Value))
                {
                    failReason = MapManager.WorldPosToSubtile(spawnLocation.Value) + " outside map.";
                    lastSpawnInfo = ComposeInfoString(amount, container, spawnLocation);

                    return false;
                }

                // if the spawning location is blocked by terrain, skip it:    
                if (spawnLocation.HasValue
                    && entityType.IntelligenceType != null
                    && entityType.IsFlyer == false
                    && The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], MapManager.WorldPosToSubtile(spawnLocation.Value)))
                {
                    // only birds may spawn on blocked tiles...
                    failReason = MapManager.WorldPosToSubtile(spawnLocation.Value) + " subtile blocked.";
                    lastSpawnInfo = ComposeInfoString(amount, container, spawnLocation);

                    return false;

                }
            }

            Entity actingOnEntity = null;
            if (ActingOnEntityName != null || ActingOnEntityObject != null)
            {
                if (!EventActionType.GetEntity(ActingOnEntityName, ActingOnEntityObject, action, out actingOnEntity, ref failReason))
                {
                    return false;
                }
            }
            
            amount = 1;

            if (Amount != null)
            {
                PropertyResult? result = Amount.Evaluate(action);
                if (result != null)
                {
                    amount = result.Value.GetIntegerResult() ?? 0;
                }
                else
                {
                    amount = 0;
                }
            }

            if (entityDataToUse == null)
            {               
                entityDataToUse = new EntityData();
                entityDataToUse.EntityKey = keyToUse;
            }

            /*
            EntityData entityDataToUse;
            GetEntityDataToUse(keyToUse, out entityDataToUse);
            */

            string siteKey = null;
            if (Site != null)
            {
                PropertyResult? siteResult = Site.Evaluate(action);
                if (siteResult.HasValue)
                {
                    siteKey = siteResult.Value.StringResult;
                }
            }


            string ownedByAllegianceKey = null, ownedByExpeditionKey = null;
            string memberOfAllegianceKey = null, memberOfExpeditionKey = null;


            if (OwnedBy != null)
            {
                OwnedBy.Resolve(action, ref ownedByAllegianceKey, ref ownedByExpeditionKey);
            }
            else if (entityDataToUse != null)
            {
                if (entityDataToUse.OwnedBy != null)
                {
                    ownedByAllegianceKey = entityDataToUse.OwnedBy.AllegianceKey;
                    ownedByExpeditionKey = entityDataToUse.OwnedBy.ExpeditionKey;
                }
                else if (entityDataToUse.MemberOf != null)
                {
                    if (entityDataToUse.MemberOf.AllegianceKey == "otherSite1Allegiance1")
                    {

                    }

                    memberOfAllegianceKey = entityDataToUse.MemberOf.AllegianceKey;
                    memberOfExpeditionKey = entityDataToUse.MemberOf.ExpeditionKey;
                }
            }

            EntityID? actingOnEntityID = null;
            if (actingOnEntity != null)
            {
                actingOnEntityID = actingOnEntity.ID;
            }

            ProcessType processType = null;
            if (ProductionProcessToUse != null)
            {
                processType = GameData.Instance.AllProcessTypes[ProductionProcessToUse];
            }

            Allegiance owningAllegiance = null;
            for (int i = 0; i < amount; i++)
            {
                // for farm plots, we need a 'worker' to set ownership
                // and we don't want to create the plot entity, instead the finish event should spawn it
                EntityID? spawnedEntityID = null;

                bool placementFailed;
                Entity spawnedEntity = MapLoader.CreateAndPlaceEntityFromEntityData(entityDataToUse, out placementFailed, container, storageCondition, offerForSale, isProductionOutput, spawnLocation,
                    ownedByAllegianceKey, ownedByExpeditionKey,
                    memberOfAllegianceKey, memberOfExpeditionKey, siteKey,
                    anchorID: actingOnEntityID, assertContainment: false, // don't assert, we will handle failed containment
                    upgradeCategory: upgradeCategory, 
                    logProductionStatistics: LogAsProduction,
                    suppressSpawningEvents: SuppressSpawnEvents); 

                spawnedEntityID = spawnedEntity.ID;

                if (placementFailed)
                {
                    spawnedEntity.Destroy();

                    failReason = "Failed to place entity.";
                    lastSpawnInfo = ComposeInfoString(amount, container, spawnLocation);

                    return false;
                }
                The.Sim.World.LastSpawnedEntity = spawnedEntityID;


                if (processType != null)
                {
                  
                    if (actingOnEntity != null)
                    {
                        // fire start events
                        Goal.FireEventActions(null, actingOnEntity.ID,
                            processType.GetStartHook(), processType.EventActions, AgentActionHooks.StartProducing, null);
                    }

                    // see if we can avoid having to create process output instead of spawning...
                    // but using process output with acting on entity would give us the location of the farm plot...

                    // this will fire end events:
                    if (!SimProcess.ProcessProductionFinished(null, processType, actingOnEntityID, spawnedEntityID, SuppressSpawnEvents)) 
                    {                       
                    }
                }


                owningAllegiance = spawnedEntity.GetAllegianceOrOwner();
             

            }

            lastSpawnInfo = ComposeInfoString(amount, container, spawnLocation);

            // give knowledge of the entity acted on to the owner of the entity:
            if (owningAllegiance != null && actingOnEntity != null && actingOnEntity.ID != EntityID.Invalid)
            {
                ProcessAction.DetectEntity(actingOnEntity, owningAllegiance);
            }


            return true;
        }

        


      /*  private void GetEntityDataToUse(string EntityKey, out EntityData entityDataToUse)
        {
            if (EntityData != null)
            {
                entityDataToUse = EntityData;
            }
            else
            {
                entityDataToUse = new EntityData();
                entityDataToUse.EntityKey = EntityKey;
            }

        }*/

      
       
        public override void PostInitValidate(ref List<string> listOfErrors)
        {
            if (EntityData == null && EntityType == null && EntityDataKey == null)
            {
                Entities.EntityType.CreateValidationError(ref listOfErrors, "Neither EntityData nor EntityKey were filled out!");
            }

            if (EntityData != null && EntityData.BioEntity != null)
            {
                EntityData.PostInitValidate(ref listOfErrors);

            }

        }

        public override void PostLoadContentValidate(ref List<string> listOfErrors)
        {
            if (EntityData != null)
            {
                EntityData.PostLoadContentValidate(listOfErrors);
            }
                        
        }

        public override void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (EntityData != null)
            {
                EntityData.PostDataCompleteValidate(ref listOfErrors);
            }


            if (EntityDataKey != null)
            {
                string constantKey = EntityDataKey.EvaluateConstant();
                if (constantKey != null)
                {
                    EntityData entityData;
                    Entities.EntityType.ValidateGameDataTypeExists(ref listOfErrors, constantKey, GameData.Instance.AllEntityData, out entityData);
                }
                  /*  string entityDataKey = entityDataKeyResult.Value.StringResult;
                    entityDataToUse = GameData.Instance.AllEntityData[entityDataKey];*/
                
            }
           
            if (ProductionProcessToUse != null)
            {
                ProcessType process;
                Entities.EntityType.ValidateGameDataTypeExists(ref listOfErrors, ProductionProcessToUse, GameData.Instance.AllProcessTypes, out process);
            }

            if (AddToContainer != null)
            {
                if (AddToContainer.StorageCondition != null)
                {
                    StorageCondition condition;
                    Entities.EntityType.ValidateGameDataTypeExists(ref listOfErrors, AddToContainer.StorageCondition, GameData.Instance.AllStorageConditions, out condition);       
                }
            }
        }


        public override string ToString()
        {
            return lastSpawnInfo ?? "Spawn entity";

        }

        private string ComposeInfoString(int? amount, Entity container, Vector3? spawnLocation)
        {
            string result = "Spawn ";
            if (amount.HasValue)
            {
                result += amount.Value + " ";
            }

            if (EntityData != null && EntityData.EntityKey != null)
            {
                result += this.EntityData.EntityKey;
            }
            else if (EntityDataKey != null)
            {
                result += EntityDataKey.ToString();
            }
            else if (EntityType != null)
            {
                result += EntityType.ToString();
            }
            else
            {
                throw new Exception("No valid EntityKey found for EntityData");
                //return null;
            }
            if (spawnLocation.HasValue)
            {
                result += " at " + spawnLocation.Value.ToString();
            }
            else if (container != null)
            {
                result += " inside ";
                if (!string.IsNullOrEmpty(container.Name))
                {
                    result += container.Name;
                }
                else
                {
                    result += container.KeyName;
                }
                result += " at " + container.Location.ToString();
            }

            return result;
        }
    }
}
