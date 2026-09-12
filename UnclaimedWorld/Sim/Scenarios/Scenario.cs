using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents;

namespace UWGame.SimSide.Scenarios
{
    public enum Source { NotSet, RefactoredGames, User }

    public enum MapSize { Small, Medium, Large }

    /// <summary>
    /// header file: scenario.xml
    /// data file: scenarioData.xml
    /// </summary>
    public class Scenario
    {
        #region Header data

      
        /// <summary>
        /// Keyname is not needed since the name will be unique - this is enforced by the file system because each scenario has its own folder
        /// in addition, we use the Source property differentiate vanilla and user scenarios
        /// </summary>
        public string Name;

        public string DisplayName;

        public string SummaryDescription;
        public string Description;

        /// <summary>
        /// Thumbnail image. this image should not be in any spritesheet. 
        /// Later, we want a rendering path for user added art in the project
        /// </summary>
        public string ThumbnailImage;

        public bool Allow32Bit = false;
     
        /// <summary>
        /// bigger image, displayed on the customize screen. MP: needs to be in this folder: Content/GUI/CRT Content.  Rebuild spritesheet GUI_CRT_Sprites.xml
        /// </summary>
        public string Image;
       
        public string MapKey;

        public DateAndTime.TimeDateYear TimeDateYear;

        /// <summary>
        /// setting this flag true will hide the scenario from users running in Release mode
        /// </summary>
        public bool IsInDevelopment = false;

        /// <summary>
        /// we have to set this so we know where to look for the sceanrio data after selection. 
        /// Also, we want to mark our own scenarios so they can be told apart from unofficial ones.
        /// </summary>
        [XmlIgnore]
        public Source Source
        {
            get;
            private set;
        }

        public int SortOrder;

        public MapSize MapSize;

        #endregion



        [XmlIgnore]
        public ScenarioData ScenarioData;


        public static string GetMapSizeAsString(MapSize size)
        {
            switch (size)
            {
                case MapSize.Small:
                    return "Small";

                case MapSize.Medium:
                    return "Medium";

                case MapSize.Large:
                    return "Large";

                default: return "";

            }
        }

        public void SetRGSource()
        {
            this.Source = Source.RefactoredGames;
        }



        public void RegisterEvents()
        {
            if (ScenarioData.ConditionalEvents != null)
            {                
                foreach (var item in ScenarioData.ConditionalEvents)
                {
                    The.Sim.PlaySite.EventManager.AddPolledEvent(item);
                }
            }
        }
    }
}
