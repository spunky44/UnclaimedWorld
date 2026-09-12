using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface.Inventory
{
    public class CategoryFilterSettingType: FilterSettingType
    {
       

        public string EntityCategory;

        [XmlIgnore]
        public EntityCategory EntityCategoryType;


        public override string GetDefaultDisplayName()
        {
            return EntityCategoryType.Name;
        }


        public override HashSet<EntityType> GetData(Predicate<EntityType> filter)
        {
            return GameData.Instance.AllEntityTypes.
                Where(e => e.Value.Category == EntityCategoryType && (filter == null || filter(e.Value))).
                Select(kvp => kvp.Value).
                ToHashSet(); // .ToDictionary(x => x.Key, x => x.Value);
        }

        public override void Initialize()
        {
            base.Initialize();

            EntityCategoryType = GameData.Instance.AllEntityCategories[EntityCategory];
        }

    }
}
