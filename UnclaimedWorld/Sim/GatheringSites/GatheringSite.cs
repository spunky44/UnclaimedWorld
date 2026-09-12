using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame;
using GameStateManagement;
using UWGame.SimSide.Entities.Containers;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;


namespace UWGame.SimSide.GatheringSites
{ 


    public enum GatheringSiteID : ulong
    {
        First = 0L,
        Invalid = uint.MaxValue,
        Max = Invalid
    }


    /// <summary>
    /// GatheringSite is NOT a container type, but is has some rudimentary container like
    /// behaviors. the big difference is that the entities that are visiting are free to come and go
    /// and the PlaceFinder checks to see whether visitor is still there just in time to give
    /// his spot to a new visitor. you might call it a "Weak" container.
    /// </summary>
    public class GatheringSite : IExit, ILookUp<GatheringSite, GatheringSiteID>, ISnapshot
    {
        

        private GatheringSiteType gatheringSiteType;
        GatheringSiteTypeID snapshotType;

        private bool closed = false;

        /// <summary>
        /// The only thing that should make positions dirty is the addition or removal of another GatheringSite 
        /// that overlaps the footprint of this one...or other blocking subtiles being assigned by some other means.
        /// </summary>
        private bool positionsDirty = true;

        private List<VisitorSpot> visitorSpotList;

        private List<VisitorSpot> VisitorSpots
        {
            get
            {
                if (visitorSpotList == null)
                {
                    visitorSpotList = new List<VisitorSpot>();

                }

                if (positionsDirty)
                {
                    RefreshVisitorSpots();
                }

                return visitorSpotList;
            }          
        }


        private Vector3 location;

        /// <summary>
        /// just for optimization - not needed in the logic
        /// </summary>
        bool hasEverAddedVisitor = false;


        public GatheringSite(GatheringSiteType siteType, Vector3 position)
        {
            AddToLookup();

            gatheringSiteType = siteType;


            // don't save the parent entity. we want this still to work after its destruction.
            location = position;

            //parent = newParent;
        }

        public GatheringSite(Entity newParent)
        {
            AddToLookup();
 
            // don't save the parent entity. we want this still to work after its destruction.

            gatheringSiteType = newParent.EntityType.GatheringSiteType;
           
            // set location when placing instead, then it will have a value.
          //  location = newParent.PlaySiteLocation;

        }

        public GatheringSite()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        /// <summary>
        /// gathering site is not designed to be used by moving entities...
        /// </summary>
        /// <param name="location"></param>
        public void SetLocation(Vector3 location)
        {
            this.location = location;
        }

        public int GetNumCurrentVisitors()
        {
            if (hasEverAddedVisitor)
                return 0;

            int count = 0;
            foreach (VisitorSpot v in VisitorSpots)
            {
                if (v.EntityID != EntityID.Invalid)
                    count++;
            }

            return count;

        }

        public bool CanAddVisitor(ref Entity visitor)
        {

            RefreshVisitorSpots();

            //TODO This would be some combination of tests, like isMobile, hasIntelligence, hasLocomotor, isAlive, canInteractWithParent etc.
            //if ( ! visitor.isTypeOfEntityThatCanVisit())
            //    return false;

            return IsDoorAvailable();
        }


        public Vector2? AddVisitor(ref Entity visitingEntity)
        {
            
            if (!CanAddVisitor(ref visitingEntity))
                return null;


            hasEverAddedVisitor = true;



            //TODO the very first action should be to detect whether any visitors have left. This should just
            //check to see whether the entity is not reasonably close to the place they are supposed to be sitting
            //seating is reserved informally, if you get up, we don't save it for you
            UnreserveMissingVisitors();

            //TODO but wait, what if he is already sitting on his seat right now... we should just return the seat he is on

            VisitorSpot you = IsVisitor(visitingEntity.EntityID);
            if (you != null && you.Arrived)
                return you.WorldLocation;
            //just on the off chance that this visitor had made prior reservations
            //let's cancel that reservation, and make a new one, here. this ensures that the 
            //most currently best spot is picked for the visitors current location
            RemoveVisitor(visitingEntity.EntityID);

            RefreshVisitorSpots();

            // offer the first unclaimed point in the list... it is sorted from the middle out
            // TODO this would be smarter if it predicted the angle of approach path
            // biased which point got chosen. chose the top ten(or less) best available points, and then
            // from those, choose the one with the least dot-product to the visitingEntity's current position
            // even better, the point where the path crosses the outer margin, but we don't always have a path

            VisitorSpot toOffer = VisitorSpots.Find(delegate(VisitorSpot v)
            {
                return v.EntityID == EntityID.Invalid;
            });

            if (toOffer == null)
                return null;

            //measure distance from visitingEntity.Location to toOffer.position
            float minDistance = (visitingEntity.PlaySiteLocation.ToVector2() - toOffer.WorldLocation).LengthSquared();

            int countdown = 7;
            foreach (VisitorSpot v in VisitorSpots)
            {
                if (v.EntityID != EntityID.Invalid)
                    continue;
                if (v == toOffer)
                    continue;

                //measure v now, and compare to ToOffer, setting toOffer to v is v is closer
                float dist = (visitingEntity.PlaySiteLocation.ToVector2() - v.WorldLocation).LengthSquared();
                if (dist < minDistance)
                {
                    minDistance = dist;
                    toOffer = v;
                }

                if (--countdown <= 0)
                    break;
            }

            toOffer.EntityID = visitingEntity.EntityID;

            UpdateVisitorPositionsToDraw();//to keep visual feedback fresh

            return toOffer.WorldLocation;
        }


        private void UnreserveMissingVisitors()
        {
            List<EntityID> toRemove = new List<EntityID>();

            foreach (VisitorSpot v in VisitorSpots)
            {
                Entity e = Entity.FindByID(v.EntityID);
                if (e == null)
                {   //apparently, our visitor died recently
                    toRemove.Add(v.EntityID);
                }
                else if (v.Arrived && (e.PlaySiteLocation - v.WorldLocation.ToVector3()).LengthSquared() > 9 * 9)
                {   //hmm, our visitor has gone away since last we checked
                    toRemove.Add(v.EntityID);
                }

            }

            foreach (EntityID id in toRemove)
            {   //<Tweet> out of the pool
                RemoveVisitor(id);
            }

            UpdateVisitorPositionsToDraw();//to keep visual feedback fresh


        }


        public void RemoveVisitor(EntityID visitorID)
        {

            if (!hasEverAddedVisitor)
                return;

            VisitorSpot toRemove = VisitorSpots.Find(delegate(VisitorSpot v) { return v.EntityID == visitorID; });
            if (toRemove != null)
            {
                toRemove.EntityID = EntityID.Invalid;
                toRemove.Arrived = false;
                UpdateVisitorPositionsToDraw();//to keep visual feedback fresh

            }
           
        }

        private VisitorSpot IsVisitor(EntityID visitorID)
        {
            if (!hasEverAddedVisitor)
                return null;
          
            return VisitorSpots.Find(delegate(VisitorSpot v) { return v.EntityID == visitorID; });

        }


        public void RefreshVisitorSpots()
        {
           // DrawVisitorPositions();//yes, we draw here, AND at the bottom of fn.
            //this first time is for the occasional state changes in visitor list


            if (visitorSpotList == null)
            {
                visitorSpotList = new List<VisitorSpot>();
            }
            visitorSpotList.Clear();

            float radius = gatheringSiteType.arc.Radius;
            if (gatheringSiteType.MaxVisitors <= 0)  // don't divide the arc by zero, that's bad
                throw new Exception("zero points or less in a visitor place finder");

            // compute a new array of visit points, uniformly distributed
            Vector2 point = location.ToVector2();

            float minRads = (float)(gatheringSiteType.arc.MinAngle * (Math.PI / 180.0));
            float maxRads = (float)(gatheringSiteType.arc.MaxAngle * (Math.PI / 180.0));

            float breadth = (float)(maxRads - minRads);
            float mCircle = (float)(2 * Math.PI); // a full circle
            float circumference = mCircle * radius;
            float arcLen = circumference * breadth / mCircle;

            float seatSize = gatheringSiteType.SeatSize;//            25f;
            float incrAngle = breadth * seatSize / arcLen; //just enough degrees to reserve a disc of said diameter

            float curAngle = minRads;

            int maxAttempts = gatheringSiteType.MaxVisitors * 2;
            int seatsCreated = 0;

            for (int count = 0; count < maxAttempts; count++)
            {
                //TODO, this assumes Parent.Location is a sensible arc center, maybe use an offset?
                point = location.ToVector2();

                point.X += (float)(Math.Sin(curAngle) * radius);
                point.Y += (float)(Math.Cos(curAngle) * radius);

                // add a randomness to position, so the sorting order is not spiral shaped
                Vector2 shift = point;
                shift -= location.ToVector2();
                shift.Normalize();
                float scale = (float)The.Sim.GameplayRandomGenerator.NextDouble("VisitorPlaceFinder") * (seatSize * 0.25f);
                shift.X *= scale;
                shift.Y *= scale;
                point += shift;


                //////////////////////////////////////
                //prepare the trig for the next point 
                curAngle += incrAngle;
                if (curAngle > maxRads - incrAngle * .5f) // too close to the first seat in this row
                {
                    curAngle -= breadth;//don't cycle past maxRads
                    radius += seatSize;//bigger circle now
                    circumference = mCircle * radius;
                    arcLen = circumference * breadth / mCircle;
                    incrAngle = breadth * seatSize / arcLen; //just enough degrees to reserve a disc of said diameter
                }


                //test whether the testpoint is within the map boundary.
                if (!The.Map.WorldLocationIsOnMap(point)) // #VISITCHANGE
                {
                    continue;
                }


                //test here whether this is atop a completely blocked subtile.
                Point subtilePos = MapManager.WorldPosToSubtile(new Vector3(point.X, point.Y, location.Z));
                if (MapManager.IsBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot].GetValue(subtilePos.X, subtilePos.Y)))
                    continue;

                /*// OLD: tested adjacents too???
                if (The.Map.SubtileIsOrAdjacentToBlockedSubtile(The.Map.TerrainCosts[SurfaceType.TransportType.Foot],
                                        MapManager.WorldPosToSubtile(new Vector3(point.X, point.Y, location.Z))))
                    continue;*/


                //TODO, we can do some more finnicky tests like: entity tile entity
                /*//for blocked subtiles           structure TileLayoutType
                //this is a straight line test
                if (MapManager.IsPathClearToPoint(leadersNextWaypoint.Location, member.GetLocationWhenInVehicle(), memberMoveMap, memberTransport))*/

                VisitorSpot newSeat = new VisitorSpot(EntityID.Invalid, point);

                // measure distance from center
                Vector2 offs = point;
                offs -= location.ToVector2();
                newSeat.DistanceSquared = offs.LengthSquared();

                // append to the list
                visitorSpotList.Add(newSeat);

                if (++seatsCreated >= gatheringSiteType.MaxVisitors)
                    break;


            }

            //sort the list by distance from the first guy's new position
            visitorSpotList.Sort();


            UpdateVisitorPositionsToDraw();


            positionsDirty = false;


        }

        public void Destroy()
        {
            RemoveIDEntry();
        }

        private void UpdateVisitorPositionsToDraw()
        {
            if (!hasEverAddedVisitor)
                return;


#if (DEBUG || PROFILE)


            if (Kensei.Dev.Options.GetOption("Overlays.Markers") == false)
                return;


            MapClient.ClearAllVisitorMarkers(this);

            if (VisitorSpots != null)
            {
                foreach (VisitorSpot v in VisitorSpots)
                {
                    Color color = Color.PaleTurquoise;

                    if (v.Arrived)
                        color = Color.Yellow;
                    else if (v.EntityID != EntityID.Invalid)
                        color = Color.Magenta;


                    MapClient.AddVisitorMarker(new Vector3(v.WorldLocation.X, v.WorldLocation.Y, location.Z), color, this);
                }
            }
#endif
        }




        #region IExit Methods
        public bool IsDoorAvailable()
        {
            //In GatheringSite, doors are open-air, so as long as there is a place to stand, we're open

            //sorry, we're closed
            if (closed)
                return false;

            //sorry, no room for new visitor
            if (GetNumCurrentVisitors() >= gatheringSiteType.MaxVisitors)
                return false;

            return true;
        }

        public ExitDoor ReserveDoorForEntryOrExit(Entity entity, bool exiting)
        {
            //In GatheringSite, doors are open-air, so as long as there is a place to stand, we're open
            return ExitDoor.NoneNeeded;
        }
        public void UseDoor(Entity entity, ExitDoor door, bool exiting)
        {
            //if exiting
            //consult the containertype for constants...
            //compute exit position and orentation using method getExitPosition()
            //dequeue the exiting entity
            //ask the locomotor of entity to teleport to that position/orientation
            //getRallyPoint()
            //push a movetoposition subgoal to the rally point
            //after that subgoal, goalthink takes over
            //else entering
            //suspend the ai (goalthink)
            //push a movetoposition subgoal to the enteroffset
            //set a collision callback to parent container, which will contain entity instantly
        }
        public void UnreserveDoor(ExitDoor door)
        {
            //clear the flag in doors[]  

        }
        public void SetRallyPoint(Vector3 pos, ExitDoor door)
        {
            //dynamically adjust or define a rally point for this door
            //these are assigned in worldspace
            //store new rally point in member doors[], keyed to door ExitDoor value
        }
        public Vector3 GetRallyPoint(ExitDoor door = ExitDoor.NextAvailable)
        {
            //if a dynamic adjustment may have taken place... lookup member array of rally points, to see if one has been assigned
            //use it
            //else if containerType defines constant offset for this door  
            //transform local offset by parent matrix
            //use it
            //else  
            //use getNaturalRallyPoint() (which is already in worldspace)

            //return it


            return location;
        }
        public bool GetNaturalRallyPoint(ref Vector3 rallyPoint, bool offset = true)
        {
            //here we procedurally compute a rally point for the contain structure based on assumptions
            //make an offset in front of the assumed "front" of the structure, distance commensurate with footprint
            //point at least two subtiles away from footprint
            //transform offset by parent matrix
            //adjust based on immovable objects or blocked tiles found here (these cases should have been prevented by the "occupied" tiles in the footprint upon bulding placement.
            //return offset

            rallyPoint.X = rallyPoint.Y = rallyPoint.Z = 0f;
            return false;
        }

    
        public void GetDebugMarkers()
        {
            // no doors, no rallying point..
           // ExitAndEntrance.GetDebugMarkers(this, ref preventRecursion);
        }

        public bool GetDoorPosition(ref Vector3 position, bool exiting, out ExitDoor doorThatWasUsed, ExitDoor door = ExitDoor.NextAvailable)
        //public bool GetDoorPosition(ref Vector3 position, bool exiting, ExitDoor door = ExitDoor.NextAvailable)
        {

            //compute exit position by mult the exit local offset by the parent's transform matrix
            //compute exit orientation by mult the exit normal by parent's transform matrix


            position.X = position.Y = position.Z = 0f;

            doorThatWasUsed = door;
            return false;
        }


        public Vector3 ComputeAccessPoint()
        {
            return GetRallyPoint();
        }

        public bool IsClearToApproach(Entity docker)
        {
            return IsDoorAvailable();
        }

        public bool ReserveApproachPosition(ref Entity docker, ref Vector3 position, out int index)
        {
            ExitDoor door = ReserveDoorForEntryOrExit(docker, false);

            if (door != ExitDoor.NoneAvailable) //valid door reserved
            {
                index = (int)door;
                float angle = 0;
                ExitDoor doorThatWasUsed;
                GetDoorPosition(ref position, false, out doorThatWasUsed, door);
            }

            index = -1;
            return false;
        }


        public bool AdvanceApproachPosition(ref Entity docker, ref Vector3 position, out int index)
        {
            /// Give Entity the next Queue point to move to, and record that that point is taken.
            //In impl, this will consult with IExit ReserveDoor()

            index = -1; // this should look for the next best unreserved door
            return false; //TODO impl
        }


        public bool IsClearToEnter(Entity docker)
        {
            return IsDoorAvailable();
        }

        public bool IsClearToAdvance(Entity docker, int dockerIndex)
        {
            return IsDoorAvailable();
        }

        public void GetEnterPosition(ref Entity docker, ref Vector3 position)
        {
            position = GetRallyPoint();
        }

        public void GetDockPosition(ref Entity docker, ref Vector3 position)
        {


            /// /// /// 

        }

        public void GetDeparturePosition(ref Entity docker, ref Vector3 position)
        {
            position = GetRallyPoint();
        }

        public void OnApproachRallyReached(ref Entity docker)
        {
            /// Entity has reached the Enter rally Point.
        }

        public void OnDockReached(ref Entity d)
        {
            /// Entity has reached the Dock point? Let's prove it! 
            Entity docker = d;
            /// find the VisitorList seat that has this entity reserved
            VisitorSpot seat = VisitorSpots.Find(delegate(VisitorSpot v) { return v.EntityID == docker.EntityID; });
            if (seat != null)
            {

                Vector3 delta = seat.WorldLocation.ToVector3() - docker.PlaySiteLocation;
                float distanceSqr = delta.LengthSquared();
                if (distanceSqr < 2 * 2)
                {
                    seat.Arrived = true;
                    UpdateVisitorPositionsToDraw();
                }
            }

            /// then see whether he is in fact standing in that spot (or very near it)
            /// and if so, set the seat to arrived status
        }

        public void OnDepartureRallyReached(ref Entity docker)
        {
            /// Entity has reached the rally point on his way out.  He is no longer busy
        }

        public bool Action(ref Entity docker)
        {
            /// Perform our specific action on visiting entity.
            /// examples, fill his basket with apples, his tank with fuel, his belly with food...
            /// Returning FALSE means there is nothing for you to do so entity should leave
            return true;
        }


        public void CancelDock(ref Entity docker)
        {
            /// Clear entity from any reserved points, and if entity was the reason we were Busy, we aren't anymore.
        }

        public bool DockOpen
        {
            /// Is the dock open to accepting dockers?
            get
            {
                return true;
            }
        }

        public bool IsAllowedtoDock(ref Entity dockingEntity)
        {
            ///can entity dock here?
            ///this should be a combination of DockOpen, and also applying an entityFilter on dockingEntity
            ///this filter should be defined in COntainerTYpe, but might be hard coded in the interim
            return true;
        }

        public bool UsesRallyPointAfterUndock
        {
            /// A minority of docks want to give you a final command to their rally point. 
            /// this should refer to data in ContainType in many cases
            get
            {
                return true; // for now
            }
        }

        #endregion



        #region ILookup

        private GatheringSiteID id = GatheringSiteID.Invalid;
        static GatheringSiteID IDCounter = GatheringSiteID.First;

        public GatheringSiteID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public GatheringSiteID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= GatheringSiteID.Max)
            {
                throw new Exception("Astounding, GatheringSiteID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public GatheringSiteID SnapshotID(Snapshotter sn, GatheringSiteID id)
        {
            return (GatheringSiteID)sn.DoEnum(id);
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != GatheringSiteID.Invalid)
                LookUp<GatheringSite, GatheringSiteID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = GatheringSiteID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<GatheringSite, GatheringSiteID>.Remove(this);
        }

        void ILookUp<GatheringSite, GatheringSiteID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = GatheringSiteID.First;
        }

        void ILookUp<GatheringSite, GatheringSiteID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<GatheringSite, GatheringSiteID>.Create();
        }


        #endregion


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.id = SnapshotID(sn, id);
            IDCounter = (GatheringSiteID)sn.DoEnum(IDCounter);

            this.closed = sn.DoBool(closed);
            this.snapshotType = (GatheringSiteTypeID)sn.SnapshotID<GatheringSiteType, GatheringSiteTypeID>(gatheringSiteType);            
            this.hasEverAddedVisitor = sn.DoBool(hasEverAddedVisitor);
            this.location = sn.DoVector3(location);
            this.positionsDirty = sn.DoBool(positionsDirty);
            this.visitorSpotList = sn.DoList(visitorSpotList);

           

            return this;
        }

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

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            this.gatheringSiteType = LookUp<GatheringSiteType, GatheringSiteTypeID>.FindByID(snapshotType);

            if (visitorSpotList != null)
            {
                foreach (VisitorSpot v in visitorSpotList)
                {
                    v.LoadPostProcess(sn);
                }
            }
        }

        #endregion
    }
}
