using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Needs
{
    public class FoodNeed : ISnapshot
    {
        public Need Parent;

        private bool bulkIsDirty = true;

        private float neededNutrientBulk;
        public float CurrentNeededNutrientBulk
        {
            get
            {
                if (bulkIsDirty) //Parent.CurrentLevelIsDirty) 
                {
                    neededNutrientBulk = TotalNeededNutrientBulk * (1f - Parent.CurrentLevel);
                    bulkIsDirty = false;
                }

                return neededNutrientBulk;
            }
        }

        /// <summary>
        /// daily needed bulk
        /// </summary>
        public float TotalNeededNutrientBulk;



        
        public FoodNeed()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public FoodNeed(Need parent)
        {
            this.Parent = parent;
        }

        public void SetNeedDirty()
        {
            bulkIsDirty = true;
        }

        public void UpdateBulk()
        {
            TotalNeededNutrientBulk = Parent.NeedType.FoodNeedType.RequiredNutrientsAsFractionOfEntityBulk * Parent.Parent.Parent.Parent.Bulk;
        }



        #region ISnapshot

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
            this.neededNutrientBulk = sn.DoFloat(neededNutrientBulk);
            this.TotalNeededNutrientBulk = sn.DoFloat(TotalNeededNutrientBulk);


            sn.Ignore(Parent); // is summarily assigned in Need Post Load
            sn.Ignore(bulkIsDirty);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //lookups and other fix-ups
        }

        #endregion
    }
}
