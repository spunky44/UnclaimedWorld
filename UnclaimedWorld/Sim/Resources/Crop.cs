using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Entities;
using GameStateManagement;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Trees;
using UWGame.SimSide.AI;
using UWGame.Control;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Resources
{

    public class Crop : ResourceContainer //, IDetectable
    {
       // public float CropBulkGrowthPerDay;

        /// <summary>
        /// Either Tree or LowVegetation
        /// can this be made better..?
        /// </summary>
        public IHasCrops Parent;
        HasCropsID snapshotParent;

        private Entity parentEntity;
        private EntityID? snapshotParentEntity;

        

       /* public DetectableID DetectableID
        {
            get
            {
                return detectableID;
            }
        }*/

        /// <summary>
        /// only set if parent is Tree
        /// </summary>
        public override Entity ParentEntity
        {
            get
            {
                return parentEntity;
            }
            //    private set;
        }




        public override EntityType EntityType
        {
            get 
            { 
                return null; 
            }
        }

        public override bool IsIntelligent
        {
            get { return false; }
        }


        public override Vector3 AccessPoint
        {
            get
            {
                return Parent.AccessPoint;
            }
        }

        /// <summary>
        /// Total of crop items!
        /// (should this be included as a part of plant's bulk?)
        /// </summary>
        public float TotalBulk;

        /// <summary>
        /// summary, for use by AI evaluators
        /// = TotalHarvestableBulk
        /// </summary>
        public float TotalBulkOfRipeItems;

        // public List<CropItem> CropItems = new List<CropItem>();

        private List<IResourceItem> ripeCropItems = new List<IResourceItem>();
        List<ResourceItemID> snapshotRipeCropItems;

        /// <summary>
        /// could this be made as a wrapper for Substance called DiscreteSubstance..?
        /// </summary>
        private List<IResourceItem> cropItems = new List<IResourceItem>();
        public override List<IResourceItem> ResourceItems
        {
            get
            {
                return ripeCropItems;

                //  return cropItems.FindAll(c => ((CropItem)c).IsRipe());
            }
        }
        private List<ResourceItemID> snapshotCropItems;

        public override IResourceItem FindHarvestableItem()
        {
            return GetRipeItem();
        }

        public Crop()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }

        //   public Entity TargetedForHarvestingBy { get; set; }

        public Crop(IHasCrops parent, ResourceType cropType)
            : base(cropType)
        {            
            ((ILookUp<IDetectable, DetectableID>)this).AddToLookup();

            this.Parent = parent;
            
            Tree parentTree = parent as Tree;
            if (parentTree != null)
            {
                parentEntity = parentTree.Parent;

                // trees need a pulsing effect on the renderable:
              //  ParentEntity.Renderable.SetPulsing(ClientSide.Renderables.Renderable.AdditionalEffect.Outline); // SetOverlayPulsing();
            }
            
        }

        public override void Destroy()
        {
            base.Destroy();

            ((ILookUp<IDetectable, DetectableID>)this).RemoveIDEntry();
        }

        public override Point MapPosition
        {
            get
            {
                return Parent.MapPosition;

               /* Tree tree = Parent as Tree;
                if (tree != null)
                {
                    return tree.Parent.MapPosition;
                }
                else
                {
                    LowVegetation vegetation = Parent as LowVegetation;
                    return new Point(vegetation.Parent.Parent.X, vegetation.Parent.Parent.Y);
                }*/
            }
        }

        public override float TotalHarvestableBulk
        {
            get
            {
                return TotalBulkOfRipeItems;
            }
        }




        public override bool IsDestroyed(SharedKnowledge knowledge)
        {
            if (ParentEntity != null)
            {
                // limited knowledge are onyl handled for entities, for now:
                // see if the parent entity is known to be destroyed:
                IKnownEntityData entityData;
                if (GoalEvaluator.EntityDataResultCausesSkip(knowledge.GetKnownData(ParentEntity.EntityID, out entityData)))
                {
                    return true;
                }
                else return false;
            }
            else
            {
                if (base.ID != ResourceID.Invalid)
                {
                    return LookUp<ResourceContainer, ResourceID>.FindByID(ID) == null;
                }
                else return true;
            }

            // OLD:
            /* Tree tree = Parent as Tree;
             if (tree != null)
             {
                 // trees status are always known...
                 if (tree.Parent.IsDestroyed)
                 {
                     return true;
                 }
             }
             else
             {
                 Entity entity = Parent as Entity;
                 if (entity != null)
                 {
                     // see if the parent entity is known to be destroyed:
                     IKnownEntityData entityData;
                     if (GoalEvaluator.EntityDataResultCausesSkip(knowledge.GetKnownData(entity, out entityData)))
                     {
                         return true;
                     }

                     return false;
                    // knowledge.GetIsDestroyed(entity);
                 }
             }
             return false;*/
        }


        public override int NoOfHarvestableItems
        {
            get
            {
                return ripeCropItems.Count;
            }
        }

        /// <summary>
        /// TODO: use a SimProcess for this
        /// </summary>
        /// <param name="deltaTimeInSeconds"></param>
        /// <param name="water"></param>
        /// <param name="nitrogen"></param>
        /// <param name="phosphorous"></param>
        /// <param name="age"></param>
        /// <param name="plantBulk"></param>
        public void GetGrowthNeeds(double deltaTimeInSeconds, out float water, out float nitrogen, out float phosphorous, float age, float plantBulk)
        {
            float maxCropBulk = resourceType.CropType.MaxSizeShareOfWholePlant * plantBulk;

            float ageFactor = Common.GetInterpolatedFunctionValue(age, resourceType.CropType.AgeProduction);

            float sizeFactor = plantBulk; // crop output increases linearly with size???

            double deltaDays = The.Sim.DateAndTime.DaysPerSecond * deltaTimeInSeconds;
            
            // TODO: replace this with proper plant growth simulation.
           // float availableForCropGrowth = (float)(ageFactor * sizeFactor * CropBulkGrowthPerDay * deltaDays);


            // TODO!

            water = 0;

            nitrogen = 0f;

            phosphorous = 0f;
        }


        public void GrowAndRipen(float availableForCropGrowth, double deltaTimeInSeconds) //, float age, float plantBulk) 
        {
            double deltaDays = The.Sim.DateAndTime.DaysPerSecond * deltaTimeInSeconds;

            // TODO!!!!
            float plantBulk = -1f; // PLACEHOLDER - replace with proper resources

            float maxCropBulk = resourceType.CropType.MaxSizeShareOfWholePlant * plantBulk;

            /*   float maxCropBulk = CropType.MaxSizeShareOfWholePlant * plantBulk;

               float ageFactor = Common.GetInterpolatedFunctionValue(age, CropType.AgeProduction);

               float sizeFactor = plantBulk; // crop output increases linearly with size???

           
               // TODO: replace this with proper plant growth simulation.
               float availableForCropGrowth = (float)(ageFactor * sizeFactor * CropBulkGrowthPerDay * deltaDays);

           
               bool canGrow = TotalBulk < maxCropBulk;
    */
            /*  if (canGrow)
              {*/

            foreach (CropItem cropItem in cropItems)
            {
                // not all may be used by crops...
                if (!cropItem.IsFullyGrown) // grow
                {
                    cropItem.Grow(ref availableForCropGrowth, deltaDays);
                }
                else if (!cropItem.IsRipe() && resourceType.CropType.RipeSpeed.HasValue) // ripen
                {
                    cropItem.Ripen(deltaDays);
                }

            }
            //  }

            // distribute the available growth out among the not fully grown crop items via round robin:
            /*   List<CropItem> cropItemsToGrow = new List<CropItem>();
               foreach (CropItem cropItem in CropItems)
               {
                   if (!cropItem.IsFullyGrown())
                   {
                       cropItemsToGrow.Add(cropItem);
                   }
               }

               CropItem cropItemToGrow;
               int cropItemIndex = 0;
               while (availableForCropGrowth > 0f)
               {
                   cropItemToGrow = cropItemsToGrow[cropItemIndex];

                   if (!cropItemToGrow.Grow(ref availableForCropGrowth, deltaTimeInSeconds))
                   {
                       cropItemsToGrow.Remove(cropItemToGrow);
                       cropItemIndex--;
                   }

                    

                   cropItemIndex++;
                   cropItemIndex = cropItemIndex % cropItemsToGrow.Count;
               }
           }*/

            //  CropItem cropItem;
            /*   int cropItemIndex = 0;
               while (availableForCropGrowth > 0f)
               {
                   cropItem = CropItems[cropItemIndex];

                   cropItem.Grow(ref availableForCropGrowth);

                   cropItemIndex++;
                   cropItemIndex = cropItemIndex % CropItems.Count;
               }*/

            UpdateBulkAndSprites(NoOfHarvestableItems);


            // replace crop items:
            SetNewCropItemBuds(maxCropBulk);


            // }

            // crop can rot or disappear too???
        }

        protected override void UpdateBulkAndSprites(int noOfItems)
        {
            base.UpdateBulkAndSprites(noOfItems);

            float oldBulk = TotalBulk, oldRipe = TotalBulkOfRipeItems;
            TotalBulk = 0f;
            TotalBulkOfRipeItems = 0f;

            ripeCropItems.Clear();

            foreach (CropItem cropItem in cropItems)
            {
                // grow each crop varyingly...
                // cropItem.UpdateSimulation(deltaTimeInSeconds, ageFactor, canGrow);

                // update convenience totals:
                TotalBulk += cropItem.Bulk;

                if (cropItem.IsRipe())
                {
                    TotalBulkOfRipeItems += cropItem.Bulk;

                    ripeCropItems.Add(cropItem);
                }
            }

            // push the state flag changes:
            if (ParentEntity != null)
            {
                if (resourceType.CropType.TreeSpriteFlag.HasValue)
                {
                    ParentEntity.Renderable.SetOrClearSpriteStateFlag(
                       TotalBulk > resourceType.CropType.BulkLimitToShowFlag, // 0f, // 1f,
                       resourceType.CropType.TreeSpriteFlag.Value);// .HasResources

                    if (resourceType.CropType.RipeSpeed.HasValue)
                    {
                        ParentEntity.Renderable.SetOrClearSpriteStateFlag(
                            NoOfHarvestableItems > 0,
                            ClientSide.Renderables.StateModifier.Ripe);
                    }
                }

            }           
        }


       
        /// <summary>
        /// designer controlled number of crop items - modified by tree size
        /// </summary>
        /// <param name="bulkFactor"></param>
        /// <param name="mean"></param>
        public void SetRandomCrops(float bulkFactor, float mean, float standardDeviation)
        {

            bulkFactor = Common.ClampTop(bulkFactor, 1f); // NEW: don't let big trees have more crops... confuses the designer / player...

            float cropAmountToSetOnFullGrownTree = (float)(The.Sim.GameplayRandomGenerator.RandomNormalDistribution(mean, standardDeviation));

            cropAmountToSetOnFullGrownTree = Common.ClampBottom(cropAmountToSetOnFullGrownTree, 0f);

            float maxCropItems = cropAmountToSetOnFullGrownTree * bulkFactor;

            bool canRipen = this.ResourceType.CropType.RipeSpeed.HasValue;

            int budsToAdd = ((int)Math.Round(maxCropItems, MidpointRounding.ToEven)); // round to nearest even number
                        
            if (canRipen)
            {
                budsToAdd *= 2;
            }

            SetResourceItems(budsToAdd);

        }

        public override void SetResourceItems(int noOfItems)
        {            
            // NEW
            int currentItems = cropItems.Count;

            if (noOfItems > currentItems)
            {
                // add items:
                AddResourceItems(noOfItems - currentItems);
            }
            else if (noOfItems < currentItems)
            {
                // remove items:
                int amounttoRemove = currentItems - noOfItems;
                RemoveResourceItems(amounttoRemove);
            }
                       
            /*
            cropItems.Clear();
            AddResourceItems(budsToAdd);       
            UpdateBulkAndSprites();
            */

           
        }

        /// <summary>
        /// what about jobs/goals - is it safe?
        /// </summary>
        /// <param name="amounttoRemove"></param>
        public override void RemoveResourceItems(int amounttoRemove)
        {
            while (amounttoRemove > 0 && cropItems.Count > 0)
            {
                IResourceItem item = cropItems[cropItems.Count - 1];

                item.Destroy();

                cropItems.RemoveAt(cropItems.Count - 1);

                amounttoRemove--;
            }

            UpdateBulkAndSprites(cropItems.Count);

        }

        public override void AddResourceItems(int noOfItemsToAdd)
        {
            for (int i = 0; i < noOfItemsToAdd; i++)
            {
                CropItem cropItem = new CropItem(this);
                cropItems.Add(cropItem);

                cropItem.Bulk = ResourceType.ResourceItemType.ItemType.MaximumBulk.Value;
                cropItem.IsFullyGrown = true;

                cropItem.Ripeness = 1f;    
            }


            UpdateBulkAndSprites(cropItems.Count);
        }

        /*
        OLD: some ripeness calcs are here...
        public override void SetResourceItems(int budsToAdd)
        {
            base.SetResourceItems(budsToAdd);

            cropItems.Clear();

            SetAbsoluteCropItemBuds(budsToAdd);


            bool canRipen = this.ResourceType.CropType.RipeSpeed.HasValue;
          
            int noOfRipes = cropItems.Count;

            if (canRipen)
            {
                // make half ripe... or???
                noOfRipes /= 2;
            }

            int ripeCounter = 0;
            foreach (CropItem cropItem in cropItems)
            {
                cropItem.Bulk = ResourceType.ResourceItemType.ItemType.MaximumBulk.Value;
                cropItem.IsFullyGrown = true;

                if (cropItem.IsFullyGrown)
                {
                    if (!canRipen || ripeCounter < noOfRipes)
                    {
                        cropItem.Ripeness = 1f;
                        ripeCounter++;
                        //noOfRipes++; ??
                    }
                    else
                    {
                        cropItem.Ripeness = 0.2f + The.Sim.GameplayRandomGenerator.RandomBetween(0f, 0.7f);
                    }

                    cropItem.Ripeness = Math.Min(1f, cropItem.Ripeness);
                }
            }

            RecalculateTotals();
        }*/

        /*    public void SetRandomCrops(float plantBulk, float probability)
            {
                // TODO: depend on season

                float maxCropBulk = CropType.MaxSizeShareOfWholePlant * plantBulk;

                maxCropBulk *= Common.RandomBetween(Globals.Instance.RandomPredictable, 0.5f, 1f);

                SetNewCropItemBuds(maxCropBulk);


                foreach (CropItem cropItem in CropItems)
                {
                    cropItem.Bulk = 0.4f * CropType.ResourceItem.ItemType.Bulk.Value + Common.RandomBetween(Globals.Instance.RandomPredictable, 0.3f * CropType.ResourceItem.ItemType.Bulk.Value, 0.7f * CropType.ResourceItem.ItemType.Bulk.Value);

                    if (cropItem.Bulk >= CropType.ResourceItem.ItemType.Bulk)
                    {
                        cropItem.IsFullyGrown = true;

                        cropItem.Bulk = CropType.ResourceItem.ItemType.Bulk.Value;
                    }

                    if (cropItem.IsFullyGrown)
                    {
                        cropItem.Ripeness = 0.2f + Common.RandomBetween(Globals.Instance.RandomPredictable, 0f, 1f);

                        cropItem.Ripeness = Math.Min(1f, cropItem.Ripeness);

                    }
                }

                RecalculateTotals();

            }*/

        private void SetNewCropItemBuds(float maxCropBulk)
        {
            maxCropBulk *= 1.5f; // aim for 150 % of maximum crop bulk... 

            float cropDelta = maxCropBulk - cropItems.Count * ResourceType.ResourceItemType.ItemType.MaximumBulk.Value;
            if (cropDelta > 0f)
            {
                // add some new buds:
                int budsToAdd = Math.Max(1, (int)(cropDelta / ResourceType.ResourceItemType.ItemType.MaximumBulk.Value));

                for (int i = 0; i < budsToAdd; i++)
                {
                    cropItems.Add(new CropItem(this));
                }
            }

        }

       

       
        public override bool GatherResource(IResourceItem cropItem)
        {
            //    CropItem cropItem = GetRipeItem();

            if (cropItem != null)
            {
                if (ripeCropItems.Contains(cropItem))
                {
                    cropItems.Remove(cropItem);

                    UpdateBulkAndSprites(cropItems.Count); // don't wait for the simulator update

                    return true;
                }
            }

            return false;
        }

        public CropItem GetRipeItem()
        {
            return (CropItem)ripeCropItems[0];

            /* foreach (CropItem cropItem in ResourceItems)
             {
                 if (cropItem.IsRipe())
                 {
                     return cropItem;
                 }
             }

             return null;*/
        }

        /// <summary>
        /// may return null!
        /// </summary>
        /// <returns></returns>
        public Tree GetTree()
        {
            Tree tree = Parent as Tree;
            return tree;
        }

        /* public Point GetCropMapPosition()
         {

             Tree tree = Parent as Tree;
             if (tree != null)
             {
                 return tree.Parent.MapPosition;
             }
             else
             {
                 LowVegetation vegetation = Parent as LowVegetation;
                 return new Point(vegetation.Parent.X, vegetation.Parent.Y);
             }
         }*/

        /// <summary>
        /// return the closest point that we can reach...
        /// this is necessary because the trees (containers) block movement!
        /// </summary>
        /// <returns></returns>
        public override bool GetClosestAccessibleHarvestLocation(SubtileLayers movemap, Vector3 fromLocation, out Vector3? closestLocation)
        {

            Vector3 cropLocation = Parent.AccessPoint;

           /* Tree tree = Parent as Tree;
            if (tree != null)
            {
                cropLocation = tree.Parent.AccessPoint;
            }
            else
            {
                LowVegetation vegetation = Parent as LowVegetation;
                cropLocation = MapManager.TileToWorldPos(new Point(vegetation.Parent.Parent.X, vegetation.Parent.Parent.Y));
            }*/

            Point? closestSubtilePosition;
            if (The.Map.GetClosestAccessiblePoint(movemap, fromLocation, cropLocation, false, out closestSubtilePosition))
            {
                closestLocation = MapManager.SubTileToWorldPos3(closestSubtilePosition.Value);
                return true;
            }
            else
            {
                closestLocation = null;
                return false;
            }
        }


        public override ClientSide.Renderables.Renderable Renderable
        {
            get
            {
                //Trees.Tree tree = Parent as Trees.Tree;

                if (ParentEntity != null) // tree != null)
                {
                    return ParentEntity.Renderable;
                }

                return null;
            }
        }

        // public Vector3 GetCropLocation()
        public override Vector3 Location
        {
            get
            {
                return Parent.Location;

            }
        }

        public override bool RequiresRollToDetect()
        {
            return true;
        }


        public override bool UsesMemory(SharedKnowledge sharedKnowledge)
        {
            return true;
        }

    



        #region ISnapshot

        
        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

           // this.detectableID = sn.DoEnum(detectableID);           
          //  this.CropBulkGrowthPerDay = sn.DoFloat(CropBulkGrowthPerDay);

            this.TotalBulk = sn.DoFloat(TotalBulk);
            this.TotalBulkOfRipeItems = sn.DoFloat(TotalBulkOfRipeItems);            
            this.snapshotParentEntity = sn.SnapshotID<Entity, EntityID>(parentEntity);
            this.snapshotParent = (HasCropsID) sn.SnapshotID<IHasCrops, HasCropsID>(Parent); 
            

            this.snapshotCropItems = cropItems.Select(r => r.ID).ToList();
            this.snapshotCropItems = sn.DoList(snapshotCropItems);
          
            this.snapshotRipeCropItems = ripeCropItems.Select(r => r.ID).ToList();
            this.snapshotRipeCropItems = sn.DoList(snapshotRipeCropItems);


            sn.Ignore(ripeCropItems);
            sn.Ignore(cropItems);

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
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);
             
            Parent = LookUpIHasCrops.FindByID(snapshotParent);
            parentEntity = Entity.FindByID(snapshotParentEntity);
            cropItems = snapshotCropItems.Select(r => LookUp<IResourceItem, ResourceItemID>.FindByID(r)).ToList();
            ripeCropItems = snapshotRipeCropItems.Select(r => LookUp<IResourceItem, ResourceItemID>.FindByID(r)).ToList();

        }

        #endregion

        /*
        #region DetectableID ILookup

        DetectableID detectableID;
        DetectableID ILookUp<IDetectable, DetectableID>.ID
        {
            get
            {
                return detectableID;
            }
        }

        DetectableID ILookUp<IDetectable, DetectableID>.GetUniqueID()
        {
            return Detectable.GetUniqueID();
        }

      
        void ILookUp<IDetectable, DetectableID>.AddToLookup()
        {
            detectableID = ((ILookUp<IDetectable, DetectableID>)this).GetUniqueID();

            if (detectableID != DetectableID.Invalid)
            {
                LookUpIDetectables.Add(detectableID, this); // uses special class!
            }
        }

        void ILookUp<IDetectable, DetectableID>.RemoveIDEntry()
        {
            LookUpIDetectables.Remove(this);  // uses special class!
        }

        void ILookUp<IDetectable, DetectableID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IDetectable, DetectableID>.SetInvalid()
        {
            detectableID = DetectableID.Invalid;
        }

        #endregion
        */
       
    }
}
