using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Soil //TODO DECOUPLE -- make (split?) this into a new renderable class and move to client
{
    public abstract class RenderedTerrainComponent: ISnapshot
    {
        private bool displayAmountIsDirty = false;
        private float amount;

        protected Terrain Parent;
        TerrainID snapshotParent;

        //protected bool isSubtileTerrain;

        public float Amount
        {
            get
            {
                return amount;
            }
            set
            {
                amount = value;
                displayAmountIsDirty = true;
            }

        }

        private float displayAmount;
        public float DisplayAmount
        {
            get
            {
                if (displayAmountIsDirty)
                {
                    displayAmount = ComputeDisplayAmount();

                    displayAmountIsDirty = false;
                }
                return displayAmount;
            }

        }

        public RenderedTerrainComponent(Terrain parent)
        {
            this.Parent = parent;
        }

        public RenderedTerrainComponent()
        {
           
        }

        protected virtual float ComputeDisplayAmount()
        {
            return amount;
        }


        public void RecomputeDisplayAmount()
        {
            displayAmount = ComputeDisplayAmount();

            displayAmountIsDirty = false;
        }



        #region ISnapshot
        // TODO: split this class and don't save client values

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            snapshotParent = (TerrainID)sn.SnapshotID<Terrain, TerrainID>(Parent);
            amount = sn.DoFloat(amount);

            // TODO: move this to client:
            displayAmount = sn.DoFloat(displayAmount);
            displayAmountIsDirty = sn.DoBool(displayAmountIsDirty);

            return this;
        }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Parent = LookUpSortedDictionary<Terrain, TerrainID>.FindByID(snapshotParent);

            if (Parent == null)
            {

            }
        }

        #endregion

    }
}
