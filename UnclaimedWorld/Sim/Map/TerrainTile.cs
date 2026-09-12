using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Trees;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Soil;
using UWGame.SimSide.AI;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Resources;
using GameStateManagement;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Jobs;
using UWGame.ClientSide;
using UWGame.ClientSide.Log;
using UWGame.ClientSide.Renderables;
using UWGame.Control;
using UWGame.ClientSide.Map;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Maps 
{
    public enum TerrainLevel { Below, Middle, Plateau }

    public enum TerrainTileID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

    public class TerrainTile : IDrawnAsGroundSprite, //TODO DECOUPLE -- move this to client -- may have simish bits, too
        ISnapshot, ILookUp<TerrainTile, TerrainTileID>  
    {
      //  public int X;
      //  public int Y;

        public int X
        {
            get
            {
                return TilePos.X;
            }
        }
        public int Y
        {
            get
            {
                return TilePos.Y;
            }
        }

        public TilePos TilePos;

        // TODO: replace the single pointer with a list?
        public Expedition OperatingAreaOf;
        ExpeditionID? snapshotOperatingAreaOf;

        public EntityGroupID? Owner;

        public TerrainLevel Level = TerrainLevel.Middle;

        /// <summary>
        /// delete this...
        /// </summary>
        public SurfaceType TerrainType = PlainsType.Instance;

        /// <summary>
        /// resource probabilities. only used in Editor mode and when the map loads and gets populated!
        /// </summary>
        public Resource[] DesignerPlacedResources;

        /// <summary>
        /// actual resources in the tile. low vegetation resources are not implemented for now. 
        /// </summary>
        public Dictionary<ResourceType, TileResourceContainer> TileResources;
       // public Dictionary<string, TileResourceContainer> TileResources;
        Dictionary<string, ResourceID> snapshotTileResources;
       

        /// <summary>
        /// jobs to harvest tree crops or tile resources on this tile
        /// </summary>
        public Dictionary<EntityGroupID, Dictionary<ResourceType, List<ProcessJob>>> HarvestJobs;
        Dictionary<EntityGroupID, Dictionary<ResourceType, List<JobID>>> snapshotHarvestJobs;


        
        /// <summary>
        /// fog of war. Don't assume that all entities on the tile are seen/detected!
        /// </summary>
        public HashSet<Allegiances.Allegiance> AllegiancesThatSeeThisTile = new HashSet<Allegiances.Allegiance>();
        List<AllegianceID> snapshotAllegiancesThatSeeThisTile;

        public HashSet<Entity> EntitiesThatSeeThisTile = new HashSet<Entity>();
        List<EntityID> snapshotEntitiesThatSeeThisTile;

      //  public static Rectangle?[,] AllRenderedRoadConnections; 
        
             
        /// <summary>
        /// TODO: make this into a list of Renderables!
        /// </summary>
        private List<RoadAndPathQuad> renderedRoads;


    /*    public List<RenderedRoad> RenderedRoads
        {
            get
            {
                if (roadConnectionsAreDirty)
                {
                    ComputeRoadConnections();
                    roadConnectionsAreDirty = false;
                }
                return renderedRoads;
            }
        }*/

        private bool roadConnectionsAreDirty = true;

        // for fences, walls, cables and other billboards that connect:
      //  public List<RenderedRoad> RenderedConnectingFeatures;

        /// <summary>
        /// either this or TerrainSubtiles will be null!!!
        /// </summary>
        public Terrain Terrain;
        TerrainID? snapshotTerrain;

        /// <summary>
        /// either this or TerrainSubtiles will be null!!!
        /// </summary>
        public Terrain[][] TerrainSubtiles;
        TerrainID[][] snapshotTerrainSubtiles;

        private float moisture = 0.3f;

        /// <summary>
        /// TODO: a zone should be related to an EntityGroup, not an allegiance...
        /// </summary>
        public Dictionary<Allegiances.Allegiance, List<Zone>> Zones;
        Dictionary<AllegianceID, List<ZoneID>> snapshotZones;

        /// <summary>
        /// of ground. storing in terrain subtiles would improve visual quality near coasts.
        /// HOWEVER this would create big problems with the link to WaterAmount on the tile level. 
        /// </summary>
        public float Moisture
        {
            get
            {
               // return 0f; // rendering test...

                return moisture;
            }
            set
            {
                moisture = value;
                /*
                if (value != moisture)
                {
                    moisture = value;
                    TotalSubtileMoistureIsDirty = true;
                }*/
            }
        }

        // make this with substances instead
        /// <summary>
        /// non-firewood material that is dry...
        /// </summary>
    /*    private float DryOrganicMaterialOnSurface;
     * 
        /// <summary>
        /// this can become firewood.
        /// </summary>
        private float FibrousMaterialOnSurface;
        private float NonFibrousMaterialOnSurface;
      
        /// <summary>
        /// nutrients and water are kept on the tile level and never in subtiles. This is because trees are able to extract them from an area...
        /// </summary>
        public float Phosphorous;
        public float Nitrogen;
        */

        public const float FreezingPointInKelvin = 273.15f;
        private const float WaterAmountToConsiderSoilFullyFlooded = 10000f;

        // Firewood should fall to the ground where there are trees. Then it goes into the nutrient cycle, or creates fire hazards. 
        // Dead trees fall to the ground, and start to rot and/or dry. Drying should be modelled...
        // Standing dead trees are more likely to dry than rot. They can be felled for firewood.        
      //  public TileResourceContainer Firewood;

               

        /// <summary>
        /// of air
        /// </summary>
        public float Humidity = 0.2f;

       
       

        private float waterAmount;
        public float WaterAmount
        {
            get
            {
                return waterAmount;
            }
            set
            {
                waterAmount = value;
                ComputeMoistureLevelFromWaterAmount();
            }
        }

        
        public Vector4 ColorOfWater = new Vector4(0.3f, 0.3f, 0.5f, 1.0f);
        public Vector4 WaterBottomTint = Vector4.One;

        /// <summary>
        /// In Kelvin
        /// </summary>
        public float Temperature; // = 288; // 15 C - replace this!
        
        

     //   public Dictionary<EntityType, Entity[]> RoadStructures;

        // yes, they can be built on top of roads...
        public List<Entity> EdgeLayoutEntities;


       // public Buildings.Road[] Roads;
        public Entity[] Roads;

       
        // 
        /// <summary>
        /// tracks and paths are special, in that they can co-exist in the same edge part.
        /// Only the highest scoring one is rendered.
        /// </summary>
        public Entity[] WheelPaths;

        private Entity[] FootPaths;

     
        

        /// <summary>
        /// Warning: Use Add and Remove methods!!!
        /// 
        /// Multi-tile entities will exist in more than one list.
        /// For now, do not place in both this and TiledStructureOnTile.
        /// 
        /// Trees exist both in this list and in TreesOnTile!
        /// 
        /// TODO: replace this collection with calls to quadtree
        /// </summary>
        public List<Entity> EntitiesOnTile;
        List<EntityID> snapshotEntitiesOnTile;

        /// <summary>
        /// for seeing/unseeing processes.
        /// only processes that are not attached to any entities will appear in this list.
        /// </summary>
        public List<SimProcessID> ProcessesOnTile;
        //List<EntityID> snapshotEntitiesOnTile;


        /// <summary>
        /// particle emitters?
        /// </summary>
        public List<Renderable> RenderablesOnTile;

        /// <summary>
        /// For rendering the entities in the fog of war to the player
        /// also for deprecating memory facts when the allegiance gets close enough.
        /// Warning: Use Add and Remove methods!!!
        /// 
        /// We don't store contained entities or parts here. Instead we iterate their containers when seeing/unseeing
        /// </summary>
        public Dictionary<SharedKnowledge, List<MemoryFact>> RememberedRootEntitiesOnTile;
        private Dictionary<AllegianceID, List<MemoryFactID>> snapshotRememberedEntitiesOnTile;

        public Dictionary<SharedKnowledge, List<ProcessMemory>> RememberedProcessesOnTile;
        private Dictionary<AllegianceID, List<ProcessMemoryID>> snapshotRememberedProcessesOnTile;

      
        /// <summary>
        /// Structures on tile  
        /// Large Buildings should not be affected by fog of war...
        ///     
        /// How will this be replaced by GeoLayout?? look up in a collision quad tree?
        /// Read reserved flag, Pad etc. from terrain
        /// 
        /// made this an ID to prevent a known crash, unable to repro it...
        /// </summary>     
        public List<EntityID> GeoLayoutEntitiesOnTile; //snapshotTiledEntities;
              

      //  public Buildings.IAddon AddonOnTile;

        /// <summary>
        /// ALERT: Is Base Center of building = MapPosition, NOT TopLeftPosition. 
        /// what about add ons? can they have the same reference tile? Or use AddonOnTile?
        /// 
        /// Delete this...
        /// </summary>
      //  public bool IsBuildingReferencePoint = false;


        /// <summary>
        /// Used for rendering light sources on multi-tile entities. 
        /// </summary>
        public List<Entity> BaseCenterForMultiTileEntities;
        List<EntityID> snapshotBaseCenterEntities;

      
        /// <summary>
        /// we use this collection when doing harvesting and nutrient uptake
        /// trees are in the Entities collection as well!!!     
        /// </summary>
        //public Entity[] TreesOnTile;
        public List<Entity> TreesOnTile;
        List<EntityID> snapshotTreesOnTile;

        public float GetTotalBulkOfItems()
        {
            float total = 0f;
            if (EntitiesOnTile != null)
            {
                foreach (Entity entity in EntitiesOnTile)
                {
                    if (entity.EntityType.ItemType != null)
                    {
                        total += entity.Bulk;
                    }
                }
            }

            return total;
        }
        

        
        /// <summary>
        /// make this using the substance system
        /// </summary>
        /// <param name="deltaTimeInSeconds"></param>
        public void UpdateSimulationInParallel(double deltaTimeInSeconds) //double deltaTimeInMilliseconds)
        {
            if (TreesOnTile != null)
            {
               // float totalNitrogenNeeds = 0f, totalPhosphorousNeeds = 0f, totalWaterNeeds = 0f;
                float nitrogenNeeds = 0f, phosphorousNeeds = 0f, waterNeeds = 0f;

                float fibrous = 0f, nonFibrous = 0f;

                // gather the plants needing resources here:
                List<PlantResourceNeeds> plants = new List<PlantResourceNeeds>();
                foreach (Entity tree in TreesOnTile)
                {
                    Trees.Tree treeComponent;
                    tree.Find(out treeComponent);

                    treeComponent.GetGrowthNeeds(deltaTimeInSeconds, out waterNeeds, out nitrogenNeeds, out phosphorousNeeds);

                    plants.Add(new PlantResourceNeeds() { Tree = tree, Nitrogen = nitrogenNeeds, Water = waterNeeds, Phosphorous = phosphorousNeeds });

                  //  treeComponent.UpdateSimulationInParallel(deltaTimeInSeconds);
                }

                // deduct tile resources and grow the trees:
                //AssignResourcesToPlants(plants);               

                // make trees lose mass to the ground:
            /*    foreach (Entity tree in TreesOnTile)
                {
                    Trees.Tree treeComponent;
                    tree.Find(out treeComponent);


                    treeComponent.LoseMass(deltaTimeInSeconds, out fibrous, out nonFibrous);

                    FibrousMaterialOnSurface += fibrous;
                    NonFibrousMaterialOnSurface += nonFibrous;
                }*/
            }

          //  ConvertOrganicMaterial(deltaTimeInSeconds);

        }

      /*  private void ConvertOrganicMaterial(double deltaTimeInSeconds)
        {
            if (FibrousMaterialOnSurface > 0f || NonFibrousMaterialOnSurface > 0f)
            {
                float dryingFactor = ComputeDryingFactor();
                float rottingFactor = ComputeRottingFactor();

                float totalRottedMaterial = 0f;

                if (FibrousMaterialOnSurface > 0f)
                {
                    float totalDriedMaterial = 0f;

                    DryAndRotMaterial(deltaTimeInSeconds, ref FibrousMaterialOnSurface,
                        GameData.Instance.Constants.RottingSpeedOfOrganicFibers,
                        GameData.Instance.Constants.DryingSpeedOfOrganicFibers,
                        dryingFactor, rottingFactor, ref totalRottedMaterial, ref totalDriedMaterial);

                    ConvertToFirewood(totalDriedMaterial);
                }

                if (NonFibrousMaterialOnSurface > 0f)
                {
                    float totalDriedMaterial = 0f;

                    DryAndRotMaterial(deltaTimeInSeconds, ref NonFibrousMaterialOnSurface,
                        GameData.Instance.Constants.RottingSpeedOfOrganicMaterial,
                        GameData.Instance.Constants.DryingSpeedOfOrganicMaterial,
                        dryingFactor, rottingFactor, ref totalRottedMaterial, ref totalDriedMaterial);

                }

                ConvertToNutrients(totalRottedMaterial);

            }
        }*/

        private void ComputeMoistureLevelFromWaterAmount()
        {
            // the water contained in the topsoil and lower down...

            // http://graph-plotter.cours-de-math.eu/
            //x^(0.5)


            // increases quickly from 0 then tapers off near 1...
            Moisture = (float)Math.Pow(WaterAmount / WaterAmountToConsiderSoilFullyFlooded, 0.5f);

            Moisture = Common.ClampTop(Moisture, 1f);

        }

        private float ComputeDryingFactor()
        {
            // 100 degress = 1f
            float temperatureFactor = Common.ClampBottom(Temperature - FreezingPointInKelvin, 0f) / 100f;

            // can be negative!
            float moistureFactor = 0.5f - Moisture;

            // can be negative!
            float humidityFactor = 0.2f - Humidity;

            float windFactor = Common.ClampTop(The.Sim.PlaySite.PlaySite.Weather.WindSpeed / 10f, 2f);

            return Common.ClampBottom(temperatureFactor + moistureFactor + humidityFactor + windFactor, 0f);


        }

        private float ComputeRottingFactor()
        {
            // 32 degress = 1f
            // (-x^2+64*x + 300)/1400
            // http://graph-plotter.cours-de-math.eu/
            float temp = Temperature - FreezingPointInKelvin; //in celsius

            float temperatureFactor = Common.Clamp((-(temp * temp) + 64f * temp + 300f) / 1400f, 0f, 1f);


            float moistureFactor = Moisture;

            float humidityFactor = Humidity;

            //  float windFactor = Common.ClampTop(WeatherManager.Instance.WindSpeed / 10f, 2f);

            // greater than 1??
            return temperatureFactor + moistureFactor + humidityFactor; // +windFactor;


        }

      //  public bool TotalSubtileMoistureIsDirty;
      //  private float totalSubtileMoisture;
      /*  private float GetMoistureInTile()
        {
            if (Terrain != null)
            {
                return Terrain.Moisture;
            }
            else
            {
                if (TotalSubtileMoistureIsDirty)
                {
                    TotalSubtileMoistureIsDirty = false;

                    totalSubtileMoisture = 0f;
                    for (int sx = 0; sx < 3; sx++)
                    {
                        for (int sy = 0; sy < 3; sy++)
                        {
                            totalSubtileMoisture += TerrainSubtiles[sx][sy].Moisture;
                        }
                    }
                }

                return totalSubtileMoisture;
            }
        }*/

        /// <summary>
        /// the idea is that firewood and dry material can become wet and later rot...
        /// </summary>
        /// <returns></returns>
      /*  private float ComputeWettingFactor()
        {
            float moistureFactor = Moisture;

            float humidityFactor = Humidity;

            return moistureFactor + humidityFactor;
        }*/


        /*
        private void DryAndRotMaterial(double deltaTimeInSeconds, ref float material, float rottingSpeed, float dryingSpeed, float dryingFactor, float rottingFactor, ref float totalRottedMaterial, ref float totalDriedMaterial)
        {
            float rottedMaterial = (float)(rottingFactor * rottingSpeed * deltaTimeInSeconds * material);

            float driedMaterial = (float)(dryingFactor * dryingSpeed * deltaTimeInSeconds * material);

            material = material - rottedMaterial - driedMaterial;

            totalRottedMaterial += rottedMaterial;

            totalDriedMaterial += driedMaterial;
        }
        */
       /* private void ConvertToFirewood(float driedFibrousMaterial)
        {
            
        }

        private void ConvertToNutrients(float rottedMaterial)
        {
            // this should match the uptake from the ground when the plants grow...!
            Phosphorous += GameData.Instance.Constants.PhosphorusAmountInOrganicMaterial * rottedMaterial;

            Nitrogen += GameData.Instance.Constants.NitrogenAmountInOrganicMaterial * rottedMaterial;

        }*/

        /// <summary>
        /// should be implmented with the Substance and Pool system
        /// </summary>
        /// <param name="plants"></param>
      /*  private void AssignResourcesToPlants(List<PlantResourceNeeds> plants) //, float availableResources)
        {
            float totalResourcesNeeded = 0;
            double resourcesSpent = 0;

            float assignedResources;
            bool resourcesShortfall;
            float availableResources;

            // WATER *************
            availableResources = waterAmount;

            foreach (PlantResourceNeeds plant in plants)
            {
                totalResourcesNeeded += plant.Water;
            }

            resourcesShortfall = totalResourcesNeeded > availableResources;
            resourcesSpent = 0;
            foreach (PlantResourceNeeds plant in plants)
            {
                if (resourcesSpent < availableResources)
                {
                    if (resourcesShortfall == true)
                    {
                        // we should modify the assigned amount of water for each plant type. Some are better at reaching water than others...
                        assignedResources = availableResources * (plant.Water / totalResourcesNeeded);
                    }
                    else
                    {
                        assignedResources = plant.Water;
                    }

                    plant.Water = assignedResources;
                    resourcesSpent += assignedResources;

                    //  resourcesSpent += producer.Produce(
                    //     (resourcesShortfall ? AvailableEnergy * (producer.NeededEnergy / totalEnergyNeeded) : producer.NeededEnergy));

                }
            }

            // NITROGEN ***************
            availableResources = Nitrogen;

            foreach (PlantResourceNeeds plant in plants)
            {
                totalResourcesNeeded += plant.Nitrogen;
            }

            resourcesShortfall = totalResourcesNeeded > availableResources;
            resourcesSpent = 0;

            foreach (PlantResourceNeeds plant in plants)
            {
                if (resourcesSpent < availableResources)
                {
                    if (resourcesShortfall == true)
                    {
                        // we should modify the assigned amount of water for each plant type. Some are better at reaching water than others...
                        assignedResources = availableResources * (plant.Nitrogen / totalResourcesNeeded);
                    }
                    else
                    {
                        assignedResources = plant.Nitrogen;
                    }

                    plant.Nitrogen = assignedResources;
                    resourcesSpent += assignedResources;
                    //  resourcesSpent += producer.Produce(
                    //     (resourcesShortfall ? AvailableEnergy * (producer.NeededEnergy / totalEnergyNeeded) : producer.NeededEnergy));

                }
            }

            // PHOSPHOROUS ***************
            availableResources = Phosphorous;
            resourcesSpent = 0;

            foreach (PlantResourceNeeds plant in plants)
            {
                totalResourcesNeeded += plant.Phosphorous;
            }

            resourcesShortfall = totalResourcesNeeded > availableResources;

            foreach (PlantResourceNeeds plant in plants)
            {
                if (resourcesSpent < availableResources)
                {
                    if (resourcesShortfall == true)
                    {
                        // we should modify the assigned amount of water for each plant type. Some are better at reaching water than others...
                        assignedResources = availableResources * (plant.Phosphorous / totalResourcesNeeded);
                    }
                    else
                    {
                        assignedResources = plant.Phosphorous;
                    }

                    plant.Phosphorous = assignedResources;
                    resourcesSpent += assignedResources;
                    //  resourcesSpent += producer.Produce(
                    //     (resourcesShortfall ? AvailableEnergy * (producer.NeededEnergy / totalEnergyNeeded) : producer.NeededEnergy));

                }
            }

            float currentWater = waterAmount;
            float currentNitrogen = Nitrogen;
            float currentPhosphorous = Phosphorous;

            Trees.Tree treeComponent;
            foreach (PlantResourceNeeds plant in plants)
            {
                // TODO: low vegetation too
                plant.Tree.Find(out treeComponent);

                float oldWater = plant.Water;
                float oldNitrogen = plant.Nitrogen;
                float oldPhosphorous = plant.Phosphorous;

                treeComponent.Grow(ref plant.Water, ref plant.Nitrogen, ref plant.Phosphorous);

                currentWater -= (oldWater - plant.Water);
                currentNitrogen -= (oldNitrogen - plant.Nitrogen);
                currentPhosphorous -= (oldPhosphorous - plant.Phosphorous);
            }


            WaterAmount = currentWater;
            Nitrogen = currentNitrogen;
            Phosphorous = currentPhosphorous;

        }
        */
        

        public bool TileIsInFogOfWar(Allegiances.Allegiance allegiance)
        {
            return !AllegiancesThatSeeThisTile.Contains(allegiance);
        }

        /// <summary>
        /// update all those that can see this object being destroyed.
        /// </summary>
        /// <param name="gameObject"></param>
        public void DeleteMemoryOfDestroyedEntity(Entity entity)
        {
            foreach (Allegiances.Allegiance allegianceType in AllegiancesThatSeeThisTile)
            {
                allegianceType.SharedKnowledge.DeleteMemoryOfEntity(entity.ID, entity.DetectableID, true); 
            }
        }


        public void DeleteMemoryOfEntityMovingOffSite(Entity entity)
        {
            foreach (Allegiances.Allegiance allegianceType in AllegiancesThatSeeThisTile)
            {
                allegianceType.SharedKnowledge.DeleteMemoryOfEntity(entity.ID, entity.DetectableID, true);
            }
        }  

      

        public bool HasEverBeenSeenByPlayer
        {
            get;
            set;
        }

        public void ChangeTileSeenBy(Entity byEntity, UWGame.SimSide.Entities.Sensor.TileStatus tileStatus)
        {
            Allegiances.Allegiance allegiance = byEntity.Intelligence.Allegiance;

            if (tileStatus == Sensor.TileStatus.Seen)
            {
                if (allegiance.AllegianceType == Allegiances.AllegianceType.Player)
                {
                    // this is a player-only limitation. AI is not affected???
                    HasEverBeenSeenByPlayer = true;
                }

                EntitiesThatSeeThisTile.Add(byEntity);

                if (allegiance != null) // only in Edit mode do we not have an allegiance.
                {
                    SeeEntitiesOnTile(byEntity, allegiance.SharedKnowledge); 
                    
                    SeeAllProcessesOnTile(allegiance.SharedKnowledge);
                       
                }

                AllegiancesThatSeeThisTile.Add(allegiance);

                DeprecateMemoryFactsOnTile(byEntity, TerrainTile.DeprecateDistance.Far);

            //    DeleteMemoryOfAllSeenGameEntities(allegiance); // Lars: I don't believe we need this...
            }
            else
            {
                EntitiesThatSeeThisTile.Remove(byEntity);

                if (allegiance != null) // only in Edit mode do we not have an allegiance.
                {
                    bool isSeenByOthersWithThisAllegiance = false;
                    foreach (Entity entity in EntitiesThatSeeThisTile)
                    {
                        if (entity.Intelligence.Allegiance == allegiance)
                        {
                            isSeenByOthersWithThisAllegiance = true;
                            break;
                        }
                    }

                    if (!isSeenByOthersWithThisAllegiance)
                    {
                        AllegiancesThatSeeThisTile.Remove(allegiance);

                        // ok, noone else from this allegiance sees this tile.                    
                        // store any entities in memory:
                       
                      //  StoreMemoryOfAllUnseenGameEntities(allegiance);

                        if (EntitiesOnTile != null)
                        {
                            foreach (Entity entity in EntitiesOnTile)
                            {
                                allegiance.SharedKnowledge.UnSeeEntity(entity);
                            }
                        }
                        
                        if (ProcessesOnTile != null)
                        {
                            for (int i = ProcessesOnTile.Count - 1; i >= 0 ; i--)
			                {
			                    SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProcessesOnTile[i]);
                                if (process != null)
                                {
                                    allegiance.SharedKnowledge.PlaySiteKnowledge.UnSeeProcess(process);
                                }
                                else 
                                {
                                    ProcessesOnTile.RemoveAt(i);
                                }
			                }
                            
                        }
                    }
                }
            }

            /*
            if (The.Sim.TotalUnPausedGameTimeInSeconds > 1 && allegiance.SharedKnowledge.PlaySiteKnowledge != null)
            {
                allegiance.SharedKnowledge.PlaySiteKnowledge.AssertSeenEntitiesOnPlaySiteNotInFOW();
            }*/

        }

        private void SeeEntitiesOnTile(Entity byEntity, SharedKnowledge sharedKnowledge)
        {
            List<IDetectable> allDetectables = new List<IDetectable>();
            Sensor sensor;
            if (byEntity.Find(out sensor))
            {
                GetDetectablesOnTile(allDetectables);

                sensor.RollToDetect(byEntity, sharedKnowledge, allDetectables, doAssert: false);
            }
        }

        /// <summary>
        /// detection chance is 100%
        /// </summary>
        /// <param name="sharedKnowledge"></param>
        private void SeeAllProcessesOnTile(SharedKnowledge sharedKnowledge)
        {
            if (ProcessesOnTile != null)
            {
                for (int i = ProcessesOnTile.Count - 1; i >= 0; i--)
                {
                    SimProcess process = SimProcess.FindById(ProcessesOnTile[i]);
                    if (process != null)
                    {
                        sharedKnowledge.PlaySiteKnowledge.SeeProcess(process);
                    }
                    else
                    {
                        ProcessesOnTile.RemoveAt(i);
                    }
                }
            }
        }

        public void GetDetectablesOnTile(List<IDetectable> allDetectables)
        {
            if (EntitiesOnTile != null)
            {
                allDetectables.AddRange(EntitiesOnTile);    
            }

            if (TileResources != null)
            {
                foreach (TileResourceContainer container in TileResources.Values)
                {
                    if (container.ResourceItems.Count > 0) // don't spot empty containers...
                    {
                        allDetectables.Add(container);
                    }
                }
            }

            if (TreesOnTile != null)
            {
                foreach (Entity ent in TreesOnTile)
                {
                    if (ent.EntityType.TreeType != null && ent.EntityType.TreeType.CropTypes != null)
                    {
                        //allDetectables.Add(ent);
                        UWGame.SimSide.Trees.Tree tree;
                        ent.Find(out tree);
                        foreach (var crop in tree.Crops)
                        {
                            if (crop.Value.ResourceItems.Count > 0) // don't spot empty containers...
                            {
                                allDetectables.Add(crop.Value);
                            }
                        }
                    }
                }
            }
        }

       

    // Static constructor
     /*   static TerrainTile()
        {
            AllRenderedRoadConnections = new RenderedRoad[,]{}
        }*/

    /*    public bool IsUnderWater()
        {
            if (Terrain != null)
            {
                return Terrain.IsUnderWater();
            }
            else
            {

            }
        }*/

       

       

        public void AddToFootPath(Common.Direction dir)
        {            
            AddToFootPath(dir, GameData.Instance.Constants.AmountToAddToPathOnTraversal);
        }

        public void AddToFootPath(Common.Direction dir, float increment)
        {
            int d = (int)dir;
            if (FootPaths == null)
            {
                FootPaths = new Entity[8]; //new float[8] { 0, 0, 0, 0, 0, 0, 0, 0 };            
            }

            if (FootPaths[d] == null)// TODO Refactor, should footpaths be entities at all, not Renderables? do you interact weith footpaths? Do footpaths have a dynamic state in the simulation? Fading and shortening should be a client behavior!
            {
                FootPaths[d] = new Entity(GameData.Instance.AllEntityTypes["footpath"]);
            }

            // pave a path:
            TerrainPath terrainPathComponent;
            FootPaths[d].Find(out terrainPathComponent);

            float previousValue = terrainPathComponent.Value;

            terrainPathComponent.Value = Common.ClampTop(terrainPathComponent.Value + increment, 1f); // GameConstants.AmountToAddToPathOnTraversal, 1f);

            // only redraw if the activation level is passed (up/down)
            float pathActivation = GameData.Instance.Constants.PathActivation;

            if ((previousValue <= pathActivation && terrainPathComponent.Value > pathActivation)
                || previousValue > pathActivation && terrainPathComponent.Value <= pathActivation)
            {
                RedrawTerrainCostsAroundEdge(dir);
                //RedrawTerrainCosts(dir);
            }



            /* OLD:
            // TODO: Remove when it degrades?
            if (previousValue < GameConstants.PathActivation && FootPaths[d].TerrainPath.Value >= GameConstants.PathActivation && HighestRankedPathFeature(d) == TerrainType.TerrainFeatures.None)
            {
                SetTileEdge(global::UWGame.SimSide.Maps.TerrainType.TerrainFeatures.FootPath, dir);
            }*/
        }

        public void AddToWheelPath(Common.Direction dir, float increment)
        {
            int d = (int)dir;
            if (WheelPaths == null)
            {
                WheelPaths = new Entity[8]; //new float[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
               /* for (int i = 0; i < 8; i++)
                {
                    WheelPaths[i] = new Entity(GameData.Instance.AllEntityTypes["tracks"]);
                }*/
            }

            if (WheelPaths[d] == null)
            {
                WheelPaths[d] = new Entity(GameData.Instance.AllEntityTypes["tracks"]);
            }

            // pave a path:
            TerrainPath terrainPathComponent = WheelPaths[d].TerrainPath;
            float previousValue = terrainPathComponent.Value;

            terrainPathComponent.Value = Common.ClampTop(terrainPathComponent.Value + increment, 1f);

            // only redraw if the activation level is passed (up/down)
            float pathActivation = GameData.Instance.Constants.PathActivation;

            if ((previousValue <= pathActivation && terrainPathComponent.Value > pathActivation)
                || previousValue > pathActivation && terrainPathComponent.Value <= pathActivation)
            {
                RedrawTerrainCostsAroundEdge(dir);
            }

            /* OLD:
            TerrainType.TerrainFeatures highestRankedFeature = HighestRankedPathFeature(d);

            // wheel path overrides foot path
            if (previousValue < GameConstants.PathActivation && WheelPaths[d].TerrainPath.Value >= GameConstants.PathActivation && 
                (highestRankedFeature == TerrainType.TerrainFeatures.None || highestRankedFeature == TerrainType.TerrainFeatures.FootPath))
            {
                SetTileEdge(global::UWGame.SimSide.Maps.TerrainType.TerrainFeatures.WheelPath, dir);
            }*/
        }

        public void AddCenterForGeoLayoutEntities(Entity entity)
        {
            if (BaseCenterForMultiTileEntities == null)
            {
                BaseCenterForMultiTileEntities = new List<Entity>();
            }

            if (!BaseCenterForMultiTileEntities.Contains(entity))
            {
                BaseCenterForMultiTileEntities.Add(entity);
            }
        }

        public void RemoveCenterForGeoLayoutEntities(Entity entity)
        {
            if (BaseCenterForMultiTileEntities != null)
            {
                BaseCenterForMultiTileEntities.Remove(entity);
            }
        }

        /// <summary>
        /// return the non-addon building on the tile.
        /// </summary>
        /// <returns></returns>
        public Entity GetMainBuildingOnTile()
        {            
            /*if (TiledEntityOnTile != null)
            {
                foreach (Entity entity in TiledEntityOnTile)
                {
                    if (entity.TileLayout.Building != null && !entity.EntityType.StructureType.IsAddon)
                    {
                        return entity;
                    }
                }
            }*/

            return null;
        }

        public bool GetStructuresOnTile(ref List<Entity> listOfStructures)
        {
            if (GeoLayoutEntitiesOnTile != null)
            {
                for (int i = GeoLayoutEntitiesOnTile.Count - 1; i >= 0; i--)
                {
                    EntityID entityIDOnTile = GeoLayoutEntitiesOnTile[i];
                    Entity entityOnTile = Entity.FindByID(entityIDOnTile);
                    if (entityOnTile != null && entityOnTile.EntityType.StructureType != null)
                    {
                        Common.AddToList(ref listOfStructures, entityOnTile);                       
                    }
                    else
                    {
                        GeoLayoutEntitiesOnTile.RemoveAt(i);
                    }
                }

                /*foreach (Entity entity in TiledEntityOnTile)
                {
                    if (entity.Structure != null)
                    {
                        if (listOfStructures == null)
                        {
                            listOfStructures = new List<Entity>();
                        }
                        listOfStructures.Add(entity);
                    }
                }*/
            }

            return listOfStructures != null && listOfStructures.Count > 0;
        }

        public bool ContainsTileEntity(Entity entity)
        {
            if (GeoLayoutEntitiesOnTile != null)
            {
                return GeoLayoutEntitiesOnTile.Contains(entity.ID);
            }

            return false;
        }

        // use this instead. More logical.
     /*   public void AddrMultiTileEntityTouchingTile(Entity entity)
        {
            if (MultiTileEntitiesTouchingTile == null)
            {
                MultiTileEntitiesTouchingTile = new List<Entity>();
            }
            MultiTileEntitiesTouchingTile.Add(entity);
        }*/

        /// <summary>
        /// redraws the 2 or 4 tiles that may contain features that overlap on this edge.
        /// </summary>
        /// <param name="edge"></param>
        public void RedrawTerrainCostsAroundEdge(Common.Direction edge)
        {
            List<Point> tilesToRedraw = GetNeighboringTiles(new Point(X, Y), edge);

            foreach (Point pos in tilesToRedraw)
            {
                // TODO: force a redraw of neighbours using the GeoLayout code.
               // The.Map.TileMap[pos.X][pos.Y].RedrawTerrainCosts();
            }

        }

        /// <summary>
        /// redraw everything in the tile
        /// </summary>
      /*  public void RedrawTerrainCosts()
        {
            // redraw everything from the bottom up:

            for (int i = 0; i < 8; i++)
            {   // draw roads on top of standard terrain:
                RedrawRoadCosts((Common.Direction)i);
            }

            if (Terrain != null)
            {
                if (Terrain.IsUnderWater())
                {
                    // block movement on water:                  
                    The.Map.SetTileCost(X, Y, 0);
                }
            }
            else
            {
                for (int sx = 0; sx < 3; sx++)
                {
                    for (int sy = 0; sy < 3; sy++)
                    {
                        if (TerrainSubtiles[sx][sy].IsUnderWater())
                        {
                            // block movement on water:                           
                            The.Map.SetSubtileCost(X, Y, 0);
                        }
                    }
                }

            }

            if (EdgeLayoutEntities != null)
            {
                foreach (Entity entity in EdgeLayoutEntities) 
                {
                    entity.DirectionalLayout.RedrawTerrainCosts();                        

                }
            }

            if (EntitiesOnTile != null)
            {
                foreach (Entity entity in EntitiesOnTile) 
                {
                    if (entity.PointLayout != null)
                    {
                        entity.PointLayout.RedrawTerrainCosts();
                    }                   
                    // there shouldn't be TiledlayoutEntities in this list.
                }
            }

        
            if (TiledEntityOnTile != null)
            {
                // redraw this tile only:
                foreach (Entity entity in TiledEntityOnTile)
                {
                    entity.TileLayout.RedrawTerrainCosts(new Point(this.X, this.Y));
                }                
            }            

        }*/

        private void RedrawRoadCosts(Common.Direction dir)
        {
            int d = (int)dir;

            // the following are in prioritized sequence...
            if (Roads != null)
            {
                if (Roads[d] != null && Roads[d].IsCompleted())
                {
                    Roads[d].DirectionalLayout.RedrawRoadCost();
                   // SetTileEdge(Roads[d].EntityType.TerrainType.PathType, dir);
                    return;
                }
            }

            float pathActivation = GameData.Instance.Constants.PathActivation;

            if (WheelPaths != null)
            {
                if (WheelPaths[d] != null && WheelPaths[d].TerrainPath.Value > pathActivation)
                {
                    WheelPaths[d].DirectionalLayout.RedrawRoadCost();

                    //SetTileEdge(WheelPaths[d].EntityType.TerrainType.PathType, dir);
                    return;
                }
            }

            if (FootPaths != null)
            {
                if (FootPaths[d] != null && FootPaths[d].TerrainPath.Value > pathActivation)
                {
                    FootPaths[d].DirectionalLayout.RedrawRoadCost();
                    //SetTileEdge(FootPaths[d].EntityType.TerrainType.PathType, dir);
                    return;
                }
            }

            // do we really want other terrain types...? then add a pointer in the class.
            // if not, then at least compensate cost for wetness, so we can do shallow water for instance.
            SetEdgeTerrainCost(GameData.Instance.AllEntityTypes["terrain:plains"].TerrainType.PathType, dir);

        }

        private void SetEdgeTerrainCost(PathType pathType, Common.Direction dir)
        {            
            //int transport;
             Point subTileStart = MapManager.TileEdgeToSubtile(new Point(X, Y));

            // STERAIN: set roads/paths terrain cost!
            // now on subtiles!
              Point edgeSubTile = MapManager.DirectionToRelativeSubtile(dir);
              edgeSubTile.X += subTileStart.X;
              edgeSubTile.Y += subTileStart.Y;

              Point centerSubtile = new Point(subTileStart.X + 1, subTileStart.Y);
              

            byte cost;
            foreach (SurfaceType.TransportType transport in MapManager.MapTransportTypeArray)
            {
                // TODO: add wetness cost here???
                cost = pathType.TransportCosts[(int)transport];

                //SetCost(transport, cost);
                The.Map.SetSubtileCost(edgeSubTile, transport, cost);

                // set the center subtile too:
                The.Map.SetSubtileCost(centerSubtile, transport, cost);

            }

            /* OLD:
            for (int t = 0; t < MapManager.TransportIndices.Length; t++)
            {
                transport = MapManager.TransportIndices[t];

                UWGame.SimSide.Instance.Map.SetEdgeCost(pos, dir, transport, pathType.TransportCosts[transport]);
            }*/
        }

        
        public List<Zone> GetListOfZones(Allegiances.Allegiance allegiance)
        {
            List<Zone> listOfZones = null;
            if (Zones != null)
            {
                Zones.TryGetValue(allegiance, out listOfZones);
            }

            return listOfZones;
        }

        public bool IsInZone(Allegiances.Allegiance allegiance, Zone zone)
        {
            List<Zone> listOfZones = GetListOfZones(allegiance);

            if (listOfZones != null && listOfZones.Count > 0)
            {
                return listOfZones.Contains(zone);
            }

            return false;
        }

        public Zone GetStockpileZone(Allegiances.Allegiance allegiance)
        {
            List<Zone> listOfZones = GetListOfZones(allegiance);
            if (listOfZones != null)
            {
                return listOfZones.Find(z => z.Stockpile != null); // && z.Stockpile.Setting != Stockpile.StockpileSetting.None);
            }

            return null;
        }

       /* public bool IsStockpilingProhibited(Allegiances.Allegiance allegiance)
        {
            List<Zone> listOfZones = GetListOfZones(allegiance);
            if (listOfZones != null)
            {
                return listOfZones.Exists(z => z.Stockpile != null); // && z.Stockpile.Setting == Stockpile.StockpileSetting.None);
            }

            return false;
        }*/

        public void AddZone(Allegiances.Allegiance allegiance, Zone zone)
        {
            List<Zone> listOfZones = null;
            if (Zones == null)
            {
                Zones = new Dictionary<Allegiances.Allegiance, List<Zone>>();
            }

            if (!Zones.TryGetValue(allegiance, out listOfZones))
            {
                listOfZones = new List<Zone>();
                Zones.Add(allegiance, listOfZones);
            }

            listOfZones.Add(zone);

        }

        public void RemoveZone(Allegiances.Allegiance allegiance, Zone zone)
        {
            if (Zones != null)
            {
                List<Zone> listOfZones = null;
                if (Zones.TryGetValue(allegiance, out listOfZones))
                {
                    listOfZones.Remove(zone);

                    if (listOfZones.Count == 0)
                    {
                        Zones.Remove(allegiance);
                    }
                }
                
            }
        }

        /// <summary>
        /// when the allegiance is not known, this will iterate all zones
        /// </summary>
        /// <param name="zone"></param>
        public void RemoveZone(Zone zone)
        {
            if (Zones != null)
            {
                foreach (var allegiance in Zones)
                {
                    if (allegiance.Value.Remove(zone))
                    {
                        return;
                    }
                }
            }

        }

        public List<Point> GetNeighboringTiles(Point tilePos, Common.Direction edge)
        {
            List<Point> listOfNeighbours = new List<Point>();
            listOfNeighbours.Add(tilePos);

            switch (edge)
            {
                case Common.Direction.East:
                    AddEastTile(tilePos, listOfNeighbours);
                    break;
                case Common.Direction.North:
                    AddNorthTile(tilePos, listOfNeighbours);
                    break;
                case Common.Direction.West:
                    AddWestTile(tilePos, listOfNeighbours);
                    break;
                case Common.Direction.South:
                    AddSouthTile(tilePos, listOfNeighbours);
                    break;
                case Common.Direction.NorthEast:
                    AddNorthTile(tilePos, listOfNeighbours);
                    AddEastTile(tilePos, listOfNeighbours);
                    AddNeighbouringTile(tilePos, listOfNeighbours, 1, -1);
                    break;
                case Common.Direction.NorthWest:
                    AddNorthTile(tilePos, listOfNeighbours);
                    AddWestTile(tilePos, listOfNeighbours);
                    AddNeighbouringTile(tilePos, listOfNeighbours, -1, -1);
                    break;
                case Common.Direction.SouthEast:
                    AddSouthTile(tilePos, listOfNeighbours);
                    AddEastTile(tilePos, listOfNeighbours);
                    AddNeighbouringTile(tilePos, listOfNeighbours, 1, 1);
                    break;
                case Common.Direction.SouthWest:
                    AddSouthTile(tilePos, listOfNeighbours);
                    AddWestTile(tilePos, listOfNeighbours);
                    AddNeighbouringTile(tilePos, listOfNeighbours, -1, 1);
                    break;

            }
            return listOfNeighbours;
        }

        private static void AddNeighbouringTile(Point tilePos, List<Point> listOfNeighbours, int dX, int dY)
        {
            Point neighbouringTile = The.Map.ClampTileMapPosition(new Point(tilePos.X + dX, tilePos.Y + dY));
            if (tilePos != neighbouringTile)
            {
                listOfNeighbours.Add(neighbouringTile);
            }
        }

        private static void AddSouthTile(Point tilePos, List<Point> listOfNeighbours)
        {
            Point southTile = The.Map.ClampTileMapPosition(new Point(tilePos.X, tilePos.Y + 1));
            if (tilePos != southTile)
            {
                listOfNeighbours.Add(southTile);
            }
        }

        private static void AddWestTile(Point tilePos, List<Point> listOfNeighbours)
        {
            Point westTile = The.Map.ClampTileMapPosition(new Point(tilePos.X - 1, tilePos.Y));
            if (tilePos != westTile)
            {
                listOfNeighbours.Add(westTile);

            }
        }

        private static void AddNorthTile(Point tilePos, List<Point> listOfNeighbours)
        {
            Point northTile = The.Map.ClampTileMapPosition(new Point(tilePos.X, tilePos.Y - 1));
            if (tilePos != northTile)
            {
                listOfNeighbours.Add(northTile);
            }
        }

        private static void AddEastTile(Point tilePos, List<Point> listOfNeighbours)
        {
            Point rightTile = The.Map.ClampTileMapPosition(new Point(tilePos.X + 1, tilePos.Y));
            if (tilePos != rightTile)
            {
                listOfNeighbours.Add(rightTile);
            }
        }

        /// <summary>
        /// ok.... what about the trees in the tile?
        /// </summary>
        /// <param name="dir"></param>
        /// 

     /*   private void RedrawTerrainCosts(Common.Direction dir)
        {
            PathType pathType;
            int d = (int)dir;

            if (EdgeLayoutEntities != null)
            {
                foreach (Entity entity in EdgeLayoutEntities)
                {
                    if (entity.EdgeLayout.EdgePosition == dir)
                    {
                        if (entity.EntityType.EdgeLayoutType.IsObstacle)
                        {   // block it...
                            SetTileEdge(0, dir);
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }

                }
            }

            // the following are in prioritized sequence...
            if (Roads != null)
            {
                if (Roads[d] != null && Roads[d].Structure.IsCompleted())
                {
                    SetTileEdge(Roads[d].EntityType.TerrainType.PathType, dir);
                    return;
                }
            }

            if (WheelPaths != null)
            {
                if (WheelPaths[d] != null && WheelPaths[d].TerrainPath.Value > GameConstants.PathActivation)
                {
                    SetTileEdge(WheelPaths[d].EntityType.TerrainType.PathType, dir);
                    return;
                }
            }

            if (FootPaths != null)
            {
                if (FootPaths[d] != null && FootPaths[d].TerrainPath.Value > GameConstants.PathActivation)
                {
                    SetTileEdge(FootPaths[d].EntityType.TerrainType.PathType, dir);
                    return;
                }
            }

            // do we really want other terrain types...?
            SetTileEdge(GameData.Instance.AllEntityTypes["terrain:plains"].TerrainType.PathType, dir);
        }*/

        public enum DeprecateDistance { Near, Far }
        public void DeprecateMemoryFactsOnTile(Entity detectingEntity, DeprecateDistance deprecateDistance)
        {
            if (RememberedRootEntitiesOnTile != null && RememberedRootEntitiesOnTile.Count > 0)
            {
                List<MemoryFact> memoryFacts;
                Allegiances.Allegiance allegiance = detectingEntity.Intelligence.Allegiance;

                if (RememberedRootEntitiesOnTile.TryGetValue(allegiance.SharedKnowledge, out memoryFacts))
                {
                    MemoryFact memoryFact;
                    for (int i = memoryFacts.Count - 1; i >= 0; i--)
                    {
                        memoryFact = memoryFacts[i];

                        bool wasDeprecated = false;
                        if (memoryFact.DeprecateIfNeeded(detectingEntity, deprecateDistance))
                        {
                            memoryFacts.RemoveAt(i);
                            wasDeprecated = true;
                        }

                        // check the leaf memory facts too. If the parent was just deprecated, then its reference is removed 
                        //and we will have lost the ability to see these memory facts again... so deprcate them all now while we can.
                        DeprecateDistance? deprecateDistanceForLeafs = deprecateDistance;
                        if (wasDeprecated)
                        {
                            deprecateDistanceForLeafs = null;
                        }

                        allegiance.SharedKnowledge.DeprecateMemoryFactLeafs(detectingEntity, memoryFact.EntityID, deprecateDistanceForLeafs);

                    }
                }
            }
        }

        

        /// <summary>
        /// has road or path on this edge
        /// </summary>
        /// <param name="directionIndex"></param>
        /// <returns></returns>
        public bool HasPath(int directionIndex)
        {
            if (Roads != null)
            {
                if (Roads[directionIndex] != null)
                {
                    return true;
                }
            }

            float pathActivation = GameData.Instance.Constants.PathActivation;

            if (WheelPaths != null && WheelPaths[directionIndex] != null && WheelPaths[directionIndex].TerrainPath.Value > pathActivation)
            {
                return true;
            }

            if (FootPaths != null && FootPaths[directionIndex] != null && FootPaths[directionIndex].TerrainPath.Value > pathActivation)
            {
                return true;
            }

            return false;
        }

        public SurfaceType.TerrainFeatures HighestRankedPathFeature(int directionIndex)
        {
            if (Roads != null)
            {
                if (Roads[directionIndex] != null)
                {
                    return SurfaceType.TerrainFeatures.GravelRoad;
                }
            }

            float pathActivation = GameData.Instance.Constants.PathActivation;

            if (WheelPaths != null && WheelPaths[directionIndex] != null && WheelPaths[directionIndex].TerrainPath.Value > pathActivation)
            {
                return SurfaceType.TerrainFeatures.WheelPath;
            }

            if (FootPaths != null && FootPaths[directionIndex] != null && FootPaths[directionIndex].TerrainPath.Value > pathActivation)
            {
                return SurfaceType.TerrainFeatures.FootPath;
            }

            return SurfaceType.TerrainFeatures.None;
        }

        

      /*  public void SetTileEdge(TerrainType.TerrainFeatures feature, Common.Direction dir)
        {
            int transport;
            Point pos = new Point(X, Y);

            for (int t = 0; t < MapManager.TransportIndices.Length; t++)
            {
                transport = MapManager.TransportIndices[t];

                UWGame.SimSide.Instance.Map.SetEdgeCost(pos, dir, transport, TerrainType.Cost((TerrainType.TransportType)transport, feature));
            }               
        }*/

        public void AddProcess(SimProcess process)
        {
            if (ProcessesOnTile == null)
            {
                ProcessesOnTile = new List<SimProcessID>();
            }

            if (!ProcessesOnTile.Contains(process.ID)) // for safety...
            {
                ProcessesOnTile.Add(process.ID);
            }

        }

        public void RemoveProcess(SimProcess process)
        {
            if (ProcessesOnTile != null)
            {
                ProcessesOnTile.Remove(process.ID);
            }
        }
        
        /// <summary>
        /// not structures, or...?
        /// </summary>
        /// <param name="entity"></param>
        public void AddEntity(Entity entity)
        {
            
            if (EntitiesOnTile == null)
            {
                EntitiesOnTile = new List<Entity>();
            }

            if (!EntitiesOnTile.Contains(entity)) // for safety...
            {
                EntitiesOnTile.Add(entity);
            }
        }
        
        public void RemoveEntity(Entity entity)
        {          

            if (EntitiesOnTile != null)
            {
                EntitiesOnTile.Remove(entity);                
            }         
        }

        public bool ContainsEntity(Entity entity)
        {
            if (EntitiesOnTile != null)
            {
                return EntitiesOnTile.Contains(entity);
            }

            return false;
        }
        /*
        public void AddRememberedEntity(Entity entity)
        {
            if (RememberedEntitiesOnTile == null)
            {
                RememberedEntitiesOnTile = new List<Entity>();
            }
            RememberedEntitiesOnTile.Add(entity);
        }

        public void RemoveRememberedEntity(Entity entity)
        {
            if (RememberedEntitiesOnTile != null)
            {
                RememberedEntitiesOnTile.Remove(entity);
            }

        }
        public bool ContainsRememberedEntity(Entity entity)
        {
            if (RememberedEntitiesOnTile != null)
            {
                return RememberedEntitiesOnTile.Contains(entity);
            }

            return false;
        }
        */

        public void AddRememberedProcess(SharedKnowledge sharedKnowledge, ProcessMemory memoryFact)
        {           
            if (RememberedProcessesOnTile == null)
            {
                RememberedProcessesOnTile = new Dictionary<SharedKnowledge, List<ProcessMemory>>();
            }

            Common.AddToMultiList(RememberedProcessesOnTile, sharedKnowledge, memoryFact);
        }



        public void AddRememberedRootEntity(SharedKnowledge sharedKnowledge, MemoryFact memoryFact)
        {
            if (memoryFact.EntityID == (EntityID)4852)
            {

            }

            if (RememberedRootEntitiesOnTile == null)
            {
                RememberedRootEntitiesOnTile = new Dictionary<SharedKnowledge, List<MemoryFact>>();
            }

            Common.AddToMultiList(RememberedRootEntitiesOnTile, sharedKnowledge, memoryFact);

            /*
            List<MemoryFact> listOfFacts;
            if (!RememberedEntitiesOnTile.TryGetValue(sharedKnowledge, out listOfFacts))
            {
                listOfFacts = new List<MemoryFact>();
                RememberedEntitiesOnTile.Add(sharedKnowledge, listOfFacts);
            }

            listOfFacts.Add(memoryFact);*/
        }

        public void RemoveRememberedRootEntity(SharedKnowledge sharedKnowledge, MemoryFact entity)
        {
            if (RememberedRootEntitiesOnTile != null)
            {
                List<MemoryFact> listOfFacts;
                if (RememberedRootEntitiesOnTile.TryGetValue(sharedKnowledge, out listOfFacts))
                {
                    listOfFacts.Remove(entity);

                    if (listOfFacts.Count == 0)
                    {
                        RememberedRootEntitiesOnTile.Remove(sharedKnowledge);
                    }
                }
            }

        }

        public void RemoveRememberedProcess(SharedKnowledge sharedKnowledge, ProcessMemory process)
        {
            if (RememberedProcessesOnTile != null)
            {
                List<ProcessMemory> listOfFacts;
                if (RememberedProcessesOnTile.TryGetValue(sharedKnowledge, out listOfFacts))
                {
                    listOfFacts.Remove(process);

                    if (listOfFacts.Count == 0)
                    {
                        RememberedProcessesOnTile.Remove(sharedKnowledge);
                    }
                }
            }

        }

        public void AddGeoLayoutEntity(Entity entity)
        {
            if (GeoLayoutEntitiesOnTile == null)
            {
                GeoLayoutEntitiesOnTile = new List<EntityID>();
            }

            if (!GeoLayoutEntitiesOnTile.Contains(entity.ID))
            {
                GeoLayoutEntitiesOnTile.Add(entity.ID);
            }
        }

        public void RemoveGeoLayoutEntity(Entity entity)
        {
            if (GeoLayoutEntitiesOnTile != null)
            {
                GeoLayoutEntitiesOnTile.Remove(entity.ID);
            }            
        }


        public bool HasGatherableResources()
        {
           /* if (Firewood.NoOfHarvestableItems > 0)
            {
                return true;
            }*/

            if (TileResources != null)
            {
                foreach (var resource in TileResources)
                {
                    if (resource.Value.NoOfHarvestableItems > 0)
                    {
                        return true;
                    }
                }
            }

            if (TreesOnTile != null)
            {
                Trees.Tree treeComponent;
                foreach (Entity tree in TreesOnTile)
                {
                    tree.Find(out treeComponent);
                    if (treeComponent.Crops != null)
                    {
                        foreach (KeyValuePair<ResourceType, Crop> kvp in treeComponent.Crops)
                        {
                            if (kvp.Value.NoOfHarvestableItems > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }


            Terrain terrain;
            if (Terrain != null)
            {
                terrain = Terrain;

                if (TerrainHasGatherableResources(terrain))
                {
                    return true;
                }
            }
            else
            {
                for (int sx = 0; sx < 3; sx++)
                {
                    for (int sy = 0; sy < 3; sy++)
                    {
                        terrain = TerrainSubtiles[sx][sy];

                        if (TerrainHasGatherableResources(terrain))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool TerrainHasGatherableResources(Terrain terrain)
        {
            if (terrain.Vegetation != null)
            {
                /*foreach (KeyValuePair<string, LowVegetation> kvp in terrain.Vegetation)
                {

                }*/
            }

            return false;
        }


      /*  public void AddVehicle(Vehicle vehicle)
        {
            if (VehiclesOnTile == null)
            {
                VehiclesOnTile = new List<Vehicle>();
            }
            VehiclesOnTile.Add(vehicle);
        }



        public void RemoveVehicle(Vehicle vehicle)
        {
            if (VehiclesOnTile != null)
            {
                VehiclesOnTile.Remove(vehicle);
            }

        }
        public bool ContainsVehicle(Vehicle vehicle)
        {
            if (VehiclesOnTile != null)
            {
                return VehiclesOnTile.Contains(vehicle);
            }

            return false;
        }*/

        public void AddEdgeStructure(Entity feature)
        {
            if (EdgeLayoutEntities == null)
            {
                EdgeLayoutEntities = new List<Entity>();
            }
            EdgeLayoutEntities.Add(feature);
        }

        public void RemoveEdgeStructure(Entity feature)
        {
            if (EdgeLayoutEntities != null)
            {
                EdgeLayoutEntities.Remove(feature);
            }

        }

        public void AddRoad(Entity road, Common.Direction dir)
        {
            if (Roads == null)
            {
                Roads = new Entity[8];
                //renderedRoads = new List<RoadAndPathQuad>();
            }
            Roads[(int)dir] = road;
                       

            roadConnectionsAreDirty = true;

            RedrawTerrainCostsAroundEdge(dir);

        }

       /* public SurfaceType GetSurfaceType(Point subtilePos)
        {
            if (Terrain != null)
            {
                return Terrain.SurfaceType;
            }
            else
            {
                Point relativePos = new Point(subtilePos.X % 3, subtilePos.Y % 3);
                return TerrainSubtiles[relativePos.X][relativePos.Y].SurfaceType;
            }
            
        }*/

        public Terrain GetTerrain(Point subtilePos)
        {
            if (Terrain != null)
            {
                return Terrain;
            }
            else
            {               
                return TerrainSubtiles[subtilePos.X % 3][subtilePos.Y % 3];
            }
        }

        public void ClearPaths(Common.Direction dir)
        {
            // remove paths when construction has finished?
            if (FootPaths != null && FootPaths[(int)dir] != null)
            {
                FootPaths[(int)dir].TerrainPath.Value = 0;
            }

            if (WheelPaths != null && WheelPaths[(int)dir] != null)
            {
                WheelPaths[(int)dir].TerrainPath.Value = 0;
            }

            roadConnectionsAreDirty = true;

            RedrawTerrainCostsAroundEdge(dir);
        }

        public void RemoveRoad(Entity road, Common.Direction dir)
        {
            if (Roads != null)
            {
                Roads[(int)dir] = null;
                roadConnectionsAreDirty = true;
            }

        }

     /*   public void AddRoad(Buildings.Road road, Common.Direction dir)
        {
            if (Roads == null)
            {
                Roads = new Road[8];
                renderedRoads = new List<RoadAndPathQuad>();
            }
            Roads[(int)dir] = road;
            roadConnectionsAreDirty = true;
        }

        public void RemoveRoad(Buildings.Road road, Common.Direction dir)
        {
            if (Roads != null)
            {
                Roads[(int)dir] = null;
                roadConnectionsAreDirty = true;
            }

        }*/


       
     /*   public float GetMoveFactor(EntityType movingEntity)
        {
            // TODO: set move skills for critters/people on terrain

            float terrainRoughness = 0f;
            if (this.Terrain != null)
            {
                terrainRoughness = this.Terrain.GetRoughness();
            }
            else
            {
                TerrainSubtiles
            }

            return (1f - movingEntity.LocomotorType.LeggedLocomotorType.TerrainNegateFactor) * terrainRoughness;
        }*/

        public void AddRenderable(Renderable renderable) 
        {
            if (RenderablesOnTile == null)
            {
                RenderablesOnTile = new List<Renderable>();
            }

            RenderablesOnTile.Add(renderable);
        }


        public void AddTree(Entity tree)
        {
            if (TreesOnTile == null)
            {               
                TreesOnTile = new List<Entity>();
            }
           
            TreesOnTile.Add(tree);
        }

        

        public void RemoveTree(Entity tree)
        {
            if (TreesOnTile != null)
            {
                //TreesOnTile[(int)edge] = null;
                TreesOnTile.Remove(tree);
            }

        }

        public bool ContainsTree(Entity tree)
        {
            if (TreesOnTile != null)
            {
                return TreesOnTile.Contains(tree);               
            }

            return false;
        }

        public bool ContainsTreeAtSubtile(Point subtile)
        {
            if (TreesOnTile != null)
            {
                foreach (Entity tree in TreesOnTile)
                {
                    if (MapManager.WorldPosToSubtile(tree.PlaySiteLocation) == subtile)
                    {
                        return true;
                    }
                    
                }
                
            }

            return false;
        }

        /*
        public bool ContainsTree(Common.Direction edge)
        {
            if (TreesOnTile != null)
            {
                return TreesOnTile[(int)edge] != null;
                //return TreesOnTile.Contains(tree);
            }

            return false;
        }
        */


        public TerrainTile(int x, int y)
        {
            AddToLookup();
            
            TilePos = new Maps.TilePos(x, y);

           
            TileResources = new Dictionary<ResourceType, TileResourceContainer>();

            
           /* TileResourceContainer firewood = new TileResourceContainer(this, GameData.Instance.AllResourceTypes["firewood"]); // TODO: can we avoid hardcoding this???
            TileResources.Add("firewood", firewood); // not needed under water...            
            The.Sim.PlaySite.AddResourceContainer(firewood); // NEW
            */

            Temperature = GameData.Instance.Constants.MeanAmbientTemperature;

            WaterAmount = 700f; // Moisture = 0.3f
        }

        public TerrainTile()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

      /*  public TerrainTile()
        {
            Firewood = new TileResourceContainer(this) { TileResourceType = GameData.Instance.AllTileResourceTypes["tileresource:firewood"] };

            WaterAmount = 700f; // Moisture = 0.3f

        }*/


        public void Destroy()
        {
            RemoveIDEntry();
        }

        public void ComputeRoadConnections()
        {
            if (Roads != null || WheelPaths != null || FootPaths != null)
            {
                if (renderedRoads == null)
                {
                    renderedRoads = new List<RoadAndPathQuad>();
                }
            }
            else
            {
                return;
            }

            renderedRoads.Clear();

            if (Roads != null )
            {          
                ComputeRoadConnections(Roads, TreatStructuresOrTerrain.Structure);
            }

            if (WheelPaths != null)
            {
                ComputeRoadConnections(WheelPaths, TreatStructuresOrTerrain.Terrain);
            }

            if (FootPaths != null)
            {
                ComputeRoadConnections(FootPaths, TreatStructuresOrTerrain.Terrain);
            }

            return;

       /*     if (Roads != null)
            {
                renderedRoads.Clear();

                for (int i = 0; i < 8; i++)
                {
                    if (Roads[i] != null)
                    {
                        Roads[i].RenderAsConnectedGroundSprite.IsRenderedAsConnection = false;
                    }
                }
                Rectangle?[,] allConnections; // = AllConnectedGroundSprites["gravelroad"];

                MapManager map = UWGame.SimSide.Instance.Map;
                for (int i = 0; i < 8; i++)
                {
                    if (Roads[i] != null)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (i != j && Roads[j] != null 
                                && !Roads[j].RenderAsConnectedGroundSprite.IsRenderedAsConnection 
                                && Roads[i].EntityType.RenderAsConnectedGroundSpriteType.SpriteName == Roads[j].EntityType.RenderAsConnectedGroundSpriteType.SpriteName 
                               )
                            {
                                allConnections = AllConnectedGroundSprites[Roads[i].EntityType.RenderAsConnectedGroundSpriteType.SpriteName];
                                if (allConnections[i, j] != null)
                                {
                                    RoadAndPathQuad quad = new RoadAndPathQuad();
                                    quad.SetupQuadVertices(MapManager.TileToWorldPos(this),
                                        allConnections[i, j].Value,
                                        UWGame.SimSide.Instance.FlatSpriteSheet.Texture);

                                    renderedRoads.Add(quad);

                                    // make sure we don't render the roads more than once:
                                    Roads[i].RenderAsConnectedGroundSprite.IsRenderedAsConnection = true;
                                    Roads[j].RenderAsConnectedGroundSprite.IsRenderedAsConnection = true;
                                }
                            }
                        }
                    }
                }
                // add nonconnected (IsRenderedAsConnection == false) roads here:
                for (int i = 0; i < 8; i++)
                {
                    if (Roads[i] != null && !Roads[i].RenderAsConnectedGroundSprite.IsRenderedAsConnection)
                    {
                        RoadAndPathQuad quad = new RoadAndPathQuad();

                        allConnections = AllConnectedGroundSprites[Roads[i].EntityType.RenderAsConnectedGroundSpriteType.SpriteName];

                        quad.SetupQuadVertices(MapManager.TileToWorldPos(this), allConnections[i, i].Value, UWGame.SimSide.Instance.FlatSpriteSheet.Texture);
                        renderedRoads.Add(quad);

                        Roads[i].RenderAsConnectedGroundSprite.IsRenderedAsConnection = true;
                    }
                }               

            }    */        
        }

        private enum TreatStructuresOrTerrain { Structure, Terrain }

        /// <summary>
        /// TODO DECOUPLE
        /// </summary>
        /// <param name="roadsOrPaths"></param>
        /// <param name="itemsToTreat"></param>
        private void ComputeRoadConnections(Entity[] roadsOrPaths, TreatStructuresOrTerrain itemsToTreat)        
        {
            if (roadsOrPaths != null)
            {
                //renderedRoads.Clear();

                for (int i = 0; i < 8; i++)
                {
                    if (roadsOrPaths[i] != null)
                    {
                        roadsOrPaths[i].Renderable.RenderAsConnectedGroundSprite.IsRenderedAsConnection = false;
                    }
                }
                Rectangle?[,] allConnections; // = AllConnectedGroundSprites["gravelroad"];

                TerrainPath terrainPathComponent;

                MapManager map = The.Map;
                for (int i = 0; i < 8; i++)
                {
                    if (roadsOrPaths[i] != null)
                    {
                        RenderAsConnectedGroundSprite iRenderAsConnectedGroundSprite = roadsOrPaths[i].Renderable.RenderAsConnectedGroundSprite;

                        for (int j = 0; j < 8; j++)
                        {
                            if (i != j && roadsOrPaths[j] != null
                                && (itemsToTreat == TreatStructuresOrTerrain.Structure || roadsOrPaths[j].TerrainPath.Value > 0) // && roadsOrPaths[j].Structure.iscom)
                                && !roadsOrPaths[j].Renderable.RenderAsConnectedGroundSprite.IsRenderedAsConnection
                                && roadsOrPaths[i].EntityType.RenderableTypeMode.RenderAsConnectedGroundSpriteType.AssetName == roadsOrPaths[j].EntityType.RenderableTypeMode.RenderAsConnectedGroundSpriteType.AssetName
                               )
                            {
                                allConnections = ClientSide.Client.AllConnectedGroundSprites[roadsOrPaths[i].EntityType.RenderableTypeMode.RenderAsConnectedGroundSpriteType.AssetName];
                                if (allConnections[i, j] != null)
                                {                                    
                                    //for tinting, take the average for now
                                    Vector4 tint1 = roadsOrPaths[i].TerrainPath.GetColor();
                                    Vector4 tint2 = roadsOrPaths[j].TerrainPath.GetColor();


                                    Vector4 tint = (tint1 + tint2) / 2f;

                                    Renderable renderable = new Renderable((Entity)null, null); // Unfinished- TODO!!!
                                    renderable.SetTintColor(new Color(tint));

                                    RoadAndPathQuad quad = new RoadAndPathQuad();

                                    Rectangle sourceRect = allConnections[i, j].Value;
                                    
                                    quad.SetupQuadVertices(
                                        MapManager.TileToWorldPos(this),
                                        new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f),
                                        sourceRect,                                        
                                        The.Client.FlatSpriteSheet.Texture, false); // , tint

                                    renderedRoads.Add(quad);

                                    // make sure we don't render the roads more than once:
                                    iRenderAsConnectedGroundSprite.IsRenderedAsConnection = true;
                                    roadsOrPaths[j].Renderable.RenderAsConnectedGroundSprite.IsRenderedAsConnection = true;
                                }
                            }
                        }
                    }
                }

                // add nonconnected (IsRenderedAsConnection == false) roads here:
                for (int i = 0; i < 8; i++)
                {
                    if (roadsOrPaths[i] != null)
                    {
                        RenderAsConnectedGroundSprite renderAsConnectedGroundSprite = roadsOrPaths[i].Renderable.RenderAsConnectedGroundSprite;

                        if (!renderAsConnectedGroundSprite.IsRenderedAsConnection
                        && (itemsToTreat == TreatStructuresOrTerrain.Structure || roadsOrPaths[i].TerrainPath.Value > 0))
                        {

                            Vector4 tint = roadsOrPaths[i].TerrainPath.GetColor();

                            Renderable renderable = new Renderable((Entity)null, null); // Unfinished- TODO!!!
                            renderable.SetTintColor(new Color(tint));


                            RoadAndPathQuad quad = new RoadAndPathQuad();

                            allConnections = ClientSide.Client.AllConnectedGroundSprites[roadsOrPaths[i].EntityType.RenderableTypeMode.RenderAsConnectedGroundSpriteType.AssetName];

                            Rectangle sourceRect = allConnections[i, i].Value;

                            quad.SetupQuadVertices(MapManager.TileToWorldPos(this),
                                new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f),
                                sourceRect, The.Client.FlatSpriteSheet.Texture, false);

                            renderedRoads.Add(quad);

                            renderAsConnectedGroundSprite.IsRenderedAsConnection = true;
                        }
                    }
                }
            }
        }

        public byte GetCost(SurfaceType.TransportType transport, SurfaceType.TerrainFeatures features)
        {
            return TerrainType.Cost(transport, features);
         
        }

        /// <summary>
        /// TODO: move to Renderable/RenderAsGroundSprite
        /// 
        /// only roads...
        /// </summary>
        /// <param name="roadAndPathVertices"></param>
        /// <param name="quadIndex"></param>
        public void CopyQuadToVertexBuffer(VertexGroundFeature[] roadAndPathVertices, ref int quadIndex, Renderable.AdditionalEffect? overridingEffectID = null)
        {
            if (roadConnectionsAreDirty)
            {
                ComputeRoadConnections();
                roadConnectionsAreDirty = false;
            }

            if (renderedRoads != null)
            {
                foreach (RoadAndPathQuad quad in renderedRoads)
                {
                    if (quad.CopyQuadToVertexBuffer(roadAndPathVertices, quadIndex))
                    {
                        quadIndex++;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            
        }

        public float GetDustFactor()
        {
            return (1f - Moisture);

        }


        public void AddFirewood()
        {
           /* if (Firewood == null)
            {
                Firewood = new TileResourceContainer(this) { TileResourceType = GameData.Instance.AllTileResourceTypes["tileresource:firewood"] };
            }*/


        }

        public TileResourceContainer AddResource(string resourceKeyName, float totalHarvestableBulk)
        {
            TileResourceContainer container = GetTileResourceContainer(resourceKeyName);

            container.SetTotalHarvestableBulk(totalHarvestableBulk);

            return container;
        }

        public TileResourceContainer AddResource(ResourceType resourceType, float totalHarvestableBulk)
        {
            TileResourceContainer container = GetTileResourceContainer(resourceType);

            container.SetTotalHarvestableBulk(totalHarvestableBulk);

            return container;
        }

        public TileResourceContainer AddResource(string resourceKeyName, int noOfResourceItems)
        {
            TileResourceContainer container = GetTileResourceContainer(resourceKeyName);         

            container.SetResourceItems(noOfResourceItems);

            return container;
        }

        public TileResourceContainer AddResource(ResourceType resourceType, int noOfResourceItems)
        {
            TileResourceContainer container = GetTileResourceContainer(resourceType);

            container.SetResourceItems(noOfResourceItems);

            return container;
        }

        private TileResourceContainer GetTileResourceContainer(string resourceKeyName)
        {
            ResourceType resourceType = GameData.Instance.AllResourceTypes[resourceKeyName];

            return GetTileResourceContainer(resourceType);
        }

        private TileResourceContainer GetTileResourceContainer(ResourceType resourceType)
        {
            if (TileResources == null)
            {
                TileResources = new Dictionary<ResourceType, TileResourceContainer>();
            }

            TileResourceContainer container;
           
            if (!TileResources.TryGetValue(resourceType, out container))
            {
                container = new TileResourceContainer(this, resourceType);

                /* TileResources.Add(resourceKeyName, container);

                 The.Sim.PlaySite.AddResourceContainer(container); // NEW*/
            }
            return container;
        }

        public Terrain GetCenterTerrain()
        {
            if (Terrain != null)
            {
                return Terrain;
            }
            else return TerrainSubtiles[1][1];
        }


        public bool IsPartlyUnderWater()
        {
            if (Terrain != null)
            {
                return Terrain.IsUnderWater();
            }
            else
            {
                for (int sx = 0; sx < 3; sx++)
                {
                    for (int sy = 0; sy < 3; sy++)
                    {
                        if (TerrainSubtiles[sx][sy].IsUnderWater())
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }


        public bool IsFullyUnderWater()
        {
            if (Terrain != null)
            {
                return Terrain.IsUnderWater();
            }
            else
            {
                for (int sx = 0; sx < 3; sx++)
                {
                    for (int sy = 0; sy < 3; sy++)
                    {
                        if (!TerrainSubtiles[sx][sy].IsUnderWater())
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        public override string ToString()
        {
            return X.ToString() + "," + Y.ToString();
        }

        #region ILookup

        private TerrainTileID id = TerrainTileID.Invalid;
        static TerrainTileID IDCounter = TerrainTileID.First;

        public TerrainTileID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public TerrainTileID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= TerrainTileID.Max)
            {
                throw new Exception("Astounding, TerrainTileID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public TerrainTileID SnapshotID(Snapshotter sn, TerrainTileID id)
        {
            return (TerrainTileID)sn.DoEnum(id);
        }

        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }



        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != TerrainTileID.Invalid)
                LookUpSortedDictionary<TerrainTile, TerrainTileID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = TerrainTileID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUpSortedDictionary<TerrainTile, TerrainTileID>.Remove(this);
        }

        void ILookUp<TerrainTile, TerrainTileID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = TerrainTileID.First;
        }

        void ILookUp<TerrainTile, TerrainTileID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUpSortedDictionary<TerrainTile, TerrainTileID>.Create();
        }


        #endregion

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            if (X == 18 && Y == 10)
            {

            }

            if (sn.mode != Snapshotter.Mode.Load)
            {
               // snapshotAllegiancesThatSeeThisTile = Snapshotter.GetIDs(AllegiancesThatSeeThisTile);
                snapshotAllegiancesThatSeeThisTile = AllegiancesThatSeeThisTile.Select(a => a.ID).ToList();
                snapshotEntitiesThatSeeThisTile = EntitiesThatSeeThisTile.Select(e => e.EntityID).ToList();

                if (TileResources != null)
                {
                    snapshotTileResources = TileResources.ToDictionary(r => r.Key.KeyName, r => r.Value.ID);
                }

                if (EntitiesOnTile != null)
                {
                    snapshotEntitiesOnTile = EntitiesOnTile.Select(e => e.EntityID).ToList();
                }                

                if (TreesOnTile != null)
                {
                    snapshotTreesOnTile = TreesOnTile.Select(t => t.ID).ToList();
                }
             

                // all this clean up should not be necessary, it is time to find the real bugs
                // #PROCCHANGE: test destroyed allegiances to see that their keys are removed as they should.
                if (RememberedRootEntitiesOnTile != null)
                {
                    snapshotRememberedEntitiesOnTile = new Dictionary<AllegianceID, List<MemoryFactID>>();
                    foreach (var item in RememberedRootEntitiesOnTile)
                    {
                        //Snapshotting cleanup. Zones can contain invalid allegiances if they have died out
                        //And those would get cleaned up later on?
                        //But we still dont want to snapshot these so we are doing some extra cleanup here in this case.
                        if (item.Key.Allegiance.ID != AllegianceID.Invalid)
                        {
                            snapshotRememberedEntitiesOnTile.Add(item.Key.Allegiance.ID, item.Value.Select(m => m.ID).ToList());
                        }                       
                    }
                }

                if (RememberedProcessesOnTile != null)
                {
                    snapshotRememberedProcessesOnTile = new Dictionary<AllegianceID, List<ProcessMemoryID>>();
                    foreach (var item in RememberedProcessesOnTile)
                    {
                        //Snapshotting cleanup. Zones can contain invalid allegiances if they have died out
                        //And those would get cleaned up later on?
                        //But we still dont want to snapshot these so we are doing some extra cleanup here in this case.
                        if (item.Key.Allegiance.ID != AllegianceID.Invalid)
                        {
                            snapshotRememberedProcessesOnTile.Add(item.Key.Allegiance.ID, item.Value.Select(m => m.ID).ToList());
                        }
                    }
                }

                if (HarvestJobs != null)
                {
                    snapshotHarvestJobs = new Dictionary<EntityGroupID,Dictionary<ResourceType,List<JobID>>>();
                    foreach (var item in HarvestJobs)
	                {
                        Dictionary<ResourceType,List<JobID>> jobs = new Dictionary<ResourceType,List<JobID>>();
                        snapshotHarvestJobs.Add(item.Key, jobs);

                        foreach (var item2 in item.Value)
	                    {
                            jobs.Add(item2.Key, item2.Value.Select(j => j.ID).ToList());		 
                        } 
                    }
                }

                if (Zones != null)
                {
                    snapshotZones = new Dictionary<AllegianceID, List<ZoneID>>();
                    foreach (var item in Zones)
                    {
                        //Snapshotting cleanup. Zones can contain invalid allegiances if they have died out
                        //And those would get cleaned up later on?
                        //But we still dont want to snapshot theese so we are doing some extra cleanup here in this case.
                        if (item.Key.ID != AllegianceID.Invalid)
                        {
                            snapshotZones.Add(item.Key.ID, item.Value.Select(z => z.ID).ToList());
                        }
                        else
                        {
                            int i = 0;
                        }

                    }
                }       
         
                if (BaseCenterForMultiTileEntities != null)
                {
                    snapshotBaseCenterEntities = new List<EntityID>();
                    foreach (var item in BaseCenterForMultiTileEntities)
                    {
                        if (item.ID != EntityID.Invalid)
                        {
                            snapshotBaseCenterEntities.Add(item.ID); // crash prevention... invalid ids were present here :(
                        }
                    }
                    //snapshotBaseCenterEntities = BaseCenterForMultiTileEntities.Select(e => e.EntityID).ToList();
                }


                if (TerrainSubtiles != null)
                {
                    int width = Common.GetJaggedArrayWidth(TerrainSubtiles);
                    int height = Common.GetJaggedArrayHeight(TerrainSubtiles);                    

                    Common.InitJaggedArray(ref snapshotTerrainSubtiles, width, height);

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            snapshotTerrainSubtiles[x][y] = TerrainSubtiles[x][y].ID;

                        }
                    }
                }

            }

            ID = SnapshotID(sn, ID);
            IDCounter = sn.DoEnum(IDCounter);

            snapshotEntitiesOnTile = sn.DoList(snapshotEntitiesOnTile);
            snapshotAllegiancesThatSeeThisTile = sn.DoList(snapshotAllegiancesThatSeeThisTile);
            snapshotEntitiesThatSeeThisTile = sn.DoList(snapshotEntitiesThatSeeThisTile);
            snapshotRememberedEntitiesOnTile = sn.DoMultiMap(snapshotRememberedEntitiesOnTile);
            snapshotRememberedProcessesOnTile = sn.DoMultiMap(snapshotRememberedProcessesOnTile);         
            if (snapshotZones != null)
            {
                foreach (var zone in snapshotZones)
                {
                    if (zone.Key == AllegianceID.Invalid)
                    {
                        int i = 0;
                    }
                }
            }
            snapshotZones = sn.DoMultiMap(snapshotZones);
            snapshotTreesOnTile = sn.DoList(snapshotTreesOnTile);
            snapshotBaseCenterEntities = sn.DoList(snapshotBaseCenterEntities);
            snapshotTileResources = sn.DoDictionary(snapshotTileResources);
            snapshotHarvestJobs = sn.DoNestedMultiMap(snapshotHarvestJobs);
            //snapshotTiledEntities = sn.DoList(snapshotTiledEntities);
            snapshotOperatingAreaOf = sn.SnapshotID<Expedition, ExpeditionID>(OperatingAreaOf);
            snapshotTerrainSubtiles = sn.DoJaggedArray(snapshotTerrainSubtiles);
            snapshotTerrain = sn.SnapshotID<Terrain, TerrainID>(Terrain);

            GeoLayoutEntitiesOnTile = sn.DoList(GeoLayoutEntitiesOnTile);
            this.ColorOfWater = sn.DoVector4(ColorOfWater);
         /*   this.DryOrganicMaterialOnSurface = sn.DoFloat(DryOrganicMaterialOnSurface);
            this.FibrousMaterialOnSurface = sn.DoFloat(FibrousMaterialOnSurface);*/
            this.HasEverBeenSeenByPlayer = sn.DoBool(HasEverBeenSeenByPlayer);
            this.Humidity = sn.DoFloat(Humidity);
            //this.Level = sn.DoEnum(Level)
            this.moisture = sn.DoFloat(moisture);
          /*  this.Nitrogen = sn.DoFloat(Nitrogen);
            this.NonFibrousMaterialOnSurface = sn.DoFloat(NonFibrousMaterialOnSurface);*/
           // this.Owner = (EntityGroupID?)sn.DoEnumNullable((ulong?)Owner);
            this.Owner = sn.DoEnumNullable(Owner);
          //  this.Phosphorous = sn.DoFloat(Phosphorous);
            this.Temperature = sn.DoFloat(Temperature);           
            this.TilePos = sn.DoTilePos(TilePos);
            this.waterAmount = sn.DoFloat(waterAmount);
            this.WaterBottomTint = sn.DoVector4(WaterBottomTint);
            this.ProcessesOnTile = sn.DoList(ProcessesOnTile);

            sn.Ignore(DesignerPlacedResources); // no Sim effect
            sn.Ignore(Terrain);
            sn.Ignore(TerrainSubtiles);
            sn.Ignore(roadConnectionsAreDirty); // DECOUPLE: should be a client flag
           
            sn.Ignore(TreesOnTile);
            sn.Ignore(EntitiesOnTile);
            sn.Ignore(BaseCenterForMultiTileEntities);
            sn.Ignore(GeoLayoutEntitiesOnTile);
            sn.Ignore(EdgeLayoutEntities);

            sn.Ignore(RememberedRootEntitiesOnTile);
            sn.Ignore(RememberedProcessesOnTile);
            sn.Ignore(RenderablesOnTile); // particle emitters, sounds? TODO, perhaps as a PersistentRenderable 
            sn.Ignore(Zones);
            sn.Ignore(EntitiesThatSeeThisTile);
            sn.Ignore(AllegiancesThatSeeThisTile);
            sn.Ignore(HarvestJobs);
            sn.Ignore(TileResources);
            sn.Ignore(Level);

            sn.Ignore(FootPaths); // perhasp add these later.
            sn.Ignore(WheelPaths);
            sn.Ignore(Roads);
            sn.Ignore(renderedRoads); // DECOUPLE

            sn.Ignore(TerrainType);


            return this;
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

        public void LoadPostProcess(Snapshotter sn)
        {
            if (X == 9 && Y == 9)
            {

            }

            sn.RegisterLoadPostProcessCall(this);

            AllegiancesThatSeeThisTile = new HashSet<Allegiance>(snapshotAllegiancesThatSeeThisTile.Select(a => LookUp<Allegiance, AllegianceID>.FindByID(a)));
            EntitiesThatSeeThisTile = new HashSet<Entity>(snapshotEntitiesThatSeeThisTile.Select(e => Entity.FindByID(e)));

            if (snapshotTileResources != null)
            {
                TileResources = snapshotTileResources.ToDictionary(
                    r => GameData.Instance.AllResourceTypes[r.Key], 
                    r => (TileResourceContainer)LookUp<ResourceContainer, ResourceID>.FindByID(r.Value));

                snapshotTileResources = null;
            }
            

            if (snapshotEntitiesOnTile != null)
            {
                EntitiesOnTile = snapshotEntitiesOnTile.Select(e => Entity.FindByID(e)).ToList();
                snapshotEntitiesOnTile = null;
            }
            

            if (snapshotTreesOnTile != null)
            {
                TreesOnTile = snapshotTreesOnTile.Select(e => Entity.FindByID(e)).ToList();
                snapshotTreesOnTile = null;
            }


           /* if (snapshotTiledEntities != null)
            {
                TiledEntityOnTile = snapshotTiledEntities.Select(e => Entity.FindByID(e)).ToList();
                snapshotTiledEntities = null;
            }*/

            if (snapshotRememberedEntitiesOnTile != null)
            {
                RememberedRootEntitiesOnTile = new Dictionary<SharedKnowledge, List<MemoryFact>>();
                foreach (var item in snapshotRememberedEntitiesOnTile)
                {
                    RememberedRootEntitiesOnTile.Add(LookUp<Allegiance, AllegianceID>.FindByID(item.Key).SharedKnowledge, 
                        item.Value.Select(m => LookUp<MemoryFact, MemoryFactID>.FindByID(m)).ToList());
                }
                snapshotRememberedEntitiesOnTile = null;
            }

            if (snapshotRememberedProcessesOnTile != null)
            {
                RememberedProcessesOnTile = new Dictionary<SharedKnowledge, List<ProcessMemory>>();
                foreach (var item in snapshotRememberedProcessesOnTile)
                {
                    RememberedProcessesOnTile.Add(LookUp<Allegiance, AllegianceID>.FindByID(item.Key).SharedKnowledge,
                        item.Value.Select(m => LookUp<ProcessMemory, ProcessMemoryID>.FindByID(m)).ToList());
                }
                snapshotRememberedProcessesOnTile = null;
            }

            if (snapshotHarvestJobs != null)
            {
                HarvestJobs = new Dictionary<EntityGroupID, Dictionary<ResourceType, List<ProcessJob>>>();
                foreach (var item in snapshotHarvestJobs)
                {
                    Dictionary<ResourceType, List<ProcessJob>> jobs = new Dictionary<ResourceType, List<ProcessJob>>();
                    HarvestJobs.Add(item.Key, jobs);

                    foreach (var item2 in item.Value)
                    {
                        jobs.Add(item2.Key, item2.Value.Select(j => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
                    }
                }
                snapshotHarvestJobs = null;
            }

            if (snapshotZones != null)
            {
                Zones = new Dictionary<Allegiance, List<Zone>>();
                foreach (var item in snapshotZones)
                {
                    if (item.Key != null)
                    {
                        Zones.Add(LookUp<Allegiance, AllegianceID>.FindByID(item.Key),
                            item.Value.Select(z => LookUp<Zone, ZoneID>.FindByID(z)).ToList());
                    }
                }
                snapshotZones = null;
            }

            if (snapshotBaseCenterEntities != null)
            {
                BaseCenterForMultiTileEntities = snapshotBaseCenterEntities.Select(e => Entity.FindByID(e)).ToList();
                snapshotBaseCenterEntities = null;
            }

            if (snapshotTerrainSubtiles != null)
            {
                int width = Common.GetJaggedArrayWidth(snapshotTerrainSubtiles);
                int height = Common.GetJaggedArrayHeight(snapshotTerrainSubtiles);

                Common.InitJaggedArray(ref TerrainSubtiles, width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        TerrainSubtiles[x][y] = LookUpSortedDictionary<Terrain, TerrainID>.FindByID(snapshotTerrainSubtiles[x][y]);
                    }
                }

                snapshotTerrainSubtiles = null;
            }

            Terrain = LookUpSortedDictionary<Terrain, TerrainID>.FindByID(snapshotTerrain);

            OperatingAreaOf = LookUp<Expedition, ExpeditionID>.FindByID(snapshotOperatingAreaOf);


        }

        #endregion


    }

  /*  public struct RenderedRoad
    {
        public Rectangle SpriteRectangle;
        public Common.Direction Direction1;
        public Common.Direction? Direction2;


    }*/
}
