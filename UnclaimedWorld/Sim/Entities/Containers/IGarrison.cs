using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Containers
{
    /// <summary>
    /// any container that agents can enter must implement this!
    /// </summary>
    interface IGarrison
    {
        int GetNoOfAgentsInside();

    }
}
