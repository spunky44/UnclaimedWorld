using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data
{
   

    public class EntityPolledEventsLoader
    {
        public static List<EntityTypePolledEvent> Init()
        {
          List<EntityTypePolledEvent> list = new List<EntityTypePolledEvent>();
            // why was this added..?
 /* 
            #region Farm Poll Events
            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "smallPlotSpawningLoop",
                DeleteRecord = true       
            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "largePlotSpawningLoop",
                DeleteRecord = true   

            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "greenhouseSpawningLoop",
                DeleteRecord = true   

            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "personWeedingJobLoop",
                DeleteRecord = true   

            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "personFertilizeJobLoop",
                DeleteRecord = true   

            });
            #endregion

            #region Fish trap poll events
            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "fishTrapShoreBasketSpawningLoop",
                DeleteRecord = true   
            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "coastFishTrapSpawningLoop",
                DeleteRecord = true   
            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "fishTrapCreekSticksSpawningLoop",
                DeleteRecord = true   
            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "checkFishTrapJobLoop",
                DeleteRecord = true   
            });

            #endregion
            #region Animal trap poll events 
            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "checkAnimalTrapJobLoop",
                DeleteRecord = true   
            });
            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "constructDeadfallTrap",
                DeleteRecord = true   
            });
            #endregion
*/
          return list;

        }
       
    }
}
