using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.ClientSide.GameEvents;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.Client.Particles;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData
{
    public class PolledEventsLoader
    {
        public static List<PolledEventType> Init()
        {
            List<PolledEventType> list = new List<PolledEventType>();


            #region Fields of Tau Ceti - shared events


            #region Music

            #region music track duration calculations
            /*
I assume secondsPerDay = 1600 (found in DateAndTime.cs)

---------------------------------------------------------------------- 
"Martin Hasseldam - A New World (Alt3) 320kBit"
duration: 5 min 12 sec =312 sec ...hard cropped: 5 min 4 sec = 304 sec

number of days: 1/1600=0,000625 *312= 0,195 .....0,190 (hard cropped)
---------------------------------------------------------------------- 

"Martin Hasseldam - Life in the Wilderness" (original title was: Digital Love)
duration 9 min 10 sec = 550 sec. ...with a lot of the end silence cropped, it's: 8 min 44 sec = 524 sec
  
 
number of days: 1/1600=0,000625 *550=0,344  ...Cropped: 0,000625 * 524= 0,328
---------------------------------------------------------------------- 

"Martin Hasseldam - Settle"
duration 15 min 40 sec = 940 sec
BUT, it has a lot of silence in the end (after 15 min 15 sec), so the more accurate, cropped lenght is 915 sec
 
number of days:  1/1600=0,000625 *915=0,572  (cropped)
---------------------------------------------------------------------- 
             * //Melancholic Nightime/very early morning (because it accelerates.)
"Martin Hasseldam - Unfamiliar Starlight" (original title was: 5 Spheres )
duration 3 min 58 sec = 238 sec
             * 
number of days: 1/1600=0,000625 *238 = 0,149
---------------------------------------------------------------------- 
"Jesper Lundager - Building a Home_320"
duration  -  5 min 2 sec = 302 sec
         
number of days: 1/1600=0,000625 *302 = 0,189 
---------------------------------------------------------------------- 
             * could be nighttime because dreamy
"Jesper Lundager - Cetian Skies_320"             
duration - 4 min 42 sec   = 282 sec ...hard cropped: 4:34 = 274 sec
            
number of days: 1/1600=0,000625 *282 = 0,176.....0,171 (hard cropped)
----------------------------------------------------------------------             
"Jesper Lundager - Muckroot Toil_320"           
duration -  3 min 19 sec  = 199 sec
          
number of days: 1/1600=0,000625 *199 = 0,124
---------------------------------------------------------------------- 
"Jesper Lundager - Prosperous Frontier_320"
 duration -  4 min 19 sec =  259 sec  
           
number of days: 1/1600=0,000625 *259 = 0,162
---------------------------------------------------------------------- 
             * Could be nighttime  because its mysterious
"Jesper Lundager - The Diamond Birds_320"             
 duration - 3 min 1 sec    = 181 sec   
            
number of days: 1/1600=0,000625 *181 = 0,113
----------------------------------------------------------------------
 
             total track time: around 3200 sec 
  so we have enough for 2 days and nights 2X1600 sec          
 */
            #endregion

            #region SANDBOXNOMADMAP_musicTrackList //
            list.Add(new PolledEventType()
            {
                Comment = "Has hardcoded relative start times. Attempts to play night music at the appropriate time, but this will fail if the sceario starts at a different time",
                KeyName = "SANDBOXNOMADMAP_musicTrackList",
                PollInterval = new ValueNode() { Decimal = 3200f }, //cycle 2 days
                AllowRandomTimeOffset = false, //no staggering
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("245afrthyjuiop2-4370-4469-9c4e-a1ebcabe0b83")
                    {                       
                        Actions = new EventActionType[]
                        {          
                            new MusicAction("faf18teyudtyyyyyyyteyuteyutes3bab")
                            {
                               
                                    Song = "Jesper Lundager - Building a Home_320" // duration  -  5 min 2 sec = 302 sec   duration number of days = 0,189.
                                
                            },

                            new MusicAction("a84teyuye567u856eureewtyuw5rtyutywrutywraa068")
                            {   DelayInSeconds = 302f, //
                               
                                    Song = "Jesper Lundager - Muckroot Toil_320"  // duration -  3 min 19 sec  = 199 sec ...number of days: 1/1600=0,000625 *199 = 0,124
                                
                            },
                            new MusicAction("a5e67u56tuuuutysdurtydsu5sw65eswtywraa068") //RelativeNoOfDays = 0.475  // dark at 0.5? (=1.0)
                            {   DelayInSeconds = 501f,//302f+199f
                                
                                    Song = "Jesper Lundager - Cetian Skies_320"  // duration - 4 min 42 sec   = 282 sec ...hard cropped: 4:34 = 274 sec........number of days: 1/1600=0,000625 *282 = 0,176.....0,171 (hard cropped)
                                
                            },

                            new MusicAction("a84fff54060-6bb5-4176-8903-30d4473aa068")
                            {   DelayInSeconds = 775f,//302f+199f+274
                               
                                    Song = "Jesper Lundager - Prosperous Frontier_320"  // duration -  4 min 19 sec =  259 sec . number of days: 1/1600=0,000625 *259 = 0,162
                                
                            },

                            new MusicAction("ad6udtyuddgjid7eyudygjutsduyudtyu68")//RelativeNoOfDays = 0.646  // night
                            {   DelayInSeconds = 1034f, ////302f+199f+274f+259f    hard cropped previous
                               
                                    Song = "Martin Hasseldam - Unfamiliar Starlight"  // duration 3 min 58 sec = 238 sec .......number of days: 1/1600=0,000625 *238 = 0,149
                                
                            },
                            new MusicAction("adddddf6e7uteyudtyutydtyudtyu4674674674tyu68")//RelativeNoOfDays = 0.795  // morning
                            {   DelayInSeconds = 1272f, //302f+259f+199f+274f+238f
                               
                                    Song = "Martin Hasseldam - Life in the Wilderness"  // duration 9 min 10 sec = 550 sec. ...with a lot of the end silence cropped, it's: 8 min 44 sec = 524 sec........number of days: 1/1600=0,000625 *550=0,344  ...Cropped: 0,000625 * 524= 0,328
                                
                            },
                            new MusicAction("wretwertrewtyrtwywrtytyutydtyudtyu4674674674tyu68") //RelativeNoOfDays = 1.123  //afternoon
                            {   DelayInSeconds = 1796f, //302f+259f+199f+274f+238f+524f    hard cropped previous
                                
                                    Song = "Martin Hasseldam - Settle"  // duration 15 min 40 sec = 940 sec  ....BUT, it has a lot of silence in the end (after 15 min 15 sec), so the more accurate, cropped lenght is 915 sec......number of days:  1/1600=0,000625 *915=0,572  (cropped)
                                
                            },
                            new MusicAction("yttutyutyutyut6t7uityutyutyudtyudtyu4674674674tyu68") //night  RelativeNoOfDays = 1.695  //
                            {   DelayInSeconds = 2711f, //302f+259f+199f+274f+238f+524f+ 915f hard cropped previous
                               
                                    Song = "Jesper Lundager - The Diamond Birds_320"  //  duration - 3 min 1 sec    = 181 sec ........number of days: 1/1600=0,000625 *181 = 0,113
                                
                            },
                            new MusicAction("klkjkljhlk67ii67iyuiyuiyuiyuidtyudtyu4674674674tyu68")
                            {   DelayInSeconds = 2892f, //302f+259f+199f+274f+238f+524f+ 915f+181f
                               
                                    Song = "Martin Hasseldam - A New World (Alt3) 320kBit"  //  duration: 5 min 12 sec =312 sec ...hard cropped: 5 min 4 sec = 304 sec........number of days: 1/1600=0,000625 *312= 0,195 .....0,190 (hard cropped)
                                
                            }
                                 //cycle ends at 2892f+312= 3204f  ..so 4 seconds cut off this last track which is ok.
                        }
                    }
                }
                }
            });
            #endregion


            //////////////////////////////////////////////////

            #endregion


            #region intro dialogue Farmers
            //

            list.Add(new PolledEventType()
            {
                KeyName = "introDialogueFarming",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []
                    {
                        new ActionSetType("793567w56efa25646673hgioppxzsusrtyurysu20")
                        {
                            Actions = new EventActionType[]
                            {
                                new TalkAction("4e8dasfeafgudgkjsankugrnksjevnsjawdwdawpastyjutysdjr657e78ue585e6756e7c")
                                {
                                    
                                        TalkPriority = TalkAction.TalkActionPriority.High,
                                        CanTalkWhileFighting = false,
                                        CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                        TurnTowardsListeners = true, //mp I don't see this happening. he speaks with his back turned..
                                        ActionByAgent = ActionByAgent.OnlySpecific,
                                        NameOfSpeaker = "Castor Hernes", //
                                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                   //     LineKey = "crashQuestion",
                                        DefaultText = "These grasslands. Have you seen anything like it?" //
                                    
                                },
                                new TalkAction("ab94vfrcfxfxfwqqfpp65edrtyuhjdyrtuhjsytruysrt9a")
                                {
                                        DelayInSeconds = 3,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                      //      LineKey = "crashAnswer",
                                            DefaultText = "Very fertile, yeah." // 
                                        
                                },
                                new TalkAction("f6ddtyu65e78u5euiteduetsyursetyuty4")
                                {
                                        DelayInSeconds = 5,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
                                     //       LineKey = "crashConclusion",
                                            DefaultText = "Just a bit of hard work and this could all be fields!" 
                                        
                                },
                                new TalkAction("cetyu5adwadvaef67fxfueysdtrhjufsghsrfhjsyhtr0")
                                {
                                        DelayInSeconds = 8,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.OnlySpecific,
                                            NameOfSpeaker = "Castor Hernes", //
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                   //         LineKey = "crashSuggestion",
                                            DefaultText = "That's the spirit!" 
                                        
                                },
                            }
                        }
                    }
                }
            });
            #endregion

            #region intro dialogue Hunting
            //

            list.Add(new PolledEventType()
            {
                KeyName = "introDialogueHunting",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []
                    {
                        new ActionSetType("793srthgfshhhhhhhhhhrsrdyuxgfyhyxfghxfghxfg0")
                        {
                            Actions = new EventActionType[]
                            {
                                new TalkAction("4e8dstyjutysdjr657e78ue585e6756edfsgijdhzfpgidzfpogih")
                                 {
                                    
                                        TalkPriority = TalkAction.TalkActionPriority.High,
                                        CanTalkWhileFighting = false,
                                        CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                        TurnTowardsListeners = true, //mp I don't see this happening. he speaks with his back turned..
                                        ActionByAgent = ActionByAgent.OnlySpecific,
                                        NameOfSpeaker = "Castor Hernes", //
                                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                   //     LineKey = "crashQuestion",
                                        DefaultText = "This is turnip territory!" //
                                    
                                },
                                new TalkAction("ab94srt76rtyrtsytesgh07sygh08se7ryhysrt9a")
                                {
                                        DelayInSeconds = 3,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                      //      LineKey = "crashAnswer",
                                            DefaultText = "Yep. Some good hunting can be done here." // 
                                        
                                },
                                new TalkAction("csfgh54675333333333333333333333re7rt7rt7rtr0")
                                {
                                        DelayInSeconds = 5,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.OnlySpecific,
                                            NameOfSpeaker = "Castor Hernes", //
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                   //         LineKey = "crashSuggestion",
                                            DefaultText = "That's what we're here for. Let's see who takes down the first!" 
                                        
                                },
                            }
                        }
                    }
                }
            });
            #endregion

            #region intro dialogue HuntingChickens
            //

            list.Add(new PolledEventType()
            {
                KeyName = "introDialogueHuntingChickens", //used on the map 4y
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new[]
                    {
                        new ActionSetType("793srthjdtgsdtgsdtgsdtgsdtgsdtgsdtgsdtgsdtgsdtgshxfg0")
                        {
                            Actions = new EventActionType[]
                            {
                                new TalkAction("4e8dstyjuttyj56y7y7y7y7y7u5y7uhzfpgidzfpogih")
                                 {
                                    
                                        TalkPriority = TalkAction.TalkActionPriority.High,
                                        CanTalkWhileFighting = false,
                                        CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                        TurnTowardsListeners = true, //mp I don't see this happening. he speaks with his back turned..
                                        ActionByAgent = ActionByAgent.OnlySpecific,
                                        NameOfSpeaker = "Castor Hernes", //
                                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                   //     LineKey = "crashQuestion",
                                        DefaultText = "You'll see these woods are teeming with thunder chickens..." //
                                    
                                },
                                new TalkAction("ab94srt5e6ujdtyujhtdyjhesd5r6y7jryhysrt9a")
                                {
                                        DelayInSeconds = 3,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                      //      LineKey = "crashAnswer",
                                            DefaultText = "Our spring traps should find good use." // 
                                        
                                },
                                new TalkAction("csfgh54675rthjurtysjtysjurt7rt7rtr0")
                                {
                                        DelayInSeconds = 5,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.OnlySpecific,
                                            NameOfSpeaker = "Castor Hernes", //
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                   //         LineKey = "crashSuggestion",
                                            DefaultText = "Let's get those rawhides!" 
                                        
                                },
                            }
                        }
                    }
                }
            });
            #endregion

            #region intro dialogue Fishing
            //

            list.Add(new PolledEventType()
            {
                KeyName = "introDialogueFishing",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []
                    {
                        new ActionSetType("7fsghasrthdfuaghdyfaugoldfabhoglidfagda20")
                        {
                            Actions = new EventActionType[]
                            {
                                new TalkAction("4egdfa968gt9r7aetg9t9d6fatg97ag76rae9tgrae7c")
                                {
                                    
                                        TalkPriority = TalkAction.TalkActionPriority.High,
                                        CanTalkWhileFighting = false,
                                        CanTalkWhileSleeping = false,
                                        CanTalkWhileThreatened = false,
                                        TurnTowardsListeners = true, //mp I don't see this happening. he speaks with his back turned..
                                        ActionByAgent = ActionByAgent.OnlySpecific,
                                        NameOfSpeaker = "Castor Hernes", //
                                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                   //     LineKey = "crashQuestion",
                                        DefaultText = "What do you think? Is this our new home?" //
                                    
                                },
                                new TalkAction("ab954t96g53e97t684g3e96t78g89ae47tg98er7atgt9a")
                                {
                                        DelayInSeconds = 3,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                      //      LineKey = "crashAnswer",
                                            DefaultText = "I like the river." // 
                                        
                                },
                                new TalkAction("f6g8u574wth58047ygh807rtysdoyiutw0s95tygs4")
                                {
                                        DelayInSeconds = 5,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
                                     //       LineKey = "crashConclusion",
                                            DefaultText = "If there's fish, we stay. Else we move on." 
                                        
                                },
                                new TalkAction("csrtg895ys98y7ghs0r8et7ygoidusghpsiuhhfsr0")
                                {
                                        DelayInSeconds = 8,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.OnlySpecific,
                                            NameOfSpeaker = "Castor Hernes", //
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                   //         LineKey = "crashSuggestion",
                                            DefaultText = "I agree. We follow the carbon tail!" 
                                        
                                },
                            }
                        }
                    }
                }
            });
            #endregion

            #region intro dialogue Versatile
            //

            list.Add(new PolledEventType()
            {
                KeyName = "introDialogueVersatile", //easy difficulty
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []
                    {
                        new ActionSetType("7fsgha6y56rtyrtwyrtwyrtwyrtwyrtswya20")
                        {
                            Actions = new EventActionType[]
                            {
                                new TalkAction("4eetyutdyudtygyuh65rtu5eutyjuty7c")
                                {
                                    
                                        TalkPriority = TalkAction.TalkActionPriority.High,
                                        CanTalkWhileFighting = false,
                                        CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                        TurnTowardsListeners = true, //mp I don't see this happening. he speaks with his back turned..
                                        ActionByAgent = ActionByAgent.OnlySpecific,
                                        NameOfSpeaker = "Castor Hernes", //                                       
                                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,                                   
                                        DefaultText = "Well. Not bad, huh?" //
                                    
                                },
                                new TalkAction("abdtyuyttttttttttttju567eir68okiryuidtt9a")
                                {
                                        DelayInSeconds = 3,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                      //      LineKey = "crashAnswer",
                                            DefaultText = "This is a nice corner of the world." // 
                                        
                                },
                                new TalkAction("f6g8u5rstyhu6r4h4eyhusrtyhsryhsryetygs4")
                                {
                                        DelayInSeconds = 5,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
                                     //       LineKey = "crashConclusion",
                                            DefaultText = "I'm impressed. Good job finding this place." 
                                        
                                },
                                new TalkAction("csr5e675e7u5yeurtysyuyrsysrtyshhfsr0")
                                {
                                        DelayInSeconds = 8,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.OnlySpecific,
                                            NameOfSpeaker = "Castor Hernes", //
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                   //         LineKey = "crashSuggestion",
                                            DefaultText = "I cannot promise life'll be easy, but we're off to a good start!" 
                                        
                                },
                            }
                        }
                    }
                }
            });
            #endregion

            #region intro dialogue Throng (bushcraft)
            //

            list.Add(new PolledEventType()
            {
                KeyName = "introDialogueThrong",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []
                    {
                        new ActionSetType("793dfsghjghfsjxdgdte6y75edtyjd20")
                        {
                            Actions = new EventActionType[]
                            {
                                new TalkAction("4edgje7e6ddurtyjutysrjudfyjsfj7c")
                                {                                    
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                        CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true, //mp I don't see this happening. he speaks with his back turned..
                                    ActionByAgent = ActionByAgent.OnlySpecific,
                                    NameOfSpeaker = "Castor Hernes", //
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                //     LineKey = "crashQuestion",
                                    DefaultText = "Great to see you all again. Sorry for the delay!" //
                                    
                                },
                                new TalkAction("abdghj5e76eiyrtukjiddghjdhjdsfj9a")
                                {
                                    DelayInSeconds = 3,                                       
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                //      LineKey = "crashAnswer",
                                    DefaultText = "It's ok. We knew you'd probably get held up." //                                         
                                },
                                new TalkAction("f6dghj576e5ejdtyjdghjdghjdghjdgy4")
                                {
                                    DelayInSeconds = 6,                                       
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.OnlySpecific,
                                    NameOfSpeaker = "Castor Hernes", //
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    DefaultText = "So. This is a good place to settle?"                                         
                                },
                                new TalkAction("cet5e678utyhjdfgjdgfjdghjdgj675tjjy7r0")
                                {
                                    DelayInSeconds = 10,                                       
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    DefaultText = "Sure hope so. We got a lot of mouths to feed." //It better be.                                        
                                }
                            }
                        }
                    }
                }
            });
            #endregion

            #region intro Event screen
            list.Add(new PolledEventType()
            {
                KeyName = "introDialogueScreen",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.015
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("6f04af432652448674886748674864786478ee92")
                { 
                        Actions = new EventActionType[]
                        {

                      new EventActionDialog("5325vfvfvcxfdgtty56378563876487648644f4e51") { DelayInSeconds = 0, 
                        DisplayText = new DynamicText(){ Text = "CASTOR HERNES: \nAlright. This is how I see it: We have radio equipment. As soon as it's set up, we can get in touch with Starsnare Point and make a deal to bring supplies and more people that are willing to join. We just need to make a landing for a boat to moor. There's a good spot nearby that I'm sure you've seen. \n \nBut the transport is not going to be cheap and we don't have much money. \nSo, my suggestion is to wait until we have goods to sell. Of course, in order to trade we need to build a simple port to store the goods. \n \nLINSEY CATTIER: \nYeah. We should also make the most of the animal migrations that are happening this time of year. They will be migrating in flocks across this landscape. Bajingan and lesser whipjaw. We should get ready to bag as many as possible, their hides can be worth a lot. \n \nCASTOR HERNES: Talk it over and we'll see what we can agree on!" ,  //  
                        },
                        DisplayImage = "GroupMeeting",
                           
                      
                                },
                        }
                }
                }

                }
            });
            #endregion

            #region lose condition

            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXMAP_loseGame",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new PlayerAllegiancePersons() // condition: all dead -MP
                {
                    MaxMembers = 0
                }
                ,
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("24zdfcxvbnm456782-4370-4469-9c4e-a1ebcabe0b83")
                    {                       
                        Actions = new EventActionType[]
                        {          
                            new LoseGameAction("db7sad23571a4-fe7b-413d-a67a-1ec7bbe0c209") // I don't want any modal text here, just fade to main screen for the end game message  -MP
                            {
                                                          
                                    LoseScreenText = " \nWith the instinctive willpower of pioneers, humans strove to tame a planet whose instincts told it to resist. This duel went on for generations as hope was built, crushed and rebuilt, and lessons were repeatedly learned and forgotten. \n \nTime would tell if the human presence on Antheia was just a temporary incursion or if they were destined to dominate this biosphere just like Earth." 
                                 
                            },
                            new MusicAction("a8454060-6gggbb5-4176-8903-30d4473aa068")
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


            // TB Init 19.03.2015
            #region globalPropertyInitializer
                /*
                 * Sets the default properties that are used for all farmPlots.
                 * 
                 * FarmingUpdateInterval -> the time interval between each growth update on the individual plot
                 * weedGrowthSpeed -> how much the weed grows each interval 0..1 |0 = not at all, 1 = grows fully in one interval
                 * weedingJobTriggerLimit -> when weed has grown this much it will start the weeding job and display the asset
                 */
                #region farmingInitializers
            list.Add(new PolledEventType()
            {
                KeyName = "initializeGlobalFarmingProperties",
                StartTimePoint = new TimePoint()
                {
                    RelativeTimeInSeconds = new ValueNode()
                    {
                        Decimal = 0.5f //run 0.5 seconds after game start
                    }  
                },               
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []
                    { 
                        new ActionSetType("b7413ad2-6bb6-4856-8527-1a9b0f508c74")
                        {                       
                            Actions = new EventActionType[]
                            {
                                #region set property FarmingUpdateInterval to 16.0f
                                new SetPropertyAction("b0e4d7df-6d48-47c9-86d2-4a5b77192b62")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "FarmingUpdateInterval",
                                        Value = new ValueNode()
                                        {
                                            Decimal = 16.0f // in seconds
                                        }
                                    
                                },
                                #endregion
                                #region set property weedGrowthSpeed
                                new SetPropertyAction("f3a23fc7-9956-4ed6-324524tsgfhp-322f7885041f")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "weedGrowthSpeed",
                                        Value = new ValueNode()
                                        {
                                            Decimal = 0.025f // 0.06f 
                                        }
                                    
                                },
                                #endregion
                                #region set property weedingJobTriggerLimit to 0.45f
                                new SetPropertyAction("f3a23fc7-99564fdfeaf-eppetet-eta885041f")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "weedingJobTriggerLimit",
                                        Value = new ValueNode()
                                        {
                                            Decimal = 0.45f
                                        }
                                    
                                },
                                #endregion
                                #region set property maxTimeBetweenWeeding
                                new SetPropertyAction("16f435yuiklope9-dcfa-4de1-9dc7-80aagdd7f27ecabe")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "maxTimeBetweenWeeding",
                                        Value = new ValueNode()
                                        {
                                            Decimal = 200f // 60f
                                        }
                                    
                                },
                                #endregion
                                #region set property fertilizeJobTriggerLimit to 0.45f
                                new SetPropertyAction("f3axrf23fc7-9956-4ed6-998d-322f7885041f")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "fertilizeJobTriggerLimit",
                                        Value = new ValueNode()
                                        {
                                            Decimal = 0.45f
                                        }
                                    
                                },
                                #endregion
                                #region set property maxTimeBetweenFertilizing to 1 ig day
                                new SetPropertyAction("16e3254thkjppe9-dcfa-4de1-9dc7-80a7f27ecabe")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "maxTimeBetweenFertilizing",
                                        Value = new ValueNode()
                                        {
                                            Decimal = 1600f
                                        }
                                    
                                },
                                #endregion
                                #region set property fertilizeReductionFactor to 0.08
                                new SetPropertyAction("16e7d4xre9-dcfa-4de1-9dcasf565pp7f27ecabe")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "fertilizeReductionFactor",
                                        // 1/(cropGrowthSpeed   +   weedGrwothSpeed *   (1  /   cropGrowthSpeed)    *   harvestAmount)
                                        // 1/(0.02             +   0.06             *   (1  /   0.02)              *   4)
                                        Value = new ValueNode()
                                        {
                                            Decimal = 0.08f
                                        }
                                    
                                },
                                #endregion
                                //
                            }
                        }
                    }
                }
            });
                #endregion
                /*
                 * Sets the default properties that are used for all fishTraps.
                 * 
                 * FishTrapSpawningLoopInterval -> the time interval between each spawning on the individual trap
                 * FishTrapCheckingLoopInterval -> the time interval between each check on the individual trap
                 */
                #region fishTrapInitializers
            list.Add(new PolledEventType()
            {
                KeyName = "initializeGlobalFishTrapProperties",
                StartTimePoint = new TimePoint()
                {
                    RelativeTimeInSeconds = new ValueNode()
                    {
                        Decimal = 0.5f //run 0.5 seconds after game start
                    }  
                },              
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []
                    { 
                        new ActionSetType("b892083b-156d-4873-b2c9-95eaa6071837")
                        {                       
                            Actions = new EventActionType[]
                            {
                                #region set property FishTrapSpawningLoopInterval to 12.0f
                                new SetPropertyAction("b4958be2-2da3-405e-bdc6-06120d16beab")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "FishTrapSpawningLoopInterval",
                                        Value = new ValueNode()
                                        {
                                            Decimal = 12.0f // in seconds
                                        }
                                    
                                },
                                #endregion
                                #region set property FishTrapCheckingLoopInterval = xx
                                new SetPropertyAction("712465b0-2005-4139-a15f-b813fa236198")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "FishTrapCheckingLoopInterval",
                                        Value = new ValueNode()
                                        {
                                            Int = 65 //16
                                        }
                                    
                                },
                                #endregion
                                #region set property maxTimeBetweenFishCheckJobs = FishTrapSpawningLoopInterval * 7 // why is this needed???
                                new SetPropertyAction("16e7agdd4e9-dcfa-4de1-9dc7-80a7f624fjlopp27ecabe")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "maxTimeBetweenFishCheckJobs",
                                        Value = new FunctionNode()
                                        {
                                            Left = new ValueNode()
                                            {
                                                TargetObject = new TargetObject()
                                                {
                                                    TargetObjectType = TargetObjectType.Root
                                                },
                                                PropertyKey = "FishTrapSpawningLoopInterval"
                                            },
                                            Operator = ExpressionOperator.Multiply,
                                            Right = new ValueNode()
                                            {
                                                Decimal = 7.0f
                                            }
                                        }
                                       /* Value = new ValueNode() //bso to speed up testing
                                        {
                                            Int = 10
                                        }*/
                                    
                                },
                                #endregion
                            }
                        }
                    }
                }
            });
                #endregion
                /*
                 * Sets the default properties that are used for all AnimalTraps.
                 * 
                 * maxTimeBetweenAnimalTrapCheckJobs -> the time interval between each check on the individual trap
                 */
                #region animalTrapInitializers
            list.Add(new PolledEventType()
            {
                KeyName = "initializeGlobalAnimalTrapProperties",
                StartTimePoint = new TimePoint()
                {
                    RelativeTimeInSeconds = new ValueNode()
                    {
                        Decimal = 0.5f //run 0.5 seconds after game start
                    }                   
                },               
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []
                    { 
                        new ActionSetType("b8920834db-156d-4873-b2c9-95eaa60d71837")
                        {                       
                            Actions = new EventActionType[]
                            {
                                #region set property maxTimeBetweenAnimalTrapCheckJobs to 180.0f
                                new SetPropertyAction("b4958dgfhbe2-2da3-405e-bdc6-06120d16bdfgheab")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "maxTimeBetweenAnimalTrapCheckJobs",
                                        Value = new ValueNode()
                                        {
                                            Decimal = 180.0f
                                        }
                                    
                                },
                                #endregion
                                #region set property animalCheckCanTalk to true
                                new SetPropertyAction("6dsb4958dge2-2da3-405e-bdc6-06120dxdf")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "animalCheckCanTalk",
                                        Value = new ValueNode()
                                        {
                                            Bool = true
                                        }
                                    
                                },
                                #endregion
                            }
                        }
                    }
                }
            });
            #endregion
            #endregion

            #region Farming
           
         
            /*
             NEW: produces weeding jobs for the expedition. Attaches to Expedition
             * creates jobs for all the plots that are owned by the expedition and known about by the allegiance
             * we cannot put this event on the entity itself, since we want it to still run even if the entity is destroyed
             */
            #region weeding job loop
            list.Add(new PolledEventType()
            {
                KeyName = "weedingJobLoop",
                 #region PollInterval = FarmingUpdateInterval
                    PollInterval = new ValueNode()
                    {
                        TargetObject = new TargetObject()
                        {
                            TargetObjectType = TargetObjectType.Root
                        },
                        PropertyKey = "FarmingUpdateInterval"
                    },
                    #endregion
               /* ConditionSet = new ConditionSet()
                {
                   
                },*/
                ActionSets = new ActionSets()
                {
                      FireMode = ActionSetsToFire.AllValid,
                      ActionTargets = new TargetObject() // the actions will be invoked on all these matching items
                      {
                          TargetObjectType = TargetObjectType.PolledEventSource,
                          GetList = new GetList()
                          {
                              HasPropertiesListKey = "OwnedEntities",
                              FilterCondition = new PropertyCondition() { PropertyKey = "isFarmPlot", BoolValue = true }, //PropertyKey = "type", StringEqual = "structure:smallPlot" }, 
                              //NextList = new GetList()
                          }
                      },
                      SetsOfActions = new []
                    {
                        // weedingJob needs to be created
                        new ActionSetType("createWeedingJob")
                        {   
                            // we include the elapsed time check because if the plot is in the FOW, weedGrowthProgress won't get updated.
                             // "weedingLastTimeStamp" > ( "getTime" - maxTimeBetweenWeeding )
                            #region run if: (weedGrowthProgress > weedingJobTriggerLimit || timeElapsedSinceWeeding > maxTimeBetweenWeeding ) && cropsAreGrowing == true
                            Condition = new ConditionFunction()
                                {
                                    #region weedGrowthProgress > weedingJobTriggerLimit
                                    Left = new ConditionFunction()
                                         {
                                            Left = new CustomCondition()
                                            {
                                                TargetObject = new TargetObject()
                                                {
                                                    TargetObjectType = TargetObjectType.DynamicTarget // use the ActionTargets                                                    
                                                },
                                                PropertyCondition = new PropertyCondition()
                                                {
                                                    PropertyKey = "weedGrowthProgress",
                                                    NumberMinimumInclusive = new ValueNode()
                                                    {
                                                        TargetObject = new TargetObject()
                                                        {
                                                            TargetObjectType = TargetObjectType.Root
                                                        },
                                                        PropertyKey = "weedingJobTriggerLimit"
                                                    }
                                                }
                                              }, 
                                              Operator = OperatorType.Or,
                                              Right = new CustomCondition()
                                                {
                                                    TargetObject = new TargetObject()
                                                    {
                                                        TargetObjectType = TargetObjectType.DynamicTarget // use the ActionTargets                                                    
                                                    },                                                           
                                                       
                                                    PropertyCondition = new PropertyCondition()
                                                    {
                                                        PropertyKey = "weedingLastTimeStamp",
                                                        NumberMaximumNotInclusive = new FunctionNode()
                                                        {  // "weedingLastTimeStamp" > ( "getTime" - maxTimeBetweenWeeding )
                                                            Left = new ValueNode()
                                                            {
                                                                PropertyKey = "getTime"
                                                            },
                                                            Operator = ExpressionOperator.Minus,
                                                            Right = new ValueNode()
                                                            {                                                                       
                                                                TargetObject = new TargetObject()
                                                                {
                                                                    TargetObjectType = TargetObjectType.Root
                                                                },
                                                                PropertyKey = "maxTimeBetweenWeeding"
                                                            }                                                                    
                                                        }
                                                    }
                                                }
                                    },
                                    #endregion
                                    Operator = OperatorType.And,                                   
                                    Right = new ConditionFunction()
                                    {
                                        #region plantJobProcessKey != null
                                        Left = new CustomCondition()
                                        {
                                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.DynamicTarget },
                                            PropertyCondition = new PropertyCondition()
                                            {
                                                PropertyKey = "plantJobProcessKey",
                                                IsNull = false                                           
                                            }                                        
                                        },
                                        #endregion
                                        Operator = OperatorType.And,
                                        #region cropsAreGrowing == true
                                        Right = new CustomCondition()
                                        {
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.DynamicTarget
                                            },
                                            PropertyCondition = new PropertyCondition()
                                            {
                                                PropertyKey = "cropsAreGrowing",
                                                BoolValue = true
                                            } 
                                        }
                                        #endregion
                                    }    
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {
                                #region start weedPlot job
                                new CreateJobAction("c10ef45b-a390-487b-97f7-de3282fcc0ec")
                                {
                                    DelayInSeconds = 0,                                   
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.DynamicTarget
                                    },
                                    ProcessTypeKey = new ValueNode()
                                    { 
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.DynamicTarget },
                                        PropertyKey = "weedJobProcessKey" 
                                    }                                    
                                },                               
                                #endregion
                            }
                        }
                    }
                }
            });
            #endregion

            #region FertilizeLoop
            //Just a copy of weedplot job, make fertilize process
            list.Add(new PolledEventType()
            {
                KeyName = "fertilizeJobLoop",
                #region PollInterval = FarmingUpdateInterval
                PollInterval = new ValueNode()
                {
                    TargetObject = new TargetObject()
                    {
                        TargetObjectType = TargetObjectType.Root
                    },
                    PropertyKey = "FarmingUpdateInterval"
                },
                #endregion
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.AllValid,
                    ActionTargets = new TargetObject() // the actions will be invoked on all these matching items
                    {
                        TargetObjectType = TargetObjectType.PolledEventSource,
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "OwnedEntities",
                            FilterCondition = new PropertyCondition() { PropertyKey = "isFarmPlot", BoolValue = true },
                        }
                    },
                    SetsOfActions = new []
                    {
                        new ActionSetType("createFertilizeJob")
                        {   
                             #region run if: (nutrientLevel > fertilizeJobTriggerLimit && cropsAreGrowing == true)
                            Condition = new ConditionFunction()
                                {
                                    #region nutrientLevel > fertilizeJobTriggerLimit
                                    Left = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.DynamicTarget },
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "nutrientLevel",
                                            NumberMaximumNotInclusive = new ValueNode()
                                            {
                                                TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                                PropertyKey = "fertilizeJobTriggerLimit"
                                            }
                                        }                                        
                                    },
                                    #endregion
                                    Operator = OperatorType.And,
                                    #region cropsAreGrowing == true
                                    Right = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.DynamicTarget
                                        },
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "cropsAreGrowing",
                                            BoolValue = true
                                        }
                                        
                                        
                                    }
                                    #endregion
                                
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {                               
                                #region start fertilize job
                                new CreateJobAction("c10ef45b-a390-487b-97f7-de328zdgw2fcc0ec")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.DynamicTarget
                                        },
                                        ProcessTypeKey = new ValueNode()
                                        { 
                                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.DynamicTarget },
                                            PropertyKey = "fertilizeJob"
                                        }
                                    
                                },                               
                                #endregion
                            }
                        }                       
                    }
                }
            });
            #endregion

            #region HarvestJobLoop

            //Just a copy of weedplot job, make harvest job
            list.Add(new PolledEventType()
            {
                KeyName = "harvestJobLoop",
                #region PollInterval = FarmingUpdateInterval
                PollInterval = new ValueNode()
                {
                    TargetObject = new TargetObject()
                    {
                        TargetObjectType = TargetObjectType.Root
                    },
                    PropertyKey = "FarmingUpdateInterval"
                },
                #endregion
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.AllValid,
                    ActionTargets = new TargetObject() // the actions will be invoked on all these matching items
                    {
                        TargetObjectType = TargetObjectType.PolledEventSource,
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "OwnedEntities",
                            FilterCondition = new PropertyCondition() { PropertyKey = "isFarmPlot", BoolValue = true },
                        }
                    },
                    SetsOfActions = new[]
                    {
                        new ActionSetType("createHarvestJob") 
                        {   
                            Comments = "crops finished growing, run if: cropGrowthElapsedTime > cropGrowthPeriod AND cropGrowthProgress > 0",
                            #region run if: cropGrowthElapsedTime > cropGrowthPeriod AND cropGrowthProgress > 0 TODO: also see if plant order is still active??? or does this make it take longer before we can replant.
                            Condition = new ConditionFunction()
                            {
                                Left = new CustomCondition()
                                {
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.DynamicTarget
                                    },
                                    PropertyCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "cropGrowthElapsedTime",
                                        NumberMinimumInclusive = new ValueNode()
                                        {
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.DynamicTarget
                                            },
                                            PropertyKey = "cropGrowthPeriod"                                                        
                                        }
                                    }
                                }, 
                                Operator = OperatorType.And,
                                Right = new CustomCondition()
                                {
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.DynamicTarget
                                    },
                                    PropertyCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "cropGrowthProgress",
                                        NumberNotEqual = new ValueNode()
                                        {
                                            Decimal = 0f                                                                                          
                                        }
                                    }
                                }                                
                            },
                            /*  Condition = new ConditionFunction()
                            {
                                #region cropGrowthElapsedTime > cropGrowthPeriod
                                Left = new CustomCondition()
                                {
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.DynamicTarget
                                    },
                                    PropertyCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "cropGrowthElapsedTime",
                                        NumberMinimumInclusive = new ValueNode()
                                        {
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.DynamicTarget
                                            },
                                            PropertyKey = "cropGrowthPeriod"                                                        
                                        }
                                    }
                                   },
                                #endregion
                             Operator = OperatorType.And,
                                #region cropsAreGrowing == true
                                Right = new CustomCondition()
                                {
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.DynamicTarget
                                    },
                                    PropertyCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "cropsAreGrowing",
                                        BoolValue = true
                                    }   
                                }
                                #endregion*/
                                
                           // },
                            #endregion                            
                            Actions = new EventActionType[]
                            {                               
                                #region start harvest job
                                new CreateJobAction("92517e64-5775-4e5f-b428-5a819aaa6ad3")
                                {                                   
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.DynamicTarget
                                    },
                                    ProcessTypeKey = new ValueNode()
                                    { 
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.DynamicTarget },
                                        PropertyKey = "harvestJobProcessKey" 
                                    }                                    
                                },                               
                                #endregion
                                #region remove weeding job                               
                                new CancelJobAction("580d7c0f-1267-4b58-9783-aa0856c84470")
                                {                                    
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.DynamicTarget
                                    },
                                    ProcessTypeKey = new ValueNode()
                                    { 
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.DynamicTarget},
                                        PropertyKey = "weedJobProcessKey"
                                    }                                    
                                }                               
                                #endregion
                            }
                        }                       
                    }
                }
            });

            #endregion

            //  PropertyKey = "plantJobProcessKey", // read this property if crops are not growing. then start a planting job // "fertilizeJob",
            #region PlantingLoop
            //Just a copy of weedplot job, make planting job
            list.Add(new PolledEventType()
            {
                KeyName = "plantingJobLoop",
                #region PollInterval = FarmingUpdateInterval
                PollInterval = new ValueNode()
                {
                    TargetObject = new TargetObject()
                    {
                        TargetObjectType = TargetObjectType.Root
                    },
                    PropertyKey = "FarmingUpdateInterval"
                },
                #endregion
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.AllValid,
                    ActionTargets = new TargetObject() // the actions will be invoked on all these matching items
                    {
                        TargetObjectType = TargetObjectType.PolledEventSource,
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "OwnedEntities",
                            FilterCondition = new PropertyCondition() { PropertyKey = "isFarmPlot", BoolValue = true },
                        }
                    },
                    SetsOfActions = new[]
                    {
                        new ActionSetType("createPlantingJob") //"createFertilizeJob")
                        {   
                             #region run if: plantJobProcessKey != null && cropsAreGrowing == false && cropGrowthProgress = 0                             
                            Condition = new ConditionFunction()
                                {
                                    #region plantJobProcessKey != null
                                    Left = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.DynamicTarget },
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "plantJobProcessKey",
                                            IsNull = false                                           
                                        }                                        
                                    },
                                    #endregion
                                    Operator = OperatorType.And,
                                    #region cropsAreGrowing == false
                                    Right = new ConditionFunction()
                                    {
                                        Left = new CustomCondition()
                                        {
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.DynamicTarget
                                            },
                                            PropertyCondition = new PropertyCondition()
                                            {
                                                PropertyKey = "cropsAreGrowing",
                                                BoolValue = false
                                            }
                                        }, 
                                        Operator = OperatorType.And,
                                        Right = new CustomCondition()
                                        {
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.DynamicTarget
                                            },
                                            PropertyCondition = new PropertyCondition()
                                            {
                                                PropertyKey = "cropGrowthProgress",
                                                NumberEqual = new ValueNode() { Decimal = 0f }
                                            }
                                        }
                                    }
                                    #endregion                                
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {                               
                                #region start planting job
                                new CreateJobAction("e787ae79-d4aa-40b7-8edc-915374bd310d")
                                {                                   
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.DynamicTarget
                                    },
                                    ProcessTypeKey = new ValueNode()
                                    { 
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.DynamicTarget },
                                        PropertyKey = "plantJobProcessKey"
                                    }                                    
                                },                               
                                #endregion
                            }
                        }                       
                    }
                }
            });
            #endregion            



            /* farmPlotLoop - proper entity sim stuff only! Remove job code!
             * fires periodically on each plot
             * spawns resources,
             * grows crops & weeds
             * no weeding = ~17% full weeding ~90%
             */
            #region farmPlotLoop - is attached to farm plot structures
            list.Add(new PolledEventType()
            {
                KeyName = "farmPlotLoop",
                #region PollInterval = FarmingUpdateInterval
                PollInterval = new ValueNode()
                {
                    TargetObject = new TargetObject()
                    {
                        TargetObjectType = TargetObjectType.Root
                    },
                    PropertyKey = "FarmingUpdateInterval"
                },
                #endregion    
               
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.AllValid,
                    SetsOfActions = new []
                    {                        
                        // Delta cropgrowth and delta weed growth
                        // seperated into own ActionSet for a cleaner look
                        new ActionSetType("deltaCropCalc")
                        {   
                            #region run if: cropsAreGrowing == true
                            Condition = new CustomCondition()
                            {
                                TargetObject = new TargetObject()
                                {
                                    TargetObjectType = TargetObjectType.PolledEventSource
                                },
                                PropertyCondition = new PropertyCondition()
                                {
                                    PropertyKey = "cropsAreGrowing",
                                    BoolValue = true
                                }                               
                                  
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {
                                //ClampTop(cropGrowthProgress+(cropGrowthSpeed*((1-(weedGrowthProgress-cropGrowthProgress))*(nutrientLevel)),1)-cropGrowthProgress
                                #region deltaCropGrowthProgress
                                new SetPropertyAction("664683a1-16f9-48f7-914e-9afe45tiupp01bc45e")
                                {
                                    DelayInSeconds = 0f,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "deltaCropGrowthProgress",
                                        Value = new FunctionNode()
                                        {
                                            Left = new UnaryFunctionNode() // new FunctionNode()
                                            {
                                                Operand = /*Left =*/ new FunctionNode()
                                                {
                                                    #region cropGrowthProgress - why add this and then subtract right after? To set deltaCropGrowthProgress = 0 if progress = 1
                                                    Left = new ValueNode()
                                                    {
                                                        TargetObject = new TargetObject()
                                                        {
                                                            TargetObjectType = TargetObjectType.PolledEventSource
                                                        },
                                                        PropertyKey = "cropGrowthProgress"
                                                    },
                                                    #endregion
                                                    Operator = ExpressionOperator.Plus,
                                                    Right = new FunctionNode()
                                                    {
                                                        #region cropGrowthSpeed
                                                        Left = new ValueNode()
                                                        {
                                                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.PolledEventSource },
                                                            PropertyKey = "cropGrowthSpeed"
                                                        },
                                                        #endregion
                                                        Operator = ExpressionOperator.Multiply,
                                                        Right = new FunctionNode()
                                                        {
                                                            #region 0.8 - (weedGrowthProgress - cropGrowthProgress) //OLD: 1-(weedGrowthProgress - cropGrowthProgress)
                                                            Left = new FunctionNode()
                                                            {
                                                                Left = new ValueNode()
                                                                {
                                                                    Decimal = 0.8f // 1.0f
                                                                },
                                                                Operator = ExpressionOperator.Minus,
                                                                #region weedGrowthProgress - cropGrowthProgress
                                                                Right = new FunctionNode()
                                                                {
                                                                    Left = new ValueNode()
                                                                    {   
                                                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.PolledEventSource },
                                                                        PropertyKey = "weedGrowthProgress"
                                                                    },
                                                                    Operator = ExpressionOperator.Minus,
                                                                    Right = new ValueNode()
                                                                    {
                                                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.PolledEventSource },
                                                                        PropertyKey = "cropGrowthProgress"
                                                                    }
                                                                }
                                                                #endregion
                                                            },
                                                            #endregion
                                                            Operator = ExpressionOperator.Multiply,
                                                            #region 1-nutrientLevel
                                                            Right = new ValueNode()
                                                                {   
                                                                    TargetObject = new TargetObject()
                                                                    {
                                                                        TargetObjectType = TargetObjectType.PolledEventSource
                                                                    },
                                                                    PropertyKey = "nutrientLevel"
                                                            }
                                                            #endregion
                                                        }
                                                    }
                                        
                                                }, 
                                                Operator = UnaryExpressionOperator.ClampToWithinZeroAndOne // = ExpressionOperator.ClampTop,
                                                /*Right = new ValueNode()
                                                {
                                                    Decimal = 1f
                                                }*/
                                            },
                                            Operator = ExpressionOperator.Minus,
                                            Right = new ValueNode()
                                            {
                                                TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.PolledEventSource },
                                                PropertyKey = "cropGrowthProgress"
                                            }
                                        }
                                    
                                },
                                #endregion
                            }
                        },
                        new ActionSetType("deltaWeedCalc")
                        {   
                            Actions = new EventActionType[]
                            {
                                // deltaWeedGrowthProgress = ClampTop(weedGrowthProgress + (weedGrowthSpeed*speedFactor)*(1-nutrientLevel), 1)-weedGrowthProgress
                                #region deltaWeedGrowthProgress
                                new SetPropertyAction("ddbb3419-17b8-40e6-a356-86ae5ssdfgxffd06ad")
                                {
                                    DelayInSeconds = 0f,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "deltaWeedGrowthProgress",
                                        Value = new FunctionNode()
                                        {
                                            Left = new FunctionNode()
                                            {
                                                Left = new FunctionNode()
                                                {
                                                    Left = new ValueNode()
                                                    {
                                                        TargetObject = new TargetObject()
                                                        {
                                                            TargetObjectType = TargetObjectType.PolledEventSource
                                                        },
                                                        PropertyKey = "weedGrowthProgress"
                                                    },
                                                    Operator = ExpressionOperator.Plus,
                                                    Right = new FunctionNode()
                                                    {
                                                        Left = new FunctionNode()
                                                        {
                                                            Left = new ValueNode()
                                                            {
                                                                TargetObject = new TargetObject(){  TargetObjectType = TargetObjectType.PolledEventSource },
                                                                PropertyKey = "weedSpeedFactor"
                                                            },
                                                            Operator = ExpressionOperator.Multiply,
                                                            Right = new ValueNode()
                                                            {
                                                                TargetObject = new TargetObject()
                                                                {
                                                                    TargetObjectType = TargetObjectType.Root
                                                                },
                                                                PropertyKey = "weedGrowthSpeed"
                                                            }
                                                        },
                                                        Operator = ExpressionOperator.Multiply,
                                                        Right = new ValueNode()
                                                        {
                                                                TargetObject = new TargetObject(){TargetObjectType = TargetObjectType.PolledEventSource},
                                                                PropertyKey = "nutrientLevel"
                                                        }
                                                    }
                                                }, 
                                                Operator = ExpressionOperator.ClampTop,
                                                Right = new ValueNode()
                                                {
                                                        Decimal = 1f
                                                }
                                            },
                                            Operator = ExpressionOperator.Minus,
                                            Right = new ValueNode()
                                            {
                                                TargetObject = new TargetObject(){TargetObjectType = TargetObjectType.PolledEventSource},
                                                PropertyKey = "weedGrowthProgress"
                                            }
                                        }
                                    
                                },
                                #endregion
                            }
                        },
                        new ActionSetType("setOvergrownFlag")
                        {   
                            #region Set Overgrown flag if weedGrowthProgress > weedingJobTriggerLimit
                            Condition = new CustomCondition()
                            {
                                TargetObject = new TargetObject()
                                {
                                    TargetObjectType = TargetObjectType.PolledEventSource
                                },
                                PropertyCondition = new PropertyCondition()
                                {
                                    PropertyKey = "weedGrowthProgress",
                                    NumberMinimumInclusive = new ValueNode()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyKey = "weedingJobTriggerLimit"
                                    }
                                }
                                    
                                   
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {
                                // this updates the asset of the structure
                                #region set the spriteFlag to Overgrown
                                new SetPropertyAction("83723b01-4dd9-4bd2-a816-239ea2443272")
                                {
                                    DelayInSeconds = 0f,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "spriteFlag",
                                        Value = new ValueNode()
                                        {
                                            String = "Overgrown"
                                        }
                                    
                                }
                                #endregion
                            }
                        },
                        
                        new ActionSetType("growCrops")
                        {   
                            Comments = "simulate crop growth & increment cropGrowthElapsedTime by FarmingUpdateInterval, only runs when the cropCycle is active",
                            // only runs when the cropCycle is active
                            #region run if: cropsAreGrowing == true
                            Condition = new CustomCondition()
                            {
                                TargetObject = new TargetObject()
                                {
                                    TargetObjectType = TargetObjectType.PolledEventSource
                                },
                                PropertyCondition = new PropertyCondition()
                                {
                                    PropertyKey = "cropsAreGrowing",
                                    BoolValue = true
                                }                               
                                  
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {   // the more weeds on the plot the less the crops grow                              

                                #region increment cropGrowthProgress
                                 new SetPropertyAction("664683a1-16f9-4afg3tyip007-914e-9d91d01bc45e")
                                    {                                        
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "cropGrowthProgress",
                                        Value = new FunctionNode()
                                        {
                                            Left = new FunctionNode()
                                            {
                                                Left = new ValueNode()
                                                {
                                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.PolledEventSource},
                                                    PropertyKey = "cropGrowthProgress"
                                                },
                                                Operator = ExpressionOperator.Plus,
                                                Right = new ValueNode()
                                                {
                                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.PolledEventSource},
                                                    PropertyKey = "deltaCropGrowthProgress"
                                                }
                                            }, 
                                            Operator = ExpressionOperator.ClampTop,
                                            Right = new ValueNode()
                                            {
                                                Decimal = 1f
                                            }
                                        }                                        
                                    },
                                #endregion

                                #region increment cropGrowthElapsedTime by FarmingUpdateInterval
                                new SetPropertyAction("aca584a3-e3d2-44ee-a49e-fa5949663974")
                                {
                                    DelayInSeconds = 0f,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "cropGrowthElapsedTime",
                                        Value = new FunctionNode()
                                        {
                                            Left = new ValueNode()
                                            {
                                                TargetObject = new TargetObject()
                                                {
                                                    TargetObjectType = TargetObjectType.PolledEventSource
                                                },
                                                PropertyKey = "cropGrowthElapsedTime"
                                            },
                                            Operator = ExpressionOperator.Plus,
                                            Right = new ValueNode()
                                            {
                                                TargetObject = new TargetObject()
                                                {
                                                    TargetObjectType = TargetObjectType.Root
                                                },
                                                PropertyKey = "FarmingUpdateInterval"
                                            }
                                        }
                                    
                                },
                                #endregion

                            }
                        },                      
                        new ActionSetType("3ea6be94-dd99-4425-94f6-5ed3daed0cf8")
                        { 
                            Comments = "set the spriteflag to display the growing crops, run if: cropsAreGrowing == true && cropGrowthProgress > 0.2f",
                            #region run if: cropsAreGrowing == true && cropGrowthProgress > 0.2f
                            Condition = new ConditionFunction()
                                {
                                    Left = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "cropsAreGrowing",
                                            BoolValue = true
                                        }                                       
                                        
                                    },
                                    Operator = OperatorType.And,
                                    Right = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "cropGrowthProgress",
                                            NumberMinimumInclusive = new ValueNode()
                                            {
                                                Decimal = 0.2f // TB: should this be a property too? or does it live the magical life?
                                            }
                                        }  
                                    }
                                
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {   
                                // display the little crops as they grow... so pretty!
                                #region set the spriteFlag to HasCrops
                                new SetPropertyAction("cd821abb-8e6a-4d17-97b1-88481677bc8d")
                                {
                                    DelayInSeconds = 0f,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "spriteFlag",
                                        Value = new ValueNode()
                                        {
                                            String = "HasCrops"
                                        }
                                    
                                },
                                #endregion
                            }
                        },                       
                        new ActionSetType("growWeeds")
                        {    
                            Comments = "update the timeStamp & grow the weeds also: this runs everytime",
                            Actions = new EventActionType[]
                            {                                   
                                // weedGrowthProgress += deltaWeedGrowthProgress
                                #region increment weedGrowthProgress
                                 new SetPropertyAction("66af132exzvr3a1-16f9-48f7-914e-9d91d01bc45e")
                                    {
                                        
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.PolledEventSource
                                            },
                                            PropertyKey = "weedGrowthProgress",
                                            Value = new FunctionNode()
                                            {
                                                Left = new FunctionNode()
                                                {
                                                    Left = new ValueNode()
                                                    {
                                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.PolledEventSource},
                                                        PropertyKey = "weedGrowthProgress"
                                                    },
                                                    Operator = ExpressionOperator.Plus,
                                                    Right = new ValueNode()
                                                    {
                                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.PolledEventSource},
                                                        PropertyKey = "deltaWeedGrowthProgress"
                                                    }
                                                }, 
                                                Operator = ExpressionOperator.ClampTop,
                                                Right = new ValueNode()
                                                {
                                                    Decimal = 1f
                                                }
                                            }
                                        
                                    },
                                #endregion
                            }
                        },

                        new ActionSetType("reduceNutrients")
                        {                             
                            Actions = new EventActionType[]
                            {   
                                // nutrientLevel = ClampBottom(nutrientLevel - (deltaWeedGrowthProgress-deltaCropGrowthProgress)*fertilizeReductionFactor, 0)
                                #region increment weedGrowthProgress by weedGrowthSpeed, clamp top to 1
                                new SetPropertyAction("ddbb3419-17b8-40e6-a356-86ae5ffd06ad")
                                {
                                    DelayInSeconds = 0f,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "nutrientLevel",
                                         Value = new FunctionNode()
                                        {
                                            Left = new FunctionNode()
                                            {
                                                Left = new ValueNode()
                                                {
                                                    TargetObject = new TargetObject()
                                                    {
                                                        TargetObjectType = TargetObjectType.PolledEventSource
                                                    },
                                                    PropertyKey = "nutrientLevel"
                                                },
                                                Operator = ExpressionOperator.Minus,
                                                Right = new FunctionNode()
                                                {
                                                    Left = new FunctionNode()
                                                    {
                                                        Left = new ValueNode()
                                                        {
                                                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.PolledEventSource },
                                                            PropertyKey = "deltaWeedGrowthProgress"
                                                        },
                                                        Operator = ExpressionOperator.Minus,
                                                        Right = new ValueNode()
                                                        {
                                                            TargetObject = new TargetObject(){TargetObjectType = TargetObjectType.PolledEventSource},
                                                            PropertyKey = "deltaCropGrowthProgress"
                                                        }
                                                    },
                                                    Operator = ExpressionOperator.Multiply,
                                                    Right = new ValueNode()
                                                    {
                                                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                                            PropertyKey = "fertilizeReductionFactor"
                                                    }
                                                }
                                            }, 
                                            Operator = ExpressionOperator.ClampBottom,
                                            Right = new ValueNode()
                                            {
                                                    Decimal = 0f
                                            }
                                        }
                                    
                                },
                                #endregion
                            }
                        },                       
                        new ActionSetType("cropsFinishedGrowing")
                        {
                            Comments = "crops finished growing, run if: cropGrowthElapsedTime > cropGrowthPeriod && cropsAreGrowing == true",
                            #region run if: cropGrowthElapsedTime > cropGrowthPeriod && cropsAreGrowing == true
                            Condition = new ConditionFunction()
                            {
                                #region cropGrowthElapsedTime > cropGrowthPeriod
                                Left = new CustomCondition()
                                {
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.PolledEventSource
                                    },
                                    PropertyCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "cropGrowthElapsedTime",
                                        NumberMinimumInclusive = new ValueNode()
                                        {
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.PolledEventSource
                                            },
                                            PropertyKey = "cropGrowthPeriod"                                                        
                                        }
                                    }
                                },
                                #endregion
                                Operator = OperatorType.And,
                                #region cropsAreGrowing == true
                                Right = new CustomCondition()
                                {
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.PolledEventSource
                                    },
                                    PropertyCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "cropsAreGrowing",
                                        BoolValue = true
                                    }   
                                }
                                #endregion
                                
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {
                                // the cycle ends here
                                #region set property cropsAreGrowing to false
                                new SetPropertyAction("e7a023c6-fe66-4733-a0dd-75a0d86d0c1d")
                                {
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "cropsAreGrowing",
                                        Value = new ValueNode()
                                        {
                                            Bool = false
                                        }
                                    
                                },
                                #endregion
                                // to ensure we have no floating jobs
                                // the job stuff should not run in the entity loop!
                              /*  #region cancel the weedPlot job
                                new CancelJobAction("580d7c0f-1267-4b58-9783-aa0856c84470")
                                {                                    
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.PolledEventSource
                                    },
                                    ProcessTypeKey = new ValueNode()
                                    { 
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.PolledEventSource},
                                        PropertyKey = "weedJobProcessKey"
                                    }                                    
                                },
                                #endregion*/
                                // one needs to clear the spriteflag to set a new one
                                #region clear the spriteFlag HasCrops
                                new SetPropertyAction("15cb4a30-96cb-4d51-857c-412aa3bb15a3")
                                {
                                    DelayInSeconds = 0f,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "clearFlag",
                                        Value = new ValueNode()
                                        {
                                            String = "HasCrops"
                                        }
                                    
                                },
                                #endregion
                                // this updates the asset of the structure
                                #region set the spriteFlag to Ripe
                                new SetPropertyAction("8ca79361-8b71-40da-a0c0-8fe3fa3d577f")
                                {
                                    DelayInSeconds = 0f,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "spriteFlag",
                                        Value = new ValueNode()
                                        {
                                            String = "Ripe"
                                        }
                                    
                                },
                                #endregion
                                // this updates the asset of the structure
                                #region set cropsFullyGrownAt property to the currentTime
                                new SetPropertyAction("820f5954-60a4-4c54-a15c-5eaa5b472e8d")
                                {
                                    DelayInSeconds = 0f,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "cropsFullyGrownAt",
                                        Value = new ValueNode()
                                        {
                                            PropertyKey = "getTime"
                                        }
                                    
                                },
                                #endregion
                                #region set the state string to Ready for harvest for side panel feedback
                                new SetPropertyAction("0f2491e0-5d3b-4ff5-b512-58cb9e8ca8d4")
                                {                                   
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "cropState",
                                        Value = new ValueNode()
                                        {
                                            String = "Ready for harvest"
                                        }
                                    
                                },
                                #endregion                                    
                              
                            }
                        },
                        
                        new ActionSetType("removeUnharvestedCrops")
                        {   
                            Comments = "removes the crops if they got too old, their age is measured from the timepoint cropsFullyGrownAt",
                            #region run if: resourceTimeToLive < ( currentTime - cropsFullyGrownAt ) 
                            Condition = new ConditionFunction()
                                {
                                    #region resourceTimeToLive < ( currentTime - cropsFullyGrownAt )
                                    Left = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "resourceTimeToLive",
                                            NumberMaximumNotInclusive = new FunctionNode()
                                            {
                                                Left = new ValueNode()
                                                {
                                                    PropertyKey = "getTime"
                                                },
                                                Operator = ExpressionOperator.Minus,
                                                Right = new ValueNode()
                                                {
                                                    TargetObject = new TargetObject()
                                                    {
                                                        TargetObjectType = TargetObjectType.PolledEventSource
                                                    },
                                                    PropertyKey = "cropsFullyGrownAt"
                                                }
                                            }
                                        }                                           
                                        
                                    },
                                    #endregion
                                    Operator = OperatorType.And,                                  
                                    #region cropGrowthProgress > 0.0f AND cropsAreGrowing = false
                                    Right = new ConditionFunction()
                                         {
                                            Left = new CustomCondition()
                                            {
                                                TargetObject = new TargetObject()
                                                {
                                                    TargetObjectType = TargetObjectType.PolledEventSource
                                                },
                                                PropertyCondition = new PropertyCondition()
                                                {
                                                    PropertyKey = "cropGrowthProgress",
                                                    NumberMinimumNotInclusive = new ValueNode() 
                                                    {
                                                        Decimal = 0.0f
                                                    }
                                                }
                                                    
                                                    
                                              }, 
                                            Operator = OperatorType.And,
                                            Right = new CustomCondition()
                                            {
                                                TargetObject = new TargetObject()
                                                {
                                                    TargetObjectType = TargetObjectType.PolledEventSource
                                                },
                                                PropertyCondition = new PropertyCondition()
                                                {
                                                    PropertyKey = "cropsAreGrowing", 
                                                    BoolValue = false                                       
                                                }
                                                    
                                                    
                                               }
                                    }
                                    #endregion
                                
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {   
                                // this is just here for The visual consistency
                                #region clear the spriteFlag Ripe
                                new SetPropertyAction("e6f1c115-3f28-4700-b9f9-a8c78d7f018f")
                                {                                  
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "clearFlag",
                                        Value = new ValueNode()
                                        {
                                            String = "Ripe"
                                        }
                                    
                                },
                                #endregion                                
                                // this updates the asset of the structure
                                #region set the spriteFlag to Dead
                                new SetPropertyAction("6df49a90-147d-4f0e-9454-6de439c464a2")
                                {                                   
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "spriteFlag",
                                        Value = new ValueNode()
                                        {
                                            String = "Dead"
                                        }
                                    
                                },
                                #endregion             
                                #region set the state string to Withered for side panel feedback
                                new SetPropertyAction("bd147d5c-0d5c-453f-a7d2-052a7373307c")
                                {      
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.PolledEventSource
                                    },
                                    PropertyKey = "cropState",
                                    Value = new ValueNode()
                                    {
                                        String = "Withered"
                                    }                                    
                                },
                                #endregion    
                                #region set the property cropGrowthProgress to 0.0 - planting is now possible again
                                new SetPropertyAction("3e0e1f23-0ee1-43dd-b866-3c57f3947d52")
                                {         
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.PolledEventSource
                                    },
                                    PropertyKey = "cropGrowthProgress",
                                    Value = new ValueNode()
                                    {
                                        Decimal = 0.0f
                                    }                                    
                                },
                                #endregion
                                #region set the client property harvestDate to null. This will suppress it from the display
                                new SetPropertyAction("e5aaf3527-62e3-4d8a-bafy46058-e1cd9ed57d0e")
                                {                              
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource // Lars: was: .TargetEntity ???
                                        },
                                        PropertyKey = "harvestDate", 
                                        SetValueToNull = true
                                    
                                },
                                #endregion
                            
                            }
                        }                       
                    }
                }
            });
            #endregion


            #endregion

            // TB FishTrap 19.03.2015
            #region FishTrap
          
            /*
             * Polled expedition that loops through fishtraps 
             * to check if it's time to start a checkFishTrap job
             */
            #region Check Fishtrap Job
            list.Add(new PolledEventType()
            {
                KeyName = "checkFishTrapJobLoop",
                #region PollInterval = FishTrapCheckingLoopInterval
                PollInterval = new ValueNode()
                {
                    TargetObject = new TargetObject()
                    {
                        TargetObjectType = TargetObjectType.Root
                    },
                    PropertyKey = "FishTrapCheckingLoopInterval"
                },
                #endregion               
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.AllValid,
                    ActionTargets = new TargetObject() // the actions will be invoked on all these matching items
                    {
                        TargetObjectType = TargetObjectType.PolledEventSource,
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "OwnedEntities",
                            FilterCondition = new PropertyCondition() { PropertyKey = "isFishTrap", BoolValue = true },
                        }
                    },
                    SetsOfActions = new []
                    {
                        // createCheckFishTrapJob needs to be created
                        new ActionSetType("createCheckFishTrapJob")
                        {   
                            // we only start check jobs on fishtraps based on time and active trap, since the fish isn't visible
                            #region run if: (maxTimeBetweenFishCheckJobs > (getTime - fishTrapLastTimeChecked)) && maxNoOfFishToSpawnAtATime > 0
                            Condition = new ConditionFunction()
                                {
                                    #region maxTimeBetweenFishCheckJobs > (getTime - fishTrapLastTimeChecked)
                                    Left = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root // use the ActionTargets
                                        },
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "maxTimeBetweenFishCheckJobs",
                                            NumberMaximumNotInclusive = new FunctionNode()
                                            {
                                                Left = new ValueNode()
                                                {
                                                    PropertyKey = "getTime"
                                                },
                                                Operator = ExpressionOperator.Minus,
                                                Right = new ValueNode()
                                                {
                                                    TargetObject = new TargetObject()
                                                    {
                                                        TargetObjectType = TargetObjectType.DynamicTarget
                                                    },
                                                    PropertyKey = "fishTrapLastTimeChecked"
                                                }
                                            }
                                        }
                                         
                                    },
                                    #endregion
                                    Operator = OperatorType.And,
                                    #region maxNoOfFishToSpawnAtATime > 0 // #FISHCHANGE: delete this
                                    Right = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.DynamicTarget
                                        },
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "maxNoOfFishToSpawnAtATime", // if maxNoOfFishToSpawnAtATime <= 0 no fish will be spawned, therefor no need for check job
                                            NumberMinimumInclusive = new ValueNode() // TODO: make player feedback
                                            {
                                                Int = 0
                                            }
                                        }                                      
                                        
                                    }
                                    #endregion
                                
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {
                                #region start check job
                                new CreateJobAction("c10ef45b-a390-487b-97f7-defdsafa3245uyoppfcc0ec")
                                {
                                    DelayInSeconds = 0,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.DynamicTarget
                                        },
                                        ProcessTypeKey = new ValueNode()
                                        { 
                                            String = "checkFishTrap" 
                                        }
                                    
                                },
                                #endregion                                  
                            }
                        }                      
                    }
                }

            });
            #endregion
           
            #region FishTrapSpawningLoop
            list.Add(new PolledEventType()
            {
                Comment = @"Polled entity
                            * Checks if the current trap has room for a new fish,
                            * check if the fish gets spawned, spawns the fish
                            * and updates the timers.",

                KeyName = "fishTrapSpawningLoop",
                #region PollInterval = FishTrapCheckingLoopInterval
                PollInterval = new ValueNode()
                {
                    TargetObject = new TargetObject()
                    {
                        TargetObjectType = TargetObjectType.Root
                    },
                    PropertyKey = "FishTrapSpawningLoopInterval"
                },
                #endregion
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.AllValid,
                    SetsOfActions = new []
                    {
                        #region set initial values
                        new ActionSetType("0cd83c05-8239-4275-9f9f-1c93d7e189b4")
                        {
                            Comments = "calculate the amount of fish to spawn and save it in the amountOfFish property. Will only spawn if the number is > 1",
                            Actions = new EventActionType[]
                            {                              
                                #region calculate the amount of fish to spawn and save it in the amountOfFish property. Will only spawn if the number is > 1
                                new SetPropertyAction("1a91a62c-244a-48c1-baed-a1b25a34cf5b")
                                {
                                    DelayInSeconds = 0f,
                                   
                                    //TargetObject = spawningLoopTargetTrap,
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.PolledEventSource
                                    },
                                    PropertyKey = "amountOfFish",
                                    Value = new FunctionNode()
                                    {
                                        Left = new ValueNode()
                                        {
                                            //TargetObject = spawningLoopTargetTrap,
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.PolledEventSource
                                            },
                                            PropertyKey = "maxNoOfFishToSpawnAtATime" // "estimatedSpawnFrequency"
                                        },
                                        Operator = ExpressionOperator.Multiply,
                                        Right = new ValueNode()
                                        {
                                            PropertyKey = "getRandomSimNumber"
                                        }
                                    }                                    
                                },
                                #endregion
                            }
                        },
                        #endregion
                        new ActionSetType("24ae08c6-3297-4106-9241-16ff5f097a3b")
                        {
                            Comments = "condition run if room for fish & fish spawns",
                            #region condition run if room for fish & fish spawns
                            Condition = new ConditionFunction()
                                {  
                                    #region if enough free space
                                    Left = new CustomCondition()
                                        {
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.PolledEventSource
                                            },
                                            PropertyCondition = new PropertyCondition()
                                            {
                                                PropertyKey = "freeStorage",
                                                NumberMinimumInclusive = new FunctionNode()
                                                {
                                                    Left = new ValueNode()
                                                    {
                                                        TargetObject = new TargetObject()
                                                        {
                                                            TargetObjectType = TargetObjectType.PolledEventSource
                                                        },
                                                        PropertyKey = "fishTypeBulk",
                                                    },
                                                    Operator = ExpressionOperator.Multiply,
                                                    Right = new ValueNode()
                                                    {
                                                        TargetObject = new TargetObject()
                                                        {
                                                            TargetObjectType = TargetObjectType.PolledEventSource
                                                        },
                                                        PropertyKey = "amountOfFish"
                                                    }
                                                }
                                            }
                                    },  
                                    #endregion
                                    Operator = OperatorType.And, 
                                    #region check if: spawnChance > randomSimNumber
                                    Right = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "spawnChance",
                                            NumberMinimumInclusive = new ValueNode()
                                            {
                                                PropertyKey = "getRandomSimNumber"
                                            }
                                        }                                       
                                                                                   
                                    } 
                                    #endregion
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {
                                #region spawn fish
                                new SpawnEntityAction("fdc4358c-3924-4184-bf72-c01917a4a8ef")
                                {   
                                    DelayInSeconds = 0f,
                                    LogAsProduction = true,
                                    AddToContainer = new ContainerLocation()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        }
                                    },
                                    Amount = new ValueNode()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "amountOfFish"
                                    },
                                    // NEW: set ownership at once, not via claim later:
                                    OwnedBy = new AllegianceAndExpedition()
                                    {
                                        DynamicAllegianceKey = new ValueNode()
                                        {
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.PolledEventSource 
                                            },
                                            PropertyKey = "owningAllegiance"
                                        },
                                        DynamicExpeditionKey = new ValueNode()
                                        {
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.PolledEventSource 
                                            }, 
                                            PropertyKey = "owningExpedition"
                                        }
                                    },
                                    EntityData = null,
                                    EntityType = new ValueNode()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "fishType"
                                    }
                                    
                                },
                                #endregion
                                // FIXME would be nice to round the number... :X 
                             /*   #region subtract the estimated spawnFrequency from maxNoOfFishToSpawnAtATime #FISHCHANGE delete this also?
                                new EventActionType("939f9156-7aec-4840-a40a-d165e3c6c3c7")
                                {
                                    DelayInSeconds = 0f,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "maxNoOfFishToSpawnAtATime",
                                        Value = new FunctionNode()
                                        {
                                            Left = new ValueNode()
                                            {
                                                TargetObject = new TargetObject()
                                                {
                                                    TargetObjectType = TargetObjectType.PolledEventSource
                                                },
                                                PropertyKey = "maxNoOfFishToSpawnAtATime"
                                            },
                                            Operator = ExpressionOperator.Minus,
                                            Right = new ValueNode()
                                            {
                                                TargetObject = new TargetObject()
                                                {
                                                    TargetObjectType = TargetObjectType.PolledEventSource
                                                },
                                                PropertyKey = "estimatedSpawnFrequency"
                                            }
                                        }
                                    }
                                },
                                #endregion*/

                                /* // NEW: unlimited spawns.
                                #region count down the fishSpawnInterval property by 1
                                new EventActionType("63440a1b-84e6-47b3-bcc1-234b3737798d")
                                {
                                    DelayInSeconds = 0f,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.PolledEventSource
                                        },
                                        PropertyKey = "fishSpawnInterval",
                                        Value = new FunctionNode()
                                        {
                                            Left = new ValueNode()
                                            {
                                                TargetObject = new TargetObject()
                                                {
                                                    TargetObjectType = TargetObjectType.PolledEventSource
                                                },
                                                PropertyKey = "fishSpawnInterval"
                                            },
                                            Operator = ExpressionOperator.Minus,
                                            Right = new ValueNode()
                                            {
                                                Int = 1
                                            }
                                        }
                                    }
                                },
                                #endregion*/
                            }
                        }                       
                    }
                }
            });
            #endregion
            #endregion

            #region animaltrap
            /*
             * looks through entities with isAnimalTrap == true, 
             * and checks if it's time to create a check job for the expedition
             */
            list.Add(new PolledEventType()
            {
                KeyName = "checkAnimalTrapJobLoop",
                #region PollInterval = 12
                PollInterval = new ValueNode()
                {
                    PropertyKey = "animalTrapPollInterval"
                },
                #endregion
              
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.FirstValid,
                    ActionTargets = new TargetObject() // the actions will be invoked on all these matching items
                    {
                        TargetObjectType = TargetObjectType.PolledEventSource,
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "OwnedEntities",
                            FilterCondition = new PropertyCondition() { PropertyKey = "isAnimalTrap", BoolValue = true },
                        }
                    },
                    SetsOfActions = new []
                    {
                        // createCheckFishTrapJob needs to be created
                        new ActionSetType("animalTrapJob")
                        {   
                            // we only start check jobs if the expedition knows it have been triggered or if maxTime have passed since last check
                            #region run if: (maxTimeBetweenAnimalTrapCheckJobs > (getTime - AnimalTrapLastTimeChecked)) || !active
                            Condition = new ConditionFunction()
                            {
                                Left = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root
                                        },
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "maxTimeBetweenAnimalTrapCheckJobs",
                                            NumberMaximumNotInclusive = new FunctionNode()
                                            {
                                                Left = new ValueNode()
                                                {
                                                    PropertyKey = "getTime"
                                                },
                                                Operator = ExpressionOperator.Minus,
                                                Right = new ValueNode()
                                                {
                                                    TargetObject = new TargetObject()
                                                    {
                                                        TargetObjectType = TargetObjectType.DynamicTarget
                                                    },
                                                    PropertyKey = "animalTrapLastTimeChecked"
                                                }
                                            }
                                        }
                                        
                                },
                                Operator = OperatorType.Or,
                                Right = new CustomCondition()
                                {
                                            
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.DynamicTarget
                                    },
                                    PropertyCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "active",
                                        BoolValue = false
                                    }                                        
                                }
                                
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {
                                #region start check job
                                new CreateJobAction("c10ef45b-a3afs325wt5-fdw4b-97f7-de3282sr324fcc0ec")
                                {
                                    DelayInSeconds = 0,
                                   
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.DynamicTarget
                                        },
                                        ProcessTypeKey = new ValueNode()
                                        { 
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.DynamicTarget
                                            },
                                            PropertyKey = "jobName"
                                        }
                                    
                                },
                                #endregion                                  
                            }
                        },
                        #region talkaction 
                        new ActionSetType("animalTrapJobTalk")
                        {   
                            #region run if: (maxTimeBetweenAnimalTrapCheckJobs > (getTime - AnimalTrapLastTimeChecked)) || !active
                            Condition = new ConditionFunction()
                                {
                                    Left = new CustomCondition()
                                        {
                                            TargetObject = new TargetObject()
                                            {
                                                TargetObjectType = TargetObjectType.Root
                                            },
                                            PropertyCondition = new PropertyCondition()
                                            {
                                                PropertyKey = "maxTimeBetweenAnimalTrapCheckJobs",
                                                NumberMaximumNotInclusive = new FunctionNode()
                                                {
                                                    Left = new ValueNode()
                                                    {
                                                        PropertyKey = "getTime"
                                                    },
                                                    Operator = ExpressionOperator.Minus,
                                                    Right = new ValueNode()
                                                    {
                                                        TargetObject = new TargetObject()
                                                        {
                                                            TargetObjectType = TargetObjectType.DynamicTarget
                                                        },
                                                        PropertyKey = "animalTrapLastTimeChecked"
                                                    }
                                                }
                                            }
                                        
                                    },
                                    Operator = OperatorType.Or,
                                    Right = new ConditionFunction()
                                        {
                                            Left = new CustomCondition()
                                            {
                                            
                                                TargetObject = new TargetObject()
                                                {
                                                    TargetObjectType = TargetObjectType.DynamicTarget
                                                },
                                                PropertyCondition = new PropertyCondition()
                                                {
                                                    PropertyKey = "active",
                                                    BoolValue = false
                                                }
                                                
                                            },
                                            Operator = OperatorType.And,
                                            Right = new CustomCondition()
                                            {
                                                TargetObject = new TargetObject()
                                                {
                                                    TargetObjectType = TargetObjectType.Root
                                                },
                                                PropertyCondition = new PropertyCondition()
                                                {
                                                    PropertyKey = "animalCheckCanTalk",
                                                    BoolValue = true
                                                }
                                            }
                                            
                                        
                                    }
                                
                            },
                            #endregion
                            Actions = new EventActionType[]
                            {
                                #region start talkAction
                                new TalkAction("c8c619b3-6180-4dd4-a0c0-08dddfd7a558") //bso: morten her er talkaction 
                                {
                                    
                                        TalkPriority = TalkAction.TalkActionPriority.Low,
                                        CanTalkWhileFighting = false,
                                        CanTalkWhileSleeping = false,
                                        CanTalkWhileThreatened = false,
                                        TurnTowardsListeners = false,
                                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                                        DefaultText = "Someone should check up on our animal trap."
                                    
                                },
                                new SetPropertyAction("c8c619as-f324u-ipppdfsfdd4-a0c0-08dddfd7456a558")
                                {
                                   
                                        TargetObject = new TargetObject(){TargetObjectType = TargetObjectType.Root},
                                        PropertyKey = "animalCheckCanTalk",
                                        Value = new ValueNode(){ Bool = false }
                                    
                                },
                                #endregion                                  
                            }
                        }
                        #endregion
                    }
                }

            });
            #endregion

            #region breeding
            #region procreation
            /*free agent simulated
             * if 2 or more members of entitype (with opposite sex?)
             * random chance of fertilization - set property on female (pregnant) with timestamp
             */

            /*
             * simulated without agents
             * agents in slots (3-5?) in a building
             * check agents in slots - sex/age etc. 
             */

            /*
             * for all pregnant members where elapsed time > gestation period,
             * show event dialog
             * spawn entities in special building
             */
            #endregion
            #region raising livestock

            /*
             * agents are in enclosure. inside enclosure is a container that they can transact with for food.
             * building that they may or may not enter?
             * container building for people to enter, after which food is placed in food area
             * 
             * feeding: process type that places output in a tool? 
             * 1. place food items on ground in enclosure
             * 2. place food items in trough in enclosure
             * 3. place food items in building that the animals can transact with
             * 
             * feeding process: output ends inside tool
             * or, process has no output but food gets spawned
             * 
             * feeding job gets created when no more food. 
             * either check container for food items, or look in radius...
             * 
             * test simulation of age groups - child to adult
             * 
             * polling for adults in buildingb (check slots)
             * 
             * if adult, either create butcher job or make it a special action?
             * action to kill animal and place its carcass inside the building
             * does the animal have to be moved inside the building first? 
             * 
             * or create job? for releasing the animal
             * 
             * 
             */

            #endregion
            #endregion


           
            return list;
        }
    }
}
