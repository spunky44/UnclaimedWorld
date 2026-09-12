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
    public class TerrainBlockingRender : ÏnfluenceRender
    {
       // float Opacity = 0.6f;


        public TerrainBlockingRender()
        {
            quad = new OverlayGroundSpriteQuad();

          
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
           /* if (isDirty)
            {
                SetDimensions();
                isDirty = false;
            }*/


           // int xScreen, yScreen;
           
            // MapManager map = The.Map;

            SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot]; // transport];


            //  Color selectionCurrentColor = Color; 
           // Color currentColor;

            

            byte lowAlpha = GameData.Instance.GUIConstants.OverlayLowAlpha; // 100;
            byte hiAlpha = GameData.Instance.GUIConstants.OverlayHiAlpha; // 150;

            byte alpha = hiAlpha;
           /* byte alpha = 100;

            byte lowAlpha = 70;
            byte hiAlpha = 100;
            */

          //  currentColor *= Opacity;

            int tempIndex = index;

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
                        /*
                        Vector2 screenCoords = The.MapUI.SubtileEdgeToScreen(x, y);
                        
                        destination = new System.Drawing.RectangleF(screenCoords.X, screenCoords.Y, (float)MapManager.subTileSize, (float)MapManager.subTileSize);
                        
                        quad.SetupQuadVertices(destination.Left, destination.Top, destination.Right, destination.Bottom,
                            sourceRectangle, The.Client.FlatSpriteSheet.Texture,
                            true,
                            color.ToVector4());

                        if (quad.CopyQuadToVertexBuffer(overlayVertices, tempIndex))
                        {
                            tempIndex++;
                        }
                        else
                        {
                            // buffer is full..?!?
                            index = tempIndex;
                            return;
                        }*/
                    }

                    alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);


                }
                alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);
            }

            bool isWithinShapes;

            if (The.InGameUI.EntitiesBeingPlaced != null)
            {
                
                foreach (var item in The.InGameUI.EntitiesBeingPlaced)
                {
                    if (item.Entity.GeometryLayout != null)
                    {               

                        bool placementIsValid = true;

                        if (item.Entity.Structure != null)
                        {
                            placementIsValid = item.Entity.Structure.IsPlacementValid(item.Position.ToVector3());
                        }

                        GeometryLayoutType geoType = item.Entity.CurrentSimState.GeometryLayoutType; // EntityType.GeometryLayoutType;
                        float padRadiusSquared = item.Entity.GeometryLayout.GetPadRadiusSquared();

                        item.Entity.GeometryLayout.IterateSubtiles(worldPos =>
                            {
                                if (The.Map.WorldLocationIsOnMap(worldPos))
                                {
                                    bool drawSubtile = false;

                                    Point subtilePos = MapManager.WorldPosToSubtile(worldPos);
                                    MapManager.SubtileValue placedValue = map.GetValue(subtilePos);
                                    cost = MapManager.GetCost(placedValue);

                                    isInPad = item.Entity.GeometryLayout.IsWithinPad(geoType, padRadiusSquared, worldPos);
                                    isWithinShapes = item.Entity.GeometryLayout.IsWithinShapes(worldPos, false);


                                    if (!Structure.CanBuildOnSubtile(isWithinShapes, isInPad, placedValue))
                                    {
                                        drawSubtile = true;
                                        color = GameData.Instance.GUIConstants.StructurePreventingPlacementColor; // blockedColor; 
                                                                           
                                    }
                                    else
                                    {                                       
                                        if (isWithinShapes)
                                        {
                                            drawSubtile = true;

                                            if (placementIsValid)
                                            {
                                                color = GameData.Instance.GUIConstants.StructureBeingPlacedColor; // Color.Blue;                                                
                                            }
                                            else
                                            {
                                                color = GameData.Instance.GUIConstants.StructureBeingPlacedOtherPointPreventingPlacementColor; 
                                            }
                                        }
                                        else if (isInPad)
                                        {
                                            drawSubtile = true;
                                            if (placementIsValid)
                                            {
                                                color = GameData.Instance.GUIConstants.PadColor; //Color.Yellow;
                                            }
                                            else
                                            {
                                                color = GameData.Instance.GUIConstants.StructureBeingPlacedOtherPointPreventingPlacementPadColor;
                                            }
                                        }
                                    }

                                    if (drawSubtile)
                                    {
                                        if (DrawSubtile(subtilePos.X, subtilePos.Y, tempIndex, color, overlayVertices))
                                        {
                                            tempIndex++;
                                        }
                                    }
                                   /* else
                                    {
                                        index = tempIndex;
                                        return; // full
                                    }*/
                                }
                            });
                    }
                   
                }

            }

            
            index = tempIndex;



           /* parent.IterateArea(tile =>
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
            });*/

        }

        private static void GetFlagsAndColor(byte alpha, MapManager.SubtileValue value, byte cost, ref Color color, ref bool drawSubtile, out bool isInPad, out bool isReserved)
        {
            isInPad = false;
            isReserved = false;

            switch (cost)
            {
                case 0://blocked
                    {
                        color = GameData.Instance.GUIConstants.BlockedColor; // blockedColor; // Color.Maroon;
                        color.A = alpha;
                        drawSubtile = true;
                        break;
                    }
                case 3://foot-normal - draw only flags
                    {
                        isInPad = MapManager.TestForFlag(value, MapManager.SubtileValue.Pad);
                        isReserved = MapManager.TestForFlag(value, MapManager.SubtileValue.Reserved);

                        if (isInPad && isReserved)
                        {
                            color = Color.LightYellow;
                            color.A = alpha;

                            drawSubtile = true;
                        }
                        else if (isInPad)
                        {
                            color = GameData.Instance.GUIConstants.PadColor; //Color.Yellow;
                            color.A = alpha;
                            drawSubtile = true;
                        }
                        else
                        {
                            if (isReserved)
                            {
                                color = Color.White;
                                color.A = alpha;
                                drawSubtile = true;
                            }
                            else
                            {
                                //continue;
                            }
                        }

                        break;
                    }
                default:
                    {
                        /*   color = Color.Black;
                           color.A = alpha;*/
                        break;
                    }

            }
        }


        

        /*
        private void DrawTerrainCosts(SurfaceType.TransportType transport)
        {
            int subTileSize = MapManager.subTileSize;

            byte alpha = 100;

            byte lowAlpha = 70;
            byte hiAlpha = 100;

            Vector2 from, to;
            Color color = Color.Black;
            color.A = alpha;

            byte cost;

            int maxY = 3 * (mapWindowTileY + noOfTilesToDisplayVertically);
            int maxX = 3 * (mapWindowTileX + noOfTilesToDisplayHorizontally);

            SubtileLayers map = The.Map.TerrainCosts[transport];

            int startY = mapWindowTileY * 3;
            int startX = mapWindowTileX * 3;

            if (startY % 2 == 0)
            {
                alpha = hiAlpha;
            }
            else
            {
                alpha = lowAlpha;
            }

            MapManager.SubtileValue value;
            bool isInPad, isReserved;
            for (int y = startY; y < maxY; y++)
            {
                for (int x = startX; x < maxX; x++)
                {
                    from = SubtileEdgeToScreen(x, y);

                    value = map.GetValue(x, y);
                    cost = MapManager.GetCost(value);


                    switch (cost)
                    {
                        case 0://blocked
                            {
                                color = Color.Maroon;
                                color.A = alpha;
                                break;
                            }
                        case 1://car-paved
                            {
                                color = Color.LightBlue;
                                color.A = alpha;
                                break;
                            }
                        case 2://car-gravel
                            {
                                color = Color.Purple;
                                color.A = alpha;
                                break;
                            }
                        case 3://foot-normal - draw only flags
                            {
                                isInPad = MapManager.TestForFlag(value, MapManager.SubtileValue.Pad);
                                isReserved = MapManager.TestForFlag(value, MapManager.SubtileValue.Reserved);

                                if (isInPad && isReserved)
                                {
                                    color = Color.LightYellow;
                                    color.A = alpha;
                                }
                                else if (isInPad)
                                {
                                    color = Color.Yellow;
                                    color.A = alpha;
                                }
                                else
                                {

                                    if (isReserved)
                                    {
                                        color = Color.White;
                                        color.A = alpha;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                break;
                            }
                        case 4://car-wheel path
                            {
                                color = Color.Magenta;
                                color.A = 200;
                                break;
                            }
                        case 5://Foot-Obstacle
                        case 6://Offroad-Obstacle
                        case 8://car-Obstacle
                            {
                                color = Color.DarkBlue;
                                color.A = 11;
                                break;
                            }
                        default:
                            {
                                color = Color.Black;
                                color.A = alpha;
                                break;
                            }

                    }


                    //Kensei.Dev.Shape.Line(from, to, color);
                    Kensei.Dev.Shape.Box(from, new Vector2(from.X + subTileSize, from.Y + subTileSize), color, true);

                    alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);

                }
                alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);
            }

        }*/
    }
}
