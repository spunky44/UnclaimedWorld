using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland.Locations;

namespace UWGame.SimSide.Overland
{
    /// <summary>
    /// defines the state of a site at game start
    /// 
    /// templates are for othersites only?
    /// 
    /// </summary>
    public class SiteData: IGameData
    {
        public string Name { get; set; }
        public string KeyName { get; set; }

        public bool DeleteRecord
        {
            get;
            set;
        }

        public string Description;


        public bool IsPlaySite;

        public GeodeticCoordinate Coords;

        public bool ShowLabel = true;
        public bool ShowTallPin = true;
        public int SiteMarkerOrder = 0;




        /// <summary>
        /// Optional. edges/buckets are allowed.
        ///        
        /// 
        /// Only makes sense for othersites...
        /// 
        /// </summary>
        public StringChance[] SiteTemplates;

        /// <summary>
        /// used for the random allegiance
        /// </summary>
        public string AllegianceKeyName;

        /// <summary>
        /// used for the random expedition
        /// </summary>
        public string ExpeditionKeyName;


        public void PreInitValidate(ref List<string> errors)
        {

        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> errors)
        {

        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {

        }

        public void PostDataCompleteInitialize()
        {

        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {


        }
    }
}
