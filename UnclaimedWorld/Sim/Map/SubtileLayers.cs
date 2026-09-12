using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps.Regions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Maps
{
    /// <summary>
    /// needed for AStar search
    /// </summary>
    public enum SubtileLayersID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// when passed to the pathfinder, it will navigate the layers and sectors within
    /// 
    /// There are no null values in this data!
    /// </summary>
    public class SubtileLayers : ISnapshot, ILookUp<SubtileLayers, SubtileLayersID>
    {
        /// <summary>
        /// higher layers override lower ones.
        /// We assert that the lowest layer is complete without holes/null values!
        /// </summary>
        public SubtileLayer[] Layers;

        SubtileLayerID[] snapshotLayers;

        /// <summary>
        /// the associated high-level representation of this map
        /// moved from MovementMap class
        /// </summary>
        public RegionMap RegionMap;

        /// <summary>
        /// only used during Snapshot - IDs are replaced during fixup
        /// </summary>
        private CyclableID snapshotRegionMap;

        public SubtileLayers()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public SubtileLayers(MovementMap moveMap, string name, SubtileLayer bottomLayer, SubtileLayer secondLayer, TerrainRegionMap terrainRegionMap)
        {           
            Layers = new SubtileLayer[2]
            {
                bottomLayer, secondLayer 
            };

            RegionMap = new DependentRegionMap(moveMap, this, terrainRegionMap); 
            RegionMap.IDName = name; //this.IDName + kvp.Key.ToString();

          

            AddToLookup();
        }

        public SubtileLayers(string name, SubtileLayer bottomLayer)
        {

            Layers = new SubtileLayer[1]
                {
                    bottomLayer 
                };

            RegionMap = new TerrainRegionMap(this); 
            RegionMap.IDName = name;


            AddToLookup();
        }

        #region Region map

      


        public ushort GetRegion(Point subtile)
        {
            return GetRegion(subtile.X, subtile.Y);
        }

        public ushort GetRegion(int subtileX, int subtileY, bool inProgress = false)
        {
            ushort relativeX, relativeY;
            ushort sectorX, sectorY;

            Layers[0].GetSectorAndRelativeCoords(subtileX, subtileY, out sectorX, out sectorY, out relativeX, out relativeY);

            return GetRegion(sectorX, sectorY, relativeX, relativeY, inProgress);

        }


        public ushort GetRegion(int sectorX, int sectorY, int relativeX, int relativeY, bool inProgress = false)
        {
            SubtileSector sector;

            if (inProgress == false)
            {
                sector = GetSector(sectorX, sectorY);
            }
            else
            {
                sector = GetUnfinishedSector(new Point(sectorX, sectorY));
            }

            if (sector != null)
            {
                if (inProgress == false)
                {
                    return sector.GetRegion(relativeX, relativeY);
                }
                else
                {
                    return sector.GetRegionInProgress(relativeX, relativeY);
                }
            }

            System.Diagnostics.Debug.Assert(false, "No holes allowed");

            return 0;//dummy
        }

        #endregion

        public SubtileSector GetSectorFromSubtile(Point subtile, out int layerIndex)
        {
            ushort relativeX, relativeY;
            ushort sectorX, sectorY;

            Layers[0].GetSectorAndRelativeCoords(subtile.X, subtile.Y, out sectorX, out sectorY, out relativeX, out relativeY);

            return GetSector(sectorX, sectorY, out layerIndex);
        }

        public SubtileSector GetUnfinishedSectorFromSubtile(Point subtile, out int layerIndex)
        {
            ushort relativeX, relativeY;
            ushort sectorX, sectorY;

            Layers[0].GetSectorAndRelativeCoords(subtile.X, subtile.Y, out sectorX, out sectorY, out relativeX, out relativeY);

            return GetSector(sectorX, sectorY, out layerIndex, false);
        }

        public SubtileSector GetSectorFromSubtile(Point subtile)
        {
            int layerIndex;

            return GetSectorFromSubtile(subtile, out layerIndex);
        }

       /* public SubtileSector GetSector(Point sectorCoords)
        {
            int layerIndex;
            return GetSector(sectorCoords.X, sectorCoords.Y, out layerIndex);
        }*/

        /// <summary>
        /// call this when building the graph.
        /// </summary>
        /// <param name="sectorCoords"></param>
        /// <returns></returns>
        public SubtileSector GetUnfinishedSector(Point sectorCoords)
        {
            int layerIndex;
            return GetSector(sectorCoords.X, sectorCoords.Y, out layerIndex, false);
        }

       

        public SubtileSector GetSector(int sectorX, int sectorY)
        {
            int layerIndex;
            return GetSector(sectorX, sectorY, out layerIndex);
        }

        public SubtileSector GetOverriddenSector(int sectorX, int sectorY)
        {
            return Layers[0].Sectors[sectorX][sectorY];
        }

        public SubtileSector GetSector(int sectorX, int sectorY, out int layerIndex, bool ignoreNewUnfinishedSectors = true)
        {
            layerIndex = -1;

            for (int i = Layers.Length - 1; i >= 0; i--)
            {
                SubtileLayer layer = Layers[i];

                SubtileSector sector = layer.Sectors[sectorX][sectorY]; // GetSector();
                if (sector != null && (ignoreNewUnfinishedSectors == false || sector.HasFinishedFirstRun))
                {
                    layerIndex = i;
                    return sector;
                }
            }

#if !RELEASE
            System.Diagnostics.Debug.Assert(false, "No holes allowed");
            throw new Exception("No sector holes allowed");
#endif

            return null;
        }

        public void SetValueOnBottomLayer(int x, int y, MapManager.SubtileValue value)
        {
            Layers[0].SetValue(x, y, value);
        }

        public void SetValueOnBottomLayer(Point subtilePos, MapManager.SubtileValue value)
        {
            Layers[0].SetValue(subtilePos.X, subtilePos.Y, value);
        }

        public MapManager.SubtileValue GetValue(int sectorX, int sectorY, int relativeX, int relativeY, out int layerIndex)
        {
            SubtileSector sector = GetSector(sectorX, sectorY, out layerIndex);
            if (sector != null)
            {
                return sector.GetValue(relativeX, relativeY).Value; 
            }

            System.Diagnostics.Debug.Assert(false, "No holes allowed");

            return new MapManager.SubtileValue();
        }

        public MapManager.SubtileValue GetValue(int sectorX, int sectorY, int relativeX, int relativeY)
        {
            SubtileSector sector = GetSector(sectorX, sectorY);
            if (sector != null)
            {
                return sector.GetValue(relativeX, relativeY).Value; // .Values[relativeX][relativeY];
            }

            System.Diagnostics.Debug.Assert(false, "No holes allowed");

            return new MapManager.SubtileValue();
        }

        public MapManager.SubtileValue GetValue(Point subtilePos)
        {
            return GetValue(subtilePos.X, subtilePos.Y);
        }

        public MapManager.SubtileValue GetValue(SubtilePos subtilePos)
        {
            return GetValue(subtilePos.X, subtilePos.Y);
        }

        /// <summary>
        /// no bounds checking is done. This is parallel to the previous lookup directly into the arrays.
        /// </summary>
        /// <param name="absoluteX"></param>
        /// <param name="absoluteY"></param>
        /// <returns></returns>
        public MapManager.SubtileValue GetValue(int absoluteX, int absoluteY)
        {
            ushort relativeX, relativeY;
            ushort sectorX, sectorY;

            Layers[0].GetSectorAndRelativeCoords(absoluteX, absoluteY, out sectorX, out sectorY, out relativeX, out relativeY);

            return GetValue(sectorX, sectorY, relativeX, relativeY);

        }

        public MapManager.SubtileValue GetValue(int absoluteX, int absoluteY, out int layerIndex)
        {
            ushort relativeX, relativeY;
            ushort sectorX, sectorY;

            Layers[0].GetSectorAndRelativeCoords(absoluteX, absoluteY, out sectorX, out sectorY, out relativeX, out relativeY);

            return GetValue(sectorX, sectorY, relativeX, relativeY, out layerIndex);

        }

        public void Destroy(bool destroyRegionMapAndBottomLayer)
        {
          /*  if (destroyRegionMapAndBottomLayer)
            {*/
                RegionMap.Destroy();
           // }

            for (int i = 0; i < Layers.Length; i++)
            {
                if (i > 0 || destroyRegionMapAndBottomLayer)
                {
                    Layers[i].Destroy();
                }
            }


            RemoveIDEntry();
        }

        #region ILookUp

        //======ILookup===============
        private SubtileLayersID id = SubtileLayersID.Invalid;
        static SubtileLayersID IDCounter = SubtileLayersID.First;

        public SubtileLayersID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public SubtileLayersID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= SubtileLayersID.Max)
            {
                throw new Exception("Astounding, SubtileLayersID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public SubtileLayersID SnapshotID(Snapshotter sn, SubtileLayersID id)
        {
            return (SubtileLayersID)sn.DoEnum(id);
        }

        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }

        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != SubtileLayersID.Invalid)
            {
                LookUp<SubtileLayers, SubtileLayersID>.Add(ID, this);

                LookUp<SubtileLayers, SubtileLayersID>.SetLoadPostProcessOrder(10);        // after SubtileLayer - we snapshot this value in LookUp so it will also be there when loading fresh
            }
        }

        public void SetInvalid()
        {
            id = SubtileLayersID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<SubtileLayers, SubtileLayersID>.Remove(this);
        }

        void UWGame.SimSide.Snapshots.ILookUp<SubtileLayers, SubtileLayersID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = SubtileLayersID.First;
        }

        void ILookUp<SubtileLayers, SubtileLayersID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<SubtileLayers, SubtileLayersID>.Create();
        }

        #endregion


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = SnapshotID(sn, id);
            IDCounter = sn.DoEnum(IDCounter);
            
            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotLayers = Layers.Select(l => l.ID).ToArray();
            }
            snapshotLayers = sn.DoArray(snapshotLayers);

            snapshotRegionMap = (CyclableID)sn.SnapshotID<ICyclable, CyclableID>(RegionMap);

            sn.Ignore(Layers);
            sn.Ignore(RegionMap);

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

            Layers = snapshotLayers.Select(l => LookUp<SubtileLayer, SubtileLayerID>.FindByID(l)).ToArray();

            RegionMap = (RegionMap)LookUp<ICyclable, CyclableID>.FindByID(snapshotRegionMap);

           
        }

        #endregion

    }
}
