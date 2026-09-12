using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Maps.Regions
{
    /// <summary>
    /// Total Colors: 65.536
    /// its region colors are in a specific range from 1 to 40.000
    /// </summary>
    public class TerrainRegionMap: RegionMap
    {
        private const float refreshIntervalInSeconds = 4f; // 5f;

     //   bool isComputing;

        /// <summary>
        /// to be sent when changes are made to the shared regions...
        /// all searches should be suspended. they will have to be restarted later.
        /// Also, all layer connection work has to be stopped and later restarted.
        /// The only thing that can be done is region filling.
        /// </summary>
        public IDActionEvent RegionsAreInvalid = new IDActionEvent();

        /// <summary>
        /// sent when the regions are done. graphs and costs must be recomputed by the dependent region maps.
        /// </summary>
        public IDActionEvent RegionsFinished = new IDActionEvent();

        private Regulator regulator;

        public override Dictionary<ushort, Dictionary<ushort, RegionEdge>> BottomRegionGraph
        {
            get
            {
                return RegionGraph;
            }
        }

        public override Dictionary<ushort, Region> BottomRegions
        {
            get { return regions; }
        }

        public override Dictionary<ushort, HashSet<ushort>> BottomBlockedEdges
        {
            get { return null; }
        }

      /*  public override Dictionary<ushort, Dictionary<ushort, RegionEdge>> BottomToUpperLayerConnectors
        {
            get { return null; }
        }*/

        protected override ushort ColorStartOfRange
        {
            get
            {
                return 1;
            }
        }

        public TerrainRegionMap()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        public TerrainRegionMap(SubtileLayers layers): base(layers, layers.Layers[0])
        {
            sectorsAreDirty = true; // start with a full recompute...
        }

        protected override void RecomputeStarted()
        {
            base.RecomputeStarted();

            if (sectorsAreDirty)
            {              
                RegionsAreInvalid.Invoke();
            }
        }

        /// <summary>
        /// called when the region map has completed a full recompute
        /// </summary>
        protected override void RecomputeFinished()
        {
            base.RecomputeFinished();
            /*
            if (isComputing)
            {
                AddLog("Recompute ended, isComputing = false");

                isComputing = false;*/
                RegionsFinished.Invoke();
           // }
        }

        protected override bool CanServiceRequests
        {
            get
            {
                return RegionGraph != null && RegionGraph.Count > 0;                
            }
        }


        public bool IsComputing
        {
            get
            {
                return isComputing;
            }
        }

        public override Region GetRegion(ushort color)
        {            
            if (color > 0)
            {
                return regions[color]; // Regions[color - 1]; // no region has the color 0 - this value is for 'no region'...
            }
            else return null;            
        }

        public override Region GetRegionInProgress(ushort color)
        {
            if (color > 0)
            {
                return newRegions[color];
            }
            else return null;
        }

        protected override bool BuildRegionGraphBetweenSectors()
        {
            bool isLastSector;

            SubtileSector sector = GetCurrentDirtySectorAndIncrement(out isLastSector); // the sectors are in order left to right, top to bottom
                     

            ScanSectorEdges(sector, null);

            return isLastSector;
        }

        protected override void AddEdgesOrConnectors(ushort fromRegion, ushort toRegion)
        {
            AddEdgesIfNotExists(fromRegion, toRegion);            
        }


        protected override void CreateRegulators()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1f / refreshIntervalInSeconds, "RegionMap");
             
        }

        /// <summary>
        /// only called for TerrainRegionMap!
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {           
            if (!The.Sim.CycleManager.IsRegistered(this))
            {
                if (regulator.IsReady())
                {
                    if (regions.Count == 0)
                    {
                        // create from bottom up:
                        progress = Progress.InitFromScratch;
                        
                    }
                    else
                    {
                        if (sectorsAreDirty)
                        {
                            // repair the map:
                            SetProgressAtRepairStart();
                            //progress = Progress.InitCleanupUnneededSectors;
                        }
                        else return;

                        /* OLD:
                        if (dirtyPoints.Count > 0 || listOfDirtyPointsOutsideRegions.Count > 0)
                        {
                            // repair the map:
                            progress = Progress.InitCleanupUnneededSectors;
                        }
                        else
                        {
                            // skip it - no change
                            return;
                        }*/
                    }

                    //  currentCenter = new Point(-regionRadius, regionRadius); // the procedure to find the next center starts by adding regionInterval = 2 * regionRadius

                    //  listOfUnassignedPoints.Clear();

                    The.Sim.CycleManager.Register(this, CycleManager.Priority.High);
                }
            }         

        }

        public override Snapshots.ISnapshot DoSnapshot(Snapshots.Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.RegionsAreInvalid = (IDActionEvent)sn.DoISnapshot(RegionsAreInvalid);
            this.RegionsFinished = (IDActionEvent)sn.DoISnapshot(RegionsFinished);


            sn.Ignore(regulator);

            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            RegionsAreInvalid.LoadPostProcess(sn);
            RegionsFinished.LoadPostProcess(sn);
        }


    }
}
