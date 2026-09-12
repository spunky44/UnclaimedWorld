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

namespace UWGame.SimSide.Jobs
{
    public class SalvageJob : ISnapshot
    {
       
        public ProcessJob ProcessJob;
        private JobID snapshotJob;
       
        private Dictionary<Entities.EntityID, Point> workersSubtilePositions = new Dictionary<Entities.EntityID, Point>();

        public SalvageJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }


        public SalvageJob(ProcessJob processJob) 
        {
            this.ProcessJob = processJob;

        }


        
        /// <summary>
        /// TODO: see why this copy of Construction Job code is necessary...
        /// TODO: scrap this copied code and use GatehringPlace instead
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="workLocation"></param>
        /// <returns></returns>
     /*   public bool GetWorkLocation(Entity entity, out Vector3 workLocation) // out Point tile, out Vector2 tileCenterOffset)
        {
            IKnownEntityData structureToConstruct;
            workLocation = -Vector3.One;
            if (!ProcessJob.GetFirstOutputEntityData(out structureToConstruct))
            {
                return false;
            }

            if (structureToConstruct.TileLayout != null) //!BuildingToConstruct.EntityType.StructureType.IsEdgeFeature)
            {
                if (workersSubtilePositions.Count > 0)
                {
                    List<Entity> entitiesToDelete = new List<Entity>();

                    // clean up the list for any workers that might have left:
                    foreach (KeyValuePair<Entity, Point> kvp in workersSubtilePositions)
                    {
                        if (!ProcessJob.TakenBy.Contains(kvp.Key))
                        {
                            entitiesToDelete.Add(kvp.Key);
                        }
                    }

                    foreach (Entity entityToDelete in entitiesToDelete)
                    {
                        workersSubtilePositions.Remove(entityToDelete);
                    }
                }
              //  Structure StructureToConstruct = ProcessJob.OutputEntities[0].Structure;
                
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
                 

                workersSubtilePositions.Add(entity, bestSubtilePos);

            }
            else
            {
                workLocation = structureToConstruct.Location;
                workLocation += new Vector3(The.Sim.GameplayRandomGenerator.Next(-MapManager.tileSizeOver4, MapManager.tileSizeOver4,"SalvageJob"),
                                                The.Sim.GameplayRandomGenerator.Next(-MapManager.tileSizeOver4, MapManager.tileSizeOver4, "SalvageJob"), 0f);
             
            }

            return true;
        }*/

        /// <summary>
        /// TODO: replace with GatheringSite
        /// </summary>
        /// <param name="subTileMap"></param>
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
                foreach (KeyValuePair<EntityID, Point> kvp in workersSubtilePositions)
                {
                    InfluenceMap.DrawLinearInfluenceCircle(subTileMap, kvp.Value,
                                -20, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, 2);
                }
            }
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
            StringBuilder b = new StringBuilder("Salvage ");
          /*  IKnownEntityData structureEntity;

            if (ProcessJob.ProcessType.IsConstruction()) // .processty)
            {
                b.Append(

            }

             if (ProcessJob.GetFirstOutputEntityData(out structureEntity) // replace with string: output at location 
                 && structureEntity != null)
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
            get;
            set;
        }

        #endregion
     
    }
}
