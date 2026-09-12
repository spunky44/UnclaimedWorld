using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Containers
{
    [DebuggerDisplay("StorageEntity: {StorageEntity}, {StorageID}")]
    public struct StorageTarget
    {       
        public EntityID StorageEntity
        {
            get
            {
                return storageEntity;
            }
        }

        private readonly EntityID storageEntity;

       /// <summary>
       /// this id will give Compartment and StorageConditions when used tgoether with Container Entity
       /// </summary>
        public StorageID StorageID
        {
            get
            {
                return storageID;
            }
        }

        private readonly StorageID storageID;


      /*  public void ResolveStorage(out Storage storage)
        {

             Storage GetToStorage

        }*/

    
        /// <summary>
        /// Use a lookup in the container instead.
        /// 
        /// this gives the answer to what ItemStorage is the target. We need it because Storage does not have a parent pointer to ItemStorage...  we don't cache ItemStorage since Storage objects in MemoryFact don't have one...
        /// </summary>
      /*  public Compartment? Compartment
        {
            get
            {
                return compartment;
            }
        }

        private readonly Compartment? compartment;
        */


        public StorageTarget(EntityID storageEntity, StorageID storageID) //, Compartment? compartment) 
        {
            this.storageID = storageID;
            this.storageEntity = storageEntity;
            //this.compartment = compartment;
          
        }



        public override bool Equals(Object obj)
        {
            return obj is StorageTarget && this == (StorageTarget)obj;
        }

        public override int GetHashCode()
        {
            // ??
            return storageID.GetHashCode() ^ storageEntity.GetHashCode(); //^ storageCondition.GetHashCode() 
        }

        public static bool operator ==(StorageTarget x, StorageTarget y)
        {
            return x.StorageEntity == y.StorageEntity
                && x.StorageID == y.StorageID;
               // && x.compartment == y.compartment;
                //&& x.StorageCondition == y.StorageCondition;
        }

        public static bool operator !=(StorageTarget x, StorageTarget y)
        {
            return !(x == y);
        }
    }
}
