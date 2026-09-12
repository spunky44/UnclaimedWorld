using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Entities;
using Xclna.Xna.Animation;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Buildings;
using GameStateManagement;
using UWGame.SimSide.AI.Goals;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers.Components; 
namespace UWGame.SimSide.Vehicles
{
    public enum Place { Driver, Passenger1, Passenger2, Passenger3, Passenger4}
    public class Vehicle : Component
    {
        // public Entity Parent;//moved to base

        /// <summary>
        /// The roll angle about the direction vector.
        /// Positive = roll to the right
        /// Negative = roll to the left
        /// </summary>
        public float Roll = 0f;


        /// <summary>
        /// The pitch angle.
        /// </summary>
        public float Pitch = 0f;

        // these are used in AI evaluators:
        //  public float BaseSpeed; // = 2f;
        public const float CommonLowestHaulingSpeed = 0.8f; //0.2f
        public const float CommonCarryLimit = 6f;

             
     //   public float Condition = 1f;

     
        public Entity DrivenBy;

        // old... i feel that the lock called TargetedForPickup on Entity can be used instead        
       // public Entity TakenBy;

        public Aircraft Aircraft;

        public List<Entity> Passengers = new List<Entity>();
        public List<Passenger> PassengerItinerary = new List<Passenger>();

        public float CargoCapacityTakenUpByPassengers;
        public List<PassengerOrCargoSlot> Slots = new List<PassengerOrCargoSlot>();

        /// <summary>
        /// used when we are accepting passengers and waiting for the passengers to take their seat:
        /// </summary>
        public int WaitingForPassengers = 0;


        public Vehicle(Entity parent) : base(parent)
        {
            VehicleContainerType vehicleType = (VehicleContainerType)Parent.EntityType.ContainerType;
            if (vehicleType.Aircraft != null)
            {
                this.Aircraft = new Aircraft();
            }

            if (vehicleType.PassengerOrCargoSlotTypes != null)
            {
                foreach (PassengerOrCargoSlotType slot in vehicleType.PassengerOrCargoSlotTypes)
                {
                    PassengerOrCargoSlot thisSlot = new PassengerOrCargoSlot(this) { PassengerOrCargoSlotType = slot };
                    if (slot.CargoSlotType != null)
                    {
                        thisSlot.CargoSlot = new CargoSlot(thisSlot);
                    }

                    if (slot.PassengerSlotType != null){
                        thisSlot.PassengerSlot = new PassengerSlot();

                        if (slot.PassengerSlotType.IsDriversSeat)
                        {
                            thisSlot.DriversSlot = new DriversSlot();
                        }
                    }


                    Slots.Add(thisSlot);
                }
            }

         /*   animatedModel = new StiffAnimatedModel();
            animatedModel.ModelAnimator = new ModelAnimator(UWGame.SimSide.Instance, Parent.EntityType.VehicleType.Model);
            // call Draw() manually:
            animatedModel.ModelAnimator.Visible = false;
            */
          //  CurrentMaximumSpeed = CalculateSpeed(0f);
        }

        public Vehicle()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }
      

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);
            sn.DoUnknownObject(this.Aircraft);
            sn.DoFloat(this.CargoCapacityTakenUpByPassengers);
            Snapshotter.Log("Not snapshotting Vehicle.DrivenBy... use an EntityID", Snapshotter.LogPriority.low);
            //sn.Do(this.DrivenBy);
            sn.DoList(this.PassengerItinerary);
            sn.DoList(this.Passengers);
            sn.DoFloat(this.Pitch);
            sn.DoFloat(this.Roll);
            sn.DoList(this.Slots);
            sn.DoInt32(this.WaitingForPassengers);

            return this;
        }

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


     /*   public virtual void Destroy()
        {   // removes the ModelAnimator from the Game.Components list.
            animatedModel.Destroy();
        }

        public void Draw(Map.GameWorldRenderer.RenderTechnique technique)
        {

            animatedModel.DrawStandard(technique, null, null, null, null);

        }

        public void Draw(Map.GameWorldRenderer.RenderTechnique technique, Matrix view, Matrix projection)
        {
            animatedModel.DrawStandard(technique, view, projection, null, null, null, null);
        }

        /// <summary>
        /// this computes the shadow transformation and draws model using the shadow effect.
        /// </summary>
        public void DrawShadow()
        {

            animatedModel.DrawShadow(Vector3.Zero);

        }*/


     /*   public void ComputeMatricesForDrawing(AnimatedModel.Transformations transformations, float scale)
        {
            Up = -Vector3.UnitZ;

            // Create rotation matrix from rotation amount
            Matrix rotationMatrix =
               Matrix.CreateRotationX(Roll) *
               Matrix.CreateRotationZ(Rotation);


            //  Vector3 X = new Vector3()
            // tilted models:
           

            Vector3 drawDirection = Direction;
            // Rotate orientation vectors
            //   drawDirection = Vector3.TransformNormal(drawDirection, tiltRotationMatrix);


            Up = Vector3.TransformNormal(Up, rotationMatrix);

            // Re-normalize orientation vectors
            // Without this, the matrix transformations may introduce small rounding
            // errors which add up over time and could destabilize the ship.
            drawDirection.Normalize();
            Up.Normalize();

            // Re-calculate Right
            Right = Vector3.Cross(drawDirection, Up);

            animatedModel.ComputeMatricesForDrawing(drawDirection, Up, Right, Parent.EntityType.VehicleType.ModelScale, Parent.Location, Parent.EntityType.VehicleType.ModelOffset, transformations);
        }*/


        


        

        

    /*    public void SetNewMapPosition(Point pos)
        {
            if (MapPosition.X < UWGame.SimSide.Instance.map.TileMap.GetUpperBound(0) &&
                MapPosition.X >= 0 &&
                MapPosition.Y >= 0 &&
                MapPosition.Y < UWGame.SimSide.Instance.map.TileMap.GetUpperBound(1))
            {
                UWGame.SimSide.Instance.map.TileMap[MapPosition.X, MapPosition.Y].RemoveVehicle(this);
            }

            //       MovingToMapPosition = null;
            //       MoveProgress = 0f;
            MapPosition = pos;

            UWGame.SimSide.Instance.map.TileMap[MapPosition.X, MapPosition.Y].AddVehicle(this);

        }*/

        public void TellPassengerToGetOff(Entity sendingEntity, Passenger passenger)
        {
            passenger.Entity.SendMessage(new Message(sendingEntity, Message.MessageTypes.GetOff, null));
            if (PassengerItinerary.Contains(passenger))
            {
                PassengerItinerary.Remove(passenger);
            }
        }

        public void TellPassengerToGetOff(Entity sendingEntity, Entity passenger)
        {
            passenger.SendMessage(new Message(sendingEntity, Message.MessageTypes.GetOff, null));

            int passengerToRemove = -1;
            for (int i = 0; i < PassengerItinerary.Count; i++)
            {
                if (PassengerItinerary[i].Entity == passenger)
                {
                    passengerToRemove = i;
                    break;
                }
            }
            if (passengerToRemove != -1)
            {
                PassengerItinerary.RemoveAt(passengerToRemove);
            }

        }


        private void RecalculateCargoCapacity()
        {
           // float totalCargoCapacity = 0f;

            CargoCapacityTakenUpByPassengers = 0f;

            foreach (PassengerOrCargoSlot slot in Slots)
            {
                if (slot.CargoSlot != null)
                {
                  /*  if ((slot.PassengerSlot == null || (slot.PassengerSlot.Passenger == null && slot.PassengerSlot.TargetedBy == null))
                        && (slot.DriversSlot == null || slot.DriversSlot.Driver == null && slot.DriversSlot.TargetedBy == null ))
                    {
                        totalCargoCapacity += slot.PassengerOrCargoSlotType.CargoSlotType.Capacity;

                    }*/

                    if ((slot.PassengerSlot != null && slot.PassengerSlot.Passenger != null)
                        || (slot.DriversSlot != null && slot.DriversSlot.Driver != null))
                    {
                        CargoCapacityTakenUpByPassengers += slot.PassengerOrCargoSlotType.CargoSlotType.Capacity;

                    }
                }
            }

            if (Parent.AgentStorage.ItemStorage != null)
            {
                Parent.AgentStorage.ItemStorage.RecalculateTotalCapacity();
            }
        }

        public bool EnterVehicleAsPassenger(Entity entity, PassengerOrCargoSlot slot)
        {            
            Passengers.Add(entity);
            entity.PassengerInVehicle = Parent.EntityID;

            slot.PassengerSlot.Passenger = entity;
            slot.PassengerSlot.TargetedBy = null;

            RecalculateCargoCapacity();

            /*
            if (DrivenBy == entity)
            {
                DrivenBy = null;
                entity.DrivingVehicle = null;
                return true;
            }
            else if (Passengers != null && Passengers.Contains(entity))
            {
                Passengers.Remove(entity);
                entity.PassengerInVehicle = null;
                return true;
            }
            */

            return false;
        }

        public bool EnterVehicleAsDriver(Entity entity, PassengerOrCargoSlot slot)
        {
            DrivenBy = entity;
            entity.DrivingVehicle = Parent.EntityID;

            slot.DriversSlot.Driver = entity;
            slot.DriversSlot.TargetedBy = null;

            RecalculateCargoCapacity();

            /*
            if (DrivenBy == entity)
            {
                DrivenBy = null;
                entity.DrivingVehicle = null;
                return true;
            }
            else if (Passengers != null && Passengers.Contains(entity))
            {
                Passengers.Remove(entity);
                entity.PassengerInVehicle = null;
                return true;
            }
            */

            return false;
        }

        public bool ExitVehicle(Entity entity)
        {
            bool isOnBoard = false;
            if (DrivenBy == entity)
            {
                DrivenBy = null;
                entity.DrivingVehicle = null;
                isOnBoard = true;
            }
            else if (Passengers != null && Passengers.Contains(entity))
            {
                Passengers.Remove(entity);
                entity.PassengerInVehicle = null;
                isOnBoard = true;
            }

            if (isOnBoard)
            {
                PassengerOrCargoSlot slot = GetEntityPlaceInVehicle(entity);

                if (slot.PassengerSlot != null)
                {
                    slot.PassengerSlot.Passenger = null;
                    slot.PassengerSlot.TargetedBy = null;
                }

                if (slot.DriversSlot != null)
                {
                    slot.DriversSlot.Driver = null;
                    slot.DriversSlot.TargetedBy = null;
                }

                RecalculateCargoCapacity();
            }

            return isOnBoard;
        }

        public static void DivideVehicles(List<EntityID> vehiclesToDivide, List<EntityGroup> newOwners)
        {
           /* int noOfPortions = newOwners.Count;

            //  float splitDivision = (float)(1 / noOfPortions);

            // use round-robin method to divide items
            int currentTurn = Globals.Instance.RandomPredictable.Next(0, noOfPortions);

            // CHANGE OWNNERSHIP
            int i;

            i = vehiclesToDivide.Count - 1;
            // start removing from the end of the list:
            while (i >= 0)
            {
                vehiclesToDivide[i].ChangeOwnership(newOwners[currentTurn]);
                currentTurn++;
                currentTurn = currentTurn % noOfPortions;
                
                i--;
            }*/
        }

        public PassengerOrCargoSlot GetEntityPlaceInVehicle(Entities.Entity entity)
        {
            foreach (PassengerOrCargoSlot slot in Slots)
            {
                if (slot.PassengerSlot != null && slot.PassengerSlot.Passenger != null && slot.PassengerSlot.Passenger == entity)
                {
                    return slot;
                }
                else if (slot.DriversSlot != null && slot.DriversSlot.Driver != null && slot.DriversSlot.Driver == entity)
                {
                    return slot;
                }
            }
           
            // more???
            return null;
        }

       /* public Place? GetEntityPlaceInVehicle(Entities.Entity entity)
        {
            if (entity.DrivingVehicle != null)
            {
                return Place.Driver;
            }
            else 
            {
                Vehicle vehicleComponent;
                entity.PassengerInVehicle.Find(out vehicleComponent);

                if (Passengers.Count > 0 && entity == vehicleComponent.Passengers[0])
                {
                    return Place.Passenger1;
                }
                else if (Passengers.Count > 1 && entity == vehicleComponent.Passengers[1])
                {
                    return Place.Passenger2;
                }
                else if (Passengers.Count > 2 && entity == vehicleComponent.Passengers[2])
                {
                    return Place.Passenger3;
                }
                else if (Passengers.Count > 3 && entity == vehicleComponent.Passengers[3])
                {
                    return Place.Passenger4;
                }
            }
            // more???
            return null;
        }

        */

        public void GetRendezvousPoint(Vector3 vehicleLocation, Vector3 entityLocation, out Vector3 rendezvousLocation)
        {
            Common.GetLocationAtDistance(vehicleLocation, entityLocation, 60f, out rendezvousLocation);
                
        }

      /*
        public void GetLoadingLocation(out Vector3 location) 
        {
            if (Parent.EntityType.VehicleType.LoadingPoint.HasValue)
            {
                ComputeRelativePointInWorld(Parent.EntityType.VehicleType.LoadingPoint.Value, out location); 
            }
            else
            {
                location = Parent.Location;               
            }
        }
        */

        public bool HasIntelligenceOnboard()
        {
            if (DrivenBy != null)
                return true;
            
            
            foreach (Entity entity in Passengers)
            {
                if (entity.Intelligence != null)
                    return true;
            }

            
            return false;
        }

        public List<PassengerOrCargoSlot> GetCargoSlotsForLoading(float bulkToLoad)
        {
            if (Slots != null)
            {                
                PassengerOrCargoSlot bestSlot = null;
              //  float bestBulk = -1f;

                List<PassengerOrCargoSlot> listOfCargoSlots = null;

                foreach (PassengerOrCargoSlot slot in Slots)
                {
                    // find a cargo slot that is not full and is not being used by passengers
                    if (slot.CargoSlot != null 
                        && !slot.CargoSlot.IsFull()
                        && (slot.PassengerSlot == null || (slot.PassengerSlot.Passenger == null && slot.PassengerSlot.TargetedBy == null))
                        && (slot.DriversSlot == null || (slot.DriversSlot.Driver == null && slot.DriversSlot.TargetedBy == null)))
                    {

                        if (listOfCargoSlots == null)
                        {
                            listOfCargoSlots = new List<PassengerOrCargoSlot>();
                        }

                        listOfCargoSlots.Add(slot);

                      /*  if (slot.CargoSlot.BulkCarried > bestBulk) // we want to fill up the slots if we can...
                        {
                            bestBulk = slot.CargoSlot.BulkCarried;
                            bestSlot = slot;
                        }*/
                    }
                }

                if (listOfCargoSlots != null)
                {
                    listOfCargoSlots.Sort((a, b) => b.CargoSlot.BulkCarried.CompareTo(a.CargoSlot.BulkCarried));

                    float bulkStillToLoad = bulkToLoad;

                    float roomInSlot;

                    List<PassengerOrCargoSlot> cargoSlotsToLoadInto = new List<PassengerOrCargoSlot>();
                    foreach (PassengerOrCargoSlot slot in listOfCargoSlots)
                    {
                        roomInSlot = slot.CargoSlot.GetRemainingRoom();

                        bulkStillToLoad -= roomInSlot;

                        cargoSlotsToLoadInto.Add(slot);

                        if (bulkStillToLoad <= 0f)
                        {
                            break;
                        }
                    }

                    return cargoSlotsToLoadInto;

                }
                else return null;
               
            }

            return null;
        }

        public void Destroy()
        {
            if (Slots != null)
            {
                foreach (var item in Slots)
                {
                    item.Destroy();
                }
            }
        }


        /// <summary>
        /// move to IDock???
        /// </summary>
        /// <param name="bulkToUnload"></param>
        /// <returns></returns>
        public List<PassengerOrCargoSlot> GetCargoSlotsForUnloading(float bulkToUnload)
        {
            if (Slots != null)
            {
                
                List<PassengerOrCargoSlot> listOfCargoSlots = null;

                foreach (PassengerOrCargoSlot slot in Slots)
                {
                    // find a cargo slot that is not empty
                    if (slot.CargoSlot != null
                        && slot.CargoSlot.BulkCarried > 0f)
                    {

                        if (listOfCargoSlots == null)
                        {
                            listOfCargoSlots = new List<PassengerOrCargoSlot>();
                        }

                        listOfCargoSlots.Add(slot);                      
                    }
                }

                if (listOfCargoSlots != null)
                {
                    // sort ascending (excluding 0 slots)
                    listOfCargoSlots.Sort((a, b) => a.CargoSlot.BulkCarried.CompareTo(b.CargoSlot.BulkCarried));

                    float bulkStillToUnload = bulkToUnload;

                    float bulkInSlot;

                    List<PassengerOrCargoSlot> cargoSlotsToUnloadFrom = new List<PassengerOrCargoSlot>();
                    foreach (PassengerOrCargoSlot slot in listOfCargoSlots)
                    {
                        bulkInSlot = slot.CargoSlot.BulkCarried; //.GetRemainingRoom();

                        bulkStillToUnload -= bulkInSlot;

                        cargoSlotsToUnloadFrom.Add(slot);

                        if (bulkStillToUnload <= 0f)
                        {
                            break;
                        }
                    }

                    return cargoSlotsToUnloadFrom;

                }
                else return null;

            }

            return null;
        }

        //TODO vehicle and inventory and storage and visitor --- all these should have a common "contain" parent. MLo

        public void UpdateCargoSlotsWithAttachedModels()
        {
            foreach (PassengerOrCargoSlot slot in Slots)
            {               
                if (slot.CargoSlot != null)
                {                    //TODO  -- this is considered not Sim-Safe, to use a model bone to define a wordspace coordinate of an entity MLo
                                    // the safer way is to signal the client to message these points back to the Entity's vehicle (contain) component
                                    // the best way is to assign the points in the container type initializer.
                   // AttachPoint attachPoint = Parent.EntityType.Renderable.RenderAsModelType.ModelData.GetAttachPointFromKeyName(slot.PassengerOrCargoSlotType.AttachPointName);
                    AttachPoint attachPoint = Parent.Renderable.RenderAsModel.ModelData.GetAttachPointFromKeyName(slot.PassengerOrCargoSlotType.AttachPointName);
                  //  string attachBoneName = attachPoint.AttachBoneName;               
     
                    if (attachPoint != null)
                    {
                        if (slot.CargoSlot.BulkCarried > 0f)
                        {
                            Parent.Renderable.AttachPooledObjectIfPossible("box", attachPoint, AttacheePoint.Bottom, true);

                           
                            // get a box model from the pool and attach it, if none is already attached:
                          /*  if (!AnimatedModel.HasAttachedBoxModel(Parent, attachBoneName))
                            {
                                Vector3? translation = attachPoint.Translation;
                               // float scaling = entity.EntityType.Renderable.RenderAsModelType.ModelScale;
                              
                                string animationToUse = attachPoint.AttachedAnimationName;
                                 
                                Entity box = UWGame.SimSide.Instance.Map.Renderer.GetFreeAttachableEntity("box");

                                Parent.Renderable.RenderAsModel.AttachEntityAndCreateLocalTransform(box, 1f, attachPoint, null, "AttacheeBottom");                                                                

                            }*/
                        
                        }
                        else 
                        {
                            // TODO...
                            Parent.Renderable.RemoveAttachedBoxModelsFromHand(); //Parent, attachPoint);
                        }
                    }

                  /*  if (listOfCargoSlots == null)
                    {
                        listOfCargoSlots = new List<PassengerOrCargoSlot>();
                    }

                    listOfCargoSlots.Add(slot);*/
                }
            }            

        }

       /* public PassengerOrCargoSlot GetCargoSlotForUnloading()
        {
            if (Slots != null)
            {
                PassengerOrCargoSlot bestSlot = null;
                float bestBulk = 100000f;
                foreach (PassengerOrCargoSlot slot in Slots)
                {
                    if (slot.CargoSlot != null)
                    {
                        if (slot.CargoSlot.BulkCarried < bestBulk) // we want to empty the slots if we can...
                        {
                            bestBulk = slot.CargoSlot.BulkCarried;
                            bestSlot = slot;
                        }
                    }
                }

                return bestSlot;
            }

            return null;
        }*/

        /// <summary>
        /// clean up procedure...
        /// </summary>
        public void EmptyAllCargoSlots()
        {
            foreach (PassengerOrCargoSlot slot in Slots)
            {
                if (slot.CargoSlot != null)
                {
                    slot.CargoSlot.BulkCarried = 0f;
                }
            }
        }


        /// <summary>
        /// IExit!!!
        /// </summary>
        /// <returns></returns>
        public PassengerOrCargoSlot GetFreeDriversSlot()
        {
            if (Slots != null)
            {
                float bestComfort = 0;
                PassengerOrCargoSlot bestSlot = null;
                foreach (PassengerOrCargoSlot slot in Slots)
                {
                    if (slot.DriversSlot != null && slot.DriversSlot.Driver == null && slot.DriversSlot.TargetedBy == null)
                    {
                        if (slot.PassengerOrCargoSlotType.PassengerSlotType.Comfort > bestComfort)
                        {
                            bestSlot = slot;
                        }
                    }
                }

                return bestSlot;
            }

            return null;

        }

        public PassengerOrCargoSlot GetPlaceForNewPassenger()
        {

            if (Slots != null)
            {
                float bestComfort = 0;
                PassengerOrCargoSlot bestSlot = null;
                foreach (PassengerOrCargoSlot slot in Slots)
                {
                    if (slot.PassengerSlot != null && slot.PassengerSlot.Passenger == null && slot.PassengerSlot.TargetedBy == null
                        && (slot.DriversSlot == null || (slot.DriversSlot.Driver == null && slot.DriversSlot.TargetedBy == null)) // test that there is no driver in the seat!
                        && (slot.CargoSlot == null || (slot.CargoSlot.BulkCarried == 0f && slot.CargoSlot.TargetedByHauler == null)))// test that it is not being used for cargo!
                    {   
                        if (slot.PassengerOrCargoSlotType.PassengerSlotType.Comfort > bestComfort)
                        {
                            bestSlot = slot;
                        }
                    }
                }

                return bestSlot;
            }

            return null;


           /* switch (Passengers.Count)
            {
                case 0:
                    return Place.Passenger1;
                case 1:
                    return Place.Passenger2;
                case 2:
                    return Place.Passenger3;
                case 3:
                    return Place.Passenger4;

            }

            return null;*/
        }

        /*
        public void GetEntryPoint(Place? place, out Vector3 entryLocation, out Vector3 seatLocation) //out Point tilePos, out Vector2 tileCenterOffset)
        {

            Vector2? modelExitPoint = null;
            Vector2? seat = null;

            switch (place)
            {
                case Place.Driver:
                    modelExitPoint = Parent.EntityType.VehicleType.DriversEntrance;
                    seat = Parent.EntityType.VehicleType.DriversSeat;
                    break;
                case Place.Passenger1:
                    modelExitPoint = Parent.EntityType.VehicleType.PassengersEntrance1;
                    seat = Parent.EntityType.VehicleType.PassengersSeat1;
                    break;
                case Place.Passenger2:
                    modelExitPoint = Parent.EntityType.VehicleType.PassengersEntrance2;
                    seat = Parent.EntityType.VehicleType.PassengersSeat2;
                    break;
                case Place.Passenger3:
                    modelExitPoint = Parent.EntityType.VehicleType.PassengersEntrance3;
                    seat = Parent.EntityType.VehicleType.PassengersSeat3;
                    break;
                case Place.Passenger4:
                    modelExitPoint = Parent.EntityType.VehicleType.PassengersEntrance3;
                    seat = Parent.EntityType.VehicleType.PassengersSeat3;
                    break;
            }

            // rotate the exit point:
            if (modelExitPoint.HasValue)
            {
                ComputeRelativePointInWorld(modelExitPoint.Value, out entryLocation); // out tileCenterOffset, out tilePos);
            }
            else
            {
                entryLocation = Parent.Location;
            

            }

            if (seat.HasValue)
            {
                ComputeRelativePointInWorld(seat.Value, out seatLocation);
            }
            else
            {
                seatLocation = Parent.Location;
            }

        }*/

        public Vector2 GetRelativePointRotated(Vector2? pointOnModel)
        {
           
            if (pointOnModel.HasValue)
            {

                Matrix rotationMatrix =
                    //Matrix.CreateRotationX(Roll) *
                       Matrix.CreateRotationZ(Parent.Rotation);

                Vector2 relativeExitPoint = new Vector2(Parent.PlaySiteLocation.X, Parent.PlaySiteLocation.Y) + Vector2.Transform(pointOnModel.Value, rotationMatrix);

               
                return relativeExitPoint;
            }
            else return new Vector2(Parent.PlaySiteLocation.X, Parent.PlaySiteLocation.Y);
        }



      /*  private void ComputeRelativePointInWorld(Vector2 pointOnModel, out Vector2 transformedTileCenterOffset, out Point transformedTilePos)
        {
            //    Vector2 vehicleRelativePos = MapManager.WorldPosToPositionWithinTile(Location);

            Matrix rotationMatrix =
                   Matrix.CreateRotationX(Roll) *
                   Matrix.CreateRotationZ(Parent.Locomotor.Rotation);

            Vector2 vehicleLocation = new Vector2(Parent.Location.X, Parent.Location.Y);

            Vector2 transformedPoint = vehicleLocation + Vector2.Transform(pointOnModel, rotationMatrix);

            Point vehicleTilePos = Parent.MapPosition; // MapManager.WorldPosToTile(Location);
            transformedTilePos = MapManager.WorldPosToTile(transformedPoint);
            // find out if the point is reachable from the location (terrain foot map)
            if (vehicleTilePos != transformedTilePos)
            {
                // get the tiles
                // STERAIN: get subtiles now!
                List<Point> subtiles = MapManager.GetSubtilesTouchedByLine(new Vector2(Parent.Location.X, Parent.Location.Y), transformedPoint);
                Point previousTile = subtiles[0];

                byte[][] map = UWGame.SimSide.Instance.Map.TerrainCosts[TerrainType.TransportType.Foot];
               
                Point subtile;
                for (int i = 1; i < subtiles.Count; i++)
                {
                    subtile = subtiles[i];
                    // we don't test edges within the same tile:
                    if (subtile != previousTile)
                    {
                        // go from tile to tile, examine all edges to see that they are not blocked                        
                        // tests both connecting edges:
                        if (map[subtile.X][subtile.Y] == 0) //UWGame.SimSide.Instance.Map.GetTerrainCost(TerrainType.TransportType.Foot, previousTile, tiles[i]) == 0)
                        {
                            // unreachable in a straight line at least. Return the vehicle's location instead.
                            //  transformedPoint = vehicleLocation;
                            transformedTileCenterOffset = MapManager.WorldPosToPositionWithinTile(vehicleLocation);
                            transformedTilePos = vehicleTilePos;
                            return;
                        }
                    }

                    previousTile = subtiles[i];
                }
            }

            transformedTileCenterOffset = MapManager.WorldPosToPositionWithinTile(transformedPoint);
            return;
        }*/

        public void ComputeRelativePointInWorld(Vector2 pointOnModel, out Vector3 transformedLocation) 
        {            
            Matrix rotationMatrix =
                   Matrix.CreateRotationX(Roll) *
                   Matrix.CreateRotationZ(Parent.Rotation);

            Vector2 vehicleLocation = Parent.PlaySiteLocation.ToVector2();

            Vector2 transformedPoint = vehicleLocation + Vector2.Transform(pointOnModel, rotationMatrix);

            if (MapManager.IsPointReachableInStraightLine(Parent.PlaySiteLocation, transformedPoint.ToVector3()))
            {
                transformedLocation = transformedPoint.ToVector3();
            }
            else 
            {
                // unreachable in a straight line at least. Return the vehicle's location instead.  
                transformedLocation = Parent.PlaySiteLocation;
            }

        }


        


        /// <summary>
        /// select a good parking spot on the structure area or outside it. Take into account other vehicles.
        /// </summary>
        /// <returns></returns>
        public static bool FindParkingSpot(Point from, Allegiances.Allegiance allegiance, IKnownEntityData vehicle, Rectangle tileArea, SurfaceType.TransportType transport, ProtectionLevel protectionLevel, 
            EntityType driverType, ThreatStance approach, 
            Point targetTile, //Point? targetTile2,
            out Vector3 foundLocation)
            //out Point foundTile, out Vector2 foundOffset)
        {

            
            Point targetTile1;
            Point? targetTile2 = null;

            foundLocation = Vector3.Zero;

            //  foundTile = new Point(-1, -1);
            // foundOffset = Vector2.Zero;


            Entity structure = null;// = UWGame.SimSide.Instance.Map.TileMap[targetTile.X, targetTile.Y].StructureOnTile;
           /* if (The.Map.IsBaseCenterOfWorkingBuilding(targetTile, ref structure)) // structure != null && structure.BaseCenterTile == targetTile)
            {
                targetTile1 = structure.TileLayout.Building.FrontDoorTilePos;
                targetTile2 = structure.TileLayout.Building.BackDoorTilePos;
            }
            else
            {*/
                targetTile1 = targetTile;
           // }

            if (The.Map.IsNoParkingSpot(vehicle.EntityType, protectionLevel, driverType, approach, targetTile))
            {   // bail out if we have previously registered this as a no parking spot area:
                return false;
            }


            MovementMap moveMap = allegiance.SharedKnowledge.GetMovementMap(protectionLevel, driverType, approach);
            SurfaceType.TransportType transportToUse = transport;
            if (transportToUse == SurfaceType.TransportType.Air)
            {
                transportToUse = SurfaceType.TransportType.OffRoad;
            }
            bool[][] isBlockedMap = Structure.CreateIsBlockedMapAvoidConstructions(tileArea, The.Map.TerrainCosts[transportToUse]);

            byte[][] subtileMap = null; // = new byte[isBlockedMap.GetLength(0), isBlockedMap.GetLength(1)];
            Common.InitJaggedArray(ref subtileMap, Common.GetJaggedArrayWidth(isBlockedMap), Common.GetJaggedArrayHeight(isBlockedMap));

            byte passableValue = 40;
            byte unpassableValue = 10;
            int targetCenterValue = 40;
            int targetRadius = 18;

            int width = Common.GetJaggedArrayWidth(subtileMap);
            int height = Common.GetJaggedArrayHeight(subtileMap);

            for (int x = 0; x < width; x++) // subtileMap.GetLength(0);
            {
                for (int y = 0; y < height; y++) //  subtileMap.GetLength(1)
                {
                    subtileMap[x][y] = passableValue;
                }
            }

            CreatePassageMap(vehicle, tileArea, subtileMap, vehicle.BoundingRadius3D, isBlockedMap, unpassableValue);

            // TODO: add discomfort to the map.


            byte[][] passageMap = null;
            if (targetTile2.HasValue)
            {   // make a copy of this map before adding influence from the target.
                passageMap = Common.CloneJaggedArray(subtileMap); // (byte[][])subtileMap.Clone();
            }

            Point targetSubtile2;
            Point targetSubtile1 = new Point((targetTile1.X - tileArea.X) * 3, (targetTile1.Y - tileArea.Y) * 3);
            InfluenceMap.DrawLinearInfluenceCircle(subtileMap, targetSubtile1, targetCenterValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, targetRadius);

            // test the two entrances one after each other. Their influence should not be combined.
            Point bestSubtilePoint = new Point(-1, -1);
            Point bestSubtilePoint1, bestSubtilePoint2;
            int bestTarget1Value, bestTarget2Value;
            bestTarget1Value = InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subtileMap, isBlockedMap, out bestSubtilePoint1);
            if (targetTile2.HasValue)
            {
                // test the other target
                targetSubtile2 = new Point((targetTile2.Value.X - tileArea.X) * 3, (targetTile2.Value.Y - tileArea.Y) * 3);
                InfluenceMap.DrawLinearInfluenceCircle(passageMap, targetSubtile2, targetCenterValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, targetRadius);
                bestTarget2Value = InfluenceMap.GetBestSubtileLocationThatIsntBlocked(passageMap, isBlockedMap, out bestSubtilePoint2);

                if (bestTarget2Value > bestTarget1Value)
                {
                    bestSubtilePoint = bestSubtilePoint2;
                }
                else if (bestTarget1Value > bestTarget2Value)
                {
                    bestSubtilePoint = bestSubtilePoint1;
                }
                else if (bestTarget1Value != -1)
                {
                    // they are equal. pick the closest target?
                    if (Common.DistanceOctile(from, bestSubtilePoint1) < Common.DistanceOctile(from, bestSubtilePoint2))
                    {
                        bestSubtilePoint = bestSubtilePoint1;
                    }
                    else
                    {
                        bestSubtilePoint = bestSubtilePoint2;
                    }
                }
                else
                {
                    // no space found.
                    // register this result:
                    The.Map.RegisterNoParkingSpot(vehicle.EntityType, protectionLevel, driverType, approach, targetTile);
                    return false;
                }
            }
            else
            {
                bestSubtilePoint = bestSubtilePoint1;
            }

            if (bestSubtilePoint.X != -1) //bestTarget1Value > -1)
            {
                // convert to tile pos
                MapManager.SubtileAndTilePosToWorldPos(bestSubtilePoint, new Point(tileArea.Left, tileArea.Top), out foundLocation);  //out foundTile, out foundOffset);

                if (foundLocation.X < 0 || foundLocation.Y < 0)
                {
                    throw new Exception();
                }

                return true;
            }

          
          //  // no space found.
          //  // register this result:
          //  The.Map.RegisterNoParkingSpot(vehicle.EntityType, protectionLevel, threat, approach, targetTile);
            return false;
        }

        public static Rectangle GetSurroundingAreaUsingEntityRadius(IKnownEntityData entity, Point destination)
        {
           // int tileRadius = (int)(GameData.Instance.AIConstants.AreaSizeRadiusForFindingParkingSpot * entity.EntityType.Renderable.RenderAsModelType.BoundingRadius / MapManager.tileSize);
            int tileRadius = (int)(GameData.Instance.AIConstants.AreaSizeRadiusForFindingParkingSpot * entity.BoundingRadius3D / MapManager.tileSize);

            Entity targetBuilding = The.Map.TileMap[destination.X][destination.Y].GetMainBuildingOnTile();  //TiledEntityOnTile;
            Point topLeft, bottomRight;
            if (targetBuilding == null)
            {
                topLeft = The.Map.ClampTileMapPosition(new Point(destination.X - tileRadius, destination.Y - tileRadius));
                bottomRight = The.Map.ClampTileMapPosition(new Point(destination.X + tileRadius, destination.Y + tileRadius));
            }
            else
            {
                topLeft = The.Map.ClampTileMapPosition(new Point(targetBuilding.MapPosition.Value.X - tileRadius, targetBuilding.MapPosition.Value.Y - tileRadius));
                bottomRight = The.Map.ClampTileMapPosition(new Point(targetBuilding.MapPosition.Value.X + targetBuilding.EntityType.StructureType.WidthInTiles + tileRadius,
                    targetBuilding.MapPosition.Value.Y + targetBuilding.EntityType.StructureType.HeightInTiles + tileRadius));
            }            

            return new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);

        }

        /// <summary>
        /// "Grows" an isBlockedMap with negative influence. SubtileMap must be pre-filled.
        /// Also adds negative influence from vehicles and entities according to the radius.
        /// </summary>
        /// <param name="subtileRadius"></param>
        /// <param name="isBlockedMap"></param>
        /// <returns></returns>
        public static void CreatePassageMap(IKnownEntityData entity, Rectangle tileArea, byte[][] subtileMap, /*int agentSubtileRadius,*/ float agentRadius, bool[][] isBlockedMap, byte notPassableValue)
        {
            int agentSubtileRadius = (int)(agentRadius / MapManager.subTileSize);

            int width = Common.GetJaggedArrayWidth(subtileMap);
            int height = Common.GetJaggedArrayHeight(subtileMap);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //tile = new Point(tileArea.X + x / 3, tileArea.Y + y / 3);

                    if (isBlockedMap[x][y])
                    {
                        // draw negative influence around all obstacles:
                        InfluenceMap.DrawLinearInfluenceCircle(subtileMap, new Point(x, y),
                                    notPassableValue, InfluenceMap.Operation.SetValue, InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, agentSubtileRadius);
                    }

                }
            }

            TerrainTile[][] map = The.Map.TileMap;
            TerrainTile tile;

            int combinedRadius;

            Vector3 topLeftOfArea = MapManager.EdgeOfTileToWorldPos(tileArea.Left, tileArea.Top);

            // draw influence from entities
            for (int x = tileArea.X; x < tileArea.X + tileArea.Width; x++)
            {
                for (int y = tileArea.Y; y < tileArea.Y + tileArea.Height; y++)
                {
                    tile = map[x][y];
                    if (tile.EntitiesOnTile != null)
                    {
                        foreach (Entity tileEntity in tile.EntitiesOnTile)
                        {
                            if (tileEntity != entity && tileEntity.ContainedBy == null 
                                && !tileEntity.IsInsideVehicle() // disregard Self and entities aboard other entities
                                && tileEntity.Renderable.RenderAsModel != null) // only for Models...
                            {
                                combinedRadius = (int)((agentRadius + tileEntity.BoundingRadius3D) / MapManager.subTileSize);
                                Point subTilePosOfEntity = MapManager.WorldPosToRelativeSubtile(tileEntity.PlaySiteLocation, topLeftOfArea);
                                InfluenceMap.DrawLinearInfluenceCircle(subtileMap, subTilePosOfEntity,
                                        notPassableValue, InfluenceMap.Operation.SetValue, InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, combinedRadius);
                            }
                        }
                    }
                }
            }


            //return subtileMap;
        }

        
    }

   
}
