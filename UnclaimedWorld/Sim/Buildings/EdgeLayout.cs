using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Trees;
using UWGame;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Buildings
{
    public class DirectionalLayout : ISnapshot
    {        
        public Common.Direction EdgePosition;
        public int SubtileIndex;

        /// <summary>
        /// cached values... replace with properties which are recomputed if the memory is excessive?
        /// </summary>
        public Point EdgeSubtile;
        public Point CenterSubtile;

        private Entity parent;

        //byte[,] subTileMap;

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.CenterSubtile = sn.DoPoint(CenterSubtile);
            this.EdgePosition = sn.DoEnum(EdgePosition);
            this.EdgeSubtile = sn.DoPoint(EdgeSubtile);
            this.SubtileIndex = sn.DoInt32(SubtileIndex);

            //TODO Entity Parent should summarily assigned by Needs.DoSnapshot

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //lookups and other fix-ups
        }

        public DirectionalLayout()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }


        public DirectionalLayout(Entity parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// NEW! Also blocks terrain depending on type, state etc.!
        /// </summary>
        /// <param name="edgePos"></param>
        public void Place(Common.Direction edgePos)
        {
            EdgePosition = edgePos;
            Point localCoords = MapManager.DirectionToRelativeSubtile(edgePos);
            SubtileIndex = MapManager.GetLocalSubtileIndex(localCoords.X, localCoords.Y);

            Point subTileStart = MapManager.TileEdgeToSubtile(parent.MapPosition.Value);

            EdgeSubtile = localCoords;
            EdgeSubtile.X += subTileStart.X;
            EdgeSubtile.Y += subTileStart.Y;
            CenterSubtile = new Point(subTileStart.X + 1, subTileStart.Y + 1);

            if (parent.EntityType.StructureType != null && parent.EntityType.StructureType.IsRoad)
            {
                The.Map.TileMap[parent.MapPosition.Value.X][parent.MapPosition.Value.Y].AddRoad(parent, EdgePosition);
            }
            else
            {
                The.Map.TileMap[parent.MapPosition.Value.X][parent.MapPosition.Value.Y].AddEdgeStructure(parent); // why not an array...?
            }

            RedrawTerrainCosts();
        }

        public byte[][] CreateBlockedSubtileMap()
        {
            byte[][] subTileMap = null;
            Common.InitJaggedArray(ref subTileMap, 3, 3);
            //subTileMap = new byte[3, 3];
            // edge structure... untested!
                     
            // STERAIN:
            subTileMap[1][1] = 255;
            Point corner = MapManager.DirectionToRelativeSubtile(EdgePosition);
            subTileMap[corner.X][corner.Y] = 255;


            return subTileMap;
        }

        public bool[][] CreateIsBlockedMap()
        {
            bool[][] subTileMap = null;
            // edge structure... untested!
            //Point coords = MapManager.GetSubtileCoordsFromDirection(EdgePosition);
            Common.InitJaggedArray(ref subTileMap, 3, 3);
            //subTileMap = new bool[3, 3];

            /*if (parent.EntityType.EdgeLayoutType.IsObstacle)
            {*/

            // STERAIN:
            subTileMap[1][1] = true;
                Point corner = MapManager.DirectionToRelativeSubtile(EdgePosition);
                subTileMap[corner.X][corner.Y] = true;

            //}

            return subTileMap;

            /*
            bool[,] subTileMap;
            // edge structure... untested!
            Point coords = MapManager.GetSubtileCoordsFromDirection(EdgePosition);
            subTileMap = new bool[3, 3];
            subTileMap[coords.X, coords.Y] = true;

            return subTileMap;*/
        }


        public void Destroy()
        {
            The.Map.GetTile(parent.MapPosition.Value).RemoveEdgeStructure(parent);

            ClearTerrainCosts(true);
        }

       
        /// <summary>
        /// Optimization: use redraw = false when we are removing many entities, to spare a redraw after each of the them.
        /// </summary>
        /// <param name="redraw"></param>
        public void ClearTerrainCosts(bool redraw)
        {         
            // STERAIN: clear the 2 subtiles touched by this entity:        

            The.Map.SetSubtileCostToSurfaceType(EdgeSubtile);
            The.Map.SetSubtileCostToSurfaceType(CenterSubtile);

            if (redraw)
            {   // this redraws us as well... if we are not removed before.

                // TODO: re-implement using the code in GeoLayout
               // The.Map.TileMap[parent.MapPosition.X][parent.MapPosition.Y].RedrawTerrainCosts();
               
            }
        }


        public void RedrawTerrainCosts()
        {     
            // STERAIN:
            /*
            Tree treeComponent;
            if (parent.Find(out treeComponent))
            {
                treeComponent.RedrawTerrainCosts();
            }
            else
            {*/
                Structure structure = parent.Structure;
                if (structure != null)
                {
                    // don't do roads here...
                    if (parent.TerrainPath == null)
                    {

                        // has work begun yet?
                        if ((structure.ConstructionHasStarted())
                            && parent.EntityType.DirectionalLayoutType.IsObstacle)
                        {
                            //UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, EdgePosition, 0);
                            // STERAIN: New - use this for fences...
                            foreach (SurfaceType.TransportType transport in MapManager.MapTransportTypeArray)
                            {
                                SetCost(transport, 0);
                            }
                        }
                    }

                }
                else if (parent.EntityType.DirectionalLayoutType.IsObstacle)
                {
                    // block it...                  
                    //UWGame.SimSide.Instance.Map.SetEdgeCost(parent.MapPosition, EdgePosition, 0);

                    // STERAIN: New - what do we use this for??? Natural fences???
                    foreach (SurfaceType.TransportType transport in MapManager.MapTransportTypeArray)
                    {
                        SetCost(transport, 0);
                    }
                }
          //  }

        }

        /// <summary>
        /// STERAIN: set the cost on the 2 subtiles that this entity touches!
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="cost"></param>
        private void SetCost(SurfaceType.TransportType transport, byte cost)
        {
            The.Map.SetSubtileCost(EdgeSubtile, transport, cost);

            // set the center subtile too:
            The.Map.SetSubtileCost(CenterSubtile, transport, cost);
        }

        /// <summary>
        /// roads only!
        /// </summary>
        public void RedrawRoadCost() //PathType pathType)
        {
            PathType pathType = parent.EntityType.TerrainType.PathType;

            //int transport;
           // Point subTileStart = MapManager.TileEdgeToSubtile(parent.MapPosition);

            // STERAIN: set roads/paths terrain cost!
            // now on subtiles!
          /*  Point edgeSubTile = MapManager.GetSubtileCoordsFromDirection(dir);
            edgeSubTile.X += subTileStart.X;
            edgeSubTile.Y += subTileStart.Y;

            Point centerSubtile = new Point(subTileStart.X + 1, subTileStart.Y);
            */

            byte cost;
            foreach (SurfaceType.TransportType transport in MapManager.MapTransportTypeArray)
            {
                cost = pathType.TransportCosts[(int)transport];

                SetCost(transport, cost);

            }

            /* OLD:
            for (int t = 0; t < MapManager.TransportIndices.Length; t++)
            {
                transport = MapManager.TransportIndices[t];

                UWGame.SimSide.Instance.Map.SetEdgeCost(pos, dir, transport, pathType.TransportCosts[transport]);
            }*/
        }
    }
}
