using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Commands
{
    /// <summary>
    /// invoke this command to pause the Sim - while paused, it will not advance the simulation.
    /// </summary>
    public class Pause : Control.Commands.Command
    {
       
        public Pause()
        {
        }
       

        public override void Execute(bool giveClientFeedback)
        {
            The.Sim.PauseGame();

            if (giveClientFeedback)
            {
                The.Client.OnPause(); 
            }
            
        }
    }
}
