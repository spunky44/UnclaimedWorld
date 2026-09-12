using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using UWGame.SimSide.Items;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
    /// <summary>
    /// handles the simulation stuff in terrain tiles that can be done with a few seconds interval in parallel.
    /// don't do any tile/tile interaction here! (Cellular automata stuff)
    /// 
    /// DISABLED
    /// </summary>
    public class TerrainTileUpdateManager: ICyclable
    {
        // phases can be used to sequence updates if there are simulations that depend on each other
        // or perhaps if we want to cache calculations that are common for all entities?

      /*  private enum Phase { Degrade, DoComposites }
        private Phase phase = Phase.Degrade;
        */

        int rowCounter = 0;

       // double deltaTimeInMilliseconds;
        double deltaTimeInSeconds;

        private Regulator regulator;

      //  const int tilesPerCycle = 500;

       // const int entitiesPerThread = 100;

        const double oneOverThousand = 0.001;

        public bool IsPaused { get; set; }

        public double? UpdateInterval
        {
            get
            {
                return 10d;
            }
        }


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

        public TerrainTileUpdateManager()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                AddToLookup();

                CreateRegulators();
            }
        }


        void CreateRegulators()
        {
            // run once every 10 seconds?
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / UpdateInterval.Value, ToString()); // new Regulator(0.1);

        }

        public void Update(GameTime gameTime)
        {
            if (!The.Sim.CycleManager.IsRegistered(this))
            {
                double milliSecondsSinceLastReady = 0;
                if (regulator.IsReady(ref milliSecondsSinceLastReady))
                {
                   // deltaTimeInMilliseconds = milliSecondsSinceLastReady; 
                    deltaTimeInSeconds = oneOverThousand * milliSecondsSinceLastReady; // get the precise time since we were last here! use this value to calculate degradation.

                   // phase = Phase.Degrade;

                    rowCounter = 0;
                    The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
                }
            }

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
            text.Append(string.Format("TerrainTileUpdateManager: "));

        }

        public bool CycleOnce()
        {
            //int start, 
            int end;

            //start = rowCounter;
            //end = rowCounter + 1;
            TerrainTile[][] tileMap = The.Map.TileMap;
            end = Common.GetJaggedArrayWidth(tileMap);

          //  end = Common.Min(rowCounter + tilesPerCycle, UWGame.SimSide.Instance.Site.Entities.Count);

            // The .Net Task scheduler will partition the source as appropriate...
            Parallel.For(0, end, (x) =>
            {                
                TerrainTile tile = tileMap[x][rowCounter];
                
                tile.UpdateSimulationInParallel(deltaTimeInSeconds); // deltaTimeInMilliseconds);
                
            });

            // debug only!!!
          /*  for (int i = start; i < end; i++)
            {
                Entity entity;
                
                entity = UWGame.SimSide.Instance.Site.Entities[i]; // only entities on "Site"???? Other entities in the world would need to age/die as well...

                entity.UpdateSimulationInParallel(deltaTimeInSeconds); // deltaTimeInMilliseconds);
            }*/

            rowCounter = rowCounter + 1;

            if (rowCounter == Common.GetJaggedArrayHeight(tileMap))
            {
                rowCounter = 0;
               // phase = Phase.DoComposites;
                return true;
            }

            return false;


        /*    switch (phase)
            {
                case Phase.Degrade:
                    start = itemCounter;
                    end = Common.Min(itemCounter + itemsPerCycle, UWGame.SimSide.Instance.AllItems.Count);

                    for (int i = start; i < end && i < UWGame.SimSide.Instance.AllItems.Count; i++)
                    {
                        item = UWGame.SimSide.Instance.AllItems[i]; // only items on "Site"????

                        if (item.Parts == null) // only degrade the parts...
                        {
                            item.Degrade(daysPassed); //deltaTime);
                        }
                    }

                    itemCounter = end;

                    if (end == UWGame.SimSide.Instance.AllItems.Count)
                    {
                        itemCounter = 0;
                        phase = Phase.DoComposites;
                        //return true;
                    }

                    return false;

            }*/
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
            // TODO - this class is not currently used...

           /* this.id = SnapshotID(sn, id);
            this.phase = (Phase)sn.DoEnum(phase);
            this.IsPaused = sn.DoBool(IsPaused);
            */


            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);
            sn.Ignore(regulator);

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

            CreateRegulators();
        }

        #endregion
    }
}
