using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data
{
    public class EntityDataLoader
    {
        public static List<EntityData> Init()
        {
            List<EntityData> list = new List<EntityData>();

            #region Leafcutter nest

            list.Add(new EntityData()
            {
                KeyName = "fieldQuaditeNest1",
                EntityKey = "terrain:fieldQuaditeNest",
                Name = "Field quadite nest 1", // referred to in bigBombActivated
                Location = new Vector3(864, 1056, 0),//
                Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#1" }
            });
                                

            #endregion

            list.Add(new EntityData()
            { 
                KeyName = "turnip",
                EntityKey = "entity:turnip",                                                                    
                BioEntity = new Maps.MapEditor.BiologicalEntity() { RaceKey = "Pale Turnip" } // omitting age group. a default age will be selected
            });

            // seems to spawn near the map edge
            list.Add(new EntityData()
            {
                KeyName = "thunderChicken",
                EntityKey = "entity:studdedThunderChicken",
                Name = "ThunderChicken(1584,2736)",               
                Location = new Vector3(1384, 70, 0),               
                BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }
            });

#region Birds

              list.Add(new EntityData()
                { 
                    KeyName = "bird#1", 
                    EntityKey = "entity:bird", 
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                    Location = new Vector3(200, 200, 0), 
                    Rotation = 100, 
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "dark" }  
                });

               list.Add(new EntityData()
               {
                   KeyName = "bird#2",
                   EntityKey = "entity:bird",
                   MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                   Location = new Vector3(220, 210, 0), 
                   Rotation = 190, 
                   BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "pale" }  
               });
                                // at inlet
                list.Add(new EntityData()
                {
                    KeyName = "bird#3",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                    Location = new Vector3(1680, 2064, 0), 
                    Rotation = 100, 
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  
                });
                list.Add(new EntityData()
                {
                    KeyName = "bird#4",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                    Location = new Vector3(1700, 2044, 0), 
                    Rotation = 175, 
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  
                });

                list.Add(new EntityData()
                {
                    KeyName = "bird#5",
                    EntityKey = "entity:bird",
                    MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },
                    Location = new Vector3(1730, 2020, 0),
                    Rotation = 250,                  
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }
                });
                    

#endregion

            return list;

        }
    }
}
