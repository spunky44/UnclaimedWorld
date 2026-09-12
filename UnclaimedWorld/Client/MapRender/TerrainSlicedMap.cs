using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Maps;

namespace UWGame.Client.MapRender
{
    public class TerrainSlicedMap
    {
        TerrainSlice[][] terrainSlices;
        private bool redrawMap = false;
        public RenderTarget2D refractionMap;
        public RenderTarget2D clipMap;


        public void Init()
        {

            PresentationParameters pp = The.Client.GraphicsDevice.PresentationParameters;

            refractionMap = new RenderTarget2D(The.Client.GraphicsDevice, The.MapUI.mapWindowWidth, The.MapUI.mapWindowHeight, false, The.Client.GraphicsDevice.DisplayMode.Format, pp.DepthStencilFormat);
            clipMap = new RenderTarget2D(The.Client.GraphicsDevice, The.MapUI.mapWindowWidth, The.MapUI.mapWindowHeight, false, The.Client.GraphicsDevice.DisplayMode.Format, pp.DepthStencilFormat);


           /* int noOfHorizontalSlices = (int)(The.Map.MapWorldWidth / The.Client.Renderer.TerrainSliceSize);
            int noOfVerticalSlices = (int)(The.Map.MapWorldHeight / The.Client.Renderer.TerrainSliceSize);
            */
            int noOfHorizontalSlices = (int)(Math.Ceiling(The.Map.MapWorldWidth / The.Client.Renderer.TerrainSliceSize));
            int noOfVerticalSlices = (int)(Math.Ceiling(The.Map.MapWorldHeight / The.Client.Renderer.TerrainSliceSize));
           

            terrainSlices = new TerrainSlice[noOfHorizontalSlices][];

            float slicePosX = 0f, slicePosY = 0f; 
            float sliceRight, sliceBottom;
            int sliceWidth, sliceHeight;
            for (int x = 0; x < noOfHorizontalSlices; x++)
            {
                sliceWidth = The.Client.Renderer.TerrainSliceSize;
                sliceRight = slicePosX + sliceWidth;

                
                if (sliceRight > The.Map.MapWorldWidth)
                {
                    sliceWidth -= (int)(sliceRight - The.Map.MapWorldWidth); 
                }

                terrainSlices[x] = new TerrainSlice[noOfVerticalSlices];
                for (int y = 0; y < noOfVerticalSlices; y++)
                {
                    sliceHeight = The.Client.Renderer.TerrainSliceSize;
                    sliceBottom = slicePosY + sliceHeight;
                    if (sliceBottom > The.Map.MapWorldHeight)
                    {
                        sliceHeight -= (int)(sliceBottom - The.Map.MapWorldHeight);
                    }

                    terrainSlices[x][y] = new TerrainSlice(The.Client.Renderer, new Vector2(slicePosX, slicePosY), sliceWidth, sliceHeight);

                    slicePosY += sliceHeight;

                    if (y >= noOfVerticalSlices - 1)
                    {
                        slicePosY = 0f;
                    }
                }

                slicePosX += sliceWidth;
            }

            redrawMap = true;

        }

        public void Destroy()
        {
            refractionMap.Dispose();
            clipMap.Dispose();


            for (int i = 0; i < Common.GetJaggedArrayWidth(terrainSlices); i++)
            {
                var column = terrainSlices[i];
                for (int y = 0; y < Common.GetJaggedArrayHeight(terrainSlices); y++)
                {
                    column[y].Destroy();
                }                
            }

        }


        public void Redraw(Vector3 location) // SubtilePos pos)
        {
            int x = (int)(location.X / The.Client.Renderer.TerrainSliceSize);
            int y = (int)(location.Y / The.Client.Renderer.TerrainSliceSize);

            if (x < Common.GetJaggedArrayWidth(terrainSlices)
                && y < Common.GetJaggedArrayHeight(terrainSlices))
            {
                terrainSlices[x][y].NeedsRedraw = true;
            }
        }


        public void RedrawMap(RenderTarget2D target)
        {
           // int size = (int)(The.Map.MapWorldWidth / The.Client.Renderer.TerrainSliceSize);

            int width = Common.GetJaggedArrayWidth(terrainSlices);
            int height = Common.GetJaggedArrayHeight(terrainSlices);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    terrainSlices[x][y].NeedsRedraw = true;
                    terrainSlices[x][y].Draw(target); // The.Client.Renderer.DiffuseMSRenderTarget);
                }
            }
        }

        public void Draw(RenderTarget2D target)
        {
            if (redrawMap == true)
            {
                redrawMap = false;
                RedrawMap(target);
            }

            GetTerrainSliceIndexesToDraw(out int minX, out int maxX, out int minY, out int maxY);


            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    terrainSlices[x][y].Draw(target);
                   // terrainSlices[x][y].Draw(The.Client.Renderer.DiffuseMSRenderTarget);
                }
            }

         //   return;

           // GameWorldRenderer.SaveRenderTargetToFile("terrainSliceClipRenderTarget", The.Client.Renderer.EdgeDetectSceneRenderTarget);

            The.Client.GraphicsDevice.SetRenderTarget(refractionMap);

          //  GameWorldRenderer.SaveRenderTargetToFile("DiffuseMSRenderTarget", The.Client.Renderer.DiffuseMSRenderTarget); // is white

         //   GameWorldRenderer.SaveRenderTargetToFile("terrainSliceClipRenderTarget", The.Client.Renderer.EdgeDetectSceneRenderTarget);


            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    terrainSlices[x][y].DrawRefractionMap();
                }
            }

            The.Client.GraphicsDevice.SetRenderTarget(clipMap);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    terrainSlices[x][y].DrawClipMap();
                }
            }
            The.Client.GraphicsDevice.SetRenderTarget(target);

            // rt is black:
          //  GameWorldRenderer.SaveRenderTargetToFile("terrainSliceClipRenderTarget", The.Client.Renderer.EdgeDetectSceneRenderTarget);



            //The.Client.GraphicsDevice.SetRenderTarget(The.Client.Renderer.EdgeDetectSceneRenderTarget);
            //System.IO.Stream ss = System.IO.File.OpenWrite("Refraction.png");
            //refractionMap.SaveAsPng(ss, 1280, 1024);
            //ss.Close();

            //ss = System.IO.File.OpenWrite("ClipMap.png");
            //clipMap.SaveAsPng(ss, 1280, 1024);
            //ss.Close();
        }


        private void GetTerrainSliceIndexesToDraw(out int minX, out int maxX, out int minY, out int maxY)
        {

            minX = (int)(The.MapUI.MapWindowWorldPosition.X / The.Client.Renderer.TerrainSliceSize);
            minX = Common.ClampBottom(minX, 0);

            maxX = (int)(Math.Ceiling((The.MapUI.MapWindowWorldPosition.X + The.MapUI.mapWindowWidth) / The.Client.Renderer.TerrainSliceSize));
            maxX = Common.ClampTop(maxX, Common.GetJaggedArrayWidth(terrainSlices) - 1);


        //    maxX = (int)((The.MapUI.MapWindowWorldPosition.X + The.MapUI.mapWindowWidth) / The.Client.Renderer.TerrainSliceSize);
         //   maxX = Common.ClampTop(maxX, (int)(The.Map.MapWorldWidth / The.Client.Renderer.TerrainSliceSize) - 1);


            minY = (int)(The.MapUI.MapWindowWorldPosition.Y / The.Client.Renderer.TerrainSliceSize);
            minY = Common.ClampBottom(minY, 0);


            maxY = (int)(Math.Ceiling((The.MapUI.MapWindowWorldPosition.Y + The.MapUI.mapWindowHeight) / The.Client.Renderer.TerrainSliceSize));
            maxY = Common.ClampTop(maxY, Common.GetJaggedArrayHeight(terrainSlices) - 1);

            /*
            maxY = (int)((The.MapUI.MapWindowWorldPosition.Y + The.MapUI.mapWindowHeight) / The.Client.Renderer.TerrainSliceSize);
            maxY = Common.ClampTop(maxY, (int)(The.Map.MapWorldHeight / The.Client.Renderer.TerrainSliceSize) - 1);
            */
        }

    }
}
