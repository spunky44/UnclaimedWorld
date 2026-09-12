using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Items;

namespace UWGame.SimSide.AI.Needs
{
    /// <summary>
    /// gets fulfilled via GoalEat
    /// </summary>
    public class FoodNeedType
    {
        public string FoodNutrient;

        [XmlIgnore]
        public FoodNutrientType FoodNutrientType;

        /// <summary>
        /// how much nutrient does it take for an entity to get from 0 -> 1 (get from hunger to fully fed) , as expressed in fractions of the entity’s bulk (mass)?  //
        /// </summary>
        public float RequiredNutrientsAsFractionOfEntityBulk;


        public bool IsEssential = true; 


        public void PostDataCompleteInitialize()
        {
            if (FoodNutrient != null)
            {
                FoodNutrientType = GameData.Instance.AllFoodNutrientTypes[FoodNutrient];
            }

        }
    }
}
