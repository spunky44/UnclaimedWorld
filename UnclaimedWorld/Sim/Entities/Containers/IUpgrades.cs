using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Containers
{
    /// <summary>
    /// implemented by homecontainer etc.
    /// used to gain access to the entity that contains the item in question
    /// </summary>
    interface IUpgrades
    {      
        bool IsUpgrade(EntityID entityID);

        Dictionary<UpgradeCategory, EntityID> ContainedUpgrades { get; }
    }
}
