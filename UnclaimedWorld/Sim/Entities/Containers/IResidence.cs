using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Buildings;

namespace UWGame.SimSide.Entities.Containers
{
    /// <summary>
    /// this can be implemented by mobile homes (vehicles) too
    /// </summary>
    interface IResidence
    {
        Residence Residence { get; }

    //    ResidenceType ResidenceType { get; }

     //   int Capacity { get; }
    }
}
