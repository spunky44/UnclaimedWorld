using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using System.Diagnostics;
using UWGame.SimSide.Entities.Locomotors;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Locomotors.Stances;
using System.Linq;
namespace UWGame.SimSide.AI.Goals
{

   
    /// <summary>
    /// contains a little copied code from GoalWait to control the animation
    /// </summary>
    public class GoalChangeStance: Goal
    {

        private double maxTimeToWait;

        public AnimAction? ActionStateToSet;
        public List<AnimModifier> ModifierStatesToSet;

        bool scaleAnimationToFillWaitPeriod;

        StanceType stanceToTake;

        public GoalChangeStance(Entity entity, StanceType stanceToTake)
            : base(entity) 
        {
            this.stanceToTake = stanceToTake;

            // Always set scaleanim = true if the anim is not supposed to loop! This will avoid twitching/freezing at the end if the animation/wait times do not match up...
            this.scaleAnimationToFillWaitPeriod = true;
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

      

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.maxTimeToWait = sn.DoDouble(maxTimeToWait);
            this.ActionStateToSet = sn.DoEnumNullable(ActionStateToSet);
            this.ModifierStatesToSet = sn.DoList(ModifierStatesToSet);
            this.scaleAnimationToFillWaitPeriod = sn.DoBool(scaleAnimationToFillWaitPeriod);
            this.stanceToTake = sn.DoGameData(stanceToTake); // sn.DoEnum(stanceToTake);

            return this;
        }

        public GoalChangeStance()
        {
        }


        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }

        protected override void Activate()
        {
            
            //LeggedLocomotor leggedLocomotor = null;
            Stance stance = null;
            Locomotor locomotor;
            if (entity.Find(out locomotor))
            {
                if (locomotor != null && locomotor.Stance != null)
                {
                    stance = locomotor.Stance;
                }
                else
                {
                    Status = Goals.Status.Completed;
                    return;
                }
            }

            StanceType currentStance = stance.CurrentStance;

            if (currentStance == stanceToTake)
            {
                Status = Goals.Status.Completed;
                return;
            }


            double? duration;
            AnimModifier? currentAnimStance;
            AnimModifier? animStanceToTake;
            bool reverse;

            GetChangeStanceAnim(entity.EntityType.LocomotorType.StancesType, stanceToTake, currentStance, out currentAnimStance, out animStanceToTake, out reverse, out duration);

            if (duration.HasValue)
            {
                ModifierStatesToSet = new List<AnimModifier>();

                if (currentAnimStance.HasValue)
                {
                    ModifierStatesToSet.Add(currentAnimStance.Value);
                }

                if (animStanceToTake.HasValue)
                {
                    ModifierStatesToSet.Add(animStanceToTake.Value);
                }

                if (reverse)
                {
                    ModifierStatesToSet.Add(AnimModifier.Reverse);
                }
             
                
                TimeLeftInSeconds = duration.Value;
                maxTimeToWait = duration.Value;
                                
                foreach (AnimModifier state in ModifierStatesToSet)
                {
                    entity.Renderable.SetAnimationStateFlag(state);
                }

                ActionStateToSet = AnimAction.ChangingStance;
                entity.Renderable.SetAnimationActionStateFlag(ActionStateToSet.Value);


                Status = Status.Active;

            }
            else
            {
                // no anim specified. set the sim stance now:
                //leggedLocomotor.CurrentStance = stanceToTake;
                stance.CurrentStance = stanceToTake;

                // done..
                Status = Goals.Status.Completed;
            }

        }

      
              
               
        /*
        private void SetStanceAndAnimFlags(List<AnimModifier> statesToTake, List<AnimModifier> statesToClear)
        {
            if (entity.Locomotor.LeggedLocomotor != null)
            {
                entity.Locomotor.LeggedLocomotor.CurrentStance = stanceToTake;
            }

            foreach (var item in statesToTake)
            {
                entity.Renderable.SetAnimationStateFlag(item);
            }

            foreach (var item in statesToClear)
            {
                entity.Renderable.ClearAnimationStateFlag(item);
            }

        }*/

       
        private static void GetChangeStanceAnim(StancesType stancesType, StanceType stanceToTake, StanceType currentStance, 
            out AnimModifier? currentAnimStance, out AnimModifier? animStanceToTake, out bool reverse, out double? duration)
        {
            // find the transition we need.
            // we cannot use flag order to specify direction. Instead we use the Reverse flag and define the
            //default direction as "get up". so, sitting down needs "reverse flag".

            duration = null; // required for animation?

            animStanceToTake = stanceToTake.AnimModifier;
            currentAnimStance = currentStance.AnimModifier;

            if (stanceToTake.Number < currentStance.Number)
            {
                reverse = true;
            }
            else reverse = false;

            if (stancesType.StanceChangeDurations != null)
            {
                Dictionary<StanceType, double> innerDict;
                if (stancesType.StanceChangeDurationsMapping.TryGetValue(currentStance, out innerDict))
                {
                    double durationValue;
                    if (innerDict.TryGetValue(stanceToTake, 
                        out durationValue))
                    {
                        duration = durationValue;
                    }
                }
            }

            return;

            /*

            reverse = false;

            currentAnimStance = null;
            animStanceToTake = null;

            duration = null;

            switch (currentStance)
            {
                case LeggedLocomotor.Stance.Lying: // Number: 0

                    currentAnimStance = AnimModifier.Lying;

                    switch (stanceToTake)
                    {

                        case LeggedLocomotor.Stance.Sitting:
                            // sleepToSitting 80 3.2
                            duration = 3.2;
                            animStanceToTake = AnimModifier.Sitting;
                            break;

                        case LeggedLocomotor.Stance.Kneeling:
                            animStanceToTake = AnimModifier.Kneeling;
                            break;

                        case LeggedLocomotor.Stance.Standing:
                            animStanceToTake = null;
                            break;

                    }
                    break;

                case LeggedLocomotor.Stance.Sitting: // Number: 1

                    currentAnimStance = AnimModifier.Sitting;

                    switch (stanceToTake)
                    {
                        case LeggedLocomotor.Stance.Lying:
                            animStanceToTake = AnimModifier.Lying;
                            reverse = true;
                            break;

                        case LeggedLocomotor.Stance.Kneeling:
                            animStanceToTake = AnimModifier.Kneeling;
                            break;

                        case LeggedLocomotor.Stance.Standing:
                            // sittingToIdle 36 1.44
                            duration = 1.44;
                            animStanceToTake = null;
                            break;
                    }
                    break;

                case LeggedLocomotor.Stance.Kneeling: // Number: 2

                    currentAnimStance = AnimModifier.Kneeling;

                    switch (stanceToTake)
                    {
                        case LeggedLocomotor.Stance.Lying:
                            reverse = true;
                            animStanceToTake = AnimModifier.Lying;
                            break;

                        case LeggedLocomotor.Stance.Sitting:
                            // kneelToSitting 36 1.44
                            duration = 1.44;
                            reverse = true;
                            animStanceToTake = AnimModifier.Sitting;
                            break;

                        case LeggedLocomotor.Stance.Standing:
                            // kneelToIdle 25 1
                            duration = 1;
                            animStanceToTake = null;
                            break;

                    }
                    break;

                case LeggedLocomotor.Stance.Standing: // Number: 3

                    currentAnimStance = null;
                    reverse = true;

                    switch (stanceToTake)
                    {
                        case LeggedLocomotor.Stance.Lying:
                            // idleToSleep 136 5.44
                            duration = 5.44;
                            animStanceToTake = AnimModifier.Lying;
                            break;

                        case LeggedLocomotor.Stance.Sitting:
                            // idleToSitting 36 1.44
                            duration = 1.44;
                            animStanceToTake = AnimModifier.Sitting;
                            break;

                        case LeggedLocomotor.Stance.Kneeling:
                            //"idleToKneel" 25 1
                            duration = 1;
                            animStanceToTake = AnimModifier.Kneeling;
                            break;
                    }

                    break;
            }*/
        }

        /*

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stanceToTake"></param>
        /// <param name="currentStance"></param>
        /// <param name="currentAnimStance"></param>
        /// <param name="animStanceToTake"></param>
        /// <param name="reverse"></param>
        /// <param name="duration"></param>
        private static void GetChangeStanceAnim(StanceType stanceToTake, StanceType currentStance, out AnimModifier? currentAnimStance, out AnimModifier? animStanceToTake, out bool reverse, out double? duration)
        {
            // find the transition we need.
            // we cannot use flag order to specify direction. Instead we use the Reverse flag and define the
            //default direction as "get up". so, sitting down needs "reverse flag".

            reverse = false;

            currentAnimStance = null;
            animStanceToTake = null;

            duration = null;

            switch (currentStance)
            {
                case LeggedLocomotor.Stance.Lying: // Number: 0

                    currentAnimStance = AnimModifier.Lying;

                    switch (stanceToTake)
                    {

                        case LeggedLocomotor.Stance.Sitting:
                            // sleepToSitting 80 3.2
                            duration = 3.2;
                            animStanceToTake = AnimModifier.Sitting;
                            break;

                        case LeggedLocomotor.Stance.Kneeling:
                            animStanceToTake = AnimModifier.Kneeling;
                            break;

                        case LeggedLocomotor.Stance.Standing:
                            animStanceToTake = null;
                            break;

                    }
                    break;



                case LeggedLocomotor.Stance.Sitting: // Number: 1

                    currentAnimStance = AnimModifier.Sitting;

                    switch (stanceToTake)
                    {
                        case LeggedLocomotor.Stance.Lying:
                            animStanceToTake = AnimModifier.Lying;
                            reverse = true;
                            break;

                        case LeggedLocomotor.Stance.Kneeling:
                            animStanceToTake = AnimModifier.Kneeling;
                            break;

                        case LeggedLocomotor.Stance.Standing:
                            // sittingToIdle 36 1.44
                            duration = 1.44;
                            animStanceToTake = null;
                            break;
                    }
                    break;

                case LeggedLocomotor.Stance.Kneeling: // Number: 2

                    currentAnimStance = AnimModifier.Kneeling;

                    switch (stanceToTake)
                    {
                        case LeggedLocomotor.Stance.Lying:
                            reverse = true;
                            animStanceToTake = AnimModifier.Lying;
                            break;

                        case LeggedLocomotor.Stance.Sitting:
                            // kneelToSitting 36 1.44
                            duration = 1.44;
                            reverse = true;
                            animStanceToTake = AnimModifier.Sitting;
                            break;

                        case LeggedLocomotor.Stance.Standing:
                            // kneelToIdle 25 1
                            duration = 1;
                            animStanceToTake = null;
                            break;

                    }
                    break;

                case LeggedLocomotor.Stance.Standing: // Number: 3

                    currentAnimStance = null;
                    reverse = true;

                    switch (stanceToTake)
                    {
                        case LeggedLocomotor.Stance.Lying:
                            // idleToSleep 136 5.44
                            duration = 5.44;
                            animStanceToTake = AnimModifier.Lying;
                            break;

                        case LeggedLocomotor.Stance.Sitting:
                            // idleToSitting 36 1.44
                            duration = 1.44;
                            animStanceToTake = AnimModifier.Sitting;
                            break;

                        case LeggedLocomotor.Stance.Kneeling:
                            //"idleToKneel" 25 1
                            duration = 1;
                            animStanceToTake = AnimModifier.Kneeling;
                            break;

                    }

                    break;


            }
        }*/

        /// <summary>
        /// Always called BEFORE the OnEnter of a next subgoal of the same parent
        /// </summary>
        public override void OnExit()
        {
            
            base.OnExit();
            
            // clear the anim states:
            if (ModifierStatesToSet != null || ActionStateToSet != null)
            {                
                foreach (AnimModifier state in ModifierStatesToSet)
                {
                    entity.Renderable.ClearAnimationStateFlag(state);
                }

                if (ActionStateToSet.HasValue)
                {
                    entity.Renderable.ClearAnimationActionStateFlag(ActionStateToSet.Value);
                }                
            }


            if (entity.HasStance()) // entity.Locomotor.Stance != null)
            {
                entity.Locomotor.Stance.CurrentStance = stanceToTake;
            }

            // get the flags to set/clear once the anim has completed (this is when we are actually sitting, standing, kneeling or whatever)
            List<AnimModifier> statesToTake = new List<AnimModifier>();
            List<AnimModifier> statesToClear = new List<AnimModifier>();

            AnimModifier? animFlag = stanceToTake.AnimModifier; // LeggedLocomotor.GetCorrespondingAnimFlag(stanceToTake);
            if (animFlag.HasValue)
            {
                statesToTake.Add(animFlag.Value);
            }
            Renderable.GetExcludedStanceFlags(animFlag, statesToClear);

            foreach (var item in statesToTake)
            {
                entity.Renderable.SetAnimationStateFlag(item);
            }

            foreach (var item in statesToClear)
            {
                entity.Renderable.ClearAnimationStateFlag(item);
            }

        }


        protected override void ProcessWhileActive(GameTime elapsed)
        {

            double scalar;
            Status = ProcessCountdown(elapsed, maxTimeToWait, out scalar);

            if (scaleAnimationToFillWaitPeriod)
            {
                // we may be running other anims at the moment, such as a gait anim. In that case, the animation is not progressed.
                // we may still be running the previous stance change anim! detect this case and ignore the call!
                entity.Renderable.UpdateMainAnimationTimeScalar(scalar);
            }


        }  


    }
}
