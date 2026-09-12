using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Maps;
using InputEventSystem;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    /// <summary>
    /// this window opens in Edit mode when tile(s) are selected
    /// </summary>
    public class SetTileResourcesWindow : HUDWindow
    {
      //  Point position = new Point(10, 400);
        
       // Rectangle screenDimensions = new Rectangle(40, 700, 400, 300);

        protected int itemHeight = 18;


        Grid outerGrid;

     /*   CollapsablePanel cpItems, cpCritters;
        Grid grdItems, grdCritters;
        */

        FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;

        ClickHandler resourceClickHandler;

        SetTileResourcesPopup popup;

     //   Window buttonPanelWindow;



        public SetTileResourcesWindow() //int screenWidth, int screenHeight, int screenX)
            : base(240, 200, true, level: Level.Bottom)
        {
            popup = new SetTileResourcesPopup();

            SetSummaryDelegate = new FullLCDPanel.SetCollapsedSummary(SidePanelEntity.SetSummaryAsTotal);

            resourceClickHandler = new ClickHandler(ResourceClicked);

            outerGrid = CreateOuterGridForCollapsableLists(The.InGameUI.gui, DisplayWindow.ViewPort);

            TextButton btClear = new TextButton(gui);
            Add(btClear);
            btClear.Text = "Clear";
            btClear.Init(TextButton.TextButtonType.HUD);
            btClear.Click += new ClickHandler(btClear_Click);
            btClear.Y = 162;
            btClear.X = 10;
            btClear.ScaleWidthToFitText();
            //btClear.Height = 20;


        /*    Label test = new Label(gui);
            test.Text = "Hej";
            DisplayWindow.box.Add(test);
          */
  
          //  AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Critters", itemHeight, out cpCritters, out grdCritters);
       //     AddCollapsablePanelAndTreeGrid(The.InGameUI.gui, outerGrid, "Items", CollapsablePanel.PanelType.Node, out cpItems, out grdItems);




          //  InitMinimizedWindow(screenX, game, gui);

           
         /*   int frameWidth = screenWidth + 24;
            int frameHeight = screenHeight + 24;
            int frameX = 7;
            int frameY = 7; //16;
            */

                        
         /*   frame = new Box(gui);
            frame.RenderType = RenderType.Overlay;
            Rectangle rect = gui.GUISpriteSheet.SourceRectangle("minimap_frame");
            frame.SetSkinLocation(SkinState.Normal,rect);
            frame.CornerSize = 26;
            frame.Position = new Point(frameX, frameY);
            frame.Width = frameWidth;
            frame.Height = frameHeight;
            DisplayWindow.Add(frame);

               */   
        }

     /*   public override void SetPosition(Point newPos)
        {
           // popup
            base.SetPosition(newPos);
        }*/

        void btClear_Click(UIComponent sender, EventArgs e)
        {
            // clear all designer placed resources on tiles and trees!

            The.InGameUI.SelectedTiles.IterateArea(tile =>
                {
                    tile.DesignerPlacedResources = null;

                    if (tile.TreesOnTile != null)
                    {
                        foreach (var tree in tile.TreesOnTile)
                        {
                            //Tree treeComponent;
                            EditorData editorData;
                            if (tree.Find(out editorData)) //out treeComponent))
                            {
                                editorData.Resources = null;
                            }
                        }
                    }
                });

           /* foreach (var tile in The.InGameUI.SelectedTiles.Coverage)
            {
                tile.DesignerPlacedResources = null;

                if (tile.TreesOnTile != null)
                {
                    foreach (var tree in tile.TreesOnTile)
                    {
                        //Tree treeComponent;
                        EditorData editorData;
                        if (tree.Find(out editorData)) //out treeComponent))
                        {
                            editorData.Resources = null;
                        }
                    }
                }
            }*/
        }




        public void ClearResource(ResourceType resourceType)
        {
            // clear all designer placed resources of the specified type on tiles and trees!
            The.InGameUI.SelectedTiles.IterateArea(tile =>
                {
                    if (resourceType.TileResourceType != null)
                    {
                        if (tile.DesignerPlacedResources != null)
                        {
                            tile.DesignerPlacedResources = tile.DesignerPlacedResources.Where(r => r.KeyName != resourceType.KeyName).ToArray();
                        }
                    }
                    else
                    {

                        if (tile.TreesOnTile != null)
                        {
                            foreach (var tree in tile.TreesOnTile)
                            {
                                EditorData editorData;
                                if (tree.Find(out editorData))
                                {
                                    editorData.Resources = editorData.Resources.Where(r => r.KeyName != resourceType.KeyName).ToArray();
                                }
                            }
                        }
                    }
                });

            /*
            foreach (var tile in The.InGameUI.SelectedTiles.Coverage)
            {
                if (resourceType.TileResourceType != null)
                {
                    if (tile.DesignerPlacedResources != null)
                    {
                        tile.DesignerPlacedResources = tile.DesignerPlacedResources.Where(r => r.KeyName != resourceType.KeyName).ToArray();
                    }
                }
                else
                {

                    if (tile.TreesOnTile != null)
                    {
                        foreach (var tree in tile.TreesOnTile)
                        {                           
                            EditorData editorData;
                            if (tree.Find(out editorData)) 
                            {
                                editorData.Resources = editorData.Resources.Where(r => r.KeyName != resourceType.KeyName).ToArray();
                            }
                        }
                    }
                }
            }
            */
        }

        public void Hide()
        {
            DisplayWindow.Hide();
            popup.DisplayWindow.Hide();
        }

        public void ResourceClicked(UIComponent sender, EventArgs eventArgs)
        {
            IGameDataButtonEventArgs iEventArgs = (IGameDataButtonEventArgs)eventArgs;

            string resourceKey = ((IGameData)iEventArgs.Item).KeyName;

            popup.DisplayWindow.Y = sender.AbsolutePosition.Y;
            popup.DisplayWindow.X = sender.AbsolutePosition.X + 16;

            // fill in resource data if single tile is selected and it is a tile resource, or if only one tree is selected, and it is a crop resource
            Resource resourceData = null;

            GetDesignerResourceDataInArea(The.InGameUI.SelectedTiles, resourceKey, out resourceData); 
            
            popup.Fill(GameData.Instance.AllResourceTypes[resourceKey], resourceData);
            
            popup.DisplayWindow.Show();


        }


        public void SaveResourceChanges(ResourceType resourceType, int? min, int? max, int? modifier) //Resource resourceData) //float? min, float? max, float? modifier) //Resource resourceData)
        {
            bool resourceWasSet = false;

            The.InGameUI.SelectedTiles.IterateArea(terrainTile =>
                {
                    if (resourceType.TileResourceType != null)
                    {
                        if (SaveResourceChangesToTile(resourceType, min, max, modifier, terrainTile))
                        {
                            resourceWasSet = true;
                        }

                    }
                    else
                    {
                        if (SaveResourceChangesToTrees(resourceType, min, max, modifier, terrainTile))
                        {
                            resourceWasSet = true;
                        }
                    }
                }
            );

            if (resourceWasSet)
            {
                The.Sim.PlaySite.EditorResources.Add(resourceType);
            }
        }

        private static bool SaveResourceChangesToTrees(ResourceType resourceType, int? min, int? max, int? modifier, 
            TerrainTile terrainTile)
        {
            // apply to trees:
            if (terrainTile.TreesOnTile != null)
            {
                List<Entity> treesThatCanHaveThisCrop = terrainTile.TreesOnTile.FindAll(t => t.EntityType.TreeType.CropTypes != null && t.EntityType.TreeType.CropTypes.Contains(resourceType));

                if (treesThatCanHaveThisCrop != null)
                {
                    
                    Resource resource;
                    EditorData editorData;
                    Resource[] resources;
                    foreach (var tree in treesThatCanHaveThisCrop)
                    {
                        if (tree.Find(out editorData))
                        {
                         
                            resources = editorData.Resources;

                            if (resources != null)
                            {
                                resource = editorData.Resources.FirstOrDefault(r => r.KeyName == resourceType.KeyName);
                                if (resource != null)
                                {
                                    resource.MinResourceItems = min;
                                    resource.MaxResourceItems = max;
                                    resource.Modifier = modifier;

                                    continue;
                                }                                
                            }

                            resource = new Resource(resourceType)
                            {                              
                                MinResourceItems = min,
                                MaxResourceItems = max,
                                Modifier = modifier
                            };

                            if (resources != null)
                            {
                                // append resource
                                List<Resource> listOfResources = editorData.Resources.ToList();
                                listOfResources.Add(resource);
                                editorData.Resources = listOfResources.ToArray();
                            }
                            else
                            {
                                editorData.Resources = new Resource[]{ resource };
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private static bool SaveResourceChangesToTile(ResourceType resourceType, int? min, int? max, int? modifier, //float? min, float? max, float? modifier, 
            TerrainTile terrainTile)
        {
            // apply to tiles:
            List<Resource> existingResources = null;
            if (terrainTile.DesignerPlacedResources != null)
            {
                Resource existingResource = terrainTile.DesignerPlacedResources.FirstOrDefault(r => r.KeyName == resourceType.KeyName);
                if (existingResource != null)
                {
                    existingResource.MinResourceItems = min; // resourceData.Min;
                    existingResource.MaxResourceItems = max; // resourceData.Max;
                    existingResource.Modifier = modifier; // resourceData.Modifier;

                }
                else
                {
                    existingResources = terrainTile.DesignerPlacedResources.ToList();
                    existingResources.Add(new Resource(resourceType)
                    {                       
                        MinResourceItems = min,
                        MaxResourceItems = max,
                        Modifier = modifier
                    });

                    terrainTile.DesignerPlacedResources = existingResources.ToArray();
                }

            }
            else
            {
                terrainTile.DesignerPlacedResources = new[]
                        {
                            new Resource(resourceType) 
                            {                               
                                MinResourceItems = min, 
                                MaxResourceItems = max, 
                                Modifier = modifier }
                        };
            }

            return true;
        }

        private void GetDesignerResourceDataInArea(MapArea mapArea, string resourceKeyName /*ResourceType resourceType,*/, out Resource resourceData)
        {
            resourceData = null;

            Resource foundResourceData = null;

            if (mapArea.Count == 1)
            {
                //TerrainTile tile = mapArea.Coverage[0];

                mapArea.IterateArea(tile =>
                    {
                        Resource resource;

                        if (tile.DesignerPlacedResources != null)
                        {
                            // see if the tile has designer-placed resources of the requested type:
                            resource = tile.DesignerPlacedResources.FirstOrDefault(r => r.KeyName == resourceKeyName); // resourceType.KeyName);
                            if (resource != null)
                            {
                                foundResourceData = resource;
                                return;
                            }

                        }
                        else
                        {
                            // if there are trees in the tile, see if one has designer-placed resources of the requested type:
                            if (tile.TreesOnTile != null) // && tile.TreesOnTile.Count == 1)
                            {
                                foreach (var tree in tile.TreesOnTile)
                                {
                                    //   Entity tree = tile.TreesOnTile[0];
                                    //if (tree.EntityType.TreeType.CropTypes.Contains(resourceType))
                                    // {
                                    resource = GetEditorResource(tree, resourceKeyName);
                                    if (resource != null)
                                    {
                                        foundResourceData = resource;
                                        return;
                                    }
                                }
                                //  }                                               

                            }
                        }
                    });
            }

            resourceData = foundResourceData;

        }

        private Resource GetEditorResource(Entity tree, string resourceKeyName)
        {
            EditorData editorData;
            Resource resource = null;
            if (tree.Find(out editorData))
            {
                if (editorData.Resources != null)
                {
                    resource = editorData.Resources.FirstOrDefault(r => r.KeyName == resourceKeyName); // resourceType.KeyName);                
                }
            }

            return resource;
        }

        /// <summary>
        /// get the crop resource types that are valid for the trees etc. in the area
        /// </summary>
        /// <param name="mapArea"></param>
        /// <param name="sum"></param>
        private void GetApplicableCropResourcesInArea(MapArea mapArea, Dictionary<ResourceType, int> sum)
        {
            mapArea.IterateArea(tile =>
                {
                    if (tile.TreesOnTile != null)
                    {
                        // Trees.Tree treeComponent;
                        foreach (Entity tree in tile.TreesOnTile)
                        {
                            if (tree.EntityType.TreeType.CropTypes != null)
                            {
                                foreach (var cropType in tree.EntityType.TreeType.CropTypes)
                                {
                                    sum[cropType] = 0;
                                }
                            }
                        }
                    }

                });
           
        }

        


        public void Populate()
        {
            Dictionary<ResourceType, int> numberOfResources = new Dictionary<ResourceType, int>();

          //  ResourceCategory carcassCategory = GameData.Instance.AllResourceCategories["carcasses"];

            GetApplicableCropResourcesInArea(The.InGameUI.SelectedTiles, numberOfResources);
            
            /*
            foreach (var item in GameData.Instance.AllResourceTypes) // tree branches... other crops too???
            {
                if (item.Value.Category != carcassCategory) // don't include carcasses
                {
                    numberOfResources.Add(item.Value, 0);
                }
            }*/

            // add the tile resources that don't depend on entities in the tile:
            foreach (var resourceType in GameData.Instance.AllResourceTypes) //AllTileResourceTypes)
            {
                if (resourceType.Value.TileResourceType != null)
                {
                    numberOfResources.Add(resourceType.Value, 0);
                }
            }

          //  GetSumOfAllResourcesInArea(The.InGameUI.SelectedTiles, numberOfResources);

            FullLCDPanel.PopulateCategoryGrid<ResourceType, int, ResourceCategory>(gui, outerGrid, CollapsablePanel.PanelType.HUD, Label.LabelType.HUDWindow,
                SetSummaryDelegate, resourceClickHandler, /*setSummaryDelegate,*/
                numberOfResources);
        }

        


     /*   private void InitMinimizedWindow(int xPos, Game game, GUIManager gui)
        {
            minimizedWindow = new Window(gui);
            minimizedWindow.Skin = gui.GUISpriteSheet.SourceRectangle("minimap_panel"); //"event_base_small2");
            minimizedWindow.CornerSize = 7;
            minimizedWindow.Margin = 0;
            minimizedWindow.IsMovable = false;
            minimizedWindow.Resizable = false;
            minimizedWindow.HasCloseButton = false;
            minimizedWindow.Position = new Point(xPos, UWGame.SimSide.Instance.GraphicsDevice.Viewport.Height - 36); // dimensions.Top);
            minimizedWindow.WindowSize = new Vector2(64, 64);

            ImageButton openButton = new ImageButton(gui);
            minimizedWindow.Add(openButton);
            openButton.Position = new Point(16, 18);
            openButton.Init(ImageButtonType.CommSlim);
            openButton.Width = openButton.Height;
            openButton.ToolTip = "Opens the minimap.";
            openButton.Click += new ClickHandler(openButton_Click);

        }*/

     

     /*   void closeButton_Click(UIComponent sender, EventArgs e)
        {
            DisplayWindow.Hide();
            cablesWindow.Hide();
            
            minimizedWindow.Show();
        }

        void openButton_Click(UIComponent sender, EventArgs e)
        {
            minimizedWindow.Hide();

            cablesWindow.Show();
            DisplayWindow.Show();

            The.InGameUI.gui.BringToBottom(cablesWindow);
        }*/

      

     

      
    }
}
