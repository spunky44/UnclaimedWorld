using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using WindowSystem;

namespace UWGame.Client.Interface.MapGUI
{
    public class MapAreaRender
    {
        public static float Opacity = 0.38f; // 0.5f;

        public Color Color = Color.Red;

        public bool DrawAsOverlay;

      //  Animation2DPlayer selectedZonePlayer;
     //   Animation2D selectionAnimation;
       

        OverlayGroundSpriteQuad quad;

        private static Rectangle sourceRectangle;

        private MapArea parent;

        /// <summary>
        /// used in culling
        /// </summary>
      //  private Point upperLeft;
     //   private Point lowerRight;

        private Rectangle? boundingRectangle;

        private bool isDirty = true;

        public MapAreaRender(MapArea parent)
        {
            this.parent = parent;

            
            quad = new OverlayGroundSpriteQuad();

            if (this.parent.Zone != null)
            {
                Color = Color.Turquoise; // Common.GetRandomColorFromSeed(parent.Zone.ID + 1);
            }
           
            //Color = Color.White;
         //   Color.A = 128;
        }

        private bool isSelected;
        public bool IsSelected
        {
            set
            {
                isSelected = value;

                if (value == true)
                {
                    The.InGameUI.SelectedCyclePlayer.StartAnimation(); //The.InGameUI.SelectedCycleAnimation);
                }
                else
                {
                   // selectedZonePlayer.StopAnimation();
                }
            }
        }


        public void PostLoadContent()
        {
            sourceRectangle = The.Client.FlatSpriteSheet.GetSourceRectangle("mapgui_zone_base");
            
        }

        public void SetIsDirty()
        {
            isDirty = true;
        }

        public void SetDimensions()
        {
            boundingRectangle = parent.GetBoundingBoxInTiles();

        }

        public bool IsOnScreen()
        {
            if (boundingRectangle.HasValue)
            {
                return The.MapUI.TileAreaIsOnScreen(boundingRectangle.Value);
            }
            else return false;
        }


        /// <summary>
        /// the zones are drawn as Overlay. Also additively rendered...? maybe not. will be all white when they are stacked.
        /// </summary>
        /// <param name="overlayVertices"></param>
        /// <param name="index"></param>
        public void SetupQuad(VertexOverlayGroundSpriteQuad[] overlayVertices, ref int index) //SpriteBatch spriteBatch) 
        {
            if (isDirty)
            {
                SetDimensions();
                isDirty = false;
            }

            if (!IsOnScreen())
            {
                return;
            }

            int xScreen, yScreen;
            Rectangle? destination = null;

            MapManager map = The.Map;

          //  Color selectionCurrentColor = Color; 
            Color currentColor;

            if (isSelected)
            {
                currentColor = The.InGameUI.SelectedCyclePlayer.GetCurrentColor(Color);
            }
            else
            {
                currentColor = Color;
            }

            currentColor *= Opacity;

            int tempIndex = index;



            parent.IterateArea(tile =>
                {
                    if (The.MapUI.TileIsOnScreen(tile.X, tile.Y))
                    {
                       
                        The.MapUI.TileCenterToScreen(tile.X, tile.Y, out xScreen, out yScreen);

                        destination = new Rectangle(xScreen - sourceRectangle.Width / 2, yScreen - sourceRectangle.Height / 2, sourceRectangle.Width, sourceRectangle.Height);
                        

                        quad.SetupQuadVertices(destination.Value.Left, destination.Value.Top, destination.Value.Right, destination.Value.Bottom,
                            sourceRectangle, The.Client.FlatSpriteSheet.Texture,
                            false, //true, // draw scanlines?
                            currentColor.ToVector4());


                        if (quad.CopyQuadToVertexBuffer(overlayVertices, tempIndex))
                        {
                            tempIndex++;
                        }
                    }
                });


            index = tempIndex;
        }
    }
}
