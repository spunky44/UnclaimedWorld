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
    /// invoke this command to unpause the Sim.
    /// </summary>
    public class Resume : Control.Commands.Command
    {
       
        public Resume()
        {
        }
       

        public override void Execute(bool giveClientFeedback)
        {
            The.Sim.ResumeGame();

            if (giveClientFeedback)
            {
                The.Client.OnResume();
            }
          
        }
    }
}
