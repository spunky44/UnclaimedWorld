using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Needs;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.Steam;
using UWGame.SimSide.Processes;

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
    /// what about unseen production? can't happen. processes never start unseen.  
    /// 
    /// unseen events?
    /// unknown items
    /// </summary>
    public class ProductionStatistics : ISnapshot
    {
        public enum StatTypes { Produced, ConsumedFood, UsedAsInput, Degraded, EatenByCreatures, Disappeared/*, ToolProductivity, SkillProductivity, EnergyProductivity, TotalProductivity*/ }

        /// <summary>
        /// float value: items produced this frame/timepoint
        /// 
        /// to show these in a graph, they would have to be divided into larger time intervals and summed up. Like 1 day intervals
        /// </summary>
        public Dictionary<StatTypes, Dictionary<EntityType, List<DataPoint<float>>>> Stats = new Dictionary<StatTypes,Dictionary<EntityType, List<DataPoint<float>>>>();

        /// <summary>
        /// used for achievements
        /// </summary>
        public Dictionary<StatTypes, Dictionary<EntityType, int>> Totals = new Dictionary<StatTypes, Dictionary<EntityType, int>>();


        /// <summary>
        /// productivity stats. entries during the same frame are replaced by a single average
        /// </summary>
        public Dictionary<EntityType, List<DataPoint<Productivity>>> ProductivityStats = new Dictionary<EntityType, List<DataPoint<Productivity>>>();


       // public Dictionary<StatTypes, Dictionary<EntityType, int>> TotalProductivity = new Dictionary<StatTypes, Dictionary<EntityType, int>>();


        /// <summary>
        /// for iterating
        /// </summary>
        public HashSet<EntityType> TypesWithStats = new HashSet<EntityType>();
        

        public ProductionStatistics()
        {
          //  System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");

            if (!Snapshotter.IsSnapshotting)
            {
                foreach (var item in Enum.GetValues(typeof(StatTypes)))
	            {
                    Stats.Add((StatTypes)item, 
                        new Dictionary<EntityType, List<DataPoint<float>>>());

                    Totals.Add((StatTypes)item,
                        new Dictionary<EntityType, int>());
	            }             
            }
    
        }

      
        public static bool CountMemberConsumption(Entity entity)
        {
            return entity.Intelligence.IsIndependent();
        }

        private bool AddProductivityTimelineEvent(EntityType entityType, Productivity amount)
        {
           /* Dictionary<EntityType, List<DataPoint<Productivity>>> group;
            if (ProductivityStats.TryGetValue(eventType, out group))
            {*/
                List<DataPoint<Productivity>> itemGroup;
                if (!ProductivityStats.TryGetValue(entityType, out itemGroup))
                {
                    itemGroup = new List<DataPoint<Productivity>>();
                    ProductivityStats.Add(entityType, itemGroup);

                    TypesWithStats.Add(entityType);
                }

               // float amountValue = amount;

                // see if data was logged already this frame... then create an average
                DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;
                if (itemGroup.Count > 0)
                {
                    DataPoint<Productivity> last = itemGroup.Last();
                    if (Common.IsEqual(last.Time.TotalDays, now.TotalDays))
                    {
                        amount.Merge(last.Value);

                        itemGroup.RemoveAt(itemGroup.Count - 1);
                    }
                }

                itemGroup.Add(new DataPoint<Productivity>(amount, now));

                // cleanup oldest data
                now.AddTime(-GameData.Instance.GUIConstants.TimeInDaysToKeepStatistics);
                Statistic.DiscardOldData(itemGroup, now);

                return true;
          //  }

          //  return false;
        }


        

        private bool AddTimelineEvent(StatTypes eventType, EntityType entityType, float amount)
        {
            Dictionary<EntityType, List<DataPoint<float>>> group;
            if (Stats.TryGetValue(eventType, out group))
            {
                List<DataPoint<float>> itemGroup;
                if (!group.TryGetValue(entityType, out itemGroup))
                {
                    itemGroup = new List<DataPoint<float>>();
                    group.Add(entityType, itemGroup);

                    TypesWithStats.Add(entityType);
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

                // cleanup oldest data (we keep the totals):
                now.AddTime(-GameData.Instance.GUIConstants.TimeInDaysToKeepStatistics);
                Statistic.DiscardOldData(itemGroup, now);

                return true;
            }

            return false;
        }

        public static void GetMeanOfDataPoints(Dictionary<EntityType, List<DataPoint<Productivity>>> dict, EntityType entityType, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to,
            out float? totalProductivity, out float? toolProductivity, out float? skillProductivity, out float? energyProductivity)
        {
            List<DataPoint<Productivity>> data;

            if (dict.TryGetValue(entityType, out data))
            {

                List<DataPoint<Productivity>> list = Statistic.GetDataPointsBetween(data, from, to);

                if (list.Count > 0)
                {
                    totalProductivity = list.Average(d => d.Value.TotalProductivity);
                    skillProductivity = list.Average(d => d.Value.SkillProductivity);
                    toolProductivity = list.Average(d => d.Value.ToolProductivity);
                    energyProductivity = list.Average(d => d.Value.EnergyProductivity);

                    return;
                }
            }
         
            totalProductivity = null;
            toolProductivity = null;
            skillProductivity = null;
            energyProductivity = null;

            //return 0f;
        }


        public void AddProductivityEvent(EntityType entityType, IKnownProcess processData) // ProductionStatistics.StatTypes statType, int amount)
        {
            AddProductivityTimelineEvent(entityType, processData.Productivity);

        }

        public void AddEvent(StatTypes eventType, EntityType entityType, int amount)
        {            
            if (AddTimelineEvent(eventType, entityType, amount))
            {
                // store the totals so we can clean...
                var totals = Totals[eventType];
                int sum;
                Common.AddToDictWithSums(totals, entityType, amount, out sum);

                CheckAchievements(eventType, entityType, sum);
            }

            /*
            Dictionary<EntityType, List<DataPoint<float>>> group;
            if (Stats.TryGetValue(eventType, out group))
            {               
                List<DataPoint<float>> itemGroup;
                if (!group.TryGetValue(entityType, out itemGroup))
                {
                    itemGroup = new List<DataPoint<float>>();
                    group.Add(entityType, itemGroup);

                    TypesWithStats.Add(entityType);
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




                // store the totals so we can clean...
                var totals = Totals[eventType];
                int sum;
                Common.AddToDictWithSums(totals, entityType, amount, out sum);


                // cleanup oldest data (we keep the totals):
                now.AddTime(-GameData.Instance.GUIConstants.TimeInDaysToKeepStatistics);
                Statistic.DiscardOldData(itemGroup, now);

                CheckAchievements(eventType, entityType, sum);
            }*/           
        }
             

       

        private void CheckAchievements(StatTypes eventType, EntityType entityType, int sum)
        {
            StartScenarioParams parms = The.Sim.StartGameParams.StartScenarioParams;
            if (parms != null
                && parms.Scenario.Source == Scenarios.Source.RefactoredGames)
            {
                string name = parms.ScenarioName;

                if (eventType == StatTypes.Produced)
                {

                    if (The.Sim.StartGameParams.GetRGScenario() == StartGameParams.RGScenario.FieldsOfTauCeti)
                    {
                        /*
                         Produce 50 thunder chicken tanned hides, 40 whipjaw tanned hides, 20 megapod tanned hides
                         */
                        if (!The.Sim.Controller.StatsAndAchievements.IsAchievementUnlocked(AchievementID.hideProducer))
                        {
                            if (The.Sim.GetDifficultyKey() == "hard"
                             || The.Sim.GetDifficultyKey() == "normal")
                            {
                                if (entityType.KeyName == "item:whipjawTannedHide"
                                    || entityType.KeyName == "item:megapodTannedHide"
                                    || entityType.KeyName == "item:thunderChickenTannedHide")
                                {
                                    var totals = Totals[eventType];

                                    int whipjawHides;
                                    if (totals.TryGetValue(GameData.Instance.AllEntityTypes["item:whipjawTannedHide"], out whipjawHides))
                                    {
                                        if (whipjawHides >= 40)
                                        {
                                            int megapodHides;
                                            if (totals.TryGetValue(GameData.Instance.AllEntityTypes["item:megapodTannedHide"], out megapodHides))
                                            {
                                                if (megapodHides >= 20)
                                                {
                                                    int chickenHides;
                                                    if (totals.TryGetValue(GameData.Instance.AllEntityTypes["item:thunderChickenTannedHide"], out chickenHides))
                                                    {
                                                        if (chickenHides >= 50)
                                                        {
                                                            The.Sim.Controller.StatsAndAchievements.UnlockAchievement(AchievementID.hideProducer);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }

                        if (!The.Sim.Controller.StatsAndAchievements.IsAchievementUnlocked(AchievementID.gunsProducer))
                        {
                            // Produce 10 blackpowder rifles and 20 blackpowder ammo
                            if (The.Sim.GetDifficultyKey() == "hard"
                             || The.Sim.GetDifficultyKey() == "normal")
                            {
                                if (entityType.KeyName == "item:gunpowderRifle"
                                    || entityType.KeyName == "item:blackPowderRifleAmmo")
                                {
                                    var totals = Totals[eventType];

                                    int rifles;
                                    if (totals.TryGetValue(GameData.Instance.AllEntityTypes["item:gunpowderRifle"], out rifles))
                                    {
                                        if (rifles >= 10)
                                        {
                                            int ammo;
                                            if (totals.TryGetValue(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"], out ammo))
                                            {
                                                if (ammo >= 20)
                                                {
                                                    The.Sim.Controller.StatsAndAchievements.UnlockAchievement(AchievementID.gunsProducer);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
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
            this.TypesWithStats = sn.DoHashSet(TypesWithStats);
            this.Totals = sn.DoNestedDictionary(Totals);

           
            return this;
        }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


        }

      
        #endregion
    }
}
