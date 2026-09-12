using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4x.Data
{
    public class EntityDataLoader
    {
        public static List<EntityData> Init()
        {
            List<EntityData> list = new List<EntityData>();

            /*
            #region dogs for trade
            list.Add(new EntityData()
            {
                KeyName = "dog",
                EntityKey = "entity:dog",
                BioEntity = new Maps.MapEditor.BiologicalEntity()
                {
                    AgeInYears = new NormalDistribution() { Mean = 4f },
                    CultureTemplates = new StringChance[] { new StringChance() { Edge = 1f, String = "dogCulture" } }
                },
            });

            #endregion
            */

            #region Leafcutter nest

            list.Add(new EntityData()
            {
                KeyName = "fieldQuaditeNest1",
                EntityKey = "terrain:fieldQuaditeNest",
                Name = "Field quadite nest 1", // referred to in bigBombActivated
                Location = new Vector3(1584, 1296, 0),//
                Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#1" }
            });


            #endregion
           
            #region Leafcutter nest 3

            list.Add(new EntityData()
            {
                KeyName = "fieldQuaditeNest3",
                EntityKey = "terrain:fieldQuaditeNest",
                Name = "Field quadite nest 3", // referred to in bigBombActivated
                Location = new Vector3(3600, 1248, 0),//
                Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#1" }
            });


            #endregion

#region Birds //birds are the only ones defined here because they require racekey

            // at rocky river bed start location: these birds are a bit..purple.
              list.Add(new EntityData()
                { 
                    KeyName = "bird#1", 
                    EntityKey = "entity:bird", 
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(2640, 3312, 0), 
                    Rotation = 100,                      
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  
                });

               list.Add(new EntityData()
               {
                   KeyName = "bird#2",
                   EntityKey = "entity:bird",
                   MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                   Location = new Vector3(2699, 3351, 0),
                   Rotation = 175, 
                   BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  
               });
                                
                list.Add(new EntityData()
                {
                    KeyName = "bird#3",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(2736, 3312, 0),
                    Rotation = 250, 
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  
                });




                //////////////////////////tiny guano birds westernmost cave

                list.Add(new EntityData()
                {
                    KeyName = "bird#4",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(3370, 2199, 0),
                    Rotation = 165,
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" }  
                });

                list.Add(new EntityData()
                {
                    KeyName = "bird#5",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(3408, 2209, 0),
                    Rotation = 195,
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" }
                });

                list.Add(new EntityData()
                {
                    KeyName = "bird#6",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(3395, 2219, 0),
                    Rotation = 145,
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" }
                });
#endregion

            return list;

        }
    }
}
