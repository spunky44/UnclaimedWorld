using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using GameStateManagement;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.AI;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Items
{ 
  
    /// <summary>
    /// is an item just 'something that can be carried' or used as material?
    /// </summary>
    public class Item : Component, IIDEventSubscriber 
    {
        
       // public Carcass Carcass;
        public Food Food;     
        public Ammunition Ammunition;
        
        /*private Entity equippedBy;

        /// <summary>
        /// use for 'equipment items': weapons and gadgets not used for a particular job but still carried.
        /// </summary>
        public Entity EquippedBy
        {
            get
            {
                Item item = GetRootAsItem();
                if (item != null)
                {
                    return item.equippedBy;
                }
                else return null; 
            }
            set 
            { 
                Item item = GetRootAsItem();
                if (item != null)
                {
                    item.equippedBy = value;
                }
            }
        }*/

        /// <summary>
        /// this flag should be set true after the person carrying the item has evaluated any hauling jobs and chosen not to haul the item himself...
        /// the purpose is to avoid harvested items being dropped and hauled by someone else unnecessarily
        /// 
        /// </summary>
        public bool? OKToTakeThisItemFromCarrier = null;


        MethodID parentBulkChangedID;

        private EntityID? replenishes;
        /// <summary>
        /// we only set this variable on the parent item for composites.
        /// it marks the item as inserted as replenishment for another entity that requires fuel, energy or ammo. This means that it must not be hauled or taken out!
        /// </summary>
        public EntityID? Replenishes
        {
            get
            {              
                Item item = GetRootAsItem();
                if (item != null)
                {
                    return item.replenishes;
                }

                return null;

            }
            set
            {
                Item item = GetRootAsItem();
                if (item != null)
                {
                    item.replenishes = value.Value;
                }
            }
        }

        /// <summary>
        /// tries to combine the condition states of two items. destroys one item
        /// </summary>
        /// <param name="entityToDestroy"></param>
        public void MergeItems(/*Entity entityToMergeWith,*/ Entity entityToDestroy)
        {
            // "combine" the condition states:
            NonLivingEntity existingNonLivingEntity;
            NonLivingEntity replenishNonLivingEntity;

            Parent.Find(out existingNonLivingEntity);
            entityToDestroy.Find(out replenishNonLivingEntity);

            if (entityToDestroy.Parts == null)
            { // if the entity has parts, dont average the condition. Too messy. Cheat and keep the original condition states.
                existingNonLivingEntity.Condition = Common.Average(existingNonLivingEntity.Condition, replenishNonLivingEntity.Condition);
                existingNonLivingEntity.MaxCondition = Common.Average(existingNonLivingEntity.MaxCondition, replenishNonLivingEntity.MaxCondition);
            }

            entityToDestroy.Destroy();

        }

        private Item GetRootAsItem()
        {
            IComposite root = Parent.GetRoot();
            Entity entity = root as Entity;
            if (entity != null)
            {
                Item item;
                if (entity.Find(out item))
                {
                    return item;
                }
            }
            return null;
        }

     
        

     /*   private Entity targetedForPickupBy;
        /// <summary>
        /// hmmm. This is not assigned when hauling. Will be using this for items for other purposes?
        /// </summary>
        public Entity TargetedForPickupBy
        {
            get 
            {
                Item item = GetRootAsItem();
                if (item != null)
                {
                    return item.targetedForPickupBy;
                }
                else return null; 
            }
            set 
            {
                Item item = GetRootAsItem();
                if (item != null)
                {
                    item.targetedForPickupBy = value;
                }            
            }
        }
        */


        
       // private List<Item> parts;
      //  public List<Entity> Parts { get; set; }
       /* {
            get { return parts; }
        }*/

     //   public bool PartBroken = false;


        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        /*  public bool IsAccessible()
          {            
          //  does this still make sense???
            replaced with Container system 
              return Parent.MapPosition.X != -1 && Parent.MapPosition.Y != -1;
          }*/


        public Item(Entity parent) : base(parent)
        {
           
            if (parent.EntityType.ItemType.FoodType != null)
            {
                this.Food = new Food(this);
            }

            if (parent.EntityType.ItemType.AmmunitionType != null)
            {
                this.Ammunition = new Ammunition()
                {
                    NoOfRounds = parent.EntityType.ItemType.AmmunitionType.MaxNoOfRounds
                };
            }
                      
           // Parent.BulkChanged += new Entity.BulkChangedHandler(Parent_BulkChanged);
            parent.BulkChangedEvent.AddAndRegister(Parent_BulkChanged,
                   this, out parentBulkChangedID);
        }

        void Parent_BulkChanged(float oldValue)
        {
            if (Food != null)
            {
                Food.UpdateNutrientAmounts(Parent);
            }
        }

        public Item()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        #region ISnapshot

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.Ammunition = (Ammunition)sn.DoISnapshot(Ammunition);        
            this.Food = (Food)sn.DoISnapshot(this.Food);

            this.OKToTakeThisItemFromCarrier = sn.DoBoolNullable(this.OKToTakeThisItemFromCarrier);
          
            this.replenishes = sn.DoEnumNullable(replenishes);
            
            this.parentBulkChangedID = sn.DoMethodID(parentBulkChangedID);

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            LoadPostProcessRegisterMethodIDs();

            if (Food != null)
            {
                Food.Parent = this;
                Food.LoadPostProcess(sn);
            }

            if (Ammunition != null)
            {
                Ammunition.LoadPostProcess(sn);
            }

        }

        public void LoadPostProcessRegisterMethodIDs()
        {
            ActionLookup<float>.Add(parentBulkChangedID, Parent_BulkChanged);           
        }

        #endregion

       

        

        public void Initialize() 
        {
                      
           /* if (Parent.EntityType.ItemType.WeaponType != null)
            {
                this.Weapon = new Weapon( Parent.EntityType.ItemType.WeaponType ); 
            }*/

            // TODO: items with special unique bulk
            if (Parent.EntityType.ItemType.MaximumBulk.HasValue)
            {
                Parent.Bulk = Parent.EntityType.ItemType.MaximumBulk.Value;
            }

            if (Food != null)
            {
                Food.UpdateNutrientAmounts(Parent);

            }                      
            
        }


        /// <summary>
        /// if this is called on a part, the entire entity/composite item will change owner!
        /// for efficiency reasons, call this on the top part only.
        /// </summary>
        /// <param name="newOwner"></param>
      /*  public void ChangeOwnership(Owner newOwner) 
        {
            if (PartOf == null) // are we at the root?
            {
                ChangeOwnershipOnParts(newOwner);
            }
            else
            {
                IComposite root = GetRoot();
               // IOwnable rootAsItem = root as Item;
                IOwnable rootAsOwnable = root as IOwnable;
                rootAsOwnable.ChangeOwnership(newOwner);
            }
        }*/

        /// <summary>
        /// don't call this, call the general method instead.
        /// </summary>
        /// <param name="newOwner"></param>
      /*  public void ChangeOwnershipOnParts(Owner newOwner)
        {
            if (Parent.Owner != null) // the item has previously been owned.
            {
                // we are changing ownership for this item.
                // we are moving this item from an old collection to a new:
                if (Parent.Owner.OwningBody.Items != newOwner.OwningBody.Items)
                {
                    // delete from old collection:
                    if (Parent.Owner.OwningBody.Items[Parent.EntityType] != null)
                    {
                        Parent.Owner.OwningBody.Items[Parent.EntityType].Remove(Parent);
                    }

                    // add to new collection:
                    AddItemToCollection(newOwner.OwningBody.Items);

                    // point the items list to the new collection, unknown to the clients...

                    Parent.Owner = newOwner;
                    //this.BelongingCollection.Items = newOwner.BelongingCollection.Items;

                    AddKnowledgeOfItemToNewOwner(newOwner, Parent);

                }
                else
                {
                    // do nothing - it is already in the collection.
                }

                if (Parts != null)
                {
                    foreach (Entity part in Parts)
                    {
                        part.Item.ChangeOwnershipOnParts(newOwner);
                    }
                }

            }
            else
            {   // we are creating the item; it has no previous owner.
                // add to new collection:
                AddItemToCollection(newOwner.OwningBody.Items);
                Parent.Owner = newOwner;

                AddKnowledgeOfItemToNewOwner(newOwner, Parent);
            }

        }*/


        public bool IsOwnedBySomebody()
        {
            return Parent.OwnedBy != null; // && Parent.Owner != The.Sim.NoOwner;

        }

       

        private void AddItemToCollection(Dictionary<EntityType, List<Entity>> belongingCollection)
        {
            if (belongingCollection.ContainsKey(Parent.EntityType))
            {
                belongingCollection[Parent.EntityType].Add(Parent);
            }
            else
            {
                List<Entity> newList = new List<Entity>();
                newList.Add(Parent);
                belongingCollection.Add(Parent.EntityType, newList);
            }
        }

      /*  public bool DrawThis()
        {
            return PartOf == null && StoredIn == null;            
        }*/
        /*
        public void CreateParts()
        {
            Entity part;
            if (Parent.EntityType.ItemType.Parts != null && Parent.EntityType.ItemType.Parts.Count > 0)
            {
                Parts = new List<Entity>();

                foreach (KeyValuePair<EntityType, int> kvp in Parent.EntityType.ItemType.Parts)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    {
                        part = new Entity(kvp.Key);
                        part.Initialize(UWGame.SimSide.Instance.Site);
                        part.InitializeModelAndOnScreenFunctionality(UWGame.SimSide.Instance.ScreenManager.Game);

                        Parts.Add(part);

                        if (part.EntityType.ItemType.Parts != null)
                        {
                            part.Item.CreateParts();
                        }
                    }
                }
            }
        }
        */
       /* public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append(ItemType.Name);
            text.Append(" ");

            text.Append(MapPosition);
            return text.ToString();
        }*/



        


     
        public void Destroy()
        {
           
            //The.Sim.Site.DegradableEntities.Remove(Parent);
        }

        

        

      

        /// <summary>
        /// removes an item from the ground or its container and places it in an agent's container as well as sets the new ownership status and creates hauling jobs if necessary!
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="newOwner"></param>
        /// <param name="placeInCompartment"></param>
        /// <returns></returns>
        public bool Pickup(Entity entity, IOwner newOwner, StorageCompartment placeInCompartment)
        {
          //  Point mapPos = Parent.MapPosition.Value;

            bool wasPickedUp;
            if (Parent.ContainedBy != null)
            {
                Entity containingEntity = Entity.FindByID(Parent.ContainedBy.Value);
                if (containingEntity != null)
                {
                    wasPickedUp = containingEntity.Contains.Uncontain(Parent, placeInStorageEntity: entity, compartment: placeInCompartment);
                }
                else return false;
            }
            else
            {
                // take from the ground:
                wasPickedUp = entity.AgentStorage.AddToContain(Parent, placeInCompartment);

             /*   if (mapPos.X > -1 && mapPos.Y > -1)
                {
                    The.Map.TileMap[mapPos.X][mapPos.Y].RemoveEntity(Parent);
                }*/
            }


            if (wasPickedUp) 
            {                
                OKToTakeThisItemFromCarrier = false;
                                
                // make sure that a newly spawned item is seen, otherwise we may get problems...
               // entity.Intelligence.Allegiance.SharedKnowledge.SeeDetectable(Parent, true, false, null, entity); 
                entity.Intelligence.Allegiance.SharedKnowledge.SeeDetectableIfRelevant(Parent, true, false, null, entity); 
            
                if (newOwner != null)
                {
                    Parent.ChangeOwnership(newOwner);

                    // do this on item pickup/creation please...
                    // NOTE! this does not evaulate the new jobs!
                    HaulingJobManager.CreateHaulingJobsForAllCarriedItemsOutOfBand(entity, newOwner.OwnedEntities);
                }

                return true;
            }
            else return false;
        }

       

      /*  public void DropByEntity(Entity entity, Entity placeInStorageEntity, Storage.Conditions? placeInStorage)
        {           
            EquippedBy = null;

            OKToTakeThisItemFromCarrier = null;
                                  
            Parent.TargetedForPickupBy = null;
            
            
            Entity container;
            if (placeInStorageEntity != null)
            {
                container = placeInStorageEntity;
            }
            else
            {
                // is the entity itself contained?
                if (!entity.GetContainedBy(out container))
                {
                    return;
                }
            }

            if (container != null)
            {
                // place the dropped item in selected storage:    
                container.Contains.AddToContain(Parent, null, placeInStorage);
            }
            else
            {
                // place on ground:
                The.Map.TileMap[entity.MapPosition.X][entity.MapPosition.Y].AddEntity(Parent);
                Parent.PlaceOnGroundWithSmallRandomOffset(entity.Location);
            }
                       

        }*/

        

        /// <summary>
        /// moved to IKnownEntityData
        /// </summary>
        /// <returns></returns>
      /*  public bool IsUnassigned()
        {
            // NEW: exclude item parts here.
            return IsAccessible() && Parent.PartOf == null && Parent.AssignedToJob == null && EquippedBy == null && TargetedForPickupBy == null;

        }*/

        /// <summary>
        /// moved to IKnownEntity
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
      /*  public bool IsUnassignedToAnythingButThisJob(Job job)
        {
            // NEW: exclude item parts here.
            return IsAccessible() && Parent.PartOf == null && (Parent.AssignedToJob == null || Parent.AssignedToJob == job) && EquippedBy == null && TargetedForPickupBy == null;

        }*/




        /// <summary>
        /// this will probably need to be changed once we have vehicles in the game
        /// </summary>
        /// <param name="bulk"></param>
        /// <returns></returns>
        public static bool IsImmovable(float bulk)
        {
            return bulk > 1f;
        }

        

        /*
        public IComposite GetRoot()
        {
            if (PartOf == null)
            {
                return this;
            }
            else
            {
                return PartOf.GetRoot();
            }
        }
        */
       /* private Storage GetStorage()
        {
            if (PartOf == null)
            {
                return StoredIn;
            }
            else
            {
                Item partOf = PartOf as Item;
                if (partOf != null)
                {
                    return partOf.GetStorage();
                }

            }
        }*/

        private MachineBodyPart GetMachineBodyPart()
        {
            if (Parent.PartOf == null)
            {
                return null;
            }
            else 
            {
                MachineBodyPart parent = Parent.PartOf as MachineBodyPart;
                if (parent != null)
                {
                    return parent;
                }
                else 
                {
                    Item parentItem = Parent.PartOf as Item;
                    if (parentItem != null)
                    {
                        return parentItem.GetMachineBodyPart();
                    }
                }
            }

            return null;
        }

      /*  public bool IsEnclosed()
        {
            MachineBodyPart parent = GetMachineBodyPart();
            if (parent != null)
            {
               // return ((MachineBodyPartType)parent.BodyPartType).IsInternal; // 

                return ((MachineBodyPartType)parent.BodyPartType).IsInternal; // 
            }

            return false;
        }*/

        public bool IsWeatherProof()
        {
            if (Parent.EntityType.NonLivingType.PartsAreWeatherProof) // PartOf == null)
            {
                return true;
            }
            else if (Parent.PartOf == null)
            {
                return false;
            }
            else
            {
                Item parent = Parent.PartOf as Item;
                if (parent != null)
                {
                    return parent.IsWeatherProof();
                }
                else return false;
            }
        }

        

       


        
        
       

     /*   public bool IsUnassignedAndOnSite(Point position)
        {
            return IsAccessible() && AssignedToJob == null && EquippedBy == null && TargetedForPickupBy == null && Parent.MapPosition == position;

        }
       */         
        

        
    }
}
