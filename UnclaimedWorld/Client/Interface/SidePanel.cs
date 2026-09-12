using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI;
 /*
namespace UWGame.ClientSide.Interface 
{
    /// <summary>
    /// merge with RosterPanel
    /// </summary>
   public class SidePanel: Panel
    {
        Image dirtRight;

      
        public bool HasStatusCRT = true;


        #region CRT screen content

        protected UIComponent statusContent;

        protected Bar crtUnderline;
        protected Label lblStatusHeading;

        protected Image imStatusBackground;

        protected UIComponent pnBillboards;
        protected Vector2 statusBillboardPanelCenter;

        #endregion


        protected Box display; 
        protected LCDScreen lcdScreen;

        protected UIComponent lcdSurface;



        public ICanBeChecked MainControlButton;

        protected InGameInterface intface;


        protected const int statusTextX = 8;
        protected const int statusTextWidth = 200;

        protected const int bottomMargin = 14; //160;

       
        protected const int hyperLinkMargin = 6;
       
        /// <summary>
        /// override...
        /// </summary>
        public new int MarginX = 15;

      //  public new int SidePanelMarginX = 16;
        public new int MarginY = 33;

        public const int ItemHeight = 20; // 22;

        public SidePanel(int height)
            : base(The.InGameUI, null,
            new Point(The.MapUI.mapWindowWidth - InGameInterface.SidePanelInterfaceWidth - 52, InGameInterface.SmallPanelTop), // position
            new Vector2(InGameInterface.SidePanelInterfaceWidth, height), // dimension
            Level.Bottom, PanelType.InfoPanel)
        {
            //AddDirtOnEdges();
            intface = The.InGameUI;            

        }

        protected void InitStatusImage()
        {
            imStatusBackground = new Image(intface.gui);
            statusContent.Add(imStatusBackground);
            imStatusBackground.Position = new Point(0, 2);
            imStatusBackground.ResizeControlToFitImage();
            imStatusBackground.RenderType = RenderType.CRTAndLCD;
            imStatusBackground.Alpha = 0.7f; // 0.35f;// use for background for text!

            
        }

        protected void InitBillboardPanel()
        {
            pnBillboards = new UIComponent(intface.gui);
            pnBillboards.Width = statusContent.Width;
            pnBillboards.Height = statusContent.Height;

            statusBillboardPanelCenter = new Vector2(statusContent.Width * 2f / 3f, statusContent.Height / 2f);
        }

        

        protected void SetHeaderText(string text)
        {
            lblStatusHeading.Text = text;
            crtUnderline.Width = lblStatusHeading.Width;
        }

        protected void ShowBillboardPanel()
        {
            // important! add on top of noise background, but behind text labels!
            statusContent.Insert(pnBillboards, 1);
        }

        /// <summary>
        /// must be drawn after LCD. Is drawn with overlay in order to stay on top of recessed part of lcd panel.
        /// </summary>
        protected void AddDirtOnEdges()
        {
            Image dirtLeft = AddImage(Interface.gui, Form, "basic_dirt_left", new Point(-20, 0));
            dirtLeft.RenderType = RenderType.Overlay;

            dirtLeft.DebugTag = "basic_dirt_left";

            Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
            AddImage(Interface.gui, Form, rect, new Point(0, Form.Height - rect.Height));

            Image dirtTop = AddImage(Interface.gui, Form, "basic_dirt_top", new Point(0, -9));
            dirtTop.RenderType = RenderType.Overlay;

            rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_right");
            dirtRight = AddImage(Interface.gui, Form, rect, new Point(Form.Width - rect.Width + 20, 0));
            dirtRight.RenderType = RenderType.Overlay;
        }

        protected void AddLCD()
        {
            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(The.InGameUI, 
                Form, 28, new Point(MarginX, MarginY), 
                out display,  out lcdSurface, ref lcdScreen, 258);

            AddOverlayDetails( display);
        }


        protected Box AddBlotForButtons(int bottomMargin = 60)
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
        }

        protected Box AddBrownBlotAndButtons(out TextButton button1, string button1Text, string button1Tooltip, int bottomMargin = 60)
        {           
            // brown blot:
            Box brown = AddBlotForButtons(bottomMargin);
           

            button1 = new TextButton(Interface.gui);
            Form.Add(button1); // add first!!! sets defaults!
            button1.Init(TextButton.TextButtonType.White);
            button1.Position = new Point(MarginX + 5, brown.Y);
            button1.Text = button1Text;
            button1.ScaleWidthToFitText();
            button1.ToolTip = button1Tooltip;

            return brown;

        }

        protected void AddButton(Box brown, TextButton lastButton, out TextButton newButton, string button1Text, string button1Tooltip)
        {
            newButton = new TextButton(Interface.gui);
            Form.Add(newButton); // add first!!! sets defaults!
            newButton.Init(TextButton.TextButtonType.White);
            newButton.Position = new Point(lastButton.X + lastButton.Width + 5, brown.Y);
            newButton.Text = button1Text;
            newButton.ScaleWidthToFitText();
            newButton.ToolTip = button1Tooltip;
        }

       
       
        /// <summary>
        /// For sidepanels... We include a small margin at top and sides
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="lcdSurface"></param>
        /// <returns></returns>
        public static Grid CreateOuterGridForCollapsableLists(GUIManager gui, UIComponent lcdSurface)
        {
            int gridTopMargin = 4;

            Grid outerGrid = new Grid(gui, ListBoxType.LCD , Label.LabelType.CRTBigGlow);
            outerGrid.FixedItemHeights = false;
            outerGrid.RenderType = RenderType.CRTAndLCD;
            lcdSurface.Add(outerGrid);
           // outerGrid.HMargin = 5; // !!!
          //  outerGrid.VMargin = 5; // !!!
            outerGrid.Font = GUIManager.LCDandHUDFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            outerGrid.Width = lcdSurface.Width;            
            outerGrid.Height = lcdSurface.Height - gridTopMargin;
            outerGrid.ItemHeight = 26;//22; 
            outerGrid.Position = new Point(0, gridTopMargin);

            return outerGrid;
        }

        public static void DisplayScaledBillboardImage(UIComponent statusContent, Image imStatusBackground, EntityType entityType, int imageHeight, bool doScaling)
        {
            // TODO: Make more image controls to display more than one billboard?
            Rectangle rect = GameData.Instance.BillboardSpriteSheet.GetSourceRectangle(
                entityType.RenderableType.Default.RenderAsBillboardType[0].AssetName);

            float factor = (float)rect.Width / (float)rect.Height;

            imStatusBackground.SetSkinLocation(SkinState.Normal,rect);
            imStatusBackground.Texture = GameData.Instance.BillboardSpriteSheet.Texture;

            if (doScaling || Math.Abs(rect.Height - imageHeight) > 18) // don't scale if we are within some margin of the desired height...
            {
                imStatusBackground.Height = imageHeight; // 100;
                imStatusBackground.Width = (int)(factor * imStatusBackground.Height);
                imStatusBackground.ScaleImageToSizeOfControl = true;
            }
            else
            {
                imStatusBackground.ScaleImageToSizeOfControl = false;
                imStatusBackground.ResizeControlToFitImage();
            }
            // right adjust the image:
            imStatusBackground.X = statusContent.Width - imStatusBackground.Width;
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

        public static void CreateAndPlaceBillboards(GUIManager gui, UIComponent pnBillboards, EntityType entityType, Vector2 imageCenter, 
             float? targetImageHeight, bool doScaling)
        {
            List<Image> imageControls = new List<Image>();

            float approximateHeight;
            if (entityType.TileLayoutType != null)
            {   // if we have a tile layout, use that:
                approximateHeight = (float)entityType.TileLayoutType.HeightInTiles * MapManager.tileSize;
            }
            else
            {
                // otherwise use the 1st billboard:
                if (entityType.RenderableType.Default.RenderAsBillboardType[0].AssetName != null)
                {
                    approximateHeight = GameData.Instance.BillboardSpriteSheet.GetSourceRectangle(entityType.RenderableType.Default.RenderAsBillboardType[0].AssetName).Height;
                }
                else
                {
                    // trees can be selected in collision edit mode... but they don't have asset name set. disregard:
                    pnBillboards.Controls.Clear();

                    return;
                }
            }

            pnBillboards.Controls.Clear();

            // don't scale if we are within some margin of the desired height...
            bool scaleThis = doScaling && Math.Abs(approximateHeight - targetImageHeight.Value) > 18;
            float scaleFactor = Common.ClampTop(targetImageHeight.Value / approximateHeight, 1.5f);

            foreach (RenderAsBillboardType renderAsBillboardType in entityType.RenderableType.Default.RenderAsBillboardType)
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

        protected void AddOverlayDetails( Box display)
        {
            
            GUIManager gui = The.InGameUI.gui;

            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("Fingerprint");
            Image fingerprint = AddImage(Interface.gui, Form, rect, new Point(display.Right - rect.Width + 8, display.Y));
            fingerprint.RenderType = RenderType.Overlay;
            // fingerprint.DebugTag = "FindFinger";
            fingerprint.ResizeControlToFitImage();
            fingerprint.Alpha = 0.35f; // 0.7f;

            // add dirt now!
            AddDirtOnEdges();


            rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_smallsplotch");
            Image smallSplotch = AddImage(Interface.gui, Form, rect, new Point(Form.Width - rect.Width, Form.Height - rect.Height));


            // overlay dirt:
            Image bigSplotch = AddImage(Interface.gui, Form, "basic_dirt_bigsplotch", new Point(228, display.Bottom - 180));
            bigSplotch.RenderType = RenderType.Overlay;

        }

        public const int SlimGridItemHeight = 18;

        public static void AddCollapsablePanelAndGrid(GUIManager gui, Grid outerGrid, string title, int? itemHeight, out CollapsablePanel panel, out Grid grid, Grid.SelectabilityOptions selectability = Grid.SelectabilityOptions.Single)
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
           // panel.Width = outerGrid.Width;

            grid = new Grid(gui, ListBoxType.LCD , Label.LabelType.LCDNormal);
            grid.FixedItemHeights = true; // false;
            grid.Width = panel.Width; // make grid fill the collapsable panel
            panel.AddContent(grid); // .ExpandedPanel.Add(categoryGrid);
            grid.ScrollBarEnabled = false;
            grid.ItemHeight = itemHeight.Value;
            grid.CanGrowInHeight = true;
            grid.Font = GUIManager.LCDandHUDFontPath;
            grid.Selectability = selectability;
            grid.IsOuterGrid = false;
            grid.CanReceiveMouseWheelEvents = false;
            
        }


        public static void AddCollapsablePanelAndGridNotFixed(GUIManager gui, Grid outerGrid, string title, out CollapsablePanel panel, out Grid grid, Grid.SelectabilityOptions selectability = Grid.SelectabilityOptions.Single)
        {
            
            panel = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownSmall);
            panel.CollapsedHeight = outerGrid.ItemHeight;
            panel.Init();
            outerGrid.AddEntry(panel, panel); // NEW: use panel as key.          
            panel.Title = title;
           // panel.Width = outerGrid.Width;

            grid = new Grid(gui, ListBoxType.LCD , Label.LabelType.LCDNormal);
            grid.FixedItemHeights = false; 
            grid.Width = panel.Width; // make grid fill the collapsable panel
            panel.AddContent(grid); // .ExpandedPanel.Add(categoryGrid);
            grid.ScrollBarEnabled = false;          
            grid.CanGrowInHeight = true;
            grid.Font = GUIManager.LCDandHUDFontPath;
            grid.Selectability = selectability;
            grid.IsOuterGrid = false;
            grid.CanReceiveMouseWheelEvents = false;

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
        
        public override void Hide()
        {
            if (MainControlButton != null)
            {
                MainControlButton.IsChecked = false;
            }

            base.Hide();
        }

        public override void Show()
        {
            if (statusContent != null)
            {
                The.InGameUI.StatusScreen.ChangeContent(statusContent);
            }

            base.Show();
        }

    }
}*/
