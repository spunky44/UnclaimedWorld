using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Needs
{
    public class PhysicalNeed : ISnapshot
    {
        public Need Parent;

        /// <summary>
        /// this counter is increased while we are in Hunger state (=0 Food Level)
        /// and slowly decreased when we are not.
        /// </summary>
        public float DaysAtZero;

      

        public PhysicalNeed()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public PhysicalNeed(Need parent)
        {
            this.Parent = parent;
        }




        public void Update(double deltaTimeInSeconds)
        {                      

        //    FoodLevel = Common.DecreaseValueBetweenZeroAndOne(foodLevelDecreaseFactor * FoodLevel, FoodLevelDecreasePerDay, deltaTimeInSeconds);

            // TODO: manage weight loss/gain:

            if (Parent.NeedType.PhysicalEffects.DaysAtZeroDecreaseFactor.HasValue)
            {
                // manage starvation effects
                if (Common.IsZero(Parent.CurrentLevel))
                {
                    DaysAtZero += (float)(deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond);

                }
                else
                {
                    if (DaysAtZero > 0f)
                    {

                        DaysAtZero = DaysAtZero - Parent.NeedType.PhysicalEffects.DaysAtZeroDecreaseFactor.Value * (float)(deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond);

                        DaysAtZero = Common.ClampBottom(DaysAtZero, 0f);
                    }
                }
            }

        }

        public bool IsStarvedToDeath()
        {
            if (Parent.NeedType.PhysicalEffects.DaysAtZeroCausingDeath.HasValue)
            {
                return DaysAtZero > Parent.NeedType.PhysicalEffects.DaysAtZeroCausingDeath; // StarvationDaysCausingDeath;
            }
            else return false;
        }

        public bool IsCollapsedFromStarvation()
        {
            if (Parent.NeedType.PhysicalEffects.DaysAtZeroCausingCollapse.HasValue)
            {
                return DaysAtZero > Parent.NeedType.PhysicalEffects.DaysAtZeroCausingCollapse;
            }
            else return false;
        }

        /// <summary>
        /// 0 - 1
        /// 0: starved near death
        /// 1: not starving
        /// </summary>
        /// <returns></returns>
        public float GetStarvedToDeathFraction()
        {
            if (Parent.NeedType.PhysicalEffects.DaysAtZeroCausingDeath.HasValue)
            {
                float value = 1f - (DaysAtZero / Parent.NeedType.PhysicalEffects.DaysAtZeroCausingDeath.Value);

                value = Common.Clamp(value, 0f, 1f);

                return value;
            }
            else return 1f;
        }

        public float GetCollapsedFraction()
        {
            if (Parent.NeedType.PhysicalEffects.DaysAtZeroCausingCollapse.HasValue)
            {
                float value = 1 - (DaysAtZero / Parent.NeedType.PhysicalEffects.DaysAtZeroCausingCollapse.Value);

                value = Common.Clamp(value, 0f, 1f);

                return value;
            }
            else return 1f;
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
            this.DaysAtZero = sn.DoFloat(DaysAtZero);
           
            
            sn.Ignore(Parent); // is summarily assigned in Need Post Load

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
