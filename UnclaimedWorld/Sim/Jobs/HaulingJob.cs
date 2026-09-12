using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.Jobs
{
    public abstract class HaulingJob : Job
    {

        /// <summary>
        /// the group of items that will be searched to fulfill the job, if needed.
        /// Since it is not an Owner object, this should support hauling by critters...
        /// </summary>
        public EntityGroupID ItemsToHaulGroup;

        /// <summary>
        /// EITHER a ground location 
        /// </summary>
        public Vector3? ToLocation;

        /// <summary>
        /// OR an entity storage
        /// 
        /// </summary>
        public StorageTarget? ToStorage;
      //  public StorageTarget? ToTradeOfferStorage;

        public bool IsToTradeOfferStorage;

        /*
        /// <summary>
        /// OR an entity storage
        /// 
        /// </summary>
        public EntityID? ToStorageEntity;
        public StorageID? ToStorageID; // NEW
        public StorageCondition ToStorageCondition;


        public EntityID? ToTradeOfferStorageEntity; // NEW
        public StorageID? ToTradeOfferStorageID; // NEW
        public StorageCondition ToTradeOfferStorageCondition; // NEW

        */

        /// <summary>
        /// when the process job dies, any matching hauling jobs will be destroyed also (in ProcessJob.Destroy)
        /// 
        /// so it should be safe to use a reference for now..
        /// </summary>
        public ProcessJob RequiredByProcessJob;
        JobID? snapshotRequiredByProcessJob;

        private double CreatedOn;


        public OwnerID? NewOwner;

        private EntityID? item;
        /// <summary>
        /// Must be set to a value when an AnyItem job is assigned an item to use.
        /// </summary>
        public EntityID? Item
        {
            get
            {
                return item;
            }
            set
            {
                if (value != item)
                {
                    EntityID? oldValue = item;
                    item = value;

#if DEBUG
                    if (item != null)
                    {
                        EntityGroup group1 = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);

                        IKnownEntityData itemData1;
                        group1.GetAllegiance().SharedKnowledge.GetKnownData(item.Value, out itemData1);

                        if (itemData1 != null && !itemData1.IsCompleted())
                        {
                            // OK if clearing items?
                       //     System.Diagnostics.Debug.Assert(false, "Assigned incomplete item?"); //MP commented out feb 24 2016 because the bug has been documented
                        }
                    }
#endif

                    // propagate the change to the process job for the Client to see:
                    if (RequiredByProcessJob != null)
                    {

                        if (oldValue.HasValue)
                        {
                            Common.RemoveFromMultiList(RequiredByProcessJob.InputsBeingHauled, null, oldValue.Value);
                        }

                        if (item.HasValue)
                        {
                            // lookup, so we can organize it by type
                            EntityGroup group = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);

                            IKnownEntityData itemData;
                            group.GetAllegiance().SharedKnowledge.GetKnownData(item.Value, out itemData);

                            if (!Common.MultiListContains(RequiredByProcessJob.InputsBeingHauled, itemData.EntityType, item.Value))
                            {
                                Common.AddToMultiList(RequiredByProcessJob.InputsBeingHauled, itemData.EntityType, item.Value);
                            }

                        }
                    }
                }
            }
        }



        public HaulingJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }


        /// <summary>
        /// we need the owner of the items in order to cleanu the AssignedToJob lock afterwards...
        /// </summary>
        /// <param name="toLocation"></param>
        /// <param name="storageEntity"></param>
        /// <param name="storage"></param>
        /// <param name="addToList"></param>
        /// <param name="newOwner"></param>
        /// <param name="priority"></param>
        public HaulingJob(Vector3? toLocation, 
            StorageTarget? toStorage,
            bool isToTradeOfferStorage,
            EntityGroup entityGroup, OwnerID? newOwner, bool addToJobsGroupNow)
            : base(entityGroup, addToJobsGroupNow)
        {
            this.ToLocation = toLocation;

            this.ToStorage = toStorage;
            this.IsToTradeOfferStorage = isToTradeOfferStorage;
        
            this.NewOwner = newOwner;

            this.CreatedOn = The.Sim.TotalUnPausedGameTimeInSeconds;

           /* this.ToStorageCondition = storage;
            this.ToStorageEntity = storageEntity;*/

            /*
            ComputeJobType();
            SetDefaultPriority(entityGroup);*/
        }


        public override void TakeJob(Entity entity)
        {
#if DEBUG || PROFILE
            // this assert has combinatorial explosion when there are many hauling jobs. causes freeze.
           /* if (entity.Intelligence.Allegiance.AllegianceType == Allegiances.AllegianceType.Player)
            {
                foreach (var item in entity.Intelligence.Allegiance.Members)
                {
                    if (item != entity)
                    {
                        if (item.Intelligence.Brain != null)
                        {
                            foreach (var goal in item.Intelligence.Brain.Subgoals)
                            {
                                GoalHaul goalHaul = goal as GoalHaul;
                                if (goalHaul != null)
                                {
                                    if (goalHaul.GoalHasJob(this))
                                    {
                                  //      throw new Exception("Taking a job that is still held by another agent's goal?? Still assigned..?");
                                    }

                                } 

                            }
                        }
                    }
                    
                }

            }
            */
#endif

            base.TakeJob(entity);
        }


        public bool IsCompleted // #HAULMANAGERFIX
        {
            get;
            set;
        }

        public override Vector3? GetCircaLocation()
        {
            return null; // TODO if needed
        }

        public bool IsStoredInTarget(IKnownEntityData entityData)
        {
            /* (job.ToStorageEntity == null 
                    || job.ToStorageEntity != itemData.StoredPermanentlyIn // not already at the destination
                    || job.ToStorageCondition != itemData.StoredPermanentlyCondition)
            */
            if (ToStorage.HasValue)
            {
                if (ToStorage == entityData.StoredPermanentlyIn)
                {
                    return true;
                }

                /*
                if (IsToTradeOfferStorage)
                {
                    if (entityData.IsOfferedForTrade && ToStorage == entityData.StoredPermanentlyIn) 
                    {
                        return true;
                    }
                }
                else
                {
                    if (!entityData.IsOfferedForTrade && ToStorage == entityData.StoredPermanentlyIn)
                    {
                        return true;
                    }
                }*/
            }

            return false;

           /* return
                (ToStorage.HasValue
                && ToStorage == entityData.StoredIn)
                ||
                (ToTradeOfferStorage.HasValue
                && ToTradeOfferStorage == entityData.OfferedForTradeIn);
               */

                /*
            return (ToStorageEntity.HasValue
                && ToStorageEntity == entityData.StoredPermanentlyIn
                && ToStorageID == entityData.StoredPermanentlyStorageID
                && ToStorageCondition == entityData.StoredPermanentlyCondition)
                ||
                (ToTradeOfferStorageEntity.HasValue
                && ToTradeOfferStorageEntity == entityData.OfferedForTradeIn 
                && ToTradeOfferStorageID == entityData.OfferedForTradeStorageID
                && ToTradeOfferStorageCondition == entityData.OfferedForTradeCondition);*/

        }

     /*   public Storage GetToStorage
        {
            get
            {
                if (ToStorage.HasValue)
                {
                    return LookUp<Storage, StorageID>.FindByID(ToStorage.Value.StorageID);
                }              
                else
                {
                    return null;
                }

            }
        }*/

        public StorageID? GetToStorageID
        {
            get
            {
                if (ToStorage.HasValue)
                {
                    return ToStorage.Value.StorageID;
                }
               /* else if (ToTradeOfferStorage.HasValue)
                {
                    return ToTradeOfferStorage.Value.StorageID;
                }*/
                else
                {
                    return null;
                }
            }
        }

        public EntityID? GetToStorageEntity
        {
            get
            {
                if (ToStorage.HasValue)
                {
                    return ToStorage.Value.StorageEntity;
                }
               /* else if (ToTradeOfferStorage.HasValue)
                {
                    return ToTradeOfferStorage.Value.StorageEntity;
                }*/
                else return null;
            }
        }

        /// <summary>
        /// this bool could be looked up from StorageID and StorageEntity, but cache it here for now...
        /// </summary>
        public bool IsOfferedForTrade
        {
            get
            {
                return IsToTradeOfferStorage;
                //return ToTradeOfferStorage.HasValue;
            }
        }

        public StorageCompartment? GetToStorageCompartment
        {
            get
            {
                if (ToStorage.HasValue)
                {
                    if (IsToTradeOfferStorage)
                    {
                        return StorageCompartment.OfferedForTrade;
                    }
                    else
                    {
                        return StorageCompartment.NormalStorage;
                    }
                }
                else return null;
            }
        }

       /* public virtual bool IsOfferedForTrade
        {
            get
            {
                return false;
            }
        }*/


        /// <summary>
        /// only for client feedback
        /// </summary>
        public void MarkClientAsInputNotEnroute()
        {
            if (RequiredByProcessJob != null
                && item != null)
            {
                EntityGroup group = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);

                if (group != null)
                {
                    IKnownEntityData itemData;
                    group.GetAllegiance().SharedKnowledge.GetKnownData(item.Value, out itemData);

                    if (itemData != null)
                    {
                        Common.RemoveFromMultiList(RequiredByProcessJob.InputsBeingHauled, itemData.EntityType, item.Value);

                    }

                }

                // RequiredByProcessJob.IsInputBeginDelivered = false;
            }
        }

        public override bool RequiresBoldStance
        {
            get
            {
                if (RequiredByProcessJob != null)
                {
                    return RequiredByProcessJob.ProcessType.RequiresBoldStance;
                }

                return false;
            }
        }

        /// <summary>
        /// NEW - there was a problem with a dangling item.AssignedToJob reference...
        /// </summary>
        /// <param name="cancelTakers"></param>
        public override void Destroy(bool cancelTakers, Entity entityToExcludeFromCancel = null)
        {
            JobID thisID = ID; // save before destroying it...

            base.Destroy(cancelTakers, entityToExcludeFromCancel);

            if (item.HasValue)
            {
                // unassign item
                EntityGroup itemGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(ItemsToHaulGroup);

                if (itemGroup == null)
                    return;

                // TODO: look up allegiance from SharedJobs instead??
                SharedKnowledge knowledge = itemGroup.Parent.Allegiance.SharedKnowledge;
                IKnownEntityData itemData;
                if (!GoalEvaluator.EntityDataResultCausesSkip(knowledge.GetKnownData(item.Value, out itemData)))
                {
                    if (itemData.AssignedToJob == thisID)
                    {
                        itemData.AssignedToJob = null;
                    }
                }

                // not sure if this is needed, but just to be safe:
                Entity itemEntity = Entity.FindByID(item.Value);
                if (itemEntity != null)
                {
                    if (itemEntity.AssignedToJob == thisID)
                    {
                        itemEntity.AssignedToJob = null;
                    }
                }

            }
        }


        /// <summary>
        /// returns false if storage is destroyed
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public bool GetToLocation(Entity agent, out Vector3? toLocation)
        {
            toLocation = null;

            if (ToLocation.HasValue)
            {
                toLocation = ToLocation.Value;

                return true;
            }
            else
            {
                IKnownEntityData storageEntity;
                EntityResult result = agent.Intelligence.Allegiance.SharedKnowledge.GetKnownData(ToStorage.Value.StorageEntity /* ToStorageEntity.Value*/, out storageEntity);
                if (result == EntityResult.Remembered || result == EntityResult.SeenDirectly)
                {
                    toLocation = storageEntity.Location;
                    return true;
                }

                return false;
            }

        }


        protected void GetToLocationAsString(StringBuilder b)
        {
            if (ToLocation.HasValue)
            {
                b.Append(MapManager.WorldPosToTile(ToLocation.Value).ToString());
            }
            else
            {
                Entity storage = Entity.FindByID(ToStorage.Value.StorageEntity); // ToStorageEntity.Value);
                if (storage != null)
                {
                    b.Append(storage.Name + " at " + storage.MapPosition.ToString());
                }
            }
        }


        public float ScoreTimePassed()
        {
           // double timeSinceCreation = The.Sim.TotalUnPausedGameTimeInSeconds - CreatedOn;

            double timePassedStarvation = The.Sim.TotalUnPausedGameTimeInSeconds - CreatedOn - GameData.Instance.AIConstants.TimePassedForHaulingJobsToScoreHigher;

            if (timePassedStarvation > 0d) // timeSinceCreation > GameData.Instance.AIConstants.TimePassedForHaulingJobsToScoreHigher)
            {
                timePassedStarvation = Common.ClampTop(timePassedStarvation, GameData.Instance.AIConstants.MaxAdditionalTimeForStarvedHaulingJobScore);
                return (float) timePassedStarvation / GameData.Instance.AIConstants.MaxAdditionalTimeForStarvedHaulingJobScore;
             
                //float timeFactor = timePassedStarvation / GameData.Instance.AIConstants.MaxAdditionalTimeForStarvedHaulingJobScore;
              //  return MathHelper.Lerp(0f, GameData.Instance.AIConstants.StarvedHaulingJobScore, timeFactor);
            }

            return 0f;
        }

        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.


            version = sn.DoVersion((Snapshotter.Version)2);  // Dec, 2016.  increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            snapshotRequiredByProcessJob = sn.SnapshotID<Job, JobID>(RequiredByProcessJob);
            this.ToStorage = sn.DoStorageTargetNullable(ToStorage);
            //this.ToTradeOfferStorage = sn.DoStorageTargetNullable(ToTradeOfferStorage);
            this.IsToTradeOfferStorage = sn.DoBool(IsToTradeOfferStorage);
           // this.ToStorageEntity = sn.DoEnumNullable(ToStorageEntity);
            this.ToLocation = sn.DoVector3Nullable(ToLocation);
           // this.ToStorageCondition = sn.DoGameData(ToStorageCondition);
            this.NewOwner = sn.DoEnumNullable(NewOwner);
            this.ItemsToHaulGroup = sn.DoEnum(ItemsToHaulGroup);
            this.item = sn.DoEnumNullable(item);
            this.IsCompleted = sn.DoBool(IsCompleted); // #HAULMANAGERFIX

            if ((uint)version >= 2)
            {
                this.CreatedOn = sn.DoDouble(CreatedOn);
            }
            else
            {
                //this.CreatedOn = The.Sim.TotalUnPausedGameTimeInSeconds;
            }

            

            sn.Ignore(RequiredByProcessJob);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            if (snapshotRequiredByProcessJob.HasValue)
            {
                RequiredByProcessJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotRequiredByProcessJob);
            }

            if ((uint)version < 2)
            {
                this.CreatedOn = The.Sim.TotalUnPausedGameTimeInSeconds;
            }
        }


        #endregion
    }
}
