using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Xclna.Xna.Animation;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Items;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities.Biological;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;//TODO DECOUPLE
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// we are unconscious and laying prone on the ground...
    /// while we are here, we will not be interrupted by messages.
    /// </summary>
    public class GoalIsDying : Goal
    {
        /* string animationKey;
         Entity targetEntity;
         Playback playback;
         StartingPoint startingPoint;
         bool runToEndIfInterrupted;
         RenderAsModel.Mode mode;
 */
        //AnimationController anim;

        bool isDone;
        OwnerID? ownerOfCarcass;
        private bool takeBleedDamage = false;

        public GoalIsDying(Entity entity, OwnerID? ownerOfCarcass, bool takeBleedDamage)
            : base(entity)
        {
            this.takeBleedDamage = takeBleedDamage;
            this.ownerOfCarcass = ownerOfCarcass;
        }

   

        public GoalIsDying()
        {
        }


        protected override void Activate()
        {
            Status = Status.Active;

           
          
            entityIntelligence.IsAwakeAndActive = false;
                       
         
        }

        /// <summary>
        /// Always called AFTER the OnExit of a previous subgoal of the same parent
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            entity.Renderable.SetAnimationActionStateFlag(AnimAction.Dying);
            entity.Renderable.SetAnimationStateFlag(AnimModifier.Post);
        }

        /// <summary>
        /// Always called BEFORE the OnEnter of a next subgoal of the same parent
        /// </summary>
        public override void OnExit()
        {
            base.OnExit();

            // we are getting up again???
           // EnableCollisions(false);

            entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Dying);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Post);

        }

        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }
        public override bool CanReactToInterest()
        {
            return false;
        }


        Regulator reduceHitpoints;
        
        protected override void ProcessWhileActive(GameTime elapsed)
        {
          /*  ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/

                double millisecondsSinceLastReady = 0;
                if (takeBleedDamage == true)
                {
                    if (reduceHitpoints.IsReady(ref millisecondsSinceLastReady))
                    {
                        float regulatorSeconds = (float)millisecondsSinceLastReady / 1000f;
                        // slowly bleed out...
                        entity.Body.GlobalHitpoints -= (float)(regulatorSeconds * GameData.Instance.Constants.FractionOfMaxHitpointsLostPerSecondWhenDying * entity.Body.MaxHitpoints);

                    }
                }

                // see if we're dead

                bool isDead;
                bool isUnconscious;
                CauseOfDeath? causeOfDeath;
                CauseOfUnconsciousness? causeOfUnconsciousness;



                entity.GetStatus(out isDead, out isUnconscious, out causeOfDeath, out causeOfUnconsciousness);

                if (isDead)
                {
                    entity.Kill(ownerOfCarcass, causeOfDeath);

                    Status = Status.Completed;
                }
        /*    }

            ExitIfFailedOrCompleted();

            return Status;*/
        }

        public override string GetStatus()
        {
            return "Dying";
        }


        protected override void CreateRegulators()
        {
            base.CreateRegulators();

            reduceHitpoints = new Regulator(The.Sim.GameplayRandomGenerator, 1, "GoalIsDyingReduceHitpoints");
        }

        public override bool HandleMessage(Message message)
        {
            // we are dying/unconscious. don't react to messages...
            return true;
        }

        

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

            this.isDone = sn.DoBool(isDone);
            this.ownerOfCarcass = sn.DoEnumNullable(ownerOfCarcass);
            this.takeBleedDamage = sn.DoBool(takeBleedDamage);
        

            return this;
        }

        #endregion
    }

}
