using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Allegiances
{
    public enum CanIterateEntitiesID : ulong
    {
        First = 0L,
        Invalid = uint.MaxValue,
        Max = Invalid
    }

    /// <summary>
    /// implemented by groups of 1 or more entities
    /// entity, household, expedition and allegiance implement this
    /// </summary>
    public interface ICanIterateEntities : ILookUp<ICanIterateEntities, CanIterateEntitiesID>
    {
        void IterateMembers(Action<Entity> iterateFunction);

        /// <summary>
        /// the allegiance should iterate all its member expeditions...
        /// </summary>
        /// <param name="iterateFunction"></param>
        void IterateOwnedItems(Action<EntityGroup> iterateFunction);


        Allegiance GetAllegiance { get; }
    }
}
