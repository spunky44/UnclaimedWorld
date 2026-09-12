using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Xclna.Xna.Animation;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Items;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Biological;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots; 
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// Collapse as unconscious or dead...
    /// while we are collapsing, we will not be interrupted by messages.
    /// 
    /// This goal will be followed by either:
    /// GoalIsDying (as a sibling goal, not as a subgoal)
    /// or Kill() being called on the entity.
    /// </summary>
    public class GoalCollapse : CompositeGoal
    {
        
        bool isDone;

        OwnerID? ownerOfCarcass;
        EntityID? killer;


        public GoalCollapse(Entity entity, OwnerID? ownerOfCarcass, EntityID? killer = null)
            : base(entity)
        {
            this.ownerOfCarcass = ownerOfCarcass;
            this.killer = killer;

        }

       

        public GoalCollapse()
        {
        }



        protected override void Activate()
        {
            Status = Status.Active;

            // immediately drop anything we may be holding or carrying:
            if (entity.AgentStorage != null)
            {
                entity.AgentStorage.IterateContained(e => entity.AgentStorage.Uncontain(e));
            }

            /* if (entity.AgentStorage != null && entity.AgentStorage.MountedToolOrWeapon != null)
             {                
                 Entity weaponOrTool = Entity.FindByID(entity.AgentStorage.MountedToolOrWeapon.Value);
                 if (weaponOrTool != null)
                 {
                     entity.AgentStorage.Uncontain(weaponOrTool);
                 }                
             }*/

            // HACK:
            // ignore collisions with sleepers...
            // this is a temporary fix because the pathfinder cannot see and avoid them. When we get footprints from entities implemented, delete this.
            EnableCollisions(false);

            if (entity.HasStance())
            {
                entity.Locomotor.Stance.CurrentStance = entity.EntityType.LocomotorType.StancesType.IncapacitatedStanceType; // LeggedLocomotor.Stance.Lying;
            }

            entityIntelligence.IsAwakeAndActive = false;


            if (entity.Intelligence.ThreatStance == ThreatStance.Bold)
            {
                AddSubgoal(new GoalWait(entity, 2, AnimAction.Dying, AnimModifier.Pre, AnimModifier.Bold, true));
            }
            else
            {
                AddSubgoal(new GoalWait(entity, 2, AnimAction.Dying, AnimModifier.Pre, true));  
            }

        }

        public override bool CanReactToInterest()
        {
            return false;
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //process the subgoals               
            if (ProcessSubgoals(elapsed) == Goals.Status.Completed)
            {
                // see if we're dead

                bool isDead;
                bool isUnconscious;
                CauseOfDeath? causeOfDeath;
                CauseOfUnconsciousness? causeOfUnconsciousness;

                entity.GetStatus(out isDead, out isUnconscious, out causeOfDeath, out causeOfUnconsciousness);

                //  isDead = false; // TESTING!!!

                if (isDead)
                {
                    // was dead when hitting the ground.
                    entity.Kill(ownerOfCarcass, causeOfDeath, killer);
                }
                else
                {
                    Status = Goals.Status.Completed; // Not dead yet. This goal is Done. The GoalIsDying has been queued up and will execute now.
                }
            }

        }


       
       /* void anim_AnimationEnded(object sender, EventArgs e)
        {
            AnimationController animController = (AnimationController)sender;
            animController.AnimationEnded -= new EventHandler(anim_AnimationEnded);
            
            isDone = true;
                      
        }*/

        public override Goal.DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
        {
            return DetectionFactor.CannotDetect;
        }


        public override Goal.DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
        {
            return DetectionFactor.CannotDetect;
        }

        /*
        public override Status Process(Microsoft.Xna.Framework.GameTime elapsed)
        {
            ActivateIfInactive();

            if (isDone)
            {
                // see if we're dead
               
                bool isDead;
                bool isUnconscious;
                BiologicalEntity.CauseOfDeath? causeOfDeath;
                BiologicalEntity.CauseOfUnconsciousness? causeOfUnconsciousness;

                entity.GetStatus(out isDead, out isUnconscious, out causeOfDeath, out causeOfUnconsciousness);

                if (isDead)
                {
                    entity.Kill(ownerOfCarcass);
                }               
               
                // (else execute GoalIsDying)

                Status = Status.Completed;
            }

            return Status;
        }
        */

        public override void OnExit()
        {
            base.OnExit();

            //entityIntelligence.IsAwakeAndActive = true; // presently we cannot be revived again...
        }

    

        public override bool HandleMessage(Message message)
        {
            // we are dying/unconscious. don't react to messages...
            return true;
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

            this.isDone = sn.DoBool(isDone);
            this.ownerOfCarcass = sn.DoEnumNullable(ownerOfCarcass);
            this.killer = sn.DoEntityIDNullable(killer);

            return this;
        }
    }

}
