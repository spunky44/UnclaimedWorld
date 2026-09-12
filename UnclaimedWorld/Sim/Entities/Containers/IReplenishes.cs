using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Containers
{
    /// <summary>
    /// implemented bu fuelcontainer, magazinecontainer etc.
    /// used to gain access to the entity that contains the item in question
    /// </summary>
    interface IReplenishes
    {
      //  ReplenishItems ReplenishItems { get; }

        bool IsReplenishing(EntityID entityID);
    }
}
