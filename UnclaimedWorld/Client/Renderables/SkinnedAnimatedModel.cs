using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
////using Microsoft.Xna.Framework.Storage;
using Xclna.Xna.Animation;
using UWGame.SimSide.Maps;
using UWGame;
using UWGame.SimSide;
using UWGame.ClientSide.Map;

namespace UWGame.ClientSide.Renderables
{
    public class SkinnedAnimatedModel: AnimatedModel
    {

      /*  public void ComputeMatricesForDrawing()
        {
            world = Matrix.Identity;
            world.Forward = direction;
            world.Up = Up;
            world.Right = Right;

            float yDrawn = UWGame.SimSide.Instance.CorrectModelYPositionForDrawing(Location.Y);

            translation = Matrix.CreateTranslation(new Vector3(Location.X, yDrawn, -Location.Z) + EntityType.ModelOffset);
            //  world.Translation = 

            scaling = Matrix.CreateScale(EntityType.ModelScale);


            transformation = scaling * world * translation;
            ModelAnimator.World = transformation;
        }*/



       

      
      //  private static BlendState overlayBlendState;

        static SkinnedAnimatedModel() // static constructor
        {
            overlayBlendState = new BlendState();
            overlayBlendState.ColorSourceBlend = Blend.SourceAlpha;
            overlayBlendState.ColorDestinationBlend = Blend.One;
            
        }


        public SkinnedAnimatedModel() { }

        public SkinnedAnimatedModel(SkinnedAnimatedModel original, bool canAnimate): base(original, canAnimate)
        {

        }

       

        /// <summary>
        ///  Note that this method iterates all meshes and assigns the effect parameters. After that, the ModelAnimator iterates again and actually draws the meshes, after applying animation.        
        /// </summary>     
        public override void DrawModel(ref Matrix world, ref Matrix view, ref Matrix projection,
            GameWorldRenderer.RenderTechnique technique, Vector3? replaceColor0, Vector3? replaceColor1, Vector3? replaceColor2, Vector3? replaceColor3, 
            float alphaFactor, float lightIntensity, float dirtLevel, Texture basicTexture = null, Color? tintColor = null)
        {

            if (The.Client == null)
                return; //TODO unneccessary after decouple

            

            ModelAnimator.World = world;

            GraphicsDevice device = The.Client.GraphicsDevice;

            HandleEmitterModelParts(technique);


            if (technique == GameWorldRenderer.RenderTechnique.Standard || 
                technique == GameWorldRenderer.RenderTechnique.DrawModelEmitters || 
                technique == GameWorldRenderer.RenderTechnique.StandardOverlay ||
                technique == GameWorldRenderer.RenderTechnique.StandardMonochrome)
            {
                CreateLights(lightIntensity, /* out mainLightColor, out fillLightColor, out backLightColor, out mainLightDirection, out fillLightDirection, out backLightDirection,*/ tintColor);
            }

            if (technique == GameWorldRenderer.RenderTechnique.Standard ||
                technique == GameWorldRenderer.RenderTechnique.StandardMonochrome ||
                 technique == GameWorldRenderer.RenderTechnique.StandardOverlay )
            {
                //Viewport viewport = The.Client.GraphicsDevice.Viewport;
                Dimension dim = The.Client.Controller.DrawArea;
                Vector2 viewportSize = new Vector2(dim.Width, dim.Height); // viewport.Width, viewport.Height);
                Vector2 scanlinesTextureDimensions = GetScanLinesDimensions();
                   

                device.DepthStencilState = DepthStencilState.Default;
               // device.DepthStencilState = DepthStencilState.None;

                if (technique == GameWorldRenderer.RenderTechnique.StandardOverlay)
                {                    
                    device.BlendState = overlayBlendState; // overlay!!!
                }
                else
                {
                    device.BlendState = BlendState.AlphaBlend; // use normal blending.
                }


                //float intensity = 1f; // Math.Max(0.8f, Weather.Instance.SunIntensity);
                                
                foreach (ModelMesh mesh in ModelAnimator.Model.Meshes)
                {
                    
                    foreach (Effect effect in mesh.Effects)
                    {
                        //   effect.World = world;
                        /*effect.View = view;
                        effect.Projection = projection;*/
                        //  effect.EnableDefaultLighting();

                        switch (technique)
                        {
                            case GameWorldRenderer.RenderTechnique.StandardMonochrome:
                                effect.CurrentTechnique = effect.Techniques["StandardRenderMonochrome"];
                                break;
                            case GameWorldRenderer.RenderTechnique.StandardOverlay:
                                effect.CurrentTechnique = effect.Techniques["StandardOverlay"];
                           
                                effect.Parameters["ViewportSize"].SetValue(viewportSize);
                                effect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);

                                Texture2D depthmap = The.Client.Renderer.DistanceHeightAndBillboardAlphaRenderTarget;
                            
                                effect.Parameters["DistanceHeightAndBillboardAlpha"].SetValue(depthmap);
                           
                                effect.Parameters["ScanlinesTexture"].SetValue(The.Client.Renderer.Scanlines);
                                effect.Parameters["ScanlinesTextureDimensions"].SetValue(scanlinesTextureDimensions);

                                effect.Parameters["OverlayGradient"].SetValue(The.Client.Renderer.OverlayGradient);

                                break;
                            default:
                                effect.CurrentTechnique = effect.Techniques["SkinnedRender"];
                                break;
                        }

                        if (basicTexture != null)
                        {
                            effect.Parameters["BasicTexture"].SetValue(basicTexture);
                        }
                        else
                        {
                            // how can we set the texture back to its default??? probably not possible. this means the texture will be the same as what rendered for the previous entity...
                           
                        }

                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["UncorrectedWorld"].SetValue(uncorrectedStandardDrawingWorldTransformation);

                        effect.Parameters["EyePosition"].SetValue(The.Client.Renderer.CameraPosition);

                   
                        // main light
                        effect.Parameters["DirLight0Direction"].SetValue(mainLightDirection);
                        effect.Parameters["DirLight0DiffuseColor"].SetValue(mainLightColor);
                        effect.Parameters["DirLight0SpecularColor"].SetValue(mainLightColor);
                    //    effect.Parameters["DirLight0SpecularColor"].SetValue(0.1f * mainLightColor);

                        // fill light
                        effect.Parameters["DirLight1Direction"].SetValue(fillLightDirection);
                        effect.Parameters["DirLight1DiffuseColor"].SetValue(fillLightColor);
                        effect.Parameters["DirLight1SpecularColor"].SetValue(fillLightColor);
                       // effect.Parameters["DirLight1SpecularColor"].SetValue(0.1f * fillLightColor);

                        // back light
                        effect.Parameters["DirLight2Direction"].SetValue(backLightDirection);
                        effect.Parameters["DirLight2DiffuseColor"].SetValue(backLightColor);
                        //effect.Parameters["DirLight2SpecularColor"].SetValue(0.1f * backLightColor);

                        effect.Parameters["ReplaceColor0"].SetValue(replaceColor0.Value); //new Vector3(1, 0, 0)); // pants
                        effect.Parameters["ReplaceColor1"].SetValue(replaceColor1.Value); //new Vector3(0, 1, 0)); // shirt
                        effect.Parameters["ReplaceColor2"].SetValue(replaceColor2.Value); //new Vector3(0, 0, 1)); // hair
                        effect.Parameters["ReplaceColor3"].SetValue(replaceColor3.Value); //new Vector3(1, 1, 0)); // skin

                        effect.Parameters["AlphaFactor"].SetValue(alphaFactor);


                    }
                    // mesh.Draw();
                }
                ModelAnimator.Draw(The.Sim.GameTime);

                device.DepthStencilState = DepthStencilState.None;

            }
            else
            {
                //BlendState restore = device.BlendState;

                foreach (ModelMesh mesh in ModelAnimator.Model.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        
                        switch (technique)
                        {

                            case GameWorldRenderer.RenderTechnique.NoLighting:
                                effect.CurrentTechnique = effect.Techniques["SkinnedRenderNoLighting"];
                                break;
                           
                            case GameWorldRenderer.RenderTechnique.DepthHeightBillboardAlpha:
                                effect.CurrentTechnique = effect.Techniques["DepthHeightBillboardAlpha"];
                                break;

                            case GameWorldRenderer.RenderTechnique.DrawModelEmitters:
                                effect.CurrentTechnique = effect.Techniques["EmittersOnly"];

                                // add some lights:
                             
                                // main light
                                effect.Parameters["DirLight0Direction"].SetValue(mainLightDirection); // midnight: 0, 0, 1
                                effect.Parameters["DirLight0DiffuseColor"].SetValue(mainLightColor);

                                // fill light
                                effect.Parameters["DirLight1Direction"].SetValue(fillLightDirection);// midnight: 0, 0, 0
                                effect.Parameters["DirLight1DiffuseColor"].SetValue(fillLightColor);

                                // back light
                                effect.Parameters["DirLight2Direction"].SetValue(backLightDirection);
                                effect.Parameters["DirLight2DiffuseColor"].SetValue(backLightColor);

                                break;

                            case GameWorldRenderer.RenderTechnique.NormalsAndDepth:
                                effect.CurrentTechnique = effect.Techniques["NormalDepth"];

                                // xna4 ???
                                //device.BlendState = BlendState.Opaque;
                                device.BlendState = BlendState.NonPremultiplied;
                                device.DepthStencilState = DepthStencilState.Default;
                                break;

                        }
                     
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["UncorrectedWorld"].SetValue(uncorrectedStandardDrawingWorldTransformation);

                        effect.Parameters["EyePosition"].SetValue(The.Client.Renderer.CameraPosition);

                        // for emitter distance rendering:
                        //Viewport viewport = The.Client.GraphicsDevice.Viewport;
                        Dimension dim = The.Client.Controller.DrawArea;
                        Vector2 viewportSize = new Vector2(dim.Width, dim.Height); // viewport.Width, viewport.Height);
                        effect.Parameters["ViewportSize"].SetValue(viewportSize);
                        effect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);

                        effect.Parameters["AlphaFactor"].SetValue(alphaFactor); 

                    }
                }
                ModelAnimator.Draw(The.Sim.GameTime);

                //device.BlendState = restore;
            }
                       

        }

        

        
    }
}
