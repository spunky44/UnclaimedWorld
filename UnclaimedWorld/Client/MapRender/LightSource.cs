using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Items;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using SpriteSheetRuntime;
using UWGame.SimSide;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Buildings;

namespace UWGame.ClientSide.Map
{
    /// <summary>
    /// NEW: make one for each LightingType (one billboard)
    /// </summary>
    public class LightSource : ILocatable
    {
        /// <summary>
        /// Can be null.
        /// </summary>
        public Renderable Parent;

        protected Vector3 location = Vector3.Zero;
        /// <summary>
        /// These are world coordinates for game object
        /// </summary>
        public virtual Vector3 Location
        {
            get { return location; }
            set
            {
                location = The.Map.ClampWorldPosition(value);
            }
        }

        public bool LightIsOn;

      //  public bool FlipHorizontally = false;

        protected VertexLightSourceQuad[] quad;

        // public LightSourceType LightSourceType;

        bool spriteOrLocationIsDirty = true;


        public LightingType LightingType;

        public Renderable AsRenderable
        {
            get
            {
                return null;
            }
        }

        public RenderAsBillboard AsRenderAsBillboard
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// NEW - moved from Lighting
        /// </summary>
        /// <param name="lightingType"></param>
        /// <param name="parent"></param>
        public LightSource(LightingType lightingType, Renderable parent) //Entity parent)
        {
            this.Parent = parent;
            this.LightingType = lightingType;
        }



        /// <summary>
        /// location must be at ground level!
        /// </summary>
        /// <param name="location"></param>
        public LightSource(Vector3 location)
        {            
            // use the 'base center' for the structure, with a bit added to the Y to make it in front!

            // Put in a real height over ground as z value!!!

            this.Location = new Vector3(location.X, location.Y + 1f, location.Z);
        }

        public void SetIsDirty()
        {
            spriteOrLocationIsDirty = true;
        }

        /// <summary>
        /// NEW: moved from Lighting
        /// </summary>
        /// <param name="baseCenterOffset"></param>
        /// <param name="lightOffset"></param>
        /// <returns></returns>
      /*  private void PlaceLightSource(Vector2 baseCenterOffset, LightSourceOffset lightOffset)
        {*/
           /* LightSource newLightSource = new LightSource(Parent.Location.Value);
            newLightSource.FlipHorizontally = Parent.FlipHorizontally;
            newLightSource.LightSourceType = lightOffset.LightSourceType;
            */

            // turn it on for testing...
           // newLightSource.LightIsOn = true;

            /*
            if (Parent.FlipHorizontally)
            {
                // TODO: fix this when WidthInTiles is deprecated
                // lightOffset.Offset.X = Parent.EntityType.StructureType.WidthInTiles * MapManager.tileSize - lightOffset.Offset.X;
            }*/

            // add the offsets:
         /*   Vector2 offsetWithRespectToBaseOfCenter;
            offsetWithRespectToBaseOfCenter.X = lightOffset.Offset.X - baseCenterOffset.X;
            offsetWithRespectToBaseOfCenter.Y = lightOffset.Offset.Y - baseCenterOffset.Y;

            Vector2 offset = new Vector2(offsetWithRespectToBaseOfCenter.X - LightingType.GetOffset(Parent.FlipHorizontally).X,
                                        (offsetWithRespectToBaseOfCenter.Y - LightingType.GetOffset(Parent.FlipHorizontally).Y));


            SetupQuadVertices(Location, offset, 20f);
            
        }*/

        public void CopyQuadToVertexBuffer(VertexLightSourceQuad[] lightSourceVertices, int index) 
        {
            if (spriteOrLocationIsDirty)
            {
                SetupQuadVertices();
            }

            index *= 4;
            for (int i = index; i < index + 4; i++)
            {
                lightSourceVertices[i] = quad[i - index];
            }
        }

       // public void SetupQuadVertices(Vector3 worldPosition, Vector2 offset, float heightOverGround)
        public void SetupQuadVertices()
        {
            float heightOverGround = 20f;

            Rectangle spriteSheetSourceRect = GameData.Instance.LightSourcesSpriteSheet.GetSourceRectangle(LightingType.SpriteName);// "circular_big");

            // add the offsets:
            /*  Vector2 offsetWithRespectToBaseOfCenter;
              offsetWithRespectToBaseOfCenter.X = LightingType.Offset.X - baseCenterOffset.X;
              offsetWithRespectToBaseOfCenter.Y = LightingType.Offset.Y - baseCenterOffset.Y;
              */
            Vector2 offsetWithRespectToBaseOfCenter;
            offsetWithRespectToBaseOfCenter.X = LightingType.Offset.X; // base center needed??
            offsetWithRespectToBaseOfCenter.Y = LightingType.Offset.Y;

            Vector2 offset = offsetWithRespectToBaseOfCenter;
            /*new Vector2(offsetWithRespectToBaseOfCenter.X - LightingType.GetOffset(Parent.FlipHorizontally).X,
                 (offsetWithRespectToBaseOfCenter.Y - LightingType.GetOffset(Parent.FlipHorizontally).Y));*/

            this.Location = new Vector3(Parent.Location.Value.X, Parent.Location.Value.Y + 1f, Parent.Location.Value.Z);

            SetupQuadVertices(Location, offset, heightOverGround, spriteSheetSourceRect, GameData.Instance.LightSourcesSpriteSheet.Texture);
        }

        public void SetupQuadVertices(Vector3 worldPosition, Vector2 offset, float heightOverGround, Rectangle textureRectangle, Texture2D texture) //, float posLeft, float posTop, float posRight, float posBottom)
        {
            
            float posLeft = offset.X;  //-spriteSheetSourceRect.Width; // / 2f;
            float posRight = textureRectangle.Width + offset.X; // / 2f;

            float posTop = offset.Y; // -spriteSheetSourceRect.Height / 2f;
            float posBottom = textureRectangle.Height + offset.Y; // -posTop;

            
            // World position y value is used to sort objects from back to front (distance from viewer) when doing lighting.
            // so, it should be at ground level if possible. (For trees, this is true as y is at the root. For buildings, it is a bit different. Here, y is in the center of the building.)
            // use vertex position values to shift the quad 'into the air', NOT the worldPosition.y!
            Location = worldPosition;           
            
            //Texture2D texture = GameData.Instance.LightSourcesSpriteSheet.Texture;
                       
            float z = 0f;
            quad = new VertexLightSourceQuad[4];

            float textureLeft = (float)textureRectangle.X / (float)texture.Width;
            float textureTop = (float)textureRectangle.Y / (float)texture.Height;

            float textureRight = textureLeft + (float)textureRectangle.Width / (float)texture.Width;
            float textureBottom = textureTop + (float)textureRectangle.Height / (float)texture.Height;

            // Height: This is only used in lighting calculations, not in placement or sorting:
            float topHeightOverGround = Location.Z + (float)texture.Height / 2f;
            float bottomHeightOverGround = Common.ClampBottom(Location.Z - (float)texture.Height / 2f, 0f);

            quad[0] = new VertexLightSourceQuad();
            quad[0].Position = new Vector3(posLeft, posTop, z);
            quad[0].WorldPosition = worldPosition; //new Vector3(worldPosition.X, worldPosition.Y, topHeightOverGround);
            quad[0].TextureCoordinate = (Parent?.FlipHorizontally == true ? new Vector2(textureRight, textureTop) : new Vector2(textureLeft, textureTop));
            quad[0].HeightAboveGround = new Vector2(topHeightOverGround, 0f);

            quad[1] = new VertexLightSourceQuad();
            quad[1].Position = new Vector3(posRight, posTop, z);
            quad[1].WorldPosition = worldPosition;  //new Vector3(worldPosition.X, worldPosition.Y, topHeightOverGround);  //worldPosition;
            quad[1].TextureCoordinate = (Parent?.FlipHorizontally == true ? new Vector2(textureLeft, textureTop) : new Vector2(textureRight, textureTop));
            quad[1].HeightAboveGround = new Vector2(topHeightOverGround, 0f);

            quad[2] = new VertexLightSourceQuad();
            quad[2].Position = new Vector3(posRight, posBottom, z);
            quad[2].WorldPosition = worldPosition;  //new Vector3(worldPosition.X, worldPosition.Y, bottomHeightOverGround); //worldPosition;
            quad[2].TextureCoordinate = (Parent?.FlipHorizontally == true ? new Vector2(textureLeft, textureBottom) : new Vector2(textureRight, textureBottom));
            quad[2].HeightAboveGround = new Vector2(bottomHeightOverGround, 0f);

            quad[3] = new VertexLightSourceQuad();
            quad[3].Position = new Vector3(posLeft, posBottom, z);
            quad[3].WorldPosition = worldPosition;  //new Vector3(worldPosition.X, worldPosition.Y, bottomHeightOverGround); //worldPosition;
            quad[3].TextureCoordinate = (Parent?.FlipHorizontally == true ? new Vector2(textureRight, textureBottom) : new Vector2(textureLeft, textureBottom));
            quad[3].HeightAboveGround = new Vector2(bottomHeightOverGround, 0f);


            spriteOrLocationIsDirty = false;
        }

        public int CompareTo(object obj)
        {
            ILocatable comparable = obj as ILocatable;
            return (int)(Location.Y - comparable.Location.Y);
        }
        
    }


    public struct VertexLightSourceQuad : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector3 WorldPosition;
        
        /// <summary>
        /// only x is used...
        /// </summary>
        public Vector2 HeightAboveGround;  

        public static int SizeInBytes = (3 + 2 + 3 + 2) * sizeof(float);
        public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector3, VertexElementUsage.Position, 1 ),
             new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1 )
         };

        private readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }
    }
}
