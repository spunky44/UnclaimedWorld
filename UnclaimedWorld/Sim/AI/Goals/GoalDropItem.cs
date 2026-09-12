using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// This goal is used to show an item being placed carefully on the ground.
    /// 
    /// items can also be dropped by simply uncontaining them.
    /// </summary>
    class GoalDropItem: CompositeGoal
    {
        private EntityID itemToDrop;

        
        /// <summary>
        ///  use this to mark the item with the process job that needs the item after dropping it off:
        /// </summary>
        private JobID? assignAsInputToJob;

        private JobID? haulJobID; // #HAULMANAGERFIX

        private Vector3? dropAtLocation;
       // private Vector2 dropAtOffset;

        /// <summary>
        /// the entity that the item should be dropped into:
        /// </summary>
      /*  private EntityID? storageEntity;
        private StorageCondition placeInStorage;
        */
        StorageTarget? placeInStorage;
      //  Compartment? compartment;
       
       // private bool offerForTrade = false;



        private bool dropItHere = false;

        


        private bool firstPickupHalfAnimStateWasSet = false;
        private bool secondPickupHalfAnimStateWasSet = false;

        private AnimAction? actionStateToSetInSecondHalf = null;     
        private List<AnimModifier> statesToSetInSecondHalf = new List<AnimModifier>();
        private float durationOfSecondHalf;

        private bool isHauledItemDestination;

        public GoalDropItem()
        {
        }

        public GoalDropItem(Entity owner, EntityID item, ProcessJob assignToJob = null) //, StorageTarget? placeInStorage) //StorageCondition placeInStorage)
            : base(owner)
        {
            Init(item, assignToJob, null);

            dropItHere = true;
        }

        public GoalDropItem(Entity owner, EntityID item, ProcessJob assignAsInputToJob, Vector3? dropAtLocation, StorageTarget? placeInStorage, bool isHauledItemDestination = false, JobID? haulJob = null)
            : base(owner)
        {
            Init(item, assignAsInputToJob, placeInStorage);

            this.dropAtLocation = dropAtLocation;
            this.isHauledItemDestination = isHauledItemDestination;
            this.haulJobID = haulJob;

            if (dropAtLocation == null && placeInStorage == null) // storageEntity == null)
            {
                dropItHere = true;
            }
        }

        private void Init(EntityID item, ProcessJob assignToJob, StorageTarget? placeInStorage) //StorageCondition placeInStorage)
        {
            this.itemToDrop = item;
            this.assignAsInputToJob = assignToJob != null ? assignToJob.ID : (JobID?)null;
            this.placeInStorage = placeInStorage;

            /*
            IKnownEntityData entityData;
            entityIntelligence.GetKnownData(item, out entityData);*/
        }

       


        protected override void Activate()
        {
            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            Entity item;
            if (EntityIsNotSeenDirectly(itemToDrop, out item))
            {
                return;
            }

          
            // similar logic to GoalPickup
            if (entity.AgentStorage.Contains(item))
            {
                if (!dropItHere)
                {

                    Entity insideBuilding;
                    if (entity.GetContainedBy(out insideBuilding))
                    {
                        if (insideBuilding != null)//We are inside an building.
                        {
                            if (placeInStorage.HasValue) // storageEntity.HasValue)
                            {   // we are in a different building - fail.
                                if (insideBuilding.EntityID != placeInStorage.Value.StorageEntity) // storageEntity.Value)
                                {
                                    AddSubgoal(new GoalExit(entity));
                                }
                            }
                            else
                            {
                                AddSubgoal(new GoalExit(entity));
                            }
                        }

                    }
                    else
                    {
                        Status = Goals.Status.Failed;
                        return;
                    }


                    if (placeInStorage.HasValue) // storageEntity != null) 
                    {                      

                        if (insideBuilding == null || insideBuilding.EntityID != placeInStorage.Value.StorageEntity) // storageEntity.Value)
                        {
                            IKnownEntityData storageEntityData;
                            EntityResult result = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(placeInStorage.Value.StorageEntity, out storageEntityData);

                            if (!EntityResultCausesFailedGoal(result))
                            {
                                AddSubgoal(new GoalMoveToPosition(entity,
                                    storageEntityData.AccessPoint.Value,  //storageEntityData.Location, // entrance location instead???
                                    null, GoalMoveToPosition.VehicleUse.NoVehicle,
                                    placeInStorage.Value.StorageEntity, //storageEntity, 
                                    false));
                            }
                            else return;

                        }
                        
                        // else we are ok. we are inside the target building.
                    }
                    else
                    {
                        // the drop target is on the ground.
                      /*  if (entity.MapPosition != dropAtTile || !GoalTraverseEdgeBetweenWaypoints.IsAtWaypoint(entity.Location, dropAtLocation.Value)) 
                        { 
                        */
                            //Walk to drop point.    
                        AddSubgoal(new GoalMoveToPosition(entity, dropAtLocation.Value, null, GoalMoveToPosition.VehicleUse.NoVehicle)
                            {
                                 PermittedDistanceSquaredToDestination = (float) Math.Pow((double)GameData.Instance.Constants.InteractionDistanceForAgents, 2d)
                            }); //, dropAtOffset));
                           
                        // turn and look at drop location:
                        AddSubgoal(new GoalTurnToFace(entity, dropAtLocation.Value.ToVector2()));

                      //  }

                    }
                }
                else
                {
                    SetFirstPartAnimationState(item); // play drop anim

                }

                Status = Status.Active;

             //   EnterBuildingIfNecessary();

              //  Status = Status.Active;
                
            }
            else
            {
                // does not have the item. Fail.
                Status = Status.Failed;
            }

            
        }

        /*
        private void SetAnimationStateGoal()
        {
          

            AnimState? burdenStateFlag = Renderable.GetBurdenAnimStateFlag(entity.AgentStorage.GetStoredPercentageOfCapacity()); //.TotalStored / entity.AgentStorage.TotalItemStorageCapacity);
                
            // play whole or half anim?
              //  AddSubgoal(new GoalWait(entity, 0.4, AnimState.Hauling, AnimState.Post)); //, true));
            if (burdenStateFlag.HasValue)
            {
                AddSubgoal(new GoalWait(entity, 0.5, AnimState.Dropping, burdenStateFlag.Value)); //, true));
            }
            else
            {
                AddSubgoal(new GoalWait(entity, 0.5, AnimState.Dropping)); //, true));
            }

        }
        */
      


        /// <summary>
        /// the part that runs until the moment when the agent lifts the item
        /// </summary>
        /// <param name="itemToDropData"></param>
        private void SetFirstPartAnimationState(IKnownEntityData itemToDropData)
        {

            AgentStorage.BurdenState currentBurdenState = entity.AgentStorage.GetCurrentBurdenState();
            AgentStorage.BurdenState burdenStateAfterDrop = GetBurdenStateAfterDrop(currentBurdenState, itemToDropData);

            float duration;

            // transitions are explained here:
            // https://drive.google.com/a/unclaimedworld-game.com/?usp=chrome_app&usp=chrome_app#folders/0B2Kxcx1AEC5nN2xLSk41RWY4SFk

            bool scaleAnimToFitWaitTime = false; // this probably won't work with scaling...

            // most of these anims look strange if the head is looking up and away
            bool canTurnHead = false;


            // TEST
           // currentBurdenState = AgentStorage.BurdenState.HaulLight;
          //  burdenStateAfterDrop = AgentStorage.BurdenState.HaulLight;

            IntelligenceType intel = entity.EntityType.IntelligenceType;

            switch (currentBurdenState)
            {
                case AgentStorage.BurdenState.Mounted:

                    if (burdenStateAfterDrop == AgentStorage.BurdenState.Mounted)
                    {
                        duration = intel.dropMountToMountActionPointDuration;

                        // TODO: use special anim here!!!
                        AddSubgoal(new GoalWait(entity, duration, AnimAction.Dropping, AnimModifier.Mount, AnimModifier.Same, scaleAnimToFitWaitTime, GoalWait.OnExitFlagAction.Leave)
                            {
                               HeadTurnAllowed = canTurnHead
                            }); // whole anim

                        durationOfSecondHalf = intel.dropMountToMountDuration - intel.dropMountToMountActionPointDuration;
                    }
                    else
                    {
                        duration = intel.dropMountedActionPointDuration;

                        AddSubgoal(new GoalWait(entity, duration, AnimAction.Dropping, AnimModifier.Mount, scaleAnimToFitWaitTime, GoalWait.OnExitFlagAction.Leave)
                        {
                            HeadTurnAllowed = canTurnHead
                        }); // whole anim

                        durationOfSecondHalf = intel.dropMountedDuration - intel.dropMountedActionPointDuration;    
                    }

                    break;
                case AgentStorage.BurdenState.None:
                case AgentStorage.BurdenState.Equipped:

                    // no change...
                    duration = intel.dropEquippedActionPointDuration;

                    AddSubgoal(new GoalWait(entity, duration, AnimAction.Dropping, AnimModifier.Equip, scaleAnimToFitWaitTime, GoalWait.OnExitFlagAction.Leave)
                        {
                               HeadTurnAllowed = canTurnHead
                            }); // whole anim

                    durationOfSecondHalf = intel.dropEquippedDuration - intel.dropEquippedActionPointDuration; 

                    break;                  


                case AgentStorage.BurdenState.HaulLight:
                    switch (burdenStateAfterDrop)
                    {
                        case AgentStorage.BurdenState.None:
                        case AgentStorage.BurdenState.Equipped:
                            duration = intel.DropLightActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.Dropping, scaleAnimToFitWaitTime, GoalWait.OnExitFlagAction.Leave)
                            {
                               HeadTurnAllowed = canTurnHead
                            }); // whole anim

                            durationOfSecondHalf = intel.DropLightDuration - intel.DropLightActionPointDuration;
                            break;

                        case AgentStorage.BurdenState.HaulLight:
                            duration = intel.DropLightToLightActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.Dropping, AnimModifier.Same, scaleAnimToFitWaitTime, GoalWait.OnExitFlagAction.Leave)
                            {
                               HeadTurnAllowed = canTurnHead
                            }); // whole anim

                            durationOfSecondHalf = intel.DropLightToLightDuration - intel.DropLightToLightActionPointDuration;

                            break;                       
                    }
                    break;
                case AgentStorage.BurdenState.HaulHeavy:
                    switch (burdenStateAfterDrop)
                    {
                        case AgentStorage.BurdenState.None:
                        case AgentStorage.BurdenState.Equipped:
                            duration = intel.DropHeavyActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.Dropping, AnimModifier.Heavy, scaleAnimToFitWaitTime, GoalWait.OnExitFlagAction.Leave)
                                {
                               HeadTurnAllowed = canTurnHead
                            }); // whole anim

                            durationOfSecondHalf = intel.DropHeavyDuration - intel.DropHeavyActionPointDuration;
                            break;

                        case AgentStorage.BurdenState.HaulLight:
                            duration = intel.DropHeavyActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.Dropping, AnimModifier.Heavy, scaleAnimToFitWaitTime, GoalWait.OnExitFlagAction.Clear)
                                {
                               HeadTurnAllowed = canTurnHead
                            }); // half anim
                                                       
                            durationOfSecondHalf = intel.PickupLightDuration - intel.PickupLightActionPointDuration;//This calculation was changed recently, is it correct?/Finn
                            //durationOfSecondHalf = GoalPickup.PickupLightActionPointDuration - GoalPickup.PickupLightActionPointDuration;

                            actionStateToSetInSecondHalf = AnimAction.PickingUp;
                            statesToSetInSecondHalf.Add(AnimModifier.Post);
                            break;    
                        case AgentStorage.BurdenState.HaulHeavy:
                            duration = intel.DropHeavyActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.Dropping, AnimModifier.Heavy, scaleAnimToFitWaitTime, GoalWait.OnExitFlagAction.Clear)
                                {
                                    HeadTurnAllowed = canTurnHead
                                });  // half anim

                            // second half is the stand-up part of heavy pick up:
                            durationOfSecondHalf = intel.PickupHeavyDuration - intel.PickupHeavyActionPointDuration;

                            actionStateToSetInSecondHalf = AnimAction.PickingUp;
                            statesToSetInSecondHalf.Add(AnimModifier.Heavy);
                            statesToSetInSecondHalf.Add(AnimModifier.Post);
                            break;
                    }

                    break;               
            }


            firstPickupHalfAnimStateWasSet = true;
        }

        private AgentStorage.BurdenState GetBurdenStateAfterDrop(AgentStorage.BurdenState currentState, IKnownEntityData itemToDropData)
        {
            if (entity.AgentStorage.MountedToolOrWeapon == itemToDrop)
            {
                // dropping a mounted item:
                return AgentStorage.BurdenState.None;
            }
            else
            {
                // if we are currently hauling, after drop we may not be hauling
                if (currentState == AgentStorage.BurdenState.HaulHeavy) 
                {
                    return GetHaulBurdenStateAfterDrop(entity, itemToDropData);
                }
                else if (currentState == AgentStorage.BurdenState.HaulLight)
                {
                    return GetHaulBurdenStateAfterDrop(entity, itemToDropData);
                }
                else
                { // if we are not hauling now. 
                    // no change

                    if (entity.AgentStorage.MountedToolOrWeapon.HasValue)
                    {
                        return AgentStorage.BurdenState.Mounted;
                    }
                    else
                    {
                       
                        return AgentStorage.BurdenState.None;
                    }
                }
            }
        }

        public static AgentStorage.BurdenState GetHaulBurdenStateAfterDrop(Entity entity, IKnownEntityData itemToDropData)
        {
            // are we still hauling after dropping this?
            if (entity.AgentStorage.RecomputeIsHauling(exceptItem: itemToDropData)) 
            {
                // get the new status:
                float newStoredPercentage = entity.AgentStorage.GetHaulingPercentageOfCapacity(entity.AgentStorage.TotalStored - itemToDropData.Bulk);
//                float newStoredPercentage = entity.AgentStorage.TotalStored - itemToDropData.Bulk / entity.AgentStorage.TotalItemStorageCapacity;

                return AgentStorage.GetHaulBurdenState(newStoredPercentage);
            }
            else
            {
                return AgentStorage.BurdenState.None;
            }
        }

        public static AgentStorage.BurdenState GetHaulBurdenStateAfterPickup(Entity entity, IKnownEntityData itemToPickupData)
        {           
            if (entity.AgentStorage.RecomputeIsHauling(withItem: itemToPickupData))
            {
                // get the new status:
                float newStoredPercentage = entity.AgentStorage.GetHaulingPercentageOfCapacity(entity.AgentStorage.TotalStored + itemToPickupData.Bulk);

                return AgentStorage.GetHaulBurdenState(newStoredPercentage);
            }
            else
            {
                return AgentStorage.BurdenState.None;
            }
        }

        


        /// <summary>
        /// the part that runs after the moment when the agent lifts the item
        /// </summary>
        /// <param name="itemToPickUpData"></param>
        private void SetSecondPartAnimationState(IKnownEntityData itemToPickUpData)
        {
            // most of these anims look strange if the head is looking up and away
            bool canTurnHead = false;

            if (statesToSetInSecondHalf.Count > 0)
            {
                // play a different animation (or half?) for the last part.
                AddSubgoal(new GoalWait(entity, durationOfSecondHalf, actionStateToSetInSecondHalf, statesToSetInSecondHalf, false)
                    {
                        HeadTurnAllowed = canTurnHead
                    });

            }
            else if (durationOfSecondHalf > 0f)
            {
                // let the current animation continue till it ends.
                AddSubgoal(new GoalWait(entity, durationOfSecondHalf, false)
                {
                    HeadTurnAllowed = canTurnHead
                });
            }

            secondPickupHalfAnimStateWasSet = true;
        }



        /// <summary>
        /// Always called BEFORE the OnEnter of a next subgoal of the same parent
        /// </summary>
        public override void OnExit()
        {
            base.OnExit();

            entity.Renderable.ClearAnimationActionStateFlag(AnimAction.PickingUp);
            entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Dropping);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Pre);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Post);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Heavy);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Mount);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Equip);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Same);
        }


        protected override void ProcessWhileActive(GameTime elapsed)
        {

            Status = ProcessSubgoals(elapsed);

            if (Status == Status.Completed)
            {
                Entity item;
                if (EntityIsNotSeenDirectly(itemToDrop, out item))
                {                   
                    return; // Status;
                }

                Entity placeInStorageEntity = null;
                Storage storage = null;
                if (placeInStorage.HasValue) // storageEntity.HasValue)
                {
                    if (EntityIsNotSeenDirectly(placeInStorage.Value.StorageEntity /* storageEntity.Value*/, out placeInStorageEntity))
                    {                       
                        return; // Status;
                    }

                    storage = ((IStorage)placeInStorageEntity.Contains).FindStorage(placeInStorage.Value.StorageID);

                   // storage = LookUp<Storage, StorageID>.FindByID(placeInStorage.Value.StorageID);
                    // also test storage for null here?? what about memory facts??

                }

                // did we animate bending down?
                if (!firstPickupHalfAnimStateWasSet)
                {
                    SetFirstPartAnimationState(item);
                    Status = Goals.Status.Active;
                    return; // Status;
                }
                else if (!secondPickupHalfAnimStateWasSet)
                {
                    // drop the item now:
                    Vector3? placeOnGround = null;
                    if (dropAtLocation.HasValue)
                    {
                        // precaution to avoid teleporting items to distant locations:
                        if (Common.DistanceOctile(entity.PlaySiteLocation, dropAtLocation.Value) < GameData.Instance.Constants.InteractionDistanceForAgents)
                        {
                            placeOnGround = dropAtLocation.Value;
                        }
                    }
                    
                    if (entity.Contains.Uncontain(item, 
                                                    storageTarget: placeInStorage, 
                                                    placeInStorageEntity: placeInStorageEntity, 
                                                    placeOnGround: dropAtLocation)) //, isOfferedForSale: offerForTrade))
                    {

                        // "Give" the item to the job that needs it:
                        if (assignAsInputToJob != null)
                        {
                            Job job = LookUp<Job, JobID>.FindByID(assignAsInputToJob);
                            if (job != null)
                            {
                                ((ProcessJob)job).AssignInput(item);
                            }
                        }

                        if (haulJobID != null) // #HAULMANAGERFIX
                        {
                            Job job = LookUp<Job, JobID>.FindByID(haulJobID);
                            if (job != null)
                            {
                                // we want to destroy the job immediately, otherwise the haul manager may clean up the job and cancel the other haul goals.
                                // but we cannot destroy the job here, because the parent goal has a reference to it, and it would break snapshot...
                                // instead, set a flag so HaulingManager will ignore the job in cleanup...
                                ((HaulingJob)job).IsCompleted = true;
                            }
                        }

                        Status = Status.Active;
                    }
                    else
                    {
                        Status = Status.Failed;

                        // ExitIfFailedOrCompleted();
                        return; // Status;
                    }

                    if (isHauledItemDestination)
                    {
                        entityIntelligence.Memory.ResetHauledItem(item.EntityID);
                    }

                    // run the last part of the anim
                    SetSecondPartAnimationState(item);

                    // box gets removed by AgentStorage!                  

                }
                else
                {
                    Status = Goals.Status.Completed;
                }
            }

        }

        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), itemToDrop);          
        }


        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.itemToDrop = sn.DoEntityID(itemToDrop);
            this.assignAsInputToJob = sn.DoEnumNullable(assignAsInputToJob);          
            this.dropAtLocation = sn.DoVector3Nullable(dropAtLocation);
            this.placeInStorage = sn.DoStorageTargetNullable(placeInStorage);
            this.dropItHere = sn.DoBool(dropItHere);
            this.firstPickupHalfAnimStateWasSet = sn.DoBool(firstPickupHalfAnimStateWasSet);
            this.secondPickupHalfAnimStateWasSet = sn.DoBool(secondPickupHalfAnimStateWasSet);
            this.actionStateToSetInSecondHalf = sn.DoEnumNullable(actionStateToSetInSecondHalf);
            this.statesToSetInSecondHalf = (List<AnimModifier>)sn.DoList(statesToSetInSecondHalf);
            this.durationOfSecondHalf = sn.DoFloat(durationOfSecondHalf);           

            this.isHauledItemDestination = sn.DoBool(isHauledItemDestination); // #HAULAI

            this.haulJobID = sn.DoEnumNullable(haulJobID); // #HAULMANAGERFIX
        
            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

           /* if (snapshotAssignToJob.HasValue)
            {
                assignToJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotAssignToJob);
            }*/

        }

        #endregion
    }
}
