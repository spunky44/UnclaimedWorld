using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Vehicles
{
    public enum PassengerOrCargoSlotID : ulong 
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

    public class PassengerOrCargoSlot : ILookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>, ISnapshot
    {
        public PassengerOrCargoSlotType PassengerOrCargoSlotType;

        /// <summary>
        /// set to an instance if this slot has room for a passenger
        /// </summary>
        public PassengerSlot PassengerSlot;

        /// <summary>
        /// set to an instance if this slot has room for cargo
        /// </summary>
        public CargoSlot CargoSlot;

        /// <summary>
        /// set to an instance if this slot can be used to drive the vehicle
        /// </summary>
        public DriversSlot DriversSlot;


        private Vehicle parent;
        EntityID snapshotParent;

        public PassengerOrCargoSlot()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        public PassengerOrCargoSlot(Vehicle parent)
        {
            AddToLookup();

            this.parent = parent;
        }

        public void GetEntryPoints(out Vector3 transformedEntry, out Vector3 transformedPointToFace) 
        {

            Vector2? entrance = PassengerOrCargoSlotType.Entrance.Offset; // null;
            Vector2? pointToFace = PassengerOrCargoSlotType.PointToFaceAtEntrance; // null;

         
            // rotate the exit point:
            if (entrance.HasValue)
            {
                parent.ComputeRelativePointInWorld(entrance.Value, out transformedEntry); 
            }
            else
            {
                transformedEntry = parent.Parent.PlaySiteLocation;
              
            }

            if (pointToFace.HasValue)
            {
                parent.ComputeRelativePointInWorld(pointToFace.Value, out transformedPointToFace);
            }
            else
            {
                transformedPointToFace = parent.Parent.PlaySiteLocation;
            }

        }

        public void Destroy()
        {
            RemoveIDEntry();
        }


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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            if (parent != null)
            {
                snapshotParent = parent.Parent.ID;
            }
            snapshotParent = sn.DoEntityID(snapshotParent);

            sn.Ignore(this.CargoSlot); // TODO
            sn.Ignore(this.PassengerSlot); // TODO
            sn.Ignore(PassengerOrCargoSlotType);// TODO


            sn.Ignore(parent);

            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            parent = Entity.FindByID(snapshotParent).Vehicle;
        }

        #endregion


        #region ILookup

        private PassengerOrCargoSlotID id = PassengerOrCargoSlotID.Invalid;
        static PassengerOrCargoSlotID IDCounter = PassengerOrCargoSlotID.First;

        public PassengerOrCargoSlotID ID
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


        public PassengerOrCargoSlotID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= PassengerOrCargoSlotID.Max)
            {
                throw new Exception("Astounding, PassengerOrCargoSlotID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public PassengerOrCargoSlotID SnapshotID(Snapshotter sn, PassengerOrCargoSlotID id)
        {
            return (PassengerOrCargoSlotID)sn.DoEnum(id);
        }

        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != PassengerOrCargoSlotID.Invalid)
            {
                LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.Add(ID, this);

                LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.SetPerformSnapshot(false); // make sure we don't snapshot this collection - it is already stored as a gamedatatype
            }
        }

        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }

        public void RemoveIDEntry()
        {
            LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.Remove(this);
        }

        void ILookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.ResetIDCounter()
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = PassengerOrCargoSlotID.First;
        }

        public void SetInvalid()
        {
            id = PassengerOrCargoSlotID.Invalid;
        }

        void ILookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.Create();
        }

        #endregion

    }
}
