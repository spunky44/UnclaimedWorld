using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.Control;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Interface.Controls;
using UWGame.SimSide.Processes;
using UWGame.ClientSide.Interface.Inventory;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    /// <summary>
    /// base class for windows appearing over the playfield
    /// 
    /// I am now using it on the main menu screens as well. Client and Sim are both null in this case...
    /// </summary>
    public abstract class HUDWindow
    {
        /// <summary>
        /// there is an ampty area with glow outside the edge in the window sprite "HUD_window_base"
        /// </summary>
        protected const int windowEdgeX = 3;
        protected const int windowEdgeY = 3;

        protected const int sideMargin = 6;
        protected const int bottomMargin = 6;
        protected const int topMargin = 6;

        protected const int correctedSideMargin = sideMargin + windowEdgeX;
        protected const int correctedTopMargin = topMargin + windowEdgeY;
        protected const int correctedBottomMargin = bottomMargin + windowEdgeY;


        protected const int singleSpacing = 6;
        protected const int doubleSpacing = 12;
        protected const int tripleSpacing = 18;


        public Window DisplayWindow;

        private Regulator updateRegulator;

        /// <summary>
        /// set this value if the HUD should stay with a point on the map when scrolling
        /// </summary>
        public Vector2? WorldPosition;

        // dimensions of the entire window:         
        //protected int windowHeight; // = 80; // 400 screenHeight + 33;
        //  protected int windowWidth; // = 200; //screenWidth + 130;

        protected GUIManager gui;

        public CommonInterface Interface;

        //  protected Game game;

        public bool UpdateWhileHidden = false;

        public bool HideOnRightClick = true;

        protected ImageButton btClose;

        private string surface;//the sprite file name for our surface

        public const int buttonWidth = 48;


        protected int CloseButtonYPos
        {
            set
            {
                if (btClose != null)
                {
                    btClose.Y = value;
                }
            }
        }

        public HUDWindow(int width, int height, bool hasSurface = true, bool hasCloseButton = false, bool isMovable = false, string spriteName = "HUD_window_base", bool hideWhenMouseExits = false, Level level = Level.RockBottom, CommonInterface intf = null)
            : this(intf ?? The.InGameUI, width, height, hasSurface, hasCloseButton, isMovable, spriteName, hideWhenMouseExits, level)
        {

        }

        public HUDWindow(CommonInterface intf, int width, int height, bool hasSurface = true, bool hasCloseButton = false, bool isMovable = false, string spriteName = "HUD_window_base", bool hideWhenMouseExits = false, Level level = Level.RockBottom)
        {
            this.gui = intf.gui;
            this.Interface = intf;

            // windowWidth = width;
            //   windowHeight = height;
            //  game = The.Sim.ScreenManager.Game;

            updateRegulator = new Regulator(intf.Game.Controller.RandomGenerator, 2, "HUDWindow");

            DisplayWindow = new Window(intf.gui);

            // Sequence matters for skins!!!
            // DisplayWindow.Skin = gui.GUISpriteSheet.SourceRectangle("HUD_window_base");
            if (hasSurface)
            {
                ChangeSurface(spriteName); //, Color.Blue, Color.Red); // can be tinted

                DisplayWindow.CornerSize = 14; // 47;
            }

            DisplayWindow.Opacity = 0.83f; // 0.75f;

            DisplayWindow.Margin = 0; // 7;
            DisplayWindow.Level = level; // Level.RockBottom;
            DisplayWindow.Resizable = false;
            DisplayWindow.IsMovable = isMovable; // true; // true; // 
            //   DisplayWindow.Position = new Point(0, The.Client.GraphicsDevice.Viewport.Height - height);
            DisplayWindow.WindowSize = new Vector2(width, height);  //new Vector2(screenDimensions.Width + 90, screenDimensions.Height + 40);
            DisplayWindow.HasCloseButton = false; // true; // false;
            DisplayWindow.HasCRTOrLCDComponents = false; // true;
            DisplayWindow.HasOverlayComponents = false; // true;
            //   DisplayWindow.IsLCDSurface = true;

            /*   DisplayWindow.SetResizableArea(ResizeAreas.TopLeft, false);
               DisplayWindow.SetResizableArea(ResizeAreas.Left, false);
               DisplayWindow.SetResizableArea(ResizeAreas.Bottom, false);
               DisplayWindow.SetResizableArea(ResizeAreas.BottomLeft, false);
               DisplayWindow.SetResizableArea(ResizeAreas.BottomRight, false);
               */

            DisplayWindow.Resize += new ResizeHandler(DisplayWindow_Resize);

           /* if (isMovable)
            {
                DisplayWindow.IsMovable
            }
            else
            {
                DisplayWindow.ViewPort.Click += DisplayWindow_Click;
            }*/

            DisplayWindow.Hide();

            if (hasCloseButton)
            {
                AddCloseButton();
            }

            if (The.InGameUI != null)
            {
                The.InGameUI.AddHudWindow(this);
            }

            if (hideWhenMouseExits)
            {
                DisplayWindow.ViewPort.MouseOut += new MouseOutHandler(ViewPort_MouseOut);
            }
        }

        void DisplayWindow_Click(UIComponent sender, EventArgs e)
        {
            DisplayWindow.BringToTop();
        }

        public void ChangeSurface(string surfaceSpriteName)
        {
            if (surfaceSpriteName != surface)
            {
                surface = surfaceSpriteName;
                DisplayWindow.SetSkin(gui.GUISpriteSheet.GetSourceRectangle(surfaceSpriteName));
            }
        }



        void DisplayWindow_Resize(UIComponent sender)
        {
            if (btClose != null)
            {
                SetCloseButtonPosition();
            }
        }

        private void SetCloseButtonPosition()
        {           
            btClose.X = DisplayWindow.Width - sideMargin - btClose.Width;
        }


        protected int TitleBarHeight
        {
            get
            {
                if (btClose != null)
                {
                    return btClose.Bottom;
                }
                else return 0;
            }
        }

        void ViewPort_MouseOut(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {

            Hide();

        }

        protected void RightJustify(Label lbl, int sideMarginToUse)
        {
            lbl.FitToText();

            lbl.X = DisplayWindow.Width - lbl.TextWidth - sideMarginToUse;

        }

        protected void RightJustify(UIComponent lbl, int sideMarginToUse)
        {
            lbl.X = DisplayWindow.Width - lbl.Width - sideMarginToUse;
        }

        public virtual void Hide()
        {
            DisplayWindow.Hide();
        }

        protected void AddCloseButton()
        {
            btClose = new ImageButton(gui);
            Add(btClose);

            btClose.Init(ImageButtonType.HUDClose);
            //salvage.Text = "X";
            btClose.Y = topMargin;
            SetCloseButtonPosition();
            //btClose.Position = new Point(DisplayWindow.Width - sideMargin - btClose.Width, topMargin);
            btClose.ToolTip = "Close";
            btClose.Click += new ClickHandler(btClose_Click);

            btClose.DebugTag = "hudClose";
            //   btClose.MouseOver += new MouseOverHandler(btClose_MouseOver);

        }

        /*  void btClose_MouseOver(InputEventSystem.MouseEventArgs args)
          {
              throw new NotImplementedException();
          }*/

        void btClose_Click(UIComponent sender, EventArgs e)
        {
            Hide();
            //DisplayWindow.Hide();
        }


        protected void SetDisplayName(string zoneName, Label lblName, Label lblHeader, Image icon)
        {
            lblName.Text = zoneName;
            lblName.FitToText();

            if (string.IsNullOrEmpty(zoneName))
            {
                icon.X = lblName.X;
            }
            else
            {
                icon.X = lblName.Right + singleSpacing;
            }

            lblHeader.X = icon.Right + singleSpacing;

        }

        protected void AddZoneNameAndHeader(string zoneName, string header, string icon, int margin, out Label lblName, out Label lblHeader, out Image headerIcon)
        {
            lblName = new Label(gui);
            Add(lblName);
            lblName.Init(Label.LabelType.HUDWindowHeader);
            lblName.Text = zoneName;
            lblName.FitToText();
            lblName.X = margin; //(int)(0.5f * (DisplayWindow.Width -lblHeader.Width - 2 * sideMargin));
            lblName.Y = margin;

            lblHeader = new Label(gui);
            Add(lblHeader);
            lblHeader.Init(Label.LabelType.HUDWindow);
            lblHeader.Text = header; // "GATHER";
            lblHeader.FitToText();
            lblHeader.X = lblName.Right + doubleSpacing; //(int)(0.5f * (DisplayWindow.Width -lblHeader.Width - 2 * sideMargin));
            lblHeader.Y = margin;


            headerIcon = new Image(gui);
            Add(headerIcon);
            headerIcon.Texture = gui.GUISpriteSheet.Texture;
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle(icon);
            headerIcon.SetSkinLocation(SkinState.Normal,rect);
            headerIcon.ResizeControlToFitImage();
            headerIcon.Y = margin + 4;

        }

        /*  protected void AddHeading(int margin, string icon, string heading)
          {
              GUIManager gui = The.InGameUI.gui;

              Label lblZone = new Label(gui);
              Add(lblZone);
              lblZone.Init(Label.LabelType.HUDWindowHeader);
              lblZone.X = margin;
              lblZone.Y = margin;        
          }*/

        /// <summary>
        /// only adds the control if it is not already in the collection.
        /// </summary>
        /// <param name="control"></param>
        protected void Add(UIComponent control)
        {
            DisplayWindow.Add(control);
        }

        /// <summary>
        /// removes the control if it is present in the collection
        /// </summary>
        /// <param name="control"></param>
        protected void Remove(UIComponent control)
        {
            DisplayWindow.Remove(control);
        }



        /// <summary>
        /// use this to move all child windows too
        /// </summary>
        public void SetScreenPosition(Point newPos)
        {
            DisplayWindow.Position = newPos;

        }

        public Point GetScreenPosition()
        {
            return DisplayWindow.Position;
        }

        public void SetWorldPosition(Point newPos)
        {
            Vector2 worldPos = The.MapUI.ScreenToWorldPos(newPos.X, newPos.Y);

            // clamp the window pos to stay inside the map:
            Vector2 worldPosOfBottomRight = worldPos + DisplayWindow.WindowSize;
            worldPosOfBottomRight = The.Map.ClampWorldPosition(worldPosOfBottomRight);
            worldPos = worldPosOfBottomRight - DisplayWindow.WindowSize;

            //  newPos = The.MapUI.WorldPosToScreen(worldPos).ToPoint();

            WorldPosition = worldPos;
        }

        public virtual void ShowInScreenSpace(int screenPosX, int screenPosY, bool modal = false) //, bool excludeInterfaceArea = true) // Vector3 worldLocation, int mouseX, int mouseY)
        {
            if (modal == true)
            {
                DisplayWindow.ShowModal();
            }
            else
            {
                DisplayWindow.Show();
            }

            SetScreenPosition(new Point(screenPosX, screenPosY));

        }

        /// <summary>
        /// shows the HUD on the playfield and gives it a world position to stay with.
        /// </summary>
        /// <param name="screenPosX"></param>
        /// <param name="screenPosY"></param>
        public virtual void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = true) //, bool excludeInterfaceArea = true) // Vector3 worldLocation, int mouseX, int mouseY)
        {
            DisplayWindow.Show();


            SetScreenPosition(new Point(screenPosX, screenPosY));
            // SetWorldPosition(new Point(screenPosX, screenPosY));

            // adjust the window position to stay inside the viewable area                
            PlaceWindowInsideViewableArea(avoidRightInterfaceArea);

            // anchor the window at the adjusted position:
            SetWorldPosition(GetScreenPosition()); // new Point(screenPosX, screenPosY));
        }

        protected void PlaceWindowInsideViewableArea(bool avoidRightInterfaceArea)
        {
            int maxWidth = The.Sim.Controller.DrawArea.Width; // .Game.GraphicsDeviceManager.PreferredBackBufferWidth;
            if (avoidRightInterfaceArea)
            {
                maxWidth -= InGameInterface.InterfaceWidth;
            }

            if (DisplayWindow.Right > maxWidth)
            {
                DisplayWindow.X = maxWidth - DisplayWindow.Width;
            }

            int maxHeight = The.Sim.Controller.DrawArea.Height - InGameInterface.BottomAreaExcludedFromHUD; // .Game.GraphicsDeviceManager.PreferredBackBufferHeight - InGameInterface.BottomAreaExcludedFromHUD;

            if (DisplayWindow.Bottom > maxHeight)
            {
                DisplayWindow.Y = maxHeight - DisplayWindow.Height;
            }
        }

        public static Grid CreateOuterGridForCollapsableLists(GUIManager gui, UIComponent surface, bool addToSurface = true)
        {
           
            Grid outerGrid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow); 
            outerGrid.FixedItemHeights = false;
            outerGrid.RenderType = RenderType.Normal; // RenderType.CRTAndLCD;
            if (addToSurface)
            {
                surface.Add(outerGrid);
            }
            outerGrid.HMargin = 0; 
            outerGrid.VMargin = 0; 
            outerGrid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            outerGrid.Width = surface.Width;
            outerGrid.Height = surface.Height;
            outerGrid.ItemHeight = 26;
            outerGrid.Position = new Point(0, 0);

            return outerGrid;
        }


        public static void AddCollapsablePanelAndGrid(GUIManager gui, Grid outerGrid, string title, int? itemHeight, out CollapsablePanel panel, out Grid grid)
        {
            if (!itemHeight.HasValue)
            {
                itemHeight = 18;
            }

            panel = new CollapsablePanel(gui, CollapsablePanel.PanelType.HUD);  //CollapsablePanel.PanelType.DropDown);
            panel.CollapsedHeight = outerGrid.ItemHeight;
            // outerGrid.AddEntry(title, panel);
            outerGrid.AddEntry(panel, panel); // NEW: use panel as key.
            panel.Init(); //CollapsablePanel.PanelType.Node);  //CollapsablePanel.PanelType.DropDown);
            panel.Title = title;
            panel.Width = outerGrid.Width;

            grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow); //ListBoxType.Main);
            grid.FixedItemHeights = true; // false;
            grid.Width = panel.Width; // make grid fill the collapsable panel
            panel.AddContent(grid); // .ExpandedPanel.Add(categoryGrid);
            grid.ScrollBarEnabled = false;
            grid.ItemHeight = itemHeight.Value;
            grid.CanGrowInHeight = true;
            grid.Font = GUIManager.LCDandHUDBodyFontPath;
            grid.Selectability = Grid.SelectabilityOptions.Single;

        }


        /// <summary>
        /// This is used for those grids that contain inner grids (such as for item categories)
        /// There are some slight differences - FixedItemHeights is false so the parent can contract and expand and other things...
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="outerGrid"></param>
        /// <param name="title"></param>
        /// <param name="innerPanelType"></param>
        /// <param name="panel"></param>
        /// <param name="grid"></param>
        public static void AddCollapsablePanelAndTreeGrid(GUIManager gui, Grid outerGrid, string title, CollapsablePanel.PanelType innerPanelType, out CollapsablePanel panel, out Grid grid)
        {
            panel = new CollapsablePanel(gui, CollapsablePanel.PanelType.HUD);  //, CollapsablePanel.PanelType.DropDown);
            panel.CollapsedHeight = outerGrid.ItemHeight;
            // outerGrid.AddEntry(title, panel);
            outerGrid.AddEntry(panel, panel); // NEW: use panel as key.
            panel.Init(); //CollapsablePanel.PanelType.DropDown);
            panel.Title = title;
            panel.Width = outerGrid.Width;

            grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow); // ListBoxType.Main);
            grid.FixedItemHeights = false;
            grid.RenderType = RenderType.Normal; // RenderType.CRTAndLCD;
            grid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            //grid.Width = lcdSurface.Width;
            int gridTopMargin = 0; // 10; // 30;
            //grid.Height = lcdSurface.Height - gridTopMargin;
            grid.Width = panel.Width; // make grid fill the collapsable panel
            panel.AddContent(grid);
            grid.ItemHeight = (innerPanelType == CollapsablePanel.PanelType.DropDownBig ? 26 : 22); //22; 
            grid.Position = new Point(0, gridTopMargin);
            grid.CanGrowInHeight = true; // ??

            //grid.HeightResize += new ResizeHandler(grdSkills_HeightResize);

        }

        /// <summary>
        /// creates a menu like grid for those menus available at the top and bottom
        /// </summary>
        /// <param name="grid"></param>
        protected void CreateMenuGrid(out Grid grid, int? yPos = null, bool hasScrollBar = true)
        {
            UIComponent listSurface = new UIComponent(gui);
            Add(listSurface);
            listSurface.Y = yPos.HasValue ? yPos.Value : doubleSpacing;
            listSurface.Width = DisplayWindow.ViewPort.Width - 2 * sideMargin;
            listSurface.Height = DisplayWindow.ViewPort.Height - listSurface.Y;

            grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow); // ListBoxType.Main);
            grid.FixedItemHeights = true;
            grid.RenderType = RenderType.Normal; // RenderType.CRTAndLCD;
            listSurface.Add(grid);
            grid.HMargin = 5; // !!!
            grid.VMargin = 5; // !!!
            grid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grid.Width = listSurface.Width;
            grid.Height = listSurface.Height;
            grid.ItemHeight = 26;
            grid.Position = new Point(0, 0);
            grid.ScrollBarEnabled = hasScrollBar;

        }

        const int captionX = 0;//42;

        const int buildX = 150;//120;//18;
        const int beginX = 100;

      /*  protected UIComponent CreateTooltipAndBuildButton(EntityType entityType, ProcessType processType, string actionLabel, EventArgs eventArgs, int menuWidth, out ImageButton btAction)
        {
            UIComponent item = new UIComponent(gui);

            Image icon;
            icon = new Image(gui);
            item.Add(icon);
            icon.Texture = gui.GUISpriteSheet.Texture;

            EntityType entityTypeToDisplayIconFor = null;

            if (processType != null)
            {
                if (processType.HasOutput)
                {
                    if (processType.Outputs[0].FinalEntityTypeToCreate != null)
                    {
                        entityTypeToDisplayIconFor = processType.Outputs[0].FinalEntityTypeToCreate;
                    }
                }
            }
            else
            {
                entityTypeToDisplayIconFor = entityType;
            }

            if (entityTypeToDisplayIconFor != null)
            {
                IconInfo iconInfo;
                Rectangle rect = entityTypeToDisplayIconFor.GetIconSprite(out iconInfo);
                icon.SetSkinLocation(SkinState.Normal,rect);
            }

            icon.ResizeControlToFitImage();
            icon.X = 2;

            btAction = new ImageButton(gui);
            btAction.Init(ImageButtonType.HUDBuild);
            btAction.ID = UIComponent.DataControlID.CurrentOrders;
            btAction.ToolTip = actionLabel;
            btAction.EventArgs = eventArgs;
            btAction.X = buildX + 2 * doubleSpacing;

            item.Add(btAction);


            DataTypeButton tbCaption;
            if (entityType != null)
            {
                tbCaption = new DataTypeButton(gui, DataTypeTooltip.InfoToShow.Production, entityType, null, true);
                tbCaption.Text = entityType.Name;
            }
            else
            {
                tbCaption = new DataTypeButton(gui, DataTypeTooltip.InfoToShow.Production, processType, null, true);
                tbCaption.Text = processType.Name;
            }

            tbCaption.Init(TextButton.TextButtonType.HUDToolTipWhite);
            tbCaption.ID = UIComponent.DataControlID.Caption;
            tbCaption.IsRoot = true;
            item.Add(tbCaption);
            tbCaption.TextAlignment = TextButton.TextAlign.Left;
            tbCaption.Width = menuWidth - btAction.Width - (menuWidth - btAction.Width - btAction.X) - (2 * doubleSpacing);
            tbCaption.X = captionX + 2 * doubleSpacing;
            tbCaption.SideToAnchorOn = InGameInterface.AnchorSide.Left;
            return item;
        }*/

       



        /// <summary>
        /// creates a grid that simulates a surface and viewport with scrollbar
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="surfaceHeight"></param>
        /// <param name="topMarginToUse"></param>
        /// <param name="sideMarginToUse"></param>
        /// <param name="canHaveFocus"></param>
        protected void CreateSurfaceWithScrollbar(out Grid grid, out UIComponent listSurface, int? surfaceHeight = null, int topMarginToUse = sideMargin, int sideMarginToUse = sideMargin, bool canHaveFocus = true)
        {
            listSurface = new UIComponent(gui);
            Add(listSurface);
            listSurface.X = sideMarginToUse;
            listSurface.Y = topMarginToUse;
            listSurface.Width = DisplayWindow.ViewPort.Width - 2 * sideMarginToUse;
            listSurface.Height = surfaceHeight.HasValue ? surfaceHeight.Value : DisplayWindow.ViewPort.Height - listSurface.Y;
            listSurface.CanHaveFocus = canHaveFocus;

            grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow); // ListBoxType.Main);
            grid.FixedItemHeights = false;
            grid.RenderType = RenderType.Normal; // RenderType.CRTAndLCD;
            listSurface.Add(grid);
            //grid.HMargin = 5; // !!!
            //grid.VMargin = 5; // !!!
            grid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grid.Width = listSurface.Width;
            grid.Height = listSurface.Height;
            //grid.ItemHeight = 26;
            grid.Position = new Point(0, 0);
            grid.CanHaveFocus = canHaveFocus;

        }

        /// <summary>
        /// only active panels get updated, and only when not paused - by Interface.
        /// </summary>
        /// <param name="elapsed"></param>
        public virtual void UpdateContent(GameTime elapsed)
        {
            if (updateRegulator.IsReady())
            {
                Refresh();
            }
        }


        /// <summary>
        /// used for timed expand, collapse etc. Also called when paused.
        /// </summary>
        /// <param name="elapsed"></param>
        public virtual void Update(GameTime elapsed)
        {

        }

        /// <summary>
        /// When this gets called, the window should refresh its contents
        /// </summary>
        public virtual void Refresh() { }
    }
}
