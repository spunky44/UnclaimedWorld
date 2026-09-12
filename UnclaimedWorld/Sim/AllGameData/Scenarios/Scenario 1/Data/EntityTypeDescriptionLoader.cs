using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data
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
                EntityName = "PRECOL explorer",
                SummaryDescription = "Participant in the PRECOL (Precolony) mission",
                Description = "\n //PRECOL MISSION DESCRIPTION//\n \n STATUS: Ended\n MAKEUP: 60 scientists, technicians, security experts\n OBJECTIVE: Explore the planet ANTHEA, evaluate gathered data and select a site for the first colony (Colony 1)\n --------------------------------\n //ISSUED EQUIPMENT//\n \n PIONEER PLANNING UNIT: Device for scanning environment, gathering and processing data and organizing work\n \n COMMUNICATOR: Short range radio\n \n SURVIVAL SUIT: Protective layers shield the wearer's body from a range of hazards: Microorganisms and parasites. Radiation, heat and cold. Mechanical trauma (piercing and slashing).\n In case of a wound, the suit actively works to reduce blood loss and automatically applies a nanofiber band aid. It detects the type of organisms that enter the wound and responds by releasing anti-infective drugs to the body.\n \n The suit enhances the carrying capacity and endurance of the wearer by a system of tiny motors and sensors.\n \n Collects and rinses water from the wearer and environment, reducing the need to find water sources in the surroundings."



            });

            return list;

        }

      


    }
}
