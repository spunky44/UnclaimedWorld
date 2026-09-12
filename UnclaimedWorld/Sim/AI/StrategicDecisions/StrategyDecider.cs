using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Allegiances;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.AI.StrategicDecisions
{
    /// <summary>
    /// this class should contain strategic decisions that may be taken by both Playsite and Othersite agents.
    /// It does not contain planner (player-replacing) behaviours, only individual, independent decisions and actions
    /// </summary>
    public class StrategyDecider
    {
       // Regulator regulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.1, "StrategyDecider");

        public virtual void Update(GameTime gameTime)
        {
           /* if (regulator.IsReady())
            {
            }*/
        }
    }
}
