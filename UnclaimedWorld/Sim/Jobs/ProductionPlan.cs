using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Jobs
{
    /// <summary>
    /// store data about a particual production plan here, for the benefit of the user so he can more easily undo its changes.
    /// </summary>
    public class ProductionPlan
    {
        public Dictionary<EntityType, int> ProductionOrderChanges;

    }
}
