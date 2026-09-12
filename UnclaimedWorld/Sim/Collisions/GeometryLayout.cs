using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using SpriteSheetRuntime;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;


namespace UWGame.SimSide.Collisions
{
    
    public enum GeoPlaceMode
    {
        NewFeature,
        ShapeChanged,
        FeatureRemoved
    }

    /// <summary>
    /// this class has little content. most of the code is in Collidable.
    /// </summary>
    public class GeometryLayout : ISnapshot
    {
        public Entity parent;
        EntityID snapshotParent;

        List<TerrainTile> touchedTiles = new List<TerrainTile>();
        List<TerrainTileID> snapshotTouchedTiles;

        TerrainTileID? baseCenterTile;

        public GeometryLayout(Entity parent)
        {
            this.parent = parent;
        }



        public GeometryLayout()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public void Place(GeoPlaceMode mode)
        {
            switch (mode)
            {
                case GeoPlaceMode.NewFeature:
                    {
                        CalculateTouchedTiles();

                        AddTileReferencesToEntity();

                        //just stamp it down
                      
                        // unbuilt structures draw a reserved flag
                        RedrawTerrainCosts();
                       

                        break;
                    }
                case GeoPlaceMode.ShapeChanged:
                    {                        

                        RemoveTileReferencesToEntity();

                        List<Entity> neighbours;

                        //erase footprint first
                        //(why does this prevent trees from unblocking big rocks?)
                        EraseFootprintAndSignalNeighborsToStampAnew(out neighbours);

                        //then stamp it down
                        RedrawTerrainCosts();
                

                        AddTileReferencesToEntity();
                        break;
                    }
                case GeoPlaceMode.FeatureRemoved:
                    {
                        List<Entity> neighbours;
                        //just erase footprint and signal neighbors to stamp anew
                        EraseFootprintAndSignalNeighborsToStampAnew(out neighbours);

                        RemoveTileReferencesToEntity();
                        break;
                    }
            }
        }


        private void SetAccessPointsDirty(List<Entity> neighbours)
        {
            foreach (var entity in neighbours)
            {
                entity.SetAccessPointDirty();
            }
        }

        private void AddTileReferencesToEntity()
        {
            if (baseCenterTile.HasValue)
            {
                // NEW: just to be safe, we don't want anymore than one reference on the map...
                TerrainTile previousTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(baseCenterTile.Value);
                previousTile.RemoveCenterForGeoLayoutEntities(parent);
            }

            // mark the structure on the tilemap for rendering:
            TerrainTile tile = The.Map.GetTile(parent.MapPosition.Value);
            tile.AddCenterForGeoLayoutEntities(parent);
            baseCenterTile = tile.ID; // store the tile instead of the position. I have seen a save crash because a tile contained a reference to a destroyed entity...

            foreach (var touchedTile in touchedTiles)
            {
                touchedTile.AddGeoLayoutEntity(parent);
            }
        }

        private void RemoveTileReferencesToEntity()
        {
            if (baseCenterTile.HasValue)
            {
                TerrainTile previousTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(baseCenterTile.Value);
                previousTile.RemoveCenterForGeoLayoutEntities(parent);
            }          
           
            foreach (var tile in touchedTiles)
            {
                tile.RemoveGeoLayoutEntity(parent);
            }           
        }

        public void CreateBlockedMapOverBoundsAndPadding(bool flipHorizontally, out Tuple<bool, Vector2>[][] isBlockedData, out byte[][] subTileMap)
        {

            SubtileLayers terrainCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];

            Vector2 from;
            Vector2 to;
            float sts = MapManager.subTileSize;
            GetBoundsWithPadding(out from, out to);

            subTileMap = null;
        
            int x = (int)((to.X - from.X) / sts) + 1;
            int y = (int)((to.Y - from.Y) / sts) + 1;


            //Init the subtileMap and the blockedDataMap
            Common.InitJaggedArray(ref subTileMap, x, y);
            Tuple<bool, Vector2>[][] blockedData = null;
            Common.InitJaggedArray(ref blockedData, x, y);


            x = 0;
            y = 0;
            IterateSubtiles((s =>
                    {
                        //Calculate the X position and the Y position using the top left corrner and the current position of the subtile.
                        //TODO: also look in mapmanager for similar functionality
                        float diff = from.X - s.X;
                        float xArrayPos = Math.Abs(diff / sts);

                        diff = from.Y - s.Y;
                        float yArrayPos = Math.Abs(diff / sts);



                        Point subtilePos = MapManager.WorldPosToSubtile(s);
                        MapManager.SubtileValue value = terrainCosts.GetValue(subtilePos.X, subtilePos.Y);

                        //isBlockedData.Add(new Pair<bool, Vector2>(!Structure.CanBuildOnSubtile(value), s));
                        blockedData[(int)(xArrayPos)][(int)(yArrayPos)] = new Tuple<bool, Vector2>(!Structure.IsNotBlockedOrReserved(value), s);

                    }));

            isBlockedData = blockedData;

            //return subtileInformationList;
        }



        /// <summary>
        /// disabled...
        /// </summary>
        /// <param name="flipHorizontally"></param>
        /// <returns></returns>
        public byte[][] CreateBlockedSubtileMap(bool flipHorizontally)
        {
            byte[][] subTileMap = null;
            /*  Common.InitJaggedArray(ref subTileMap, 3, 3);
              //subTileMap = new byte[3, 3];           

              Point subtile = MapManager.WorldPosToRelativeSubtile(parent.Location);
              subTileMap[subtile.X][subtile.Y] = 255;
              */
            return subTileMap;
        }



        private void EraseFootprintAndSignalNeighborsToStampAnew(out List<Entity> neighbours)
        {

            //compute the size of the erase footprint, should be bigger than actual footprint
            float sts = MapManager.subTileSize;

            Vector2 padVec = new Vector2(Math.Max(sts, parent.CurrentSimState.GeometryLayoutType.Pad)); 

            //iterate all the tiles inside the shape bounds rectangle 
            Vector2 from = parent.Collidable.Bounds.BoundsUpperLeft - padVec;
            Vector2 to = parent.Collidable.Bounds.BoundsLowerRight + padVec;

            //clamps
            if (from.X < 0)
                from.X = 0;
            if (from.Y < 0)
                from.Y = 0;

            //snap from to nearest subtile
            //from = MapManager.SubTileToWorldPos3(MapManager.WorldPosToSubtile(from)).ToVector2();

            from.X -= from.X % sts;
            from.Y -= from.Y % sts;


            if (to.X >= The.Map.MapWorldWidth)
                to.X = The.Map.MapWorldWidth - 1;
            if (to.Y >= The.Map.MapWorldHeight)
                to.Y = The.Map.MapWorldHeight - 1;

            Point subtile;
            Vector3 location;
            //erase all the subtiles in the footprint
            for (float x = from.X; x < to.X; x += sts)
            {
                for (float y = from.Y; y < to.Y; y += sts)
                {
                    location = new Vector3(x, y, 0f);
                    subtile = MapManager.WorldPosToSubtile(location);

                    The.Map.SetSubtileCostToSurfaceType(subtile);

                    // remove flags too:
                    The.Map.ClearSubtileTerrainValueFlag(subtile, MapManager.SubtileValue.Reserved | MapManager.SubtileValue.Pad);

                    /*
                    Point subtilePos = MapManager.WorldPosToSubtile(new Vector2(x, y));

                    The.Map.SetSubtileCost(subtilePos, PlainsType.Instance.Cost(SurfaceType.TransportType.Foot,); // hm. what about water? other surface types?
                */
                }
            }

            //find all the neighbors whose footprints intersect with erase footprint
            /*List<Entity>*/
            neighbours = new List<Entity>();
            foreach (TerrainTile tile in touchedTiles)
            {
                if (tile.GeoLayoutEntitiesOnTile != null)
                {
                    for (int i = tile.GeoLayoutEntitiesOnTile.Count - 1; i >= 0; i--)
                    {
                        EntityID entityIDOnTile = tile.GeoLayoutEntitiesOnTile[i];
                        Entity entityOnTile = Entity.FindByID(entityIDOnTile);
                        if (entityOnTile != null)
                        {
                            if (entityOnTile != parent && entityOnTile.GeometryLayout != null)
                            {
                                neighbours.Add(entityOnTile);
                            }
                        }
                        else
                        {
                            tile.GeoLayoutEntitiesOnTile.RemoveAt(i);
                        }
                    }
                }
            }

            neighbours = neighbours.Distinct().ToList();

            //iterate all neighbors and have them stamp anew
            foreach (var neighbour in neighbours)
            {
                neighbour.GeometryLayout.Place(GeoPlaceMode.NewFeature);

                //NEW: tell them to compute a new access point too:
                neighbour.SetAccessPointDirty();
            }
            
        }


        public void Destroy()
        {
            Structure structure = parent.Structure;
            if (structure != null && structure.State == StructureStates.BeingPlaced) // !structure.ConstructionHasStarted())
            {
                return; // structures being placed
            }

            if (parent.Collidable == null)
                return; // for editor entities being placed


            ClearTerrainCosts(false);

            // delete from TiledEntityOnTile etc.

            RemoveTileReferencesToEntity();


        }

        public void ClearTerrainCosts(bool redraw)
        {
            // clear terrain costs & redraw!          
            // //////////////////////////////////////////////////////
            //The most efficient way to clear the blocked tiles is to blank every subtile in our bounding rectangle
            //and then signal every entity that collides with the bounds to redraw themselves
            // //////////////////////////////////////////////////////

            List<Entity> neighbours;
            EraseFootprintAndSignalNeighborsToStampAnew(out neighbours);

            if (redraw)
            {
                RedrawTerrainCosts();
            }
        }


        /// <summary>
        /// get the tiles that we touch and cache them
        /// </summary>
        private void CalculateTouchedTiles()
        {
            try
            {

                float tileSize = MapManager.tileSize;

                Vector2 padVec = new Vector2(Math.Max(tileSize, parent.CurrentSimState.GeometryLayoutType.Pad)); //EntityType.GeometryLayoutType.Pad));

                //iterate all the tiles inside the shape bounds rectangle 
                Vector2 from = parent.Collidable.Bounds.BoundsUpperLeft - padVec;
                Vector2 to = parent.Collidable.Bounds.BoundsLowerRight + padVec;

                //clamps
                if (from.X < 0)
                    from.X = 0;
                if (from.Y < 0)
                    from.Y = 0;

                //snap from to nearest tile          
                from.X -= from.X % tileSize;
                from.Y -= from.Y % tileSize;

                from.X += 1f; // but make sure we are not on the edge between two tiles...
                from.Y += 1f;


                if (to.X >= The.Map.MapWorldWidth)
                    to.X = The.Map.MapWorldWidth - 1;
                if (to.Y >= The.Map.MapWorldHeight)
                    to.Y = The.Map.MapWorldHeight - 1;

                touchedTiles.Clear();

                TerrainTile tile;
                for (float x = from.X; x < to.X; x += tileSize)
                {
                    for (float y = from.Y; y < to.Y; y += tileSize)
                    {
                        Point tilePos = MapManager.WorldPosToTile(new Vector2(x, y));

                        tile = The.Map.GetTile(tilePos);
                        touchedTiles.Add(tile);
                    }
                }
            }
            catch (Exception e)
            {
                string exceptionString = e.Message;
                if (parent == null)
                {
                    exceptionString += "PARENT NULL\n";
                }
                else
                {
                    exceptionString += Entity.GetExceptionInformation(parent);
                }

                if (parent.Collidable == null)
                {
                    exceptionString += "Collidable NULL \n";
                }
                else if (parent.Collidable.Bounds == null)
                {
                    exceptionString += "Bounds NULL\n";
                }

                if (touchedTiles == null)
                {
                    exceptionString += "touchedTiles NULL\n";
                }
                throw new Exception(exceptionString);

            }

        }

       // public delegate void IterateMethod(Vector2 subtile);


        public void GetBoundsWithPadding(out Vector2 from, out Vector2 to)
        {
             GeometryLayoutType geoType = parent.CurrentSimState.GeometryLayoutType; 

             parent.Collidable.GetBoundsWithPadding(geoType.Pad, out from, out to);

           /*
            Vector2 padVec = new Vector2(geoType.Pad); 

            //iterate all the tiles inside the shape bounds rectangle 
            from = parent.Collidable.Bounds.BoundsUpperLeft - padVec;
            to = parent.Collidable.Bounds.BoundsLowerRight + padVec;

            // clamp first:
            from = The.Map.ClampWorldPosition(from);
            to = The.Map.ClampWorldPosition(to);

            // convert to center of subtiles instead of corners:
            from = MapManager.SubTileToWorldPos(MapManager.WorldPosToSubtile(from));
            to = MapManager.SubTileToWorldPos(MapManager.WorldPosToSubtile(to));*/
        }

        /// <summary>
        /// iterates over subtiles covered by shapes or in the pad area.
        /// </summary>
        /// <param name="iterateMethod"></param>
        public void IterateSubtiles(Action<Vector2> iterateMethod)  //IterateMethod iterateMethod)
        {            
            Vector2 from;
            Vector2 to;

            GetBoundsWithPadding(out from, out to);

            MapManager.IterateSubtiles(from, to, iterateMethod);
        }

       


        private void RedrawTerrainCosts()
        {
            GeometryLayoutType geoType = parent.CurrentSimState.GeometryLayoutType; // EntityType.GeometryLayoutType;

            float padRadiusSquared = GetPadRadiusSquared();

            bool isReservedForStructure = false;

            if (parent.EntityType.StructureType != null && !parent.Structure.ConstructionHasStarted())
            {
                isReservedForStructure = true;
            }

            IterateSubtiles(worldPos =>
                {
                    bool isInPad = IsWithinPad(geoType, padRadiusSquared, worldPos);
                    bool isWithinShapes = IsWithinShapes(worldPos, false);

                    StampSubtile(worldPos, 0, isWithinShapes, isInPad, isReservedForStructure);
                   // StampSubtile(s, 0, isInPad, false, isReservedForStructure);
                });

           
            //then after the circles are all drawn, we tunnel-out the doors
            if (!isReservedForStructure
                && parent.Contains != null)
            {
                IExit exit = parent.Contains as IExit;
                if (exit != null)
                {
                    for (ExitDoor t = ExitDoor.Door1; t < ExitDoor.Max; t++)
                    {
                        Vector3 door = Vector3.Zero;
                        ExitDoor doorThatWasUsed;
                        if (exit.GetDoorPosition(ref door, false, out doorThatWasUsed, t))
                        {
                            Vector3 rally = exit.GetRallyPoint(t);

                            //now, paint a line of costly tiles between rally and door

                            Vector3 doorToRally = rally - door;

                            float lineLength = doorToRally.Length();
                            float dotGap = 0.1f;
                            for (float incr = 0; incr < 1; incr += dotGap)
                            {
                                Vector3 dot = door + doorToRally * incr;

                                bool isWithinShapes = IsWithinShapes(dot.ToVector2(), true);
                                bool isWithinPad = true;

                                StampSubtile(dot.ToVector2(), 7, isWithinShapes, isWithinPad, false);
                            }
                        }
                    }
                }
            }

            CalculateTouchedTiles();

        }

    

        /// <summary>
        /// we cannot clear trees or terrain yet... so permit those on building sites
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool AllowEntityOnShapes(Entity entity)
        {
            return entity == parent || entity.EntityType.TerrainType != null || entity.EntityType.TreeType != null;
        }


        public bool IsWithinPad(GeometryLayoutType geoType, float padRadiusSquared, Vector2 worldPos)
        {
            bool isInPad = (geoType.Pad > 0f);

            if (geoType.PadShape == CollidePrim.Circle)
            {
                float squaredDistanceFromCenter = (parent.PlaySiteLocation.ToVector2() - worldPos).LengthSquared();
                isInPad = squaredDistanceFromCenter < padRadiusSquared;
            }

            return isInPad;
        }

        /// <summary>
        /// the pad is a simple rectangular area derived from the Bounds - a rectangle around all geo shapes - and an added margin.
        /// </summary>
        /// <returns></returns>
        public float GetPadRadiusSquared()
        {
            GeometryLayoutType geoType = parent.CurrentSimState.GeometryLayoutType; // EntityType.GeometryLayoutType;

            float padRadius = parent.Collidable.Bounds.Radius + geoType.Pad;
            float padRadiusSquared = padRadius * padRadius;

            return padRadiusSquared;
        }


      
        private void StampSubtile(Vector2 worldPos, byte cost, bool isWithinShapes, bool isInPad, bool isUnbuiltStructure)
        {
          //  Point subtilePos = MapManager.WorldPosToSubtile(worldPos);

          /*  if (subtilePos.X == 47 && subtilePos.Y == 20)
            {

            }*/

            if (isWithinShapes)
            {
                Point subtilePos = MapManager.WorldPosToSubtile(worldPos);

                if (!isUnbuiltStructure)
                {
                    The.Map.SetSubtileTerrainCost(subtilePos, cost); // 0 = blocked!
                }
                else
                {
                    The.Map.SetSubtileTerrainValueFlag(subtilePos, MapManager.SubtileValue.Reserved); // reserved for structure                   
                }
            }
            else if (isInPad)
            {
                Point subtilePos = MapManager.WorldPosToSubtile(worldPos);

                if (!isUnbuiltStructure)
                {
                    // mark as Pad
                    The.Map.SetSubtileTerrainValueFlag(subtilePos, MapManager.SubtileValue.Pad);
                }
                else
                {
                    The.Map.SetSubtileTerrainValueFlag(subtilePos, MapManager.SubtileValue.Reserved);  // reserved for Pad    
                }

            }
        }

       /* private void StampSubtile(Vector2 worldPos, byte cost, bool isInPad, bool testIfAnyPartOfSubtileIsInBounds, bool isUnbuiltStructure)
        {

            bool isWithinBounds = GetIsWithinBounds(ref worldPos, testIfAnyPartOfSubtileIsInBounds);

            if (isWithinBounds)
            {
                Point subtilePos = MapManager.WorldPosToSubtile(worldPos);

                if (!isUnbuiltStructure)
                {
                    The.Map.SetSubtileTerrainCost(subtilePos, cost); // 0 = blocked!
                }
                else
                {
                    The.Map.SetSubtileTerrainValueFlag(subtilePos, MapManager.SubtileValue.Reserved); // reserved for structure                   
                }
            }
            else if (isInPad)
            {
                Point subtilePos = MapManager.WorldPosToSubtile(worldPos);

                if (!isUnbuiltStructure)
                {
                    // mark as Pad
                    The.Map.SetSubtileTerrainValueFlag(subtilePos, MapManager.SubtileValue.Pad);
                }
                else
                {
                    The.Map.SetSubtileTerrainValueFlag(subtilePos, MapManager.SubtileValue.Reserved);  // reserved for Pad    
                }               

            }
        }*/

        public bool IsWithinShapes(Vector2 worldPos, bool testIfAnyPartOfSubtileIsInBounds)
        {
            return parent.Collidable.IsWithinShapes(worldPos, testIfAnyPartOfSubtileIsInBounds);

        }

        #region ISnapshot
        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            snapshotParent = (EntityID)sn.SnapshotID<Entity, EntityID>(parent);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotTouchedTiles = touchedTiles.Select(t => t.ID).ToList();
            }
            snapshotTouchedTiles = sn.DoList(snapshotTouchedTiles);
            baseCenterTile = sn.DoEnumNullable(baseCenterTile);

            sn.Ignore(touchedTiles);
       

            return this;
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


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //lookups and other fix-ups
            parent = Entity.FindByID(snapshotParent);
            touchedTiles = snapshotTouchedTiles.Select(t => LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(t)).ToList();

        }

        #endregion


    }
}
