using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.Trade
{
    public class OfferDemandState: IEdge
    {
        public float Edge { get; set; }

        /// <summary>
        /// the daily change while in this state
        /// </summary>
        public NormalDistribution OfferDemandChange;

      
    }

    /*
    public class OfferDemandState //: IEdge
    {
        public string Key;

        /// <summary>
        /// the daily change while in this state
        /// </summary>
        public NormalDistribution OfferDemandChange;

        public StringChance[] Transitions;


    }*/
}
