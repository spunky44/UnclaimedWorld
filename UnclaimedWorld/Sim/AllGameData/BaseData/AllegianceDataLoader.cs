using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData
{
    public class AllegianceDataLoader // only add allegiances that are shared between scenarios here. otherwise create them in-line.
    {
        public static List<AllegianceData> Init()
        {
            List<AllegianceData> list = new List<AllegianceData>();

              
            #region "farming site"
            list.Add(new AllegianceData()
            {
                KeyName = "farmingAllegiance",               
                AllegianceType = Allegiances.AllegianceType.Other,
                EntityType = "entity:human",
                StatsData = new StatsData()
                {
                    RandomComfort = new NormalDistribution() { Min = 0.15f, Max = 0.35f },
                    RandomFood = new NormalDistribution() { Min = 0.15f, Max = 0.35f },
                    RandomSecurity = new NormalDistribution() { Min = 0.15f, Max = 0.35f }
                },
                AllegianceTemplates = new[]
                {
                    new StringChance() { Edge = 1f, String = "farmingAllegianceTemplate"  }
                }                
            });
            #endregion

            #region "mining site"
            list.Add(new AllegianceData()
            {
                KeyName = "miningAllegiance",
                AllegianceType = Allegiances.AllegianceType.Other,
                EntityType = "entity:human",
                StatsData = new StatsData()
                {
                    RandomComfort = new NormalDistribution() { Min = 0.15f, Max = 0.35f },
                    RandomFood = new NormalDistribution() { Min = 0.15f, Max = 0.35f },
                    RandomSecurity = new NormalDistribution() { Min = 0.15f, Max = 0.35f }
                },
                AllegianceTemplates = new[]
                {
                    new StringChance() { Edge = 1f, String = "miningAllegianceTemplate"  }
                }
            });
            #endregion

            #region "fishing site"
            list.Add(new AllegianceData()
            {
                KeyName = "fishingAllegiance",
                AllegianceType = Allegiances.AllegianceType.Other,
                EntityType = "entity:human",
                StatsData = new StatsData()
                {
                    RandomComfort = new NormalDistribution() { Min = 0.15f, Max = 0.35f },
                    RandomFood = new NormalDistribution() { Min = 0.15f, Max = 0.35f },
                    RandomSecurity = new NormalDistribution() { Min = 0.15f, Max = 0.35f }
                },
                AllegianceTemplates = new[]
                {
                    new StringChance() { Edge = 1f, String = "fishingAllegianceTemplate"  }
                }
            });
            #endregion




            #region "Advanced farming site"
            list.Add(new AllegianceData()
            {
                KeyName = "advancedFarmingAllegiance",              
                AllegianceType = Allegiances.AllegianceType.Other,
                EntityType = "entity:human",
                StatsData = new StatsData()
                {
                    RandomComfort = new NormalDistribution() { Min = 0.55f, Max = 0.95f },
                    RandomFood = new NormalDistribution() { Min = 0.35f, Max = 0.55f },
                    RandomSecurity = new NormalDistribution() { Min = 0.65f, Max = 0.85f }
                },
                AllegianceTemplates = new[]
                {
                    new StringChance() { Edge = 1f, String = "advancedFarmingAllegianceTemplate"  }
                }
            });
            #endregion

            #region "Advanced mining site"
            list.Add(new AllegianceData()
            {
                KeyName = "advancedMiningAllegiance",         
                AllegianceType = Allegiances.AllegianceType.Other,
                EntityType = "entity:human",
                StatsData = new StatsData()
                {
                    RandomComfort = new NormalDistribution() { Min = 0.55f, Max = 0.95f },
                    RandomFood = new NormalDistribution() { Min = 0.35f, Max = 0.55f },
                    RandomSecurity = new NormalDistribution() { Min = 0.65f, Max = 0.85f }
                },
                AllegianceTemplates = new[]
                {
                    new StringChance() { Edge = 1f, String = "advancedMiningAllegianceTemplate"  }
                }
            });
            #endregion

            #region "Advanced fishing site"
            list.Add(new AllegianceData()
            {
                KeyName = "advancedFishingAllegiance",         
                AllegianceType = Allegiances.AllegianceType.Other,
                EntityType = "entity:human",
                StatsData = new StatsData()
                {
                    RandomComfort = new NormalDistribution() { Min = 0.55f, Max = 0.95f },
                    RandomFood = new NormalDistribution() { Min = 0.35f, Max = 0.55f },
                    RandomSecurity = new NormalDistribution() { Min = 0.65f, Max = 0.85f }
                },
                AllegianceTemplates = new[]
                {
                    new StringChance() { Edge = 1f, String = "advancedFishingAllegianceTemplate"  }
                }
            });
            #endregion






            #region Destiny river delta
            list.Add(
               new AllegianceData()
                 {
                       Name = "Starsnare Point",
                       KeyName = "destinyRiverDeltaDescentAllegiance",
                       EntityType = "entity:human",
                       AllegianceType = Allegiances.AllegianceType.Other,
                       StatsData = new StatsData()
                       {
                           Security = .49f,
                           Comfort = .43f,
                           FoodSupply = .32f,
                       }
               });
            #endregion

          

            return list;

        }

    }
}
