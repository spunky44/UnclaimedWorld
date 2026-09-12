using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Interface.MapGUI
{
    public abstract class ÏnfluenceRender
    {
        protected OverlayGroundSpriteQuad quad;

        /// <summary>
        /// sprite rect
        /// </summary>
        protected Rectangle sourceRectangle;


        protected bool DrawTile(int x, int y, int tempIndex, Color color, VertexOverlayGroundSpriteQuad[] overlayVertices)
        {
            System.Drawing.RectangleF destination;

            Vector2 screenCoords = The.MapUI.TileEdgeToScreen(x, y);

            destination = new System.Drawing.RectangleF(screenCoords.X, screenCoords.Y, (float)MapManager.tileSize, (float)MapManager.tileSize);

            quad.SetupQuadVertices(destination.Left, destination.Top, destination.Right, destination.Bottom,
                sourceRectangle, The.Client.FlatSpriteSheet.Texture,
                true,
                color.ToVector4());

            if (quad.CopyQuadToVertexBuffer(overlayVertices, tempIndex))
            {
                return true;
            }
            else
            {
                // buffer is full..?!?               
                return false;
            }

        }

        protected bool DrawSubtile(int x, int y, int tempIndex, Color color, VertexOverlayGroundSpriteQuad[] overlayVertices)
        {
            System.Drawing.RectangleF destination;

            Vector2 screenCoords = The.MapUI.SubtileEdgeToScreen(x, y);

            destination = new System.Drawing.RectangleF(screenCoords.X, screenCoords.Y, (float)MapManager.subTileSize, (float)MapManager.subTileSize);

            quad.SetupQuadVertices(destination.Left, destination.Top, destination.Right, destination.Bottom,
                sourceRectangle, The.Client.FlatSpriteSheet.Texture,
                true,
                color.ToVector4());

            if (quad.CopyQuadToVertexBuffer(overlayVertices, tempIndex))
            {
                return true;
            }
            else
            {
                // buffer is full..?!?               
                return false;
            }

        }
    }
}
