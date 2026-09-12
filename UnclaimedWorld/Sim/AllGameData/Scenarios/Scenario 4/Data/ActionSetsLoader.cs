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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4.Data
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

//MP I would like to add more dialogsets to choose from, but it's not THAT simple..

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
                        DefaultText = "They'll be scuttling along Emerald river. Should we catch some?" }, //Let's catch some!
                       }
                        }, 
#endregion

                #region lesserwhipjaw spawns
                    new ActionSetType("d92apd-fpie701d-ab98-447d-9b02-7cccbfbe39f5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnAllegianceAction("bb1eedfb-8b66-4508-b25d-517e6846ef6f")
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
                                  Location = new Vector3(6120, 1440, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("6f04678674864786476reuijte7jdtyj8950f1") { DelayInSeconds = 0.6,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1440, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },
                            new SpawnEntityAction("04961fdtyjdt7tjddgtde56jueac51bd4") { DelayInSeconds = 0.2,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1440, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("ea7srtdtyjt76u6rudtyjudtub61b2c") { DelayInSeconds = 1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1488, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },                   
                /*            new EventActionType("4dd53ddtyu67e56u5e6u5e6ue8588213") { DelayInSeconds = 0.8,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1488, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, }},*/
//////////////////////////////////////
            /*                new EventActionType("d7t5426245645y54ws6y546yju498") { DelayInSeconds = 6.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1440, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, }},
                            new EventActionType("6f044sw56y4w6y754w6y343qe7jdtyj8950f1") { DelayInSeconds = 6.6,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1440, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, }},*/
                            new SpawnEntityAction("043q456t34q56tae5456jueac51bd4") { DelayInSeconds = 6.4,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1440, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
////////////////////////////////////////

                /*            new EventActionType("ea56y46y7restyesryrtswyyjudtub61b2c") { DelayInSeconds = 8,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1488, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, }},    */               
                            new SpawnEntityAction("4d54w6rewtyrsetysyaee6ue8588213") { DelayInSeconds = 8.8,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1488, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },
//////////////////////////////////
//////////////////////////////on the south side of the stream:
                            new SpawnEntityAction("04961fdt4fxgyhnj6rtysthj6w4ysr45y54wac51bd4") { DelayInSeconds = 0.2,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1750, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("easr45yrs5t6tdfydstyasetyrsttub61b2c") { DelayInSeconds = 1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1750, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },                   
                            new SpawnEntityAction("4dwrsrty64y765674w5767455464yw213") { DelayInSeconds = 0.8,
                            EntityData = new EntityData()
                                { EntityKey = "entity:lesserWhipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "lesserWhipjawAllegianceWest" },
                                  Location = new Vector3(6120, 1750, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
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
                    //MP I would like to add more dialogsets to choose from, but it's not THAT simple..

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
                    new ActionSetType("99352d92a70532532531afaxfsaafaf31d-abasfasf98-447d-9b02-7cccbfbe39f5")
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
                }
                            ,

#region      //North edge, center:
                            new SpawnEntityAction("d75675637856e78563785787867498") { DelayInSeconds = 5.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3456, 30, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("6f0467867486478647864786748950f1") { DelayInSeconds = 5.6,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3552, 28, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },
                            new SpawnEntityAction("04961f4c-9srtyues5e6rue56jueac51bd4") { DelayInSeconds = 5.2,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3500, 36, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("ea7srtyuh65e78uerthjryuyryub61b2c") { DelayInSeconds = 6,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3600, 28, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },                   
               /*             new EventActionType("4dd53dtyuteu657856eudtyrute8588213") { DelayInSeconds = 5.8,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(3650, 30, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, }},*/
#endregion

#region            ////////////north east corner:
                            new SpawnEntityAction("sfgjghfnjxdyrtjdrtyjds67498") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(6120, 624, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("sfgjhnxdgtrydtjufghx48950f1") { DelayInSeconds = 0.6,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(6120, 624, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },
                            new SpawnEntityAction("dhjxghnjxrsyhjugfshsfgheac51bd4") { DelayInSeconds = 0.2,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(6120, 724, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
             /*               new EventActionType("xsfgjhnxgfhjgfsjdtukub61b2c") { DelayInSeconds = 1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(6120, 724, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, }},                   
                            new EventActionType("xdgfhjfysdyfdghjxfvxfxte8588213") { DelayInSeconds = 0.8,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(6120, 724, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, }},*/
#endregion

#region          //south edge (Easy area))
                            new SpawnEntityAction("sfawfwa24rsf56dfghjthjddtyjdtyj8") { DelayInSeconds = 1.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("sfdghjd567raw24r5ejutudtgy0f1") { DelayInSeconds = 1.6,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },
                            new SpawnEntityAction("dhdghjdysfa2423a7dt6ydghgsdhj1bd4") { DelayInSeconds = 1.2,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("xsddghj6y75re68uasfa24256eu562c") { DelayInSeconds = 2,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(4704, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },                   
                  /*          new EventActionType("xddgdrt6u7dtyudtyudt13") { DelayInSeconds = 1.8,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(4704, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, }},*/
#endregion
#region                                //south edge (Easy area)) repeat
                            new SpawnEntityAction("sf56dfghjtwfa242rasfhjddtyjdtyj8") { DelayInSeconds = 3.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                            new SpawnEntityAction("sfdghjd5faw242asf67e6j65ejutudtgy0f1") { DelayInSeconds = 3.6,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },
                            new SpawnEntityAction("dhdghjdy7dwfa2443t6ydghgsdhj1bd4") { DelayInSeconds = 3.2,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(5040, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, } }, },
                  /*          new EventActionType("xsddghj6y75re68u56eu562c") { DelayInSeconds = 3,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(4704, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, }},                   
                            new EventActionType("xddgdrt6u7dtyudtyudt13") { DelayInSeconds = 3.8,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Bajingan4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bajinganAllegianceWest" },
                                  Location = new Vector3(4704, 6120, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, }},*/
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
                    new ActionSetType("d92a701d-ab9s358-453247d-9b02-7cccbfbe39f5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("edf63ea9-esaf3524e2-4c37-8312fas25-7ce1235ffs944a9d9e") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5130, 5907, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, },
                        }    
                    },
                    new ActionSetType("e8esaf2353aed9-7e56-43f8-8ad9-ac28f14ab0c1")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("asf21q3523af-0f8d-4cf0-a924-ef7e14b3e21b") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5824, 5604, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, },
                        }    
                    },
                    new ActionSetType("d92a701d-gdsbfsfeaab98-sd62447d-9b02-7cccbfbe39f5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("edf63ea9asd25233252c37-8312-7ce1944a9d9e") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5130, 5907, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, },
                        }    
                    },
                    new ActionSetType("e8e3aed9-7e235dfs56-43f8-8ad9-ac28f14ab0c1")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("d5e5a91c-0a62eda8d-4cf0-a924-ef7e14b3e21b") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", Name = "BushDragon2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouth" },
                                  Location = new Vector3(5824, 5604, 0), Bulk = 1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,} }, },
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
                                { AgeGroup = AIAgeGroup.Adult,} }, },
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
                    new ActionSetType("ef6e8basfqfef-0qwrqrafb-4b0f-8365-50ad3f301eba")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("1faw32523tae05-443b-431b-b379-d3dbf9d99229") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(1570,314, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" } }, },
                        }    
                    },
                    new ActionSetType ("983hyung74b-5120-4181-bea8-d0f7405c7d50")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("25safwqfw4bc6-e7cf-4a79-8644-945ce9e56ba4") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(849,734, 0), Bulk = 4.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Copper Turnip" } }, },
                        }
                    },
                    new ActionSetType("ef6e8bef-0aawf253fb-4b0f-8365-50ad3f301eba")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("1575ee05-asf235235-431b-b379-d3dbf9d99229") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(1570,314, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" } }, },
                        }    
                    },
                    new ActionSetType ("983hyungmin3b74b-5120-4181-bea8-d0f7405c7d50")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("25ee2bc6-easf23526f-4a79-8644-945ce9e56ba4") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(849,734, 0), Bulk = 4.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Copper Turnip" } }, },
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
                            new SpawnEntityAction("f39d3dffdaw242528-4613-9477-1372b18710fxdr98") 
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
                            new SpawnEntityAction("fdwsa242457c-0847-442d-ab93-c57fbsdrsr7f395de") 
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
                            new SpawnEntityAction("d913ba37-sewda2424a30-4970-91dd-aea79dd0a242") 
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
                                { AgeGroup = AIAgeGroup.Adult } }, },
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
                                { AgeGroup = AIAgeGroup.Adult } }, },
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
                                { AgeGroup = AIAgeGroup.Adult} }, },
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
                                { AgeGroup = AIAgeGroup.Adult } }, },
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
                            new SpawnEntityAction("91dwafwa2540e2-6774-4aa8-b1f6-86a2f352d843") 
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
                            new SpawnEntityAction("91dfzwafsa242390e2-6774-4aa8-bdse56-86a2f352d843") 
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
                            new SpawnEntityAction("91fwar2424a90e2-6774-4aa8-b1f6-86a2f352d843") 
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
                                { AgeGroup = AIAgeGroup.Adult} }, },
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
                                { AgeGroup = AIAgeGroup.Adult} }, },
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
                    new ActionSetType("3efawfb9-fd76-42b5-9a12-526b2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4asf32565-f9f8-4590-8dd9-3f0d081b487e") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#1" },
                                  Location = new Vector3(4992,624, 0), Bulk = 2f, BioEntity = new Maps.MapEditor.BiologicalEntity() //5725,3845
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }    
                    },
                    new ActionSetType ("8ed0a13c-8590-417e-a246-63d6c26bbe69")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("a2dd9aef-2011-4673-a7ca-99cf7a72c7c3") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#1" },
                                  Location = new Vector3(4336, 3657, 0), Bulk = 2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },
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
                    new ActionSetType("3eae1eb9-fsaf23523-42b5-9a12-526b2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cf65-saf62368-4590-8dd9-3f0d081b487e") { DelayInSeconds = 0.1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#2" },
                                  Location = new Vector3(4992,624, 0), Bulk = 2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
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
                    new ActionSetType("3fa235235b9-fd76-42b5-9a12-526zxzdb2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("fasfq2y67q06cf65-f9f8-4590-8dd9-3f0d08zxczg1b487e") 
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
                    new ActionSetType("3eae1eb9-wasfafa6-42b5-9a12-526zxzdb2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("49ag353acf65-f9f8-4590-8dd9-3f0d08zxczg1b487e") 
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
                    new ActionSetType("3eae1eb9-fd76-4aetwr-9a12-526zxzdb2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906agezafe65-f9f8-4590-8dd9-3f0d08zxczg1b487e") 
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
                    new ActionSetType("3eae1eb9-fd76-42b5-9asfqf3f2-526zxzdb2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cfaedg4ed-f9f8-4590-8dd9-3f0d08zxczg1b487e") 
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
                    new ActionSetType("3eae1eb9-fd76-42b5-9a12-526a3235zdb2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cf65-af3qahf8-4590-8dd9-3f0d08zxczg1b487e") 
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
                    new ActionSetType("waf433a56d7-1383-415d-92db-ab9f211e6a8c")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("9afw262cc0-30ae-4f81-82fe-06cdc739ecfe")
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
                    new ActionSetType ("7253qaf3fr4-c861-49ee-98ab-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("998af6523qgtaega0-30ae-4f81-82fe-06cdc739ecfe")
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
                    new ActionSetType ("d1fa353d29-c48e-45ab-a00b-5100e0c7d673")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875af5623qeagae-4f81-82fe-06cdc739ecfe")
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
                    new ActionSetType ("78d6ba84-c8625rwafggrvae-98ab-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875ccasfq6523tga4f81-82fe-06cdc739ecfe")
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
                    new ActionSetType("d71132d7-1af43665-415d-92db-ab9f211e6a8c")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("99875cafa53ae-4f81-82fe-06cdc739ecfe")
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
                    new ActionSetType ("78d6ba84-c861-49eeawfrsvaefr9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875cc0-3af263tgta-4f81-82fe-06cdc739ecfe")
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
                    new ActionSetType ("d1a58d29-c48e-4af35225-a00b-5100e0c7d673")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("4eefeeeeawd9875cc0-30ae-4f81-82fe-06cdc739ecfe")
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
                    new ActionSetType ("78d6ba84-c861-4221rfwfsaa567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("af32632qegag875cc0-30ae-4f81-82fe-06cdc739ecfe")
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
                    new ActionSetType("c23afw53255327-d2e8-4d44-a0f8-73d7d00efcd8")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("453500fb-wadwa5322b13-bc06-79a2571945eb") {
                            EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(2064,4704, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }    
                    },
                    new ActionSetType("1bea65f8-2asfwqa5329654-0aa5ba257958")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("1f2d7af353a59-4439-ae13-657b72031cba") {
                            EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(1584,4560, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }    
                    },
                    new ActionSetType("6d07dcasfa25325-f531-40e0-99fc-778aa233e6a2")
                    { 
                        Actions = new EventActionType[]
                        { 
                            new SpawnEntityAction("53awf52448-4ecc-40a5-b7cc-b770d92ebebc") {
                            EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(1920,4224, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }
                    },
                    new ActionSetType ("0e210d04-6745a32ard5-460b-9723-c6efdd4bb4db")
                    {
                        Actions = new []
                        {
                            new SpawnEntityAction("14c316wa252-e87f-4d16-b1cb-da9e503e86df") {
                            EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                Location = new Vector3(2016,4944, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }
                    },
                    new ActionSetType ("6b3f64da-4d35-4waf2552e-9304-ab0e522eefa4")
                    {
                        Actions = new []
                        {
                            new SpawnEntityAction("fasaw52208b9-afe1-4560-9718-c4fb6ef8c2b9") {
                            EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod5", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(2688,4512, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }
                    },
                    new ActionSetType("c2321887-faf32535e8-4d44-a0f8-73d7d00efcd8")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("45350awf5363e178-4b13-bc06-79a2571945eb") {
                            EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(2064,4704, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }    
                    },
                    new ActionSetType("1bea65f8-2ca9-489f-96asfw5t2352aba257958")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("1f2d70eawf59-4439-ae13-657b72031cba") {
                            EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(1584,4560, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }    
                    },
                    new ActionSetType("6d07dcb3-faf253531-40e0-99fc-778aa233e6a2")
                    { 
                        Actions = new EventActionType[]
                        { 
                            new SpawnEntityAction("53f81448-4efwaf2a52cc-40a5-b7cc-b770d92ebebc") {
                            EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(1920,4224, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }
                    },
                    new ActionSetType ("0e210d04-64d5-460b-9aw25253-c6efdd4bb4db")
                    {
                        Actions = new []
                        {
                            new SpawnEntityAction("14c3165e-eawfwa257f-4d16-b1cb-da9e503e86df") {
                            EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                Location = new Vector3(2016,4944, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }
                    },
                    new ActionSetType ("6b3f64da-2a524a35-46ae-9304-ab0e522eefa4")
                    {
                        Actions = new []
                        {
                            new SpawnEntityAction("88b208b9-a2a525asaf1-4560-9718-c4fb6ef8c2b9") {
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
                    new ActionSetType("d71132d7-1383-4af3665t3ag-92db-ab9f211e6a8c")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("935aedadt375cc0-30ae-4f81-82fe-06cdc739ecfe")
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
                    new ActionSetType ("78d62q5rqfagae-c861-49ee-98ab-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new EventActionType("9afa35350-30ae-4f81-82fe-06cdc739ecfe")
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
                    new ActionSetType ("d1a58d29-cwaf35a53b-a00b-5100e0c7d673")
                    {
                        Actions = new EventActionType[]
                        {
                            new EventActionType("998fsa25235c0-30ae-4f81-82fe-06cdc739ecfe")
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
                    new ActionSetType ("78d6ba84-aw2t3grefb-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("998afrawghjan3bcrfe-06cdc739ecfe")
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
                    new ActionSetType ("78d6ba825t3aghtjyuoip98ab-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new EventActionType("998aehsvnyec2ndxasfaegnev")
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
                    new ActionSetType ("782q14rqfas861-49ee-98ab-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875asfwaffag81-82fe-06cdc739ecfe")
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
                    new ActionSetType ("78d6ba84afwsvadfbjklkb-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("99875asfagaf81-82fe-06cdc739ecfe")
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
                    new ActionSetType ("78d6ba84-w3qafazafzfsee-98ab-a567a9b10cd5")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("998awfagdbaed-30ae-4f81-82fe-06cdc739ecfe")
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
            double nestSpawnInterval = 1600d;
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
                    new ActionSetType("d92a70rjgoiushg974w1d-ab98-447d-9b02-7cccbfbe39f5")
                    {                                                       
                        Actions = new EventActionType[]
                        {
                            new ParticleEffectAction("91e523hgjpxrr191-d051-43ce-859f-24a284ce1504")
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
                            new ParticleEffectAction("91efe191-d05da1-4533ce-859f-24a284ce1504")
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
                            new ParticleEffectAction("91efe1saf2591-d535051-43ce-8f59f-24a284ce1504")
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
                            new ParticleEffectAction("2591e55fe191-daf35051-43cffaafafe-859f-24sda284ce1504")
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
                            new SoundEffectAction("91efe1fag3691-d051-g43ce-859dsadf-24a284ce1504")
                            {
                                DelayInSeconds = 5d,                               
                                KeyName ="traps/mineExplosionHardwDebris",
                                Sound = "traps/mineExplosionHardwDebris"                                
                            },
                            new DestroyEntityAction("edf63ea9-e4eagfhh4c37-8312-7ce1944a9d9e")
                            {
                                DelayInSeconds = 5.5d, // make sure the particles have been started before destroying the target, otherwise the particles won't have a start location
                               
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                               
                            },
                        }
                    },
                    #region respawn #1
                    new ActionSetType("d92a701d-ab98-447dfihhhe-9b02-7cccbfbe39f5")
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
                            new SpawnEntityAction("edf63ea9-e4e2-afa3562622-7ce192652yeadfd9e")
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
                            new SetPropertyAction("edf6faf32525asf3ea9-e4e2-4cgdsdg37-8gdag4624ygiuog312-7ce1944a9d9e")
                            {                                
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest1",Value = new ValueNode(){Bool = true }                                
                            }
                        }
                    },
                    #endregion
                    #region respawn #2
                    new ActionSetType("d92a701d-ab98-447d-9b02-7cc25uh2adsapecbfbe39f5")
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
                            new SpawnEntityAction("edf63ea9-e4e2-4c37-8asffceafs626262635621944a9d9e")
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
                            new SetPropertyAction("edfsaf325259-e4e2-4affc37-83asa25212-7ce1944a9d9e")
                            {
                                
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest2",Value = new ValueNode(){Bool = true }
                                
                            }
                        }
                    },
                    #endregion
                    #region respawn #3
                    new ActionSetType("d935123932sd2a701d-asdsb98-447d-9b02-7cccbfbe39f5")
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
                            new SpawnEntityAction("edf6safaf23523523a9-e4e2-4c37-8312-fafaf527ce1944a9d9e")
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
                            new SetPropertyAction("edfasd253235a9-e4e2-4c3as7-8312-7ce1944a9d9e")
                            {
                                
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest3",Value = new ValueNode(){Bool = true }
                                
                            }
                        }
                    },
                    #endregion
                    #region respawn #4
                    new ActionSetType("d92a701d-abadqw98-4ff4347d-93asqqb02-7cccbfbe39f5")
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
                            new SpawnEntityAction("asedf63ea52529-e4e2safsaf-4c37-83dsadwqr12-7ce1944a9d9e")
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
                            new SetPropertyAction("edfsa525f63ea9-e4e2-4c37sad5-83155rhpp2-7ce1944a9d9e")
                            {
                                
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest4",Value = new ValueNode(){Bool = true }
                                
                            }
                        }
                    },
                    #endregion
                    #region respawn #5
                    new ActionSetType("d9zvzv2a701d-ab98-447d-zssa9b02-7cccbfxvzbe39f5")
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
                            new SpawnEntityAction("edf635325sfea9-efaf2525ytyi4e2-4c37-8312-7ce1944a9d9e")
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
                            new SetPropertyAction("edf63eyitiuoyupa9-e4eyupy2-4cuyo37-8312-7ce1944a9d9e")
                            {
                                
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest5",Value = new ValueNode(){Bool = true }
                                
                            }
                        }
                    },
                    #endregion
                    #region property #1
                    new ActionSetType("d92a70vvwvw1d-evegab98-447ddg-9asab02-7cccbfbe39f5")
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
                                new SetPropertyAction("4e8afa235awgfdghjdgjdghzjd56666665gjdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest1",Value = new ValueNode(){Bool = false }
                                    
                                }
                        }
                    },
                    #endregion
                    #region property #2
                    new ActionSetType("d92f32yypa701d-aasvfhb98-447waqqqd-sfa9b02-7cccbfbe39f5")
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
                                new SetPropertyAction("4e8dasfaw2532a2a5dghzjd56666665gjdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest2",Value = new ValueNode(){Bool = false }
                                    
                                }
                        }
                    },
                    #endregion
                    #region property #3
                    new ActionSetType("d92a7afafve01d-ab98-44fash7d-9b02-7cccbfafbe39f5")
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
                            new SetPropertyAction("edyuoof63ea9-yuoyoyo4e2-4c37-8yuoyo312-7ce1944a9d9e")
                            {
                                
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                    PropertyKey = "nest3",Value = new ValueNode(){Bool = false }
                                
                            }
                        }
                    },
                    #endregion
                    #region property #4
                    new ActionSetType("d92afa235a701d-ab98-44afa007d-9b02-7cccbfbe39f5")
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
                                new SetPropertyAction("4e8dghjafwa252ajd56666665gjdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest4",Value = new ValueNode(){Bool = false }
                                    
                                }
                        }
                    },
                    #endregion
                    #region property #5
                    new ActionSetType("d92a701d-ab98-44sada1417d-i76759b02-7cccbfbe39f5")
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
                                new SetPropertyAction("4e8dghjdawa254yaeghzjd56666665gjdgjdg503c7")
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
