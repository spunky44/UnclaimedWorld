using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.ClientSide;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.Entities.Body
{
    

    /// <summary>
    /// not a component - has a wrapper classs that serves that purpose
    /// </summary>
    public class Body : IHasBodyParts, ISnapshot //, ILookUp<Body, BodyID>
    {
        /// <summary>
        /// is null for Entity!
        /// </summary>       
        public MemoryFact ParentMemoryFact;
        MemoryFactID? snapshotParentMemoryFact;

        /// <summary>
        /// is null for MemoryFact!
        /// </summary>
        public Entity Parent;
        EntityID? snapshotParentEntity;

        /// <summary>
        /// should be lower than the sum of body parts' hp
        /// </summary>
        public float MaxHitpoints;
      
        /// <summary>
        /// a caching of the current state of the various body parts, to be used in evaluators
        /// </summary>
        public double FunctionalScore = 1.0;

        private float globalHitpoints;

       
        /// <summary>
        /// keeps track of the lowest hitpoints fraction we have ever had
        /// </summary>
        private float lowestHitpointsFraction;
     
       // public event EventHandler HitpointsChanged; // easier to push changes than snapshot event


        private List<BodyPart> bodyParts = new List<BodyPart>();
        public List<BodyPart> BodyParts
        {
            get { return bodyParts; }
            set { bodyParts = value; }
        }

        public Body(Entity parent)
        {
            Parent = parent;
            lowestHitpointsFraction = 1f;

        }

        public Body()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }
      

        /// <summary>
        /// Clone the body for memoryfact
        /// </summary>
        /// <param name="original"></param>
        public Body(Body original) //: base(original.Parent)
        {
            MaxHitpoints = original.MaxHitpoints;
            FunctionalScore = original.FunctionalScore;

            globalHitpoints = original.globalHitpoints;

            CopyBodyParts(bodyParts, original.bodyParts);
            //Parent = original.Parent;
        }

       

        private void CopyBodyParts(List<BodyPart> copiedBodyParts, List<BodyPart> originalBodyParts)
        {
            foreach (var originalBodyPart in originalBodyParts)
            {
               // BodyPart copiedBodyPart = new BodyPart(originalBodyPart);
                BodyPart copiedBodyPart;
                if (originalBodyPart is BiologicalBodyPart)
                {
                    copiedBodyPart = new BiologicalBodyPart(originalBodyPart);
                }
                else
                {
                    copiedBodyPart = new MachineBodyPart(originalBodyPart);
                }

                copiedBodyPart.Body = this;

                bodyParts.Add(copiedBodyPart);

                if (originalBodyPart.BodyParts != null)
                {
                    copiedBodyPart.BodyParts = new List<BodyPart>();
                    CopyBodyParts(copiedBodyPart.BodyParts, originalBodyPart.BodyParts);
                }
            }
        }



        /// <summary>
        /// 0 is bad, 1 is good
        /// </summary>
        /// <returns></returns>
        public float GetHurtVitalBodyPartFactorForMorale()
        {
            float hurtVitalBodyPartFactor = 1.0f;
            foreach(BodyPart bodyPart in bodyParts)
            {
                bodyPart.GetHurtVitalBodyPartFactor(ref hurtVitalBodyPartFactor);
            }
            return hurtVitalBodyPartFactor;
        }

        public float GetModifiedHitpointsForPresentation()
        {
            float currentHitpoints = 0f;
            float totalMaxHitpoints = 0f; // the sum is greater than the Body.MaxHitpoints
            foreach (BodyPart bodyPart in bodyParts)
            {
                bodyPart.GetModifiedHitpointsForPresentation(ref currentHitpoints, ref totalMaxHitpoints);
            }

            return Common.Clamp(currentHitpoints / totalMaxHitpoints /* MaxHitpoints*/, 0f, 1f);
        }

        private float HitpointsFractionLeft
        {
            get
            {
                return GlobalHitpoints / MaxHitpoints;
            }
        }

       

       /// <summary>
       /// current hitpoints
       /// </summary>
        public float GlobalHitpoints
        {
            get { return globalHitpoints; }
            set 
            {
                if (value != globalHitpoints)
                {
                    globalHitpoints = value;
                    /*MaxHitpoints = ComputeHitpointsFromBulk(Parent.Bulk);
                    GlobalHitpoints = MaxHitpoints;

                    //GlobalHitpointsCausingUnconsciousness = GameData.Instance.Constants.FractionOfHitpointsCausingCollapse * GlobalHitpoints;

                    InitializeBodyPartHitpoints();*/

                    lowestHitpointsFraction = Math.Min(HitpointsFractionLeft, lowestHitpointsFraction);

                    UpdateRenderableHitpoints();


                    // push the change instead of using an event that we have to snapshot:
                    if (Parent != null
                        && Parent.EntityType.IntelligenceType != null)
                    {
                        Parent.Intelligence.SetPanicLevelDirty();
                    }

                   /* if (HitpointsChanged != null)
                    {
                        // notify listeners of change
                        HitpointsChanged(this, null);
                    }*/

                  /*  Intelligence intelligence;
                    if (Parent.Find(out intelligence))
                    {
                        intelligence.ResetThreatStance();
                    }*/
                }
            }

        }

        public void UpdateRenderable()
        {
            UpdateRenderableHitpoints();
        }

        private void UpdateRenderableHitpoints()
        {
            if (Parent.Renderable != null)
            {
                if (HitpointsFractionLeft < 0.5f)
                {
                    Parent.Renderable.SetAnimationStateFlag(ClientSide.Renderables.AnimModifier.Damaged);
                }
                else
                {
                    Parent.Renderable.ClearAnimationStateFlag(ClientSide.Renderables.AnimModifier.Damaged);
                }
            }
        }

        

        public void RegainHitpoints(double deltaTimeInSeconds)
        {
            if (Parent.Name != null && Parent.Name.Contains("Conlan"))
            {
                int i = 0;
            }
            if (GlobalHitpoints < MaxHitpoints)
            {
                BiologicalEntity bioEntity = Parent.BiologicalEntity;

                // calculate the limit to what we can regain
                float regainLimit = lowestHitpointsFraction + (1f - lowestHitpointsFraction) * bioEntity.MaxRegainLimit; 

                if (HitpointsFractionLeft < regainLimit)
                {
                    // modify the regen speed by Energy (change this if/when hitpoints become a factor in Energy?)
                    float regenSpeed = Common.ClampBottom(Parent.BiologicalEntity.EnergyLevel, 0.5f) * bioEntity.FractionOfMaxHitpointsGainedPerDay;

                    float totalHitpointsGained = (float)(regenSpeed * MaxHitpoints * deltaTimeInSeconds / The.Sim.DateAndTime.SecondsPerDay);

                    // clamp the value so we don't go over the Max:
                    totalHitpointsGained = Math.Min(totalHitpointsGained, MaxHitpoints - GlobalHitpoints);

                    // increase the bodyparts:
                    RegainInBodyParts(totalHitpointsGained);
                    
                    // some bodyparts will most likely not heal completely even with full regen...

                    GlobalHitpoints += totalHitpointsGained;
                }
            }
        }


        /// <summary>
        /// distribute the gained hitpoints over the bodyparts
        /// </summary>
        /// <param name="totalRegainAmount"></param>
        private void RegainInBodyParts(float totalRegainAmount)
        {
            foreach (BodyPart bodyPart in BodyParts)
            {
                bodyPart.Regain(totalRegainAmount);
            }
        }


        public BodyPart GetRandomBodyPartToHit(BodyPart.AttackDirection attackDirection)
        {
            List<BodyPartChance> bodyParts = new List<BodyPartChance>();

            foreach (BodyPart bodyPart in BodyParts)
            {
                bodyPart.GatherBodyParts(bodyParts, attackDirection);
            }

            float max;
            Common.BuildEdgesFromBucketSizes(bodyParts, true, out max);

            int bodyPartIndex;
            return Common.GetStairStepIndex(bodyParts, out bodyPartIndex,The.Sim.GameplayRandomGenerator, max).BodyPart;

        }


        public void UpdateBulk(float oldBulk) //float bulkIncrease) //float oldBulk)
        {
            if (!Parent.IsDead)
            {
                if (GlobalHitpoints == 0)
                {
                    UpdateHitpoints();
                }
                else
                {
                    UpdateHitpointsWithNewBulk(oldBulk);
                }
            }
        }

        public void ChangeMaxHitpoints(float newMaxHitPointsValue)
        {
            MaxHitpoints = newMaxHitPointsValue;
            UpdateBodyHitpoints();
        }


        public void Initialize()
        {
            if (Parent.EntityType.BodyType.Bulk != null)
            {
                Parent.Bulk = Parent.EntityType.BodyType.Bulk.Value;
            }

        }

        void UpdateBodyHitpoints()
        {
            GlobalHitpoints = MaxHitpoints;

            UpdateBodyPartHitpoints();
        }

        private void UpdateHitpoints()
        {
            float? resilience = null;

            if (Parent.BiologicalEntity != null)
            {
                resilience = Parent.BiologicalEntity.Resilience;
            }

            MaxHitpoints = ComputeHitpoints(this.Parent.EntityType, Parent.Bulk, resilience);

            UpdateBodyHitpoints();
        }

        public static float ComputeHitpoints(EntityType entityType, float bulk, float? resilience)
        {
            if (entityType.BodyType.Hitpoints.HasValue)
            {
                // for robots...
                return entityType.BodyType.Hitpoints.Value;
            }
            else
            {
                // for biologicals...
                return ComputeHitpoints(bulk, resilience.Value);
            }
        }

        public void UpdateBodyPartHitpoints()
        {
            foreach (BodyPart bodyPart in BodyParts)
            {
                bodyPart.InitializeHitpoints();
            }
        }


        private bool IsBodyPartFatallyDamaged()
        {
            foreach (BodyPart bodyPart in BodyParts)
            {
                if (bodyPart.IsBodyPartDamageFatal())
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsBodyPartCausingCollapse()
        {
            foreach (BodyPart bodyPart in BodyParts)
            {
                if (bodyPart.IsBodyPartDamageCausingCollapse())
                {
                    return true;
                }
            }

            return false;
        }

       
        public bool IsDead()
        {
            return GlobalHitpoints <= 0 || IsBodyPartFatallyDamaged(); 
        }

        public void GetStatus(out bool isDead, out bool isUnconscious, 
            ref CauseOfDeath? causeOfDeath, 
            ref CauseOfUnconsciousness? causeOfUnconsciousness)
        {
            isDead = IsDead();
            isUnconscious = false;

         //   causeOfDeath = null;
        //    causeOfUnconsciousness = null;

            if (!isDead)
            {
                isUnconscious = GlobalHitpoints <= GameData.Instance.Constants.FractionOfHitpointsCausingCollapse * MaxHitpoints; // GlobalHitpointsCausingUnconsciousness;

                if (!isUnconscious)
                {   // test vital body parts for low hitpoints:
                    isUnconscious = IsBodyPartCausingCollapse();
                }
            }

            if (isDead)
            {
                causeOfDeath = CauseOfDeath.Wounds;
            }

            if (isUnconscious)
            {
                causeOfUnconsciousness = CauseOfUnconsciousness.Wounds;
            }
        }

        private static float ComputeHitpoints(float bulk, float resilience)
        {
           /* float modifier = 1f;
            BiologicalEntity bioEntity;
            if (Parent.Find(out bioEntity))
            {
                modifier = bioEntity.Resilience;
            }*/

            return 100f * resilience * bulk;
        }

     /*   public void GetBodyParts(ref List<IHasExposedProperties> listToFillWithProperties)
        {
            if (BodyParts != null)
            {
                foreach (BodyPart bodyPart in BodyParts)
                {
                    bodyPart.GetList(ref listToFillWithProperties);
                }
            }
        }*/

        public void GetBodyParts<Type>(ref List<Type> listToFillWithProperties) where Type : IHasExposedProperties 
        {
            if (BodyParts != null)
            {
                foreach (BodyPart bodyPart in BodyParts)
                {
                    bodyPart.GetList<Type>(ref listToFillWithProperties);
                }
            }
        }

        private void UpdateHitpointsWithNewBulk(float oldBulk) //float bulkIncrease)
        {            
            if (GlobalHitpoints == MaxHitpoints) // ComputeHitpointsFromBulk(oldBulk))
            {   
                // still at max...
                UpdateHitpoints();
            }
            else if (!this.Parent.EntityType.BodyType.Hitpoints.HasValue)
            {
                BiologicalEntity bioEntity;
                float resilience = 1f;
                if (Parent.Find(out bioEntity))
                {
                    resilience = bioEntity.Resilience;
                }
                MaxHitpoints = ComputeHitpoints(Parent.Bulk, resilience);

                // make changes in relation to bulk change +/-:
                float percentageIncrease = GetBulkPercentageIncrease(oldBulk, Parent.Bulk);

                GlobalHitpoints = (1f + percentageIncrease) * GlobalHitpoints;

              //  GlobalHitpointsCausingUnconsciousness = GameData.Instance.Constants.FractionOfHitpointsCausingCollapse * GlobalHitpoints;

                // TODO: check for death/collapse here...

                foreach (BodyPart bodyPart in BodyParts)
                {
                    bodyPart.UpdateHitpointsWithNewBulk(percentageIncrease);
                }
            }
        }

        public static float GetBulkPercentageIncrease(float oldBulk, float currentBulk)
        {
            float percentageIncrease = (currentBulk - oldBulk) / oldBulk;
            return percentageIncrease;
        }

        public float GetHitpointsFractionUntilUnconsciousness()
        {
            return GetHitpointsUntilUnonsciousness() / GetMaxHitpointsUntilUnconsciousness();
        }

        private float GetMaxHitpointsUntilUnconsciousness()
        {
            return MaxHitpoints * (1 - GameData.Instance.Constants.FractionOfHitpointsCausingCollapse);
        }

        private float GetHitpointsUntilUnonsciousness()
        {
            float differenceToMaxHitpoints = MaxHitpoints - globalHitpoints;
            return GetMaxHitpointsUntilUnconsciousness() - differenceToMaxHitpoints;
        }

        public BodyPart FindBodyPartOfType(BodyPartType bodyPartType)
        {
            BodyPart foundBodyPart;
            foreach (BodyPart bodyPart in BodyParts)
            {
                foundBodyPart = bodyPart.FindFirstMatchingBodyPart(p => p.BodyPartType == bodyPartType);

                if (foundBodyPart != null)
                {
                    return foundBodyPart;
                }
            }

            return null;
        }
        

        public struct BodyPartChance: IScore, IEdge
        {
            public BodyPart BodyPart;
            public float Score{get; set;}
            public float Edge{get; set;}
        }


        public BodyPart FindBodyPart(BodyPartID bodyPartID)
        {
            BodyPart foundBodyPart;
            foreach (BodyPart bodyPart in BodyParts)
            {
                foundBodyPart = bodyPart.FindFirstMatchingBodyPart(p => p.BodyPartID == bodyPartID);

                if (foundBodyPart != null)
                {
                    return foundBodyPart;
                }
            }

            return null;            
        }

        /*public void GetPresentationInformation(PresentationTypeCategory presentationCategory, Dictionary<PresentationTypeSubCategory, List<PresentationData>> presentationInfoList)
        {
            foreach (BodyPart bodyPart in BodyParts)
            {
                bodyPart.GetPresentationInformation(presentationCategory, presentationInfoList);
            }
        }*/



        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {            
            this.bodyParts = sn.DoList(this.bodyParts);
            this.FunctionalScore = sn.DoDouble(this.FunctionalScore);

            this.globalHitpoints = sn.DoFloat(this.globalHitpoints);
            this.lowestHitpointsFraction = sn.DoFloat(this.lowestHitpointsFraction);
            this.MaxHitpoints = sn.DoFloat(this.MaxHitpoints);

        /*    if (ParentMemoryFact != null)
            {
                snapshotParentMemoryFact = ParentMemoryFact.ID;
                snapshotParentMemoryFact = (MemoryFactID)sn.DoEnumNullable(snapshotParentMemoryFact);
            }*/

            snapshotParentMemoryFact = sn.SnapshotID<MemoryFact, MemoryFactID>(ParentMemoryFact);
            snapshotParentEntity = sn.SnapshotID<Entity, EntityID>(Parent);


            sn.Ignore(Parent);
            sn.Ignore(ParentMemoryFact);

            return this;
        }

        public bool IsSnapshotted { get; set; }


        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public void LoadPostProcess(Snapshotter sn) 
        {

            sn.RegisterLoadPostProcessCall(this);

            if (snapshotParentMemoryFact.HasValue)
                ParentMemoryFact = LookUp<MemoryFact, MemoryFactID>.FindByID(snapshotParentMemoryFact.Value);

            snapshotParentMemoryFact = null; // cleared for next save

            Parent = Entity.FindByID(snapshotParentEntity);

            foreach (var item in bodyParts)
            {
                item.LoadPostProcess(sn);
            }           
        }

        #endregion
    }

}
