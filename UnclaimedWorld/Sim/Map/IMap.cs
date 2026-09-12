using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
    public enum IMapID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }

    public interface IMap: ILookUp<IMap, IMapID>, ISnapshot 
    {
        //byte[][] Map { get; }
        TileLayer Map { get; }

       // bool[][] IsBlocked { get; }

     
        bool IsReady { get; }
       
        bool DoCycle();

        IMap GetCurrent();

      //  bool SectorIsDirty(int x, int y);

   //     Sector[][] Sectors { get; set; }

        List<Dependence> Children { get; set; }

    }
}
