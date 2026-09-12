using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Items;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Trees;
using UWGame.SimSide.AI.Goals;
using GameStateManagement;
using UWGame.SimSide.Processes;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide.Map;
using UWGame.Control;
using UWGame.SimSide.AI;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Collisions;
namespace UWGame.SimSide.Buildings
{

    public enum StructureStates { BeingPlaced, PlacedButNotStarted, UnderConstruction, ConstructionPaused, Operational, Mothballed }

    /// <summary>
    /// Structures are something that can be built on-site.
    /// </summary>
    public class Structure : Component
    {
       /// <summary>
       /// not used/working
       /// </summary>
        public List<IAddon> AddOns = new List<IAddon>();

        public EntityID AddonTo;


        protected StructureStates state = StructureStates.BeingPlaced;

        /// <summary>
        /// a terrain entity that the structure was built on via a special action process type with structure output via ActingOnEntity.
        /// We can use this to hide the build special action, and to reenable it when this structure is destroyed.
        /// </summary>
        public EntityID? AnchorID;


       
        public string GetStateDescription()
        {
            switch (State)
            {
                //BeingPlaced
                case StructureStates.PlacedButNotStarted:
                {
                    return "Planned";
                }
                case StructureStates.UnderConstruction:
                {
                    return "Under construction";
                }
                case StructureStates.ConstructionPaused:
                {
                    return "Construction started";
                }
                case StructureStates.Operational:
                {
                    return "";  // more neutral word than "Operational", or nothing, because everything is ok. instead it should now show condition of the structure.
                }
                case StructureStates.Mothballed:
                {
                    return "Mothballed";
                }
                default:
                {
                    return "";
                }
            }
        }

        public bool ConstructionHasStarted()
        {
            return State != StructureStates.PlacedButNotStarted && State != StructureStates.BeingPlaced;
        }

        /// <summary>
        /// this determines how often we scan the construction site for obstacles
        /// </summary>
        private Regulator createClearingJobsRegulator;


        public void UpdateRenderable()
        {
            UpdateRenderableState();
        }

        private void CreateRegulators()
        {
            createClearingJobsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.5, "StructureCreateClearingJobs");
        }


        public StructureStates State
        {
            get
            {
                return state;
            }

            set
            {
                // update sprites and blocked areas
                if (state != value)
                {
                    state = value;

                    UpdateRenderableState();

                    Parent.RecomputeUpdateInterval();
                }
            }
        }

       


        public Structure(Entity parent)
            : base(parent)
        {

            UpdateRenderable();

            CreateRegulators();            

        }

     

        private void UpdateRenderableState()
        {
            if (Parent.Renderable != null)
            {
                switch (state)
                {
                    case StructureStates.BeingPlaced:
                    case StructureStates.PlacedButNotStarted:
                        Parent.SetSpriteStateFlag(StateModifier.Ordered);
                        break;

                    case StructureStates.UnderConstruction:
                        Parent.ClearSpriteStateFlag(StateModifier.Ordered);
                        Parent.SetSpriteStateFlag(StateModifier.BeingBuilt);
                        Parent.Renderable.SetOverlayRendering(false);
                        break;

                    default:
                        Parent.ClearSpriteStateFlag(StateModifier.BeingBuilt);
                        Parent.ClearSpriteStateFlag(StateModifier.Ordered);
                        break;
                }
            }
        }

        public Structure()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }
       

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

           // AddOns = (List<IAddon>)sn.DoList(AddOns);
            AddonTo = sn.DoEnum(this.AddonTo);

            AnchorID = sn.DoEnumNullable(AnchorID);

            state = sn.DoEnum(state);

            sn.Ignore(createClearingJobsRegulator);
            sn.Ignore(AddOns);

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

            CreateRegulators();
        }
 /*

        public Vector3 ConvertTopLeftOffsetToWorldPos(Rectangle sourceRect, Vector2 offset)
        {
            return new Vector3(TopLeftMapPosition.X * MapManager.tileSize + offset.X,
                                  (TopLeftMapPosition.Y + parent.EntityType.StructureType.HeightInTiles) * MapManager.tileSize - sourceRect.Height + offset.Y, 0f);

        }

        public Vector3 ConvertTopLeftOffsetToWorldPos(int heightOfUtilityMap, Vector2 offset)
        {
            return new Vector3(TopLeftMapPosition.X * MapManager.tileSize + offset.X,
                                  (TopLeftMapPosition.Y + parent.EntityType.StructureType.HeightInTiles) * MapManager.tileSize - heightOfUtilityMap + offset.Y, 0f);

        }

       
        public Point ConvertTopLeftOffsetToWorldPos(Point offset)
        {
            return new Point(TopLeftMapPosition.X * MapManager.tileSize + offset.X,
                               TopLeftMapPosition.Y * MapManager.tileSize + offset.Y);

        }
        */


        /// <summary>
        /// Make sure that placement is valid BEFORE calling!
        /// </summary>
        /// <param name="pos"></param>
        public void PlaceBuilding()
        {
            State = StructureStates.PlacedButNotStarted;
            
            The.Sim.AllStructures.Add(Parent.EntityID);


        /*    MapManager map = UWGame.SimSide.Instance.Map;
            // clear the area of trees... for now. 
            // TODO: Delete this!!! create clearing jobs instead!
            for (int x = parent.TopLeftMapPosition.X; x < parent.TopLeftMapPosition.X + parent.EntityType.StructureType.WidthInTiles; x++)
            {
                for (int y = parent.TopLeftMapPosition.Y; y < parent.TopLeftMapPosition.Y + parent.EntityType.StructureType.HeightInTiles; y++)
                {
                    // clear trees... just for testing.
                    if (map.TileMap[x][y].TreesOnTile != null)
                    {
                        for (int i = 0; i < map.TileMap[x][y].TreesOnTile.Count; i++)
                        {
                            map.TileMap[x][y].TreesOnTile[i] = null;

                        }
                    }
                }
            }*/

        }

        /*    private void ClearBlockedEdges()
            {
                byte cost;
                Point relativeTilePos;
                Point absoluteTilePos;
                Common.Direction edge;

                for (int x = 0; x < StructureType.WidthInTiles * 3; x++)
                {
                    for (int y = 0; y < StructureType.HeightInTiles * 3; y++)
                    {
                        // get the tile we're in:
                        relativeTilePos = new Point(x / 3, y / 3);
                        absoluteTilePos = Common.AddPoints(relativeTilePos, MapPosition); // MapPosition is still upper left tile..!

                        for (int transport = 0; transport < MapManager.TransportIndices.Length; transport++)
                        {
                            UWGame.SimSide.Instance.map.SetEdgeCost(new Point(x, y), , transport, PlainsType.Instance.Cost((TerrainType.TransportType)transport, TerrainType.TerrainFeatures.None));
                        }
                    }
                }

            }*/

        public void Destroy()
        {

            // remove the blocked edges:
            /*  if (parent.EntityType.TileLayoutType != null) // !parent.EntityType.StructureType.IsEdgeFeature)
              {
                  //BlockEdges(Operation.Unblock, GetUtilityMap());

              }*/
                                                

            The.Sim.AllStructures.Remove(Parent.EntityID);

            Entity entityAddedOnTo = Entity.FindByID(AddonTo);
            if (entityAddedOnTo != null)
            {
               // AddonTo.Structure.ResidentialBuilding.AddOns.Remove(parent);
                entityAddedOnTo.Structure.AddOns.Remove(Parent);
            }

            if (AnchorID.HasValue)
            {
                Entity anchor = Entity.FindByID(AnchorID);
                if (anchor != null)
                {
                    if (anchor.EntityType.IsSpecialActionOutput(Parent.EntityType))
                    {
                        // re-enable special action(s) on the anchor:
                        anchor.EnableSpecialActionsUsingAnchor();
                        //Entity.EnableSpecialActionsUsingAnchor(anchor);
                    }
                }
            }
            
        }




        /// <summary>
        /// THIS code has been temporarily disabled, but should be enabled again to test for trees in the area...
        /// Agents can escape from blocked subtiles however, so don't test for those.
        /// 
        /// make sure that the structure subtiles of the next stage are unblocked
        /// also create clearing jobs (regulated by a few seconds or so)
        /// </summary>
        /// <returns></returns>
      /*  public bool TestAreaIsClearAndAddClearingJobs()
        {
            bool areaIsClear = false;
            // make sure that the structure subtiles of the next stage are unblocked:

            byte[][] subTileMap = null;
          
            if (Parent.DirectionalLayout != null)
            {
                subTileMap = Parent.DirectionalLayout.CreateBlockedSubtileMap();
            }
            else if (Parent.PointLayout != null)
            {
                subTileMap = Parent.PointLayout.CreateBlockedSubtileMap();
            }

            //last resort: use GeometryLayout
            if (subTileMap == null)
            {
                if (Parent.GeometryLayout != null)
                {
                //    Parent.GeometryLayout.CreateIsBlockedMap(Parent.FlipHorizontally, out isBlockedData, out subTileMap);
                    subTileMap = Parent.GeometryLayout.CreateBlockedSubtileMap(Parent.FlipHorizontally);
                }
                else
                    return false;
            }

            areaIsClear = TestAllSubtilesClear(subTileMap, Parent);


            // something blocked our progress. See to it that it is cleared:
            if (!areaIsClear && createClearingJobsRegulator.IsReady())
            {
                // select the main dumping area:
                // HAULTEST
                Edge placeMaterialsAlongEdge = (Edge)The.Sim.GameplayRandomGenerator.Next(0, 3, "Structure");
                EntityGroup owner;
                LookUpOwners.ResolveEntityOwner(Parent, out owner);
                CreateClearingJobs(placeMaterialsAlongEdge, owner);

                // TODO: also add clearing of vegetation, rocks etc.!!
            }

            return areaIsClear;
        }*/

        /// <summary>
        /// if just one subtile in the blocking map is occupied by some GameObject, we return false.
        /// </summary>
        /// <param name="subtileMap"></param>
        /// <returns></returns>
        private bool TestAllSubtilesClear(byte[][] subtileMap, Entity exceptForEntity = null)
        {
            TerrainTile[][] map = The.Map.TileMap;
            TerrainTile tile;
            Vector3 structureTopLeftWorldPos = new Vector3(MapManager.tileSize * Parent.TopLeftMapPosition.Value.X, MapManager.tileSize * Parent.TopLeftMapPosition.Value.Y, 0f);
            Point itemSubtile;

            for (int x = 0; x < Parent.EntityType.StructureType.WidthInTiles; x++)
            {
                for (int y = 0; y < Parent.EntityType.StructureType.HeightInTiles; y++)
                {
                    tile = map[Parent.TopLeftMapPosition.Value.X + x][Parent.TopLeftMapPosition.Value.Y + y];
                    

                    // TODO: put this code back when the agents can clear trees etc.:
                    // test for entities:
               /*     if (tile.EntitiesOnTile != null)
                    {
                        foreach (Entity entity in tile.EntitiesOnTile)
                        {
                            if (exceptForEntity != null && exceptForEntity == entity)
                            {
                                continue;
                            }
                            else
                            {
                                if (ObjectIsBlocking(subtileMap, structureTopLeftWorldPos, entity.Location))
                                {
                                    // this entity is on top of where our structure needs to be
                                    return false;
                                }
                            }
                        }
                    }

                    // test for trees:
                    if (tile.TreesOnTile != null)
                    {
                        foreach (Entity tree in tile.TreesOnTile)
                        {
                            if (tree != null)
                            {
                                if (ObjectIsBlocking(subtileMap, structureTopLeftWorldPos, tree.Location))
                                {
                                    // this tree is on top of where our structure needs to be! It must be cleared before we can continue building!
                                    return false;
                                }                               
                            }
                        }
                    }*/
                }
            }


            return true;
        }

        /*
        private bool ObjectIsBlocking(byte[][] subtileMap, Vector3 structureTopLeftWorldPos, Vector3 objectLocation)
        {
            Point subtile = MapManager.WorldPosToRelativeSubtile(objectLocation, structureTopLeftWorldPos);

            if (MapManager.PointIsWithinArea(subtileMap, subtile)) // why do we sometimes get entities outside the area???
            {
                if (subtileMap[subtile.X][subtile.Y] == 255)
                {
                    // this tree is on top of where our structure needs to be! It must be cleared before we can continue building!
                    return true;
                }
            }
            else
            {

            }

            return false;
        }
        */

        public void ConstructionStarted(EntityID? anchorID)
        {
            // we need the anchor to re-enable actions if the process is cancelled and the output structure gets salvaged.
            this.AnchorID = anchorID;


            State = StructureStates.UnderConstruction;

            if (Parent.DirectionalLayout != null)
            {
                // NEW:                
                Parent.DirectionalLayout.RedrawTerrainCosts();

            }

            if (Parent.GeometryLayout != null)
            {
                Parent.FootprintIsDirty = true; // redraws during Update
            }
        }


        public void ConstructionFinished(EntityID? anchorID, bool createParts = false)
        {
            this.AnchorID = anchorID;

            if (Parent.NonLivingEntity != null)
                Parent.NonLivingEntity.Progress = 1f;

            State = StructureStates.Operational;


            // first unblock using the previous state       
            // clear & redraw with the new state!
            if (Parent.DirectionalLayout != null)
            {   // NEW:
                // clear & redraw with the new state!
                Parent.DirectionalLayout.ClearTerrainCosts(true);

            }
            else if (Parent.PointLayout != null)
            {  
                // clear & redraw with the new state!
                Parent.PointLayout.ClearTerrainCosts(true);

            }


            if (Parent.GeometryLayout != null) //geometrylayout does not exclude other layouts
            {
                Parent.GeometryLayout.ClearTerrainCosts(true);
            }



            if (Parent.EntityType.TerrainType != null && Parent.EntityType.TerrainType.PathType != null) //parent.EntityType.StructureType.IsRoad)
            {
                TerrainTile tile = The.Map.GetTile(Parent.MapPosition.Value);

                //tile.SetTileEdge(parent.EntityType.TerrainType.PathType, parent.EdgeLayout.EdgePosition);

                // set newly built roads to be in perfect condition:
                Parent.TerrainPath.Value = 1f;

                // remove any foot paths or tracks:
                // this calls recompute, which sees the completed road...
                tile.ClearPaths(Parent.DirectionalLayout.EdgePosition);
            }


       /*     if (Parent.Renderable != null
                && Parent.EntityType.TileLayoutType != null) // deprecate TileLayout, light sources should be placed in data
            {
                Parent.Renderable.PlaceLightSources(Parent.EntityType.TileLayoutType.GetBaseCenter(Parent.FlipHorizontally));
            }*/

            if (createParts)
            {
                Parent.CreateParts();
            }

            Parent.SetAccessPointDirty(); // NEW: recompute access points since Reserved tiles are blocked

            /*
            if (AnchorID.HasValue)
            {
                Entity anchor = Entity.FindByID(AnchorID);
                if (anchor != null)
                {
                    if (anchor.EntityType.IsSpecialActionOutput(Parent.EntityType))
                    {
                        // disable special action(s) on the anchor:
                        Entity.DisableSpecialActionsUsingAnchor(anchor);                              
                    }
                }
            }*/

        }

        

        /// <summary>
        /// delete this
        /// </summary>
        /*   private enum Operation { Block, Unblock }
           private void BlockEdges(Operation operation, UtilityMap spriteState)
           {
               byte cost;
               Point relativeTilePos;
               Point absoluteTilePos;
               Common.Direction edge;

               byte[,] blockedEdges = null;
               if (parent.EntityType.TileLayoutType.BlockedEdges != null)
               {
                   blockedEdges = parent.EntityType.TileLayoutType.GetBlockedEdges(parent.FlipHorizontally)[spriteState];
               }

               if (operation == Operation.Block)
               {
                   parent.TileLayout.RedrawTerrainCosts(TopLeftMapPosition, null, blockedEdges);
               }
               else
               {
                   //if unblock: recompute surrounding tiles!!!
                   parent.TileLayout.ClearTerrainCosts(TopLeftMapPosition, true, blockedEdges);
               }           

           }*/

        /*
        private Rectangle? GetCurrentSpriteRectangle(int billboardIndex)
        {
            string spriteName;

            if (state == StructureStates.BeingPlaced)
            {
                spriteName = Parent.EntityType.RenderableType.RenderAsBillboardType[billboardIndex].AssetName;
            }
            else if (state == StructureStates.UnderConstruction)
            {
                spriteName = Parent.EntityType.RenderableType.RenderAsBillboardType[billboardIndex].AssetName + "_construct";

            }
            else
            {
                spriteName = Parent.EntityType.RenderableType.RenderAsBillboardType[billboardIndex].AssetName;
            }


            try
            {
                return GameData.Instance.BillboardSpriteSheet.GetSourceRectangle(spriteName);
            }
            catch (KeyNotFoundException)
            {
                return null;
                
                //return GameData.Instance.BillboardSpriteSheet.SourceRectangle(parent.EntityType.Renderable.RenderAsBillboardType[billboardIndex].SpriteName);
            }
        }*/
        /*
        public Rectangle GetCurrentGroundSpriteRectangle()
        {

            if (The.Client == null)
                return Rectangle.Empty;//TODO icky


            string spriteName;
            if (Parent.NonLivingEntity != null && Parent.NonLivingEntity.Progress < 1f) // ConstructionProgress < 1f)
            {

                spriteName = Parent.EntityType.RenderableType.RenderAsGroundSpriteType.AssetName.Replace("_g", "_construct_g");
                // UnfinishedGroundSpriteName = SpriteName + "_construct_g";
                //  spriteName = parent.EntityType.StructureType.UnfinishedGroundSpriteName;
            }
            else
            {
                //spriteName = parent.EntityType.StructureType.FinishedGroundSpriteName;
                spriteName = Parent.EntityType.RenderableType.RenderAsGroundSpriteType.AssetName;
            }

            try
            {
                return The.Client.FlatSpriteSheet.GetSourceRectangle(spriteName);
            }
            catch (KeyNotFoundException)
            {
                return The.Client.FlatSpriteSheet.GetSourceRectangle(Parent.EntityType.RenderableType.RenderAsGroundSpriteType.AssetName);
            }

        }*/




        protected bool StartBuildingJob(EntityGroup ownerOfBuilding) //, Priority priority)
        {
            bool success = false;
            
            ProcessJob buildingJob;

            List<ProcessType> processTypes;
            if ( ! GameData.Instance.ProcessYieldsThisOutput.TryGetValue(Parent.EntityType, out processTypes) ) 
                return false;

            // start the job:                // more processes than just [0]???
            buildingJob = new ProcessJob(null, processTypes[0], Parent.EntityType, ownerOfBuilding); //, priority);
           /* {
                ProductionSiteLocation = Parent.Location // not needed since we always have an output entity!
            };*/

            buildingJob.BuildingJob = new BuildingJob(buildingJob); 
            // assign the output right away (different from production jobs, where the output is only created when the agent reaches the site):
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(buildingJob.ProductionProcess);
            process.AddOutputEntity(Parent);
            //buildingJob.OutputEntities.Add(Parent.EntityID);

            MapManager map = The.Map;

            // it is better if building materials are scattered around the site. They should be clear of the finished building though.
            // select the main dumping area:

            // HAULTEST - select a random edge area...
            Edge placeMaterialsAlongEdge = (Edge)The.Sim.GameplayRandomGenerator.Next(0, 3, "Structure");

          /*  if (CreateClearingJobs(placeMaterialsAlongEdge,ownerOfBuilding))
            {*/
                success = PlaceMaterialHaulingJobs(buildingJob, ownerOfBuilding, placeMaterialsAlongEdge);
           /* }
            else
            {
                success = false;
            }*/

            if (!success)
            {
                // we failed... remove the job.
                buildingJob.Destroy(true);
            }

            return success;
        }

        /// <summary>
        /// would it be better if this were a part of HaulingJobManager?
        /// </summary>
        /// <param name="materialsAlongEdge"></param>
        /// <returns></returns>
        private bool CreateClearingJobs(Edge materialsAlongEdge, EntityGroup ownerOfJob)
        {
            TerrainTile[][] map = The.Map.TileMap;
            TerrainTile tile;

            Point? dumpingGround = null;

            Item itemComponent;
            for (int x = 0; x < Parent.EntityType.StructureType.WidthInTiles; x++)
            {
                for (int y = 0; y < Parent.EntityType.StructureType.HeightInTiles; y++)
                {
                    tile = map[Parent.TopLeftMapPosition.Value.X + x][Parent.TopLeftMapPosition.Value.Y + y];
                    if (tile.EntitiesOnTile != null)
                    {
                        foreach (Entity item in tile.EntitiesOnTile)
                        {
                            if (item.Find(out itemComponent))
                            {
                                if (item.IsUnassigned(ownerOfJob.GetAllegiance().SharedKnowledge))
                                {
                                    if (!dumpingGround.HasValue)
                                    {
                                        dumpingGround = FindDumpingAreaOffSite(materialsAlongEdge, new Point(tile.X, tile.Y));
                                        if (dumpingGround == null)
                                        {
                                            // clearing failed. No dumping ground was found. (Preconditions in DoConstruct: area must be clear!)
                                            return false;
                                        }
                                    }                                    
                                  
                                    HaulingJobSpecificItem haulingJob = new HaulingJobSpecificItem(MapManager.TileToWorldPos(dumpingGround.Value), null, 
                                        ownerOfJob,
                                       // HaulingJobManager.GetPriority(ownerOfJob), 
                                        item, null,  // owner should stay the same
                                        true, false, ownerOfJob);
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildJob"></param>
        /// <param name="ownerOfBuilding"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        private bool PlaceMaterialHaulingJobs(ProcessJob buildJob, EntityGroup ownerOfBuilding, Edge edge)
        {
            HaulingJobAnyItemOfType haulingJob;

            StructureType structureType = Parent.EntityType.StructureType;


            int maxPileSize = The.Sim.GameplayRandomGenerator.Next(2,"Structure") + 2;
            int itemsInPile = 0;
          
            Vector3 foundLocation;
            bool useOneTileBorder = false;

            
            byte[][] subtileMap = null;

            //TODO: Check what corner position i need to use.
            Tuple<bool, Vector2>[][] IsBlockedData = null;
           // if (isBlocked == null)
            {
                if (Parent.CurrentSimState != null && Parent.CurrentSimState.GeometryLayoutType != null) // Parent.EntityType.GeometryLayoutType != null)
                {
                    Parent.GeometryLayout.CreateBlockedMapOverBoundsAndPadding(Parent.FlipHorizontally, out IsBlockedData, out subtileMap);
                }
                else
                    return false;
            }

            int influenceRadiusExistingItems = 1;
            int influenceCenterFromExistingItems = -8;

            int influenceRadiusFromNewPiles = 2;
            int influenceCenterFromNewPiles = -18;
            DrawInfluenceMapForMaterialPlacement(edge, subtileMap, IsBlockedData, 1, influenceCenterFromExistingItems);

            // find first pile position
            if (!FindDropOffPointForBuildingMaterials(subtileMap,IsBlockedData, useOneTileBorder, out foundLocation, influenceRadiusExistingItems, influenceCenterFromExistingItems))
            {
                // no spots found within structure tiles. we must find a spot outside the structure area:
                
                bool[][] isBlocked = null;
                isBlocked = CreateOneTileBorderIsBlockedMapForConstructionJobs(Parent);
                useOneTileBorder = true;
                subtileMap = null;
                Common.InitJaggedArray(ref subtileMap, (structureType.WidthInTiles + 2) * 3, (structureType.HeightInTiles + 2) * 3);

               DrawInfluenceMapForMaterialPlacement(edge, subtileMap, IsBlockedData, 1, influenceCenterFromExistingItems);

                if (!FindDropOffPointForBuildingMaterials(subtileMap, isBlocked, useOneTileBorder, out foundLocation, influenceRadiusFromNewPiles, influenceCenterFromNewPiles))
                {
                    //we cannot start this building job - there's no room for materials.
                    return false;
                }
            }

            if (buildJob.ProcessType.InputsByType != null)
            {
                Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> inputs;        
                if (!buildJob.GetAssignedInputs(out inputs))
                {
                    return false;
                }

                foreach (KeyValuePair<EntityType, Input> kvp in buildJob.ProcessType.InputsByType) 
                {
                    Input input = kvp.Value;
                   // int needHauling = buildJob.NeededItems[input.EntityType].NoOfItems.Value - buildJob.AssignedInputsToThisJob[input.EntityType].Count; 
                    int needHauling = buildJob.ProcessType.InputsByType[input.EntityType].Amount.NoOfItems.Value - inputs[input.EntityType].Count; 

                    for (int i = 0; i < needHauling; i++)
                    {
                        if (itemsInPile >= maxPileSize)
                        {
                            maxPileSize = The.Sim.GameplayRandomGenerator.Next(2,"Structure") + 2;
                            itemsInPile = 0;

                            FindDropOffPointForBuildingMaterials(subtileMap, IsBlockedData, useOneTileBorder, out foundLocation, influenceRadiusFromNewPiles, influenceCenterFromNewPiles);
                        }

                        haulingJob = new HaulingJobAnyItemOfType(foundLocation, ownerOfBuilding, input.EntityType, ownerOfBuilding.ID, null); // ownerOfBuilding.ID); //new HaulingJobAnyItemOfType(FrontDoorTilePos, FrontDoorTileCenterOffset, ownerOfBuilding.OwningBody.HaulingJobs, input.ItemType);

                        itemsInPile++;

                        haulingJob.RequiredByProcessJob = buildJob;
                    }
                }
            }

            return true;
        }




        private void DrawInfluenceMapForMaterialPlacement(Edge edge, byte[][] subtileMap, Tuple<bool, Vector2>[][] IsBlockedData, int circleRadius, int centerValue)
        {
            PlaceMainOverlaysForConstructionMaterials(edge, subtileMap, 20);

           // DrawInfluenceFromItemsOnSubtileMap(subtileMap, IsBlockedData, circleRadius, centerValue);
        }

        public enum Edge { Left = 0, Bottom = 1, Right = 2 }
        private void PlaceMainOverlaysForConstructionMaterials(Edge edge, byte[][] subtileMap, byte centerValue)
        {
            int subtileWidth = Common.GetJaggedArrayWidth(subtileMap); //.GetLength(0); //StructureType.WidthInTiles * 3;
            int subtileHeight = Common.GetJaggedArrayHeight(subtileMap); //.GetLength(1);  //StructureType.HeightInTiles * 3;

            float fractionOfAreaToUse = 0.25f;

            //int circleRadius;

            //Random random = Globals.Instance.RandomPredictable;

            int maxRandom = centerValue / 6;
            byte minValue = (byte)(centerValue / 2);

            //int x;
            //int y;
            switch (edge)
            {
                case Edge.Left:
                    //along left edge...
                    InfluenceMap.DrawGradientRectangle(subtileMap, Point.Zero, new Point((int)(fractionOfAreaToUse * subtileWidth), subtileHeight - 1), false, centerValue, minValue, maxRandom, InfluenceMap.GradientDirection.TopToBottom);

                    break;
                case Edge.Bottom:
                    //along bottom edge
                    InfluenceMap.DrawGradientRectangle(subtileMap, new Point(0, (int)((1f - fractionOfAreaToUse) * subtileHeight)), new Point(subtileWidth - 1, subtileHeight - 1), false, centerValue, minValue, maxRandom, InfluenceMap.GradientDirection.LeftToRight);

                    break;
                case Edge.Right:
                default:
                    //along right edge
                    InfluenceMap.DrawGradientRectangle(subtileMap, new Point((int)((1f - fractionOfAreaToUse) * subtileWidth), 0), new Point(subtileWidth - 1, subtileHeight - 1), false, centerValue, minValue, maxRandom, InfluenceMap.GradientDirection.TopToBottom);

                    break;
            }
        }

        /// <summary>
        /// avoid exisitng items
        /// </summary>
        /// <param name="subtileMap"></param>
        private void DrawInfluenceFromItemsOnSubtileMap(byte[][] subtileMap, Vector3 topLeftSubtilePosition, int circleRadius, int centerValue)
        {
            TerrainTile[][] tileMap = The.Map.TileMap;

            Vector3 structureTopLeftWorldPos = topLeftSubtilePosition;

            float sts = MapManager.subTileSize;



            Vector2 from;
            Vector2 to;
            Parent.GeometryLayout.GetBoundsWithPadding(out from,out to);
          //  float padRadius = parent.CollisionProxy.Bounds.Radius + geoType.Pad;
         //   float padRadiusSquared = padRadius * padRadius;

            Item item;
            int width = Common.GetJaggedArrayWidth(subtileMap);
            int height = Common.GetJaggedArrayHeight(subtileMap);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tileMap[x][y].EntitiesOnTile != null)
                    {
                        foreach (Entity entity in tileMap[x][y].EntitiesOnTile)
                        {
                            if (entity.Find(out item))
                            {
                                //Select subtile
                                InfluenceMap.DrawLinearInfluenceCircle(subtileMap, MapManager.WorldPosToRelativeSubtile(entity.PlaySiteLocation, structureTopLeftWorldPos),
                                                centerValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, circleRadius);
                                //
                            }
                        }
                    }
                }
            }

            
            //for (int x = startX; x < startX; x++)
            //{
            //    for (int y = startY; y < endY; y++)
            //    {
            //        if (tileMap[x][y].EntitiesOnTile != null)
            //        {
            //            foreach (Entity entity in tileMap[x][y].EntitiesOnTile)
            //            {
            //                if (entity.Find(out item))
            //                {
            //                    InfluenceMap.DrawLinearInfluenceCircle(subtileMap, MapManager.WorldPosToRelativeSubtile(entity.Location, structureTopLeftWorldPos),
            //                        centerValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, circleRadius);
            //                }
            //            }
            //        }

            //    }
            //}

            /*     TerrainTile[,] tileMap = UWGame.SimSide.Instance.map.TileMap;

                 Vector3 structureTopLeftWorldPos = new Vector3(MapManager.tileSize * mapPosition.X, MapManager.tileSize * mapPosition.Y, 0f); 

                 for (int x = 0; x < StructureType.WidthInTiles; x++)
                 {
                     for (int y = 0; y < StructureType.HeightInTiles; y++)
                     {
                         if (tileMap[x, y].ItemsOnTile != null)
                         {
                             foreach (Item item in tileMap[x, y].ItemsOnTile)
                             {
                                 InfluenceMap.DrawLinearInfluenceCircle(subtileMap, MapManager.WorldPosToRelativeSubtile(item.Location, structureTopLeftWorldPos),
                                     centerValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, circleRadius);
                             }
                         }
                    
                     }
                 }  */

        }

        ///// <summary>
        ///// avoid exisitng items
        ///// </summary>
        ///// <param name="subtileMap"></param>
        //private void DrawInfluenceFromItemsOnSubtileMap(byte[][] subtileMap, Tuple<bool, Vector2>[][] IsBlockedData, int circleRadius, int centerValue)
        //{
        //    TerrainTile[][] tileMap = The.Map.TileMap;
        //    Vector3 topLeftWorldPos = IsBlockedData[0][0].Item2.ToVector3();
        //    Point topLeftTilePoint = new Point((int)topLeftWorldPos.X / MapManager.tileSize, (int)topLeftWorldPos.Y / MapManager.tileSize);
        //   // Vector3 topLeftWorldPos = new Vector3(topLeftTilePos.X, topLeftTilePos.Y, 0f);

        //    int startX = topLeftTilePoint.X;
        //    int endX = topLeftTilePoint.X + Common.GetJaggedArrayWidth(subtileMap) / 3;
        //    int startY = topLeftTilePoint.Y;
        //    int endY = topLeftTilePoint.Y + Common.GetJaggedArrayHeight(subtileMap) / 3;

        //    Item item;
        //    for (int x = startX; x < startX; x++)
        //    {
        //        for (int y = startY; y < endY; y++)
        //        {
        //            if (tileMap[x][y].EntitiesOnTile != null)
        //            {
        //                foreach (Entity entity in tileMap[x][y].EntitiesOnTile)
        //                {
        //                    if (entity.Find(out item))
        //                    {
        //                        InfluenceMap.DrawLinearInfluenceCircle(subtileMap, MapManager.WorldPosToRelativeSubtile(entity.Location, topLeftWorldPos),
        //                            centerValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, circleRadius);
        //                    }
        //                }
        //            }

        //        }
        //    }

        /*     TerrainTile[,] tileMap = UWGame.SimSide.Instance.map.TileMap;

             Vector3 structureTopLeftWorldPos = new Vector3(MapManager.tileSize * mapPosition.X, MapManager.tileSize * mapPosition.Y, 0f); 

             for (int x = 0; x < StructureType.WidthInTiles; x++)
             {
                 for (int y = 0; y < StructureType.HeightInTiles; y++)
                 {
                     if (tileMap[x, y].ItemsOnTile != null)
                     {
                         foreach (Item item in tileMap[x, y].ItemsOnTile)
                         {
                             InfluenceMap.DrawLinearInfluenceCircle(subtileMap, MapManager.WorldPosToRelativeSubtile(item.Location, structureTopLeftWorldPos),
                                 centerValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, circleRadius);
                         }
                     }
                    
                 }
             }  

    }*/



        private bool FindDropOffPointForBuildingMaterials(byte[][] subtileMap, bool[][] isBlockedMap, bool oneTileBorder, out Vector3 foundLocation, /*out Point foundPoint, out Vector2 foundTileCenterOffset,*/ int circleRadius, int centerValue)
        {
            // foundPoint = new Point(-1, -1);
            // foundTileCenterOffset = Vector2.Zero;

            /*if (parent.EntityType.StructureType.BlockedEdges == null)
            {   // no utility map?
                throw new Exception("Missing utility map?");
                //return false;
            }*/

            foundLocation = new Vector3(-1);

            Point bestSubtilePoint;

            if (InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subtileMap, isBlockedMap, out bestSubtilePoint) > -1)
            {
                Point upperLeft = Parent.TopLeftMapPosition.Value;
                if (oneTileBorder)
                {   // adjust the tile position...
                    upperLeft = Common.AddPoints(upperLeft, new Point(-1, -1));
                }

                // convert to tile pos
                MapManager.SubtileAndTilePosToWorldPos(bestSubtilePoint, upperLeft, out foundLocation); //out foundPoint, out foundTileCenterOffset);


                InfluenceMap.DrawLinearInfluenceCircle(subtileMap, bestSubtilePoint,
                                centerValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, circleRadius);

                return true;
            }

            return false;

        }

        private bool FindDropOffPointForBuildingMaterials(byte[][] subtileMap, Tuple<bool, Vector2>[][] isBlockedData, bool oneTileBorder, out Vector3 foundLocation, /*out Point foundPoint, out Vector2 foundTileCenterOffset,*/ int circleRadius, int centerValue)
        {
           // foundPoint = new Point(-1, -1);
           // foundTileCenterOffset = Vector2.Zero;

            /*if (parent.EntityType.StructureType.BlockedEdges == null)
            {   // no utility map?
                throw new Exception("Missing utility map?");
                //return false;
            }*/

            foundLocation = new Vector3(-1);

            Vector2 bestSubtilePosition;

            if(InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subtileMap,isBlockedData, out bestSubtilePosition) > -1)
            {
                //Point upperLeft = Parent.TopLeftMapPosition;
                //if (oneTileBorder)
                //{   // adjust the tile position...
                //    upperLeft = Common.AddPoints(upperLeft, new Point(-1, -1));
                //}
              
                // convert to tile pos
               // MapManager.SubtileAndTilePosToWorldPos(bestSubtilePoint, upperLeft, out foundLocation); //out foundPoint, out foundTileCenterOffset);

                foundLocation = bestSubtilePosition.ToVector3();
           // TODO:  InfluenceMap.DrawLinearInfluenceCircle(subtileMap, bestSubtilePoint,
             //                   centerValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, circleRadius);
                return true;
            }

            return false;

        }




              

        /// <summary>
        /// this is an improvement of MoveMap that takes unbuilt structures into account. Used for finding parking/landing spots
        /// we don't want to park on top of construction work. 
        /// Also avoids unbuilt edge structures.
        /// </summary>
        /// <param name="tileArea"></param>
        /// <param name="transport"></param>
        /// <param name="protectionLevel"></param>
        /// <param name="threat"></param>
        /// <param name="approach"></param>
        /// <returns></returns>
        public static bool[][] CreateIsBlockedMapAvoidConstructions(Rectangle tileArea, SubtileLayers costMapToUse) //, ProtectionLevel protectionLevel, ThreatCategory threat, EntityApproach approach)
        {
            bool[][] isBlockedMap = null; // = new bool[tileArea.Width * 3, tileArea.Height * 3];
            Common.InitJaggedArray(ref isBlockedMap, tileArea.Width * 3, tileArea.Height * 3);
          
            Point tilePos;
            int subTileIndex;
                     
            Point structureRelativeSubtile;
           
            List<Entity> structuresOnTile = null;

            TerrainTile currentTile;

            Point subTileStart = new Point(tileArea.X * 3, tileArea.Y * 3);

            int width = Common.GetJaggedArrayWidth(isBlockedMap);
            int height = Common.GetJaggedArrayHeight(isBlockedMap);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // get the absolute tile position from the subtile, add the offset as well:
                    tilePos = new Point(tileArea.X + x / 3, tileArea.Y + y / 3); // Common.AddPoints(mapPosition, new Point(x / 3, y / 3));
                    
                    if (!The.Map.TileIsOnMap(tilePos))
                    {
                        isBlockedMap[x][y] = true;
                        continue;
                    }

                    // we are inside the map. get the structures on the tile to block them if necessary.
                    // we consider buildings being constructed blocked, as well as their special item dumping tiles. This does not prevent unloading items onto these tiles though...

               
                    if (MapManager.IsBlocked(costMapToUse.GetValue(subTileStart.X + x, subTileStart.Y + y)))
                    {
                        isBlockedMap[x][y] = true;
                        continue;
                    }


                    currentTile = The.Map.TileMap[tilePos.X][tilePos.Y];

                    // TODO: use the map flag Reserved instead
                    // NEW: Edge structures being built:
                    // Untested!
                   /* bool subTileIsBlockedByEdgeStructure = false;
                    if (currentTile.EdgeLayoutEntities != null)
                    {
                        subTileIndex = MapManager.GetLocalSubtileIndex(x, y); // keep this???
                       
                        foreach (Entity edgeStructure in currentTile.EdgeLayoutEntities)
                        {
                            if (edgeStructure.Structure != null && edgeStructure.Structure.State == StructureStates.UnderConstruction &&
                                subTileIndex == edgeStructure.DirectionalLayout.SubtileIndex)
                            {
                                isBlockedMap[x][y] = true;
                                subTileIsBlockedByEdgeStructure = true;
                                break;
                            }
                        }

                        if (subTileIsBlockedByEdgeStructure)
                        {
                            continue;
                        }
                    }*/


                    if (structuresOnTile != null)
                    {
                        structuresOnTile.Clear();
                    }

                    //structureOnTile = UWGame.SimSide.Instance.Map.TileMap[tile.X, tile.Y].GetStructuresOnTile(ref buildingsOnTile); // GetBuildingOnTile(); //TiledEntityOnTile;

                    // TODO: use the map flag Reserved instead
                    /*
                    if (currentTile.GetStructuresOnTile(ref structuresOnTile)) 
                    {
                        for (int i = currentTile.TiledEntityOnTile.Count - 1; i >= 0; i--)
                        {
                            EntityID entityIDOnTile = currentTile.TiledEntityOnTile[i];
                            Entity entityOnTile = Entity.FindByID(entityIDOnTile);
                            if (entityOnTile != null)
                            {
                                // if we are within the structure itself, use the pregenerated isblocked map.
                                // we don't just use the movementmap because we want to block unbuilt structures too!                                        
                                structureRelativeSubtile = new Point((entityOnTile.TopLeftMapPosition.X - tileArea.Location.X) * 3,
                                    (entityOnTile.TopLeftMapPosition.Y - tileArea.Location.Y) * 3);

                                bool[][] structureIsBlockedMap = entityOnTile.EntityType.TileLayoutType.GetIsBlockedSubtileMap(entityOnTile.FlipHorizontally);
                                if (MapManager.PointIsWithinArea(structureIsBlockedMap, new Point(x, y)) &&
                                    structureIsBlockedMap[x - structureRelativeSubtile.X][y - structureRelativeSubtile.Y])
                                {
                                    isBlockedMap[x][y] = true;
                                    break;
                                }

                            }
                            else
                            {
                                currentTile.TiledEntityOnTile.RemoveAt(i);
                            }
                        }*/

                        /* foreach (Entity entity in currentTile.TiledEntityOnTile)
                        {
                           if (entity.TileLayout.Building != null &&
                                (entity.TileLayout.Building.FrontDoorTilePos == tilePos
                                || entity.TileLayout.Building.BackDoorTilePos == tilePos
                                || entity.MapPosition == tilePos))
                            {
                                // block the entrance tiles completely so that items won't get dumped here and brought into the building.
                                isBlockedMap[x][y] = true;
                                break;
                            }
                            else
                            {
                                // if we are within the structure itself, use the pregenerated isblocked map.
                                // we don't just use the movementmap because we want to block unbuilt structures too!                                        
                                structureRelativeSubtile = new Point((entity.TopLeftMapPosition.X - tileArea.Location.X) * 3,
                                    (entity.TopLeftMapPosition.Y - tileArea.Location.Y) * 3);

                                bool[][] structureIsBlockedMap = entity.EntityType.TileLayoutType.GetIsBlockedSubtileMap(entity.FlipHorizontally);
                                if (MapManager.PointIsWithinArea(structureIsBlockedMap, new Point(x, y)) &&
                                    structureIsBlockedMap[x - structureRelativeSubtile.X][y - structureRelativeSubtile.Y])
                                {
                                    isBlockedMap[x][y] = true;
                                    break;
                                }
                            //}
                        }*/
                  //  }                                      

                }
            }

            return isBlockedMap;
        }

        /// <summary>
        /// Creates a blocked map with a 1 tile border around the structure.
        /// This routine is different from the other one because we only use the terrain map - not the discomfort/movement map.
        /// </summary>
        /// <returns></returns>
        public static bool[][] CreateOneTileBorderIsBlockedMapForConstructionJobs(IKnownEntityData structureData)
        {
            StructureType structureType = structureData.EntityType.StructureType;

            Rectangle tileArea =
                new Rectangle(structureData.TopLeftMapPosition.Value.X - 1, structureData.TopLeftMapPosition.Value.Y - 1, structureType.WidthInTiles + 2, structureType.HeightInTiles + 2);

            return CreateIsBlockedMapAvoidConstructions(tileArea, The.Map.TerrainCosts[SurfaceType.TransportType.Foot]);


            /*
            
            bool[,] isBlockedMap = new bool[(structureType.WidthInTiles + 2) * 3, (structureType.HeightInTiles + 2) * 3];

            // don't use discomfort map here.
            byte[, , ,] terrain = UWGame.SimSide.Instance.Map.TerrainCosts;

            Rectangle structureRectangle =
                new Rectangle(TopLeftMapPosition.X, TopLeftMapPosition.Y, structureType.WidthInTiles, structureType.HeightInTiles);
            
            Point tile;
            Common.Direction edge;
            Entity structureOnTile;
            for (int x = 0; x < isBlockedMap.GetLength(0); x++)
            {
                for (int y = 0; y < isBlockedMap.GetLength(1); y++)
                {
                    // get the absolute tile position from the subtile, add the offset as well:
                    tile = new Point(TopLeftMapPosition.X - 1 + x / 3, TopLeftMapPosition.Y - 1 + y / 3); // Common.AddPoints(mapPosition, new Point(x / 3, y / 3));

                    if (!UWGame.SimSide.Instance.Map.TileIsOnMap(tile))
                    {   // we are outside the map - blocked, of course:
                        isBlockedMap[x, y] = true;
                        continue;
                    }

                    structureOnTile = UWGame.SimSide.Instance.Map.TileMap[tile.X, tile.Y].TiledEntityOnTile;

                    if (structureOnTile != null)
                    {
                        if (structureOnTile.TileLayout.Building.FrontDoorTilePos == tile ||
                            structureOnTile.TileLayout.Building.BackDoorTilePos == tile ||
                            structureOnTile.MapPosition == tile)
                        {
                            // new: block the entrance tiles completely so that items won't get dumped here and brought into the building.
                            isBlockedMap[x, y] = true;
                            continue;
                        }
                        else
                        {
                            // if we are within the structure itself, use the pregenerated isblocked map:
                            if (structureRectangle.Contains(tile))
                            {
                                isBlockedMap[x, y] = parent.EntityType.TileLayoutType.GetIsBlockedSubtileMap(parent.FlipHorizontally)[x - 3, y - 3];
                                continue;
                            }
                        }
                    }

                    // we are outside the structure itself. use the terrain map:
                    int subTileIndex = MapManager.GetLocalSubtileIndex(x, y);
                    if (subTileIndex != 4) // ignore the center subtile.
                    {
                        edge = MapManager.GetDirectionFromSubTile(subTileIndex);

                        if (terrain[(int)TerrainType.TransportType.Foot, tile.X, tile.Y, (int)edge] == 0)
                        {
                            isBlockedMap[x, y] = true;
                            continue;
                        }

                        // NEW: Edge structures (obstacles only) being built, (or completed):
                        // Untested!
                        if (UWGame.SimSide.Instance.Map.TileMap[tile.X, tile.Y].EdgeLayoutEntities != null)
                        {
                            foreach (Entity edgeStructure in UWGame.SimSide.Instance.Map.TileMap[tile.X, tile.Y].EdgeLayoutEntities)
                            {
                                if (//edgeStructure.EntityType.StructureType.EdgeLayoutType.IsObstacle &&
                                    edgeStructure.Structure.State == States.UnderConstruction &&
                                    subTileIndex == edgeStructure.EdgeLayout.SubtileIndex)
                                {
                                    isBlockedMap[x, y] = true;
                                    break;
                                }
                            }
                        }
                    }

                }
            }

            return isBlockedMap;
             * */
        }

        /// <summary>
        /// see if the point is the entrance of a building. We don't want to move other people's items inside other people's buildings.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        

        /// <summary>
        /// examine the middle points of each edge of the structure to see if it is accessible... return one of the good edges.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public Point? FindDumpingAreaOffSite(Edge edge, Point from)
        {
            // don't use discomfort map here.
           SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot]; // UWGame.SimSide.Instance.map.GetMovementMap(ProtectionLevel.Exposed, ThreatCategory.Human, EntityApproach.Normal);

           Point left = Common.AddPoints(Parent.TopLeftMapPosition.Value, new Point(-1, Parent.EntityType.StructureType.HeightInTiles / 2));
            bool leftIsGood = The.Map.TileIsAccessible(map, from, left);

            Point bottom = Common.AddPoints(Parent.TopLeftMapPosition.Value, new Point(Parent.EntityType.StructureType.WidthInTiles / 2, Parent.EntityType.StructureType.HeightInTiles));
            bool bottomIsGood = The.Map.TileIsAccessible(map, from, bottom);

            Point right = Common.AddPoints(Parent.TopLeftMapPosition.Value, new Point(Parent.EntityType.StructureType.WidthInTiles, Parent.EntityType.StructureType.HeightInTiles / 2));
            bool rightIsGood = The.Map.TileIsAccessible(map, from, right);

            switch (edge)
            {
                case Edge.Left:
                    if (leftIsGood)
                    {
                        return left;
                    }
                    break;
                case Edge.Bottom:
                    if (bottomIsGood)
                    {
                        return bottom;
                    }
                    break;
                case Edge.Right:
                    if (rightIsGood)
                    {
                        return right;
                    }
                    break;
            }
            if (leftIsGood)
            {
                return left;
            }
            else if (rightIsGood)
            {
                return right;
            }
            else if (bottomIsGood)
            {
                return bottom;
            }
            // what do we do now...
            return null;
        }

       
        

        /*
        public bool IsPointPlacementValid(Vector3 location)
        {
            // test for blocked subtiles
            Point subtilePos = MapManager.WorldPosToSubtile(location);
            UWGame.SimSide.Maps.MapManager.TerrainValue[][] terrainCosts = The.Map.TerrainCosts[TerrainType.TransportType.Foot];

            if (terrainCosts[subtilePos.X][subtilePos.Y] == 0)
            {
                return false;
            }

            return true;
        }
        */

        /// <summary>
        /// Return a bool if the value is not blocked or if the value is not reserved
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNotBlockedOrReserved(MapManager.SubtileValue value)
        {
            return !((value == MapManager.SubtileValue.Blocked) || value.HasFlag(MapManager.SubtileValue.Reserved));
        }

        public static bool CanBuildOnSubtile(bool structureIsInShapes, bool structureIsInPad, MapManager.SubtileValue mapValue)
        {
            if (structureIsInShapes)
            {
                if (MapManager.IsBlocked(mapValue) 
                    || mapValue.HasFlag(MapManager.SubtileValue.Pad) 
                    || mapValue.HasFlag(MapManager.SubtileValue.Reserved))
                {
                    return false;
                }
            }
            else if (structureIsInPad)
            {
                if (MapManager.IsBlocked(mapValue)
                    || mapValue.HasFlag(MapManager.SubtileValue.Reserved)) // reserved could mean pad or blocked!
                {
                    return false;
                }
            }
            

            return true;

           // return !(mapValue == MapManager.SubtileValue.Blocked || mapValue.HasFlag(MapManager.SubtileValue.Pad) || mapValue.HasFlag(MapManager.SubtileValue.Reserved));
        }

        public bool IsPlacementValid(Vector3 location)
        {
            //tests for placed structures also
            // TODO: set collidable efter build finished

            bool isValid = true;
            SubtileLayers terrainCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
        
            if (Parent.GeometryLayout != null)
            {               
                GeometryLayoutType geoType = Parent.CurrentSimState.GeometryLayoutType; // EntityType.GeometryLayoutType;
                float padRadiusSquared = Parent.GeometryLayout.GetPadRadiusSquared();

                Parent.GeometryLayout.IterateSubtiles(worldPos =>
                    {
                        if (The.Map.WorldLocationIsOnMap(worldPos))
                        {
                            // examine the local subtile:
                            Point subtilePos = MapManager.WorldPosToSubtile(worldPos);
                            MapManager.SubtileValue value = terrainCosts.GetValue(subtilePos);

                            bool isWithinBounds = Parent.GeometryLayout.IsWithinShapes(worldPos, false);
                            bool isWithinPad = Parent.GeometryLayout.IsWithinPad(geoType, padRadiusSquared, worldPos);       

                            if (!CanBuildOnSubtile(isWithinBounds, isWithinPad, value))
                            {
                                isValid = false;
                                //return false;
                            }
                        }
                        else
                        {
                            isValid = false;
                        }
                    });
            }

            return isValid;
        }

        public bool IsPlacementValid(Point pos, Common.Direction? dir = null)
        {
           /* StructureType structureType = Parent.EntityType.StructureType;

            if (!The.Map.TileRectangleIsInsideMap(pos.X, pos.Y, structureType.WidthInTiles, structureType.HeightInTiles))
            {
                return false;
            }


            // test for blocked subtiles (remove this when they can fell trees):
            Point subtilePos = MapManager.TileEdgeToSubtile(pos);
            SubtileLayers terrainCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
            for (int x = 0; x < structureType.WidthInTiles * 3; x++)
            {
                for (int y = 0; y < structureType.HeightInTiles * 3; y++)
                {
                    MapManager.SubtileValue value = terrainCosts.GetValue(subtilePos.X + x, subtilePos.Y + y);
                       
                    if (!CanBuildOnSubtile(value))
                    {
                        return false;
                    }
                }
            }


            // from Road.cs:
            if (Parent.EntityType.StructureType.IsRoad)
            {
                // TODO: upgrade of roads is allowed
                if (!(The.Map.TileMap[pos.X][pos.Y].Roads == null ||
                    The.Map.TileMap[pos.X][pos.Y].Roads[(int)dir] == null))
                {
                    return false;
                }
            }*/

            return true;
        }

        /*   public override string ToString()
           {
               return string.Format("{0} {1}", StructureType.Name, MapPosition);
           }*/

       /* public bool PrepareAndStartBuildingJob(Owner ownerOfBuilding, Priority priority, Vector3 location)
        {
            return PrepareAndStartBuildingJob(ownerOfBuilding, priority, location, null, null);

        }*/

        /*
        public bool PrepareAndStartBuildingJob(Owner ownerOfBuilding, Priority priority, Point position, Common.Direction? dir = null)
        {
            return PrepareAndStartBuildingJob(ownerOfBuilding, priority, null, position, dir);
        }*/

        /// <summary>
        /// Owner is optional (critters)
        /// </summary>
        /// <param name="structureType"></param>
        /// <param name="entityGroup"></param>
        /// <param name="ownerOfNewStructure"></param>
        /// <param name="priority"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static bool PrepareAndStartBuildingJob(string structureType, EntityGroup entityGroup, IOwner ownerOfNewStructure, Vector3 location)
        {
            EntityType type = GameData.Instance.AllEntityTypes[structureType];
            Entity entity = new Entity(type, true);
            entity.Structure.State = StructureStates.PlacedButNotStarted;
            entity.NonLivingEntity.Progress = 0f;

            entity.Initialize(The.Sim.PlaySite, entityGroup.GetAllegiance()); // The.Sim.PlaySite.PlayerAllegiance);
            entity.InitializeModelAndOnScreenFunctionality();

            entity.SetPosition(location);

            return entity.Structure.PrepareAndStartBuildingJob(entityGroup, ownerOfNewStructure, location);
        }

        /// <summary>
        /// this may theoretically fail if there's no room for clearance piles or materials...
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dir"></param>
        /// <param name="buildingJobs"></param>
        /// <param name="newOwnerOfBuilding"></param>
        /// <returns></returns>
        public bool PrepareAndStartBuildingJob(EntityGroup entityGroup, IOwner newOwnerOfBuilding, /* Priority priority,*/ Vector3? location)
        {           
            Parent.NonLivingEntity.Progress = 0f;

            // this is needed for the new hauling jobs:
           // ChangeOwnership(ownerOfBuilding);
                       
            // this may fail if there's not room for clearance piles or materials:
            if (StartBuildingJob(entityGroup)) //, priority))
            {                              
                Parent.PlaceEntityOnPlaySite(location.Value, null, Entity.StructureState.Ordered, new Entity.SetOwnerInfo(newOwnerOfBuilding));
                  
                Parent.Renderable.SetOverlayRendering(true);

                return true;
            }
            else
            {   
                // this structure will not be built... remove it from the ownership list again:

                IOwner parentOwner;

                if (LookUpOwners.ResolveEntityOwner(Parent, out parentOwner)
                    && parentOwner != null)
                {
                    parentOwner.OwnedEntities.DeleteEntity(Parent);

                   /* if (parentOwner.Structures[Parent.EntityType].Contains(Parent.EntityID))
                    {
                        parentOwner.Structures[Parent.EntityType].Remove(Parent.EntityID);
                    }*/
                }
                else
                {
                    newOwnerOfBuilding.OwnedEntities.DeleteEntity(Parent);

                    /*
                    if (ownerOfBuilding.Structures[Parent.EntityType].Contains(Parent.EntityID))
                    {
                        ownerOfBuilding.Structures[Parent.EntityType].Remove(Parent.EntityID);
                    }*/
                }

            }

            return false;
        }

       /*

        public void TurnOnLights(int noToTurnOn, LightSource[] list)
        {
            int index = The.Sim.GameplayRandomGenerator.Next(0, list.Length, "Structure"); //pick a random item
            int numberTurnedOn = 0;
            int seenTurnedOn = 0;

            while (numberTurnedOn < noToTurnOn && seenTurnedOn < list.Length)
            {
                while (list[index].LightIsOn == true)
                {
                    seenTurnedOn++;
                    index++;
                    index = index % list.Length;
                }

                list[index].LightIsOn = true;
                numberTurnedOn++;
            }

        }*/
    }


}
