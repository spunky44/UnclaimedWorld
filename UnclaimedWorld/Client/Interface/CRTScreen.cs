using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowSystem;

namespace UWGame.ClientSide.Interface
{
    public enum ReflectionToUse {None, Big, Small}
    public class CRTScreen: DisplayScreen 
    {        

        protected CRTQuad quad;

      /*  public delegate void UpdateHandler(GameTime gameTime);
        public event UpdateHandler UpdateEvent;

        public delegate void DrawContentHandler();
        public event DrawContentHandler DrawContentEvent;  
        */

       // protected int bigReflectionsTextureWidth, bigReflectionsTextureHeight, smallReflectionsTextureWidth, smallReflectionsTextureHeight;

        protected bool isMonochrome;

        protected float alpha;
        
        protected ReflectionToUse reflectionToUse;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src">The rectangle on the large content render target that we will show on the panel.</param>
        /// <param name="dest">Where the panel will be shown on screen, and how large it will be.</param>
        /// <param name="textureWidth"></param>
        /// <param name="textureHeight"></param>
        /// <param name="effectTextureWidth"></param>
        /// <param name="effectTextureHeight"></param>
        public CRTScreen(UIComponent displayBox, Rectangle src, Rectangle dest, int contentTextureWidth, int contentTextureHeight, int effectTextureWidth, int effectTextureHeight, 
           // int bigReflectionsTextureWidth, int bigReflectionsTextureHeight, int smallReflectionsTextureWidth, int smallReflectionsTextureHeight, 
            bool isMonochrome,
            ReflectionToUse toUse, Window window, float alpha = 1f)
            : base(displayBox, src, dest, contentTextureWidth, contentTextureHeight, effectTextureWidth, effectTextureHeight, window)
        {
           
            this.isMonochrome = isMonochrome;
            this.reflectionToUse = toUse;
            this.alpha = alpha;

           /* this.bigReflectionsTextureHeight = bigReflectionsTextureHeight;
            this.bigReflectionsTextureWidth = bigReflectionsTextureWidth;
            this.smallReflectionsTextureHeight = smallReflectionsTextureHeight;
            this.smallReflectionsTextureWidth = smallReflectionsTextureWidth;
            */

            quad = new CRTQuad();
            SetupQuadVertices(); //textureWidth, textureHeight, effectTextureWidth, effectTextureHeight);
        }

        public void SetupQuadVertices() //int contentTextureWidth, int contentTextureHeight, int effectTextureWidth, int effectTextureHeight) 
        {
            base.SetupQuadVertices();

            Vector2 topLeftPos = Vector2.Zero; 
            
            float posLeft = destinationRectangle.X;
            float posRight = posLeft + destinationRectangle.Width;

            float posTop = destinationRectangle.Y;
            float posBottom = posTop + destinationRectangle.Height;

            quad.SetupQuadVertices(posLeft, posTop, posRight, posBottom, sourceRectangle, contentTextureWidth, contentTextureHeight, effectTextureWidth, effectTextureHeight,
              //  bigReflectionsTextureWidth, bigReflectionsTextureHeight, smallReflectionsTextureWidth, smallReflectionsTextureHeight,
                destinationRectangle.Width, destinationRectangle.Height, reflectionToUse, isMonochrome,
                alpha * (ParentWindow != null ? ParentWindow.TransitionValue : 1f));

        }

        public void CopyQuadToVertexBuffer(VertexCRTQuad[] featureVertices, int index)
        {
            if (IsDirty)
            {
                SetupQuadVertices();
            }

            quad.CopyQuadToVertexBuffer(featureVertices, index);
        }

       

      /*    public virtual void Update(GameTime gameTime)
        {
            if (UpdateEvent != null)
            {
                UpdateEvent.Invoke(gameTime);
            }
        }

      public virtual void DrawContent()
        {
            if (DrawContentEvent != null)
            {
                DrawContentEvent.Invoke();
            }          

        }*/
    }
}
