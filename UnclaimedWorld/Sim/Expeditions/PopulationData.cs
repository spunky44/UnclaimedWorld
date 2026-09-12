using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.Expeditions
{
    /// <summary>
    /// randomize more..?
    /// </summary>
    public class PopulationData
    {
        public int StartMembers;

      
        public int MaxMembers;

        /// <summary>
        /// exponential growth
        /// </summary>
        public float? GrowthInPercentagePerDay;

        /// <summary>
        /// linear growth
        /// </summary>
        public float? GrowthInMembersPerDay;


        /// <summary>
        /// edges/buckets are allowed.
        /// </summary>
        public StringChance[] RandomMembers;

        /// <summary>
        /// if not filled, RandomMembers will be used to fill the starting pop.
        /// </summary>
        public string[] StartMembersList;


        /// <summary>
        /// DefaultSpawnRadius is default
        /// </summary>
        public float? SpawnRadius;

        /// <summary>
        /// NAMES of nests.
        /// spawns can only happen at these locations, and only if the entities exist
        /// </summary>
        public string[] SpawnSources;

        /// <summary>
        /// entityData KEYS
        /// in order to spawn starting members, some spawn sources may be required. Add them here if needed
        /// </summary>
        public string[] StartSpawnSources;


        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (StartSpawnSources != null)
            {
                foreach (var item in StartSpawnSources)
                {
                    EntityData entityData;
                    EntityType.ValidateGameDataTypeExists(ref listOfErrors, item, GameData.Instance.AllEntityData, out entityData);
                }
            }
        }
    }
}
