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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data
{
    class CreatureLoader
    {

        public static void Init(List<EntityType> listOfEntityTypes)
        {
            #region Testing robots/creatures that are not used in the game

            listOfEntityTypes.Add(new EntityType("entity:weedingRobot")
            {
                DeleteRecord = true
            });
            
            listOfEntityTypes.Add(new EntityType("entity:patrolRobot")
            {
                DeleteRecord = true
            });
            
            listOfEntityTypes.Add(new EntityType("entity:gunDog")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("entity:domesticatedTwinkler")
            {
                DeleteRecord = true
            });
            
            #endregion

            #region robots
            listOfEntityTypes.Add(new EntityType("entity:robotSmall")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("entity:haulingRobot")
            {
                DeleteRecord = true
            });

            listOfEntityTypes.Add(new EntityType("entity:weedingRobot")
            {
                DeleteRecord = true
            });
            #endregion
        }
    }
}
