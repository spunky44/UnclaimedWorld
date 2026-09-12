using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Maps.MapEditor;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.Client.Particles;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities.Biological;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4x.Data
{
    public class ActionSetsLoader
    {

        public static List<ActionSets> Init()
        {
            List<ActionSets> list = new List<ActionSets>();
                     
           
            #region Animal Migration spawns (moving from one end of map to the other etc.)

            #region 'Lesser whipjaw'  migration and dialogue .East to West
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.AllValid,
                KeyName = "migrationLesserWhipjawEast", //mp 9 individuals currently
                SetsOfActions = new []
                { 
#region lesser whipjaw dialogue
new ActionSetType("1df356eyurtyhrtysurykykyk7w756a91")
                        {    
                       //     ChanceToFire = 0.5f,
                          //  MaxFirings = 1,  
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                            
                            , 
                       Actions = new EventActionType[]{new TalkAction("de0ctyuee565e367856385385738a3150a3") {  
                      
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        TurnTowardsListeners = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Hey...this is the time when Lesser whipjaw are migrating!" }, // too specific: "Hey...it's that time of year. Lesser whipjaw are migrating now!"

                    new TalkAction("fecd467867ejuidtjdtsyhjuyty0878") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        TurnTowardsListeners = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "They'll be scuttling along Emerald river. Let's catch some!" },
                       }
                        },
#endregion

                #region lesserwhipjaw spawns
                    new ActionSetType("d92a5155578701d-ab98-447d-9b02-7cccbfdsa212be39f5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnAllegianceAction()
                            {
                                Comments = "The allegiance will be destroyed when the last member leaves. so we make sure it exists at each spawn event.",

                                KeyName = "spawnLesserWhipjawExpeditionWest",
                               
                                    Site = "playSite",
                                    ExpeditionData = new ExpeditionData()
                                    {
                                        KeyName = "lesserWhipjawAllegianceWest",
                                        Name = "Lesser whipjaw Allegiance West",
                                        AllegianceKey = "lesserWhipjawAllegianceWest",
                                        Location = new ValueNode()
                                        {
                                            Location = new Vector2(20, 3600)
                                        }
                                    },
                                    AllegianceData = new AllegianceData()
                                    {
                                        ForageAndHuntingRadius = 48,
                                        Name = "Lesser whipjaw Allegiance West",
                                        KeyName = "lesserWhipjawAllegianceWest", // poor key name for expedition
                                        EntityType = "entity:lesserWhipjaw",
                                        AllegianceType = Allegiances.AllegianceType.Other,
                                        StatsData = new StatsData()
                                        {
                                            Security = 1f,
                                            Comfort = 1f,
                                            FoodSupply = 1f,
                                        }
                                    }
                                
                            },
                            new SpawnEntityAction("d7tdyujkdgjdjdjdtyju498") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(3814, 3092, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("6f04678674864786476reuijte7jdtyj8950f1") { DelayInSeconds = 0.6,
                           EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(3814, 3092, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },
                            new SpawnEntityAction("04961fdtyjdt7tjddgtde56jueac51bd4") { DelayInSeconds = 0.2,
                           EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(3814, 3092, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("ea7srtdtyjt76u6rudtyjudtub61b2c") { DelayInSeconds = 1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(3814, 3092, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },                   
 
                            new SpawnEntityAction("043q456t34q56tae5456jueac51bd4") { DelayInSeconds = 6.4,
                           EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(3814, 3092, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
////////////////////////////////////////

           
                            new SpawnEntityAction("4d54w6rewtyrsetysyaee6ue8588213") { DelayInSeconds = 8.8,
                           EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(3814, 3092, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },
//////////////////////////////////
//////////////////////////////:
                            new SpawnEntityAction("04961fdt4fxgyhnj6rtysthj6w4ysr45y54wac51bd4") { DelayInSeconds = 4.2,
                           EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(3814, 3092, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("easr45yrs5t6tdfydstyasetyrsttub61b2c") { DelayInSeconds = 3.5,
                           EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(3814, 3092, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },                   
                            new SpawnEntityAction("4dwrsrty64y765674w5767455464yw213") { DelayInSeconds = 2.8,
                           EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(3814, 3092, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },

                        }    
                    },
#endregion
                }
            });
            #endregion


            #region Bajingan migration and dialogue. coming from 3 corners of the map , moving to West
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.AllValid,
                KeyName = "migrationBajinganNorth", //mp 20 individuals currently coming from 3 corners of the map
                SetsOfActions = new []
                { 

#region bajingan dialogue
new ActionSetType("1df356tyueyeyee5e56u56u5e6u5e6ua91")
                        {    
                       //     ChanceToFire = 0.5f,
                          //  MaxFirings = 1,  
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                            
                            , 
                       Actions = new EventActionType[]{new TalkAction("de0e56666666666666666uesuwsrtyusr50a3") {  
                      
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        TurnTowardsListeners = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Heads up! Bajingan will soon be swarming around here." }, //

                    new TalkAction("fecdteteteteteteteteteteteteteu467u56ty0878") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        TurnTowardsListeners = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Ok. Better secure the food stores till they've passed through." },
                       }
                        },
#endregion

                    //Bajingan spawns:
                    new ActionSetType("d92a70151561d-ab9777asa8-44fag37d-9b02-7cccbfbe39f5")
                    {                                                       
                        Actions = new EventActionType[]
                        { 
  
                            new SpawnAllegianceAction()
                            {
                                Comments = "The allegiance will be destroyed when the last member leaves. so we make sure it exists at each spawn event.",

                                
                 KeyName = "spawnBajinganExpeditionWest",
               
                    Site = "playSite",
                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "bajinganAllegianceWest",
                        Name = "Bajingan Allegiance West",
                        AllegianceKey = "bajinganAllegianceWest",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(48, 1776)
                        }
                    },
                    AllegianceData = new AllegianceData()
                    {
                        ForageAndHuntingRadius = 110,
                        Name = "Bajingan Allegiance West",
                        KeyName = "bajinganAllegianceWest",
                        EntityType = "entity:bajingan",
                        AllegianceType = Allegiances.AllegianceType.Other,
                        StatsData = new StatsData()
                        {
                            Security = 1f,
                            Comfort = 1f,
                            FoodSupply = 1f,
                        }
                    }
                
                            },  
#region      //North edge, center:
                            new SpawnEntityAction("d75675637856e78563785787867498") { DelayInSeconds = 5.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3456, 30, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, }  }},
                            new SpawnEntityAction("6f0467867486478647864786748950f1") { DelayInSeconds = 5.6,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3552, 28, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  }  }},
                            new SpawnEntityAction("04961f4c-9srtyues5e6rue56jueac51bd4") { DelayInSeconds = 5.2,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3500, 36, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, }  }},
                            new SpawnEntityAction("ea7srtyuh65e78uerthjryuyryub61b2c") { DelayInSeconds = 6,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3600, 28, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  }  }},                   
               /*             new EventActionType("4dd53dtyuteu657856eudtyrute8588213") { DelayInSeconds = 5.8,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3650, 30, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, }},*/
#endregion

#region            ////////////north east :
                            new SpawnEntityAction("sfgjghfnjxdyrtjdrtyjds67498") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3810, 240, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, }  }},
                            new SpawnEntityAction("sfgjhnxdgtrydtjufghx48950f1") { DelayInSeconds = 0.6,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3810, 240, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  }  }},
                            new SpawnEntityAction("dhjxghnjxrsyhjugfshsfgheac51bd4") { DelayInSeconds = 0.2,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3810, 240, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, }  }},

#endregion

#region          //not used
               /*             new EventActionType("sf56dfgaf242rasfhjddtyjdtyj8") { DelayInSeconds = 1.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeInYears = 18, } }, }},
                            new EventActionType("sfdghjd56fae2465ejutudtgy0f1") { DelayInSeconds = 1.6,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeInYears = 18,  } }, }},
                            new EventActionType("dhdghjdy7dt6ydfafwa2gsdhj1bd4") { DelayInSeconds = 1.2,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeInYears = 18, } }, }},
                            new EventActionType("xsddghj6y75re68u56eu562c") { DelayInSeconds = 2,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(4704, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeInYears = 18,  } }, }},      */             

#endregion
#region                                //not used
                     /*/       new SpawnEntityAction("sf56waf242rfasdtyjdtyj8") { DelayInSeconds = 3.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeInYears = 18, }  }},
                            new SpawnEntityAction("sfdghjd56a34223as5ejutudtgy0f1") { DelayInSeconds = 3.6,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeInYears = 18,  }  }},
                            new SpawnEntityAction("dhdghjdy7dt6ydgada24242hgsdhj1bd4") { DelayInSeconds = 3.2,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeInYears = 18, }  }},*/
                 
#endregion

                        }    
                    },
                }
            });
            #endregion


            #endregion
            #region//// Continuous spawns

            #region Bush Dragon South
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid, 
                KeyName = "continualSpawnBushDragonSouth",
                SetsOfActions = new []
                { 
                    new ActionSetType("d925515-3a701d-asfafb98-447d-9b02-7cccbfbe39f5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("eduyogytf63ea9-euyyyoug4e2-4c37-8312-7ce1944a9d9e") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5130, 5907, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,}  }},
                        }    
                    },
                    new ActionSetType("e8e3aed9-7e56-sg46264egse-8ad9-ac28f14ab0c1")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("d5e5a91c-0f8d-4cgjkuuioip0-a924-ef7e14b3e21b") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5824, 5604, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,}  }},
                        }    
                    },
                    new ActionSetType("d92asfsafa2525701d-ab98-447d-9b05253fa2-7cccbfbe39f5-dwad5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("edf63ipea9-e4e2-hohuotuhjguoupp4c37-8312-7ce1ip944a9d9e") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5130, 5907, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,}  }},
                        }    
                    },
                    new ActionSetType("e8e3aed9-7e56-43f8-8ad9-ac28sgd22tytupp4ab0c1")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("d5e5a91c-0f8d-4cf0-a924-ef532twrgtyiupsfq3e21b") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5824, 5604, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,}  }},
                        }    
                    },
                }
            });
            #endregion

            # region Twinklers North
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnTwinklerNorth",
                SetsOfActions = new []
                { 
                    new ActionSetType("c24b0bad-c787-45d0-a329-ad0bc73665f8")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("19a9a825-5c40-4c51-9e7f-3af069d39238") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:twinkler", Name = "Twinkler1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegianceNorth" },
                                  Location = new Vector3(5496, 562, 0), Bulk = 1.5f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,}  }},
                        }    
                    },
                }
            });
            #endregion

            #region Turnips North
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnTurnipNorth",
                SetsOfActions = new []
                { 
                    new ActionSetType("ef6e8bef-0afb-4b0saf325f-8365-50ad3f301eba")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("1575ee05-443b-4fas235235fa-b379-d3dbf9d99229") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(1570,314, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" }  }},
                        }    
                    },
                    new ActionSetType ("9833b74b-51hyungniiiim20-4181-bea8-d0f7405c7d50")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("25ee2bc6-e7cf-4af224369-8644-945ce9e56ba4") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(849,734, 0), Bulk = 4.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Copper Turnip" }  }},
                        }
                    },
                    new ActionSetType("ef6e8bef-0afb-4b0f-83asf23532-50ad3f301eba")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("1575ee05-443b-431b-bf32t4wegae-d3dbf9d99229") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(1570,314, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" }  }},
                        }    
                    },
                    new ActionSetType ("9833asf325b74b-5120-4181-bea8-d0f7405c7d50")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("25ee2bc6-e7cf-4a79-8fsa2362644-945ce9e56ba4") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(849,734, 0), Bulk = 4.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Copper Turnip" }  }},
                        }
                    },
                }
            });
            #endregion

            #region binal rats #3
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,
                KeyName = "continualSpawnBinalRats#3",
                SetsOfActions = new []
                { 
                    new ActionSetType("3e1fe44e-1e39-49cc-bb64-ffxca6dba0832c")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("f39d3dff8-2268aw424a2sfa3-9477-1372b18710fxdr98") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat1(6048,4800)",
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#3" },
                                        Location = new Vector3(6048,4800, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                        }    
                    },
                    new ActionSetType ("687280f3-a63a-45serd3-8cfd-76csrhadddbaffc")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("f608b57c-0847-4afws2422d-ab93-c57fbsdrsr7f395de") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat(5280,5040)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#3" },
                                        Location = new Vector3(5280,5040, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                        }
                    },
                    new ActionSetType ("dc2ac90c-68e4-4828-serh89f4-a777b93cee68")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("d913ba37-serhdc30-4970-91dd-aea79dfwa24242aa242") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat(4704,6000)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#3" },
                                        Location = new Vector3(4704,6000, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                }
            });
            #endregion

            # region binal rats #2
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnBinalRats#2",
                SetsOfActions = new []
                { 
                    new ActionSetType("3e1fe44e-1e39-49cc-bb64-ffa6dba0832c")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("f39d3dff8-2268-4613-9477-1372b1871098") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:binalRat", Name = "BinalRat1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#2" },
                                  Location = new Vector3(3072,1488, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }    
                    },
                    new ActionSetType ("687280f3-a63a-45d3-8cfd-76cadddbaffc")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("f608b57c-0847-442d-ab93-c57fb7f395de") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:binalRat", Name = "BinalRat2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#2" },
                                  Location = new Vector3(5403,1575, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }
                    },
                    new ActionSetType ("dc2ac90c-68e4-4828-89f4-a777b93cee68")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("d913ba37-dc30-4970-91dd-aea79dd0a242") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:binalRat", Name = "BinalRat2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#2" },
                                  Location = new Vector3(3832,2221, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult}  }},
                        }
                    },
                }
            });
            #endregion

            # region binal rats #1
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnBinalRats#1",
                SetsOfActions = new []
                { 
                    new ActionSetType("7f269bf2-a78c-45b5-a8f8-21bd269681ec")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("f38e9025-0b17-4940-aba0-13e0a22003c1") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:binalRat", Name = "BinalRat1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#1" },
                                  Location = new Vector3(1392,2064, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }    
                    },
                }
            });
            #endregion

            #region thunder chicken #3 //mp jan 2016 not used
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnThunderChicken#3",
                SetsOfActions = new []
                { 
                    new ActionSetType("8203c536765w3u7655w36uw5eu56rseuserud5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("91dfze56ue56u5e6u76ruetyrtueuetuyd843") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(5808,5376)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#3" },
                                        Location = new Vector3(5808,5376, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                    new ActionSetType("82457656u65w3u653wuwew46w5uw56d5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("91wryue567tydiutdiyri786r8iri8r843") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(5760,5000)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#3" },
                                        Location = new Vector3(5760,5000, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },

                }
            });
            #endregion

            #region thunder chicken #2
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnThunderChicken#2",
                SetsOfActions = new []
                { 
                    new ActionSetType("8203c1d8-0esdf5d-44ee-9407-b374091ed5d5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("91dfzsb2390e2-6a2424asf4aa8-b1f6-86a2f352d843") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(3120,2688)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#2" },
                                        Location = new Vector3(3120,2688, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                    new ActionSetType("8203c1d8-0esdf5d-44ee-9407-b374xdf091ed5d5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("91dfzsb2390e2-fa24248-bdse56-86a2f352d843") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(1584,2736)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#2" },
                                        Location = new Vector3(1584,2736, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                    new ActionSetType("8203c1d8-0esdf5d-44ee-9407-b374091eawe4d5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("91dfz452390e2-6774-faww242a8-b1f6-86a2f352d843") 
                            { 
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(576,2928)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#2" },
                                        Location = new Vector3(576,2928, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }    
                    },
                }
            });
            #endregion

            #region thunder chicken #1
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnThunderChicken#1",
                SetsOfActions = new []
                { 
                    new ActionSetType("8203c1d8-0e5d-44ee-9407-b374091ed5d5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("912390e2-6774-4aa8-b1f6-86a2f352d843") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#1" },
                                  Location = new Vector3(2640,1536, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult}  }},
                        }    
                    },
                    new ActionSetType("f585519b-724b-4a8f-b84e-a36aaf3ccca2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("5616c2e3-03f7-498d-8287-495c391c7817") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#1" },
                                  Location = new Vector3(2880,1392, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult}  }},
                        }    
                    },
                }
            });
            #endregion

            #region Snatcher
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnSnatchers#1",
                SetsOfActions = new []
                { 
                    new ActionSetType("3eae1eb9-fd76-4asf235236-9a12-526b2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cf65-f9f8-4fahdzf0-8dd9-3f0d081b487e") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#1" },
                                  Location = new Vector3(3792,576, 0), Bulk = 2f, BioEntity = new Maps.MapEditor.BiologicalEntity() //5725,3845
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }    
                    },
                    new ActionSetType ("8ed0a13c-8590-417e-a246-63d6c26bbe69")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("a2dd9aef-2011-4673-a7ca-99cf7a72c7c3") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#1" },
                                  Location = new Vector3(3792, 1248, 0), Bulk = 2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  }  }},
                        }
                    },
                }
            });

            #endregion

            #region Snatcher #2 north
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnSnatchers#2",
                SetsOfActions = new []
                { 
                    new ActionSetType("3eae1eb9-fd76-42b5-9asfehrys2-526b2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cf65-f9f8-4590-8aawafq9-3f0d081b487e") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#2" },
                                  Location = new Vector3(4992,624, 0), Bulk = 2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }    
                    },
                }
            });

            #endregion

            #region demonTree #1 south
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualdemonTree#1",
                SetsOfActions = new []
                { 
                    new ActionSetType("3eae1eb9-fd76-42b5-9a12-526zxzdaf326250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cf65-afzvaahw-4590-8dd9-3f0d08zxczg1b487e") 
                            {
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:spoakDendront", Name = "Demon tree south", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#1" },
                                        Location = new Vector3(2592, 2750, 0), Bulk = 1.1f, 
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                }
            });
            #endregion

            #region demonTree #2 west
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualdemonTree#2",
                SetsOfActions = new []
                { 
                    new ActionSetType("3eae1eb9-fd76-42b5-9a12-526zxzdb225asf32qwa2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cf65-f9f8-agzedtta90-8dd9-3f0d08zxczg1b487e") 
                            {
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:spoakDendront", Name = "Demon tree west", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#2" },
                                        Location = new Vector3(770, 2800, 0), Bulk = 1.1f, 
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                }
            });
            #endregion

            #region demonTree #3 north
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualdemonTree#3",
                SetsOfActions = new []
                { 
                    new ActionSetType("3eae1eb9-fd76-42b5-9a12-526zxzdb2250e1asft326252")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cf65-f9f8-45agete333r-8dd9-3f0d08zxczg1b487e") 
                            {
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:spoakDendront", Name = "Demon tree west", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#3" },
                                        Location = new Vector3(1749, 201, 0), Bulk = 1.1f, 
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                }
            });
            #endregion

            #region swampDemonTree #1 south
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSwampDemonTree#1",
                SetsOfActions = new []
                { 
                    new ActionSetType("3eae1eb9-fd76-42b5-9af3256tq26zxzdb2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cf65-f9f8-4590-afagghd9-3f0d08zxczg1b487e") 
                            {
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:swampDendront", Name = "Swamp demon tree south", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "swampDemonTreeAllegiance#1" },
                                        Location = new Vector3(768,5328, 0), Bulk = 1.1f, 
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                }
            });
            #endregion

            #region swampDemonTree #2 north
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSwampDemonTree#2",
                SetsOfActions = new []
                { 
                    new ActionSetType("3eaeafwwwwww-fd76-42baf62269a12-526zxzdb2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cf65-f9f8-4590-8dagfbwhh-3f0d08zxczg1b487e") 
                            {
                                DelayInSeconds = 0.1,
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:swampDendront", Name = "Swamp demon tree south", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "swampDemonTreeAllegiance#2" },
                                        Location = new Vector3(624,336, 0), Bulk = 1.1f, 
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                }
            });
            #endregion

            #region swamp binal rats
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnBinalRatsSwamp",
                SetsOfActions = new []
                {
                    #region ratspawns
                    new ActionSetType("d71132d7-1383-415d-awfredg4652db-ab9f211e6a8c")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("998saf360ae-4f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat(48,5328)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSwamp" },
                                        Location = new Vector3(48,5328, 0), Bulk = 0.28f,BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                        }    
                    },
                    new ActionSetType ("78d6ba84-c86qawrtfghr54r-98ab-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875fa36tagh1-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat(1065,5034)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSwamp" },
                                        Location = new Vector3(1065,5034, 0), Bulk = 0.28f,BioEntity = new Maps.MapEditor.BiologicalEntity(){AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                    new ActionSetType ("d1awaf353qa528e-45ab-a00b-5100e0c7d673")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875cag63a5a381-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat(3504,5664)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSwamp" },
                                        Location = new Vector3(3504,5664, 0), Bulk = 0.28f,BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                        }
                    },
                    new ActionSetType ("78d6ba84-c8fsa35t3aa567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875csaf6236f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat(2181,4239)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSwamp" },
                                        Location = new Vector3(2181,4239, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                        }
                    },
                    #endregion
                } 
            });
            #endregion

            #region swamp binal rats 2
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnBinalRatsSwamp2",
                SetsOfActions = new []
                {
                    #region ratspawns
                    new ActionSetType("d71132d7-1383-415d-92db-aaf3563a11e6a8c")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("99875fsa3626e-4f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat(288,5424)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSwamp2" },
                                        Location = new Vector3(288,5424, 0), Bulk = 0.28f,BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                        }    
                    },
                    new ActionSetType ("78d6ba84af3t398ab-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875ccsaf6244wyag81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat(960,5568)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSwamp2" },
                                        Location = new Vector3(960,5568, 0), Bulk = 0.28f,BioEntity = new Maps.MapEditor.BiologicalEntity(){AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                    new ActionSetType ("d1a58d29-cawfa55-a00b-5100e0c7d673")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875cc0-3awrf6263tghaufe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat(336,4656)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSwamp2" },
                                        Location = new Vector3(336,4656, 0), Bulk = 0.28f,BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                        }
                    },
                    new ActionSetType ("78d6ba84-c861-49afwghr567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875cc0-15qt3ageyatwe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat(768,4608)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSwamp2" },
                                        Location = new Vector3(768,4608, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                        }
                    },
                    #endregion
                }
            });
            #endregion

            #region swamp slugs (worms / megapods)
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnSlugs",
                SetsOfActions = new []
                {
                    new ActionSetType("c2321887-d2eaf3252344-a0f8-73d7d00efcd8")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("453500fb-e17fa352sfaafqwfsxd6-79a2571945eb") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(2064,4704, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }    
                    },
                    new ActionSetType("1bea65f8-2casfa55a25taa5ba257958")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("1f2d70e9afa3552439-ae13-657b72031cba") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(1584,4560, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }    
                    },
                    new ActionSetType("6d07dcb3-f531-4fwa553-99fc-778aa233e6a2")
                    { 
                        Actions = new EventActionType[]
                        { 
                            new SpawnEntityAction("53f81448-4ecc-4af32525a5-b7cc-b770d92ebebc") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(1920,4224, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }
                    },
                    new ActionSetType ("0eafa52204-64d5-460b-9723-c6efdd4bb4db")
                    {
                        Actions = new []
                        {
                            new SpawnEntityAction("14c3165e-e87f-4dawf25526-b1cb-da9e503e86df") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                Location = new Vector3(2016,4944, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }
                    },
                    new ActionSetType ("6a2525a64da-4d35-46ae-9304-ab0e522eefa4")
                    {
                        Actions = new []
                        {
                            new SpawnEntityAction("88b208b9-afe1-4fa250-9718-c4fb6ef8c2b9") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod5", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(2688,4512, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }
                    },
                    new ActionSetType("c2321887-d2e8-4dawf532ws-73d7d00efcd8")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("453500fb-e178-4b13-bwafsafa3fa571945eb") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(2064,4704, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }    
                    },
                    new ActionSetType("1bea6afa252ca9-489f-9654-0aegagaha7958")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("1f2d70e9-awf2525-ae13-657b72031cba") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(1584,4560, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }    
                    },
                    new ActionSetType("6d07dcb3-f531-40e0-9fa53252c-778aa233e6a2")
                    { 
                        Actions = new EventActionType[]
                        { 
                            new SpawnEntityAction("53f81448-4ecc-40a5-af252acc-b770d92ebebc") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(1920,4224, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }
                    },
                    new ActionSetType ("0e210d04-6af252-460b-9723-c6efdd4bb4db")
                    {
                        Actions = new []
                        {
                            new SpawnEntityAction("14c3165e-e87f-4d16-bfaw2525-da9e503e86df") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                Location = new Vector3(2016,4944, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult }  }},
                        }
                    },
                    new ActionSetType ("6b3f64da-4d35-46ae-93afa25524-ab0e522eefa4")
                    {
                        Actions = new []
                        {
                            new SpawnEntityAction("88b208b9-afe1-4560-fa258-c4fb6ef8c2b9") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod5", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(2688,4512, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } }, },
                        }
                    }
                }
            });
            #endregion

            #region leafcutter Quadite
            #region  #1
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnLeafcutter#1",
                SetsOfActions = new []
                {
                    #region spawns
                    new ActionSetType("d71132d7-1afrr36547y4aff-ab9f211e6a8c")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("9987agehq25336ytae-4f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(1728,1008)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#1" },
                                        Location = new Vector3(1584,1296, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }    
                    },/*
                    new ActionSetType ("78d6ba84-c8waf35ae-98ab-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new EventActionType("99awsg356yagtfae-4f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(480,816)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#1" },
                                        Location = new Vector3(3264,1920, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                }
                            },
                        }
                    },
                    new ActionSetType ("d1a58d29a3537sfsdb-a00b-5100e0c7d673")
                    {
                        Actions = new EventActionType[]
                        {
                            new EventActionType("99875cc0-30ae-4f81-8532tafggtwdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(2448,96)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#1" },
                                        Location = new Vector3(2448,96, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                }
                            },
                        }
                    },*/
                    #endregion
                }
            });
            #endregion

            #region #2
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnLeafcutter#2",
                SetsOfActions = new []
                {
                    #region spawns
                    new ActionSetType ("78d6ba84-c8ahwsfaet359b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875cc0-30ae-4f81-253tefat45r6hyuiopppqq06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(1536,1536)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#2" },
                                        Location = new Vector3(2496,240, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },/*
                    new ActionSetType ("78d6ba84-c861-4afeatb-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new EventActionType("99875ccawfa53ytae-4f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(2304,1680)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#2" },
                                        Location = new Vector3(2880,96, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                }
                            },
                        }
                    },*/
                    #endregion
                }
            });
            #endregion

            #region #3

            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnLeafcutter#3",
                SetsOfActions = new []
                {
                    #region spawns
                    new ActionSetType ("78d6ba84-c861-a5r325tfvedf67a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875cc0-afw3526t3agq3wtyhyeaav-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(2496,288)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#3" },
                                        Location = new Vector3(3600,1248, 0), Bulk = 0.21f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                    new ActionSetType ("78d6ba84-c861-4at353taf0cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875awf3526qtafw81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(384,2592)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#3" },
                                        Location = new Vector3(432,912, 0), Bulk = 0.21f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                    #endregion
                }
            });
            #endregion

            #region #4

            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnLeafcutter#4",
                SetsOfActions = new []
                {
                    #region spawns
                    new ActionSetType ("78d6ba84-a523tafvfee-98ab-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875cc0-30ae-4f81-523afagewf739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(2784,1632)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#4" },
                                        Location = new Vector3(2784,1632, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    }
                    #endregion
                }
            });
            #endregion

            #region #5

            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnLeafcutter#5",
                SetsOfActions = new []
                {
                    #region spawns
                    new ActionSetType ("78d6baxz84-c861-49ee-98ab-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("998zx75cc0-30ae-4f81-82fe-06cdc739ecfe")
                            {
                                
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(912,1248)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#5" },
                                        Location = new Vector3(912,1248, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                    #endregion
                }
            });
            #endregion
            #endregion
            #endregion

            #region special action
            double nestSpawnInterval = 2400d; //mp july 2016 was 1600d   but i think it should take a bit longer before nest respawns.
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.AllValid,
                KeyName = "bigBombActivated",
                ActionTargets = new TargetObject()
                {
                    TargetObjectType = TargetObjectType.TargetEntity
                },
                SetsOfActions = new []
                {
                    new ActionSetType("d92a701d-ab98-447d-9b02-7cc53515y46bfbe39f5")
                    {                                                       
                        Actions = new EventActionType[]
                        {
                            new ParticleEffectAction("91eff242fe191-d051-43crafdge-pp859f-24a284ce1504")
                            {
                                DelayInSeconds = 5,
                               
                                    UseLocationOfEntity = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    DurationInSeconds = 0.4d,
                                    ParticleEmitters = new[]
                                    {
                                        new ParticleEmitterEffect()
                                        {
                                            AttachToEntity = false,
                                            ParticleSystemKey = "explosionSmokeCloud",
                                        }
                                    }
                                
                            },
                            new ParticleEffectAction("91efe191-d05133-qqq43ce-85rwr9f-24a284ce1504")
                            {
                                DelayInSeconds = 5,
                               
                                    UseLocationOfEntity = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    DurationInSeconds = 12d,
                                    ParticleEmitters = new[]
                                    {
                                        new ParticleEmitterEffect()
                                        {
                                            AttachToEntity = false,
                                            ParticleSystemKey = "explosionSmokeCloudLong",
                                        }
                                    }
                                
                            },
                            new ParticleEffectAction("91esfafe191-d051-77900043ce-835559f-24a284ce1504")
                            {
                                DelayInSeconds = 5,
                               
                                    UseLocationOfEntity = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    DurationInSeconds = 0.4d,
                                    ParticleEmitters = new[]
                                    {
                                        new ParticleEmitterEffect()
                                        {
                                            AttachToEntity = false,
                                            ParticleSystemKey = "explosion",
                                        }
                                    }
                                
                            },
                            new ParticleEffectAction("asf391e5353fe191-d051sa-43ce-859f-24a28adsawr-4ce1504")
                            {
                                DelayInSeconds = 5,
                               
                                    UseLocationOfEntity = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    DurationInSeconds = 0.4d,
                                    ParticleEmitters = new[]
                                    {
                                        new ParticleEmitterEffect()
                                        {
                                            AttachToEntity = false,
                                            ParticleSystemKey = "mineExplosion",
                                        }
                                    }
                                
                            },
                            new SoundEffectAction("91efe191-ddas6051-4sadaf3ce-833332259f-24a284ce1504")
                            {
                                DelayInSeconds = 5d,
                                
                                    KeyName ="traps/mineExplosionHardwDebris",
                                    Sound = "traps/mineExplosionHardwDebris"
                                
                            },
                            new DestroyEntityAction("ipedf63ea9-e4eui2-4c3iypyipljhl7-8312-7ce1944a9d9e")
                            {
                                DelayInSeconds = 5.5d, // make sure the particles have been started before destroying the target, otherwise the particles won't have a start location
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                
                            },
                        }
                    },
                    #region respawn #1
                    new ActionSetType("d92a701d-ab95sdad21335211674-d-9b02-7ccsfagcbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Leafcutter nest (coord. 33;27)"
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edf63jhlea9-e4jhlhjle2-4glglflc37-8312-7ce1944a9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,
                                   
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 33;27)",
                                            Location = new Vector3(1584,1296, 0),
                                            Threat = new Threat() { ThreatGroupName = "twinklerAllegianceNorth" } //test
                                        }
                                    
                            },
                            new SetPropertyAction("edf6llju3ea9-e4ehukhjkfk2-4lulihc37-8312-7ce1944a9d9e")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest1",Value = new ValueNode(){Bool = true }
                                
                            }
                        }
                    },
                    #endregion
                    #region respawn #2
                    new ActionSetType("d92a70-1dsaaw3511163-ab98-447d-9b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Leafcutter nest (coord. 52;5)"
                            }
                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edf63hjgjyiea9-e4e2-4c37-83ytihgf12-7ce1944a9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,
                                   
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 52;5)",
                                            Location = new Vector3(2496,240, 0),
                                            Threat = new Threat() { ThreatGroupName = "twinklerAllegianceNorth" } //test
                                        }
                                    
                            },
                            new SetPropertyAction("edf63efjgrjjfjfja9-e4e2-ttrgjfiuoyuupu4c37-8jfjft312-7ce1944a9d9e")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest2",Value = new ValueNode(){Bool = true }
                                
                            }
                        }
                    },
                    #endregion
                    #region respawn #3
                    new ActionSetType("d92a701d-ab98-447d-9b02-7ccc466gdhsxsbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Leafcutter nest (coord. 75;26)"
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edhjliuf63ea9-e4e2-4uyy5856yc37-8ypy312-7ce1944a9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,
                                   
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 75;26)",
                                            Location = new Vector3(3600,1248, 0),
                                            Threat = new Threat() { ThreatGroupName = "twinklerAllegianceNorth" } //test
                                        }
                                    
                            },
                            new SetPropertyAction("edf63gjhgj65473rhdfea9-e4e2-4c68e4yr37-8312-7ce1944a9d9e")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest3",Value = new ValueNode(){Bool = true }
                                
                            }
                        }
                    },
                    #endregion
                    #region respawn #4
                    new ActionSetType("d92252535a701d-aad325b98-447d-9bee502-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Leafcutter nest (coord. 58;34)"
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edf63edfh4a9-e4ewt4wsg2-4c372121qw-8312-7ce1944a9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,
                                   
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 58;34)",
                                            Location = new Vector3(2784,1632, 0),
                                            Threat = new Threat() { ThreatGroupName = "twinklerAllegianceNorth" } //test
                                        }
                                    
                            },
                            new SetPropertyAction("edf6wf3232223ea9-e4e2-4csdf2fqf37-8312-7ce1944a9d9e")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest4",Value = new ValueNode(){Bool = true }
                                
                            }
                        }
                    },
                    #endregion
                    #region respawn #5
                    new ActionSetType("d92a701d-ab98-447d-9b02-7cccbfsaf366be39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Leafcutter nest (coord. 19;26)"
                            }
                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edfsaf325263ea9-e4e2-4casf253237-852312-7ce1944a9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,
                                   
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 19;26)",
                                            Location = new Vector3(912,1248, 0),
                                            Threat = new Threat() { ThreatGroupName = "twinklerAllegianceNorth" } //test
                                        }
                                    
                            },
                            new SetPropertyAction("edfwqr4354t4663sgs252ea9-e4e5afs235222352-4c37-83sfs5212-7ce1944a9d9e")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest5",Value = new ValueNode(){Bool = true }
                                
                            }
                        }
                    },
                    #endregion
                    #region property #1
                    new ActionSetType("d92a12455sfafepp701d-ab98-447d-9b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Leafcutter nest (coord. 33;27)"
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                                new SetPropertyAction("4e8dgfawa25252aghzjd56666665gjdgjdg503c7")
                                {
                                    
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest1",Value = new ValueNode(){Bool = false }
                                    
                                }
                        }
                    },
                    #endregion
                    #region property #2
                    new ActionSetType("d92awq23525701d-absdad98-447d-9b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Leafcutter nest (coord. 52;5)"
                            }
                            
                        },
                        Actions = new EventActionType[]
                        {
                                new SetPropertyAction("4e8dghjdgjafwa5225zjd56666665gjdgjdg503c7")
                                {
                                    
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest2",Value = new ValueNode(){Bool = false }
                                    
                                }
                        }
                    },
                    #endregion
                    #region property #3
                    new ActionSetType("d9251515asfgtytuia701d-ab98-4safa47d-9b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Leafcutter nest (coord. 75;26)"
                            }
                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SetPropertyAction("edf6afs32263ea9-e4e2-4322c37-624638312-7c5235dage1944a9d9e")
                            {
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest3",Value = new ValueNode(){Bool = false }
                                
                            }
                        }
                    },
                    #endregion
                    #region property #4
                    new ActionSetType("d92a7saf362601d-abdad121198-447d-9b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Leafcutter nest (coord. 58;34)"
                            }
                            
                        },
                        Actions = new EventActionType[]
                        {
                                new SetPropertyAction("4e8dghjdgjdfaw255252d56666665gjdgjdg503c7")
                                {
                                    
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest4",Value = new ValueNode(){Bool = false }
                                    
                                }
                        }
                    },
                    #endregion
                    #region property #5
                    new ActionSetType("d9262462a701d-ab98-447d-faa6469b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Leafcutter nest (coord. 19;26)"
                            }
                            
                        },
                        Actions = new EventActionType[]
                        {
                                new SetPropertyAction("4e8dghjdgjdghzjd566fa25252afsdgjdg503c7")
                                {
                                    
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest5",Value = new ValueNode(){Bool = false }
                                    
                                }
                        }
                    },
                    #endregion
                }
            });
            #endregion
            return list;
         
        }
    }
}
