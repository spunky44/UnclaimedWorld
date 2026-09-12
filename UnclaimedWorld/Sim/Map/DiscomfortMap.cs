using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using System.Threading.Tasks;
using UWGame.SimSide.AI;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.Maps
{

   /* public enum DiscomfortMapID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }*/


    /// <summary>
    /// exposed: map contains blocked tiles. Protected: no tiles are blocked.
    /// 
    /// this map should have Fire and Threat as children... and maybe also a map for general discomfort such as wetness, poison etc.
    /// The discomfort map is the added result of its child maps! It does not draw anything by itself, nor should it ever!!
    ///    
    /// </summary>
    public enum ProtectionLevel : ulong { Exposed, Protected }
    public class DiscomfortMap: IMap, ISnapshot
    {
        public byte MaxValue = 200;
      //  public float PropagationConstant = 0.9f;
      //  public Point MapPosition;
      //  public int Radius = 10;
        public ProtectionLevel ProtectionLevel;

        /// <summary>
        /// this is set true when the map has finished updating itself (could take several cycles) and can be added to the parent map(s)
        /// </summary>
        public bool IsReady {get; set;} // = false;

        /// <summary>
        /// NEW - can have holes.
        /// </summary>
        public TileLayer Map { get; private set; }
        

        public string IDName;


        /// <summary>
        /// during drawing and after clear, all sectors that get a value are gathered in this set.
        /// sectors that are not drawn into, and which are not added to by a child map, should be removed at the end.
        /// </summary>
      /*  private HashSet<Point> affectedSectors = new HashSet<Point>();

        /// <summary>
        /// these are the sectors that were drawn into last time. We compare the two sets for any changes in order to signal the dependent maps via the IsDirty flag.
        /// </summary>
        private HashSet<Point> previouslyAffectedSectors = new HashSet<Point>();
       */

        private List<Dependence> children = new List<Dependence>();

        /// <summary>
        /// for now, only has a Threat map as child.
        /// </summary>
        public List<Dependence> Children { get { return children; } set { children = value; } }

      //  public Dependence Parent { get; set; }

        public DiscomfortMap()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
   
        }

        public DiscomfortMap(ProtectionLevel protectionLevel, ThreatMap childThreatMap)           
        {
            ((ILookUp<IMap, IMapID>)this).AddToLookup();

            this.ProtectionLevel = protectionLevel;

            int mapWidth, mapHeight;

            Map = new TileLayer((ushort)The.Map.mapTileWidth, (ushort)The.Map.mapTileHeight);
            /*
            mapWidth = Common.GetJaggedArrayWidth(childThreatMap.Map);
            mapHeight = Common.GetJaggedArrayHeight(childThreatMap.Map);

            Common.InitJaggedArray(ref map, mapWidth, mapHeight);
            Common.InitJaggedArray(ref isBlocked, mapWidth, mapHeight);
            */

            Dependence dep1 = new Dependence(childThreatMap, 1f); 
            Children.Add(dep1);
          
           // shared.AddDiscomfortMap(this, childThreatMap);

        }

        public IMap GetCurrent()
        {
           /* if (IsDirty())
            {
                AddChildMaps();

            }*/

            return this;
        }


        public void Destroy()
        {
            RemoveIDEntry();
        }

        //???????????????????????
       /* public virtual bool IsDirty()
        {

            if (!isDirty)
            {
                foreach (Dependence dep in Children)
                {
                    if (dep.IsDirty || dep.Child.IsDirty())
                    {
                        return true;
                    }
                }
            }
            else { return true; }

            return false;
        }*/

     /*   public bool SectorIsDirty(int x, int y) 
        {
            return Sectors[x][y].IsDirty;
        }*/


        private void GetSectorsToReAdd()
        {
            sectorsToReAdd.Clear();

         //   sectorsToReAdd.UnionWith(childSectorsThatHaveBeenRemovedCR);
            sectorsToReAdd.UnionWith(childSectorsThatHaveChangedCC);
           // sectorsToReAdd.UnionWith(sectorsThatHaveChangedDiscomfortDC);            

        }

        private void ClearSectorsBeforeReadding()
        {
           /* HashSet<Point> sectorsThatNeedClearing = new HashSet<Point>();
            sectorsThatNeedClearing.UnionWith(sectorsToReAdd);
         //   sectorsThatNeedClearing.ExceptWith(sectorsThatHaveDrawnDiscomfortDD); 
            */

            Parallel.ForEach(sectorsToReAdd, s =>
            {
                TileSector sector = Map.Sectors[s.X][s.Y];

                if (sector != null)
                {
                    sector.ClearMap();
                }               

            });
        }

        /// <summary>
        /// signal move map of changes via IsDirty:
        /// dd, dc, cc, cr
        /// </summary>
        private void SetDirtyFlagToNotifyMoveMap()
        {
            // notify move map of all sectors that changed.
          /*  HashSet<Point> sectorsToSetDirty = new HashSet<Point>();
            //sectorsToSetDirty.UnionWith(sectorsToRemove); // we can not notify about removed sectors... move map has to track those.
           // sectorsToSetDirty.UnionWith(sectorsThatHaveChangedDiscomfortDC);
            sectorsToSetDirty.UnionWith(childSectorsThatHaveChangedCC);
            */

            foreach (var coords in childSectorsThatHaveChangedCC) //sectorsToSetDirty)
            {
                TileSector ownSector = Map.GetOrCreateSector(coords.X, coords.Y);
                if (ownSector != null)
                {
                    ownSector.IsDirty = true; // set the flag to notify MoveMap!
                }               
            }            
        }

        /// <summary>
        /// cases:
        /// discomfort drawn: dd
        /// discomfort change: dc 
        /// child sector change: cc
        /// child sector removed: cr
        /// 
        /// re-add:
        /// dc, cc
        /// </summary>
        private void AddSectors()
        {
         
            // let's create the sectors outside the parallel loop, not sure if its thread safe:
            foreach (var sector in sectorsToReAdd)
            {
                TileSector ownSector = Map.GetOrCreateSector(sector.X, sector.Y);                 
            }


            Parallel.ForEach(sectorsToReAdd, sector =>
            {
                TileSector ownSector = Map.GetSector(sector.X, sector.Y); // Map.Sectors[sector.X][sector.Y];
                

                foreach (Dependence child in Children)
                {
                    TileSector sectorToAdd = child.Child.Map.Sectors[sector.X][sector.Y];
                    if (sectorToAdd != null)
                    {
                        ownSector.AddSector(sectorToAdd, child.Weight);
                    }

                    //  InfluenceMap.AddTilesInSector(sector.TileArea, child.Weight, map, IsBlocked, child.Child.Map, child.Child.IsBlocked);
                }

               // ownSector.IsDirty = true; // set the flag to notify MoveMap!

            });

            /*
            Parallel.ForEach(listOfSectors, sector =>
            {               
                foreach (Dependence child in Children)
                {
                    TileSector sectorToAdd = child.Child.Map.Sectors[sector.Coords.X][sector.Coords.Y];
                    if (sectorToAdd != null)
                    {
                        sector.AddSector(sectorToAdd, child.Weight);
                    }
               
                  //  InfluenceMap.AddTilesInSector(sector.TileArea, child.Weight, map, IsBlocked, child.Child.Map, child.Child.IsBlocked);
                }
                sector.IsDirty = true;
            });*/
        }

       


      /*  public static Point GetClosestNonBlockedTile(Point from, int searchRadius, ThreatCategory usersCategory, ProtectionLevel protLevel, EntityApproach approach)
        {
            int minX, maxX, minY, maxY;
            MapManager.GetClampedMapArea(from, searchRadius, out minX, out maxX, out minY, out maxY);
            int currentDist, bestDist = 100000000;
            byte bestComfort = 255, currentComfort;
            Point bestTile = from, currentTile;
           
            DiscomfortMap map = UWGame.SimSide.Instance.Map.GetDiscomfortMap(protLevel, usersCategory, approach);

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    if (!map.IsBlocked[x, y])
                    {
                        currentTile = new Point(x, y);
                        currentDist = Common.DistanceOctile(from, currentTile);
                        if (currentDist < bestDist)
                        {
                            bestDist = currentDist;
                            bestComfort = map.Map[x, y];
                            bestTile = currentTile;
                        }
                        else if (currentDist == bestDist)
                        {
                            currentComfort = map.Map[x, y];
                            if (currentComfort < bestComfort)
                            {
                                bestDist = currentDist;
                                bestComfort = map.Map[x, y];
                                bestTile = currentTile;
                            }
                        }
                    }
                }
            }

            return bestTile;
        }
        */

       
        private enum Phase { InitSectors, ComputeChildMaps, GetSectorsToAdd, ClearSectors, AddChildMaps, RemoveEmptySectors }
        private Phase phase = Phase.InitSectors;

        HashSet<Point> childSectorsThatHaveChangedCC = new HashSet<Point>();
        HashSet<Point> childSectorsThatHaveBeenRemovedCR = new HashSet<Point>();
      /*  HashSet<Point> sectorsThatHaveDrawnDiscomfortDD = new HashSet<Point>();
        HashSet<Point> sectorsThatHaveChangedDiscomfortDC = new HashSet<Point>();
        */

        HashSet<Point> sectorsToRemove = new HashSet<Point>();           
        HashSet<Point> sectorsToReAdd = new HashSet<Point>();
      
        /// <summary>
        /// used to detect removed child sectors
        /// </summary>
        HashSet<Point> previousChildSectors = new HashSet<Point>();

     //   List<TileSector> listOfChildSectorsToAdd = new List<TileSector>();

        private int childMapIndex = 0;

      
        /// <summary>
        /// not called from CycleManager, but from MovementMap.DoCycle()
        /// 
        /// Does not do a full re-draw! only when there are changes in the children. otherwise the current data is kept.
        /// 
        /// re-add only when:
        /// child sector changes  
        /// 
        /// cases:    
        /// child sector change: cc
        /// child sector removed: cr
        ///       
        ///          
        /// 
        /// </summary>
        /// <returns></returns>
        public bool DoCycle()
        {
            switch (phase)
            {
                case Phase.InitSectors:
                    
                    //InfluenceMap.ClearSectors(Sectors);
                    Map.IterateSectors(s => s.IsDirty = false);

                    phase = Phase.ComputeChildMaps;
                    return false;

                case Phase.ComputeChildMaps:
                    if (Children[childMapIndex].Child.DoCycle() == true)
                    {
                        childMapIndex++;

                        if (childMapIndex == Children.Count)
                        {
                            phase = Phase.GetSectorsToAdd;
                            childMapIndex = 0;
                        }
                    }

                    return false;

                case Phase.GetSectorsToAdd:
                   
                    GetChildSectorsThatHaveChanged(); // cc
                    GetChildSectorsThatHaveBeenRemoved(); // cr

                  //  Map.BeginDrawing(); // no need to track changes or use double buffer.

                    phase = Phase.ClearSectors;
                    return false;                      

              /*  case Phase.DrawDiscomfort:
                    DrawDiscomfort();
                    phase = Phase.CompareSectors;
                    return false;

                case Phase.CompareSectors: // compare discomfort changes before adding!

                    GetSectorsThatHaveChangedDiscomfort();       // dc , dd                        

                    phase = Phase.ClearSectorTiles;
                    return false; 
                    */

                case Phase.ClearSectors:
                    GetSectorsToReAdd();
                    ClearSectorsBeforeReadding();
                    phase = Phase.AddChildMaps;
                    return false;

                case Phase.AddChildMaps:
                    AddSectors(); // 
                   
                    phase = Phase.RemoveEmptySectors;
                    return false;  

                case Phase.RemoveEmptySectors: 
                    RemoveEmptySectors();


                    SetDirtyFlagToNotifyMoveMap();


                    phase = Phase.InitSectors;
                    return true;  // done.

                default: return true;
            }
            
        }

      /*  void GetSectorsThatHaveChangedDiscomfort()
        {
            // add the sectors that we touched last time, but didn't this time.
            HashSet<Point> currentAndPreviousAffectedSectors = new HashSet<Point>();
            currentAndPreviousAffectedSectors.UnionWith(Map.PreviouslyAffectedSectors);
            currentAndPreviousAffectedSectors.UnionWith(Map.AffectedSectors);

            sectorsThatHaveChangedDiscomfortDC = Map.GetSectorsThatHaveChanged(currentAndPreviousAffectedSectors);

            sectorsThatHaveDrawnDiscomfortDD.Clear();
            sectorsThatHaveDrawnDiscomfortDD.UnionWith(Map.AffectedSectors);

        }*/


        void RemoveEmptySectors()
        {            
            /// remove sectors 
            /// when no child sector exists.
            
            HashSet<Point> allSectors = Map.GetAllSectors();
           // sectorsNotDrawDiscomfort.ExceptWith(sectorsThatHaveDrawnDiscomfortDD);


            sectorsToRemove.Clear();

            foreach (var item in allSectors)
            {
                if (Children.TrueForAll(d => d.Child.Map.GetSector(item.X, item.Y) == null))
                {
                    sectorsToRemove.Add(item);
                }                
            }

            Map.RemoveSectors(sectorsToRemove);

        }

        void GetChildSectorsThatHaveChanged()
        {
            childSectorsThatHaveChangedCC.Clear();
            foreach (Dependence child in Children)
            {
                child.Child.Map.GetDirtySectors(ref childSectorsThatHaveChangedCC);
            }

        }

        void GetChildSectorsThatHaveBeenRemoved()
        {
            childSectorsThatHaveBeenRemovedCR.Clear();
            childSectorsThatHaveBeenRemovedCR.UnionWith(previousChildSectors);

            previousChildSectors.Clear();
            foreach (Dependence child in Children)
            {
                HashSet<Point> allSectors = child.Child.Map.GetAllSectors();
                childSectorsThatHaveBeenRemovedCR.ExceptWith(allSectors);

                previousChildSectors.UnionWith(allSectors); // gather sectors for next update.
            }

        }

        #region ILookup

        private IMapID id = IMapID.Invalid;
       
        public IMapID ID
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

      

        public IMapID SnapshotID(Snapshotter sn, IMapID id)
        {
            return (IMapID)sn.DoEnum(id);
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
            if (ID != IMapID.Invalid)
                LookUp<IMap, IMapID>.Add(ID, this);
        }


        public IMapID GetUniqueID()
        {
            return IMapCounter.GetUniqueID();
        }

        public void SetInvalid()
        {
            id = IMapID.Invalid;
        }

        public void RemoveIDEntry()
        {
            // LookUp<Entity, EntityID>.Remove(ID);
            LookUp<IMap, IMapID>.Remove(this);
        }

        void UWGame.SimSide.Snapshots.ILookUp<IMap, IMapID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        void ILookUp<IMap, IMapID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<IMap, IMapID>.Create();
        }
        
        #endregion



        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = SnapshotID(sn, id);

            this.Map = (TileLayer)sn.DoISnapshot(Map);
            this.childMapIndex = sn.DoInt32(childMapIndex);
            this.children = sn.DoList(children);
            this.IDName = sn.DoString(IDName);          
            this.previousChildSectors = sn.DoHashSet(previousChildSectors);
          
            this.MaxValue = sn.DoByte(MaxValue);
            this.phase = sn.DoEnum(phase);
            this.ProtectionLevel = sn.DoEnum(ProtectionLevel);

            this.IsReady = sn.DoBool(IsReady);

                
          /*  this.sectorsThatHaveChangedDiscomfortDC = sn.DoHashSet(sectorsThatHaveChangedDiscomfortDC);
            this.sectorsThatHaveDrawnDiscomfortDD = sn.DoHashSet(sectorsThatHaveDrawnDiscomfortDD);*/
            this.sectorsToReAdd = sn.DoHashSet(sectorsToReAdd);
            this.childSectorsThatHaveChangedCC = sn.DoHashSet(childSectorsThatHaveChangedCC);
            this.childSectorsThatHaveBeenRemovedCR = sn.DoHashSet(childSectorsThatHaveBeenRemovedCR);
            this.sectorsToRemove = sn.DoHashSet(sectorsToRemove);

            
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

            foreach (var item in Children)
            {
                item.LoadPostProcess(sn);
            }

            Map.LoadPostProcess(sn);


        }

        #endregion


     /*   #region IMapID ILookup

        IMapID mapID;
        IMapID ILookUp<IMap, IMapID>.ID
        {
            get
            {
                return mapID;
            }
        }

        IMapID ILookUp<IMap, IMapID>.GetUniqueID()
        {
            return IMapCounter.GetUniqueID();
        }

        IMapID ILookUp<IMap, IMapID>.SnapshotID(Snapshotter sn, IMapID id)
        {
            return (IMapID)sn.DoEnum(id);
        }

        void ILookUp<IMap, IMapID>.AddToLookup()
        {
            mapID = ((ILookUp<IMap, IMapID>)this).GetUniqueID();

            if (mapID != IMapID.Invalid)
            {
                LookUpMap.Add(mapID, this); // uses special class!
            }
        }

        void ILookUp<IMap, IMapID>.RemoveIDEntry()
        {
            LookUpMap.Remove(this);  // uses special class!
        }

        void ILookUp<IMap, IMapID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IMap, IMapID>.SetInvalid()
        {
            mapID = IMapID.Invalid;
        }

        #endregion*/

    }
}
