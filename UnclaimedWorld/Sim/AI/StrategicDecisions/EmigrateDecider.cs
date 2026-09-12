using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.StrategicDecisions;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI.Constants;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.AI.StrategicDecisions
{
    public class EmigrateDecider : StrategyDecider, ISnapshot
    {

        Entity parent;
        EntityID snapshotParent;

        Regulator regulator;

        Allegiance ownAllegiance;


        private bool isWaitingToEmigrate = false;


        /// <summary>
        /// storing the latest ratings makes it possible to compose breakdowns on demand
        /// </summary>
        Dictionary<AllegianceID, AllegianceRatings> CurrentRatingsOfOtherAllegiances; // = new Dictionary<AllegianceID,AllegianceRatings>();

        /// <summary>
        /// Daily chance of migrating. for client display
        /// </summary>
        public float MigrationRisk;

       // public float IntervalChance;

        public AllegianceID? PreferredMigrationTarget;




        public EmigrateDecider(Entity entity)
        {
            parent = entity;
            ownAllegiance = parent.Intelligence.Allegiance;

            CreateRegulators();
        }

        public EmigrateDecider()
        {

        }

        public double? GetUpdateInterval()
        {
            if (IsFirstTimeComputingRatings())
            {
                return 0;
            }
            else 
            {
                return GameData.Instance.AIConstants.EmigrateDeciderUpdateIntervalInSeconds; //.MigrationChecksPerDay;
            }
        }

        private void CreateRegulators()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / GameData.Instance.AIConstants.EmigrateDeciderUpdateIntervalInSeconds, "EmigrateDecider");
        }

        public override void Update(GameTime gameTime)
        {                       
         
            if (IsFirstTimeComputingRatings() || regulator.IsReady())
            {               
                ScoreDesireToEmigrate();

               /* if (The.Sim.TotalUnPausedGameTimeInSeconds > GameData.Instance.Constants.PeriodBeforeFirstEmigrateRollInSeconds)
                {*/
                    RollForChanceToEmigrate();
               // }

                /*
                if(emigrateScore >= desireThreshold)
                    Emigrate();*/
            }
        }


        public AllegianceRatings GetRatings(AllegianceID allegiance)
        {
            AllegianceRatings rating = null;
            if (CurrentRatingsOfOtherAllegiances != null)
            {
                CurrentRatingsOfOtherAllegiances.TryGetValue(allegiance, out rating);
            }

            return rating;            
        }


        public string GetMigrateRiskTooltip()
        {
            string migrateTooltip = null;
            migrateTooltip = "Daily emigration risk: " + Common.PercentageToString(MigrationRisk);
                       
            if (PreferredMigrationTarget.HasValue)
            {
                Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(PreferredMigrationTarget);

                if (allegiance != null)
                {
                    AllegianceRatings rating;
                    rating = parent.Intelligence.GetRatingsForAllegiance(allegiance.ID);

                    migrateTooltip += " \nPreferred migration target:  " + allegiance.Site.Name;

                    migrateTooltip += " \n" + rating.Breakdown;

                }
            }

            return migrateTooltip;
        }


        public bool CanEmigrateProperty(List<Tuple<string, bool>> effectComponents = null)
        {
            bool value = parent.GetEffect(AffectsFlags.CanEmigrate, true, effectComponents: effectComponents);

            return value;
        }

        
        public string GetCanEmigrateToTargetTooltip(Allegiance target) //bool isWithinTargetPopCap) // pop cap is a target allegiance property
        {
            bool typeCanEmigrate = true, isOnlyMember = false, recentlyJoined = false;
            bool isOverTargetPopCap = true;

            List<Tuple<string, bool>> effectComponents = new List<Tuple<string, bool>>();
            bool result = CanEmigrateToTarget(true, ref typeCanEmigrate, ref isOnlyMember, ref recentlyJoined, ref isOverTargetPopCap, effectComponents, target); // && isWithinTargetPopCap;

            StringBuilder tooltip = new StringBuilder();
            if (result)
            {
                Common.AppendLine(tooltip, "Can emigrate.");
            }
            else
            {
                Common.AppendLine(tooltip, "Cannot emigrate.");                
            }

            Common.AppendLine(tooltip);

            Common.AppendDivider(tooltip);

            // use rephrasing and inversions to avoid negatives:
            Common.AppendLine(tooltip, "A character can only leave if all of the below are " + Common.BoolToString(true, true) + ":"); //mp was: A character can only emigrate if all of the below are
            //Common.AppendLine(tooltip, "True if all of the below are true:");
            Common.AppendLine(tooltip);

            if (!typeCanEmigrate) // hidden by default..
            {
                tooltip.Append("Character type can emigrate: ");
                Common.AppendLine(tooltip, Common.BoolToString(typeCanEmigrate, true));
            }

            if (isOverTargetPopCap) // hidden by default..
            {
                tooltip.Append("The colony has room for more: ");
                Common.AppendLine(tooltip, Common.BoolToString(!isOverTargetPopCap, true));
            }

            tooltip.Append("Our colony has more than one member: "); //mp was:   There is more than one member:
            Common.AppendLine(tooltip, Common.BoolToString(!isOnlyMember, true));
            tooltip.Append("Character joined some time ago: ");  //was: Joined some time ago: 
            Common.AppendLine(tooltip, Common.BoolToString(!recentlyJoined, true));
            foreach (var item in effectComponents)
            {
                tooltip.Append(item.Item1 + ": ");
                Common.AppendLine(tooltip, Common.BoolToString(item.Item2, true));
            }

          

            return tooltip.ToString();
        }

        public bool CanEmigrateToAnyTarget() 
        {
            bool typeCanEmigrate = true, isOnlyMember = false, recentlyJoined = false, isOverTargetPopCap = true;
            //Allegiance target = null;
            return CanEmigrateToTarget(false, ref typeCanEmigrate, ref isOnlyMember, ref recentlyJoined, ref isOverTargetPopCap, null, null);
        }

        public bool CanEmigrateToTarget(Allegiance allegiance)
        {
            bool typeCanEmigrate = true, isOnlyMember = false, recentlyJoined = false, isOverTargetPopCap = true;
            //Allegiance target = null;
            return CanEmigrateToTarget(false, ref typeCanEmigrate, ref isOnlyMember, ref recentlyJoined, ref isOverTargetPopCap, null, allegiance);
        }

        /// <summary>
        /// make this output components for tooltip
        /// </summary>
        /// <returns></returns>
        public bool CanEmigrateToTarget(bool compileTooltip, ref bool typeCanEmigrate, ref bool isOnlyMember, ref bool recentlyJoined, ref bool isOverTargetPopCap, List<Tuple<string, bool>> effectComponents, Allegiance targetAllegiance) 
        {
           // string migrateTooltip = null;
            bool result = true;

            typeCanEmigrate = true;
            isOverTargetPopCap = false;

            if (!parent.Intelligence.CanEmigrate())
            {
                if (compileTooltip)
                {
                    typeCanEmigrate = false;
                    result = false;
                }
                else
                {
                    return false;
                }
            }


            if (parent.Intelligence.Allegiance.Members.Count == 1 && parent.IsOnPlaySite())
            {
                if (compileTooltip)
                {
                    isOnlyMember = true;
                    result = false;
                }
                else
                {
                    return false; // last guy on playsite shouldn't emigrate
                }
            }

            if (parent.Intelligence.Memory.RecentlyJoinedExpedition()) 
            {
                if (compileTooltip)
                {
                    recentlyJoined = true;
                    result = false;
                }
                else
                {
                    return false;
                }
            }

            if (!CanEmigrateProperty(effectComponents))
            {
                if (compileTooltip)
                {                   
                    result = false;
                }
                else
                {
                    return false;
                }
            }

            if (targetAllegiance != null && !targetAllegiance.IsWithinPopulationCap(1))
            {
                if (compileTooltip)
                {
                    isOverTargetPopCap = true;
                    result = false;
                }
                else
                {
                    return false;
                }
            }

            return result; // true;
        }

        /// <summary>
        /// updates the scores for all the allegiances that exist
        /// </summary>
        private void ScoreDesireToEmigrate()//out Allegiance destinationAllegiance)
        {
            if (CurrentRatingsOfOtherAllegiances == null)
            {
                CurrentRatingsOfOtherAllegiances = new Dictionary<AllegianceID, AllegianceRatings>();
            }

            if (!CanEmigrateToAnyTarget())
            {
                PreferredMigrationTarget = null;
                MigrationRisk = 0f;

                return; 
            }

           // double capacityScore;
            float statScore;
          
            /*double inertiaScore;
            double finalEmigrateScore;

            int maxEmigrantCapacity = 1;*/

            Personality personality = parent.PersonEntity.Personality;

            float securityPrinciple;
            float comfortPrinciple;
            float foodPrinciple;

            personality.Principles.TryGetValue(RatingTypes.Security, out securityPrinciple);
            personality.Principles.TryGetValue(RatingTypes.Comfort, out comfortPrinciple);
            personality.Principles.TryGetValue(RatingTypes.Food, out foodPrinciple);


            /*
            * iterate possible sites/allegiances - for each, compute a score
            * create list of combos
       
             * DesireToEmigrate = DeltaRating - Happiness - MigrationInertia - Adaptability + Noise
               The first 2 terms are computed for each rating component (food/sec/comfort).
            * 
            */           
            
            
            // creatures won't have principles, their ratings don't have as many levels because they only have basic needs.

            AllegianceRatings allegianceRating;
            float ownFood, ownSecurity, ownComfort;
            float ownFoodHappiness = 0f, ownSecurityHappiness = 0f, ownComfortHappiness = 0f;
            float otherFood, otherSecurity, otherComfort;
            float foodDiff, securityDiff, comfortDiff;
          //  double foodDeltaHappiness, securityDeltaHappiness, comfortDeltaHappiness; // the change in happiness upon moving
            float foodScore, securityScore, comfortScore;

          //  float happiness = parent.PersonEntity.Personality.Happiness;

            ownFood = parent.Intelligence.Statistics.GetRating(RatingTypes.Food);            
            ownSecurity = parent.Intelligence.Statistics.GetRating(RatingTypes.Security);
            ownComfort = parent.Intelligence.Statistics.GetRating(RatingTypes.Comfort);

            if (parent.Name.Contains("Darzi"))
            {

            }

            if (parent.PersonEntity != null)
            {
                ownFoodHappiness = Personality.ComputeHappinessComponent(personality.Principles[RatingTypes.Food], ownFood);
                ownSecurityHappiness = Personality.ComputeHappinessComponent(personality.Principles[RatingTypes.Security], ownSecurity);
                ownComfortHappiness = Personality.ComputeHappinessComponent(personality.Principles[RatingTypes.Comfort], ownComfort);
            }
            // default is neutral.


            float currentStability = personality.GetCurrentStability(); // fluctuates over time -1 to 1
          //  float adventurousness = personality.Adventurousness - 0.5f; // shift to -0.5 - 0.5 // static

            Migration migration = GameData.Instance.AIConstants.Migration;

            List<AllegianceID> encounteredIDs = null;
            foreach (var site in The.Sim.World.AllSites)
            {
                foreach (Allegiance otherAllegiance in site.Value.Allegiances)
                {
                    if (otherAllegiance != ownAllegiance && otherAllegiance.Statistics != null && IsCompatible(otherAllegiance))
                    {
                        Common.AddToList(ref encounteredIDs, otherAllegiance.ID);

                        if (!CurrentRatingsOfOtherAllegiances.TryGetValue(otherAllegiance.ID, out allegianceRating))
                        {
                            allegianceRating = new AllegianceRatings();
                            allegianceRating.AllegianceID = otherAllegiance.ID;
                            CurrentRatingsOfOtherAllegiances.Add(otherAllegiance.ID, allegianceRating);
                        }

                        otherFood = otherAllegiance.Statistics.GetRating(RatingTypes.Food); 
                        otherSecurity = otherAllegiance.Statistics.GetRating(RatingTypes.Security); 
                        otherComfort = otherAllegiance.Statistics.GetRating(RatingTypes.Comfort); 

                        // save the computations, not just the result.
                        foodDiff = otherFood - ownFood;
                        securityDiff = otherSecurity - ownSecurity;
                        comfortDiff = otherComfort - ownComfort;


                        foodScore = foodDiff - ownFoodHappiness;
                        comfortScore = comfortDiff - ownComfortHappiness;
                        securityScore = securityDiff - ownSecurityHappiness;

                        allegianceRating.SetConditions(RatingTypes.Food, ownFood, otherFood);
                        allegianceRating.SetConditions(RatingTypes.Security, ownSecurity, otherSecurity);
                        allegianceRating.SetConditions(RatingTypes.Comfort, ownComfort, otherComfort);

                        allegianceRating.SetResults(RatingTypes.Food, foodDiff, foodScore);
                        allegianceRating.SetResults(RatingTypes.Security, securityDiff, securityScore);
                        allegianceRating.SetResults(RatingTypes.Comfort, comfortDiff, comfortScore);

                        statScore = (foodScore + securityScore + comfortScore) / 3f;

                        statScore = (1f - migration.WeightOfPersonalTotal) * statScore;

                        allegianceRating.TotalStatScore = statScore;

                        float attraction = 0f;
                        if (personality.Attraction != null)
                        {
                            personality.Attraction.TryGetValue(otherAllegiance.ID, out attraction);
                        }

                       // float personalAddend = migration.WeightOfPersonalAdventurousness * adventurousness + migration.WeightOfPersonalRandom * currentStability;
                        float personalAddend = migration.WeightOfPersonalRandom * currentStability;
                        personalAddend = migration.WeightOfPersonalTotal * personalAddend; 

                        allegianceRating.Personal = personalAddend;
                        allegianceRating.Attraction = attraction;

                        allegianceRating.Desirability = statScore + personalAddend + attraction;      
                    }
                   
                }
            }  
        
            // cleanup:
            var currentRatings = CurrentRatingsOfOtherAllegiances.Keys.ToList();
            foreach (var item in currentRatings) //CurrentRatingsOfOtherAllegiances)
            {
                if (encounteredIDs == null || !encounteredIDs.Contains(item))
                {
                    CurrentRatingsOfOtherAllegiances.Remove(item);
                }
            }

            PreferredMigrationTarget = null;
            MigrationRisk = 0f;

            if (CurrentRatingsOfOtherAllegiances.Count > 0)
            {

                var highScorer = CurrentRatingsOfOtherAllegiances.Where(i => PermitsMigration(i.Key)).OrderByDescending(i => i.Value.Desirability).FirstOrDefault();
                // var highScorer = CurrentRatingsOfOtherAllegiances.Max(r => r.Value.Desirability);

                if (highScorer.Value != null)
                {
                    PreferredMigrationTarget = highScorer.Key;
                    MigrationRisk = ConvertDesirabilityToMigrationChance(highScorer.Value.Desirability);
                }                
                
            }
        }


        private bool IsFirstTimeComputingRatings()
        {
            if (CurrentRatingsOfOtherAllegiances != null) //.Count > 0)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// we want to present a number the player can relate to, such as the risk to migrate per day
        /// also compute the chance for the short interval rolls...
        /// </summary>
        /// <param name="desirability"></param>
        /// <returns></returns>
        private float ConvertDesirabilityToMigrationChance(float desirability)
        {
           
            if (desirability > 0f)
            {
                float maxDailyChance = GameData.Instance.AIConstants.Migration.MaximumMigrateRisk; // 0.5f;
                float minDailyChance = GameData.Instance.AIConstants.Migration.MinimumMigrateRisk;
                float maxDesirability = GameData.Instance.AIConstants.Migration.DesirabilityGivingMaximumMigrateRisk;

                float dailyChance = MathHelper.Lerp(minDailyChance, maxDailyChance, Math.Min(desirability, maxDesirability));
              

              /*  float maxDailyChance = 0.5f;
                float maxDesirability = 1;

                float dailyChance = MathHelper.Lerp(0f, maxDailyChance, Math.Min(desirability, maxDesirability));
                */

                return dailyChance;
            }

            return 0f;
        }



        private float ComputeIntervalChanceFromDailyChance(float dailyChance)
        {
            double intervalsPerDay = DateAndTime.secondsPerDay / GameData.Instance.AIConstants.EmigrateDeciderUpdateIntervalInSeconds; // .MigrationChecksPerDay; // 20; // how many rolls per day

            float intervalChance = (float)(dailyChance / intervalsPerDay);
            //float intervalChance = (float)Math.Pow(dailyChance, 1d / intervalsPerDay);

            return intervalChance;
        }

        private bool IsCompatible(Allegiance allegiance)
        {
            return allegiance.RepresentativeEntityType == parent.Intelligence.Allegiance.RepresentativeEntityType;
        }


        private bool PermitsMigration(AllegianceID allegianceID)
        {
            Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(allegianceID);
            if (allegiance != null
                && allegiance.PermitsImmigration)
            {
                return true;
            }

            return false;

        }

        public static int[] EmigrateRollFrequency; // = new int[1000];


        /// <summary>
        /// roll with regular intervals. The decision to emigrate is then handled by EvaluateEmigrate
        /// </summary>
        private void RollForChanceToEmigrate()
        {
            // when there are multiple destinations to choose from, create a bucket list . Use the highest chance as the upper limit.
            if (!CanEmigrateToAnyTarget())
            {
                return; // last guy on playsite shouldn't emigrate
            }
            
            //make the first roll:
            if (parent.Intelligence.Memory.EmigrateTarget == null
                && PreferredMigrationTarget.HasValue 
                && CurrentRatingsOfOtherAllegiances.Count > 0)
            {
                AllegianceRatings rating = CurrentRatingsOfOtherAllegiances[PreferredMigrationTarget.Value];

                List<AllegianceRatings> listOfRatings = new List<AllegianceRatings>(); // CurrentRatingsOfOtherAllegiances.Values.ToList();
  
                // only roll for targets that allow migration
                foreach (var item in CurrentRatingsOfOtherAllegiances)
                {
                    Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(item.Key);
                    if (allegiance != null
                        && allegiance.PermitsImmigration
                        && allegiance.AllegianceType != Allegiances.AllegianceType.Player)
                    {
                        listOfRatings.Add(item.Value);
                    }
                }

               
                if (listOfRatings.Count > 0)
                {
                    var highScorer = listOfRatings.OrderByDescending(i => i.Desirability).FirstOrDefault();
              

                    float migrationRisk = ConvertDesirabilityToMigrationChance(highScorer.Desirability);

                    if (Common.IsGreaterThan(migrationRisk, 0f))
                    {
                        float intervalChance = ComputeIntervalChanceFromDailyChance(migrationRisk);

                        double roll = The.Sim.GameplayRandomGenerator.NextDouble("emigrateDecider");

                        /*
#if DEBUG
                        Console.WriteLine(string.Format("{4:N1} Roll to emigrate for {0}, roll: {1} / {2} {3}", parent, 
                           // Common.PercentageToString(roll), Common.PercentageToString(intervalChance), 
                            roll, intervalChance,
                            roll <= intervalChance? "SUCCESS" : "", The.Sim.TotalUnPausedGameTimeInSeconds));

#endif
                        */


                        if (roll <= intervalChance)
                        {
                            float totalScore;
                            Common.BuildEdgesFromBucketSizes(listOfRatings, false, out totalScore);

                            int index;
                            AllegianceRatings selected = Common.GetStairStepIndex(listOfRatings, out index, The.Sim.GameplayRandomGenerator, totalScore);


                            if (parent.IsOnPlaySite())
                            {
                                // emigrate when the evaluator is ready
                                parent.Intelligence.Memory.SetEmigrateDecision(selected.AllegianceID, parent);
                            }
                            else
                            {
                                // emigrate now?

                            }

                        }
                    }
                }
            }

        }

        private void Emigrate()
        {
            ownAllegiance.OtherSiteAllegianceManager.AddEmigrantToQueue(parent.EntityID);
            isWaitingToEmigrate = true;
        }

       
        public bool HasDesireToEmigrate(Allegiance toAllegiance)
        {
            AllegianceRatings rating = GetRatings(toAllegiance.ID);
            if (rating != null && rating.Desirability > 0)
            {
                return true;
            }

            return false;
        }

       


        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            snapshotParent = (EntityID)sn.SnapshotID<Entity, EntityID>(parent);
            isWaitingToEmigrate = sn.DoBool(isWaitingToEmigrate);

            this.CurrentRatingsOfOtherAllegiances = sn.DoDictionary(CurrentRatingsOfOtherAllegiances);

            PreferredMigrationTarget = sn.DoEnumNullable(PreferredMigrationTarget);
            MigrationRisk = sn.DoFloat(MigrationRisk);

            sn.Ignore(ownAllegiance);
            sn.Ignore(parent);
            sn.Ignore(EmigrateRollFrequency);

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

            parent = Entity.FindByID(snapshotParent);
            ownAllegiance = parent.Intelligence.Allegiance;

            CreateRegulators();
        }


        #endregion
    }



}
