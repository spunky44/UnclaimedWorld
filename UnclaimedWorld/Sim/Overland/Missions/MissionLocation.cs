using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Overland.Missions.Templates;

namespace UWGame.SimSide.Overland.Missions
{
    public enum MissionStopID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }


    /// <summary>
    /// a stop on a mission and a series of actions to undertake there.
    /// </summary>
    public class MissionStop : ISnapshot, ILookUp<MissionStop, MissionStopID>
    {
        public MissionStopTemplate MissionStopTemplate;
        MissionStopTemplateID snapshotMissionStopTemplate;
        
        public Queue<MissionAction> Actions = new Queue<MissionAction>();       

        public TravelAction TravelAction;

        public Mission mission;



        public MissionStop(Mission mission, MissionStopTemplate locationType)
        {
            AddToLookup();

            this.mission = mission;
            this.MissionStopTemplate = locationType;

            MissionAction action = null;
            if (locationType.Actions != null)
            {
                foreach (var item in locationType.Actions)
                {
                    action = item.CreateMissionAction(mission);

                    Actions.Enqueue(action);
                }
            }

            if (locationType.TravelAction != null)
            {
                this.TravelAction = new TravelAction(mission, locationType.TravelAction);
            }

        }

        public MissionStop()
        {
            
        }

        public void Destroy()
        {
            foreach (var item in Actions)
            {
                item.Destroy();
            }

            if (TravelAction != null)
            {
                TravelAction.Destroy();
            }

            Actions.Clear();

            RemoveIDEntry();
        }

        public void StartMission()
        {
            foreach (var item in Actions)
            {
                item.StartMission();
            }

            if (TravelAction != null)
            {
                TravelAction.StartMission();
            }

        }

        public void Update(GameTime gameTime)
        {
            if (Actions.Count > 0)
            {
                bool isCompleted = Actions.Peek().Update(gameTime);
                if (isCompleted
                    && Actions.Count > 0) // abort will clear the queue...
                {
                    Actions.Dequeue();
                }

            }
            else if (TravelAction != null)
            {
                if (TravelAction.Update(gameTime)) // this will set the next MissionStop as Current when needed
                {
                    TravelAction = null;
                }
            }          

        }

        public bool IsEndLocation()
        {
            return TravelAction == null;
        }

        public MissionStop GetEnd()
        {
            if (IsEndLocation())
            {
                return this;
            }
            else
            {
                return TravelAction.ToMissionStop.GetEnd();
            }

        }


      /*  public void GetOrdersToRefund()
        {
           
        }*/

        public void SetParentPostLoad(Mission parent)
        {
            this.mission = parent;

            if (TravelAction != null)
            {
                TravelAction.SetParentPostLoad(parent);
            }

            foreach (var item in Actions)
	        {
		        item.parent = parent;
            }

        }



        #region ILookup

        private MissionStopID id = MissionStopID.Invalid;
        static MissionStopID IDCounter = MissionStopID.First;

        public MissionStopID ID
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

        public MissionStopID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= MissionStopID.Max)
            {
                throw new Exception("Astounding, MissionStopID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
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
            if (ID != MissionStopID.Invalid)
                LookUp<MissionStop, MissionStopID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = MissionStopID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<MissionStop, MissionStopID>.Remove(this);
        }

        void ILookUp<MissionStop, MissionStopID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = MissionStopID.First;
        }

        void ILookUp<MissionStop, MissionStopID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<MissionStop, MissionStopID>.Create();
        }

        #endregion


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.id = sn.DoEnum(id);
            IDCounter = sn.DoEnum(IDCounter);

            Actions = sn.DoQueue(Actions);

            snapshotMissionStopTemplate = (MissionStopTemplateID)sn.SnapshotID<MissionStopTemplate, MissionStopTemplateID>(MissionStopTemplate);

            this.TravelAction = (TravelAction)sn.DoISnapshot(TravelAction);

            sn.Ignore(mission);

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

            MissionStopTemplate = LookUp<MissionStopTemplate, MissionStopTemplateID>.FindByID(snapshotMissionStopTemplate);

            if (TravelAction != null)
            {
                TravelAction.LoadPostProcess(sn);
            }

            foreach (var item in Actions)
            {
                // item.parent = mission; moved to SetParentPostLoad
                item.LoadPostProcess(sn);
            }

        }


        #endregion

       
    }
}
