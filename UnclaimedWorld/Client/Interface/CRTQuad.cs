using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface;

namespace UWGame.ClientSide.Interface
{
    public class CRTQuad
    {
        protected VertexCRTQuad[] quad;


        public CRTQuad()
        {

           // SetupQuadVertices();
        }

        public void CopyQuadToVertexBuffer(VertexCRTQuad[] crtVertices, int index)
        {
            //  xScreen = 0;
            //   yScreen = 0;
            //Vector3 windowPos = new Vector3(UWGame.SimSide.Instance.map.mapWindowWorldPosition, 0f);
            index *= 4;
            for (int i = index; i < index + 4; i++)
            {

                crtVertices[i] = quad[i - index];
            }

        }

        public void DrawQuad(VertexCRTQuad[] featureVertices, int index) //int xScreen, int yScreen)
        {           
            //Vector2 topLeftTranslatePos = Vector2.Zero; //HERE: new Vector2(MapManager.tileWidth * MapPosition.X, MapManager.tileHeight * MapPosition.Y);
                      
            //Matrix translate = Matrix.CreateTranslation(topLeftTranslatePos.X, topLeftTranslatePos.Y, 0f);

            index *= 4;
            for (int i = index; i < index + 4; i++)
            {
                featureVertices[i] = quad[i - index];
                // screens don't move:
             //   featureVertices[i].Position = Vector3.Transform(featureVertices[i].Position, translate);
            }

            return;
        }

        
        public void SetupQuadVertices(float posLeft, float posTop, float posRight, float posBottom,
            Rectangle spriteSheetSourceRect, int contentTextureWidth, int contentTextureHeight, int crtEffectTextureWidth, int crtEffectTextureHeight,
            //int bigReflectionsTextureWidth, int bigReflectionsTextureHeight, int smallReflectionsTextureWidth, int smallReflectionsTextureHeight,
            float panelWidth, float panelHeight, ReflectionToUse toUse, bool isMonochrome, float alpha) 
        {
            float z = 0f;
            quad = new VertexCRTQuad[4];

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

            /*
            float bigReflectionRight, bigReflectionTop, bigReflectionLeft, bigReflectionBottom;
            float smallReflectionRight, smallReflectionTop, smallReflectionLeft, smallReflectionBottom;

            // reflections. We have two sizes, and we want to right and top align them both:
            if (toUse == ReflectionToUse.Big)
            {
                bigReflectionRight = 1f;
                bigReflectionTop = 0f;
                bigReflectionLeft = -(panelWidth - bigReflectionsTextureWidth) / panelWidth;
                bigReflectionBottom = panelHeight / bigReflectionsTextureHeight; // (panelHeight - bigReflectionsTextureHeight) / panelHeight;

                smallReflectionBottom = smallReflectionLeft = smallReflectionRight = smallReflectionTop = 5f;
            }
            else if (toUse == ReflectionToUse.Small)
            {

                smallReflectionRight = 1f;
                smallReflectionTop = 0f;
                smallReflectionLeft = -(panelWidth - smallReflectionsTextureWidth) / panelWidth;
                smallReflectionBottom = panelHeight / smallReflectionsTextureHeight; //-(panelHeight - smallReflectionsTextureHeight) / panelHeight;

                bigReflectionBottom = bigReflectionLeft = bigReflectionTop = bigReflectionRight = 5f;
            }
            else
            {
                // disable reflection:
                smallReflectionBottom = smallReflectionLeft = smallReflectionRight = smallReflectionTop = 5f;
                bigReflectionBottom = bigReflectionLeft = bigReflectionTop = bigReflectionRight = 5f;
            }*/

            float isMono = (isMonochrome ? 1f : 0f);

            quad[0] = new VertexCRTQuad();
            quad[0].Position = new Vector3(posLeft, posTop, z);
            quad[0].TextureCoordinate = new Vector2(textureLeft, textureTop);
            quad[0].TruncatedEffectCoordinate = new Vector2(crtTextureLeft, crtTextureTop);
            quad[0].ScaledEffectCoordinate = new Vector2(0f, 0f);
           /** quad[0].BigReflectionCoordinate = new Vector2(bigReflectionLeft, bigReflectionTop);
            quad[0].SmallReflectionCoordinate = new Vector2(smallReflectionLeft, smallReflectionTop);
            */
            quad[1] = new VertexCRTQuad();
            quad[1].Position = new Vector3(posRight, posTop, z);
            quad[1].TextureCoordinate = new Vector2(textureRight, textureTop);
            quad[1].TruncatedEffectCoordinate = new Vector2(crtTextureRight, crtTextureTop);
            quad[1].ScaledEffectCoordinate = new Vector2(1f, 0f);
          /*  quad[1].BigReflectionCoordinate = new Vector2(bigReflectionRight, bigReflectionTop);
            quad[1].SmallReflectionCoordinate = new Vector2(smallReflectionRight, smallReflectionTop);
            */

            quad[2] = new VertexCRTQuad();
            quad[2].Position = new Vector3(posRight, posBottom, z);
            quad[2].TextureCoordinate = new Vector2(textureRight, textureBottom);
            quad[2].TruncatedEffectCoordinate = new Vector2(crtTextureRight, crtTextureBottom);
            quad[2].ScaledEffectCoordinate = new Vector2(1f, 1f);
          /*  quad[2].BigReflectionCoordinate = new Vector2(bigReflectionRight, bigReflectionBottom);
            quad[2].SmallReflectionCoordinate = new Vector2(smallReflectionRight, smallReflectionBottom);
            */
            quad[3] = new VertexCRTQuad();
            quad[3].Position = new Vector3(posLeft, posBottom, z);
            quad[3].TextureCoordinate = new Vector2(textureLeft, textureBottom);
            quad[3].TruncatedEffectCoordinate = new Vector2(crtTextureLeft, crtTextureBottom);
            quad[3].ScaledEffectCoordinate = new Vector2(0f, 1f);
           /* quad[3].BigReflectionCoordinate = new Vector2(bigReflectionLeft, bigReflectionBottom);
            quad[3].SmallReflectionCoordinate = new Vector2(smallReflectionLeft, smallReflectionBottom);
            */

            for (int i = 0; i < quad.Length; i++)
            {
                quad[i].IsMonochrome = isMono;
                quad[i].Alpha = alpha;
            }

           // SetupFourCornerVertices(posLeft, posTop, posRight, posBottom, z, textureLeft, textureTop, textureRight, textureBottom);      
        }

       
    }



    public struct VertexCRTQuad : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector2 TruncatedEffectCoordinate;
        public Vector2 ScaledEffectCoordinate;
      //  public Vector2 BigReflectionCoordinate;
      //  public Vector2 SmallReflectionCoordinate;
        public float IsMonochrome;
        public float Alpha;
      

       // public static int SizeInBytes = (3 + 2 + 2 + 2 + 2 + 2 + 1 + 1) * sizeof(float);
        public static int SizeInBytes = (3 + 2 + 2 + 2 + 1 + 1) * sizeof(float);
        public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1 ),
             new VertexElement(sizeof(float) * 7, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2 ),             
             new VertexElement(sizeof(float) * 9, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3 ),
             new VertexElement(sizeof(float) * 10, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4 )
          
         };
       /* public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1 ),
             new VertexElement(sizeof(float) * 7, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2 ),
             new VertexElement(sizeof(float) * 9, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 3 ),
             new VertexElement(sizeof(float) * 11, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 4 ),
             new VertexElement(sizeof(float) * 13, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 5 ),
             new VertexElement(sizeof(float) * 14, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 6 )
          
         };*/

        private readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }
    }
}
