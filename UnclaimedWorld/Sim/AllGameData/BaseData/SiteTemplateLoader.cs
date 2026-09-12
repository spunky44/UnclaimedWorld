using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData
{
    public class SiteTemplateLoader
    {
        public static List<SiteTemplate> Init()
        {
            List<SiteTemplate> list = new List<SiteTemplate>();

           
            #region "farming site"
            list.Add(new SiteTemplate()
            {
                KeyName = "smallFarmingSite",
                SizeFactor = 0.8f,
                Names = new[] { "Batten Fields", "Cudale" },
                Description = "A small farming community which exports seasonal crops. Has an occasional need for tools or medical supplies.",
                Allegiances = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "farmingAllegiance" } } // size independent
                    }
                }
            });
            #endregion

            #region "farming site"
            list.Add(new SiteTemplate()
            {
                KeyName = "smallMiningSite",
                SizeFactor = 0.8f,
                Names = new[] { "Sulfur Lake", "Conlans Claim", "Breakneck", "Kooten Pass" },
                Description = "A small community which specializes in mineral extraction and refining. Imports some food.",
                Allegiances = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "miningAllegiance" } } // size independent
                    }
                }
            });
            #endregion

            #region "fishing site"
            list.Add(new SiteTemplate()
            {
                KeyName = "smallFishingSite",
                SizeFactor = 0.8f,
                Names = new[] { "Riverbend", "Yellowwater" },
                Description = "A small fishing community. Imports some tools and spirits.",
                Allegiances = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "fishingAllegiance" } } // size independent
                    }
                }
            });
            #endregion


            #region "Advanced farming site"
            list.Add(new SiteTemplate()
            {
                KeyName = "smallAdvancedFarmingSite",
                SizeFactor = 1f, // 0.8f,
                Names = new[] { "Haven" },
                Description = "A small commune of independent-minded people. The farmers here sell their crops and occasionally buy tools.",              
                Allegiances = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "advancedFarmingAllegiance" } } 
                    }
                }
            });
            #endregion

            #region "Advanced fishing site"
            list.Add(new SiteTemplate()
            {
                KeyName = "smallAdvancedFishingSite",
                SizeFactor = 1f, //0.8f,
                Names = new[] { "Clearbrook" },
                Description = "A small commune of independent-minded people, fishing the nearby waters. Imports tools and supplies.",
                Allegiances = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "advancedFishingAllegiance" } } 
                    }
                }
            });
            #endregion

            #region "Advanced mining site"
            list.Add(new SiteTemplate()
            {
                KeyName = "smallAdvancedMiningSite",
                SizeFactor = 1f, //0.8f,
                Names = new[] { "Rockfall" },
                Description = "A small commune of independent-minded people, producing hand-crafted items. Imports food and supplies.",
                Allegiances = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "advancedMiningAllegiance" } } 
                    }
                }
            });
            #endregion

            return list;

        }

    }
}
