using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Containers;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    ///  GoalPickup can only pickup an item that is on the ground, or is inside a container (building) that we can enter??? / are in???.
    ///  if the item is inside a structure that we cannot enter physically, then GoalUnload should be performed first.
    ///  
    /// use MoveToDestination if vehicle use should be allowed on the way to the pickup.
    /// 
    /// the goal will handle buildings and final move adjustments on foot.
    /// 
    /// 
    /// regarding animations - we use pick up in the following situations:
    /// pick up to haul item/tool somewhere else (where perhaps the tool will be mounted)
    /// pick up to mount a weapon immediately - play pickup_equip
    /// 
    /// </summary>
    class GoalPickup : CompositeGoal
    {
        private EntityID itemToPickUp;

        /// <summary>
        /// if filled, if the owner cannot be resolved, the goal fails
        /// </summary>
        public OwnerID? NewOwner;


        StorageCompartment placeInCompartment;
        bool mountItemAfterPickup;
        bool bendDown;
        bool standUpAfterwards;


       

        private bool firstPickupHalfAnimStateWasSet = false;
        private bool secondPickupHalfAnimStateWasSet = false;

        //private Entity.BurdenState burdenStateAfterPickup;

        private AnimAction? actionStateToSetInSecondHalf = null;
        private List<AnimModifier> modifierStatesToSetInSecondHalf = new List<AnimModifier>();

        private float durationOfSecondHalf;


       // private bool addBoxAfterPickup = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="item"></param>
        /// <param name="newOwner">Can be null</param>
        public GoalPickup(Entity owner, EntityID item, OwnerID? newOwner, StorageCompartment? placeInCompartment = StorageCompartment.Haul, bool? mountItemAfterPickup = false, bool bendDown = true, bool standUpAfterwards = true)
            : base(owner)
        {
            itemToPickUp = item;
            NewOwner = newOwner;


            this.placeInCompartment = placeInCompartment.Value;
            this.mountItemAfterPickup = mountItemAfterPickup.Value;
            this.bendDown = bendDown;
            this.standUpAfterwards = standUpAfterwards;
        }



        public GoalPickup()
        {
        }


        protected override void Activate()
        {
            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            IKnownEntityData itemToPickUpData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(itemToPickUp, out itemToPickUpData)))
            {
                return;
            }

        
            // first check if we are already carrying this item:
            if (entity.AgentStorage != null && entity.AgentStorage.Contains(itemToPickUp)) //entity.ItemStorage != null && entity.ItemStorage.StoredItems.Contains(itemToPickUp))
            {
                Status = Status.Active; //(Completed)
            }
            else if (entity.AgentStorage != null && entity.AgentStorage.GetCompartment(placeInCompartment).HasCapacityForItem(itemToPickUpData.Bulk))
            {


                Entity insideBuilding;
                if (entity.GetContainedBy(out insideBuilding))
                {
                    if (insideBuilding != null)//We are inside an building.
                    {

                        if (itemToPickUpData.ContainedBy.HasValue)
                        {   // we are in a different building - fail.
                            if (insideBuilding.EntityID != itemToPickUpData.ContainedBy.Value)
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


                if (itemToPickUpData.ContainedBy != null) //itemToPickUp.InsideBuilding != null)
                {
                    IKnownEntityData storageEntityData;
                    EntityResult result = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(itemToPickUpData.ContainedBy.Value, out storageEntityData);

                    if (!storageEntityData.EntityType.ContainerType.AllowedInContainer(entity.EntityType))
                    {
                        // we cannot enter
                        Status = Goals.Status.Failed;
                        return;
                    }

                    if (!EntityResultCausesFailedGoal(result))
                    {


                        if (insideBuilding == null || insideBuilding.EntityID != itemToPickUpData.ContainedBy.Value)
                        {
                            // walk over land to the item container
                            AddSubgoal(new GoalMoveToPosition(entity,
                                storageEntityData.AccessPoint.Value, null,
                                GoalMoveToPosition.VehicleUse.NoVehicle,
                                storageEntityData.EntityID));

                        }
                        else
                        {
                            // pick up... uncontain???
                            if (bendDown)
                            {
                                SetFirstPartAnimationState(itemToPickUpData);
                            }
                        }
                    }

                    // else we are ok. we are inside the same building as the item to pick up.
                }
                else
                {
                    // the item is on the ground.
                    float maxDistanceSquared = (float)Math.Pow((double)GameData.Instance.Constants.InteractionDistanceForAgents, 2d);
                    if (Vector3.DistanceSquared(entity.PlaySiteLocation, itemToPickUpData.PlaySiteLocation) > maxDistanceSquared)
                    {
                        //Walk to item if not within reach.           
                        AddSubgoal(new GoalMoveToPosition(entity, itemToPickUpData.PlaySiteLocation, null,
                            GoalMoveToPosition.VehicleUse.NoVehicle)
                            {
                                PermittedDistanceSquaredToDestination = maxDistanceSquared
                            });

                        AddSubgoal(new GoalTurnToFace(entity, itemToPickUpData.PlaySiteLocation.ToVector2()));
                    }
                    else
                    {
                        // then pick up...
                        if (bendDown)
                        {
                            SetFirstPartAnimationState(itemToPickUpData);
                        }
                    }

                }

                Status = Status.Active;
            }
            else
            {
                // cannot get the item. Fail.
                Status = Status.Failed;
            }

        }

                
        protected override bool ArePreconditionsOK()
        {
            // same as GoalHaul...
            // add more preconditions: item not destroyed, assigned to / carried by / onboard someone else etc?

            IKnownEntityData data;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(itemToPickUp, out data)))
            {
                return false;
            }

            if (!data.CanBeHauled())
                return false;

            // are we still the owner of the item:
            // should we check Contains in an EntityGroup here? If so, we need an ID
            /*   if (data.OwnedBy != ownerOfItem) 
                   return false;*/

            return true;
        }



        /// <summary>
        /// the part that runs until the moment when the agent lifts the item
        /// </summary>
        /// <param name="itemToPickUpData"></param>
        private void SetFirstPartAnimationState(IKnownEntityData itemToPickUpData)
        {
            //man_pickupHaul 10 0,36
            //man_pickupEquip 44 1,72

            AgentStorage.BurdenState currentBurdenState = entity.AgentStorage.GetCurrentBurdenState();
            AgentStorage.BurdenState burdenStateAfterPickup = GetBurdenStateAfterPickup(currentBurdenState, itemToPickUpData);

            float duration;

            // transitions are explained here:
            // https://drive.google.com/a/unclaimedworld-game.com/?usp=chrome_app&usp=chrome_app#folders/0B2Kxcx1AEC5nN2xLSk41RWY4SFk

            bool scaleAnimToWaitDuration = false;

            // most of these anims look strange if the head is looking up and away
            bool canTurnHead = false;

            // TEST of broken anim:
            //scaleAnimToWaitDuration = true;

            /*    duration = PickupHeavyActionPointDuration;

                AddSubgoal(new GoalWait(entity, duration, AnimAction.PickingUp, AnimModifier.Heavy, scaleAnimToWaitDuration, GoalWait.OnExitFlagAction.Leave)); // whole anim
             //   AddSubgoal(new GoalWait(entity, duration, AnimAction.PickingUp, AnimModifier.Heavy, scaleAnimToWaitDuration, GoalWait.OnExitFlagAction.Clear)); // whole anim

                durationOfSecondHalf = PickupHeavyDuration - PickupHeavyActionPointDuration;
            
                duration = switchLightToLightActionPointDuration;

                AddSubgoal(new GoalWait(entity, duration, AnimAction.PickingUp, AnimModifier.Same, false, GoalWait.OnExitFlagAction.Leave)); // whole anim

                durationOfSecondHalf = switchLightToLightDuration - switchLightToLightActionPointDuration;
                // TEST
                firstPickupHalfAnimStateWasSet = true;
                return;*/

            IntelligenceType intel = entity.EntityType.IntelligenceType;
                      

            switch (currentBurdenState)
            {
                case AgentStorage.BurdenState.None:
                case AgentStorage.BurdenState.Equipped:

                    switch (burdenStateAfterPickup)
                    {
                        case AgentStorage.BurdenState.HaulLight:
                            duration = intel.PickupLightActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.PickingUp, scaleAnimToWaitDuration, GoalWait.OnExitFlagAction.Leave)
                                {
                                    HeadTurnAllowed = canTurnHead
                                }); // whole anim

                            durationOfSecondHalf = intel.PickupLightDuration - intel.PickupLightActionPointDuration;

                            break;
                        case AgentStorage.BurdenState.HaulHeavy:
                            duration = intel.PickupHeavyActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.PickingUp, AnimModifier.Heavy, scaleAnimToWaitDuration, GoalWait.OnExitFlagAction.Leave)
                                {
                                    HeadTurnAllowed = canTurnHead
                                }); // whole anim

                            durationOfSecondHalf = intel.PickupHeavyDuration - intel.PickupHeavyActionPointDuration;

                            //addBoxAfterPickup = true;

                            break;
                        case AgentStorage.BurdenState.Mounted:
                            duration = intel.pickupMountActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.PickingUp, AnimModifier.Mount, scaleAnimToWaitDuration, GoalWait.OnExitFlagAction.Leave)
                                {
                                    HeadTurnAllowed = canTurnHead
                                }); // whole anim

                            durationOfSecondHalf = intel.pickupMountDuration - intel.pickupMountActionPointDuration;

                            break;
                        case AgentStorage.BurdenState.Equipped:
                        case AgentStorage.BurdenState.None:
                            duration = intel.pickupEquipActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.PickingUp, AnimModifier.Equip, scaleAnimToWaitDuration, GoalWait.OnExitFlagAction.Leave)
                                {
                                    HeadTurnAllowed = canTurnHead
                                }); // whole anim

                            durationOfSecondHalf = intel.pickupEquipDuration - intel.pickupEquipActionPointDuration;

                            break;
                    }

                    break;
                case AgentStorage.BurdenState.HaulLight:
                    switch (burdenStateAfterPickup)
                    {
                        case AgentStorage.BurdenState.HaulLight:
                            duration = intel.SwitchLightToLightActionPointDuration;

                            // this anim is broken for some reason..
                            // scaling the anim duration to the wait progress seems to help?
                            scaleAnimToWaitDuration = true;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.PickingUp, AnimModifier.Same, scaleAnimToWaitDuration, GoalWait.OnExitFlagAction.Leave)
                                {
                                    HeadTurnAllowed = canTurnHead
                                }); // whole anim

                            durationOfSecondHalf = intel.SwitchLightToLightDuration - intel.SwitchLightToLightActionPointDuration;

                            break;
                        case AgentStorage.BurdenState.HaulHeavy:
                            duration = intel.DropLightActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.Dropping, scaleAnimToWaitDuration)
                                {
                                    HeadTurnAllowed = canTurnHead
                                });  // half anim

                            // second half is the stand-up part of heavy pick up:
                            durationOfSecondHalf = intel.PickupHeavyDuration - intel.PickupHeavyActionPointDuration;
                            actionStateToSetInSecondHalf = AnimAction.PickingUp;
                            modifierStatesToSetInSecondHalf.Add(AnimModifier.Heavy);
                            modifierStatesToSetInSecondHalf.Add(AnimModifier.Post);

                            //addBoxAfterPickup = true;

                            break;
                    }
                    break;
                case AgentStorage.BurdenState.HaulHeavy:
                    switch (burdenStateAfterPickup)
                    {
                        case AgentStorage.BurdenState.HaulHeavy:
                            duration = intel.DropHeavyActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.Dropping, AnimModifier.Heavy, scaleAnimToWaitDuration, GoalWait.OnExitFlagAction.Clear)
                                {
                                    HeadTurnAllowed = canTurnHead
                                });  // half anim

                            // second half is the stand-up part of heavy pick up:
                            durationOfSecondHalf = intel.PickupHeavyDuration - intel.PickupHeavyActionPointDuration;
                            actionStateToSetInSecondHalf = AnimAction.PickingUp;
                            modifierStatesToSetInSecondHalf.Add(AnimModifier.Heavy);
                            modifierStatesToSetInSecondHalf.Add(AnimModifier.Post);

                            break;
                    }

                    break;
                case AgentStorage.BurdenState.Mounted:
                    switch (burdenStateAfterPickup)
                    {
                        case AgentStorage.BurdenState.None:
                        case AgentStorage.BurdenState.Mounted:
                        case AgentStorage.BurdenState.Equipped:

                            duration = intel.pickupMountedEquippedActionPointDuration;

                            AddSubgoal(new GoalWait(entity, duration, AnimAction.PickingUp, AnimModifier.Mount, AnimModifier.Equip, scaleAnimToWaitDuration, GoalWait.OnExitFlagAction.Leave)
                                {
                                    HeadTurnAllowed = canTurnHead
                                }); // whole anim

                            durationOfSecondHalf = intel.pickupMountedEquippedDuration - intel.pickupMountedEquippedActionPointDuration;

                            break;
                    }
                    break;
                //case Entity.BurdenState.Equipped:
                //switch()
            }


            firstPickupHalfAnimStateWasSet = true;
        }



        /// <summary>
        /// the part that runs after the moment when the agent lifts the item
        /// </summary>
        /// <param name="itemToPickUpData"></param>
        private void SetSecondPartAnimationState(IKnownEntityData itemToPickUpData)
        {
            if (actionStateToSetInSecondHalf.HasValue || modifierStatesToSetInSecondHalf.Count > 0)
            {
                // play a different animation for the last part.
                AddSubgoal(new GoalWait(entity, durationOfSecondHalf, actionStateToSetInSecondHalf, modifierStatesToSetInSecondHalf, false));

            }
            else if (durationOfSecondHalf > 0f)
            {
                // let the current animation continue till it ends.
                AddSubgoal(new GoalWait(entity, durationOfSecondHalf, false));
            }

            secondPickupHalfAnimStateWasSet = true;
        }



        /// <summary>
        /// similar to GoalDropItem
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="itemToPickUpData"></param>
        /// <returns></returns>
        private AgentStorage.BurdenState GetBurdenStateAfterPickup(AgentStorage.BurdenState currentState, IKnownEntityData itemToPickUpData)
        {
            if (mountItemAfterPickup)
            {
                return AgentStorage.BurdenState.Mounted;
            }
            else
            {
                // if we are currently hauling, after pickup we will still be hauling
                if (currentState == AgentStorage.BurdenState.HaulHeavy) // entity.IsHauling())
                {
                    return AgentStorage.BurdenState.HaulHeavy;
                }
                else if (currentState == AgentStorage.BurdenState.HaulLight)
                {
                    return GoalDropItem.GetHaulBurdenStateAfterPickup(entity, itemToPickUpData); //return GetNewHaulBurdenState(itemToPickUpData);

                }
                else
                {
                    AgentStorage.BurdenState newState = GoalDropItem.GetHaulBurdenStateAfterPickup(entity, itemToPickUpData);

                    if (newState == AgentStorage.BurdenState.None)
                    {
                        if (placeInCompartment == StorageCompartment.Equipment)
                        {
                            return AgentStorage.BurdenState.Equipped;
                        }

                        if (currentState == AgentStorage.BurdenState.Equipped)
                        {
                            return AgentStorage.BurdenState.Equipped;
                        }
                        else
                        {
                            return AgentStorage.BurdenState.None;
                        }
                    }
                    else
                    {
                        return newState;
                    }

                    /*
                    // if we are not hauling now
                    Job assignedToJob = EvaluateJob.ResolveAssignedToJob(itemToPickUpData);
                    
                    if (assignedToJob is HaulingJob)
                    {
                        // hauling after pickup.
                        return GetNewHaulBurdenState(itemToPickUpData);
                    }
                    else
                    {
                        // not hauling after pickup.

                        if (placeInCompartment == StorageCompartment.Equipment)
                        {
                            return AgentStorage.BurdenState.Equipped;
                        }

                        if (currentState == AgentStorage.BurdenState.Equipped)
                        {
                            return AgentStorage.BurdenState.Equipped;
                        }
                        else
                        {
                            return AgentStorage.BurdenState.None;
                        }
                    }*/
                }
            }
        }

        private AgentStorage.BurdenState GetNewHaulBurdenState(IKnownEntityData itemToPickUpData)
        {
           // float newStoredPercentage = entity.AgentStorage.TotalStored + itemToPickUpData.Bulk / entity.AgentStorage.TotalItemStorageCapacity;
            float newStoredPercentage = entity.AgentStorage.GetHaulingPercentageOfCapacity(entity.AgentStorage.TotalStored + itemToPickUpData.Bulk);
                        
            return AgentStorage.GetHaulBurdenState(newStoredPercentage);

            /*
            if (newStoredPercentage > GameData.Instance.Constants.StoredPercentageMeansHeavyHaul)
            {
                return AgentStorage.BurdenState.HaulHeavy;
            }
            else
            {
                return AgentStorage.BurdenState.HaulLight;
            }*/
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
                         
            if (!preconditionsRegulator.IsReady() ||
               ArePreconditionsOK())
            {

                Status = ProcessSubgoals(elapsed);

                if (Status == Status.Completed)
                {
                    Entity item;
                    if (EntityIsNotSeenDirectly(itemToPickUp, out item))
                    {
                        //  ExitIfFailedOrCompleted();
                        return; // return Status;
                    }

                    //if (Common.DistanceOctile(entity.PlaySiteLocation, item.PlaySiteLocation) > 30f)// distance check... don't care if we are not completely within reach here. We have done the fine movement we need.
                    if (Common.DistanceOctile(entity.AccessPoint.Value, item.AccessPoint.Value) > 30f)// the above didn't work with items in huts?
                    {
                        Status = Goals.Status.Failed;

                        //   ExitIfFailedOrCompleted();
                        return; //return Status;
                    }

                    // did we animate bending down?
                    if (!firstPickupHalfAnimStateWasSet)
                    {
                        if (bendDown)
                        {
                            SetFirstPartAnimationState(item);
                        }
                        else
                        {
                            firstPickupHalfAnimStateWasSet = true;
                        }

                        Status = Goals.Status.Active;
                        return; //return Status;
                    }
                    else if (!secondPickupHalfAnimStateWasSet)
                    {
                        // do the pickup, then run the last part of the anim (standing up):
                        if (entity.AgentStorage.GetCompartment(placeInCompartment).StoredItems.Contains(itemToPickUp))
                        {
                            // we had it all along...
                            //Status = Status.Completed;
                        }
                        else
                        {
                            IOwner newOwner = LookUpOwners.FindByID(NewOwner);
                            if (item.Item.Pickup(entity, newOwner, placeInCompartment))
                            {
                                // we now have the item:  
                                if (mountItemAfterPickup)
                                {
                                    entity.AgentStorage.MountedToolOrWeapon = item.EntityID;
                                }

                                //Status = Status.Completed;
                            }
                            else
                            {
                                Status = Status.Failed;

                                return; //return Status;
                            }
                        }

                        // run the last part of the anim
                        if (standUpAfterwards)
                        {
                            SetSecondPartAnimationState(item);
                        }
                        else
                        {
                            secondPickupHalfAnimStateWasSet = true;
                        }

                       // BoxHandlingWhenHauling boxHandling = entity.EntityType.RenderableType.BoxHandlingWhenHauling;
                      
                        // show the big box now if needed:                         
                        entity.Renderable.UpdateAttachBoxModelToHand(/*addBoxAfterPickup, boxHandling*/); // if this could be scrapped, then boxes could become Temporary Attachables. But Temporary attachables have some bugs... the brief translation bug, and brief idle states cause the attachables to dissappear briefly
                       

                        Status = Goals.Status.Active;
                        return; //return Status;
                    }
                    else
                    {
                        // see if we now have the item:
                        if (entity.AgentStorage.Contains(itemToPickUp))
                        {
                            Status = Goals.Status.Completed;
                        }
                        else
                        {
                            Status = Goals.Status.Failed;
                        }

                    }

                }
            }
            else
            {
                Status = Goals.Status.Failed;
            }
        }


        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), itemToPickUp);
          
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

            this.itemToPickUp = sn.DoEntityID(itemToPickUp);
            this.NewOwner = sn.DoEnumNullable(NewOwner);
            this.placeInCompartment = sn.DoEnum(placeInCompartment);
            this.mountItemAfterPickup = sn.DoBool(mountItemAfterPickup);
            this.bendDown = sn.DoBool(bendDown);
            this.standUpAfterwards = sn.DoBool(standUpAfterwards);
            this.firstPickupHalfAnimStateWasSet = sn.DoBool(firstPickupHalfAnimStateWasSet);
            this.secondPickupHalfAnimStateWasSet = sn.DoBool(secondPickupHalfAnimStateWasSet);
            this.actionStateToSetInSecondHalf = sn.DoEnumNullable(actionStateToSetInSecondHalf);
            this.modifierStatesToSetInSecondHalf = sn.DoList(modifierStatesToSetInSecondHalf);
            this.durationOfSecondHalf = sn.DoFloat(durationOfSecondHalf);
           // this.addBoxAfterPickup = sn.DoBool(addBoxAfterPickup);

            return this;
        }

        #endregion
    }
}
