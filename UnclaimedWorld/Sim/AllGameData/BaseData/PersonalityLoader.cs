using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;

namespace UWGame.SimSide.AllGameData
{
    public class PersonalityLoader
    {
        public static List<PersonalityType> Init()
        {
            List<PersonalityType> list = new List<PersonalityType>();


        /*    #region noComplainerPersonality // replaced with Leader effect
            list.Add(new PersonalityType("noComplainerPersonality") //used for a leader/founder who should never complain or emigrate.   satisfied with survival. they will  be very happy where they are so they need a big personal interest (Attraction) to cancel that out.
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
                        { "playerAllegiance", new NormalDistribution(){ Mean = 0.55f, StandardDeviation = 0.1f  }} //copied from earlyjoiner.. 
                    },
                Principles = new SerializableDictionary<RatingTypes, NormalDistribution>()
                    {
                        { RatingTypes.Food, new NormalDistribution(){ Mean = 0.04f, StandardDeviation = 0.01f } }, //never quit, never complain
                        { RatingTypes.Security, new NormalDistribution(){ Mean = 0.04f, StandardDeviation = 0.01f } },
                        { RatingTypes.Comfort, new NormalDistribution(){ Mean = 0.04f, StandardDeviation = 0.01f } }
                    },
                SpokenLines = new SerializableDictionary<string, string>()
                {
                }
            });
            #endregion*/



                #region earlyJoinerPersonality
                list.Add(new PersonalityType("earlyJoinerPersonality") // have a high attraction to player ensure that they can join player at the earliest stage. satisfied with survival. they will  be very happy where they are so they need a big personal interest (Attraction) to cancel that out.
                {
                    /*     Adventurousness = new NormalDistribution()
                         {
                             Mean = 0.2f,
                             StandardDeviation = 0.07f
                         },*/
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
                        { "playerAllegiance", new NormalDistribution(){ Mean = 0.55f, StandardDeviation = 0.1f  }} //very high 
                    },
                    Principles = new SerializableDictionary<RatingTypes, NormalDistribution>()
                    {
                        { RatingTypes.Food, new NormalDistribution(){ Mean = 0.2f, StandardDeviation = 0.05f } }, //was 0.1f. with heavy starvation (food rating 0%, i would like around half of these to leave and half to die from starvation (takes 2 days))
                        { RatingTypes.Security, new NormalDistribution(){ Mean = 0.15f, StandardDeviation = 0.05f } },
                        { RatingTypes.Comfort, new NormalDistribution(){ Mean = 0.1f, StandardDeviation = 0.08f } }
                    },
                    SpokenLines = new SerializableDictionary<string, string>()
                    {
                    }
                });
                #endregion

                #region survivalTierPersonality
                list.Add(new PersonalityType("survivalTierPersonality") // satisfied with survival. they will  be very happy where they are, so they need a big personal interest (Attraction) to cancel that out. SEE ALSO: earlyJoiner
                {
                    /*     Adventurousness = new NormalDistribution()
                         {
                             Mean = 0.2f,
                             StandardDeviation = 0.07f
                         },*/
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
                        { "playerAllegiance", new NormalDistribution(){ Mean = 0.3f, StandardDeviation = 0.2f  }} //this will decide whether they want to join a primitive colony as immigrant. needs to be high.
                    },
                    Principles = new SerializableDictionary<RatingTypes, NormalDistribution>()
                    {
                        { RatingTypes.Food, new NormalDistribution(){ Mean = 0.2f, StandardDeviation = 0.05f } }, //was 0.1f. with heavy starvation (food rating 0%, i would like around half of these to leave and half to die from starvation (takes 2 days))
                        { RatingTypes.Security, new NormalDistribution(){ Mean = 0.15f, StandardDeviation = 0.05f } },
                        { RatingTypes.Comfort, new NormalDistribution(){ Mean = 0.1f, StandardDeviation = 0.08f } }
                    },
                    SpokenLines = new SerializableDictionary<string, string>()
                    {
                    }
                });
                #endregion

                #region basicTierPersonality
                list.Add(new PersonalityType("basicTierPersonality") //  satisfied with basic tier. has a chance of being unhappy at a small neighbor colony which will increase chance of migrating.
                {
                    /*     Adventurousness = new NormalDistribution()
                         {
                             Mean = 0.2f,
                             StandardDeviation = 0.07f
                         },*/
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
                        { "playerAllegiance", new NormalDistribution(){ Mean = 0.2f, StandardDeviation = 0.08f  }} //this will decide whether they want to join a primitive colony as immigrant
                    },
                    Principles = new SerializableDictionary<RatingTypes, NormalDistribution>()
                    {
                        { RatingTypes.Food, new NormalDistribution(){ Mean = 0.35f, StandardDeviation = 0.15f } },
                        { RatingTypes.Security, new NormalDistribution(){ Mean = 0.30f, StandardDeviation = 0.1f } },
                        { RatingTypes.Comfort, new NormalDistribution(){ Mean = 0.3f, StandardDeviation = 0.1f } }
                    },
                    SpokenLines = new SerializableDictionary<string, string>()
                    {
                    }
                });
                #endregion

                #region mediumTierPersonality
                list.Add(new PersonalityType("mediumTierPersonality") //  satisfied with medium tier
                {
                    /*     Adventurousness = new NormalDistribution()
                         {
                             Mean = 0.2f,
                             StandardDeviation = 0.07f
                         },*/
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
                        { "playerAllegiance", new NormalDistribution(){ Mean = 0.10f, StandardDeviation = 0.04f  }} //this will decide whether they want to join a primitive colony as immigrant
                    },
                    Principles = new SerializableDictionary<RatingTypes, NormalDistribution>()
                    {
                        { RatingTypes.Food, new NormalDistribution(){ Mean = 0.5f, StandardDeviation = 0.15f } },
                        { RatingTypes.Security, new NormalDistribution(){ Mean = 0.55f, StandardDeviation = 0.13f } },
                        { RatingTypes.Comfort, new NormalDistribution(){ Mean = 0.55f, StandardDeviation = 0.13f } }
                    },
                    SpokenLines = new SerializableDictionary<string, string>()
                    {
                    }
                });
                #endregion

 
                #region randomTierPersonality
                list.Add(new PersonalityType("randomTierPersonality") //  completely random
                {
                    /*     Adventurousness = new NormalDistribution()
                         {
                             Mean = 0.2f,
                             StandardDeviation = 0.07f
                         },*/
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
                        { "playerAllegiance", new NormalDistribution(){ Mean = 0.15f, StandardDeviation = 0.18f  }}
                    },
                    Principles = new SerializableDictionary<RatingTypes, NormalDistribution>()
                    {
                        { RatingTypes.Food, new NormalDistribution(){ Mean = 0.6f, StandardDeviation = 0.4f } }, //MP I want food principles to not go too low, so that people can leave before they starve to death.
                        { RatingTypes.Security, new NormalDistribution(){ Mean = 0.5f, StandardDeviation = 0.5f } },
                        { RatingTypes.Comfort, new NormalDistribution(){ Mean = 0.5f, StandardDeviation = 0.5f } }
                    },
                    SpokenLines = new SerializableDictionary<string, string>()
                    {
                    }
                });
                #endregion

                #region advancedSecurityPersonality //food and comf is basic
                list.Add(new PersonalityType("advancedSecurityPersonality") 
                {
                    /*     Adventurousness = new NormalDistribution()
                         {
                             Mean = 0.2f,
                             StandardDeviation = 0.07f
                         },*/
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
                        { "playerAllegiance", new NormalDistribution(){ Mean = 0.15f, StandardDeviation = 0.08f  }} //this will decide whether they want to join a primitive colony as immigrant
                    },
                    Principles = new SerializableDictionary<RatingTypes, NormalDistribution>()
                    {
                        { RatingTypes.Food, new NormalDistribution(){ Mean = 0.35f, StandardDeviation = 0.15f } }, // was 0.35f
                        { RatingTypes.Security, new NormalDistribution(){ Mean = 0.80f, StandardDeviation = 0.1f } }, 
                        { RatingTypes.Comfort, new NormalDistribution(){ Mean = 0.3f, StandardDeviation = 0.1f } } // was 0.3f for testing purposes this has been changed
                    },
                    SpokenLines = new SerializableDictionary<string, string>()
                    {
                    }
                });
                #endregion

                return list;


        }



    }
}
