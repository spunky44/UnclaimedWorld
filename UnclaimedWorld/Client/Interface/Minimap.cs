using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Maps;
using InputEventSystem;
using UWGame.ClientSide.Map.Water;
using UWGame.SimSide;
using UWGame.ClientSide.Map;
using UWGame.Control;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.AI;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.SimSide.Trees;



namespace UWGame.ClientSide.Interface
{
    public class Minimap
    {

        Image jack, grungeBottomLeft, grungeTopLeft, grungeTopRight, displayDust, buttonDust;
        Box frame;

        //  public bool ShowZones = false;

        public Window DisplayWindow;


        Image minimap;
        Image minimapLocationFrame;


        //   Window buttonPanelWindow;

        CRTScreen crtScreen;

        Regulator mapRegulator = new Regulator(The.Client.ClientRandomGenerator, 0.5, "Minimap");
        Texture2D mapTexture;
        Color[] mapTextureColors, mapBackBuffer;

        Texture2D frameTexture;
        Color[] frameTextureColors;


        // MORTEN FARVER MINIMAP MORTEN

        Color deepWaterColor = new Color(36, 63, 52); // darkest greenish: (36, 63, 52).. MP was (0, 14, 30)...... new Color(45, 72, 102); new Color(50, 94, 145); new Color(90, 90, 90); // new Color(25, 25, 50, 255);
        Color shallowWaterColor = new Color(65, 94, 83);  //dark greenish.. MP was (200, 215, 231)....  new Color(215, 226, 238); new Color(128, 128, 230, 255);

        // 7B9FC8
        Color groundColor = new Color(161, 182, 174); //medium Greenish .. MP was Color.WhiteSmoke......  Color.Transparent;//new Color(0, 8, 19); //new Color(5, 9, 14); new Color(123, 159, 200); // brown: new Color(160, 117, 89);
        Color structureColor = Color.DarkOrchid; //new Color(255, 237, 0);
        Color unitColor = Color.Tomato;//new Color(255, 255, 255);
        Color fovColor = new Color(130, 148, 141);


        int mapWidth;
        int mapHeight;

        float minimapScaleFactorX;
        float minimapScaleFactorY;

        private bool draggingInMap = false;

        int screenWidth, screenHeight;



        public Minimap(int screenWidth, int screenHeight, int screenX)
        {
            // dimensions of the entire window:
            int windowHeight = screenHeight + StatusScreen.GetPlasticFrameHeight(); // 33;
            int windowWidth = GetWidth(screenWidth);

            // dimensions of display:
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;

            GUIManager gui = The.InGameUI.gui;
            Rectangle rect;


            Game game = The.Sim.Controller.Game;


            int cornerSize = 30;
            DisplayWindow = new Window(gui);
            // Sequence matters for skins!!!
            // DisplayWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle("TV_panel"); // empty sprite! 
            DisplayWindow.CornerSize = cornerSize;
            DisplayWindow.Margin = 0; // 7;
            DisplayWindow.Level = Level.Bottom;
            DisplayWindow.Resizable = false;
            DisplayWindow.IsMovable = false;
            DisplayWindow.Position = new Point(screenX, The.Client.Controller.DrawArea.Height - windowHeight);  //GraphicsDevice.Viewport.Height - windowHeight);
            DisplayWindow.WindowSize = new Vector2(windowWidth, windowHeight);  //new Vector2(screenDimensions.Width + 90, screenDimensions.Height + 40);
            DisplayWindow.HasCloseButton = false;
            DisplayWindow.HasCRTOrLCDComponents = true;
            DisplayWindow.HasOverlayComponents = true;
            // DisplayWindow.Show();
            DisplayWindow.Destroyed += DisplayWindow_Destroyed;


            displayDust = new Image(gui);
            rect = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
            displayDust.SetSkinLocation(SkinState.Normal,rect);
            displayDust.Alpha = 0.04f;
            DisplayWindow.Add(displayDust);
            displayDust.Position = new Point(0, 0);
            displayDust.ResizeControlToFitImage();

            //InitMinimizedWindow(screenX, game, gui);


            CreateMap();

            int frameWidth = screenWidth + 24;
            int frameHeight = screenHeight + 24;
            int frameX = 7;
            int frameY = 7; //16;

            minimap = new Image(gui);
            minimap.Texture = mapTexture;
            DisplayWindow.Add(minimap);
            minimap.Position = new Point(frameX + 7 /*12*/, frameY + 7 /* 12*/);
            minimap.Width = screenWidth; //mapWidth;// 
            minimap.Height = screenHeight; // mapHeight; // 
            minimap.ScaleImageToSizeOfControl = true; // false; // true;
            minimap.RenderType = RenderType.CRTAndLCD;
            /*  minimap.MouseDown += new InputEventSystem.MouseDownHandler(minimap_MouseDown);
              minimap.MouseUp += new MouseUpHandler(minimap_MouseUp);
              minimap.MouseMove += new MouseMoveHandler(minimap_MouseMove);*/
            minimap.DebugTag = "minimap";

            minimapLocationFrame = new Image(gui);
            minimapLocationFrame.Texture = frameTexture;
            DisplayWindow.Add(minimapLocationFrame);
            minimapLocationFrame.Position = minimap.Position;
            minimapLocationFrame.Width = screenWidth;
            minimapLocationFrame.Height = screenHeight;
            minimapLocationFrame.ScaleImageToSizeOfControl = true;
            minimapLocationFrame.RenderType = RenderType.CRTAndLCD;
            minimapLocationFrame.MouseDown += new InputEventSystem.MouseDownHandler(minimap_MouseDown);
            minimapLocationFrame.MouseUp += new MouseUpHandler(minimap_MouseUp);
            minimapLocationFrame.MouseMove += new MouseMoveHandler(minimap_MouseMove);


            ComputeScaleFactor();


            if (The.Sim.Controller.GraphicsLevelSetting == Control.Controller.GraphicsLevel.High)
            {

                crtScreen = The.InGameUI.DisplayPanelRenderer.AddCRT(minimap,
                    new Point(minimap.AbsolutePosition.X, minimap.AbsolutePosition.Y),
                    minimap.Width, minimap.Height, Level.Bottom, DisplayWindow, ReflectionToUse.Small, false);

            }

            StatusScreen.AddCRTPlasticFrame(gui, DisplayWindow, minimap.Position /*framePosition*/, minimap.Width, minimap.Height /*crtWidth, crtHeight*/, out frame);


            // The.InGameUI.gui.BringToBottom(cablesWindow);

            DrawMapTexture();

            Show();

        }

        void DisplayWindow_Destroyed()
        {
            Destroy();
        }

        public static int GetWidth(int screenWidth)
        {
            return screenWidth + StatusScreen.GetPlasticFrameWidth(); // 33; 
        }


        private void Destroy()
        {
            frameTexture.Dispose();
            mapTexture.Dispose();
        }

        private void ComputeScaleFactor()
        {
            minimapScaleFactorX = 1f / ((float)minimap.Width / (float)mapWidth);
            minimapScaleFactorY = 1f / ((float)minimap.Height / (float)mapHeight);
        }

        public void CreateMap()
        {
            mapWidth = The.Map.mapTileWidth;
            mapHeight = The.Map.mapTileHeight;

           
            // mapTexture = new Texture2D(UWGame.SimSide.Instance.GraphicsDevice, mapWidth, mapHeight, 1, TextureUsage.None, SurfaceFormat.Color);// XNA 3
            mapTexture = new Texture2D(The.Client.GraphicsDevice, mapWidth, mapHeight, false, SurfaceFormat.Color);
            frameTexture = new Texture2D(The.Client.GraphicsDevice, mapWidth, mapHeight, false, SurfaceFormat.Color);

            mapTextureColors = new Color[mapWidth * mapHeight];
            mapBackBuffer = new Color[mapWidth * mapHeight];

            frameTextureColors = new Color[mapWidth * mapHeight];


            mapTexture.SetData(mapTextureColors);
            frameTexture.SetData(frameTextureColors);

            // when loading a new map, resize the screen:
            if (minimap != null)
            {
                minimap.Texture = mapTexture;
                minimap.SetSkinLocation(SkinState.Normal,new Rectangle(0, 0, mapTexture.Width, mapTexture.Height));


                minimapLocationFrame.Texture = frameTexture;
                minimapLocationFrame.SetSkinLocation(SkinState.Normal,new Rectangle(0, 0, frameTexture.Width, frameTexture.Height));

                //minimap.Width = screenWidth; //mapWidth;// 
                //minimap.Height = screenHeight; // mapHeight; // 

                ComputeScaleFactor();

            }
        }


        public void Hide()
        {
            DisplayWindow.Hide();
            //   The.InGameUI.MinimapAccessPanel.btAccessMinimap.IsChecked = false;
            The.InGameUI.OverlayPanel.btAccessMinimap.IsChecked = false;
        }

        public void Show()
        {
            DisplayWindow.Show();
            //The.InGameUI.MinimapAccessPanel.btAccessMinimap.IsChecked = true;
            if (The.InGameUI.OverlayPanel.btAccessMinimap != null)
            {
                The.InGameUI.OverlayPanel.btAccessMinimap.IsChecked = true;
            }
        }

        /* private void InitMinimizedWindow(int xPos, Game game, GUIManager gui)
         {
             minimizedWindow = new Window(gui);
             minimizedWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle("minimap_panel"); //"event_base_small2");
             minimizedWindow.CornerSize = 7;
             minimizedWindow.Margin = 0;
             minimizedWindow.IsMovable = false;
             minimizedWindow.Resizable = false;
             minimizedWindow.HasCloseButton = false;
             minimizedWindow.Position = new Point(xPos, The.Client.GraphicsDevice.Viewport.Height - 36); // dimensions.Top);
             minimizedWindow.WindowSize = new Vector2(64, 64);

             ImageButton openButton = new ImageButton(gui);
             minimizedWindow.Add(openButton);
             openButton.Position = new Point(16, 18);
             openButton.Init(ImageButtonType.CommSlim);
             openButton.Width = openButton.Height;
             openButton.ToolTip = "Opens the minimap.";
             openButton.Click += new ClickHandler(openButton_Click);

         }*/

        /*
        void btVegetation_Click(UIComponent sender, EventArgs e)
        {
            cbVegetation.IsChecked = !cbVegetation.IsChecked;
            MapDirty=true;
        }

        void btUnits_Click(UIComponent sender, EventArgs e)
        {
            cbUnits.IsChecked = !cbUnits.IsChecked;
            MapDirty = true;
        }
        


        void btBuildings_Click(UIComponent sender, EventArgs e)
        {
            cbBuildings.IsChecked = !cbBuildings.IsChecked;
            MapDirty = true;
        }

        void btMinerals_Click(UIComponent sender, EventArgs e)
        {
            cbMinerals.IsChecked = !cbMinerals.IsChecked;
            MapDirty = true;

            ShowZones = cbMinerals.IsChecked;
        }

        private void AddCheckBoxLED(int checkboxLeft, int checkboxY, CheckBoxFlavor flavor, string text, out CheckBox checkbox, out ImageButton button)
        {
            GUIManager gui = The.InGameUI.gui;
            Game game = The.Sim.ScreenManager.Game;

            int checkboxAndButtonSpacing = 28;
            int buttonX = checkboxLeft + 10; //4;
            checkbox = new CheckBox(gui); //new RadioButton(game, gui);
            DisplayWindow.Add(checkbox); // add first!!! sets defaults!
            checkbox.Position = new Point(checkboxLeft, checkboxY);
            // cbVegetation.Text = "VEG";
            checkbox.Init(CheckBoxType.LED, flavor);
            checkbox.Text = text;
            checkbox.ClickEnabled = false;
            checkbox.RenderType = RenderType.Overlay;

            button = new ImageButton(gui);
            DisplayWindow.Add(button);
            button.Position = new Point(buttonX, checkboxY + checkboxAndButtonSpacing);
            button.Init(ImageButtonType.MinimapRubber);
        }*/

        /*  void closeButton_Click(UIComponent sender, EventArgs e)
          {
              DisplayWindow.Hide();
             // cablesWindow.Hide();
            
              minimizedWindow.Show();
          }

          void openButton_Click(UIComponent sender, EventArgs e)
          {
              minimizedWindow.Hide();

           //   cablesWindow.Show();
              DisplayWindow.Show();

             // The.InGameUI.gui.BringToBottom(cablesWindow);
          }*/

        void minimap_MouseMove(MouseEventArgs args)
        {
            if (this.draggingInMap)
            {
                MoveMap(args);
            }
        }

        void minimap_MouseUp(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                this.draggingInMap = false;
            }
        }

        void minimap_MouseDown(InputEventSystem.MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                this.draggingInMap = true;
                // this.draggingThumb = true;
                //  this.lastLocation = args.Position;
                //  this.thumb.CurrentSkinState = SkinState.Pressed;

                // stop tracking on click in minimap:
                The.InGameUI.EnableTracking(false);
                MoveMap(args);

            }
        }

        private void MoveMap(InputEventSystem.MouseEventArgs args)
        {
            // Convert mouse location to image location
            int yPosition = args.Position.Y - minimap.AbsolutePosition.Y;
            int xPosition = args.Position.X - minimap.AbsolutePosition.X;


            int x = (int)((float)xPosition * minimapScaleFactorX);
            x = The.Map.ClampTileMapXPosition(x);
            //  - UWGame.SimSide.Instance.Map.noOfTilesToDisplayHorizontally / 2;


            int y = (int)((float)yPosition * minimapScaleFactorY);
            y = The.Map.ClampTileMapYPosition(y);
            //  - UWGame.SimSide.Instance.Map.noOfTilesToDisplayVertically / 2;

            The.MapUI.ZoomToMapPosition(x, y);
            //The.MapUI.ZoomToMapPosition( new Vector3( x * MapManager.tileSize, y * MapManager.tileSize, 0 ), MapClient.Centering.Middle );

            frameIsDirty = true;
        }

        private bool frameIsDirty;
        private bool settingsAreDirty;

        public void SetFrameDirty()
        {
            //MapManger.Update is calling this method
            frameIsDirty = true;
        }

        public void SetSettingsDirty()
        {
            settingsAreDirty = true;

            // cancel current draw and restart:
            currentMapTileRow = 0;
        }

        public void Update(GameTime gameTime)
        {          
            if (settingsAreDirty || mapRegulator.IsReady() || currentMapTileRow > 0)
            {
                DrawMapTexture();

                settingsAreDirty = false;
            }

            if (frameIsDirty)
            {
                SetMinimapLocationFrameTexture();
                frameIsDirty = false;
            }
        }



        #region Colors

        private Color? GetTreeResourceColorEditor(TerrainTile tile, bool isInGodMode)
        {
            if (tile.TreesOnTile != null)
            {               
                foreach (var tree in tile.TreesOnTile)
                {
                    EditorData editorData;
                    if (tree.Find(out editorData))
                    {
                        if (editorData.Resources != null)
                        {
                            foreach (var item in editorData.Resources)
                            {
                                if (The.InGameUI.OverlaySettings.DisplayResourceType(item.ResourceType))
                                {
                                    return item.ResourceType.Category.Color ?? Color.White;
                                }
                            }
                        }
                    }

                  /*  if (tree.EntityType.TreeType.CropTypes != null)
                    {
                        Tree treeComponent;
                        tree.Find(out treeComponent);

                        foreach (var item in treeComponent.Crops)
                        {
                            if (The.InGameUI.OverlaySettings.DrawResource(item.Value, isInGodMode))
                            {
                                return item.Value.ResourceType.Category.Color ?? Color.White;
                            }
                        }
                    }*/
                }
            }

            return null;
        }

        private Color? GetTreeResourceColor(TerrainTile tile, bool isInGodMode)
        {
            if (tile.TreesOnTile != null)
            {
                SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;

                foreach (var tree in tile.TreesOnTile)
                {
                    if (tree.EntityType.TreeType.CropTypes != null)
                    {
                        Tree treeComponent;
                        tree.Find(out treeComponent);

                        foreach (var item in treeComponent.Crops)
                        {
                            if (The.InGameUI.OverlaySettings.DrawResource(item.Value, isInGodMode))
                            {
                                return item.Value.ResourceType.Category.Color ?? Color.White;
                            }
                        }
                    }
                }
            }


          /*  foreach (var tree in tile.TreesOnTile)
            {
                if (tree.EntityType.TreeType.CropTypes != null)
                {
                    foreach (var item in tree.EntityType.TreeType.CropTypes)
                    {
                        if (The.InGameUI.OverlaySettings.ResourceTypesToDisplay.TryGetValue(item, out display) && display)
                        {
                            // not very efficient code here...
                            if (sharedKnowledge.AllKnownResourceContainers.TryGetValue(item, out list) && list.Count > 0)
                            {
                                foreach (var resourceID in list)
                                {        
                                    ResourceContainer resourceContainer = LookUp<ResourceContainer, ResourceID>.FindByID(resourceID);

                                    if (resourceContainer.MapPosition.X == tile.X && resourceContainer.MapPosition.Y == tile.Y)
                                    {
                                        return item.Category.Color ?? Color.White;
                                    }                                                             
                                }                               
                            }
                        }
                    }
                }
            }*/

            return null;
        }

        private Color? GetTileResourceColorEditor(TerrainTile tile, bool isInGodMode)
        {
            if (tile.DesignerPlacedResources != null) // TileResources != null)
            {
                foreach (var item in tile.DesignerPlacedResources)
                {
                    if (The.InGameUI.OverlaySettings.DisplayResourceType(item.ResourceType))
                    {
                        return item.ResourceType.Category.Color ?? Color.White;
                    }
                }
            }

            return null;

        }

        private Color? GetTileResourceColor(TerrainTile tile, bool isInGodMode)
        {
            if (tile.TileResources != null)
            {
                foreach (var item in tile.TileResources)
                {
                    if (The.InGameUI.OverlaySettings.DrawResource(item.Value, isInGodMode))
                    {
                        return item.Value.ResourceType.Category.Color ?? Color.White;
                    }
                }
            }

          /*  foreach (var resourceContainers in tile.TileResources)
            {
                foreach (var item in resourceContainers.Value.ResourceItems)
                {
                    bool display = false;

                    if (The.InGameUI.OverlaySettings.ResourceTypesToDisplay.TryGetValue(item.Container.ResourceType, out display) && display)
                    {
                        if (sharedKnowledge.AllKnownResourceContainers.TryGetValue(item.Container.ResourceType, out List) && List.Count > 0)
                        {
                            if (item.Container.MapPosition.X == tile.X && item.Container.MapPosition.Y == tile.Y)
                            {
                                return item.Container.ResourceType.Category.Color ?? Color.White;
                            }
                        }
                    }
                }
            }*/

            return null;
        }

        private Color? GetTileEntityColor(TerrainTile tile)
        {
            SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
          
            foreach (var entity in tile.EntitiesOnTile)
            {
                if (entity.EntityType.Person != null && The.InGameUI.OverlaySettings.EntityTypeGroupingsToDisplay[EntityGrouping.ColonyMembers])
                {
                    return OverlaySettings.personsColor;
                }

                if (The.InGameUI.OverlaySettings.DisplayEntityType(entity.EntityType)
                    && sharedKnowledge.AllDetectedEntities.Contains(entity.DetectableID))
                {
                    return OverlaySettings.GetGroupingColorFromEntity(entity, false);
                }
            }               
        

            List<MemoryFact> rememberedEntities;
            if (tile.RememberedRootEntitiesOnTile != null)
            {
                if (tile.RememberedRootEntitiesOnTile.TryGetValue(sharedKnowledge, out rememberedEntities))
                {
                    foreach (var entity in rememberedEntities)
                    {
                        if (The.InGameUI.OverlaySettings.DisplayEntityType(entity.EntityType))
                        {
                            return OverlaySettings.GetGroupingColorFromEntity(entity, true);
                        }
                    }
                }
            }


           /* foreach (var entity in tile.EntitiesOnTile)
            {
                if (entity.EntityType.Person != null && The.InGameUI.OverlaySettings.EntityTypeGroupingsToDisplay[EntityGrouping.ColonyMembers])
                {
                    return OverlaySettings.persons;
                }
                if (The.InGameUI.OverlaySettings.EntityTypesToDisplay.TryGetValue(entity.EntityType, out display) && display)
                {
                    if (sharedKnowledge.AllDetectedEntities.ContainsKey(entity))
                    {
                        return OverlaySettings.GetGroupingColorFromEntityType(entity.EntityType, false);
                    }
                }
            }

            List<MemoryFact> rememberedEntities;
            if (tile.RememberedRootEntitiesOnTile != null)
            {
                if (tile.RememberedRootEntitiesOnTile.TryGetValue(sharedKnowledge, out rememberedEntities))
                {
                    foreach (var entity in rememberedEntities)
                    {
                        if (The.InGameUI.OverlaySettings.EntityTypesToDisplay.TryGetValue(entity.EntityType, out display) && display)
                        {
                            return OverlaySettings.GetGroupingColorFromEntityType(entity.EntityType, true);
                        }
                    }
                }
            }*/
            return null;
        }

        #endregion

        /// <summary>
        /// use this to control how far we have come in the redraw
        /// </summary>
        private int currentMapTileRow = 0;

        private void DrawMapTexture() //int width, int height)
        {
            int width = The.Map.mapTileWidth;
            int height = The.Map.mapTileHeight;

            TerrainTile[][] map = The.Map.TileMap;
            //set the color to the amount of pixels in the textures

            bool isInGodMode = GameWorldRenderer.GetIsInGodMode();

            // timeslice this.

            int rowIndex;
            Color tileColor;
            TerrainTile tile;

            int noOfRowsToRedraw = 16;

            int endRow = Math.Min(height, currentMapTileRow + noOfRowsToRedraw);

            bool displayResources = The.InGameUI.OverlaySettings.DisplayAnyResources();
            bool displayEntities = The.InGameUI.OverlaySettings.DisplayAnyEntities(); 

            bool isInGameMode = The.Sim.Mode == Sim.EngineMode.Game;

            for (int y = currentMapTileRow; y < endRow; y++)
            {
                rowIndex = y * width;
                

                for (int x = 0; x < width; x++)
                {
                    tile = map[x][y];

                    if (!isInGodMode && !tile.HasEverBeenSeenByPlayer)
                    {
                        // draw the shroud
                        tileColor = Color.Black;
                    }
                    else
                    {
                        SharedKnowledge sharedknowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
                        // draw from SharedKnowledge, if defined in OverlaySettings
                        // for each tile, chek the resources, look up in AllDetectables to see if they know about it.
                        // iterate trees also. they can have crops = ResourceContainer
                        // all entities should only render if in AllDetectedEntities.

                        // priority:
                        // 1. entity
                        // 2. resource
                      
                        if (displayEntities)
                        {
                            if (tile.EntitiesOnTile != null)
                            {
                                Color? color = GetTileEntityColor(tile);
                                if (color.HasValue)
                                {
                                    tileColor = color.Value;
                                    mapBackBuffer[rowIndex + x] = tileColor;
                                    continue;
                                }
                            }
                        }

                        if (displayResources)
                        {
                            Color? color = null;

                            if (isInGameMode)
                            {
                                color = GetTreeResourceColor(tile, isInGodMode);
                                
                                if (color == null)
                                {
                                    color = GetTileResourceColor(tile, isInGodMode);
                                }
                            }
                            else
                            {
                                color = GetTreeResourceColorEditor(tile, isInGodMode);
                                
                                if (color == null)
                                {
                                    color = GetTileResourceColorEditor(tile, isInGodMode);
                                }
                            }


                            if (color.HasValue)
                            {
                                tileColor = color.Value;
                                mapBackBuffer[rowIndex + x] = tileColor;
                                continue;
                            }


                            /*
                            if (tile.TileResources != null)
                            {
                                Color? color = GetTileResourceColor(tile, isInGodMode);
                                if (color.HasValue)
                                {
                                    tileColor = color.Value;
                                    mapBackBuffer[rowIndex + x] = tileColor;
                                    continue;
                                }
                            }

                            if (tile.TreesOnTile != null)
                            {
                                Color? color = GetTreeResourceColor(tile, isInGodMode);
                                if (color.HasValue)
                                {
                                    tileColor = color.Value;
                                    mapBackBuffer[rowIndex + x] = tileColor;
                                    continue;
                                }
                            }*/
                        }

                        if (tile.GetCenterTerrain().LevelBelowWater > 0f)
                        {
                            tileColor = Color.Lerp(shallowWaterColor, deepWaterColor, (tile.GetCenterTerrain().LevelBelowWater / Water.WaterDepthForDeepestBlue));
                        }
                        else
                        {
                            if (tile.TileIsInFogOfWar(sharedknowledge.Allegiance))
                            {
                                tileColor = fovColor;
                            }
                            else
                            {
                                tileColor = groundColor;
                            }
                        }
                    }
                    mapBackBuffer[rowIndex + x] = tileColor;
                }
                currentMapTileRow = y + 1;
            }


            if (endRow == height)
            {
                // switch the textures to show the new one.
                // reset for next redraw.

                currentMapTileRow = 0;
                //Array.Copy(mapBackBuffer, mapTextureColors, mapBackBuffer.Length); // needed?

                mapTexture.SetData(mapBackBuffer);   //set the color data on the texture - does this copy or use a reference??? test this.
            }
        }

        private bool IsThereAnyEntityToDisplay()
        {
            foreach (var item in The.InGameUI.OverlaySettings.EntityTypesToDisplay)
            {
                if (item.Value)
                { return true; }
            }
            return false;
        }


        

        private void SetMinimapLocationFrameTexture()
        {
            int rowIndex;
            int width = The.Map.mapTileWidth;
            int height = The.Map.mapTileHeight;

            // create a sepearte texture/image for the frame.
            // redraw when the map moves. 
            // the texture should be mostly transparent so the map texture can be seen through it.

            // Color gray = Common.ColorFromHex("#CCCCCC");
            // gray.A = 255;
            Color gray = Color.Gray;

            frameTextureColors = new Color[mapWidth * mapHeight];
            rowIndex = The.MapUI.mapWindowTileY * width;
            int boxEndX = (int)Common.ClampTop(The.MapUI.mapWindowTileX + The.MapUI.noOfTilesToDisplayHorizontally, mapWidth - 1);

            for (int x = The.MapUI.mapWindowTileX; x <= boxEndX; x++)
            {
                /* System.IndexOutOfRangeException was unhandled Message=Index was outside the bounds of the array.Source=UnclaimedWorld StackTrace:
                 * the errror comes when the map size is smaller than the screen resolution that has been set in Game.cs. Reduce the screen resolution to make it go away.
                 * */
                

                frameTextureColors[rowIndex + x] = gray;
            }

            rowIndex = (int)Common.ClampTop(
                (The.MapUI.mapWindowTileY + The.MapUI.noOfTilesToDisplayVertically),
                height - 1) * width;

            for (int x = The.MapUI.mapWindowTileX; x <= boxEndX; x++)
            {
                frameTextureColors[rowIndex + x] = gray;
            }

            int boxEndY = (int)Common.ClampTop(The.MapUI.mapWindowTileY + The.MapUI.noOfTilesToDisplayVertically, height - 1);
            int boxStartX = The.MapUI.mapWindowTileX;
            for (int y = The.MapUI.mapWindowTileY; y < boxEndY; y++)
            {
                frameTextureColors[y * width + boxStartX] = gray;
            }

            for (int y = The.MapUI.mapWindowTileY; y < boxEndY; y++)
            {
                frameTextureColors[y * width + boxEndX] = gray;
            }

            frameTexture.SetData(frameTextureColors);//set the color data on the texture            
        }
    }
}
