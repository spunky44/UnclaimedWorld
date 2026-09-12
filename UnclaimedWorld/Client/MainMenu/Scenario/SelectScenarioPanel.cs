using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using WindowSystem;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.ClientSide.Interface.LCD;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace UWGame.ClientSide.MainMenu.Scenario
{
    public class SelectScenarioPanel : Panel
    {
        Box display; //, edges;
        LCDScreen lcdScreen;
        UIComponent lcdSurface;

        Grid grid;
            
      
        public event EventHandler CancelClick;

        private string replayFileName;
       
        private SelectScenarioInterface selectScenarioInterface;

        const int itemHeight = 112;

        int itemPadding = SingleSpacing;

        public SelectScenarioPanel(SelectScenarioInterface intf, Point position) :
            base(intf, "SCENARIOS", position,
                 new Vector2(800 /*792*/, 620), Level.Middle)
        {
            selectScenarioInterface = intf;
           
            RosterPanel.CreateRosterStyleLCDPanel(intf, Window, out display, out lcdSurface, ref lcdScreen, 55);

            grid = FullLCDPanel.AddGridWithFixedItemHeights(intf.gui, lcdSurface, 0); 
            grid.ItemHeight = itemHeight;
            grid.Selectability = Grid.SelectabilityOptions.None;
        
            InitButtons();

          //  TextButton btClose = ExpandedPanel.AddCloseButton(Form);
          //  btClose.Click += new ClickHandler(btClose_Click);

           
           // Populate();

         
        }

       
        private List<SimSide.Scenarios.Scenario> GetListOfScenarios()
        {
            // get vanilla scenarios and user scenarios:
            List<SimSide.Scenarios.Scenario> rgScenarios = RGScenarioLoader.LoadAllScenarioHeaders();

            // sort the list:
            rgScenarios = rgScenarios.OrderBy(s => s.SortOrder).ToList();

            return rgScenarios;

        }


        const int selectButtonWidth = 80;

        public const int ThumbnailPanelWidth = 112; // 135;
        public const int DescriptionPanelWidth = 469; // 446;
        public const int SelectPanelWidth = 148;

        const int horizPadding = 8;
        const int vertPadding = 5;

        private void AddItemRow(SimSide.Scenarios.Scenario scenario)
        {
            Image thumbnail;
            UIComponent item;

            item = new UIComponent(Interface.gui);

            LCDInnerPanel thumbnailPanel = new LCDInnerPanel(Interface.gui, ThumbnailPanelWidth, false); // lcdSurfaceOptions.Width);
            item.Add(thumbnailPanel.Panel);


            int thumbnailWidth = itemHeight - 2 * itemPadding;
            if (!string.IsNullOrEmpty(scenario.ThumbnailImage))
            {
                thumbnail = new Image(Interface.gui);
                thumbnailPanel.AddContentSetFullWidth(thumbnail); // .Panel.Add(thumbnail);

                Rectangle? rect;
                if (!Interface.gui.GUISpriteSheet.TryGetSourceRectangle(scenario.ThumbnailImage, out rect))
                {                   
                    thumbnail.Texture = Interface.gui.ContentManager.Load<Texture2D>(scenario.ThumbnailImage);
                 //   thumbnail.SetSkinLocation(SkinState.Normal,thumbnail.Texture.Bounds);
                }
                else
                {
                    thumbnail.SetSkinLocation(SkinState.Normal,rect.Value);
                    thumbnail.Texture = Interface.gui.GUISpriteSheet.Texture;        
                }
                //   Rectangle rect = intf.gui.GUI_SpriteSheet.GetSourceRectangle(scenario.Image); //???

               
               // item.Add(thumbnail);
               
              //  thumbnail.SetSkinLocation(SkinState.Normal,rect);
              //  thumbnail.Texture = Interface.gui.GUISpriteSheet.Texture;              
                thumbnail.Position = new Point(itemPadding, itemPadding);
                thumbnail.Height = thumbnailWidth;
                thumbnail.Width = thumbnailWidth;
                //icon.ResizeControlToFitImage();
            }

            LCDInnerPanel descriptionPanel = new LCDInnerPanel(Interface.gui, DescriptionPanelWidth, false); // lcdSurfaceOptions.Width);
            item.Add(descriptionPanel.Panel);
            descriptionPanel.Panel.X = thumbnailPanel.Panel.Right - 2; // collapse borders
            descriptionPanel.HorizontalContentPadding = horizPadding;
            descriptionPanel.VerticalContentPadding = vertPadding;
            descriptionPanel.ContentHeight = 100;

            Label lblName = new Label(Interface.gui);          
            descriptionPanel.AddContent(lblName);
            lblName.Init(Label.LabelType.LCDHeadingBlue); // LCDSmallHeadingBanner);
            lblName.Text = scenario.DisplayName ?? "(Missing)"; // scenario.Name.ToUpper(Config.Culture);

            Label lblMapSize = new Label(Interface.gui);
            descriptionPanel.AddContent(lblMapSize);
            lblMapSize.Init(Label.LabelType.LCDHeadingSteelGrey); //LCDHeadingBlue); // LCDSmallHeadingBanner);
            lblMapSize.Text = "Map size: " + UWGame.SimSide.Scenarios.Scenario.GetMapSizeAsString(scenario.MapSize);
            lblMapSize.ToolTip = "The map size gives a hint about the hardware requirements for the AI to function properly. Larger maps usually have higher CPU demands.";//MP: was "The map size gives a hint about the hardware requirements. Larger maps usually have higher CPU demands."
            lblMapSize.FitToText();
            lblMapSize.X = descriptionPanel.ContentWidth - lblMapSize.Width;

            TextArea taDescription = new TextArea(Interface.gui, ListBoxType.LCD); // ListBoxType.Main); // allow scrolling???
            descriptionPanel.Panel.Add(taDescription);
            descriptionPanel.AddContentSetFullWidth(taDescription);
            taDescription.Init(Label.LabelType.LCDNormal);
            taDescription.CanGrowInHeight = false;
            taDescription.ScrollBarEnabled = false;          
            taDescription.Y = lblName.Bottom + 4;
         //   taDescription.Position = new Point(descriptionPanel., lblName.Bottom + SingleSpacing);
         //   taDescription.Width = descriptionPanel.ContentWidth; // 350; // grid.Width - taDescription.X - selectButtonWidth - SingleSpacing; // itemPadding;
            taDescription.Height = descriptionPanel.ContentHeight - taDescription.Y;
            taDescription.HMargin = 0;
            taDescription.VMargin = 0;
            taDescription.Text = scenario.SummaryDescription;
            

            LCDInnerPanel selectPanel = new LCDInnerPanel(Interface.gui, SelectPanelWidth, false); // lcdSurfaceOptions.Width);
            item.Add(selectPanel.Panel);
            selectPanel.Panel.X = descriptionPanel.Panel.Right - 2;


            TextButton tbSelect = new TextButton(Interface.gui);
            selectPanel.Panel.Add(tbSelect);
            tbSelect.Init(TextButton.TextButtonType.LCD); //TextButton.TextButtonType.LCD);
            tbSelect.Text = "SELECT";
            tbSelect.ScaleWidthToFitText();
            tbSelect.X = selectPanel.Panel.Width - tbSelect.Width - selectPanel.HorizontalContentPadding;
            tbSelect.Y = selectPanel.Panel.Height - tbSelect.Height; // -selectPanel.VerticalContentPadding;
          //  tbSelect.Position = new Point(taDescription.Right + SingleSpacing, itemPadding);
         //   tbSelect.EventArgs = eventArgs; // new ItemTypeButtonEventArgs(entityType);
            tbSelect.EventArgs = new ScenarioEventArgs() { Scenario = scenario };
            tbSelect.Click += new ClickHandler(tbSelect_Click);
            
            if (!Environment.Is64BitProcess && scenario.Allow32Bit == false)
            {
                tbSelect.Enabled = false;
                tbSelect.ToolTip = "Requires 64 bit, not available under 32 bit.";
            }


            grid.AddEntry(scenario, item);

        }

        void tbSelect_Click(UIComponent sender, EventArgs e)
        {
            selectScenarioInterface.Screen.SelectScenario(((ScenarioEventArgs)e).Scenario);           
        }

        private void Populate()
        {
            List<SimSide.Scenarios.Scenario> listScenarios = GetListOfScenarios();

            grid.BeginAddingEntries();
            grid.Clear();

            foreach (var item in listScenarios)
            {
                AddItemRow(item);
            }

            grid.EndAddingEntries();
        }

        void btClose_Click(UIComponent sender, EventArgs e)
        {
            Window.Hide();

            if (CancelClick != null)
            {
                CancelClick.Invoke(sender, e);
            }
        }


        private void InitButtons()
        {
            
          
            /////////////////////////////////
           /* Box brown = new Box(Interface.gui);
            Form.Add(brown);
            rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_darkblue"); //"basic_brown");
            brown.SetSkinLocation(SkinState.Normal,rect);
            brown.CornerSize = 3;
            brown.Position = new Point(display.X, Form.Height - 40 - MarginY);//edges.Y + edges.Height + 10 + 70);
            brown.Width = display.Width;
            brown.Height = 40;
            */
          

            TextButton btCancel = new TextButton(Interface.gui);
            Window.Add(btCancel); // add first!!! sets defaults!
            btCancel.Init(TextButton.TextButtonType.White);
            PlaceLeftButtonUnderLCD(btCancel);
            //btCancel.Position = new Point(20, Form.Height - 42 - MarginY);
            btCancel.Text = "MAIN";
            btCancel.ScaleWidthToFitText();
            btCancel.ToolTip = "Return to the main menu";

            btCancel.Click += new ClickHandler(btCancel_Click);


            Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
            Panel.AddImage(Interface.gui, Window, rect, new Point(40, 30));

        }

      /*  void buttonLoad_Click(UIComponent sender, EventArgs e)
        {
            object key;
            if (grid.GetSelectedKey(out key))
            {
                replayFileName = key.ToString();
            }
            else
            {
                replayFileName = null;
            }

           
        }*/

        public override void ShowDialog(bool modal)
        {

            base.ShowDialog(modal);

            Populate();
        }

        void btCancel_Click(UIComponent sender, EventArgs e)
        {

            Window.Hide();

            if (CancelClick != null)
            {
                CancelClick.Invoke(sender, e);
            }
        }

       
        public override void Update(GameTime elapsed)
        {
            base.Update(elapsed);

        }


        class ScenarioEventArgs : EventArgs
        {
            public SimSide.Scenarios.Scenario Scenario; 
        }
    }
}
