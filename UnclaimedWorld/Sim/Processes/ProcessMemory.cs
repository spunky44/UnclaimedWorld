using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Processes
{
    public enum ProcessMemoryID : long
    {
        First = 0L,
        Invalid = long.MaxValue,
        Max = Invalid
    }

    /// <summary>
    /// only Started processes should have a ProcessMemory.
    /// Currently, it is not possible to start a job process in the FOW.
    /// </summary>
    public class ProcessMemory : IKnownProcess, ILookUp<ProcessMemory, ProcessMemoryID>, ISnapshot
    {
        public SimProcessID ProcessID { get; set; }

        public Point? MapPosition { get; set; }

        public bool IsStarted { get; set; }

        public ProcessType ProcessType { get; private set; }

        /// <summary>
        /// not an IKnownProcess member.
        /// </summary>
        public bool RealProcessIsCompleted; // { get; set; }

       // public Productivity CompletedProcessProductivity;

        /// <summary>
        /// it should never happen that we assign inputs to a ProcessMemory.
        /// </summary>
        public Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> AssignedInputs { get; set; }

        public EntityID? ImmovableTool { get; set; }
        public EntityID? ImmovableInput { get; set; }
        public Vector3? GroundLocation { get; set; }

        public EntityID? ContainerToPlaceOutputsIn { get; set; }

        public List<EntityID> OutputEntities { get; private set; }

        public UpgradeCategory UpgradeCategory { get; set; }

        public List<EntityID> StationaryTools { get; set; }

        public EntityAndRoot? ActingOnEntity { get; set; }


        public float ProgressSpeed { get; set; }


        /// <summary>
        /// gets set when the real process ends... somehow, we know how the process went.
        /// </summary>
        public Productivity Productivity { get; set; }

        /*
        public float SkillProductivity
        {
            get; 
            set;
        }

        public float ToolProductivity
        {
            get;
            set;
        }

        public float EnergyProductivity
        {
            get;
            set;
        }

        public float TotalProductivity
        {
            get;
            set;
        }*/


       // public EntityID? ActingOnEntity { get; set; }

        /// <summary>
        /// The root, if any, of the ActingOnEntity
        /// NOT in the process.
        /// Only used for validating that the part is still a part during the process
        /// repair and replenish requires this validation now.
        /// </summary>
       // public EntityID? ActingOnEntityRoot { get; set; }

        // all locations, outputs etc. should be frozen/constant. Otherwise, FOW could be defeated which we don't want. So, compute locations during Init
        #region Computed constants

        Vector3? computedProductionSiteLocation, computedCurrentLocation, computedFixedJobLocation;
        float progress;

        bool isCompleted, outputExists;

        #endregion


        public ProcessMemory()
        { }

        public ProcessMemory(SimProcess process) 
        {
            AddToLookup();

            ProcessID = process.ID;
        }

        /// <summary>
        /// it is possible for this to fail, if the Process has problems...
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public bool Init(SimProcess process, SharedKnowledge sharedKnowledge)
        {
            ImmovableTool = process.ImmovableTool;
            ImmovableInput = process.ImmovableInput;

            ActingOnEntity = process.ActingOnEntity;
           // ActingOnEntityRoot = process.ActingOnEntityRoot;

            GroundLocation = process.GroundLocation;
            MapPosition = process.MapPosition;
            IsStarted = process.IsStarted;
            ProcessType = process.ProcessType;
            UpgradeCategory = process.UpgradeCategory;

            StationaryTools = process.StationaryTools;
                     
            if (process.OutputEntities != null)
            {
                OutputEntities = new List<EntityID>(process.OutputEntities);
            }

            if (process.AssignedInputs != null)
            {
                AssignedInputs = new Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>>();
                // deep copy:
                foreach (var item in process.AssignedInputs)
                {
                    AssignedInputs.Add(item.Key, new List<Tuple<EntityID, WorldLocation>>(item.Value));
                }
            }


            ProgressSpeed = process.ProgressSpeed;

            if (process.Productivity != null)
            {
                Productivity = new Productivity(process.Productivity);  
            }

            // compute constants:
            // use knowledge since some entities may be hidden even though the SimPorcess is not.
            // NOTE: NewUnknownEntity status will cause these to fail...
            if (!process.GetProductionSiteLocation(out computedProductionSiteLocation, sharedKnowledge))
            {
                return false;
            }

            if (!process.GetCurrentLocation(out computedCurrentLocation, sharedKnowledge))
            {
                return false;
            }

            if (!process.GetFixedJobLocation(out computedFixedJobLocation, sharedKnowledge))
            {
                return false;
            }

            if (!process.IsCompleted(out isCompleted, sharedKnowledge))
            {
                return false;
            }

            if (!process.OutputExists(out outputExists, sharedKnowledge))
            {
                return false;
            }


            if (!process.GetKnownProgress(sharedKnowledge, out progress))
            {
                return false;
            }

            return true;
        }

        


        public bool GetKnownProgress(SharedKnowledge sharedKnowledge, out float progress)
        {
            progress = this.progress;
            return true;
        }


        public bool GetProductionSiteLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge)
        {
            location = computedProductionSiteLocation;
            return true;            
        }


        public bool GetCurrentLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge)
        {
            location = computedCurrentLocation;
            return true;     
        }

        public bool GetFixedJobLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge)
        {
            location = computedFixedJobLocation;
            return true;     
        }

        public bool IsCompleted(out bool isCompleted, SharedKnowledge sharedKnowledge)
        {
            isCompleted = this.isCompleted;
            return true;
        }

        public bool OutputExists(out bool outputExists, SharedKnowledge sharedKnowledge)
        {
            outputExists = this.outputExists;
            return true;
        }

        public bool HasFixedLocation()
        {
            return SimProcess.HasFixedLocation(GroundLocation);
        }


        public void Destroy()
        {
            RemoveIDEntry();

           // The.Map.TileMap[MapPosition.X][MapPosition.Y].RemoveRememberedProcess(sharedKnowledge, this); // handled by caller instead.

        }

        public void SetCompletedProcess(SimProcess process)
        {
            RealProcessIsCompleted = true; // cheat... use this later. 
            Productivity = new Productivity(process.Productivity); // somehow, we know how the process went.
        }


        #region ILookup

        private ProcessMemoryID id = ProcessMemoryID.Invalid;
        static ProcessMemoryID IDCounter = ProcessMemoryID.First;

        public ProcessMemoryID ID
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

        public ProcessMemoryID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ProcessMemoryID.Max)
            {
                throw new Exception("Astounding, ProcessMemoryID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public ProcessMemoryID SnapshotID(Snapshotter sn, ProcessMemoryID id)
        {
            return (ProcessMemoryID)sn.DoEnum(id);
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
            if (ID != ProcessMemoryID.Invalid)
                LookUp<ProcessMemory, ProcessMemoryID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = ProcessMemoryID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<ProcessMemory, ProcessMemoryID>.Remove(this);
        }

        void ILookUp<ProcessMemory, ProcessMemoryID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = ProcessMemoryID.First;
        }

        void ILookUp<ProcessMemory, ProcessMemoryID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ProcessMemory, ProcessMemoryID>.Create();
        }

        #endregion


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
            IDCounter = sn.DoEnum(IDCounter);
            id = SnapshotID(sn, id);

            ProcessID = sn.DoEnum(ProcessID);
            ProcessType = sn.DoGameData(ProcessType);
            ImmovableTool = sn.DoEnumNullable(ImmovableTool);
            ImmovableInput = sn.DoEnumNullable(ImmovableInput);
            ContainerToPlaceOutputsIn = sn.DoEnumNullable(ContainerToPlaceOutputsIn);
            ActingOnEntity = sn.DoEntityAndRootNullable(ActingOnEntity);
           /* ActingOnEntity = sn.DoEnumNullable(ActingOnEntity);
            ActingOnEntityRoot = sn.DoEnumNullable(ActingOnEntityRoot);
            */
            GroundLocation = sn.DoVector3Nullable(GroundLocation);
            IsStarted = sn.DoBool(IsStarted);
            RealProcessIsCompleted = sn.DoBool(RealProcessIsCompleted);

            OutputEntities = sn.DoList(OutputEntities);
            AssignedInputs = sn.DoMultiMap(AssignedInputs);

            StationaryTools = sn.DoList(StationaryTools);

            MapPosition = sn.DoPointNullable(MapPosition);

            UpgradeCategory = sn.DoGameData(UpgradeCategory);

            computedCurrentLocation = sn.DoVector3Nullable(computedCurrentLocation);
            computedFixedJobLocation = sn.DoVector3Nullable(computedFixedJobLocation);
            computedProductionSiteLocation = sn.DoVector3Nullable(computedProductionSiteLocation);

            isCompleted = sn.DoBool(isCompleted);
            outputExists = sn.DoBool(outputExists);

            progress = sn.DoFloat(progress);

            ProgressSpeed = sn.DoFloat(ProgressSpeed);

            Productivity = (Productivity)sn.DoISnapshot(Productivity);


            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);    
        }

        #endregion


       
    }
}
