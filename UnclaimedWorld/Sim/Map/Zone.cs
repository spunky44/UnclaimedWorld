using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Entities;
using UWGame.Client.Interface.MapGUI;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Resources;
using WindowSystem;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;
using System.Diagnostics;

namespace UWGame.SimSide.Maps
{
   
    public enum ZoneID : long
    {
        Invalid = long.MaxValue,
        Max = Invalid,
        First = 1
    }


    /// <summary>
    /// a zone is a player or AI defined map area for some preferred activity for the AI
    /// it is Sim side!!!
    /// </summary>
    [DebuggerDisplay("{MapArea}")]
    public class Zone: ILookUp<Zone, ZoneID>, ISnapshot
    {       
        public string Name;

        public MapArea MapArea;

        /// <summary>
        /// used when searching - they are in Zone instead of MapArea because zones are registered on map tiles, map areas are not.
        /// </summary>
        public HashSet<TerrainTile> EdgeTiles = new HashSet<TerrainTile>();
        List<TerrainTileID> snapshotEdgeTiles;

        /// <summary>
        /// client-only event
        /// </summary>
        public event EventHandler ZoneOrdersChanged;

        

        private ScoutingJob forageJob;
        JobID? snapshotForageJob;

        private ScoutingJob scoutingJob;
        JobID? snapshotScoutingJob;

     
        private PatrolJob patrolJob;
        JobID? snapshotPatrolJob;

        private AttackAreaJob attackAreaJob;
        JobID? snapshotAttackAreaJob;


        public PatrolJob PatrolJob
        {
            get { return patrolJob; }
            set
            {
                if (patrolJob != value)
                {
                    patrolJob = value;
                    RemoveZoneOrFireOrdersChangedEvent();
                }
            }
        }



        public ScoutingJob ScoutingJob
        {
            get { return scoutingJob; }
            set
            {
                if (scoutingJob != value)
                {
                    scoutingJob = value;
                    RemoveZoneOrFireOrdersChangedEvent();
                }
            }
        }

        public ScoutingJob ExamineJob
        {
            get { return forageJob; }
            set
            {
                if (forageJob != value)
                {
                    forageJob = value;
                    RemoveZoneOrFireOrdersChangedEvent();
                }
            }
        }

      
        public AttackAreaJob AttackAreaJob
        {
            get { return attackAreaJob; }
            set
            {
                if (attackAreaJob != value)
                {
                    attackAreaJob = value;
                    RemoveZoneOrFireOrdersChangedEvent();
                }
            }
        }

        public Dictionary<ResourceType, List<ProcessJob>> HarvestJobs = new Dictionary<ResourceType, List<ProcessJob>>();
        Dictionary<ResourceType, List<JobID>> snapshotHarvestJobs;

        /// <summary>
        /// signals to the JobManager if it can create gather jobs here. Also, the zone may not be auto-destroyed if it contains these
        /// </summary>
        public HashSet<ResourceType> AllowStandingOrderHarvest = new HashSet<ResourceType>();


       

        public ZoneHunt ZoneHunt;


        private Stockpile stockpile = null;
        public Stockpile Stockpile // new Stockpile();
        {
            get
            {              
                return stockpile;
            }
            set
            {
                stockpile = value;
                RemoveZoneOrFireOrdersChangedEvent();
            }
        }
               



        /// <summary>
        /// is null for the selection zone...
        /// </summary>
        private EntityGroupID? ownerID;

        public EntityGroupID? Owner
        {
            get
            {
                return ownerID;
            }
        }

        /// <summary>
        /// will copy the mapArea selection of tiles
        /// </summary>
        /// <param name="mapArea"></param>
        public Zone(EntityGroup zoneOwner, MapArea mapArea)
        {
            AddToLookup();

           
            if (zoneOwner != null)
            {
                this.ownerID = zoneOwner.ID;
                zoneOwner.Zones.Add(this);
            }


            this.MapArea = new MapArea(mapArea, this);

            ZoneHunt = new Maps.ZoneHunt();

            Allegiances.Allegiance allegiance = zoneOwner.GetAllegiance();

            MapArea.IterateArea(tile =>
                {
                    tile.AddZone(allegiance, this);
                });

            ComputeEdges();
        }

        public Zone()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }

        #region ILookup

        private ZoneID id = ZoneID.Invalid;
        static ZoneID IDCounter = ZoneID.First;


        public ZoneID ID
        {
            get
            {
                return id;
            }

            private set
            {
                /*if (!producing && !Snapshotter.IsSnapshotting)
                    throw new Exception("who's setting an entity's ID outside of EntityFactory?");
                */
                id = value;
            }
        }

        public ZoneID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ZoneID.Max)
            {
                throw new Exception("Astounding, ZoneID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public ZoneID SnapshotID(Snapshotter sn, ZoneID id)
        {
            return (ZoneID)sn.DoEnum(id);
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
            if (ID != ZoneID.Invalid)
                LookUp<Zone, ZoneID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = ZoneID.Invalid;
        }

        public void RemoveIDEntry()
        {          
            LookUp<Zone, ZoneID>.Remove(this);
        }

        void UWGame.SimSide.Snapshots.ILookUp<Zone, ZoneID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = ZoneID.First;
        }

        void ILookUp<Zone, ZoneID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<Zone, ZoneID>.Create();
        }

        //======ILookup===============

        #endregion

        public string GetDisplayName()
        {
            return Name ?? ("#" + ID.ToString());
        }

        public void ComputeEdges()
        {
           /* if (EdgeTiles == null)
                EdgeTiles = new HashSet<TerrainTile>();
            */

            EdgeTiles.Clear();

            if (MapArea.BoundingRectangle == null) // zero tiles...
                return; 

            // scan left to right:

            bool isInZone;
            TerrainTile[] currentTileRow;
            TerrainTile currentTile, previousTile;

            bool isLastTile;

            EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(ownerID);
            if (owner == null)
                return;

            Allegiances.Allegiance allegiance = owner.GetAllegiance();

            for (int y = MapArea.BoundingRectangle.Value.Top; y < MapArea.BoundingRectangle.Value.Bottom; y++)
            {
                isInZone = false;
                previousTile = null;
               // currentTileRow = 
                for (int x = MapArea.BoundingRectangle.Value.Left; x < MapArea.BoundingRectangle.Value.Right; x++)
                {
                    isLastTile = x == MapArea.BoundingRectangle.Value.Right - 1;
                    currentTile = The.Map.TileMap[x][y];

                    HandleTile(ref isInZone, allegiance, currentTile, previousTile, isLastTile);

                    previousTile = currentTile;
                }
            }


            for (int x = MapArea.BoundingRectangle.Value.Left; x < MapArea.BoundingRectangle.Value.Right; x++)
            {
                isInZone = false;
                previousTile = null;

                for (int y = MapArea.BoundingRectangle.Value.Top; y < MapArea.BoundingRectangle.Value.Bottom; y++)
                {
                    isLastTile = y == MapArea.BoundingRectangle.Value.Bottom - 1;
                    currentTile = The.Map.TileMap[x][y];

                    HandleTile(ref isInZone, allegiance, currentTile, previousTile, isLastTile);

                    previousTile = currentTile;
                }
            }
        }

        private void HandleTile(ref bool isInZone, Allegiances.Allegiance allegiance, TerrainTile currentTile, TerrainTile previousTile, bool isLastTile) // int x, int y)
        {
            //TerrainTile currentTile = The.Map.TileMap[x][y];

            if (!isInZone)
            {
                if (currentTile.IsInZone(allegiance, this)) 
                {
                    isInZone = true;
                  
                    EdgeTiles.Add(currentTile);
                }
            }
            else
            {
                if (!isLastTile)
                {
                    // if this tile is not in the zone, but the previous one was, add the previous one
                    if (!currentTile.IsInZone(allegiance, this)) 
                    {
                        isInZone = false;

                        EdgeTiles.Add(previousTile);
                    }
                }
                else 
                { // we have reached the edge - add this tile:
                    if (currentTile.IsInZone(allegiance, this)) 
                    {                        
                        EdgeTiles.Add(currentTile);
                    }

                }
            }
           
        }

        /// <summary>
        /// if there are no orders we remove the zone, otherwise - notify client ui that the data has changed
        /// </summary>
        public void RemoveZoneOrFireOrdersChangedEvent()
        {
            //if (DestroyZoneIfNoOrders())

            if (!HasOrders())
            {
                Destroy();
            }
            else
            {
                if (ZoneOrdersChanged != null)
                {
                    // for the client:
                    ZoneOrdersChanged(this, null);
                }
            }
        }


        public RegionMap.Result GetClosestSafeEdgeTile(Job job, Vector3 aSourceLocation, RegionMap regionMap, ThreatStance threatStance, Entity entity, out float? distanceToClosestTile, out TerrainTile closestTile, EntityGroup owner = null)
        {
            float? currentDistance = null;
            closestTile = null;
            distanceToClosestTile = null;
            RegionMap.Result currentResult;
            RegionMap.Result finalCalculateDistanceResult = RegionMap.Result.NoAccess;

            foreach (var tile in EdgeTiles)
            {

                currentResult = CalculateDistanceToTile(aSourceLocation, tile, regionMap, entity, ref currentDistance);

                if (currentResult == RegionMap.Result.Wait)
                {
                    return RegionMap.Result.Wait;
                }
                else if (currentResult == RegionMap.Result.NoAccess)
                {
                    continue;
                }
                else
                {
                    // distance OK

                    if (!GoalEvaluator.WorkSiteIsSafe(entity, MapManager.TileToWorldPos(tile) /* currentJobLocation.Value*/, threatStance))
                    {
                        continue; // not safe...
                    }
                }

                /*  if (currentDistance.HasValue == false)
                  {
                      if (distanceToClosestTile.HasValue == false && currentResult == RegionMap.Result.Wait)
                      {
                        
                          return RegionMap.Result.Wait;
                      }
                      else
                      {
                        
                          continue; // no path to tile 
                      }
                  }*/

                if (distanceToClosestTile.HasValue == false)
                {
                    distanceToClosestTile = currentDistance.Value;
                    closestTile = tile;
                    finalCalculateDistanceResult = currentResult;
                }
                else
                {
                    if (currentDistance.Value < distanceToClosestTile.Value)
                    {
                        distanceToClosestTile = currentDistance.Value;
                        closestTile = tile;
                        finalCalculateDistanceResult = currentResult;
                    }
                }
            }

            if (owner != null)
            {
                if (finalCalculateDistanceResult == RegionMap.Result.OK)
                {
                    The.Client.SetJobInaccessible(job, owner.Parent, false); // job.SetIsInaccessible(owner.InternalOwner, false);
                }

                if (finalCalculateDistanceResult == RegionMap.Result.NoAccess)
                {
                    The.Client.SetJobInaccessible(job, owner.Parent, true); //job.SetIsInaccessible(owner.InternalOwner, true);

                    bool blockedByThreat = false;
                    float distance = 0;
                    foreach (var tile in EdgeTiles)
                    {
                        //checkevery tile

                        Point sourceSubtile = MapManager.WorldPosToSubtile(aSourceLocation);

                        Point destinationSubtile = MapManager.TileToCenterSubtile(new Point(tile.X, tile.Y));

                        RegionMap.Result result2 = The.Map.FootTerrainRegionMap.GetDistance(entity, sourceSubtile, destinationSubtile, ref distance, false);
                        if (result2 == RegionMap.Result.OK)
                        {
                            The.Client.SetJobBlockedByThreat(job, owner.Parent, true);
                            //job.SetBlockedByThreat(owner.InternalOwner, true);
                            blockedByThreat = true;
                            break;
                        }

                    }

                    if (blockedByThreat == false)
                    {
                        The.Client.SetJobBlockedByThreat(job, owner.Parent, false);
                        //job.SetBlockedByThreat(owner.InternalOwner, false);

                    }
                }
            }
            return finalCalculateDistanceResult;
        }

        private static RegionMap.Result CalculateDistanceToTile(Vector3 sourceLocation, TerrainTile destinationTile, RegionMap regionMap, Entity entity, ref float? distanceResult)
        {
            float distance = 0.0f;

            Point sourceSubtile = MapManager.WorldPosToSubtile(sourceLocation);

            Point destinationSubtile = MapManager.TileToCenterSubtile(new Point(destinationTile.X, destinationTile.Y));


            if (regionMap.ID == (CyclableID)170 && 
                entity != null && entity.ID == (EntityID)4582
                && sourceSubtile.X == 114 && sourceSubtile.Y == 44 //fromRegion == 523
                   && The.Sim.TotalUnPausedGameTimeInSeconds > 35) //35.610) // 35) //35.4259783 // crash: 35.6104778)
            {

            }


            RegionMap.Result getDistanceResult = regionMap.GetDistance(entity, sourceSubtile, destinationSubtile, ref distance);
            if (getDistanceResult != RegionMap.Result.OK)
            {
                return getDistanceResult;
            }

            distanceResult = distance;
            return RegionMap.Result.OK;
        }

        public bool HasOrders()
        {
            if (PatrolJob == null && ScoutingJob == null && ExamineJob == null && AttackAreaJob == null
                && Stockpile == null
                && !HasHarvestJobs()
                && !ZoneHunt.HasFindPreyJobs()                
                && !ZoneHunt.HasHuntOrders()
                && AllowStandingOrderHarvest.Count == 0)
            {
                return false;
            }

            return true;
        }

    /*    public bool HasOrders()
        {
            if (!Scout && !Forage && !FindPrey
                && Stockpile == null
                && !HasHarvestJobs())
            {
                return false;
            }

            return true;
        }*/

        public bool HasHarvestJobs()
        {
            if (HarvestJobs != null)
            {
                foreach (var resource in HarvestJobs)
                {
                    if (resource.Value.Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

      /*  public bool HasFindPreyJobs()
        {
            if (ZoneHunt != null)
            {
                return ZoneHunt.HasFindPreyJobs();
            }

            return false;
        }*/

        public float GetTotalBulkOfItems()
        {
            float total = 0f;
            
            MapArea.IterateArea(tile => total += tile.GetTotalBulkOfItems());

            return total;
        }

        public float GetBulkCapacity()
        {
            return MapArea.Count * GameData.Instance.Constants.BulkCapacityForSingleTile;
        }

        public void Destroy()
        {
            EntityGroup ownerContent = LookUp<EntityGroup, EntityGroupID>.FindByID(ownerID);
            if (ownerContent != null)
            {                
                ownerContent.Zones.Remove(this);
            }

            if (HarvestJobs != null)
            {
                foreach (var item in HarvestJobs)
                {
                    for (int i = item.Value.Count - 1; i >= 0; i--)
                    {
                        item.Value[i].Destroy(true);
                    }
                }
                
            }

            if (scoutingJob != null)
            {
                scoutingJob.Destroy(true);
            }
            if (forageJob != null)
            {
                forageJob.Destroy(true);
            }
            
            if (patrolJob != null)
            {
                patrolJob.Destroy(true);
            }

            if (attackAreaJob != null)
            {
                attackAreaJob.Destroy(true);
            }

            ZoneHunt.Destroy();

            EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(ownerID);
            Allegiances.Allegiance allegiance = null;
            if (owner != null)
            {
                allegiance = owner.GetAllegiance();
            }
            
            MapArea.IterateArea(tile =>
            {
                if (allegiance != null)
                {
                    tile.RemoveZone(allegiance, this);
                }
                else
                {
                    tile.RemoveZone(this);
                }
            });

            if (The.InGameUI.SelectedZone == this)
            {
                The.InGameUI.SelectedZone = null;
            }
            
        }

        public void AddHarvestJob(ProcessJob job)
        {
            job.HarvestJob.Zone = this;

            List<ProcessJob> list;
            if (!HarvestJobs.TryGetValue(job.HarvestJob.ResourceType, out list))
            {
                list = new List<ProcessJob>();
                HarvestJobs.Add(job.HarvestJob.ResourceType, list);
            }

            int noOfJobsBefore = list.Count;
            list.Add(job);

            if (noOfJobsBefore == 0)
            {
                RemoveZoneOrFireOrdersChangedEvent();
            }
        }

        public void RemoveHarvestJob(ProcessJob job)
        {
            if (HarvestJobs != null)
            {
                List<ProcessJob> list;
                if (HarvestJobs.TryGetValue(job.HarvestJob.ResourceType, out list)) // zone.HarvestJobs != null && zone.HarvestJobs.Contains(ProcessJob))
                {
                    list.Remove(job);

                    if (list.Count == 0)
                    {
                        RemoveZoneOrFireOrdersChangedEvent();
                    }
                }
            }
        }


        public void RemoveFindPreyJob(FindPreyJob job)
        {
            //ZoneHunt.FindPreyJobs.Remove(job);

            
            if (ZoneHunt.FindPreyJobs.Remove(job))
            {                
                // only removes the zone if no orders or jobs exist:
                RemoveZoneOrFireOrdersChangedEvent();
            }
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
            this.id = SnapshotID(sn, id);
            IDCounter = (ZoneID)sn.DoEnum(IDCounter);


            this.Name = sn.DoString(Name);
            this.MapArea = (MapArea)sn.DoISnapshot(MapArea);
            this.ownerID = sn.DoEnumNullable(ownerID);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotEdgeTiles = EdgeTiles.Select(k => k.ID).ToList();

                snapshotHarvestJobs = new Dictionary<ResourceType, List<JobID>>();
                foreach (var item in HarvestJobs)
                {
                    snapshotHarvestJobs.Add(item.Key, item.Value.Select(j => j.ID).ToList());
                }
            }

            this.snapshotEdgeTiles = sn.DoList(snapshotEdgeTiles);           
            this.snapshotPatrolJob = sn.SnapshotID<Job, JobID>(patrolJob);
            this.snapshotAttackAreaJob = sn.SnapshotID<Job, JobID>(attackAreaJob);
            this.snapshotScoutingJob = sn.SnapshotID<Job, JobID>(scoutingJob);
            this.snapshotForageJob = sn.SnapshotID<Job, JobID>(forageJob);
            this.snapshotHarvestJobs = sn.DoMultiMap(snapshotHarvestJobs);
            this.stockpile = (Stockpile)sn.DoISnapshot(stockpile);
            this.AllowStandingOrderHarvest = sn.DoHashSet(AllowStandingOrderHarvest);

            this.ZoneHunt = (ZoneHunt)sn.DoISnapshot(ZoneHunt);

            sn.Ignore(HarvestJobs);
            sn.Ignore(EdgeTiles);
            sn.Ignore(scoutingJob);
            sn.Ignore(forageJob);
            sn.Ignore(patrolJob);
            sn.Ignore(attackAreaJob);
            sn.Ignore(ZoneOrdersChanged);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            foreach (var item in snapshotEdgeTiles)
            {
                EdgeTiles.Add(LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(item));
            }

            foreach (var item in snapshotHarvestJobs)
            {
                HarvestJobs.Add(item.Key, item.Value.Select(j => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
            }


            if (snapshotScoutingJob != null)
            {
                scoutingJob = (ScoutingJob)LookUp<Job, JobID>.FindByID(snapshotScoutingJob.Value);
            }

           
            if (snapshotPatrolJob != null)
            {
                patrolJob = (PatrolJob)LookUp<Job, JobID>.FindByID(snapshotPatrolJob.Value);
            }

            if (snapshotAttackAreaJob != null)
            {
                attackAreaJob = (AttackAreaJob)LookUp<Job, JobID>.FindByID(snapshotAttackAreaJob.Value);
            }

            if (snapshotForageJob != null)
            {
                forageJob = (ScoutingJob)LookUp<Job, JobID>.FindByID(snapshotForageJob.Value);
            }

            if (ZoneHunt != null)
            {
                ZoneHunt.LoadPostProcess(sn);
            }

            MapArea.LoadPostProcess(sn);
        }

        #endregion


    }
}
