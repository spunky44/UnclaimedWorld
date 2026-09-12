using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using GameStateManagement;
using UWGame.SimSide.AI.Goals;
using UWGame.Control;
using UWGame.SimSide.AI;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Jobs
{
    public class BuildingJob: ISnapshot 
    {
       
        public ProcessJob ProcessJob;
        private JobID snapshotJob;
       
        /// <summary>
        /// replace with PlaceFinder???
        /// </summary>
        private Dictionary<Entities.EntityID, Point> workersSubtilePositions = new Dictionary<Entities.EntityID, Point>();
       // private Dictionary<Entities.Entity, Point> workersSubtilePositions = new Dictionary<global::UWGame.SimSide.Entities.Entity, Point>();


        public BuildingJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        public BuildingJob(ProcessJob processJob /*Entity buildingToConstruct*/) //, ProcessType processType, List<Job> addToList)
          //  : base(addToList)
        {
            this.ProcessJob = processJob;

         
        }



    

        public bool GetWorkLocation(Entity entity, out Vector3 workLocation, out IKnownEntityData structureToConstruct) 
        {
            workLocation = - Vector3.One;
            if (!ProcessJob.GetStructureOutputData(out structureToConstruct)) // only needs Access point for now..
            {
                return false;
            }

            // TODO: use a GatheringPlace to distribute workers
            workLocation = structureToConstruct.AccessPoint.Value; //Location;
            workLocation += new Vector3(The.Sim.GameplayRandomGenerator.Next(-MapManager.tileSizeOver4, MapManager.tileSizeOver4, "BuildingJob"),
                                            The.Sim.GameplayRandomGenerator.Next(-MapManager.tileSizeOver4, MapManager.tileSizeOver4, "BuildingJob"), 0f);          

            return true;
        
           /* if (structureToConstruct.EntityType.TileLayoutType != null) //.TileLayout != null) 
            {
                if (workersSubtilePositions.Count > 0)
                {
                    List<EntityID> entitiesToDelete = new List<EntityID>();

                    // clean up the list for any workers that might have left:
                    foreach (var kvp in workersSubtilePositions)
                    {
                        if (!ProcessJob.TakenBy.Contains(kvp.Key))
                        {
                            entitiesToDelete.Add(kvp.Key);
                        }
                    }

                    foreach (EntityID entityToDelete in entitiesToDelete)
                    {
                        workersSubtilePositions.Remove(entityToDelete);
                    }
                }
                 
                int subTileMapWidth = structureToConstruct.EntityType.StructureType.WidthInTiles * 3;
                int subTileMapHeight = structureToConstruct.EntityType.StructureType.HeightInTiles * 3;
                
                byte[][] subTileMap = null; // new byte[subTileMapWidth, subTileMapHeight];
                Common.InitJaggedArray(ref subTileMap, subTileMapWidth, subTileMapHeight);

                bool[][] isBlockedMap = structureToConstruct.EntityType.TileLayoutType.GetIsBlockedSubtileMap(structureToConstruct.FlipHorizontally);

                DrawInfluenceMapWorkWorkerPosition(subTileMap);


                Point bestSubtilePos;
                if (InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subTileMap, isBlockedMap, out bestSubtilePos) == -1)
                {
                    // no work location found within the structure area. Look at the border around it:
                    subTileMapWidth = (structureToConstruct.EntityType.StructureType.WidthInTiles + 2) * 3;
                    subTileMapHeight = (structureToConstruct.EntityType.StructureType.HeightInTiles + 2) * 3;
                    subTileMap = null; //new byte[subTileMapWidth][subTileMapHeight];
                    Common.InitJaggedArray(ref subTileMap, subTileMapWidth, subTileMapHeight);
                    isBlockedMap = Structure.CreateOneTileBorderIsBlockedMapForConstructionJobs(structureToConstruct);
            
                    DrawInfluenceMapWorkWorkerPosition(subTileMap);
                    if (InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subTileMap, isBlockedMap, out bestSubtilePos) == -1)
                    {
                       // tile = new Point(-1, -1);
                       // tileCenterOffset = Vector2.Zero;
                        workLocation = new Vector3(-1);
                        return false;
                    }

                    MapManager.SubtileAndTilePosToWorldPos(bestSubtilePos, structureToConstruct.TopLeftMapPosition, out workLocation); // out tile, out tileCenterOffset);
                    // adjust the location because we looked at a border...
                    workLocation += new Vector3(-MapManager.tileSize, -MapManager.tileSize, 0f);
                    //tile = Common.AddPoints(tile, new Point(-1, -1));

                }
                else
                {
                    MapManager.SubtileAndTilePosToWorldPos(bestSubtilePos, structureToConstruct.TopLeftMapPosition, out workLocation); // out tile, out tileCenterOffset);                          
                }                
                 

                workersSubtilePositions.Add(entity.EntityID, bestSubtilePos);

            }
            else
            {*/
                // TODO: use a GatheringPlace to distribute workers
            /*    workLocation = structureToConstruct.AccessPoint; //Location;
                workLocation += new Vector3(The.Sim.GameplayRandomGenerator.Next(-MapManager.tileSizeOver4, MapManager.tileSizeOver4,"BuildingJob"),
                                                The.Sim.GameplayRandomGenerator.Next(-MapManager.tileSizeOver4, MapManager.tileSizeOver4, "BuildingJob"), 0f);            
         //   }

            return true;*/
        }

        private void DrawInfluenceMapWorkWorkerPosition(byte[][] subTileMap)
        {
            // draw a circle emanating from  the center and reaching the edges of the site:
            int width = Common.GetJaggedArrayWidth(subTileMap);
            int height = Common.GetJaggedArrayHeight(subTileMap);

            InfluenceMap.DrawLinearInfluenceCircle(subTileMap, new Point(width / 2, height / 2),
                                40, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, width / 2);

            //add a gradient with noise along bottom edge because we prefer positions that the player can see:
            InfluenceMap.DrawGradientRectangle(subTileMap, new Point(0, 0),
                new Point(width - 1, height - 1), true, 0, 20, 5, InfluenceMap.GradientDirection.TopToBottom);

            if (workersSubtilePositions.Count > 0)
            {
                // draw a negative influence for each worker already on the site to ensure a nice spread:
                foreach (var kvp in workersSubtilePositions)
                {
                    InfluenceMap.DrawLinearInfluenceCircle(subTileMap, kvp.Value,
                                -20, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, 2);
                }
            }
        }



        public void DestroyUnstartedStructure()
        {
           
          /*  bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ProcessJob.ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);
            */
            // don't limit by knowledge. the process has not yet been started
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProcessJob.ProductionProcess);

            if (process != null)
            {
                Entity outputEntity;

                foreach (var item in process.OutputEntities)
                {
                    outputEntity = Entity.FindByID(item);

                    if (outputEntity != null)
                    {
                        if (outputEntity.Structure != null)
                        {
                            if (!outputEntity.Structure.ConstructionHasStarted())
                            {                               
                                outputEntity.Destroy();

                                if (The.InGameUI.SelectedEntity == item)
                                {
                                    The.InGameUI.SelectEntity(null); // why..?
                                }
                            }
                        }
                    }
                }

            }

            //ProcessJob.GetOutputEntity()
                     

            /*if (pJob != null && pJob.BuildingJob != null)
            {
                if (pJob.BuildingJob.ProcessJob.OutputEntities[0].Structure != null)
                {
                    if (pJob.BuildingJob.ProcessJob.OutputEntities[0].Structure.ConstructionHasStarted() == false)
                    {
                        pJob.BuildingJob.ProcessJob.OutputEntities[0].Structure.State = global::UWGame.SimSide.Buildings.States.UnderConstruction;
                        pJob.BuildingJob.ProcessJob.OutputEntities[0].Destroy();
                        The.InGameUI.SelectedEntity = null;
                    }
                }
            }*/
        }

        private static float GetTotalBulk(List<Entity> items)
        {
            float total = 0f;
            foreach (var item in items)
            {
                total += item.Bulk;
            }

            return total;
        }


        

        public void Abandon(Entity entity)
        {
            if (workersSubtilePositions.ContainsKey(entity.EntityID))
            {
                workersSubtilePositions.Remove(entity.EntityID);
            }

           // base.Abandon(entity);
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder("Build ");

          /*  IKnownEntityData structureEntity;
            if (ProcessJob.GetStructureOutputData(out structureEntity))
            {
                b.Append(structureEntity.EntityType.Name);
                b.Append(" At: ");
                b.Append(structureEntity.MapPosition.ToString());
            }*/

            return b.ToString();
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.snapshotJob = (JobID)sn.SnapshotID<Job, JobID>(ProcessJob);
            this.workersSubtilePositions = (Dictionary<EntityID, Point>)sn.DoDictionary(workersSubtilePositions);


            sn.Ignore(ProcessJob);

            return this;
        }

        Snapshotter.Version version;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            ProcessJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);

        }

        public bool IsSnapshotted
        {
            get; set;
        }

        #endregion
    }
}
