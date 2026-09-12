using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// only the head of the household should do this
    /// </summary>
    class GoalMoveInToNewHome: CompositeGoal, ITopLevelGoal
    {
        EntityID newHome;
        public double TimeSpentInTopLevelGoal { get; set; }

        public GoalMoveInToNewHome(Entity owner, EntityID newHome, List<EntityGroupID> ownersOfVehicles)
            : base(owner)
        {
            this.newHome = newHome;
            this.ownersOfVehicles = ownersOfVehicles;
        }

        protected override void Activate()
        {
            if (entity.ID == (EntityID)5142)
            {

            }

            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();
            
            IKnownEntityData homeData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(newHome, out homeData)))
            {
                return;
            }
                      

            // if the home is seen to exist, we can 'move in' directly. otherwise, we must first go there to inspect         
            Entity homeEntity = homeData as Entity;
            if (homeEntity != null)
            {
                 IResidence residence = homeEntity.Contains as IResidence;
               
                // Residence residenceComponent;
                 if (residence != null) // homeEntity.Find(out residenceComponent))
                 {
                     if (residence.Residence.Residents >= homeEntity.EntityType.ContainerType.ResidenceType.LivingCapacity)
                     {
                         // no room... include family members here...?                        
                         Status = Goals.Status.Failed;
                         return;
                     }
                     else
                     {
                         if (!MoveInToNewHome(personEntity, homeEntity))
                         {
                             Status = Goals.Status.Failed;
                             return;
                         }

                         Status = Goals.Status.Completed;
                     }
                 }
                 else
                 {
                     Status = Goals.Status.Failed;
                     return;
                 }

            }
            else
            {
                AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, homeData, GoalMoveToPosition.VehicleUse.FreeUpAfterUse));
                AddSubgoal(new GoalDoMoveInToNewHome(entity, newHome));

            }



            // possessions are moved by the haulingjobmanager.
           
        }

        /// <summary>
        /// only the head of household should do this!
        /// </summary>
        /// <param name="personEntity"></param>
        /// <param name="homeEntity"></param>
        /// <returns></returns>
        public static bool MoveInToNewHome(Person personEntity, Entity homeEntity)
        {
            // must have capacity:
            if (Residence.HasCapacity(personEntity.Household, homeEntity.Residents.Value, homeEntity.EntityType)) 
            {
                return personEntity.Household.SetHome(homeEntity.ID);
            }
            else return false;
        }

       

        public GoalMoveInToNewHome()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
      
        }

        public override string GetStatus()
        {
            return "Moving home";
        }

      
        protected override void ProcessWhileActive(GameTime elapsed)
        {                           
            //process the subgoals                
            Status = ProcessSubgoals(elapsed);

            // while moving, if we can see the home, move in straight away. otherwise other people will take it.
            if (Status == Goals.Status.Active)
            {
                IKnownEntityData homeData;
                if (entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(newHome, out homeData) == EntityResult.SeenDirectly)
                {
                    if (MoveInToNewHome(personEntity, (Entity)homeData))
                    {
                        Status = Goals.Status.Completed;
                    }
                    else
                    {
                        Status = Goals.Status.Failed;
                    }
                }
            }
        }

        public double ScoreGoal()
        {
            return entityIntelligence.GetCurrentGoalUtility().Value;
        }

        #region ISnapshot

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.newHome = sn.DoEntityID(newHome);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            return this;
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


        #endregion
    }
}
