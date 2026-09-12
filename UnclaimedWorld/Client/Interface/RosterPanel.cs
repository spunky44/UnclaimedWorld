using System;
using System.Collections.Generic;
using System.Text;
using WindowSystem;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Storage;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide.Interface.Controls;
namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// these panels are accessed from the roster access menu
    /// </summary>
    public class RosterPanel : Panel
    {

        protected InGameInterface intface = The.InGameUI;

        public bool HasStatusCRT = false;

        public ICanBeChecked MainControlButton;


        #region CRT screen content

        protected const int statusTextX = 8;
        protected const int statusTextWidth = 200;

        protected UIComponent statusContent;

        protected Bar crtUnderline;
        protected Label lblStatusHeading;

        protected Image imStatusBackground;

        protected UIComponent pnBillboards;
        protected Vector2 statusBillboardPanelCenter;

        #endregion

        protected const int hyperLinkMargin = 6;
        public const int SlimGridItemHeight = 18;

        public const int ItemHeight = 20; // 22;



        protected Box display;
        protected LCDScreen lcdScreen;
        protected UIComponent lcdSurface;


        protected bool hasBeenDrawn = false;

        // Image dirtRight;


        /// <summary>
        /// optional, controls pressed state
        /// </summary>
        public ImageButton AccessButton;

        Icon tintedBackground;

        protected const int columnHeaderHeight = 20;

        protected const int headingsY = 0; // 30;

        //  protected float topMargin = 20;

        protected int titleBottom = 0;

        const int sidePanelInterfaceWidth = 278; // 274; //263; // 274; 

        const int sidePanelAccessPanelOverlap = 13; // 11; // 9;

        // event archive: 17
        const int eventArchiveAccessPanelOverlap = 17;

        // roster: 19
        const int rosterPanelAccessPanelOverlap = 19; // 26;

        public const int IrregularCornerSize = 242;


        const Level level = Level.BelowBelowMiddle;

        private ModalOverlay modalOverlay;

        protected Color? BackgroundTint
        {
           /* get
            {
                if (tintedBackground.Visible)
                {
                    return tintedBackground.Color;
                }
                else return null;
            }*/
            set
            {
                SetBackgroundTint(value, tintedBackground);                
            }
        }


        public RosterPanel(string topTitle, int width, bool needBottomMarginForButtons)
            : this(topTitle,
                width, The.InGameUI.rosterPanelHeight, needBottomMarginForButtons, panelType: PanelType.RosterPanel)
        {
            modalOverlay = new ModalOverlay(Window);


        }


        /// <summary>
        /// old SidePanel ctor
        /// </summary>
        /// <param name="isInfoPanel"></param>
        public RosterPanel(int height, bool isInfoPanel, int bottomMargin = 28)
            : base(The.InGameUI, "",
                        new Point(The.MapUI.mapWindowWidth - sidePanelInterfaceWidth - RosterAccessPanel.Width + sidePanelAccessPanelOverlap, InGameInterface.SmallPanelTop), // position
                        new Vector2(sidePanelInterfaceWidth, height), // dimension
                        level,
                        PanelType.InfoPanel)
        {
            modalOverlay = new ModalOverlay(Window);

            Window.HasCloseButton = false;

            CreateInfoLCD(bottomMargin);


            HasStatusCRT = true;


            AddDirtOnSidePanel();

        }



      
        /// <summary>
        /// roster panel
        /// </summary>
        /// <param name="topTitle"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="yPos"></param>
        /// <param name="panelType"></param>
        public RosterPanel(string topTitle,
                int width, int height, bool needBottomMarginForButton, int yPos = InGameInterface.rosterPanelTop, PanelType panelType = PanelType.RosterPanel)
            : base(The.InGameUI, topTitle,
                new Point(The.MapUI.mapWindowWidth - width - RosterAccessPanel.Width + rosterPanelAccessPanelOverlap, // + 5,     // position
                          yPos),
                new Vector2(width, height),
                level /*Level.Middle*/,
                panelType) // PanelType.RosterPanel)
        {
            Window.HasCloseButton = false;
            modalOverlay = new ModalOverlay(Window);

            if (needBottomMarginForButton)
            {
                CreateRosterStyleLCDPanel(The.InGameUI, Window, out display, out lcdSurface, ref lcdScreen, rightMargin: 10);
            }
            else
            {
                CreateRosterStyleLCDPanel(The.InGameUI, Window, out display, out lcdSurface, ref lcdScreen, BottomMarginWithoutButtons, rightMargin: 10);
            }

            AddTintedBackground(ref tintedBackground, display);


        }

        /// <summary>
        /// event archive roster only
        /// </summary>
        /// <param name="topTitle"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="yPos"></param>
        /// <param name="panelType"></param>
        public RosterPanel(string topTitle, int width, int height) //, int yPos = InGameInterface.rosterPanelTop)
            : base(The.InGameUI, topTitle,
                new Point(The.MapUI.mapWindowWidth - width - RosterAccessPanel.Width + eventArchiveAccessPanelOverlap, // position
                          201 /*InGameInterface.SmallPanelTop*/),
                new Vector2(width, height),
                level /*Level.Middle*/,
                PanelType.EventArchive)
        {
            Window.HasCloseButton = false;
            modalOverlay = new ModalOverlay(Window);

            CreateRosterStyleLCDPanel(The.InGameUI, Window, out display, out lcdSurface, ref lcdScreen, rightMargin: 7, addTopEdgeDirt: false);
        }


        /// <summary>
        /// necessary, because "display" includes edges which should not be tinted. 
        /// </summary>
        public static void AddTintedBackground(ref Icon tintedBackground, Box display)
        {
            tintedBackground = new Icon(display.guiManager);
            tintedBackground.SetSkinLocation(SkinState.Normal,display.guiManager.GUISpriteSheet.GetSourceRectangle("whiteSquare")); //, color, color);
            tintedBackground.ScaleImageToSizeOfControl = true;
            tintedBackground.X = 8;
            tintedBackground.Y = 8;
            tintedBackground.Width = display.Width - 16;
            tintedBackground.Height = display.Height - 16;
            tintedBackground.Visible = false;
            display.Add(tintedBackground);
        }

        public static void SetBackgroundTint(Color? color, Icon tintedBackground)
        {
            if (color.HasValue)
            {
                tintedBackground.Visible = true;
                tintedBackground.Color = color;
            }
            else
            {
                tintedBackground.Visible = false;
            }
        }

        /// <summary>
        /// creates an LCD screen that fills the whole panel.
        /// not all panels need an LCD screen 
        /// </summary>
        /// <param name="intf"></param>
        /// <param name="form"></param>
        /// <param name="display"></param>
        /// <param name="surface"></param>
        /// <param name="lcdScreen"></param>
        /// <param name="bottomMargin"></param>
        public static void CreateRosterStyleLCDPanel(CommonInterface intf, Window form, out Box display, out UIComponent surface, ref LCDScreen lcdScreen,
            int bottomMargin = BottomMarginForButtons, int? rightMargin = null,
            bool addTopEdgeDirt = true,
            bool addBottomEdgeDirt = true)
        {

            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, form, bottomMargin,
                RosterMargin, out display, out surface, ref lcdScreen, rightMargin: rightMargin);

            AddWatermark(intf.gui, surface);

            // called now to overlay on LCD...
            AddDirtOnIrregularEdges(intf.gui, form, addTopEdgeDirt, addBottomEdgeDirt);

        }

        public void ShowModalOverlay()
        {
            modalOverlay.Show(Window, lcdSurface, display);
        }

        public void RemoveModalOverlay()
        {
            modalOverlay.Remove(Window, lcdSurface, display);
        }

        protected void CreateInfoLCD(int bottomMargin = 28)
        {
            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(The.InGameUI,
                Window, bottomMargin, new Point(15, 33),
                out display, out lcdSurface, ref lcdScreen, 260); // 258);

        }

        protected void AddDirtOnSidePanel() //Box display)
        {
            GUIManager gui = The.InGameUI.gui;

            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("Fingerprint");
            Image fingerprint = AddImage(Interface.gui, Window, rect, new Point(display.Right - rect.Width + 8, display.Y));
            fingerprint.RenderType = RenderType.Overlay;
            // fingerprint.DebugTag = "FindFinger";
            fingerprint.ResizeControlToFitImage();
            fingerprint.Alpha = 0.5f; // 0.35f; // 0.7f;

            AddDefaultDirt();

            rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_smallsplotch");
            Image smallSplotch = AddImage(Interface.gui, Window, rect, new Point(Window.Width - rect.Width, Window.Height - rect.Height));
            smallSplotch.RenderType = RenderType.Overlay;

            // overlay dirt:
            Image bigSplotch = AddImage(Interface.gui, Window, "basic_dirt_bigsplotch", new Point(228, display.Bottom - 180));
            bigSplotch.RenderType = RenderType.Overlay;
        }

        public override void Hide()
        {
            if (AccessButton != null)
            {
                AccessButton.IsChecked = false;
            }

            if (HasStatusCRT)
            {
                // The.InGameUI.StatusScreen.DisplayWindow.Hide(); //??
            }

            base.Hide();
        }

        protected void SetHeaderText(string text)
        {
            lblStatusHeading.Text = text;
            crtUnderline.Width = lblStatusHeading.Width;
        }

        /// <summary>
        /// creates an image control that will scale all sprites to its size
        /// </summary>
        /// <param name="fixedWidth"></param>
        /// <param name="fixedHeight"></param>
        protected void InitStatusImage(int fixedWidth, int fixedHeight)
        {
            InitStatusImage();
            imStatusBackground.Width = fixedWidth;
            imStatusBackground.Height = fixedHeight;
            imStatusBackground.ScaleImageToSizeOfControl = true;
        }

        protected void InitStatusImage()
        {
            imStatusBackground = new Image(The.InGameUI.gui);
            statusContent.Add(imStatusBackground);
            imStatusBackground.Position = new Point(0, 2);
            imStatusBackground.ResizeControlToFitImage();
            imStatusBackground.RenderType = RenderType.CRTAndLCD;
            imStatusBackground.Alpha = 0.7f; // 0.35f;// use for background for text!

        }

        protected void ShowBillboardPanel()
        {
            // important! add on top of noise background, but behind text labels!
            statusContent.Insert(pnBillboards, 1);
        }

        /// <summary>
        /// For sidepanels... We include a small margin at top and sides
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="lcdSurface"></param>
        /// <returns></returns>
        public static Grid CreateOuterGridForCollapsableLists(GUIManager gui, UIComponent lcdSurface, int bottomMargin = 0)
        {
            int gridTopMargin = 4;

            Grid outerGrid = new Grid(gui, ListBoxType.LCD /* ListBoxType.Main*/, Label.LabelType.CRTBigGlow);
            outerGrid.FixedItemHeights = false;
            outerGrid.RenderType = RenderType.CRTAndLCD;
            lcdSurface.Add(outerGrid);
            outerGrid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            outerGrid.Width = lcdSurface.Width;
            outerGrid.Height = lcdSurface.Height - gridTopMargin - bottomMargin;
            outerGrid.ItemHeight = 26;//22; 
            outerGrid.Position = new Point(0, gridTopMargin);

            return outerGrid;
        }

        public static void CreateAndPlaceBillboards(GUIManager gui, UIComponent pnBillboards, EntityType entityType, Vector2 imageCenter,
            float? targetImageHeight, bool doScaling)
        {
            List<Image> imageControls = new List<Image>();

            float approximateHeight;
            /*  if (entityType.TileLayoutType != null)
              {   // if we have a tile layout, use that:
                  approximateHeight = (float)entityType.TileLayoutType.HeightInTiles * MapManager.tileSize;
              }
              else
              {*/
            // otherwise use the 1st billboard:
            if (entityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName != null)
            {
                approximateHeight = GameData.Instance.BillboardSpriteSheet.GetSourceRectangle(entityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName).Height;
            }
            else
            {
                // trees can be selected in collision edit mode... but they don't have asset name set. disregard:
                pnBillboards.Controls.Clear();

                return;
            }
            // }

            pnBillboards.Controls.Clear();

            // don't scale if we are within some margin of the desired height...
            bool scaleThis = doScaling && Math.Abs(approximateHeight - targetImageHeight.Value) > 18;
            float scaleFactor = Common.ClampTop(targetImageHeight.Value / approximateHeight, 1.5f);

            foreach (RenderAsBillboardType renderAsBillboardType in entityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType)
            {
                Vector2 baseCenterWorld; // = new Vector2(90, 90); //= Parent.Location;
                Vector2 baseCenterOffset;

                Rectangle rect = GameData.Instance.BillboardSpriteSheet.GetSourceRectangle(renderAsBillboardType.AssetName);

                if (renderAsBillboardType.BaseCenter == Vector2.Zero)
                {   // if no base center has been entered, we use the center of the sprite:
                    baseCenterOffset = new Vector2(rect.Width / 2f, rect.Height / 2f);
                }
                else
                {
                    baseCenterOffset = renderAsBillboardType.BaseCenter;
                }

                //  baseCenterOffset.X = Common.FlipOffset(baseCenterOffset.X, rect.Width, flip);


                // flip the offset
                Vector2 offsetFromParent = renderAsBillboardType.Offset;
                /*   if (flip && offsetFromParent != Vector2.Zero) // we cant flip an Image...?
                   {
                       offsetFromParent.X = -offsetFromParent.X;
                   }*/

                //baseCenterWorld = imageCenter - baseCenterOffset + offsetFromParent;

                // create an image:
                Image imStatusBackground = new Image(gui);
                pnBillboards.Add(imStatusBackground);
                // imStatusBackground.Position = new Point((int)baseCenterWorld.X, (int)baseCenterWorld.Y); // new Point(0, 2);
                imStatusBackground.ResizeControlToFitImage();
                imStatusBackground.RenderType = RenderType.CRTAndLCD;
                imStatusBackground.Alpha = 0.7f;

                imStatusBackground.SetSkinLocation(SkinState.Normal,rect);
                imStatusBackground.Texture = GameData.Instance.BillboardSpriteSheet.Texture;

                if (scaleThis)
                {

                    imStatusBackground.Height = (int)(scaleFactor * rect.Height); //imageHeight; // 100;
                    imStatusBackground.Width = (int)(scaleFactor * rect.Width);
                    imStatusBackground.ScaleImageToSizeOfControl = true;

                    baseCenterWorld = imageCenter + scaleFactor * (-baseCenterOffset + offsetFromParent);

                    imStatusBackground.Position = new Point((int)baseCenterWorld.X, (int)baseCenterWorld.Y); // new Point((int)(imStatusBackground.Position.X * factor), (int)(imStatusBackground.Position.Y * factor));

                }
                else
                {
                    baseCenterWorld = imageCenter - baseCenterOffset + offsetFromParent;

                    imStatusBackground.Position = new Point((int)baseCenterWorld.X, (int)baseCenterWorld.Y); // new Point(0, 2);

                    imStatusBackground.ScaleImageToSizeOfControl = false;
                    imStatusBackground.ResizeControlToFitImage();
                }
                // right adjust the image:
                // imStatusBackground.X = statusContent.Width - imStatusBackground.Width;

            }


            // sort by baseCenterWorld.Y:


        }

        protected static bool AddPanelIfNotPresent(bool addPanel, Grid outerGrid, CollapsablePanel cp)
        {
            if (addPanel)
            {
                if (!outerGrid.EntriesByKey.ContainsKey(cp))
                {
                    outerGrid.AddEntry(cp, cp);
                }
            }
            else
            {
                outerGrid.TryRemoveEntry(cp);
            }

            return addPanel;
        }

        /*  protected Box AddBlotForButtons(int bottomMargin = 60)
          {
              Rectangle rect;
              // brown blot:
              Box brown = new Box(Interface.gui);
              Form.Add(brown);
              rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_darkblue"); //"basic_brown");
              brown.SetSkinLocation(SkinState.Normal,rect);
              brown.CornerSize = 3;
              brown.Position = new Point(MarginX, Form.Height - bottomMargin); //new Vector2(40f, 40f);//
              brown.Width = GetContentWidth();
              brown.Height = 40; //40; // 

              Image scratch1 = AddImage(Interface.gui, Form, "basic_scratch1", new Point(brown.X - 4, brown.Y - 9));
              Image scratch2 = AddImage(Interface.gui, Form, "basic_scratch2", new Point(brown.X - 10, brown.Y + 22));

              return brown;
          }*/

        protected void InitBillboardPanel()
        {
            pnBillboards = new UIComponent(The.InGameUI.gui);
            pnBillboards.Width = statusContent.Width;
            pnBillboards.Height = statusContent.Height;

            statusBillboardPanelCenter = new Vector2(statusContent.Width * 2f / 3f, statusContent.Height / 2f);
        }

        protected static void InitStatusCRTHeader(GUIManager gui, UIComponent content, int leftMargin, out Label lblStatusHeading, out Bar underline)
        {
            lblStatusHeading = new Label(gui);
            content.Add(lblStatusHeading);
            lblStatusHeading.Position = new Point(statusTextX, 14);
            lblStatusHeading.Init(Label.LabelType.CRTNormal); // Label.LabelType.CRTGlow);
            lblStatusHeading.Width = statusTextWidth;
            lblStatusHeading.Height = 24;
            // lblStatusHeading.DebugTag = "CRTStatusHeader";

            underline = new Bar(gui);
            content.Add(underline);
            underline.Position = new Point(leftMargin, 36);
            underline.EdgeSize = 6;
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("CRT_LayoutLine");
            underline.SetSkinLocation(SkinState.Normal,rect);
            underline.Width = 290; // crtContent.Width - 2 * leftMargin;
            underline.Height = rect.Height;
            underline.RenderType = RenderType.CRTAndLCD;
            underline.DebugTag = "underline";

        }

        /*
          protected void AddTitle(UIComponent lcdSurface, string title)
          {
              Label lblHeader = new Label(Interface.gui) { Text = title, Position = new Point(0, 0) };           
              lcdSurface.Add(lblHeader);
              lblHeader.Init(Label.LabelType.LCDBigHeaderBanner);
              lblHeader.Width = lcdSurface.Width;

              titleBottom = lblHeader.Bottom;
          }*/

        private static void AddWatermark(GUIManager gui, UIComponent lcdSurface, int y = 58)
        {
            Image image = new Image(gui);
            image.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle("Watermark_UWLogo"));
            image.ResizeControlToFitImage();
            //image.Alpha = 0.7f; // make it a bit more subdued
            lcdSurface.Add(image);
            image.Y = y;
            lcdSurface.CenterChildHorizontally(image);
        }

        public static void AddCollapsablePanelAndGrid(GUIManager gui, Grid outerGrid, string title, int? itemHeight, out CollapsablePanel panel, out Grid grid, 
            Grid.SelectabilityOptions selectability = Grid.SelectabilityOptions.Single, int sortOrder = 0)
        {
            if (!itemHeight.HasValue)
            {
                itemHeight = SlimGridItemHeight;
            }

            panel = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownSmall);
            panel.CollapsedHeight = outerGrid.ItemHeight;
            panel.Init();
            outerGrid.AddEntry(panel, panel); // NEW: use panel as key.    
            panel.Title = title;
            panel.OrderByTag1 = sortOrder;

            // panel.Width = outerGrid.Width;

            grid = new Grid(gui, ListBoxType.LCD /* ListBoxType.Main*/, Label.LabelType.LCDNormal);
            grid.FixedItemHeights = true; // false;
            grid.Width = panel.Width; // make grid fill the collapsable panel
            panel.AddContent(grid); // .ExpandedPanel.Add(categoryGrid);
            grid.ScrollBarEnabled = false;
            grid.ItemHeight = itemHeight.Value;
            grid.CanGrowInHeight = true;
            grid.Font = GUIManager.LCDandHUDBodyFontPath;
            grid.Selectability = selectability;
            grid.IsOuterGrid = false;
            grid.CanReceiveMouseWheelEvents = false;

        }

        public static void AddCollapsablePanelAndGridNotFixed(GUIManager gui, Grid outerGrid, string title, out CollapsablePanel panel, out Grid grid,
            Grid.SelectabilityOptions selectability = Grid.SelectabilityOptions.Single)
        {

            panel = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownSmall);
            panel.CollapsedHeight = outerGrid.ItemHeight;
            panel.Init();
            outerGrid.AddEntry(panel, panel); // NEW: use panel as key.          
            panel.Title = title;
            // panel.Width = outerGrid.Width;

            grid = new Grid(gui, ListBoxType.LCD /* ListBoxType.Main*/, Label.LabelType.LCDNormal);
            grid.FixedItemHeights = false;
            grid.Width = panel.Width; // make grid fill the collapsable panel
            panel.AddContent(grid); // .ExpandedPanel.Add(categoryGrid);
            grid.ScrollBarEnabled = false;
            grid.CanGrowInHeight = true;
            grid.Font = GUIManager.LCDandHUDBodyFontPath;
            grid.Selectability = selectability;
            grid.IsOuterGrid = false;
            grid.CanReceiveMouseWheelEvents = false;

        }

        /*
        public static TextButton AddCloseButton(Window Form, Action<object, EventArgs> closeFunction)
        {
            TextButton btClose = new TextButton(Form.guiManager); //intf.gui);
            Form.Add(btClose); // add first!!! sets defaults!
            btClose.Init(TextButton.TextButtonType.White);
            btClose.Position = new Point(Form.Width - 36, 6);
            btClose.Text = "X";
            btClose.Width = 32;
            btClose.Height = 32;
            btClose.ToolTip = "Close the panel.";
            btClose.Click += new ClickHandler(closeFunction); 
            return btClose;
        }*/

        protected TextButton AddCloseButton(Window Form, Action<object, EventArgs> closeFunction)
        {
            TextButton btClose = AddLowerButton("CLOSE", "Closes the panel", Align.Right); // new TextButton(Form.guiManager); //intf.gui);
            /* Form.Add(btClose); // add first!!! sets defaults!
             btClose.Init(TextButton.TextButtonType.White);           
             btClose.Text = "CLOSE";
             btClose.Width = 90;
             btClose.Height = 28;
             btClose.Position = new Point(Form.Width - btClose.Width - 18, Form.Height - btClose.Height - 4);*/
            btClose.Click += new ClickHandler(closeFunction);
            return btClose;
        }

        /* public static void AddDirtOnLeftEdge(GUIManager gui, Window Form)
         {
             Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
            // Image dirtLeft = AddImage(gui, Form, rect, new Point(0, 0));
             Image dirtLeft = AddImage(gui, Form, rect, new Point(0, rect.Height));
             dirtLeft.RenderType = RenderType.Overlay;
         }*/

        /*  protected override void Initialize(Window form)
         {
             base.Initialize(form);
             Form.HasCloseButton = false; // we use our own...
             Form.Level = Level.Middle;

             TextButton btClose = new TextButton(intf.gui);
             Form.Add(btClose); // add first!!! sets defaults!
             btClose.Init(TextButton.TextButtonType.Brown);
             btClose.Position = new Vector2(Form.Width - 36, 6);
             btClose.Text = "X";
             btClose.Width = 32;
             btClose.Height = 32;
             btClose.ToolTip = "[Esc] Closes the panel.";
             btClose.Click += new ClickHandler(Close_OnPress);

       
             Form.Close += new CloseHandler(Form_Close);
         }*/

        /* void Form_Close(UIComponent sender)
         {
             if (intf.displayedExpandedPanel != null)
             {
                // intf.displayedExpandedPanel.Form.Hide();
                 intf.displayedExpandedPanel = null;
             }  
         } */

        protected virtual void OnCancel()
        {

        }

        public override void Show()
        {
            base.Show();

            if (statusContent != null)
            {
                The.InGameUI.StatusScreen.ChangeContent(statusContent);
            }
        }
    }
}
