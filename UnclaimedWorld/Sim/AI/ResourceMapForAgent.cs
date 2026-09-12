using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Items;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI
{

    /// <summary>
    /// This class is used to compute the influence map for an agent to decide where to go harvesting, by preferring areas with large concentration of resources
    /// One instance is kept in each entity intelligence... only one may be active at a time. 
    /// The class seems to have gotten a bit complex and costly though...
    /// 
    /// Consider using this code for combat positioning...
    /// 
    ///  TODO: integrate with random detection of resources    
    /// </summary>
    public class ResourceMapForAgent : ICyclable, IIDEventSubscriber
    {
        private enum Phase { GetResourceMap, Clear, DrawDistances, DrawOtherAgents, Search }
        private Phase phase = Phase.GetResourceMap;

        /// <summary>
        /// point this to the general influence map used by evaluators...
        /// </summary>
        ushort[][] Values;

        int maxValue;

        //byte[][] Values;

      //  int mapWidth;
        int twoTimesMaxMapOctileDistance;

        private Entity entity;
        EntityID snapshotEntity;

        private HarvestJob harvestJob;
       // private ProcessJob harvestJob;
        JobID? snapshotHarvestJob;


        private byte[][] cropsMap;

        MethodID notifyWhenRegionSearchIsFinishedMethodID;
       
        private ResourceItemID? /* IResourceItem*/ bestResourceItemID;
    
        const int resourcesToDrawPerCycle = 20;

        private Regulator regulator;

        bool isDirty = true;
       
        MethodID crops_FinishedMethodID;
        MethodID listOfCrops_ListItemRemovedMethodID;

        // progress variables
        int cropCounter = 0;
        int rowCounter = 0;

        public bool IsPaused
        {
            get { return isWaiting; }
        }


        public double? UpdateInterval
        {
            get
            {
                return 6d;
            }
        }

        private bool isWaiting = false;

        public double StartedOnTimeInSeconds { get; set; }
        static double totalComputationAllInstancesInSeconds;
        public double TotalComputationAllInstancesInSeconds
        {
            get
            {
                return totalComputationAllInstancesInSeconds;
            }
            set
            {
                totalComputationAllInstancesInSeconds = value;
            }
        }
        public double ComputationTimeSpentInSeconds { get; set; }

        public ResourceMapForAgent()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }
      

        public ResourceMapForAgent(Entity entity)
        {
            AddToLookup();

           
            this.Values = entity.Intelligence.EvaluatorInfluenceMap;
            this.entity = entity;

           
            int maxMapOctileDistance = (int)(Common.DistanceOctile(Point.Zero, new Point(The.Map.mapTileWidth, The.Map.mapTileHeight)));

            twoTimesMaxMapOctileDistance = 2 * maxMapOctileDistance;

            maxValue = UInt16.MaxValue;

            // register callback methods and save their IDs:
            crops_FinishedMethodID = ActionLookup.AddWithNewID(crops_FinishedEvent);
            notifyWhenRegionSearchIsFinishedMethodID = ActionLookup.AddWithNewID(NotifyWhenRegionSearchIsFinished);
            listOfCrops_ListItemRemovedMethodID = ActionLookup<int>.AddWithNewID(listOfCrops_ListItemRemoved);

            CreateRegulators();
        }

        private void CreateRegulators()
        {

            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / UpdateInterval.Value, "ResourceMapForAgent");
        }

        public void Destroy()
        {
            RemoveIDEntry();

            The.Sim.CycleManager.UnRegister(this);            

            ActionLookup.Remove(crops_FinishedMethodID);
            ActionLookup<int>.Remove(listOfCrops_ListItemRemovedMethodID);
        }


        /*  public CropsMapForAgent(Entity entity, HarvestJob harvestJob)
          {            
             this.Values = entity.Intelligence.EvaluatorInfluenceMap;
             this.entity = entity;
             this.harvestJob = harvestJob;

             regulator = new Regulator(updatesPerSecond);

          }*/

        public enum Result { Wait, NoTarget, OK }

        /// <summary>
        /// what about gathering different types at once...?
        /// </summary>
        /// <param name="harvestJob"></param>
        /// <param name="bestResourceItem"></param>
        /// <returns></returns>
        public Result GetBestHarvestLocation(HarvestJob harvestJob, ref IResourceItem bestResourceItem)
        {
            System.Diagnostics.Debug.Assert(entity.ID != EntityID.Invalid, "Invalid entity?!?!"); // added to find a save crash...

           // ObservableList<Crop> listOfCrops;
            ObservableList<ResourceContainer> listOfResources;

            if (!The.Sim.PlaySite.Resources.TryGetValue(harvestJob.ResourceType, out listOfResources))
            {
                // bail out early...
                return Result.NoTarget;
            }
            else if (listOfResources.Count == 0)
            {
                // bail out early...
                return Result.NoTarget;
            }
            else
            {               
                
                if (this.harvestJob == null 
                    || this.harvestJob.ResourceType != harvestJob.ResourceType // make sure that we don't give back a result that was produced with another crop type as input.
                                //|| (this.bestResourceItem != null && this.bestResourceItem .NoOfHarvestableItems == 0) // - and that the currently stored crop result still has harvestable items!
                    || isDirty)
                {
                    if (!The.Sim.CycleManager.IsRegistered(this))
                    {
                        // start the request:
                        // CropsMapForAgent request = new CropsMapForAgent(entity, harvestJob);

                        this.harvestJob = harvestJob;
                        // this.collectionLocation = collectionLocation;

                        bestResourceItem = null;

                        The.Sim.CycleManager.Register(this /*request*/, CycleManager.Priority.Medium);
                    }

                    return Result.Wait;
                }
                else
                {
                    // the result is ready...

                    bestResourceItem = LookUp<IResourceItem, ResourceItemID>.FindByID(this.bestResourceItemID);

                    return Result.OK;
                }
            }

            /*
            this.Values = entity.Intelligence.EvaluatorInfluenceMap;
            this.entity = entity;
            this.harvestJob = harvestJob;*/
        }

        private void listOfCrops_ListItemRemoved(int indexOfRemovedItem)
        {
            ObservableList<ResourceContainer>.UpdateCounterWhenItemIsRemoved(ref cropCounter, indexOfRemovedItem);
        }

        public void Update(GameTime gameTime)
        {
           // System.Diagnostics.Debug.Assert(entity.ID != EntityID.Invalid, "Invalid entity?!?!");

            if (regulator.IsReady())
            {
                isDirty = true; // this will trigger a redraw when a request comes in.
            }
        }

        public void NotifyWhenRegionSearchIsFinished()
        {
            // continue...
            isWaiting = false;
        }

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("ResourceMapForAgent {0}:", ID));

        }


        public bool CycleOnce()
        {

            switch (phase)
            {
                case Phase.GetResourceMap:

                    // get the crop map:

                    ResourceMap crops = entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.GetCropsMap(harvestJob.ResourceType); // CropsMaps[harvestJob.CropType.CropItem];
                    ResourceMap.Result result = crops.GetMap(ref cropsMap); // store it...

                    if (result == ResourceMap.Result.OK)
                    {
                        phase = Phase.Clear;
                    }
                    else
                    {
                        // now wait for it. please notify us:
                        crops.FinishedEvent.Add(crops_FinishedMethodID, this);
                       // crops.FinishedEvent += new EventHandler(crops_FinishedEvent);
                        isWaiting = true;

                        // we will come here again when it is finished.
                    }

                    return false;

                case Phase.Clear:
                    {
                        phase = Phase.DrawDistances;

                        // register as an observer for changes in number of crops:
                        ObservableList<ResourceContainer> cropsList;
                        The.Sim.PlaySite.Resources.TryGetValue(harvestJob.ResourceType, out cropsList);
                        cropsList.ListMemberRemoved.Add(listOfCrops_ListItemRemovedMethodID, this);

                        //cropsList.ListMemberRemoved += new ObservableList<ResourceContainer>.ListMemberRemovedHandler(listOfCrops_ListItemRemoved);

                        return false;
                    }
                case Phase.DrawDistances:
                    {
                        // add inverse of distance (use region map)
                        // from collection center:

                        ThreatStance threatStance;
                        RegionMap regionMapToUse = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStance);

                        SubtileLayers moveMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel,
                            entity.EntityType, entity.Intelligence.ThreatStance).Layers[entity.GetTransportType()];

                        Point entitySubtilePos = MapManager.WorldPosToSubtile(entity.AccessPoint.Value); // #ACCESS.PlaySiteLocation);
                        float distanceToEntity = 0f;

                        EntityGroup jobOwner = LookUp<EntityGroup, EntityGroupID>.FindByID(harvestJob.Owner);
                        if (jobOwner == null)
                        {
                            // job is invalid. Untested...
                          
                            harvestJob.ProcessJob.Destroy(true);
                            harvestJob = null;

                            // start from the top next time:
                            phase = Phase.GetResourceMap;

                            // let the agent evaluator continue now:
                            EndSearch();

                            return true;
                        }

                        Point collectionCenterSubtilePos = MapManager.WorldPosToSubtile(jobOwner.Parent.Location.Value);
                        float distanceToCollectionCenter = 0f;

                       // List<Crop> listOfCrops = UWGame.SimSide.Instance.Site.CropTrees[harvestJob.CropType];
                        ObservableList<ResourceContainer> listOfCrops = The.Sim.PlaySite.Resources[harvestJob.ResourceType];

                        int start = cropCounter;
                        int end = Common.Min(cropCounter + resourcesToDrawPerCycle, listOfCrops.Count);

                       // Crop crop;
                        ResourceContainer crop;

                        Point mapPos = Point.Zero;
                        Point cropSubtilePos;


                        //   byte cropsValue;

                        //  bool noAccess = false;

                        byte cropsMapValue;
                        ushort tileValue;

                        for (int i = start; i < end; i++)
                        {
                            crop = listOfCrops[i];

                            mapPos = crop.MapPosition;

                            //cropsMapValue = Values[mapPos.X][mapPos.Y];
                            cropsMapValue = cropsMap[mapPos.X][mapPos.Y];

                            if (cropsMapValue == 0)
                            {   // skip crops with zero ripe items
                                continue;
                            }

                            // prevent drawing distances twice on a tile
                            tileValue = Values[mapPos.X][mapPos.Y];
                            if (tileValue >= cropsMapValue)
                            {
                                continue; // we were already here...
                            }

                            // get an accessible subtile in the tile - not the where the tree is standing!
                          //  cropSubtilePos = GetAccessibleSubtile(mapPos);

                            Point? closestSubTile;
                            if (!The.Map.GetClosestAccessiblePoint(moveMap, null, crop.Location, true, out closestSubTile))
                            {
                                // set influence map value to zero:
                                Values[mapPos.X][mapPos.Y] = 0;
                                continue;
                            }
                            else
                            {
                                cropSubtilePos = closestSubTile.Value;
                            }
                                                    

                            RegionMap.Result collectionCenterDistanceResult = regionMapToUse.GetDistance(entity, collectionCenterSubtilePos, cropSubtilePos, ref distanceToCollectionCenter, false, notifyWhenRegionSearchIsFinishedMethodID);

                            if (collectionCenterDistanceResult == RegionMap.Result.Wait)
                            {
                                isWaiting = true;
                                return false;
                            }
                            else if (collectionCenterDistanceResult == RegionMap.Result.NoAccess)
                            {
                                // set influence map value to zero:                               
                                Values[mapPos.X][mapPos.Y] = 0;
                                continue;
                            }

                            RegionMap.Result entityDistanceResult = regionMapToUse.GetDistance(entity, entitySubtilePos, cropSubtilePos, ref distanceToEntity, false, notifyWhenRegionSearchIsFinishedMethodID);

                            if (entityDistanceResult == RegionMap.Result.Wait)
                            {
                                isWaiting = true;
                                return false;
                            }
                            else if (entityDistanceResult == RegionMap.Result.NoAccess)
                            {
                                // set influence map value to zero:
                                //noAccess = true;

                                Values[mapPos.X][mapPos.Y] = 0;
                                continue;
                            }

                            // int distanceValue = Common.Clamp((int)(256f - MapManager.oneOverTileSize * (distanceToEntity + distanceToCollectionCenter)), 0, 256);
                            int distanceValue = (int)Common.Clamp(twoTimesMaxMapOctileDistance - MapManager.oneOverTileSize * (distanceToEntity + distanceToCollectionCenter), 0, maxValue);

                            // byte newValue = (byte)Common.Clamp(cropsMapValue + distanceValue, 0, 255);
                            ushort newValue = (ushort)Common.Clamp(cropsMapValue + distanceValue, 0, maxValue);


                            Values[mapPos.X][mapPos.Y] = newValue; // (byte)(currentValue + distanceValue);
                            
                        }

                        cropCounter = end;

                        if (end == listOfCrops.Count)
                        {
                            phase = Phase.DrawOtherAgents;
                            cropCounter = 0;

                            // de-register:
                            ObservableList<ResourceContainer> cropsList;
                            The.Sim.PlaySite.Resources.TryGetValue(harvestJob.ResourceType, out cropsList);
                            cropsList.ListMemberRemoved.Remove(listOfCrops_ListItemRemovedMethodID);

                        }

                        return false;

                    }
                case Phase.DrawOtherAgents:
                    // draw negative influence from any other harvesters, to get harvesters to spread out:

                    // draw them all in one go...
                    SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;
                   // List<Entity> cropHarvesters;
                    List<Tuple<EntityID, ResourceID>> cropHarvesters;
                    if (sharedKnowledge.PlaySiteKnowledge.ResourceHarvesters.TryGetValue(harvestJob.ResourceType.ResourceItemType, out cropHarvesters))
                    {
                       // foreach (Entity cropHarvester in cropHarvesters)
                        foreach (var cropHarvester in cropHarvesters)
                        {
                            if (cropHarvester.Item1 != entity.EntityID)// don't draw ourselves...
                            {
                                // draw where the crop target is
                                ResourceContainer resource = LookUp<ResourceContainer, ResourceID>.FindByID(cropHarvester.Item2);
                                if (resource != null)
                                {
                                    InfluenceMap.DrawLinearInfluenceCircle(Values, resource.MapPosition, -10, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.FalloffEachTile, 4);
                                }
                            }
                        }
                    }

                    phase = Phase.Search;

                    return false;

                case Phase.Search:
                    {
                        //List<Crop> listOfCrops = UWGame.SimSide.Instance.Site.CropTrees[harvestJob.CropType];
                        ObservableList<ResourceContainer> listOfCrops = The.Sim.PlaySite.Resources[harvestJob.ResourceType];

                        /*  int start = cropCounter;
                          int end = Common.Min(cropCounter + cropsToDrawPerCycle, listOfCrops.Count);
                          */

                        //Crop crop;
                        ResourceContainer container;

                        /*byte*/
                        ushort bestScore = 0;
                        /*byte*/
                        ushort currentScore;

                        bestResourceItemID = null;

                        Point mapPos;
                        for (int i = 0; i < listOfCrops.Count; i++)
                        {
                            container = listOfCrops[i];

                            if (container.TotalHarvestableBulk > 0) // TotalBulkOfRipeItems > 0)
                            {
                                mapPos = container.MapPosition; // .GetCropMapPosition();

                                currentScore = Values[mapPos.X][mapPos.Y];

                                if (currentScore > bestScore)
                                {
                                    bestScore = currentScore;
                                    IResourceItem bestItem = container.FindHarvestableItem(); // store the result...
                                    if (bestItem != null)
                                    {
                                        bestResourceItemID = bestItem.ID;
                                    }
                                    else bestResourceItemID = null; //??
                                }
                            }
                        }

                        EndSearch();

                        return true;

                    }
            }

            return false;


        }

        private void EndSearch()
        {
            isDirty = false;

            //when done... the evaluator can continue now:

            entity.SendMessage(new Message(Message.MessageTypes.BestCropFound));

            phase = Phase.GetResourceMap;
        }


      /*  private Point GetAccessibleSubtile(byte[][] map, Point fromSubtile)
        {
            float thisDistance;
            Point thisPoint;
            for (int x = start.X; x < end.X; x++)
            {
                for (int y = start.Y; y < end.Y; y++)
                {
                    thisPoint = new Point(x, y);
                    if (map[x][y] != 0)
                    {
                        thisDistance = Common.DistanceOctile(thisPoint, fromSubtile);

                        if (thisDistance < bestDistance)
                        {
                            bestPoint = thisPoint;
                            bestDistance = thisDistance;
                        }
                    }
                }
            }
        }*/

        public List<ResourceContainer> GetCropsAtTilePos(List<ResourceContainer> listOfCrops, Point pos)
        {
            return listOfCrops.FindAll(c => c.MapPosition == pos);

        }


        /*    public static Vector3? FindBestHarvestLocation(byte[][] values, HarvestJob harvestJob)
            {

                List<Crop> listOfCrops = UWGame.SimSide.Instance.Site.CropTrees[harvestJob.CropType];

           

                Crop crop;

                byte bestScore = 0;
                byte currentScore;
                Crop bestCrop = null;
                Point mapPos;
                for (int i = 0; i < listOfCrops.Count; i++)
                {
                    crop = listOfCrops[i];

                    mapPos = GetCropMapPosition(crop);

                    currentScore = values[mapPos.X][mapPos.Y];

                    if (currentScore > bestScore)
                    {
                        bestScore = currentScore;
                        bestCrop = crop;
                    }
                }

                values

                if (bestCrop != null)
                {

                }
                else
                {
                    return null;
                }
            }*/

        /* private static Point GetCropMapPosition(Crop crop)
         {
           
             Tree tree = crop.Parent as Tree;
             if (tree != null)
             {
                 return tree.Parent.MapPosition;
             }
             else
             {
                 LowVegetation vegetation = crop.Parent as LowVegetation;
                 return new Point(vegetation.Parent.X, vegetation.Parent.Y);
             }
         }*/

        void crops_FinishedEvent()
        {
            // now we can continue...
            isWaiting = false;

            ResourceMap crops = entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.GetCropsMap(harvestJob.ResourceType); //.CropsMaps[harvestJob.CropType.CropItem];

            //crops.FinishedEvent -= new EventHandler(crops_FinishedEvent);
            crops.FinishedEvent.Remove(crops_FinishedMethodID);
        }


        #region ILookup

        private CyclableID id = CyclableID.Invalid;
     
        //=================== ILookup Methods =====================
        public CyclableID ID
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

        public CyclableID GetUniqueID()
        {
            return Cyclable.GetUniqueID();
        }

        public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
        {
            return (CyclableID)sn.DoEnum(id);
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
            if (ID != CyclableID.Invalid)
                LookUp<ICyclable, CyclableID>.Add(ID, this);
        }

        public void RemoveIDEntry()
        {
            LookUp<ICyclable, CyclableID>.Remove(this);
        }

        public void SetInvalid()
        {
            id = CyclableID.Invalid;
        }

        public void ResetIDCounter() // interface method - does nothing... Sim will call Cyclable.ResetIDCounter.
        {
        }

        void ILookUp<ICyclable, CyclableID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ICyclable, CyclableID>.Create();
        }

        #endregion


        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return false; // we snapshot the current progress and pick up after load.
            }
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // when snapshotting, we scrap the current progress.

            this.id = SnapshotID(sn, id);
            this.isWaiting = sn.DoBool(isWaiting);
            this.phase = sn.DoEnum(phase);
            this.bestResourceItemID = sn.DoEnumNullable(bestResourceItemID);
            this.cropCounter = sn.DoInt32(cropCounter);

            this.crops_FinishedMethodID = sn.DoEnum(crops_FinishedMethodID);
            this.listOfCrops_ListItemRemovedMethodID = sn.DoEnum(listOfCrops_ListItemRemovedMethodID);
            this.notifyWhenRegionSearchIsFinishedMethodID = sn.DoEnum(notifyWhenRegionSearchIsFinishedMethodID);

            this.cropsMap = sn.DoJaggedArray(cropsMap); 
            this.isDirty = sn.DoBool(isDirty);
          //  this.mapWidth = sn.DoInt32(mapWidth);
            this.maxValue = sn.DoInt32(maxValue);
            this.rowCounter = sn.DoInt32(rowCounter);

         /*   if (sn.mode != Snapshotter.Mode.Load)
            {
                System.Diagnostics.Debug.Assert(entity.ID != EntityID.Invalid, "Invalid entity?!?!");
            }*/

            this.snapshotEntity = (EntityID)sn.SnapshotID<Entity, EntityID>(entity);
           
            this.snapshotHarvestJob = null;
            if (harvestJob != null)
            {
                this.snapshotHarvestJob = harvestJob.ProcessJob.ID;
            }
            this.snapshotHarvestJob = sn.DoEnumNullable(snapshotHarvestJob);
            //this.snapshotHarvestJob = sn.DoEnumNullable(snapshotHarvestJob);
            
            this.twoTimesMaxMapOctileDistance = sn.DoInt32(twoTimesMaxMapOctileDistance);

            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);

            sn.Ignore(harvestJob);
            sn.Ignore(Values);
           
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
            sn.RegisterLoadPostProcessCall(this);

            LoadPostProcessRegisterMethodIDs();

            if (snapshotHarvestJob.HasValue)
            {
                harvestJob = ((ProcessJob)LookUp<Job, JobID>.FindByID(snapshotHarvestJob.Value)).HarvestJob;
            }
            snapshotHarvestJob = null; // remember to clear/set to null for the next save

            entity = Entity.FindByID(snapshotEntity); 
            Values = entity.Intelligence.EvaluatorInfluenceMap;

            CreateRegulators();
        }

        #endregion


        public void LoadPostProcessRegisterMethodIDs()
        {
            ActionLookup.Add(crops_FinishedMethodID, crops_FinishedEvent);
            ActionLookup.Add(notifyWhenRegionSearchIsFinishedMethodID, NotifyWhenRegionSearchIsFinished);           
            ActionLookup<int>.Add(listOfCrops_ListItemRemovedMethodID, listOfCrops_ListItemRemoved);


        }
    }
}
