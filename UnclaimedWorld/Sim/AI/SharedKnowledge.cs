using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Owners;

namespace UWGame.SimSide.AI
{
    public enum EntityResult
    {
        Remembered,
        SeenDirectly,
        EntityStatusIsNowUnknown, Destroyed,
        NewUnknownEntity // the state for an item we do not know about yet, but we may still own it... such as trapped prey
    }
    public enum ProcessResult { Remembered, SeenDirectly, Destroyed }

    public class SharedKnowledge : ISnapshot//, IIDEventSubscriber
    {

        public Allegiances.Allegiance Allegiance
        {
            get;
            private set;
        }

        private AllegianceID snapshotAllegiance;

        /// <summary>
        /// we only store memory about items not currently seen!
        /// </summary>
        public Dictionary<EntityID, MemoryFact> MemoryFacts = new Dictionary<EntityID, MemoryFact>();
        private Dictionary<EntityID, MemoryFactID> snapshotMemoryFacts = new Dictionary<EntityID, MemoryFactID>();

        /// <summary>
        /// here we store leaf memory facts with their parent as key, in addition to the main collection
        /// </summary>
        private Dictionary<EntityID, List<EntityID>> memoryFactLeafs = new Dictionary<EntityID, List<EntityID>>();



        /// <summary>
        /// the purpose of InUseBy is to enable agents to place locks on items not used in jobs. Such as GoalEat, or weapons/equipment being used in GoalEat and other non-job activities.
        /// 
        /// InuseBy will be set when the agent targets the item for pickup, and cleared when he is done with it (but he may still carry it)
        /// 
        /// 
        /// For simplicity reasons, the lock is also set together with AssignedToJob. 
        /// 
        /// For process tools in unattended processes, the lock is cleared when the agent leaves the process. Then AssignedToJob is the only lock left.
        /// 
        /// Key: item, Value: user
        /// </summary>
       // public Dictionary<EntityID, EntityID> InUseBy;

        public Dictionary<EntityID, EntityLock> EntityLocks;

        /// <summary>      
        /// separate availability of actions into PHYSICAL (for all, all the time) and LOCKS (per allegiance)
        /// PHYSICAL availability is toggled with Anchor.
        /// 
        /// Grow crop: this is a lock (not physical. does not affect other allegiances.)
        /// Use fertilizer: also a lock.
        /// 
        /// Locks: stored in SharedKnowledge for the allegiance, no syncing required. Are set on IKnownEntityData. 
        /// Physical: are only set on the entity. Require syncing       
        /// </summary>
        public Dictionary<EntityID, List<ProcessType>> SpecialActionLocks { get; set; }


        /// <summary>
        /// new generated items (from degradation etc.) that we do not know about (yet)
        /// </summary>
        //  private Dictionary<EntityID, EntityID> NewUnknownItems = new Dictionary<EntityID, EntityID>();



        //  public Dictionary<ItemType, List<Entity>> CropHarvesters = new Dictionary<ItemType, List<Entity>>();

        /// <summary>
        /// for entities that can dissappear in the fog of war:
        /// </summary>
        //   public Dictionary<ResourceType, ObservableList<IResourceItemContainer>> Resources = new Dictionary<ResourceType, ObservableList<IResourceItemContainer>>();


        #region IKnownData collections

        /// <summary>
        /// this collection can be passed to evaluators.        
        /// </summary>
        public EntityGroup AllKnownEntities;
        EntityGroupID snapshotAllKnownEntities;

        // these collections are for entities of some importance that are stored whether they are currently seen or not.
        // if they are removed from MemoryFacts (because they are forgotten) or known to be destroyed, they are removed from these collections also.

        /// <summary>
        /// The main collection, contains all items, entities that we currently know about! Seen or remembered!
        /// The extra collections below contain subsets of these.
        /// 
        /// NOTE! Does not contain allegiance members!!!
        /// 
        /// The multilist structure optimizes Food item evaluation, otherwise it does not seem to be needed.
        /// </summary>
        /*    public Dictionary<EntityType, List<EntityID>> AllKnownEntities = new Dictionary<EntityType, List<EntityID>>();

            /// <summary>
            /// track food items that we are aware of with the purpose to steal them. 
            /// items are only deleted when a) they are seen to be destroyed or b) their memory fact is 'forgotten'
            /// - not used by colonists. only by critters that don't respect ownership. People agents have similar code in OwnerContent
            /// 
            /// food is defined as anything that is directly consumable, or from which a consumable item can be extracted via the entity's innate extraction processes (like teeth)
            /// </summary>      
            public Dictionary<EntityType, List<EntityID>> AllKnownFoodItems
            {
                get
                {
                    if (foodItemsAreDirty)
                    {
                        EntityGroup.UpdateFoodItems(AllKnownEntities, food, Allegiance.IsEatable);
                        foodItemsAreDirty = false;
                    }

                    return food;
                }
            }
            private Dictionary<EntityType, List<EntityID>> food = new Dictionary<EntityType, List<EntityID>>();
            private bool foodItemsAreDirty = false;

            */

        /// <summary>
        /// TODO: move to PlaySiteKnowledge
        /// here we track (only intelligent, for now) entities belonging to different allegiances that we know about, which are on the playsite.
        /// 
        /// Agents moving to the playsite will require special handling if we have already detected them off-map!! Also agents that change allegiance!
        /// 
        /// Note: some structures like terminals and sensors are also considered agents...
        /// </summary>
        // public Dictionary<EntityID, EntityID> AllKnownOutsideAgentsOnPlaySite = new Dictionary<EntityID, EntityID>();


        /// <summary>
        /// Twinkler nests are in this list - as well as all other threatening entities which are not agents...
        /// </summary>
        //   public Dictionary<EntityID, EntityID> AllKnownThreatSources = new Dictionary<EntityID, EntityID>();


        #endregion



        /// <summary>
        /// Contains only Detectables (stealthy entities) that we currently See (=not in FOW).
        /// Contains entities that we have actually spotted for rendering.   
        /// 
        /// Should contain othersite entities too. For attainable computations etc.
        /// Othersite allegiances should be able to see entities offered for sale/hire.
        /// 
        /// Entities are deleted from this list whenever we lose sight of them, for instance if being removed from terminal containers or going into the FOW
        /// Also when going off-site while still in communication range? Probably yes.
        /// 
        /// Originally this list was only intended to cull RollToDetect, but now it seems it has found use elsewhere also...
        ///      
        /// </summary>
        public HashSet<DetectableID> AllDetectedEntities = new HashSet<DetectableID>();


        public PlaySiteKnowledge PlaySiteKnowledge;


        public SharedKnowledge()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        public SharedKnowledge(Allegiances.Allegiance allegiance) //, bool computeAuxiliaryMaps = true)
        {
            this.Allegiance = allegiance;
            snapshotAllegiance = Allegiance.ID;

            AllKnownEntities = new EntityGroup(allegiance, false, false); //, Allegiance.FoodExtraction); 

            EntityLocks = new Dictionary<EntityID, EntityLock>();
            SpecialActionLocks = new Dictionary<EntityID, List<ProcessType>>();

            CreateRegulators();

            if (allegiance.Site.IsPlaySite)
            {
                PlaySiteKnowledge = new PlaySiteKnowledge(this);
            }

        }

        private EntityLock GetOrCreateLock(EntityID item)
        {
            EntityLock entityLock;
            if (!EntityLocks.TryGetValue(item, out entityLock))
            {
                entityLock = new EntityLock();
                EntityLocks[item] = entityLock;
            }

            return entityLock;
        }

        /// <summary>
        /// includes disabled
        /// </summary>
        /// <returns></returns>
        public bool HasSpecialActionsForDisplay(IKnownEntityData entityData)
        {
            List<ProcessType> list = Entity.GetSharedSpecialActionsForDisplay(entityData);

            if (list != null && list.Count > 0)
                return true;

            list = GetOrCreateAvailableSpecialActionLocks(entityData);

            if (list != null && list.Count > 0)
                return true;

            return false;
        }

        public bool SpecialActionIsAvailable(IKnownEntityData entityData, ProcessType processType)
        {
            bool sharedActionIsAvailable = false;
            if (entityData.EntityType.SharedSpecialActionTypes != null
                && entityData.EntityType.SharedSpecialActionTypes.Contains(processType)
                && (entityData.AvailableSharedSpecialActions == null
                || !entityData.AvailableSharedSpecialActions.Contains(processType)))
            {
                sharedActionIsAvailable = false;
                //return false;
            }
            else
            {
                return true;
                //sharedActionIsAvailable = true;
            }
           
            if (!sharedActionIsAvailable)
            {
                List<ProcessType> locks = GetOrCreateAvailableSpecialActionLocks(entityData);
                if (locks != null && locks.Contains(processType))
                {
                    return true;
                }
            }


            return false;
        }

        public List<ProcessType> GetSpecialActionsForDisplay(IKnownEntityData entityData)
        {
            List<ProcessType> list = Entity.GetSharedSpecialActionsForDisplay(entityData);
            
            List<ProcessType> locks = GetOrCreateAvailableSpecialActionLocks(entityData);

            Common.AddRangeToList(ref list, locks);

            return list;
        }

       /*
        public bool HasAvailableSpecialActions(IKnownEntityData entityData)
        {
            if (entityData.AvailableSpecialActions != null && entityData.AvailableSpecialActions.Count > 0
                && entityData.AvailableSpecialActions)
            {
                return true;
            }

            List<ProcessType> actions = GetOrCreateAvailableSpecialActionLocks(entityData);

            return actions != null && actions.Count > 0;
        }*/


        /// <summary>
        /// entities now have unique processes!
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="processKey"></param>
        public void EnableSpecialActionLock(IKnownEntityData entityData, ProcessType sharedProcessType)
        {
            if (entityData.EntityType.SpecialActionLockTypes != null) // only the actions in the set can be user started 
            {
                // look up the custom process with the same original:
                ProcessType uniqueProcessType = entityData.EntityType.SpecialActionLockTypes.FirstOrDefault(p => p.OriginalProcess == sharedProcessType);

                List<ProcessType> locks = GetOrCreateAvailableSpecialActionLocks(entityData);

                if (uniqueProcessType != null && !locks.Any(p => p.OriginalProcess == sharedProcessType))
                {
                    locks.Add(uniqueProcessType);
                }
            }

        }

        public void DisableSpecialActionLock(IKnownEntityData entityData, ProcessType sharedProcessType)
        {
            if (entityData.EntityType.SpecialActionLockTypes != null) // only the actions in the set can be user started 
            {
                // look up the custom process with the same original:
                ProcessType uniqueProcessType = entityData.EntityType.SpecialActionLockTypes.FirstOrDefault(p => p.OriginalProcess == sharedProcessType);

                if (uniqueProcessType != null)
                {
                    List<ProcessType> locks = GetOrCreateAvailableSpecialActionLocks(entityData);

                    locks.Remove(uniqueProcessType);
                }
            }

            /*
            if (entity.AvailableSpecialActions.Contains(sharedProcessType.KeyName)) //!EntityType.SpecialActionTypes.Contains(action))
            {
                entity.AvailableSpecialActions.Remove(sharedProcessType.KeyName);
            }*/
        }


        private List<ProcessType> GetOrCreateAvailableSpecialActionLocks(IKnownEntityData entityData) //EntityID item)
        {
            List<ProcessType> actions = null;
            if (SpecialActionLocks == null || !SpecialActionLocks.TryGetValue(entityData.EntityID, out actions))
            {
                if (entityData.EntityType.SpecialActionLockTypes != null) 
                {
                    actions = new List<ProcessType>();
                    SpecialActionLocks[entityData.EntityID] = actions;

                    foreach (var item in entityData.EntityType.SpecialActionLockTypes)
                    {
                        if (item.SpecialActionEnabledAtStart == true)
                        {
                            actions.Add(item);
                        }
                    }
                }
            }

            return actions;
        }

        public void ClearInUseBy(EntityID itemID, EntityID userID)
        {
            EntityLock entityLock;
            if (EntityLocks.TryGetValue(itemID, out entityLock))
            {
                if (entityLock.InUseBy == userID)
                {
                    entityLock.InUseBy = null;
                }

                DestroyLockIfEmpty(entityLock, itemID); // cleanup      
            }

            /*
            EntityID currentUser;

            if (InUseBy.TryGetValue(itemID, out currentUser)
                && currentUser == userID)
            {
                InUseBy.Remove(itemID);

            }*/
        }

        private void DestroyLockIfEmpty(EntityLock entityLock, EntityID itemID)
        {
            if (entityLock.IsEmpty())
            {
                EntityLocks.Remove(itemID);
            }  
        }

        public void SetInUseBy(EntityID itemID, EntityID userID)
        {
            GetOrCreateLock(itemID).InUseBy = userID;
            //InUseBy[itemID] = userID;

           /*
            EntityID idToSet;
            if (InUseBy.TryGetValue(itemID, out idToSet))
            {
                InUseBy.Remove(entityGroup.ID);
            }

            InUseBy.Add(entityGroup.ID, userID);
           

            DebugLog.Add(string.Format("SetInUseBy: EntityGroup {0}, Entity {1}", entityGroup.ID, userID));
            * */
        }

        public EntityID? GetInUseBy(EntityID itemID)
        {
            EntityLock entityLock;
            if (EntityLocks.TryGetValue(itemID, out entityLock))
            {
                return entityLock.InUseBy;
            }

            return null;

            /*
            EntityID userID;

            if (!InUseBy.TryGetValue(itemID, out userID))
                return null;

            return userID;*/
        }




        /// <summary>
        /// set this dirty when the items in the food list no longer corresponds to the food process types/eating habits in the owning group.
        /// This pattern is also used in OwnerContent.
        /// </summary>
        public void SetFoodDirty()
        {
            AllKnownEntities.SetFoodDirty();

            /*  if (Allegiance.RepresentativeEntityType.Person == null) // people don't use this list, they use ownership lists instead
              {
                  foodItemsAreDirty = true;
              }*/
        }


        public void Destroy()
        {
            if (PlaySiteKnowledge != null)
            {
                PlaySiteKnowledge.Destroy();
            }

            foreach (var item in MemoryFacts)
            {
                item.Value.Destroy();
                //RemoveMemoryFact(item.Value);
            }

            if (AllKnownEntities != null)
            {
                AllKnownEntities.Destroy();
            }
        }



        private Regulator memoryFactsCleanupRegulator;

        private void CreateRegulators()
        {
            memoryFactsCleanupRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.1, "SharedKnowledge");
        }

       // public static object DestroyEntityLock = new object();


        public void Update(GameTime gameTime)
        {
            if (PlaySiteKnowledge != null)
            {
                PlaySiteKnowledge.Update(gameTime);
            }

            if (memoryFactsCleanupRegulator.IsReady())
            {
                List<MemoryFact> memoryFactsToForget = null;

                double currentTimeInSeconds = The.Sim.TotalUnPausedGameTimeInSeconds;

                // memory facts are curently kept for a very long time...
                double oldestDateToKeep = currentTimeInSeconds - The.Sim.DateAndTime.SecondsPerDay * Allegiance.RepresentativeEntityType.IntelligenceType.MemoryInDays; //GameData.Instance.AIConstants.InGameDaysToStoreMemoryFacts;

                // forget stuff here. this would be better with a sleepy updater.
                foreach (var item in MemoryFacts)
                {
                    // here we forget facts about entities that may still exist!
                    // But we never forget owned items/entities!
                    if (!item.Value.IsDeprecated && item.Value.OwnedBy == null)
                    {
                        if (item.Value.TimeStampInSecondsOfGameTime < oldestDateToKeep)
                        {
                            // set facts in deprecated mode before deleting them:
                            item.Value.IsDeprecated = true;
                            item.Value.ToBeDeletedOnTimeStampInSecondsOfGameTime = currentTimeInSeconds + GameData.Instance.AIConstants.SecondsToKeepDeprecatedMemoryFacts; // delete this after X seconds
                        }
                    }
                    else
                    {
                        // delete deprecated facts:
                        // owned entities that are deprecated are assumed to have been destroyed (otherwise we could end up keeping references forever... perhaps realistic, but too much i feel.)
                        // when stealing is implemented, or animals dragging off with items, we should record the old ownership elsewhere... so it can have a chance to go back to the original owner.
                        if (item.Value.ToBeDeletedOnTimeStampInSecondsOfGameTime < currentTimeInSeconds)
                        {
                            if (memoryFactsToForget == null)
                            {
                                memoryFactsToForget = new List<MemoryFact>();
                            }

                            memoryFactsToForget.Add(item.Value);
                        }
                    }
                }

                if (memoryFactsToForget != null)
                {
                    foreach (var item in memoryFactsToForget)
                    {
                        DeleteMemoryOfEntity(item.EntityID, null, true);

                    }
                }
            }


            Entity detectedEntity;
            List<DetectableID> invalidIDs = null;

            // crashed here when destroyed entity???
         /*   lock (DestroyEntityLock)
            {*/
                foreach (var item in AllDetectedEntities)// we don´t need to update memory fact position because they stay in the same place
                {
                    IDetectable detectable = LookUpIDetectables.FindByID(item);
                    if (detectable != null)
                    {
                        detectedEntity = detectable as Entity;
                        if (detectedEntity != null) 
                        {
                            if (detectedEntity.EntityID != EntityID.Invalid)
                            {
                                if (detectedEntity.Site != null && detectedEntity.Site.IsPlaySite) // New: we can have detected off-site entities too.
                                {
                                    if (PlaySiteKnowledge != null)
                                    {
                                        PlaySiteKnowledge.KnownEntityDataTree.UpdateObject(detectedEntity.EntityID, detectedEntity.PlaySiteLocation.ToVector2());
                                    }
                                }
                            }
                            else
                            {
                                Common.AddToList(ref invalidIDs, item);
                            }
                        }
                    }
                    else
                    {
                        Common.AddToList(ref invalidIDs, item);
                    }
                }
         //   }

            if (invalidIDs != null)
            {
                foreach (var item in invalidIDs)
                {
                    if (item == (DetectableID)12947)
                    {

                    }

                    AllDetectedEntities.Remove(item);
                }
            }
            

        }


        /*   public void AddCropHarvester(ItemType cropItem, Entity harvester)
           {
               List<Entity> cropHarvesters;
               if (!CropHarvesters.TryGetValue(cropItem, out cropHarvesters))
               {
                   cropHarvesters = new List<Entity>();
                   CropHarvesters.Add(cropItem, cropHarvesters);
               }

               cropHarvesters.Add(harvester);
           }*/






        /// <summary>
        /// returns known data about this entity.
        /// the current data if we can see the entity, else the stored, remembered data from last we saw it.
        /// if the memory has gone stale, null is returned.
        /// if the seen entity does not exist anymore, null is returned.
        /// 
        /// 
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns>SeenDirectly: it is safe to cast the data to Entity</returns>
        public EntityResult GetKnownData(EntityID entityID, out IKnownEntityData data)
        {
            EntityResult result;
            if (TryGetMemoryFacts(entityID, out data, out result))
            {
                return result;
            }
            else
            {
                data = Entity.FindByID(entityID);
                if (data == null)
                {
                    return EntityResult.Destroyed;
                }
                else
                {
                    if (data.AllegianceID == Allegiance.ID || AllDetectedEntities.Contains(((Entity)data).DetectableID)
                        || !UsesMemory((Entity)data)) // #NEWUNKNOWN - trees are always seen
                    {
                        // it is now safe to cast the data to Entity
                        return EntityResult.SeenDirectly;
                    }
                    else
                    {
                        return EntityResult.NewUnknownEntity; // #NEWUNKNOWN
                    }
                }
            }
        }

        public Entity GetKnownDataAsEntity(EntityID entityID)
        {
            IKnownEntityData data;
            EntityResult result = /*The.InGameUI.UIAllegiance.SharedKnowledge.*/GetKnownData(entityID, out data);

            if (result == EntityResult.SeenDirectly)
            {
                return (Entity)data;
            }
            else
            {
                return null;
            }
        }



        public bool TryGetMemoryFacts(EntityID entityID, out IKnownEntityData data, out EntityResult returnValue)
        {
            MemoryFact status;
            if (MemoryFacts.TryGetValue(entityID, out status))
            {
                if (status.IsDeprecated)
                {
                    data = null;
                    returnValue = EntityResult.EntityStatusIsNowUnknown;
                }
                else
                {
                    data = status;
                    returnValue = EntityResult.Remembered;
                }
                return true;
            }

            returnValue = EntityResult.EntityStatusIsNowUnknown;
            data = null;
            return false;
        }



        /// <summary>
        /// TODO: don't use this method - deprecate it. 
        /// call Location directly on either Entity or IKnownEntityData
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public Vector3 GetLocation(Entity gameObject)
        {
            MemoryFact status;
            if (MemoryFacts.TryGetValue(gameObject.EntityID, out status))
            {
                return status.PlaySiteLocation;
            }
            else
            {
                // we can see it directly:
                return gameObject.PlaySiteLocation;
            }
        }

                    

        /// <summary>
        /// defines which entities we want included in the memory/fog of war system
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool UsesMemory(IDetectable detectable) // Entity entity)
        {
            return detectable.UsesMemory(this);

            /*   return !entity.IsNeverInFogOfWar() // exclude trees and rocks
                   && !entity.IsNotStarted() // exclude entities not being started               
                   && HasInterestInEntity(entity); // can interact with the entity
             */
        }


        
        /// <summary>
        /// NEW: parallel to UnseeEntity
        /// </summary>
        /// <param name="detectable"></param>
        /// <param name="testForUsesMemory"></param>
        /// <param name="suppressClientFeedback"></param>
        /// <param name="resourceDetectionFactor"></param>
        /// <param name="detectingEntity"></param>
        public void SeeDetectableIfRelevant(IDetectable detectable, bool testForUsesMemory = true, bool suppressClientFeedback = false, DetectionFactor resourceDetectionFactor = null, Entity detectingEntity = null, bool doAssert = true)
        {
          /*  if (AllDetectedEntities.Contains(detectable.ID))
            {
                return;               
            }*/

            Entity asEntity = detectable as Entity;

            if (testForUsesMemory == false || asEntity == null || UsesMemory(asEntity))
            {
                SeeDetectable(detectable, suppressClientFeedback, resourceDetectionFactor, detectingEntity, doAssert);              
            }
        }

        /// <summary>
        /// should be in symmetry with StoreMemoryOfEntity...
        /// TODO: most calls to this should be replaced by a call to SeeDetectableIfRelevant
        /// </summary>
        /// <param name="detectable"></param>
        /// <param name="testForUsesMemory"></param>
        /// <param name="suppressClientFeedback"></param>
        /// <param name="resourceDetectionFactor"></param>
        /// <param name="detectingEntity"></param>
        public void SeeDetectable(IDetectable detectable, bool suppressClientFeedback = false, DetectionFactor resourceDetectionFactor = null, Entity detectingEntity = null, bool doAssert = true)
        {
            if (detectable == detectingEntity)
                return;

            // detectingEntity can be null it seems! Watch out.

            // make sure we have not already detected this:

            if (AllDetectedEntities.Contains(detectable.ID))
                return;

            AllDetectedEntities.Add(detectable.ID);


            if (detectingEntity != null)
            {
                HandleInterestOfDetectable(detectingEntity, detectable, resourceDetectionFactor);
            }


            Entity entity = detectable as Entity;


            // is trigger message enough?
            if (detectingEntity != null
                && entity != null
                && entity.CanBeHunted(Allegiance)) // The.Map.EntityCanBeMarkedAsHuntTarget(asEntity))
            {
                detectingEntity.SendMessage(new Message(entity, Message.MessageTypes.PreyIsNear, null));//the message needs to be sent in the sammer manner as the corresponding trigger message
            }


            if (!suppressClientFeedback)
            {
                if (Allegiance.AllegianceType == Allegiances.AllegianceType.Player)
                {
                    The.Client.GiveDetectionFeedback(detectable, resourceDetectionFactor, detectingEntity, Allegiance);
                }

                if (detectingEntity != null)
                {
                    FirePlayerDetectionEvents(detectingEntity, detectable);
                }
            }



            if (entity != null)
            {
                EntityID entityID = entity.EntityID;

                AddToCollectionsOfKnownEntities(entity);

              
                DeleteMemoryOfEntity(entity.ID, null, false);
               

                // Handle leafs. 
                // see inside containers:
                if (entity.EntityType.ContainerType != null) // contains != null)
                {

                    // only see inside containers that the entity can interact through/enter.
                    // entities should be able to see inside containers that are owned by/belong to the same allegiance
                    if (CanSeeInsideContainer(entity))
                    {                       
                        // see inside recursively                    
                        entity.Contains.IterateContained((containedEntity) => SeeDetectableIfRelevant(containedEntity, true, suppressClientFeedback, null, detectingEntity, doAssert: doAssert));
                    }
                }

                //handle parts here:
                if (entity.Parts != null)
                {
                    foreach (var part in entity.Parts)
                    {
                        SeeDetectableIfRelevant(part, true /* testForUsesMemory*/, suppressClientFeedback, resourceDetectionFactor, detectingEntity, doAssert: doAssert);
                    }
                }


              

                // now that we have handled parts and contained (=leaf) entities,
                // see if any leaf memory facts are left over from the leaf entities having been destroyed:
                TerrainTile.DeprecateDistance deprecateDistance = TerrainTile.DeprecateDistance.Near; // always use Near here. The parent is always seen at Far distance, but we won't get the chance to see it again...


                // if the leafmemory facts are not deprecated now, it will still be possible to deprecate them later since their parent still exists.
                DeprecateMemoryFactLeafs(detectingEntity, entityID, deprecateDistance);


                // see any attached processes:
                if (PlaySiteKnowledge != null)
                {
                    if (entity.Processes != null)
                    {
                        foreach (var processID in entity.Processes)
                        {
                            SimProcess process = SimProcess.FindById(processID);
                            if (process != null)
                            {
                                PlaySiteKnowledge.SeeProcess(process);
                            }
                        }
                    }
                }
            }

            // TODO: add to collections for resources here, once we have a memory system for those...
            // not yet using resource memories... this collection is for UI purposes.
            if (PlaySiteKnowledge != null)
            {
                ResourceContainer resource = detectable as ResourceContainer;
                if (resource != null)
                {
                    Common.AddToMultiList(PlaySiteKnowledge.AllKnownResourceContainers, resource.ResourceType, resource.ID);

                    // delete memory of resources here... once implemented.
                }

                if (entity != null && entity.EntityType.BiologicalType != null)
                {
                    var preyTypes = Allegiance.RepresentativeEntityType.IntelligenceType.PreyTypes;
                    if (preyTypes != null && preyTypes.Contains(entity.EntityType))
                    {
                        PlaySiteKnowledge.SpottedPrey.Add(entity.EntityType);
                    }

                    PlaySiteKnowledge.SpottedAnimals.Add(entity.EntityType); // this is set after we have given feedback/logged the detection
                }

                // log statistics for produced items (fish from the fish trap)
                if (entity != null && entity.SpawnedByOwner != null)
                {
                    // log for the product owner
                    // this makes sure ledger statistics does not give info about the FOW
                    IOwner owner = LookUpOwners.FindByID(entity.SpawnedByOwner);
                    if (owner != null && owner.Allegiance == Allegiance)
                    {
                        owner.Allegiance.LogProductionStatistics(entity, null);
                        entity.SpawnedByOwner = null;
                    }
                }



                if (doAssert)
                {
                    PlaySiteKnowledge.AssertSeenEntitiesOnPlaySiteNotInFOW();
                }
            }           
         
        }


        /*
        /// <summary>
        /// should be in symmetry with StoreMemoryOfEntity...
        /// </summary>
        /// <param name="detectable"></param>
        /// <param name="testForUsesMemory"></param>
        /// <param name="suppressClientFeedback"></param>
        /// <param name="resourceDetectionFactor"></param>
        /// <param name="detectingEntity"></param>
        public void SeeDetectable(IDetectable detectable, bool testForUsesMemory = true, bool suppressClientFeedback = false, DetectionFactor resourceDetectionFactor = null, Entity detectingEntity = null)
        {
            if (detectable == detectingEntity)
                return;



            // make sure we have not already detected this:

            if (AllDetectedEntities.Contains(detectable.ID))
                return;

            AllDetectedEntities.Add(detectable.ID);


            if (detectingEntity != null)
            {
                HandleInterestOfDetectable(detectingEntity, detectable, resourceDetectionFactor);
            }

          
            Entity entity = detectable as Entity;

          
            // is trigger message enough?
            if (detectingEntity != null
                && entity != null
                && entity.CanBeHunted(Allegiance)) // The.Map.EntityCanBeMarkedAsHuntTarget(asEntity))
            {
                detectingEntity.SendMessage(new Message(entity, Message.MessageTypes.PreyIsNear, null));//the message needs to be sent in the sammer manner as the corresponding trigger message
            }


            if (!suppressClientFeedback)
            {
                GiveDetectionFeedback(detectable, resourceDetectionFactor, detectingEntity);

                if (detectingEntity != null)
                {
                    FirePlayerDetectionEvents(detectingEntity, detectable);
                }
            }



            if (entity != null)
            {
                EntityID entityID = entity.EntityID;

                AddToCollectionsOfKnownEntities(entity);


                // Handle leafs. 
                // see inside containers:
                if (entity.EntityType.ContainerType != null) // contains != null)
                {

                    // only see inside containers that the entity can interact through/enter.
                    // entities should be able to see inside containers that are owned by/belong to the same allegiance
                    if (CanSeeInsideContainer(entity))// entity.EntityType.ContainerType.CanTransactWithContainer(allegiance.RepresentativeEntityType)) 
                    {
                        //Container contains = Contains;

                        // see inside recursively                    
                        entity.Contains.IterateContained((containedEntity) => SeeDetectable(containedEntity, true, suppressClientFeedback, null, detectingEntity));

                    }
                }

                //handle parts here:
                if (entity.Parts != null)
                {
                    foreach (var part in entity.Parts)
                    {
                        SeeDetectable(part, testForUsesMemory, suppressClientFeedback, resourceDetectionFactor, detectingEntity);
                    }
                }


                if (!testForUsesMemory || UsesMemory(detectable))
                {
                    DeleteMemoryOfEntity(entity.ID, null, false);
                }

                // now that we have handled parts and contained (=leaf) entities,
                // see if any leaf memory facts are left over from the leaf entities having been destroyed:
                TerrainTile.DeprecateDistance deprecateDistance = TerrainTile.DeprecateDistance.Near; // always use Near here. The parent is always seen at Far distance, but we won't get the chance to see it again...
               

                // if the leafmemory facts are not deprecated now, it will still be possible to deprecate them later since their parent still exists.
                DeprecateMemoryFactLeafs(detectingEntity, entityID, deprecateDistance);


                // see any attached processes:
                if (PlaySiteKnowledge != null)
                {
                    if (entity.Processes != null)
                    {
                        foreach (var processID in entity.Processes)
                        {
                            SimProcess process = SimProcess.FindById(processID);
                            if (process != null)
                            {
                                PlaySiteKnowledge.SeeProcess(process);
                            }
                        }
                    }
                }
            }

            // TODO: add to collections for resources here, once we have a memory system for those...
            // not yet using resource memories... this collection is for UI purposes.
            if (PlaySiteKnowledge != null)
            {
                ResourceContainer resource = detectable as ResourceContainer;
                if (resource != null)
                {
                    Common.AddToMultiList(PlaySiteKnowledge.AllKnownResourceContainers, resource.ResourceType, resource.ID);

                    // delete memory of resources here... once implemented.
                }
            }

        }*/

        public void DeprecateMemoryFactLeafs(Entity detectingEntity, EntityID parentOfLeafsEntityID, TerrainTile.DeprecateDistance? deprecateDistance)
        {
            List<EntityID> leafs;
            if (memoryFactLeafs.TryGetValue(parentOfLeafsEntityID, out leafs))
            {
                for (int i = leafs.Count - 1; i >= 0; i--)
                {
                    MemoryFact memoryFact;
                    if (MemoryFacts.TryGetValue(leafs[i], out memoryFact))
                    {
                        if (memoryFact.DeprecateIfNeeded(detectingEntity, deprecateDistance))
                        {
                            //if deprecated, remove... same procedure as for tile references.
                            leafs.RemoveAt(i);
                        }
                    }
                    else
                    {
                        // clean up invalid fact:
                        leafs.RemoveAt(i);
                    }

                    //DeleteMemoryOfEntity(leafs[i], false); // these get deleted instantly, not set as Deprecated first...
                }

                if (leafs.Count == 0)
                {
                    memoryFactLeafs.Remove(parentOfLeafsEntityID);
                }
            }
        }




        private void FirePlayerDetectionEvents(Entity detectingEntity, IDetectable detectable)
        {
            if (Allegiance.AllegianceType == Allegiances.AllegianceType.Player) // only the player will get these sorts of events (tutorial-like dialogs)
            {
                Entity asEntity = detectable as Entity;

                if (asEntity != null)
                {
                    Sensor.HandleEntityTypeDetectionEvents(detectingEntity, asEntity);
                }
                else
                {
                    ResourceContainer resourceContainer = detectable as ResourceContainer;
                    if (resourceContainer != null)
                    {
                        Sensor.HandleResourceDetectionEvents(detectingEntity, resourceContainer.ResourceType);
                    }
                }
            }
        }



        //HashSet<EntityType> DetectedAnimals = new HashSet<EntityType>();

               


        private static void HandleInterestOfDetectable(Entity detectingEntity, IDetectable detectable, DetectionFactor resourceDetectionFactor)
        {
            float interest = 0f;
            Vector3? interestingLocation = null;
            EntityID? interestingEntity = null;

            Entity asEntity = detectable as Entity;

            if (asEntity != null)
            {
                //entity will perhaps make the head turn:
                interest = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(
                           GameData.Instance.Constants.InterestLevelForSpottedEntityMean,
                           GameData.Instance.Constants.InterestLevelForSpottedEntityStdDeviation);

                interestingEntity = asEntity.EntityID;

                //  detectingEntity.Intelligence.SetNewCenterOfAttention(asEntity.EntityID, null, interestLevel);
            }
            else
            {
                // resource                    
                float interestMean, interestStdDev;
                if (resourceDetectionFactor != null)
                {
                    interestMean = resourceDetectionFactor.InterestLevelForSpottedResourceMean ?? GameData.Instance.Constants.InterestLevelForSpottedResourceMean;
                    interestStdDev = resourceDetectionFactor.InterestLevelForSpottedResourceStdDeviation ?? GameData.Instance.Constants.InterestLevelForSpottedResourceStdDeviation;
                }
                else
                {
                    interestMean = GameData.Instance.Constants.InterestLevelForSpottedResourceMean;
                    interestStdDev = GameData.Instance.Constants.InterestLevelForSpottedResourceStdDeviation;
                }


                if (!Common.IsZero(interestMean))
                {
                    interest = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(
                               GameData.Instance.Constants.InterestLevelForSpottedResourceMean,
                               GameData.Instance.Constants.InterestLevelForSpottedResourceStdDeviation);

                    // detectingEntity.Intelligence.SetNewCenterOfAttention(detectable.Location, interestLevel);
                }

                interestingLocation = detectable.Location;
            }


            if (interest > 0f)
            {
                // send a message for the detecting agent to respond to
                // sender is null
                Message message = Trigger.CreateInterestMessage(null, null, interestingEntity, interestingLocation, interest);
                detectingEntity.SendMessage(message);
            }
        }



        private bool IsMemoryRoot(IKnownEntityData entityData)
        {
            return entityData.PartOfID == null && entityData.ContainedBy == null;
        }

        private EntityID? GetMemoryParent(IKnownEntityData entityData)
        {
            if (entityData.ContainedBy.HasValue)
            {
                return entityData.ContainedBy;
            }
            else if (entityData.PartOfID.HasValue)
            {
                IComposite parent = LookUpIComposites.FindByID(entityData.PartOfID.Value);
                Entity parentAsEntity = parent as Entity;
                if (parentAsEntity != null)
                {
                    return parentAsEntity.EntityID;
                }
            }

            return null;
        }




        /// <summary>
        /// Call this for entities that may have been seen before to store memory
        /// 
        /// For newly spawned items, call StoreMemoryOfEntity instead.
        /// </summary>
        /// <param name="entity"></param>
        public void UnSeeEntity(Entity entity)
        {
          
            if ((AllDetectedEntities.Contains(entity.DetectableID)) == false)
            {
                return;
                // only set update memory status if the entity does not require detection, or if it has been detected
            }

            if (UsesMemory(entity))
            {
                StoreMemoryOfEntity(entity, false); // this calls UnSeeEntity recursively on contained entities.

            }
        }


        /// <summary>
        /// the idea is to make this method parallel/symmetrical in structure to SeeDetectable.
        /// so they will both have the same recursion with respect to parts and contained items...
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="testIfEntityBelongs"></param>
        public void StoreMemoryOfEntity(Entity entity, bool testIfEntityBelongs = true)
        {
            if (testIfEntityBelongs && !UsesMemory(entity)) // exclude the test if done previously
            {
                return;
            }

            EntityID entityID = entity.EntityID;
            DetectableID detectableID = entity.DetectableID;

            MemoryFact memoryFact;
            if (MemoryFacts.TryGetValue(entityID, out memoryFact))
            {
                // if we already have it in memory, just update...

                if (memoryFact.MapPosition != entity.MapPosition)
                {
                    if (memoryFact.MapPosition.HasValue)
                    {
                        The.Map.GetTile(memoryFact.MapPosition.Value).RemoveRememberedRootEntity(this, memoryFact);
                    }

                    // move to new position so we can render it in the correct place:
                    // also refresh location/animation???
                    if (IsMemoryRoot(memoryFact)
                        && entity.MapPosition.HasValue)
                    {
                        The.Map.GetTile(entity.MapPosition.Value).AddRememberedRootEntity(this, memoryFact);
                    }

                    if (PlaySiteKnowledge != null)
                    {
                        PlaySiteKnowledge.AddOrUpdateKnownEntityLocation(entity, memoryFact.PlaySiteLocation.ToVector2());                     
                       // PlaySiteKnowledge.KnownEntityDataTree.UpdateObject(entityID, memoryFact.PlaySiteLocation.ToVector2());
                    }
                }

                if (!memoryFact.Init(entity, Allegiance))
                {
                    // the entity is now destroyed...                 
                    DeleteMemoryOfEntity(entityID, detectableID, true); // the entity doesn´t exist anymore, remove knowledge of it

                    return;
                }
            }
            else
            {
                // create a knowledge unit for this object:                  
                memoryFact = MemoryFact.GetNew(entity, Allegiance);

                if (memoryFact != null)
                {
                    if (entity.IsOnPlaySite())
                    {
                        if (IsMemoryRoot(memoryFact))
                        {
                            // if not a part or contained, ie. a root, place on the map so we can render it in the correct place, and also re-detect it:                   
                            The.Map.GetTile(entity.MapPosition.Value).AddRememberedRootEntity(this, memoryFact);
                            //The.Map.TileMap[entity.MapPosition.X][entity.MapPosition.Y].AddRememberedRootEntity(this, memoryFact);
                        }
                        else
                        {
                            // store a reference under its parent so we can clean it up in case it gets destroyed in FOW:
                            Common.AddToMultiList(memoryFactLeafs, GetMemoryParent(memoryFact).Value, memoryFact.EntityID);
                        }

                        if (PlaySiteKnowledge != null)
                        {
                            PlaySiteKnowledge.AddOrUpdateKnownEntityLocation(entity, memoryFact.PlaySiteLocation.ToVector2());
                        }

                    }


                    MemoryFacts.Add(entityID, memoryFact);

                }
                else
                {
                    // the entity is now destroyed...                 
                    DeleteMemoryOfEntity(entityID, detectableID, true); // the entity doesn´t exist anymore, remove knowledge of it   
                    return;
                }
            }

            // NEW: store memory of processes too:
            if (PlaySiteKnowledge != null)
            {
                if (entity.Processes != null)
                {
                    for (int i = entity.Processes.Count - 1; i >= 0; i--)
                    {
                        // Warning! Storing memory of a process requires that we can see its products! If the outputs have not been detected, this will fail and lead to the process being removed!
                        SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(entity.Processes[i]);
                        if (process != null)
                        {
                            PlaySiteKnowledge.StoreMemoryOfProcess(process);                           
                        }
                        else
                        {
                            entity.Processes.RemoveAt(i);
                        }
                    }
                }
            }

            // call recursively on children/parts here
            // code is structured exactly the same as SeeDetectable!

            // unsee inside containers:                
            if (entity.EntityType.ContainerType != null) // contains != null)
            {
                // only see inside containers that the entity can interact through/enter.
                // entities should be able to see inside containers that are owned by/belong to the same allegiance
                if (CanSeeInsideContainer(entity)) 
                {
                    Container contains = entity.Contains;

                    // unsee inside recursively
                    contains.IterateContained(UnSeeEntity);

                }
            }

            //handle parts here:
            if (entity.Parts != null)
            {
                foreach (var part in entity.Parts)
                {
                    UnSeeEntity(part);
                    //StoreMemoryOfEntity(part, testIfEntityBelongs);
                }
            }

            // also remove the Detected status
       
            AllDetectedEntities.Remove(entity.DetectableID);
        }

       

        private bool CanSeeEntity(EntityID entityID, out bool processIsInvalid)
        {
            processIsInvalid = false;

            Entity entity = Entity.FindByID(entityID);
            if (entity != null)
            {
                if (entity.ContainedBy.HasValue)
                {
                    Entity container = Entity.FindByID(entity.ContainedBy.Value);
                    if (container != null)
                    {
                        if (!CanSeeInsideContainer(container))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        processIsInvalid = true;
                        return false;
                    }
                }
            }

            return true;
        }



        /// <summary>
        /// parts can always be seen, but we are not completely sure about containers yet...
        /// </summary>
        /// <returns></returns>
        public bool CanSeeInsideContainer(Entity containerEntity)
        {
            // members can see inside themselves, stomach, carried objects etc.:
            if (containerEntity.AllegianceID == Allegiance.ID)
            {
                return true;
            }

           
            // only see inside containers that the entity can interact through/enter.
            // entities should be able to see inside containers that are owned by/belong to the same allegiance
           
            return containerEntity.EntityType.ContainerType.CanTransactWithContainer(Allegiance.RepresentativeEntityType);
        }




        public void AddKnowledgeOfItemToNewOwner(Entity gameEntity)
        {

            //If the object is inside an container we want to update it using the container status
            /*
                  in container with Remembered status - Remembered |Done|
                  in container with DirectlySeen status - Directly Seen |Done|
                 */

            // If not on playsite, always detect, do not add memory facts.

            if (gameEntity.ContainedBy != null)
            {
                AI.IKnownEntityData data;
                AI.EntityResult result = GetKnownData(gameEntity.ContainedBy.Value, out data);
                //Get the status of the container
                //And then store a memory or seedetectable depending on the container status

                if (gameEntity.IsOnPlaySite() && result == AI.EntityResult.Remembered)
                {
                    StoreMemoryOfEntity(gameEntity);
                }
                else if (result == AI.EntityResult.SeenDirectly)
                {
                   // SeeDetectable(gameEntity);
                    SeeDetectableIfRelevant(gameEntity, doAssert: false);
                }

            }
            else //If we are not inside an container
            {
                /*
                 in FOW  - with Remembered status
                 not in FOW - Directly Seen
                 */

                // on playsite, test the fog of war:
                if (gameEntity.IsOnPlaySite() && The.Map.EntityIsInFogOfWar(gameEntity, Allegiance))
                {
                    StoreMemoryOfEntity(gameEntity);
                }
                else
                {
                   // SeeDetectable(gameEntity);
                    SeeDetectableIfRelevant(gameEntity);
                }
            }

            //call recursively on parts:
            if (gameEntity.Parts != null)
            {
                foreach (var part in gameEntity.Parts)
                {
                    AddKnowledgeOfItemToNewOwner(part);
                }
            }
        }

        /// <summary>
        /// symmetric with AddKnowledgeOfItemToNewOwner
        /// </summary>
        /// <param name="process"></param>
        /*   public void AddKnowledgeOfProcessToCreator(SimProcess process)
           {

               if (The.Map.ProcessIsInFogOfWar(process, Allegiance))
               {
                   StoreMemoryOfProcess(process);
               }
               else
               {
                   SeeProcess(process);
               }           
       
           }*/




        /// <summary>
        /// call this to clean up the playsite knowledge collections of entities that we see leaving the playsite.
        /// This is needed if we want to ensure the quad tree only contains valid entities.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="detectableID"></param>
        /// <param name="removeAllKnowledge"></param>
        /// <returns></returns>
        public void DeletePlaySiteKnowledgeOfEntity(EntityID entityID)
        {
            // why not delete from AllDetectedEntities too?

            if (PlaySiteKnowledge != null)
            {
                PlaySiteKnowledge.RemoveFromCollectionsOfKnownEntities(entityID);
            }

        }


        /// <summary>
        /// call this when the entity has been detected, or when we want to forget it.
        ///      
        /// </summary>
        /// <param name="gameObject"></param>
        public bool DeleteMemoryOfEntity(EntityID entityID, DetectableID? detectableID, bool removeAllKnowledge) // IDetectable detectable) 
        {
            Entity entity = Entity.FindByID(entityID);

            if (entity == null || removeAllKnowledge)
            {
                EntityType entityType = null;
                if (entity != null)
                {
                    entityType = entity.EntityType;
                }

                RemoveFromCollectionsOfKnownEntities(entityType, entityID);

            }

            if (removeAllKnowledge
                && detectableID.HasValue)
            {
               
                AllDetectedEntities.Remove(detectableID.Value); // don't do this step when seeing entities!      
            }

            MemoryFact memoryFact;
            if (MemoryFacts.TryGetValue(entityID, out memoryFact))
            {
                // remove from main collection
                MemoryFacts.Remove(entityID);

                // as well as from the leaf collection:
                EntityID? parent = GetMemoryParent(memoryFact);
                if (parent.HasValue)
                {
                    Common.RemoveFromMultiList(memoryFactLeafs, parent.Value, memoryFact.EntityID, true);
                }

                // Sync entity with the properties that were set on the memory fact:               
                if (entity != null)
                {
                    entity.SyncWithMemoryFact(Allegiance, memoryFact);

                }


                // before destroying the memory fact, see if it has processes attached. the entity may no longer have them if they finished in FOW.
                // so in order to clean up the process memories, we have to do it now.
                if (PlaySiteKnowledge != null)
                {
                    PlaySiteKnowledge.DeleteMemoryOfProcesses(memoryFact);
                }

                memoryFact.Destroy();


                return true;
            }


            return false;
        }



        public void RemoveInvalidEntityIDs(List<EntityID> list)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    RemoveInvalidEntityID(item);
                }
            }


        }

        public void RemoveInvalidEntityID(EntityID entityID)
        {

            RemoveFromCollectionsOfKnownEntities(null, entityID);

            /* 

            DeleteMemoryOfEntity();*/

        }

        public void AddToCollectionsOfKnownEntities(Entity entity)
        {
            EntityID entityID = entity.ID;
           
            if (!AllKnownEntities.Contains(entity))
            {
                AllKnownEntities.AddEntity(entity); // <- adds to Food and AllEntities
            }


            if (entity.Site != null && entity.Site.IsPlaySite)
            {
                if (PlaySiteKnowledge != null)
                {
                    PlaySiteKnowledge.AddOrUpdateKnownEntityLocation(entity, entity.PlaySiteLocation.ToVector2());
                }
            }

        }

        /// <summary>
        /// Removes seen entities as well as memoryfacts.
        /// 
        /// if entitytype is not provided, removal will take longer...
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="entityID"></param>
        private void RemoveFromCollectionsOfKnownEntities(EntityType entityType, EntityID entityID)
        {
            AllKnownEntities.DeleteEntity(entityID, entityType);

            // We don't delete from AllDetectedEntities here, since this method gets called from DeleteMemoryOfEntity as well

            SpecialActionLocks.Remove(entityID);

            EntityLocks.Remove(entityID); // NEW


            if (PlaySiteKnowledge != null)
            {
                PlaySiteKnowledge.RemoveFromCollectionsOfKnownEntities(entityID);
            }
        }



        public DiscomfortMap GetDiscomfortMap(ProtectionLevel p, EntityType c, ThreatStance a)
        {
            return PlaySiteKnowledge.GetDiscomfortMap(p, c, a);
        }

        public ThreatMap GetThreatMap(EntityType c, ThreatStance a)
        {
            return PlaySiteKnowledge.GetThreatMap(c, a);
        }

        /// <summary>
        /// WARNING: crashes if the sentry is used as lookup entity type!
        /// </summary>
        /// <param name="p"></param>
        /// <param name="c"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public MovementMap GetMovementMap(ProtectionLevel p, EntityType c, ThreatStance a)
        {
            return PlaySiteKnowledge.GetMovementMap(p, c, a);
        }

        /// <summary>
        /// WARNING: crashes if the sentry is used as lookup entity type!
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="threatStance"></param>
        /// <returns></returns>
        public RegionMap GetFootRegionMap(Entity entity, ThreatStance? threatStance = null)
        {
            return GetMovementMap(entity, threatStance).Layers[SurfaceType.TransportType.Foot].RegionMap;
        }

        public RegionMap GetVehicleRegionMap(Entity entity, ThreatStance? threatStance = null)
        {
            return GetMovementMap(entity, threatStance).Layers[SurfaceType.TransportType.OffRoad].RegionMap;
        }

        public RegionMap GetRegionMapToUseForEntity(Entity entity, ThreatStance? threatStance = null)
        {
            if (entity.DrivingVehicle != null)
            {
                // TODO: probably should not use this from an evaluator! don't assume that a vehicle will be useful/available...
                return GetVehicleRegionMap(entity, threatStance);
            }
            else
            {
                return GetFootRegionMap(entity, threatStance);
            }
        }

        public MovementMap GetMovementMap(Entity entity, ThreatStance? threatStance = null)
        {
            return PlaySiteKnowledge.GetMovementMap(entity, threatStance);
        }

        /*   public ThreatMap GetThreatMap(ThreatCategory c, ThreatStance a)
           {
               return (ThreatMap)ThreatMaps[c][a].GetCurrent();
           }*/

        /*  public RegionMap GetRegionMap(ProtectionLevel p, ThreatCategory c, EntityApproach a)
          {
              return (RegionMap)AllRegionMaps[p][c][a];
          }

          private void AddRegionMap(ProtectionLevel p, ThreatCategory c, EntityApproach a, RegionMap map)
          {
           
              if (!AllRegionMaps.ContainsKey(p))
              {
                  AllRegionMaps.Add(p, new Dictionary<ThreatCategory, Dictionary<EntityApproach, RegionMap>>());
              }
              if (!AllRegionMaps[p].ContainsKey(c))
              {
                  AllRegionMaps[p].Add(c, new Dictionary<EntityApproach, RegionMap>());
              }
              AllRegionMaps[p][c].Add(a, map);

          }*/



        /// <summary>
        /// crate a new set of maps if needed for the new member type -only for movers?
        /// </summary>
        /// <param name="newMember"></param>
        public void AddMember(Entity newMember)
        {
            if (PlaySiteKnowledge != null)
            {
                PlaySiteKnowledge.AddMember(newMember);
            }
        }

        public void RemoveMember(Entity memberToRemove)
        {
            if (PlaySiteKnowledge != null)
            {
                PlaySiteKnowledge.RemoveMember(memberToRemove);
            }
        }




        /// <summary>
        /// refresh our knowledge about this object
        /// </summary>
        /// <param name="gameObject"></param>
        /*  public void UpdateKnownStatus(IGameEntity gameObject)
          {            
              KnownStatus status;
              if (Statuses.TryGetValue(gameObject, out status))
              {
                  status.IsDestroyed = gameObject.IsDestroyed;
                  status.Location = gameObject.Location;
              }
              else
              {
                  if (!gameObject.IsDestroyed)
                  {
                      // create a knowledge unit for this object:
                      status = new KnownStatus() { Location = gameObject.Location, IsDestroyed = gameObject.IsDestroyed };//, Condition = gameOb}
                      Statuses.Add(gameObject, status);
                  }
                  // if we didn't know about the object before, well, now it is destroyed, so it doesn't matter.                    
              }            
          }*/

        // also other knowledge...?


        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        /// <summary>
        /// debug method..
        /// </summary>
        /// <returns></returns>
        private DetectableID GetID(KeyValuePair<IDetectable, bool> kvp)
        {
            System.Diagnostics.Debug.Assert(kvp.Key.ID != DetectableID.Invalid, "Invalid ID!!");
            return kvp.Key.ID;
        }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.snapshotAllegiance = sn.DoEnum(snapshotAllegiance);

            this.PlaySiteKnowledge = (PlaySiteKnowledge)sn.DoISnapshot(PlaySiteKnowledge);

            AllDetectedEntities = sn.DoHashSet(AllDetectedEntities);

            //  this.AllKnownOutsideAgentsOnPlaySite = sn.DoDictionary(AllKnownOutsideAgentsOnPlaySite);
            //  this.AllKnownThreatSources = sn.DoDictionary(AllKnownThreatSources);

            this.snapshotAllKnownEntities = (EntityGroupID)sn.SnapshotID<EntityGroup, EntityGroupID>(AllKnownEntities);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                // gather the data to save:

                //  this.snapshotKnownEntityDataTree = knownEntityDataTree != null ? knownEntityDataTree.GetAllObjectsAndPositions() : null;
                this.snapshotMemoryFacts = MemoryFacts.ToDictionary(m => m.Key, m => m.Value.ID);
            }


            /*  this.snapshotKnownEntityDataTree = sn.DoList(snapshotKnownEntityDataTree); // save tree members for recreation post-load
              this.knownEntityDataTree = (PointQuadTree<EntityID>)sn.DoISnapshot(knownEntityDataTree); // only snapshots some data, not the whole quad tree and nodes.
              */

            // this.NewUnknownItems = sn.DoDictionary(NewUnknownItems);

            this.snapshotMemoryFacts = sn.DoDictionary(snapshotMemoryFacts);
            this.memoryFactLeafs = sn.DoMultiMap(memoryFactLeafs);

            this.EntityLocks = sn.DoDictionary(EntityLocks);
            this.SpecialActionLocks = sn.DoMultiMap(SpecialActionLocks);

            // this.TransientThreats not currently used... maybe added later?

            sn.Ignore(Allegiance);
            sn.Ignore(AllDetectedEntities);
            sn.Ignore(MemoryFacts);


            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);

            AllKnownEntities = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotAllKnownEntities);


            /* if (knownEntityDataTree != null)
             {
                 knownEntityDataTree.SetPreLoadPostProcess(snapshotKnownEntityDataTree);
                 knownEntityDataTree.LoadPostProcess(sn); // this will fix the quad tree

                 snapshotKnownEntityDataTree = null;
             }*/

            if (PlaySiteKnowledge != null)
            {
                PlaySiteKnowledge.Parent = this;
                PlaySiteKnowledge.LoadPostProcess(sn);
            }


            if (snapshotMemoryFacts != null)
            {
                MemoryFacts = snapshotMemoryFacts.ToDictionary(m => m.Key, m => (MemoryFact)LookUp<MemoryFact, MemoryFactID>.FindByID(m.Value));
            }

            if (EntityLocks != null)
            {
                foreach (var item in EntityLocks)
                {
                    item.Value.LoadPostProcess(sn);
                }
            }


            CreateRegulators();
        }

        #endregion



    }
}
