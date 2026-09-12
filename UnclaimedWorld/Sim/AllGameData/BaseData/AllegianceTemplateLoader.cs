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
    public class AllegianceTemplateLoader
    {
        public static List<AllegianceTemplate> Init()
        {
            List<AllegianceTemplate> list = new List<AllegianceTemplate>();
            
            #region "farming site"
            list.Add(new AllegianceTemplate()
            {
                KeyName = "farmingAllegianceTemplate",
                Expeditions = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "farmingProfileExpedition" } } 
                    }
                }               
            });
            #endregion

            #region "mining site"
            list.Add(new AllegianceTemplate()
            {
                KeyName = "miningAllegianceTemplate",
                Expeditions = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "miningProfileExpedition" } } 
                    }
                }               
            });
            #endregion

            #region "fishing site"
            list.Add(new AllegianceTemplate()
            {
                KeyName = "fishingAllegianceTemplate",
                Expeditions = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "fishingProfileExpedition" } } 
                    }
                }
            });
            #endregion



            #region "advanced farming site"
            list.Add(new AllegianceTemplate()
            {
                KeyName = "advancedFarmingAllegianceTemplate",
                Expeditions = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "advancedFarmingProfileExpedition" } } 
                    }
                }
            });
            #endregion

            #region "advanced mining site"
            list.Add(new AllegianceTemplate()
            {
                KeyName = "advancedMiningAllegianceTemplate",
                Expeditions = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "advancedMiningProfileExpedition" } } 
                    }
                }
            });
            #endregion

            #region "advanced fishing site"
            list.Add(new AllegianceTemplate()
            {
                KeyName = "advancedFishingAllegianceTemplate",
                Expeditions = new StringChanceSet[] 
                { 
                    new StringChanceSet()
                    {
                         Chances = new[]{ new StringChance() { Edge = 1f, String = "advancedFishingProfileExpedition" } } 
                    }
                }
            });
            #endregion


            return list;

        }

    }
}
