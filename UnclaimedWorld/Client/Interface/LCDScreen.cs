using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowSystem;

namespace UWGame.ClientSide.Interface
{
    
    public class LCDScreen: DisplayScreen
    {
        public enum ReflectionToUse{ Circular, Lamp }
        protected LCDQuad quad;

        public bool DrawDust = true;
       // public ReflectionToUse Reflection = ReflectionToUse.Circular;
        
        
        protected int grungeTextureWidth, grungeTextureHeight;

     /*   public Rectangle SourceRectangle
        {
            get { return sourceRectangle; }
            set 
            { 
                sourceRectangle = value;
              //  SetupQuadVertices(); //contentTextureWidth, contentTextureHeight, effectTextureWidth, effectTextureHeight, grungeTextureWidth, grungeTextureHeight);
            }
        }

        public Rectangle DestinationRectangle
        {
            get { return destinationRectangle; }
            set 
            { 
                destinationRectangle = value;
            //    SetupQuadVertices(); //contentTextureWidth, contentTextureHeight, effectTextureWidth, effectTextureHeight, grungeTextureWidth, grungeTextureHeight);
            }

        }*/


        /// <summary>
        /// 
        /// </summary>
        /// <param name="src">The rectangle on the large content render target that we will show on the panel.</param>
        /// <param name="dest">Where the panel will be shown on screen, and how large it will be.</param>
        /// <param name="textureWidth"></param>
        /// <param name="textureHeight"></param>
        /// <param name="effectTextureWidth"></param>
        /// <param name="effectTextureHeight"></param>
        public LCDScreen(UIComponent displayBox, Rectangle src, Rectangle dest, int contentTextureWidth, int contentTextureHeight, 
            int effectTextureWidth, int effectTextureHeight, 
            int grungeTextureWidth, int grungeTextureHeight, Window window, bool drawDust) //, ReflectionToUse reflection)
            : base(displayBox, src, dest, contentTextureWidth, contentTextureHeight, effectTextureWidth, effectTextureHeight, window)
        {
           

            this.grungeTextureWidth = grungeTextureWidth;
            this.grungeTextureHeight = grungeTextureHeight;

            this.DrawDust = drawDust;
          //  this.Reflection = reflection;

            //this.isMonochrome = isMonochrome;

            quad = new LCDQuad();

            SetupQuadVertices(); 

           // SetupQuadVertices(textureWidth, textureHeight, effectTextureWidth, effectTextureHeight);
        }


       
        

        protected override void SetupQuadVertices() 
        {
            base.SetupQuadVertices();

            Vector2 topLeftPos = Vector2.Zero;
            // NEW
            float posLeft = destinationRectangle.X;
            float posRight = posLeft + destinationRectangle.Width;

            float posTop = destinationRectangle.Y;
            float posBottom = posTop + destinationRectangle.Height;


            quad.SetupQuadVertices(posLeft, posTop, posRight, posBottom, sourceRectangle, contentTextureWidth,
                contentTextureHeight, effectTextureWidth, effectTextureHeight,
                grungeTextureWidth, grungeTextureHeight, destinationRectangle.Width, destinationRectangle.Height, DrawDust, 
                (ParentWindow != null? ParentWindow.TransitionValue: 1f)); //, Reflection == ReflectionToUse.Lamp);

        }

      /*  public void SetupQuadVertices(int contentTextureWidth, int contentTextureHeight, int effectTextureWidth, int effectTextureHeight) //Texture2D texture)
        {
            
        

            Vector2 topLeftPos = Vector2.Zero; 
            // NEW
            float posLeft = destinationRectangle.X;
            float posRight = posLeft + destinationRectangle.Width;

            float posTop = destinationRectangle.Y;
            float posBottom = posTop + destinationRectangle.Height;


            quad.SetupQuadVertices(posLeft, posTop, posRight, posBottom, sourceRectangle, contentTextureWidth, contentTextureHeight, effectTextureWidth, effectTextureHeight,
                , , destinationRectangle.Width, destinationRectangle.Height, false);

        }*/



        public void CopyQuadToVertexBuffer(VertexLCDQuad[] featureVertices, int index)
        {
            if (IsDirty)
            {
                SetupQuadVertices();
            }

            quad.CopyQuadToVertexBuffer(featureVertices, index);
        }

        public virtual void Update(GameTime gameTime)
        {
            
        }

        public virtual void DrawContent()
        {
           
           

        }
    }
}
