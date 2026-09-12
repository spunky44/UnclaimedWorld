using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4y.Data
{
    public class EntityDataLoader
    {
        public static List<EntityData> Init()
        {
            List<EntityData> list = new List<EntityData>();

            #region Leafcutter nests

            #region #1
            list.Add(new EntityData()
            {
                KeyName = "fieldQuaditeNest1",
                EntityKey = "terrain:fieldQuaditeNest",
                Name = "Field quadite nest 1", // referred to in bigBombActivated
                Location = new Vector3(1926, 2592, 0),// //was 1811, 1248
                Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#1" }
            });
            #endregion

            #region #3
            list.Add(new EntityData()
            {
                KeyName = "fieldQuaditeNest3",
                EntityKey = "terrain:fieldQuaditeNest",
                Name = "Field quadite nest 3", // referred to in bigBombActivated
                Location = new Vector3(2112, 2256, 0), // NA was 3600,1248
                Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#1" }
            });
            #endregion

            #endregion

            #region Birds //birds are the only ones defined here because they require racekey

            // at  river : these birds are a bit..purple.
              list.Add(new EntityData()
                { 
                    KeyName = "bird#1", 
                    EntityKey = "entity:bird", 
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(3120, 2544, 0), 
                    Rotation = 100,                      
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  
                });

               list.Add(new EntityData()
               {
                   KeyName = "bird#2",
                   EntityKey = "entity:bird",
                   MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                   Location = new Vector3(3173, 2510, 0),
                   Rotation = 175, 
                   BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  
               });
                                
                list.Add(new EntityData()
                {
                    KeyName = "bird#3",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(3120, 2640, 0),
                    Rotation = 250, 
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  
                });




                //////////////////////////tiny guano birds north east caves

                list.Add(new EntityData()
                {
                    KeyName = "bird#4",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(3552, 288, 0),
                    Rotation = 165,
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" }  
                });

                list.Add(new EntityData()
                {
                    KeyName = "bird#5",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(3558, 300, 0),
                    Rotation = 195,
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" }
                });

                list.Add(new EntityData()
                {
                    KeyName = "bird#6",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(3369, 240, 0),
                    Rotation = 145,
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" }
                });
#endregion

            return list;

        }
    }
}
