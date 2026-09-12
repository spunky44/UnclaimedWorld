using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Needs;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;

namespace UWGame.SimSide.Allegiances.Statistics
{
   
    /// <summary>
    /// tracks production and consumption of owned items
    /// no polled data, event driven instead.
    /// 
    /// Data can be shown as a graph, or summarized in the ledger
    /// 
    /// To prevent running out of memory: only one year back, only for the player?
    /// 
    /// seen events:
    /// production of an item
    /// consumption of an item  
    /// 
    /// loss of an item:
    /// degradation of an item
    /// item eaten by creatures
    ///  
    /// what about unseen production?
    /// add an event when seen?
    /// 
    /// unseen events?
    /// unknown items
    /// </summary>
    public class NutrientStatistics : ISnapshot
    {
        public enum StatTypes { Produced, Consumed, Overconsumed }

        /// <summary>
        /// float value: items produced this frame/timepoint
        /// 
        /// to show these in a graph, they would have to be divided into larger time intervals and summed up. Like 1 day intervals
        /// </summary>
        public Dictionary<StatTypes, Dictionary<FoodNutrientType, List<DataPoint<float>>>> Stats = new Dictionary<StatTypes, Dictionary<FoodNutrientType, List<DataPoint<float>>>>();

      

        public NutrientStatistics() //EntityType representativeEntityType)
        {
          //  System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");

            if (!Snapshotter.IsSnapshotting)
            {              
                foreach (var item in Enum.GetValues(typeof(StatTypes))) //GameData.Instance.AllFoodNutrientTypes)  //Enum.GetValues(typeof(StatTypes)))
	            {
                    Stats.Add((StatTypes)item,
                        new Dictionary<FoodNutrientType, List<DataPoint<float>>>());
	            }             
            }

    
        }


        public void AddEvent(StatTypes eventType, FoodNutrientType nutrientType, float amount)
        {
            Dictionary<FoodNutrientType, List<DataPoint<float>>> group = Stats[eventType];

            List<DataPoint<float>> itemGroup;
            if (!group.TryGetValue(nutrientType, out itemGroup))
            {
                itemGroup = new List<DataPoint<float>>();
                group.Add(nutrientType, itemGroup);

            }

            float amountValue = amount;

            // see if data was logged already this frame... then delete that point first:
            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;
            if (itemGroup.Count > 0)
            {
                DataPoint<float> last = itemGroup.Last();
                if (Common.IsEqual(last.Time.TotalDays, now.TotalDays))
                {
                    amountValue += last.Value;
                    itemGroup.RemoveAt(itemGroup.Count - 1);
                }
            }

            itemGroup.Add(new DataPoint<float>(amountValue, now));

          //  DateAndTime.TimeDateYear oldestDateToKeep = 
            now.AddTime(-GameData.Instance.GUIConstants.TimeInDaysToKeepStatistics);
            Statistic.DiscardOldData(itemGroup, now);             

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
            this.Stats = sn.DoNestedMultiMap(Stats);
           

            return this;
        }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


        }

      
        #endregion
    }
}
