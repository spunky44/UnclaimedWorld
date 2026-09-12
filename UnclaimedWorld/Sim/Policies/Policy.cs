using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Policies
{
    public class AllegiancePolicy: ISnapshot
    {
        /// <summary>
        /// belongs in Allegiance together with ThreatJobManager
        /// </summary>
        public enum CreaturePolicy { NeverAttack, Defend, HuntForProducts, HuntToDestroy };
        public Dictionary<EntityType, CreaturePolicy> PolicyTowardsCreatures = new Dictionary<EntityType, CreaturePolicy>();

        /// <summary>
        /// the key is prevented to be used in all products in the list.
        /// </summary>
        public Dictionary<EntityType, List<EntityType>> DoNotUseInProduct = new Dictionary<EntityType, List<EntityType>>();

       
        /*
         /// <summary>
        /// Creates a Policy from a PolicyData
        /// </summary>
        public static AllegiancePolicy CreateFromPolicyData(AllegiancePolicyData policyData)
        {


        }*/


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
           
            //HasChasedHuntedCritter = sn.DoDictionary(HasChasedHuntedCritter);

            //FractionIndependentsAllowedToSleep = sn.DoFloatNullable(FractionIndependentsAllowedToSleep);
           // IndependentsToStayAwake = sn.DoInt32Nullable(IndependentsToStayAwake);


            sn.Postpone(PolicyTowardsCreatures);
            sn.Postpone(DoNotUseInProduct);
           
            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);



        }

        #endregion
    }
}
