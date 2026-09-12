using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Overland.Missions.Templates
{
    public class DisembarkActionTemplate : MissionActionTemplate
    {
        // later on, this class can specify the entity types to unload...

        /// <summary>
        /// 
        /// </summary>
        //public List<EntityType> CargoToUnload;

        public DisembarkActionTemplate()
        {
            // needed for XmlSerializer
        }

        public DisembarkActionTemplate(MissionStopTemplate missionStopTemplate, bool allowDeleting)
            : base(missionStopTemplate, allowDeleting)
        {
           
        }

        public override string Name
        {
            get { return TemplateName; }
        }

        public static string TemplateName
        {
            get { return "Disembark"; }
        }

        public override ActionTypes ActionType
        {
            get { return ActionTypes.Disembark; }
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
            return new DisembarkAction(mission, this);
        }
    }
}
