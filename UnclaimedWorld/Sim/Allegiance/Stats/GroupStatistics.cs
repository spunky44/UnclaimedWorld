using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.ClientSide.PropertyPresentation;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI.StrategicDecisions;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Items;
using UWGame.Steam;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Allegiances.Statistics
{

    /// <summary>
    /// these stats are one part in computing the ratings which determine the pull effect on migrants
    /// 
    /// Also used in setting importance score on production jobs.
    /// When more expeditions are available, gather production stats per expedition for this purpose.
    /// </summary>
    public class GroupStatistics: ISnapshot
    {
        // static values for designer placed groups etc.:
/*
        /// <summary>
        /// 0 - 1
        /// </summary>
        public float Security; 

        /// <summary>
        /// 0 - 1
        /// </summary>
        public float Comfort;

        /// <summary>
        /// 0 - 1
        /// </summary>
        public float FoodSupply;
        */
       
      /// <summary>
      /// periodically updated
      /// </summary>
        public Dictionary<RatingTypes, Rating> Ratings; 

        /// <summary>
        /// not polled, event driven
        /// </summary>
        public PopulationStatistics PopulationStatistics;


        /// <summary>
        /// event driven. Player only!
        /// </summary>
        public ProductionStatistics ProductionStatistics;

        public KillStatistics KillStatistics;

        public NutrientStatistics NutrientStatistics;

        /// <summary>
        /// used for polling the group's members
        /// </summary>
        public CanIterateEntitiesID CanIterateEntitiesID;


        public EntityType RepresentativeEntityType;


       // private Regulator statRegulator;
        private Regulator achievementsRegulator;

       

        public event Action RatingsChanged;


        public GroupStatistics()
        {
        }

        /// <summary>
        /// for allegiances!
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="representativeEntityType"></param>
        public GroupStatistics(ICanIterateEntities group,  EntityType representativeEntityType)
        {
            this.CanIterateEntitiesID = group.ID;
           // this.OwnedItems = ownerID;
            this.RepresentativeEntityType = representativeEntityType;

            Common.AddToDictionary(ref Ratings, RatingTypes.Food, new FoodStatisticsForAllegiance(this));
            Common.AddToDictionary(ref Ratings, RatingTypes.Comfort, new ComfortStatisticsForAllegiance(this));
            Common.AddToDictionary(ref Ratings, RatingTypes.Security, new SecurityStatisticsForAllegiance(this));

            if (GatherStatisticsForDisplayOnly(group.GetAllegiance))
            {
                PopulationStatistics = new PopulationStatistics();
                ProductionStatistics = new ProductionStatistics();
                NutrientStatistics = new NutrientStatistics();
                KillStatistics = new KillStatistics();
            }

            CreateRegulators();
        }


        /// <summary>
        /// it is too wasteful to gather stats that have no sim function...
        /// </summary>
        /// <param name="allegiance"></param>
        /// <returns></returns>
        public static bool GatherStatisticsForDisplayOnly(Allegiance allegiance)
        {
            return allegiance.AllegianceType == AllegianceType.Player;

        }


        /// <summary>
        /// for non-allegiances!
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="parentAllegiance"></param>
        public GroupStatistics(CanIterateEntitiesID groupID, Allegiance parentAllegiance) // EntityType representativeEntityType)
        {
            this.CanIterateEntitiesID = groupID;
            // this.OwnedItems = ownerID;
            this.RepresentativeEntityType = parentAllegiance.RepresentativeEntityType; // representativeEntityType;

            Common.AddToDictionary(ref Ratings, RatingTypes.Food, new FoodStatisticsForMembers(this, parentAllegiance));
            Common.AddToDictionary(ref Ratings, RatingTypes.Comfort, new ComfortStatisticsForMembers(this, parentAllegiance));
            Common.AddToDictionary(ref Ratings, RatingTypes.Security, new SecurityStatisticsForMembers(this, parentAllegiance));

            CreateRegulators();

        }

        private void CreateRegulators()
        {
           // statRegulator = new Regulator(The.Sim.GameplayRandomGenerator, .5, "Statistic");

            achievementsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d, "GroupStatistics");

        }

        public static GroupStatistics CreateFromStatsData(StatsData statsData, ICanIterateEntities /* CanIterateEntitiesID*/ group, EntityType representativeEntityType)
        {
            GroupStatistics stats = new GroupStatistics(group, representativeEntityType);
            /*
            stats.Security = statsData.Security;
            stats.Comfort = statsData.Comfort;
            stats.FoodSupply = statsData.FoodSupply;
            */

            // store a single static value for other site allegiances:
            AddSharedRating(stats, RatingTypes.Food, statsData.GetRating(RatingTypes.Food));
            AddSharedRating(stats, RatingTypes.Comfort, statsData.GetRating(RatingTypes.Comfort));
            AddSharedRating(stats, RatingTypes.Security, statsData.GetRating(RatingTypes.Security));

            return stats;
        }


        private static void AddSharedRating(GroupStatistics stats, RatingTypes ratingType, float value)
        {
            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;

            Rating rating = stats.Ratings[ratingType]; 

            rating.Ratings.Add(new DataPoint<float>()
            {
                Time = now,
                Value = value
            });

            rating.AddSharedRating(value); 

        }

        public void ChangeAllegiance(Allegiance newAllegiance)
        {
            foreach (var item in Ratings)
            {
                item.Value.ChangeAllegiance(newAllegiance);
            }

        }

       


        /// <summary>
        /// called once by static entities to get constant ratings
        /// </summary>
        public void UpdateOnce()
        {
            foreach (var stat in Ratings)
            {
                stat.Value.ToggleComposeBreakdown(true);

                stat.Value.GatherPolledData();

                stat.Value.ToggleComposeBreakdown(false);
            }
        }


       

        /// <summary>
        /// only player allegiance (and playsite entities?) should get this update.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {                        
            foreach (var stat in Ratings)
            {
                stat.Value.Update(gameTime);
            }

            if (RatingsChanged != null)
            {
                RatingsChanged.Invoke();
            }

           

#if DEBUG
            if (ProductionStatistics != null && ProductionStatistics.Stats.Count == 0)
            {
                throw new Exception();
            }
#endif
        }


        public void CheckAchievements()
        {

            if (achievementsRegulator.IsReady())
            {
                Allegiance allegiance = null;
                ICanIterateEntities canIterate = LookUpICanIterateEntities.FindByID(CanIterateEntitiesID);

                if (canIterate != null)
                {
                    allegiance = canIterate.GetAllegiance;
                }
                else
                {
                    return;
                }

              /*  if (allegiance.AllegianceType != AllegianceType.Player)
                {
                    return;
                }*/

                StatsAndAchievements ach = The.Sim.Controller.StatsAndAchievements;
                StartGameParams.RGScenario scenario = The.Sim.StartGameParams.GetRGScenario();

                /* if (scenario == StartGameParams.RGScenario.MakingHeadway)
                 {
                     if (!ach.IsAchievementUnlocked(AchievementID.headwayRatings))
                     {

                         PropertyResult? comfort = The.Sim.PlaySite.GetPropertyValue("comfortTarget", null);
                         PropertyResult? security = The.Sim.PlaySite.GetPropertyValue("securityTarget", null);
                         PropertyResult? food = The.Sim.PlaySite.GetPropertyValue("foodTarget", null);

                         if (allegiance.IndependentMembers.Count >= 20
                                 && Ratings[RatingTypes.Food].GetLatestValue() >= food.Value.NumberResult // 0.4f
                                 && Ratings[RatingTypes.Comfort].GetLatestValue() >= comfort.Value.NumberResult // 0.4f
                                 && Ratings[RatingTypes.Security].GetLatestValue() >= security.Value.NumberResult // 0.4f
                                 )
                         {
                             ach.UnlockAchievement(AchievementID.headwayRatings);
                         }

                     }
                 }
                 else*/
                if (scenario == StartGameParams.RGScenario.TheClayPit)
                {
                    if (!ach.IsAchievementUnlocked(AchievementID.claypitRatings))
                    {
                        if (allegiance.IndependentMembers.Count >= 15
                                && allegiance.TradeCredits >= 200
                                && Ratings[RatingTypes.Food].GetLatestValue() >= 0.25f
                                && Ratings[RatingTypes.Comfort].GetLatestValue() >= 0.25f
                                && Ratings[RatingTypes.Security].GetLatestValue() >= 0.25f
                                )
                        {
                            ach.UnlockAchievement(AchievementID.claypitRatings);
                        }

                    }

                }
                else if (scenario == StartGameParams.RGScenario.FieldsOfTauCeti)
                {
                    /*
                     - Trader: Get 20 population and ratings of: ? Food ?Sec ?Comf
                        Without establishing any farm plots or fish traps
                     */

                    if (!ach.IsAchievementUnlocked(AchievementID.fieldsOfTauCetiTrader))
                    {
                        if (The.Sim.GetDifficultyKey() == "hard"
                          || The.Sim.GetDifficultyKey() == "normal")
                        {
                            if (allegiance.IndependentMembers.Count >= 12
                                && Ratings[RatingTypes.Food].GetLatestValue() >= 0.35f
                                && Ratings[RatingTypes.Comfort].GetLatestValue() >= 0.35f
                                && Ratings[RatingTypes.Security].GetLatestValue() >= 0.35f
                                )
                            {
                                var produced = this.ProductionStatistics.Totals[Statistics.ProductionStatistics.StatTypes.Produced];

                                if (!HasProduced("structure:smallPlot", produced)
                                    && !HasProduced("structure:largePlot", produced)
                                    && !HasProduced("structure:fishTrapCreekNet", produced)
                                    && !HasProduced("structure:fishTrapCoast", produced)
                                    && !HasProduced("structure:fishTrapShoreHoopNet", produced)
                                    && !HasProduced("structure:fishTrapShoreBasket", produced)
                                    && !HasProduced("structure:greenhouse", produced)
                                    && !HasProduced("structure:improvisedGreenhouse", produced))
                                {
                                    ach.UnlockAchievement(AchievementID.fieldsOfTauCetiTrader);
                                }
                            }
                        }
                    }

                    if (!ach.IsAchievementUnlocked(AchievementID.hunterGatherers))
                    {
                        if (The.Sim.GetDifficultyKey() == "hard")
                        {
                            if (allegiance.IndependentMembers.Count >= 5
                                && The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays - The.Sim.DateAndTime.StartTimeDateYear.TotalDays >= 2d * DateAndTime.DaysPerYear)
                                //&& The.Sim.TotalUnPausedGameTimeInSeconds >= 2d * DateAndTime.secondsPerDay * DateAndTime.DaysPerYear)
                            {
                                bool basicTierUnlocked = false;
                                foreach (var item in allegiance.Expeditions)
                                {
                                    if (item.Policy.TierIsUnlocked(RatingTypes.Security, GameData.Instance.AllTierTypes["basic"])
                                        || item.Policy.TierIsUnlocked(RatingTypes.Food, GameData.Instance.AllTierTypes["basic"])
                                        || item.Policy.TierIsUnlocked(RatingTypes.Comfort, GameData.Instance.AllTierTypes["basic"]))
                                    {
                                        basicTierUnlocked = true;
                                        break;
                                    }
                                }

                                if (!basicTierUnlocked)
                                {
                                    ach.UnlockAchievement(AchievementID.hunterGatherers);
                                }

                            }
                        }

                    }
                }
                else if (scenario == StartGameParams.RGScenario.MuckrootMiningCamp)
                {
                    if (!ach.IsAchievementUnlocked(AchievementID.muckrootNoRefining))
                    {
                        if (allegiance.IndependentMembers.Count >= 15
                                && Ratings[RatingTypes.Food].GetLatestValue() >= 0.45f
                                && Ratings[RatingTypes.Comfort].GetLatestValue() >= 0.45f
                                && Ratings[RatingTypes.Security].GetLatestValue() >= 0.45f
                                )
                        {
                            var produced = this.ProductionStatistics.Totals[Statistics.ProductionStatistics.StatTypes.Produced];

                            if (!HasProduced("item:scandium", produced)
                                && !HasProduced("item:terbium", produced))
                            {
                                ach.UnlockAchievement(AchievementID.muckrootNoRefining);
                            }
                        }
                    }
                    else if (!ach.IsAchievementUnlocked(AchievementID.muckrootMining))
                    {
                        if (allegiance.IndependentMembers.Count >= 20
                                && allegiance.TradeCredits >= 1000
                                && Ratings[RatingTypes.Food].GetLatestValue() >= 0.45f
                                && Ratings[RatingTypes.Comfort].GetLatestValue() >= 0.45f
                                && Ratings[RatingTypes.Security].GetLatestValue() >= 0.45f
                                )
                        {

                            ach.UnlockAchievement(AchievementID.muckrootMining);

                        }
                    }
                }
            }
        }

        private bool HasProduced(string entityTypeKey, Dictionary<EntityType, int> produced)
        {
            EntityType entityType;
            if (GameData.Instance.AllEntityTypes.TryGetValue(entityTypeKey, out entityType))
            {
                int total;
                if (produced.TryGetValue(entityType, out total))
                {
                    return total > 0;
                }
            }

            return false;
        }

        public void NotifyPopulationChanged(int members)
        {
            if (PopulationStatistics != null)
            {
                PopulationStatistics.SetPopulation(members);
            }
        }

        public void AddViolentEvent(Entity victim, string description, ViolentEventType type)
        {           
            Rating stat;
            Ratings.TryGetValue(RatingTypes.Security, out stat);
            SecurityStatistics securityStat = (SecurityStatistics)stat;

            securityStat.AddViolentEvent(victim, description, type);
        }

        public void AddProductionEvent(EntityType entityType, ProductionStatistics.StatTypes statType, int amount)
        {
            if (this.ProductionStatistics != null)
            {
                ProductionStatistics.AddEvent(statType, entityType, amount);
            }
        }

        public void AddProductivityEvent(EntityType entityType, IKnownProcess processData) // ProductionStatistics.StatTypes statType, int amount)
        {
            if (this.ProductionStatistics != null)
            {
                ProductionStatistics.AddProductivityEvent(entityType, processData);
            }
        }

        public void AddNutrientEvent(FoodNutrientType needType, NutrientStatistics.StatTypes statType, float amount)
        {
            if (this.NutrientStatistics != null)
            {
                NutrientStatistics.AddEvent(statType, 
                    needType, amount);
            }
        }


        public void AddKillEvent(EntityType victim)
        {
            if (KillStatistics != null)
            {
                KillStatistics.AddKillEvent(victim);
            }
        }


        /// <summary>
        /// only independent agents need to gather statistics.
        ///  
        /// for player allegiances, we don't want to gather stats for robots, animals etc. Filter them here        
        /// 
        /// this check also avoids animal needs from showing up in Graphs.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool GatherStatisticsForEntity(Entity entity)
        {
            return entity.Intelligence.IsIndependent();
        }
      
       

        public Rating GetStatisticByKey(RatingTypes statType)
        {
            Rating stat;
            Ratings.TryGetValue(statType, out stat);
            return stat;
        }

        public float GetRating(RatingTypes ratingType)
        {
            Rating stat = GetStatisticByKey(ratingType);

            return stat.GetLatestValue();
        }

        

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();

            foreach (Statistic stat in Ratings.Values)
            {
                text.Append(stat.ToString());
                text.Append("\n");
            }

            return text.ToString();
        }


        const float securityWeight = 1f;
        const float comfortWeight = 1f;
        const float foodWeight = 1f;

        /// <summary>
        /// 0 - 1
        /// </summary>
        /// <returns></returns>
        public double GetOverallRating()
        {
            Rating stat;

            int numberOfStats = 3;

            Ratings.TryGetValue(RatingTypes.Food, out stat);
            double foodRating = ((Rating)stat).GetLatestValue();

            Ratings.TryGetValue(RatingTypes.Security, out stat);
            double securityRating = ((Rating)stat).GetLatestValue();

            Ratings.TryGetValue(RatingTypes.Comfort, out stat);
            double comfortRating = ((Rating)stat).GetLatestValue();
           

            double overallRating =
                (securityRating * securityWeight +
                comfortRating * comfortWeight +
                foodRating * foodWeight) / numberOfStats;

            return overallRating;

        }


        internal void RecordDeathOrEmigration(bool isDestroyed)
        {
            if (PopulationStatistics != null)
            {
                if (isDestroyed)
                {
                    PopulationStatistics.RecordDeath();
                }
                else
                {
                    PopulationStatistics.RecordEmigration();
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
          
            this.Ratings = sn.DoDictionary(Ratings);
            this.CanIterateEntitiesID = sn.DoEnum(CanIterateEntitiesID);
     
            this.PopulationStatistics = (PopulationStatistics)sn.DoISnapshot(PopulationStatistics);
            this.ProductionStatistics = (ProductionStatistics)sn.DoISnapshot(ProductionStatistics);
            this.NutrientStatistics = (NutrientStatistics)sn.DoISnapshot(NutrientStatistics);
            this.KillStatistics = (KillStatistics)sn.DoISnapshot(KillStatistics);

            this.RepresentativeEntityType = sn.DoGameData(RepresentativeEntityType);


            sn.Ignore(RatingsChanged);


            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (PopulationStatistics != null)
            {
                PopulationStatistics.LoadPostProcess(sn);
            }

            if (ProductionStatistics != null)
            {
                ProductionStatistics.LoadPostProcess(sn);
            }

            if (NutrientStatistics != null)
            {
                NutrientStatistics.LoadPostProcess(sn);
            }

            if (KillStatistics != null)
            {
                KillStatistics.LoadPostProcess(sn);
            }

            foreach (var item in Ratings)
            {
                item.Value.LoadPostProcess(sn);

                item.Value.Parent = this;
            }

             // 1.0.0.1: added new field RepresentativeEntityType:
           /* if ((int)version < 2)
            {
                if (RepresentativeEntityType == null)
                {

                    ICanIterateEntities group = LookUpICanIterateEntities.FindByID(GroupID);

                    Allegiance allegiance = group as Allegiance;

                    if (allegiance != null) // && allegiance.AllegianceType == AllegianceType.Player)
                    {
                        this.RepresentativeEntityType = allegiance.RepresentativeEntityType; // GameData.Instance.allent
                    }
                    else
                    {
                        group.IterateMembers(e =>
                        {
                            this.RepresentativeEntityType = e.EntityType;
                        });
                    }
                }
            }*/

            CreateRegulators();
        }

        #endregion

       
    }
}

