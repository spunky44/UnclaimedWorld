using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Processes
{
    public enum ToolTypeCombinationID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// during game data load/init, we assign an ID and store in a look up collection for Goals to use. The collection is not snapshotted, however.
    /// 
    /// This means that any changes to ToolTypeCombination data will break save games!
    /// </summary>
    public class ToolTypeCombination : ILookUp<ToolTypeCombination, ToolTypeCombinationID>
    {
        /// <summary>
        /// the number is DegradePerSecond
        /// </summary>
        public List<Tuple<EntityType, float>> Tools;

        /// <summary>
        /// combined (average) productivity of these tools when applied to a certain process
        ///       
        /// </summary>
        public float Productivity;



        public ToolTypeCombination()
        {
            if (!Snapshotter.IsSnapshotting) // done both when loading from xml and when instantiating directly in GameDataLoaders!
            {
                /* Load snapshot without gamedata:
                 * 1. load from xml/instantiate in GameDataLoader - creates IDs, dictionary, increases IDCounter to what it was when the snapshot was made
                 * 2. load objects from save file with referencing IDs
                 *              
                 * Load with gamedata:
                 * 2. load objects from save file with referencing IDs - the objects are already in a dictionary, the IDCounter is up-to-date
                 * 
                 * This will only work if the sequence is exactly the same, so the IDs will get assigned the same values each time the game data types are loaded. 
                // And of course it will break if there are changes to the data types.
                 * 
                 * // make sure we don't snapshot this collection - it is already stored as a gamedatatype
                 * 
                 */

                AddToLookup(); // this also sets a flag not to snapshot the collection.
            }
        }


      /*  public static float ComputeProductivity()
        {
            listOfTools.Average(t => t.Productivity) // productivity is computed as average of all tools in set
        }*/

        #region ILookup

        private ToolTypeCombinationID id = ToolTypeCombinationID.Invalid;

        /// <summary>
        /// We don't need to snapshot this.
        /// </summary>
        static ToolTypeCombinationID IDCounter = ToolTypeCombinationID.First;

        public ToolTypeCombinationID ID
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


        public ToolTypeCombinationID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ToolTypeCombinationID.Max)
            {
                throw new Exception("Astounding, ToolTypeCombinationID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public ToolTypeCombinationID SnapshotID(Snapshotter sn, ToolTypeCombinationID id)
        {
            // never gets called.
            return (ToolTypeCombinationID)sn.DoEnum(id);
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
            if (ID != ToolTypeCombinationID.Invalid)
            {
                LookUp<ToolTypeCombination, ToolTypeCombinationID>.Add(ID, this);

                LookUp<ToolTypeCombination, ToolTypeCombinationID>.SetPerformSnapshot(false); // make sure we don't snapshot this collection - it is already stored as a gamedatatype
            }
        }

        public void RemoveIDEntry()
        {
            LookUp<ToolTypeCombination, ToolTypeCombinationID>.Remove(this);
        }

        void ILookUp<ToolTypeCombination, ToolTypeCombinationID>.ResetIDCounter()
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = ToolTypeCombinationID.First;
        }

        void ILookUp<ToolTypeCombination, ToolTypeCombinationID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ToolTypeCombination, ToolTypeCombinationID>.Create();
        }

        public void SetInvalid()
        {
            id = ToolTypeCombinationID.Invalid;
        }


        public static ToolTypeCombination FindByID(ToolTypeCombinationID id)
        {
            if (id == ToolTypeCombinationID.Invalid)
                return null;

            return LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(id);
        }

        #endregion
    }
}
