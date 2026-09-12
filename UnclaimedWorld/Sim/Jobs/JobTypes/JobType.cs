using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Jobs.JobTypes
{
    public enum JobLabelTypes { Blue, Green, Red, Brown, Grey, SteelGrey }

    /// <summary>
    /// used to group jobs together for assigning priorities to them
    ///   
    /// example
    /// process job categories (weeding)
    /// hauling job to storage
    /// scouting jobs
    /// patrol jobs
    /// vermin patrol jobs
    /// harvest job categories/types?     
    /// </summary>
    [XmlInclude(typeof(ProcessJobType))]
    [XmlInclude(typeof(StaticJobType))]
    public abstract class JobType: IGameData
    {
        public string KeyName
        {
            get;
            set;
        }

        public string Name { get; set; }

        public string Comments;

        public bool DeleteRecord
        {
            get;
            set;
        }

        public JobLabelTypes? LabelType;

        public abstract bool IsType(Job job);

        public abstract string GetDefaultDisplayName();

        /// <summary>
        /// this method's purpose is to avoid copying lists of jobs
        /// </summary>
        /// <param name="entityGroup"></param>
        /// <param name="iterateFunction"></param>
        public abstract void IterateJobs(EntityGroup entityGroup, Action<Job> iterateFunction);
       

        public virtual void Initialize()
        {
        }

        public virtual void PreInitValidate(ref List<string> listOfErrors) { }
        public virtual void PostInitValidate(ref List<string> listOfErrors) { }
        public virtual void PostDataCompleteInitialize()
        {
        }
        public virtual void PreDataCompleteValidate(ref List<string> listOfErrors) { }

        public virtual void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }
}
