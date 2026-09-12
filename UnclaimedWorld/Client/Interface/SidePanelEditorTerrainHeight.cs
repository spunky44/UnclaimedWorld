using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
using UWGame.ClientSide.Interface.Editor.Controls;
using UWGame.ClientSide.Interface.Editor;
using UWGame.ClientSide.Interface.Editor.MapTools;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Soil;


namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// change terrain height
    /// </summary>
    public class SidePanelEditorTerrainHeight : RosterPanel, IEditorPanel 
    {
              
        Label lblStatusInfo1, lblStatusInfo2;
               
        
        Toolbar tools;
        

        public SidePanelEditorTerrainHeight()
            : base(The.InGameUI.sidePanelFullHeight, true) //, 150)
        {
            
            InitStatusContentPanel();

            // keep this fixed at the bottom:
            tools = new Toolbar(this, Toolbar.Resolution.Subtile);
            lcdSurface.Add(tools); // Window.Add(tools);
            tools.Y = 400; // outerGrid.Bottom + 12; 

        }

        public void AffectMap(List<MapTool.TileAndChange> affectedTiles)
        {

        }


        public void AffectMap(List<MapTool.SubTileAndChange> affectedTiles)
        {
            int left = affectedTiles.Min(t => t.SubtilePos.X);
            int right = affectedTiles.Max(t => t.SubtilePos.X);
            int top = affectedTiles.Min(t => t.SubtilePos.Y);
            int bottom = affectedTiles.Max(t => t.SubtilePos.Y);

            Point topLeft = MapManager.SubTileToTilePos(new Point(left, top));
            Point bottomRight = MapManager.SubTileToTilePos(new Point(right, bottom));
            bottomRight.X = Common.ClampBottom(bottomRight.X, topLeft.X + 1);
            bottomRight.Y = Common.ClampBottom(bottomRight.Y, topLeft.Y + 1);

            Rectangle tileArea = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);

            // start by subdividing all tiles (we have to keep existing terrain values!):
            MapLoader.CreateTerrainSubtiles(tileArea);


            float waterLevelBelowTerrain = The.Client.Renderer.Water.WaterHeight - MapManager.TerrainZLevel;

                      
            foreach (var item in affectedTiles)
            {
                Terrain terrain = The.Map.GetTerrain(item.SubtilePos.ToPoint());
              
                float valueToAdd = -item.Change;       
                float depth = terrain.TerrainDepth + valueToAdd;

                MapLoader.SetTerrainDepth(waterLevelBelowTerrain, terrain, depth);
                            
                    
                The.Map.SetSubtileCostToSurfaceType(item.SubtilePos.ToPoint());
              
               // The.Client.Renderer.terrainSlicedMap.Redraw(MapManager.SubTileToWorldPos3(item.SubtilePos));

                The.Client.Renderer.terrainSlicedMap.Redraw(MapManager.SubTileToWorldPos3(item.SubtilePos));
            }

          
            // make it a bit bigger?

            // re-divide the tiles after changing heights
            MapLoader.RecomputeSubdivision(tileArea);

         
            // needed for rendering:
            The.Map.IterateTileArea(tileArea, The.Client.Renderer.RecomputeTerrainTilePositions); // UpdateTerrainTilePositions);
            //The.Map.IterateTileArea(tileArea, (t) => The.Client.Renderer.CreateTerrainTilePositions(t.X, t.Y, t));
            // also need to update tiles in the gutter that point to these tiles...

        }


       /* void UpdateTerrainTilePositions(TerrainTile tile)
        {
            // needed for rendering:
          // The.Client.Renderer.CreateTerrainTilePositions(tile.X, tile.Y, tile);

            // also need to update tiles in the gutter that point to this tile...
            The.Client.Renderer.RecomputeTerrainTilePositions(tile); // tilesToPositions

        }*/


       /* void CreateTerrainTilePositions(Rectangle tileArea)
        {             
            TerrainTile mapTile;
            MapManager map = The.Map;

            // create terrainsubtiles:

            for (int x = tileArea.Left; x < tileArea.Right; x++)
            {
                for (int y = tileArea.Top; y < tileArea.Bottom; y++)
                {
                    mapTile = map.TileMap[x][y];
                    The.Client.Renderer.CreateTerrainTilePositions(x, y, mapTile);

                }
            }
        }*/


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

              
       
        public override void Hide()
        {
            base.Hide();
        }

        public override void Refresh()
        {

        }
    }
}
