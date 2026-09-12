using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Xclna.Xna.Animation;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;//TODO DECOUPLE
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>   
    /// while we are being hit, we will not be interrupted by messages.
    /// </summary>
    public class GoalBeingHit : Goal
    {
  
        public GoalBeingHit(Entity owner)
            : base(owner)
        {
            if (The.Sim.TotalUnPausedGameTimeInSeconds > 24 &&
                entity.ID == (Entities.EntityID)19)
            {

            }
        }

        // this ccould be extended to have lots of fractional count events, 
        // that trigger all sorts of events as the countdown progresses
        public double TotalTimeInSeconds
        {
            get;
            set;
        }

        public override bool IsSame(Jobs.Job job)
        {
            return false;
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

            //No local data to snapshot

            return this;
        }

        public GoalBeingHit()
        {
        }


        protected override void Activate()
        {
            Status = Status.Active;

            TotalTimeInSeconds = 0.5; // one half second //TODO driven by data?
            TimeLeftInSeconds = TotalTimeInSeconds;// start the countdown

            entity.Renderable.SetAnimationActionStateFlag(AnimAction.Recoiling);

            if (entityIntelligence.ThreatStance == ThreatStance.Bold)
            {
                entity.Renderable.SetAnimationStateFlag(AnimModifier.Bold);
            }

         }

   
        protected override void ProcessWhileActive(GameTime elapsed)           
        {
           /* ActivateIfInactive();

            if (Status == Status.Active)
            {*/
                double scalar;
                Status = ProcessCountdown(elapsed, TotalTimeInSeconds, out scalar);

                entity.Renderable.UpdateMainAnimationTimeScalar(scalar);
            /*}

            ExitIfFailedOrCompleted();

            return Status;*/
        }


        public override void OnExit()
        {
            base.OnExit();

            entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Recoiling);
          
            //entity.Renderable.ClearAnimationStateFlag(AnimState.Bold);

        }

     

        public override bool CanReactToInterest()
        {
            return false;
        }

        public override bool HandleMessage(Message message)
        {
            // we are occupied. don't react to messages... 
            return true;
        }
        
    }

}
