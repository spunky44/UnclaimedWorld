using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Snapshots
{
   
    public interface ILookUpCollectible: ISnapshot
    {
        bool SnapshotThis { get; }

        void ClearCollection();

        /// <summary>
        /// some 'main' collections depend on interface type collections.
        /// 
        /// for example, LookUp ResourceContainer
        /// depends on LookUpIHasCrops
        /// </summary>
        int LoadPostProcessOrder { get; }

       
    }

}
