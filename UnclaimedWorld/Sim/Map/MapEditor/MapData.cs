using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using UWGame.SimSide.Trees;
using UWGame.SimSide.InGameEvents;

namespace UWGame.SimSide.Maps.MapEditor
{
    public class MapData
    {
        [XmlIgnore]
        public string FolderName;

        public string Name;

        public Point Dimensions;

        /// <summary>
        ///  moved to 'Scenario'
        /// </summary>
      //  public DateAndTime.TimeDateYear? StartDate;
               
        /// <summary>
        /// TODO: move to Scenario
        /// </summary>
        public PolledEventType[] Events;

        /// <summary>
        /// events defined in GameData (.xml)
        /// 
        /// move to Scenario
        /// </summary>
        public string[] PredefinedEvents;


       /* public bool ShouldSerializeStartDate()
        {
            return StartDate != null;
        }*/

     //   public float? TimeOfDay;
      //  public int? DayNo;
       // public float? TimeOfYear;
        
                
      //  public MapDataTile[][] TileMap;

        public List<EntityData> SavedMapEntities;
        

        public List<EntityData> Trees;


        public List<Tile> Tiles;

    }
}
