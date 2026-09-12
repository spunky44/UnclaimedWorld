using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Maps;
using UWGame.SimSide;
using UWGame.SimSide.Allegiances;

namespace UWGame.ClientSide.Interface
{
    public class FogMap
    {
        public Window DisplayWindow;

        Image fogmap;
       
        Texture2D mapTexture;
        Color[] mapTextureColors;

        int mapWidth  = The.Map.mapTileWidth;
        int mapHeight = The.Map.mapTileHeight;
  
        public FogMap( )
        {
            DisplayWindow = new Window(The.InGameUI.gui);
            DisplayWindow.Level = Level.FoggyBottom;
            DisplayWindow.Position = new Point(0,0);//the corner
            DisplayWindow.WindowSize = new Vector2(1f, 1f); //one pixel in the corner, out of the way   
            DisplayWindow.CornerSize = 1; // set to 1 to avoid Debug assertion in Box.CornerSize               //keep
            DisplayWindow.Margin = 0;                   //keep
            DisplayWindow.Resizable = false;            //keep
            DisplayWindow.IsMovable = false;            //keep
            DisplayWindow.HasCloseButton = false;       //keep
            DisplayWindow.HasOverlayComponents = true;  //keep
            DisplayWindow.CanHaveFocus = false;         //keep
            DisplayWindow.Show();

            DisplayWindow.Destroyed += DisplayWindow_Destroyed;

            CreateMap();

            fogmap = new Image(The.InGameUI.gui);
            fogmap.Texture = mapTexture;            
            DisplayWindow.Add(fogmap);
            fogmap.Position = new Point(-(int)The.MapUI.MapWindowWorldPosition.X, -(int)The.MapUI.MapWindowWorldPosition.Y);
            fogmap.Width = mapWidth * MapManager.tileSize;
            fogmap.Height = mapHeight * MapManager.tileSize;   
            fogmap.ScaleImageToSizeOfControl = true;  
            fogmap.RenderType = RenderType.Overlay;
            fogmap.DebugTag = "fogmap";
            fogmap.ClipThis = false;//prevents the 1x1 pixel window from cropping the fogmap  

            DrawMapTexture();
        }

        void DisplayWindow_Destroyed()
        {
            Destroy();
        }


        private void Destroy()
        {
            mapTexture.Dispose();
        }

        public void MoveToPosition(Point pos)
        {
            fogmap.Position = pos; // new Point(-(int)The.MapUI.MapWindowWorldPosition.X, -(int)The.MapUI.MapWindowWorldPosition.Y);
        }

        public void CreateMap()  //check if you have a scenario loading
        {
            mapTexture = new Texture2D(The.Client.GraphicsDevice, mapWidth, mapHeight, false, SurfaceFormat.Color);
            mapTextureColors = new Color[mapWidth * mapHeight];
            for (int c = 0; c < mapTextureColors.Length; ++c )
                mapTextureColors[c] = Color.Black;//fade from black
            ;

            mapTexture.SetData(mapTextureColors);

            if (fogmap != null)
                fogmap.Texture = mapTexture;
        }
 
 
        public void Update(GameTime gameTime)
        {
            //TODO add if(Dirty)
            DrawMapTexture();
           
            // moved to Draw to prevent an annoying lagging effect
            //fogmap.Position = new Point(-(int)The.MapUI.MapWindowWorldPosition.X, -(int)The.MapUI.MapWindowWorldPosition.Y); 
        }

 
        private void DrawMapTexture()  
        {
            Allegiance al = The.InGameUI.UIAllegiance; // The.Sim.Site.PlayerAllegiance;
            TerrainTile[][] map = The.Map.TileMap;

            Color tint = Color.Black * The.MapUI.FogOfWarTint;

            //TODO Instead of iterating every tile in the map, feed a collection of fading tiles
            //and update only those until each expires after fade is done.
            //The collection will be signaled by the FOW setter in Map
            
            int rowIndex;
            Color tileColor;
            TerrainTile tile;
            float lerp = The.MapUI.FogOfWarFadeRate;
            for (int y = 0; y < mapHeight; y++)
            {
                rowIndex = y * mapWidth;
                for (int x = 0; x < mapWidth; x++)
                {
                    tile = map[x][y];

                    if (x == 9 && y == 9)
                    {

                    }

                    if (tile.HasEverBeenSeenByPlayer == false)
                        tileColor = Color.Black;
                    else 
                    if (tile.AllegiancesThatSeeThisTile.Contains(al))
                        tileColor = Color.Transparent;
                    else
                        tileColor = tint;

                    mapTextureColors[rowIndex + x] = Color.Lerp(mapTextureColors[rowIndex + x], tileColor, lerp);
                }

            }

             mapTexture.SetData(mapTextureColors);
         }
    }
}
