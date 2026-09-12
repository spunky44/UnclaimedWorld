using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data //Twinkler Island
{
    public class PersonalityLoader
    {
        public static List<PersonalityType> Init()
        {
            List<PersonalityType> list = new List<PersonalityType>();
            
                list.Add(new PersonalityType("Conlan") //  task-oriented
                {              
                     Adaptability = new NormalDistribution()
                     {
                         Mean = 0.5f,
                         StandardDeviation = 0.05f
                     },
                     Stability = new NormalDistribution()
                     {
                         Mean = 0.75f,
                         StandardDeviation = 0.03f
                     },
                     Attraction = new SerializableDictionary<string, NormalDistribution>()
                    {
                        { "playerAllegiance", new NormalDistribution(){ Mean = 0.3f, StandardDeviation = 0.08f }}
                    },
                     Principles = new SerializableDictionary<RatingTypes, NormalDistribution>()
                    {
                        { RatingTypes.Food, new NormalDistribution(){ Mean = 0.25f, StandardDeviation = 0.01f } },
                        { RatingTypes.Security, new NormalDistribution(){ Mean = 0.45f, StandardDeviation = 0.01f } },
                        { RatingTypes.Comfort, new NormalDistribution(){ Mean = 0f, StandardDeviation = 0.01f } }
                    },                   
                  

                    SpokenLines = new SerializableDictionary<string, string>()
                    {
                        {"crashQuestion", "Rough landing. Everyone OK?" },
                        {"crashAnswer", "No injuries it seems!" },
                        {"crashConclusion", "We might just have a chance, then." }, // In that case, we should have a decent chance
                        {"crashSuggestion", "Now, I want to see what we managed to bring." }, //about unloading supplies.   was: We only have one rifle and a few cutting tools. I'd recommend that we make some more weapons, the sooner the better.

                        {"branchesQuestion", "So we got spoak branches. Remind me what they were for?" },
                        {"branchesAnswer", "Fences. Arrange them tightly, and they should keep out any quadites." },
                        {"branchesAnswerComeback", "Alright." },

                        {"onlyThreeMembersLeftComment", "We need to stick together now." },
                        {"onlyThreeMembersLeftAnswer", "Agreed. This must not happen again." },
                        
                    }
                });

                list.Add(new PersonalityType("Khan") // outgoing, group-oriented
                {
                 
                    Adaptability = new NormalDistribution()
                    {
                        Mean = 0.5f,
                        StandardDeviation = 0.05f
                    },
                    Stability = new NormalDistribution()
                    {
                        Mean = 0.5f,
                        StandardDeviation = 0.1f
                    },
                    Attraction = new SerializableDictionary<string, NormalDistribution>()
                    {
                        { "playerAllegiance", new NormalDistribution(){ Mean = 0.3f, StandardDeviation = 0.08f  }}
                    },
                    Principles = new SerializableDictionary<RatingTypes, NormalDistribution>()
                    {
                        { RatingTypes.Food, new NormalDistribution(){ Mean = 0.45f, StandardDeviation = 0.01f } },
                        { RatingTypes.Security, new NormalDistribution(){ Mean = 0.3f, StandardDeviation = 0.01f } },
                        { RatingTypes.Comfort, new NormalDistribution(){ Mean = 0.15f, StandardDeviation = 0.01f } }
                    },
                    SpokenLines = new SerializableDictionary<string, string>()
                    {
                        {"crashQuestion", "Is everyone alright?" },
                        {"crashAnswer", "Looks like we're all in one piece." },
                        {"crashConclusion", "Good. I'm sure we'll pull through." },
                        {"crashSuggestion", "Now, I want to see what we managed to bring." },

                        {"branchesQuestion", "So we got spoak branches. What was it you wanted them for?" },
                        {"branchesAnswer", "Fences. Arrange them tightly, and they should keep out any quadites." },
                        {"branchesAnswerComeback", "OK." },

                        {"onlyThreeMembersLeftComment", "We need to stick together now." },
                        {"onlyThreeMembersLeftAnswer", "Agreed. This must not happen again." },

                    }
                });

                list.Add(new PersonalityType("Yeboah") // independent, flegmatic
                {
            
                    Adaptability = new NormalDistribution()
                    {
                        Mean = 0.5f,
                        StandardDeviation = 0.05f
                    },
                    Stability = new NormalDistribution()
                    {
                        Mean = 0.8f,
                        StandardDeviation = 0.05f
                    },
                    Attraction = new SerializableDictionary<string, NormalDistribution>()
                    {
                        { "playerAllegiance", new NormalDistribution(){ Mean = 0.3f, StandardDeviation = 0.08f  }}
                    },
                    Principles = new SerializableDictionary<RatingTypes, NormalDistribution>()
                    {
                        { RatingTypes.Food, new NormalDistribution(){ Mean = 0.35f, StandardDeviation = 0.03f } },
                        { RatingTypes.Security, new NormalDistribution(){ Mean = 0.25f, StandardDeviation = 0.03f } },
                        { RatingTypes.Comfort, new NormalDistribution(){ Mean = 0.40f, StandardDeviation = 0.04f } }
                    },
                    SpokenLines = new SerializableDictionary<string, string>()
                    {
                        {"crashQuestion", "Anyone got hurt in the crash?" },
                        {"crashAnswer", "Nothing serious here." },
                        {"crashConclusion", "Good. Let's keep it that way." },
                        {"crashSuggestion", "Now, we should look through our supplies, see what things survived the crash." }, //We're going to need more materials. Since the aircraft won't fly again, we might as well start scrapping it.

                        {"branchesQuestion", "We got spoak branches. What was it you wanted them for?" },
                        {"branchesAnswer", "Fences. Arrange them tightly, and they should keep out any quadites." },
                        {"branchesAnswerComeback", "'Should'." },

                        {"onlyThreeMembersLeftComment", "From now on, we have to be extra careful." },
                        {"onlyThreeMembersLeftAnswer", "Yes. We can make it, but only if we don't take unnecessary chances." },
                    }
                });

                list.Add(new PersonalityType("Lehner") // introvert, pessimistic
                {
             
                    Adaptability = new NormalDistribution()
                    {
                        Mean = 0.5f,
                        StandardDeviation = 0.05f
                    },
                    Stability = new NormalDistribution()
                    {
                        Mean = 0.8f,
                        StandardDeviation = 0.05f
                    },
                    Attraction = new SerializableDictionary<string, NormalDistribution>()
                    {
                        { "playerAllegiance", new NormalDistribution(){ Mean = 0.3f, StandardDeviation = 0.08f  }}
                    },
                    Principles = new SerializableDictionary<RatingTypes, NormalDistribution>()
                    {
                        { RatingTypes.Food, new NormalDistribution(){ Mean = 0.25f, StandardDeviation = 0.02f } },
                        { RatingTypes.Security, new NormalDistribution(){ Mean = 0.35f, StandardDeviation = 0.02f } },
                        { RatingTypes.Comfort, new NormalDistribution(){ Mean = 0.3f, StandardDeviation = 0.05f } }
                    },
                    SpokenLines = new SerializableDictionary<string, string>()
                    {
                        {"crashQuestion", "The skimmer took a beating. What about you - are you all ok?" },
                        {"crashAnswer", "Some minor bruising, but I'll be fine." },  //was: A few bruises is all.
                        {"crashConclusion", "Let's hope things stay that way." },
                        {"crashSuggestion", "Now, we should look through our supplies, see what things survived the crash." }, //One thing we don't have is food.

                        {"branchesQuestion", "Here, I have the branches. What was it you wanted them for?" },
                        {"branchesAnswer", "Fences. Arrange them tightly, and they should keep out any quadites." },  //doesn't sound good: Some people believe they should keep out the quadites.
                        {"branchesAnswerComeback", "'Should'." },

                        {"onlyThreeMembersLeftComment", "From now on, we have to be extra careful." },
                        {"onlyThreeMembersLeftAnswer", "Yes. We can make it, but only if we don't take unnecessary chances." },
                    }
                });

                return list;


        }



    }
}
