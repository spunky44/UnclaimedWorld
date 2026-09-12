using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using GameStateManagement;
using UWGame.SimSide.AI;
using UWGame.ClientSide;
using UWGame.Control;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Resources
{
   
    public class TileResourceContainer : ResourceContainer, ISleepingUpdatable //, IDetectable 
    {
      //  private Point mapPosition;

        public TerrainTile TerrainTile;
        TerrainTileID snapshotTerrainTile;
        

        private Renderable renderable;

        // then gather/batch from MapResourceRenderer when drawing
        public override Renderable Renderable
        {
            get { return renderable; }
        }

        #region "Client" fields

        Renderable.SnapshotRenderable snapshotRenderable;

        #endregion

      /*  public DetectableID DetectableID
        {
            get
            {
                return detectableID;
            }
        }*/

        public override EntityType EntityType
        {
            get { return null;  } 
        }

        public override Entity ParentEntity
        {
            get { return null; }
        }

        public override bool IsIntelligent 
        {
            get { return false; } 
        }

        public override Vector3 AccessPoint
        {
            get
            {
                return Location;

            }
        }

        public override Point MapPosition
        {
            get
            {
                return new Point(TerrainTile.X, TerrainTile.Y);
            }
        }

        public override Vector3 Location
        {
            get
            {
                return MapManager.TileToWorldPos(new Point(TerrainTile.X, TerrainTile.Y));
            }
        }

        private List<IResourceItem> resourceItems = new List<IResourceItem>();
        public override List<IResourceItem> ResourceItems
        {
            get
            {
                return resourceItems;
            }
        }
        private List<ResourceItemID> snapshotResourceItems;

        private bool IsRenderedWithSprite()
        {
            return resourceType.TileResourceType.IsRenderedWithSprites;

           /* return resourceType.TileResourceType.RenderableType != null
                && resourceType.TileResourceType.RenderableType.Default != null
                && resourceType.TileResourceType.RenderableType.Default.RenderAsGroundSpriteType != null;*/
          
        }

        public TileResourceContainer(TerrainTile parent, ResourceType type)
            : base(type)
        {
            ((ILookUp<IDetectable, DetectableID>)this).AddToLookup();

            this.TerrainTile = parent;

            CreateRenderable();

            if (ResourceType.CanReplenish())
            {
                The.Sim.PlaySite.PlaySite.AddTileResourceContainer(this); // start receiving updates               
            }

          //  parent.TileResources.Add(resourceKeyName, this);
            parent.TileResources.Add(type, this);

            The.Sim.PlaySite.AddResourceContainer(this);
        }

        private void CreateRenderable()
        {           
           // renderable = RenderableFactory.Produce(null, ResourceType.TileResourceType.RenderableType, snapshotRenderable);
            renderable = RenderableFactory.Produce(null, ResourceType.TileResourceType.RenderableTypeMode, snapshotRenderable);

            Renderable.SetPulsing(ClientSide.Renderables.Renderable.AdditionalEffect.Outline);

            Vector3 loc = MapManager.TileToWorldPos(new Point(TerrainTile.X, TerrainTile.Y));
            Vector3 scatterOffset = Vector3.Zero;

            if (IsRenderedWithSprite())
            {
                scatterOffset = new Vector3((float)Math.Sin(loc.Y) * 10f, (float)Math.Cos(loc.X) * 10f, 0);
              
                Renderable.SetSpriteStateFlag(RenderableType.GetRandomFlavour(this.ResourceType.TileResourceType.MaxFlavours));
            }

            Renderable.Location = loc + scatterOffset;

        }

        public TileResourceContainer()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }


        public override void Destroy()
        {
            base.Destroy();

            ((ILookUp<IDetectable, DetectableID>)this).RemoveIDEntry();
        }

        private float totalHarvestableBulk;

        /// <summary>
        /// setting this will cause the resource item list to be repopulated.
        /// </summary>
        public override float TotalHarvestableBulk 
        {
            get 
            { 
                return totalHarvestableBulk; 
            }
          /*  set
            {                                
                totalHarvestableBulk = value;
                                
                CreateResourceItemsFromTotals();

                UpdateSpritesToUse(value);
                if (Renderable != null)
                {
                    if (totalHarvestableBulk <= 0)
                    {
                        Renderable.IsAllowedToRenderOverlay = false;
                    }
                    else
                    {
                        Renderable.IsAllowedToRenderOverlay = true;
                    }
                }
            }*/
        }

        public void SetTotalHarvestableBulk(float value)
        {
            float oldValue = totalHarvestableBulk;

            totalHarvestableBulk = value;

            CreateResourceItemsFromTotals();

            UpdateSpritesToUse(oldValue); //value);

        }
              
        

        private void UpdateSpritesToUse(float? oldValue)
        {
            if (!IsRenderedWithSprite())
                return; // rendered with a standard icon instead of sprites
                        
            if (NoOfHarvestableItems == 0)
            {
                renderable.ClearSpriteStateFlag(StateModifier.Less); // set to None
                renderable.ClearSpriteStateFlag(StateModifier.More); // set to None
              
                //renderable.RenderAsGroundSprite.Redraw(null);
            }
            else
            {
                bool selectNewSprite = false;

                bool bulkIsLessThanLimit = totalHarvestableBulk < ResourceType.TileResourceType.MoreSpriteLimit;

                if (oldValue.HasValue)
                {
                    bool bulkWasLessThanLimit = oldValue.Value < ResourceType.TileResourceType.MoreSpriteLimit;

                    if (bulkWasLessThanLimit != bulkIsLessThanLimit)
                        selectNewSprite = true;
                }
                else
                {
                    selectNewSprite = true;
                }

               
                if (selectNewSprite) 
                {
                    // how to flip??
                  //  renderable.FlipHorizontally = The.Sim.GameplayRandomGenerator.NextDouble("TileResourceContainer") > 0.5;

                    if (bulkIsLessThanLimit)
                    {
                        renderable.SetSpriteStateFlag(StateModifier.Less);
                    }
                    else
                    {
                        renderable.SetSpriteStateFlag(StateModifier.More);
                    }
                }
            }
        }


        /// <summary>
        /// creates a list of n resource items, used when populating the map
        /// </summary>
        /// <param name="noOfItems"></param>
        public override void SetResourceItems(int noOfItems)
        {           
            int currentItems = resourceItems.Count;

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
        }


        /// <summary>
        /// what about jobs/goals - is it safe?
        /// </summary>
        /// <param name="amounttoRemove"></param>
        public override void RemoveResourceItems(int amounttoRemove)
        {
            while (amounttoRemove > 0 && resourceItems.Count > 0)
            {
                IResourceItem item = resourceItems[resourceItems.Count - 1];

                item.Destroy();

                resourceItems.RemoveAt(resourceItems.Count - 1);

                amounttoRemove--;
            }

            UpdateBulkAndSprites(resourceItems.Count);
           
        }

        public override void AddResourceItems(int noOfItemsToAdd)
        {            
            for (int i = 0; i < noOfItemsToAdd; i++)
            {
                resourceItems.Add(new TileResourceItem(this));
            }


            UpdateBulkAndSprites(resourceItems.Count);
        }

        protected override void UpdateBulkAndSprites(int noOfItems)
        {
            base.UpdateBulkAndSprites(noOfItems);

            SetTotalHarvestableBulk(ResourceType.ResourceItemType.ItemType.MaximumBulk.Value * resourceItems.Count);

            UpdateSpritesToUse(null);
        }


        /// <summary>
        /// creates a list of n resource items, used when populating the map
        /// </summary>
        /// <param name="noOfItems"></param>
      /*  public override void SetResourceItems(int noOfItems)
        {
            resourceItems.Clear();

            for (int i = 0; i < noOfItems; i++)
            {
                resourceItems.Add(new TileResourceItem(this));

            }

            SetTotalHarvestableBulk(ResourceType.ResourceItem.ItemType.MaximumBulk.Value * noOfItems);

            UpdateSpritesToUse(null);
        }*/

       

       
        public override IResourceItem FindHarvestableItem()
        {
            return resourceItems[0];
        }

        public override int NoOfHarvestableItems
        {
            get
            {
                // what about 'pile item types?'
                return resourceItems.Count; // (int)(TotalHarvestableBulk / TileResourceType.ResourceItem.ItemType.Bulk);
            }
        }

        public override bool IsDestroyed(SharedKnowledge knowledge) 
        { 
            // never destroyed??? not for now. perhaps later when we implement forest fires and such...
            return false;             
        }


        /// <summary>
        /// TODO: Deprecate this. Evaluators should only use AccessPoint.
        /// The agent goal should be ArriveAsVisitor, which handles slots for agents, approaches etc.
        /// </summary>
        /// <param name="movemap"></param>
        /// <param name="fromLocation"></param>
        /// <param name="closestLocation"></param>
        /// <returns></returns>
        public override bool GetClosestAccessibleHarvestLocation(SubtileLayers movemap, Vector3 fromLocation, out Vector3? closestLocation)
        {
            Point? closestSubtilePosition;
            if (The.Map.GetClosestAccessiblePoint(movemap, fromLocation, Location, false, out closestSubtilePosition))
            {
                closestLocation = MapManager.SubTileToWorldPos3(closestSubtilePosition.Value);
                closestLocation = MapManager.VaryLocationWithinSubtile(closestLocation.Value);

                return true;
            }
            else
            {
                closestLocation = null;
                return false;
            }
        }

        public override bool GatherResource(IResourceItem resourceItem)
        {
            // how do we harvest pile item types?

            if (resourceItems.Contains(resourceItem))
            {
                resourceItems.Remove(resourceItem);

                SetTotalHarvestableBulk(resourceItems.Count * ResourceType.ResourceItemType.ItemType.MaximumBulk.Value);
            
            //    RecalculateTotals();

                return true;
            }
            else return false;
        }


      /*  private void RecalculateTotals()
        {
            TotalHarvestableBulk = resourceItems.Count * TileResourceType.ResourceItem.ItemType.Bulk.Value;
                        
        }*/




        /// <summary>
        /// updates the list of resource items with changes to the total
        /// </summary>
        private void CreateResourceItemsFromTotals()
        {
            int noOfNeededResourceItems = ResourceType.GetHarvestableItemsFromBulk(totalHarvestableBulk); 

            if (resourceItems.Count < noOfNeededResourceItems)
            {
                for (int i = 0; i < noOfNeededResourceItems - resourceItems.Count; i++)
			    {
                    resourceItems.Add(new TileResourceItem(this));
			    }
            }
            else if (resourceItems.Count > noOfNeededResourceItems)
            {
                // TODO: remove some items... preferably ones that have not been targeted for harvesting...

            }
        }

     /*   public void CopyQuadToVertexBuffer(VertexGroundFeature[] featureVertices, ref int index)
        {
            if (isDirty)
            {
                SetupQuadVertices();
            }

            if (SourceRect.HasValue)
            {

                quad.CopyQuadToVertexBuffer(featureVertices, index);

                index++;
            }
        }*/


        /*  public Vector2 baseCenterOffset;
      public void SetIsDirty()
        {
            isDirty = true;
        }*/

        /*
        public void SetupQuadVertices()
        {
            if (TileResourceSpriteName != null)
            {
                SourceRect = The.Client.FlatSpriteSheet.GetSourceRectangle(TileResourceSpriteName);

                

                // for others - use the center of the sprite
                baseCenterOffset = new Vector2(SourceRect.Value.Width / 2f + SpriteOffset.X, SourceRect.Value.Height / 2f + SpriteOffset.Y); //quad.StaticSourceRectangle.Width / 2f, quad.StaticSourceRectangle.Height / 2f);


                quad.SetupQuadVertices(Location,
                                            baseCenterOffset,
                                            SourceRect.Value,
                                            The.Client.FlatSpriteSheet.Texture, Tint);
            }
            else
            {
                SourceRect = null;
            }

            isDirty = false;
        }*/

    

      /*  public bool IsEatable(EntityType inquiringEntityType)
        {
            return true;
        }*/

        public override bool UsesMemory(SharedKnowledge sharedKnowledge)
        {
            return true;
        }

       /* public void SeeByAllegiance(Allegiances.Allegiance allegiance)
        {
            if (IsEatable(allegiance.RepresentativeEntityType))
            {
                
            }
        }*/

        public override bool RequiresRollToDetect()
        {
            return true;
        }

               
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

        #region ISleepingUpdatable


        /// <summary>
        /// expiry
        /// </summary>
        public double? TimePointInSeconds
        {
            get;
            private set;
        }

        public void SetNextTimepoint(double? timepoint)
        {
            TimePointInSeconds = timepoint;
        }


        public double? UpdateInterval
        {
            get 
            { 
                return GetUpdateInterval(); 
            } 
        }

        void ISleepingUpdatable.CreateSleepyLookupCollection()
        {

        }

        public static void CreateSleepyLookupCollection()
        {
            LookUpSleepyUpdater<TileResourceContainer>.Create();
        }


        public SleepyUpdaterID SleepyUpdater { get; set; }

        public void Update(GameTime gameTime, out bool wasDestroyed)
        {
            base.Update(gameTime);

            wasDestroyed = false;
            
        }


      
        /*
        private void Destroy()
        {
            The.Client.DestroyAccessibility(this);
        }*/

        public void RecomputeUpdateInterval(out bool intervalChanged)
        {
            // never change the expiry interval.
            intervalChanged = false;

            /*
            if (!Common.IsEqual(UpdateInterval, currentInterval))
            {
                UpdateInterval = currentInterval; // if changed, will alert the sleepy updater to resort the list

                intervalChanged = true;
            }*/
        }


        #endregion


        #region ISnapshot

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);                     

           // this.detectableID = sn.DoEnum(detectableID);
        
            this.snapshotResourceItems = resourceItems.Select(r => r.ID).ToList();
            this.snapshotResourceItems = sn.DoList(snapshotResourceItems);
            this.snapshotTerrainTile = (TerrainTileID)sn.SnapshotID<TerrainTile, TerrainTileID>(TerrainTile);
            this.totalHarvestableBulk = sn.DoFloat(totalHarvestableBulk);

            this.TimePointInSeconds = sn.DoDoubleNullable(TimePointInSeconds);       
            this.SleepyUpdater = sn.DoEnum(SleepyUpdater);
          

            if (sn.mode != Snapshotter.Mode.Load
                && The.Client != null
                && Renderable != null)
            {
                snapshotRenderable = Renderable.GetFieldsToSnapshot(); // ugly, but necessary..
            }
            snapshotRenderable = (Renderable.SnapshotRenderable)sn.DoISnapshot(snapshotRenderable);



            sn.Ignore(resourceItems);

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

            resourceItems = snapshotResourceItems.Select(r => LookUp<IResourceItem, ResourceItemID>.FindByID(r)).ToList();
            TerrainTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotTerrainTile);


            CreateRenderable();
        }

        #endregion
    }
}
