using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Goals;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// only the head of the household should do this
    /// </summary>
    public class GoalDoMoveInToNewHome: Goal
    {
        EntityID newHome;

        public GoalDoMoveInToNewHome(Entity owner, EntityID newHome)
            : base(owner)
        {
            this.newHome = newHome;
           
        }


        public GoalDoMoveInToNewHome()
        {
        }


        protected override void Activate()
        {
            Status = Status.Active;

            IKnownEntityData homeData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(newHome, out homeData)))
            {
                return;
            }

            // if the home is seen to exist, we can 'move in' directly. otherwise, fail
            Entity homeEntity = homeData as Entity;
            if (homeEntity != null)
            {
                IResidence residence = homeEntity.Contains as IResidence;
                /*if (residenceComponent != null homeEntity.Find(out residenceComponent))
                {*/
                if (residence.Residence.Residents >= homeEntity.EntityType.ContainerType.ResidenceType.LivingCapacity)
                    {
                        // no room... include family members here...?                        
                        Status = Goals.Status.Failed;
                        return;
                    }
                    else
                    {
                        if (!GoalMoveInToNewHome.MoveInToNewHome(personEntity, homeEntity))
                        {
                            Status = Goals.Status.Failed;
                            return;
                        }

                        Status = Goals.Status.Completed;
                    }
               // }
            }
            else
            {
                Status = Goals.Status.Failed;
            }
          
        }
        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {

            //process the subgoals
            //Status = ProcessSubgoals(elapsed);

         /*   if (Status == Status.Completed)
            {  
            }
            else if (Status == Status.Failed)
            {  
            }*/

         
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

            this.newHome = sn.DoEntityID(newHome);
           

            return this;
        }

        #endregion

    }
}
