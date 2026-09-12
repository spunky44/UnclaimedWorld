using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame 
{    
    /// <summary>
    /// Tuples cannot be assigned to, use this class for pairs that will be assigned to.
    /// </summary>
    public class Pair<T, U> //: ISnapshot
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }



        public override bool Equals(object o)
        {
            Pair<T, U> obj = o as Pair<T, U>;
            return First.Equals(obj.First) && Second.Equals(obj.Second);

        }
    }
}
