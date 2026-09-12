using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Containers
{
    /// <summary>
    /// is referenced from the stand-alone container class ReplenishContainer, as well as VehicleContainer to manage its power/fuel
    /// 
    /// TODO: generalize this with options: continuous consumption (campfire) while 'on' (IsPrepared) / manual discrete consumption (bait for fishing rod) / consumption while using (fuel cells for power tools)
    /// and either discrete / bulk consumption of replenish items, or use of some internal Substance (fuel cell charge/ammo)
    /// Also, create general access/query methods instead of referencing the contained classes (RequiresFuel)
    ///   
    /// </summary>
    public class ReplenishItems: ISnapshot
    {
        private List<EntityID> containedItems = new List<EntityID>();

        public Entity Parent;
        private EntityID parentID;

       
        public ReplenishItems()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        public ReplenishItems(Entity parent)
        {
            this.Parent = parent;


            if (parent.EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType != null)
            {
                RequiresFuel = new RequiresFuel(this);
            }

           /* if (parent.EntityType.ContainerType.RequiresReplenishType.RequiresPowerType != null)
            {
                RequiresPower = new RequiresPower();
            }*/
        }



        public void ResetPlaySiteRegulators()
        {
            if (RequiresFuel != null)
            {
                RequiresFuel.ResetPlaySiteRegulators();
            }
        }

        #region From RequiresEnergy:

         public RequiresFuel RequiresFuel;

       // public RequiresPower RequiresPower;


        // public Entity Parent;//moved to base


        public bool Start()
        {
            if (RequiresFuel != null)
            {
                return RequiresFuel.LightFire();
            }

            return true;
        }

        public void Destroy()
        {
            if (RequiresFuel != null)
            {
                RequiresFuel.Extinguish();
            }
        }

      

        public bool HasEnergyForDuration(float durationInDays)
        {
            bool hasFuel = true;
            if (RequiresFuel != null)
            {
                hasFuel = RequiresFuel.HasFuelForDuration(durationInDays);
            }

            bool hasPower = true;
         /*   if (RequiresPower != null)
            {
                hasPower = RequiresPower.HasPowerForDuration(durationInDays);
            }*/

            return hasFuel && hasPower;           


        }

        public void Update()
        {            
            if (RequiresFuel != null)
            {
                RequiresFuel.Update();
            }
        }
        
        #endregion


        /// <summary>
        /// ejects all stored entities (items) and may apply condition damage to them
        /// </summary>
        /// <param name="damageStandardDev"></param>
        /// <param name="damageSpread"></param>
        public void UncontainAllEntities(float? damageStandardDev = null, float? damageSpread = null) //float? chanceToDestroy = null)
        {

            EntityID entityID;
            Entity entity;
            for (int i = containedItems.Count - 1; i >= 0; i--)
            {
                entityID = containedItems[i];

                entity = Entity.FindByID(entityID);

                if (entity != null)
                {
                    if (Parent.Contains.Remove(entity))
                    {
                        Parent.Contains.EjectEntity(entity);

                        if (damageStandardDev.HasValue && damageSpread.HasValue)
                        {
                            float damage = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(damageStandardDev.Value, damageSpread.Value);

                            entity.DoDamage(damage); //, false);
                        }

                    }
                }
                else
                {
                    containedItems.RemoveAt(i);
                }

            }
        }

        private void RemoveOutdatedItem(AmmoOfType ammoOfType, EntityID entityID)
        {
            ammoOfType.Items.Remove(entityID);

            ammoOfType.TotalIsDirty = true;

        }


        public bool Add(EntityID entity)
        {
            containedItems.Add(entity);

            return true;
        }

        public void IterateContained(Action<Entity> iterateMethod) //Container.IterateMethod iterateMethod)
        {
            Garrison.IterateList(containedItems, iterateMethod);
        }


        public void GetContainedItemsList(Predicate<Entity> rule, List<Entity> items)
        {
           // List<Entity> items = new List<Entity>();
            EntityID entityID;
            Entity entityInside;
            for (int i = containedItems.Count - 1; i >= 0; i--)
            {
                entityID = containedItems[i];

                entityInside = Entity.FindByID(entityID);
                if (entityInside != null)
                {
                    if (rule == null || rule(entityInside))
                    {
                        items.Add(entityInside);
                    }
                }
                else
                {
                    Remove(entityID);
                }
            }

          //  return items;

        }

        public bool Contains(EntityID entityID)
        {
            return containedItems.Contains(entityID);
        }

        public bool Remove(EntityID entity)
        {
            return containedItems.Remove(entity);
        }

        public int NoOfItems()
        {
            return containedItems.Count;
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
            this.containedItems = sn.DoList(containedItems);
            this.parentID = (EntityID)sn.SnapshotID<Entity, EntityID>(Parent);

            this.RequiresFuel = (RequiresFuel)sn.DoISnapshot(RequiresFuel);
           
            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Parent = Entity.FindByID(parentID);

            if (RequiresFuel != null)
            {
                RequiresFuel.Parent = this;

                RequiresFuel.LoadPostProcess(sn);
            }
        }


        #endregion
    }
}
