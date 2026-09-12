using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;

namespace UWGame.SimSide.AllGameData
{
    public class SiteDataLoader // only add sites that are reused between scenarios. Otherwise create them in-line
    {
        public static List<SiteData> Init()
        {
            List<SiteData> list = new List<SiteData>();

            #region Scenario 4y
            list.Add(new SiteData()
                    {
                        Name = "Cudgel Hills", 
                        KeyName = "playSite", // change this...
                        Description = "This varied landscape was chosen as a home by us, the founders of Castor's Homestead.", //was: This varied landscape was chosen by the settlers of Castor's Homestead and so it became our home.
                        Coords = new Overland.Locations.GeodeticCoordinate(9.65d, 72.1d), 
                        IsPlaySite = true,
                        ShowLabel = true,
                        ShowTallPin = true,
                        SiteMarkerOrder = 10
                    });

            list.Add(new SiteData()
                  {
                      KeyName = "randomSmallSiteDestinyRiver",
                      SiteTemplates = new[] { new StringChance() { Edge = 0.33f, String = "smallFarmingSite" }, new StringChance() { Edge = 0.66f, String = "smallMiningSite" }, new StringChance() { Edge = 1f, String = "smallFishingSite" } },
                      Coords = new Overland.Locations.GeodeticCoordinate(9.8, 71.7d),
                      IsPlaySite = false,
                      ShowLabel = true,
                      ShowTallPin = true,
                      SiteMarkerOrder = 10
                  });

            list.Add(new SiteData()
                    {
                        Name = "Destiny River Delta", 
                        KeyName = "destinyRiverDeltaDescentEraSite",                                                                                        //text gets cut off here.
                        Description = "The big settlement that we set out from. Originally a research outpost founded by our ancestors, the first pioneers.", //mp scrollbar doesnt work to so text gets cut off..was: The place we set out from. It was originally a research outpost founded by our ancestors which later grew into a major settlement. We can rely on them for trading and hiring.    
                        Coords = new Overland.Locations.GeodeticCoordinate(10.25d, 71.25d),
                        IsPlaySite = false,
                        ShowLabel = true,
                        ShowTallPin = true,
                        SiteMarkerOrder = 10
                    });
            #endregion

           /* 
            #region Scenario 7 
            list.Add(new SiteData()
            {
                KeyName = "randomSmallAdvancedSite",
                SiteTemplates = new[] { new StringChance() { Edge = 0.33f, String = "smallAdvancedFarmingSite" }, new StringChance() { Edge = 0.66f, String = "smallAdvancedMiningSite" }, new StringChance() { Edge = 1f, String = "smallAdvancedFishingSite" } },
                Coords = new Overland.Locations.GeodeticCoordinate(13, 50d),
                IsPlaySite = false,
                ShowLabel = true,
                ShowTallPin = true,
                SiteMarkerOrder = 10
            });
            
            #endregion
            */

            return list;

        }

    }
}
