using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
    [ComponentAttribute]
    public abstract class Component : ISnapshot
    {
       
        public Entity Parent;
        private EntityID parentID;

       
        /// <summary>
        /// if the regulator and Entity/SleepingUpdatable have the same update interval (like 10s for trees), 
        /// the component will miss an update if the regulator drifts and gets set to a slightly higher value than the Entity.Timepoint.
        /// 
        /// To avoid this, give the regulator an interval that is lightly less (about 0.1%?) than the Entity/SleepingUpdatable
        /// 
        /// if the component regulator has an interval that is higher than the Entity/SleepingUpdatable, the difference will not matter.
        /// </summary>
        private Regulator updateRegulator;

        /// <summary>
        /// this regulator is reset when going offsite.
        /// </summary>
        private Regulator updatePlaySiteRegulator;


        private double updateIntervalInSeconds;

        public Component(Entity parent, double updateIntervalInSeconds)
        {
            this.updateIntervalInSeconds = updateIntervalInSeconds;

            System.Diagnostics.Debug.Assert(Common.IsGreaterThan(updateIntervalInSeconds, 0d), "Should be greater than zero");  
 
            this.Parent = parent;

            double intervalToUse = GetIntervalToUse(updateIntervalInSeconds);

            updateRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / intervalToUse, "Component", Regulator.Modes.Sim);
            CreatePlaySiteRegulator();
        }

        private static double GetIntervalToUse(double updateIntervalInSeconds)
        {
            double intervalToUse = updateIntervalInSeconds - 1d / 60d;
            return intervalToUse;
        }

        private void CreatePlaySiteRegulator()
        {
            double intervalToUse = GetIntervalToUse(updateIntervalInSeconds);

            updatePlaySiteRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / intervalToUse, "Component", Regulator.Modes.Sim);

        }

      

        public Component(Entity parent)
        {
            this.Parent = parent;
        }

        public Component()
        {
          //  System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");   
        }

        public virtual double? GetUpdateInterval()
        {
            return null;
        }

        /// <summary>
        /// override this if no regulation is needed... like if all entity components use the same interval (trees or dead objects?)
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            bool isReady = true;
            double? timeSinceLastUpdate = null;

            if (updateRegulator != null)
            {
                double timePassed;
                isReady = updateRegulator.IsReadyGetTimeElapsedInSeconds(out timePassed);
                timeSinceLastUpdate = timePassed;
            }

            if (isReady)
            {
                UpdateRegulated(timeSinceLastUpdate);
            }

        }

        /// <summary>
        /// override this if no regulation is needed.
        /// this method has its own regulator that gets reset when moving off-site
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void UpdatePlaySite(GameTime gameTime)
        {
            bool isReady = true;
            double? timeSinceLastUpdate = null;

            if (updatePlaySiteRegulator != null)
            {
                double timePassed;
                isReady = updatePlaySiteRegulator.IsReadyGetTimeElapsedInSeconds(out timePassed);
                timeSinceLastUpdate = timePassed;
            }

            if (isReady)
            {
                UpdatePlaySiteRegulated(timeSinceLastUpdate);
            }
        }

        /// <summary>
        /// all playsite regulators must be reset when leaving the playsite! 
        /// Otherwise elapsed time will be accrued while the entity is away, and can result in a huge time delta when it returns
        /// </summary>
        public virtual void ResetPlaySiteRegulators()
        {
            if (updatePlaySiteRegulator != null)
            {
                CreatePlaySiteRegulator();
            }
        }

        protected virtual void UpdateRegulated(double? timeSinceLastUpdate)
        {

        }

        /// <summary>
        /// the regulator controlling this will be reset when leaving the playsite to avoid accumulating time.
        /// </summary>
        /// <param name="timeSinceLastUpdate"></param>
        protected virtual void UpdatePlaySiteRegulated(double? timeSinceLastUpdate)
        {

        }


        #region ISnapshot

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            
            parentID = (EntityID)sn.SnapshotID<Entity, EntityID>(Parent);

            updateIntervalInSeconds = sn.DoDouble(updateIntervalInSeconds);

            updateRegulator = (Regulator)sn.DoISnapshot(updateRegulator);
            updatePlaySiteRegulator = (Regulator)sn.DoISnapshot(updatePlaySiteRegulator);
           
            return this;
        }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Parent = Entity.FindByID(parentID);

            if (updateRegulator != null)
            {
                updateRegulator.LoadPostProcess(sn);
            }

            if (updatePlaySiteRegulator != null)
            {
                updatePlaySiteRegulator.LoadPostProcess(sn);
            }
        }

        Snapshotter.Version version;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public bool IsSnapshotted { get; set; }

        #endregion

    }
}
