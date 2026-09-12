using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.ClientSide;
using UWGame.ClientSide.Particles;
using UWGame.Client.Particles;
using Xclna.Xna.Animation;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Biological;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data //Twinkler Island
{
    public class ItemsLoader
    {

        public static void Init(List<EntityType> listOfEntityTypes)
        {

            #region delete records:  Skimmer (flying!!). Mule vehicle. Materials not used in-game.

            //flying skimmer:
            #region flying skimmer

            listOfEntityTypes.Add(new EntityType("entity:skimmer")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:skimmerMotor")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:skimmerRotor")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:skimmerSeat")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:skimmerCanopy")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:skimmerHull")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:skimmerWing")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:skimmerLandingGear")
            {
                DeleteRecord = true
            });
            #endregion

            #region  Mule vehicle
            listOfEntityTypes.Add(new EntityType("item:muleVehicleBody")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:wheelMotor")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:suspension")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:vehicleSeat")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:controlPanel")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:wheel")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:tyre")
            {
                DeleteRecord = true
            });
            #endregion

            #region Materials not used in-game:
            listOfEntityTypes.Add(new EntityType("item:metalParts")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:cement")
            {
                DeleteRecord = true
            });
            #endregion

            #endregion
            #region delete records: mesh test
            listOfEntityTypes.Add(new EntityType("item:meshTest")
            {
                DeleteRecord = true
            });
            #endregion



           

/*
            #region delete records: items for Muckroot station NOT necessary because of Production manager functionality. but listed here in case they are needed.

            #region materials
            listOfEntityTypes.Add(new EntityType("item:paint")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("item:acetylene")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:ironCanister")
            {
                DeleteRecord = true

            });
            listOfEntityTypes.Add(new EntityType("item:plastCrete")
            {
                DeleteRecord = true
            });
            //todo antennas molecular ass etc
            #endregion
            #region molecular assembler components
            listOfEntityTypes.Add(new EntityType("item:assemblerPlateA")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("item:masterAssemblerPlateA")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("item:assemblerPlateB")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("item:masterAssemblerPlateB")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("item:vacuumChamber")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("item:assemblerCabinet")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("item:assemblerCooling")
            {
                DeleteRecord = true
            });
            

            #endregion
            #region PRECOL weather station, antenna, field kitchen

            listOfEntityTypes.Add(new EntityType("item:weatherStationMast")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("item:weatherStationSensors")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("item:satelliteGroundStation")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("item:fieldKitchenStove")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("item:fieldKitchenEquipment")
            {
                DeleteRecord = true
            });
            #endregion

            #endregion

            #region delete records: robot
            listOfEntityTypes.Add(new EntityType("item:weedingRobotTool")
            {
                DeleteRecord = true
            });

            #endregion
*/
 

        }
    }
}
