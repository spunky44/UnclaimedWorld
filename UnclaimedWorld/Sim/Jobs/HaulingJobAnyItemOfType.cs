using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;
namespace UWGame.SimSide.Jobs
{
    [DebuggerDisplay("{RequiredItemType.KeyName} {ToLocation} {ToStorage}")]
    public class HaulingJobAnyItemOfType : HaulingJob, ISnapshot
    {     
        
        public EntityType RequiredItemType;

      //  public Owner OwnerOfItemsToHaul;
        
      
        /// <summary>
        /// 
        /// </summary>    
        /// <param name="toTileCenterOffset"></param>      
        /// <param name="requiredItemType"></param>
        /// <param name="ownerOfItemsToHaul"></param>
        public HaulingJobAnyItemOfType(Vector3 toLocation, EntityGroup entityGroup, /*Priority priority,*/ EntityType requiredItemType, EntityGroupID ownerOfItemsToHaul, OwnerID? newOwner)
            : base(toLocation, null, false, entityGroup, newOwner, /*priority,*/ false)
        {
            RequiredItemType = requiredItemType;

            // add to group now (this depends on RequiredItemType):
            entityGroup.AddJob(this);

            ItemsToHaulGroup = ownerOfItemsToHaul;

            ComputeJobType();
            SetDefaultPriority(entityGroup);
        }

        public HaulingJobAnyItemOfType()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

       
        public override string ToString()
        {
            StringBuilder b = new StringBuilder("Haul ");            
            if (Item != null)
            {
                b.Append(Item.ToString());
            }
            else
            {
                b.Append(RequiredItemType.Name);
            }
            b.Append(" To: ");
            GetToLocationAsString(b);

            return b.ToString();
        }

        public override void Destroy(bool cancelTakers, Entity entityToExcludeFromCancel = null)
        {
            if (base.RequiredByProcessJob != null)
            {
               /* if (base.RequiredByProcessJob.ID != JobID.Invalid && Item.HasValue && !RequiredByProcessJob.AssertInputIsAssigned(Item))
                {
                    System.Diagnostics.Debug.Assert(false, "Input not assigned when hauling job was destroyed?");
                }*/


                RequiredByProcessJob.AddLog("Destroyed HaulingJobAnyItemOfType: " + RequiredItemType.KeyName + ", JobID: " + ID + 
                    " , ItemID: " + (Item.HasValue? Item.Value.ToString() : "" ));
            }


            base.Destroy(cancelTakers, entityToExcludeFromCancel);

           
        }

        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);
            this.RequiredItemType = sn.DoGameData(RequiredItemType);

            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);



        }


        #endregion
    }
}
