using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface.MapGUI
{
    /// <summary>
    /// the rectangle that gets drawn when the user left-drags the mouse on the map. it is drawn 'along the ground'
    /// </summary>
    public class SelectRectangle
    {
        List<GUIRect> guiRects = new List<GUIRect>();

        Rectangle? source;

        OverlayGroundSpriteQuad[] quads;

        public bool Visible = false;


        public SelectRectangle()
        {
            quads = new OverlayGroundSpriteQuad[9];

            for (int i = 0; i < 9; i++)
            {
                quads[i] = new OverlayGroundSpriteQuad();
            }
                       
        }


        public void SetSize(int x, int y, int width, int height)
        {
            if (!source.HasValue)
            {

                source = The.Client.FlatSpriteSheet.GetSourceRectangle("selectionRectangle");

        //        source = The.Client.FlatSpriteSheet.SourceRectangle("gridsquare");

            }

            // we can tint this box...
            Box.CreateBox(guiRects, source.Value, new Microsoft.Xna.Framework.Rectangle(x, y, width, height), 10, Color.LightGray, Color.LightGray); //, Color.Blue, Color.Red);
                      
        }

        public void SetupQuad(VertexOverlayGroundSpriteQuad[] overlayVertices, ref int index) //SpriteBatch spriteBatch) 
        {
            if (!Visible)
            {
                return;
            }

            int xScreen, yScreen;
            Rectangle? destination = null;
            // Color color = Color.White;

            int i = 0;
            OverlayGroundSpriteQuad quad;
            foreach (var rect in guiRects)
            {
                quad = quads[i];
               // map.AbsoluteTileCenterToScreen(selTile.X, selTile.Y, out xScreen, out yScreen);

               // destination = new Rectangle(xScreen - source.Width / 2, yScreen - source.Height / 2, source.Width, source.Height);

                //player.Draw(spriteBatch, destination.Value, color);

                //selectionCurrentColor = selectedCirclePlayer.GetCurrentColor(selectionTintingColor);
                //int destinationLeft = rect.

                quad.SetupQuadVertices(rect.Destination.Left, rect.Destination.Top, rect.Destination.Right, rect.Destination.Bottom,
                    rect.Source, The.Client.FlatSpriteSheet.Texture, false /* true*/, rect.Color.ToVector4() /*Vector4.One*/); // selectionCurrentColor.ToVector4());

                i++;

                quad.CopyQuadToVertexBuffer(overlayVertices, index);
                index++;
            }       
        }
    }
}
