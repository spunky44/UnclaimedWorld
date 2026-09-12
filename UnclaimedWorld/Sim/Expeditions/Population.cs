using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Expeditions
{
    /// <summary>
    /// the class simulates both immigration and reproduction. So new spawns should be a mix of ages.
    /// Later, the class can be refined...
    /// 
    /// we could require CultureTemplates for all Population instances, but that requires more designer work (to add both an EntityData and a CultureTemplate). 
    /// So this class will per default spawn entities that make sense, (no babies, fewer old, most adult)
    /// </summary>
    public class Population: ISnapshot
    {
        /// <summary>
        /// Required
        /// </summary>
        public int StartMembers;
               
        /// <summary>
        /// Required
        /// </summary>
        public int MaxMembers;

        /// <summary>
        /// Optional. Refers to EntityData keys. EntityData can have CultureTemplate buckets
        /// 
        /// if none are specified, by default a reasonable age will be selected for the representative entity type of the allegiance
        /// </summary>
        public StringChance[] RandomMembers;

        /// <summary>
        /// Optional. These members will always spawn first, then random members to top off.
        /// </summary>
        public string[] StartMembersList;

        /// <summary>
        /// exponential growth
        /// </summary>
        public float? GrowthInPercentagePerDay;

        /// <summary>
        /// linear growth
        /// </summary>
        public float? GrowthInMembersPerDay;


        private float timeInDaysElapsedSinceMemberSpawn;


        private Regulator regulator;


        public Expedition Expedition;

        /// <summary>
        /// Optional.
        /// names of nests.
        /// If defined, spawns can only happen at these locations, and only if the entities exist
        /// </summary>
        public string[] SpawnSources;

        public string[] StartSpawnSources;

        /// <summary>
        /// controls respawn of nests
        /// </summary>
        public float? GrowthInSpawnSourcesPerDay;


        /// <summary>
        /// Optional.
        /// if defined, spawned entities without a location will get a random location inside this radius from the expedition center.
        /// There will be a check so that the point is accessible from the expedition center
        /// </summary>
        public float? SpawnRadius;

        public Population()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                CreateRegulators();
            }
        }

        /// <summary>
        /// Creates a Population from a PopulationData
        /// </summary>
        public static Population CreateFromPopulationData(Expedition exp, PopulationData populationData)
        {
            Population population = new Population();
                      
            population.MaxMembers = populationData.MaxMembers;
            population.StartMembers = populationData.StartMembers;
            population.RandomMembers = populationData.RandomMembers;
            population.StartMembersList = populationData.StartMembersList;
            population.StartSpawnSources = populationData.StartSpawnSources;
            population.GrowthInMembersPerDay = populationData.GrowthInMembersPerDay;
            population.GrowthInPercentagePerDay = populationData.GrowthInPercentagePerDay;
            population.SpawnRadius = populationData.SpawnRadius;
            if (populationData.SpawnSources != null)
            {
                population.SpawnSources = new string[populationData.SpawnSources.Length];
                Array.Copy(populationData.SpawnSources, population.SpawnSources, populationData.SpawnSources.Length);
            }

            if (populationData.StartSpawnSources != null)
            {
                population.StartSpawnSources = new string[populationData.StartSpawnSources.Length]; 
                Array.Copy(populationData.StartSpawnSources, population.StartSpawnSources, populationData.StartSpawnSources.Length);
            }

            population.Expedition = exp;
            
            return population;
        }


        public void SpawnStartingPopulation()
        {
            if (StartSpawnSources != null)
            {
                foreach (var item in StartSpawnSources)
                {
                    SpawnMember(item, true);
                }
            }

            // may depend on StartSpawnSources:
            if (StartMembersList != null)
            {
                foreach (var item in StartMembersList)
                {
                    SpawnMember(item, true);
                }
            }

            if (StartMembers - Expedition.Members.Count > 0) // && RandomMembers != null)
            {
                int amount = StartMembers - Expedition.Members.Count;
                SpawnMembers(ref amount, true);
            }
        }


        bool isWaitingForRegions = false;
        int membersToSpawn;

        public void Update(GameTime gameTime)
        {
            if (isWaitingForRegions)
            {                
                // keep trying...
                if (SpawnMembers(ref membersToSpawn, false) != SpawnResult.Processing)
                {
                    isWaitingForRegions = false;
                }
            }
            else
            {
                double timeSinceLastReady = 0d;
                if (regulator.IsReady(ref timeSinceLastReady))
                {
                    float daysElapsed = (float)((timeSinceLastReady / 1000d) / DateAndTime.secondsPerDay);

                  //  timeInDaysElapsedSinceMemberSpawn += daysElapsed; // does this respawn immediately if the max has been reached for a while??

                    int membersToFill = MaxMembers - Expedition.Members.Count;


                    if (GrowthInMembersPerDay.HasValue)
                    {
                        membersToSpawn = GetNoToSpawn(GrowthInMembersPerDay.Value, daysElapsed, ref timeInDaysElapsedSinceMemberSpawn, membersToFill);

                        if (SpawnMembers(ref membersToSpawn, false) == SpawnResult.Processing)
                        {
                            isWaitingForRegions = true;
                        }

                        /*
                        float linearGrowthInterval = 1f / GrowthInMembersPerDay.Value;

                        if (timeInDaysElapsedSinceMemberSpawn > linearGrowthInterval)
                        {
                            membersToSpawn = (int)(timeInDaysElapsedSinceMemberSpawn / linearGrowthInterval);
                            timeInDaysElapsedSinceMemberSpawn -= membersToSpawn * linearGrowthInterval;

                            membersToSpawn = Math.Min(membersToSpawn, membersToFill);

                            if (SpawnMembers(ref membersToSpawn, false) == SpawnResult.Processing)
                            {
                                isWaitingForRegions = true;
                            }
                        }*/
                    }
                }
            }
        }

        /// <summary>
        /// returns whole number progress/change, either positive or negative.
        /// growth can be + or -
        /// 
        /// progress is only changed if there is amount to fill
        /// 
        /// progress will stay between 0 and 1
        /// </summary>
        /// <param name="growthPerDay"></param>
        /// <param name="daysElapsed"></param>
        /// <param name="progress"></param>
        /// <param name="freeAmount"></param>
        /// <returns></returns>
        public static int GetStepsFromProgress(float growthPerDay, double daysElapsed, ref float progress, int currentAmount, int minAmount, int maxAmount) //int freeAmount)
        {
            int wholeNumbers = 0;

            if ((growthPerDay > 0f && currentAmount < maxAmount)
                || (growthPerDay < 0f && currentAmount > minAmount))
            {
                // only change the progress if there is room to grow/decrease:
               // progress += (float)daysElapsed / growthPerDay;
                progress += (float)daysElapsed * growthPerDay;

                // test if we are moving beyond the 0-1 interval:

                if (growthPerDay > 0f)
                {
                    // increasing.
                    if (progress > 1f)
                    {
                        wholeNumbers = (int)(Math.Floor(progress));

                        progress -= wholeNumbers;
                    }

                    wholeNumbers = Math.Min(wholeNumbers, maxAmount - currentAmount);
                }
                else
                {
                    // decreasing. find the whole steps to decrease by:
                    if (progress < 0f)
                    {
                        wholeNumbers = (int)(Math.Floor(Math.Abs(progress) + 1f));

                        progress += wholeNumbers;
                    }

                    wholeNumbers = Math.Min(wholeNumbers, currentAmount - minAmount);

                    wholeNumbers *= -1;
                }

                System.Diagnostics.Debug.Assert(progress >= 0f && progress <= 1f, "Outside interval...");
                

                /*
                float absProgress = Math.Abs(progress);
                if (absProgress > 1f) 
                {
                    wholeNumbers = (int)(Math.Floor(absProgress));

                    wholeNumbers = Math.Min(wholeNumbers, Math.Abs(freeAmount));

                    wholeNumbers *= Math.Sign(progress);

                    progress -= wholeNumbers;

                  //  membersToSpawn = Math.Min(membersToSpawn, amountToFill);
                }*/
            }

            return wholeNumbers;
        }

        /// <summary>
        /// can this decrease also..? no.
        /// </summary>
        /// <param name="growthPerDay"></param>
        /// <param name="daysElapsed"></param>
        /// <param name="timeInDaysElapsedSinceSpawn"></param>
        /// <param name="amountToFill"></param>
        /// <returns></returns>
        public static int GetNoToSpawn(float growthPerDay, double daysElapsed, ref float timeInDaysElapsedSinceSpawn, int amountToFill)
        {
            int membersToSpawn = 0;

            if (amountToFill > 0)
            {
                timeInDaysElapsedSinceSpawn += (float)daysElapsed; // NEW - only increase unless max cap is reached

                float linearGrowthInterval = 1f / growthPerDay; 

                if (timeInDaysElapsedSinceSpawn > linearGrowthInterval)
                {
                    membersToSpawn = (int)(timeInDaysElapsedSinceSpawn / linearGrowthInterval);
                    timeInDaysElapsedSinceSpawn -= membersToSpawn * linearGrowthInterval;

                    membersToSpawn = Math.Min(membersToSpawn, amountToFill);
                }
            }

            return membersToSpawn;
        }



        enum SpawnResult { Done, Processing, Fail }

        private SpawnResult SpawnMembers(ref int amount, bool ignoreDanger)
        {
            int numberSpawned = 0;
            for (int i = 0; i < amount; i++)
            {
                int index;

                StringChance entityDataKey;

                SpawnResult result;

                if (RandomMembers != null)
                {
                    entityDataKey = Common.GetStairStepIndex(RandomMembers, out index, The.Sim.GameplayRandomGenerator);
                    result = SpawnMember(entityDataKey.String, ignoreDanger);
                }
                else
                {
                    result = SpawnMember(ignoreDanger);
                }

                if (result == SpawnResult.Processing)
                {
                    amount = amount - numberSpawned; // save the number of spawns still to do
                    return SpawnResult.Processing; // continue next update
                }
                else if (result == SpawnResult.Fail)
                {
                    amount = amount - numberSpawned; // save the number of spawns still to do
                    return SpawnResult.Fail;
                }
                else
                {
                    numberSpawned++;
                }
               
            }

            amount = 0;
            return SpawnResult.Done;
        }


        /// <summary>
        /// should avoid threats, if possible. What about human settlements...
        /// </summary>
        /// <returns></returns>
        private SpawnResult FindRandomSpawnLocation(EntityData entityData, bool ignoreDanger, out Vector3? randomLocation)
        {
            randomLocation = null;
            if (entityData.BioEntity != null &&
                SpawnSources != null)
            {
                List<string> shuffled = Common.Randomize(SpawnSources.ToList(), The.Sim.GameplayRandomGenerator);

                foreach (var item in shuffled)
                {
                    EntityID entityID;
                    if (The.Sim.PlaySite.EntitiesByName.TryGetValue(item, out entityID))
                    {
                        Entity source = Entity.FindByID(entityID);
                        if (source != null)
                        {
                            randomLocation = source.AccessPoint.Value;
                            return SpawnResult.Done;
                        }
                    }
                }

                return SpawnResult.Fail;
               
            }
            else
            {

                RegionMap map;

                if (ignoreDanger)
                {
                    map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot].RegionMap;
                }
                else
                {
                    map = Expedition.Allegiance.SharedKnowledge.GetMovementMap(ProtectionLevel.Exposed, GameData.Instance.AllEntityTypes[entityData.EntityKey], ThreatStance.Normal)
                                                                       .Layers[Maps.SurfaceType.TransportType.Foot].RegionMap;
                }

                float spawnRadius = SpawnRadius ?? GameData.Instance.Constants.DefaultSpawnRadius;

                int maxTries = 20;
                // continue until a valid point is found
                int tries = 0;

                while (tries < maxTries)
                {

                    Vector2 randomDirection = new Vector2(Common.RandomBetween(The.Sim.GameplayRandomGenerator, 0f, 1f), Common.RandomBetween(The.Sim.GameplayRandomGenerator, 0f, 1f));
                    randomDirection.Normalize();

                    randomLocation = Expedition.Center + (randomDirection * Common.RandomBetween(The.Sim.GameplayRandomGenerator, 0f, spawnRadius)).ToVector3();

                    randomLocation = The.Map.ClampWorldPosition(randomLocation.Value);

                    float distance = 0f;
                    RegionMap.Result result =
                            map.GetDistance(null, MapManager.WorldPosToSubtile(Expedition.Center.Value), MapManager.WorldPosToSubtile(randomLocation.Value),
                            ref distance, sendMessageToEntity: false, registerIfNotReady: false);

                    tries++;

                    if (result == RegionMap.Result.OK)
                    {
                        return SpawnResult.Done;
                    }
                    else if (result == RegionMap.Result.NoAccess)
                    {
                        continue;
                    }
                    else if (result == RegionMap.Result.Wait)
                    {
                        // wait

                        return SpawnResult.Processing;
                    }
                }


                randomLocation = Expedition.Center; // after X tries, fall back on this.
                return SpawnResult.Done;
            }
        }

      

      //  public AIAgeGroup GetRandomAgeGroup(EntityType entityType)
        /// <summary>
        /// gets a random age, not baby class...
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static float GetRandomAgeAndCasteForMapSpawn(EntityType entityType, ref CasteType caste)
        {
            int index;
            if (caste == null)
            {
                caste = Common.GetStairStepIndexComputeLastEdge(entityType.BiologicalType.Castes, out index, The.Sim.GameplayRandomGenerator);
            }

            float? lowEdge = null, upperEdge = null;
            float? lastEdge = null;
            foreach (var item in caste.AgeGroupTypes)
            {               
                if (lowEdge == null)
                {
                    if (item.AIAgeGroup == AIAgeGroup.Baby)
                    {
                        lowEdge = item.Edge + Common.floatEpsilon;
                    }
                    else
                    {
                        // no baby age group defined.
                        lowEdge = 0f;
                    }
                }

              /*  if (upperEdge == null)
                {
                    if (item.AIAgeGroup == AIAgeGroup.Old)
                    {
                        upperEdge = lastEdge ?? item.Edge;
                        break;
                    }
                }*/

                lastEdge = item.Edge;
            }

            if (upperEdge == null)
            {
                upperEdge = lastEdge;
            }

            float ageToUse = MathHelper.Lerp(lowEdge.Value, upperEdge.Value, (float)The.Sim.GameplayRandomGenerator.NextDouble("Population"));

            return ageToUse;

         //   return Common.GetStairStepIndex(ageToUse, caste.AgeGroupTypes, out index).AIAgeGroup;

            /*caste.AgeGroupTypes
            entityType.BiologicalType.AdultMemberAgeGroup 
            */
        }

        private SpawnResult SpawnMember(bool ignoreDanger)
        {
             // select a default age that makes sense:
            CasteType caste = null;
            float age = GetRandomAgeAndCasteForMapSpawn(Expedition.Allegiance.RepresentativeEntityType, ref caste);

            EntityData entityData = new EntityData()
            {
                EntityKey = Expedition.Allegiance.RepresentativeEntityType.KeyName,
                BioEntity = new Maps.MapEditor.BiologicalEntity()
                {
                     CasteKey = caste.KeyName,
                     AgeInYears = new NormalDistribution() {  Mean = age }
                }
            };

            return SpawnMember(entityData, ignoreDanger);

        }

        /// <summary>
        /// the entity data should preferably contain culturetemplate
        /// </summary>
        /// <param name="entityDataKey"></param>
        /// <param name="ignoreDanger"></param>
        /// <returns></returns>
        private SpawnResult SpawnMember(string entityDataKey, bool ignoreDanger)
        {
            EntityData entityData = GameData.Instance.AllEntityData[entityDataKey];

            return SpawnMember(entityData, ignoreDanger);
        }

        private SpawnResult SpawnMember(EntityData entityData, bool ignoreDanger)
        {
          
            Vector3? locationToUse = null;
            if (entityData.Location == null)
            {
                SpawnResult result = FindRandomSpawnLocation(entityData, ignoreDanger, out locationToUse);
                if (result != SpawnResult.Done)
                {
                    return result;
                }
            }

            if (locationToUse == null)
            {

            }

            // get a reasonable age if only race/caste were filled in
            float? age = null;
            string caste = null;
            if (entityData.BioEntity != null && (entityData.BioEntity.AgeGroup == null && entityData.BioEntity.AgeInYears == null && entityData.BioEntity.CultureTemplates == null))
            {
                EntityType entityType = GameData.Instance.AllEntityTypes[entityData.EntityKey];

                CasteType casteType = null;
                if (entityData.BioEntity.CasteKey != null)
                {
                    casteType = entityType.BiologicalType.Castes.FirstOrDefault(c => c.KeyName == entityData.BioEntity.CasteKey);
                }

                age = GetRandomAgeAndCasteForMapSpawn(entityType, ref casteType);

                caste = casteType.KeyName;
            }

            bool placementFailed;
            Entity spawnedEntity = MapLoader.CreateAndPlaceEntityFromEntityData(entityData, out placementFailed,
                memberOfAllegianceKey: Expedition.Allegiance.KeyName, memberOfExpeditionKey: Expedition.KeyName, locationToUse: locationToUse, age: age, caste: caste);
            

#if DEBUG || PROFILE
            StringBuilder logMessage = new StringBuilder(The.Sim.TotalUnPausedGameTimeInSeconds.ToString());
            logMessage.Append(" ");
            logMessage.Append(entityData.KeyName);
            
            logMessage.Append(placementFailed == true ? "(NO EXEC)" : "");
            string logString = logMessage.ToString();

            // the dev dialog is created after some events.
            if (!Kensei.Dev.Options.AppendPopSpawnText(logString))
            {
                The.Sim.AddStartPopulationSpawnMessage(logString);
            }
#endif

            // smae as in SpawnEntityAction
            if (placementFailed)
            {
                spawnedEntity.Destroy();

                // failReason = "Failed to place entity.";
                // lastSpawnInfo = ComposeInfoString(amount, container, spawnLocation);

                // return false;
            }

            return SpawnResult.Done;
        }

        private void CreateRegulators()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.2, "Expedition");
       }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            StartMembers = sn.DoInt32(StartMembers);
            StartMembersList = sn.DoArray(StartMembersList);         
            MaxMembers = sn.DoInt32(MaxMembers);
            timeInDaysElapsedSinceMemberSpawn = sn.DoFloat(timeInDaysElapsedSinceMemberSpawn);
            SpawnRadius = sn.DoFloatNullable(SpawnRadius);
            GrowthInMembersPerDay = sn.DoFloatNullable(GrowthInMembersPerDay);
            GrowthInPercentagePerDay = sn.DoFloatNullable(GrowthInPercentagePerDay);
            GrowthInSpawnSourcesPerDay = sn.DoFloatNullable(GrowthInSpawnSourcesPerDay);

            membersToSpawn = sn.DoInt32(membersToSpawn);
            isWaitingForRegions = sn.DoBool(isWaitingForRegions);
            RandomMembers = sn.DoArray(RandomMembers);
            SpawnSources = sn.DoArray(SpawnSources);
            StartSpawnSources = sn.DoArray(StartSpawnSources);

            sn.Ignore(Expedition);

            return this;
        }



        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (RandomMembers != null)
            {
                foreach (var item in RandomMembers)
                {
                    item.LoadPostProcess(sn);
                }
            }

           
            CreateRegulators();
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

        #endregion
    }
}
