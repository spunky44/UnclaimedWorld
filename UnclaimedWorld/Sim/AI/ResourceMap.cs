using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI
{

    
    public class ResourceMap : ICyclable, IIDEventSubscriber
    {
        /// <summary>
        /// Tile map!
        /// jagged arrrys outperform multi-dimensional arrays by a factor of 2 when doing indexing...    
        /// </summary>
        private byte[][] values;
        private byte[][] newValues;

        int cropsToDrawPerCycle = 10;

        // progress varialbes
        int cropCounter = 0;
       
        private enum Phase { Clear, Draw }
        private Phase phase = Phase.Clear;

        public bool IsPaused { get; set; }

        public const byte MaxValue = 100;

        ResourceType ResourceType;

        bool isDirty = true;

     //   private const float updatesPerSecond = 1f / 30f; // once per 30 secs
        private Regulator regulator;

        MethodID cropsMap_ListItemRemovedMethodID;

        //public event EventHandler FinishedEvent;
        public IDActionEvent FinishedEvent;

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

        public double? UpdateInterval
        {
            get
            {
                return 30d;
            }
        }

        public ResourceMap()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        public ResourceMap(ResourceType resourceType)
        {
           
            this.ResourceType = resourceType;

           
            //ObservableList<Crop> listOfTrees;
            ObservableList<ResourceContainer> listOfTrees;
            if (!The.Sim.PlaySite.Resources.TryGetValue(resourceType, out listOfTrees))
            {
                //listOfTrees = new ObservableList<Crop>();
                listOfTrees = new ObservableList<ResourceContainer>();
                The.Sim.PlaySite.Resources.Add(resourceType, listOfTrees);
            }

            listOfTrees.ListMemberRemoved.AddAndRegister(CropsMap_ListItemRemoved, this, out cropsMap_ListItemRemovedMethodID); //  += new ObservableList<ResourceContainer>.ListMemberRemovedHandler(CropsMap_ListItemRemoved);
            // listOfTrees.ListItemRemoved += new ObservableList<ResourceContainer>.ListItemRemovedHandler(CropsMap_ListItemRemoved);

            AddToLookup();

            CreateRegulators();
        }

        private void CreateRegulators()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / UpdateInterval.Value, "ResourceMap");
        }

        void CropsMap_ListItemRemoved(int indexOfRemovedItem)
        {
            ObservableList<Entity>.UpdateCounterWhenItemIsRemoved(ref cropCounter, indexOfRemovedItem);
        }


        public void Update(GameTime gameTime)
        {
            if (regulator.IsReady())
            {
                isDirty = true; // this will trigger a redraw when a request comes in.
            }
        }

        public void Destroy()
        {
            if (The.Sim.CycleManager.IsRegistered(this))
            {
                The.Sim.CycleManager.UnRegister(this);
            }

            ActionLookup<int>.Remove(cropsMap_ListItemRemovedMethodID);

            RemoveIDEntry();
        }

        public enum Result { OK, Wait }

        /// <summary>
        /// they may have to wait for the result.    
        /// </summary>
        /// <param name="fromRegion"></param>
        /// <param name="toRegion"></param>
        /// <returns></returns>
        public Result GetMap(ref byte[][] outValues)
        {
            if (isDirty)
            {
                
                if (!The.Sim.CycleManager.IsRegistered(this))
                {
                    cropCounter = 0;

                    if (this.values == null)
                    {
                        int mapWidth = The.Map.mapTileWidth;
                        int mapHeight = The.Map.mapTileHeight;

                        Common.InitJaggedArray(ref this.values, mapWidth, mapHeight);
                        Common.InitJaggedArray(ref newValues, mapWidth, mapHeight);

                        /*
                        values = new byte[mapWidth][];

                        for (int i = 0; i < mapWidth; i++)
                        {
                            values[i] = new byte[mapHeight];
                        }*/

                    }
                    else
                    {                        
                        // clear the scratch pad, keep the real values:
                        Common.ClearJaggedArray(newValues);                      
                    }

                    The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
                }

                return Result.Wait;
            }

            outValues = this.values;
            return Result.OK;
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

        #region ICyclable Members

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("ResourceMap {0}:", ID));

        }

        //private enum ProductionJobTypeToAdd { Harvest, }
        public bool CycleOnce()
        {

            switch (phase)
            {
                case Phase.Clear:

                    Common.ClearMap(values);

                    phase = Phase.Draw;

                    return false;

                case Phase.Draw:
                   
                    ObservableList<ResourceContainer> listOfCrops = The.Sim.PlaySite.Resources[ResourceType];
                    //List<Crop> listOfCrops = UWGame.SimSide.Instance.Site.CropTrees[CropType];

                    // we know all trees, but not their state, or current crops...
                    int start = cropCounter;
                    int end = Common.Min(cropCounter + cropsToDrawPerCycle, listOfCrops.Count);

                  
                   // Crop crop;
                    ResourceContainer crop;

                    Point mapPos = Point.Zero;
                    int cropValue;

                    for (int i = start; i < end; i++)
                    {
                        crop = listOfCrops[i];
                        
                        mapPos = crop.MapPosition;

                        cropValue = (int)(8f * crop.TotalHarvestableBulk); //TotalBulkOfRipeItems); // TODO: this should be looked up in the SharedKnowledge!
                        cropValue = Math.Min(cropValue, MaxValue);

                        if (crop.TotalHarvestableBulk > 0) //TotalBulkOfRipeItems > 0)
                        {
                            cropValue = Math.Max(1, cropValue);
                        }

                        if (cropValue > 0)
                        {
                            InfluenceMap.DrawLinearInfluenceCircle(newValues, mapPos, cropValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.FalloffEachTile, 2);
                        }

                    }

                    cropCounter = end;

                    if (end == listOfCrops.Count)
                    {
                        // we copy over now!
                        Common.CopyJaggedArray(newValues, values);

                        cropCounter = 0;
                        phase = Phase.Clear;

                        isDirty = false;

                        if (FinishedEvent != null)
                        {
                            // how we notify...
                            FinishedEvent.Invoke();                           
                        }

                        return true; 
                    }

                    return false;

            }

            return false;
        }

        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return false; // TODO
            }
        }
        #endregion



        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.id = SnapshotID(sn, id);
            this.phase = sn.DoEnum(phase);
            this.IsPaused = sn.DoBool(IsPaused);
            this.cropsMap_ListItemRemovedMethodID = sn.DoEnum(cropsMap_ListItemRemovedMethodID);
            this.cropCounter = sn.DoInt32(cropCounter);
            this.cropsToDrawPerCycle = sn.DoInt32(cropsToDrawPerCycle);
            this.FinishedEvent = (IDActionEvent)sn.DoISnapshot(FinishedEvent);
            this.isDirty = sn.DoBool(isDirty);
            this.newValues = sn.DoJaggedArray(newValues);
            this.values = sn.DoJaggedArray(values);
            this.ResourceType = sn.DoGameData(ResourceType);

            sn.Ignore(regulator);
            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);

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

            CreateRegulators();
        }

        #endregion

        public void LoadPostProcessRegisterMethodIDs()
        {
            ActionLookup<int>.Add(cropsMap_ListItemRemovedMethodID, CropsMap_ListItemRemoved);           

        }

    }
}
