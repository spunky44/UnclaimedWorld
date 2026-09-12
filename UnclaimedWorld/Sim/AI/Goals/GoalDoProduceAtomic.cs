using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using GameStateManagement;
using UWGame.SimSide.Processes;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Expeditions;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Maps;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// TODO: move production code to SimProcess
    /// 
    /// In Activate or similar, creates the process (or continues a stalled/interrupted process..?)
    /// 
    /// for work processes,
    /// here the agent will look up the process and register that he is able to provide work that frame.
    /// later he will reccieve a callback from the process telling him how much work he should actually perform
    /// 2 steps are needed because there could be other limiting factors in production
    /// 
    /// the same is done for energy inputs, which come from somewhere else.
    /// 
    /// for autonomous processes, the goal terminates after teh process has been activated
    /// 
    /// 
    /// </summary>
    public class GoalDoProduceAtomic : Goal
    {
        /// <summary>
        /// can be null!!!
        /// </summary>
       /* private ProcessJob job;
        JobID? snapshotJob;
        */
      //  private ProcessType processType;

        private float maxPossibleProgress = 0f;
       
        /// <summary>
        /// has productivity info
        /// </summary>
     //   private ToolTypeCombination toolTypeCombination;
     //   private List<Entity> tools;
      //  private float? toolProductivityFactor;

        double goalProgress = 0.0;

        private static Queue<GoalDoProduceAtomic> freeGoals = new Queue<GoalDoProduceAtomic>();

        GoalDoProduce parentGoal;
        GoalID snapshotParentGoal;

       // SimProcess process = null;

        /// <summary>
        /// is only public because Snapshot needs it. But new goals should always be retrieved via the pool.
        /// </summary>
        public GoalDoProduceAtomic()
            : base()
        {
           
        }

       
        private void Init(Entity owner, GoalDoProduce parentGoal) // ProcessJob job, Owner ownerOfProduct, List<Entity> tools, ToolTypeCombination toolTypeCombination, Regulator chanceToDestroyToolRegulator) //float? toolProductivityFactor) //)
        {
          //  this.process = parentGoal.Process;
            this.parentGoal = parentGoal;// better to give a reference to the parent goal...
           // this.job = parentGoal.Job;
           // this.processType = parentGoal.ProductionProcess.ProcessType;
          //  this.OwnerOfProduct = ownerOfProduct; 
            goalProgress = 0.0;
           // this.toolProductivityFactor = toolProductivityFactor;
           // this.toolTypeCombination = toolTypeCombination; 
          //  this.tools = tools;
            base.Init(owner);
        }

        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }

        /// <summary>
        /// necessary to avoid goal hanging on to Entity instances and creating leaks...
        /// </summary>
        public static void ClearPool()
        {
            freeGoals.Clear();
        }

        public static GoalDoProduceAtomic GetGoal(Entity owner, GoalDoProduce parentGoal) //ProcessJob job, Owner ownerOfProduct, List<Entity> tools, ToolTypeCombination toolTypeCombination, Regulator chanceToDestroyToolRegulator) //float? toolProductivityFactor) //ToolTypeCombination toolTypeCombination)
        {
            if (freeGoals.Count == 0)
            {   // add some fresh goals, we've run out:
                for (int i = 0; i < 30; i++)
                {
                    freeGoals.Enqueue(new GoalDoProduceAtomic());
                }
            }
            GoalDoProduceAtomic goal = freeGoals.Dequeue();
            goal.Init(owner, parentGoal); // job, ownerOfProduct, tools, toolTypeCombination, chanceToDestroyToolRegulator);
            return goal;
        }

        public override void RetireGoal()
        {
            freeGoals.Enqueue(this);
        }


        

        protected override void Activate()
        {           
            Status = Status.Active;            

        }

        public override void OnEnter()
        {
            // setting anim states here instead of in the parent goal allows us to reset from any pushing that may have set Moving and overwritten these states
            // clearing them in the parent goal prevents glitches..
            IKnownProcess processData;
            if (!GoalEvaluator.ProcessDataResultCausesSkip(entityIntelligence.GetKnownProcessData(parentGoal.ProductionProcess, out processData)))
            {
                SetProcessAnimStates(processData.ProcessType);
                //  SetProcessAnimStates(parentGoal.ProductionProcess.ProcessType); 
            }

            base.OnEnter();
        }

        
       /* public override void OnExit()
        {
            
            base.OnExit();
        }*/            

        /*   protected override bool ArePreconditionsOK()
           {
            
              // return !job.ProductionSite.IsDestroyed;  
            
           }*/

        private void EmptyToolContainers()
        {


        }

        private bool Produce(GameTime elapsed)
        {
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(parentGoal.ProductionProcess);

            SimProcess.StatusOfProcess statusOfProcess = SimProcess.StatusOfProcess.Active;

            if (process == null)
            {
                Status = Goals.Status.Failed;
                return false;
            }
           
            if (process.IsStarted == false)
            {
                OwnerID? ownerID = parentGoal.OwnerOfProduct;

                List<EntityID> stationaryTools;
                if (!GoalDoProduce.GetStationaryTools(parentGoal.Tools, out stationaryTools))
                {
                    return false;
                }

                statusOfProcess = process.Start(entity, ownerID, parentGoal.ToolTypeCombination, stationaryTools);

                if (statusOfProcess == SimProcess.StatusOfProcess.Failed)
                {
                    Status = Status.Failed;
                    return false;
                }
               
            }

            return true;

            /*
          
            //Set GoalStatus depending on what the process status is.
            switch (statusOfProcess)
            {
                case SimProcess.StatusOfProcess.Active:
                    {
                        Status = Goals.Status.Active;
                        break;
                    }
                case SimProcess.StatusOfProcess.Complete:
                    {
                        Status = Goals.Status.Completed;
                        break;
                    }
                case SimProcess.StatusOfProcess.Failed:
                    {
                        Status = Goals.Status.Failed;
                        break;
                    }
            }
     
            return statusOfProcess != SimProcess.StatusOfProcess.Failed;*/
        }

       

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            if (entity.ID == (EntityID)4941)
            {

            }

            // the process must exist!
          /*  IKnownProcess processData;
            if (GoalEvaluator.ProcessDataResultCausesSkip(entityIntelligence.GetKnownProcessData(parentGoal.ProductionProcess, out processData)))
            {
                Status = Status.Failed;
                return;
            }*/

            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(parentGoal.ProductionProcess);
            if (process == null)
            {
                Status = Status.Failed;
                return;
            }

            float currentProgress;
            if (!process.GetKnownProgress(entityIntelligence.Allegiance.SharedKnowledge, out currentProgress)) // !parentGoal.ProductionProcess.GetKnownProgress(entityIntelligence.Allegiance.SharedKnowledge, out currentProgress)) 
            {
                Status = Status.Failed;
            }
            else
            {

                if (!NonLivingEntity.IsCompleted(currentProgress)) 
                {
                   
                    if (Produce(elapsed) == false)
                    {
                        Status = Status.Failed;
                        return; // Status;
                    }

                    if (entity.ID == (EntityID)4941)
                    {

                    }

                    // goal must only last 0.2 seconds:
                    goalProgress += elapsed.ElapsedGameTime.TotalSeconds;
                    if (goalProgress > GameData.Instance.AIConstants.AtomicGoalPeriodInSeconds)
                    {
                        Status = Status.Completed;
                    }

                }
                else
                {
                    Status = Status.Completed;
                }
            }

        }

        

        //OLD: staged consumption of inputs
        /*   private void InitJobEstimate()
           {
               // do init - shallow clone:
               job.CumulativelyEstimatedAssignedInputsToThisJob.Clear(); 
               foreach (KeyValuePair<EntityType, List<Entity>> kvp in job.AssignedInputsToThisJob)
               {
                   List<Entity> items = new List<Entity>();
                   for (int i = 0; i < kvp.Value.Count; i++)
                   {
                       items.Add(kvp.Value[i]);
                   }
                   job.CumulativelyEstimatedAssignedInputsToThisJob.Add(kvp.Key, items);
               }

               job.CumulativelyEstimatedNeededItems.Clear();

               foreach (var kvp in job.ProcessType.InputsByType) // job.NeededItems)
               {
                   job.CumulativelyEstimatedNeededItems.Add(kvp.Key, kvp.Value.Amount);
		 
               }

               job.CumulativelyEstimatedProgress = job.GetCurrentProgress();
               job.CumulativelyEstimatedNeededEnergy = 0;

               job.EstimationHasStarted = true;

           }*/


        /// <summary>
        /// OLD: consumption of inputs in stages
        /// Calculate the needed energy with the manpower and materials available
        /// </summary>
        /// <param name="elapsed"></param>
        /// <param name="neededEnergy"></param>
        /*   private void EstimateCumulativeProduction(GameTime elapsed, Entity worker)
           {
               Dictionary<EntityType, List<Entity>> assignedInputs;
             //  Dictionary<EntityType, Amount> neededItems;

               if (job.TakenBy.Count > 1)
               {
                   // are we the first worker in this cycle?
                   if (!job.EstimationHasStarted)// job.CumulativelyEstimatedNeededItems == null)
                   {
                       InitJobEstimate();
                   }

                   assignedInputs = job.CumulativelyEstimatedAssignedInputsToThisJob;
                   neededItems = job.CumulativelyEstimatedNeededItems;
               }
               else
               {
                   // only one worker - don't make copies:
                   assignedInputs = job.AssignedInputsToThisJob;
                   neededItems = job.NeededItems;
                   // init:
                   job.CumulativelyEstimatedProgress = job.GetCurrentProgress(); 
                   job.CumulativelyEstimatedNeededEnergy = 0;
               }

               if (job.GetCurrentProgress() >= 1f)
               {
                   //   Status = Status.Completed;
                  // NeededEnergy = 0f;
               }
               else
               {
                   float progressDelta;

                   if (job.ProcessType.WorkOrTimeNeeded.ManSecondsOfWorkNeeded.HasValue)
                   {
                       progressDelta = job.CalculateProgressDelta(elapsed.ElapsedGameTime.TotalSeconds, worker,
                           job.ProcessType.WorkOrTimeNeeded.ManSecondsOfWorkNeeded.Value, job.ProcessType.RequiredSkillType);
                   }
                   else
                   {
                       progressDelta = job.CalculateProgressDelta(elapsed.ElapsedGameTime.TotalSeconds, job.ProcessType.WorkOrTimeNeeded.TimeNeeded.Value);
                   }
                       //job.ProducerType.CalculateProgressDelta(elapsed.ElapsedGameTime.TotalSeconds, worker);

                   float progressForThisInput;
                   maxPossibleProgress = 0f;
                   float maxProgressDelta;
                   bool progressHasBeenSet = false;

                   if (job.ProcessType.InputsByType != null)
                   {
                       foreach (KeyValuePair<EntityType, Input> kvp in job.ProcessType.InputsByType)
                       {
                           Input input = kvp.Value;

                           progressForThisInput = job.ProcessType.FindMaxProgressWithAvailableMaterials(
                                job.CumulativelyEstimatedProgress,
                                progressDelta,
                                assignedInputs[input.EntityType].Count,
                                input.StageLength);

                           // we are limited by the lowest no of available raw materials:

                           if (progressHasBeenSet)
                           {   // compare with previous value
                               maxPossibleProgress = Math.Min(maxPossibleProgress, progressForThisInput);
                           }
                           else
                           {
                               maxPossibleProgress = progressForThisInput;
                               progressHasBeenSet = true;
                           }
                       }
                   }
                   else
                   {
                       maxPossibleProgress = job.CumulativelyEstimatedProgress + progressDelta;
                   }

                   maxProgressDelta = maxPossibleProgress - job.CumulativelyEstimatedProgress;
                    
                   if (job.TakenBy.Count > 1)
                   {
                       //'consume', so the next worker can calculate accurately:
                       foreach (KeyValuePair<EntityType, Input> kvp in job.ProcessType.InputsByType)
                       {
                           Input input = kvp.Value;

                           int consumed = job.ProcessType.NeededMaterials(job.CumulativelyEstimatedProgress,
                                  maxProgressDelta,
                                  input.StageLength);

                           Amount neededAmount = job.CumulativelyEstimatedNeededItems[input.EntityType];

                           for (int i = 0; i < consumed; i++)
                           {
                               Entity itemToConsume = job.CumulativelyEstimatedAssignedInputsToThisJob[input.EntityType][0];
                               job.CumulativelyEstimatedAssignedInputsToThisJob[input.EntityType].RemoveAt(0);

                               neededAmount.NoOfItems = neededAmount.NoOfItems.Value - 1;
                               //job.CumulativelyEstimatedNeededItems[input.ItemType]--;
                           }
                       }
                   }                   
                
                   // determine the amount of energy we need:
                   NeededEnergy = job.ProcessType.NeededEnergy(maxProgressDelta);
                   job.CumulativelyEstimatedNeededEnergy += NeededEnergy;
                   // increase the 'progress' for the next worker to estimate accurately:
                   job.CumulativelyEstimatedProgress = maxPossibleProgress;

               }
           }
           */
       


        public override float GetExertionLevel()
        {
          
            IKnownProcess processData;
            if (!GoalEvaluator.ProcessDataResultCausesSkip(entityIntelligence.GetKnownProcessData(parentGoal.ProductionProcess, out processData)))
            {
                return processData.ProcessType.PhysicalWorkFactor ?? GameData.Instance.Constants.PhysicalWork.DefaultWork;
            }

            return base.GetExertionLevel();

           // return parentGoal.ProductionProcess.ProcessType.PhysicalWorkFactor ?? GameData.Instance.Constants.PhysicalWork.DefaultWork;          
        }


     /*   public static bool DoDamageToLeafParts(Entity item, float damage, bool rollForChanceToDestroy)
        {
            if (item.Parts != null)
            {
                Entity part;
                bool partWasDestroyed = false;

                for (int i = item.Parts.Count - 1; i >= 0; i--) // iterate backwards - parts may dissappear
                {
                    part = item.Parts[i];
                    partWasDestroyed = DoDamageToLeafParts(part, damage, rollForChanceToDestroy) | partWasDestroyed;
                }

                return partWasDestroyed;
            }
            else
            {
                return item.NonLivingEntity.DoConditionDamage(damage, rollForChanceToDestroy);
            }
        }*/


        /*
        private void CreateProduct(Entity part)
        {
            // create the product  
            job.OutputEntities = Item.CreateItem(job.ProducerType.Outputs[0].EntityType, new List<Entity>(){ part }); // TODO: respect multiple outputs!!!
                 
           // job.OutputEntity = new Item(job.ProducerType.Output, part); // (Item)Activator.CreateInstance(job.ProducerType.Output.InstanceType, new Object[] { });

            job.OutputEntities.PlaceItemWithSmallRandomOffset(MapManager.TileToWorldPos(job.ProductionSite.MapPosition));

            job.OutputEntities.ChangeOwnership(OwnerOfProduct);
            UWGame.SimSide.Instance.Map.TileMap[job.OutputEntities.MapPosition.X][job.OutputEntities.MapPosition.Y].AddEntity(job.OutputEntities);

            if (job.ProductionSite.ItemStorage != null)
            {               
                // NEW: place the new item in building's storage:
                job.ProductionSite.ItemStorage.Add(job.OutputEntities, null);
            }
        }*/

        

        /*  private void CreateProducts()
          {
              Entity outputEntity;
              foreach (var output in job.ProcessType.Outputs)
              {
                  int noOfItemsToCreate = 1;

                  // create the products  
                  for (int i = 0; i < noOfItemsToCreate; i++)
                  {

                      outputEntity = Item.CreateItem(output.FinalEntityType, new List<Entity>());

                      outputEntity.Progress = 0f; // not yet a 'physical object' !

                      Vector3? productionLocation = job.GetProductionSiteLocation();

                      outputEntity.PlaceItemWithSmallRandomOffset(productionLocation.Value); // MapManager.TileToWorldPos(job.ProductionSite.MapPosition));

                      outputEntity.ChangeOwnership(OwnerOfProduct);
                      UWGame.SimSide.Instance.Map.TileMap[outputEntity.MapPosition.X][outputEntity.MapPosition.Y].AddEntity(outputEntity);


                      if (job.ProductionSite != null && job.ProductionSite.ItemStorage != null)
                      {
                          // NEW: place the new item in building's storage:
                          job.ProductionSite.ItemStorage.Add(outputEntity, null);
                      }

                      job.OutputEntities.Add(outputEntity);

                  }
              }
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

            this.maxPossibleProgress = sn.DoFloat(maxPossibleProgress);
            this.goalProgress = sn.DoDouble(goalProgress);
            this.snapshotParentGoal = (GoalID)sn.SnapshotID<Goal, GoalID>(parentGoal);             
         //   this.snapshotJob = sn.SnapshotID<Job, JobID>(job);
         //   this.processType = sn.DoGameData(processType);

            sn.Ignore(freeGoals);
            sn.Ignore(parentGoal);
         

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            parentGoal = (GoalDoProduce)LookUpGoals.FindByID(snapshotParentGoal);

           /* if (snapshotJob.HasValue)
            {
                job = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }*/


        }

        #endregion


    }
}
