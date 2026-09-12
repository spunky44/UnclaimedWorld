using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Containers
{
    public interface IHasReplenishItems
    {
        /// <summary>
        /// can be null. 
        /// 
        /// perhaps we are exposing a bit too much, here
        /// </summary>
        ReplenishItems ReplenishItems { get; }
    }
}
