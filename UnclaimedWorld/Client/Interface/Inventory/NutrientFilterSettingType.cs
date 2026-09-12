using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
namespace UWGame.ClientSide.Interface.Inventory
{
    public class NutrientFilterSettingType: FilterSettingType
    {
        public string Nutrient;

        [XmlIgnore]
        public FoodNutrientType NutrientType;

        public float Limit;

        public string ConsumableByEntity;

        [XmlIgnore]
        public EntityType ConsumableByEntityType;


        public override HashSet<EntityType> GetData(Predicate<EntityType> filter)
        {
            // returns food item types that can be consumed by the ui allegiance and which are over the limit in the specified nutrient

            HashSet<EntityType> listOfEntities = new HashSet<EntityType>();
           
            foreach (var kvp in GameData.Instance.AllItemTypes)
            {
                if ((filter == null || filter(kvp.Value))
                    && kvp.Value.ItemType.FoodType != null && kvp.Value.ItemType.FoodType.FoodNutrientProfile != null)
                {
                    foreach (var tag in ConsumableByEntityType.BiologicalType.FoodItemTagsThatCanBeConsumed) // The.InGameUI.UIAllegiance.RepresentativeEntityType.BiologicalType.FoodItemTagsThatCanBeConsumed)
                    {
                        if (kvp.Value.ItemType.FoodType.FoodTags != null && kvp.Value.ItemType.FoodType.FoodTags.Contains(tag))
                        {
                            foreach (var nutrientType in kvp.Value.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes)
                            {
                                if (this.NutrientType == nutrientType.Nutrient && nutrientType.Amount >= Limit)
                                {
                                    if (!listOfEntities.Contains(kvp.Value))
                                    {
                                        listOfEntities.Add(kvp.Value);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return listOfEntities;
        }

     

        public override void Initialize()
        {
            base.Initialize();

            NutrientType = GameData.Instance.AllFoodNutrientTypes[Nutrient];
            ConsumableByEntityType = GameData.Instance.AllEntityTypes[ConsumableByEntity];
        }

        public override string GetDefaultDisplayName()
        {
            return "High in " + NutrientType.Name;
        }

    }
}
