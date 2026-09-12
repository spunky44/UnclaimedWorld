using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities
{
    public class PersonalityType: IGameData
    {
        public string Comments;

        /// <summary>
        /// 0 - 1
        /// the preference for various life conditions
        /// </summary>
        public SerializableDictionary<RatingTypes, NormalDistribution> Principles;

        /// <summary>
        /// 0 - 1
        /// the preference for trying new things (the inversion of Inertia - the tendency to keep doing the same)
        /// </summary>
        public NormalDistribution Adventurousness;

        public NormalDistribution Adaptability;


        /// <summary>
        /// optional: attraction to allegiances
        /// </summary>
        public SerializableDictionary<string, NormalDistribution> Attraction;

        

        /// <summary>
        /// 0 - 1
        /// Volatile:0 - Stable:1 
        /// a factor that indicates how much a person's opinions change over time, can relate to emotinal stability, life factors etc.        
        /// This becomes the noise frequency of a random number addend
        /// 
        /// </summary>
        public NormalDistribution Stability;


        public string KeyName
        {
            get;
            set;
        }

        public string Name
        {
            get; set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }


        public SerializableDictionary<string, string> SpokenLines;




        public PersonalityType(string key)
        {
            this.KeyName = key;
        }  

        public PersonalityType() { }



        public void FillEntity(Personality personality) //Entity entity)
        {
           /* Person personComponent = entity.PersonEntity;
            Personality personality = personComponent.Personality;

            if (personComponent != null)
            {*/

                personality.Principles = new Dictionary<RatingTypes, float>();

                foreach (var item in Principles)
                {
                    personality.Principles.Add(item.Key, (float)item.Value.GetRandomValue(The.Sim.GameplayRandomGenerator, true)); //(float)NormalDistribution.GetRandomValue(The.Sim.GameplayRandomGenerator,
                    //item.Value.Mean, item.Value.StandardDeviation));
                }

                if (Attraction != null)
                {
                    personality.Attraction = new Dictionary<AllegianceID, float>();
                    foreach (var item in Attraction)
                    {
                        Allegiance allegiance = The.Sim.World.GetAllegianceFromKey(item.Key);
                        if (allegiance != null)
                        {
                            personality.Attraction.Add(allegiance.ID, (float)item.Value.GetRandomValue(The.Sim.GameplayRandomGenerator, true)); //(float)NormalDistribution.GetRandomValue(The.Sim.GameplayRandomGenerator,
                        }
                    }
                }

                personality.Adaptability = (float)this.Adaptability.GetRandomValue(The.Sim.GameplayRandomGenerator, true);
            /*
                personality.Adventurousness = (float)Adventurousness.GetRandomValue(The.Sim.GameplayRandomGenerator, true);
                personality.Adventurousness = Common.ShiftValue(personality.Adventurousness); // shift to -1 - 1
            */
                personality.Stability = (float)Stability.GetRandomValue(The.Sim.GameplayRandomGenerator, true);


           // }
        }



        public string GetSpokenLine(string key, string defaultLine)
        {
            string matchingLine = null;
            if (SpokenLines.TryGetValue(key, out matchingLine))
            {
                return matchingLine;
            }
            else return defaultLine;
        }


        public void PreInitValidate(ref List<string> errors)
        {
           
        }

        public void Initialize()
        {
            
        }

        public void PostInitValidate(ref List<string> errors)
        {
           
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteInitialize()
        {
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }

    public class RandomParams 
    {
        public float Mean;
        public float StandardDeviation;
    }
}
