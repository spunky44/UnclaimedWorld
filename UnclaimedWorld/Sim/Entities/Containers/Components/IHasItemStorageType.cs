using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWGame.SimSide.Entities.Containers.Components
{
    /// <summary>
    /// parallel to IStorage? No, more restricted... not implemented by AgentStorageType...
    /// </summary>
    interface IHasItemStorageType
    {
        ItemStorageType ItemStorageType  { get; }

        bool AllowsStockpiling { get; }
    }
}
