using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.XmlCollections;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// this class can serialize the Color type as hex
    /// 
    /// strings are useful for formatting text etc.
    /// </summary>
    public class GUIConstants : IGameDataObject, IXmlSerializable
    {
        /// <summary>
        /// show / hide the top panel on the production panel accordingly
        /// </summary>
        public bool EnableFilters = true;

        /// <summary>
        /// changing this requires the panels to be recreated
        /// </summary>
        public bool EnableStandingOrders = true; // true;

        public Color StandingOrderTint = Common.ColorFromHex("#FFF58E"); // "#FFEE6D"); // "#F3F1DA");

 
        //mp this is colors for Tracking functionality on production manager and tooltip. colors for tracked objects. : 
        public string[] TrackingColorsHex = new[] { 
             //colors that go well together: lush green: #76ca70 -----light blue #50bbff  ------apricot: #ed833a------cyan: #13cead -----warm blue: #7691d5
            "#76ca70", 
            "#50bbff", 
            "#ed833a",
            "#13cead", 
            "#7691d5" };  

       
        [XmlIgnore]
        public List<Color> TrackingColors; 
          
        // orange #ed833a ////// //a bit darker version orange: #e87301
        //blue #5197e4
        // pink #e44f85
        //cool green: #14c786
        //purple #6e6f9c 
        //too close to the other blue hues: blueish turquoise: #3bd1d6
        //  yellow #efd045 (not visible with light text or on light background)  /////good colors but too light when used as text on LCD combo box BG: light grass green #3ae07e   , /////

        public Color AttainableColor = Common.ColorFromHex("#4AA863"); // "#4BAF66");
        public Color UnattainableColor = Common.ColorFromHex("#c6000e");

        const string positiveHex = "#048628"; // green    was too light: "#389953"
        const string negativeHex = "#953540"; // red

        public string PositiveTintHex = positiveHex; 
        public string NegativeTintHex = negativeHex;

        [XmlIgnore]
        public Color NegativeColor = Common.ColorFromHex(negativeHex);

        [XmlIgnore]
        public Color PositiveColor = Common.ColorFromHex(positiveHex); 
       

        public string ValueTintHex = "#0181C6"; // "#0198ef"; //blue              was: orange  was too light: "#F7D31D"

        public string FoodColor = "#59C24E";
        public string SecurityColor = "#559DBA";
        public string ComfortColor = "#C76098";

        public string FoodColorConstant = "#COLORFOOD";
        public string SecurityColorConstant = "#COLORSECURITY";
        public string ComfortColorConstant = "#COLORCOMFORT";

        public Color SellingButtonTint = Common.ColorFromHex("#75D668"); // green #75D668

        /// <summary>
        /// green
        /// </summary> //screen BG tint
        public Color SellingTint = Common.ColorFromHex("#75D668"); // green   #75D668


        /// <summary>
        /// red
        /// </summary>
        public Color BuyingButtonTint = Common.ColorFromHex("#ffba77"); // red #FF7783

        /// <summary>
        /// red
        /// </summary> //screen BG tint 
        public Color BuyingTint = Common.ColorFromHex("#D2984D"); // red #CE5A66 


        /// <summary>
        /// used in events, tutorials and elsewhere
        /// </summary>
        public SerializableDictionary<string, string> CustomColors = new SerializableDictionary<string, string>()
        {
            { "#COLORPOSITIVE", positiveHex },
            { "#COLORNEGATIVE", negativeHex },
            { "#COLORDATE", "#914B6F" },
            { "#COLORMEMBER", "#A08500" },
            { "#COLORHEADER", "#FFA500" },
            { "#COLORHEADERDARK", "#A55D00" }, // orange
            { "#COLORACTION", "#0073A0" }, // blue
            { "#COLORTYPE", "#CCCC00" },
            { "#COLORFOOD", "#59C24E"},
            { "#COLORSECURITY", "#559DBA"},
            { "#COLORCOMFORT", "#C76098"}
        };

        /*
        public const string HeaderColor = "#FF0000";
        public const string TextColor = "#FFFF00";
        public const string HintColor = "#00FFFF";
     */

      

        #region Overlay

        public byte OverlayLowAlpha = 100;
        public byte OverlayHiAlpha = 150;

        /// <summary>
        /// this could have been done by the serializer. oh well
        /// </summary>
        public string PadColorHex = "FFFF00";
        public string BlockedColorHex = "BA0B3C";
        public string StructureBeingPlacedColorHex = "0000FF";
        public string StructurePreventingPlacementColorHex = "BA0B3C";
        public string StructureBeingPlacedOtherPointPreventingPlacementColorHex = "FF6100";
        public string StructureBeingPlacedOtherPointPreventingPlacementPadColorHex = "FFC300";
        public string InPadAndReservedColorHex = "FFFFE0";
      

        [XmlIgnore]
        public Color PadColor;

        [XmlIgnore]
        public Color BlockedColor; // = Common.ColorFromHex("BA0B3C");

        [XmlIgnore]
        public Color StructureBeingPlacedColor; // = Common.ColorFromHex("BA0B3C");

        [XmlIgnore]
        public Color StructureBeingPlacedOtherPointPreventingPlacementColor;

        [XmlIgnore]
        public Color StructureBeingPlacedOtherPointPreventingPlacementPadColor;

        [XmlIgnore]
        public Color StructurePreventingPlacementColor;

        [XmlIgnore]
        public Color InPadAndReservedColor;

       
        #endregion


        [XmlIgnore]
        public Color PositiveTint; // = Common.ColorFromHex(PositiveTintHex); // "#389953"); // green

        [XmlIgnore]
        public Color NegativeTint; // = new Color(0x95, 0x35, 0x40); // red


        public string FirstToolOption = "PRIMARY TOOL OPTIONS:";                //was "TOOL OPTIONS:"           
        public string SecondToolOption = "SECONDARY TOOL OPTIONS:";      //was  "SECONDARY TOOL NEEDED:"  
        public string ThirdToolOption = "THIRD TOOL OPTIONS:";      //was  "ADDITIONAL TOOL NEEDED:" 

        public int NoOfToolsToDisplayWhenCollapsed = 3;
        public int NoOfToolsToDisplayWhenExpanded = 15;

        public Color sidePanelTextColor = Common.ColorFromHex("#3a7177");

        public float BetterToolsFilterLimit = 0.71f;

        /// <summary>
        /// move these constants if the dialogs ever get a Sim effect
        /// </summary>
        public double DefaultTimeInGameSecondsBeforeGroupMeeting = 400;

        public double MinimumTimeInInGameSecondsBetweenGroupMeetings = 3000;


        public float TimeInDaysToKeepStatistics = 100;

        public float SliderButtonDelay = 0.4f;

        public float TimeBetweenSliderButtonIncrements = 0.1f;

        /// <summary>
        /// we need to allow more gather jobs to enable batching by agents
        /// </summary>
       // public int JobsPerWorkerToTriggerWarning = 5; // 3;
        public int OrderedJobsWithSameOutputToTriggerWarning = 5;

        public double TimeBetweenReplenishAlerts = 10;

        public int UnlimitedStockpileValue = 99;

        public int MaxStandingOrder = 99; //50;

        public int UnlimitedStandingOrderValue = 99;

        public int MaxLogEventsToKeep = 200;

        public int MaxToolsToShowInUsedForList = 15;

        public string[] ProductionFilterSettings = new string[]
        {
            
            //StaticFilters
            "containers",
            "storage",
            "fuel",//  TODO: include Fertilizer
            "betterTools",
            "structures",
            "items",
            "usableAsWeapon",
            //Nutrients
            "highProteinForHumans",
            "highCaloriesForHumans",
            "highMicronientsForHumans",
            "highStimulantsForHumans",
             //Categories
             "ammunition",
             "waste",
             "weapons",
             "tools",
             "rawMaterials",
             "ingredients",
             "preparedFood",
             "affectsFoodRating",
             "affectsComfortRating",
             "affectsSecurityRating",
             "survivalTier",
             "basicTier",
             "mediumTier",
             "advancedTier",
             "comfortPolicy",
             "foodPolicy",
             "securityPolicy"
             //"bodies"
        };

        public SerializableDictionary<OverlayTypes, bool> OverlayDefaultTypeDisplaySettings = new SerializableDictionary<OverlayTypes, bool>()
        {
            { OverlayTypes.ColonyMembers, false },
            { OverlayTypes.BuildAreas, false },
            { OverlayTypes.Threats, false }
        };

        public SerializableDictionary<EntityGrouping, bool> OverlayDefaultDisplaySettings = new SerializableDictionary<EntityGrouping, bool>()
        {
            { EntityGrouping.Animals, false },
            { EntityGrouping.ColonyMembers, false },
            { EntityGrouping.Interest, false },
            { EntityGrouping.Structures, false }
        };

        public SerializableDictionary<EditorOverlayTypes, bool> OverlayDefaultTypeEditorDisplaySettings = new SerializableDictionary<EditorOverlayTypes, bool>()
        {
            { EditorOverlayTypes.Coords, false },
            { EditorOverlayTypes.BuildAreas, false },
            { EditorOverlayTypes.EntityIDs, false },
            { EditorOverlayTypes.TerrainDivision, false }
        };

        public SerializableDictionary<string, bool> OverlayDefaultEntityTypeDisplaySettings = new SerializableDictionary<string, bool>();


        public SerializableDictionary<string, bool> OverlayDefaultResourceCategoryDisplaySettings = new SerializableDictionary<string, bool>()
        {
            { "food", true }
        };

        public SerializableDictionary<string, bool> OverlayDefaultResourceTypeDisplaySettings = new SerializableDictionary<string, bool>();

        [XmlIgnore]
        public HashSet<EntityCategory> StockpileExcludesCategoriesFinal;
      

        public string[] StockpileExcludesCategories = new string[]
        {
            "upgrades"
        };

     //   public SerializableDictionary<string, int> StorageConditionsToDisplay;
        public StorageDurationToDisplay[] StorageDurationToDisplay = new StorageDurationToDisplay[]
        {
            new StorageDurationToDisplay()
            {
                  DisplayName = "Outside",
                  StorageCondition = "exposed",
                  IsStorageOfWeatherProofPart = false,
                  DisplayForStructure = true,
                  DisplayAlways = true
                  //Priority = 1,
                 // FoodItems = true
            },
            new StorageDurationToDisplay()
            {
                  DisplayName = "Outside (protected part)",
                  StorageCondition = "exposed",
                  Tooltip = "Used as a part in a structure that provides some protection against the environment",
                  SortAtTop = true,
                  IsStorageOfWeatherProofPart = true, 
                  DisplayForStructure = false,
                 // Priority = 5,
                //  FoodItems = false
            },
            new StorageDurationToDisplay()
            {
                  DisplayName = "Inside",
                  StorageCondition = "isolated",
                  SortAtTop = true,
                  IsStorageOfWeatherProofPart = false,
                  DisplayForStructure = false,
                //  Priority = 10,
                //  FoodItems = false
            },           
            new StorageDurationToDisplay()
            {
                  DisplayName = "Earth cooled",
                  StorageCondition = "earthCooled",
                  IsStorageOfWeatherProofPart = false,
                  DisplayForStructure = false,
                  DisplayForFoodOnly = true,
               //   Priority = 15,
                 // FoodItems = true
            },           
            new StorageDurationToDisplay()
            {
                  DisplayName = "Refrigerated",
                  StorageCondition = "refrigerator",
                  IsStorageOfWeatherProofPart = false,
                  DisplayForStructure = false,
                  DisplayForFoodOnly = true,
               //   Priority = 25,
                 // FoodItems = true
            },
            new StorageDurationToDisplay()
            {
                  DisplayName = "Freezer",
                  StorageCondition = "freezer",
                  IsStorageOfWeatherProofPart = false,
                  DisplayForStructure = false,
                  DisplayForFoodOnly = true,
                //  Priority = 35,
                 // FoodItems = true
            }
        };



        public bool ShowOverlaysOnGameAreaDefault = true;


        public string[] FoodProductionCategories = new string[]
        {
            "ingredients", "preparedFood"
        };

        public string[] ProductionCategories = new string[]
        {
            "rawMaterials", "tools", "weapons", "equipment", "ammunition", "shelter", "production", "defense", "miscellaneous", "ingredients", "preparedFood"  //"upgrades"
        };

        public GUIConstants()
        {
            /* PositiveTint = Common.ColorFromHex(PositiveTintHex); 
             NegativeTint = Common.ColorFromHex(NegativeTintHex); */
        }


        public void Initialize()
        {
            // this could have been done by the serializer. oh well
            PositiveTint = Common.ColorFromHex(PositiveTintHex);
            NegativeTint = Common.ColorFromHex(NegativeTintHex); 

            PadColor = Common.ColorFromHex(PadColorHex);
            BlockedColor = Common.ColorFromHex(BlockedColorHex);
            StructureBeingPlacedColor = Common.ColorFromHex(StructureBeingPlacedColorHex);
            StructurePreventingPlacementColor = Common.ColorFromHex(StructurePreventingPlacementColorHex);
            StructureBeingPlacedOtherPointPreventingPlacementColor = Common.ColorFromHex(StructureBeingPlacedOtherPointPreventingPlacementColorHex);
            StructureBeingPlacedOtherPointPreventingPlacementPadColor = Common.ColorFromHex(StructureBeingPlacedOtherPointPreventingPlacementPadColorHex);
            InPadAndReservedColor = Common.ColorFromHex(InPadAndReservedColorHex);

           
            if (TrackingColorsHex != null)
            {
                TrackingColors = new List<Color>();
                foreach (var item in TrackingColorsHex)
                {
                    TrackingColors.Add(Common.ColorFromHex(item));
                }
            }

           
        }

        public void PostDataCompleteInitialize()
        {
            StockpileExcludesCategoriesFinal = new HashSet<EntityCategory>();

            if (StockpileExcludesCategories != null)
            {
                foreach (var item in StockpileExcludesCategories)
                {
                    StockpileExcludesCategoriesFinal.Add(GameData.Instance.AllEntityCategories[item]);
                }
            }
        }

        // this was easier than having 2 members for all the colors.
        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
        }

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(GUIConstants))
        {
            TypeMappings = //BaseDataLoader.GetListOfTypeMappings(true)
            new List<CustomXmlSerializer.XmlTypeMappingBase>()
            {
               new CustomXmlSerializer.XmlTypeMapping<Color, string>()
               {
                   GetterMethod = t => t.ToHex(true),
                   SetterMethod = s => Common.ColorFromHex(s)
               }
            }
        };
       
        #endregion
    }
}
