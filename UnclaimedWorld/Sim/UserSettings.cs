using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using UWGame.ClientSide.Interface;
namespace UWGame.SimSide
{
    /// <summary>
    /// these settings are only used by the client...
    /// </summary>
  /*  public class UserSettings
    {
       
        public string CultureString = System.Globalization.CultureInfo.CurrentCulture.ToString();

        [XmlIgnore]
        public CultureInfo Culture;

        public Color ProductionStatusNoToolsAndNoInputsColor = Common.ColorFromHex("#e9a042"); // Color.DarkRed;
        public Color ProductionStatusNoInputsOrNoToolsColor = Common.ColorFromHex("#ffcd76"); // Color.Red;

        public Color BuildingAvailabilityBuildableColour = Common.ColorFromHex("#ffffff"); 
        public Color BuildingAvailabilityNearlyBuildableColour = Common.ColorFromHex("#ffcd76"); 

        public InventoryPanel.ProductionMode ProductionMode = InventoryPanel.ProductionMode.Basic;
        public bool ShowAllTools = false;

        public int LogMessagesPerPage = 20; // 50

        public int TalkLogMessagesToShow = 60;

        public int MaxAlerts = 5;
        public float AlertLifetime = 5f;
    }*/
}
