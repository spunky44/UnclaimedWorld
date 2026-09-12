using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Inventory
{
    public class FilterSetting //: ISnapshot
    {

        public FilterSettingType FilterSettingType;


        // cache the results:
        HashSet<EntityType> Results;


        public FilterSetting()
        {
        }

        public FilterSetting(FilterSettingType type)
        {
            this.FilterSettingType = type;
        }

        public string GetDisplayString()
        {
            return FilterSettingType.GetDisplayName();
        }

        public HashSet<EntityType> GetData(Predicate<EntityType> filter)
        {
            if (this.Results == null)
            {
                Results = this.FilterSettingType.GetData(filter);
            }
            return Results;
        }
    }
}
