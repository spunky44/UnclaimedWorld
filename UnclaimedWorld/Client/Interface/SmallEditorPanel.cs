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
////using Microsoft.Xna.Framework.Storage;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Trees;
using GameStateManagement;
using UWGame.ClientSide.Interface;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances;
using UWGame.Control;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.ClientSide.Interface
{
    public class SidePanelEditorEntity : RosterPanel //SidePanel
    {
        Grid outerGrid;

        public EntityType SelectedEntityType;

        Label lblStatusInfo1, lblStatusInfo2;

        CollapsablePanel cpTerrain, cpTrees; //, cpCritters;
        Grid terrainGrid, treeGrid; //, critterGrid;

        MapEditorSaveLoadPanel saveDialog; //, loadDialog;

        CheckBox /*cbDrawIds, cbDrawCoords,*/ cbDelete, cbFlipHorizontally, cbYoung, cbGrown, cbSummer, cbWinter, cbRandom, cbSmallBrush, cbBigBrush;

        int buttonYPos;

      /*  public bool RenderIds
        {
            get { return cbDrawIds.IsChecked; }
        }
            

        public bool PrintCoords
        {
            get { return cbDrawCoords.IsChecked; }
        }*/

        public SidePanelEditorEntity()
            : base(The.InGameUI.sidePanelFullHeight, true, 150)
        {

          //  FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(The.InGameUI, Form, 150, new Point(MarginX, MarginX), out display, /*out edges,*/ out lcdSurface, ref lcdScreen);

           // AddOverlayDetails(/*edges,*/ );

            int itemHeight = ItemHeight;

            outerGrid = CreateOuterGridForCollapsableLists(The.InGameUI.gui, lcdSurface);

           /* AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Critters", itemHeight, out cpCritters, out critterGrid);
            critterGrid.SelectedChanged += new SelectionChangedHandler(critterGrid_SelectedChanged);
            */
            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Terrain", itemHeight, out cpTerrain, out terrainGrid);
            terrainGrid.SelectedChanged += new SelectionChangedHandler(terrainGrid_SelectedChanged);
            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Trees", itemHeight, out cpTrees, out treeGrid);
            treeGrid.SelectedChanged += new SelectionChangedHandler(treeGrid_SelectedChanged);

            PopulateGrid(); // only do this once...

            InitStatusContentPanel();


            InitCheckBoxes();

            TextButton btSave, btLoad, btClear;
            AddBottomButtonInSequence(null, out btSave, "SAVE!", "Saves the map.");
            btSave.Click += new ClickHandler(saveButton_Click);

            // disabled for now, crashy:
          /*  AddBottomButtonInSequence(btSave, out btLoad, "LOAD!", "Loads a new map.");
            btLoad.Click += new ClickHandler(btLoad_Click);*/

            AddBottomButtonInSequence(btSave, out btClear, "CLEAR!", "Clears the contents of the tile you click on.");
            btClear.Click += new ClickHandler(btClear_Click);

            buttonYPos = btSave.Y;

            /*   btClear = new TextButton(intf.gui);
               Form.Add(btClear); // add first!!! sets defaults!
               btClear.Init(TextButton.TextButtonType.Brown);
               btClear.Position = new Point(brown.X, cbFlipHorizontally.Y + 28);
               btClear.Text = "CLEAR";
               btClear.ToolTip = "Clears the contents of the tile you click on.";
               btClear.Click += new ClickHandler(btClear_Click);
               */

            int x, y;

            int yTopRow = buttonYPos - 86;

            /*
            cbDrawIds = new CheckBox(Interface.gui);
            cbDrawIds.Text = "IDS";
            cbDrawIds.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
            cbDrawIds.Label.NormalColor = Color.Black;
            cbDrawIds.Width = 80;
            x = display.X;
            y = yTopRow;
            Window.Add(cbDrawIds);
            cbDrawIds.Position = new Point(x, y);
            */


          /*  cbDrawResources = new CheckBox(Interface.gui);
            cbDrawResources.Text = "Res.";
            cbDrawResources.Init(CheckBoxType.LED, CheckBoxFlavor.Red);
            cbDrawResources.Label.NormalColor = Color.Black;
            cbDrawResources.Width = 80;
            x = cbDrawIds.Right;
            y = yTopRow;
            Window.Add(cbDrawResources);
            cbDrawResources.Position = new Point(x, y);
            cbDrawResources.Click += new ClickHandler(cbDrawResources_Click);
            */

            /*
            cbDrawCoords = new CheckBox(Interface.gui);
            cbDrawCoords.Text = "Coords";
            cbDrawCoords.Init(CheckBoxType.LED, CheckBoxFlavor.Blue);
            cbDrawCoords.Label.NormalColor = Color.Black;
            cbDrawCoords.Width = 80;
            x = cbDrawIds.Right; // cbDrawResources.Right;
            y = yTopRow;
            Window.Add(cbDrawCoords);
            cbDrawCoords.Position = new Point(x, y);
            cbDrawCoords.Click += new ClickHandler(cbDrawCoords_Click);
            */

            cbDelete = new CheckBox(Interface.gui);
            cbDelete.Text = "DEL";
            cbDelete.Init(CheckBoxType.LED, CheckBoxFlavor.Red);
            cbDelete.Label.NormalColor = Color.Black;
            cbDelete.Width = 80;
            cbDelete.Click += new ClickHandler(cbDelete_Click);
            x = display.X; // edges.X; // edges.Position.X + 130;
            y = yTopRow; // cbDrawCoords.Bottom + 30; // yTopRow;
            Window.Add(cbDelete);
            cbDelete.Position = new Point(x, y);


            saveDialog = new MapEditorSaveLoadPanel(MapEditorSaveLoadPanel.SaveOrLoad.Save, Interface, Point.Zero);
          //  loadDialog = new MapEditorSaveLoadPanel(MapEditorSaveLoadPanel.SaveOrLoad.Load, Interface, Point.Zero);

           // loadDialog.SaveOrLoadClick += new EventHandler(loadDialog_SaveOrLoadClick);
            saveDialog.SaveOrLoadClick += new EventHandler(saveDialog_SaveOrLoadClick);
        }

        void cbDrawCoords_Click(UIComponent sender, EventArgs e)
        {
          /*  if (cbDrawCoords.IsChecked)
            {
                cbDrawResources.IsChecked = false;
            }*/
        }

        void cbDrawResources_Click(UIComponent sender, EventArgs e)
        {
          /*  if (cbDrawResources.IsChecked)
            {
                cbDrawCoords.IsChecked = false;
            }*/
        }

        void cbDelete_Click(UIComponent sender, EventArgs e)
        {
            if (cbDelete.IsChecked)
            {
                The.InGameUI.DestroyEntityBeingPlaced();

                intface.InterfaceMode = InGameInterface.InterfaceState.EditorDeleteEntities;

            }
            else
            {
                if (SelectedEntityType != null)
                {
                    intface.InterfaceMode = InGameInterface.InterfaceState.EditorPlaceEntity;

                    CreateEntityForPlacement(SelectedEntityType);
                }
                else
                {
                    intface.InterfaceMode = InGameInterface.InterfaceState.None;
                }
            }
        }

        void saveDialog_SaveOrLoadClick(object sender, EventArgs e)
        {

        }

      /*  void loadDialog_SaveOrLoadClick(object sender, EventArgs e)
        {         
           // The.Sim.LoadEditorMap(loadDialog.MapDataXmlPath);

            if (The.Sim.PlaySite != null)
            {
                MapLoader mapLoader = new MapLoader(loadDialog.MapDataXmlPath);

                // TODO: this won't work because the editor site and allegiance is not destroyed and recreated.
                // so it crashes with out of bounds when going from a small map to a larger map.
                // right now, only possible to load edit from main menu

                // TODO: need to cycle this:
                bool result;
                do
                {
                    result = mapLoader.QueueLoad();
                }
                while (result == false);
               // The.Map.LoadMap(loadDialog.MapDataXmlPath);

                The.Sim.PostLoadMap();
            }
        }*/

        void cbYoungTree_Click(UIComponent sender, EventArgs e)
        {
            cbGrown.IsChecked = false;
        }



        void btClear_Click(UIComponent sender, EventArgs e)
        {
            intface.InterfaceMode = InGameInterface.InterfaceState.EditorClearTile;
        }

       /* void btLoad_Click(UIComponent sender, EventArgs e)
        {
            loadDialog.ShowDialog(true);
        }*/

        void cbFlipHorizontally_Click(UIComponent sender, EventArgs e)
        {
            if (intface.EntitiesBeingPlaced != null)
            {
                foreach (InGameInterface.EntityPosition ep in intface.EntitiesBeingPlaced)
                {
                    FlipEntityIfChosen(ep.Entity);
                }
            }
        }

        private void InitCheckBoxes()
        {
            cbFlipHorizontally = new CheckBox(Interface.gui);
            //  Form.Add(cbFlipHorizontally);
            cbFlipHorizontally.Text = "FLIPPED";
            cbFlipHorizontally.Init(CheckBoxType.LED, CheckBoxFlavor.Purple);
            //  cbFlipHorizontally.Position = new Point(edges.Position.X, edges.Y + edges.Height + 5);
            cbFlipHorizontally.Click += new ClickHandler(cbFlipHorizontally_Click);
            cbFlipHorizontally.Label.NormalColor = Color.Black;
            cbFlipHorizontally.Width = 100;

            // options.Add(cbFlipHorizontally.Text, cbFlipHorizontally);



            cbYoung = new CheckBox(Interface.gui);
            //   Form.Add(cbYoungTree);
            cbYoung.Text = "YOUNG";
            cbYoung.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
            //   cbYoungTree.Position = new Point(cbFlipHorizontally.Position.X + 80, cbFlipHorizontally.Y);
            cbYoung.Click += new ClickHandler(cbYoungTree_Click);
            cbYoung.Label.NormalColor = Color.Black;
            cbYoung.Width = 80;

            cbGrown = new CheckBox(Interface.gui);
            cbGrown.Text = "GROWN";
            cbGrown.Click += new ClickHandler(cbGrownTree_Click);
            cbGrown.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
            cbGrown.Label.NormalColor = Color.Black;
            cbGrown.Width = 80;

            cbSummer = new CheckBox(Interface.gui);
            cbSummer.Text = "SUMR";
            cbSummer.Click += new ClickHandler(cbSummer_Click);
            cbSummer.Init(CheckBoxType.LED, CheckBoxFlavor.Blue);
            cbSummer.Label.NormalColor = Color.Black;
            cbSummer.Width = 80;

            cbWinter = new CheckBox(Interface.gui);
            cbWinter.Text = "WITR";
            cbWinter.Click += new ClickHandler(cbWinter_Click);
            cbWinter.Init(CheckBoxType.LED, CheckBoxFlavor.Blue);
            cbWinter.Label.NormalColor = Color.Black;
            cbWinter.Width = 80;

            //   options.Add(cbYoungTree.Text, cbYoungTree);

            cbRandom = new CheckBox(Interface.gui);
            cbRandom.Text = "RANDOM";
            cbRandom.Init(CheckBoxType.LED, CheckBoxFlavor.Red);
            //    cbRandom.Click += new ClickHandler(cbRandom_Click);
            cbRandom.Label.NormalColor = Color.Black;
            cbRandom.Width = 100;

            cbSmallBrush = new CheckBox(Interface.gui);
            cbSmallBrush.Text = "S BRUSH";
            cbSmallBrush.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
            cbSmallBrush.Click += new ClickHandler(cbSmallBrush_Click);
            cbSmallBrush.Label.NormalColor = Color.Black;
            cbSmallBrush.Width = 100;

            cbBigBrush = new CheckBox(Interface.gui);
            cbBigBrush.Text = "L BRUSH";
            cbBigBrush.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
            cbBigBrush.Click += new ClickHandler(cbBigBrush_Click);
            //    cbRandom.Click += new ClickHandler(cbRandom_Click);
            cbBigBrush.Label.NormalColor = Color.Black;
            cbBigBrush.Width = 100;
        }



        void cbBigBrush_Click(UIComponent sender, EventArgs e)
        {
            cbSmallBrush.IsChecked = false;
        }

        void cbSmallBrush_Click(UIComponent sender, EventArgs e)
        {
            cbBigBrush.IsChecked = false;
        }

        void cbGrownTree_Click(UIComponent sender, EventArgs e)
        {
            cbYoung.IsChecked = false;
        }

        void cbWinter_Click(UIComponent sender, EventArgs e)
        {
            cbSummer.IsChecked = false;
        }

        void cbSummer_Click(UIComponent sender, EventArgs e)
        {
            cbWinter.IsChecked = false;
        }


        Dictionary<string, CheckBox> options = new Dictionary<string, CheckBox>();
        private void UpdateCheckBoxes()
        {
            foreach (KeyValuePair<string, CheckBox> item in options)
            {
                Window.Remove(item.Value);
            }

            options.Clear();

            if (intface.EntitiesBeingPlaced != null && intface.EntitiesBeingPlaced.Count > 0)
            {
                Entity representative = intface.EntitiesBeingPlaced[0].Entity;

                options.Add(cbFlipHorizontally.Text, cbFlipHorizontally);

                if (representative.EntityType.TreeType != null ||
                    representative.EntityType.BiologicalType != null)
                {
                    options.Add(cbYoung.Text, cbYoung);
                    options.Add(cbGrown.Text, cbGrown);
                }

                if (representative.EntityType.TreeType != null)
                {
                    if (representative.EntityType.TreeType.HasSummerWinterCycle)
                    {
                        options.Add(cbSummer.Text, cbSummer);
                        options.Add(cbWinter.Text, cbWinter);
                    }

                }

                if (representative.EntityType.TerrainType != null)
                {
                    options.Add(cbRandom.Text, cbRandom);
                }

                options.Add(cbSmallBrush.Text, cbSmallBrush);
                options.Add(cbBigBrush.Text, cbBigBrush);
            }
            

            int x, y;
            x = display.X; 
            y = buttonYPos - 40;
            foreach (KeyValuePair<string, CheckBox> item in options) // placed from the bottom up
            {
                Window.Add(item.Value);
                item.Value.Position = new Point(x, y);

                x += item.Value.Width - 15;
                if (x > display.Right - 60)
                {
                    x = display.X;
                    y -= 22;
                }
            }
        }


        private void InitStatusContentPanel()
        {
            statusContent = The.InGameUI.StatusScreen.GetNewSurfaceContent();

            Game game = The.Sim.Controller.Game;
            GUIManager gui = The.InGameUI.gui;

            InitStatusImage();

            InitBillboardPanel();

            /*
            imStatusBackground = new Image(gui);            
            statusContent.Add(imStatusBackground);
            imStatusBackground.Position = new Point(0, 2);
            imStatusBackground.ResizeToFit();
            imStatusBackground.RenderType = RenderType.CRTAndLCD;
            imStatusBackground.Alpha = 0.7f; // 0.35f;// use for background for text!
            */

            InitStatusCRTHeader(gui, statusContent, statusTextX, out lblStatusHeading, out crtUnderline);

            /*
            lblStatusHeading = new Label(gui);
            statusContent.Add(lblStatusHeading);
            lblStatusHeading.Position = new Point(statusTextX, 14);
       //     lblStatusHeading.Text = "A. NKBELE MBUTU";
            lblStatusHeading.Init(Label.LabelType.CRTNormal); // Label.LabelType.CRTGlow);
            lblStatusHeading.Width = statusTextWidth;
            lblStatusHeading.Height = 24;
            lblStatusHeading.DebugTag = "CRTStatusHeader";
            */

            lblStatusInfo1 = new Label(gui);
            statusContent.Add(lblStatusInfo1);
            lblStatusInfo1.Position = new Point(statusTextX, 80);
            //   lblStatusInfo1.Text = "MALE";
            lblStatusInfo1.Init(Label.LabelType.CRTSmall);
            lblStatusInfo1.Width = statusTextWidth;

            lblStatusInfo2 = new Label(gui);    
            statusContent.Add(lblStatusInfo2);
            lblStatusInfo2.Position = new Point(statusTextX, 100);
            //   lblStatusInfo2.Text = "AGE: 34";
            lblStatusInfo2.Init(Label.LabelType.CRTSmall);
            lblStatusInfo2.Width = statusTextWidth;

        }

        private void RefreshStatusScreen()
        {
            if (SelectedEntityType != null)
            {
                if (SelectedEntityType.RenderableTypeMode != null
                    && SelectedEntityType.RenderableTypeMode.DefaultClientState != null
                    && SelectedEntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType != null)
                {
                    //DisplayScaledBillboardImage(statusContent, imStatusBackground, selectedEntityType, StatusScreen.HeightOfStatusImage, true);
                    ShowBillboardPanel();

                    CreateAndPlaceBillboards(intface.gui, pnBillboards, SelectedEntityType, statusBillboardPanelCenter,
                        StatusScreen.HeightOfStatusImage, true); //true);

                    SetHeaderText(SelectedEntityType.Name.ToUpper(Config.Culture));

                    // lblStatusInfo1.Text = "WORK REQ.: " + selectedEntityType.StructureType.ManSecondsOfWorkNeeded.ToString();
                }
            }

        }



        private void PopulateGrid()
        {
            /*outerGrid.BeginAddingEntries();
            outerGrid.Clear();

            Image icon;
            TextButton expand;
            Label label;
           
            CollapsablePanel cpCategory;
            Grid categoryGrid = null;
            UIComponent item;
            Rectangle clampedRect;*/


            // AllTerrainFeatureTypes

            outerGrid.BeginAddingEntries();
            terrainGrid.BeginAddingEntries();

            foreach (KeyValuePair<string, EntityType> terrainType in GameData.Instance.AllTerrainFeatureTypes)
            {
                if (terrainType.Value.TerrainType.CanBeMapEditorPlaced)
                {
                    terrainGrid.AddEntry(terrainType.Value, terrainType.Value.Name);
                }
            }


            terrainGrid.EndAddingEntries();

            treeGrid.BeginAddingEntries();
            foreach (KeyValuePair<string, EntityType> treeType in GameData.Instance.AllTreeTypes)
            {
                /* if (terrainType.Value.TerrainType.CanBeMapEditorPlaced)
                 {*/
                treeGrid.AddEntry(treeType.Value, treeType.Value.Name);
                // }
            }

            treeGrid.EndAddingEntries();

        /*    critterGrid.BeginAddingEntries();
            foreach (KeyValuePair<string, EntityType> entityType in GameData.Instance.AllEntityTypes)
            {
                if (entityType.Value.BiologicalType != null && entityType.Value.Person == null)
                {
                    critterGrid.AddEntry(entityType.Value, entityType.Value.Name);
                }

            }

            critterGrid.EndAddingEntries();*/

            outerGrid.EndAddingEntries();

            /* foreach (KeyValuePair<StructureCategory, List<EntityType>> kvp in Sim.Instance.StructureTypesInCategory)
             {
                 cpCategory = null;

                 if (kvp.Value.Count > 0)
                 {
                     foreach (EntityType structureTypeInCategory in kvp.Value)
                     {
                         if (cpCategory == null)
                         {
                             AddCollapsablePanelAndGrid(outerGrid, kvp.Key.Name, itemHeight, out cpCategory, out categoryGrid);
                             categoryGrid.Selectability = Grid.SelectabilityOptions.Single;
                             categoryGrid.DebugTag = "structureCategory";
                             categoryGrid.BeginAddingEntries();
                             // this is needed for the button to be clickable (it gets hidden under something?)
                             //cpCategory.X = 10;
                         }

                         //item = new UIComponent(intf.gui);

                         categoryGrid.AddEntry(structureTypeInCategory, structureTypeInCategory.Name);

                     }

                     categoryGrid.EndAddingEntries();
                     categoryGrid.SelectedChanged += new SelectionChangedHandler(categoryGrid_SelectedChanged);
                 }
             }

             outerGrid.EndAddingEntries();*/
        }


        //private float 

        public void CreateEntityForPlacement(EntityType entityType)
        {
            Vector2 cursorPlacement = new Vector2();

            List<Vector2> listOfPlacements = new List<Vector2>();

            if (cbSmallBrush.IsChecked)
            {
                listOfPlacements = UniformPoissonDiskSampler.SampleCircle(cursorPlacement, 60f, 40f);
            }
            else if (cbBigBrush.IsChecked)
            {
                listOfPlacements = UniformPoissonDiskSampler.SampleCircle(cursorPlacement, 120f, 40f);
            }
            else
            {
                listOfPlacements.Add(cursorPlacement);
            }

            foreach (Vector2 v in listOfPlacements) // EntityPosition item in )
            {
                if (entityType.TerrainType != null)
                {
                    // pick a different entity type:
                    if (cbRandom.IsChecked || cbSmallBrush.IsChecked || cbBigBrush.IsChecked)
                    {

                        string key = entityType.KeyName;

                        string prefix = GetPrefix(key);

                        entityType = PickRandomTerrainFeatureWithPrefix(prefix);
                    }
                }

                Entity entity = CreateThisEntityForPlacement(entityType);
                intface.EntitiesBeingPlaced.Add(new InGameInterface.EntityPosition() { Entity = entity, Position = v });
            }


            // InitializeEditorPlacedEntity(entity);

        }

        public static string GetPrefix(string key)
        {
            int stopIndex = key.IndexOfAny(stopChars);

            string prefix;
            if (stopIndex > 0)
            {
                prefix = key.Substring(0, stopIndex);
            }
            else
            {
                prefix = key;
            }
            return prefix;
        }

        /// <summary>
        /// create the entity that is placed in the editor.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private Entity CreateThisEntityForPlacement(EntityType entityType)
        {
            Entity entity = new Entity(entityType);

            EditorData editorData = new EditorData(entity);
            entity.Add(editorData);

            Allegiance allegiance = null;

            // don't create allegiances in Edit mode.   
            if (entity.Intelligence != null)
            {
                // just save the key...                              
                editorData.AllegianceKey = "Allegiance #" + entity.EntityID; // (Sim.Instance.Site.Allegiances.Count + 1).ToString();

                /* allegiance = new Allegiance.Allegiance(Allegiance.AllegianceType.Other, entity.EntityType, false)
                 {
                     Name = "Allegiance #" + (Sim.Instance.Site.Allegiances.Count + 1).ToString()
                 };*/
            }

            if (entity.BiologicalEntity != null)
            {
                if (The.Sim.Mode == Sim.EngineMode.Edit)
                {
                    entity.Intelligence.DisableAI = true;
                }

                AIAgeGroup? ageGroup = null;

                if (cbYoung.IsChecked)
                {
                    ageGroup = AIAgeGroup.YoungAdult;
                }
                else if (cbGrown.IsChecked)
                {
                    ageGroup = AIAgeGroup.Adult;
                }

                The.Sim.InitializeBioEntityToPlace(Sim.PersonSex.Male, null, ageGroup, allegiance, entity);
            }
            else
            {
                UWGame.SimSide.Trees.Tree tree;
                if (entity.Find(out tree))
                {
                    float? age = null;
                    if (cbYoung.IsChecked)
                    {
                        age = 0.5f * entity.EntityType.TreeType.MatureAge;

                        // set an age that won't be saved, in order to show the correct sprites
                        tree.SetAgePreInit(age.Value);

                        // this will assign the tree a random young age on game start
                        editorData.TreeAgeGroup = UWGame.SimSide.Trees.AgeGroup.Young;

                    }
                    else if (cbGrown.IsChecked)
                    {
                        age = 1.5f * entity.EntityType.TreeType.MatureAge;

                        // set an age that won't be saved, in order to show the correct sprites
                        tree.SetAgePreInit(age.Value);

                        // this will assign the tree a random young age on game start
                        editorData.TreeAgeGroup = UWGame.SimSide.Trees.AgeGroup.Grown;
                    }
                    else
                    {
                        tree.SetAgePreInit();
                        editorData.TreeAgeGroup = null;
                    }                   


                    if (cbSummer.IsChecked)
                    {
                        tree.InSeason = InSeason.Summer;
                    }
                    else if (cbWinter.IsChecked)
                    {
                        tree.InSeason = InSeason.Winter;
                    }
                    else
                    {
                        if (entityType.TreeType.HasSummerWinterCycle)
                        {
                            if (The.Client.ClientRandomGenerator.Next(2, "SmallEditorPanel", false) == 0)
                            {
                                tree.InSeason = InSeason.Summer;
                            }
                            else
                            {
                                tree.InSeason = InSeason.Winter;
                            }
                        }
                    }
                }

                entity.Initialize(The.Sim.PlaySite, allegiance);
                entity.InitializeModelAndOnScreenFunctionality();
            }

            entity.FlipHorizontally = The.Client.ClientRandomGenerator.Next(2, "SmallEditorPanel", false) == 0;

            FlipEntityIfChosen(entity);

            return entity;
        }

        /* private void InitializeEditorPlacedEntity(Entity entity) //, Allegiance.Allegiance allegiance = null)
         {
            
         }*/



        private static char[] stopChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '_' };

        void terrainGrid_SelectedChanged(UIComponent sender)
        {
            HandleUserSelectedAnEntityType(sender);

        }

        private EntityType PickRandomTerrainFeatureWithPrefix(string prefix)
        {
            List<EntityType> matches = GetMatchingTerrainFeatureTypes(prefix);

            int rand = The.Client.ClientRandomGenerator.Next(matches.Count, "SmallEditorPanel", false);

            return matches[rand];

        }

        public static List<EntityType> GetMatchingTerrainFeatureTypes(string prefix)
        {
            List<EntityType> matches = new List<EntityType>();
            foreach (KeyValuePair<string, EntityType> kvp in GameData.Instance.AllTerrainFeatureTypes)
            {
                if (MatchesPrefix(kvp.Value, prefix))
                {
                    matches.Add(kvp.Value);
                }

            }

            return matches;
        }

        public static bool MatchesPrefix(EntityType entityType, string prefix)
        {
            if (entityType.KeyName == prefix)
            {
                return true;
            }
            else if (entityType.KeyName.StartsWith(prefix))
            {
                string key = entityType.KeyName;

                int stopIndex = key.IndexOfAny(stopChars);

                if (stopIndex > 0)
                {
                    string currentPrefix = key.Substring(0, stopIndex);

                    if (currentPrefix == prefix)
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        void critterGrid_SelectedChanged(UIComponent sender)
        {
            HandleUserSelectedAnEntityType(sender);
            /*
            SelectedEntityType = null;

            Grid clickedGrid = sender as Grid;

            if (clickedGrid != null)
            {
                object key;
                if (clickedGrid.GetKey(clickedGrid.SelectedItem, out key))
                {
                    SelectedEntityType = (EntityType)key;
                }
            }

            RefreshStatusScreen();

            if (SelectedEntityType != null)
            {
                intface.InterfaceMode = InGameInterface.InterfaceState.EditorPlaceEntity;

                CreateEntityForPlacement(SelectedEntityType);


            }
            else
            {
                intface.InterfaceMode = InGameInterface.InterfaceState.None;
            }

            UpdateCheckBoxes();*/
        }

        void treeGrid_SelectedChanged(UIComponent sender)
        {
            HandleUserSelectedAnEntityType(sender);
        }

        private void HandleUserSelectedAnEntityType(UIComponent sender)
        {
            SelectedEntityType = null;

            Grid clickedGrid = sender as Grid;

            if (clickedGrid != null)
            {
                object key;
                if (clickedGrid.GetKey(clickedGrid.SelectedItem, out key))
                {
                    SelectedEntityType = (EntityType)key;
                }
            }

            if (SelectedEntityType != null)
            {
                if (cbDelete.IsChecked)
                {
                    intface.InterfaceMode = InGameInterface.InterfaceState.EditorDeleteEntities;
                }
                else
                {
                    intface.InterfaceMode = InGameInterface.InterfaceState.EditorPlaceEntity;

                    CreateEntityForPlacement(SelectedEntityType);
                }

            }
            else
            {
                intface.InterfaceMode = InGameInterface.InterfaceState.None;
            }

            UpdateCheckBoxes();
        }

        private void FlipEntityIfChosen(Entity entity)
        {
            if (cbFlipHorizontally.IsChecked)
            {
                entity.FlipHorizontally = true;
            }
        }


        void saveButton_Click(UIComponent sender, EventArgs e)
        {
            saveDialog.ShowDialog(true);
        }

        public override void Hide()
        {

            base.Hide();
        }

        public override void Refresh()
        {


            //  PopulateGrid();




        }
    }
}
