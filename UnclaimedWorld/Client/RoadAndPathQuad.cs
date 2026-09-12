using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Map;

namespace UWGame.ClientSide
{
    public class RoadAndPathQuad  
    {
        protected VertexGroundFeature[] quad;
        //public bool FlipSprite = false;

      //  public Vector4 Tint = Vector4.One;

        public RoadAndPathQuad()
        {
        }

       /* public RoadAndPathQuad(RoadAndPathQuad original)
        {
            quad = new VertexGroundFeature[original.quad.Length];
            Array.Copy(original.quad, quad, original.quad.Length);

            FlipSprite = original.FlipSprite;
        }*/

        /// <summary>
        /// returns false if the buffer is full and the quad was not added.
        /// </summary>
        /// <param name="featureVertices"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool CopyQuadToVertexBuffer(VertexGroundFeature[] featureVertices, int index)
        {            
            index *= 4;

           /* if (index >= featureVertices.Count())
                index = featureVertices.Count() - 1;
            */

            if (index + 4 >= featureVertices.Length)
            {
                return false;
                //index = featureVertices.Count() - 1;
            }
            else
            {
                for (int i = index; i < index + 4; i++)
                {
                    featureVertices[i] = quad[i - index];
                }

                return true;
            }
           
        }


       
        /// <summary>
        /// push these properties from RenderAsGroundSprite during recomputation. they do not require recomputed coordinates.
        /// </summary>
        public void SetProperties(Vector4 tint)
        {           
           // this.Tint = tint;


            for (int i = 0; i < quad.Length; i++)
            {
                quad[i].TintColor = tint;
            }
        }

        /// <summary>
        /// Base center offset is the center of the rectangle for roads and paths, but is the base center marked on the utility map for structure ground sprites!
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="baseCenterOffset"></param>
        /// <param name="sourceRect"></param>
        /// <param name="texture"></param>
        /// <param name="tint"></param>
        public void SetupQuadVertices(Vector3 worldPosition, Vector2 baseCenterOffset, Rectangle sourceRect, Texture2D texture, bool flipSprite) //, Vector4 tint)
        {
            ///quad = new FeatureQuad();

           
            //Rectangle sourceRect = UWGame.SimSide.Instance.Map.renderer.BillboardSpriteSheet.SourceRectangle(spriteName);
           // Texture2D texture = UWGame.SimSide.Instance.Map.renderer.BillboardSpriteSheet.Texture;

            float width = sourceRect.Width;
            float height = sourceRect.Height;

            // NEW
            float posLeft = -baseCenterOffset.X;
            float posRight = posLeft + width;

            float posTop = -baseCenterOffset.Y;
            float posBottom = posTop + height;


          /*  


            float baseX = width / 2f;
            float baseY = height / 2f;

            float posLeft = -baseX;
            float posRight = baseX;

            float posTop = -baseY;
            float posBottom = posTop + height;*/


            float z = 0f;
            quad = new VertexGroundFeature[4];

            float textureLeft = (float)sourceRect.X / (float)texture.Width;
            float textureTop = (float)sourceRect.Y / (float)texture.Height;

            float textureRight = textureLeft + (float)sourceRect.Width / (float)texture.Width;
            float textureBottom = textureTop + (float)sourceRect.Height / (float)texture.Height;

            Vector4 tint = Vector4.One;

            quad[0] = new VertexGroundFeature();
            quad[0].Position = new Vector3(posLeft, posTop, z);
            quad[0].TextureCoordinate = (flipSprite ? new Vector2(textureRight, textureTop) : new Vector2(textureLeft, textureTop));// new Vector2(textureLeft, textureTop);
            quad[0].WorldPosition = worldPosition;
            quad[0].TintColor = tint;


            quad[1] = new VertexGroundFeature();
            quad[1].Position = new Vector3(posRight, posTop, z);
            quad[1].TextureCoordinate = (flipSprite ? new Vector2(textureLeft, textureTop) : new Vector2(textureRight, textureTop)); //new Vector2(textureRight, textureTop);
            quad[1].WorldPosition = worldPosition;
            quad[1].TintColor = tint;
           
            quad[2] = new VertexGroundFeature();
            quad[2].Position = new Vector3(posRight, posBottom, z);
            quad[2].TextureCoordinate = (flipSprite ? new Vector2(textureLeft, textureBottom) : new Vector2(textureRight, textureBottom));// new Vector2(textureRight, textureBottom);
            quad[2].WorldPosition = worldPosition;
            quad[2].TintColor = tint;
           
            quad[3] = new VertexGroundFeature();
            quad[3].Position = new Vector3(posLeft, posBottom, z);
            quad[3].TextureCoordinate = (flipSprite ? new Vector2(textureRight, textureBottom) : new Vector2(textureLeft, textureBottom)); // new Vector2(textureLeft, textureBottom);
            quad[3].WorldPosition = worldPosition;
            quad[3].TintColor = tint;

          //  SetupFourCornerVertices(ref worldPosition, posLeft, posTop, posRight, posBottom, z, textureLeft, textureTop, textureRight, textureBottom);

          //  SetupQuadVertices(Location, posLeft, posTop, posRight, posBottom, sourceRect, texture, 0f, 0f);


            //isDirty = false;

        }

    /*    public void SetupQuadVertices(Vector3 worldPosition, float posLeft, float posTop, float posRight, float posBottom,
            Rectangle spriteSheetSourceRect, Texture2D spriteSheetTexture, float randomValue, float bendyness)
        {
            float z = 0f; 
            quad = new VertexRoadAndPath[4];

            float textureLeft = (float)spriteSheetSourceRect.X / (float)spriteSheetTexture.Width;
            float textureTop = (float)spriteSheetSourceRect.Y / (float)spriteSheetTexture.Height;

            float textureRight = textureLeft + (float)spriteSheetSourceRect.Width / (float)spriteSheetTexture.Width;
            float textureBottom = textureTop + (float)spriteSheetSourceRect.Height / (float)spriteSheetTexture.Height;

            SetupFourCornerVertices(worldPosition, posLeft, posTop, posRight, posBottom, z, textureLeft, textureTop, textureRight, textureBottom);
        }*/


      /*  private void SetupFourCornerVertices(ref Vector3 worldPosition, float posLeft, float posTop, float posRight, float posBottom, float z, 
            float textureLeft, float textureTop, float textureRight, float textureBottom)
        {
            quad[0] = new VertexGroundFeature();
            quad[0].Position = new Vector3(posLeft, posTop, z);
            quad[0].TextureCoordinate = new Vector2(textureLeft, textureTop);
            quad[0].WorldPosition = worldPosition;
         //   quad[0].Bendyness = bendyness; // only top two vertices can bend.       

            quad[1] = new VertexGroundFeature();
            quad[1].Position = new Vector3(posRight, posTop, z);
            quad[1].TextureCoordinate = new Vector2(textureRight, textureTop);
            quad[1].WorldPosition = worldPosition;
       //     quad[1].Bendyness = bendyness;

            quad[2] = new VertexGroundFeature();
            quad[2].Position = new Vector3(posRight, posBottom, z);
            quad[2].TextureCoordinate = new Vector2(textureRight, textureBottom);
            quad[2].WorldPosition = worldPosition;
       //     quad[2].Bendyness = 0f;

            quad[3] = new VertexGroundFeature();
            quad[3].Position = new Vector3(posLeft, posBottom, z);
            quad[3].TextureCoordinate = new Vector2(textureLeft, textureBottom);
            quad[3].WorldPosition = worldPosition;
       //     quad[3].Bendyness = 0f;
        
       
           // return worldPosition;
        }*/
    }
}
