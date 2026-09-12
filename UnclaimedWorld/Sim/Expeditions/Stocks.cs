using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Expeditions
{
    public class ProductionOrders: ISnapshot
    {
        /// <summary>
        /// contains an entry for all item types
        /// </summary>
        public Dictionary<EntityType, ProductionOrder> Orders = new Dictionary<EntityType, ProductionOrder>();

        /// <summary>
        /// carcass orders also go into this smaller collection for iterating
        /// </summary>
      //  public Dictionary<EntityType, ProductionOrder> CarcassOrders = new Dictionary<EntityType, ProductionOrder>();


        bool ordersAreDirty = true;
        int totalOrders = 0;

        public ProductionOrders()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                foreach (KeyValuePair<string, EntityType> kvp in GameData.Instance.AllItemTypes)
                {
                    Orders.Add(kvp.Value, new ProductionOrder());

                   /* if (kvp.Value.ItemType.CarcassType != null)
                    {
                        CarcassOrders.Add(kvp.Value, new ProductionOrder());
                    }*/
                }
            }
        }


        public void SetDirectOrder(EntityType entityType, int amount)
        {           
            ProductionOrder order = Orders[entityType];
            order.ProductionJobsToComplete = amount;
            order.AmountToKeepInStore = null; // mutex

            ordersAreDirty = true;          
        }

        public void SetStandingOrder(EntityType entityType, int amount)
        {            
            ProductionOrder order = Orders[entityType];
            order.AmountToKeepInStore = amount;
            order.ProductionJobsToComplete = null; // mutex
            
            //ordersAreDirty = true;
        }

        /// <summary>
        /// direct orders are removed when the job starts, not when the job finishes :(
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public bool OrdersExist(EntityType entityType)
        {
            ProductionOrder orders;
            if (Orders.TryGetValue(entityType, out orders))
            {
                if (orders.ProductionJobsToComplete.HasValue && orders.ProductionJobsToComplete.Value > 0)
                {
                    return true;                  
                }
                else if (orders.AmountToKeepInStore.HasValue && orders.AmountToKeepInStore.Value > 0)
                {
                    return true;                 
                }
            }

            return false;
        }


       
     

        /// <summary>
        /// should only count direct order jobs, not standing order jobs (which the JobManager is responsible for)
        /// </summary>
        public int TotalDirectOrders
        {
            get
            {                
                if (ordersAreDirty)
                {
                    totalOrders = 0;
                    EntityType entityType;

                    foreach (var item in Orders)
                    {
                        entityType = item.Key;
                        ProductionOrder order = item.Value;
                        if (order.ProductionJobsToComplete.HasValue)
                        {
                            totalOrders += order.ProductionJobsToComplete.Value;
                        }
                    }

                    //totalJobs += ProductionJobs.Sum(p => p.Value.Count);

                    ordersAreDirty = false;
                }

                return totalOrders;
            }
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            Orders = sn.DoDictionary(Orders);
         
            sn.Ignore(totalOrders);
            sn.Ignore(ordersAreDirty);

            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            foreach (var item in Orders)
            {
                item.Value.LoadPostProcess(sn);
            }
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

        #endregion
    }

    /// <summary>
    /// only one target can be active at a time
    /// </summary>
    public class ProductionOrder : ISnapshot
    {
        /// <summary>
        /// for direct ordering from the inventory panel only - not gathering yet!
        /// Currently: counts jobs - not amount of products.
        ///      
        ///     
        /// 
        /// this number is reduced when the job starts, not when it completes. Not the best design...
        /// 
        /// 
        /// LATER: if this is set higher than the amount to keep in store, the surplus can be moved/traded to other expeditions
        /// It doesn't make sense to set this value lower than KeepInStore (unless we want to disable production of the item?)
        /// </summary>
        public int? ProductionJobsToComplete;


        /// <summary>
        /// if filled, the job manager will create jobs to to fulfill the order at all times.
        /// 
        /// NEW: -1 means unlimited order
        /// </summary>
        public int? AmountToKeepInStore;





        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            ProductionJobsToComplete = sn.DoInt32Nullable(ProductionJobsToComplete);
            AmountToKeepInStore = sn.DoInt32Nullable(AmountToKeepInStore);

            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

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

        #endregion
    }
}


// for firewood, there is both a production process and a gather process. Directly ordering from both sources 
// means this counter must include both, otherwise the jobs will be removed immediately...