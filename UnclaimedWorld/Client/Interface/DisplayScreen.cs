using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface
{
    public abstract class DisplayScreen
    {        
        /// <summary>
        /// for now, source and dest are the same.
        /// </summary>
        public Rectangle sourceRectangle;
        public Rectangle destinationRectangle;

        public WindowSystem.Window ParentWindow;

        public UIComponent DisplayBox;

        public bool IsDirty = false;

     //   int width, height;

        protected int contentTextureWidth, contentTextureHeight, effectTextureWidth, effectTextureHeight;

        public DisplayScreen(UIComponent displayBox, Rectangle src, Rectangle dest, int contentTextureWidth, int contentTextureHeight, int effectTextureWidth, int effectTextureHeight, Window window)
        {
            ParentWindow = window;

            if (window != null)
            {
                window.Transitioning += new TransitioningHandler(WindowTransitioning);

                window.Move += new MoveHandler(window_Move);
            }

             
            this.DisplayBox = displayBox;

            /*
            if (DisplayBox != null)
            {
                // redraw quad when the control changes location (like when animating)
                displayBox.Move += new MoveHandler(window_Move);
            }*/


            sourceRectangle = src;
            destinationRectangle = dest;

        //    width = dest.Width;
        //    height = dest.Height;           
            
            this.contentTextureWidth = contentTextureWidth;
            this.contentTextureHeight = contentTextureHeight;
            this.effectTextureWidth = effectTextureWidth;
            this.effectTextureHeight = effectTextureHeight;

        }

        public void SetDimensions(Rectangle dimensions)
        {
            sourceRectangle = dimensions;
            destinationRectangle = dimensions;

            SetupQuadVertices();

        }

        protected virtual void SetupQuadVertices() 
        {
            if (DisplayBox != null)
            {
                Rectangle newSource = sourceRectangle;
                newSource.Location = DisplayBox.AbsolutePosition;
                newSource.Height = DisplayBox.Height;
                newSource.Width = DisplayBox.Width;
                sourceRectangle = newSource;
                destinationRectangle = sourceRectangle;
            }


            IsDirty = false;
        }

        void window_Move(UIComponent sender)
        {
            IsDirty = true;
        }
        
        public void WindowTransitioning(Window sender)
        {
            IsDirty = true;
        }

    }
}
