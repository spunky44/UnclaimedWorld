using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using System.Diagnostics;
namespace UWGame.SimSide.Jobs
{
    [DebuggerDisplay("{Item} {ToLocation} {ToStorage}")]
    public class HaulingJobSpecificItem: HaulingJob
    {             
       
        /// <summary>
        /// if this is true, the job is not that important and may be overriden if the item has a better use.
        /// Also true for trade offered items.
        /// </summary>
        public bool IsHaulJobToStorage;

      //  private bool isHaulJobToOfferForTrade;

     
        public HaulingJobSpecificItem()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        /// <summary>
        /// for now, we cannot haul items without an Owner - but this should be changed to allow critters to haul items too.
        /// </summary>
        /// <param name="toLocation"></param>
        /// <param name="storage"></param>
        /// <param name="storageEntity"></param>
        /// <param name="entityGroup"></param>
        /// <param name="priority"></param>
        /// <param name="specificItem"></param>
        /// <param name="newOwner"></param>
        /// <param name="haulToStorage"></param>
        public HaulingJobSpecificItem(Vector3? toLocation,
            StorageTarget? toStorage,
            EntityGroup entityGroup,
            IKnownEntityData specificItem, OwnerID? newOwner, bool haulToStorage, bool haulToTrade, EntityGroup itemsGroup)
            : base(toLocation, 
                    toStorage, haulToTrade,      
                    entityGroup, newOwner, false) // true)
        {

          
            specificItem.AssignedToJob = this.ID;
            Item = specificItem.EntityID;

            // add to group now (this depends Item)
            entityGroup.AddJob(this);

            IsHaulJobToStorage = haulToStorage;
         
            ItemsToHaulGroup = itemsGroup.ID;  // Owner must not be null...


            ComputeJobType(); // cannot be move to base ctor since we need the IsHaulJobToStorage to be set first.
            SetDefaultPriority(entityGroup);
        }


      /*  public override bool IsOfferedForTrade
        {
            get
            {
                return isHaulJobToOfferForTrade; // isOfferedForTrade;
            }
        }*/

        public override string ToString()
        {
            StringBuilder b = new StringBuilder("Haul ");

            if (Item.HasValue)
            {
                Entity item = Entity.FindByID(Item.Value);

                if (item != null)
                {
                    b.Append(item.EntityType.ItemType.Name ?? item.EntityType.ItemType.KeyName);
                    b.Append(" To: ");
                    GetToLocationAsString(b);

                    return b.ToString();
                }
            }

            return base.ToString();
        }



        #region ISnapshot

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.IsHaulJobToStorage = sn.DoBool(IsHaulJobToStorage);
           // this.isHaulJobToOfferForTrade = sn.DoBool(isHaulJobToOfferForTrade);


            return this;
        }


        #endregion


    }
}
