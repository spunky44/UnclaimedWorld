#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
////using Microsoft.Xna.Framework.Storage;
using System.Xml;
using WindowSystem;
using InputEventSystem;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide;
using UWGame.Control;
#endregion

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// a window panel that can refresh itself with a fixed interval
    /// only active panels get updated, and only when not paused - by Interface.
    /// </summary>
    public class Panel
    {
        public Window Window;
      
        private Regulator updateRegulator;

        protected Label lblTitle;

        public CommonInterface Interface; 
     
        /// <summary>
        /// use for distance from panel edge, including the black edge.
        /// </summary>
        public const int MarginX = 16; // 9;

        /// <summary>
        /// use for distance from panel edge, including the slope.
        /// TODO: split in MarinTop and MarginBottom
        /// </summary>
        public const int MarginY = 8;

        public int MarginTop = 8;

        public int MarginBottom = 8;


        public const int BottomMarginForButtons = 52;

        public const int BottomMarginWithoutButtons = 20;

        public static readonly Point RosterMargin = new Point(15 /*16*/, 48); 


        public const int SingleSpacing = 6;

        /// <summary>
        /// Use for distances betweeen elements inside the panel.
        /// </summary>
        public const int DoubleSpacing = 12;

        protected const int BottomButtonYDistance = 48;
        protected const int bottomButtonXMargin = 20;


        protected const int lcdSideMargin = 6;


        public enum PanelType { RosterPanel, IrregularEdges, RegularEdges, InfoPanel, MainMenu, EventDialog, Ratings, Counters, EventArchive }

        public PanelType panelType;

        public Panel(CommonInterface intf, string title, Point position, Vector2 dimension, Level level, PanelType panelType = PanelType.IrregularEdges)
        {
            this.Interface = intf;
            this.panelType = panelType;

            updateRegulator = new Regulator(intf.Game.Controller.RandomGenerator, 1, "Panel");   

            //displayRegulator = new Regulator(The.Client.ClientRandomGenerator, 2);


            Window = new Window(intf.gui);
            

            Window.Position = position;
            Window.WindowSize = dimension;
            Window.Level = level;

            Window.IsMovable = false;
            Window.Resizable = false;
            Window.Margin = 0;

            Window.HasCloseButton = false;

            string sprite;
            int cornerSize;

            /* // old title with yellow bg
            int titleX = 48;
            int titleY = 10;*/

            int titleX = 40;
            int titleY = 0;

            Label.LabelType titleType = Label.LabelType.RosterTitle;
            bool hasTitle = true;

            switch (panelType)
            {
                case PanelType.RosterPanel:
                    sprite = "rosterpanel_base";
                    cornerSize = RosterPanel.IrregularCornerSize;
                    MarginTop = 48;
                    MarginBottom = 12;
                    break;

                case PanelType.EventArchive:
                    sprite = "rosterpanel_eventarchive";
                    cornerSize = RosterPanel.IrregularCornerSize;
                    MarginTop = 48;
                    MarginBottom = 12;
                    break;

                case PanelType.InfoPanel:
                    sprite = "infopanel";
                    cornerSize = 116;
                    hasTitle = false;
                    break;

                case PanelType.RegularEdges:
                    sprite = "optionspanel_base";
                    cornerSize = 111;
                    MarginTop = 64;
                    MarginBottom = 19;

                    break;

                case PanelType.MainMenu:
                    sprite = "mainmenu_panel";
                    cornerSize = 88;
                    titleType = Label.LabelType.PlainPanelNormal;
                    titleX = 62;
                    titleY = 16;
                    break;

                case PanelType.EventDialog:
                    sprite = "eventpanel";
                    cornerSize = 1;
                    
                    titleY = -1;
                   // titleX = 40;

                    /* OLD
                    titleX = 62;*/

                    MarginTop = 50;
                    MarginBottom = 8;

                    break;

                case PanelType.Ratings:
                    sprite = "ratings_panel";
                    cornerSize = 68;
                    hasTitle = false;                   
                   // MarginTop = 50;
                  //  MarginBottom = 8;

                    break;

                case PanelType.Counters:
                    sprite = "counter_panel";
                    cornerSize = 1; // no scaling for now!
                    hasTitle = false;

                    break;

                case PanelType.IrregularEdges:
                default:
                    sprite = "rosterpanelUnattached_base";
                    cornerSize = RosterPanel.IrregularCornerSize;
                    MarginTop = 48;
                    MarginBottom = 12;

                    break;
            }

            Window.Skin = intf.gui.GUISpriteSheet.GetSourceRectangle(sprite);
            Window.CornerSize = cornerSize;

            Window.Destroyed += Window_Destroyed;

            if (hasTitle)
            {
                lblTitle = new Label(intf.gui);
                Window.Add(lblTitle);

                if (title != null)
                {
                    lblTitle.Text = title;
                }

                lblTitle.X = titleX;
                lblTitle.Y = titleY;
                lblTitle.Init(titleType);
            }

            Window.Hide();
            
            Window.DrawContentEvent += new Window.DrawContentDelegate(DrawContent);
        }

        void Window_Destroyed()
        {
            this.Destroy();
        }

        protected virtual void Destroy()
        {

        }

        public enum PanelOptions { None, SteelAndDust }
       /* public static Window CreatePanelWindow(GUIManager gui, Point position, Vector2 dimension, Level level, PanelType panelType) // string sprite = "basic_base_flat", int cornerSize = 1)
        {
           
            return form;
        }*/

       /* protected void AddBrownBlotAndButtons(out TextButton button1, string button1Text, string button1Tooltip, int bottomMargin = 60)
        {
            //  Box brown = AddBlotForButtons(bottomMargin);

            button1 = new TextButton(Interface.gui);
            Form.Add(button1); // add first!!! sets defaults!
            button1.Init(TextButton.TextButtonType.White);
            button1.Position = new Point(MarginX + 5, Form.Height - BottomMarginForButtons); // brown.Y);
            button1.Text = button1Text;
            button1.ScaleWidthToFitText();
            button1.ToolTip = button1Tooltip;
        }*/

        protected void AddBottomButtonInSequence(TextButton lastButton, out TextButton newButton, string button1Text, string button1Tooltip)
        {
            newButton = new TextButton(Interface.gui);
            Window.Add(newButton); // add first!!! sets defaults!
            newButton.Init(TextButton.TextButtonType.White);
            newButton.Text = button1Text;
            newButton.ScaleWidthToFitText();
            newButton.ToolTip = button1Tooltip;

            if (lastButton == null)
            {
                PlaceLeftButtonUnderLCD(newButton);
            }
            else
            {
                PlaceButtonUnderLCD(newButton, lastButton.Right + 5);                
            }          
        }
       
        protected void PlaceLeftButtonUnderLCD(UIComponent button)
        {
            button.X = bottomButtonXMargin;
            button.Y = Window.Height - BottomButtonYDistance;
        }

        protected void PlaceRightButtonUnderLCD(UIComponent button)
        {
            button.X = Window.Width - bottomButtonXMargin - button.Width;
            button.Y = Window.Height - BottomButtonYDistance;
        }

        protected void PlaceButtonUnderLCD(UIComponent button, int xPos = 20)
        {
            button.X = xPos;
            button.Y = Window.Height - BottomButtonYDistance;
        }

        /// <summary>
        /// call this after placing the LCDs, so the dirt will overlap
        /// </summary>
        protected void AddDefaultDirt()
        {
            switch (panelType)
            {
                case PanelType.RosterPanel:
                case PanelType.EventArchive:
                    AddDirtOnIrregularEdges(this.Interface.gui, Window);
                    //AddDirtOnStraightEdges();
                    break;

                case PanelType.IrregularEdges:
                    AddDirtOnIrregularEdges(this.Interface.gui, Window);
                    break;          
          
                case PanelType.RegularEdges:
                    AddDirtOnStraightEdges();
                    break;

                case PanelType.InfoPanel:
                    AddDirtOnInfoPanel();
                    break;

            }

        }

        /// <summary>
        /// must be drawn after LCD. Is drawn with overlay in order to stay on top of recessed part of lcd panel.
        /// </summary>
        public static void AddDirtOnIrregularEdges(GUIManager gui, Window Form, bool addTopEdgeDirt = true, bool addBottomEdgeDirt = true)
        {
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
            Image dirtLeft = AddImage(gui, Form, rect, new Point(0, 0));
            //dirtLeft = AddImage(gui, Form, rect, new Point(0, rect.Height));
            dirtLeft.RenderType = RenderType.Overlay;

            if (addBottomEdgeDirt)
            {
                rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
                Image dirtBottom = AddImage(gui, Form, rect, new Point(Form.Width - rect.Width, Form.Height - rect.Height));//new Point(0, Form.Height - rect.Height));
                dirtBottom.RenderType = RenderType.Overlay;
            }

            if (addTopEdgeDirt)
            {
                AddDirtOnIrregularTopEdge(gui, Form);
            }

            rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_right");
            Image dirtRight = AddImage(gui, Form, rect, new Point(Form.Width - rect.Width + 26, 30));
            dirtRight.RenderType = RenderType.Overlay;
        }

        public static void AddDirtOnIrregularTopEdge(GUIManager gui, Window Form)
        {
            // don't draw dirt on the rounded top right corner:
            int dirtX = 360;
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_top");
            if (dirtX + rect.Width < Form.Width - 14)
            {
                Image dirtTop = AddImage(gui, Form, rect, new Point(dirtX, 21));
                dirtTop.RenderType = RenderType.Overlay;
            }

        }

        protected void AddDirtOnStraightEdges(bool excludeBottomDirt = false) //int topLeftCornerToExcludeX = 0, int topLeftCornerToExcludeY = 0)
        {
            // avoid placing dirt over the title:
            int topLeftCornerToExcludeX = 130; // 30;
            int topLeftCornerToExcludeY = 43;

            AddDirtOnStraightEdges(Interface.gui, Window, topLeftCornerToExcludeX, topLeftCornerToExcludeY, excludeBottomDirt);
        }


        protected void AddDirtOnInfoPanel()
        {
            Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
            Image dirtLeft = AddImage(Interface.gui, Window, rect, new Point(0, 43));
            dirtLeft.RenderType = RenderType.Overlay;

            AddDirtOnBottomEdge(Interface.gui, Window, true);

         
           /* rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_top");
            Image dirtTop = AddImage(Interface.gui, Form, rect, new Point(0, 30));           
            dirtTop.RenderType = RenderType.Overlay;            
            */

          /*  rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_right");
            Image dirtRight = AddImage(Interface.gui, Form, rect, new Point(Form.Width - rect.Width + 26, 30));
            dirtRight.RenderType = RenderType.Overlay;
            */

            Window.HasOverlayComponents = true;


        }

        /// <summary>
        /// must be drawn after LCD. Is drawn with overlay in order to stay on top of recessed part of lcd panel.
        /// </summary>
        public static void AddDirtOnStraightEdges(GUIManager gui, Window Form, int topLeftCornerToExcludeX = 0, int topLeftCornerToExcludeY = 0, bool excludeBottomDirt = false)
        {
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
            Image dirtLeft = AddImage(gui, Form, rect, new Point(0, topLeftCornerToExcludeY));          
            dirtLeft.RenderType = RenderType.Overlay;

            if (excludeBottomDirt == false)
            {
                AddDirtOnBottomEdge(gui, Form, true);
            }

            AddDirtOnTopEdge(gui, Form, topLeftCornerToExcludeX, true);

            rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_right");
            Image dirtRight = AddImage(gui, Form, rect, new Point(Form.Width - rect.Width + 26, 0));
            dirtRight.RenderType = RenderType.Overlay;

            Form.HasOverlayComponents = true;
        }

        public static void AddDirtOnTopEdge(GUIManager gui, Window Form, int topLeftCornerToExcludeX, bool renderAsOverlay = true)
        {
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_top");
            Image dirtTop = AddImage(gui, Form, rect, new Point(topLeftCornerToExcludeX, -9));

            if (renderAsOverlay)
            {
                dirtTop.RenderType = RenderType.Overlay;
            }
            
        }


        public static Image AddDirtOnBottomEdge(GUIManager gui, Window Form, bool renderAsOverlay = true)
        {
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");          
            Image dirtBottom = AddImage(gui, Form, rect, new Point(0, Form.Height - rect.Height));

            if (renderAsOverlay)
            {
                dirtBottom.RenderType = RenderType.Overlay;
            }

            return dirtBottom;
        }

        public static void CreateGridWithColumnHeadingsWithFixedLength(UIComponent lcdSurface, int yPos, int itemHeight, out Grid grid, int bottomMargin = 0, params Tuple<string, int,int>[] columnHeadings)
        {
            CreateColumnHeadingsWithFixedLength(lcdSurface, yPos, columnHeadings);

            grid = FullLCDPanel.AddGridWithFixedItemHeights(lcdSurface.guiManager, lcdSurface, yPos + 20, bottomMargin);
            grid.ItemHeight = itemHeight;
            grid.Selectability = Grid.SelectabilityOptions.None;
        }



        public static void CreateGridWithColumnHeadings(UIComponent lcdSurface, int yPos, int itemHeight, out Grid grid, int bottomMargin = 0, params Tuple<string, int>[] columnHeadings)
        {
            CreateColumnHeadings(lcdSurface, yPos, columnHeadings);

            grid = FullLCDPanel.AddGridWithFixedItemHeights(lcdSurface.guiManager, lcdSurface, yPos + 20, bottomMargin);
            grid.ItemHeight = itemHeight;
            grid.Selectability = Grid.SelectabilityOptions.None;
        }

        /// <summary>
        /// adjusts the x position of an item within an lcd inner panel with respect to the header 
        /// like on Inventory, Missions, Personnel and Buy/Sell
        /// </summary>
        /// <param name="headerXPos"></param>
        /// <returns></returns>
        public static int GetItemColumnFromHeader(int headerXPos)
        {
            return headerXPos - 6;
        }

        protected static void CreateColumnHeadings(UIComponent lcdSurface, int yPos, params Tuple<string, int>[] columnHeadings)
        {
            foreach (var item in columnHeadings)
            {
                Label lblHeading = new Label(lcdSurface.guiManager);
                lblHeading.Init(Label.LabelType.LCDSmallHeadingBanner);
                lblHeading.Text = item.Item1; // "NAME";
                lcdSurface.Add(lblHeading);
                lblHeading.Y = yPos;
                lblHeading.X = item.Item2;
                lblHeading.FitToText();
            }
        }

        protected static void CreateColumnHeadingsWithFixedLength(UIComponent lcdSurface, int yPos, params Tuple<string, int,int >[] columnHeadings)
        {
            foreach (var item in columnHeadings)
            {
                Label lblHeading = new Label(lcdSurface.guiManager);
                lblHeading.Init(Label.LabelType.LCDSmallHeadingBanner);
                lblHeading.Text = item.Item1; // "NAME";
                lcdSurface.Add(lblHeading);
                lblHeading.Y = yPos;
                lblHeading.X = item.Item2;
                lblHeading.Width = item.Item3;
            }
        }

        /// <summary>
        /// use this to move all child windows too
        /// </summary>
        public void SetScreenPosition(Point newPos)
        {
            Window.Position = newPos;

        }

        protected enum Align { Left, Right }
        protected TextButton AddLowerButton(string text, string tooltip, Align align)
        {
            TextButton bt = new TextButton(Window.guiManager); 
            Window.Add(bt); // add first!!! sets defaults!
            bt.Init(TextButton.TextButtonType.White);
            bt.Text = text; 
            bt.ToolTip = tooltip; 
            bt.Height = 28;
            bt.ScaleWidthToFitText(); //Width = 90;

            if (align == Align.Left)
            {
                PlaceLeftButtonUnderLCD(bt);
                //bt.X = MarginX;
            }
            else if (align == Align.Right)
            {
                PlaceRightButtonUnderLCD(bt);
                //bt.X = Form.Width - bt.Width - 18;
            }
           
            return bt;
        }


        public static TextButton AddTextButton(GUIManager gui, Window window, Point position, TextButton.TextButtonType type, string text, string tooltip)
        {
            TextButton btMoveDown = new TextButton(gui); //Interface.Instance.gui);
            window.Add(btMoveDown); // add first!!! sets defaults!
            btMoveDown.Init(type);
            btMoveDown.Position = position;
            btMoveDown.Text = text;
            btMoveDown.ToolTip = tooltip;

            return btMoveDown;

        }

        protected ActionButtonPanel CreateActionButtons(int width)
        {
            ActionButtonPanel actionButtons = new ActionButtonPanel(Interface.gui);
            Window.Add(actionButtons);
            actionButtons.Init();

            actionButtons.Width = width; //GetContentWidth();
            actionButtons.X = MarginX;
            actionButtons.Y = Window.Height - actionButtons.Height - MarginY; //Form.Height - 60;

            return actionButtons;
        }
              

       
        public static Box AddIndentation(GUIManager gui, Window window, Point position, int width, int height)
        {
            Box indent = new Box(gui);
            window.Add(indent);
            Rectangle r = gui.GUISpriteSheet.GetSourceRectangle("indent");
            indent.SetSkinLocation(SkinState.Normal,r);
            indent.CornerSize = 9;
            indent.Width = width;
            indent.Height = height;
            indent.Position = position;

            return indent;

        }

        public static Box AddMetalPlate(GUIManager gui, Window window, Point position, Point dimension)
        {
            Box metal = new Box(gui);
            window.Add(metal);
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("metal_plate");
            metal.SetSkinLocation(SkinState.Normal,rect);
            metal.CornerSize = 4;
            metal.Position = position; // new Point(MarginX, controlTop);
            metal.Width = dimension.X; // 198;
            metal.Height = dimension.Y; // 85;

            return metal;
        }
                

        public virtual void Hide()
        {
            if (Window.guiManager.GetModal() == Window)
            {
                if (The.Client != null) // in gmae only
                {
                    The.Client.SetModal(false);     
                }
            }


            Window.Hide();            

        }

        public virtual void ShowInScreenSpace(int screenPosX, int screenPosY, bool modal = false) 
        {
            if (modal == true)
            {
                Window.ShowModal();
            }
            else
            {
                Window.Show();
            }
                        
            // NEW: let's clamp to screen edges:           
            
            screenPosX = Common.ClampTop(screenPosX, Interface.gui.ScreenWidth - Window.Width);
            screenPosY = Common.ClampTop(screenPosY, Interface.gui.ScreenHeight - Window.Height);

            SetScreenPosition(new Point(screenPosX, screenPosY));

            // NEW:
            Refresh();
        }

        public virtual void Show()
        {            
            Window.Show();
                       
            // make sure the data is up-to-date!
            Refresh();
        }


      

        /// <summary>
        /// shows the panel centered on the screen as a dialog.
        /// </summary>
        public virtual void ShowDialog(bool modal)
        {
            int left = (this.Interface.gui.ScreenWidth - Window.Width) / 2;
            int top = (this.Interface.gui.ScreenHeight - Window.Height) / 2;

            Window.Position = new Point(left, top);

           // Form.Position = new Point(The.MapUI.mapWindowWidth / 2 - Form.Width / 2, The.MapUI.mapWindowHeight / 2 - Form.Height / 2);

            Window.Show();

            if (modal)
            {
                Interface.gui.SetModal(Window);

                if (The.Client != null)
                {
                    The.Client.SetModal(true); // new
                }
            }
        }

        /// <summary>
        /// creates a grid that simulates a surface and viewport with scrollbar
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="surfaceHeight"></param>
        /// <param name="topMarginToUse"></param>
        /// <param name="sideMarginToUse"></param>
        /// <param name="canHaveFocus"></param>
        protected void CreateSurfaceWithScrollbar(out Grid grid, UIComponent surface, bool canHaveFocus = true, int topMargin = 0, int bottomMargin = 0)
        {
          
            grid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal); 
            grid.FixedItemHeights = false;
            grid.RenderType = RenderType.Normal; // RenderType.CRTAndLCD;
            surface.Add(grid);
            grid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grid.Width = surface.Width;
            grid.Height = surface.Height - topMargin - bottomMargin;
            grid.Position = new Point(0, topMargin);
            grid.CanHaveFocus = canHaveFocus;

        }


        public static void CreateTextArea(CommonInterface intf, ref TextArea area, Grid surfaceGrid)
        {
            area = new TextArea(intf.gui, ListBoxType.LCD);
            area.RenderType = RenderType.CRTAndLCD;
            area.Init(Label.LabelType.LCDNormal);
            area.CanGrowInHeight = true;
            surfaceGrid.AddEntry(area, area);
            area.X = lcdSideMargin; // sideMargin;
            area.Y = 45;
            area.Width = surfaceGrid.SurfaceWidth - 2 * lcdSideMargin; // lcdSurface.Width - 2 * lcdSideMargin; // sideMargin;       
        }

        public int GetBottom()
        {
            return Window.Height - MarginY;
        }

        public int GetContentWidth()
        {
            return Window.Width - 2 * MarginX;
        }

        protected void InitSmallPanel()
        {
          /*  Form.Position = new Vector2(intf.MainLeft, Interface.SmallPanelTop);
            Form.WindowSize = new Vector2(Interface.InterfaceWidth, intf.infoPanelHeight);
            Form.Level = Level.Bottom; // Top; // Bottom; 
           * */
        }

     /*   protected virtual void Initialize(Window window)
        {
            this.Form = window;
            Form.HasTitleBar = false;
            Form.IsMovable = false;
            Form.Resizable = false;
            Form.TitleBarHeight = 0;
            Form.Margin = 0;

            Form.Skin = gui.GUISpriteSheet.SourceRectangle("detailpanel_base"); //"event_base_small2");
            Form.CornerSize = 15;

            dust = new Image(intf.gui);
            rect = gui.GUISpriteSheet.SourceRectangle("event_dust");
            dust.SetSkinLocation(SkinState.Normal,rect);
            dust.Alpha = 0.04f; // 0.07f;
            Form.Add(dust);
            dust.Position = new Vector2(0f, 0f);
            dust.ResizeToFit();

           // window.HasCloseButton = false;

          //  Interface.Instance.gui.Add(Form);
            
            Form.Hide();

            Form.DrawContentEvent += new Window.DrawContentDelegate(DrawContent);

        }*/

        public static Image AddImage(GUIManager gui, Window window, string spriteName, Point position)
        {
            Image image;
            Rectangle rect;
            image = new Image(gui);
            rect = gui.GUISpriteSheet.GetSourceRectangle(spriteName);
            image.SetSkinLocation(SkinState.Normal,rect);
            window.Add(image);
            image.Position = position;
            image.ResizeControlToFitImage();
            image.CanHaveFocus = false; // don't destroy buttons!
            return image;
        }

        public static Image AddImage(GUIManager gui, UIComponent addToUIComponent, string spriteName, Point position)
        {
            Image image;
            Rectangle rect;
            image = new Image(gui);
            rect = gui.GUISpriteSheet.GetSourceRectangle(spriteName);
            image.SetSkinLocation(SkinState.Normal,rect);
            addToUIComponent.Add(image);
            image.Position = position;
            image.ResizeControlToFitImage();
            image.CanHaveFocus = false; // don't destroy buttons!
            return image;
        }

        public static Image AddImage(GUIManager gui, Window window, Rectangle rect, Point position)
        {
            Image image;            
            image = new Image(gui);            
            image.SetSkinLocation(SkinState.Normal,rect);
            window.Add(image);
            image.Position = position;
            image.ResizeControlToFitImage();
            image.CanHaveFocus = false; // don't destroy buttons!
            return image;
        }

        public static Image AddDust(GUIManager gui, Window window)
        {
            
            Image dust = new Image(gui);
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
            dust.SetSkinLocation(SkinState.Normal,rect);
            dust.Alpha = 0.04f; 
            window.Add(dust);
            dust.Position = new Point(0, 0);
            dust.ResizeControlToFitImage();
            dust.CanHaveFocus = false; // don't destroy buttons!

            return dust;
        }

        public static Image AddDust(GUIManager gui, Window window, Rectangle rect, Point position)
        {
            Image dust = new Image(gui);            
            dust.SetSkinLocation(SkinState.Normal,rect);
            dust.Alpha = 0.04f; // 0.07f;
            window.Add(dust);
            dust.Position = position;
            dust.ResizeControlToFitImage();
            dust.CanHaveFocus = false; // don't destroy buttons!
            return dust;
        }

        public static void AddDustOnFrame(GUIManager gui, Rectangle frame, int frameEdgeWidth, Window window, RenderType renderType)
        {
            //int frameEdgeWidth = 37;
          //  Game game = UWGame.SimSide.Instance.ScreenManager.Game;
           // GUIManager gui = Interface.Instance.gui;

            // top edge
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
            rect.Width = frame.Width;
            rect.Height = frameEdgeWidth;
            Image dust = Panel.AddDust(gui, window, rect, new Point(frame.X, frame.Y));
            dust.RenderType = renderType;

            // bottom edge
            rect = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
            rect.Y = rect.Y + frame.Height - frameEdgeWidth;
            rect.Width = frame.Width;
            rect.Height = frameEdgeWidth;
            dust = Panel.AddDust(gui, window, rect, new Point(frame.X, frame.Y + frame.Height - frameEdgeWidth));
            dust.RenderType = renderType;

            // left edge
            rect = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
            rect.Y = rect.Y + frameEdgeWidth;
            rect.Width = frameEdgeWidth;
            rect.Height = frame.Height - 2 * frameEdgeWidth;
            dust = Panel.AddDust(gui, window, rect, new Point(frame.X, frame.Y + frameEdgeWidth));
            dust.RenderType = renderType;

            // right edge
            rect = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
            rect.X = rect.X + frame.Width - frameEdgeWidth;
            rect.Y = rect.Y + frameEdgeWidth;
            rect.Width = frameEdgeWidth;
            rect.Height = frame.Height - 2 * frameEdgeWidth;
            dust = Panel.AddDust(gui, window, rect, new Point(frame.X + frame.Width - frameEdgeWidth, frame.Y + frameEdgeWidth));
            dust.RenderType = renderType;

        }

        public static void AddSteelTexture(GUIManager gui, Window window)
        {            

            int x = 0;
            int y = 0;

            Image texture;
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("main_panel_texture");
            while (x < window.Width)
            {
                y = 0;
                while (y < window.Height)
                {    

                    texture = new Image(gui);
                    texture.DebugTag = "main_panel_texture";
                    texture.SetSkinLocation(SkinState.Normal,rect);
                   // texture.Alpha = 0.07f;
                    window.Add(texture);
                    texture.Position = new Point(x, y);
                    texture.ResizeControlToFitImage();
                    texture.CanHaveFocus = false; // don't destroy buttons!

                    y += rect.Height;
                }

                x += rect.Width;
            }


        }

        public virtual void DrawContent(Window sender, SpriteBatch formSpriteBatch) { }

       
      //  public virtual void Update(GameTime elapsed) { }

        /// <summary>
        /// only active panels get updated, and only when not paused - by Interface.
        /// </summary>
        /// <param name="elapsed"></param>
        public virtual void Update(GameTime elapsed)
        {
            if (The.Sim != null) // only used in-game...
            {
                if (updateRegulator.IsReady())
                {
                    Refresh();
                }
            }
        }


        /// <summary>
        /// refresh the panel info
        /// </summary>
        public virtual void Refresh() { }

    /*    public List<Button> Buttons = new List<Button>();
        public UWGame.SimSide game;

        public void AddButton(Rectangle rect, string text, UWGame.SimSide.ButtonClick del)
        {
            Button b = new Button(rect, text, game, del);
            Buttons.Add(b);
            
        }

        public void Draw()
        {
            foreach(Button b in Buttons){
                b.Draw();
            }
        }*/
    }
}
