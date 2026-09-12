using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Items;

namespace UWGame.SimSide.Entities
{
    public enum OwnerID : long
    {
        Invalid = long.MaxValue,
        Max = Invalid,
        First = 1
    }

    public interface IOwner : ILookUp<IOwner, OwnerID>
    {
        /// <summary>    
        /// this contains the actual owned items
        /// </summary>
        EntityGroup OwnedEntities { get; }

        Allegiances.Allegiance Allegiance { get; } // give a reference to Allegiance instead?

        /// TODO: add OwnerID property here..
    }
}
