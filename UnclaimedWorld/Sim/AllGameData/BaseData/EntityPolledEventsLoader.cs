using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData
{
   

    public class EntityPolledEventsLoader
    {
        public static List<EntityTypePolledEvent> Init()
        {
            List<EntityTypePolledEvent> list = new List<EntityTypePolledEvent>();

            #region Farm Poll Events
            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "smallPlotSpawningLoop",
                TypeKey = "structure:smallPlot",
                Scope = Entities.Scope.Entity,
                PolledEventKey = "farmPlotLoop"             
                
            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "largePlotSpawningLoop",
                TypeKey = "structure:largePlot",
                Scope = Entities.Scope.Entity,
                PolledEventKey = "farmPlotLoop"

            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "improvisedGreenhouseSpawningLoop",
                TypeKey = "structure:improvisedGreenhouse",
                Scope = Entities.Scope.Entity,
                PolledEventKey = "farmPlotLoop"

            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "greenhouseSpawningLoop",
                TypeKey = "structure:greenhouse",
                Scope = Entities.Scope.Entity,
                PolledEventKey = "farmPlotLoop"

            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "personWeedingJobLoop",
                TypeKey = "entity:human",
                Scope = Entities.Scope.Expedition,
                PolledEventKey = "weedingJobLoop"

            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "personFertilizeJobLoop",
                TypeKey = "entity:human",
                Scope = Entities.Scope.Expedition,
                PolledEventKey = "fertilizeJobLoop"

            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "personPlantingJobLoop",
                TypeKey = "entity:human",
                Scope = Entities.Scope.Expedition,
                PolledEventKey = "plantingJobLoop"

            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "personHarvestJobLoop",
                TypeKey = "entity:human",
                Scope = Entities.Scope.Expedition,
                PolledEventKey = "harvestJobLoop"

            });
            #endregion

            #region Fish trap poll events
            //////////// shore traps
            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "fishTrapShoreBasketSpawningLoop",
                TypeKey = "structure:fishTrapShoreBasket", //carbon tail basket
                Scope = Entities.Scope.Entity,
                PolledEventKey = "fishTrapSpawningLoop"
            });
            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "fishTrapShoreHoopNetSpawningLoop",
                TypeKey = "structure:fishTrapShoreHoopNet", //carbon tail HoopNet
                Scope = Entities.Scope.Entity,
                PolledEventKey = "fishTrapSpawningLoop"
            });
            ////////////

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "fishTrapCoastSpawningLoop",
                TypeKey = "structure:fishTrapCoast", // fyke. streak fin
                Scope = Entities.Scope.Entity,
                PolledEventKey = "fishTrapSpawningLoop"
            });

/////////////Weirs:
            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "fishTrapCreekSticksSpawningLoop",
                TypeKey = "structure:fishTrapCreekSticks", //Sticks weir
                Scope = Entities.Scope.Entity,
                PolledEventKey = "fishTrapSpawningLoop"
            });

            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "fishTrapCreekNetSpawningLoop",
                TypeKey = "structure:fishTrapCreekNet", //Net weir
                Scope = Entities.Scope.Entity,
                PolledEventKey = "fishTrapSpawningLoop"
            });
//////////////////////
            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "checkFishTrapJobLoop",
                TypeKey = "entity:human",
                Scope = Entities.Scope.Expedition,
                PolledEventKey = "checkFishTrapJobLoop"
            });

            #endregion
            #region Animal trap poll events 
            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "checkAnimalTrapJobLoop",
                TypeKey = "entity:human",
                Scope = Entities.Scope.Expedition,
                PolledEventKey = "checkAnimalTrapJobLoop"
            });
            list.Add(new EntityTypePolledEvent()
            {
                KeyName = "constructDeadfallTrap",
                TypeKey = "entity:human",
                Scope = Entities.Scope.Expedition,
                PolledEventKey = "checkAnimalTrapJobLoop"
            });
            #endregion

            return list;

        }
       
    }
}
