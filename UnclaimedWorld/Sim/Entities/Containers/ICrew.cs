using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Entities.Containers
{
    /// <summary>
    /// crew for a vehicle etc.
    /// </summary>
    interface ICrew
    {
        bool IsDriver(Entity entity);

        EntityID? Driver { get; set; }
    }
}
