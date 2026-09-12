using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using WindowSystem;
using System.IO;
using UWGame.ClientSide.Map;
using GameStateManagement;

namespace UWGame.ClientSide.Interface
{
    public class DisplayPanelRenderer
    {
        public Texture2D scanlines, /*crtReflectionsBig, crtReflectionsSmall, lcdReflections, lcdReflectionLampCorner,*/ dropshadow, fingerprint, /*backgroundNoise,*/ dustOnEdges, cleanedLCD, cleanedMoreLCD, dust;//ball;

        public Effect CRTEffect;
        public Effect LCDEffect;

        private VertexCRTQuad[] crtDisplayVertices;
        // use 16 bit indices to support older cards:
        private short[] allCRTIndices;

        private int noOfCRTQuads = 6;
        int DisplayQuadIndex;

        private VertexLCDQuad[] lcdDisplayVertices;
        private short[] lcdIndices;

        private int noOfLCDQuads = 6;
        int LCDQuadIndex;

        public Dictionary<Level, List<CRTScreen>> CRTPanels = new Dictionary<Level, List<CRTScreen>>();
        public Dictionary<Level, List<LCDScreen>> LCDPanels = new Dictionary<Level, List<LCDScreen>>();

        public RenderTarget2D DisplayPanelContentRenderTarget;

        int scanlinesWidth, scanlinesHeight /*lcdBigReflectionWidth, lcdBigReflectionHeight, lcdSmallReflectionWidth, lcdSmallReflectionHeight, bigReflectionWidth, bigReflectionHeight, smallReflectionWidth, smallReflectionHeight*/;
        
        GraphicsDevice device;
        //GraphicsDeviceManager graphics;
       // UWGame.SimSide game;
        UnclaimedWorld game;

        
        public DisplayPanelRenderer(UnclaimedWorld game) // UWGame.SimSide game)
        {
            this.game = game;

            LCDPanels.Add(Level.FoggyBottom, new List<LCDScreen>());
            LCDPanels.Add(Level.RockBottom, new List<LCDScreen>());
            LCDPanels.Add(Level.Bottom, new List<LCDScreen>());

            LCDPanels.Add(Level.BelowBelowBelowMiddle, new List<LCDScreen>());
            LCDPanels.Add(Level.BelowBelowMiddle, new List<LCDScreen>());
            LCDPanels.Add(Level.BelowMiddle, new List<LCDScreen>());
            LCDPanels.Add(Level.Middle, new List<LCDScreen>());

           // LCDPanels.Add(Level.EntityTypeInfo, new List<LCDScreen>());            
            LCDPanels.Add(Level.Dialogs, new List<LCDScreen>());
            LCDPanels.Add(Level.StackedDialogs, new List<LCDScreen>());
            LCDPanels.Add(Level.EventDialog, new List<LCDScreen>());
            LCDPanels.Add(Level.Menu, new List<LCDScreen>());
            LCDPanels.Add(Level.MessageBox, new List<LCDScreen>());

            CRTPanels.Add(Level.FoggyBottom, new List<CRTScreen>());
            CRTPanels.Add(Level.RockBottom, new List<CRTScreen>());
            CRTPanels.Add(Level.Bottom, new List<CRTScreen>());

            CRTPanels.Add(Level.BelowBelowBelowMiddle, new List<CRTScreen>());
            CRTPanels.Add(Level.BelowBelowMiddle, new List<CRTScreen>());
            CRTPanels.Add(Level.BelowMiddle, new List<CRTScreen>());
            CRTPanels.Add(Level.Middle, new List<CRTScreen>());
          //  CRTPanels.Add(Level.EntityTypeInfo, new List<CRTScreen>());
            CRTPanels.Add(Level.Dialogs, new List<CRTScreen>());
            CRTPanels.Add(Level.StackedDialogs, new List<CRTScreen>());
            CRTPanels.Add(Level.EventDialog, new List<CRTScreen>());
            CRTPanels.Add(Level.Menu, new List<CRTScreen>());
            CRTPanels.Add(Level.MessageBox, new List<CRTScreen>());
         //   CRTPanels.Add(Level.Tooltip, new List<CRTScreen>());



            device = game.GraphicsDevice;
            PresentationParameters pp = device.PresentationParameters;

            // shared by both display types:
            /*DisplayPanelContentRenderTarget = new RenderTarget2D(device, // XNA 3
                pp.BackBufferWidth, pp.BackBufferHeight, 1,
                pp.BackBufferFormat, game.MultiSampleTypeToUse, 0);*/

            // no depth buffer. With multisampling! 
           /* DisplayPanelContentRenderTarget = new RenderTarget2D(device,
                pp.BackBufferWidth, pp.BackBufferHeight, false,
                pp.BackBufferFormat, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);*/

            // #MONOCHANGE - BEFORE: RenderTargetUsage param causes crash https://github.com/mono/MonoGame/issues/4814
            // we need the depth buffer to render 3d properly!
          /*  DisplayPanelContentRenderTarget = new RenderTarget2D(device,
                pp.BackBufferWidth, pp.BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            */

            // #MONOCHANGE - AFTER: 
            DisplayPanelContentRenderTarget = new RenderTarget2D(device,
                pp.BackBufferWidth, pp.BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat);

        }

        public void Destroy()
        {
            DisplayPanelContentRenderTarget.Dispose();
        }

        public void Initialize()
        {
            crtDisplayVertices = new VertexCRTQuad[noOfCRTQuads * 4];
            allCRTIndices = new short[noOfCRTQuads * 6];
            GameWorldRenderer.SetUpIndices(noOfCRTQuads, allCRTIndices);

            lcdDisplayVertices = new VertexLCDQuad[noOfLCDQuads * 4];
            lcdIndices = new short[noOfLCDQuads * 6];
            GameWorldRenderer.SetUpIndices(noOfLCDQuads, lcdIndices);

           /* CRTPanels.Add(new CRTPanel(new Rectangle(400, 200, 400, 300), new Rectangle(400, 200, 400, 300), CRTContentRenderTarget.Width, CRTContentRenderTarget.Height,
                scanlines.Width, scanlines.Height)); */

            
        }

        public void LoadContent()
        {
            device = game.GraphicsDevice;
            
            if (game.Controller.GraphicsLevelSetting == Control.Controller.GraphicsLevel.High)
            {
                scanlines = game.Content.Load<Texture2D>("GUI\\CRT_ScanLines");
                cleanedLCD = game.Content.Load<Texture2D>("GUI\\LCD_Cleaned");
                cleanedMoreLCD = game.Content.Load<Texture2D>("GUI\\LCD_Cleaned_more");
               // crtReflectionsBig = game.Content.Load<Texture2D>("GUI\\CRT_Lamp_reflections");
              //  crtReflectionsSmall = game.Content.Load<Texture2D>("GUI\\CRT_Lamp_Reflections_Small");  // white rect, Morten nulled it //"GUI\\Plain\\lcd_reflex");
              //  lcdReflections = game.Content.Load<Texture2D>("GUI\\LCD_CircularReflex");  // white rect, Morten nulled it // omit this from GUI sprite sheet eventually, it is not needed there.
              //  lcdReflectionLampCorner = game.Content.Load<Texture2D>("GUI\\Lamp_Reflection_Corner");

                // scales with frame edges:               
                //backgroundNoise = game.Content.Load<Texture2D>("GUI\\EmptyBG_Dark");
                dustOnEdges = game.Content.Load<Texture2D>("GUI\\CRT_Grunge_v2");
                dust = game.Content.Load<Texture2D>("GUI\\Comm\\event_dust"); // used by LCD; also part of gui sprite sheet!

                CRTEffect = game.Content.Load<Effect>("GUI\\CRT");
                LCDEffect = game.Content.Load<Effect>("GUI\\LCD");

                scanlinesWidth = scanlines.Width; 
                scanlinesHeight = scanlines.Height;
             /*   bigReflectionWidth = crtReflectionsBig.Width;
                bigReflectionHeight = crtReflectionsBig.Height;
                smallReflectionWidth = crtReflectionsSmall.Width;
                smallReflectionHeight = crtReflectionsSmall.Height;

                lcdBigReflectionWidth = lcdReflections.Width;
                lcdBigReflectionHeight = lcdReflections.Height;

                lcdSmallReflectionWidth = lcdReflectionLampCorner.Width;
                lcdSmallReflectionHeight = lcdReflectionLampCorner.Height;
                */
            /*    device = game.GraphicsDevice;
                PresentationParameters pp = device.PresentationParameters;


                // shared by both display types:
                DisplayPanelContentRenderTarget = new RenderTarget2D(device,
                    pp.BackBufferWidth, pp.BackBufferHeight, 1,
                    pp.BackBufferFormat, game.MultiSampleTypeToUse, 0);*/
            }
        }

     /*   public ModelDisplayCRT AddModelDisplayCRT(Point sourcePos, Point destPos, int width, int height, Level level, Window window)
        {
            ModelDisplayCRT modelDisplayCRT = new ModelDisplayCRT(new Rectangle(sourcePos.X, sourcePos.Y, width, height), new Rectangle(destPos.X, destPos.Y, width, height),
                    DisplayPanelContentRenderTarget.Width, DisplayPanelContentRenderTarget.Height,
                    effectTextureWidth, effectTextureHeight, window);
            CRTPanels[level].Add(modelDisplayCRT);
            return modelDisplayCRT;
        }*/

     /*   public void AddMinimapDisplayCRT(Point destPos, int width, int height)
        {
             CRTPanels.Add(new MinimapCRT(destPos, new Rectangle(destPos.X, destPos.Y, width, height),
                    DisplayPanelContentRenderTarget.Width, DisplayPanelContentRenderTarget.Height,
                    effectTextureWidth, effectTextureHeight));
        }*/

        public CRTScreen AddCRT(UIComponent displayBox, Point destPos, int width, int height, Level level, Window window, 
            ReflectionToUse toUse, bool isMonochrome, float alpha = 1f)
        {
            return AddCRT(displayBox, new Rectangle(destPos.X, destPos.Y, width, height), new Rectangle(destPos.X, destPos.Y, width, height),
                level, window, toUse, isMonochrome, alpha);
        }

        public CRTScreen AddCRT(UIComponent displayBox, Rectangle source, Rectangle destination, Level level, Window window,
            ReflectionToUse toUse, bool isMonochrome, float alpha = 1f)
        {
            CRTScreen crt = new CRTScreen(displayBox, source, destination, 
                   DisplayPanelContentRenderTarget.Width, DisplayPanelContentRenderTarget.Height,
                   scanlinesWidth, scanlinesHeight,
                   //bigReflectionWidth, bigReflectionHeight, smallReflectionWidth, smallReflectionHeight,
                   isMonochrome, toUse, window, alpha);

            CRTPanels[level].Add(crt);

          //  crt.DisplayBox = displayBox;
          
            return crt;
        }

        public LCDScreen AddLCD(UIComponent displayBox, //Point destPos, int width, int height, 
            Level level, Window window, bool drawDust) //, LCDScreen.ReflectionToUse reflection)
        {
            Rectangle rect = new Rectangle(displayBox.X, displayBox.Y, displayBox.Width, displayBox.Height);

           /* int reflectionWidth, reflectionHeight;
            if (reflection == LCDScreen.ReflectionToUse.Circular)
            {
                reflectionWidth = lcdBigReflectionWidth;
                reflectionHeight = lcdBigReflectionHeight;
            }
            else
            {
                reflectionWidth = lcdSmallReflectionWidth;
                reflectionHeight = lcdSmallReflectionHeight;
            }*/

            LCDScreen lcd = new LCDScreen(displayBox, rect, rect,               
                     DisplayPanelContentRenderTarget.Width, DisplayPanelContentRenderTarget.Height,
                     DisplayPanelContentRenderTarget.Width, DisplayPanelContentRenderTarget.Height, //reflectionWidth, reflectionHeight, 
                     dust.Width, dust.Height, window, drawDust); //, reflection);

            LCDPanels[level].Add(lcd);

           // lcd.DisplayBox = displayBox;
           
            return lcd;
        }

        public void Update(GameTime gameTime)
        {
        /*    if (CRTPanels.Count == 0)
           {
                 CRTPanels.Add(new ModelDisplayCRT(new Rectangle(400, 200, scanlines.Width, scanlines.Height), new Rectangle(400, 260, scanlines.Width, scanlines.Height),
                    DisplayPanelContentRenderTarget.Width, DisplayPanelContentRenderTarget.Height,
                    scanlines.Width, scanlines.Height));
                
               CRTPanels.Add(new MinimapCRT(new Point(10, 400), new Rectangle(10, 700, 400, 300),
                    DisplayPanelContentRenderTarget.Width, DisplayPanelContentRenderTarget.Height,
                    scanlines.Width, scanlines.Height));
                
           }*/

        /*   foreach (KeyValuePair<Level, List<CRTScreen>> kvp in CRTPanels)
           {
               foreach (CRTScreen panel in kvp.Value)
               {
                   panel.Update(gameTime);
               }
           }*/
        }

        public void StartPanelRendering()
        {
            // draw panel contents:
            device.SetRenderTarget(DisplayPanelContentRenderTarget);
           // device.DepthStencilBuffer = game.ScreenManager.MultiSamplingStencilBuffer; // XNA 3: for drawing models...       

            Color clearColor = new Color(0, 0, 0, 0);

            //device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearColor, 1.0f, 0); // xna 3
            device.Clear(ClearOptions.Target, clearColor, 1.0f, 0); // we no longer have a depth buffer...

        }

        public void Draw(Level level) //int CRTQuadIndex)
        {      
            
            
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            game.Controller.SetZoomRenderTaget(); //game.GraphicsDevice.SetRenderTarget(null);
         
            
          /*  if (level == Level.Bottom)
            {
                using (Stream stream = File.Create("displayPanel.png"))
                {
                    DisplayPanelContentRenderTarget.SaveAsPng(stream, DisplayPanelContentRenderTarget.Width, DisplayPanelContentRenderTarget.Height);
                }               

            }*/

         
           
            DrawCRTsAtThisLevel(level);

            DrawLCDsAtThisLevel(level);
           
            
        }

        private void DrawCRTsAtThisLevel(Level level)
        {
            List<CRTScreen> list;
            if (!CRTPanels.TryGetValue(level, out list) || list.Count == 0)
                return;

            //SetupQuadVertices();
            DisplayQuadIndex = 0;
            foreach (CRTScreen panel in CRTPanels[level])
            {
                if (panel.ParentWindow != null && panel.ParentWindow.Visible == false)
                {
                    continue;
                }

                panel.CopyQuadToVertexBuffer(crtDisplayVertices, DisplayQuadIndex);
                DisplayQuadIndex++;
            }

            if (DisplayQuadIndex == 0)
                return;

          
        /*    if (level == Level.Dialogs)
            {            
                GameWorldRenderer.SaveTextureToFile("crtContent", DisplayPanelContentRenderTarget);
            }*/
            
                       

          /*  if (game.ScreenManager.PixelShaderVersion > 2) 
            {*/
                CRTEffect.CurrentTechnique = CRTEffect.Techniques["CRT_HighQuality"];
           /* }
            else
            {
                CRTEffect.CurrentTechnique = CRTEffect.Techniques["CRT"];
            }*/

            CRTEffect.Parameters["ScanlinesTexture"].SetValue(scanlines);

           /* CRTEffect.Parameters["BigReflectionTexture"].SetValue(crtReflectionsBig); //crtReflections2); //);
            CRTEffect.Parameters["SmallReflectionTexture"].SetValue(crtReflectionsSmall);
            */

            //CRTEffect.Parameters["BallTexture"].SetValue(ball);
            // CRTEffect.Parameters["NoiseBackgroundTexture"].SetValue(backgroundNoise);

            CRTEffect.Parameters["GrungeTexture"].SetValue(dustOnEdges);
            CRTEffect.Parameters["PanelContentTexture"].SetValue(DisplayPanelContentRenderTarget);

           // Viewport viewport = game.GraphicsDevice.Viewport;
            Dimension dim = game.Controller.DrawArea;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
            CRTEffect.Parameters["ViewportSize"].SetValue(viewportSize);
            //      CRTEffect.Parameters["WindowPosition"].SetValue(map.mapWindowWorldPosition);

            /*       CRTEffect.Parameters["CloudCoverLimit"].SetValue(1f - Weather.Instance.CloudCover);
                   CRTEffect.Parameters["CloudPosition"].SetValue(Weather.Instance.CloudPosition);
                   CRTEffect.Parameters["ShadowAlpha"].SetValue(DateAndTime.Instance.GetDropShadowAlphaFactor());
                   */

         //   game.GraphicsDevice.VertexDeclaration = crtVertexDeclaration;

          //  CRTEffect.Begin();
            foreach (EffectPass pass in CRTEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                game.GraphicsDevice.DrawUserIndexedPrimitives(
                       PrimitiveType.TriangleList, crtDisplayVertices, 0, DisplayQuadIndex * 4, allCRTIndices, 0, DisplayQuadIndex * 2);

            }
        }

        private void DrawLCDsAtThisLevel(Level level)
        {
            List<LCDScreen> list;
            if (!LCDPanels.TryGetValue(level, out list) || list.Count == 0)
                return;
          

            //SetupQuadVertices();
            DisplayQuadIndex = 0;

           
           foreach (LCDScreen panel in LCDPanels[level])
           {
               // cull invisible windows
               if (panel.ParentWindow != null && panel.ParentWindow.Visible == false)
               {
                   continue;
               }

               panel.CopyQuadToVertexBuffer(lcdDisplayVertices, DisplayQuadIndex);
               DisplayQuadIndex++;
           }

           if (DisplayQuadIndex == 0)
               return;

            float y = lcdDisplayVertices[0].Position.Y;


           /* if (game.ScreenManager.PixelShaderVersion > 2) //UWGame.SimSide.Instance.PixelShaderVersion >= 3)
            {*/
             //   LCDEffect.CurrentTechnique = LCDEffect.Techniques["LCDFancy"];
           /* }
            else
            {*/
                LCDEffect.CurrentTechnique = LCDEffect.Techniques["LCD"];
           /* }*/


           /* LCDEffect.Parameters["ReflectionTexture"].SetValue(lcdReflections); //crtReflectionsSmall); // 
            LCDEffect.Parameters["LampReflectionTexture"].SetValue(lcdReflectionLampCorner); //crtReflectionsSmall);
            */

            //CRTEffect.Parameters["BallTexture"].SetValue(ball);
            // CRTEffect.Parameters["NoiseBackgroundTexture"].SetValue(backgroundNoise);

            LCDEffect.Parameters["GrungeTexture"].SetValue(dust);
            LCDEffect.Parameters["CleanedTexture"].SetValue(cleanedMoreLCD); // cleanedLCD);
            LCDEffect.Parameters["PanelContentTexture"].SetValue(DisplayPanelContentRenderTarget);

            //Viewport viewport = game.GraphicsDevice.Viewport;
            Dimension dim = game.Controller.DrawArea;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
            //Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            LCDEffect.Parameters["ViewportSize"].SetValue(viewportSize);


          //  game.GraphicsDevice.VertexDeclaration = lcdVertexDeclaration;

           // LCDEffect.Begin();
            foreach (EffectPass pass in LCDEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                game.GraphicsDevice.DrawUserIndexedPrimitives(
                       PrimitiveType.TriangleList, lcdDisplayVertices, 0, DisplayQuadIndex * 4, lcdIndices, 0, DisplayQuadIndex * 2);

            }
            
        }
    }

   
}
