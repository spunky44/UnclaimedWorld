using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Trees
{
    public enum HasCropsID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }


    public interface IHasCrops : ILookUp<IHasCrops, HasCropsID>
    {
        Vector3 Location { get; }
        Vector3 AccessPoint { get; }
        Point MapPosition { get; }

      
    }
}
