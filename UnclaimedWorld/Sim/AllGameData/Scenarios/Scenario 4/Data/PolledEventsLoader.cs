using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.ClientSide.GameEvents;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Maps;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.Client.Particles;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Entities.Biological;
//using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4.Data
{
    public class PolledEventsLoader
    {

        public static List<InGameEvents.PolledEventType> Init()
        {
            List<PolledEventType> list = new List<PolledEventType>();
                       

            #region SANDBOXNOMADMAP_timedSpawnBeginningPopulationNormal // TODO: make this populations instead
            PolledEventType polledEvent = new PolledEventType() 
            {
                KeyName = "SANDBOXNOMADMAP_timedSpawnBeginningPopulationNormal",
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("0a4f3522-1ea4-46f1-b2e4-7a4305b5a326")
                    {         
                        Actions = new EventActionType[]
                        {
                                                       
                            #region 2x Demon Tree #3 North
                            new SpawnEntityAction("3dd273e1-338d-42a4-8167-ba8db92c1966") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spoakDendront", Name = "Demon Tree", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#3" },
                                  Location = new Vector3(1933, 264, 0), Bulk = 1.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } },},
                            /*new EventActionType("1fb7fc43-d9ab-470a-a49e-d0e569c5a3e0") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spoakDendront", Name = "Demon Tree",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#3" }, 
                                    Location = new Vector3(1755, 290, 0), Bulk = 1.5f, BioEntity = new Maps.MapEditor.BiologicalEntity() 
                                { AgeGroup = AIAgeGroup.Adult } }, }},*/
                            #endregion
                            
                            #region  Demon Tree #2 West //  mp removed for optimization jan 2016
                     /*       new EventActionType("7e69b265-cd37-4754-83bf-5c7d6932cc18") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spoakDendront", Name = "Demon Tree", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#2" },
                                  Location = new Vector3(770,2850, 0), Bulk = 1.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeInYears = 17 } }, }},*/

                      /*      new EventActionType("27d03b4e-9410-47af-92b8-7ef0f4daab26") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spoakDendront", Name = "Demon Tree",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#2" }, 
                                    Location = new Vector3(770,2750, 0), Bulk = 1.5f, BioEntity = new Maps.MapEditor.BiologicalEntity() 
                                { AgeInYears = 17 } }, }},*/
                            #endregion

                            #region Demon Tree #1
                            new SpawnEntityAction("aa0adw2q53256-e54d-4a8d-a48a-dfb1f37600e7") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spoakDendront", Name = "Demon Tree", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#1" },
                                  Location = new Vector3(3189, 2645, 0), Bulk = 1.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                          /*  new EventActionType("f99425f6-30c0-4108-9fe9-fa9e6da0f9f8") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spoakDendront", Name = "Demon Tree",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#1" }, 
                                    Location = new Vector3(2779,3052, 0), Bulk = 1.5f, BioEntity = new Maps.MapEditor.BiologicalEntity() 
                                { AgeGroup = AIAgeGroup.Adult } }, }},
                            new EventActionType("489530df-46e8-480e-9520-62187514e88f") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spoakDendront", Name = "Demon Tree",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#1" }, 
                                    Location = new Vector3(2560,2742, 0), Bulk = 1.2f, BioEntity = new Maps.MapEditor.BiologicalEntity() 
                                { AgeInYears = 17 } }, }},
                            new EventActionType("bcbdb151-5ee5-42c5-bd90-009b0ba126f2") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spoakDendront", Name = "Demon Tree", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#2" },
                                  Location = new Vector3(2864, 2466, 0), Bulk = 1.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeInYears = 17 } }, }},*/
                            #endregion

                            #region swampDemonTree #1 south //mp removed for optimization jan 2016
                     /*       new EventActionType("aa089awf535354d-4a8d-a48a-dfb1f37600e7")
                            {
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:swampDendront", Name = "Swamp Demon Tree", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "swampDemonTreeAllegiance#1" },
                                        Location = new Vector3(768,5328, 0), Bulk = 1.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                }
                            },*/
                            #endregion

                            #region swampDemonTree #2 north
                            new SpawnEntityAction("aa089ba6-e54afw2525a24a48a-dfb1f37600e7")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:swampDendront", Name = "Swamp Demon Tree", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "swampDemonTreeAllegiance#2" },
                                        Location = new Vector3(624, 336, 0), Bulk = 1.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                            #endregion

                                                

                            #region Bush Dragons // have been moved to separate event below, for difficulty selection reasons
                    /*
                            new EventActionType("b7a69ed1-8224-414f-b386-35574974654a") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(4871, 5609, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, }},
                                new EventActionType("d156aafwfwa428c3-4dd7-af64-7e1688e699ff") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5022, 5542, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, }},
                                new EventActionType("d156ad0e-fwa2wa424d7-af64-7e1688e699ff") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5322, 5592, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, }},
                                new EventActionType("e03da83f-a3c9-453f-b9fd-55b8831ba420") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(4849, 5152, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, }},
                                new EventActionType("166d7be1-c292-4d74-9fde-da2a7b7afbbb") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5520, 5133, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, }},
                        */        
                            #endregion

                            #region Thunder Chicken
                            #region "thunderChickenAllegiance#1" //mp removed for optimization jan 2016
                  /*          new EventActionType("912390e2-6774-4aa8-b1f6-86a2f352d843") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#1" },
                                  Location = new Vector3(2640,1536, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult} }, }},
                            new EventActionType("5616c2e3-03f7-498d-8287-495c391c7817") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#1" },
                                  Location = new Vector3(2880,1392, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult} }, }},*/
#endregion
                            #region south-mid
                            new SpawnEntityAction("91dfzsb3a5242asf-6774-4aa8-b1f6-86a2f352d843") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(3120,2688)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#2" },
                                        Location = new Vector3(3120,2688, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                            new SpawnEntityAction("91dfzsb2fa242774-4aa8-bdse56-86a2f352d843") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(1584,2736)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#2" },
                                        Location = new Vector3(1584,2736, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                            new SpawnEntityAction("91dfz452390e2-6awfa2424-4aa8-b1f6-86a2f352d843") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(576,2928)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#2" },
                                        Location = new Vector3(576,2928, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                            #endregion 
                            #region south-east corner. thunderChickenAllegiance#3  //mp removed for optimization jan 2016
                 /*           new EventActionType("91d536753687t6yudtyud5e52d843") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(5808,5376)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#3" },
                                        Location = new Vector3(5808,5376, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                }
                            },
                            new EventActionType("91ddtyiue76u6e7ueytdjdtjtydtyj765762d843") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(5760,5000)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#3" },
                                        Location = new Vector3(5760,5000, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                }
                            },
                 */
                            #endregion 

                            #endregion
  

                            #region Turnip
                            new SpawnEntityAction("d78d8657-f25e-49eb-a465-1456b0eb3a98") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(2587, 356, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" } }, },
                            new SpawnEntityAction("6f0be88a-eee5-42c9-8483-252947b950f1") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(2387, 356, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" } }, },
                       /*     new EventActionType("d046dc3f-22a4-4cc1-9ef3-544454400d11") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(2487, 256, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" } }, }},*/
                    /*        new EventActionType("fda5f482-7e8b-4e2e-816e-dfa9f10f2a88") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(2487, 456, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" } }, }},
                            new EventActionType("6326ca9a-7b1e-4b1b-96f9-5144d5bc0888") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(2287, 456, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" } }, }},*/


                            new SpawnEntityAction("04961f4c-9623-4b26-8d10-b8e26ac51bd4") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(1819, 1013, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Copper Turnip" } }, },
                            new SpawnEntityAction("ea761587-970b-4ed0-b29e-02ebf3b61b2c") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(1619, 1013, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Copper Turnip" } }, },
                      /*      new EventActionType("6341a13d-d7b4-4a96-b581-2e8583bba531") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(1719, 1113, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Copper Turnip" } }, }},
                            new EventActionType("db05c0e5-672f-434a-94ea-b2204ed7361c") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(1719, 913, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Copper Turnip" } }, }},*/
                            new SpawnEntityAction("4dd5379e-17c0-4575-99d9-2f3728588213") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(1519, 913, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Copper Turnip" } }, },
                            #endregion

                            #region Megapod (Slug)
                            new SpawnEntityAction("b4fawr2a42d-7d60-45d0-a0e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(2064,4704, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9f2faw242460-45d0-a0e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(1584,4560, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9f25d-7dfwa24245d0-a0e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(1920,4224, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9f25d-af2453t-45d0-a0e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(2016,4944, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9f25d-agfeafa60-45d0-a0e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod5", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(2688,4512, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9f25d-7d60-afaws42e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod6", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(1776,5184, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult, CasteKey="male" } 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9f25d-7wasfw24-45d0-a0e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod(1584,5088)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(1584,5088, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult, CasteKey="male" } 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9f25d-7d60-4awfa24252e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod(1680,5328)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(1680,5328, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult, CasteKey="male" } 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9ffwas24420-45d0-a0e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod(2448,4944)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(2448,4944, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult, CasteKey="male" } 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9f25d-fwaw353-45d0-a0e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod(2304,5472)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(2304,5472, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult, CasteKey="male" } 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9f25sfaa2d-7d60-45d0-a0e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod(2496,4848)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(2496,4848, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult, CasteKey="male" } 
                                    }, 
                                
                            },
                            #endregion
                            
                            #region Snatcher
                            new SpawnEntityAction("9c4bdc61-13bd-49f7-91c0-db90b9fd3b0d") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#1" },
                                  Location = new Vector3(4992,624, 0), Bulk = 2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        /*    new EventActionType("cf4d0482-86b1-40d3-a45b-25238eaf434c") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#1" },
                                  Location = new Vector3(4336, 3657, 0), Bulk = 1.5f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adultf,  } }, }},*/
                 /*           new EventActionType("e1c0e5ad-b6f4-47a9-b4d9-f448675ed7d2") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#1" },
                                  Location = new Vector3(4734, 4051, 0), Bulk = 2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adultf, } }, }},
                            new EventActionType("d2af128d-fd88-4151-b832-5e14f82285f7") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Armored", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#1" },
                                  Location = new Vector3(5640, 4086, 0), Bulk = 1.5f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult0f } }, }},*/
                            #endregion
                            
                            #region Birds
                            new SpawnEntityAction("b1232ac8-cbf8-48d4-9973-5f4bdb043f75") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(1280, 4023, 0), Rotation = 100, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "dark" } }, },
                            new SpawnEntityAction("ffd9157e-351d-41c6-bc29-2115475cd555") { DelayInSeconds = 3.5,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(1322, 3997, 0), Rotation = 190, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "pale" } }, },

                                // at rocky river bed start location: these birds are a bit..purple.
                            new SpawnEntityAction("6e06ef9f-996e-4e69-bb23-5d725699af93") { DelayInSeconds = 0.25,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(2640, 3312, 0), Rotation = 100, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" } }, },
                            new SpawnEntityAction("0dfce0bf-9d9b-468c-9e2e-4d3c400afaa4") { DelayInSeconds = 1.5,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(2699, 3351, 0), Rotation = 175, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" } }, },
                            new SpawnEntityAction("7fc004f6-79b8-4ec3-8bdb-5b27ae7c8d1d") { DelayInSeconds = 2.75,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(2736, 3312, 0), Rotation = 250, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" } }, },
 
 //////////////////////////////////////                           

                            new SpawnEntityAction("a1cbea62-ca2d-4e01-8dd3-5581764e263a") { DelayInSeconds = 0.25,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(216, 5972, 0), Rotation = 208, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "pale" } }, },
                            new SpawnEntityAction("8fec7376-c118-41ae-b5fc-342b34e026db") { DelayInSeconds = 1.5,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(274, 5945, 0), Rotation = 137, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "dark" } }, },
                            new SpawnEntityAction("9c0c7900-d2d0-42a7-ad11-337931ba33d3") { DelayInSeconds = 3.5,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(359, 5983, 0), Rotation = 316, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "yellow" } }, },
                                
                            new SpawnEntityAction("fda48cee-e931-4690-8d71-97bec4a70d10") { DelayInSeconds = 0.2,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(4543, 185, 0), Rotation = 120, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "red" } }, },
                            new SpawnEntityAction("7195180f-bb65-46a1-b729-888329aab2a6") { DelayInSeconds = 2.5,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(4678, 195, 0), Rotation = 135, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "black" } },},
                            new SpawnEntityAction("ccffadaa-05ff-4a5b-8415-d759612a56e9") { DelayInSeconds = 4.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(4673, 245, 0), Rotation = 260, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "pale" } }, },

                            new SpawnEntityAction("b3cc39d1-901f-411c-93b2-ef8626fc2a57") { DelayInSeconds = 1.5,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(3761, 5158, 0), Rotation = 135, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "dark" } }, },


//////////////////////////tiny guano birds westernmost cave
                            new SpawnEntityAction("b3cc395eyujtdeu67657u7u6wc2a57") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(3370, 2199, 0), Rotation = 165, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" } }, },
                            new SpawnEntityAction("b3ce56ueytyrutyyetu6756eu657") { DelayInSeconds = 6.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(3408, 2209, 0), Rotation = 195, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" } }, },

                            new SpawnEntityAction("b3ce56urtyeyyeteyuteyud657") { DelayInSeconds = 3.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(3395, 2219, 0), Rotation = 145, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" } }, },
                            

                        //tiny cave guano birds east
                            new SpawnEntityAction("b3cc39d536777777reyueeetuytfc2a57") { DelayInSeconds = 1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(3888, 2064, 0), Rotation = 135, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkTurqoise" } }, },
                            new SpawnEntityAction("b3cc39d536478674986r789rety7u6ui6t7ui7ytfc2a57") { DelayInSeconds = 7.3,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(3942, 2074, 0), Rotation = 165, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkTurqoise" } }, },
                            new SpawnEntityAction("b3cc39d55e6urtyutysrusrtyusrtyutyytfc2a57") { DelayInSeconds = 0.5,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(4224, 2112, 0), Rotation = 183, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" } }, },
                            new SpawnEntityAction("b3cc3946787568tdty8f768i678t8fc2a57") { DelayInSeconds = 5.5,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(4210, 2064, 0), Rotation = 103, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" } }, },
                            new SpawnEntityAction("b3e65u56eudt6u5eu756eu56u56et8fc2a57") { DelayInSeconds = 2.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(4300, 1950, 0), Rotation = 65, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" } }, },
                            new SpawnEntityAction("b3cc3953678ue56u5e67ue5u65ew6t8fc2a57") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(4116, 1960, 0), Rotation = 165, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" } }, },
                            new SpawnEntityAction("b3ccd6fsaw2442autyutd6utd6ud6u6dd657") { DelayInSeconds = 6.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(4106, 1955, 0), Rotation = 195, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" } }, },

                            new SpawnEntityAction("b3ccd6utyutd6utd6udat253arfw6u6dd657") { DelayInSeconds = 3.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(4502, 2020, 0), Rotation = 145, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "veryDarkGreen" } }, },
                            #endregion



                            #region scavengers

                            #region ratspawns
                            #region "binalRatAllegiance#2"
                                    new SpawnEntityAction("99875afta3w5cc0-30ae-4f81-82fe-06cdc739ecfe")
                                    {
                                       
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(1296,1344)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#2" },
                                                Location = new Vector3(1296,1344, 0), Bulk = 0.21f,
                                                BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                            }, 
                                        
                                    },
                                    new SpawnEntityAction("99safa32tg-30ae-4f81-82fe-06cdc739ecfe")
                                    {
                                       
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(480,816)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#2" },
                                                Location = new Vector3(480,816, 0), Bulk = 0.21f,
                                                BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                            }, 
                                        
                                    },
                                    new SpawnEntityAction("9agdfsawf0ae-4f81-82fe-06cdc739ecfe")
                                    {
                                       
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(2448,96)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#2" },
                                                Location = new Vector3(2448,96, 0), Bulk = 0.21f,
                                                BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                            }, 
                                        
                                    },
#endregion
                            #region "binalRatAllegiance#1"
                                    new SpawnEntityAction("99875awf2a5ag-4f81-82fe-06cdc739ecfe")
                                    {
                                       
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(5856,528)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#1" },
                                                Location = new Vector3(5856,528, 0), Bulk = 0.21f,
                                                BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                            }, 
                                        
                                    },
                                    new SpawnEntityAction("99facdvd-30ae-4f81-82fe-06cdc739ecfe")
                                    {
                                       
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(2304,1680)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#1" },
                                                Location = new Vector3(2304,1680, 0), Bulk = 0.21f,
                                                BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                            }, 
                                        
                                    },
                                    new SpawnEntityAction("998wasfsfgc0-30ae-4f81-82fe-06cdc739ecfe")
                                    {
                                       
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(3072,3312)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#1" },
                                                Location = new Vector3(3072,3312, 0), Bulk = 0.21f,
                                                BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                            }, 
                                        
                                    },
                                    new SpawnEntityAction("998zxctatg-30ae-4f81-82fe-06cdc739ecfe")
                                    {
                                       
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(384,2592)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#1" },
                                                Location = new Vector3(384,2592, 0), Bulk = 0.21f,
                                                BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                            }, 
                                        
                                    },
#endregion
                            #region south east
                                    new SpawnEntityAction("f39d3dff8fwaf242413-9477-1372b18710fxdr98") 
                                    { 
                                        DelayInSeconds = 0.1,
                                       
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat1(6048,4800)",
                                                MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#3" },
                                                Location = new Vector3(6048,4800, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                            }, 
                                        
                                    },
                                    new SpawnEntityAction("f608b57c-0da242447-442d-ab93-c57fbsdrsr7f395de") 
                                    { 
                                        DelayInSeconds = 0.1,
                                       
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(5280,5040)", 
                                                MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#3" },
                                                Location = new Vector3(5280,5040, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                            }, 
                                        
                                    },
                                    new SpawnEntityAction("d913ba37-serhdc30-4fwa24242-91dd-aea79dd0a242") 
                                    { 
                                        DelayInSeconds = 0.1,
                                       
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(4704,6000)", 
                                                MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#3" },
                                                Location = new Vector3(4704,6000, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                            }, 
                                        
                                    },
                            #endregion
                          
                            #endregion

                            #region leafcutter
#region leafcutterAllegiance#1                                    
                            new SpawnEntityAction("99wasftayhyagc0-30ae-4f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(1296,1344)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#1" },
                                        Location = new Vector3(1728,1008, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
#endregion
#region leafcutterAllegiance#4  not used
                         /*   new EventActionType("agfgdbhc0-30ae-4f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(480,816)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#4" },
                                        Location = new Vector3(2784,1632, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                }
                            },*/
 #endregion   
#region leafcutterAllegiance#5  not used                            
  /*                          new EventActionType("9fhrtjyukiuocc0-30ae-4f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(2448,96)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#5" },
                                        Location = new Vector3(912,1248, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                }
                            },*/
 #endregion 
#region leafcutterAllegiance#2 mp removed for optimization jan 2016
           /*                 new EventActionType("99wafsf664-30ae-4f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(5856,528)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#2" },
                                        Location = new Vector3(1536,1536, 0), Bulk = 0.20f, 
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                }
                            },
                            new EventActionType("99wafsa64w7wygscdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(2304,1680)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#2" },
                                        Location = new Vector3(2880,96, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                }
                            },*/
#endregion
#region leafcutterAllegiance#3
                            new SpawnEntityAction("99875c25732756dighyshbea739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(3072,3312)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#3" },
                                        Location = new Vector3(2496,288, 0), Bulk = 0.21f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                            new SpawnEntityAction("998afs3663eaae-4f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(384,2592)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#3" },
                                        Location = new Vector3(432,912, 0), Bulk = 0.21f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
#endregion
                            #endregion

                            #endregion

                        }
                    }
                    }
                }
            };
            list.Add(polledEvent);


            #endregion
            #region SANDBOXNOMADMAP_timedSpawnBeginningPopulationBushdragonsNormal -Bush dragons have their own spawn event, for diffulty selection reasons
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_timedSpawnBeginningPopulationBushdragonsNormal",
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("0a4f34376wrtuywtruywryuhwrhwrh26")
                    {         
                        Actions = new EventActionType[]
                        {

                            #region Bush Dragon
                            new SpawnEntityAction("b7a69ed1-8224-414f-b386-35574974654a") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(4871, 5609, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} },},
                                new SpawnEntityAction("d156ad0faw2424-4dd7-af64-7e1688e699ff") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5022, 5542, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, },
                                new SpawnEntityAction("d156ada242rwafegtefdsx7-af64-7e1688e699ff") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5322, 5592, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, },
                                new SpawnEntityAction("e03da83f-a3c9-453f-b9fd-55b8831ba420") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(4849, 5152, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, },
                                new SpawnEntityAction("166d7be1-c292-4d74-9fde-da2a7b7afbbb") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5520, 5133, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, },
                                
                            #endregion


                        }
                    }
                    }
                }
            });
            #endregion

    

            #region///////Continual spawning




            #region Migration Spawns (moving from one end of map to the other etc.)

            #region migration Lesser Whipjaw East to West
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_migrationLesserWhipjawEast",
                PollInterval = new ValueNode() { PropertyKey = "lesserWhipjawMigrationInterval" }, //(there's 1600 sec / day)  
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { PropertyKey = "lesserWhipjawMigrationInterval" }},
                AllowRandomTimeOffset = false,
                Condition = new CustomCondition()
                    {
                        // don't exceed max number:
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "lesserWhipjawAllegianceWest"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxLesserWhipjaw"
                            }
                        }
                },

                ActionSetsKey = "migrationLesserWhipjawEast"
            });
            #endregion

            #region migration Bajingan North to West
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_migrationBajinganNorth",
                PollInterval = new ValueNode() { PropertyKey = "bajinganMigrationInterval" }, //(there's 1600 sec / day)  
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { PropertyKey = "bajinganMigrationInterval" } },             
                AllowRandomTimeOffset = false,
                Condition = new CustomCondition()
                    {
                        // don't exceed max number:
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "bajinganAllegianceWest"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxBajingan"
                            }
                        }
                },

                ActionSetsKey = "migrationBajinganNorth"
            });
            #endregion


            #endregion

            #region Bush Dragon South
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnBushDragonSouth",
                PollInterval = new ValueNode() { PropertyKey = "bushDragonSpawnInterval" },
                StartAfterInterval = true, // #POLLCHANGE TEST THIS
                
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "bushDragonAllegianceSouth"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxBushDragons"
                            }
                        }
                    
                    
                },

                ActionSetsKey = "continualSpawnBushDragonSouth"
            });
            #endregion
            
            #region Twinkler north
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnTwinklerNorth",
                PollInterval = new ValueNode() { PropertyKey = "twinklerSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                {
                    TargetObject = new TargetObject()
                    {
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "allegiances",
                            FilterCondition = new PropertyCondition()
                            {
                                PropertyKey = "keyName",
                                ConstantStringEqual = "twinklerAllegianceNorth"
                            },
                            NextList = new GetList()
                            {
                                HasPropertiesListKey = "members"
                            }
                        }
                    },
                    ListCondition = new ListCondition()
                    {
                        CountMaximum = new ValueNode()
                        {
                            PropertyKey = "maxTwinklers"
                        }
                    }
                    
                    
                },

                ActionSetsKey = "continualSpawnTwinklerNorth"
            });
            #endregion

            #region Turnips north
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnTurnipsNorth",
                PollInterval = new ValueNode() { PropertyKey = "turnipSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "turnipAllegianceNorth"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxTurnips"
                            }
                        } 
                },

                ActionSetsKey = "continualSpawnTurnipNorth"
            });
            #endregion

            #region Binal rats #3
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnBinalRats#3",
                PollInterval = new ValueNode() { PropertyKey = "binalRatSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "binalRatAllegiance#3"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxBinalRats"
                            }
                        }
                },

                ActionSetsKey = "continualSpawnBinalRats#3"
            });
            #endregion
            
            #region Binal rats #2
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnBinalRats#2",
                PollInterval = new ValueNode() { PropertyKey = "binalRatSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "binalRatAllegiance#2"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxBinalRats"
                            }
                        }                   
                    
                },

                ActionSetsKey = "continualSpawnBinalRats#2"
            });
            #endregion

            #region Binal rats #1
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnBinalRats#1",
                PollInterval = new ValueNode() { PropertyKey = "binalRatSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                        {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "binalRatAllegiance#1"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxBinalRats"
                            }
                        }
                },

                ActionSetsKey = "continualSpawnBinalRats#1"
            });
            #endregion

            #region Binal rats swamps
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnBinalRatsSwamp",
                PollInterval = new ValueNode() { PropertyKey = "binalRatSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                {
                    TargetObject = new TargetObject()
                    {
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "allegiances",
                            FilterCondition = new PropertyCondition()
                            {
                                PropertyKey = "keyName",
                                ConstantStringEqual = "binalRatAllegianceSwamp"
                            },
                            NextList = new GetList()
                            {
                                HasPropertiesListKey = "members"
                            }
                        }
                    },
                    ListCondition = new ListCondition()
                    {
                        CountMaximum = new ValueNode()
                        {
                            PropertyKey = "maxBinalRats"
                        }
                    }                   
                    
                },
                ActionSetsKey = "continualSpawnBinalRatsSwamp"
            });
            #endregion

            #region Binal rats swamps
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnBinalRatsSwamp2",
                PollInterval = new ValueNode() { PropertyKey = "binalRatSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "binalRatAllegianceSwamp"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxBinalRats"
                            }
                        }                    

                },
                ActionSetsKey = "continualSpawnBinalRatsSwamp2"
            });
            #endregion

            #region thunder chicken # 1
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnThunderChickens#1",
                PollInterval = new ValueNode() { PropertyKey = "thunderChickenSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "thunderChickenAllegiance#1"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxThunderChickens"
                            }
                        }
                },
                ActionSetsKey = "continualSpawnThunderChicken#1"
            });
            #endregion

            #region thunder chicken # 2
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnThunderChickens#2",
                PollInterval = new ValueNode() { PropertyKey = "thunderChickenSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "thunderChickenAllegiance#2"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxThunderChickens"
                            }
                        }
                },
                ActionSetsKey = "continualSpawnThunderChicken#2"
            });
            #endregion

            #region thunder chicken # 3
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnThunderChickens#3",
                PollInterval = new ValueNode() { PropertyKey = "thunderChickenSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "thunderChickenAllegiance#3"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxThunderChickens"
                            }
                        }                    

                },
                ActionSetsKey = "continualSpawnThunderChicken#3"
            });
            #endregion

            #region snatcher
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnSnatcher",
                PollInterval = new ValueNode() { PropertyKey = "snatcherSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "snatcherAllegiance#1"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxSnatchers"
                            }
                        }
                    
                    
                },

                ActionSetsKey = "continualSpawnSnatchers#1"
            });
            #endregion

            #region snatcher #2 north
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnSnatcher#2",
                PollInterval = new ValueNode() { PropertyKey = "snatcherSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "snatcherAllegiance#2"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                Int = 0
                            }
                        }                   

                },

                ActionSetsKey = "continualSpawnSnatchers#2"
            });
            #endregion

            #region demonTree #1 south
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnDemonTree#1",
                PollInterval = new ValueNode() { PropertyKey = "demonTreeSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "demonTreeAllegiance#1"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxDemonTree"
                            }
                        }
                    
                },
                ActionSetsKey = "continualdemonTree#1"
            });
            #endregion

            #region demonTree #2 south
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnDemonTree#2",
                PollInterval = new ValueNode() { PropertyKey = "demonTreeSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "demonTreeAllegiance#2"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxDemonTree"
                            }
                        }
                    
                },
                ActionSetsKey = "continualdemonTree#2"
            });
            #endregion

            #region demonTree #3 south
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnDemonTree#3",
                PollInterval = new ValueNode() { PropertyKey = "demonTreeSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "demonTreeAllegiance#3"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxDemonTree"
                            }
                        }
                    
                },
                ActionSetsKey = "continualdemonTree#3"
            });
            #endregion

            #region swampDemonTree #1 south
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnSwampDemonTree#1",
                PollInterval = new ValueNode() { PropertyKey = "demonTreeSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "swampDemonTreeAllegiance#1"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxDemonTree"
                            }
                        }
                    
                },
                ActionSetsKey = "continualSwampDemonTree#1"
            });
            #endregion

            #region swampDemonTree #2 north
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnSwampDemonTree#2",
                PollInterval = new ValueNode() { PropertyKey = "demonTreeSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                {
                    TargetObject = new TargetObject()
                    {
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "allegiances",
                            FilterCondition = new PropertyCondition()
                            {
                                PropertyKey = "keyName",
                                ConstantStringEqual = "swampDemonTreeAllegiance#2"
                            },
                            NextList = new GetList()
                            {
                                HasPropertiesListKey = "members"
                            }
                        }
                    },
                    ListCondition = new ListCondition()
                    {
                        CountMaximum = new ValueNode()
                        {
                            PropertyKey = "maxDemonTree"
                        }
                    }
                    
                },
                ActionSetsKey = "continualSwampDemonTree#2"
            });
            #endregion

            #region slugs
            //not in use in the moment, left in case of changes of the mind
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnSlugs",
                PollInterval = new ValueNode() { PropertyKey = "slugSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                {
                    TargetObject = new TargetObject()
                    {
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "allegiances",
                            FilterCondition = new PropertyCondition()
                            {
                                PropertyKey = "keyName",
                                ConstantStringEqual = "slugAllegiance#1"
                            },
                            NextList = new GetList()
                            {
                                HasPropertiesListKey = "members"
                            }
                        }
                    },
                    ListCondition = new ListCondition()
                    {
                        CountMaximum = new ValueNode()
                        {
                            PropertyKey = "maxSlugs"
                        }

                    }
                },
                ActionSetsKey = "continualSpawnSlugs"
            });
            #endregion

            #region leafcutter spawns

            #region #1
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnLeafcutter#1",
                PollInterval = new ValueNode() { PropertyKey = "leafcutterSpawnInterval" },
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                {
                    Left = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "allegiances",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "keyName",
                                        ConstantStringEqual = "leafcutterAllegiance#1"
                                    },
                                    NextList = new GetList()
                                    {
                                        HasPropertiesListKey = "members"
                                    }
                                }
                            },
                            ListCondition = new ListCondition()
                            {
                                CountMaximum = new ValueNode()
                                {
                                    PropertyKey = "maxLeafcutters"
                                }
                            }


                        },
                    Operator = OperatorType.And,
                    Right = new CustomCondition()
                    {
                        TargetObject = new TargetObject() { TargetObjectType = TargetObjectType.Root },
                        PropertyCondition = new PropertyCondition()
                        {
                            PropertyKey = "nest1",
                            BoolValue = true
                        }

                    }
                },

                ActionSetsKey = "continualSpawnLeafcutter#1"
            });
            #endregion
            #region #2
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnLeafcutter#2",
                PollInterval = new ValueNode() { PropertyKey = "leafcutterSpawnInterval" },
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Left = new CustomCondition()
                            {
                                TargetObject = new TargetObject()
                                {
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "allegiances",
                                        FilterCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "keyName",
                                            ConstantStringEqual = "leafcutterAllegiance#2"
                                        },
                                        NextList = new GetList()
                                        {
                                            HasPropertiesListKey = "members"
                                        }
                                    }
                                },
                                ListCondition = new ListCondition()
                                {
                                    CountMaximum = new ValueNode()
                                    {
                                        PropertyKey = "maxLeafcutters"
                                    }
                                }

                            
                        },
                        Operator = OperatorType.And,
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject() { TargetObjectType = TargetObjectType.Root },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "nest2",
                                BoolValue = true
                            }
                            
                        }
                    
                },

                ActionSetsKey = "continualSpawnLeafcutter#2"
            });
            #endregion
            #region #3
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnLeafcutter#3",
                PollInterval = new ValueNode() { PropertyKey = "leafcutterSpawnInterval" },
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Left = new CustomCondition()
                            {
                                TargetObject = new TargetObject()
                                {
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "allegiances",
                                        FilterCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "keyName",
                                            ConstantStringEqual = "leafcutterAllegiance#3"
                                        },
                                        NextList = new GetList()
                                        {
                                            HasPropertiesListKey = "members"
                                        }
                                    }
                                },
                                ListCondition = new ListCondition()
                                {
                                    CountMaximum = new ValueNode()
                                    {
                                        PropertyKey = "maxLeafcutters"
                                    }
                                }
                        },
                        Operator = OperatorType.And,
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject() { TargetObjectType = TargetObjectType.Root },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "nest3",
                                BoolValue = true
                            }
                        }                       
                    
                },

                ActionSetsKey = "continualSpawnLeafcutter#3"
            });
            #endregion
            #region #4
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnLeafcutter#4",
                PollInterval = new ValueNode() { PropertyKey = "leafcutterSpawnInterval" },
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Left = new CustomCondition()
                            {
                                TargetObject = new TargetObject()
                                {
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "allegiances",
                                        FilterCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "keyName",
                                            ConstantStringEqual = "leafcutterAllegiance#4"
                                        },
                                        NextList = new GetList()
                                        {
                                            HasPropertiesListKey = "members"
                                        }
                                    }
                                },
                                ListCondition = new ListCondition()
                                {
                                    CountMaximum = new ValueNode()
                                    {
                                        PropertyKey = "maxLeafcutters"
                                    }
                                }

                            
                        },
                        Operator = OperatorType.And,
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject() { TargetObjectType = TargetObjectType.Root },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "nest4",
                                BoolValue = true
                            }
                        }                       
                    
                },

                ActionSetsKey = "continualSpawnLeafcutter#4"
            });
            #endregion
            #region #5
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_continualSpawnLeafcutter#5",
                PollInterval = new ValueNode() { PropertyKey = "leafcutterSpawnInterval" },
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Left = new CustomCondition()
                            {
                                TargetObject = new TargetObject()
                                {
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "allegiances",
                                        FilterCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "keyName",
                                            ConstantStringEqual = "leafcutterAllegiance#5"
                                        },
                                        NextList = new GetList()
                                        {
                                            HasPropertiesListKey = "members"
                                        }
                                    }
                                },
                                ListCondition = new ListCondition()
                                {
                                    CountMaximum = new ValueNode()
                                    {
                                        PropertyKey = "maxLeafcutters"
                                    }
                                }                            
                        },
                        Operator = OperatorType.And,
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject() { TargetObjectType = TargetObjectType.Root },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "nest5",
                                BoolValue = true
                            }
                        }
                        
                    
                },

                ActionSetsKey = "continualSpawnLeafcutter#5"
            });
            #endregion
            #endregion

            #endregion

            #region leafCutterNest
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_spawnLeafcutterNests",               
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []
                    {
                        new ActionSetType("79dfgshsfghfsgzjshgfjs346x34fhjhs6e20")
                        {
                            Actions = new EventActionType[]
                            {
                                new SpawnEntityAction("4e8dghzjdgjdghjd56666665gjdgxjdg503c1")
                                {
                                    
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 33;27)", // search on this term and chane the coords in all cases
                                            Location = new Vector3(1584,1296,0),//1584,1296,0),//384,912(2208f, 1248f, 0), for testing at grassland
                                            Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#1" }
                                        }
                                    
                                },
                                new SpawnEntityAction("4e8dghjzdgjdghjd56x666665gjdgjdg503c3")
                                {
                                    
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 52;5)",
                                            Location = new Vector3(2496,240,0),//1536,1536, 0),
                                            Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#2" }
                                        }
                                    
                                },
                                new SpawnEntityAction("4e8dghjdgjdghjd5xz6666665gjdgjdg503c4")
                                {
                                    
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 75;26)",
                                            Location = new Vector3(3600,1248,0),//2496,288, 0),
                                            Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#3" }
                                        }
                                    
                                },/*
                                new EventActionType("4e8dghjdgjxzdghjd56666665gjdgjdg503c5")
                                {
                                    
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 58;34)",
                                            Location = new Vector3(2784,1632, 0),
                                            Threat = new Threat() { ThreatGroupName = "twinklerAllegianceNorth" }
                                        }
                                    }
                                },
                                new EventActionType("4e8dghawfa522hzjd56666665gjdgjdg503c7")
                                {
                                    
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 19;26)",
                                            Location = new Vector3(912,1248, 0),
                                            Threat = new Threat() { ThreatGroupName = "twinklerAllegianceNorth" }
                                        }
                                    }
                                },*/
                                #region properties
                                new SetPropertyAction("4e8dghjdgjdaa25taged566fwaw2266665gjdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest1",Value = new ValueNode(){Bool = true }
                                    
                                },
                                new SetPropertyAction("4e8dghjdfaw25252jd56666665gjdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest2",Value = new ValueNode(){Bool = true }
                                    
                                },
                                new SetPropertyAction("4e8dghjdgjdghzjd56666665gjdgjafawf2542dg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest3",Value = new ValueNode(){Bool = true }
                                    
                                },
                                new SetPropertyAction("4e8dghjdgjdghzjd56666665gafawf2aw5242ajdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest4",Value = new ValueNode(){Bool = true }
                                    
                                },
                                new SetPropertyAction("4e8dghjdgfwa2525a2jdghzjd56666665gjdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest5",Value = new ValueNode(){Bool = true }
                                    
                                },
                                #endregion
                            }
                        },
                    }
                }
            });
            /* didn't work
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_spawnLeafcutterNests2",
                ConditionSet = new ConditionSet()
                {

                    Value = new TimeCondition()
                    {
                        RelativeNoOfDays = 0.0015
                    }
                },
                ActionSets = new ActionSets()
                {
                    ActionTargets = new TargetObject() // the actions will be invoked on all these matching items
                    {
                        TargetObjectType = TargetObjectType.PolledEventSource,
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "entities",
                            FilterCondition = new PropertyCondition()
                            {
                                PropertyKey = "type",
                                StringEqual = "terrain:fieldQuaditeNest"
                            }
                        }
                    },
                    SetsOfActions = new []
                    {
                        new ActionSetType("79dfgshsfghfsgjshgfjs34634fhjhs6e20")
                        {
                            Actions = new EventActionType[]
                            {
                                new EventActionType("4e8dghjdgjdghjd56666665gstsjdgjdgfga503c1")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.DynamicTarget },
                                        PropertyKey = "activeNest",
                                        Value = new ValueNode(){ Bool = false }
                                    }
                                },
                            }
                        }
                    }
                }
            });
             * */
            #endregion
            return list;
        }
    }
}
