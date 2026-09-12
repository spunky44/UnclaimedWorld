using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Resources;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Maps;
using System.Xml.Serialization;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.InGameEvents.Actions
{

    /// <summary>
    /// use this to change the value of resource containers on the map.
    /// 
    /// Presently, only resource manipulations at startup is supported! There needs to be more robust handling of removed resource items, perhaps with IDs, before it should be used in ingame events.
    /// 
    /// select the containers either by category or type (maybe later: location?)
    /// 
    /// change the value:
    /// Add/multiply/set to constant
    /// 
    /// SimplexNoise - this takes a position as input and returns a factor that can change the resources in the same area by a continuous amount
    /// 
    /// </summary>
    public class ChangeResourcesAction : EventActionType
    {
        #region Resource selectors

        /// <summary>
        /// optional string result to use as resource key
        /// </summary>
        public EvalNode DynamicResourceType;

        public string[] ResourceType;

        public string[] ResourceCategory;

        /// <summary>
        /// if true, all resources are affected, but mey also be excluded 
        /// </summary>
        public bool? AllResources;

        /// <summary>
        /// if needed, list the resource types that should not be affected
        /// </summary>
        public string[] ExcludeResourceTypes;
        public string[] ExcludeResourceCategories;

        #endregion

        #region Area

        /// <summary>
        /// fill in this to limit the operation to an area
        /// </summary>
        public Area Area;

       
        #endregion

        /// <summary>
        /// fill in this if the resources should be set to some expression other than noise-derived
        /// </summary>
        public EvalNode Value;


        public NoiseParams NoiseParameters;

        public enum Operation { Multiply, Set, Add, SetMinimum }

        /// <summary>
        /// specifies how the value should be applied to each container's resources
        /// </summary>
        public Operation OperationToUse;

        
        public ChangeResourcesAction(string keyName): base(keyName)
        {

        }

        public ChangeResourcesAction()           
        {

        }

       


        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
            List<ResourceType> resourceTypes = GetResourceTypes(action);

            List<ResourceContainer> containers;

            if (Area == null)
            {
                // get resource containers from the whole map:
                containers = GetGlobalResourceContainers(resourceTypes);
            }
            else
            {
                // get/create resource containers in an area:
                containers = GetLocalResourceContainers(resourceTypes, action);
            }

            if (containers != null)
            {
                SetValues(containers, action);
            }

            return true;
        }

        private void SetValues(List<ResourceContainer> resourceContainers, EventAction action) // EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            float? evalValue = null;
            
            foreach (var item in resourceContainers)
            {
               
                float numberResult;

                if (Value != null)
                {
                    // use a constant
                    if (evalValue == null)
                    {
                        PropertyResult? value = Value.Evaluate(action); // triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

                        evalValue = value.Value.NumberResult.Value;
                    }

                    numberResult = evalValue.Value;
                }
                else if (NoiseParameters != null)
                {

                    // get/create the noise seeds for a noise map (one for each resource type):
                    SimplexNoise simplexNoise = CreateNoiseSeed(item.ResourceType); //, ref noiseSeeds);

                    // create a noise value, depending on the container position
                    numberResult = GetResourceNoiseValue(simplexNoise, NoiseParameters, item.MapPosition);

                    if (numberResult > 0)
                    {

                    }

                }
                else
                {
                    return;
                }

                int currentAmount = item.NoOfHarvestableItems;
                int newAmount;

                switch (OperationToUse)
                {
                    case Operation.Multiply:

                        newAmount = (int)Math.Round(numberResult * currentAmount); // round to nearest integer instead of towards zero
                        break;

                    case Operation.Add:

                        newAmount = (int)Math.Round(numberResult + currentAmount);
                        break;

                    case Operation.SetMinimum:

                        newAmount = (int)Math.Round(numberResult);
                        newAmount = Math.Max(currentAmount, newAmount);

                        break;

                    case Operation.Set:
                    default:

                        newAmount = (int)Math.Round(numberResult);
                        break;
                }

                newAmount = Common.ClampBottom(newAmount, 0);

                if (newAmount > 0)
                {

                }

                // TODO: test destruction of resources while jobs and goals exist!
                item.SetResourceItems(newAmount);
            }
        }


        public override string ToString()
        {
            if (ResourceType != null)
            {
                return string.Concat(this.ResourceType);
            }
            else if (ResourceCategory != null)
            {
                return string.Concat(ResourceCategory);
            }

            return "";
        }

        public static float GetResourceNoiseValue(SimplexNoise simplexNoise, NoiseParams noiseParams, Point tilePos) // ResourceContainer item)
        {
            float numberResult;
            float noise;

            noise = simplexNoise.Generate2D((float)tilePos.X, (float)tilePos.Y, noiseParams.NoiseFrequency);
           // noise = SimplexNoiseGenerator.Generate2D((float)tilePos.X, (float)tilePos.Y, noiseParams.NoiseFrequency);
                    

            noise += 1f; // now in range 0 - 2

            if (noiseParams.NoiseAmplitude.HasValue)
            {
                numberResult = noise * noiseParams.NoiseAmplitude.Value;
            }
            else
            {
                numberResult = noise;
            }

            if (noiseParams.NoiseAddend.HasValue)
            {
                numberResult += noiseParams.NoiseAddend.Value;
            }

            return numberResult;
        }


        private SimplexNoise CreateNoiseSeed(ResourceType resourceType) //, ref Dictionary<ResourceType, byte[]> noiseSeeds)
        {
          //  Tuple<NoiseParams, byte[]> noise;
            Tuple<NoiseParams, SimplexNoise> noise;

           // byte[] noiseSeed;
            SimplexNoise simplexNoise;

            if (The.Sim.ResourceNoiseSeeds == null)
            {
                The.Sim.ResourceNoiseSeeds = new Dictionary<Resources.ResourceType, Tuple<NoiseParams, SimplexNoise>>();
            }

            
            if (!The.Sim.ResourceNoiseSeeds.TryGetValue(resourceType, out noise))
            {
                simplexNoise = new SimplexNoise();

               // noiseSeed = SimplexNoiseGenerator.CreateSeedNumbers(The.Sim.GameplayRandomGenerator);
                
                noise = new Tuple<NoiseParams, SimplexNoise>(this.NoiseParameters, simplexNoise);
                The.Sim.ResourceNoiseSeeds.Add(resourceType, noise); 
                                 
            }
            else
            {
                simplexNoise = noise.Item2;
            }

            return simplexNoise;

           // SimplexNoiseGenerator.SeedNumbers = noiseSeed;

        }



        private List<ResourceType> GetResourceTypes(EventAction action) //EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            List<ResourceType> list = new List<ResourceType>();
            if (AllResources == true)
            {
                foreach (var item in GameData.Instance.AllResourceTypes)
                {
                    list.Add(item.Value);
                }
            }

            if (DynamicResourceType != null)
            {
                PropertyResult? resourceResult = DynamicResourceType.Evaluate(action); // triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                if (resourceResult != null)
                {
                    AppendResourceType(list, resourceResult.Value.StringResult);
                }
            }

            if (ResourceType != null)
            {
                foreach (var item in ResourceType)
                {
                    AppendResourceType(list, item);
                }
            }

            if (ResourceCategory != null)
            {
                foreach (var item in ResourceCategory)
                {
                    AppendResourceCategory(list, item);
                }
            }

            if (ExcludeResourceTypes != null)
            {
                foreach (var item in ExcludeResourceTypes)
                {
                    RemoveResourceType(list, item);
                }
            }

            if (ExcludeResourceCategories != null)
            {
                foreach (var item in ExcludeResourceCategories)
                {
                    RemoveResourceCategory(list, item);
                }
            }

            list = list.Distinct().ToList();

            return list;
        }

        private List<ResourceContainer> GetGlobalResourceContainers(List<ResourceType> resourceTypes)
        {
            List<ResourceContainer> containers = new List<ResourceContainer>();
            foreach (var item in resourceTypes)
            {
                ObservableList<ResourceContainer> list;
                if (The.Sim.PlaySite.Resources.TryGetValue(item,
                    out list))
                {
                    containers.AddRange(list.GetAsList());
                }   
            }

            return containers;

        }


        private List<ResourceContainer> GetLocalResourceContainers(List<ResourceType> resourceTypes, 
           EventAction action) // EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            List<ResourceContainer> containers = new List<ResourceContainer>();

            //Rectangle tileArea = ComputeArea(triggeringEntity, targetEntity);
            List<TilePos> tilePositions = Area.ComputeArea(action); //triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

            TerrainTile tile;
            TileResourceContainer container;
            foreach (var pos in tilePositions)
            {
                tile = The.Map.GetTile(pos);

                foreach (var resourceType in resourceTypes)
                {
                    if (!tile.TileResources.TryGetValue(resourceType, out container))
                    {                       
                        // create and add an empty container if it does not exist:
                        container = tile.AddResource(resourceType, 0);
                    }

                    containers.Add(container);
                }
            }


            return containers;
        }


       
        private List<ResourceContainer> GetContainers(List<ResourceType> resourceTypes)
        {
            List<ResourceContainer> containers = new List<ResourceContainer>();

            foreach (var item in resourceTypes)
            {
                ObservableList<ResourceContainer> list;
                if (The.Sim.PlaySite.Resources.TryGetValue(item, out list))
                {
                    containers.AddRange(list.GetAsList());
                }
            }

            return containers;
        }

        private void AppendResourceType(List<ResourceType> resourceTypes, string resourceKey) 
        {
            resourceTypes.Add(GameData.Instance.AllResourceTypes[resourceKey]);
        }

        private void RemoveResourceType(List<ResourceType> resourceTypes, string resourceKey)
        {
            ResourceType type = GameData.Instance.AllResourceTypes[resourceKey];
            resourceTypes.RemoveAll(r => r == type);
        }

        private void AppendResourceCategory(List<ResourceType> resourceTypes, string categoryKey) 
        {
            var resourceTypesOfCategory = GetResourceTypesOfCategory(categoryKey);

            resourceTypes.AddRange(resourceTypesOfCategory);

        }

        private IEnumerable<Resources.ResourceType> GetResourceTypesOfCategory(string categoryKey)
        {
            ResourceCategory category = GameData.Instance.AllResourceCategories[categoryKey];

            // get the resource types with this category:
            var resourceTypePairs = GameData.Instance.AllResourceTypes.Where(r => r.Value.Category == category);

            var resourceTypesOfCategory = resourceTypePairs.Select(k => k.Value);
            return resourceTypesOfCategory;
        }

        private void RemoveResourceCategory(List<ResourceType> resourceTypes, string categoryKey)
        {
            var resourceTypesOfCategory = GetResourceTypesOfCategory(categoryKey);

            resourceTypes.RemoveAll(r => resourceTypesOfCategory.Contains(r));

        }
    }

    [XmlInclude(typeof(RectangularArea))]
    [XmlInclude(typeof(Circle))]
    public abstract class Area
    {
        public abstract List<TilePos> ComputeArea(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget);

        /// <summary>
        /// optional - Tile Vector2 location
        /// </summary>
        public EvalNode Offset;

        public List<TilePos> ComputeArea(EventAction action)
        {
            return ComputeArea(action.TriggeringEntity, action.TargetEntity, action.PolledEventSource, action.DynamicTarget);
        }



        protected TilePos? GetTilePos(EvalNode node, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            PropertyResult? result = node.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

            TilePos? tilePos = null;
            if (result.HasValue)
            {
                if (result.Value.LocationResult.HasValue)
                {
                    tilePos = MapManager.WorldPosToTilePos(result.Value.LocationResult.Value.ToVector3());
                }
            }

            return tilePos;
        }


        protected TilePos? AddOffset(TilePos? upperLeftTilePos, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            if (Offset != null)
            {
                TilePos? offset = null;
                offset = GetTilePos(Offset, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

                return new TilePos(upperLeftTilePos.Value.X + offset.Value.X, upperLeftTilePos.Value.Y + offset.Value.Y);
            }
            else
            {
                return upperLeftTilePos;
            }

        }
    }

    public class RectangularArea: Area
    {
       
        public EvalNode UpperLeftLocation;

        /// <summary>
        /// optional - Tiles
        /// </summary>
        public EvalNode Width;

        /// <summary>
        /// optional - Tiles
        /// </summary>
        public EvalNode Height;



        public override List<TilePos> ComputeArea(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            List<TilePos> list = new List<TilePos>();
          
            TilePos? upperLeftTilePos = GetTilePos(UpperLeftLocation, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

            upperLeftTilePos = AddOffset(upperLeftTilePos, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
           


            int? width = null, height = null;

            if (Width != null && Height != null)
            {
                PropertyResult? result = Width.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                width = (int)Math.Round(result.Value.NumberResult.Value);

                result = Height.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                height = (int)Math.Round(result.Value.NumberResult.Value);

                Rectangle clampedArea = The.Map.GetClampedMapAreaUsingTiles(upperLeftTilePos.Value.ToPoint(), width.Value, height.Value);

                for (int x = clampedArea.X; x < clampedArea.X + clampedArea.Width; x++)
                {
                    for (int y = clampedArea.Y; y < clampedArea.Y + clampedArea.Height; y++)
                    {
                        list.Add(new TilePos(x, y));
                    }
                }

            }
            else
            {
                if (The.Map.TileIsOnMap(upperLeftTilePos.Value.ToPoint()))
                {
                    list.Add(upperLeftTilePos.Value);
                }
            }

            return list;

        }
    }



    public class Circle: Area
    {
        public EvalNode CenterLocation;

       
        /// <summary>
        /// optional - Tile radius
        /// </summary>
        public EvalNode Radius;



        public override List<TilePos> ComputeArea(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            List<TilePos> list = new List<TilePos>();
            PropertyResult? result;

            TilePos? centerTilePos = GetTilePos(CenterLocation, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

            centerTilePos = AddOffset(centerTilePos, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
           

            int? radius = null;

            if (Radius != null)
            {
                result = Radius.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                radius = (int)Math.Round(result.Value.NumberResult.Value);
                int minX, maxX, minY, maxY;

                MapManager.GetClampedMapAreaUsingTiles(centerTilePos.Value, radius.Value, out minX, out maxX, out minY, out maxY);

                for (int x = minX; x < maxX; x++)
                {
                    for (int y = minY; y < maxY; y++)
                    {
                        if (Common.DistanceOctile(centerTilePos.Value, new TilePos(x, y)) <= radius)
                        {
                            list.Add(new TilePos(x, y));
                        }
                    }
                }

            }
            else
            {
                if (The.Map.TileIsOnMap(centerTilePos.Value.ToPoint()))
                {
                    list.Add(centerTilePos.Value);
                }
            }

            return list;

        }
    }
}
