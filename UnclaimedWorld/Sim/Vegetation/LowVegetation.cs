using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Soil;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Vegetation//TODO DECOUPLE -- needs a renderable part..
{
    public enum LowVegetationID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

   /// <summary>
   /// Represents the growth of this type of vegetation in one tile.
   /// </summary>
    public class LowVegetation : RenderedTerrainComponent, IHasCrops, 
        ILookUp<LowVegetation, LowVegetationID> // this is needed to provide an ID for IHasCrops
    {
        public LowVegetationType LowVegetationType;

        
        public float Lushness;

        //public Crop Crop;
      //  ResourceID snapshotCrop;
     


        protected override float ComputeDisplayAmount()
        {
            float factor = 1f;
            if (Parent.IsSubtileTerrain())
            {
                factor = 9f;
            }

            return Common.ClampTop(factor * 1.4f * Amount, 1f);           

        }


        public LowVegetation()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }

        public LowVegetation(Terrain parent, LowVegetationType type): base(parent)
        {
            AddToLookup();
            ((ILookUp<IHasCrops, HasCropsID>)this).AddToLookup();

            this.LowVegetationType = type;
           
        }

        public void Destroy()
        {
            RemoveIDEntry();
        }

        #region ILookup

        private LowVegetationID id = LowVegetationID.Invalid;
        static LowVegetationID IDCounter = LowVegetationID.First;

        public LowVegetationID ID
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

        public LowVegetationID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= LowVegetationID.Max)
            {
                throw new Exception("Astounding, LowVegetationID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public LowVegetationID SnapshotID(Snapshotter sn, LowVegetationID id)
        {
            return (LowVegetationID)sn.DoEnum(id);
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
            if (ID != LowVegetationID.Invalid)
                LookUpSortedDictionary<LowVegetation, LowVegetationID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = LowVegetationID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUpSortedDictionary<LowVegetation, LowVegetationID>.Remove(this);
        }

        void ILookUp<LowVegetation, LowVegetationID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = LowVegetationID.First;
        }

        void ILookUp<LowVegetation, LowVegetationID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUpSortedDictionary<LowVegetation, LowVegetationID>.Create();
        }

        #endregion


        #region IHasCrops

        public Point MapPosition
        {
            get
            {
                return new Point(Parent.Parent.X, Parent.Parent.Y);               
            }
        }

        public Vector3 AccessPoint
        {
            get
            {
                return MapManager.TileToWorldPos(new Point(Parent.Parent.X, Parent.Parent.Y));        
            }
        }

        public Vector3 Location
        {
            get
            {
                return MapManager.TileToWorldPos(new Point(Parent.Parent.X, Parent.Parent.Y));             
            }
        }


        #endregion


        #region HasCropsID ILookup

        HasCropsID hasCropsID;
        HasCropsID ILookUp<IHasCrops, HasCropsID>.ID
        {
            get
            {
                return hasCropsID;
            }
        }

        HasCropsID ILookUp<IHasCrops, HasCropsID>.GetUniqueID()
        {
            return HasCrops.GetUniqueID();
        }

       

        void ILookUp<IHasCrops, HasCropsID>.AddToLookup()
        {
            hasCropsID = ((ILookUp<IHasCrops, HasCropsID>)this).GetUniqueID();

            if (hasCropsID != HasCropsID.Invalid)
            {
                LookUpIHasCrops.Add(hasCropsID, this); // uses special class!
            }
        }

        void ILookUp<IHasCrops, HasCropsID>.RemoveIDEntry()
        {
            LookUpIHasCrops.Remove(this);  // uses special class!
        }

        void ILookUp<IHasCrops, HasCropsID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IHasCrops, HasCropsID>.SetInvalid()
        {
            hasCropsID = HasCropsID.Invalid;
        }

        void ILookUp<IHasCrops, HasCropsID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

       /* public static void CreateLookupCollection()
        {
            LookUpIHasCrops.Create();
        }*/

        #endregion

        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            id = SnapshotID(sn, id);
            IDCounter = (LowVegetationID)sn.DoEnum(IDCounter);
            this.hasCropsID = (HasCropsID)sn.DoEnum(hasCropsID);

          //  this.snapshotCrop = sn.SnapshotID<ResourceContainer, ResourceID>(Crop);

            this.LowVegetationType = sn.DoGameData(LowVegetationType);
            this.Lushness = sn.DoFloat(Lushness);
            

            return this;
        }




        #endregion
    }
}
