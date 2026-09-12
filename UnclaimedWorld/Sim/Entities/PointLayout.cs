using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
    public class PointLayout : ISnapshot
    {
        Entity parent;
        EntityID snapshotParent;

        public PointLayout(Entity parent)
        {
            this.parent = parent;
        }

     

        public PointLayout()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        
        public void RedrawTerrainCosts()
        {  

            Tree treeComponent;
            if (parent.Find(out treeComponent))
            {
                treeComponent.RedrawTerrainCosts();
            }
            else
            {
                Structure structure = parent.Structure;
                if (structure != null)
                {
                    /*  RedrawTerrainCosts(structure.TopLeftMapPosition, tileToRedraw,
                          parent.EntityType.TileLayoutType.GetBlockedEdges(parent.FlipHorizontally)[structure.GetUtilityMap()]);*/

                    // has work begun yet?
                    if ((structure.ConstructionHasStarted()))
                    {
                        The.Map.SetSubtileTerrainCost(parent.Location.Value, 0);
                    }

                }
                else
                {
                    /* else if (parent.EntityType.EdgeLayoutType.IsObstacle)
                     {*/
                    // block it...     
                    if (parent.EntityType.PointLayoutType.IsBlocking)
                    {

                        The.Map.SetSubtileTerrainCost(parent.Location.Value, 0);


                        if (parent.EntityType.RockType != null)//HACK HACK XXX remove this!!!
                            The.Map.SetSubtileTerrainCost(parent.Location.Value, 2);//HACK XXX remove this!!!!


                    }

                    // }
                }
            }

        }


        public bool[][] CreateIsBlockedMap()
        {
            bool[][] subTileMap = null;
            Common.InitJaggedArray(ref subTileMap, 3, 3);

            Point subtile = MapManager.WorldPosToRelativeSubtile(parent.PlaySiteLocation);
            subTileMap[subtile.X][subtile.Y] = true;

            return subTileMap;
        }

        public byte[][] CreateBlockedSubtileMap()
        {
            byte[][] subTileMap = null;
            Common.InitJaggedArray(ref subTileMap, 3, 3);
            //subTileMap = new byte[3, 3];           

            Point subtile = MapManager.WorldPosToRelativeSubtile(parent.PlaySiteLocation);
            subTileMap[subtile.X][subtile.Y] = 255;

            return subTileMap;
        }


        public void Place()
        {
            The.Map.GetTile(parent.MapPosition.Value).AddEntity(parent);

            RedrawTerrainCosts();

        }


        /// <summary>
        /// Optimization: use redraw = false when we are removing many entities, to spare a redraw after each of the them.
        /// </summary>
        /// <param name="redraw"></param>
        public void ClearTerrainCosts(bool redraw)
        {
            // clear the subtile touched by this entity:        
            The.Map.SetSubtileTerrainCostToSurfaceType(parent.PlaySiteLocation);

            /*
            if (redraw)
            {   // this redraws us as well... if we are not removed before.
                The.Map.TileMap[parent.MapPosition.X][parent.MapPosition.Y].RedrawTerrainCosts();
            }*/

        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            snapshotParent = (EntityID)sn.SnapshotID<Entity, EntityID>(parent);


            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            parent = Entity.FindByID(snapshotParent);
        }

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


        #endregion

    }
}
