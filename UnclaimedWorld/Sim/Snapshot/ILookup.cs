using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace UWGame.SimSide.Snapshots
{
    /// <summary>
    /// Use this interface if the class should be included in a collection and be able to be referenced by an ID.
    /// This is very useful for snapshotting. The collection will be snapshotted implicitly, by default, but this can be turned off. 
    /// 
    /// Returns null if the entry has been removed from the collection.
    /// 
    /// Implementing checklist:
    /// - call AddToLookUp() in ctor (NOT in the empty ctor!)
    /// - call RemoveIDEntry in Destroy()
    /// - snapshot id and counter in DoSnapshot:
    ///  IDCounter = (FoodExtractionID)sn.DoEnum(IDCounter);
    ///  id = SnapshotID(sn, id);
    /// 
    /// </summary>
    public interface ILookUp<T,Id> 
        where T : ILookUp<T,Id>
        where Id : struct  // cannot specify enum - this allows nullable operations on Id enums
    {
        /// <summary>
        /// when implementing, add a private setter
        /// </summary>
        Id ID { get; }

        /// <summary>
        /// implement and call from AddToLookup
        /// </summary>
        /// <returns></returns>
        Id GetUniqueID();   

       
        /// <summary>
        /// call this from the constructor - but never when snapshotting!!!
        /// </summary>
        void AddToLookup();

        /// <summary>
        /// Make sure to call this in Destroy() to clean up properly
        /// </summary>
        void RemoveIDEntry();

        /// <summary>
        /// called by invoke, do not remove
        /// </summary>
        void ResetIDCounter();


        /// <summary>
        /// NEW: static col instance is no longer lazily inited in the static ctor, but must be created explicitly on startup
        /// </summary>
        void CreateLookupCollection();

        /// <summary>
        /// called from Lookup after Remove() to make it apparent that the instance is now invalid.
        /// </summary>
        void SetInvalid();

        /// <summary>
        /// NOTE: there is also a similar member on Collections...
        /// </summary>       
        int LoadPostProcessOrder { get; }
    }
     


}
