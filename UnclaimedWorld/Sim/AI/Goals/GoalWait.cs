using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Locomotors;
namespace UWGame.SimSide.AI.Goals
{

    /// <summary>
    /// this goal can set anim states on other entities????
    /// </summary>
    public class GoalWait: Goal
    {
     //   double? maxPeriodInSeconds;
     //   double waitProgress = 0f;

        public bool HeadTurnAllowed = true;

        public double? MaxTimeToWait
        {
            get;
            set;
        }

        public AnimAction? ActionStateToSet;
        public List<AnimModifier> ModifierStatesToSet; // generalized to take array or list... may be hard to serialize...

        /// <summary>
        /// i added these callback methods/hooks to make it easier to add small goal functionality to this goal instead of creating new goal classes...
        /// 
        /// Lars: if used, replace with MethodIDs
        /// </summary>
      /*  public Action<GoalWait> ActionOnEnter;
        public Action<GoalWait> ActionOnActivate;
        public Action ActionOnCompletion;*/


        public enum OnExitFlagAction { Leave, Clear }

        /// <summary>
        ///  for anims that run over 2 GoalWaits (like pickup, attack and drop), we don't want the states cleared at the end
        /// </summary>
        private OnExitFlagAction onExitFlagActionValue = OnExitFlagAction.Clear;

        bool scaleAnimationToFillWaitPeriod;


        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }
       // EntityID targetEntityID = EntityID.Invalid;

        /// <summary>
        /// Always set scaleanim = true if the anim is not supposed to loop! This will avoid twitching/freezing at the end if the animation/wait times do not match up...
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="maxPeriodInSeconds"></param>
        /// <param name="scaleAnimationToFillWaitPeriod"></param>
        public GoalWait(Entity owner, double? maxPeriodInSeconds, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear )
            : base(owner) 
        {
          
            if (maxPeriodInSeconds.HasValue)
            {               
                Debug.Assert(Common.IsGreaterThanOrEqual(maxPeriodInSeconds.Value, 0d), "Waiting time should be greater than zero!");

                MaxTimeToWait = maxPeriodInSeconds.Value;

                // new: clamp the wait time to zero
                MaxTimeToWait = Common.ClampBottom(maxPeriodInSeconds.Value, 0); // maxPeriodInMilliSecs.Value * 0.001d; //in milliseconds              

            }
           

            this.scaleAnimationToFillWaitPeriod = scaleAnimationToFillWaitPeriod;
            this.onExitFlagActionValue = onExitAction;

            ModifierStatesToSet = new List<AnimModifier>();
        }

     /*   public GoalWait(Entity owner, double? maxPeriod) //,  EntityID targetID = EntityID.Invalid)
            : this(owner, maxPeriod)
        {
            //targetEntityID = targetID;

        }*/
        public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear) //, EntityID targetID = EntityID.Invalid)
            : this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction)
        {
            ActionStateToSet = actionStateToSet;          
        }

        public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, List<AnimModifier> statesToSet, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear) //, EntityID targetID = EntityID.Invalid)
            : this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction)
        {
            ActionStateToSet = actionStateToSet;
            ModifierStatesToSet = statesToSet;

        }

        public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, AnimModifier state, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear) //, EntityID targetID = EntityID.Invalid)
            : this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction)
        {
            ActionStateToSet = actionStateToSet;
            ModifierStatesToSet.Add(state);

            //targetEntityID = targetID;

        }
        public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, AnimModifier state1, AnimModifier state2, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear) //, EntityID targetID = EntityID.Invalid)
            : this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction)
        {
            ActionStateToSet = actionStateToSet;
            ModifierStatesToSet.Add(state1);
            ModifierStatesToSet.Add(state2);

            //targetEntityID = targetID;

        }
        public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, AnimModifier state1, AnimModifier state2, AnimModifier state3, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear) //, EntityID targetID = EntityID.Invalid)
            : this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction)
        {
            ActionStateToSet = actionStateToSet;
            ModifierStatesToSet.Add(state1);
            ModifierStatesToSet.Add(state2);
            ModifierStatesToSet.Add(state3);

        //    targetEntityID = targetID;

        }

        public GoalWait(Entity owner)
            : base(owner)
        {
        }



        public GoalWait()
        {
        }

        protected override void Activate()
        {
            Status = Status.Active;

            if (MaxTimeToWait.HasValue)
            {
                TimeLeftInSeconds = MaxTimeToWait.Value;// start the countdown
            }


            Entities.Locomotors.Locomotor locomotor;
            if (entity.Find(out locomotor))
            {
                // attempt to stop lerping:
                locomotor.CurrentMoveTarget = entity.PlaySiteLocation;
            }

            // here we can do some simple init stuff...
           /* if (ActionOnActivate != null)
                ActionOnActivate.Invoke(this);*/
        }

     
        /// <summary>
        /// Always called AFTER the OnExit of a previous subgoal of the same parent
        /// 
        /// Called in AddSubgoal!!! - is that good?
        /// </summary>
        public override void OnEnter()
        {            
            base.OnEnter();

            // here we can do some simple init stuff...
           /* if (ActionOnEnter != null)
                ActionOnEnter.Invoke(this);*/


            //if the goal that constructed us as a subgoal specified states to set then set only those
            if (ModifierStatesToSet != null || ActionStateToSet.HasValue)
            {
                if (ModifierStatesToSet != null)
                {
                    foreach (AnimModifier state in ModifierStatesToSet)
                    {
                        entity.Renderable.SetAnimationStateFlag(state);
                    }
                }

                if (ActionStateToSet.HasValue)
                {
                    entity.Renderable.SetAnimationActionStateFlag(ActionStateToSet.Value);
                }
            }
            //otherwise, set the idle state if we are waiting but not aboard a vehicle
            else if (entity.DrivingVehicle == null && entity.PassengerInVehicle == null)
            {
                entity.Renderable.SetAnimationActionStateFlag(AnimAction.Idle);

                /* TODO - make stances data driven
               // don't change stance...                
                Entities.Locomotors.Locomotor locomotor;
                entity.Find(out locomotor);
                                         
                LeggedLocomotor.Stance currentStance = LeggedLocomotor.Stance.Standing;
                if (locomotor != null && locomotor.LeggedLocomotor != null)
                {
                    currentStance = locomotor.LeggedLocomotor.CurrentStance;
                }

                if (currentStance == LeggedLocomotor.Stance.Sitting)
                {
                    entity.Renderable.SetAnimationStateFlag(AnimModifier.Sitting);
                }
                else if (currentStance == LeggedLocomotor.Stance.Kneeling)
                {
                    entity.Renderable.SetAnimationStateFlag(AnimModifier.Kneeling);
                }
                 */
            }                
          
        }

        /// <summary>
        /// Always called BEFORE the OnEnter of a next subgoal of the same parent
        /// </summary>
        public override void OnExit()
        {
            if (entity.Locomotor != null 
                && entity.Locomotor.CollisionResponder != null)
            {
                entity.Locomotor.CollisionResponder.WaitsToGiveRoomToOtherAgent = false;
            }

            base.OnExit();
            
            if (ModifierStatesToSet != null || ActionStateToSet != null)
            {
                if (onExitFlagActionValue == OnExitFlagAction.Clear)
                {
                    if (ModifierStatesToSet != null)
                    {
                        foreach (AnimModifier state in ModifierStatesToSet)
                        {
                            entity.Renderable.ClearAnimationStateFlag(state);
                        }
                    }

                    if (ActionStateToSet.HasValue)
                    {
                        entity.Renderable.ClearAnimationActionStateFlag(ActionStateToSet.Value);
                    }
                }
            }


#if PROFILE || DEBUG
            Tuple<UWGame.SimSide.Sim.WaitingFor, double> entry;
            if (The.Sim.WaitingAgents.TryGetValue(entity.ID, out entry)
                && entry.Item1 == Sim.WaitingFor.Path)
            {
                The.Sim.WaitingAgents.Remove(entity.ID);
            }
#endif


            // this might set the flags that we just cleared, so order matters:
         /*   if (ActionOnCompletion != null)
                ActionOnCompletion.Invoke();*/

         /*   else // MLo, unsure whether the driving or passenger test should be applied before clearing idle, here
                entity.Renderable.ClearAnimationStateFlag(Modifier.Idle);
            */
        }


        protected override void ProcessWhileActive(GameTime elapsed)
        {           

                double scalar;

                if (MaxTimeToWait.HasValue)//Only count down time if the waiting session has an end time
                {
                    Status = ProcessCountdown(elapsed, MaxTimeToWait.Value, out scalar);

                    if (scaleAnimationToFillWaitPeriod)
                    {
                        // scale the animation speed. Be careful, if wrong values are used for character animation, this will result in a horror movie effect
                        // we may be running other anims at the moment, such as a gait anim. In that case, the animation is not progressed.
                        entity.Renderable.UpdateMainAnimationTimeScalar(scalar);
                    }
                    else
                    {
                        // advance the animation in normal time

                    }
                }

                //Apparently GoalWait does not have any relationship to animations per se
                //If it ever does need to control an animation's progress via timer
                //then do it like this:
                //entity.Renderable.UpdateAnimationTimeScalar(scalar);
          
        }


        public override bool CanReactToInterest()
        {
            return HeadTurnAllowed;
        }

        public override bool HandleMessage(Message message)
        {
            switch (message.MessageType)
            {                
                    // not used?!???
                case Message.MessageTypes.GetOff:
                case Message.MessageTypes.HopOnBoard:
                    // Stop waiting!!!
                    Status = Status.Completed;
                    return true;
                case Message.MessageTypes.PathFound:
                case Message.MessageTypes.PathNotFound:
                    // Stop waiting!!!
                    Status = Status.Completed;
                    The.Sim.WaitingAgents.Remove(entity.ID);
                    return false; // handle it higher up

                case Message.MessageTypes.StartGroupMovement: 
                    // TODO: If we are the group leader, only carry on if all members are ready.
                    Status = Status.Completed;
                    return false; // handle it higher up
                case Message.MessageTypes.EndGroupMovement:
                    Status = Status.Completed;
                    return false; // handle it higher up
            }           

            return false;
        }

     /*   protected override double ScoreThisTopLevelGoal()
        {
            return GetCurrentGoalScore();
        }*/



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

            this.HeadTurnAllowed = sn.DoBool(HeadTurnAllowed);
            this.MaxTimeToWait = sn.DoDoubleNullable(MaxTimeToWait);
            this.ActionStateToSet = sn.DoEnumNullable(ActionStateToSet);
            this.ModifierStatesToSet = sn.DoList(ModifierStatesToSet);          
            this.onExitFlagActionValue = sn.DoEnum(onExitFlagActionValue);
            this.scaleAnimationToFillWaitPeriod = sn.DoBool(scaleAnimationToFillWaitPeriod);

            return this;
        }

        #endregion
    }
}
