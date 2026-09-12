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
using UWGame.SimSide.Scenarios;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.XmlCollections;

namespace UWGame.ClientSide.MainMenu.Scenario
{
    public class CustomizeScenarioPanel : Panel
    {
        #region Description screens

        LCDScreen lcdScreenDescription = null;
        UIComponent lcdSurfaceDescription;

        // in LCD screen:
        Grid descriptionSurfaceGrid;

        Label lblHeader;
      //  UIComponent headerPadding;

        TextArea area;

        // in CRT screen:
        Image image;


        int descriptionBottom;

        CRTScreen crtScreen;

        #endregion

        #region Options screen

        Box displayBoxOptions; 
        LCDScreen lcdScreenOptions;
        UIComponent lcdSurfaceOptions;

        /// <summary>
        /// this outer grid has a scrollbar and contains main difficulty and a panel
        /// </summary>
        Grid optionsSurfaceGrid;

        /// <summary>
        /// this bordered panel is shown when Custom is selected
        /// </summary>
        LCDInnerPanel pnCustom;

        /// <summary>
        /// this grid sits inside the bordered panel and contains the list of options
        /// </summary>
        Grid grdOptions;

        //Label lblScoreHeader;
        Label lblExtraOptions;

        int scoreHeaderRight = 856;
         

        #endregion

        const int minimumContentWidth = 640;
        const int maximumContentWidth = 1024;

        public event EventHandler CancelClick;

       
        private CommonInterface customizeScenarioInterface;

        const int itemHeight = 90;

        int itemPadding = SingleSpacing;

       // SimSide.Scenarios.Scenario scenario;

        CustomizeScenarioScreen screen;

        RadioGroup mainDifficultyRadioGroup;
        RadioButton rbCustom;

        UIComponent errorPaddingBox;
        Label lblError;

        TextArea taDifficulty;

        Image columnDivider1, columnDivider2;

        int selectionColumnWidth;

        const int setTitleColumnX = 0;
        const int difficultyColumnX = 180;
        const int randomizeColumnX = 480;
        const int selectionColumnX = 640;
        const int scoreColumnX = 800;

        const int columnSpacing = DoubleSpacing;

        #region Customize

      //  bool optionsArePopulated = false;

       
        ///*UIComponent*/ Box pnCustom;

     

        #endregion

        TextButton btStart;
        TextButton btCancel;

        public CustomizeScenarioPanel(CommonInterface intf, Point position, CustomizeScenarioScreen screen) : //SimSide.Scenarios.Scenario scenario) :
            base(intf, "CREATE GAME", position,
                 new Vector2(1000, intf.gui.ScreenHeight - 180), // not final values - the panel gets shrinked (width AND height!) after populating      
                Level.Middle)        
        {
            customizeScenarioInterface = intf;

            this.screen = screen;
            //this.scenario = scenario;
            

            selectionColumnWidth = scoreColumnX - selectionColumnX - columnSpacing;

         //   CreateDescriptionScreens();           

            // we scale the whole window based on the content width of the options screen:
            CreateOptionsScreen(intf);
            PopulateMainDifficulty();          
            PopulateCustomOptions();

            // after populating, the whole screen will be shrinked/grown to fit:
            SetPanelDimensions();

            // create the description screen after we know the width...
            CreateDescriptionScreens();
            PopulateDescription();


            // called now to overlay on LCD...
            RosterPanel.AddDirtOnIrregularEdges(intf.gui, Window);

        }

        private void CreateOptionsScreen(CommonInterface intf)
        {


            int lcdY = 280; // 237;

            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, BottomMarginForButtons, new Point(MarginX, lcdY), out displayBoxOptions, out lcdSurfaceOptions, ref lcdScreenOptions);

           // AddWatermark(intf.gui, lcdSurfaceOptions);

           

          /*  lbl = new Label(intf.gui);
            lcdSurfaceOptions.Add(lbl);
            lbl.Init(Label.LabelType.LCDNormal);
            lbl.Text = screen.Scenario.Name;
            lbl.FitToText();
            lbl.X = SingleSpacing;
            lbl.Y = DoubleSpacing;
            */

           
            CreateSurfaceWithScrollbar(out optionsSurfaceGrid, lcdSurfaceOptions); //, false);

            optionsSurfaceGrid.DebugTag = "options";

            
           // pnCustom = new UIComponent(intf.gui);

            taDifficulty = new TextArea(intf.gui, ListBoxType.LCD);
            taDifficulty.RenderType = RenderType.CRTAndLCD;         
            taDifficulty.Width = optionsSurfaceGrid.SurfaceWidth;
            taDifficulty.Init(Label.LabelType.LCDNormal);
            taDifficulty.CanGrowInHeight = true;
            taDifficulty.ScrollBarEnabled = false;
            taDifficulty.DebugTag = "taDifficulty";
           // taDifficulty.CanHaveFocus = false; // don't make the lines selctable, needed because the container grid can have focus

            // these are addeed when the custom button is clicked:
            pnCustom = new LCDInnerPanel(intf.gui, optionsSurfaceGrid.SurfaceWidth, true); // lcdSurfaceOptions.Width);

            errorPaddingBox = new UIComponent(intf.gui);
            errorPaddingBox.Width = pnCustom.Panel.Width;
            errorPaddingBox.Height = 32;

            lblError = new Label(intf.gui);
         //   pnCustom.Panel.Add(lblError);
            lblError.Init(Label.LabelType.LCDError);          
            lblError.Width = pnCustom.Panel.Width;
           // lblError.Y = 8;                        
            errorPaddingBox.Add(lblError);
            errorPaddingBox.CenterChildVertically(lblError);


         /*   pnCustom = new Box(intf.gui);
            pnCustom.CornerSize = 20;
            pnCustom.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"));
            pnCustom.Width = lcdSurfaceOptions.Width;
            pnCustom.Height = 100;
            pnCustom.X = -5;

            Image upperRightDecor = new Image(intf.gui);
            upperRightDecor.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("lcd_panel_decor_uppercorner"));
            upperRightDecor.ResizeControlToFitImage();
            pnCustom.Add(upperRightDecor);

            Image lowerRightDecor = new Image(intf.gui);
            lowerRightDecor.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("lcd_panel_decor_lowercorner"));
            lowerRightDecor.ResizeControlToFitImage();
            pnCustom.Add(lowerRightDecor);
*/


           

          /*  Label lblRandomHeader = new Label(intf.gui);
            pnCustom.Add(lblRandomHeader);
            lblRandomHeader.Init(Label.LabelType.LCDHeader);
            lblRandomHeader.Text = "Randomize";
            lblRandomHeader.Position = new Point(randomizeColumnX, itemPadding);
            lblRandomHeader.Width = selectionColumnX - randomizeColumnX - itemPadding;
            */
            int headerY = 13;

            lblExtraOptions = new Label(intf.gui);
            pnCustom.Panel.Add(lblExtraOptions);
            lblExtraOptions.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblExtraOptions.Text = "EXTRA OPTIONS";
            lblExtraOptions.Position = new Point(selectionColumnX, headerY);
           // lblSelectHeader.Width = scoreColumnX - selectionColumnX - itemPadding;



            
          /*  lblScoreHeader = new Label(intf.gui);
            pnCustom.Panel.Add(lblScoreHeader);
            lblScoreHeader.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblScoreHeader.Text = "SCORE";
            lblScoreHeader.Position = new Point(scoreColumnX, headerY);
            */
           

            grdOptions = new Grid(intf.gui, ListBoxType.LCD /* ListBoxType.Main*/, Label.LabelType.LCDNormal);
           // pnCustom.Panel.Add(grdOptions);
            pnCustom.AddContentSetFullWidth(grdOptions);
            grdOptions.X = pnCustom.HorizontalContentPadding;
            grdOptions.Y = 30;
            grdOptions.IsOuterGrid = true;
            grdOptions.FixedItemHeights = true;
            grdOptions.ItemHeight = 40;
            SetOptionsGridWidth();
            grdOptions.HeightResize += new ResizeHandler(grdOptions_HeightResize);
            grdOptions.CanGrowInHeight = true;
            grdOptions.ScrollBarEnabled = false;

            Rectangle rect = intf.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
            Panel.AddImage(intf.gui, Window, rect, new Point(40, 30));

            int dividerY = 6;
            columnDivider1 = new Image(intf.gui);
            columnDivider1.SetSkinLocation(SkinState.Normal,intf.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
            columnDivider1.Y = dividerY;
            columnDivider1.Width = 1;
            columnDivider1.ScaleImageToSizeOfControl = true;
            pnCustom.Panel.Add(columnDivider1);

            columnDivider2 = new Image(intf.gui);
            columnDivider2.SetSkinLocation(SkinState.Normal,intf.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
            columnDivider2.Y = dividerY;
            columnDivider2.Width = 1;
            columnDivider2.ScaleImageToSizeOfControl = true;
            pnCustom.Panel.Add(columnDivider2);


            /////////////////////////////////
        //    int blotY = Form.Height - 40 - MarginY;
            
            /*   Box brown = new Box(intf.gui);
               Form.Add(brown);
               rect = intf.gui.GUISpriteSheet.GetSourceRectangle("basic_darkblue"); //"basic_brown");
               brown.SetSkinLocation(SkinState.Normal,rect);
               brown.CornerSize = 3;
               brown.Position = new Point(edges.X, blotY);//edges.Y + edges.Height + 10 + 70);
               brown.Width = edges.Width;
               brown.Height = 40;
               */

            int buttonsY = displayBoxOptions.Bottom + SingleSpacing;

            btCancel = new TextButton(intf.gui);
            Window.Add(btCancel); // add first!!! sets defaults!
            btCancel.Init(TextButton.TextButtonType.White);        
            btCancel.Position = new Point(displayBoxOptions.X - 2, buttonsY);
          //  PlaceButtonUnderLCD(btCancel);
            btCancel.Text = "MAIN";
            btCancel.ScaleWidthToFitText();
            btCancel.ToolTip = "Return to the main menu";
            btCancel.Click += new ClickHandler(btCancel_Click);


            btStart = new TextButton(intf.gui);
            Window.Add(btStart); 
            btStart.Init(TextButton.TextButtonType.White);          
            btStart.Text = "START";
            btStart.ToolTip = "Start the game";
            btStart.ScaleWidthToFitText();
         //   btStart.Position = new Point(brown.Right - btStart.Width - 20, blotY - 2);  
            btStart.Position = new Point(displayBoxOptions.Right - btStart.Width + 3, buttonsY);  
            btStart.Click += new ClickHandler(btStart_Click);
        }

        private void SetOptionsGridWidth()
        {
            // this grid sits inside the inner panel, so must account for its padding:
            grdOptions.Width = pnCustom.Panel.Width - grdOptions.X;
        }

        void grdOptions_HeightResize(UIComponent sender)
        {
            //Grid  grid = (Grid)sender;
            //pnCustom.Height = ((Grid)sender).Height;
            
            pnCustom.ContentHeight = grdOptions.Bottom;
            //pnCustom.Height = grdOptions.Bottom;

            columnDivider1.Height = columnDivider2.Height = grdOptions.Height;

        }


        private void CreateDescriptionScreens()
        {
            int height = 216; 

            Box display, /*edges,*/ crtPlasticEdge;
            lcdScreenDescription = null;
            
            int crtX = MarginX; 
            int crtY = 52; // MarginX; 

            int crtEdge = 4;
            int crtWidth = 280;
            int crtHeight = height;

            //*** CRT ****
            image = new WindowSystem.Image(Interface.gui);
            Window.Add(image);
            image.RenderType = RenderType.CRTAndLCD;
            image.ScaleImageToSizeOfControl = true;
            image.Width = crtWidth; // -2 * crtEdge;
            image.Height = crtHeight; // -2 * crtEdge;
            image.X = crtX; // +crtEdge; 
            image.Y = crtY; // +crtEdge; 

            StatusScreen.AddCRTPlasticFrame(Interface.gui, Window, image.Position, crtWidth, crtHeight, out crtPlasticEdge);

           /* crtPlasticEdge = new Box(Interface.gui);
            //edges.RenderType = RenderType.Overlay;
            Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("TV_plastic_edge");
            crtPlasticEdge.SetSkinLocation(SkinState.Normal,rect);
            crtPlasticEdge.CornerSize = 30; // MP : me screwing around.. was 15
            crtPlasticEdge.Position = new Point(crtX, crtY);
            crtPlasticEdge.Width = crtWidth;
            crtPlasticEdge.Height = crtHeight;
            crtPlasticEdge.RenderType = RenderType.Overlay;
            Form.Add(crtPlasticEdge);
            */

            crtScreen = Interface.DisplayPanelRenderer.AddCRT(image, 
                new Point(crtPlasticEdge.AbsolutePosition.X + crtEdge,
                          crtPlasticEdge.AbsolutePosition.Y + crtEdge),
                    crtPlasticEdge.Width - 2 * crtEdge, crtPlasticEdge.Height - 2 * crtEdge, Window.Level, Window,
                    ReflectionToUse.Small, true);

            // ****** LCD *********
        
            int lcdX = crtPlasticEdge.Right; // + DoubleSpacing;

            FullLCDPanel.AddLCDPanel(Interface, Window, 
                new Point(lcdX, MarginTop /* crtY*/),
                Window.Width - MarginX - lcdX,               //FullLCDPanel.GetLCDPanelFullWidth(Form, MarginX) - lcdX,              
                224,
                out display, out lcdSurfaceDescription, ref lcdScreenDescription);

            descriptionBottom = display.Bottom;  //  edges.Bottom;

            int sideMargin = 12;

            CreateSurfaceWithScrollbar(out descriptionSurfaceGrid, lcdSurfaceDescription, false);

            descriptionSurfaceGrid.DebugTag = "descriptionGrid";

            descriptionSurfaceGrid.BeginAddingEntries();

            lblHeader = new Label(Interface.gui);
            descriptionSurfaceGrid.AddEntry(lblHeader, lblHeader);
            lblHeader.Init(Label.LabelType.LCDBigHeaderBanner);
            lblHeader.Y = 0; // itemPadding;
            lblHeader.X = 0;
            lblHeader.Width = descriptionSurfaceGrid.Width;

          /*  headerPadding = new UIComponent(Interface.gui);
            headerPadding.Height = 36;

            lblHeader = new Label(Interface.gui);    
            headerPadding.Add(lblHeader);
            descriptionSurfaceGrid.AddEntry(headerPadding, headerPadding); // this will reset font on the label...
            lblHeader.Init(Label.LabelType.LCDHeader); // this will set the font we want to use
            lblHeader.X = sideMargin;        
            headerPadding.CenterVertically(lblHeader);
          */

            area = new TextArea(Interface.gui, ListBoxType.LCD); 
            area.RenderType = RenderType.CRTAndLCD;
            area.Init(Label.LabelType.LCDNormal);         
            area.CanGrowInHeight = true;
            descriptionSurfaceGrid.AddEntry(area, area);
            area.X = sideMargin;
            area.Y = 55;
            area.Width = lcdSurfaceDescription.Width - 2 * sideMargin; // 196; // triggers BreakText that requires font to be set
            // area.Height = lcdSurface.Height - 2 * topMargin; // 126;
            //area.RenderType = RenderType.CRTAndLCD; // RenderType.Normal; 
            //  area.ZOrder = 1f;


            descriptionSurfaceGrid.EndAddingEntries();
           

        }

        void btStart_Click(UIComponent sender, EventArgs e)
        {
            lblError.Text = "";
            SerializableDictionary<string, Option> selectedOptions;

            // gather the settings and compile the events for the new game:
            if (rbCustom != null && rbCustom.IsChecked)
            {
                string error;
               
                if (GetCustomSettings(out error, out selectedOptions))
                {
                    StartGame(selectedOptions, null);
                }
                else
                {
                    lblError.Text = error;
                }

            }
            else
            {
                Difficulty mainDifficulty;
                GetMainSettings(out selectedOptions, out mainDifficulty);
                StartGame(selectedOptions, mainDifficulty);
            }
        }


        private void StartGame(SerializableDictionary<string, Option> options, Difficulty mainDifficulty)
        {

            ((CustomizeScenarioInterface)Interface).Screen.StartGame(options, mainDifficulty);

        }

        private bool GetCustomSettings(out string error, out SerializableDictionary<string, Option> selectedOptions)
        {
            error = null;
            selectedOptions = new SerializableDictionary<string, Option>();

            // for hidden entries (where there is only one option to select from), we retrieve the data same as with GetMain (not custom)
            foreach (var item in screen.Scenario.ScenarioData.OptionSets)
            {
                UIComponent gridEntry;
                if (grdOptions.TryGetEntry(item, out gridEntry)) // OptionSet is key
                {
                    OptionSet optionSet = (OptionSet)gridEntry.Tag1;

                    ComboBox cbDifficulty = (ComboBox)gridEntry.FindChildById(UIComponent.DataControlID.Difficulty);
                    string difficultyKey = (string)cbDifficulty.SelectedKey;

                    var optionsToSelectFrom = optionSet.OptionsGroupedByDifficulty.First(g => g.Key == difficultyKey).ToList();


                    CheckBox cbRandom = (CheckBox)gridEntry.FindChildById(UIComponent.DataControlID.Randomize);
                    if (cbRandom.IsChecked)
                    {
                        // get a random option to use:

                        //optionsToSelectFrom.
                        var random = Common.GetRandomListMember(optionsToSelectFrom, screen.Controller.RandomGenerator);

                        selectedOptions.Add(optionSet.KeyName, random);

                    }
                    else
                    {
                        Option option = null;
                        if (optionsToSelectFrom.Count > 1)
                        {
                            // get the user's choice - abort if no selection has been made:
                            ComboBox cbOptionSelector = (ComboBox)gridEntry.FindChildById(UIComponent.DataControlID.Option);

                            if (cbOptionSelector.SelectedKey == null)
                            {
                                error = "INPUT NEEDED - Please make a selection under the EXTRA OPTIONS column for: " + optionSet.Name;  //"No selection for " + optionSet.Name;
                                return false;
                            }
                            else
                            {
                                option = optionSet.Options.First(o => o.KeyName == (string)cbOptionSelector.SelectedKey);
                            }
                        }
                        else
                        {
                            option = optionsToSelectFrom[0];
                        }

                        selectedOptions.Add(optionSet.KeyName, option);
                    }

                }
                else
                {
                    selectedOptions.Add(item.KeyName, item.Options[0]);

                }                
            } 

/*
            foreach (var item in grdOptions.Entries)
            {
                OptionSet optionSet = (OptionSet)item.Tag1;

                ComboBox cbDifficulty = (ComboBox)item.FindChildById(UIComponent.DataControlID.Difficulty);
                string difficultyKey = (string)cbDifficulty.SelectedKey;

                var optionsToSelectFrom = optionSet.OptionsGroupedByDifficulty.First(g => g.Key == difficultyKey).ToList();


                CheckBox cbRandom = (CheckBox)item.FindChildById(UIComponent.DataControlID.Randomize);
                if (cbRandom.IsChecked)
                {
                    // get a random option to use:
                   
                    //optionsToSelectFrom.
                    var random = Common.GetRandomListMember(optionsToSelectFrom, screen.ScreenManager.RandomGenerator);

                    selectedOptions.Add(optionSet.KeyName, random);

                }
                else 
                {
                    Option option = null;
                    if (optionsToSelectFrom.Count > 1)
                    {
                        // get the user's choice - abort if no selection has been made:
                        ComboBox cbOptionSelector = (ComboBox)item.FindChildById(UIComponent.DataControlID.Option);

                        if (cbOptionSelector.SelectedKey == null)
                        {
                            error = "INPUT NEEDED - Please make a selection under the EXTRA OPTIONS column for: " + optionSet.Name;  //"No selection for " + optionSet.Name;
                            return false;
                        }
                        else
                        {
                            option = optionSet.Options.First(o => o.KeyName == (string)cbOptionSelector.SelectedKey);                          
                        }
                    }
                    else
                    {
                        option = optionsToSelectFrom[0];
                    }

                    selectedOptions.Add(optionSet.KeyName, option);
                }

            }*/

            return true;

        }

        private void GetMainSettings(out SerializableDictionary<string, Option> selectedOptions, out Difficulty mainDifficulty)
        {
            mainDifficulty = null;

            selectedOptions = new SerializableDictionary<string, Option>();
            if (mainDifficultyRadioGroup != null)
            {
                RadioButton rbSelected = mainDifficultyRadioGroup.GetSelected();

                mainDifficulty = (Difficulty)rbSelected.Tag1;

                OptionSet[] optionSets = screen.Scenario.ScenarioData.OptionSets;

                foreach (var item in mainDifficulty.OptionsToUse)
                {                   
                    if (item.Key == "equipment")
                    {                        

                    }

                    OptionSet optionSet = optionSets.FirstOrDefault(o => o.KeyName == item.Key);
                    string randomOptionKey = Common.GetRandomListMember(item.Value, screen.Controller.RandomGenerator);

                    Option randomOption = optionSet.Options.FirstOrDefault(o => o.KeyName == randomOptionKey);

                    if (randomOption == null)
                    {

                    }

                    selectedOptions.Add(optionSet.KeyName, randomOption);
                }
            }

        }

        private class DifficultyEventArgs: EventArgs
        {
            //public string KeyName;
            public Difficulty Difficulty;
        }

        private class CustomDifficultyEventArgs : EventArgs
        {
           
            public OptionSet OptionSet;
            public string CustomDifficultyKey;

        }

        private class CustomDifficultyRandomizeEventArgs : EventArgs
        {           
            public OptionSet OptionSet;         
        }

        int mainDifficultyYPos = 0; // 60;

        private void PopulateDescription()
        {
            string text = screen.Scenario.Description; 
            string imageName = screen.Scenario.Image;
            string heading = screen.Scenario.Name.ToUpper(Config.Culture);            

            
            if (imageName != null)
            {
                Window.Add(image);
                image.SetSkinLocation(SkinState.Normal,Interface.gui.GUI_CRT_SpriteSheet.GetSourceRectangle(imageName));
            }
            else
            {
                // show default image??
                Window.Remove(image);
            }


            image.Texture = Interface.gui.GUI_CRT_SpriteSheet.Texture;

            descriptionSurfaceGrid.BeginAddingEntries();

            lblHeader.Text = heading; // heading ?? "";
            //lblHeader.FitToText();            
            //headerPadding.CenterVertically(lblHeader);

            area.Text = text;

            descriptionSurfaceGrid.EndAddingEntries();
           
        }




        private void PopulateMainDifficulty()
        {
            if (screen.Scenario.ScenarioData.MainDifficultySettings == null
                || screen.Scenario.ScenarioData.MainDifficultySettings.Length == 0)
            {
                return;
            }

       /*     Label lbl = new Label(intf.gui);
            lcdSurfaceOptions.Add(lbl);
            lbl.Init(Label.LabelType.LCDNormal);
            lbl.Text = "Difficulty:";
            lbl.FitToText();
            lbl.X = SingleSpacing;
            lbl.Y = mainDifficultyYPos;*/
            

            optionsSurfaceGrid.BeginAddingEntries();

            Box headerBanner = new Box(Interface.gui);
            headerBanner.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_header_big"));
            headerBanner.Height = 36;
            headerBanner.CornerSize = 18;
            optionsSurfaceGrid.AddEntry(headerBanner, headerBanner);

            Label lblMainDifficulty = new Label(Interface.gui);
          //  optionsSurfaceGrid.AddEntry(lblHeading, lblHeading);
            headerBanner.Add(lblMainDifficulty);
            lblMainDifficulty.Init(Label.LabelType.LCDNormalLight); //.LCDBigHeaderBanner);

          //  lblMainDifficulty.Y = mainDifficultyYPos;
            lblMainDifficulty.X = 14;
            lblMainDifficulty.Text = "DIFFICULTY:";
            lblMainDifficulty.FitToText();
            headerBanner.CenterChildVertically(lblMainDifficulty);



            mainDifficultyRadioGroup = new RadioGroup(Interface.gui);

            headerBanner.Add(mainDifficultyRadioGroup);
            //optionsSurfaceGrid.AddEntry(buttonGroup, buttonGroup);
          
            mainDifficultyRadioGroup.Width = optionsSurfaceGrid.Width; // 600;
            mainDifficultyRadioGroup.Height = 40;
            mainDifficultyRadioGroup.Position = new Point(lblMainDifficulty.Right + DoubleSpacing, mainDifficultyYPos);
            mainDifficultyRadioGroup.DebugTag = "radioGroup";

            optionsSurfaceGrid.EndAddingEntries();
            

           // int radioWidth = 100;

            int radioYpos = 7; // SingleSpacing;
            int lastX = DoubleSpacing;

            if (HasCustomOptions())
            {
                rbCustom = new RadioButton(Interface.gui);
                mainDifficultyRadioGroup.Add(rbCustom);
                rbCustom.Init(CheckBoxType.LCDRadio);
                rbCustom.X = lastX;
                rbCustom.Y = radioYpos;
                rbCustom.Text = "CUSTOM";
                rbCustom.Click += new ClickHandler(rbCustom_Click);

                lastX = rbCustom.Right + DoubleSpacing;
            }
            
            bool defaultWasSet = false;
            foreach (var item in screen.Scenario.ScenarioData.MainDifficultySettings)
            {
              /*  Label lbl = new Label(Interface.gui);
                headings.Add(lbl);
                lbl.Init(Label.LabelType.LCDNormal);
                lbl.Text = item.Name;
                lbl.FitToText();
                lbl.X = lastX - lbl.Width / 2; // center
                */
                if (item.OptionsToUse != null)
                {
                    RadioButton rbDifficultySetting = new RadioButton(Interface.gui);
                    mainDifficultyRadioGroup.Add(rbDifficultySetting);
                    rbDifficultySetting.Init(CheckBoxType.LCDRadio);
                    //rbDifficultySetting.Width = radioWidth;
                    rbDifficultySetting.X = lastX; 
                    rbDifficultySetting.Y = radioYpos;
                    rbDifficultySetting.Text = item.Name.ToUpper(Config.Culture);
                    rbDifficultySetting.Tag1 = item;
                    rbDifficultySetting.EventArgs = new DifficultyEventArgs() { Difficulty = item };
                    rbDifficultySetting.Click += new ClickHandler(rbDifficultySetting_Click);
                    //rbDifficultySetting.
                    if (item.IsDefault)
                    {
                        rbDifficultySetting.IsChecked = true;
                        defaultWasSet = true;

                        
                    }

                    lastX = rbDifficultySetting.Right + DoubleSpacing;
                }

                
            }

            if (!defaultWasSet)
            {
                ((RadioButton)mainDifficultyRadioGroup.Controls[1]).IsChecked = true; // check the first button per default
            }

            ShowDifficulty((Difficulty)(mainDifficultyRadioGroup.GetSelected().Tag1));

            /*
            RadioButton rbEasy = new RadioButton(intf.gui);
            buttonGroup.Add(rbEasy);
            rbEasy.Init(CheckBoxType.LCDRadio);
            rbEasy.X = 140;
            rbEasy.Text = "Easy";
            rbEasy.Width = radioWidth;

            rbEasy.DebugTag = "radioButton";

            RadioButton rbNormal = new RadioButton(intf.gui);
            buttonGroup.Add(rbNormal);
            rbNormal.Init(CheckBoxType.LCDRadio);
            rbNormal.X = rbEasy.Right + DoubleSpacing;
            rbNormal.Text = "Normal";
            rbNormal.Width = radioWidth;

            RadioButton rbHard = new RadioButton(intf.gui);
            buttonGroup.Add(rbHard);
            rbHard.Init(CheckBoxType.LCDRadio);
            rbHard.X = rbNormal.Right + DoubleSpacing;
            rbHard.Text = "Hard";
            rbHard.Width = radioWidth;
            */

        }

        /// <summary>
        /// shrinks the columns
        /// </summary>
        private void ArrangeColumns()
        {
           
            Label lblName, lblScore;
            ComboBox cbDifficulty, cbOptions;
            CheckBox cbRandom;

            // in this loop some header elements are repositioned more than once.
            foreach (var item in grdOptions.Entries)
            {
                lblName = (Label)item.FindChildById(UIComponent.DataControlID.Caption);
                lblName.Width = nameColumnWidth;

                cbDifficulty = (ComboBox)item.FindChildById(UIComponent.DataControlID.Difficulty);
                cbDifficulty.X = lblName.Right + 6;

                columnDivider1.X = cbDifficulty.Right + grdOptions.X + 10;

                cbRandom = (CheckBox)item.FindChildById(UIComponent.DataControlID.Randomize);
                cbRandom.X = columnDivider1.Right;

                lblExtraOptions.X = cbRandom.X + grdOptions.X + 4;

                //lblError.X = lblExtraOptions.Right + 12;

                cbOptions = (ComboBox)item.FindChildById(UIComponent.DataControlID.Option);
                cbOptions.X = cbRandom.Right + 6;

                columnDivider2.X = cbOptions.Right + grdOptions.X + 4;

                /*
                lblScore = (Label)item.FindChildById(UIComponent.DataControlID.Score);
                lblScore.X = columnDivider2.Right;

                lblScoreHeader.X = lblScore.X + grdOptions.X;*/
                                
            }

          
        }

        private void SetPanelDimensions()
        {
            // resize:
            int widthToSet = GetWidthForContentSize();
            
            SetContentWidth(widthToSet);
            SetContentHeight();

            // set the size of the lcd screen and the panel around it:
            SetLCDAndWindowDimensions();

            
         //   FullLCDPanel.SetFullsizeLCDScreenWidthFromContent(Form, this.displayBoxOptions, this.lcdScreenOptions, MarginX, lcdSurfaceOptions.Width);

        }

        private void SetContentHeight()
        {
            if (pnCustom.Panel.Bottom < 586) // if over this limit, don't make the screen higher. scroll the content 
            {
                int bottom = pnCustom.Panel.Bottom; 

                optionsSurfaceGrid.Height = bottom + 72; // leave room for the error message to appear underneath the options

                lcdSurfaceOptions.Height = optionsSurfaceGrid.Height;
            }
        }

        private int GetWidthForContentSize()
        {
            int mainDifficultyRight = ((RadioButton) mainDifficultyRadioGroup.Controls.FindLast(c => true)).Right + 6;

            int optionsRight = pnCustom.HorizontalContentPadding + scoreHeaderRight /* lblScoreHeader.Right*/ + 21;

            int contentWidth = Math.Max(mainDifficultyRight, optionsRight);


            int width = Common.Clamp(contentWidth, minimumContentWidth, maximumContentWidth);

            return width;

        }

        private void SetContentWidth(int width)
        {
            
            // set the content containers from the inside out:

            grdOptions.Width = width; 

            pnCustom.ContentWidth = grdOptions.Width;
          
            optionsSurfaceGrid.SetContentWidth(pnCustom.Panel.Width);                        

            lcdSurfaceOptions.Width = optionsSurfaceGrid.Width;

           // taDifficulty.Width = optionsSurfaceGrid.SurfaceWidth;

        }

        private void SetLCDAndWindowDimensions()
        {
            FullLCDPanel.SetFullsizeLCDScreenDimensionsFromContent(Window, this.displayBoxOptions, this.lcdScreenOptions, MarginX, lcdSurfaceOptions.Width, lcdSurfaceOptions.Height);

            btStart.X = displayBoxOptions.Right - btStart.Width + 3;

            // the window has 1 CRT and 2 lcd screens on it:
            Window.Height = displayBoxOptions.Bottom + BottomMarginForButtons;  // leave room for buttons 

            PlaceLeftButtonUnderLCD(btCancel);
            PlaceRightButtonUnderLCD(btStart);
           
            /*int buttonY = displayBoxOptions.Bottom + 4;
            btStart.Y = buttonY;
            btCancel.Y = buttonY;*/
        }

        void rbDifficultySetting_Click(UIComponent sender, EventArgs e)
        {
            ShowDifficulty(((DifficultyEventArgs)e).Difficulty);

            RemoveCustomControls();
        }

        private void RemoveCustomControls()
        {
            optionsSurfaceGrid.BeginAddingEntries();

            optionsSurfaceGrid.TryRemoveEntry(pnCustom.Panel);
            optionsSurfaceGrid.TryRemoveEntry(errorPaddingBox);
            
            lblError.Text = "";

            optionsSurfaceGrid.EndAddingEntries();

        }


        private void ShowDifficulty(Difficulty difficulty)
        {
            if (!optionsSurfaceGrid.EntriesByKey.ContainsKey(taDifficulty))
            {
                optionsSurfaceGrid.AddEntry(taDifficulty, taDifficulty);
                taDifficulty.CanHaveFocus = false; // don't make the lines selctable, needed because the container grid can have focus

            }

            taDifficulty.Text = difficulty.Description;

        }

    /*    void rbCustomDifficultySetting_Click(UIComponent sender, EventArgs e)
        {
            RadioButton rbDifficulty = sender as RadioButton;
            CustomDifficultyEventArgs args = sender.EventArgs as CustomDifficultyEventArgs;

            OptionSet optionsSet = args.OptionSet;
            string customDifficultyKey = args.CustomDifficultyKey;
           
            // get the grid row:
            UIComponent row;
            grdOptions.TryGetItem(optionsSet, out row);

            // get the controls:
            ImageButton cbRandomize = (ImageButton)row.FindChildById(UIComponent.DataControlID.Randomize);
            ComboBox cbOptions = (ComboBox)row.FindChildById(UIComponent.DataControlID.Option);

           
            PopulateExtraOptionSettings(optionsSet, customDifficultyKey, cbRandomize, cbOptions);
        }*/

        /// <summary>
        /// see if this selected difficulty has more than one option, and enable/diable the selection controls accordingly
        /// </summary>
        /// <param name="optionSet"></param>
        /// <param name="customDifficultyKey"></param>
        /// <param name="cbRandomize"></param>
        /// <param name="cbOptions"></param>
        private static void PopulateExtraOptionSettings(OptionSet optionSet, string customDifficultyKey, CheckBox cbRandomize, ComboBox cbOptions)
        {
            var optionsWithThisDifficulty = optionSet.OptionsGroupedByDifficulty.First(g => g.Key == customDifficultyKey);

            /*
              when extra options are available, default state should be none selected. both randomize and dropdown should be in OUT state (Not IN or INACTIVE). 
             * clicking START will then prompt the player to make a selection, making him aware of both options, both the randomize and the drop-down
             */

            if (optionsWithThisDifficulty.Count() > 1) // optionsGroupedByDifficulty.Count() > 1)
            {
               
                cbRandomize.Enabled = true;
                cbRandomize.IsChecked = false; // true;
                cbRandomize.Visible = true;

                cbOptions.Enabled = true; // false;
                cbOptions.Visible = true;

                PopulateOptionSelector(optionsWithThisDifficulty, cbOptions);

            }
            else
            {
                /* cbRandomize.Visible = false;
                 cbOptions.Visible = false;*/

                cbRandomize.Enabled = false;
                cbRandomize.IsChecked = false;
                cbRandomize.DebugTag = "disabledcb";
                cbRandomize.Visible = false;

                cbOptions.Enabled = false;
                cbOptions.Clear();
                cbOptions.Visible = false;
            }
        }
      
        void rbCustom_Click(UIComponent sender, EventArgs e)
        {
            if (rbCustom.IsChecked)
            {
                if (!optionsSurfaceGrid.EntriesByKey.ContainsKey(pnCustom.Panel))
                {
                    pnCustom.Panel.Y = mainDifficultyYPos + DoubleSpacing;

                   
                    optionsSurfaceGrid.BeginAddingEntries();

                    optionsSurfaceGrid.TryRemoveEntry(taDifficulty);

                    // the panel has already been populated.
                    optionsSurfaceGrid.AddEntry(pnCustom.Panel, pnCustom.Panel);
                    optionsSurfaceGrid.AddEntry(errorPaddingBox, errorPaddingBox);


                    optionsSurfaceGrid.EndAddingEntries();
                }
            }
            else
            {
                RemoveCustomControls();
            }
        }


        private bool HasCustomOptions()
        {
            if (screen.Scenario.ScenarioData.CustomEnabled)
            {
                foreach (var item in screen.Scenario.ScenarioData.OptionSets)
                {
                    if (item.Options.Length > 1)
                    {
                        return true;
                    }
                }
            }

            return false;

        }



        int nameColumnWidth = 80; // min width
        const int difficultyWidth = 180; // fixed width
        int randomizeWidth = 60;

        private void AddOptionSetRow(OptionSet optionSet)
        {
            if (optionSet.Options.Length < 2)
            {
                return; // don't show options when there is no choice to be made
            }

           
            UIComponent item;

            item = new UIComponent(Interface.gui);
            grdOptions.AddEntry(optionSet, item);

            int styleToUse = optionSet.DisplayGroup % 3;
            Label.LabelType labelStyle;
            switch (styleToUse)
            {
                case 0:
                    labelStyle = Label.LabelType.LCDHeadingBlue;
                    break;
                case 1:
                    labelStyle = Label.LabelType.LCDHeadingGreen;
                    break;
                case 2:
                    labelStyle = Label.LabelType.LCDHeadingRed;
                    break;
                default: 
                    labelStyle = Label.LabelType.LCDHeadingBlue;
                    break;
            }

            Label lblName = new Label(Interface.gui);
            item.Add(lblName);
            lblName.Init(labelStyle); // Label.LabelType.LCDNormal);
            lblName.Text = optionSet.Name.ToUpper(Config.Culture);
            lblName.Position = new Point(0, 0);
           // lblName.Width = difficultyColumnX - lblName.X - SingleSpacing; 
            lblName.FitToText();
            lblName.ToolTip = optionSet.Description;
            item.CenterChildVertically(lblName);
            lblName.Y += 2;
            lblName.ID = UIComponent.DataControlID.Caption;
            nameColumnWidth = Math.Max(nameColumnWidth, lblName.Width);

            ComboBox cbDifficulty = new ComboBox(Interface.gui, ListBoxType.LCDCombo /* .HUDAndLCD*/, false);
            item.Add(cbDifficulty);
            cbDifficulty.Init(ComboBoxTypes.LCD);
            cbDifficulty.ID = UIComponent.DataControlID.Difficulty;
            cbDifficulty.X = difficultyColumnX;
            //cbDifficulty.Y = itemPadding;
            //cbDifficulty.IsEditable = false;
            cbDifficulty.Width = difficultyWidth;          
            cbDifficulty.Tag1 = optionSet;
            item.CenterChildVertically(cbDifficulty);
           

          /*  cbDifficulty.EventArgs = new CustomDifficultyEventArgs()
            {
                OptionSet = optionSet,
                CustomDifficultyKey = difficulty.Key
            };*/


            Option firstOptionInGroup;

            Difficulty[] mainDifficulties = screen.Scenario.ScenarioData.MainDifficultySettings;

            string selectedDifficultyKey = null;

              // group the options by difficulty (key name) and list the unique difficulty choices:
            foreach (var customDifficulty in optionSet.OptionsGroupedByDifficulty)
            {
                firstOptionInGroup = customDifficulty.First();
                Difficulty mainDifficulty = mainDifficulties.First(d => d.KeyName == firstOptionInGroup.Difficulty.KeyName);

                // allow the diffuclty name to be overridden by the option:
                string difficultyName = firstOptionInGroup.Difficulty.Name ?? mainDifficulty.Name; // mainDifficulties.First(d => d.KeyName == firstOptionInGroup.Difficulty.KeyName).Name;

                cbDifficulty.AddEntry(customDifficulty.Key, difficultyName.ToUpper(Config.Culture));

                if (firstOptionInGroup.Difficulty.IsDefault)
                {                   
                    selectedDifficultyKey = customDifficulty.Key;
                   
                }

            }

            // set default selection:
            if (selectedDifficultyKey == null)
            {
                cbDifficulty.SelectedIndex = 0;
                selectedDifficultyKey = (string)cbDifficulty.SelectedKey;
            }
            else
            {
                cbDifficulty.SelectedKey = selectedDifficultyKey;              
            }

           

            // now we can set event handler without invoking it:
            cbDifficulty.SelectionChanged += new SelectionChangedHandler(cbDifficulty_SelectionChanged);

         
            // selecting a difficulty:
            // if more than one option has the selected difficulty - then show the random checkbox
            // if only one option has the difficulty - then hide the random checkbox
           
            CheckBox cbRandomize = new CheckBox(Interface.gui);
            item.Add(cbRandomize);
            cbRandomize.Init(CheckBoxType.LCD);
            cbRandomize.X = cbDifficulty.Right + DoubleSpacing; // buttonGroup.Right;
        //    cbRandomize.Y = itemPadding;
            cbRandomize.Text = "RANDOMIZE";
            cbRandomize.IsChecked = true;
            cbRandomize.EventArgs = new CustomDifficultyRandomizeEventArgs() { OptionSet = optionSet };
            cbRandomize.Click += new ClickHandler(cbRandomize_Click);
            cbRandomize.ID = UIComponent.DataControlID.Randomize;
            item.CenterChildVertically(cbRandomize);
            cbRandomize.Y += 1;
            cbRandomize.FitToText();
            randomizeWidth = cbRandomize.Width;

            // add randomize checkbox/drop down list:
         /*   ImageButton cbRandomize = new ImageButton(Interface.gui);
            item.Add(cbRandomize);
            cbRandomize.Init(ImageButtonType.LCDCheckbox);
            cbRandomize.X = cbDifficulty.Right + DoubleSpacing; // buttonGroup.Right;
            cbRandomize.Y = itemPadding;
            cbRandomize.IsChecked = true;
            cbRandomize.EventArgs = new CustomDifficultyRandomizeEventArgs() { OptionSet = optionSet };
            cbRandomize.Click += new ClickHandler(cbRandomize_Click);         
            cbRandomize.ID = UIComponent.DataControlID.Randomize;
            */

            ComboBox cbOptions = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            item.Add(cbOptions);
            cbOptions.Init(ComboBoxTypes.LCD);
            cbOptions.ID = UIComponent.DataControlID.Option;
            cbOptions.X = selectionColumnX;
            item.CenterChildVertically(cbOptions);
            cbOptions.Width = selectionColumnWidth;

            PopulateExtraOptionSettings(optionSet, selectedDifficultyKey, cbRandomize, cbOptions);

           
            /*
            Label lblScore = new Label(Interface.gui);
            item.Add(lblScore);
            lblScore.Init(Label.LabelType.LCDNormal);
            lblScore.ID = UIComponent.DataControlID.Score;
            lblScore.Text = "N/A";
            lblScore.Position = new Point(scoreColumnX, itemPadding);
            item.CenterChildVertically(lblScore);
            lblScore.Width = 100;
           */

        }

        void cbDifficulty_SelectionChanged(UIComponent sender)
        {
            ComboBox cbDifficulty = sender as ComboBox;
           /* CustomDifficultyEventArgs args = sender.EventArgs as CustomDifficultyEventArgs;

            OptionSet optionsSet = args.OptionSet;*/
            OptionSet optionsSet = (OptionSet)cbDifficulty.Tag1;

            string customDifficultyKey = (string)cbDifficulty.SelectedKey; // args.CustomDifficultyKey;

            // get the grid row:
            UIComponent row;
            grdOptions.TryGetEntry(optionsSet, out row);

            // get the controls:
            //ImageButton cbRandomize = (ImageButton)row.FindChildById(UIComponent.DataControlID.Randomize);
            CheckBox cbRandomize = (CheckBox)row.FindChildById(UIComponent.DataControlID.Randomize);
           
            ComboBox cbOptions = (ComboBox)row.FindChildById(UIComponent.DataControlID.Option);


            PopulateExtraOptionSettings(optionsSet, customDifficultyKey, cbRandomize, cbOptions);

        }

        private static void PopulateOptionSelector(/*OptionSet optionSet */ IGrouping<string, Option> optionsForSelectedDifficulty, ComboBox cbOptions)
        {
            cbOptions.Clear();

           // var optionsForSelectedDifficulty = optionSet.OptionsGroupedByDifficulty.First(o => o.Key == selectedDifficultyKey);
            foreach (var option in optionsForSelectedDifficulty) // .Options)
            {
                cbOptions.AddEntry(option.KeyName, option.Name);
            }
        }

        private void ShowRandomOption()
        {


        }

        void cbRandomize_Click(UIComponent sender, EventArgs e)
        {
           // ImageButton checkbox = sender as ImageButton;
            CheckBox checkbox = sender as CheckBox;
            CustomDifficultyRandomizeEventArgs args = sender.EventArgs as CustomDifficultyRandomizeEventArgs;

            OptionSet optionsSet = args.OptionSet;

            // get the grid row:
            UIComponent row;
            grdOptions.TryGetEntry(optionsSet, out row);

            // get the drop down control:
            ComboBox cbOptions = (ComboBox)row.FindChildById(UIComponent.DataControlID.Option);
                      
           
            // show/hide it:
            if (!checkbox.IsChecked)
            {
                cbOptions.Enabled = true;

                if (cbOptions.Count == 0)
                {
                    ComboBox cbDifficulty = (ComboBox)row.FindChildById(UIComponent.DataControlID.Difficulty);
                    string selectedDifficultyKey = (string)cbDifficulty.SelectedKey;

                    // find the selected radio button, and the difficulty key
                   /* RadioGroup radioGroup = (RadioGroup)row.FindChildById(UIComponent.DataControlID.Difficulty);
                    RadioButton selectedRadioButton = radioGroup.GetSelected();
                    CustomDifficultyEventArgs radioArgs = selectedRadioButton.EventArgs as CustomDifficultyEventArgs;
                    string selectedDifficultyKey = radioArgs.CustomDifficultyKey;
                    */

                    PopulateOptionSelector(
                        optionsSet.OptionsGroupedByDifficulty.First(g => g.Key == selectedDifficultyKey), 
                        cbOptions);
                }

                //cbOptions.Visible = true;              
            }
            else
            {
                cbOptions.Enabled = false;
                // clear the combobox when disabled:
                cbOptions.Clear();

                //cbOptions.Visible = false;
            }
        }

        void tbSelect_Click(UIComponent sender, EventArgs e)
        {
            
        }

        private void Populate()
        {
           
           
        }

        private void PopulateCustomOptions()
        {
           // optionsSurfaceGrid.BeginAddingEntries();

            grdOptions.BeginAddingEntries();
            grdOptions.Clear();

            foreach (var item in screen.Scenario.ScenarioData.OptionSets)
            {
                AddOptionSetRow(item);
            }

            grdOptions.EndAddingEntries();

           // optionsArePopulated = true;

        //    optionsSurfaceGrid.EndAddingEntries();

            //space the columns to best fit:
            ArrangeColumns();
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

            PopulateMainDifficulty();
            
           // Populate();
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
    }
}
