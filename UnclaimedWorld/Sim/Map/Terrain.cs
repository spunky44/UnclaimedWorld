using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Soil;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Resources;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps 
{
    public enum TerrainID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }

    public class Terrain: ILookUp<Terrain, TerrainID>, ISnapshot
    {
        /// <summary>
        /// soil + veg are normalized to give 1 when summed together over a tile
        /// They describe what the surface is composed of/covered by.
        /// NOTE: the amounts are a factor 9 less if we are in subtiles...
        /// </summary>
        public Dictionary<LowVegetationType, LowVegetation> Vegetation;
        Dictionary<string, LowVegetationID> snapshotVegetation;

       // public Dictionary<string, SoilComponent> SoilComponents;
        public Dictionary<SoilComponentType, SoilComponent> SoilComponents;
        

        /// <summary>
        /// The absolute depth of this tile - this will probably never change.
        /// </summary>
        public float TerrainDepth = 0f;

        /// <summary>
        /// How far below CURRENT water level are we?
        /// Positive and greater than zero if we are under water.
        /// </summary>
        public float LevelBelowWater = 0f;


        /// <summary>
        /// used in movement
        /// </summary>
        public SurfaceType SurfaceType;

        /// <summary>
        /// i did not want to give an ID to SurfaceType, it should be made into GameData or removed...
        /// </summary>
        bool snapshotSurfaceTypeIsPlains;


        
        public TerrainTile Parent;
        TerrainTileID snapshotParent;

       // public float RenderAtPosX;
      //  public float RenderAtPosY;

        /// <summary>
        /// cached data for the renderer. these values never change
        /// </summary>
       // public Vector3 RenderPosition;
      //  public Vector2 RenderTextureCoordinate;

        
        public Terrain(TerrainTile parent)
        {
            AddToLookup();

            this.Parent = parent;

        }

        public Terrain()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }

        public bool IsUnderWater()
        {
            return LevelBelowWater > 0f;
        }

      /*  public void AddVegetation(string keyName, float amount) //LowVegetation veg)
        {
            if (Vegetation == null)
            {
                Vegetation = new Dictionary<string, LowVegetation>();
            }

            LowVegetation existingVegetation;
            if (Vegetation.TryGetValue(veg.LowVegetationType.KeyName, out existingVegetation))
            {
                existingVegetation.Amount += veg.Amount;
            }
            else
            {
                Vegetation.Add(veg.LowVegetationType.KeyName, veg);
            }
        }*/

        public void AddVegetation(LowVegetationType vegType, float amount) //LowVegetation veg)
        {
            if (Vegetation == null)
            {
                Vegetation = new Dictionary<LowVegetationType, LowVegetation>(); // new Dictionary<string, LowVegetation>();
            }

            LowVegetation terrainVegetation;
            if (!Vegetation.TryGetValue(vegType, out terrainVegetation))
            {
                terrainVegetation = new LowVegetation(this, vegType);
                Vegetation.Add(vegType, terrainVegetation);
            }

            terrainVegetation.Amount += amount;
            terrainVegetation.Amount = Common.ClampBottom(terrainVegetation.Amount, 0f); // clamp top too???
        }

        public void AddSoilComponent(SoilComponentType soilType, float amount)
        {
            if (SoilComponents == null)
            {
                SoilComponents = new Dictionary<SoilComponentType, SoilComponent>(); // new Dictionary<string, SoilComponent>();
            }

            SoilComponent terrainSoil;
            if (!SoilComponents.TryGetValue(soilType, out terrainSoil))
            {
                //SoilComponent tileSoil 
                terrainSoil = new SoilComponent(this, soilType);               
                SoilComponents.Add(soilType, terrainSoil);
            }

            float finalAmount = terrainSoil.Amount + amount;
            finalAmount = Common.ClampBottom(finalAmount, 0f); // clamp top too???
            terrainSoil.Amount = finalAmount;
        }

        /// <summary>
        /// 0: flat - 1: most rugged
        /// 
        /// TODO: cache the value and use it in the pathfinder... perhaps split up into types of factors... and in the pathfinder, the entity can negate some of them...
        /// </summary>
        /// <returns></returns>
        public float GetRoughness()
        {
            float soilFactor = 0f;
            if (SoilComponents != null)
            {
                // the soil amounts are normalized to 1, so use the amounts as weights and sum them:
                soilFactor = SoilComponents.Sum(s => s.Value.SoilComponentType.MoveFactor * s.Value.Amount);
                
            }

            float vegFactor = 0f;
            if (Vegetation != null)
            {
                vegFactor = Vegetation.Sum(s => s.Value.LowVegetationType.MoveFactor * s.Value.Amount);
            }

            float combinedRoughness = soilFactor + vegFactor;

            // compensate for fractional subtile amounts:
            if (IsSubtileTerrain())
            {
                combinedRoughness *= 9f;
            }

            return Common.ClampTop(combinedRoughness, 1f);

        }

    /*    public void AddSoilComponent(SoilComponent soil)
        {
            if (SoilComponents == null)
            {
                SoilComponents = new Dictionary<string, SoilComponent>();
            }

            SoilComponent existingSoil;
            if (SoilComponents.TryGetValue(soil.SoilComponentType.KeyName, out existingSoil))
            {
                existingSoil.Amount += soil.Amount;
            }
            else
            {
                SoilComponents.Add(soil.SoilComponentType.KeyName, soil);
            }
            
        }
        */
        private float GetMaxAmountForResources()
        {
            if (IsSubtileTerrain())
            {
                return maxAmountPerSubtile;
            }
            else return 1f;
        }

        public void RecomputeDisplayAmounts()
        {
            // we need to call this after combining the subtiles:
            if (Vegetation != null)
            {
                foreach (var veg in Vegetation)
                {
                    veg.Value.RecomputeDisplayAmount();
                }
            }

            if (SoilComponents != null)
            {
                foreach (var soil in SoilComponents)
                {
                    soil.Value.RecomputeDisplayAmount();
                }
            }
        }

        public void NormalizeVegetation()
        {
            
            if (Vegetation != null)
            {

                float factorToReduceWith;
                float totalGrowth = 0f;

                float maxAmount = GetMaxAmountForResources();


                foreach (var veg in Vegetation)
                {
                    totalGrowth += veg.Value.Amount;
                }

                // float factor = totalGrowth / currentTile.Vegetation.Count;
                if (totalGrowth > maxAmount)
                {
                    factorToReduceWith = maxAmount / totalGrowth;
                    foreach (var veg in Vegetation)
                    {
                        veg.Value.Amount = factorToReduceWith * veg.Value.Amount;
                        // veg.Value.Amount = veg.Value.Amount / totalGrowth;

                    }
                }

            }

            
        }

        private const float maxAmountPerSubtile = 1f / 9f;


        public void NormalizeSoil()
        {           

            if (SoilComponents != null)
            {
                float totalAmount = 0f;
                float factorToReduceWith;

                float maxAmount = GetMaxAmountForResources();

                foreach (var soil in SoilComponents)
                {
                    // don't normalize rocks...?
                    /*if (!soil.Value.SoilComponentType.HasTransparency)
                    {*/
                    totalAmount += soil.Value.Amount;
                }

                if (totalAmount > maxAmount)
                {
                    factorToReduceWith = maxAmount / totalAmount;

                    foreach (var soil in SoilComponents)
                    {
                        //  soil.Value.Amount = soil.Value.Amount / totalAmount;                                      
                        soil.Value.Amount = soil.Value.Amount * factorToReduceWith;
                    }
                }
            }

        }

        public bool IsSubtileTerrain()
        {
            return Parent.Terrain != this;
        }


        private void GetSubtileCoords(out int? sx, out int? sy)
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (Parent.TerrainSubtiles[x][y] == this)
                    {
                        sx = x;
                        sy = y;
                        return;
                    }
                }
            }

            sx = null;
            sy = null;
        }

        #region ILookup

        private TerrainID id = TerrainID.Invalid;
        static TerrainID IDCounter = TerrainID.First;

        public TerrainID ID
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

        public TerrainID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= TerrainID.Max)
            {
                throw new Exception("Astounding, TerrainID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public TerrainID SnapshotID(Snapshotter sn, TerrainID id)
        {
            return (TerrainID)sn.DoEnum(id);
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
            if (ID != TerrainID.Invalid)
                LookUpSortedDictionary<Terrain, TerrainID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = TerrainID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUpSortedDictionary<Terrain, TerrainID>.Remove(this);
        }

        void ILookUp<Terrain, TerrainID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = TerrainID.First;
        }

        void ILookUp<Terrain, TerrainID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUpSortedDictionary<Terrain, TerrainID>.Create();
        }


        #endregion

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = SnapshotID(sn, id);
            IDCounter = sn.DoEnum(IDCounter);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                if (Vegetation != null)
                {
                    snapshotVegetation = Vegetation.ToDictionary(v => v.Key.KeyName, 
                        v => v.Value.ID);
                }
            }

            this.LevelBelowWater = sn.DoFloat(LevelBelowWater);
            this.SoilComponents = sn.DoDictionary(SoilComponents);
            this.TerrainDepth = sn.DoFloat(TerrainDepth);
            this.snapshotVegetation = sn.DoDictionary(snapshotVegetation);
            this.snapshotParent = (TerrainTileID)sn.SnapshotID<TerrainTile, TerrainTileID>(Parent);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                if (SurfaceType.Name.Contains("Water")) // HACK!!
                {
                    snapshotSurfaceTypeIsPlains = false;
                }
                else
                {
                    snapshotSurfaceTypeIsPlains = true;
                }
            }

            snapshotSurfaceTypeIsPlains = sn.DoBool(snapshotSurfaceTypeIsPlains);

            sn.Ignore(SurfaceType);
            sn.Ignore(Vegetation);

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

            Parent = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotParent);

            if (snapshotVegetation != null)
            {
                Vegetation = snapshotVegetation.ToDictionary(v => GameData.Instance.AllLowVegetationTypes[v.Key], 
                    v => LookUpSortedDictionary<LowVegetation, LowVegetationID>.FindByID(v.Value));
            }
            snapshotVegetation = null; // cleared for next save

            if (snapshotSurfaceTypeIsPlains)
            {
                SurfaceType = PlainsType.Instance;                
            }
            else
            {
                SurfaceType = WaterType.Instance;                
            }

            if (SoilComponents != null)
            {
                foreach (var item in SoilComponents)
                {
                    item.Value.LoadPostProcess(sn);
                }
            }
        }

        #endregion
        
    }
}
