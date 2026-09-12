using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using System.Xml.Serialization;
using System.ComponentModel;

namespace UWGame.SimSide.GatheringSites
{
   // [Flags()]
    public enum GatheringSiteTypeID : long // ulong makes XmlSerializer throw exception
        /*Serialization of Enumerations of Unsigned Long
            The XmlSerializer cannot be instantiated to serialize an enumeration if the following conditions are true: The enumeration is of type unsigned long (ulong in C#) and the enumeration 
         * contains any member with a value larger than 9,223,372,036,854,775,807. */
    {
        First = 0L,
        Invalid = long.MaxValue, // ulong.MaxValue,
        Max = Invalid
    }

    /// <summary>
    /// can be defined in the type, but can also get created during runtime
    /// 
    /// Let's snapshot this class. 
    /// For the GatheringSiteType defined in EntityType, there will be many references for each instance, so we need an ID also.
    /// </summary>
    public class GatheringSiteType : ISnapshot, ILookUp<GatheringSiteType, GatheringSiteTypeID>
    {
        public Arc arc;
        public int MaxVisitors = 99;
        public float SeatSize = 25f;

        public GatheringSiteType()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                AddToLookup();   
            }
        }


        #region ILookup

        private GatheringSiteTypeID id = GatheringSiteTypeID.Invalid;
        static GatheringSiteTypeID IDCounter = GatheringSiteTypeID.First;

        [XmlIgnore]
        public GatheringSiteTypeID ID
        {
            get
            {
                return id;
            }

            set // public, because XmlSerializer requires it
            {
                id = value;
            }
        }

        /// <summary>
        /// workaround, for XmlSerializer to serialize ID values which are not named in the enum type.
        /// </summary>
        [XmlElement("ID")]
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public long IDLong
        {
            get { return (long)ID; }
            set { ID = (GatheringSiteTypeID)value; }
        }


        public GatheringSiteTypeID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= GatheringSiteTypeID.Max)
            {
                throw new Exception("Astounding, GatheringSiteTypeID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public GatheringSiteTypeID SnapshotID(Snapshotter sn, GatheringSiteTypeID id)
        {
            return (GatheringSiteTypeID)sn.DoEnum(id);
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
            if (ID != GatheringSiteTypeID.Invalid)
                LookUp<GatheringSiteType, GatheringSiteTypeID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = GatheringSiteTypeID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<GatheringSiteType, GatheringSiteTypeID>.Remove(this);
        }

        void ILookUp<GatheringSiteType, GatheringSiteTypeID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = GatheringSiteTypeID.First;
        }

        void ILookUp<GatheringSiteType, GatheringSiteTypeID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<GatheringSiteType, GatheringSiteTypeID>.Create();
        }

        #endregion

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.id = SnapshotID(sn, id);
            IDCounter = (GatheringSiteTypeID)sn.DoEnum(IDCounter);

            this.arc = (Arc)sn.DoISnapshot(arc);
            this.MaxVisitors = sn.DoInt32(MaxVisitors);
            this.SeatSize = sn.DoFloat(SeatSize);
           

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

            if (arc != null)
                arc.LoadPostProcess(sn);

        }

        #endregion
    }
}
