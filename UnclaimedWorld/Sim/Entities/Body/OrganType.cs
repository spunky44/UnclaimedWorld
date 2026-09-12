using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Body
{
    public class OrganType
    {
        public string Name;

        public enum OrganDepth { Internal, External }

        public OrganDepth Depth;

        public enum OrganFunctions { NerveSystem, Digestive, Respiratory, Vision, Hearing, Appearance }

        public bool IsVital;

        public OrganFunctions[] Functions;


        public override string ToString()
        {
            return Name;
        }
    }
}
