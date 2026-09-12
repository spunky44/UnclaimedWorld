using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;
using System.Xml.Serialization;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.Items
{
    /// <summary>
    /// also smoking products?
    /// </summary>
    public class FoodType
    {
       
        public FoodNutrientProfile FoodNutrientProfile;
             

        public bool IsMeal = false;

        public string[] FoodTags;


        /// <summary>
        /// default is false. If set to true, "Eating" will become "Drinking" in the client feedback
        /// </summary>
        public bool IsDrunk;

        /// <summary>
        /// scale with bulk..?
        /// </summary>
        public string[] Effects;

        [XmlIgnore]
        public List<EffectProfileType> EffectTypes;
        

        public FoodType()
        {

        }

        public void Initialize()
        {
            
        }


        public void PostDataCompleteInitialize()
        {

            if (Effects != null)
            {
                EffectTypes = new List<EffectProfileType>();
                foreach (var item in Effects)
                {
                    EffectTypes.Add(GameData.Instance.AllEffectProfileTypes[item]);
                }

                foreach (var item in EffectTypes)
                {
                    if (item.Affects(AffectsNumbers.AgentComfort)) // .SatisfiesComfort())
                    {
                        foreach (var nutrient in FoodNutrientProfile.FoodNutrientTypes)
                        {
                            nutrient.Nutrient.SatisfiesComfort = true;

                        }

                    }
                }
            }
        }


      /*  public void PostDataCompleteInitialize()
        {
            // mark needTypes as comfort-satisfiying
            if (EffectTypes != null)
            {
                foreach (var item in EffectTypes)
                {
                    if (item.Affects(Affects.AgentComfort)) // .SatisfiesComfort())
                    {
                        foreach (var nutrient in FoodNutrientProfile.FoodNutrientTypes)
                        {                           
                            nutrient.Nutrient.SatisfiesComfort = true;

                        }                       

                    }
                }
            }

        }*/


    }



}
