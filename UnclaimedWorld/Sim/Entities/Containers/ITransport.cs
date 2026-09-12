using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace GameEngine.Sim.Sim.Entities.Container
{
    /// <summary>
    /// transport of passengers
    /// </summary>
    interface ITransport
    {
        bool IsPassenger(Entity entity);

    }
}
