using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data
{
    public class EntityDataLoader
    {
        public static List<EntityData> Init()
        {
            List<EntityData> list = new List<EntityData>();

            #region Patricians #1

            list.Add(new EntityData()
            {
                KeyName = "patrician#1",
                EntityKey = "entity:patrician",           
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.YoungAdult }
            });

            list.Add(new EntityData()
           {
               KeyName = "patrician#2",
               EntityKey = "entity:patrician",
            //   Location = new Vector3(768, 432, 0),
               BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }
           });

            
            #endregion

            #region Mud Worm #1 //these are pre-spawned mud worms. others will spawn by trigger zones

            list.Add(new EntityData()
            {
                KeyName = "mudWorm#1",
                EntityKey = "entity:mudWorm",
                //Location = new Vector3(1300, 728, 0),
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }
            });

           /* list.Add(new EntityData()
            {
                KeyName = "mudWorm#2",
                EntityKey = "entity:mudWorm",//               
                Location = new Vector3(1320, 748, 0),
                Bulk = 0.28f,
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeInYears = 5 }
            });

            list.Add(new EntityData()
            {
                KeyName = "mudWorm#3",
                EntityKey = "entity:mudWorm",//                
                Location = new Vector3(1340, 768, 0),
                Bulk = 0.28f,
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeInYears = 5 }
            });*/

          
            #endregion

            #region Thunder Chicken #2

            list.Add(new EntityData()
            {
                KeyName = "studdedThunderChicken#1",
                EntityKey = "entity:studdedThunderChicken",
                Location = new Vector3(3024, 432, 0),  //at map edge, East
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }
            });

            list.Add(new EntityData()
            {
                KeyName = "studdedThunderChicken#2",
                EntityKey = "entity:studdedThunderChicken", 
                Location = new Vector3(3024, 582, 0), //at map edge, East
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult } 
            });

            list.Add(new EntityData()
            {
                KeyName = "studdedThunderChicken#3",
                EntityKey = "entity:studdedThunderChicken", 
                Location = new Vector3(2736,342, 0), //in the grove
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }
            }); 

            #endregion


            #region Birds
            //at blue creek

            list.Add(new EntityData()
            {
                KeyName = "bird#1",
                EntityKey = "entity:bird",
                Location = new Vector3(1824, 1152, 0),
                Rotation = 100,
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "dark" }
            });

            list.Add(new EntityData()
                {
                    KeyName = "bird#2",
                    EntityKey = "entity:bird",
                    Location = new Vector3(1865, 1172, 0),
                    Rotation = 190,
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "pale" }
                });

            // at south rocky cliff
            list.Add(new EntityData()
                  {
                      KeyName = "bird#3",
                      EntityKey = "entity:bird",
                      Location = new Vector3(1200, 2304, 0),
                      Rotation = 100,
                      BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "black" }
                  });

            //at west rocky cliff
            list.Add(new EntityData()
                {
                    KeyName = "bird#4",
                    EntityKey = "entity:bird",
                    Location = new Vector3(672, 2112, 0),
                    Rotation = 175,
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "red" }
                });
            list.Add(new EntityData()
                 {
                     KeyName = "bird#5",
                     EntityKey = "entity:bird",
                     Location = new Vector3(624, 2134, 0),
                     Rotation = 250,
                     BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "dark" }
                 });

            list.Add(new EntityData()
            {
                KeyName = "bird#6",
                EntityKey = "entity:bird",
                Location = new Vector3(336, 1728, 0),
                Rotation = 290,
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "dark" }
            });
            #endregion


            #region ratspawns
            #region "binalRatAllegiance#1"  //west of camp
            list.Add(new EntityData()
            {
                KeyName = "binalRat#1Allegiance#1",
                EntityKey = "entity:binalRat",            
                Location = new Vector3(626, 1932, 0),
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }
            });

            #endregion

            #region "binalRatAllegiance#2"  // east of camp
            list.Add(new EntityData()
                    {
                        KeyName = "binalRat#1Allegiance#2",
                        EntityKey = "entity:binalRat",                    
                        Location = new Vector3(2076, 1874, 0),
                        BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }
                    });

            list.Add(new EntityData()
                    {
                        KeyName = "binalRat#2Allegiance#2",
                        EntityKey = "entity:binalRat",                   
                        Location = new Vector3(1824, 1364, 0),
                        BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }
                    });

            #endregion
            #region binalRatAllegiance#3
            list.Add(new EntityData()
            {
                KeyName = "binalRat#1Allegiance#3",
                EntityKey = "entity:binalRat",
                Location = new Vector3(1726, 892, 0), //on south shore
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }
            });

            list.Add(new EntityData()
            {
                KeyName = "binalRat#2Allegiance#3",
                EntityKey = "entity:binalRat",
                Location = new Vector3(1252, 1208, 0), //on south shore
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }
            });

            #endregion
            #endregion


            return list;

        }
    }
}
