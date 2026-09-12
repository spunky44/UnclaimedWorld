using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Maps;
using System.Diagnostics;

namespace UWGame.ClientSide.Interface
{
    public class OverlayGroundSpriteQuad
    {
        protected VertexOverlayGroundSpriteQuad[] quad;


        public OverlayGroundSpriteQuad()
        {

           // SetupQuadVertices();
        }

        /// <summary>
        /// returns false if there is no room in the buffer.
        /// </summary>
        /// <param name="crtVertices"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool CopyQuadToVertexBuffer(VertexOverlayGroundSpriteQuad[] crtVertices, int index)
        {
           
            index *= 4;

            int max = index + 4;

            if (max < crtVertices.Length)
            {
                for (int i = index; i < max; i++)
                {

                    crtVertices[i] = quad[i - index];
                }

                return true;
            }

            return false;
        }

        public void DrawQuad(VertexOverlayGroundSpriteQuad[] featureVertices, int index) 
        {           
           
            index *= 4;
            for (int i = index; i < index + 4; i++)
            {
                featureVertices[i] = quad[i - index];             
            }

            return;
        }

        public void SetupQuadVertices(/*Vector3 worldPosition,*/ float posLeft, float posTop, float posRight, float posBottom,
            Rectangle spriteSheetSourceRect, Texture2D spriteSheetTexture,
            bool drawScanlines, Vector4 color)
        {

            if (The.Client == null)
                return;//TODO this should be unneccessary after decouple

            bool FlipSprite = false;

            
#if DEBUG || PROFILE
            if (spriteSheetSourceRect.Width == 0f)
            {
                throw new Exception();
            }
#endif

            float z = 0f;
            if (quad == null)
            {
                quad = new VertexOverlayGroundSpriteQuad[4];
            }

            int crtEffectTextureWidth = The.Client.Renderer.Scanlines.Width;
            int crtEffectTextureHeight = The.Client.Renderer.Scanlines.Width;

            float textureLeft = (float)spriteSheetSourceRect.X / (float)spriteSheetTexture.Width;
            float textureTop = (float)spriteSheetSourceRect.Y / (float)spriteSheetTexture.Height;

            float textureRight = textureLeft + (float)spriteSheetSourceRect.Width / (float)spriteSheetTexture.Width;
            float textureBottom = textureTop + (float)spriteSheetSourceRect.Height / (float)spriteSheetTexture.Height;

            // these are into the scanlines texture:
            float crtTextureLeft = 0f;
            float crtTextureTop = 0f;
            float crtTextureRight = spriteSheetSourceRect.Width / (float)crtEffectTextureWidth;
            float crtTextureBottom = spriteSheetSourceRect.Height / (float)crtEffectTextureHeight;

            quad[0] = new VertexOverlayGroundSpriteQuad();            
            quad[0].Position = new Vector3(posLeft, posTop, z);
          //  quad[0].WorldPosition = worldPosition;
            quad[0].TextureCoordinate = (FlipSprite ? new Vector2(textureRight, textureTop) : new Vector2(textureLeft, textureTop));
            quad[0].TruncatedEffectCoordinate = new Vector2(crtTextureLeft, crtTextureTop);
            
            quad[1] = new VertexOverlayGroundSpriteQuad();
            quad[1].Position = new Vector3(posRight, posTop, z);
          //  quad[1].WorldPosition = worldPosition;
            quad[1].TextureCoordinate = (FlipSprite ? new Vector2(textureLeft, textureTop) : new Vector2(textureRight, textureTop));
            quad[1].TruncatedEffectCoordinate = new Vector2(crtTextureRight, crtTextureTop);
            
            quad[2] = new VertexOverlayGroundSpriteQuad();
            quad[2].Position = new Vector3(posRight, posBottom, z);
         //   quad[2].WorldPosition = worldPosition;
            quad[2].TextureCoordinate = (FlipSprite ? new Vector2(textureLeft, textureBottom) : new Vector2(textureRight, textureBottom));
            quad[2].TruncatedEffectCoordinate = new Vector2(crtTextureRight, crtTextureBottom);
            
            quad[3] = new VertexOverlayGroundSpriteQuad();
            quad[3].Position = new Vector3(posLeft, posBottom, z);
        //    quad[3].WorldPosition = worldPosition;
            quad[3].TextureCoordinate = (FlipSprite ? new Vector2(textureRight, textureBottom) : new Vector2(textureLeft, textureBottom));
            quad[3].TruncatedEffectCoordinate = new Vector2(crtTextureLeft, crtTextureBottom);
            

            float drawScan = (drawScanlines ? 1f : 0f);

            for (int i = 0; i < quad.Length; i++)
            {
                quad[i].DrawScanlines = drawScan;
                quad[i].Color = color;
            }
        
        }


       
    }



    public struct VertexOverlayGroundSpriteQuad : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector2 TruncatedEffectCoordinate;
        public float DrawScanlines;
        public Vector4 Color;

        public static int SizeInBytes = (3 + 2 + 2 + 1 + 4) * sizeof(float);
        public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1 ), 
             new VertexElement(sizeof(float) * 7, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2 ),
             new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3 )
         };

        private readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }
    }
}
