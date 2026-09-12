using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SpriteSheetRuntime;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework.Input;
using UWGame.SimSide.Soil;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using UWGame.SimSide.Vegetation;
using System.IO;
using System.Threading.Tasks;
using UWGame.SimSide.AI;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Resources;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.Maps;
using UWGame.ClientSide.Map.Water;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.ClientSide.Map
{
    public class TerrainSlice
    {
        
        // public Texture2D Texture = null;
        RenderTarget2D terrainRenderTargetTexture = null;
        public bool NeedsRedraw = false;
        public bool HasRedrawn = false;
        private Vector2 screenPos;
        Vector2 position;
        private GameWorldRenderer renderer;
        
        public TerrainSlice(GameWorldRenderer renderer, Vector2 pos, int width, int height)
        {
            this.renderer = renderer;

            pos.X -= 0.5f; //Adding half a pixel to fix the rendering position to the object, 
            pos.Y -= 0.5f; //http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
            position = pos;

            
            PresentationParameters pp = The.Client.GraphicsDevice.PresentationParameters;
            terrainRenderTargetTexture = new RenderTarget2D(The.Client.GraphicsDevice, width, height, // (int)The.Client.Renderer.TerrainSliceSize, (int)The.Client.Renderer.TerrainSliceSize, 
                false, The.Client.GraphicsDevice.DisplayMode.Format, pp.DepthStencilFormat);

            NeedsRedraw = true;

            clipPlane = GameWorldRenderer.CreatePlane(renderer.Water.WaterHeight + 1.5f, new Vector3(0, 0, -1), true); //false);
            clipRenderTarget = new RenderTarget2D(The.Client.GraphicsDevice, width, height, //(int)The.Client.Renderer.TerrainSliceSize, (int)The.Client.Renderer.TerrainSliceSize, 
                false, The.Client.GraphicsDevice.DisplayMode.Format,
                                pp.DepthStencilFormat);

        }

        public void Destroy()
        {
            terrainRenderTargetTexture.Dispose();
            clipRenderTarget.Dispose();
        }

        public void DiscardTexture()
        {
            // Texture = null;
            terrainRenderTargetTexture = null;

        }

        List<TerrainBatch> mySliceBatches = null;
        private void Redraw()
        {
            /**
             * Draw to Texture
            **/
            The.Client.GraphicsDevice.SetRenderTarget(terrainRenderTargetTexture);
            The.Client.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

         
            if (mySliceBatches == null)
            {
                renderer.SetUpTerrainVerticesAndIndicesInCurrentView(The.Client.Renderer.TerrainSliceSize, position, out mySliceBatches);
            }
            /*
            Matrix adjustedMatrix = renderer.TerrainViewMatrix;
            adjustedMatrix.M41 = position.X;
            adjustedMatrix.M42 = position.Y;
            */

            
            float minX = mySliceBatches[0].terrainVerticesList.Min(v => v.Position.X);
            float maxX = mySliceBatches[0].terrainVerticesList.Max(v => v.Position.X);
            float minY = mySliceBatches[0].terrainVerticesList.Min(v => v.Position.Y);
            float maxY = mySliceBatches[0].terrainVerticesList.Max(v => v.Position.Y);

            Vector2 renderTargetSize = new Vector2(terrainRenderTargetTexture.Width, terrainRenderTargetTexture.Height);

            renderer.DrawTerrainUserVertices(renderer.terrainBatches, null, position, renderTargetSize, mySliceBatches);

            The.Client.Controller.SetZoomRenderTaget(); //The.Client.GraphicsDevice.SetRenderTarget(null);

            
            The.Client.GraphicsDevice.SetRenderTarget(clipRenderTarget);

           // GameWorldRenderer.SaveRenderTargetToFile("terrainSliceRenderTarget", renderTarget);

            The.Client.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            renderer.DrawTerrainUserVertices(renderer.terrainBatches, clipPlane, position, renderTargetSize, mySliceBatches);
            
            The.Client.Controller.SetZoomRenderTaget(); // The.Client.GraphicsDevice.SetRenderTarget(null);
                      


            mySliceBatches = null;

            NeedsRedraw = false;
        }

        public RenderTarget2D clipRenderTarget;

        private Plane clipPlane;



        public void DrawRefractionMap()
        {

            if (terrainRenderTargetTexture != null)
            {
                if (The.Client.spriteBatch != null)
                {
                    screenPos = The.MapUI.WorldPosToScreen(new Vector2(position.X - 0.5f, position.Y - 0.5f));//Rendering screen position

                    The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                    The.Client.spriteBatch.Draw(terrainRenderTargetTexture, screenPos, Color.White);
                    The.Client.spriteBatch.End();
                }
            }
        }

        public void DrawClipMap()
        {

            if (clipRenderTarget != null)
            {
                if (The.Client.spriteBatch != null)
                {
                    screenPos = The.MapUI.WorldPosToScreen(new Vector2(position.X - 0.5f, position.Y - 0.5f));//Rendering screen position

                    The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                    The.Client.spriteBatch.Draw(clipRenderTarget, screenPos, Color.White);
                    The.Client.spriteBatch.End();
                }
            }
        }

        public void Draw(RenderTarget2D oldRenderTarget)
        {
            /**
             * Redraw Texture if needed then render texture that contains the current terrain 
             **/
            if (terrainRenderTargetTexture.IsContentLost)
            {
                NeedsRedraw = true;
            }

            if (NeedsRedraw == true)
            {                
                Redraw();
                The.Client.GraphicsDevice.SetRenderTarget(oldRenderTarget);
            }

            if (terrainRenderTargetTexture != null)
            {
                if (The.Client.spriteBatch != null)
                {
                    screenPos = The.MapUI.WorldPosToScreen(new Vector2(position.X - 0.5f, position.Y - 0.5f));//Rendering screen position

                    // Rectangle rect = new Rectangle((int)(screenPos.X), (int)(screenPos.Y), 32, 32);

                    The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

                    //   The.Client.spriteBatch.Draw(renderTarget, rect, Color.White); //new

                    The.Client.spriteBatch.Draw(terrainRenderTargetTexture, screenPos, Color.White); //original
                    The.Client.spriteBatch.End();
                }
            }
        }

    }


}
