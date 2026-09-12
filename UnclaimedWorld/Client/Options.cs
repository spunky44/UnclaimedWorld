using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AllGameData;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using System.Globalization;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.Inventory;
using Microsoft.Xna.Framework.Input;

namespace UWGame.ClientSide
{
   
    public class Options
    {
        public const string FileName = "Options.xml";

        /// <summary>
        /// only controls the ability to set the music volume ingame. if false, the music volume below is turned off
        /// </summary>
        public bool MusicEnabled = true;
        public float MusicVolume = 0.1f;  // Lars: 0.25f        SET MUSIC VOLUME 

        public bool SoundEnabled = true;
        public float SoundFXVolume = 0.45f; // MP: 0.6f                  SET SOUND VOLUME  SOUND FX 

        public bool FullScreen = true; // true;
        public bool Borderless = false;

        public bool HardwareModeSwitch = false; // true;

        public int ResolutionWidth;
        public int ResolutionHeight;

        public bool EnableSteam = true;
               
        public string CultureString = System.Globalization.CultureInfo.CurrentCulture.ToString();

        [XmlIgnore]
        public CultureInfo Culture;

        public Color ProductionStatusNoToolsAndNoInputsColor = Common.ColorFromHex("#e9a042"); // Color.DarkRed;
        public Color ProductionStatusNoInputsOrNoToolsColor = Common.ColorFromHex("#ffcd76"); // Color.Red;

      /*  public Color BuildingAvailabilityBuildableColour = Common.ColorFromHex("#ffffff");
        public Color BuildingAvailabilityNearlyBuildableColour = Common.ColorFromHex("#ffcd76");
        */

    //    public InventoryPanel.ProductionMode ProductionMode = InventoryPanel.ProductionMode.Basic;
        public bool ShowAllTools = false;

        public int MinimumLogMessagesPerPage = 20; 

        public int TalkLogMessagesToShow = 60;

        public int MaxAlerts = 5;
 
        public float AlertLifetime = 5f;

        public bool PlayVideo = true;

        public bool RecordGame = false;

        public int MaxNoOfRecordedGamesToKeep = 20;

        public bool LimitFramerateWhenPaused = true;
        public int TargetFramerateWhenPaused = 25;

        public bool SynchronizeWithVerticalRetrace = true;

        public float KeyScrollSpeedPerSecond = 1200f;

        public float ZoomFactor = 1f; // 2f;

        public Keys KeyScrollLeft = Keys.A; // Keys.Left;
        public Keys KeyScrollUp = Keys.W; //Up;
        public Keys KeyScrollRight = Keys.D; // Right;
        public Keys KeyScrollDown = Keys.S; //Down;

        public Keys KeyPause1 = Keys.Pause;
        public Keys KeyPause2 = Keys.Space;
        public Keys KeyPause3 = Keys.P;

        public Keys KeyGameSpeed1 = Keys.D1;
        public Keys KeyGameSpeed2 = Keys.D2;
        public Keys KeyGameSpeed3 = Keys.D3;

        public Keys ToggleIngameMenu = Keys.M; // Keys.Escape;

        public Keys CloseRosterPanel = Keys.Escape;

        public bool ShowPerformanceWarning = true;

        public void Write()
        {
            BaseDataLoader.SerializeObject(this, "", FileName, Config.DataType.UserSettings);
        }



        public void SetDefaultResolution()
        {
            // set default
            try
            {
                // this can fail! (app not DPI aware?) http://community.monogame.net/t/currentdisplaymode-is-sometimes-null-when-starting/8134/8
                ResolutionWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                ResolutionHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            catch(Exception)
            {
                // set some foolproof settings
                ResolutionWidth = 1024;
                ResolutionHeight = 800;

                FullScreen = false;
            }
        }


        /// <summary>
        /// had to add this because of the serializer error that writes zero values...
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            return MaxAlerts > 0
               // && LogMessagesPerPage > 0
                && TalkLogMessagesToShow > 0;
                /*&& ResolutionWidth > 0
                && ResolutionHeight*/

        }

      /*  public static string GetPathToFile()
        {
            return Config.GetDocumentsFolderPath("", "Options.opt");
                     
        }*/
    }
}
