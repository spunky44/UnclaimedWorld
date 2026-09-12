using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Substances
{
    /// <summary>
    /// TODO: replace resource container items with substances... not sure how much code can be reused
    /// </summary>
    public class SubstanceComponent : Component, IIDEventSubscriber
    {
        private Dictionary<SubstanceType, SubstanceAmount> bulkAmounts = null;
        /// <summary>
        /// current bulk amounts of all substances
        /// </summary>
        public Dictionary<SubstanceType, SubstanceAmount> BulkAmounts
        {
            get
            {
                return bulkAmounts;
            }
            set
            {
                bulkAmounts = value;
            }

        }
       
        MethodID parentBulkChangedID;

        public SubstanceComponent(Entity parent): base(parent)
        {           
            Parent.BulkChangedEvent.AddAndRegister(Parent_BulkChanged,
                this, out parentBulkChangedID);

           // Parent.BulkChanged += new Entity.BulkChangedHandler(Parent_BulkChanged);
        }

        public SubstanceComponent()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       
        }


        /// <summary>
        /// reduce or increase substance amounts in proportion to the bulk change
        /// </summary>
        void Parent_BulkChanged(float oldValue)
        {
            if (BulkAmounts == null)
            {
                InitializeSubstanceAmounts();
            }
            else
            {
                UpdateSubstanceAmountsWithNewBulk(oldValue);
            }
        }


        /// <summary>
        /// will also reduce the bulk of the parent
        /// </summary>
        /// <param name="type"></param>
        /// <param name="changeInBulk"></param>
        public void ChangeSubstanceBulk(SubstanceType type, float changeInBulk)
        {
            BulkAmounts[type].Amount += changeInBulk;

            // don't receive the event before modifying parent bulk!
            //Parent.BulkChanged -= new Entity.BulkChangedHandler(Parent_BulkChanged);
            Parent.BulkChangedEvent.Remove(parentBulkChangedID);

            Parent.Bulk += changeInBulk;

            // hook up the event again:
            Parent.BulkChangedEvent.Add(parentBulkChangedID, this);

            //Parent.BulkChanged += new Entity.BulkChangedHandler(Parent_BulkChanged);
        }


        /// <summary>
        /// meat can turn into rotten meat...
        /// </summary>
        public void ConvertSubstance(SubstanceType from, SubstanceType to)
        {

        }

        /// <summary>
        /// reduce or increase substance amounts in proportion to the bulk change
        /// </summary>
      /*  public void UpdateBulk(float oldBulk)
        {
            if (BulkAmounts == null)
            {
                InitializeSubstanceAmounts();
            }
            else
            {
                UpdateSubstanceAmountsWithNewBulk(oldBulk);
            }
        }*/

        private void InitializeSubstanceAmounts()
        {
            BulkAmounts = new Dictionary<SubstanceType,SubstanceAmount>();
            SubstanceAmount amount;
            foreach (var item in Parent.EntityType.SubstancesType.FinalSubstanceFractions)
            {
                if (BulkAmounts.TryGetValue(item.Key, out amount))
                {
                    amount.Amount = item.Value * Parent.Bulk;
                }
                else
                {
                    BulkAmounts.Add(item.Key, new SubstanceAmount(item.Value * Parent.Bulk, item.Key));
                }
            }

        }

        private void UpdateSubstanceAmountsWithNewBulk(float oldBulk)
        {
            float percentageIncrease = Entities.Body.Body.GetBulkPercentageIncrease(oldBulk, Parent.Bulk);
                       
            foreach (var item in BulkAmounts.Keys.ToList()) // to allow modifying the collection, the keys are copied
            {
                float oldSubstanceAmount = BulkAmounts[item].Amount;
                float newSubstanceBulk = oldSubstanceAmount * percentageIncrease;

                // clamp/normalize values???

                BulkAmounts[item].Amount = newSubstanceBulk;
            }

        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            parentBulkChangedID = sn.DoMethodID(parentBulkChangedID);
            BulkAmounts = sn.DoDictionary(BulkAmounts);
                

            return this;
        }

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


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            LoadPostProcessRegisterMethodIDs();
        }


        public void LoadPostProcessRegisterMethodIDs()
        {           
            ActionLookup<float>.Add(parentBulkChangedID, Parent_BulkChanged);
        }
    }
}
