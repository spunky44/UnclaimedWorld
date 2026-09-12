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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data
{
    public class PolledEventsLoader
    {

        public static List<InGameEvents.PolledEventType> Init()
        {
            List<PolledEventType> list = new List<PolledEventType>();

            #region MUCKROOTMAP_musicTrackList //   mp copied from SANDBOXNOMADMAP
            list.Add(new PolledEventType()
            {
                KeyName = "MUCKROOTMAP_musicTrackList",
                PollInterval = new ValueNode() { Decimal = 3200f }, //cycle 2 days
                AllowRandomTimeOffset = false, //no staggering
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("2452dfjdfujr6ysdt75ud6jhufgdhb83")
                    {                       
                        Actions = new EventActionType[]
                        {          
                            new MusicAction("faf18teyudtydurytsusrtyusr5ab")
                            {
                                Comments = "duration  -  5 min 2 sec = 302 sec   duration number of days = 0,189.",                               
                                    Song = "Jesper Lundager - Building a Home_320" 
                                
                            },
                            new MusicAction("a8yujdtjyrtydujytdjd4473aa068")
                            {   
                                DelayInSeconds = 302f,                               
                                Comments = "duration -  4 min 19 sec =  259 sec . number of days: 1/1600=0,000625 *259 = 0,162",
                                Song = "Jesper Lundager - Prosperous Frontier_320"                                  
                            },
                            new MusicAction("a8dtyujdhgjhgdjhgdjytreewtyuw5rtyutywrutywraa068")
                            {  
                                Comments = "duration -  3 min 19 sec  = 199 sec ...number of days: 1/1600=0,000625 *199 = 0,124",
                                DelayInSeconds = 561f,                                
                                Song = "Jesper Lundager - Muckroot Toil_320"                                 
                            },
                            new MusicAction("atyutydutyjudytjtysdurtydsu5sw65eswtywraa068")
                            {  
                                Comments = "RelativeNoOfDays = 0.475  // dark at 0.5? (=1.0)",
                                DelayInSeconds = 760f,//302f+259f+199f                               
                                Song = "Jesper Lundager - Cetian Skies_320"  // duration - 4 min 42 sec   = 282 sec ...hard cropped: 4:34 = 274 sec........number of days: 1/1600=0,000625 *282 = 0,176.....0,171 (hard cropped)
                                
                            },
                            new MusicAction("addtyujdytujdtyjtydjid7eyudygjutsduyudtyu68")
                            {   
                                Comments = "RelativeNoOfDays = 0.646   night",
                                DelayInSeconds = 1034f, ////302f+259f+199f+274f    hard cropped previous
                                Song = "Martin Hasseldam - Unfamiliar Starlight"  // duration 3 min 58 sec = 238 sec .......number of days: 1/1600=0,000625 *238 = 0,149
                                
                            },
                            new MusicAction("adkjglkjglgkjlgjkuteyudtyutydtyudtyu4674674674tyu68")
                            {   
                                Comments = "RelativeNoOfDays = 0.795  // morning",
                                DelayInSeconds = 1272f, //302f+259f+199f+274f+238f                            	
                                Song = "Martin Hasseldam - Life in the Wilderness"  // duration 9 min 10 sec = 550 sec. ...with a lot of the end silence cropped, it's: 8 min 44 sec = 524 sec........number of days: 1/1600=0,000625 *550=0,344  ...Cropped: 0,000625 * 524= 0,328
                                
                            },
                            new MusicAction("wgjklkjlgjklgjkltwywrtytyutydtyudtyu4674674674tyu68") //RelativeNoOfDays = 1.123  //afternoon
                            {   
                                Comments = "",
                                DelayInSeconds = 1796f, //302f+259f+199f+274f+238f+524f    hard cropped previous                               
                                Song = "Martin Hasseldam - Settle"  // duration 15 min 40 sec = 940 sec  ....BUT, it has a lot of silence in the end (after 15 min 15 sec), so the more accurate, cropped lenght is 915 sec......number of days:  1/1600=0,000625 *915=0,572  (cropped)
                                
                            },
                            new MusicAction("ygkjlkjglgjkljkuityutyutyudtyudtyu4674674674tyu68") //night  RelativeNoOfDays = 1.695  //
                            {   
                                DelayInSeconds = 2711f, //302f+259f+199f+274f+238f+524f+ 915f hard cropped previous                            	
                                Song = "Jesper Lundager - The Diamond Birds_320"  //  duration - 3 min 1 sec    = 181 sec ........number of days: 1/1600=0,000625 *181 = 0,113
                                
                            },
                            new MusicAction("kadfgfdagadfgaf67iyuiyuiyuiyuidtyudtyu4674674674tyu68")
                            {   
                                DelayInSeconds = 2892f, //302f+259f+199f+274f+238f+524f+ 915f+181f                               
                                Song = "Martin Hasseldam - A New World (Alt3) 320kBit"  //  duration: 5 min 12 sec =312 sec ...hard cropped: 5 min 4 sec = 304 sec........number of days: 1/1600=0,000625 *312= 0,195 .....0,190 (hard cropped)
                                
                            }
                                 //cycle ends at 2892f+312= 3204f  ..so 4 seconds cut off this last track which is ok.
                        }
                    }
                }
                }
            });
            #endregion

            #region Deaths, immigration and supply drops
          
            
            // burialOccurred
            list.Add(new PolledEventType()
            {
                KeyName = "showEndScenarioPromptAfterBurial",
                PollInterval = new ValueNode() { Decimal = 1f },  // seconds   
                  
                Condition = new ConditionFunction()
                {
                    Operator = OperatorType.And,
                    Left = new CustomCondition() { PropertyCondition = new PropertyCondition() { PropertyKey = "homebaseIsFriendly", BoolValue = true }   },
                    Right = new CustomCondition() { PropertyCondition = new PropertyCondition() { PropertyKey = "burialOccurred", BoolValue = true }  }                    

                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("e97919d6-83d0-4895-8764-8ed07d4391cd")
                    {    
                        Actions = new EventActionType[]{
           
                        new SetPropertyAction("8f475e27-cb99-4411-88de-53fff91d6944")
                        { 
                           
                                PropertyKey = "burialOccurred", // reset to false
                                 Value = new ValueNode() { Bool = false },
                             
                        },
                        new EventActionDialog("726ac8fc-bfe3-4b95-8457-ddc04c47cb31")
                        { 
                            
                                 /*We are welcome to abort the mission and go back. what do we choose?
No thank you- we will carry on our work- (this continues the game.)
*/
                                   DisplayText = new DynamicText(){ Text = "We are welcome to abort the mission and go back. what do we choose?" },
                                   DisplayImage = "Rescue",

                                   DialogOptions = new []{ new DialogOption() {  Text = "CONTINUE", Tooltip = "No thanks, we will continue our mission." },
                                                           new DialogOption() {  Text = "ABORT", Tooltip = "Yes, let's end it here.", ActionSet = "missionAborted" } 

                             }
                        }
                            }
                    }
                }
                }
            });


            #endregion


            #region MUCKROOTMAP Win/lose and timed exposition

            #region lose condition

            list.Add(new PolledEventType()
            {
                KeyName = "MUCKROOTMAP_loseGame",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new PlayerAllegiancePersons() // condition: all dead -MP
                    {
                        MaxMembers = 0
                    }
                ,
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("a85a34e8-9304-4240-aa11-1e661c4ac8cb")
                    {                       
                        Actions = new EventActionType[]
                        {          
                            new LoseGameAction("4742abae-3b47-41ca-83f7-71673f9ef428")
                            {
                                                          
                                    LoseScreenText = " \nWith the instinctive willpower of pioneers, humans strove to tame a planet whose instincts told it to resist. This duel went on for generations as hope was built, crushed and rebuilt, and lessons were repeatedly learned and forgotten. \n \nTime would tell if the human presence on Antheia was just a temporary incursion or if they were destined to dominate this biosphere just like Earth." 
                                
                            },
                            new MusicAction("11b0c9c0-08e9-4647-87ca-b5f774d72629")
                            {
                               
                                    Song = "Martin Hasseldam - Unfamiliar Starlight" 
                                
                            }
                        }
                    }
                }
                }
            });
            #endregion
            
            #endregion



            #region MUCKROOTMAP Beginning population spawn // TODO: make this an action instead

            ///Beginning place nests:  

            list.Add(new PolledEventType()
            {
                KeyName = "MUCKROOTMAP_placeNests",
               /* ConditionSet = new ConditionSet()
                {
                    Value = new TimeCondition()
                        {
                            RelativeNoOfDays = 0.0
                        }
                },*/
                ActionSetsKey = "placeNests"
            });
           

            ////Normal and hard fauna: Beginning animal population (spawn):

            list.Add(new PolledEventType()
            {
                KeyName = "MUCKROOTMAP_timedSpawnBeginningPopulationNormal", // TODO: make this an action instead
               /* ConditionSet = new ConditionSet()
                {
                    Value = new TimeCondition()
                        {
                            RelativeNoOfDays = 0.0
                        }
                }
                ,*/
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("de53dc48-ee8b-4579-b82b-103a7ce8eb50")
                    {     
                             
                        Actions = new EventActionType[]{

                //Bushdragons
            // on Amager:



            //Patrician Spawns:

#region PatricianSpawns
            new SpawnEntityAction("0936dda4-c34b-4ca4-986b-9b4600aa4be2") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:patrician", Name = "Patrician", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Patrician Allegiance#1" },
                                  Location = new Vector3(1848,1181, 0), Bulk = 5.5f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Zebra patrician"} }, },
                                new SpawnEntityAction("5bf36fa1-a4cd-49cf-ad73-1bc5a37d498c") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:patrician", Name = "Patrician",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Patrician Allegiance#2" }, 
                                    Location = new Vector3(835,1748, 0), Bulk = 4.7f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, }, 

  /*                              new EventActionType("61eb7a46-d9d4-4f6b-b5ae-19d0540c7a4a") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:patrician", Name = "Patrician",  Location = new Vector3(2304,1632, 0), Bulk = 5.0f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  AllegianceKey = "Patrician Allegiance#3", } }, }}, 
  */
                                new SpawnEntityAction("879f5c82-e70b-47af-bab0-58114eacc8ef") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:patrician", Name = "Patrician",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Patrician Allegiance#4" }, 
                                  Location = new Vector3(1152,2256, 0), Bulk = 4.8f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, }, 

                                new SpawnEntityAction("61079c7f-f2b2-426b-bdd1-4222f8ffc529") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:patrician", Name = "Patrician",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Patrician Allegiance#5" }, 
                                  Location = new Vector3(960, 3552, 0), Bulk = 5.5f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, }, 


                                new SpawnEntityAction("c62aab15-5b78-4761-b2f7-e2b9299abdc9") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:patrician", Name = "Patrician",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Patrician Allegiance#6" },
                                  Location = new Vector3(5424,3456, 0), Bulk = 4.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, }, 

                                new SpawnEntityAction("0ea2dda2-f99c-47f8-be31-47092fb17b4b") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:patrician", Name = "Patrician",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Patrician Allegiance#7" }, 
                                  Location = new Vector3(3984,2496, 0), Bulk = 4.8f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, }, 

                                new SpawnEntityAction("cf0e002c-981f-42cb-865b-1030df3e1b10") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:patrician", Name = "Patrician",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Patrician Allegiance#7" }, 
                                  Location = new Vector3(720,2160, 0), Bulk = 4.8f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, }, 
#endregion
#region TurnipSpawns
            new SpawnEntityAction("8bc9b967-e062-4286-b1d0-8355dfa63d7a") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "TurnipAllegiance" }, 
                                    Location = new Vector3(3360, 1008, 0), Bulk = 8, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },

                                new SpawnEntityAction("5be138c4-2d5c-4f0e-b204-e8a697b48e74") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "TurnipAllegiance" },
                                  Location = new Vector3(1440,96, 0), Bulk = 8, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },

                                new SpawnEntityAction("cef57d19-66d6-453e-8a1f-64f2d8d86829") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "TurnipAllegiance" },
                                  Location = new Vector3(3888,4416, 0), Bulk = 6, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },

                                new SpawnEntityAction("9a3b9865-b7e6-4399-b953-4ca9ccf85ab2") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "TurnipAllegiance" },
                                  Location = new Vector3(4128,4368, 0), Bulk = 5, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },

                                new SpawnEntityAction("1d351afd-05ae-4229-9115-eba93765044a") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "TurnipAllegiance" },
                                  Location = new Vector3(1488,1296, 0), Bulk = 5, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },

#endregion
#region BushDragonSpawns
                                new SpawnEntityAction("7c982eac-10bb-4531-92e3-3725a2c0afc5") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #80" },
                              Location = new Vector3(2160, 2400, 0), Bulk = 0.98f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },
                     

                            new SpawnEntityAction("dc14206c-ab78-40f5-b3be-f6241e542d4e") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #80" },
                              Location = new Vector3(2400, 2544, 0), Bulk = 1.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },



                             new SpawnEntityAction("7b215c2e-d556-4b80-80f5-900c0547361b") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #80" },
                              Location = new Vector3(4848,2448, 0), Bulk = 0.9f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },
#endregion
#region TwinklerSpawns
                            new SpawnEntityAction("6363a945-3d6f-42bb-b297-1ff2dd9011d8") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "Twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(1344,2352, 0), Bulk = 0.5f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, },
                            new SpawnEntityAction("ccda6e33-443e-44df-ac18-05c59618b690") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "Twinkler",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(1152,1296, 0), Bulk = 0.5f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, },
            new SpawnEntityAction("6eb20215-f892-44e4-bfc8-7c1e64c9184c") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "Twinkler",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(2976, 1248, 0), Bulk = 0.5f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter"} }, },
                            new SpawnEntityAction("29e65745-24f8-4323-a2e7-9b1462323a26") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "Twinkler",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(2736, 864, 0), Bulk = 0.6f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, },
                            new SpawnEntityAction("533125cd-7f6f-49d9-b3fa-a6c69f16a3e6") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "Twinkler",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(2688, 288, 0), Bulk = 0.7f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, },

                            new SpawnEntityAction("85e440dc-81ea-4720-8f7c-d7f6e33254a6") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "Twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(336, 2074, 0), Bulk = 0.5f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, },

                            new SpawnEntityAction("ff175b6c-cc9b-4df5-9d3f-2b46aa3574c3") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "Twinkler",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(5856,2352, 0), Bulk = 0.4f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, },

                            new SpawnEntityAction("5b69f792-8982-4639-ad1f-cd1eb87b0d5e") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "Twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                              Location = new Vector3(5760,3888, 0), Bulk = 0.6f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, },
#endregion
#region BinalRatSpawns
                             new SpawnEntityAction("0a51c013-d621-463d-b674-3a5b9f98f2f3") { DelayInSeconds = 9,
                            EntityData = new EntityData()
                                { EntityKey = "entity:binalRat", Name = "BinalRat11",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorthWest" },
                                  Location = new Vector3(1296,1344, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //sw
                                { AgeGroup = AIAgeGroup.Adult} }, },

                            new SpawnEntityAction("9fa25c93-68c4-46c6-bc27-0add6663cd55") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:binalRat", Name = "BinalRat6",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorthWest" },
                                  Location = new Vector3(1344,1344, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw
                                { AgeGroup = AIAgeGroup.Adult } }, },
#endregion
#region ThunderChickenSpawns
                new SpawnEntityAction("cc29a7b9-b66d-4336-8682-629af0e46d1f") { DelayInSeconds = 2,
            EntityData = new EntityData()
                { EntityKey = "entity:pygmyThunderChicken", Name = "Thunder Chicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceNorthWest" },
                  Location = new Vector3(432,2160, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw
                { AgeGroup = AIAgeGroup.Adult } }, },

            new SpawnEntityAction("c3d8d2c6-28e9-4547-92c3-482a2e351638") { DelayInSeconds = 8,
            EntityData = new EntityData()
                { EntityKey = "entity:whiteThunderChicken", Name = "Thunder Chicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceNorthWest" },
                  Location = new Vector3(1344,1344, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw   
                { AgeGroup = AIAgeGroup.Adult } }, },


                            new SpawnEntityAction("b574244f-536756375675637536-0832067cd35f") { DelayInSeconds = 1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:whiteThunderChicken", Name = "Thunder Chicken Bulky", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceNorthWest" }, //todo "bushDragonAllegianceSouth"
                                  Location = new Vector3(440, 2200, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, CasteKey = "male"} }, },

                                new SpawnEntityAction("4ac3a89tryutyudyudtyu96e3bb064") { DelayInSeconds = 1,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bajingan", Name = "Thunder Chicken Thin", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceNorthWest" }, //todo "bushDragonAllegianceSouth"
                                  Location = new Vector3(450, 2180, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, CasteKey = "male"} }, },

#endregion
#region BirdSpawns


                        new SpawnEntityAction("e363cda2-4859-4d97-bc54-0d56c184aa5d") { DelayInSeconds = 1,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" },
                              Location = Maps.MapManager.TileToWorldPosVector2(new Point(14, 62)).ToVector3(), Rotation = 167, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("c093719f-0856-4ba5-ab1b-382bd905becd") { DelayInSeconds = 1.85,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" },
                              Location = Maps.MapManager.TileToWorldPosVector2(new Point(16, 64)).ToVector3(), Rotation = 208, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("748700ca-0738-4f79-8ce8-d4deb83ecd9b") { DelayInSeconds = 2.6,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" },
                              Location = Maps.MapManager.TileToWorldPosVector2(new Point(15, 63)).ToVector3(), Rotation = 137, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("253b90c4-4846-4d59-bff1-28bf3027d101") { DelayInSeconds = 3.2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" },
                              Location = Maps.MapManager.TileToWorldPosVector2(new Point(16, 61)).ToVector3(), Rotation = 316, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

#endregion


#region MegapodSpawns

                            //firegrass forest SE:
                                new SpawnEntityAction("093635673dwa2542asf555555555565673aa4be2") { DelayInSeconds = 2,
                            EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Worm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Worm Allegiance#1" },
                                  Location = new Vector3(3892,2548, 0), Bulk = 6f, BioEntity = new Maps.MapEditor.BiologicalEntity() // close to camp: Location = new Vector3(1744,1496, 0)
                                { AgeGroup = AIAgeGroup.Adult } }, },

                                new SpawnEntityAction("093635673567355687ht5555565673aa4be2") { DelayInSeconds = 2,
                            EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Worm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Worm Allegiance#1" },
                                  Location = new Vector3(4800,816, 0), Bulk = 6f, BioEntity = new Maps.MapEditor.BiologicalEntity() // close to camp: Location = new Vector3(1744,1496, 0)
                                { AgeGroup = AIAgeGroup.Adult } }, },


#endregion



#region SnatcherSpawns

                            //desert
                                new SpawnEntityAction("09363rwytuhjstrjhsfgjhgfsjsfggjsyj4be2") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Snatcher", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance" },
                                  Location = new Vector3(1104,2832, 0), Bulk = 3f, BioEntity = new Maps.MapEditor.BiologicalEntity() // close to camp: Location = new Vector3(1744,1496, 0)
                                { AgeGroup = AIAgeGroup.Adult, CasteKey = "female" } }, },

                                new SpawnEntityAction("093srytu656576eiu7e5iutyiujsjhfsgbe2") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance" },
                                  Location = new Vector3(1200,3000, 0), Bulk = 3f, BioEntity = new Maps.MapEditor.BiologicalEntity() // close to camp: Location = new Vector3(1744,1496, 0)
                                { AgeGroup = AIAgeGroup.Adult, CasteKey = "female" } }, },

                                     new SpawnEntityAction("0933r6wytuhjrjhsfstgjhgfsjsfggjsyj4be2") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance" },
                                  Location = new Vector3(1150,2632, 0), Bulk = 3f, BioEntity = new Maps.MapEditor.BiologicalEntity() // close to camp: Location = new Vector3(1744,1496, 0)
                                { AgeGroup = AIAgeGroup.Adult, CasteKey = "male" } }, },


#endregion

                           
#region DemonTreeSpawns

                                //for tree demons, their autogenerated expeditions are not enough, we need to control size of expedition radii, so they dont move outside of forests,
                                //therefore we make expeditions spawns, see "spawnDemonTreeExpedition

                            //firegrass forest SE:
                                new SpawnEntityAction("0936356735673fwa2easd5555555565673aa4be2") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spoakDendront", Name = "Demon Tree", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#1" },
                                  Location = new Vector3(4224,2592, 0), Bulk = 1.3f, BioEntity = new Maps.MapEditor.BiologicalEntity() // close to camp: Location = new Vector3(1744,1496, 0)
                                { AgeGroup = AIAgeGroup.Adult } }, },

                            //way north:
                                new SpawnEntityAction("5b35677777777777777536753675367798c") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spoakDendront", Name = "Demon Tree",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#2" }, 
                                    Location = new Vector3(2419,558, 0), Bulk = 1.4f, BioEntity = new Maps.MapEditor.BiologicalEntity() 
                                { AgeGroup = AIAgeGroup.Adult } }, }, 

                        // close to camp: 
                                new SpawnEntityAction("5b35fdsgdhdfhgdfjdhdhdghjdhjghj7798c") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spoakDendront", Name = "Demon Tree",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "demonTreeAllegiance#3" }, 
                                    Location = new Vector3(1735,1648, 0), Bulk = 1.4f, BioEntity = new Maps.MapEditor.BiologicalEntity() 
                                { AgeGroup = AIAgeGroup.Adult } }, }, 


#endregion

 


#region SpikePlantSpawns
/*
                            //firegrass forest SE:
                                new EventActionType("093tetetetetetetetetetetete57656776555e2") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spikePlant", Name = "Ursinix", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Patrician Allegiance#1" },
                                  Location = new Vector3(3792,2548, 0), Bulk = 1.3f, BioEntity = new Maps.MapEditor.BiologicalEntity() // close to camp: Location = new Vector3(1744,1496, 0)
                                { AgeGroup = AIAgeGroup.Adult } }, }},

                                new EventActionType("5b3wrtyuyyyyyyyrywuttttttttttttt5673356535675367c") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spikePlant", Name = "Ursinix",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Patrician Allegiance#2" }, 
                                    Location = new Vector3(1296,2928, 0), Bulk = 1.4f, BioEntity = new Maps.MapEditor.BiologicalEntity() 
                                { AgeGroup = AIAgeGroup.Adult } }, }}, 

// close to camp: Location = new Vector3(1735,1648, 0)
                                new EventActionType("5dgdgdgdgdgdgdgdgdgdgdgdgdgdgdgjtyjuetj65e6c") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:spikePlant", Name = "Ursinix",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Patrician Allegiance#2" }, 
                                    Location = new Vector3(1488,2352, 0), Bulk = 1.4f, BioEntity = new Maps.MapEditor.BiologicalEntity() 
                                { AgeGroup = AIAgeGroup.Adult } }, }}, 

*/
#endregion






                        }
                    }
                    }
                }
            });


            //  easy fauna: Beginning animal population (spawn)..only a hunter:




            list.Add(new PolledEventType() // TODO: make this an action instead
            {
                KeyName = "MUCKROOTMAP_timedSpawnBeginningPopulationEasy",
               /* ConditionSet = new ConditionSet()
                {
                    Value = new TimeCondition()
                        {
                            RelativeNoOfDays = 0.0                          
                        }
                }
                ,*/
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("ee47e50e-deb0-4e3d-800c-a544639c659f")
                    {     
                             
                        Actions = new EventActionType[]{

                //Bushdragons
            // on Amager:
                        new SpawnEntityAction("4a3396fd-061c-414f-b192-13b4fb05b18e") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #80" },
                              Location = new Vector3(2160, 2400, 0), Bulk = 0.98f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },

                        new SpawnEntityAction("8b27f9c3-3de2-4331-85d6-de67f85c30a4") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #80" },
                              Location = new Vector3(2400, 2544, 0), Bulk = 1.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },


                            // close, to the East of camp:
                        new SpawnEntityAction("beb84854-5448-4da8-b654-bbf31bf5293c") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #80" },
                              Location = new Vector3(1632, 2160, 0), Bulk = 0.9f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },


//birds
                        new SpawnEntityAction("0be05359-5480-4138-94b4-cc3da6569668") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(448, 2719, 0), Rotation = 115, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("6c7f4665-b922-4e3f-9fa2-11d4853b476e") { DelayInSeconds = 1,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(489, 2734, 0), Rotation = 167, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("7ac057e0-bfea-4dce-86d6-a656e6ee121f") { DelayInSeconds = 1.85,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(565, 2869, 0), Rotation = 208, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("3a0e3496-09c4-4fb7-bf83-ad2308135723") { DelayInSeconds = 2.6,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(580, 2839, 0), Rotation = 137, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("59922eb7-2dd2-44d2-bf28-0699ce1167a1") { DelayInSeconds = 3.2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(626, 2854, 0), Rotation = 316, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("c2fd62b9-1bca-4a81-9248-eee0c04a7d90") { DelayInSeconds = 2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(659, 2908, 0), Rotation = 112, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

// MP moved thunder spawn to "spawnDestroyThunderChickens"





//: 
                        new SpawnEntityAction("cef097d2-1a3f-4826-8bea-19ed0cb4b04b") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                                Location = new Vector3(1008, 1632, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, },




//close, to the east: 
/*
                        new EventActionType("26d3b323-ac6a-48e8-aa90-e50d13e47627") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler5", Location = new Vector3(1968, 1968, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter", AllegianceName = "twinklerAllegiance", } }, }},

*/

                        }
                    }
                    }
                }
            });






            #endregion


            #region MUCKROOTMAP continual spawns 



            list.Add(new PolledEventType()
            {
                KeyName = "MUCKROOTMAP_continualSpawnThunderChickenNorthWest",
                PollInterval = new ValueNode() { Int = 32 /*Decimal = 340f*/  },  //  sec. .......(there's 1600 sec / day) 
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 32 }},
                AllowRandomTimeOffset = true,
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
                                ConstantStringEqual = "thunderChickenAllegianceNorthWest"
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
                                Int = 10
                            }
                                
                    }
                        
                },

                ActionSetsKey = "continualSpawnThunderChickenNorthWest"
            });

            list.Add(new PolledEventType()
            {
                KeyName = "MUCKROOTMAP_continualSpawnTwinklersNorthWestCrevice",
                PollInterval = new ValueNode() { PropertyKey = "twinklerSpawnIntervalSouth" /*Decimal = 340f*/ },  //  sec. .......(there's 1600 sec / day) 
                StartAfterInterval = true,              
                AllowRandomTimeOffset = true,
                Condition = new CustomCondition()
                        {
                            // don't exceed max number of binal rats:
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "allegiances",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "keyName",
                                        ConstantStringEqual = "twinklerAllegiance"
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

                ActionSetsKey = "continualSpawnTwinklersNorthWestCrevice"
            });

            list.Add(new PolledEventType()
            {
                KeyName = "MUCKROOTMAP_continualSpawnBinalRatsNorthWest1",
                PollInterval = new ValueNode() { Int = 30 /*Decimal = 340f*/  },  //  sec. .......(there's 1600 sec / day) 
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Int = 30 } },
                AllowRandomTimeOffset = true,
                Condition = new CustomCondition()
                        {
                            // don't exceed max number of binal rats:
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "allegiances",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "keyName",
                                        ConstantStringEqual = "binalRatAllegianceNorthWest1"
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
                                        Int = 15
                                    }
                                
                            }
                        
                },

                ActionSetsKey = "continualSpawnBinalRatsNorthWest1"
            });


            list.Add(new PolledEventType()
            {
                KeyName = "MUCKROOTMAP_continualSpawnBinalRatsNorthWest2",
                PollInterval = new ValueNode() { Int = 30 /*Decimal = 340f*/  },  //  sec. .......(there's 1600 sec / day) 
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Int = 30 } },
                AllowRandomTimeOffset = true,
                Condition = new CustomCondition()
                        {
                            // don't exceed max number of binal rats:
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "allegiances",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "keyName",
                                        ConstantStringEqual = "binalRatAllegianceNorthWest2"
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
                                        Int = 15
                                    }
                                
                            }
                        
                },

                ActionSetsKey = "continualSpawnBinalRatsNorthWest2"
            });


            list.Add(new PolledEventType()
            {
                KeyName = "MUCKROOTMAP_continualSpawnBinalRatsNorthWest3",
                PollInterval = new ValueNode() { Int = 30 /*Decimal = 340f*/ },  //  sec. .......(there's 1600 sec / day) 
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Int = 30 } },
                AllowRandomTimeOffset = true,
                Condition = new CustomCondition()
                        {
                            // don't exceed max number of binal rats:
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "allegiances",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "keyName",
                                        ConstantStringEqual = "binalRatAllegianceNorthWest2"
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
                                        Int = 15
                                    }
                                
                            }
                        
                },

                ActionSetsKey = "continualSpawnBinalRatsNorthWest3"
            });



            #endregion

            return list;
        }
    }
}
