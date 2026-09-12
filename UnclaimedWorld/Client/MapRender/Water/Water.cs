using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Maps;
using UWGame.SimSide;

namespace UWGame.ClientSide.Map.Water  
{
    public class Water
    {
        Texture2D waterBumpMap, waterBumpMapLarge;
        
        Model skyDome;

        // vandspejl!!!
        private float waterHeight = MapManager.TerrainZLevel + 70f; 

        /// <summary>
        /// absolute height of water (compare with ground level of the terrain)
        /// 1190f: low water level 1070: high level
        /// Call map.UpdateTileMapFromWaterLevel after changing this!!!
        /// </summary>
        public float WaterHeight
        {
            get
            {
                return waterHeight;  // 1190f; // low water level}
            }
            set
            {
                waterHeight = value;
            }
        }
        
        /// <summary>
        /// used when rendering the terrain for the refraction map
        /// </summary>
        public const float WaterDepthForDeepestBlue = 255f;

        public RenderTarget2D refractionRenderTarget;
        
        RenderTarget2D reflectionRenderTarget;
        //Texture2D reflectionMap;

     /*   DepthStencilBuffer reflectionDepthStencilBuffer;
        DepthStencilBuffer oldDepthStencilBuffer;
        */
        // ONE OR TWO?????
        int waterQuadWidth; // = MapManager.tileWidth * (MapManager.noOfTilesToDisplayHorizontally + 2); // . UWGame.SimSide.Instance.map.mapWidth;
        int waterQuadHeight; // = MapManager.tileHeight * (MapManager.noOfTilesToDisplayVertically + 2); // UWGame.SimSide.Instance.map.mapHeight; //48 * terrainLength;

        Matrix reflectionViewMatrix;

        // quad for the water texture to be drawn on:
        VertexBuffer waterVertexBuffer;
       // VertexDeclaration waterVertexDeclaration;

        private VertexWater[] waterVertices; //OLD: = new VertexWater[6];
        int currentWaterVertex = 0;

        // use 16 bit indices to support older cards:
        private /*int[]*/ short[] waterIndices;

        private float totalElapsedUnpausedTime = 0f;

        #region Sky

       // VertexDeclaration skyVertexDeclaration;
        VertexPositionNormalTexture[] skyQuad = new VertexPositionNormalTexture[6];
        Texture2D cloudMap;
        BasicEffect skyQuadEffect;

        #endregion

        GraphicsDevice device;
      //  GraphicsDeviceManager graphics;

        Effect waterEffect;
       // Effect clipTerrainEffect;

        private Sim game;

        private GameWorldRenderer renderer;

        public Water(GameWorldRenderer renderer)
        {
            this.renderer = renderer;
        }

        public void LoadContent()
        {
            device = The.Client.GraphicsDevice;
            
            game = The.Sim;

            Dimension dim = The.Client.Controller.DrawArea;
            waterQuadWidth = dim.Width; // device.Viewport.Width;
            waterQuadHeight = dim.Height; // device.Viewport.Height;

            waterEffect =  The.Client.Content.Load<Effect>("water");
           
            cloudMap = The.Client.Content.Load<Texture2D>("cloudMap");
            waterBumpMap = The.Client.Content.Load<Texture2D>("waterbump");
            waterBumpMapLarge = The.Client.Content.Load<Texture2D>("waterbumpFlipped");  

            UpdateReflectedViewMatrix();
        

            PresentationParameters pp = device.PresentationParameters;
           // refractionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, 1, device.DisplayMode.Format); // XNA 3
          //  reflectionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, 1, device.DisplayMode.Format); // XNA 3

            Dimension drawArea = The.Client.Controller.DrawArea;
            refractionRenderTarget = new RenderTarget2D(device, drawArea.Width, drawArea.Height, // pp.BackBufferWidth, pp.BackBufferHeight, 
                                false, pp.BackBufferFormat,
                                pp.DepthStencilFormat);

            reflectionRenderTarget = new RenderTarget2D(device, drawArea.Width, drawArea.Height, //pp.BackBufferWidth, pp.BackBufferHeight, 
                                false, pp.BackBufferFormat,
                                pp.DepthStencilFormat);   

           
            SetupSkyVertices();
          //  skyVertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);            
            skyQuadEffect = new BasicEffect(device);
            skyQuadEffect.EnableDefaultLighting();

            skyQuadEffect.World = Matrix.Identity;
            skyQuadEffect.View = reflectionViewMatrix;
            skyQuadEffect.Projection = The.Client.Projection;
            skyQuadEffect.TextureEnabled = true;
            skyQuadEffect.Texture = cloudMap;

        //    LoadVertices();
        //    LoadTextures();
        }

        public void UnloadContent()
        {

        }

        public void Destroy()
        {
            reflectionRenderTarget.Dispose();
            refractionRenderTarget.Dispose();
        }

        /// <summary>
        /// called from MapManager (during BeginRun)
        /// </summary>
        public void Initialize()
        {
            // NEW:
           /* waterVertices = new VertexWater[(renderer.noOfVerticesHorizontal) * (renderer.noOfVerticesVertical)];
            waterIndices = new short[(renderer.noOfVerticesHorizontal - 1) * (renderer.noOfVerticesVertical - 1) * 6];
            */
            waterVertices = new VertexWater[(renderer.noOfVerticesHorizontal + 2) * (renderer.noOfVerticesVertical)];
            waterIndices = new short[(renderer.noOfVerticesHorizontal + 1) * (renderer.noOfVerticesVertical - 1) * 6];
            
            SetUpWaterIndices();

            //SetUpWaterVertices(); OLD
            //waterVertexDeclaration = new VertexDeclaration(device, VertexWater.VertexElements); 

        }

        private void UpdateWaterVertices()
        {
            Vector2 windowPos = The.MapUI.MapWindowWorldPosition;

            float bumpTextureUTotal = (float)waterQuadWidth / (float)waterBumpMap.Width;
            float bumpTextureVTotal = (float)waterQuadHeight / (float)waterBumpMap.Height;

            float topLeftU = windowPos.X / waterBumpMap.Width;
            float topLeftV = windowPos.Y / waterBumpMap.Height;

            waterVertices[0].BumpTextureCoordinate = new Vector2(topLeftU, topLeftV);
            waterVertices[2].BumpTextureCoordinate = new Vector2(topLeftU, topLeftV + bumpTextureVTotal);
            waterVertices[1].BumpTextureCoordinate = new Vector2(topLeftU + bumpTextureUTotal, topLeftV);

            waterVertices[3].BumpTextureCoordinate = new Vector2(topLeftU, topLeftV + bumpTextureVTotal);
            waterVertices[5].BumpTextureCoordinate = new Vector2(topLeftU + bumpTextureUTotal, topLeftV + bumpTextureVTotal);
            waterVertices[4].BumpTextureCoordinate = new Vector2(topLeftU + bumpTextureUTotal, topLeftV);


        }

        private void SetUpWaterIndices()
        {
           /* int noOfVerticesVertical = renderer.noOfVerticesVertical;
            int noOfVerticesHorizontal = renderer.noOfVerticesHorizontal;
            */
            int noOfVerticesVertical = renderer.noOfVerticesVertical;
            int noOfVerticesHorizontal = renderer.noOfVerticesHorizontal + 2;

            int counter = 0;
            for (int y = 0; y < noOfVerticesVertical - 1; y++)
            {
                for (int x = 0; x < noOfVerticesHorizontal - 1; x++)
                {
                    short topLeft = (short)(x + y * noOfVerticesHorizontal);
                    short topRight = (short)((x + 1) + y * noOfVerticesHorizontal);
                    short lowerRight = (short)((x + 1) + (y + 1) * noOfVerticesHorizontal);
                    short lowerLeft = (short)(x + (y + 1) * noOfVerticesHorizontal);

                    waterIndices[counter++] = topLeft;
                    waterIndices[counter++] = lowerRight;
                    waterIndices[counter++] = lowerLeft;

                    waterIndices[counter++] = topLeft;
                    waterIndices[counter++] = topRight;
                    waterIndices[counter++] = lowerRight;
                }
            }
        }


        private void SetupSkyVertices()
        {
            Dimension dim = The.Client.Controller.DrawArea;
            float width = dim.Width; // The.Client.GraphicsDevice.Viewport.Width;
            float height = dim.Height; // The.Client.GraphicsDevice.Viewport.Height;

            float skyHeight = -1000f;

            skyQuad[0] = new VertexPositionNormalTexture();
            skyQuad[0].Position = new Vector3(0, 0, skyHeight);
            skyQuad[0].TextureCoordinate = new Vector2(0, 0);

            skyQuad[2] = new VertexPositionNormalTexture();
            skyQuad[2].Position = new Vector3(0, height, skyHeight);
            skyQuad[2].TextureCoordinate = new Vector2(0, 1);

            skyQuad[1] = new VertexPositionNormalTexture();
            skyQuad[1].Position = new Vector3(width, 0, skyHeight);
            skyQuad[1].TextureCoordinate = new Vector2(1, 0);

            skyQuad[3] = new VertexPositionNormalTexture();
            skyQuad[3].Position = new Vector3(0, height, skyHeight);
            skyQuad[3].TextureCoordinate = new Vector2(0, 1);

            skyQuad[5] = new VertexPositionNormalTexture();
            skyQuad[5].Position = new Vector3(width, height, skyHeight);
            skyQuad[5].TextureCoordinate = new Vector2(1, 1);

            skyQuad[4] = new VertexPositionNormalTexture();
            skyQuad[4].Position = new Vector3(width, 0, skyHeight);
            skyQuad[4].TextureCoordinate = new Vector2(1, 0);


            // Provide a normal for each vertex
            for (int i = 0; i < skyQuad.Length; i++)
            {
                skyQuad[i].Normal = Vector3.UnitZ;
            }

        }

        /// <summary>
        /// Recompute visible vertices every frame
        /// </summary>
      /*  public void SetUpWaterVerticesInCurrentView()
        {
            TerrainTile tileToDraw;

            Terrain centerTerrain;

            int lastXToDraw = The.MapUI.mapX + game.Map.noOfTilesToDisplayHorizontally + 2;  // add one for the edges! we are starting at -2 - that gives four extra tiles.
            int lastYToDraw = The.MapUI.mapY + game.Map.noOfTilesToDisplayVertically + 2;

            currentWaterVertex = 0;
            float xPos, yPos;

            for (int y = The.MapUI.mapY - 2; y < lastYToDraw; y++)
            {
                yPos = y * MapManager.tileSize + MapManager.tileSizeOver2;
                for (int x = The.MapUI.mapX - 2; x < lastXToDraw; x++)
                {
                    xPos = x * MapManager.tileSize + MapManager.tileSizeOver2;

                    int closestXOnMap = Common.Clamp(x, 0, The.MapUI.mapWidth - 1);
                    int closestYOnMap = Common.Clamp(y, 0, The.MapUI.mapHeight - 1);
                    tileToDraw = game.Map.TileMap[closestXOnMap][closestYOnMap];

                    centerTerrain = tileToDraw.GetCenterTerrain();

                    if (centerTerrain.LevelBelowWater > 0f)
                    {

                        // use negative water levels for tiles above water:
                        waterVertices[currentWaterVertex].WaterDepth = centerTerrain.LevelBelowWater;


                        waterVertices[currentWaterVertex].Position = new Vector3(xPos, yPos, WaterHeight);

                        // use the refraction map texture that covers the whole drawing area / viewport:
                        waterVertices[currentWaterVertex].TextureCoordinate.X = xPos / waterQuadWidth; //xPos / 480f;
                        waterVertices[currentWaterVertex].TextureCoordinate.Y = yPos / waterQuadHeight; /// 480f;
                        /// 
                        // set the ripple texture uvs:
                        waterVertices[currentWaterVertex].BumpTextureCoordinate = new Vector2(xPos / (float)waterBumpMap.Width, yPos / (float)waterBumpMap.Height);

                        waterVertices[currentWaterVertex].WaterColor = tileToDraw.ColorOfWater; // new Vector4(0.3f, 0.3f, 0.5f, 1.0f);
                        waterVertices[currentWaterVertex].BottomTint = tileToDraw.WaterBottomTint;

                        currentWaterVertex++;
                    }
                }

            }

        }
       */
        /// <summary>
        /// Recompute visible vertices every frame
        /// </summary>
        public void SetUpWaterVerticesAndIndicesInCurrentView()
        {
            TerrainTile tileToDraw;
            Terrain centerTerrain;

            int lastXToDraw = The.MapUI.mapWindowTileX + The.MapUI.noOfTilesToDisplayHorizontally + 4;//2;  // add one for the edges! we are starting at -2 - that gives four extra tiles.
            int lastYToDraw = The.MapUI.mapWindowTileY + The.MapUI.noOfTilesToDisplayVertically + 2;  

            currentWaterVertex = 0;
            float xPos, yPos;
                              
            for (int y = The.MapUI.mapWindowTileY - 2; y < lastYToDraw; y++)
            {
                yPos = y * MapManager.tileSize + MapManager.tileSizeOver2+10;
                for (int x = The.MapUI.mapWindowTileX - 2; x < lastXToDraw; x++)
                {
                    xPos = x * MapManager.tileSize + MapManager.tileSizeOver2;

                    int closestXOnMap = Common.Clamp(x, 0,  The.Map.mapTileWidth - 1);
                    int closestYOnMap = Common.Clamp(y, 0, The.Map.mapTileHeight - 1);
                    tileToDraw = The.Map.TileMap[closestXOnMap][closestYOnMap];

                    centerTerrain = tileToDraw.GetCenterTerrain();

                  /*  if (tileToDraw.LevelBelowWater > 0f)
                    {*/

                        // use negative water levels for tiles above water:
                    waterVertices[currentWaterVertex].WaterDepth = centerTerrain.LevelBelowWater;


                    waterVertices[currentWaterVertex].Position = new Vector3(xPos, yPos, WaterHeight);

                    // use the refraction map texture that covers the whole drawing area / viewport:
                    waterVertices[currentWaterVertex].TextureCoordinate.X = xPos / waterQuadWidth; //xPos / 480f;
                    waterVertices[currentWaterVertex].TextureCoordinate.Y = yPos / waterQuadHeight; /// 480f;
                    /// 
                    // set the ripple texture uvs:
                    waterVertices[currentWaterVertex].BumpTextureCoordinate = new Vector2(xPos / (float)waterBumpMap.Width, yPos / (float)waterBumpMap.Height);

                    waterVertices[currentWaterVertex].WaterColor = tileToDraw.ColorOfWater; // new Vector4(0.3f, 0.3f, 0.5f, 1.0f);
                    waterVertices[currentWaterVertex].BottomTint = tileToDraw.WaterBottomTint;

                    currentWaterVertex++;
                   // }
                }

            }

        }

        private void DrawSkyQuad() //Plane clipPlane)
        {
           
            // look up at the sky:
            skyQuadEffect.View = reflectionViewMatrix;
            // keep the sky moving with the camera:
            skyQuadEffect.World = Matrix.CreateTranslation(new Vector3(The.MapUI.MapWindowWorldPosition, 0f));

            // perhaps add clipping later, for now we only draw a reflection of the sky, so we don't bother...
           // skyQuadEffect.Parameters["ClipPlane0"].SetValue(new Vector4(clipPlane.Normal, clipPlane.D));

           
            foreach (EffectPass pass in skyQuadEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.DrawUserPrimitives<VertexPositionNormalTexture>(
                    PrimitiveType.TriangleList, skyQuad, 0, 2);
            }
        }

    /*    private void DrawSkyDome(Matrix currentViewMatrix)
        {
            // Scale?? ???
            float scale = 48f * 100f;

            device.RenderState.DepthBufferWriteEnable = false;

            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(cameraPosition);
            //     Matrix wMatrix = Matrix.CreateTranslation(0, 0, 0) * Matrix.CreateScale(scale) * Matrix.CreateRotationX(MathHelper.Pi) * Matrix.CreateTranslation(cameraPosition);

            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(cloudMap);
                    currentEffect.Parameters["xEnableLighting"].SetValue(false);
                }
                mesh.Draw();
            }
            device.RenderState.DepthBufferWriteEnable = true;
        }*/

        public void UpdateReflectedViewMatrix()
        {            
          /*  Vector3 cameraOriginalTarget = cameraPosition; // new Vector3(0, -1, 0);
            cameraOriginalTarget.Y = 0f;
            Vector3 cameraOriginalUpVector = Vector3.Cross(cameraOriginalTarget - cameraPosition, Vector3.Left); //new Vector3(-1, 0, 0);
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraOriginalTarget, cameraOriginalUpVector);

            Vector3 cameraFinalTarget = cameraOriginalTarget; // cameraPosition + cameraRotatedTarget;
            */

            // the reflected camera should look straight up at the sky!
            Vector3 reflectionCameraPosition = renderer.TerrainCameraPosition;
            reflectionCameraPosition.Z = WaterHeight;
            Vector3 reflectionTargetPos = reflectionCameraPosition;
            reflectionTargetPos.Z = -1000f;

            /*    reflTargetPos.Y = -cameraFinalTarget.Y + waterHeight * 2;

                Vector3 cameraRight = Vector3.Transform(new Vector3(1, 0, 0), cameraRotation);*/
            Vector3 invUpVector = Vector3.Cross(Vector3.Left, reflectionTargetPos - reflectionCameraPosition);

            reflectionViewMatrix = Matrix.CreateLookAt(reflectionCameraPosition, reflectionTargetPos, invUpVector);
        }
        
        public void DrawRefractionMap(List<TerrainBatch> terrainBatches)
        {

            if (refractionRenderTarget.IsContentLost == true)
            {
                PresentationParameters pp = device.PresentationParameters;
                Dimension drawArea = The.Client.Controller.DrawArea;
                refractionRenderTarget = new RenderTarget2D(device, drawArea.Width, drawArea.Height, //pp.BackBufferWidth, pp.BackBufferHeight, 
                                    false, pp.BackBufferFormat,
                                    pp.DepthStencilFormat);
            }

            //device.SetRenderTarget(refractionRenderTarget);
            //device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            //renderer.DrawTerrainUserVertices(renderer.TerrainViewMatrix, terrainBatches, null);
            //device.SetRenderTarget(null);
            refractionRenderTarget = renderer.terrainSlicedMap.refractionMap;
 


            //Save one clipped texutre and one unclipped in same call or make new shader that return black / white tiles only ? TODO:
            


            

            
            //System.IO.Stream ss = System.IO.File.OpenWrite("Refraction.png");
            //clipRenderTarget.SaveAsPng(ss, 1024, 768);
            //ss.Close();


        }

        public void DrawReflectionMap()
        {
            // add clipping later if we ever want to reflect anything other than the sky, like vehicles or ships...
            // see diskussion here:
            // http://www.riemers.net/Forum/index.php?var=2006&var2=0
           /* Plane reflectionPlane = CreatePlane(WaterHeight + 0.5f, new Vector3(0, 0, -1), reflectionViewMatrix, false); // true
            device.ClipPlanes[0].Plane = reflectionPlane;
            device.ClipPlanes[0].IsEnabled = true;*/

            
            if (reflectionRenderTarget.IsContentLost == true)
            {
                PresentationParameters pp = device.PresentationParameters;
                Dimension drawArea = The.Client.Controller.DrawArea;
                reflectionRenderTarget = new RenderTarget2D(device, drawArea.Width, drawArea.Height, // pp.BackBufferWidth, pp.BackBufferHeight, 
                                    false, pp.BackBufferFormat,
                                    pp.DepthStencilFormat);
            }


            device.SetRenderTarget(reflectionRenderTarget);

          //  GameWorldRenderer.SaveRenderTargetToFile("refractionRenderTarget", refractionRenderTarget);

            // only draw planes, ships and so on?
            // maybe later...
       //     game.map.DrawTerrainUserVertices(reflectionViewMatrix, 0f, true);

            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            // draw sky as a quad instead?
       //     DrawSkyDome(reflectionViewMatrix);
            DrawSkyQuad(); //reflectionPlane);

          //  device.ClipPlanes[0].IsEnabled = false;

            /*
            if (game.AntiAliasingEnabled)
            {
                graphics.GraphicsDevice.DepthStencilBuffer = game.MultiSamplingStencilBuffer; 
            }*/

         //   reflectionMap = reflectionRenderTarget.GetTexture();

         //   reflectionMap.Save("reflectionMap.jpg", ImageFileFormat.Jpg);

        }

        Vector3 sunPositionSpecularityHack = new Vector3(0f, 0f, 1.5f); //	{X:-0,9 Y:-1,2 Z:2}	intensity: 20
        Vector3 camPositionSpecularityHack = new Vector3(0f, -3600f, 0f); //	{X:-0,9 Y:-1,2 Z:2}	intensity: 20

        float specularIntensity = 12f; //11f
        public void DrawWater(GameTime gameTime)
        {
            totalElapsedUnpausedTime = (float)(The.Sim.TotalUnPausedGameTime.TotalMilliseconds / 100f);

           /* if (!game.IsPaused)
            {
                
                totalElapsedUnpausedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 100.0f;
            }*/


            // OLD:
            //float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;

            //UpdateReflectedViewMatrix();
            // OLD - single quad water:
            // UpdateWaterVertices();
            // NEW - same number of quads as terrain:
            SetUpWaterVerticesAndIndicesInCurrentView();

            waterEffect.CurrentTechnique = waterEffect.Techniques["Water"];
            Vector2 windowPos = The.MapUI.MapWindowWorldPosition;
            Matrix worldMatrix = Matrix.Identity; // OLD: Matrix.CreateTranslation(new Vector3(windowPos.X, windowPos.Y, 0f)); 
            waterEffect.Parameters["xWorld"].SetValue(worldMatrix);
          //  waterEffect.Parameters["xView"].SetValue(renderer.TerrainViewMatrix);
         //   waterEffect.Parameters["xProjection"].SetValue(UWGame.SimSide.Instance.Projection);
          //  waterEffect.Parameters["xReflectionView"].SetValue(reflectionViewMatrix);
            waterEffect.Parameters["reflectWorldViewProjection"].SetValue(reflectionViewMatrix * The.Client.Projection);
            waterEffect.Parameters["refractWorldViewProjection"].SetValue(renderer.TerrainViewMatrix * The.Client.Projection);

         
            waterEffect.Parameters["xReflectionMap"].SetValue(reflectionRenderTarget); //reflectionMap);
            waterEffect.Parameters["xRefractionMap"].SetValue(renderer.terrainSlicedMap.refractionMap); //refractionMap);
            waterEffect.Parameters["xClipMap"].SetValue(renderer.terrainSlicedMap.clipMap); //refractionMap);

            waterEffect.Parameters["xWaterBumpMap"].SetValue(waterBumpMap);
            waterEffect.Parameters["xWaterBumpMapLarge"].SetValue(waterBumpMapLarge);
            waterEffect.Parameters["xWaveLength"].SetValue(0.8f); // 0.2f); //0.1f);
            waterEffect.Parameters["xWaveHeight"].SetValue(0.1f); //0.03f);//0.4f); //0.3f);

            Vector3 xCamPos = renderer.TerrainCameraPosition;
          //  xCamPos += camPositionSpecularityHack;

            waterEffect.Parameters["xCamPos"].SetValue(xCamPos);
            waterEffect.Parameters["camPositionSpecularityHack"].SetValue(camPositionSpecularityHack);
            waterEffect.Parameters["specularIntensity"].SetValue(Common.ClampBottom(The.Sim.PlaySite.PlaySite.Weather.SunIntensity, 0.5f) * specularIntensity);
                              
            //{X:0 Y:0,8101608 Z:-0,5862078}
            // NEW: Fixed specularity pos:
            Vector3 xLightDirection = new Vector3(0f, -0.81016f, 0.58620f); 
            // OLD:
            // Vector3 xLightDirection = -DateAndTime.Instance.SunPosition;

            xLightDirection += sunPositionSpecularityHack;

            waterEffect.Parameters["xLightDirection"].SetValue(xLightDirection); // new Vector3(-0.5f, -1, -0.5f));
        

            waterEffect.Parameters["xTime"].SetValue(totalElapsedUnpausedTime); // time);
            waterEffect.Parameters["xWindForce"].SetValue(0.001f);
            // effect.Parameters["xWindForce"].SetValue(0.002f);            
            
            // disabled wind direction due to shader? bug
            waterEffect.Parameters["xWindDirection"].SetValue(new Vector3(1f, 0.15f, 0f)); //new Vector3(WeatherManager.Instance.WindDirection, 0f));

            
            foreach (EffectPass pass in waterEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                               
               
               // device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, waterVertices, 0, waterVertices.Length, waterIndices, 0, waterIndices.Length / 3);
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, waterVertices, 0, currentWaterVertex, waterIndices, 0, waterIndices.Length / 3);
               /* System.IO.Stream ss = System.IO.File.OpenWrite("waterTEXTtestX.jpg");
                renderTarget.SaveAsJpeg(ss, 1024, 768);
                ss.Close();*/
                // draw only the tiles that can be seen?
               // device.DrawUserPrimitives(PrimitiveType.TriangleList, waterVertices, 0, currentWaterVertex / 3);
               
            }
        }

       
    }

    public struct VertexWater : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector2 BumpTextureCoordinate;
        public float WaterDepth;
        public Vector4 WaterColor;
        public Vector4 BottomTint;

        public static int SizeInBytes = (3 + 2 + 2 + 1 + 4 + 4) * sizeof(float);
        public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1 ),
             new VertexElement(sizeof(float) * 7, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2 ),
             new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3 ),
             new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4 )
         };

        private readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }
    }
}
