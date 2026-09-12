using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.IngameEvents;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Overland
{
    public class PlaySite : ISnapshot
    {
        // all ICyclables need an ID field for reconnect after load

       // private DegradeManager degradeManager;
       // private CyclableID degradeManagerID;
        
     
        public WeatherManager Weather;
        private CyclableID weatherID;


        /// <summary>
        /// playsite only... 
        /// 
        /// we should update this after agents (workers)
        /// </summary>
        public SleepyUpdater<SimProcess> Processes;
        List<SimProcessID> snapshotProcesses;


        public SleepyUpdater<TileResourceContainer> TileResources;
        List<ResourceID> snapshotResources;


        public CollisionManager<Expedition> ExpeditionRadiusQuadTree;
        List<ExpeditionID> snapshotExpeditionRadiusQuadTree;
     


        public PlaySite()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                if (The.Sim.Mode == Sim.EngineMode.Game)
                {
                    int maxCollidableEntities = 999;
                    ExpeditionRadiusQuadTree = new CollisionManager<Expedition>(new Vector2(The.Map.MapWorldWidth, The.Map.MapWorldHeight), maxCollidableEntities);
                }
              
                #region ICyclables

                // save an id for these

              /*  degradeManager = new DegradeManager();
                degradeManagerID = degradeManager.ID;
                */
             
                Weather = new WeatherManager();
                weatherID = Weather.ID;

                InitProcesses();
                InitTileResources();

                #endregion


            }

        }


        private void InitProcesses()
        {
            Processes = new SleepyUpdater<SimProcess>(Module.Sim, true);
        }

        public void AddProcess(SimProcess process)
        {
            Processes.Add(process);
        }

        public void RemoveProcess(SimProcess process)
        {
            Processes.Remove(process);

        }

        private void InitTileResources()
        {
            TileResources = new SleepyUpdater<TileResourceContainer>(Module.Sim, true);
        }

        public void AddTileResourceContainer(TileResourceContainer resource)
        {
            TileResources.Add(resource);
        }

        public void RemoveTileResourceContainer(TileResourceContainer resource)
        {
            TileResources.Remove(resource);
        }


        public void Update(GameTime gameTime)
        {
          //  degradeManager.Update(gameTime);
            
            Weather.Update(gameTime);

            Processes.Update(gameTime);

            TileResources.Update(gameTime);
        }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {           
            if (sn.mode != Snapshotter.Mode.Load)
            {       
                if (Processes != null)
                {
                    snapshotProcesses = new List<SimProcessID>();
                    Processes.IterateItems(e => snapshotProcesses.Add(e.ID));
                }

                if (TileResources != null)
                {
                    snapshotResources = new List<ResourceID>();
                    TileResources.IterateItems(e => snapshotResources.Add(e.ID));
                }
                               
                this.snapshotExpeditionRadiusQuadTree = ExpeditionRadiusQuadTree.GetAllObjects().Select(e => e.ID).ToList();                 
            }

            ExpeditionRadiusQuadTree = (CollisionManager<Expedition>)sn.DoISnapshot(ExpeditionRadiusQuadTree);
            this.snapshotExpeditionRadiusQuadTree = sn.DoList(snapshotExpeditionRadiusQuadTree);
         

            // we are responsible for snapshotting SleepyUpdater members:
            snapshotProcesses = sn.DoList(snapshotProcesses);
            snapshotResources = sn.DoList(snapshotResources);
          

            #region ICyclables - only Ids

          //  degradeManagerID = (CyclableID)sn.DoEnum(degradeManagerID);
            weatherID = (CyclableID)sn.DoEnum(weatherID);

            #endregion

            sn.Ignore(Weather);
            //sn.Ignore(degradeManager);
          
            sn.Ignore(Processes);
            sn.Ignore(TileResources);

            return this;
        }



        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);
                      

            InitProcesses();
            if (snapshotProcesses != null)
            {
                foreach (var item in snapshotProcesses)
                {
                    Processes.Add(LookUp<SimProcess, SimProcessID>.FindByID(item), keepExistingTimepoint: true);
                }
                snapshotProcesses.Clear();  // clear for next save
            }

            InitTileResources();
            if (snapshotResources != null)
            {
                foreach (var item in snapshotResources)
                {
                    TileResources.Add((TileResourceContainer)LookUp<ResourceContainer, ResourceID>.FindByID(item), keepExistingTimepoint: true);
                }
                snapshotResources.Clear();  // clear for next save
            }


            List<Collidable<Expedition>> listToRebuildCollisionManagerFrom =
            snapshotExpeditionRadiusQuadTree.Select(t => Expedition.FindByID(t).Collidable).ToList(); // Requires Expedition.LoadPostProcess

            ExpeditionRadiusQuadTree.SetPreLoadPostProcess(listToRebuildCollisionManagerFrom);
            ExpeditionRadiusQuadTree.LoadPostProcess(sn); // this will fix the quad tree



            #region ICyclables

          //  degradeManager = (DegradeManager)LookUp<ICyclable, CyclableID>.FindByID(degradeManagerID);
            Weather = (WeatherManager)LookUp<ICyclable, CyclableID>.FindByID(weatherID);

       
            #endregion

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


    }
}
