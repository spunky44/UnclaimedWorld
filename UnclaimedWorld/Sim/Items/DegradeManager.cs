using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items
{
    /// <summary>
    /// simulates degradation of non-living entities
    /// </summary>
   /* public class DegradeManager : ICyclable, IIDEventSubscriber
    {
        int itemCounter = 0;

        double deltaTime;

        private enum Phase { Degrade, DoComposites }
        private Phase phase = Phase.Degrade;

        private Regulator regulator;

        const int itemsPerCycle = 20;

       
        MethodID allItems_ListItemRemovedMethodID;

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
                return 10d;
            }
        }

        public DegradeManager()
        {
           
            if (!Snapshotter.IsSnapshotting)
            {
                The.Sim.PlaySite.DegradableEntities.ListMemberRemoved.AddAndRegister(AllItems_ListItemRemoved, this, out allItems_ListItemRemovedMethodID); //  += new ObservableList<Entity>.ListMemberRemovedHandler(AllItems_ListItemRemoved);

                CreateRegulator();

                AddToLookup();
            }
        }

        private void CreateRegulator()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1f / UpdateInterval.Value, "DegradeManager");
        }

        public bool IsPaused { get; set; }

        void AllItems_ListItemRemoved(int indexOfRemovedItem)
        {
            ObservableList<Entity>.UpdateCounterWhenItemIsRemoved(ref itemCounter, indexOfRemovedItem);
        }

        

        public void Update(GameTime gameTime)
        {
            if (!The.Sim.CycleManager.IsRegistered(this))
            {
                double milliSecondsSinceLastReady = 0;
                if (regulator.IsReady(ref milliSecondsSinceLastReady))
                {
                    deltaTime = milliSecondsSinceLastReady; // get the precise time since we were last here! use this value to calculate degradation.
                    
                    phase = Phase.Degrade;
                    itemCounter = 0;
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



        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != CyclableID.Invalid)
                LookUp<ICyclable, CyclableID>.Add(ID, this);
        }

        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
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


        #endregion

        #region ICyclable Members

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("DegradeManager"));

        }
        public bool CycleOnce()
        {
            int start, end;
            Entity entity;

            
            // TODO: mark handled items because the list is volatile? Timestamp...

            // probably better to place all this in Entity Update with a sleepy update, and staggered update intervals...

            //double degradeTimeFactor = deltaTime * 0.001;
            // delta time is in milliseconds
            double daysPassed = 0.001 * deltaTime * The.Sim.DateAndTime.DaysPerSecond;

            switch (phase)
            {
                case Phase.Degrade:
                    start = itemCounter;
                    end = Common.Min(itemCounter + itemsPerCycle, The.Sim.PlaySite.DegradableEntities.Count);
                    
                    for (int i = start; i < end && i < The.Sim.PlaySite.DegradableEntities.Count; i++)
                    {
                        entity = The.Sim.PlaySite.DegradableEntities[i]; // only items on "Site"????

                        if (entity.IsStarted() == true
                            && (entity.IsLeaf // only degrade the leaf parts...
                            || entity.IsCompositeRoot)) // NEW: degrade the root also (integrity)
                        {
                            // do degrade damage corresponding to environment, and have a chance for total brokedown
                            entity.NonLivingEntity.Degrade(daysPassed); 
                        }
                    }

                    itemCounter = end;

                    if (end == The.Sim.PlaySite.DegradableEntities.Count)
                    {
                        itemCounter = 0;
                        phase = Phase.DoComposites;
                        //return true;
                    }

                    return false;

                case Phase.DoComposites:

                    start = itemCounter;
                    end = Common.Min(itemCounter + itemsPerCycle, The.Sim.PlaySite.DegradableEntities.Count);

                    for (int i = start; i < end; i++)
                    {
                        entity = The.Sim.PlaySite.DegradableEntities[i];

                        if (entity.IsCompositeRoot) // entity.Parts != null && entity.IsRoot) // .PartOf == null) // !(entity.PartOf is Item)) // do the composites now... start with the top level!
                        {
                            // set their condition to average of their parts...
                            entity.NonLivingEntity.ComputeConditionOfComposite();
                            // todo: broken parts.
                        }
                    }

                    itemCounter = end;

                    if (end == The.Sim.PlaySite.DegradableEntities.Count)
                    {                        
                        return true;
                    }
                    else
                    {
                        return false;
                    }                   

                default: return true;
            }
        }


        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return false; // don't unregister, we save our progress
            }
        }

        #endregion


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // when snapshotting, we save the current progress to avoid over-degrading...

            this.id = SnapshotID(sn, id);
            this.phase = (Phase)sn.DoEnum(phase);
            this.IsPaused = sn.DoBool(IsPaused);

            this.deltaTime = sn.DoDouble(deltaTime);
            this.itemCounter = sn.DoInt32(itemCounter);
            this.allItems_ListItemRemovedMethodID = (MethodID)sn.DoEnum(allItems_ListItemRemovedMethodID);

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

            CreateRegulator();

        }
        
        #endregion


        public void LoadPostProcessRegisterMethodIDs()
        {
            ActionLookup<int>.Add(allItems_ListItemRemovedMethodID, AllItems_ListItemRemoved);
        }
    }*/
}
