using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide
{
    public class OverlayQuad : QuadBase
    {
        protected VertexOverlayQuad[] quad;

      //  public bool FlipSprite = false;

        
        /// <summary>
        /// an overlay quad has 3 tint colors that form a vertical gradient.
        /// These colors must be finely tuned... if they are all set to white, the sprite will render as a white solid...
        /// </summary>
        public Vector4 GradientColor1 = Color.DarkGray.ToVector4();
        public Vector4 GradientColor2 = Color.LightGray.ToVector4();
        public Vector4 GradientColor3 = Color.White.ToVector4();



        public OverlayQuad()
        {

        }



        /// <summary>
        /// push these properties from RenderAsBillboard during recomputation. they do not require recomputed coordinates.
        /// </summary>
        public void SetProperties(Vector4 tint, Vector4 gradient1, Vector4 gradient2, Vector4 gradient3)
        {
            // should we save the Tint as in FeatureQuad..?

            GradientColor1 = gradient1;
            GradientColor2 = gradient2;
            GradientColor3 = gradient3;


            VertexOverlayQuad currentQuad;
            for (int i = 0; i < quad.Length; i++)
            {
                currentQuad = quad[i];

                currentQuad.GradientColor1 = GradientColor1 * tint;
                currentQuad.GradientColor2 = GradientColor2 * tint;
                currentQuad.GradientColor3 = GradientColor3 * tint;

                quad[i] = currentQuad; // needed because struct is a value type
            }
        }


        public void CopyQuadToVertexBuffer(VertexOverlayQuad[] crtVertices, int index)
        {
           /* if (!Player.IsFinished)
            {
                UpdateVerticeColorFromPlayer();
            }*/

            index *= 4;
            for (int i = index; i < index + 4; i++)
            {
                crtVertices[i] = quad[i - index];
            }

        }



      /* replaced with effect on Renderable
        public void UpdateVerticeColorFromPlayer()
        {
            if (quad != null)
            {
                Vector4 currentColor = Player.GetCurrentColor(new Color(GradientColor1)).ToVector4();
                for (int i = 0; i < quad.Length; i++)
                {
                  //  quad[i].GradientColor1 = currentColor;
                    quad[i].GradientColor1 *= currentColor;
                    quad[i].GradientColor2 *= currentColor;
                }
            }
        }*/

       

        public void SetupQuadVertices(Vector3 worldPosition, float posLeft, float posTop, float posRight, float posBottom,
            /*Rectangle spriteSheetSourceRect, Texture2D spriteSheetTexture,*/
            bool drawScanlines, bool flipSprite)
        {

           
            float z = worldPosition.Z; 
            quad = new VertexOverlayQuad[4];

            int crtEffectTextureWidth = The.Client.Renderer.Scanlines.Width;
            int crtEffectTextureHeight = The.Client.Renderer.Scanlines.Width;

            float textureLeft = (float)StaticSourceRectangle.X / (float)Texture.Width;
            float textureTop = (float)StaticSourceRectangle.Y / (float)Texture.Height;

            float textureRight = textureLeft + (float)StaticSourceRectangle.Width / (float)Texture.Width;
            float textureBottom = textureTop + (float)StaticSourceRectangle.Height / (float)Texture.Height;

            // these are into the scanlines texture:
            float crtTextureLeft = 0f;
            float crtTextureTop = 0f;
            float crtTextureRight = StaticSourceRectangle.Width / (float)crtEffectTextureWidth;
            float crtTextureBottom = StaticSourceRectangle.Height / (float)crtEffectTextureHeight;
                 

            quad[0] = new VertexOverlayQuad();
            quad[0].Position = new Vector3(posLeft, posTop, z);
            quad[0].WorldPosition = worldPosition;
            quad[0].TextureCoordinate = (flipSprite ? new Vector2(textureRight, textureTop) : new Vector2(textureLeft, textureTop));
            quad[0].TruncatedEffectCoordinate = new Vector2(crtTextureLeft, crtTextureTop);
          

            quad[1] = new VertexOverlayQuad();
            quad[1].Position = new Vector3(posRight, posTop, z);
            quad[1].WorldPosition = worldPosition;
            quad[1].TextureCoordinate = (flipSprite ? new Vector2(textureLeft, textureTop) : new Vector2(textureRight, textureTop));
            quad[1].TruncatedEffectCoordinate = new Vector2(crtTextureRight, crtTextureTop);
          

            quad[2] = new VertexOverlayQuad();
            quad[2].Position = new Vector3(posRight, posBottom, z);
            quad[2].WorldPosition = worldPosition;
            quad[2].TextureCoordinate = (flipSprite ? new Vector2(textureLeft, textureBottom) : new Vector2(textureRight, textureBottom));
            quad[2].TruncatedEffectCoordinate = new Vector2(crtTextureRight, crtTextureBottom);
          

            quad[3] = new VertexOverlayQuad();
            quad[3].Position = new Vector3(posLeft, posBottom, z);
            quad[3].WorldPosition = worldPosition;
            quad[3].TextureCoordinate = (flipSprite ? new Vector2(textureRight, textureBottom) : new Vector2(textureLeft, textureBottom));
            quad[3].TruncatedEffectCoordinate = new Vector2(crtTextureLeft, crtTextureBottom);
            

            float drawScan = (drawScanlines ? 1f : 0f);

            for (int i = 0; i < quad.Length; i++)
            {
                quad[i].DrawScanlines = drawScan;
                quad[i].GradientColor1 = GradientColor1; 
                quad[i].GradientColor2 = GradientColor2;
                quad[i].GradientColor3 = GradientColor3;
            }
        
        }



        /*
        public void SetupQuadVertices(float posLeft, float posTop, float posRight, float posBottom,
            Rectangle spriteSheetSourceRect, int contentTextureWidth, int contentTextureHeight, int crtEffectTextureWidth, int crtEffectTextureHeight,            
            float panelWidth, float panelHeight, bool drawScanlines) 
        {
            float z = 0f;
            quad = new VertexOverlayQuad[4];



            // these coordinates are into the big content texture (from render target)
            float textureLeft = (float)spriteSheetSourceRect.X / (float)contentTextureWidth; //0; // 
            float textureTop = (float)spriteSheetSourceRect.Y / (float)contentTextureHeight; // 0f; // 
            float textureRight = textureLeft + (float)spriteSheetSourceRect.Width / (float)contentTextureWidth; //1; // 
            float textureBottom = textureTop + (float)spriteSheetSourceRect.Height / (float)contentTextureHeight; //1f; // 

            // these are into the scanlines texture:
            float crtTextureLeft = 0f; 
            float crtTextureTop = 0f;
            float crtTextureRight = panelWidth / (float)crtEffectTextureWidth;
            float crtTextureBottom = panelHeight / (float)crtEffectTextureHeight;
                                   

            
            quad[0] = new VertexOverlayQuad();
            quad[0].Position = new Vector3(posLeft, posTop, z);
            quad[0].TextureCoordinate = new Vector2(textureLeft, textureTop);
            quad[0].TruncatedEffectCoordinate = new Vector2(crtTextureLeft, crtTextureTop);
          
            quad[1] = new VertexOverlayQuad();
            quad[1].Position = new Vector3(posRight, posTop, z);
            quad[1].TextureCoordinate = new Vector2(textureRight, textureTop);
            quad[1].TruncatedEffectCoordinate = new Vector2(crtTextureRight, crtTextureTop);
          
            quad[2] = new VertexOverlayQuad();
            quad[2].Position = new Vector3(posRight, posBottom, z);
            quad[2].TextureCoordinate = new Vector2(textureRight, textureBottom);
            quad[2].TruncatedEffectCoordinate = new Vector2(crtTextureRight, crtTextureBottom);
          
            quad[3] = new VertexOverlayQuad();
            quad[3].Position = new Vector3(posLeft, posBottom, z);
            quad[3].TextureCoordinate = new Vector2(textureLeft, textureBottom);
            quad[3].TruncatedEffectCoordinate = new Vector2(crtTextureLeft, crtTextureBottom);

            float isMono = (drawScanlines ? 1f : 0f);

            for (int i = 0; i < quad.Length; i++)
            {
                quad[i].DrawScanlines = isMono;               
            }

           // SetupFourCornerVertices(posLeft, posTop, posRight, posBottom, z, textureLeft, textureTop, textureRight, textureBottom);      
        }*/

       
    }



    public struct VertexOverlayQuad : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector2 TruncatedEffectCoordinate;
        public Vector3 WorldPosition;
        public float DrawScanlines;
        public Vector4 GradientColor1;
        public Vector4 GradientColor2;
        public Vector4 GradientColor3;

        public static int SizeInBytes = (3 + 2 + 2 + 3 + 1 + 4 + 4 + 4) * sizeof(float);
        public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1 ),             
             new VertexElement(sizeof(float) * 7, VertexElementFormat.Vector3, VertexElementUsage.Position, 1 ),
             new VertexElement(sizeof(float) * 10, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2 ),
             new VertexElement(sizeof(float) * 11, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3 ),
             new VertexElement(sizeof(float) * 15, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4 ),
             new VertexElement(sizeof(float) * 19, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5 )
         };

        private readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }
    }
}
