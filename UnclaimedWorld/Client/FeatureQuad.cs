using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.ClientSide
{
    /// <summary>
    /// only used by RenderAsBillboard it seems.
    /// </summary>
    public class FeatureQuad : QuadBase
    {
        protected VertexFeatureQuad[] quad;

        /// <summary>
        /// not sure why this is needed
        /// </summary>
        private Vector4 Tint = Vector4.One;
        private float Bendyness = 0f;
        private float RandomValue = 0f;

       
        public float WidthHeightRatio = 1f;

        public FeatureQuad() { }

     


        /// <summary>
        /// push these properties from RenderAsBillboard during recomputation. they do not require recomputed coordinates.
        /// </summary>
        public void SetProperties(float bendyness, float randomValue, Vector4 tint)
        {          
            this.Bendyness = bendyness;
            this.RandomValue = randomValue;
            this.Tint = tint;

            quad[0].Bendyness = Bendyness; // only top two vertices can bend.       
            quad[1].Bendyness = Bendyness; // 
            quad[2].Bendyness = 0f;
            quad[3].Bendyness = 0f;

            for (int i = 0; i < quad.Length; i++)
            {
                VertexFeatureQuad currentQuad = quad[i];

                currentQuad.Random = RandomValue;

                currentQuad.Tint = Tint;

                quad[i] = currentQuad; // needed because struct is a value type
            }
        }

        public void SetTint(Vector4 tint)
        {
            this.Tint = tint;

            for (int i = 0; i < quad.Length; i++)
            {
                VertexFeatureQuad currentQuad = quad[i];
                
                currentQuad.Tint = Tint;

                quad[i] = currentQuad; // needed because struct is a value type
            }
        }

        public bool CopyQuadToVertexBuffer(VertexFeatureQuad[] featureVertices, int index)
        {           
            index *= 4;

            if (index + 4 >= featureVertices.Length)
            {
                return false;                
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

      
        
        public void SetupQuadVertices(Vector3 worldPosition, float posLeft, float posTop, float posRight, float posBottom, bool isFlipped) 
        {
            float z = 0f; // if this is constant zero, depth test must be off.

            quad = new VertexFeatureQuad[4];
           
            float textureLeft = (float)StaticSourceRectangle.X / (float)Texture.Width;
            float textureTop = (float)StaticSourceRectangle.Y / (float)Texture.Height;

            float textureRight = textureLeft + (float)StaticSourceRectangle.Width / (float)Texture.Width;
            float textureBottom = textureTop + (float)StaticSourceRectangle.Height / (float)Texture.Height;
            
            worldPosition = SetupFourCornerVertices(worldPosition, posLeft, posTop, posRight, posBottom, z, textureLeft, textureTop, textureRight, textureBottom, 1f, isFlipped);      
        }

       

        public void SetupQuadVerticesFromBaseCenter(Vector3 worldPosition, float posLeft, float posTop, float posRight, float posBottom,
             Vector2 baseCenter, bool isFlipped) 
        {
            float z = 0f;
            quad = new VertexFeatureQuad[4];

            float textureLeft = (float)StaticSourceRectangle.X / (float)Texture.Width;
            float textureTop = (float)StaticSourceRectangle.Y / (float)Texture.Height;

            float textureRight = textureLeft + (float)StaticSourceRectangle.Width / (float)Texture.Width;
            float textureBottom = textureTop + (float)(baseCenter.Y) / (float)Texture.Height; 

            // since structures tend to be widest at the base center, squash the bottom vertices a bit:
            worldPosition = SetupFourCornerVertices(worldPosition, posLeft, posTop, posRight, posBottom, z, textureLeft, textureTop, textureRight, textureBottom, 0.75f, isFlipped); 
        }

        private Vector3 SetupFourCornerVertices(Vector3 worldPosition, float posLeft, float posTop, float posRight, float posBottom,
             float z, float textureLeft, float textureTop, float textureRight, float textureBottom, float squashBottomFactor, bool flipSprite) 
        {
            //worldPosition.Z = worldPosition.Y; // ??? how do we pass the depth test?

            bool flipNormalMap = flipSprite;

            quad[0] = new VertexFeatureQuad();
            quad[0].Position = new Vector3(posLeft, posTop, z);
            quad[0].WorldPosition = worldPosition;
            quad[0].TextureCoordinate = (flipSprite ? new Vector2(textureRight, textureTop) : new Vector2(textureLeft, textureTop));
            quad[0].NormalTextureCoordinate = (flipNormalMap ? new Vector2(textureRight, textureTop) : new Vector2(textureLeft, textureTop));
            quad[0].Bendyness = Bendyness; // bendyness; // only top two vertices can bend.       
          
            quad[1] = new VertexFeatureQuad();
            quad[1].Position = new Vector3(posRight, posTop, z);
            quad[1].WorldPosition = worldPosition;
            quad[1].TextureCoordinate = (flipSprite ? new Vector2(textureLeft, textureTop) : new Vector2(textureRight, textureTop));
            quad[1].NormalTextureCoordinate = (flipNormalMap ? new Vector2(textureLeft, textureTop) : new Vector2(textureRight, textureTop));
            quad[1].Bendyness = Bendyness; // bendyness;
    
            quad[2] = new VertexFeatureQuad();
            quad[2].Position = new Vector3(posRight * squashBottomFactor, posBottom, z);
            quad[2].WorldPosition = worldPosition;
            quad[2].TextureCoordinate = (flipSprite ? new Vector2(textureLeft, textureBottom) : new Vector2(textureRight, textureBottom));
            quad[2].NormalTextureCoordinate = (flipNormalMap ? new Vector2(textureLeft, textureBottom) : new Vector2(textureRight, textureBottom));
            quad[2].Bendyness = 0f;


            quad[3] = new VertexFeatureQuad();
            quad[3].Position = new Vector3(posLeft * squashBottomFactor, posBottom, z);
            quad[3].WorldPosition = worldPosition;
            quad[3].TextureCoordinate = (flipSprite ? new Vector2(textureRight, textureBottom) : new Vector2(textureLeft, textureBottom));
            quad[3].NormalTextureCoordinate = (flipNormalMap ? new Vector2(textureRight, textureBottom) : new Vector2(textureLeft, textureBottom));
            quad[3].Bendyness = 0f;

            

            for (int i = 0; i < quad.Length; i++)
            {
                quad[i].Random = RandomValue; // randomValue;
                quad[i].FlipNormals = (flipNormalMap ? 1f : 0f);
                
                quad[i].WidthHeightRatio = WidthHeightRatio;
                quad[i].Tint = Tint;
            }


            return worldPosition;
        }
    }



    public struct VertexFeatureQuad : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector2 NormalTextureCoordinate;
        public float Random;
        public float Bendyness; // 1 for top two vertices of the quad, zero for bottom.
        public Vector3 WorldPosition;
        
        public float FlipNormals;
        public float WidthHeightRatio;

        public Vector4 Tint;

        public static int SizeInBytes = (3 + 2 + 2 + 1 + 1 + 3 + 1 + 1 + 4) * sizeof(float); // +1 ???
        public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1 ),
             new VertexElement(sizeof(float) * 7, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2 ),
             new VertexElement(sizeof(float) * 8, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3 ),
             new VertexElement(sizeof(float) * 9, VertexElementFormat.Vector3, VertexElementUsage.Position, 1 ),
             new VertexElement(sizeof(float) * 12, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4 ),
             new VertexElement(sizeof(float) * 13, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 5 ),
             new VertexElement(sizeof(float) * 14, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 6)

          /*   new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
             new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement( 0, sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1 ),
             new VertexElement( 0, sizeof(float) * 7, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 2 ),
             new VertexElement( 0, sizeof(float) * 8, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 3 ),
             new VertexElement( 0, sizeof(float) * 9, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 1 ),
             new VertexElement( 0, sizeof(float) * 12, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 4 ),
             new VertexElement( 0, sizeof(float) * 13, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 5 )
          */
          
         };

        private readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }
    }
}
