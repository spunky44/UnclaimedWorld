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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data //Fields of Tau ceti
{
    class ItemsLoader
    {

        public static void Init(List<EntityType> listOfEntityTypes)
        {



            #region  delete records: Mule vehicle
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

            #region delete records: Materials not used in-game:
            listOfEntityTypes.Add(new EntityType("item:metalParts")
            {
                DeleteRecord = true
            });
            listOfEntityTypes.Add(new EntityType("item:cement")
            {
                DeleteRecord = true
            });
            #endregion


            #region delete records: mesh test
            listOfEntityTypes.Add(new EntityType("item:meshTest")
            {
                DeleteRecord = true
            });
            #endregion

            
           
          
        }
    }
}
