using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Maps
{
    public enum SubtileLayerID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

   
    public class SubtileLayer : Layer, ISnapshot, ILookUp<SubtileLayer, SubtileLayerID>
    {
        public SubtileSector[][] Sectors;


       

        public override int SectorSize
        {
            get { return MapManager.SectorSizeInSubtiles; }
        }
             

       
        //  public MapManager.SubtileValue[][] Values;


        public SubtileLayer(ushort subTileWidth, ushort subTileHeight): base(subTileWidth, subTileHeight)
        {
            AddToLookup();

            Common.InitJaggedArray(ref Sectors, The.Map.NoOfSectorsAcrossWidth, The.Map.NoOfSectorsAcrossHeight);


        }

        public SubtileLayer()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        /// <summary>
        /// called during init only
        /// </summary>
        public void CreateAllSectors()
        {
            for (int y = 0; y < SectorsAcrossHeight; y++)
            {
                for (int x = 0; x < SectorsAcrossWidth; x++)
                {

                    Sectors[x][y] = new SubtileSector(this, x, y, true);             
      
                }
            }
        }

        public SubtileSector GetSector(int sectorX, int sectorY)
        {
            return Sectors[sectorX][sectorY];
        }

        public SubtileSector GetSectorFromSubtiles(int absoluteSubtileX, int absoluteSubtileY)
        {
            return Sectors[absoluteSubtileX / MapManager.SectorSizeInSubtiles][absoluteSubtileY / MapManager.SectorSizeInSubtiles];
        }

        public SubtileSector GetOrCreateSector(int sectorX, int sectorY, out bool isNew)
        {
            SubtileSector sector = Sectors[sectorX][sectorY];
            if (sector == null)
            {
                sector = new SubtileSector(this, sectorX, sectorY);
                Sectors[sectorX][sectorY] = sector;
                isNew = true;
            }
            else
            {
                isNew = false;
            }

            return sector;
        }

        public void SetValue(int x, int y, MapManager.SubtileValue value)
        {
            ushort relativeX, relativeY;
            ushort sectorX, sectorY;

            GetSectorAndRelativeCoords(x, y, out sectorX, out sectorY, out relativeX, out relativeY);

            SubtileSector sector = Sectors[sectorX][sectorY];
            if (sector != null)
            {
                sector.Values[relativeX][relativeY] = value;
            }
        }

        public MapManager.SubtileValue? GetValue(int x, int y)
        {
            ushort relativeX, relativeY;
            ushort sectorX, sectorY;

            GetSectorAndRelativeCoords(x, y, out sectorX, out sectorY, out relativeX, out relativeY);

            SubtileSector sector = Sectors[sectorX][sectorY];
            if (sector != null)
            {
                return sector.Values[relativeX][relativeY];
            }

            return null;
        }

        /*  public static MapManager.SubtileValue GetValue(SubtileLayer[] layers, int x, int y)
          {


          }*/

        /// <summary>
        /// sectors where blocked status has changed
        /// </summary>
        /// <param name="action"></param>
        public void IterateDirtySectors(Action<SubtileSector> action)
        {
            IterateSectors(s =>
            {
                if (s.BlockedStatusHasChanged)
                {
                    action(s);
                }
            });

        }

        /// <summary>
        /// iterate left to right, top to bottom
        /// </summary>
        /// <param name="action"></param>
        public void IterateSectors(Action<SubtileSector> action)
        {
            for (int y = 0; y < SectorsAcrossHeight; y++)
            {
                for (int x = 0; x < SectorsAcrossWidth; x++)
                {
                    SubtileSector sector = Sectors[x][y];
                    if (sector != null)
                    {
                        action(sector);
                    }
                }
            }

        }

        public void Destroy()
        {          
            RemoveIDEntry();
        }

        #region ILookUp

        //======ILookup===============
        private SubtileLayerID id = SubtileLayerID.Invalid;
        static SubtileLayerID IDCounter = SubtileLayerID.First;

        public SubtileLayerID ID
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

        public SubtileLayerID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= SubtileLayerID.Max)
            {
                throw new Exception("Astounding, SubtileValueArrayID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public SubtileLayerID SnapshotID(Snapshotter sn, SubtileLayerID id)
        {
            return (SubtileLayerID)sn.DoEnum(id);
        }

        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != SubtileLayerID.Invalid)
            {
                LookUp<SubtileLayer, SubtileLayerID>.Add(ID, this);

                LookUp<SubtileLayer, SubtileLayerID>.SetLoadPostProcessOrder(0);        // before SubtileLayers
            }
        }

        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }

        public void SetInvalid()
        {
            id = SubtileLayerID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<SubtileLayer, SubtileLayerID>.Remove(this);
        }

        void UWGame.SimSide.Snapshots.ILookUp<SubtileLayer, SubtileLayerID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = SubtileLayerID.First;
        }

        void ILookUp<SubtileLayer, SubtileLayerID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<SubtileLayer, SubtileLayerID>.Create();
        }

        #endregion

        #region ISnapshot

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            id = SnapshotID(sn, id);
            IDCounter = (SubtileLayerID)sn.DoEnum(IDCounter);

            this.Sectors = sn.DoJaggedArray(Sectors);

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn);

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            sn.RegisterLoadPostProcessCall(this);


            IterateSectors(s => s.LoadPostProcess(sn));

        }

        #endregion
    }

    
}
