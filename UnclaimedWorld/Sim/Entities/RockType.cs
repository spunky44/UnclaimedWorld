using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities
{
    public class RockType
    {
        public float Bulk;

        [XmlIgnore]
        public bool Animates = false;

    }
}
