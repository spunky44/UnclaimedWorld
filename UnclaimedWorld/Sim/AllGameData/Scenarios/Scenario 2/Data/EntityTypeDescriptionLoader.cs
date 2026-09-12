using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data
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
                EntityName = "Member of research team",
                SummaryDescription = "Member of the research team studying the muckroot biome",
                Description = ""



            });

            return list;

        }

      


    }
}
