using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Maps;
using System.Xml.Serialization;
using GameStateManagement;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Resources;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide.Map;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Trees
{
    public enum AgeGroup { Young, Grown }
    public enum InSeason { Summer, Winter }


    public class Tree : Component, IHasCrops   
    {

        public Dictionary<ResourceType, Crop> Crops; 
        Dictionary<ResourceType, ResourceID> snapshotCrops;

        public Tree()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }
       
        
       
        private InSeason inSeason = InSeason.Summer;

        /// <summary>
        /// TODO: the tree should decide based on its type and the date whether it is in season or not...
        /// let the Entity.Update() ask for a next update time point (should be staggered!) for the SleepyUpdater
        /// </summary>
        public InSeason InSeason
        {
            get { return inSeason; }
            set
            {
                // update sprites and blocked areas
                if (inSeason != value)
                {
                    inSeason = value;

                    UpdateRenderableSeason();

                }
            }
        }

        /// <summary>
        /// can be called at any time...
        /// </summary>
        public void UpdateRenderable()
        {
            UpdateRenderableSeason();
            UpdateRenderableSize();
            UpdateRenderableFlavour();

            if (Parent.EntityType.TreeType.CropTypes != null)
            {
                // trees need a pulsing effect on the renderable:
                Parent.Renderable.SetPulsing(ClientSide.Renderables.Renderable.AdditionalEffect.Outline);           
            }
        }

        private void UpdateRenderableSeason()
        {
            if (Parent.Renderable != null)
            {
                Parent.Renderable.SetOrClearSpriteStateFlag(inSeason == Trees.InSeason.Winter, StateModifier.Winter);
            }
        }

        private void UpdateRenderableFlavour()
        {
            if (Parent.Renderable != null)
            {
                switch (flavour)
                {
                    case 1:
                        Parent.SetSpriteStateFlag(StateModifier.Flavour1);
                        break;
                    case 2:
                        Parent.SetSpriteStateFlag(StateModifier.Flavour2);
                        break;
                    case 3:
                        Parent.SetSpriteStateFlag(StateModifier.Flavour3);
                        break;
                    default:
                        Parent.SetSpriteStateFlag(StateModifier.Flavour1);
                        break;
                }
            }
        }

        private void UpdateRenderableSize()
        {
            if (Parent.Renderable != null)
            {
                SizeType newSizeType = GetSize();

                if (newSizeType == SizeType.Young)
                {
                    Parent.SetSpriteStateFlag(StateModifier.Young);
                }
                else
                {
                    Parent.ClearSpriteStateFlag(StateModifier.Young);
                }
            }
        }


        private float? ageInYears;
        /// <summary>
        /// in years
        /// </summary>
        public float AgeInYears
        {
            get
            {
                return ageInYears.Value;
            }
            set
            {
                ageInYears = value;
            }
        }
        
        // distort width/height by factor:
        private float? shapeFactor;
        public float ShapeFactor
        {
            get
            {
                return shapeFactor.Value;
            }
            set 
            {
                shapeFactor = value;
            }
        }

     //   public bool IsMapEditorPlaced = false;

        /// <summary>
        /// the size where the sprite is used at normal scale
        /// </summary>
        private const float sizeForUsingMatureSprite = 1f;
        public const float sizeForUsingYoungSprite = 0.25f;

        private const float maxMatureSpriteScaling = 0.1f;
        private const float maxYoungSpriteScaling = 0.1f;

        // a stable age distribution...
        private static readonly float[] ageProbabilities = new float[] { 0.2f, 0.4f, 0.6f, 0.8f, 0.88f, 0.95f, 0.98f, 0.99f };


      //  private SizeType sizeType = SizeType.Young; 

        private float? size;
        /// <summary>
        /// 0 - unlimited. 1 = size at maturity on average.
        /// We compute Bulk from Size! Set Size before Initialize for Bulk to be properly computed.
        /// </summary>
        public float Size
        {
            get { return size.Value; }
            set
            {
                if (value != size)
                {
                    size = value;

                    UpdateRenderableSize();

                    /* OLD
                    if (sizeType == SizeType.Young)
                    {
                        // we cannot shrink...
                        SizeType newSizeType = GetSize();

                        if (newSizeType != sizeType)
                        {
                            sizeType = newSizeType;

                            RedrawBillboard();
                        }
                    }                                                                           
                    */
                    
                }
            }
        }

       



        // [XmlIgnore]
        // protected FeatureQuad quad = new FeatureQuad();

       // public bool FlipHorizontally = false;

        // seed for waving animation
        // private float randomValue;

        private int? flavour;
        /// <summary>
        /// append number to sprite name to get the correct sprite...
        /// </summary>
        public int Flavour
        {
            get
            {
                return flavour.Value;
            }
            set
            {
                if (flavour != value)
                {
                    flavour = value;

                    UpdateRenderableFlavour();
                }

            }
        }

       

        

       
        /// <summary>
        /// related to Size by a factor specific to the species
        /// This Bulk is excluding Crops!!!
        /// 
        /// merged with Entity.Bulk.
        /// </summary>
        /* public float Bulk
         {
             get 
             { 
                 return bulk; 
             }
             set 
             {
                 bulk = value;                
             }
         }*/


        #region IHasCrops

        public Point MapPosition
        {
            get
            {
                return Parent.MapPosition.Value;
            }
        }

        public Vector3 AccessPoint
        {
            get
            {
                return Parent.AccessPoint.Value;
            }
        }

        public Vector3 Location
        {
            get
            {
                return Parent.PlaySiteLocation;
            }
        }


        #endregion

        public void UpdateBulk()
        {

            ComputeSizeFromBulk();

           
            // block the edge for movement:
            // also for vehicles? depends on bulk of tree
            // { Foot = 0, Car = 1, ATV = 2, Air = 3 };
            //float bulkOfTree = Size * Parent.EntityType.TreeType.BulkPerSize;

            //Bulk = Size * Parent.EntityType.TreeType.BulkPerSize;


            BlockedTransport newBlockedState;


            if (Parent.Bulk >= GameData.Instance.Constants.BulkOfTreeBlockingFootTransport)
            {
                newBlockedState = BlockedTransport.Foot;
            }
            else if (Parent.Bulk >= GameData.Instance.Constants.BulkOfTreeBlockingATVTransport)
            {
                newBlockedState = BlockedTransport.OffRoad;
            }
            else if (Parent.Bulk >= GameData.Instance.Constants.BulkOfTreeBlockingCarTransport)
            {
                newBlockedState = BlockedTransport.Car;
            }
            else
            {
                newBlockedState = BlockedTransport.None;
            }

            if (newBlockedState != blockedTransportState)// we have changed, clear the old costs
            {
                // set the new state before redraw gets called in Clear():
                blockedTransportState = newBlockedState;

                if (Parent.HasBeenPlaced())
                {
                    // clear and redraw the neighbours as well as ourselves!!!   

                    if (Parent.PointLayout != null)
                    {
                        Parent.PointLayout.ClearTerrainCosts(true);
                    }
                   /* else if (Parent.TileLayout != null)
                    {
                        Parent.TileLayout.ClearTerrainCosts(true,
                            Parent.EntityType.TileLayoutType.GetBlockedEdges(Parent.FlipHorizontally)[GetUtilityMap()]);
                    }*/


                }
                /* else
                 {
                     blockedTransportState = newBlockedState;
                     RedrawTerrainCosts(); // not necessary to clear and redraw neighbours
                 }*/

            }
        }

    


        private enum SizeType {Young, Grown}
        private SizeType GetSize()
        {
            if (Size < sizeForUsingMatureSprite - maxMatureSpriteScaling)
            {
                return SizeType.Young;
            }
            else
            {
                return SizeType.Grown;
            }
        }


        private void ComputeSizeFromBulk()
        {
            Size = Parent.Bulk / Parent.EntityType.TreeType.BulkPerSize;           
        }

      
       
        public Tree(Entity parent) : base(parent)
        {

            if (The.Sim.GameplayRandomGenerator.Next(100,"Tree") > 50)
            {
                Parent.FlipHorizontally = true;                
            }

            ((ILookUp<IHasCrops, HasCropsID>)this).AddToLookup();

        }

        public void SetAgePreInit(float age)
        {
            AgeInYears = age;
        }

        public void SetAgePreInit(AgeGroup ageGroup)
        {
            SetRandomAge(ageGroup);
        }

        public void SetAgePreInit()
        {
            SetRandomAge();
        }

       /* public void SetAgePreInit(float? age) 
        {
            if (age.HasValue)
            {               
                AgeInYears = age.Value;
            }
            else
            {              
                SetRandomAge();
            }
        }*/

        //public override void Update(GameTime gameTime)
        public override void UpdatePlaySite(GameTime gameTime)
        {
            if (Crops != null)
            {
                foreach (var item in Crops)
                {
                    item.Value.Update(gameTime);
                }
            }                     

        }


        public override double? GetUpdateInterval()
        {
           
            double? tempInterval = null, currentInterval = null;

            if (Crops != null)
            {
                foreach (var item in Crops)
                {
                    tempInterval = item.Value.GetUpdateInterval();
                    UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
                }
            }

            return currentInterval;
        }


        public void Initialize()
        {
            if (!ageInYears.HasValue)
            {
                SetRandomAge();
            }

            if (!size.HasValue)
            {
                SetRandomSize(AgeInYears);
            }
           
           
            if (!flavour.HasValue)
            { // pick a flavour...
                SetRandomFlavour();
            }

            if (!shapeFactor.HasValue)
            {
                SetRandomShapeFactor();
            }

            Parent.Bulk = Parent.EntityType.TreeType.BulkPerSize * Size;
            

            if (Parent.EntityType.TreeType.CropTypes != null)
            {
                Crops = new Dictionary<ResourceType, Crop>();

                foreach (ResourceType cropType in Parent.EntityType.TreeType.CropTypes)
                {                  
                    Crop crop = new Crop(this, cropType);
                   // crop.CropBulkGrowthPerDay = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(cropType.CropType.CropBulkGrowthPerDayMean, cropType.CropType.CropBulkGrowthPerDayStandardDeviation);
                    Crops.Add(cropType, crop);

                }
            }


            if (Crops != null)
            {
                foreach (KeyValuePair<ResourceType, Crop> kvp in Crops)
                {
                    // store useful trees for AI convenience (all trees are known!):
                    Parent.Site.AddResourceContainer(kvp.Value);

                 /*   ObservableList<ResourceContainer> listOfContainers;
                    if (!Parent.Site.Resources.TryGetValue(kvp.Value.ResourceType, out listOfContainers))
                    {                     
                        listOfContainers = new ObservableList<ResourceContainer>();
                        Parent.Site.Resources.Add(kvp.Value.ResourceType, listOfContainers);
                    }                  
                    listOfContainers.Add(kvp.Value);*/
                }
            }
        }

        private void SetRandomFlavour()
        {
            if (Parent.EntityType.TreeType.MaxFlavours > 1)
            {
                Flavour = The.Sim.GameplayRandomGenerator.Next(1, Parent.EntityType.TreeType.MaxFlavours + 1,"Tree");
            }
            else
            {
                Flavour = 1;
            }
        }

        private void SetRandomShapeFactor()
        {
            ShapeFactor = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(1.0, 0.03); // we want a max 'spread' of 0.10 to each side - divide by 3.
            ShapeFactor = MathHelper.Clamp(ShapeFactor, 0.9f, 1.1f);
        }

        private void SetRandomAge()
        {
            int ageBucketIndex = Common.GetStairStepIndex((float)The.Sim.GameplayRandomGenerator.NextDouble("Tree"), ageProbabilities);

            float sizeOfBucket = Parent.EntityType.TreeType.MaxAge / ageProbabilities.Length;

            AgeInYears = (ageBucketIndex + (float)The.Sim.GameplayRandomGenerator.NextDouble("Tree")) * sizeOfBucket;
        }

        private void SetRandomAge(AgeGroup ageGroup)
        {
            float matureAge = Parent.EntityType.TreeType.MatureAge;
            float maxAge = Parent.EntityType.TreeType.MaxAge;
            float age;
            if (ageGroup == AgeGroup.Young)
            {
                age = (float)The.Sim.GameplayRandomGenerator.NextDouble("Tree") * matureAge;
                age = Common.Clamp(age, Common.floatEpsilon, matureAge - Common.floatEpsilon);
            }
            else
            {   // mature
                float sizeOfBucket = maxAge / ageProbabilities.Length;

                int startBucketIndex = (int)(matureAge / sizeOfBucket);

              //  float ageFractionOfMaxAge = MathHelper.Lerp(matureAge / maxAge * sizeOfBucket, 1f, (float)The.Sim.GameplayRandomGenerator.NextDouble("Tree"));
                float ageFractionOfMaxAge = MathHelper.Lerp(ageProbabilities[startBucketIndex], 1f, (float)The.Sim.GameplayRandomGenerator.NextDouble("Tree"));
                        

                int ageBucketIndex = Common.GetStairStepIndex(ageFractionOfMaxAge, ageProbabilities);                               
                
                age = (ageBucketIndex + (float)The.Sim.GameplayRandomGenerator.NextDouble("Tree")) * sizeOfBucket;
                age = Common.Clamp(age, matureAge + Common.floatEpsilon, maxAge);
            }

            AgeInYears = age;
        }

        public void SetRandomSize(float age)
        {   // Function file: TreeAges
            if (age < Parent.EntityType.TreeType.MatureAge)
            {
                Size = MathHelper.SmoothStep(0.01f, 1f, age / Parent.EntityType.TreeType.MatureAge);
            }
            else
            {   // keep growing more slowly after maturity:
                Size = 0.05f * age + 0.94f;
            }
        }

        public void GetSizeScaling(ref float width, ref float height)
        {
            return;

            float sizeScaling;
            if (Size < sizeForUsingMatureSprite - maxMatureSpriteScaling)
            {               
                sizeScaling = MathHelper.Clamp(Size / sizeForUsingYoungSprite, 1f - maxYoungSpriteScaling, 1f + maxYoungSpriteScaling);
            }
            else
            {
               
                sizeScaling = MathHelper.Clamp(Size / sizeForUsingMatureSprite, 1f - maxMatureSpriteScaling, 1f + maxMatureSpriteScaling);
            }

            // TODO: Move this to RenderAsBillboard???
            width = width * ShapeFactor * sizeScaling;
            height = height / ShapeFactor * sizeScaling;

           
        }


        public void Place()
        {            
            The.Map.GetTile(Parent.MapPosition.Value).AddTree(Parent);

          /*  if (Parent.EntityType.TileLayoutType != null)
            {
                // we should probalby use Location here..?
                // TODO: light sources should be placed in Entity!
                Vector2 baseCenter = Parent.EntityType.TileLayoutType.GetBaseCenter(Parent.FlipHorizontally);
                Parent.Renderable.PlaceLightSources(baseCenter);          
            }*/
        }

        /// <summary>
        /// unblock terrain
        /// </summary>
        public void Destroy()
        {
            //UWGame.SimSide.Instance.Map.TileMap[parent.MapPosition.X, parent.MapPosition.Y].RemoveTree(parent, parent.DirectionalLayout.EdgePosition);
            The.Map.GetTile(Parent.MapPosition.Value).RemoveTree(Parent);
                        
            if (Crops != null)
            {
                // remove from convenience list
                foreach (KeyValuePair<ResourceType, Crop> kvp in Crops)
                {
                    //Parent.Site.CropTrees[kvp.Key].Remove(Parent);
                    Parent.Site.Resources[kvp.Key].Remove(kvp.Value);

                    kvp.Value.Destroy();

                }
            }

            ((ILookUp<IHasCrops, HasCropsID>)this).RemoveIDEntry();

        }

        /*
        public void ClearTerrainCosts(bool redraw)
        {
            UWGame.SimSide.Instance.Map.ResetEdgeCost(parent.MapPosition, parent.EdgeLayout.EdgePosition, TerrainType.TransportType.Foot);
            UWGame.SimSide.Instance.Map.ResetEdgeCost(parent.MapPosition, parent.EdgeLayout.EdgePosition, TerrainType.TransportType.OffRoad);
            UWGame.SimSide.Instance.Map.ResetEdgeCost(parent.MapPosition, parent.EdgeLayout.EdgePosition, TerrainType.TransportType.Car);

            if (redraw)
            {   // this redraws us as well...
                UWGame.SimSide.Instance.Map.TileMap[parent.MapPosition.X, parent.MapPosition.Y].RedrawTerrainCostsAroundEdge(parent.EdgeLayout.EdgePosition);
            }           

        }
        */
        

        
        /// <summary>
        /// it is most difficult to pass trees by car, then atv, then foot.
        /// </summary>
        private enum BlockedTransport { NotPlaced, None, Car, OffRoad, Foot }

        /// <summary>
        /// TODO: get rid of this...
        /// </summary>
        private BlockedTransport blockedTransportState = BlockedTransport.NotPlaced;

        PlantResourceNeeds plantNeeds, cropNeeds, sustainNeeds;

        /// <summary>
        /// TODO: replace with Process and water etc with Substances
        /// </summary>
        /// <param name="deltaTimeInSeconds"></param>
        /// <param name="water"></param>
        /// <param name="nitrogen"></param>
        /// <param name="phosphorous"></param>
        public void GetGrowthNeeds(double deltaTimeInSeconds, out float water, out float nitrogen, out float phosphorous)
        {
           // TODO: 
            // the plant will have a "growth budget" that depends on energy/nutrients/water uptake
            // it can dedicate it to either "plant" growth or crops growth (seeds), or fighting parasites (poison, spikes?)
            // the dedication to crops depends on plant age and season.
            // it will also have to use some to sustain itself. Otherwise it will die...

            // we can't alter tile resources in parallel! that's why we grow the trees from the tile's update method.
            float currentWater = 0f, currentNitrogen = 0f, currentPhosphorous = 0f;

            // TODO: store the needs so we can distribute what we get correctly...
            if (Crops != null)
            {
                GetCropGrowthNeeds(deltaTimeInSeconds, out currentWater, out currentNitrogen, out currentPhosphorous);

                cropNeeds = new PlantResourceNeeds() { Water = currentWater, Nitrogen = currentNitrogen, Phosphorous = currentPhosphorous };
            }
            else
            {
                cropNeeds = null;
            }

            water = currentWater;
            nitrogen = currentNitrogen;
            phosphorous = currentPhosphorous;

            GetPlantGrowthNeeds(deltaTimeInSeconds, out currentWater, out currentNitrogen, out currentPhosphorous);

            plantNeeds = new PlantResourceNeeds() { Water = currentWater, Nitrogen = currentNitrogen, Phosphorous = currentPhosphorous };

            water += currentWater;
            nitrogen += currentNitrogen;
            phosphorous += currentPhosphorous;

            GetNeedsForPestsAndSelfSustainment(deltaTimeInSeconds, out currentWater, out currentNitrogen, out currentPhosphorous);

            sustainNeeds = new PlantResourceNeeds() { Water = currentWater, Nitrogen = currentNitrogen, Phosphorous = currentPhosphorous };

            water += currentWater;
            nitrogen += currentNitrogen;
            phosphorous += currentPhosphorous;

          /*  water = 0;

            nitrogen = 0f;

            phosphorous = 0f;*/
        }

        private void GetPlantGrowthNeeds(double deltaTimeInSeconds, out float water, out float nitrogen, out float phosphorous)
        {
            // return the maximum resources we can use for plant growth. Determined by species.

            float maxBulkGrowth = (float)(deltaTimeInSeconds * Parent.EntityType.TreeType.BulkGrowthSpeedInSeconds);

            // TODO: modify by available sunlight (crowding?) etc.

            water = maxBulkGrowth * Parent.EntityType.TreeType.WaterNeedsPerBulk;
            nitrogen = maxBulkGrowth * Parent.EntityType.TreeType.NitrogenNeedsPerBulk;
            phosphorous = maxBulkGrowth * Parent.EntityType.TreeType.PhosphorousNeedsPerBulk;

        }

        /// <summary>
        /// the plant will shrink/get sick if this need is not satisfied!
        /// </summary>
        /// <param name="deltaTimeInSeconds"></param>
        /// <param name="water"></param>
        /// <param name="nitrogen"></param>
        /// <param name="phosphorous"></param>
        private void GetNeedsForPestsAndSelfSustainment(double deltaTimeInSeconds, out float water, out float nitrogen, out float phosphorous)
        {
            // perhaps there should be an energy store...?

            float bulkNeededForSelfSustainment = (float)(deltaTimeInSeconds * Parent.EntityType.TreeType.SelfSustainmentNeedsInBulkPercentagePerSecond * Parent.Bulk);

            water = bulkNeededForSelfSustainment * Parent.EntityType.TreeType.WaterNeedsPerBulk;
            nitrogen = bulkNeededForSelfSustainment * Parent.EntityType.TreeType.NitrogenNeedsPerBulk;
            phosphorous = bulkNeededForSelfSustainment * Parent.EntityType.TreeType.PhosphorousNeedsPerBulk;

            //float maxBulkGrowth = (float)(deltaTimeInSeconds * Parent.EntityType.TreeType.BulkGrowthSpeedInSeconds);

           // (float)(deltaTimeInSeconds * Parent.EntityType.TreeType.BulkGrowthSpeedInSeconds * );

        }

        private void GetCropGrowthNeeds(double deltaTimeInSeconds, out float water, out float nitrogen, out float phosphorous)
        {
            water = 0f;
            nitrogen = 0f;
            phosphorous = 0f;

            float thisCropWater, thisCropNitrogen, thisCropPhosphorous;

            foreach (KeyValuePair<ResourceType, Crop> crop in Crops)
            {
                crop.Value.GetGrowthNeeds(deltaTimeInSeconds, out thisCropWater, out thisCropNitrogen, out thisCropPhosphorous, AgeInYears, Parent.Bulk);  

                water += thisCropWater;
                nitrogen += thisCropNitrogen;
                phosphorous += thisCropPhosphorous;
            } 
        }
        
        public void Grow(ref float water, ref float nitrogen, ref float phosphorous)
        { 
         
            // TODO: see if we were allocated less than what we need (shortfall):

            // 
            // plantNeeds, cropNeeds...

            float totalGrowthBudget;

            //float 

           // float waterForPlant = 

            // grow the tree...
            Parent.Bulk += ProducePlantBulk(ref water, ref nitrogen, ref phosphorous);
           
          
            if (Crops != null)
            {
                foreach (KeyValuePair<ResourceType, Crop> kvp in Crops)
                {
                    kvp.Value.GrowAndRipen(AgeInYears, Parent.Bulk); 
                }
            }            
        }

        private float ProducePlantBulk(ref float water, ref float nitrogen, ref float phosphorous)
        {
            // first find the limiting factor in growth:
            float bulkToGrowFromPhosphorous = phosphorous / Parent.EntityType.TreeType.WaterNeedsPerBulk;
            float bulkToGrowFromWater = water / Parent.EntityType.TreeType.PhosphorousNeedsPerBulk;
            float bulkToGrowFromNitrogen = nitrogen / Parent.EntityType.TreeType.NitrogenNeedsPerBulk;

            float bulkToGrow = 0f;

            if (bulkToGrowFromPhosphorous < bulkToGrowFromWater)
            {
                if (bulkToGrowFromPhosphorous < bulkToGrowFromNitrogen)
                {
                    bulkToGrow = bulkToGrowFromPhosphorous;
                }
                else
                {
                    bulkToGrow = bulkToGrowFromNitrogen;
                }
            }
            else
            {
                if (bulkToGrowFromWater < bulkToGrowFromNitrogen)
                {
                    bulkToGrow = bulkToGrowFromWater;                   
                }
                else
                {
                    bulkToGrow = bulkToGrowFromNitrogen;            
                }
            }

            // consume only what we need
            Consume(bulkToGrow, ref water, ref nitrogen, ref phosphorous);

            return bulkToGrow;
        }

        private void Consume(float bulkToGrow, ref float water, ref float nitrogen, ref float phosphorous)
        {
            water = water - bulkToGrow * Parent.EntityType.TreeType.WaterNeedsPerBulk;
            nitrogen = nitrogen - bulkToGrow * Parent.EntityType.TreeType.NitrogenNeedsPerBulk;
            phosphorous = phosphorous - bulkToGrow * Parent.EntityType.TreeType.PhosphorousNeedsPerBulk;
        }
       

        public void LoseMass(double deltaTimeInSeconds, out float fibrousMaterial, out float nonFibrousMaterial)
        {
            // this represents parts of the tree (branches + leaves) falling to the ground to rot or convert into firewood on the surface.
            // it would be cool if a storm could rip tree branches off...

            // also shed leaves here...
            
            // do a random toss to see how much material we shed... modify by wind factor
            
            float bulkToRemove;
            float randomFactor = The.Sim.GameplayRandomGenerator.RandomBetween(0.5f, 1.2f);
                       
            //f
            bulkToRemove = (float)(deltaTimeInSeconds * randomFactor * Parent.EntityType.TreeType.MassLossPercentagePerSecond * Parent.Bulk);

            if (Parent.EntityType.TreeType.FibrousPercentageOfTotalMass > 0f)
            {
                float firewoodGainFactor = The.Sim.GameplayRandomGenerator.RandomBetween(0.5f, 1.2f);

                fibrousMaterial = Parent.EntityType.TreeType.FibrousPercentageOfTotalMass * bulkToRemove;
            }
            else 
            {
                fibrousMaterial = 0f;
            }
            

            nonFibrousMaterial = bulkToRemove - fibrousMaterial;

            Parent.Bulk -= bulkToRemove;
        }

       
      

     /*   public byte GetCost(TerrainType.TransportType transport)
        {
            TerrainTile tile = UWGame.SimSide.Instance.Map.TileMap[parent.MapPosition.X, parent.MapPosition.Y];

            // set the new costs
            switch (blockedTransportState)
            {   // set the transport maps to either blocked or obstacle, depending on the bulk of the tree
                case BlockedTransport.Foot:
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.EdgeLayout.EdgePosition, (int)TerrainType.TransportType.Foot, 0);
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.EdgeLayout.EdgePosition, (int)TerrainType.TransportType.OffRoad, 0);
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.EdgeLayout.EdgePosition, (int)TerrainType.TransportType.Car, 0);
                    break;

            }
        }*/

        public void RedrawTerrainCosts()
        {
            TerrainTile tile = The.Map.GetTile(Parent.MapPosition.Value);
            
            // trees with GeoLayout?

            // set the new costs
            switch (blockedTransportState)
            {   // set the transport maps to either blocked or obstacle, depending on the bulk of the tree
                case BlockedTransport.Foot:
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Foot, 0);
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.OffRoad, 0);
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Car, 0);
                    break;
                case BlockedTransport.OffRoad:
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Foot,
                        tile.GetCost(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.Obstacle));
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.OffRoad, 0);
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Car, 0);
                    break;
                case BlockedTransport.Car:
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Foot,
                        tile.GetCost(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.Obstacle));
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.OffRoad,
                        tile.GetCost(SurfaceType.TransportType.OffRoad, SurfaceType.TerrainFeatures.Obstacle));
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Car, 0);
                    break;
                case BlockedTransport.None:
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Foot,
                        tile.GetCost(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.Obstacle));
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.OffRoad,
                        tile.GetCost(SurfaceType.TransportType.OffRoad, SurfaceType.TerrainFeatures.Obstacle));
                    The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Car,
                        tile.GetCost(SurfaceType.TransportType.Car, SurfaceType.TerrainFeatures.Obstacle));
                    break;
            }

            /*
            // set the new costs
            switch (blockedTransportState)
            {   // set the transport maps to either blocked or obstacle, depending on the bulk of the tree
                case BlockedTransport.Foot:
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.Foot, 0);
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.OffRoad, 0);
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.Car, 0);
                    break;
                case BlockedTransport.OffRoad:
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.Foot,
                        tile.GetCost(TerrainType.TransportType.Foot, TerrainType.TerrainFeatures.Obstacle));
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.OffRoad, 0);
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.Car, 0);
                    break;
                case BlockedTransport.Car:
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.Foot,
                        tile.GetCost(TerrainType.TransportType.Foot, TerrainType.TerrainFeatures.Obstacle));
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.OffRoad,
                        tile.GetCost(TerrainType.TransportType.OffRoad, TerrainType.TerrainFeatures.Obstacle));
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.Car, 0);
                    break;
                case BlockedTransport.None:
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.Foot,
                        tile.GetCost(TerrainType.TransportType.Foot, TerrainType.TerrainFeatures.Obstacle));
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.OffRoad,
                        tile.GetCost(TerrainType.TransportType.OffRoad, TerrainType.TerrainFeatures.Obstacle));
                    UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, parent.DirectionalLayout.EdgePosition, (int)TerrainType.TransportType.Car,
                        tile.GetCost(TerrainType.TransportType.Car, TerrainType.TerrainFeatures.Obstacle));
                    break;
            }*/
        }

        #region ISnapshot



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.hasCropsID = sn.DoEnum(hasCropsID);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                if (Crops != null)
                {                   
                    snapshotCrops = Crops.ToDictionary(k => k.Key, k => k.Value.ID);
                }
            }


            ageInYears = sn.DoFloatNullable(ageInYears);
            blockedTransportState = sn.DoEnum(blockedTransportState);

            // needs - not implemented
            //  cropNeeds = (PlantResourceNeeds)sn.DoObject_______NotYetSupported_______(cropNeeds); TODO?
            // this.plantNeeds = 

            this.snapshotCrops = sn.DoDictionary(snapshotCrops);            
            this.flavour = sn.DoInt32Nullable(flavour);
            this.inSeason = (Trees.InSeason)sn.DoEnum(inSeason);
            this.shapeFactor = sn.DoFloatNullable(shapeFactor);
            this.size = sn.DoFloatNullable(size);
           // this.sizeType = (SizeType)sn.DoEnum(sizeType);

            sn.Ignore(cropNeeds);
            sn.Ignore(plantNeeds);
            sn.Ignore(sustainNeeds);
            sn.Ignore(ageProbabilities);
            sn.Ignore(Crops);

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

            if (snapshotCrops != null)
            {
                Crops = snapshotCrops.ToDictionary(k => k.Key, k => (Crop)LookUp<ResourceContainer, ResourceID>.FindByID(k.Value));
                snapshotCrops = null;
            }
        }

        #endregion



        #region HasCropsID ILookup

        HasCropsID hasCropsID;
        HasCropsID ILookUp<IHasCrops, HasCropsID>.ID
        {
            get
            {
                return hasCropsID;
            }
        }

        HasCropsID ILookUp<IHasCrops, HasCropsID>.GetUniqueID()
        {
            return HasCrops.GetUniqueID();
        }

      
        void ILookUp<IHasCrops, HasCropsID>.AddToLookup()
        {
            hasCropsID = ((ILookUp<IHasCrops, HasCropsID>)this).GetUniqueID();

            if (hasCropsID != HasCropsID.Invalid)
            {
                LookUpIHasCrops.Add(hasCropsID, this); // uses special class!
            }
        }

        void ILookUp<IHasCrops, HasCropsID>.RemoveIDEntry()
        {
            LookUpIHasCrops.Remove(this);  // uses special class!
        }

        void ILookUp<IHasCrops, HasCropsID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IHasCrops, HasCropsID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUpIHasCrops.Create();
        }

        void ILookUp<IHasCrops, HasCropsID>.SetInvalid()
        {
            hasCropsID = HasCropsID.Invalid;
        }


        int ILookUp<IHasCrops, HasCropsID>.LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        #endregion

    }
}
