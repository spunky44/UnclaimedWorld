using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using System.Xml.Serialization;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Overland.Missions.Templates
{
    public enum ActionTypes { Load, Unload, Buy, Sell, Explore, Fish, Travel, Embark, Disembark }

    public enum CargoActionTypes { Load, Unload, Buy, Sell /*, Embark, Disembark*/ }


    public enum MissionActionTemplateID : long
    {
        Invalid = long.MaxValue,
        Max = Invalid,
        First = 1
    }


    /*has to be xmlserializable, since these classes are parts of the CreateMissionTemplate command.*/
    [XmlInclude(typeof(BuySellActionTemplate))]
  //  [XmlInclude(typeof(SellActionTemplate))]    
    [XmlInclude(typeof(LoadActionTemplate))]
    [XmlInclude(typeof(UnloadActionTemplate))]
    [XmlInclude(typeof(TravelActionTemplate))]
    [XmlInclude(typeof(EmbarkActionTemplate))]
    [XmlInclude(typeof(DisembarkActionTemplate))]
    public abstract class MissionActionTemplate : ISnapshot, ILookUp<MissionActionTemplate, MissionActionTemplateID>
    {

        public bool AllowDeleting;

        /// <summary>
        /// The ID will get created manually in the CreateMissionTemplate command when executing.
        /// </summary>
        [XmlIgnore]
        private /*long*/ MissionStopTemplateID snapshotMissionStopTemplateID;


      //  private MissionStopTemplate missionStopTemplate;

        [XmlIgnore]
        public MissionStopTemplate MissionStopTemplate;
       /* {
            get
            {
                if (missionStopTemplate == null)
                {
                    // IDs are invalid until START RUN
                    missionStopTemplate = LookUp<MissionStopTemplate, MissionStopTemplateID>.FindByID((MissionStopTemplateID)MissionStopTemplateID);
                }

                return missionStopTemplate;
            }
        }*/

        public MissionActionTemplate(MissionStopTemplate missionStopTemplate, bool allowDeleting)
        {
            this.AllowDeleting = allowDeleting;

            //this.MissionStopTemplateID = (long)missionStopTemplate.ID; IDs are invalid until START RUN
            this.MissionStopTemplate = missionStopTemplate;
        }

        public MissionActionTemplate()
        {
            // needed for XmlSerializer
        }

        public abstract MissionAction CreateMissionAction(Mission mission);

        public abstract ActionTypes ActionType
        {
            get;
        }
        
       
        public abstract string Name
        {
            get;
        }

        public abstract bool Validate(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors);
        



        public virtual void Destroy()
        {
            RemoveIDEntry();
        }

        /// <summary>
        /// remove the parameter if we decide to keep a permanent reference to the parent (requires an ID).
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public abstract decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost); 


        public abstract float ComputeTotalCargoBulk();


        public static string GetName(ActionTypes action)
        {
            switch (action)
            {
                case ActionTypes.Buy:
                    return "Buy"; // BuyActionTemplate.TemplateName;

                case ActionTypes.Load:
                    return LoadActionTemplate.TemplateName;

                case ActionTypes.Unload:
                    return UnloadActionTemplate.TemplateName;

                case ActionTypes.Embark:
                    return EmbarkActionTemplate.TemplateName;

                case ActionTypes.Disembark:
                    return DisembarkActionTemplate.TemplateName;

                case ActionTypes.Sell:
                    return "Sell"; 
            }

            return null;
        }

        /// <summary>
        /// happens on START RUN or Command.Execute()
        /// </summary>
        public virtual void AssignIDs()
        {           
            AddToLookup();
        }


        public void SetParentID(MissionStopTemplate parent)
        {
            this.MissionStopTemplate = parent;
            //this.MissionStopTemplateID = (long)parentID;
        }


        public static bool ValidateWorkingTerminal(IKnownEntityData terminalData, ref List<string> errors)
        {
            if (!terminalData.IsCompleted()
                || !Entity.IsFunctional(terminalData)) // Common.IsZero(GoalEvaluator.ScoreIsEntityFunctional(terminalData)))
            {
                Common.AddToList(ref errors, "The terminal is not in a working state.");
                return false;
            }

            return true;
        }

        #region ILookup

        private MissionActionTemplateID id = MissionActionTemplateID.Invalid;
        static MissionActionTemplateID IDCounter = MissionActionTemplateID.First;

        /// <summary>
        /// make sure we don't attempt to xmlserialize this. It should not exist before the Command has executed. That way, we can cancel out of the dialog without affecting the Sim
        /// 
        /// The ID will get created manually in the CreateMissionTemplate command when executing.
        /// </summary>
        [XmlIgnore]
        public MissionActionTemplateID ID
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

        public MissionActionTemplateID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= MissionActionTemplateID.Max)
            {
                throw new Exception("Astounding, MissionActionTemplateID just exceeded 64 bits. Something seriously wrong has happened.");
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
            if (ID != MissionActionTemplateID.Invalid)
                LookUp<MissionActionTemplate, MissionActionTemplateID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = MissionActionTemplateID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<MissionActionTemplate, MissionActionTemplateID>.Remove(this);
        }

        void ILookUp<MissionActionTemplate, MissionActionTemplateID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = MissionActionTemplateID.First;
        }

        void ILookUp<MissionActionTemplate, MissionActionTemplateID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<MissionActionTemplate, MissionActionTemplateID>.Create();
        }

        #endregion

        #region ISnapshot

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = sn.DoEnum(id);
            IDCounter = sn.DoEnum(IDCounter);

            //this.MissionStopTemplateID = sn.DoInt64(MissionStopTemplateID);

            this.snapshotMissionStopTemplateID = (MissionStopTemplateID)sn.SnapshotID<MissionStopTemplate, MissionStopTemplateID>(MissionStopTemplate);

            this.AllowDeleting = sn.DoBool(AllowDeleting);

           // sn.Ignore(missionStopTemplate);

            return this;

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        [XmlIgnore]
        public bool IsSnapshotted { get; set; }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            MissionStopTemplate = LookUp<MissionStopTemplate, MissionStopTemplateID>.FindByID(snapshotMissionStopTemplateID);

        }

        #endregion
    }
}
