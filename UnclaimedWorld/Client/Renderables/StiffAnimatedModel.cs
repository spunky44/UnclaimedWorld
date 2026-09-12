using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Xclna.Xna.Animation;
using UWGame.SimSide.Maps;
using UWGame.SimSide;
using UWGame.ClientSide.Map;

namespace UWGame.ClientSide.Renderables
{
    /// <summary>
    /// this model is drawn without skinning, but with a grime map and a reflection map.
    /// </summary>
    public class StiffAnimatedModel: AnimatedModel
    {

      
        
        

        public StiffAnimatedModel() { }

        public StiffAnimatedModel(StiffAnimatedModel original, bool canAnimate)
            : base(original, canAnimate)
        {

        }

      /*  public void DrawShadow()
        {
            //draw the model's shadow
            Matrix transformation;

            transformation = scaling * world * Weather.Instance.ShadowMatrix * translation;

            DrawModel(transformation, UWGame.SimSide.Instance.view,
                UWGame.SimSide.Instance.projection, MapManager.RenderTechnique.NoLighting, null, null, null, null);

        }*/


     /*   Vector3 mainLightColor;
        Vector3 fillLightColor;
        Vector3 backLightColor;
        Vector3 mainLightDirection;
        Vector3 fillLightDirection;
        Vector3 backLightDirection;*/

        /// <summary>
        /// Note that this method iterates all meshes and assigns the effect parameters. After that, the ModelAnimator iterates again and actually draws the meshes, after applying animation.
        /// </summary>      
        public override void DrawModel(ref Matrix world, ref Matrix view, ref Matrix projection, GameWorldRenderer.RenderTechnique technique
            , Vector3? replaceColor0, Vector3? replaceColor1, Vector3? replaceColor2, Vector3? replaceColor3, float alphaFactor, float lightIntensity, float dirtLevel, Texture basicTexture = null,
            Color? tintColor = null) 
        {

            if (The.Client == null)
                return; //TODO uneccessary after decouple

            ModelAnimator.World = world;

            GraphicsDevice device = The.Client.GraphicsDevice;

            HandleEmitterModelParts(technique);

            float reflectivityFactor = 1f;

            // this ensures we can't see through the skimmer's floor:
            //renderState.CullMode = CullMode.None; // xna 3
            device.RasterizerState = RasterizerState.CullNone; // xna 4

            device.DepthStencilState = DepthStencilState.Default;


            if (technique == GameWorldRenderer.RenderTechnique.Standard || 
                technique == GameWorldRenderer.RenderTechnique.DrawModelEmitters || 
                technique == GameWorldRenderer.RenderTechnique.StandardOverlay || 
                technique == GameWorldRenderer.RenderTechnique.StandardMonochrome)
            {
                CreateLights(technique, lightIntensity, ref reflectivityFactor, tintColor);                  
            }



         //   Viewport viewport = The.Client.GraphicsDevice.Viewport;
            Dimension dim = The.Client.Controller.DrawArea;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height); // viewport.Width, viewport.Height);

            
            if (technique == GameWorldRenderer.RenderTechnique.Standard ||
                technique == GameWorldRenderer.RenderTechnique.StandardMonochrome ||
                technique == GameWorldRenderer.RenderTechnique.StandardOverlay)
            {
                /*renderState.AlphaBlendEnable = true;  // xna 3
                renderState.AlphaTestEnable = false;
                renderState.DepthBufferEnable = true;*/

                Vector2 scanlinesTextureDimensions = GetScanLinesDimensions();
                
                
               // device.DepthStencilState = DepthStencilState.Default;


                if (technique == GameWorldRenderer.RenderTechnique.StandardOverlay)
                {
                    // renderState.SourceBlend = Blend.SourceAlpha; // xna 3
                    //  renderState.DestinationBlend = Blend.One;

                    device.BlendState = overlayBlendState; // overlay!!!

                    //renderState.CullMode = CullMode.CullCounterClockwiseFace; // xna 3
                   // device.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace; // DEMO XXX - gives error?
                }
                else
                {
                    device.BlendState = BlendState.AlphaBlend; // use normal blending.
                }
                                           

                // Draw the model.

                /* Within a Model, each ModelMesh represents a single physical object. 
                 * Each ModelMesh can be moved independently, and some can be drawn while others are skipped. 
                 * For instance a Model of a car would probably contain one ModelMesh for the body of the vehicle, four for the wheels, 
                 * and a couple for the doors    */
                foreach (ModelMesh mesh in ModelAnimator.Model.Meshes)
                {
                    
                    /*
                     * The Draw method belongs to the ModelMesh, rather than to the root Model, 
                     * because we might want to draw each ModelMesh at a different position (making the car wheels spin), 
                     * or we might want to only draw some subset of the ModelMesh entries  */

                    if (transforms == null)
                        break; //MLo bug fix... transforms is null when building sentry... needs attention TODO

                    Matrix localWorld = transforms[mesh.ParentBone.Index] * world;

                    foreach (Effect effect in mesh.Effects)
                    {
                        /*The ModelMesh.Effects property is just a shortcut that gives you a combined list 
                         * of all the effects used on all the parts of the mesh. 
                         Multiple Effects just means multiple ModelMeshParts!!! Not multiple shaders or anything like that. 
                         The importer groups/splits the meshes parts based on shared material. */

                        // TODO: Select other technique if EnvironmentMapEnabled is false:
                        if (technique == GameWorldRenderer.RenderTechnique.StandardMonochrome)
                        {
                            effect.CurrentTechnique = effect.Techniques["StandardRenderMonochrome"];
                        }
                        else if (technique == GameWorldRenderer.RenderTechnique.StandardOverlay)
                        {
                            effect.CurrentTechnique = effect.Techniques["StandardOverlay"];
                           
                            effect.Parameters["ViewportSize"].SetValue(viewportSize);
                            effect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);//TODO reclaim this from Sim to client

                            Texture2D depthmap = The.Client.Renderer.DistanceHeightAndBillboardAlphaRenderTarget;
                            //depthmap.Save("DistanceHeightAndBillboardAlpha", ImageFileFormat.Png);
                            effect.Parameters["DistanceHeightAndBillboardAlpha"].SetValue(depthmap);
                         //   effect.Parameters["UncorrectedWorldPositionY"].SetValue(base.worldPositionY);

                            effect.Parameters["ScanlinesTexture"].SetValue(The.Client.Renderer.Scanlines);
                            effect.Parameters["ScanlinesTextureDimensions"].SetValue(scanlinesTextureDimensions);

                            effect.Parameters["OverlayGradient"].SetValue(The.Client.Renderer.OverlayGradient);
                            
                        }
                        else
                        {
                            effect.CurrentTechnique = effect.Techniques["StandardRender"];
                        }

                        if (basicTexture != null)
                        {
                            effect.Parameters["BasicTexture"].SetValue(basicTexture);
                        }

                        effect.Parameters["AlphaFactor"].SetValue(alphaFactor); 

                        // Vehicle.fx
                        effect.Parameters["World"].SetValue(localWorld);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["UncorrectedWorld"].SetValue(uncorrectedStandardDrawingWorldTransformation);

                        effect.Parameters["EyePosition"].SetValue(The.Client.Renderer.CameraPosition);

                        // some people believe the reflectivity should be lower in CRT mode:
                        effect.Parameters["ReflectivityFactor"].SetValue(reflectivityFactor);

                        // main light
                        effect.Parameters["DirLight0Direction"].SetValue(mainLightDirection);
                        effect.Parameters["DirLight0DiffuseColor"].SetValue(mainLightColor);
                        effect.Parameters["DirLight0SpecularColor"].SetValue(mainLightColor);

                        // fill light
                        effect.Parameters["DirLight1Direction"].SetValue(fillLightDirection);
                        effect.Parameters["DirLight1DiffuseColor"].SetValue(fillLightColor);
                        effect.Parameters["DirLight1SpecularColor"].SetValue(fillLightColor);

                        // back light
                        effect.Parameters["DirLight2Direction"].SetValue(backLightDirection);
                        effect.Parameters["DirLight2DiffuseColor"].SetValue(backLightColor);
                        effect.Parameters["DirLight2SpecularColor"].SetValue(backLightColor);

                        /*
                        // Set parameters on a color replacement effect
                        effect.Parameters["WorldViewProjection"].SetValue(
                            localWorld * view * projection);
                        effect.Parameters["World"].SetValue(localWorld);
                        effect.Parameters["TargetColor"].SetValue(Color.ToVector3());*/

                        effect.Parameters["ReplaceColor0"].SetValue(replaceColor0.Value); //new Vector3(1, 0, 0)); // pants
                        effect.Parameters["ReplaceColor1"].SetValue(replaceColor1.Value); //new Vector3(0, 1, 0)); // shirt
                        effect.Parameters["ReplaceColor2"].SetValue(replaceColor2.Value); //new Vector3(0, 0, 1)); // hair
                        effect.Parameters["ReplaceColor3"].SetValue(replaceColor3.Value); //new Vector3(1, 1, 0)); // skin

                        effect.Parameters["DirtLevel"].SetValue(dirtLevel); 
                    }

                   // mesh.Draw();

                }
                //NEW:
                ModelAnimator.Draw(The.Sim.GameTime);

                /* xna 3 - reset blend states. is this necessary? we'll find out...
                if (technique == global::UWGame.SimSide.Maps.GameWorldRenderer.RenderTechnique.StandardOverlay)
                {
                    renderState.SourceBlend = Blend.SourceAlpha;
                    renderState.DestinationBlend = Blend.InverseSourceAlpha;
                }

                renderState.AlphaBlendEnable = true; // false;
                renderState.AlphaTestEnable = false;
                renderState.DepthBufferEnable = false;*/
            }
            else {
                
                foreach (ModelMesh mesh in ModelAnimator.Model.Meshes)
                {
                    /*
                     * The Draw method belongs to the ModelMesh, rather than to the root Model, 
                     * because we might want to draw each ModelMesh at a different position (making the car wheels spin), 
                     * or we might want to only draw some subset of the ModelMesh entries  */
                   
                    if (transforms == null)
                        break; //MLo bug fix... transforms is null when building sentry... needs attention TODO

                    Matrix localWorld = transforms[mesh.ParentBone.Index] * world;

                    foreach (Effect effect in mesh.Effects)
                    {

                        switch (technique)
                        {
                            case GameWorldRenderer.RenderTechnique.NoLighting:
                                effect.CurrentTechnique = effect.Techniques["RenderNoLighting"];
                                break;

                            case GameWorldRenderer.RenderTechnique.DepthHeightBillboardAlpha:
                                effect.CurrentTechnique = effect.Techniques["DepthHeightBillboardAlpha"];
                                break;

                            case GameWorldRenderer.RenderTechnique.NormalsAndDepth:
                                effect.CurrentTechnique = effect.Techniques["NormalDepth"];
                                break;

                            case GameWorldRenderer.RenderTechnique.DrawModelEmitters:
                                effect.CurrentTechnique = effect.Techniques["EmittersOnly"];

                                // main light
                                effect.Parameters["DirLight0Direction"].SetValue(mainLightDirection);
                                effect.Parameters["DirLight0DiffuseColor"].SetValue(mainLightColor);
                                effect.Parameters["DirLight0SpecularColor"].SetValue(mainLightColor);

                                // fill light
                                effect.Parameters["DirLight1Direction"].SetValue(fillLightDirection);
                                effect.Parameters["DirLight1DiffuseColor"].SetValue(fillLightColor);
                                effect.Parameters["DirLight1SpecularColor"].SetValue(fillLightColor);

                                // back light
                                effect.Parameters["DirLight2Direction"].SetValue(backLightDirection);
                                effect.Parameters["DirLight2DiffuseColor"].SetValue(backLightColor);
                                effect.Parameters["DirLight2SpecularColor"].SetValue(backLightColor);

                                break;

                        }
                        effect.Parameters["ViewportSize"].SetValue(viewportSize);
                        effect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);

                        effect.Parameters["World"].SetValue(localWorld);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["UncorrectedWorld"].SetValue(uncorrectedStandardDrawingWorldTransformation);
                    }
                }

                ModelAnimator.Draw(The.Sim.GameTime);
            }

           /* else if (technique == Map.GameWorldRenderer.RenderTechnique.NoLighting)
            {
                foreach (ModelMesh mesh in ModelAnimator.Model.Meshes)
                {
                   
                    
                    Matrix localWorld = transforms[mesh.ParentBone.Index] * world;

                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.CurrentTechnique = effect.Techniques["RenderNoLighting"];

                        effect.Parameters["World"].SetValue(localWorld);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["UncorrectedWorld"].SetValue(uncorrectedStandardDrawingWorldTransformation);

                    }

                   // mesh.Draw();
                }
                //NEW:
                ModelAnimator.Draw(UWGame.SimSide.Instance.GameTime);

            }
            else if (technique == Map.GameWorldRenderer.RenderTechnique.DepthHeightBillboardAlpha)
            {                
                //renderState.AlphaBlendEnable = true; // xna 3
                //renderState.AlphaTestEnable = false;
                //renderState.DepthBufferEnable = true;

                device.DepthStencilState = DepthStencilState.Default;

                foreach (ModelMesh mesh in ModelAnimator.Model.Meshes)
                {                    
                    Matrix localWorld = transforms[mesh.ParentBone.Index] * world;

                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.CurrentTechnique = effect.Techniques["DepthHeightBillboardAlpha"];
                        
                        effect.Parameters["ViewportSize"].SetValue(viewportSize);
                        effect.Parameters["WindowPosition"].SetValue(UWGame.SimSide.Instance.Map.MapWindowWorldPosition);

                        effect.Parameters["World"].SetValue(localWorld);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["UncorrectedWorld"].SetValue(uncorrectedStandardDrawingWorldTransformation);

                       // effect.Parameters["UncorrectedWorldPositionY"].SetValue(base.worldPositionY);

                    }

                   // mesh.Draw();
                }

                //NEW:
                ModelAnimator.Draw(UWGame.SimSide.Instance.GameTime);

                // reset states - still necessary?
                //renderState.AlphaBlendEnable = true; // xna 3
                //renderState.AlphaTestEnable = false;
                //renderState.DepthBufferEnable = false;
            }
            else if (technique == Map.GameWorldRenderer.RenderTechnique.NormalsAndDepth)
            {
                //renderState.AlphaBlendEnable = true; // xna 3
                //renderState.AlphaTestEnable = false;
                //renderState.DepthBufferEnable = true;

                device.DepthStencilState = DepthStencilState.Default;

                foreach (ModelMesh mesh in ModelAnimator.Model.Meshes)
                {

                    Matrix localWorld = transforms[mesh.ParentBone.Index] * world;

                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.CurrentTechnique = effect.Techniques["NormalDepth"];

                        effect.Parameters["World"].SetValue(localWorld);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["UncorrectedWorld"].SetValue(uncorrectedStandardDrawingWorldTransformation);

                    }

                  //  mesh.Draw();
                }

                //NEW:
                ModelAnimator.Draw(UWGame.SimSide.Instance.GameTime);

                //  reset states - still necessary?
                //renderState.AlphaBlendEnable = true; // xna 3
                //renderState.AlphaTestEnable = true;
                //renderState.DepthBufferEnable = false;
            }*/

        }

        float mainLightTweakFactor = 0.8f;
        float fillLightTweakFactor = 1f;
        float backLightTweakFactor = 1f;

        private void CreateLights(GameWorldRenderer.RenderTechnique technique, float lightIntensity, ref float reflectivityFactor, Color? tintColor = null)
        {
            // move light computations out into their own method - as with computeMatrices?
            // float intensity = 1.5f; // Math.Max(0.8f, Weather.Instance.SunIntensity);

            //lightIntensity *= 0.8f;
            
            if (technique == GameWorldRenderer.RenderTechnique.StandardMonochrome)
            {

                mainLightColor = 0.5f * lightIntensity * Vector3.One; // intensity * Vector3.One;
                fillLightColor = 0.2f * lightIntensity * Vector3.One;
                backLightColor = 0.4f * lightIntensity * Vector3.One;
                reflectivityFactor = 0.3f;
            }
            else
            {
                if (tintColor.HasValue)
                {
                    Vector3 color = tintColor.Value.ToVector3();
                   
                    mainLightColor = mainLightTweakFactor * lightIntensity * color; 
                    fillLightColor = fillLightTweakFactor * 0.6f * lightIntensity * color; 
                    backLightColor = backLightTweakFactor * lightIntensity * color; 
                }
                else
                {
                    // yellow tint:
                    mainLightColor = mainLightTweakFactor * lightIntensity * mainLightColorConstant;
                    fillLightColor = fillLightTweakFactor * 0.6f * lightIntensity * fillLightColorConstant; 
                    // blue tint:
                    backLightColor = backLightTweakFactor * lightIntensity * backLightColorConstant; 
                }
            }

            SetLightDirections();

            /*
            mainLightDirection = -The.Sim.DateAndTime.SunPosition;
            fillLightDirection = Vector3.Cross(mainLightDirection, Vector3.UnitY);
            //  fillLightDirection.X *= Math.Sign(mainLightDirection.X); // opposite right/left from main light
            if (mainLightDirection.X != 0f)
            {
                fillLightDirection.X *= Math.Sign(mainLightDirection.X); // opposite right/left from main light
            }
            fillLightDirection.Z = Math.Abs(fillLightDirection.Z); // make fill light always from above

            // has no real effect... or?
            backLightDirection = -The.Client.Renderer.CameraDirection;
            backLightDirection.Normalize();
           */

        }

        /*
         * BasicEffect basicEffect = effect as BasicEffect;
                        if (basicEffect != null)
                        {
                            if (!isLit)
                            {
                                basicEffect.TextureEnabled = false;
                                basicEffect.LightingEnabled = false;
                            }
                            else
                            {
                                basicEffect.PreferPerPixelLighting = true;
                                //    basicEffect.TextureEnabled = true;
                                basicEffect.LightingEnabled = true;

                                  basicEffect.SpecularColor = Vector3.One;
                                  basicEffect.SpecularPower = 64;
                            
                                  basicEffect.Alpha = 0.2f;

                                basicEffect.EnableDefaultLighting();
                                // use colored light?

                                // main light
                                basicEffect.DirectionalLight0.Direction = mainLightDirection;
                                       basicEffect.DirectionalLight0.DiffuseColor = mainLightColor;
                                       basicEffect.DirectionalLight0.SpecularColor = mainLightColor;
                                       basicEffect.DirectionalLight0.Enabled = true;
                                       
                                // fill light
                                basicEffect.DirectionalLight1.Direction = fillLightDirection;
                                      basicEffect.DirectionalLight1.DiffuseColor = fillLightColor;
                                      basicEffect.DirectionalLight1.SpecularColor = fillLightColor;
                                      basicEffect.DirectionalLight1.Enabled = true;
          
                                // back light
                                basicEffect.DirectionalLight2.Direction = backLightDirection;
                                      basicEffect.DirectionalLight2.DiffuseColor = backLightColor;
                                      basicEffect.DirectionalLight2.SpecularColor = backLightColor;
                                      basicEffect.DirectionalLight2.Enabled = true;
                                     


                                
                                effect.EnabledLights = EnabledLights.Four; // EnabledLights.One;

                                effect.PointLights[0].Color = Vector3.One * 0.3f; //0.5f; // main light
                                effect.PointLights[0].Position = new Vector3(0, 0, -10); // z is inverted!

                                effect.PointLights[1].Color = Vector3.One * 0.8f; //0.1f; // fill light
                                effect.PointLights[1].Position = new Vector3(-10, 0, -10);
                                effect.PointLights[2].Color = Vector3.One * 0.6f; //0.5f; // back light
                                effect.PointLights[2].Position = UWGame.SimSide.Instance.CameraDirection; 
                                effect.PointLights[3].Color = Vector3.Zero;
                                




                                     basicEffect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);

                                     basicEffect.SpecularColor = Vector3.One; //  new Vector3(0.1f, 0.1f, 0.1f);
                                     basicEffect.SpecularPower = 32f;
                            }

                            // NOT USED?   basicEffect.PreferPerPixelLighting = true;

                            basicEffect.Parameters["World"].SetValue(localWorld);
                            basicEffect.Parameters["View"].SetValue(view);
                            basicEffect.Parameters["Projection"].SetValue(projection);


                        }
          */

    }
}
