using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_7.Data
{
    public class EntityDataLoader
    {
        public static List<EntityData> Init()
        {
            List<EntityData> list = new List<EntityData>();

            #region nests
            //expedition 2
            #region #1
            list.Add(new EntityData()
            {
                KeyName = "swarmerNest1",
                EntityKey = "terrain:swarmerNest",
                Name = "Swarmer nest 1", // referred to in bigBombActivated
                Location = new Vector3(1080, 1080, 0),// //was 1811, 1248
                Threat = new Threat() { ThreatGroupName = "swarmerAllegiance#1" }
            });
            #endregion
            #region #5
            list.Add(new EntityData()
            {
                KeyName = "swarmerNest5",
                EntityKey = "terrain:swarmerNest",
                Name = "Swarmer nest 5", // referred to in bigBombActivated
                Location = new Vector3(1200, 1344, 0), // NA was 3600,1248
                Threat = new Threat() { ThreatGroupName = "swarmerAllegiance#1" }
            });
            #endregion
            #region #6
            list.Add(new EntityData()
            {
                KeyName = "swarmerNest6",
                EntityKey = "terrain:swarmerNest",
                Name = "Swarmer nest 6", // referred to in bigBombActivated
                Location = new Vector3(576, 1766, 0), // NA was 3600,1248
                Threat = new Threat() { ThreatGroupName = "swarmerAllegiance#1" }
            });
            #endregion

            //expedition 1
            #region #3
            list.Add(new EntityData()
            {
                KeyName = "swarmerNest3",
                EntityKey = "terrain:swarmerNest",
                Name = "Swarmer nest 3", // referred to in bigBombActivated
                Location = new Vector3(2400, 1685, 0), // NA was 3600,1248
                Threat = new Threat() { ThreatGroupName = "swarmerAllegiance#1" }
            });
            #endregion
            #region #2
            list.Add(new EntityData()
            {
                KeyName = "swarmerNest2",
                EntityKey = "terrain:swarmerNest",
                Name = "Swarmer nest 2", // referred to in bigBombActivated
                Location = new Vector3(1584, 2160, 0), // NA was 3600,1248
                Threat = new Threat() { ThreatGroupName = "swarmerAllegiance#1" }
            });
            #endregion
            #region #4
            list.Add(new EntityData()
            {
                KeyName = "swarmerNest4",
                EntityKey = "terrain:swarmerNest",
                Name = "Swarmer nest 4", // referred to in bigBombActivated
                Location = new Vector3(1872, 1344, 0), // NA was 3600,1248
                Threat = new Threat() { ThreatGroupName = "swarmerAllegiance#1" }
            });
            #endregion


            #region field Quadite nest 1
            list.Add(new EntityData()
            {
                KeyName = "fieldQuaditeNest1",
                EntityKey = "terrain:fieldQuaditeNest",
                Name = "Field Quadite Nest 1", // referred to in bigBombActivated
                Location = new Vector3(4368, 4848, 0), 
                Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#1" }
            });
            #endregion
           

            #endregion

            #region Patricians

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

            #region Birds //birds are the only ones defined here because they require racekey

            // at  river : these birds are a bit..purple.
              list.Add(new EntityData()
                { 
                    KeyName = "bird#1", 
                    EntityKey = "entity:bird", 
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(2706, 5230, 0), 
                    Rotation = 100,                      
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  
                });

               list.Add(new EntityData()
               {
                   KeyName = "bird#2",
                   EntityKey = "entity:bird",
                   MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                   Location = new Vector3(2716, 5280, 0),
                   Rotation = 175, 
                   BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  
               });
                                
                list.Add(new EntityData()
                {
                    KeyName = "bird#3",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(2726, 5260, 0),
                    Rotation = 250, 
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  
                });




                //////////////////////////tiny guano birds north east caves

                list.Add(new EntityData()
                {
                    KeyName = "bird#4",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(5510, 4075, 0),
                    Rotation = 165,
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" }  
                });

                list.Add(new EntityData()
                {
                    KeyName = "bird#5",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(5500, 4065, 0),
                    Rotation = 195,
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" }
                });

                list.Add(new EntityData()
                {
                    KeyName = "bird#6",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(5490, 4055, 0),
                    Rotation = 145,
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" }
                });
#endregion

            return list;

        }
    }
}
