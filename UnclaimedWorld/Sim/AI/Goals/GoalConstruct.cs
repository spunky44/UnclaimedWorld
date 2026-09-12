using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// 1. Move to site. 2. Start producing.
    /// </summary>
    class GoalConstruct : CompositeGoal, ITopLevelGoal
    {
        public ProcessJob job;
        JobID? snapshotJob;

        public List<EntityID> Tools = new List<EntityID>();
        public double TimeSpentInTopLevelGoal { get; set; }

        /// <summary>
        /// has productivity info
        /// </summary>
        public ToolTypeCombination ToolTypeCombination;
        ToolTypeCombinationID? snapshotToolCombo;

     //   private string snapshotProcessToolset; // don't want to make an id for the ToolTypeCombination since it gets Initialized from xml data...
      //  private int snapshotToolTypeComboIndex;


        public Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools;


        public GoalConstruct(Entity entity, ProcessJob job, List<EntityGroupID> ownersOfVehicles,
            List<EntityID> tools, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools, ToolTypeCombination toolTypeCombination)
            : base(entity)
        {
            this.job = job;
            this.ownersOfVehicles = ownersOfVehicles;
            this.Tools = tools;
            this.ToolTypeCombination = toolTypeCombination;
            this.replenishItemsForTools = replenishItemsForTools;
                       
        }

     

        public GoalConstruct()
        {
        }


        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            // wait a bit, to see if we are asked to cancel by other agents.
            AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));

            // LOCKS
            job.TakeJob(entity);

            SetLocksOnReplenishItems(job, replenishItemsForTools);

            //////


          //  Point tile;
          //  Vector2 tileCenterOffset;
            Vector3 workLocation;

            // move this to Evaluator instead? At least an accessibility test...
            // Vector3? location;
            IKnownEntityData structureToConstruct = null;
            if (job.BuildingJob.GetWorkLocation(entity, out workLocation, out structureToConstruct)) 
            {
                // wait a bit, to see if we are asked to cancel by other agents.
                AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));

                List<IKnownEntityData> toolsData = null;
                if (!ResolveTools(Tools, ref toolsData))
                {
                    return;
                }

                // gather tools:
                if (!GatherToolsOrWeapons(toolsData, ownersOfVehicles, job, false, StorageCompartment.Haul))
                {
                    return;
                }
                

                // move to our assigned work location
                // AddSubgoal(new GoalMoveToPosition(entity, job.BuildingToConstruct.FrontDoorTilePos, GoalMoveToPosition.VehicleUse.FreeUpAfterUse, ownersOfVehicles));                 
                AddSubgoal(new GoalMoveToPosition(entity, workLocation, ownersOfVehicles) { IsFinalDestination = false });

                PlaceStationaryToolsAtWorkSite(toolsData, workLocation);

                ReplenishToolsOrWeapons(replenishItemsForTools, ownersOfVehicles, job);

                AddSubgoal(new GoalMoveToPosition(entity, workLocation, ownersOfVehicles) { IsFinalDestination = true });


                AddSubgoal(new GoalTurnToFace(entity, structureToConstruct.PlaySiteLocation.ToVector2()));

                if (entity.HasStance())
                {
                    ChangeStance(entity.Locomotor.Stance.PickRandomProcessStance(job.ProcessType.StanceTypes));
                }
               

                // start constructing                      
                AddSubgoal(new GoalDoProduce(entity, job, structureToConstruct.OwnedBy, Tools, ToolTypeCombination));                
            }
            else
            {
                // failed to find a work location.
                Status = Status.Failed;
            }            
           
        }


        protected override bool ArePreconditionsOK()
        {
            if (!AreToolsOK(Tools))
                return false;

            if (!IsOutputOKAndNotCompleted(job, true))
            {
                return false;
            }

            return true;

            /*
            return job.OutputEntities[0] != null
                && !The.Sim.IsKnownToBeDestroyed(entityIntelligence, job.OutputEntities[0]) 
                && job.OutputEntities[0].NonLivingEntity.Progress < 1f;*/
        }

       
        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
            /*  ActivateIfInactive();

             // if (Status != Status.Failed)
              if (Status == Status.Active)
              {*/
            // preconditions!

            if (!preconditionsRegulator.IsReady() ||
                ArePreconditionsOK())
            {
                //process the subgoals
                Status = ProcessSubgoals(elapsed);
            }
            else
            {
                //Added a check to see if the output was completed as we had an case that we could not reproduce
                //That occured when an job still existed with completed outputs
                //Now if we see that the output is completed we will set the status to Completed instead of failed as before
                // LPE: I removed this hack. during testing, find out if it can still occur.
              /*  if (job.IsCompleted())
                {
                    Status = Status.Completed;
                }
                else
                {
                    Status = Status.Failed;
                }

                // OLD:
                job.GetFirstOutputEntityData(out outputData); // replace with IsCompleted and OutputExists
                if (outputData != null)
                {
                    //If the outputdata is completed we want to remove the job.
                    if (outputData.IsCompleted())
                    {
                        Status = Status.Completed;
                    }
                    else
                    {
                        Status = Status.Failed;
                    }
                }
                else
                {
                    Status = Status.Failed;
                }*/
            }

            if (Status == Status.Completed)
            {                  
               // RemoveHandToolLocksFromJob(job, Tools, replenishItemsForTools);  // moved to Deactivate           

            }
            else if (Status == Status.Failed)
            {   // we don't want it anymore...
                job.Abandon(entity);               
            }
        }

        public override string GetStatus()
        {
            return "Constructing";
        }

        public double ScoreGoal()
        {
                  
            return ScoreJobGoal(job, new ToolParams() 
            { 
                Tools = this.Tools,
                ToolProductivity = GoalProduce.GetToolCombinationProductivity(ToolTypeCombination), 
                ReplenishStatus = null, JobDurationInDays = null });  
        }

        

        public override bool IsSame(Jobs.Job job)
        {
            return job == this.job;

        
        }

      
        public override void Deactivate()
        {
            if (job != null)
            {
                //job.Abandon(entity);

                //// de-assign replenish items:
                //AssignReplenishItems(job, replenishItemsForTools, false);

                RemoveProcessToolLocks(job, Tools, replenishItemsForTools);             
                
            }
                      
        }

       /* public override void Terminate()
        {
            job.Abandon(entity);

            ReleaseLocksOnTools(Tools, job);

            base.Terminate();
        }*/


        public override bool HandleMessage(Message message)
        {
            //first, pass the message down the goal hierarchy
          bool handled = ForwardMessageToFrontMostSubgoal(message);

          //if the msg was not handled, test to see if this goal can handle it
          if (handled == false)
          {
              switch (message.MessageType)
              {
                  // someone wants us to stop doing this job:
                  case Message.MessageTypes.CancelJobOrItemInUse:
                  case Message.MessageTypes.CancelJobForAIReset:

                      Status = Status.Failed;
                      job.Abandon(entity);

                      return true; //msg handled

                  default: return false;
              }
          }
          else
          {
              return true;
          }
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

            this.Tools = sn.DoList(Tools);           
            this.replenishItemsForTools = sn.DoMultiMap(replenishItemsForTools);
            this.snapshotJob = sn.SnapshotID<Job, JobID>(job);
            this.snapshotToolCombo = sn.SnapshotID<ToolTypeCombination, ToolTypeCombinationID>(ToolTypeCombination);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);


            sn.Ignore(job);
            
            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            ToolTypeCombination = LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(snapshotToolCombo);
            //job = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            if (snapshotJob != null)
            {
                job = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }

        }


        #endregion
    }
}
