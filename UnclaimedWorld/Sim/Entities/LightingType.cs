using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Buildings;
using SpriteSheetRuntime;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Entities //TODO DECOUPLE move to clientSide
{
    /// <summary>
    /// Use this instead of LightSourceType
    /// read coords and offsets from data. Place in Renderable and use sprite state to set lights on/off
    /// </summary>
    public class LightingType
    {
        // NEW
        public string SpriteName;

       
        public Point Offset;

        /// <summary>
        /// Call GetOffset!
        /// </summary>
        public Point OffsetFlipped;
        /*   {
               set { offsetFlipped = value; }
           }*/


        public Point GetOffset(bool flipHorizontally)
        {
            if (flipHorizontally)
            {
                return OffsetFlipped;
            }
            else return Offset;
        }

        /*
        //OLD
        [XmlIgnore]
        public List<LightSourceOffset> IndoorLightSourceTypes = new List<LightSourceOffset>();

        [XmlIgnore]
        public List<LightSourceOffset> OutdoorLightSourceTypes = new List<LightSourceOffset>();

        */


        /*   public void LoadUtilityMapData(BuildingTypeData data)
           {
               foreach (KeyValuePair<string, Point> lightSourceOffset in data.LightSourceOffsets)
               {
                   // here we get an error if the marker pixel has the wrong color...
                   LightSourceType currentLightSourceType = GameData.Instance.AllLightSourceTypes[lightSourceOffset.Key];

                   // add the offset info:
                   LightSourceOffset offset = new LightSourceOffset();
                   offset.Offset = lightSourceOffset.Value;
                   offset.LightSourceType = currentLightSourceType;

                   if (currentLightSourceType.IsIndoor)
                   {
                       IndoorLightSourceTypes.Add(offset);
                   }
                   else
                   {
                       OutdoorLightSourceTypes.Add(offset);
                   }
               }
           }*/
    }
}
