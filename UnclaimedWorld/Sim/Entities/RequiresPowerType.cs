using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities
{
    public class RequiresPowerType
    {
        /// <summary>
        /// the number of power cells that can (must?) be inserted
        /// </summary>
        public int PowerCellCapacity;


        public float EnergyUsePerDay;

        public void PostLoadContentInitialize()
        {


        }
    }
}
