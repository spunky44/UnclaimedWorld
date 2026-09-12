#region File Description
//-----------------------------------------------------------------------------
// BloomComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Map;
#endregion

namespace UWGame.ClientSide  
{
    public class BloomComponent// : DrawableGameComponent
    {
        #region Fields

        SpriteBatch spriteBatch;

        Effect bloomExtractEffect;
        Effect bloomCombineEffect;
        Effect gaussianBlurEffect;

        //ResolveTexture2D resolveTarget; XNA 3
        //RenderTarget2D sceneRenderTarget;

        RenderTarget2D renderTarget1;
        RenderTarget2D renderTarget2;


        // Choose what display settings the bloom should use.
        public BloomSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        BloomSettings settings = BloomSettings.PresetSettings[0];

        public BloomSettings BaseSettings;

        // Optionally displays one of the intermediate buffers used
        // by the bloom postprocess, so you can see exactly what is
        // being drawn into each rendertarget.
        public enum IntermediateBuffer
        {
            PreBloom,
            BlurredHorizontally,
            BlurredBothWays,
            FinalResult,
        }

        public IntermediateBuffer ShowBuffer
        {
            get { return showBuffer; }
            set { showBuffer = value; }
        }

        IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;


        #endregion

        #region Initialization

        Game game;

        public BloomComponent(Game game)
          //  : base(game)
        {
            this.game = game;

            if (game == null)
                throw new ArgumentNullException("game");

            LoadContent();
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected void LoadContent()
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);

            bloomExtractEffect = game.Content.Load<Effect>("BloomExtract");
            bloomCombineEffect = game.Content.Load<Effect>("BloomCombine");
            gaussianBlurEffect = game.Content.Load<Effect>("GaussianBlur");


            // Look up the resolution and format of our main backbuffer.
            PresentationParameters pp = game.GraphicsDevice.PresentationParameters;

            int width = The.Client.Controller.DrawArea.Width; // pp.BackBufferWidth; // should use play area
            int height = The.Client.Controller.DrawArea.Height; // pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;

            // Create a texture for reading back the backbuffer contents.
           /* resolveTarget = new ResolveTexture2D(GraphicsDevice, width, height, 1, // XNA 3
                format);*/
            //resolveTarget = new RenderTarget2D(GraphicsDevice, width, height, 1, format); // XNA 3   
           
          /*  sceneRenderTarget = new RenderTarget2D(GraphicsDevice, width, height, false,
                                       format, pp.DepthStencilFormat, pp.MultiSampleCount,
                                       RenderTargetUsage.DiscardContents);*/
       

            // Create two rendertargets for the bloom processing. These are half the
            // size of the backbuffer, in order to minimize fillrate costs. Reducing
            // the resolution in this way doesn't hurt quality, because we are going
            // to be blurring the bloom images in any case.
            width /= 2;
            height /= 2;

           // renderTarget1 = new RenderTarget2D(GraphicsDevice, width, height, 1, format); // XNA 3
            renderTarget1 = new RenderTarget2D(game.GraphicsDevice, width, height, false, format, DepthFormat.None);

            // renderTarget2 = new RenderTarget2D(GraphicsDevice, width, height, 1, format); // XNA 3
            renderTarget2 = new RenderTarget2D(game.GraphicsDevice, width, height, false, format, DepthFormat.None);
            
          
          /*  renderTarget1 = new RenderTarget2D(GraphicsDevice, width, height, false, format, DepthFormat.None, pp.MultiSampleCount,
                                       RenderTargetUsage.PreserveContents);

            // renderTarget2 = new RenderTarget2D(GraphicsDevice, width, height, 1, format); // XNA 3
            renderTarget2 = new RenderTarget2D(GraphicsDevice, width, height, false, format, DepthFormat.None, pp.MultiSampleCount,
                                       RenderTargetUsage.PreserveContents);*/
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        public void UnloadContent()
        {
           /*
            renderTarget1.Dispose();
            renderTarget2.Dispose();*/
        }

        public void Destroy()
        {
            renderTarget1.Dispose();
            renderTarget2.Dispose();
        }

        #endregion

        #region Draw


        /// <summary>
        /// This is where it all happens. Grabs a scene that has already been rendered,
        /// and uses postprocess magic to add a glowing bloom effect over the top of it.
        /// </summary>
        public /*override*/ void Draw(Texture2D sceneTexture) //GameTime gameTime)
        {
            // Resolve the scene into a texture, so we can
            // use it as input data for the bloom processing.
            //GraphicsDevice.ResolveBackBuffer(resolveTarget); // XNA 3
           // GraphicsDevice.SetRenderTarget(sceneRenderTarget);
           // GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            // Pass 1: draw the scene into rendertarget 1, using a
            // shader that extracts only the brightest parts of the image.
            bloomExtractEffect.Parameters["BloomThreshold"].SetValue(
                Settings.BloomThreshold);

         /*   DrawFullscreenQuad(sceneTexture, renderTarget1,
                               bloomExtractEffect,
                               IntermediateBuffer.PreBloom);*/

            game.GraphicsDevice.SetRenderTarget(renderTarget1);

            DrawFullscreenQuad(sceneTexture,
                               renderTarget1.Width, renderTarget1.Height,
                               bloomExtractEffect, IntermediateBuffer.PreBloom);


            
            // Pass 2: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.
            SetBlurEffectParameters(1.0f / (float)renderTarget1.Width, 0);
            
            /*
            DrawFullscreenQuad(renderTarget1, renderTarget2,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredHorizontally);*/

            game.GraphicsDevice.SetRenderTarget(renderTarget2);

          //  GameWorldRenderer.SaveRenderTargetToFile("renderTarget1", renderTarget1);
          //  GameWorldRenderer.SaveTextureToFile("sceneTexture", sceneTexture);



            DrawFullscreenQuad(renderTarget1,
                               renderTarget2.Width, renderTarget2.Height,
                               gaussianBlurEffect, IntermediateBuffer.BlurredHorizontally);



       //     Texture2D extract = renderTarget1.GetTexture();
       //     extract.Save("bloomExtract.png", ImageFileFormat.Png);

            // Pass 3: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            SetBlurEffectParameters(0, 1.0f / (float)renderTarget1.Height);

        //    Texture2D blurredHorizontally = renderTarget2.GetTexture();
       //     blurredHorizontally.Save("blurredHorizontally.png", ImageFileFormat.Png);

          /*  DrawFullscreenQuad(renderTarget2, renderTarget1,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredBothWays);*/

            game.GraphicsDevice.SetRenderTarget(renderTarget1);

           // GameWorldRenderer.SaveRenderTargetToFile("renderTarget2", renderTarget2);
          
            DrawFullscreenQuad(renderTarget2,
                               renderTarget1.Width, renderTarget1.Height,
                               gaussianBlurEffect, IntermediateBuffer.BlurredBothWays);


            // Pass 4: draw both rendertarget 1 and the original scene
            // image back into the main backbuffer, using a shader that
            // combines them to produce the final bloomed result.
            The.Client.Controller.SetZoomRenderTaget();

            EffectParameterCollection parameters = bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(Settings.BloomIntensity);
            parameters["BaseIntensity"].SetValue(Settings.BaseIntensity);
            parameters["BloomSaturation"].SetValue(Settings.BloomSaturation);
            parameters["BaseSaturation"].SetValue(Settings.BaseSaturation);

            parameters["BaseTexture"].SetValue(sceneTexture);
           // game.GraphicsDevice.Textures[1] = sceneTexture; 

            //Viewport viewport = game.GraphicsDevice.Viewport;
            Dimension dim = The.Client.Controller.DrawArea;

            DrawFullscreenQuad(renderTarget1,
                               dim.Width, dim.Height,
                               bloomCombineEffect,
                               IntermediateBuffer.FinalResult);

          //  GameWorldRenderer.SaveTextureToFile("sceneTexture", sceneTexture);
          
        }


        /// <summary>
        /// Helper for drawing a texture into a rendertarget, using
        /// a custom shader to apply postprocessing effects.
        /// </summary>
   /*     void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget,
                                Effect effect, IntermediateBuffer currentBuffer)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);

            DrawFullscreenQuad(texture,
                               renderTarget.Width, renderTarget.Height,
                               effect, currentBuffer);

            GraphicsDevice.SetRenderTarget(null);
        }*/


        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuad(Texture2D texture, int width, int height,
                                Effect effect, IntermediateBuffer currentBuffer)
        {
            // NEW:
            if (showBuffer >= currentBuffer)
            {                
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, effect: effect);
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            }

            // Begin the custom effect, if it is currently enabled. If the user
            // has selected one of the show intermediate buffer options, we still
            // draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
           /* if (showBuffer >= currentBuffer)
            {               
                effect.CurrentTechnique.Passes[0].Apply(); 
            }*/

            // Draw the quad.
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
                        
        }


        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }


        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        float ComputeGaussian(float n)
        {
            float theta = Settings.BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }


        #endregion
    }
}
