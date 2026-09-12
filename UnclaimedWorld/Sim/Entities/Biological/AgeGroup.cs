using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using GameStateManagement;
using UWGame.Control;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Biological
{
    public enum AIAgeGroup { Baby = 0, Child = 1, YoungAdult = 2, Adult = 3, Old = 4 };
    public class AgeGroup : ISnapshot
    {
        public AgeGroupType AgeGroupType;
        //int snapshotAgeGroupIndex;

        private float age = 0f;
        public float Age
        {           
            get { return age; }
        }
     
        // public AIAgeGroup EntityAgeGroup;

        public BiologicalEntity Parent;
        EntityID snapshotParent;

        // protected Regulator ageUpdateRegulator = new Regulator(1);


   

        public AgeGroup()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public AgeGroup(BiologicalEntity parent)
        {
            this.Parent = parent;
        }

        public void UpdateAge(double deltaTimeInSeconds) //float addYears)
        {
            float addYears = (float)(deltaTimeInSeconds * The.Sim.DateAndTime.YearsPerSecond);

            age += addYears;

         /*   if (ageUpdateRegulator.IsReady(UWGame.SimSide.Instance.GameTime))
            {*/
                UpdateAgeGroup();
          //  }
        }

        public void SetAge(float age)
        {
            this.age = age;

            UpdateAgeGroup();
        }

        public void SetRandomAge(AIAgeGroup ageGroup)
        {
            //AgeGroupType ageGroupType = Parent.CasteType.AgeGroups.Find(a => a.AIAgeGroup == ageGroup);
            int ageGroupTypeIndex = Parent.CasteType.AgeGroupTypes.FindIndex(a => a.AIAgeGroup == ageGroup);
            AgeGroupType ageGroupType = Parent.CasteType.AgeGroupTypes[ageGroupTypeIndex];

            float lowerBound = (ageGroupTypeIndex > 0 ? Parent.CasteType.AgeGroupTypes[(int)ageGroupTypeIndex - 1].Edge : 0f);
            float ageSpan = Parent.CasteType.AgeGroupTypes[ageGroupTypeIndex].Edge - lowerBound;

            SetAge((float)(The.Sim.GameplayRandomGenerator.NextDouble("AgeGroup") * ageSpan) + lowerBound);
        }

        public static float GetAdultAge(List<AgeGroupType> AgeGroupTypes)
        {
            float lastEdge = 0f;
            foreach (AgeGroupType ageGroupType in AgeGroupTypes)
            {
                if (ageGroupType.AIAgeGroup == AIAgeGroup.Adult)
                {
                    return lastEdge;
                }

                lastEdge = ageGroupType.Edge;
            }

            return Math.Max(0.1f, lastEdge);
        }

        private void UpdateAgeGroup()
        {

           // AIAgeGroup currentAgeGroup = EntityAgeGroup;
            AgeGroupType currentAgeGroup = AgeGroupType;

            int ageGroupIndex;
            AgeGroupType = Common.GetStairStepIndex(Age, Parent.CasteType.AgeGroupTypes, out ageGroupIndex);

          
            // did we change age group?
            if (currentAgeGroup != AgeGroupType) //EntityAgeGroup)
            {
                // update evaluators:
                //Brain

                if (Parent.Parent.PersonEntity != null)
                {
                    Parent.Parent.PersonEntity.UpdateAgeGroup();
                }

                Parent.Needs.UpdateSetOfNeeds();
            }
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
            this.age = sn.DoFloat(age);
            snapshotParent = (EntityID)sn.SnapshotID<Entity, EntityID>(Parent != null ? Parent.Parent : null);
            
           
            sn.Ignore(Parent);
            sn.Ignore(AgeGroupType);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Parent = Entity.FindByID(snapshotParent).BiologicalEntity;

            int ageGroupIndex;
            AgeGroupType = Common.GetStairStepIndex(Age, Parent.CasteType.AgeGroupTypes, out ageGroupIndex);
          
        }

        #endregion
    }
}
