using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Tiers;

namespace UWGame.ClientSide.Interface.Inventory
{
    public class PolicyAreaFilterSettingType: FilterSettingType
    {
        public RatingTypes RatingType;




        public override string GetDefaultDisplayName()
        {
            return "Policy area: " + Statistic.RatingsTypeToString(RatingType);
        }


        public override HashSet<EntityType> GetData(Predicate<EntityType> filter)
        {
            return GameData.Instance.AllEntityTypes.
                Where(e => e.Value.TierOrAreaType != null && e.Value.TierOrAreaType.TierArea != null && e.Value.TierOrAreaType.TierArea.Area == RatingType && (filter == null || filter(e.Value))).
                Select(kvp => kvp.Value).
                ToHashSet(); 
        }

        public override void Initialize()
        {
            base.Initialize();
        }

    }
}
