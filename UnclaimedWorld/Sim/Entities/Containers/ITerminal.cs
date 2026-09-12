using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Containers
{
   
    public interface ITerminal
    {
        void UncontainAllProductionOutput();

        bool HasCapacityForOutput(Entity item);

        float? TotalOutputCapacity { get; }
        float? TotalStoredOutput { get; }


       // bool HasOutputStorage { get; }
    }
}
