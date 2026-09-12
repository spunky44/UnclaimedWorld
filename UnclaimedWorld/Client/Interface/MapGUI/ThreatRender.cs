using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Interface.MapGUI
{
    public class ThreatRender : ÏnfluenceRender
    {
       // float Opacity = 0.6f;

        List<MapClient.OverlayLimit> threatMapLimits = new List<MapClient.OverlayLimit>();

        public ThreatRender()
        {
            quad = new OverlayGroundSpriteQuad();
            
            MapClient.GetThreatLimits(threatMapLimits, (int)ThreatStance.Normal);
        }


        public void PostLoadContent()
        {
            sourceRectangle = The.Client.FlatSpriteSheet.GetSourceRectangle("whiteRectangle");

        }

       
        /// <summary>
        /// the maps are drawn as Overlay. Also additively rendered...? maybe not. will be all white when they are stacked.
        /// </summary>
        /// <param name="overlayVertices"></param>
        /// <param name="index"></param>
        public void SetupQuad(VertexOverlayGroundSpriteQuad[] overlayVertices, ref int index)
        {
           
           // SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot]; // transport];

            byte lowAlpha = GameData.Instance.GUIConstants.OverlayLowAlpha; // 100;
            byte hiAlpha = GameData.Instance.GUIConstants.OverlayHiAlpha; // 150;

            byte alpha = hiAlpha;
           /* byte alpha = 100;

            byte lowAlpha = 70;
            byte hiAlpha = 100;
            */

          //  currentColor *= Opacity;

            int tempIndex = index;

            TileLayer map = The.InGameUI.UIAllegiance.SharedKnowledge.PlaySiteKnowledge.ThreatMaps[The.InGameUI.UIAllegiance.RepresentativeEntityType][ThreatStance.Normal].Map;

            byte value;
            MapClient.OverlayLimit limit = null;
            Color baseColor;

            for (int sectorY = 0; sectorY < map.SectorsAcrossHeight; sectorY++)
            {
                for (int sectorX = 0; sectorX < map.SectorsAcrossWidth; sectorX++)
                {
                    TileSector sector = map.Sectors[sectorX][sectorY];
                    if (sector != null && The.MapUI.TileAreaIsOnScreen(sector.TileArea))
                    {
                        for (int y = 0; y < sector.TileArea.Height; y++)
                        {
                            for (int x = 0; x < sector.TileArea.Width; x++)
                            {
                                value = sector.GetValue((ushort)x, (ushort)y);

                                int limitIndex;
                                if (threatMapLimits != null && threatMapLimits.Count > 0)
                                {
                                    limit = Common.GetStairStepIndex((float)value, threatMapLimits, out limitIndex);
                                }

                                if (limit != null)
                                {
                                    baseColor = limit.Color;
                                }
                                else
                                {
                                    baseColor = Color.Blue;
                                }

                                Color color = baseColor * Common.Clamp((value / 50f), 0f, 1f);
                               // Color color = baseColor * Common.Clamp((value / 30f), 0f, 1f);

                              //  from = MapClient.TileEdgeToScreen(x + sector.TileArea.Left, y + sector.TileArea.Top);

                                if (DrawTile(x + sector.TileArea.Left, y + sector.TileArea.Top, tempIndex, color, overlayVertices))
                                {
                                    tempIndex++;
                                }
                                else
                                {
                                    index = tempIndex;
                                    return; // full
                                } 
                            }

                        }

                      //  Kensei.Dev.Shape.Box(TileEdgeToScreen(sector.TileArea.Location), TileEdgeToScreen(sector.TileArea.Right, sector.TileArea.Bottom), Color.White, false);
                     

                    }
                }
            }


            /*

            Point subtileStart = MapManager.TileToUpperLeftSubtile(new Point(The.Client.Renderer.TileStartX, The.Client.Renderer.TileStartY));
            Point subtileEnd = MapManager.TileToUpperLeftSubtile(new Point(The.Client.Renderer.TileEndX, The.Client.Renderer.TileEndY));
            subtileEnd.X += 2;
            subtileEnd.Y += 2;

            MapManager.SubtileValue value;
            byte cost;

            Color color = Color.Black;
            bool isInPad, isReserved;
            for (int x = subtileStart.X; x <= subtileEnd.X; x++)
            {               
                for (int y = subtileStart.Y; y <= subtileEnd.Y; y++)
                {
                    value = map.GetValue(x, y);
                    cost = MapManager.GetCost(value);

                    bool drawSubtile = false;
                    //color = null;

                    GetFlagsAndColor(alpha, value, cost, ref color, ref drawSubtile, out isInPad, out isReserved);

                    if (drawSubtile)
                    {
                        if (DrawSubtile(x, y, tempIndex, color, overlayVertices))
                        {
                            tempIndex++;
                        }
                        else
                        {
                            index = tempIndex;
                            return; // full
                        }                       
                    }

                    alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);


                }
                alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);
            }

            bool isWithinShapes;

           
            */


            index = tempIndex;
        }

       

        private bool DrawSubtile(int x, int y, int tempIndex, Color color, VertexOverlayGroundSpriteQuad[] overlayVertices)
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
