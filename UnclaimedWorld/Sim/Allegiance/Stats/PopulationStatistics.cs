using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Needs;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics
{
   
    /// <summary>
    /// tracks population.
    /// no polled data, event driven instead.
    /// </summary>
    public class PopulationStatistics : ISnapshot
    {
       
        public List<DataPoint<float>> Population = new List<DataPoint<float>>();

        /// <summary>
        /// used in achievements
        /// #ACHIEVEMENTS
        /// </summary>
        public List<DataPoint<float>> Emigration = new List<DataPoint<float>>();        
        public List<DataPoint<float>> IndependentDeaths = new List<DataPoint<float>>();


        public PopulationStatistics()
        {
          //  System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

      /*  public PopulationStatistics(GroupStatistics parent)           
        {
           
        }*/

        public void RecordDeath()
        {

        }

        public void RecordEmigration()
        {

        }

        public void SetPopulation(int members)
        {
            // see if data was logged already this frame... then delete that point first:
            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;
            if (Population.Count > 0)
            {
                if (Common.IsEqual(Population.Last().Time.TotalDays, now.TotalDays))
                {
                    Population.RemoveAt(Population.Count - 1);
                }
            }

            Population.Add(new DataPoint<float>(members, now));
        }



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
            this.Population = sn.DoList(Population);
            this.Emigration = sn.DoList(Emigration);
            this.IndependentDeaths = sn.DoList(IndependentDeaths);


            return this;
        }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


        }

      
        #endregion
    }
}
