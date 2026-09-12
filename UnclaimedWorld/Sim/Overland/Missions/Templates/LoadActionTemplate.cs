using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Overland.Missions.Templates
{
    public class LoadActionTemplate : MissionActionTemplate
    {
        /// <summary>
        /// EntityType / amount
        /// </summary>
        public SerializableDictionary<string, List<long>> Orders;

        public override string Name
        {
            get { return TemplateName; }
        }

        public static string TemplateName
        {
            get { return "Load"; }
        }

        public override ActionTypes ActionType
        {
            get { return ActionTypes.Load; }
        }


        public LoadActionTemplate(MissionStopTemplate missionStopTemplate, Dictionary<EntityType, List<EntityID>> orders, bool allowDeleting)
            : base(missionStopTemplate, allowDeleting)
        {
            Orders = new SerializableDictionary<string, List<long>>(orders.ToDictionary(k => k.Key.KeyName, k => k.Value.Select(e => (long)e).ToList())); // cannot cast generic types

        }

        public LoadActionTemplate()
        {

        }

        public override float ComputeTotalCargoBulk()
        {
            return 0;
        }

        public override decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost)
        {
            boughtItemsCost = 0;
            soldItemsCost = 0;

            return 0;
        }

        public override bool Validate(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors)
        {            

            return true;
        }

        public override MissionAction CreateMissionAction(Mission mission)
        {
            return new LoadAction(mission, this);
        }
    }
}
