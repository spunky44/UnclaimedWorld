using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
namespace UWGame.SimSide.Items
{
    
    public class CarcassType 
    {
        public string[] EdibleByDesignerTags;

        [XmlIgnore]
        public List<EntityType> EdibleBy;

        public CarcassType()
        {

        }


        public void Initialize()
        {


        }

    }
}
