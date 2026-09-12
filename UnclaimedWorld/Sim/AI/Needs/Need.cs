using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Needs
{
    public class Need : ISnapshot
    {
        public NeedType NeedType;
        string snapshotNeedTypeKey;

        public PhysicalNeed PhysicalNeed;

        public FoodNeed FoodNeed;

        public float DecreasePerDay;

        public Needs Parent;

      //  public float NormalizedDecreasedEnergyWeight;

        private float currentLevel = 1f;
        /// <summary>
        /// 0 - 1
        /// </summary>
        public float CurrentLevel
        {
            get
            {
                return currentLevel;
            }
            set
            {
                value = Common.Clamp(value, 0f, 1f);
                if (currentLevel != value)
                {
                    currentLevel = value;

                    Parent.SetNeedsDirty();

                    if (FoodNeed != null)
                    {
                        FoodNeed.SetNeedDirty();
                    }
                }
            }
        }

        /// <summary>
        /// TODO: delete this
        /// </summary>
        public bool CurrentLevelIsDirty = true;

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
            this.currentLevel = sn.DoFloat(currentLevel);
            this.CurrentLevelIsDirty = sn.DoBool(CurrentLevelIsDirty);
            this.DecreasePerDay = sn.DoFloat(DecreasePerDay);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotNeedTypeKey = NeedType.KeyName;
            }
            this.snapshotNeedTypeKey = sn.DoString(snapshotNeedTypeKey);
                       
            
            //this.NormalizedDecreasedEnergyWeight = sn.DoFloat(NormalizedDecreasedEnergyWeight);
          
            this.PhysicalNeed = (PhysicalNeed)sn.DoISnapshot(PhysicalNeed);
            this.FoodNeed = (FoodNeed)sn.DoISnapshot(FoodNeed);


            sn.Ignore(Parent); // is summarily assigned in PostLoad
            sn.Ignore(NeedType);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            NeedType = Parent.Parent.AgeGroup.AgeGroupType.NeedTypes.First(n => n.KeyName == snapshotNeedTypeKey);

            if (PhysicalNeed != null)
            {
                PhysicalNeed.Parent = this;

                PhysicalNeed.LoadPostProcess(sn);
            }

            if (FoodNeed != null)
            {
                FoodNeed.Parent = this;

                FoodNeed.LoadPostProcess(sn);
            }
        }

        public Need()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public Need(Needs parent, NeedType needType)
        {
            this.Parent = parent;
            this.NeedType = needType;

            if (needType.PhysicalEffects != null)
            {
                this.PhysicalNeed = new PhysicalNeed(this); 
            }

            if (needType.FoodNeedType != null)
            {
                this.FoodNeed = new FoodNeed(this);
            }
        }

        public float? GetEnergyFactor()
        {
            return NeedType.GetEnergyFactor(CurrentLevel);
        }

       

        public void Satisfy(float amount)
        {
            CurrentLevel += amount;
        }

        public void Update(double deltaTimeInSeconds)
        {
            float nutrientLevelDecreaseFactor = 1f;
            if (PhysicalNeed != null && NeedType.PhysicalEffects.UseExertionFactorToDecrease)
            {
                // modify uptake with activity level:
                nutrientLevelDecreaseFactor = Parent.Parent.Parent.Intelligence.Brain.GetExertionLevelOfActivity();
            }

            CurrentLevel = Common.DecreaseValueBetweenZeroAndOne(CurrentLevel, nutrientLevelDecreaseFactor * DecreasePerDay, deltaTimeInSeconds);

            if (PhysicalNeed != null)
            {
                PhysicalNeed.Update(deltaTimeInSeconds);
            }

        }


        /// <summary>
        /// returns a number from 0 - 1 by combining current level and starvation, scaled in the following way:
        /// 1: full
        /// 0.5: empty, but 0 starvation
        /// 0: starved near death
        /// 
        /// if not a phsyical need, returns the current level.
        /// </summary>
        /// <param name="needToGetResultFrom"></param>
        /// <param name="getterKnowledge"></param>
        /// <returns></returns>
        public float GetWeightedStatus()
        {
            if (PhysicalNeed != null)
            {
                return (0.5f * CurrentLevel) + 0.5f * PhysicalNeed.GetStarvedToDeathFraction();

            }
            else return CurrentLevel;
        }
    }
}
