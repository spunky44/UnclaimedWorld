using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Containers
{
   
    public class UpgradeItems: ISnapshot
    {
        private List<EntityID> containedItems = new List<EntityID>();

        private Dictionary<UpgradeCategory, EntityID> containedUpgrades = new Dictionary<UpgradeCategory, EntityID>();

        public Entity Parent;
        private EntityID parentID;

       
        public UpgradeItems()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        public UpgradeItems(Entity parent)
        {
            this.Parent = parent;

        }

        

        /// <summary>
        /// Always destroy upgrades. We don't want them to exist outside the upgradable
        /// </summary>
        /// <param name="damageStandardDev"></param>
        /// <param name="damageSpread"></param>
        public void DestroyAllEntities() //float? chanceToDestroy = null)
        {

            EntityID entityID;
            Entity entity;
            for (int i = containedItems.Count - 1; i >= 0; i--)
            {
                entityID = containedItems[i];

                entity = Entity.FindByID(entityID);

                if (entity != null)
                {
                    if (Parent.Contains.Remove(entity)) // OLD: left the entity in a crashy state:  Remove(entity.EntityID))
                    {
                        entity.Destroy();

                       /* Parent.Contains.EjectEntity(entity);

                        if (damageStandardDev.HasValue && damageSpread.HasValue)
                        {
                            float damage = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(damageStandardDev.Value, damageSpread.Value);

                            entity.DoDamage(damage); //, false);
                        }*/

                    }
                }
                else
                {
                    containedItems.RemoveAt(i);
                    RemoveUpgradeItem(entityID);
                }

            }
        }

       


        public bool Add(UpgradeCategory upgradeCategory, Entity entity)
        {
            if (entity.EntityType.Upgrader != null)
            {
                containedItems.Add(entity.ID);
                containedUpgrades[upgradeCategory] = entity.ID;

                if (Entity.IsFunctional(entity)
                    && NonLivingEntity.IsCompleted(entity.Progress))
                {
                    StartEffects(entity);

                    SetSpriteModifier(entity.EntityType.Upgrader, true);
                }

                return true;
            }
            else return false;
        }

        public void ApplyUpgradeEffects(Entity entity)
        {
            StartEffects(entity);

            SetSpriteModifier(entity.EntityType.Upgrader, true);

        }

        public void SetSpriteModifier(Upgrader upgrader, bool set) // Entity entity)
        {
            if (upgrader.SpriteModifier.HasValue)
            {
                if (set)
                {
                    Parent.SetSpriteStateFlag(upgrader.SpriteModifier.Value);
                }
                else
                {
                    Parent.ClearSpriteStateFlag(upgrader.SpriteModifier.Value);
                }
            }
        }

        public void StartEffects(Entity entity)
        {
            // activate effects:
            if (entity.EntityType.Upgrader.EffectsFinal != null)
            {
                foreach (var item in entity.EntityType.Upgrader.EffectsFinal)
                {
                    Parent.SimEffects.Start(item);
                }
            }
        }

       

        public void EndEffects(Entity item)
        {
            if (item.EntityType.Upgrader.EffectsFinal != null)
            {
                foreach (var effect in item.EntityType.Upgrader.EffectsFinal)
                {
                    Parent.SimEffects.Remove(effect);
                }
            }
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

        public Dictionary<UpgradeCategory, EntityID> ContainedUpgrades
        {
            get
            {
                return containedUpgrades;
            }
        }



        private void RemoveUpgradeItem(EntityID entityID)
        {
            UpgradeCategory category = GetUpgradeCategory(entityID);

            if (category != null)
            {
                containedUpgrades.Remove(category);
            }
        }

        public UpgradeCategory GetUpgradeCategory(EntityID entityID)
        {
            // linear search, hmm. Can we get the category elsewhere?
            UpgradeCategory category = null;
            foreach (var item in containedUpgrades)
            {
                if (item.Value == entityID)
                {
                    category = item.Key;
                    break;
                }
            }
            return category;
        }

        public bool Remove(EntityID entityID)
        {
            bool wasRemoved = containedItems.Remove(entityID);
            RemoveUpgradeItem(entityID);

            if (wasRemoved)
            {
                Entity entity = Entity.FindByID(entityID);
                EndEffects(entity);

                SetSpriteModifier(entity.EntityType.Upgrader, false);
            }

            return wasRemoved;
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
            this.containedUpgrades = sn.DoDictionary(containedUpgrades);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Parent = Entity.FindByID(parentID);
        }


        #endregion
    }
}
