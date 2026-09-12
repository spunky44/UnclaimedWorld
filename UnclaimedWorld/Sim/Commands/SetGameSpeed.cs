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
    /// invoke this command set the sim speed
    /// </summary>
    public class SetGameSpeed : Control.Commands.Command
    {
        public Speeds Speed;
       // public float SpeedFactor;

        public SetGameSpeed()
        {

        }

        public SetGameSpeed(Speeds speed)
        {
            this.Speed = speed;
        }
       

        public override void Execute(bool giveClientFeedback)
        {
            The.Sim.SetGameSpeed(Speed);

            if (giveClientFeedback)
            {
                The.Client.OnSetSpeed(Speed); 
            }
            
        }
    }
}
