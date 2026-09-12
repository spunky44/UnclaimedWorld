using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Map;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    public enum EntranceToUse { Front, Back }
    class GoalEnter: CompositeGoal
    {
        private EntityID entityToEnter;

         public GoalEnter(Entity entity, EntityID entityToEnter )  
            : base(entity)
        {
            this.entityToEnter = entityToEnter;
            
        }


         

        public GoalEnter()
        {
        }


        protected override void Activate()
        {
            IKnownEntityData buildingData = null;
            EntityResult result = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(entityToEnter, out buildingData);
            if (EntityResultCausesFailedGoal(result))
            {
                return;
            }

            Entity container;
            if (!entity.GetContainedBy(out container))
            {
                Status = Goals.Status.Failed;
                return;
            }
            
            Entity buildingEntity = buildingData as Entity;
            if (buildingEntity == null || buildingEntity.Contains == null)//must have a container
            {
                Status = Goals.Status.Failed;
                return;
            }

            if (container == buildingData)
            {
                Status = Status.Completed; //we're already inside
            }
            else 
            {

                Vector3 rallyPoint = buildingEntity.AccessPoint.Value;
                Vector3 doorLocation = rallyPoint;//a tragic, but neccessary default value

                IExit exit = buildingEntity.Contains as IExit;

                System.Diagnostics.Debug.Assert(exit != null, "No door??");

                if (exit != null)
                {
                    ExitDoor doorNumber = exit.ReserveDoorForEntryOrExit(entity, false);

                    if (doorNumber == ExitDoor.NoneNeeded || doorNumber == ExitDoor.NoneAvailable)
                    {   
                        //lets just march on in
                        exit.GetNaturalRallyPoint(ref rallyPoint);
                    }
                    else if (doorNumber < ExitDoor.Max)//a valid door
                    {   
                        //lets approach from the polite direction toward the door
                        rallyPoint = exit.GetRallyPoint(doorNumber);
                    }

                    ExitDoor doorThatWasUsed;
                    exit.GetDoorPosition(ref doorLocation, false, out doorThatWasUsed, doorNumber);

                }

                // perhaps if the rally point is blocked, the entity should be able to go straight to the door...
                // coordinate with evaluator - currently, it checks the access point = rally point for accessibility, not the door...
                // rally point != access point!



                // NEW: does not work if rally point or door is blocked... this can only happen because special actions are allowed to spawn structures inside the pad! (TODO: fix that)
                // the evaluator only checks access point. Access point is never blocked
                // so for now, adding fallback code to prevent AI lockup by entering through access point

                // this code will lock down the AI if there is not a route from acces point to door/rally point...

                Point rallyPointSubtilePos = Maps.MapManager.WorldPosToSubtile(rallyPoint);
                if (!The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[Maps.SurfaceType.TransportType.Foot], rallyPointSubtilePos))
                { 
                    //if we are not already at the rally point
                    if (!IsCloseEnoughForPickupAndWaypoints(rallyPoint, entity.Location.Value))
                    {                     
                        AddSubgoal(new GoalMoveToPosition(entity, rallyPoint, null, GoalMoveToPosition.VehicleUse.NoVehicle)); 
                    }
                }

                Point doorSubtilePos = Maps.MapManager.WorldPosToSubtile(doorLocation);
                if (!The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[Maps.SurfaceType.TransportType.Foot], doorSubtilePos))
                {
                    //if we are not already at the door point
                    if (!IsCloseEnoughForPickupAndWaypoints(doorLocation, entity.PlaySiteLocation))
                    {                         
                        AddSubgoal(new GoalMoveToPosition(entity, doorLocation, null, GoalMoveToPosition.VehicleUse.NoVehicle));
                    }
                }
                else
                {
                    // go to the access point instead if the door is blocked...
                    if (!IsCloseEnoughForPickupAndWaypoints(buildingEntity.AccessPoint.Value, entity.PlaySiteLocation))
                    {                        
                        AddSubgoal(new GoalMoveToPosition(entity, buildingEntity.AccessPoint.Value, null, GoalMoveToPosition.VehicleUse.NoVehicle));
                    }
                }

                Status = Goals.Status.Active;
            }
        }



        private bool IsCloseEnoughForPickupAndWaypoints( Vector3 from, Vector3 to )
        {
            return (Vector3.DistanceSquared(from, to) <= GameData.Instance.Constants.DistanceSquaredLimitForWaypoints);
        }



        protected override void ProcessWhileActive(GameTime elapsed)
        {
           
            Status = ProcessSubgoals(elapsed);

            if (Status != Status.Failed)
            {

                if (Status == Status.Completed)//we should be standing at the door
                {
                    IKnownEntityData buildingData = null;
                    EntityResult result = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(entityToEnter, out buildingData);

                    if (result != EntityResult.SeenDirectly) //EntityResultCausesFailedGoal(result))
                    {
                        Status = Goals.Status.Failed;
                        return; // Status;
                    }

                    Entity buildingEntity = (Entity)buildingData;

                    if (!buildingEntity.Contains.Contains(entity.ID)) // #CONTAINSFIX
                    {
                        buildingEntity.Contains.AddToContain(entity);


                        if (!The.Sim.DateAndTime.SunIsUp)
                        {
                            // turn on some lights...
                            int noOfPeopleInside = 1; //at least
                            IGarrison garrison = buildingEntity.Contains as IGarrison;
                            if (garrison != null)
                                noOfPeopleInside = garrison.GetNoOfAgentsInside();

                            // how full is the building
                            //float peopleShare = MathHelper.Clamp((float)noOfPeopleInside / (float)buildingEntity.EntityType.StructureType.PeopleCapacity, 0f, 1f);
                            float peopleShare = MathHelper.Clamp((float)noOfPeopleInside / (float)buildingEntity.EntityType.ContainerType.GetCapacityForIdlingPeople(), 0f, 1f);

                            // buildingEntity.Renderable.TurnOnDesiredShareOfLights(peopleShare);
                            buildingEntity.TurnOnDesiredShareOfLights(peopleShare);

                        }
                    }

                    Status = Status.Completed;
                }
            }

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

            this.entityToEnter = sn.DoEntityID(entityToEnter);
           

            return this;
        }


        #endregion
    }
}
