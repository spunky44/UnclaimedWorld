using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using System.Xml.Serialization;

namespace UWGame.SimSide.Jobs
{
    public class ProductionToolSet
    {
        public bool IsRequired;

        public ProductionToolType[] Tools;


        [XmlIgnore]
        public ItemType BestTool;


        public void Init()
        {


        }


    }
}
