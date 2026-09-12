using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Tiers;

namespace UWGame.ClientSide.Interface.Inventory
{
    public class TierFilterSettingType: FilterSettingType
    {
        public string Tier;


        [XmlIgnore]
        public TierType TierType;


        public override string GetDefaultDisplayName()
        {
            return "Tier: " + TierType.Name;
        }


        public override HashSet<EntityType> GetData(Predicate<EntityType> filter)
        {
            return GameData.Instance.AllEntityTypes.
                Where(e => e.Value.TierOrAreaType != null
                    && ((e.Value.TierOrAreaType.TierArea != null && e.Value.TierOrAreaType.TierArea.TierType == TierType) || (e.Value.TierOrAreaType.TierType != null && e.Value.TierOrAreaType.TierType == TierType))
                    && (filter == null || filter(e.Value))).
                Select(kvp => kvp.Value).
                ToHashSet(); 
        }

        public override void Initialize()
        {
            base.Initialize();

            TierType = GameData.Instance.AllTierTypes[Tier];
        }

    }
}
