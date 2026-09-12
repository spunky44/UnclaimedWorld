using System;
using System.Collections.Generic;
using System.Text;

namespace UWGame.ClientSide
{
    #region Using Statements
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    #endregion



    public partial class QuadRendererComponent //: Microsoft.Xna.Framework.DrawableGameComponent  
    {

      //  VertexDeclaration vertexDecl = null;
        VertexPositionTexture[] verts = null;
        short[] ib = null;

       /* public QuadRendererComponent(Game game)
            : base(game)
        {

        }
        */
        public QuadRendererComponent()
        {

        }

        public void Render(GraphicsDevice device, Vector2 v1, Vector2 v2)
        {          

            verts[0].Position.X = v2.X;
            verts[0].Position.Y = v1.Y;

            verts[1].Position.X = v1.X;
            verts[1].Position.Y = v1.Y;

            verts[2].Position.X = v1.X;
            verts[2].Position.Y = v2.Y;

            verts[3].Position.X = v2.X;
            verts[3].Position.Y = v2.Y;

            device.DrawUserIndexedPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
        }

        /*
        public void Render(Vector2 v1, Vector2 v2)
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)
                this.Game.Services.GetService(typeof(IGraphicsDeviceService));

            GraphicsDevice device = graphicsService.GraphicsDevice;
          //  device.VertexDeclaration = vertexDecl;

            verts[0].Position.X = v2.X;
            verts[0].Position.Y = v1.Y;

            verts[1].Position.X = v1.X;
            verts[1].Position.Y = v1.Y;

            verts[2].Position.X = v1.X;
            verts[2].Position.Y = v2.Y;

            verts[3].Position.X = v2.X;
            verts[3].Position.Y = v2.Y;

            device.DrawUserIndexedPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
        }*/

        public void LoadContent()
        {
           /* IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)
                this.Game.Services.GetService(typeof(IGraphicsDeviceService));
            */

            // why is this content..?
            verts = new VertexPositionTexture[]
            {
                new VertexPositionTexture(
                    new Vector3(0,0,0),
                    new Vector2(1,1)),
                new VertexPositionTexture(
                    new Vector3(0,0,0),
                    new Vector2(0,1)),
                new VertexPositionTexture(
                    new Vector3(0,0,0),
                    new Vector2(0,0)),
                new VertexPositionTexture(
                    new Vector3(0,0,0),
                    new Vector2(1,0))
            };

            ib = new short[] { 0, 1, 2, 2, 3, 0 };
        }


       /* public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }*/
        /*
        protected override void LoadContent()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)
                this.Game.Services.GetService(typeof(IGraphicsDeviceService));

            verts = new VertexPositionTexture[]
            {
                new VertexPositionTexture(
                    new Vector3(0,0,0),
                    new Vector2(1,1)),
                new VertexPositionTexture(
                    new Vector3(0,0,0),
                    new Vector2(0,1)),
                new VertexPositionTexture(
                    new Vector3(0,0,0),
                    new Vector2(0,0)),
                new VertexPositionTexture(
                    new Vector3(0,0,0),
                    new Vector2(1,0))
            };

            ib = new short[] { 0, 1, 2, 2, 3, 0 };
        }*/

      /*  protected override void UnloadContent()
        {
           
            base.UnloadContent();

            verts = null;
            ib = null;
        }*/

    }
}
