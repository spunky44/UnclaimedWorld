using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Storage;
using System.Xml;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using SpriteSheetRuntime;
using WindowSystem;
using InputEventSystem;
using System.IO;
using System.Windows.Forms;
using UWGame.SimSide.Trees;
using GameStateManagement;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.MapGUI;
using UWGame.SimSide;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI;
using UWGame.Client.Interface;
using UWGame.SimSide.Expeditions;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Interface.Controls;
using UWGame.SimSide.Resources;
using UWGame.Client.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.DateAndWeatherPanel;
using UWGame.SimSide.Snapshots;
using UWGame.ClientSide.Interface.Missions;
using UWGame.Control.Commands;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Scenarios;
using UWGame.ClientSide.Interface.Personnel;
using UWGame.ClientSide.Interface.World_map;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Interface.BuyAndSell;
using UWGame.SimSide.AI.Goals;
using UWGame.ClientSide.Interface.Tasks;
using UWGame.SimSide.XmlCollections;
using UWGame.ClientSide.Interface.Policy;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Policies;
using System.Text;
using System.Threading;
using UWGame.ClientSide.Interface.Ledger;

namespace UWGame.ClientSide.Interface
{
    public class InGameInterface : CommonInterface
    {
        // Regulator displayRegulator = new Regulator(4);

        // public Buildings.Feature SelectedBuilding
        /*   public Entity SelectedBuilding
           {
               get { return selectedBuilding; }
               set 
               { 
                   selectedBuilding = value;
                   if (selectedBuilding != null)
                   {
                       Change(SidePanelBuilding, true);
                       //SidePanelBuilding.Refresh();
                   }
               }
           }*/

        Allegiance uiAllegiance;
        /// <summary>
        /// NEVER poll this from the Sim!!! Remember decoupling!
        /// 
        /// this can be changed to show the world from the viewpoints of other allegiances
        ///       
        /// </summary>
        public Allegiance UIAllegiance
        {
            get 
            {
                if (uiAllegiance == null)
                {
                    uiAllegiance = The.Sim.PlaySite.PlayerAllegiance;
                }

                return uiAllegiance;
            }

            set 
            {
                uiAllegiance = value;            
            }

        }

        /// <summary>
        /// this will update when scrolling to show the closest expedition 
        /// the idea is that the interface data source ahould change as the player pans over the world as in the Anno games
        /// </summary>      
        public ExpeditionID? UIExpedition;
        public EntityGroupID? UIOwner; // the owner object belonging to the expedition

      //  private List<IKnownEntityData> entitiesToShowMarkerWindowsFor = new List<IKnownEntityData>();
        private HashSet<IKnownEntityData> entitiesToShowMarkerWindowsFor = new HashSet<IKnownEntityData>();
        private List<object> markersToRemove = new List<object>();

        /*
        public OverlaySettings OverlaySettings = new OverlaySettings();
        public InventorySettings InventorySettings = new InventorySettings();
        public TrackTarget TrackedEntityType = new TrackTarget();
        public FilterSetting FilterSetting = new FilterSetting();
        */
        public OverlaySettings OverlaySettings;
        public InventorySettings InventorySettings;
        public FoodProductionSettings FoodProductionSettings;
        public ProductionSettings ProductionSettings;
        public KillsSettings KillsSettings;
        public NutrientSheetSettings NutrientSheetSettings;
        public TaskSettings TaskSettings;
       
       // public TrackTarget TrackedEntityType;
       
        public BuySellPanelSettings BuySettings;
        public BuySellPanelSettings SellSettings;
       

        public enum InterfaceState { None, /*TileInfo,*/ Build, Threat, BlockSubtile, UnblockSubtile, Hunt, FindPrey, Salvage, PlaceExpeditionCenter, 
            EditorPlaceEntity, EditorDeleteEntities, EditorClearTile, Launch, EditorTool };

        private InterfaceState interfaceMode = InterfaceState.None;
        public InterfaceState InterfaceMode
        {
            get
            {
                return interfaceMode;
            }
            set
            {
                DestroyEntityBeingPlaced();

                interfaceMode = value;
            }
        }

        //IKnownEntityData hoverEntity;
        public EntityID? HoverEntity
        {
            get;
            private set;
        }



     //   public IKnownEntityData MarkerWindowHoverEntity;


        /// <summary>
        /// not currently used... but perhaps we will later addd commands that require pointing to a tile
        /// </summary>
        public TerrainTile HoverTile;


        /// <summary>
        /// the tile(s) that are selected - this MapArea is never set to null, but its coverage cleared and added to.
        /// 
        /// SelectedTiles.Zone will always be null!
        /// </summary>
        public MapArea SelectedTiles;


        /// <summary>
        /// tiles in the selection preview
        /// </summary>
        public MapArea SelectedTilesPreview;


        private Zone selectedZone;
        public Zone SelectedZone
        {
            get
            {
                return selectedZone;
            }
            set
            {
                if (selectedZone != value)
                {
                    if (selectedZone != null)
                    {   // old zone
                        selectedZone.MapArea.MapAreaRender.IsSelected = false;
                    }

                    selectedZone = value;

                    if (selectedZone != null)
                    {   // new zone
                        selectedZone.MapArea.MapAreaRender.IsSelected = true;

                        RemoveSelectedTiles(); // make sure there is not another selected area of tiles
                    }
                }
            }
        }


        public void SetHoverEntity(EntityID? /* IKnownEntityData*/ value, Color? hoverTintingColor = null)
        {
            if (HoverEntity != value
                    || The.InGameUI.Selection.HoverTintingColor != hoverTintingColor) // also restart hover anim if the hover color has changed.
            {
                HoverEntity = value;

                Selection.StartHoverOverEntity(hoverTintingColor);
            } 

        }
        
        public Expedition GetExpedition()
        {
            if (The.InGameUI.UIExpedition.HasValue)
            {
                Expedition expedition = Expedition.FindByID(The.InGameUI.UIExpedition.Value);

                return expedition;
            }
            else return null;
        }

        public void RemoveSelectedTiles()
        {
            SelectedTiles.Clear();
            if (ContextMenuOpener != null)
            {
                ContextMenuOpener.Hide();
            }
        }

        public delegate void SelectedEntityChanged(EntityID? oldEntity, EntityID? newEntity);
        public event SelectedEntityChanged SelectedEntityChangedEvent;

        public EntityID? SelectedEntity
        {
            get { return selectedEntity; }
            private set
            {
                if (selectedEntity != value)
                {
                    // disabel tracking
                    EnableTracking(false);

                    EntityID? oldEntity = selectedEntity;

                    selectedEntity = value;

                   
                   /* if (SidePanelEntity != null)
                    {                       
                        Entity dataToPush = null; // = value as Entity;
                        ((EntityPanel.EntityPanel)SidePanelEntity.ExpandedPanel).SelectedEntity = dataToPush;
                    }*/

                    if (selectedEntity != null)
                    {
                        ChangeRosterPanel(SidePanelEntity, true);
                    }

                    if (SelectedEntityChangedEvent != null)
                    {
                        SelectedEntityChangedEvent.Invoke(oldEntity, selectedEntity);
                    }
                }
            }
        }


        public bool TrackSelectedEntity = false;

        //private TerrainTile selectedTile;
        //private TerrainTile selectedTile;

      

        private bool showOverlaysAndMarkerWindows = true;

        /// <summary>
        /// is the UI in "select" mode - then show all marker windows.
        /// </summary>
        public bool ShowOverlaysAndMarkerWindows
        {
            get
            {
                return showOverlaysAndMarkerWindows;
            }
            set
            {
                if (showOverlaysAndMarkerWindows != value)
                {
                    showOverlaysAndMarkerWindows = value;

                    if (showOverlaysAndMarkerWindows == false)
                    {
                        foreach (var marker in activeMarkerWindows)
                        {
                            marker.Value.Hide();
                        }
                    }
                    else
                    {
                        foreach (var marker in activeMarkerWindows)
                        {
                            marker.Value.Show();
                        }
                    }
                }
            }
        }

        public void SetExpeditionMarkerPositions()
        {
            if (ShowOverlaysAndMarkerWindows == true)
            {
                foreach (var marker in activeMarkerWindows)
                {
                    if (marker.Value.Expedition != null)
                    {
                        marker.Value.Show();
                    }
                }
            }
        }

        /// <summary>
        /// returns the OwnerID of the UIExpedition
        /// </summary>
        /// <returns></returns>
        public OwnerID? GetUIOwnerID()
        {
            if (UIExpedition != null)
            {
                Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID(UIExpedition);
                if (expedition != null)
                {
                    return ((IOwner)expedition).ID;
                }
            }

            return null;
        }

        //public List<TerrainTile> SelectedTiles = new List<TerrainTile>();
        /*   {
               get { return selectedTile; }
               set
               {
                   if (selectedTile != value)
                   {
                       selectedTile = value;
                       if (selectedTile != null)
                       {
                           Change(SidePanelTile, true);
                       }
                   }
               }
           }*/
        /*  public TerrainTile SelectedTile
          {
              get { return selectedTile; }
              set 
              {
                  if (selectedTile != value)
                  {
                      selectedTile = value;
                      if (selectedTile != null)
                      {
                          Change(SidePanelTile, true);                       
                      }                   
                  }
              }
          }*/

        /// <summary>
        /// the animated selection circle around selected entities
        /// </summary>
        public Selection Selection;

        /// <summary>
        /// shows in build mode
        /// </summary>
        public TerrainBlockingRender TerrainBlockingRender;

        public ThreatRender ThreatRender;

        //private Buildings.Feature selectedBuilding;

        private EntityID? selectedEntity;

        //  public Entity EntityBeingPlaced;

        public List<EntityPosition> EntitiesBeingPlaced = new List<EntityPosition>();



        public int leftMargin = 20;
        public int topMargin = 60;
        private int maxPanelHeight = 800; // 400;
      
        public const int InterfaceWidth = 309;
      
        public const int BottomAreaExcludedFromHUD = 160;

        public int MainLeft;
        public int MainTop;

        public const int SmallPanelTop = 213;

        const int rosterPanelBottomMargin = 100;

        public int expandedInterfaceLeft;
        public const int rosterPanelTop = 18; // 93;
        public int expandedInterfaceWidth = 800;
        public int rosterPanelHeight;

        public int sidePanelHeight;
        public int sidePanelFullHeight;

       
      //  public ResourceCategory ResourceOutlinesToRender = null;

        private RosterPanel displayedRosterPanel;
       // private SidePanel displayedSidePanel;
        
        
      //  private ExpandedPanel displayedExpandedPanel;


      

        //*****
        // only one of these ROSTER panels are displayed at a time:

        public InventoryPanel InventoryPanel;
        public JobsPanel JobsPanel;
        public GraphPanel GraphPanel;
        public LedgerPanel LedgerPanel;
        public DiplomacyPanel DiplomacyPanel;
        public WorldMapRosterPanel WorldMapPanel;
        public MissionsPanel MissionsPanel;
        public EventArchivePanel EventArchivePanel;
        public PersonnelRosterPanel PersonnelRosterPanel;
        public PolicyPanel PolicyPanel;

        // this roster panel currently does not have an access button on the acces bar:
        public CreateMissionPanel CreateMissionPanel;

        //******

        public MessageBox MessageBox;
        public SaveLoadMessageBox SaveLoadMessageBox;

        public InGameMenuDialog MenuDialog;

        public BuySellPanel BuySellDialog;
        public PersonnelDialog PersonnelDialog;
        public WorldMapDialog WorldMapDialog; 

        public FramedCRT framedCRT;

        public SpriteFont InterfaceFont;
    

        public HUDOverlayPanel HUDOverlayPanel;
        public HUDHelpPanel HUDHelpPanel;
    
        public HUDEntityContextMenu HUDActionPanel;

        public SidePanelEmpty SidePanelEmpty;
        public SidePanelEntity SidePanelEntity;
        public SidePanelMapArea SidePanelMapArea;



        public SidePanelEditorEntity SidePanelEditorEntity;
        public SidePanelEditorSoil SidePanelEditorSoil;
        public SidePanelEditorTerrainHeight SidePanelEditorTerrainHeight;


        public LogPanel LogPanel;
              
        
        public RosterAccessPanel RosterAccessPanel;
        public MainPanel MainPanel;

        public Minimap Minimap;

      //  public ActionPanel ActionPanel;
        public OverlayPanel OverlayPanel;

        public FogMap FogMap;

        private List<HUDWindow> hudWindows = new List<HUDWindow>();
        //private Dictionary<Zone, MarkerWindow> zoneMarkers = new Dictionary<Zone, MarkerWindow>();
        //private List<ExpeditionMarker> expeditionMarkers = new List<ExpeditionMarker>();
        //private Dictionary<Expedition, MarkerWindow> expeditionMarkers = new Dictionary<Expedition, MarkerWindow>();
        // private List<SpokenLine> spokenLines = new List<SpokenLine>();
        private Pool<SpokenLine> poolOfSpokenLines;
        private List<SpokenLine> activeSpokenLines = new List<SpokenLine>();

       
        private Pool<MarkerWindow> poolOfMarkerWindows;
       
        /// <summary>
        /// the marker windows currently being shown
        /// </summary>
        private Dictionary<object, MarkerWindow> activeMarkerWindows = new Dictionary<object, MarkerWindow>();

        //private Dictionary<object, MarkerWindow> activeMarkerWindows = new Dictionary<object,MarkerWindow>();
        //private List<MarkerWindow> activeMarkerWin
        private float screenBoundsExtension = 100.0f;

        // HUD windows
        // public UWGame.ClientSide.Interface.HUD_Windows.ContextMenu ContextMenu;
        public UWGame.ClientSide.Interface.HUD_Windows.ContextMenuOpener ContextMenuOpener;

        public TileSelectionContextMenu ContextMenu;



        /// <summary>
        /// the tool tips currently shown on screen - arranged as children and parents - no branching!!
        /// </summary>
        public List<DataSheet> EntityTypeTooltipsStack;

        /// <summary>
        /// need this to check if a tooltip is already displayed when spawning a new one
        /// </summary>
        public HashSet<DataSheet> PinnedDataTypeTooltips;

        /// <summary>
        /// unpinning means not in stack or pinned
        /// </summary>
        public HashSet<DataSheet> UnpinnedDataTypeTooltipsOutsideStack;


        //public List<EntityTypeTooltip> EntityTypeTooltipsStack;
        public Tooltip Tooltip;

        // let's create a pool of available entity type tooltips
        public Pool<EntityDataSheet> poolOfEntityTypeTooltips;
        public Pool<ProcessTypeDataSheet> poolOfProcessTypeTooltips;
       // public Pool<EntityTypeTooltip> poolOfTooltips;
       


        // public Pool<EventDialog> poolOfEventDialogs;
        /// <summary>
        /// can only show one modal dialog at a time:
        /// </summary>
        public EventDialog EventDialog;

       
        //public Pool<HelpTopicDialog> poolOfHelpTopicDialogs;
        // public Pool<TalkDialog> poolOfTalkDialogs;

        public TalkPanel TalkPanel;

        public Dictionary<HelpTopic, HelpTopicDialog> HelpTopicDialogs = new Dictionary<HelpTopic, HelpTopicDialog>();


        public SetTileResourcesWindow SetTileResources;

        public EntityListWindow EntityListWindow;

        public SiteWindow SiteWindow;

     //   public StatsWindow RatingsWindow;
        public RatingsPanel RatingsPanel;

       // public AssetsWindow AssetsWindow;
        public CounterPanel CounterPanel;

        //***
        private Rectangle characterNearbyBounds;

        /// <summary>
        /// the bounding rectangle that the user creates when left-dragging
        /// </summary>
        public SelectRectangle SelectRectangle;

        public StatusScreen StatusScreen;
        public Help Help;
       // public DateAndWeather DateAndWeather;
        private DateAndWeather dateAndWeather;
       
        private Window window;

        /// <summary>
        /// the slow fading cycle associated with selection
        /// </summary>
        /// 
        public Animation2D SelectedCycleAnimationQuick;
        public Animation2DPlayer SelectedCyclePlayerFlashing;
        public Animation2D SelectedCycleAnimation;
        public Animation2DPlayer SelectedCyclePlayer; // all selections will cycle fade in unison

       public const int minimapMinWidth = 200;
       public const int minimapMaxWidth = 280;

        private TextWindow replayTime;

        //int maximumNumberOfStatusIcons = 100;
        public InGameInterface(UnclaimedWorld game, SerializableDictionary<string, string> customColors)
            : base(game)//: base(UWGame.SimSide.Instance.ScreenManager.Game) 
        {

            if (customColors != null)
            {
                foreach (var item in customColors)
                {
                    gui.CustomColors.Add(item.Key, Common.ColorFromHex(item.Value));
                }
            }

            // put stuff in Initialize instead.

        }

        /// <summary>
        /// this can't be used on menu screens etc...
        /// </summary>
        //public static InGameInterface Instance
        //{
        //    get
        //    {
        //        return instance;                
        //    }

        //    set { instance = value; }
        //}

        public struct EntityPosition
        {
            public Vector2 Position;
            public Entity Entity;
        }

        public void AddHudWindow(HUDWindow window)
        {
            hudWindows.Add(window);
        }

        public void RemoveSpokenLine(/*EntityID speaker) */ SpokenLine line)
        {
            //SpokenLine line = spokenLines.Find(m => m. == zone);

            if (line != null)
            {
                // prevents accidentally retiring more than once...
                if (activeSpokenLines.Remove(line))
                {
                    line.Speaker = null;
                    line.Parent.ShowWhileLineIsSpoken = false;
                    line.Parent = null;

                    line.Hide();

                    poolOfSpokenLines.Retire(line);
                }
            }
        }

        public void ShowSpokenLine(Entity speaker, string text, float spokenLineDisplaySecondsLeft) 
        {
           
            // get the currently displayed window for that entity if possible!
            MarkerWindow markerWindow;
            if (!activeMarkerWindows.TryGetValue(speaker.EntityID, out markerWindow))
            {
                TryToAddEntityMarkerWindow(speaker.EntityID, out markerWindow);
            }

            if (markerWindow != null)
            {
        
                SpokenLine line = activeSpokenLines.Find(s => s.Speaker == speaker.EntityID);

                if (line == null)
                {
                    line = poolOfSpokenLines.Get();
                    activeSpokenLines.Add(line);
                }

                line.Init(speaker, text, activeMarkerWindows[speaker.EntityID]);

                line.Show();
           }

        }

        /// <summary>
        /// use this when invoking BuySell from InGame. Use another method when opening from Main Menu/scenario screen
        /// </summary>
        /// <param name="absolutePosition"></param>
        /// <param name="selectedAction"></param>
        /// <param name="allowOrders"></param>
        /// <param name="currentOrders"></param>
        /// <param name="allegiance"></param>
        /// <param name="ownerOfItems"></param>
        /// <param name="getItems"></param>
        public void FillAndShowBuySellDialog(Point absolutePosition, BuySellPanel.BuySellDialogMode mode, // CargoActionTypes selectedAction, bool allowOrders,
            Dictionary<EntityType, List<EntityID>> currentOrders,
            BuySellPanel.CanTradeDelegate canTrade,
           // Allegiance allegiance, 
            EntityGroup ownerOfItems, EntityGroup buyerOfItems,
            BuySellPanel.Func<EntityType, bool, List<EntityID>> getItems)
        {
           
            BuySellPanelSettings settings = null;

            switch(mode)
            {
                case BuySellPanel.BuySellDialogMode.ActionBuyAtNPC:
                case BuySellPanel.BuySellDialogMode.ViewBuyAtNPC:
                    settings = The.InGameUI.BuySettings;
                    break;
                case BuySellPanel.BuySellDialogMode.ActionSellAtPlayer:
                case BuySellPanel.BuySellDialogMode.ViewSellAtNPC:
                    settings = The.InGameUI.SellSettings;
                    break;

            }

            /*
            if (selectedAction == CargoActionTypes.Buy)
            {
                settings = The.InGameUI.BuySettings;
            }
            else
            {
                settings = The.InGameUI.SellSettings;
            }*/


            EntityGroupID? buyerOfItemsID = null;
            if (buyerOfItems != null)
            {
                buyerOfItemsID = buyerOfItems.ID;
            }

            EntityGroupID? ownerOfItemsID = null;
            if (ownerOfItems != null)
            {
                ownerOfItemsID = ownerOfItems.ID;
            }

            BuySellDialog.Fill(mode, getItems, canTrade, ownerOfItemsID, buyerOfItemsID, 
                currentOrders, //selectedAction, allowOrders, 
                settings);

            // center dialog over the click source:
            BuySellDialog.ShowInScreenSpace(absolutePosition.X - BuySellDialog.Window.Width / 2, absolutePosition.Y / 2, false);


        }

        /// <summary>
        /// embargoes, polices etc...
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool CanTrade(Expedition expedition, EntityType entityType, out TierOrAreaType unavailablePolicy)  //TierArea unavailablePolicy)
        { 
            unavailablePolicy = null;

            if (expedition != null)
            {
                if (!expedition.Policy.CanProduceOrTrade(entityType.TierOrAreaType))
                {               
                    unavailablePolicy = entityType.TierOrAreaType;               
                }
            }

            return unavailablePolicy == null;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            InterfaceFont = The.Client.Content.Load<SpriteFont>("Arial");
           
            // cannot do this from load thread:
          /*  if (!Thread.CurrentThread.IsBackground)
            {
                SetInterfaceCursor();
            }*/

            // after load game, only available in LoadPostProcess...
            if (!Snapshotter.IsSnapshotting && The.Sim.StartGameParams.StartScenarioParams != null)
            {
                The.Sim.StartGameParams.StartScenarioParams.Scenario.ScenarioData.LoadContent(The.Client.Content);
            }
        }

        public void OnSetProduction(string entityTypeKey)
        {
            InventoryPanel.OnSetProduction(entityTypeKey);

            foreach (var item in EntityTypeTooltipsStack)
            {
                item.OnSetProduction();
            }

            foreach (var item in PinnedDataTypeTooltips)
            {
                item.OnSetProduction();
            }

            foreach (var item in UnpinnedDataTypeTooltipsOutsideStack)
            {
                item.OnSetProduction();
            }
        }

        /// <summary>
        /// only for testing...
        /// </summary>
        public void SetUIAllegianceToSelectedEntity()
        {
            if (SelectedEntity.HasValue)
            {
                Entity entity = Entity.FindByID(SelectedEntity.Value);
                if (entity != null)
                {
                    UIAllegiance = entity.Intelligence.Allegiance;

                    List<EntityID> memoryFactsToForget = null;
                    foreach (var item in UIAllegiance.SharedKnowledge.MemoryFacts)
                    {
                        Entity factEntity = Entity.FindByID(item.Key);
                        if (factEntity != null)
                        {
                            The.Client.SetRenderableOnMemoryFact(item.Value, factEntity, UIAllegiance);
                        }
                        else
                        {
                            // else crash...
                            Common.AddToList(ref memoryFactsToForget, item.Key);
                        }
                    }   
                
                    if (memoryFactsToForget != null)
                    {
                        foreach (var item in memoryFactsToForget)
                        {
                            UIAllegiance.SharedKnowledge.DeleteMemoryOfEntity(item, null, true);
                        }
                    }
                }
            }
        }

        public void ResetUIAllegiance()
        {
            UIAllegiance = The.Sim.PlaySite.PlayerAllegiance;
        }

        /// <summary>
        /// Called each frame
        /// 
        /// This will return marker windows that are offscreen or unused to the pool. For entity markers it will also add new markers if necessary   
        /// 
        /// control spokenLine window and action marker window visibility here as well - these are child windows from the marker window. But perhaps we should not couple them...
        /// </summary>
        /// <param name="elapsedTime"></param>
        private void UpdateMarkerWindows(GameTime elapsedTime, SimSide.Collisions.CollideShape2D screenBounds)
        {
            RefreshEntityMarkers(elapsedTime, screenBounds);
            RefreshExpeditionMarkers(screenBounds);
            RefreshZoneMarkers(screenBounds);

            //this code will remove the marker windows that should no longer be displayed, and update the rest
            foreach (var activeMarkerWindow in activeMarkerWindows)
            {
                if (activeMarkerWindow.Value.GetCurrentType() == MarkerWindow.MarkerType.Entity)
                {
                    // remove markers for entities that have gone off-site
                    if (!activeMarkerWindow.Value.EntityIsValid())
                    {
                        markersToRemove.Add(activeMarkerWindow.Key);
                        continue;
                    }
                }

                if (activeMarkerWindow.Value.GetCurrentType() != MarkerWindow.MarkerType.Entity)//entity markers use the quad tree to check if the marker is on screen
                {
                    if (screenBounds.ContainsPoint(activeMarkerWindow.Value.MarkerWorldPosition) == false)
                    {
                        markersToRemove.Add(activeMarkerWindow.Key);
                        continue;
                    }
                }

                if (activeMarkerWindow.Value.IsRenewedThisFrame == false
                    && !activeMarkerWindow.Value.ShowWhileLineIsSpoken)
                {               
                    markersToRemove.Add(activeMarkerWindow.Key);
                }

                activeMarkerWindow.Value.Update();

                activeMarkerWindow.Value.IsRenewedThisFrame = false; // reset for next frame
            }

            foreach (var markerKey in markersToRemove)
            {
                MarkerWindow markerWindow = activeMarkerWindows[markerKey];
                activeMarkerWindows.Remove(markerKey);

                markerWindow.Reset();
                markerWindow.Hide();
                poolOfMarkerWindows.Retire(markerWindow);
            }

            markersToRemove.Clear();
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="screenBounds"></param>
        public void UpdateSpokenLines(SimSide.Collisions.CollideShape2D screenBounds)
        {
            foreach (var spokenLine in activeSpokenLines)
            {
                if (screenBounds.ContainsPoint(spokenLine.GetScreenPosition().ToVector2()))
                {

                }
                else
                {

                }
            }
        }

        
        public DataSheet GetAnySpawnedTooltip(DataTypeButton /* UIComponent*/ spawningButton)
        {
            DataSheet otherTooltip = EntityTypeTooltipsStack.Find(tt => tt.SpawningControl == spawningButton);
            if (otherTooltip == null)
            {
               // DataTypeButton dataTypeButton = spawningButton as DataTypeButton;
                foreach (var item in PinnedDataTypeTooltips)
                {
                   // if (item.SpawningControl == spawningButton)
                    if (spawningButton.ShowsData(item))
                    {
                        otherTooltip = item;
                        break;
                    }
                }

                foreach (var item in UnpinnedDataTypeTooltipsOutsideStack)
                {
                    if (spawningButton.ShowsData(item))
                    {
                        otherTooltip = item;
                        break;
                    }
                }

            }

            return otherTooltip;
        }

        public bool TryToAddZoneMarker(Zone zone, SimSide.Collisions.CollideShape2D screenBounds)
        {
            // crash here: http://steamcommunity.com/app/284100/discussions/2/351660338689301391/

            try
            {
                if (zone.MapArea.BottomLeftTile == null)
                {
#if !RELEASE
                    throw new Exception("BottomLeftTile error");
#endif

                    return false;
                } 

                if (screenBounds.ContainsPoint(MapManager.TilePosToWorldPos(zone.MapArea.BottomLeftTile.TilePos).ToVector2()) == true)
                {
                    MarkerWindow marker = poolOfMarkerWindows.Get();
                    if (marker != null)
                    {
                        marker.FillWithZoneInfo(zone);
                        activeMarkerWindows.Add(zone, marker);

                        marker.Show();
                        return true;
                    }
                    else
                    {
                        // could not get a marker window, this should never happen
                    }
                }

                return false;
            }
            catch(Exception e)
            {
                StringBuilder info = new StringBuilder();
                info.AppendLine(e.Message);
                info.AppendLine("Info: ");
                if (screenBounds == null)
                {
                    info.AppendLine("Screenbounds is null");
                }
                if (zone == null)
                {
                    info.AppendLine("Zone is null");
                }
                else
                {
                    if (zone.MapArea == null)
                    {
                        info.AppendLine("zone.MapArea is null");
                    }
                    else
                    {
                        if (zone.MapArea.BottomLeftTile == null)
                        {
                            info.AppendLine("zone.MapArea.BottomLeftTile is null");
                        }
                    }
                }

                throw new Exception(info.ToString());
            }
        }

        private void RefreshExpeditionMarkers(SimSide.Collisions.CollideShape2D screenBounds)
        {
            if (showOverlaysAndMarkerWindows)
            {
                if (UIAllegiance.HumanActivities != null)
                {
                    MarkerWindow markerWindow;
                    foreach (var expedition in UIAllegiance.Expeditions)
                    {
                        if (!activeMarkerWindows.TryGetValue(expedition, out markerWindow))
                        {
                            if (TryToAddExpeditionMarker(expedition, screenBounds))
                            {
                                activeMarkerWindows[expedition].IsRenewedThisFrame = true;
                            }
                        }
                        else
                        {
                            markerWindow.IsRenewedThisFrame = true;
                        }
                    }
                }
            }
        }

        private void RefreshZoneMarkers(SimSide.Collisions.CollideShape2D screenBounds)
        {
            if (The.InGameUI.UIExpedition != null)
            {
                Expedition expedition = Expedition.FindByID(The.InGameUI.UIExpedition.Value);
                if (expedition != null)
                {
                    MarkerWindow markerWindow;

                    foreach (var zone in expedition.OwnedEntities.Zones)
                    {
                        if (!activeMarkerWindows.TryGetValue(zone, out markerWindow))                     
                        {
                            if (TryToAddZoneMarker(zone, screenBounds))
                            {
                                activeMarkerWindows[zone].IsRenewedThisFrame = true;
                            }
                        }
                        else
                        {
                            markerWindow.IsRenewedThisFrame = true;
                        }
                    }
                }
            }
        }

        public bool TryToAddExpeditionMarker(Expedition expedition, SimSide.Collisions.CollideShape2D screenBounds)
        {
            if (screenBounds.ContainsPoint(expedition.Location.Value.ToVector2()) == true)
            {
                MarkerWindow marker = poolOfMarkerWindows.Get();
                if (marker != null)
                {
                    marker.FillWithExpeditionInfo(expedition);
                    activeMarkerWindows.Add(expedition, marker);

                    marker.Show();
                    return true;
                }
                else
                {
                    //could not get marker window from pool, this shouldn´t happen
                }
            }
            return false;
        }

        private void RefreshEntityMarkers(GameTime elapsedTime, SimSide.Collisions.CollideShape2D screenBounds)
        {
           // GetEntitiesToShowMarkerWindowsFor(screenBounds);

            List<Pair<EntityID, Vector2>> entitiesOnScreen = new List<Pair<EntityID, Vector2>>();
            UIAllegiance.SharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.GetObjectsIntersectingBounds(screenBounds, null, ref entitiesOnScreen);

            IKnownEntityData currentData;
            MarkerWindow activeMarkerWindow = null;

            foreach (var member in entitiesOnScreen)
            {
                bool showMarker = false;
                activeMarkerWindow = null;
              
                The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(member.First, out currentData); // The.Sim.PlaySite.PlayerAllegiance.SharedKnowledge.GetKnownData(member.First, out currentData);

                if (currentData != null)
                {
                    // filter entities that we never show status for, like parts:
                    if (currentData.PartOfID != null)
                    {
                        continue;
                    }

                   // is the marker window currently shown?
                    activeMarkerWindows.TryGetValue(currentData.EntityID, out activeMarkerWindow);

                    if (currentData.EntityID == SelectedEntity)
                    {
                        showMarker = true; // selected entity always shows its marker window
                    }
                    else
                    {
                        EntityType.ShowMarkerWindowMode showMode = currentData.EntityType.GetShowMarkerWindowMode(); // ShowMarkerWindowSetting;

                        switch (showMode)
                        {
                            case EntityType.ShowMarkerWindowMode.Always:
                                if (showOverlaysAndMarkerWindows == true)
                                {
                                    showMarker = true;
                                }

                                //entitiesToShowMarkerWindowsFor.Add(currentData);
                                break;

                            case EntityType.ShowMarkerWindowMode.ByStatus:
                                if (showOverlaysAndMarkerWindows == true)
                                {
                                    if (currentData.IsTimeToShowStatusMarkerWindow()) // don't do this test every frame, only once per second or so
                                    {
                                        if (MarkerWindow.HasStatusIconsToShow(currentData as IHasExposedProperties))
                                        {
                                            showMarker = true;
                                            // entitiesToShowMarkerWindowsFor.Add(currentData);
                                        }
                                        else
                                        {
                                            showMarker = false;
                                        }
                                    }
                                    else if (activeMarkerWindow != null)
                                    {
                                        // keep showing the window if it is currently shown, until it is time to check the status again
                                        showMarker = true;
                                    }
                                }

                                break;
                        }
                    }
                }
                
                if (showMarker == true)
                {
                    // create and fill the window if it odes not exist:
                    if (activeMarkerWindow == null)
                    {
                        if (TryToAddEntityMarkerWindow(member.First, out activeMarkerWindow) == false) 
                        {
                            break;
                        }
                    }

                    if (!activeMarkerWindow.DisplayWindow.Visible) // show it
                    {
                        activeMarkerWindow.Show();
                    }

                    activeMarkerWindow.IsRenewedThisFrame = true; 
                }
                   

            }            

        }


        private bool TryToAddEntityMarkerWindow(EntityID entityID, out MarkerWindow activeMarkerWindow)
        {
            activeMarkerWindow = poolOfMarkerWindows.Get();
            if (activeMarkerWindow == null)
            {
                return false;
            }
            else
            {
                activeMarkerWindow.FillWithEntityInfo(entityID);
                activeMarkerWindows.Add(entityID, activeMarkerWindow);

                activeMarkerWindow.Show();

                return true;
            }
        }
       

        public void Initialize()
        {           

            expandedInterfaceWidth = Math.Min(750, The.MapUI.mapWindowWidth);

            The.MapUI.noOfTilesLeftOfDetailsInterface =
                Math.Max((The.MapUI.mapWindowWidth - expandedInterfaceWidth) / MapManager.tileSize, 0);

            The.MapUI.widthOfWindowLeftOfDetailsInterface = Math.Max(The.MapUI.mapWindowWidth - expandedInterfaceWidth, 0);

            MainLeft = The.MapUI.mapWindowWidth - InterfaceWidth;
            MainTop = The.MapUI.mapWindowHeight;


            sidePanelHeight = (int)(Math.Min(The.MapUI.mapWindowHeight - SmallPanelTop - rosterPanelBottomMargin, maxPanelHeight));
            sidePanelFullHeight = (int)(The.MapUI.mapWindowHeight - SmallPanelTop - 10);

           
            // not used..
            expandedInterfaceLeft = Math.Max(The.MapUI.mapWindowWidth - expandedInterfaceWidth, 0);

            rosterPanelHeight = The.MapUI.mapWindowHeight - 110;//210; // 300;// 295; //160;

            PinnedDataTypeTooltips = new HashSet<DataSheet>();
            UnpinnedDataTypeTooltipsOutsideStack = new HashSet<DataSheet>();


            gui.HyperlinkClicked += new GUIManager.HyperlinkClickedHandler(gui_HyperlinkClicked);
            gui.MouseOverWindow += new Action<Window>(gui_MouseOverWindow);
            gui.MouseOutOfWindow += new Action<Window>(gui_MouseOutOfWindow);
            //characterNearbyBounds = new Rectangle(
            /*
            DisplayPanelRenderer = new DisplayPanelRenderer(UWGame.SimSide.Instance);
            DisplayPanelRenderer.Initialize();
            */

            //UIAllegiance = The.Sim.PlaySite.PlayerAllegiance;


            //MapUI = new MapUI();

            Selection = new Selection();            
            SelectRectangle = new SelectRectangle();

            TerrainBlockingRender = new MapGUI.TerrainBlockingRender();
           
            SelectedCycleAnimationQuick = new WindowSystem.Animation2D(null, 0.15f, true)  //(null, 0.15f, true) MP the quick icon blinking anim for newly found resources
            {
                DoColorInterpolation = true, // false,
                Cells = new List<WindowSystem.Cell>() 
                { 
                    new Cell() { Color = Color.White },
                    new Cell() { Color = Animation2D.halfTransp },
                    new Cell() { Color = Color.White
                    }                
                }
            };

            SelectedCyclePlayerFlashing = new Animation2DPlayer();
            SelectedCyclePlayerFlashing.StartAnimation(SelectedCycleAnimationQuick);

            SelectedCycleAnimation = new WindowSystem.Animation2D(null, 0.6f, true)
            {
                DoColorInterpolation = true, // false,
                Cells = new List<WindowSystem.Cell>() 
                { 
                    new Cell() { Color = Color.White },
                    new Cell() { Color = Animation2D.halfTransp },
                    new Cell() { Color = Color.White
                    }                
                }
            };

            SelectedCyclePlayer = new Animation2DPlayer();
            SelectedCyclePlayer.StartAnimation(SelectedCycleAnimation);

            //this special zone is always selected when it is visible
            SelectedTiles = new MapArea(); 
            SelectedTiles.Initialize(); // needed because we are using the empty ctor while snapshotting
            SelectedTiles.MapAreaRender.IsSelected = true;
            SelectedTiles.MapAreaRender.Color = Color.Yellow; // Color.White;

            SelectedTilesPreview = new MapArea();
            SelectedTilesPreview.Initialize();
            SelectedTilesPreview.MapAreaRender.IsSelected = false;
            SelectedTilesPreview.MapAreaRender.Color = Color.Gray;


            MenuDialog = new InGameMenuDialog();

            // windows for the pools cannot be created earlier:
            poolOfEntityTypeTooltips = new Pool<EntityDataSheet>(1, 15); // takes several seconds
            poolOfProcessTypeTooltips = new Pool<ProcessTypeDataSheet>(1, 5); // takes several seconds

            poolOfMarkerWindows = new Pool<MarkerWindow>(20, 1);//new Pool<StatusIcon>(0, 0);
                      
            poolOfSpokenLines = new Pool<SpokenLine>(1, 5);
            
            EventDialog = new EventDialog(this);
            BuySellDialog = new BuySellPanel(this, Point.Zero);
            MessageBox = new Interface.MessageBox(this);
            SaveLoadMessageBox = new Interface.SaveLoadMessageBox(this);
            PersonnelDialog = new Personnel.PersonnelDialog(this, Point.Zero);
            WorldMapDialog = new World_map.WorldMapDialog(this, Point.Zero);
            // poolOfHelpTopicDialogs = new Pool<HelpTopicDialog>(1, 10);

            // create one collection of dialogs:
            HelpTopicDialogs = new Dictionary<HelpTopic, HelpTopicDialog>(); // to enable Init() to be called again
            foreach (var item in GameData.Instance.AllHelpTopics)
            {
                HelpTopicDialog dlg = new HelpTopicDialog(item.Value);
                HelpTopicDialogs.Add(item.Value, dlg);
            }

            foreach (var item in GameData.Instance.AllTutorialTopics)
            {
                HelpTopicDialog dlg = new HelpTopicDialog(item.Value);
                HelpTopicDialogs.Add(item.Value, dlg);
            }

            replayTime = new TextWindow(false);
            replayTime.ShowInScreenSpace(10, 100);


            if (The.Sim.Mode == Sim.EngineMode.Game) // NEW
            {
                InventorySettings = new InventorySettings();
                OverlaySettings = new OverlaySettings();
                BuySettings = new BuySellPanelSettings();
                SellSettings = new BuySellPanelSettings();
                TaskSettings = new TaskSettings();
                FoodProductionSettings = new FoodProductionSettings();
                ProductionSettings = new ProductionSettings();
                KillsSettings = new Ledger.KillsSettings();
                NutrientSheetSettings = new NutrientSheetSettings();

                ThreatRender = new MapGUI.ThreatRender();

               // MainPanel.OnSetSpeed(The.Sim.spee)
            }
                     
        }

        void gui_MouseOutOfWindow(Window obj)
        {
            
        }

        void gui_MouseOverWindow(Window obj)
        {
            
        }

        public void ShowInGameMenu()
        {            
            The.InGameUI.MenuDialog.ShowDialog(true);
                      
            MainPanel.tbMain.IsChecked = true;
              
        }

        public void HideInGameMenu()
        {
            The.InGameUI.MenuDialog.Hide();

            The.Client.SetModal(false);     

            MainPanel.tbMain.IsChecked = false;
            
        }


      /*  public void HideSidePanels()
        {
            StatusScreen.DisplayWindow.Hide();

            displayedSidePanel.Hide();          
        }*/
        
       
        public void ShowContextMenu(int screenX, int screenY, Window parentWindow)
        {
            ContextMenu.OpeningWindow = parentWindow;

            if (!ContextMenu.DisplayWindow.IsVisibleAndActive)
            {
                ContextMenu.RefreshThisPanel();

                ContextMenu.ShowOnPlayfield(screenX, screenY); // + DisplayWindow.Height + 4);

            }
        }


     /*   public void ShowSidePanels()
        {
            if (displayedSidePanel != null)
            {
                StatusScreen.DisplayWindow.Show();
                displayedSidePanel.Show();
            }
        }*/


        public void DestroyEntityBeingPlaced()
        {
            if (EntitiesBeingPlaced != null)
            {
                foreach (EntityPosition ep in EntitiesBeingPlaced)
                {
                    ep.Entity.Destroy();
                }
            }

            //EntityBeingPlaced = null;
            EntitiesBeingPlaced.Clear();
        }
        /*  public override void UnloadContent()
          {

              // Very important to avoid getting more and more components when going to the start menu and back:
              UWGame.SimSide.Instance.ScreenManager.Game.Components.Remove(input);
              UWGame.SimSide.Instance.ScreenManager.Game.Components.Remove(gui);
              UWGame.SimSide.Instance.ScreenManager.Game.Services.RemoveService(typeof(IInputEventsService));

              // dispose cursors?           
              fingerCursor.Dispose();
              fingerDownRight.Dispose();
              fingerLeftRight.Dispose();
              fingerUpRight.Dispose();
              fingerUpDown.Dispose();
              fingerMove.Dispose();
              lcdCursor.Dispose();
              worldCursor.Dispose();

              base.UnloadContent();
          }*/

     /*   public bool IsCurrentlyDisplayed(ExpandedPanel panel)
        {
            return displayedExpandedPanel == panel;
        }*/

        /*  void gui_SetMouseCursorEvent(MouseSprites mouseSprite)
          {
              switch (mouseSprite)
              {
                  case MouseSprites.Moving:
                      windowForm.Cursor = worldCursor;
                      break;
                  default:
                      windowForm.Cursor = lcdCursor;
                      break;
              }
          }*/



        void gui_HyperlinkClicked(uint? entityID, uint? containerID, uint? zoneID, Point? mapPosition,
            //IHyperlinkTarget linktarget, 
            WindowSystem.GUIManager.MouseButtonClicked button)
        {

            if (entityID.HasValue) // clickedEntityData != null)
            {
                IKnownEntityData clickedEntityData;
                UIAllegiance.SharedKnowledge.GetKnownData((EntityID)entityID.Value, out clickedEntityData);

                if (CanSelectEntity(clickedEntityData))
                {
                    if (button == GUIManager.MouseButtonClicked.Left)
                    {
                        if (selectedEntity == clickedEntityData.EntityID)
                        {
                            ZoomToEntity(clickedEntityData);
                        }
                        else
                        {
                            SelectEntity(clickedEntityData);
                        }
                    }
                    else
                    {
                        ZoomToEntity(clickedEntityData);
                    }
                }
                // remove dead links??? must be done during their update.
            }

            //Zone zone = linktarget as Zone;
            if (mapPosition.HasValue)
            {
                The.MapUI.ZoomToMapPosition(mapPosition.Value.X, mapPosition.Value.Y);
            }

            if (zoneID.HasValue)
            {
                Zone zone = LookUp<Zone, ZoneID>.FindByID((ZoneID)zoneID.Value);
                if (zone != null)
                {
                    The.MapUI.ZoomToMapPosition(zone.MapArea.UpperLeftTile.TilePos.ToPoint());
                    /*  // TODO
                    The.InGameUI.SelectedZone = zone;*/
                }
                
            }

            if (containerID.HasValue)
            {

                ResourceContainer container = LookUp<ResourceContainer, ResourceID>.FindByID((ResourceID)containerID.Value);

                if (container != null)
                {
                    container.FlashWhenClicked();
                    The.MapUI.ZoomToMapPosition(container.MapPosition.X, container.MapPosition.Y);
                }
            }
        }

        public void EnableTracking(bool enable)
        {
            if (StatusScreen != null)
                StatusScreen.SetCenterButtonChecked(enable);

            /*
            if (SidePanelEntity != null)
                SidePanelEntity.SetCenterButtonChecked(enable);*/

            TrackSelectedEntity = enable;
        }

        public void ZoomToEntity(IKnownEntityData entity)
        {
            /*if (displayedExpandedPanel != null)
            {
                The.MapUI.ZoomToMapPosition(entity.Location, MapClient.Centering.LeftOfCenter); //entity.MapPosition.X, entity.MapPosition.Y, MapManager.Centering.LeftOfCenter);
            }
            else
            {*/
            The.MapUI.ZoomToMapPosition(entity.PlaySiteLocation); //entity.MapPosition.X, entity.MapPosition.Y, MapManager.Centering.Middle);
            //}
        }


        public static void PlaceWindowInsideViewableArea(UIComponent window, UIComponent boundingArea /* Rectangle area*/)
        {
            int maxWidth = boundingArea.Width; 
           /* if (avoidRightInterfaceArea)
            {
                maxWidth -= InGameInterface.InterfaceWidth;
            }*/

            if (window.X < 0) // boundingArea.X)
            {
                window.X = 0; // boundingArea.X;
            }

            if (window.Right > maxWidth)
            {
                window.X = maxWidth - window.Width;
            }

            int maxHeight = boundingArea.Height; 

            if (window.Y < 0) //boundingArea.Y)
            {
                window.Y = 0; // boundingArea.Y;
            }

            if (window.Bottom > maxHeight)
            {
                window.Y = maxHeight - window.Height;
            }
        }

        /// <summary>
        /// hides most windows and overlays
        /// </summary>
        public void HideMapInterface()
        {
            InterfaceMode = InGameInterface.InterfaceState.None;

            ChangeRosterPanel(The.InGameUI.SidePanelEmpty);

            SelectedEntity = null;

            //The.InGameUI.GatherResources.Hide();

            RemoveSelectedTiles();


         //   HideSidePanels();

            CloseRosterPanel();

           

            foreach (var window in hudWindows)
            {
                if (window.HideOnRightClick)
                {
                    if (window.DisplayWindow.IsVisibleAndActive)
                    {
                        window.Hide();
                    }
                }
            }

            ShowOverlaysAndMarkerWindows = false;
        }


      /*  public bool ShowOrHideRosterPanel(RosterPanel rosterPanel)
        {
            if (displayedRosterPanel == rosterPanel)
            {
                CloseRosterPanel();
                
                // bring side panel back??                    
               // ShowSidePanels(); // shows CRT also
               
                return false;
            }
            else 
            {
                if (displayedRosterPanel != null)
                {
                    CloseRosterPanel();
                }

              //  HideSidePanels(); // hides CRT also...
                     

                displayedRosterPanel = rosterPanel;

                displayedRosterPanel.Show();

                return true;
            }
        }*/

        public void CloseRosterPanel()
        {
            if (displayedRosterPanel != null)
            {
                displayedRosterPanel.Hide();

                displayedRosterPanel = null;
            }

            HideStatusScreen();

        }

        public void PostLoadContent()
        {

            if (The.Sim.Mode != Sim.EngineMode.Edit)
            {
                FogMap = new FogMap();
            }

            /* Rectangle rect = gui.GUISpriteSheet.SourceRectangle("right_side_table");
             CreateBackgroundWindow(gui, rect, new Point(UWGame.SimSide.Instance.GraphicsDevice.Viewport.Width - rect.Width,
                 UWGame.SimSide.Instance.GraphicsDevice.Viewport.Height - rect.Height));
             */

            Rectangle directSource = new Rectangle(The.InGameUI.expandedInterfaceLeft, InGameInterface.rosterPanelTop, framedCRTWidth, framedCRTHeight);



            //    "right here is the latest we can wait before adding InGameUI to screen manager"

            //   this.Game.Components.Add(gui);//move to here from CommonInterface ctor


            framedCRT = new FramedCRT(this, new Rectangle(The.InGameUI.expandedInterfaceLeft, InGameInterface.rosterPanelTop, framedCRTWidth, framedCRTHeight),
                                               directSource /*centerOfScreen*/, Level.Middle);

            framedCRT.crtTextAnimatorCharacter.TimeBetweenUpdates = 0.03f;
            framedCRT.crtTextAnimatorLine.TimeBetweenUpdates = 0.12f;

            StatusScreen = new StatusScreen();

            HUDOverlayPanel = new HUDOverlayPanel();

            if (The.Sim.Mode != Sim.EngineMode.Edit)
            {               
               // HUDOverlayPanel = new HUDOverlayPanel();
               
                HUDActionPanel = new HUD_Windows.HUDEntityContextMenu();
                HUDHelpPanel = new HUDHelpPanel();               
            }

            SidePanelEmpty = new SidePanelEmpty();
            ChangeRosterPanel(SidePanelEmpty);

            //   gui.ShowTooltip += new GUIManager.ShowTooltipHandler(gui_ShowTooltip);
            //  gui.HideTooltip += new GUIManager.ShowTooltipHandler(gui_HideTooltip);
            //    gui.WindowClosed += new GUIManager.WindowClosedHandler(gui_WindowClosed);


            // forms.Add(new Form(new Vector2(100f, 100f), new Vector2(300f, 200f), "Test Form", Color.White, Color.Black, "tahoma", 1f, true, true, true, true, Form.BorderStyle.FixedSingle, Form.Style.Default));


            // Small Entity panel:
            SidePanelEntity = new SidePanelEntity();
            SidePanelMapArea = new SidePanelMapArea();

            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
              /*  InventorySettings = new InventorySettings();
                               
                BuySettings = new BuySellPanelSettings();
                SellSettings = new BuySellPanelSettings();
                */

                InventoryPanel = new InventoryPanel();
                JobsPanel = new JobsPanel();
                GraphPanel = new GraphPanel();
                LedgerPanel = new LedgerPanel();
                WorldMapPanel = new WorldMapRosterPanel();
                MissionsPanel = new MissionsPanel();
                CreateMissionPanel = new CreateMissionPanel();
                EventArchivePanel = new EventArchivePanel();
                PersonnelRosterPanel = new PersonnelRosterPanel();
                PolicyPanel = new PolicyPanel();
              
            }
            else
            {
                SidePanelEditorEntity = new SidePanelEditorEntity();
                SidePanelEditorSoil = new SidePanelEditorSoil();
                SidePanelEditorTerrainHeight = new SidePanelEditorTerrainHeight();
                SetTileResources = new SetTileResourcesWindow();
            }

          
            // Tile panel:
            //    SidePanelTile = new SmallTilePanel();

            // Main panel:
            //   mainControlPanel = new MainControlPanel();

            RosterAccessPanel = new RosterAccessPanel();

            // bottom panels
            // ------------------
        
            
                     
            MainPanel = new Interface.MainPanel();

            
            int screenWidth = (int)Common.Clamp(0.22 * The.MapUI.mapWindowWidth, minimapMinWidth, minimapMaxWidth);
            int screenHeight = (int)(0.75 * screenWidth);

            int screenX = (screenWidth == minimapMaxWidth ? 15 : 0);
           
            OverlayPanel = new OverlayPanel(screenX + Minimap.GetWidth(screenWidth));
            Minimap = new Minimap(screenWidth, screenHeight, screenX);

            EntityTypeTooltipsStack = new List<DataSheet>();
           
            Tooltip = new Tooltip(The.InGameUI);


            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                SetupEventLogPanel();

                SetupCountersAndDateTimePanels();

                TalkPanel = new Interface.TalkPanel();

                ContextMenuOpener = new HUD_Windows.ContextMenuOpener();
                ContextMenu = new HUD_Windows.TileSelectionContextMenu();

                EntityListWindow = new HUD_Windows.EntityListWindow();

                RatingsPanel = new RatingsPanel(); // new HUD_Windows.StatsWindow(325, 20, 110, 65);
                CounterPanel = new CounterPanel(400);
                SetCounterPanelPosition();

                if (The.Sim.StartGameParams.StartScenarioParams != null)
                {
                    EnableScenarioUI(The.Sim.StartGameParams.StartScenarioParams.Scenario.ScenarioData);
                }

            }

            Selection.PostLoadContent();

            SelectedTiles.PostLoadContent();

            TerrainBlockingRender.PostLoadContent();
            ThreatRender.PostLoadContent();

            // detect & handle overlap on small screens:
            ArrangeAccessPanels();
            ArrangeTopPanels();


            // hide stuff to begin with:
            The.InGameUI.HideMapInterface();
        }

        private void SetCounterPanelPosition()
        {
            CounterPanel.Window.X = RatingsPanel.Window.Right - 2;

        }

        private void ArrangeTopPanels()
        {
            if (CounterPanel != null 
                && CounterPanel.Window.Right > dateAndWeather.PlasticPanel.X)
            {
                // overlap detected. move the panels close together:
                RatingsPanel.Window.X = MainPanel.DisplayWindow.Right;
                SetCounterPanelPosition();
                dateAndWeather.SetPosition(CounterPanel.Window.Right);
            }
        }


        private void ArrangeAccessPanels()
        {
            /*if (LogPanel != null && ActionPanel.DisplayWindow.Right > LogPanel.PlasticPanel.X)
            {
                // overlap detected. move the access buttons close together:
               // OverlayPanel.DisplayWindow.X = screenX + Minimap.GetWidth(screenWidth);//MinimapAccessPanel.DisplayWindow.Right;

                ActionPanel.DisplayWindow.X = OverlayPanel.DisplayWindow.Right;
            }*/

        }

        public override void Destroy()
        {
            // don't hang on to these...
            /*gui.ShowTooltip -= new GUIManager.ShowTooltipHandler(gui_ShowTooltip);
            gui.HideTooltip -= new GUIManager.ShowTooltipHandler(gui_HideTooltip);
            */
            if (Tooltip != null)
            {
                Tooltip.Destroy();
            }

            //   gui.WindowClosed -= new GUIManager.WindowClosedHandler(gui_WindowClosed);

            gui.HyperlinkClicked -= new GUIManager.HyperlinkClickedHandler(gui_HyperlinkClicked);


            base.Destroy();
        }

        /*   void gui_WindowClosed(Window sender)
           {
               // make sure there are no orphan tooltips
               if (Tooltip.SpawningWindow == sender)
               {
                   Tooltip.Hide();
               }
           }*/





        /*
        /// <summary>
        /// gets called on every ui element...
        /// </summary>
        /// <param name="sender"></param>
        void gui_HideTooltip(UIComponent sender)
        {
            if (sender.ToolTip == null)
                return;

            this.Tooltip.Hide();
           
        }
        */

        /*  public void HandleEntityTypeInfoClick(UIComponent sender, EventArgs e)
          {
              HandleEntityTypeClick(sender, e, EntityTypeTooltip.InfoToShow.Data);
          }

          public void HandleEntityTypeProductionClick(UIComponent sender, EventArgs e)
          {
              HandleEntityTypeClick(sender, e, EntityTypeTooltip.InfoToShow.Production);
          }*/

        /// <summary>
        /// the player clicked an entity type. make sure that its tooltip window is shown and expanded immediately
        /// 
        /// if already expanded, a click will close it
        /// </summary>
        /// <param name="button"></param>
        /// <param name="e"></param>
      /*  public void HandleEntityTypeButtonClick(EntityTypeButton button, EventArgs e, HUD_Windows.DataTypeTooltip.InfoToShow infoToShow)
        {
            int x, y;

            // TODO: move this method to the button class

            DataTypeTooltip tooltip;

            EntityTypeButtonEventArgs typeArgs = e as EntityTypeButtonEventArgs;


            // is the window already shown for this spawning control?
            tooltip = EntityTypeTooltipsStack.Find(tt => tt.SpawningControl == button);

            if (tooltip != null)
            {
                // is the tool tip expanded?
                if (tooltip.CurrentState != DataTypeTooltip.State.Collapsed)
                {
                    // then close it:
                    tooltip.Hide();
                }
                else
                {
                    // expand it:

                    SelectAnchorPoint(button, tooltip.DisplayWindow, button.SideToAnchorOn, DataTypeTooltip.ExpandedHeight, true, DataTypeTooltip.GetAnchorPointYOffset(), out x, out y);

                    tooltip.PopulateAndShow(button, typeArgs.Owner, typeArgs.UseUIOwner, x, y);
                    button.SetTooltipData(tooltip);
                    tooltip.Expand(infoToShow);
                }
            }
            else
            {

                if (button.IsRoot)
                {
                    // close other line of tooltips:
                    CloseAllEntityTooltips();
                }

                // get another tooltip and expand it:             
                tooltip = The.InGameUI.poolOfEntityTypeTooltips.Get();
           
                SelectAnchorPoint(button, tooltip.DisplayWindow, button.SideToAnchorOn, DataTypeTooltip.ExpandedHeight, true, DataTypeTooltip.GetAnchorPointYOffset(), out x, out y);

                tooltip.PopulateAndShow(button, typeArgs.Owner, typeArgs.UseUIOwner, x, y);
                button.SetTooltipData(tooltip);
                tooltip.Expand(infoToShow);
            }

        }*/

        public void CloseAllEntityTooltips()
        {
            for (int i = EntityTypeTooltipsStack.Count - 1; i >= 0; i--)
            {
                EntityTypeTooltipsStack[i].Hide();
            }
        }

        /*

           void gui_ShowTooltip(UIComponent sender)
           {           
               Tooltip.StartCountdownToShow(sender);
                      
           }

           */

        public DataSheet GetChildTooltip(DataSheet tooltip)
        {
            int thisIndex = EntityTypeTooltipsStack.IndexOf(tooltip);

            if (thisIndex > -1
              && EntityTypeTooltipsStack.Count > thisIndex + 1)
            {
                return EntityTypeTooltipsStack[thisIndex + 1];
            }

            return null;
        }

        public DataSheet GetParentTooltip(DataSheet tooltip)
        {
            int thisIndex = EntityTypeTooltipsStack.IndexOf(tooltip);

            if (thisIndex > 0)
            {
                return EntityTypeTooltipsStack[thisIndex - 1];
            }

            return null;
        }

        private void EnableScenarioUI(ScenarioData scenario)
        {
            if (scenario.EnableMissions == false)
            {
                RosterAccessPanel.DisableMissions();
            }

            if (scenario.EnableGraphs == false)
            {
                RosterAccessPanel.DisableGraphs();
            }

           /* if (scenario.EnableContacts == false)
            {
                RosterAccessPanel.DisableContacts();
            }*/
            if (scenario.EnablePersonell == false)
            {
                RosterAccessPanel.DisablePersonell();
            }

            if (scenario.EnableWorldMap == false)
            {
                RosterAccessPanel.DisableWorldMap();
            }

            if (scenario.EnableLedger == false)
            {
                RosterAccessPanel.DisableLedger();
            }

            if (scenario.EnablePolicy == false)
            {
                RosterAccessPanel.DisablePolicy();
            }  
        }


        private void SetupEventLogPanel()
        {
            int width = 459;
           // int commPanelXPos = (int)Common.Clamp(0.3 * The.MapUI.mapWindowWidth, Minimap.DisplayWindow.X + Minimap.DisplayWindow.Width + 2, 615);
           // int commPanelXPos = (int)Common.Clamp(0.3 * The.MapUI.mapWindowWidth, OverlayPanel.DisplayWindow.X + OverlayPanel.DisplayWindow.Width + 2, 615);
            int commPanelXPos = (int)Common.Clamp(0.3 * The.MapUI.mapWindowWidth, OverlayPanel.DisplayWindow.Right + 115, 615);

          
            //  int displayWindowWidth = (int)Common.Clamp(0.5 * The.MapUI.mapWindowWidth, 340f, 660f);
            // int maxDisplayWindowWidth = (int)(The.MapUI.mapWindowWidth - commPanelXPos - CommPanel.ButtonPanelWidth);
            LogPanel = new Interface.LogPanel(commPanelXPos,width);
            //commPanel = new CommPanel(commPanelXPos, displayWindowWidth, maxDisplayWindowWidth);
        }

        private void SetupCountersAndDateTimePanels()
        {

            int topEmptySpace = The.MapUI.mapWindowWidth - DateAndWeather.Width;

            int countersPreferredWidth = 300;

            int dateAndWeatherX;
            int countersWidth;
            if (topEmptySpace < countersPreferredWidth)
            {
                countersWidth = topEmptySpace;
                dateAndWeatherX = countersWidth;
            }
            else
            {
                topEmptySpace = topEmptySpace - countersPreferredWidth;
                countersWidth = countersPreferredWidth;
                dateAndWeatherX = (countersWidth + topEmptySpace) / 2;
            }

            dateAndWeather = new DateAndWeather(dateAndWeatherX);

            //  Summary = new Counters(10, countersWidth);
        }

       /* public static Window CreateBackgroundWindow(GUIManager gui, Rectangle rect, Point pos)
        {
            // Rectangle rect = UWGame.SimSide.Instance.GUISpriteSheet.SourceRectangle(spriteName);
            Window window = new Window(gui); //Interface.Instance.gui);

            // Sequence matters for skins!!!
            window.Skin = rect;
            window.CornerSize = 5; // 7;

            window.Margin = 0;

            window.Position = pos;
            window.Width = rect.Width;
            window.Height = rect.Height;

            window.Level = Level.RockBottom;

            window.Show(); //Make it visible
            window.Resizable = false;
            window.IsMovable = false;
            window.HasCloseButton = false;
            window.Level = Level.RockBottom;
            window.CanHaveFocus = false;

            window.IsBackgroundGraphics = true;

            gui.BringToBottom(window);

            return window;
        }*/

        public void SelectNextEntityOfType(string typeKey)
        {
            //begin iterating entity IDs starting after the current selected entity's ID
            //and skip back to zero, if the last one is reached

            EntityID prevID = SelectedEntity == null ? EntityID.Invalid : SelectedEntity.Value;

            if (prevID == EntityID.Invalid)
                prevID = (EntityID)0;

            int countdown = (int)Entity.LastUsedID;

            for (EntityID iterID = prevID + 1; iterID != prevID; iterID++)
            {
                if (iterID > Entity.LastUsedID)
                    iterID = (EntityID)0;

                Entity entity = Entity.FindByID(iterID);
                if (entity != null && entity.IsOnPlaySite())
                {
                    if (entity.EntityType.KeyName == typeKey)
                    {
                        SelectEntity(entity, true);
                        return;
                    }
                }

                if (countdown-- <= 0)
                    return;
            }


        }

        public static bool CanSelectEntity(IKnownEntityData entityData)
        {
            return entityData != null && Entity.IsOnPlaySite(entityData);
        }

        public void SelectEntity(EntityID entityID, bool centerInView = false)
        {
            IKnownEntityData data;
            The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID, out data);

            if (data != null && CanSelectEntity(data))
            {
                SelectEntity(data, centerInView);
            }
            else
            {
                SelectEntity(null);
            }
        }

        public void SelectEntity(IKnownEntityData selectedEntity, bool centerInView = false)
        {
            if (selectedEntity != null)
            {
                SelectedEntity = selectedEntity.EntityID;

                // start animation:
                Selection.SelectEntity(selectedEntity);
            }
            else
            {
                SelectedEntity = null;
            }
            
           
            if (centerInView && SelectedEntity != null)
            {
                Point mapPos = selectedEntity.MapPosition.Value;
                The.MapUI.ZoomToMapPosition(mapPos.X, mapPos.Y);
            }
        }

        /* public void SelectTiles(List<TerrainTile> tiles)
         {
             SelectedEntity = null;


         }*/

        public void SelectTile(TerrainTile clickedTile)
        {
             //Command selectTileCommand = new Command();
             //selectTileCommand.SelectTile = new SelectTile(clickedTile.ID);
             //The.Client.ScreenManager.StoreAndExecuteCommand(selectTileCommand);

            SelectedEntity = null;

            SelectedZone = null;

            SelectedTiles.Clear();

            SelectedTiles.Add(clickedTile);

            Rectangle boundingRectangle = new Rectangle(clickedTile.X, clickedTile.Y, 1, 1);

            SelectedTiles.StartDragTile = new Point(clickedTile.X, clickedTile.Y);
            SelectedTiles.BoundingRectangle = boundingRectangle;


            //SelectedTiles = clickedTile;       

            // start animation:
            //   Selection.SelectTile();

        }

        /*     public void SelectBuilding(Entity structure)
             {
                 if (SelectedBuilding != structure)
                 {
                     // start animation:
                     Selection.SelectEntity();

                     SelectedBuilding = structure;
                     SelectedTile = null;
                     SelectedEntity = null;
                     //Change(SidePanelBuilding); // Interface.GUIPanels.Building);
                 }
             }*/

        public void Select()
        {

        }

       
        /*
        public void ExpandSidePanel()
        {
            if (displayedExpandedPanel == null && displayedSidePanel.ExpandedPanel != null)
            {
                displayedExpandedPanel = displayedSidePanel.ExpandedPanel;

                if (displayedExpandedPanel.HasCRT)
                {
                    framedCRT.Show();
                    framedCRT.TurnOn();

                    // turn the small screen off:
                    if (displayedSidePanel.HasStatusCRT)
                    {
                        StatusScreen.TurnOff();
                    }
                }


                displayedExpandedPanel.Show();
            }
        }

        public void CollapseExpandedPanel()
        {
            if (displayedExpandedPanel != null)
            {
                if (displayedExpandedPanel.HasCRT)
                {
                    framedCRT.TurnOff();
                    framedCRT.Hide();

                    // turn the small screen off:
                    if (displayedSidePanel.HasStatusCRT)
                    {
                        StatusScreen.TurnOn();
                    }
                }

                displayedExpandedPanel.Hide();
                displayedExpandedPanel = null;
            }
        }

        public void ExpandOrCollapse()
        {
            if (displayedExpandedPanel != null)
            {
                CollapseExpandedPanel();
            }
            else
            {
                ExpandSidePanel();
                //intf.ShowExpandedGUIPanel(Interface.GUIPanels.Entity);
            }
        }*/


        public void ShowInventoryPanel()
        {
            if (!IsDisplayed(InventoryPanel))
            {
                ChangeRosterPanel(InventoryPanel);
            }
        }
       

        /// <summary>
        /// clears selection! 
        /// </summary>
        /// <param name="panel"></param>
        public void ChangeRosterPanel(RosterPanel panel) 
        {            
            ChangeRosterPanel(panel, false);
        }

        private bool IsDisplayed(RosterPanel rosterPanel)
        {
            return rosterPanel == displayedRosterPanel;
        }

        public void ChangeRosterPanel(RosterPanel panel, bool refreshCurrentPanelWithNewContent)
        {
            if (!refreshCurrentPanelWithNewContent)
            {
                if (IsDisplayed(panel)) // displayedRosterPanel == panel)
                {
                    CloseRosterPanel(); // NEW
                    return;
                }
            }

            bool isRefreshingExistingPanel = (refreshCurrentPanelWithNewContent && IsDisplayed(panel)); // displayedRosterPanel == panel);


            if (panel != displayedRosterPanel && displayedRosterPanel != null)
            {
                displayedRosterPanel.Hide();
                //displayedSidePanel.Hide();
            }


            if (!IsDisplayed(panel)) // panel != displayedRosterPanel)
            {
                // switch the side panels.
                displayedRosterPanel = panel;
                displayedRosterPanel.Show();
                //displayedSidePanel.Show();
            }
            else
            {
                // refresh the side panel and status screen only if the expanded panel is not currently shown:

                // refresh the current one.
                if (displayedRosterPanel != null)
                    displayedRosterPanel.Refresh();

            }


            // refresh the side panel and status screen

            if (displayedRosterPanel != null && displayedRosterPanel.HasStatusCRT)
            {
                if (!StatusScreen.DisplayWindow.IsVisibleAndActive)
                {
                    StatusScreen.Show();
                }

                if (!StatusScreen.IsOn)
                {
                    StatusScreen.TurnOn();
                }
                else
                {
                    StatusScreen.Switch();
                }
            }
            else
            {
                HideStatusScreen();
            }


            if (displayedRosterPanel.AccessButton != null)
            {
                RosterAccessPanel.CheckAccessButton(displayedRosterPanel.AccessButton);

            }

        }


        public bool RosterIsDisplayed(RosterPanel roster)
        {
            return roster == displayedRosterPanel;
        }

        private void HideStatusScreen()
        {
            if (StatusScreen != null && StatusScreen.DisplayWindow.IsVisibleAndActive)
            {
                if (StatusScreen.IsOn)
                {
                    StatusScreen.TurnOff();
                }

                StatusScreen.Hide();
            }
        }


      /*  public void Change(RosterPanel sidePanel, bool refreshCurrentPanelWithNewContent)
        {
            if (!refreshCurrentPanelWithNewContent)
            {
                if (displayedSidePanel == sidePanel)
                    return;
            }

            bool isRefreshingExistingPanel = (refreshCurrentPanelWithNewContent && displayedSidePanel == sidePanel);


            if (sidePanel != displayedSidePanel && displayedSidePanel != null)
            {
                displayedSidePanel.Hide();
            }


            if (sidePanel != displayedSidePanel)
            {
                // switch the side panels.
                displayedSidePanel = sidePanel;
                displayedSidePanel.Show();
            }
            else
            {
                // refresh the side panel and status screen only if the expanded panel is not currently shown:
                
                // refresh the current one.
                if (displayedSidePanel != null)
                    displayedSidePanel.Refresh();
               
            }


            // refresh the side panel and status screen
          
            if (displayedSidePanel != null && displayedSidePanel.HasStatusCRT)
            {
                if (!StatusScreen.DisplayWindow.Visible)
                {
                    StatusScreen.DisplayWindow.Show();
                }

                if (!StatusScreen.IsOn)
                {
                    StatusScreen.TurnOn();
                }
                else
                {
                    StatusScreen.Switch();
                }
            }
            else
            {
                if (StatusScreen != null && StatusScreen.IsOn)
                {
                    StatusScreen.TurnOff();
                }
            }


        }*/

        //  public enum ExpandedGUIPanels { Build, Stock, Building, Entity, Tile };
        /*   public void ShowExpandedGUIPanel(GUIPanels panelType)
           {
               if (displayedExpandedPanel != null)
               {
                   displayedExpandedPanel.Hide();
                   //displayedExpandedPanel.Form.Hide();
               }

               switch (panelType)
               {
                   case GUIPanels.Stock:
                       displayedExpandedPanel = stocksPanel;
                       break;
                   case GUIPanels.Entity:
                       displayedExpandedPanel = entityPanel;
                       break;
               }

               if (displayedExpandedPanel != null)
               {
                   displayedExpandedPanel.Show();
                   //displayedExpandedPanel.Form.Show();
               }
           }*/

        public void StocksExpandedPanel_DrawContentEvent(Window sender, SpriteBatch formSpriteBatch)
        {

        }

        public bool IsShowingMapEditor()
        {
            return displayedRosterPanel == this.SidePanelEditorEntity;
        }

        public bool IsMouseInsideInterface()
        {
            /*   if (displayedExpandedPanel != null)
               {
                   return UWGame.SimSide.Instance.IsMouseInsideArea(new Rectangle(leftExpanded, intf.);
               }
               else return */

            return false;
        }


        /// <summary>
        /// HUDs are in world space
        /// </summary>
        public void MoveHUDWindows()
        {
            foreach (var window in hudWindows)
            {
                if (window is EntityListWindow )
                {

                }

                if (window.DisplayWindow.Visible && window.WorldPosition.HasValue)
                {
                    window.DisplayWindow.Position =
                        The.MapUI.WorldPosToScreenPoint(window.WorldPosition.Value);
                }
            }
        }

        /// <summary>
        /// the fog of war is a UI window...
        /// </summary>
        public void MoveFogMap()
        {
            if (FogMap != null)
            {
                FogMap.MoveToPosition(new Point(-(int)The.MapUI.MapWindowWorldPosition.X, -(int)The.MapUI.MapWindowWorldPosition.Y));
                //FogMap.Position = new Point(-(int)The.MapUI.MapWindowWorldPosition.X, -(int)The.MapUI.MapWindowWorldPosition.Y); 
            }
        }

        Regulator ownerRegulator = new Regulator(The.Client.ClientRandomGenerator, 2, "InGameInterfaceOwner");

        private void UpdateUISimPerspective()
        {
            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
               /* if (UIAllegiance == null)
                {
                    UIAllegiance = The.Sim.PlaySite.PlayerAllegiance;
                }*/

                if (UIOwner == null || ownerRegulator.IsReady())
                {

                    Vector2 centerOfScreen = The.MapUI.GetCenterOfScreenWorldLocation();

                    Expedition expedition = The.Map.GetClosestExpedition(centerOfScreen.ToVector3());

                    if (expedition != null)
                    {
                        UIOwner = expedition.OwnedEntities.ID;
                        UIExpedition = expedition.ID;
                    }
                    else
                    {
                        UIOwner = null;
                        UIExpedition = null;
                    }
                }
            }
        }

        public virtual void SetWeatherNow(string cloudCover, string wind)
        {
            dateAndWeather.HUDPanel.SetWeatherNow(cloudCover, wind);
        }


        public virtual void SetTimeAndDate(string timeOfDayString, /*int monthNo,*/ string season, string date) // double year) // int yearNo)
        {
            dateAndWeather.HUDPanel.SetTime(timeOfDayString);            
           // dateAndWeather.HUDPanel.SetMonth(monthNo);
            dateAndWeather.HUDPanel.SetSeason(season);
            dateAndWeather.HUDPanel.SetDate(date); // SetYear(year);
        }

        public override void Update(GameTime gameTime)
        {
            bool wasExiting = The.Client.IsExiting;
            base.Update(gameTime); // <- game over, The.Sim = null, causes client to crash???

            bool isExiting = The.Client.IsExiting;

            if (The.Sim == null)
            {
                return;               
            }

            // pull the sim data we need to show:
            UpdateUISimPerspective(); 

           
            string replayTimeText = The.Client.Controller.GetReplayTime();
            if (replayTimeText != null)
            {
                replayTime.Text = replayTimeText;
            }
            replayTime.Update(gameTime);

            if (FogMap != null)
                FogMap.Update(gameTime);

           
            //input.Update(gameTime);

            Selection.Update(gameTime);

            StatusScreen.Update(gameTime);

            if (TalkPanel != null)
            {
                TalkPanel.Update(gameTime);
            }

            if ((InventoryPanel != null && IsDisplayed(InventoryPanel))
                || (ContextMenu != null &&
                    (ContextMenu.ZoneGatherResourcesWindow.DisplayWindow.IsVisibleAndActive))
                || (HUDActionPanel != null && HUDActionPanel.UpgradeWindow.DisplayWindow.IsVisibleAndActive)
                || EntityTypeTooltipsStack.Count > 0)
            {
                InventorySettings.Update(gameTime);
            }

            Minimap.Update(gameTime);

            SelectedCyclePlayer.Update(gameTime);
            SelectedCyclePlayerFlashing.Update(gameTime);

            Vector2 extraBounds = new Vector2(screenBoundsExtension, screenBoundsExtension);
            SimSide.Collisions.CollideShape2D ExtendedScreenBounds = new SimSide.Collisions.CollideShape2D(
                The.MapUI.MapWindowWorldPosition - extraBounds,
                The.MapUI.MapWindowWorldPosition + new Vector2(The.MapUI.mapWindowWidth, The.MapUI.mapWindowHeight) + extraBounds);

            UpdateSpokenLines(ExtendedScreenBounds);
            UpdateMarkerWindows(gameTime, ExtendedScreenBounds);

          /*  if (MenuDialog.Window.IsVisibleAndActive)
            {
                MenuDialog.Update(gameTime);// save game process needs this
                // MessageBox.Update(gameTime);
            }*/

            if (SaveLoadMessageBox.Window.IsVisibleAndActive)
            {
                SaveLoadMessageBox.Update(gameTime);// save game process needs this               
            }

            if (!The.Sim.IsPaused)
            {        
                if (displayedRosterPanel != null)
                {
                    displayedRosterPanel.Update(gameTime);
                }
                              

                int noVisible = 0;
                foreach (var hudWindow in hudWindows)
                {
                    if (hudWindow.DisplayWindow.IsVisibleAndActive)
                    {
                        noVisible++;
                        hudWindow.UpdateContent(gameTime);
                    }
                }

                if (Game.Controller.GraphicsLevelSetting == Control.Controller.GraphicsLevel.High)
                {
                    // no longer needed?
                    DisplayPanelRenderer.Update(gameTime);
                }

                if (LogPanel != null)
                {
                    LogPanel.Update(gameTime);
                }
             
                if (RatingsPanel != null)
                {
                    RatingsPanel.Update(gameTime);
                }
            }           

            if (CounterPanel != null) // money can change while paused
            {
                CounterPanel.Update(gameTime);
            }

            HUDWindow window;
            for (int i = hudWindows.Count - 1; i >= 0; i--) // collection may be modified while iterating!
            {
                window = hudWindows[i];
                if (window.DisplayWindow.IsVisibleAndActive || window.UpdateWhileHidden)
                {
                    window.Update(gameTime);
                }
            }


            if ((InterfaceMode == InterfaceState.Build || InterfaceMode == InterfaceState.EditorPlaceEntity))
            {

                //UWGame.SimSide.Instance.EntityBeingPlaced.Structure.SetPosition(map.MouseMapPosition, Common.Direction.North);

                UpdateEntitiesBeingPlacedPositions();
            }
        }

        public void UpdateEntitiesBeingPlacedPositions()
        {
            foreach (EntityPosition ep in EntitiesBeingPlaced)
            {
                Vector3 newPosition = The.MapUI.MouseWorldLocation;
                newPosition += ep.Position.ToVector3();

                newPosition = The.Map.ClampWorldPosition(newPosition);

                if (ep.Entity.EntityType.StructureType != null)
                {

                    ep.Entity.SetPosition(newPosition);

                    bool placementIsValid;

                    placementIsValid = ep.Entity.Structure.IsPlacementValid(newPosition);

                    if (!placementIsValid)
                    {
                        // tint with red...                           
                        ep.Entity.Renderable.SetOverlayGradientColors(Color.DarkRed, Color.OrangeRed, Color.White);
                    }
                    else
                    {
                        ep.Entity.Renderable.SetOverlayGradientColors(Color.DeepSkyBlue, Color.Blue, Color.White); // Color.White);
                    }
                }
                else
                {
                    ep.Entity.SetPosition(newPosition);
                }
            }
        }


        /// <summary>
        /// Currently not used for anything. Will draw a text on a set position with a set color.
        /// It currently uses the InterfaceFont
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <param name="color"></param>
        public void DrawText(string text, Vector2 pos, Color color)
        {
            The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            The.Client.spriteBatch.DrawString(The.InGameUI.InterfaceFont, text, pos, color);
            The.Client.spriteBatch.End();

        }

        public void DrawPauseIcon(bool isPaused)
        {            
            if (isPaused && The.Sim.Mode != Sim.EngineMode.Edit)
            {
                //As the pause icon gets created using position information of the MainPanel I placed the rendering call there.
                //Should be easy to move if needed. 
                MainPanel.DrawPauseIcon();
            }
        }






        public void ShowSelectedEntityPanel(bool refreshCurrentPanel = true)
        {
            ChangeRosterPanel(SidePanelEntity, refreshCurrentPanel);
        }

        public void ShowSelectedMapAreaPanel(bool refreshCurrentPanel = true)
        {
            ChangeRosterPanel(SidePanelMapArea, refreshCurrentPanel);
        }


    }
}
