using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using System.Windows.Forms;
using WindowSystem;
using InputEventSystem;
using System.IO;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;

namespace UWGame.ClientSide.Interface  
{
    public abstract class CommonInterface
    {
        public UnclaimedWorld Game;

        //public InputEvents input;
        public GUIManager gui;

        System.Windows.Forms.Form windowForm;

        public DisplayPanelRenderer DisplayPanelRenderer;

        //public GraphicsLevelSetting graphicsLevelSetting; // = GraphicsLevelSetting.High;

        public const int framedCRTWidth = 533; // (int)((4f / 3f) * 400f);
        public const int framedCRTHeight = 400;

        private Rectangle upperLeftQuadrant, upperRightQuadrant, lowerLeftQuadrant, lowerRightQuadrant;

        protected Cursor fingerCursor, fingerUpDown, fingerLeftRight, fingerUpRight, fingerDownRight, fingerMove, /*lcdCursor,*/ worldCursor,
            worldUpDown, worldLeftRight, worldUpRight, worldDownRight, worldMove;

        protected Tooltip Tooltip; // = new Tooltip(this);

        public CommonInterface(UnclaimedWorld game, bool addGuiManagerNow = true, bool addTooltip = false)
        {
            this.Game = game;
            
            gui = new GUIManager(game, game.Controller.DrawArea.Width, game.Controller.DrawArea.Height, 
                game.Controller.InputData, game.Controller.Content, addGuiManagerNow);
       
            gui.SetMouseCursorEvent += new GUIManager.SetMouseCursorHandler(gui_SetMouseCursorEvent);


            //Set property on image files: "Copy to output directory" = if newer!
         /*   fingerCursor = CreateCursor("Content/GUI/Cursors/finger.png", new Point(4, 4));
            fingerDownRight = CreateCursor("Content/GUI/Cursors/finger_scale_downright.png", new Point(4, 4));
            fingerLeftRight = CreateCursor("Content/GUI/Cursors/finger_scale_horiz.png", new Point(4, 4));
            fingerUpRight = CreateCursor("Content/GUI/Cursors/finger_scale_upright.png", new Point(4, 4));
            fingerUpDown = CreateCursor("Content/GUI/Cursors/finger_scale_up_down.png", new Point(4, 4));
            fingerMove = CreateCursor("Content/GUI/Cursors/finger_move.png", new Point(4, 4));
            */
            //lcdCursor = CreateCursor("Content/GUI/Cursors/lcd_cursor.png", Point.Zero); 

            //Set properties on image files: "Copy to output directory" = "If newer" and "Build action": "None"
            worldCursor = CreateCursor("Content/GUI/Cursors/HUD_cursor.png", new Point(5, 5));
            worldDownRight = CreateCursor("Content/GUI/Cursors/HUD_cursor_scale_downright.png", new Point(15, 15));
            worldLeftRight = CreateCursor("Content/GUI/Cursors/HUD_cursor_scale_horiz.png", new Point(16, 15));
            worldUpRight = CreateCursor("Content/GUI/Cursors/HUD_cursor_scale_upright.png", new Point(15, 15));
            worldUpDown = CreateCursor("Content/GUI/Cursors/HUD_cursor_scale_vertical.png", new Point(16, 16));         
            worldMove = CreateCursor("Content/GUI/Cursors/HUD_cursor_move.png", new Point(16, 15));
        


            DisplayPanelRenderer = new DisplayPanelRenderer(game); // UWGame.SimSide.Instance);
            DisplayPanelRenderer.Initialize();

            if (addTooltip)
            {
                Tooltip = new Tooltip(this);
            }
        }

        /// <summary>
        /// used in ui panel placement
        /// </summary>
        private void InitScreenQuadrants()
        {
           // GraphicsDeviceManager graphicsManager = ((UnclaimedWorld)gui.Game).GraphicsDeviceManager;
            Dimension dim = ((UnclaimedWorld)gui.Game).Controller.DrawArea;
            int widthOfQuadrant = dim.Width / 2;
            int heightOfQuadrant = dim.Height / 2; // graphicsManager.PreferredBackBufferHeight / 2;
           
            /*int widthOfQuadrant = The.Sim.ScreenManager.Game.graphics.PreferredBackBufferWidth / 2;
            int heightOfQuadrant = The.Sim.ScreenManager.Game.graphics.PreferredBackBufferHeight / 2;
            */

            upperLeftQuadrant = new Rectangle(0, 0, widthOfQuadrant, heightOfQuadrant);
            upperRightQuadrant = new Rectangle(widthOfQuadrant, 0, widthOfQuadrant, heightOfQuadrant);
            lowerLeftQuadrant = new Rectangle(0, heightOfQuadrant, widthOfQuadrant, heightOfQuadrant);
            lowerRightQuadrant = new Rectangle(widthOfQuadrant, heightOfQuadrant, widthOfQuadrant, heightOfQuadrant);

        }

        const int tooltipOverlap = 8;

        public enum AnchorSide { Left, Right }

        public void SelectAnchorPoint(UIComponent control, Window windowToAnchor, AnchorSide? sideToAnchorOn, int heightToUse, bool doOverlap, int yOffset, out int x, out int y, int? overlapToUse = null)
        {
            int left = control.AbsolutePosition.X;
            int right = control.AbsolutePosition.X + control.Width;
            int top = control.AbsolutePosition.Y;
            int bottom = control.AbsolutePosition.Y + control.Height;

           // int overlapToUse;
            if (doOverlap)
            {
                overlapToUse = overlapToUse ?? tooltipOverlap;
            }
            else
            {
                overlapToUse = overlapToUse ?? 0;
            }

            AnchorSide anchorOnSide;



            if (upperLeftQuadrant.Contains(right, top))
            {
                // anchor at right edge of control:
                anchorOnSide = AnchorSide.Right;
                //x = right - overlapToUse;
                y = top + yOffset;
            }
            else if (upperRightQuadrant.Contains(left, top))
            {
                anchorOnSide = AnchorSide.Left;
                //x = left - windowToAnchor.Width + overlapToUse;
                y = top + yOffset;
            }
            else if (lowerLeftQuadrant.Contains(right, bottom))
            {
                anchorOnSide = AnchorSide.Right;
                //x = right - overlapToUse;

                y = top + yOffset;
            }
            else if (lowerRightQuadrant.Contains(left, bottom))
            {
                anchorOnSide = AnchorSide.Left;
                //x = left - windowToAnchor.Width + overlapToUse;

                y = top + yOffset;
            }
            else
            {
                // anchor at right edge of control:
                anchorOnSide = AnchorSide.Right;
                //x = right - overlapToUse;
                y = top + yOffset;
            }

            // override horiz position using param:
            if (sideToAnchorOn.HasValue)
            {
                anchorOnSide = sideToAnchorOn.Value;
            }

            if (anchorOnSide == AnchorSide.Left)
            {
                // left:
                x = left - windowToAnchor.Width + overlapToUse.Value;
            }
            else
            {
                // right:
                x = right - overlapToUse.Value;
            }

            y = Common.ClampTop(y, lowerLeftQuadrant.Bottom - heightToUse - 36);
        }


        public virtual void Destroy()
        {
            gui.SetMouseCursorEvent -= new GUIManager.SetMouseCursorHandler(gui_SetMouseCursorEvent);
            gui.Destroy();

            gui = null;

            if (Tooltip != null)
            {
                Tooltip.Destroy();
            }

            DisplayPanelRenderer.Destroy();

        }

        void gui_SetMouseCursorEvent(MouseSprites mouseSprite)
        {
            SetCursor(gui.MouseSprite);
        }

        public virtual void LoadContent()
        {
            DisplayPanelRenderer.LoadContent();

            InitScreenQuadrants();
        }

        public virtual void UnloadContent()
        {          
         
            // dispose cursors?           
          /*  fingerCursor.Dispose();
            fingerDownRight.Dispose();
            fingerLeftRight.Dispose();
            fingerUpRight.Dispose();
            fingerUpDown.Dispose();
            fingerMove.Dispose();*/
            //lcdCursor.Dispose();

            worldCursor.Dispose();
            worldDownRight.Dispose();
            worldLeftRight.Dispose();
            worldUpRight.Dispose();
            worldUpDown.Dispose();
            worldMove.Dispose();

        }

        public void SetInterfaceCursor()
        {
            if (windowForm == null )
                windowForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Game.Window.Handle);
    
            if (windowForm.Cursor != worldCursor)
            {
                windowForm.Cursor = worldCursor;
            }
        }

        public void SetCursor(MouseSprites sprite)
        {

            if (windowForm == null)
                windowForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Game.Window.Handle);
                      
            Cursor newCursor;
                      
            switch (sprite)
            {
                case MouseSprites.Normal:
                    newCursor = worldCursor;
                    break;

                case MouseSprites.Moving:
                    newCursor = worldMove;
                    break;

                case MouseSprites.ResizingNESW:
                    newCursor = worldUpRight;
                    break;

                case MouseSprites.ResizingNWSE:
                    newCursor = worldDownRight;
                    break;

                case MouseSprites.ResizingWE:
                    newCursor = worldLeftRight;
                    break;

                case MouseSprites.ResizingNS:
                    newCursor = worldUpDown;
                    break;

                default:
                    newCursor = worldCursor;
                    break;
            }

            if (windowForm.Cursor != newCursor)
                windowForm.Cursor = newCursor;

        }

        public void SetFingerCursor(MouseSprites sprite)
        {

            if (windowForm == null)
                windowForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Game.Window.Handle);
            
            /* if (windowForm.Cursor != fingerCursor)
             {*/

            switch (sprite)
            {
                case MouseSprites.Normal:
                    windowForm.Cursor = fingerCursor;
                    break;

                case MouseSprites.Moving:
                    windowForm.Cursor = fingerMove;
                    break;

                case MouseSprites.ResizingNESW:
                    windowForm.Cursor = fingerUpRight;
                    break;

                case MouseSprites.ResizingNWSE:
                    windowForm.Cursor = fingerDownRight;
                    break;

                case MouseSprites.ResizingWE:
                    windowForm.Cursor = fingerLeftRight;
                    break;

                case MouseSprites.ResizingNS:
                    windowForm.Cursor = fingerUpDown;
                    break;
            }
            // }
        }
      /*  public void SetLCDCursor()
        {
            if (windowForm.Cursor != lcdCursor)
            {
                windowForm.Cursor = lcdCursor;
            }
        }*/

       

        private Cursor CreateCursor(string cursorImage, Point hotspot)
        {           
            using (System.Drawing.Bitmap b = new System.Drawing.Bitmap(cursorImage)) // try to prevent leaks of unmanaged resources...
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b))
                {
                    IntPtr cursor = b.GetHicon();

                    IconInfo tmp = new IconInfo();

                    GetIconInfo(cursor, ref tmp);

                    tmp.xHotspot = hotspot.X; // 0; 
                    tmp.yHotspot = hotspot.Y; // 0; 
                    tmp.fIcon = false;

                    cursor = CreateIconIndirect(ref tmp);

                    //windowForm.Cursor = new System.Windows.Forms.Cursor(ptr); 

                    // dispose cursor ptr also?
                    // see UnloadContent...

                    b.Dispose();
                    g.Dispose();  
                    
                    return new System.Windows.Forms.Cursor(cursor);
                }
            }

          
        }

        public void Draw(GameTime gameTime)
        {
            //forms.Draw();
            for (Level levelToDraw = Level.Min; levelToDraw < Level.Max; levelToDraw++)
                DrawWindowLevel(gameTime, levelToDraw);

        }

        public virtual void Update(GameTime gameTime)
        {
            gui.Update(gameTime);

            if (Tooltip != null)
            {
                Tooltip.Update(gameTime);
            }
        }

    

        private void DrawWindowLevel(GameTime gameTime, Level levelToDraw)
        {
            gui.Draw(gameTime, RenderType.Normal, levelToDraw);

            if (Game.Controller.GraphicsLevelSetting == Control.Controller.GraphicsLevel.High)
            {
                // CRT and LCD rendering.
                // draw into render target first, then draw with effects:
                DisplayPanelRenderer.StartPanelRendering();
                gui.Draw(gameTime, RenderType.CRTAndLCD, levelToDraw);


                DisplayPanelRenderer.Draw(levelToDraw);
            }
            else
            {
                // draw directly, without effects:
                gui.Draw(gameTime, RenderType.CRTAndLCD, levelToDraw);

            }

            gui.Draw(gameTime, RenderType.Overlay, levelToDraw);
        }



        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(ref IconInfo icon);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetIconInfo(IntPtr ptr, ref IconInfo icon);
        private struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

    }
}
