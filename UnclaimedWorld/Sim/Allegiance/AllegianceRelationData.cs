using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Allegiances
{
    /// <summary>
    /// defines starting relations between 2 allegiances
    /// </summary>
    public class AllegianceRelationData
    {
        public string Allegiance1;
        public string Allegiance2;

        /// <summary>
        /// 0 - 1: 0 war/hate, 1: alliance
        /// </summary>
        public float Relation;

    }
}
