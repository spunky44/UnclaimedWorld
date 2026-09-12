using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items
{
    public enum CompositeID : ulong
    {
        First = 0L,
        Invalid = uint.MaxValue,
        Max = Invalid
    }

    public interface IComposite : ILookUp<IComposite, CompositeID>
    {
        /// <summary>
        /// don't add or remove from this list, use the methods instead
        /// </summary>
        List<Entity> Parts
        {
            get;
        }
       
        void SetPart(Entity newPart);
        void RemovePart(Entity part, bool setPartOfToNull = true);

        void SetBrokenPart();

        /// <summary>
        /// parts should call this
        /// </summary>
        void SetConditionDirty();

        IComposite GetRoot();

    }
}
