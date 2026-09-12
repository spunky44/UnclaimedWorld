using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface;

namespace UWGame.ClientSide.Interface
{
    public class LCDQuad
    {
        protected VertexLCDQuad[] quad;

      //  protected GameObject parent;

        public LCDQuad()
        {

           // SetupQuadVertices();
        }

        public void CopyQuadToVertexBuffer(VertexLCDQuad[] crtVertices, int index)
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

        public void DrawQuad(VertexLCDQuad[] featureVertices, int index) //int xScreen, int yScreen)
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
            Rectangle spriteSheetSourceRect, int contentTextureWidth, int contentTextureHeight, int reflectionTextureWidth, int reflectionTextureHeight,
            int grungeTextureWidth, int grungeTextureHeight,
            float panelWidth, float panelHeight, bool drawWithDust, float alpha) //, bool useLampReflection) 
        {

            

            float z = 0f;
            quad = new VertexLCDQuad[4];

            // these coordinates are into the big content texture (from render target)
            float textureLeft = (float)spriteSheetSourceRect.X / (float)contentTextureWidth; //0; // 
            float textureTop = (float)spriteSheetSourceRect.Y / (float)contentTextureHeight; // 0f; // 
            float textureRight = textureLeft + (float)spriteSheetSourceRect.Width / (float)contentTextureWidth; //1; // 
            float textureBottom = textureTop + (float)spriteSheetSourceRect.Height / (float)contentTextureHeight; //1f; // 

           
            // NEW: Reflection map is set at the top right corner instead of the left:
            float reflectionTextureLeft = 1f - panelWidth / (float)reflectionTextureWidth; //0f; 
            float reflectionTextureTop = 0f;
            float reflectionTextureRight = 1f; // panelWidth / (float)effectTextureWidth;
            float reflectionTextureBottom = panelHeight / (float)reflectionTextureHeight;

         
            float grungeTextureLeft = 0f;
            float grungeTextureTop = 1f - panelHeight / (float)grungeTextureHeight;
            float grungeTextureRight = panelWidth / (float)grungeTextureWidth;
            float grungeTextureBottom = 1f; // grungeTextureTop + panelHeight / (float)grungeTextureHeight;

            float drawDust = (drawWithDust ? 1f : 0f);
           // float useLamp = (useLampReflection ? 1f : 0f);

            quad[0] = new VertexLCDQuad();
            quad[0].Position = new Vector3(posLeft, posTop, z);
            quad[0].TextureCoordinate = new Vector2(textureLeft, textureTop);
            //quad[0].BigReflectionCoordinate = new Vector2(reflectionTextureLeft, reflectionTextureTop);
            quad[0].ScaledEffectCoordinate = new Vector2(0f, 0f);
            quad[0].GrungeCoordinate = new Vector2(grungeTextureLeft, grungeTextureTop);
          
            quad[1] = new VertexLCDQuad();
            quad[1].Position = new Vector3(posRight, posTop, z);
            quad[1].TextureCoordinate = new Vector2(textureRight, textureTop);
          //  quad[1].BigReflectionCoordinate = new Vector2(reflectionTextureRight, reflectionTextureTop);
            quad[1].ScaledEffectCoordinate = new Vector2(1f, 0f);
            quad[1].GrungeCoordinate = new Vector2(grungeTextureRight, grungeTextureTop);
          
            quad[2] = new VertexLCDQuad();
            quad[2].Position = new Vector3(posRight, posBottom, z);
            quad[2].TextureCoordinate = new Vector2(textureRight, textureBottom);
           // quad[2].BigReflectionCoordinate = new Vector2(reflectionTextureRight, reflectionTextureBottom);
            quad[2].ScaledEffectCoordinate = new Vector2(1f, 1f);
            quad[2].GrungeCoordinate = new Vector2(grungeTextureRight, grungeTextureBottom);
           
            quad[3] = new VertexLCDQuad();
            quad[3].Position = new Vector3(posLeft, posBottom, z);
            quad[3].TextureCoordinate = new Vector2(textureLeft, textureBottom);
           // quad[3].BigReflectionCoordinate = new Vector2(reflectionTextureLeft, reflectionTextureBottom);
            quad[3].ScaledEffectCoordinate = new Vector2(0f, 1f);
            quad[3].GrungeCoordinate = new Vector2(grungeTextureLeft, grungeTextureBottom);
           
            for (int i = 0; i < quad.Length; i++)
            {
                quad[i].DrawDust = drawDust;
                quad[i].Alpha = alpha;
              //  quad[i].UseLampReflection = useLamp;
            }

           // SetupFourCornerVertices(posLeft, posTop, posRight, posBottom, z, textureLeft, textureTop, textureRight, textureBottom);      
        }

       
    }



    public struct VertexLCDQuad : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
       // public Vector2 BigReflectionCoordinate;
        public Vector2 ScaledEffectCoordinate;
        public float DrawDust;
        public Vector2 GrungeCoordinate;
        public float Alpha;
       // public float UseLampReflection;
       // public Vector2 SmallReflectionCoordinate;

      //  public static int SizeInBytes = (3 + 2 + 2 + 2 + 1 + 2 + 1 + 1) * sizeof(float);
        public static int SizeInBytes = (3 + 2 + 2 + 1 + 2 + 1) * sizeof(float);
        public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),         
             new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1 ),
             new VertexElement(sizeof(float) * 7, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2 ),
             new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 3 ),
             new VertexElement(sizeof(float) * 10, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4 )
          
         };
       /* public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1 ),
             new VertexElement(sizeof(float) * 7, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2 ),
             new VertexElement(sizeof(float) * 9, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3 ),
             new VertexElement(sizeof(float) * 10, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 4 ),
             new VertexElement(sizeof(float) * 12, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 5 ),
             new VertexElement(sizeof(float) * 13, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 6 )
          
         };
        */
        private readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }
    }
}
