using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Soil 
{

    // the parallel loops in MapLoader will give bugs if SoilComponent gets an ID!!
    public class SoilComponent : RenderedTerrainComponent
    {
        public SoilComponentType SoilComponentType;
                

        public SoilComponent(Terrain parent, SoilComponentType type): base(parent)
        {
            this.SoilComponentType = type;
           
        }

        public SoilComponent()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }

        protected override float ComputeDisplayAmount()
        {
            float factor = 1f;
            if (Parent.IsSubtileTerrain())
            {
                factor = 9f;
            }

            if (SoilComponentType.ScaleDisplayAmountAsWithRocks)
            {
                return GetRockAlpha(factor);
            }
            else
            {
                return Common.ClampTop(factor * Amount, 1f);
            }

        }


        /// <summary>
        /// TODO: move to client
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        public float GetRockAlpha(float factor)
        {
            //f(x) = (2 / (1 + e^(-5*x))) - 1

            // Amount = 0.2 -> 0.75
            return Common.ClampTop((float)((2.0 / (1.0 + Math.Pow(Math.E,(-10.0 * factor * Amount)))) - 1.0), 1f);

        }


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

            this.SoilComponentType = sn.DoGameData(SoilComponentType);


            return this;
        }


      
        #endregion

    }
}
