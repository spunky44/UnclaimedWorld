using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface.Inventory
{
    public class StaticFilterSettingType : FilterSettingType
    {
        public StaticFilterSettings StaticFilterSetting;
        
        public override string GetDefaultDisplayName()
        {
            switch (StaticFilterSetting)
            {
                case StaticFilterSettings.Containers: return "Containers ";
                case StaticFilterSettings.Storage: return "Storage ";
                case StaticFilterSettings.Fuel/*Fertilizer*/: return "Fuel";// or fertilizer (!fertilizer)";
                case StaticFilterSettings.BetterTools: return "Better tools";
                case StaticFilterSettings.Structures: return "Structures";
                case StaticFilterSettings.Items: return "Items";
                case StaticFilterSettings.UsableAsWeapon: return "Usable as weapon";
                case StaticFilterSettings.AffectsComfortRating: return "Affects comfort rating";
                case StaticFilterSettings.AffectsSecurityRating: return "Affects security rating";
                case StaticFilterSettings.AffectsFoodRating: return "Affects food rating";

                default: return "No display string for:" + StaticFilterSetting.ToString();
            }
        }

        public override HashSet<EntityType> GetData(Predicate<EntityType> filter)
        {
            switch (StaticFilterSetting)
            {
                case StaticFilterSettings.Structures: return GetEntityTypes(GameData.Instance.AllStructureTypes, filter);
                case StaticFilterSettings.Items: return GetEntityTypes(GameData.Instance.AllItemTypes, filter);
                case StaticFilterSettings.Containers: return GetContainersAndStorages(GameData.Instance.AllItemTypes, filter);  
                case StaticFilterSettings.Storage: return GetContainersAndStorages(GameData.Instance.AllStructureTypes, filter);
                case StaticFilterSettings.Fuel/*Fertilizer*/: return GetFuelsAndFertilizers(filter);
                case StaticFilterSettings.UsableAsWeapon: return GetItemsUseableAsWeapon(filter);
                case StaticFilterSettings.BetterTools: return GetBetterTools(filter);
                case StaticFilterSettings.AffectsFoodRating: return GetFoodRatingTypes(filter);
                case StaticFilterSettings.AffectsComfortRating: return GetComfortRatingTypes(filter);
                case StaticFilterSettings.AffectsSecurityRating: return GetSecurityRatingTypes(filter);  
            }
            return null;
        }


        private HashSet<EntityType> GetEntityTypes(Dictionary<string, EntityType> collection, Predicate<EntityType> filter)
        {
            if (filter == null)
            {
                return collection.Values.ToHashSet();
            }
            else
            {
                HashSet<EntityType> set = new HashSet<EntityType>();

                foreach (var item in collection)
                {
                    if (filter == null || filter(item.Value))
                    {
                        set.Add(item.Value);  
                    }
                 }

                return set;
            }
        }


        private string category = "weapons";
        private HashSet<EntityType> GetItemsUseableAsWeapon(Predicate<EntityType> filter)
        {
            HashSet<EntityType> set = new HashSet<EntityType>();
           
            foreach (var item in GameData.Instance.AllItemTypes)
            {
                if ((filter == null || filter(item.Value))
                    && item.Value.ItemType.WeaponType != null 
                    && item.Value.Category != GameData.Instance.AllEntityCategories[category]
                    && item.Value.ItemType.WeaponType.IsIntrinsic != true)
                {
                    set.Add(item.Value);                   
                }
            }

            return set;
        }

        private HashSet<EntityType> GetContainersAndStorages(Dictionary<string, EntityType> collection, Predicate<EntityType> filter)
        {
            HashSet<EntityType> set = new HashSet<EntityType>();
          
            foreach (var item in collection)
            {
                if ((filter == null || filter(item.Value))
                    && (item.Value.ContainerType != null 
                        && (item.Value.StructureType != null 
                        || (item.Value.ItemType == null || item.Value.ItemType.WeaponType == null))))
                {
                    set.Add(item.Value);                    
                }
            }

            return set;
        }

        //TODO: Fertilizer (we don't have a type for them yet)
        private HashSet<EntityType> GetFuelsAndFertilizers(Predicate<EntityType> filter)
        {
            HashSet<EntityType> set = new HashSet<EntityType>();
           
            foreach (var Item in GameData.Instance.AllItemTypes)
            {
                if ((filter == null || filter(Item.Value))
                    && Item.Value.ItemType.FuelType != null)// || fertilizer)
                {
                    set.Add(Item.Value);
                    
                }
            }

            return set;
        }

        
        private HashSet<EntityType> GetFoodRatingTypes(Predicate<EntityType> filter)
        {
            // get the items that affect the food stockpile rating

            HashSet<EntityType> set = new HashSet<EntityType>();
           
            foreach (var item in GameData.Instance.AllItemTypes)
            {
                if ((filter == null || filter(item.Value)) && FoodStatisticsForAllegiance.AffectsFoodRating(The.InGameUI.UIAllegiance, item.Value))
                {
                    set.Add(item.Value);
                }                
            }

            return set;
        }

        private HashSet<EntityType> GetSecurityRatingTypes(Predicate<EntityType> filter)
        {
            // get the items that affect the security weapon stockpile or defender rating

            HashSet<EntityType> set = new HashSet<EntityType>();

            foreach (var item in GameData.Instance.AllEntityTypes) // agents, structures, weapons...
            {
                if ((filter == null || filter(item.Value))
                    && SecurityStatisticsForAllegiance.AffectsSecurityRating(item.Value))
                {
                    set.Add(item.Value);
                }
            }

            /*
            foreach (var item in GameData.Instance.AllItemTypes)
            {
                if ((filter == null || filter(item.Value))
                    && SecurityStatisticsForAllegiance.AffectsSecurityRating(item.Value))
                {
                    set.Add(item.Value);
                }
            }

            foreach (var item in GameData.Instance.AllStructureTypes)
            {
                if ((filter == null || filter(item.Value))
                    && SecurityStatisticsForAllegiance.AffectsSecurityRating(item.Value))
                {
                    set.Add(item.Value);
                }
            }*/

            return set;
        }

        private HashSet<EntityType> GetComfortRatingTypes(Predicate<EntityType> filter)
        {            
            HashSet<EntityType> set = new HashSet<EntityType>();

            float weight;
            EntityType consumer = The.InGameUI.UIAllegiance.RepresentativeEntityType;
           // NeedType[] needs = consumer.BiologicalType.GetAdultNeedsAndWeight(out weight);

            foreach (var item in GameData.Instance.AllEntityTypes) // agents, structures, weapons...
            {
                if ((filter == null || filter(item.Value))
                    && ComfortStatisticsForAllegiance.AffectsComfortRating(The.InGameUI.UIAllegiance, item.Value)) //, needs))
                {
                    set.Add(item.Value);
                }
            }

            return set;
        }

        private HashSet<EntityType> GetBetterTools(Predicate<EntityType> filter)
        {
            HashSet<EntityType> set = new HashSet<EntityType>();
           
            foreach (var item in GameData.Instance.AllProcessToolSets)
            {
                if (item.Value.Tools.Length > 0)
                {
                    foreach (var toolAlternatives in item.Value.Tools)
                    {
                        if (toolAlternatives.Tools.Length > 0)
                        {
                            foreach (var tools in toolAlternatives.Tools)
                            {
                                if (tools.ProductivityFactor > GameData.Instance.GUIConstants.BetterToolsFilterLimit)
                                {
                                    foreach (var entityType in tools.ToolEntityTypes)
                                    {
                                        if ((filter == null || filter(entityType))
                                            && entityType.StructureType == null)
                                        {
                                            set.Add(entityType);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return set;
        }

    }

    
}
