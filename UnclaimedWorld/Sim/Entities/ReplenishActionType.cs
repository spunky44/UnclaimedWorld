using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Snapshots;
using System.Xml.Serialization;
using System.ComponentModel;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities
{
    public enum ReplenishActionTypeID : long // ulong
    { 
        /*Serialization of Enumerations of Unsigned Long
            The XmlSerializer cannot be instantiated to serialize an enumeration if the following conditions are true: The enumeration is of type unsigned long (ulong in C#) and the enumeration 
         * contains any member with a value larger than 9,223,372,036,854,775,807. */
        Invalid = long.MaxValue, // ulong.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// This type class is special, it is given an ID, however the collection is not snapshotted... instead we rely on game data loading in the same sequence and assigning the same IDs each time.
    /// </summary>
    public class ReplenishActionType : ILookUp<ReplenishActionType, ReplenishActionTypeID>
    {
        /// <summary>
        /// this is used as the target compartment for the action. Should be refactored to be sort of like output Substance in output/actedOnEntity.
        /// </summary>
        public GoalReplenish.ReplenishAction ReplenishAction;


        //public AgentAction AgentAction;

        public string AgentAction;

        [XmlIgnore]
        public ProcessType AgentActionType;

        public ReplenishActionType()
        {
            if (!Snapshotter.IsSnapshotting) // done both when loading from xml and when instantiating directly in GameDataLoaders!
            {
                /*
                 * 1. load from xml/instantiate in GameDataLoader - creates IDs
                 * 2. load objects from save file with referencing IDs
                 * 
                 * Same???
                 * This will only work if the sequence is exactly the same, so the IDs will get assigned the same values each time the game data types are loaded. 
                // And of course it will break if there are changes to the data types.
                 * 
                 * // make sure we don't snapshot this collection - it is already stored as a gamedatatype
                 * 
                 */
                             

                AddToLookup(); // this also sets a flag not to snapshot the collection.
                
            }
        }

        public void PostLoadContentInitialize()
        {
            if (AgentAction != null)
            {
                AgentActionType = GameData.Instance.AllProcessTypes[AgentAction];
            }

        }


        #region ILookup

        private ReplenishActionTypeID id = ReplenishActionTypeID.Invalid;

        /// <summary>
        /// We don't need to snapshot this.
        /// </summary>
        static ReplenishActionTypeID IDCounter = ReplenishActionTypeID.First;

        [XmlIgnore]
        public ReplenishActionTypeID ID
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
            set { ID = (ReplenishActionTypeID)value; }
        }

        public ReplenishActionTypeID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ReplenishActionTypeID.Max)
            {
                throw new Exception("Astounding, ReplenishActionTypeID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public ReplenishActionTypeID SnapshotID(Snapshotter sn, ReplenishActionTypeID id)
        {
            // never gets called.
            return (ReplenishActionTypeID)sn.DoEnum(id);
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
            if (ID != ReplenishActionTypeID.Invalid)
            {
                LookUp<ReplenishActionType, ReplenishActionTypeID>.Add(ID, this);

                LookUp<ReplenishActionType, ReplenishActionTypeID>.SetPerformSnapshot(false); // make sure we don't snapshot this collection - it is already stored as a gamedatatype
            }
        }

        public void RemoveIDEntry()
        {
            LookUp<ReplenishActionType, ReplenishActionTypeID>.Remove(this);
        }

        void ILookUp<ReplenishActionType, ReplenishActionTypeID>.ResetIDCounter()
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = ReplenishActionTypeID.First;
        }

        void ILookUp<ReplenishActionType, ReplenishActionTypeID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ReplenishActionType, ReplenishActionTypeID>.Create();
        }


        public void SetInvalid()
        {
            id = ReplenishActionTypeID.Invalid;
        }

        #endregion
    }
}
