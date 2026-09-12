using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data
{
    public class EntityTypeDescriptionLoader
    {
        public static List<EntityTypeDescription> Init()
        {
            List<EntityTypeDescription> list = new List<EntityTypeDescription>();

            list.Add(new EntityTypeDescription()
            {
                KeyName = "personDescription",
                EntityType = "entity:human",
                EntityName = "Crew member",
                SummaryDescription = "Member of the fishing crew on the sailboat Welcome Winds",
                Description = "\n \n //FISHING TRIP DESCRIPTION//\n \n VESSEL: Welcome Winds (Home port: Noame)\n \n START DATE:\n March 25, 2408\n DESTINATION: Fishing for carbon tail in the Rust Archipelago"
            });

/*NOT USED
   list.Add(new EntityTypeDescription()
            {
                KeyName = "thornsDescription",
                EntityType = "item:thorns",
                EntityName = "Thorns",
                SummaryDescription = "Pointy, sharp thorns",
                Description = "These thorns could be harvested with the right tools"
            });
*/
            return list;

        }

      


    }
}
