using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities.Body;
using System.Linq;
using UWGame.ClientSide.Renderables;
using System.Reflection;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AI.Goals
{
    public class GoalThink : CompositeGoal
    {
        protected List<GoalEvaluator> evaluators = new List<GoalEvaluator>();
       
        Regulator arbitrateRegulator; // = new Regulator(The.Sim.GameplayRandomGenerator, 2, "GoalDoThinkArbitrate");
    
        Regulator arbitrateRegulatorWhileBusy; // = new Regulator(The.Sim.GameplayRandomGenerator, GameData.Instance.AIConstants.NoOfTimesPerSecondToArbitrateWhileBusy, "GoalDoThinkArbitrateBusy"); // every 3 seconds

        private bool waitingForEvaluator = false;
        private int evaluatorBeingProcessed = 0;

        GoalEvaluator previousMostDesirable = null;
        
        double bestScore = 0;
        GoalEvaluator bestEvaluator = null;
               

        bool hasArbitratedWhileBusy = false;
      

        public GoalThink(Entity entity)
            : base(entity)
        {
            /* evaluators.Add(UWGame.SimSide.DayPhases.Work, new List<GoalEvaluator>());
             evaluators.Add(UWGame.SimSide.DayPhases.Leisure, new List<GoalEvaluator>());
             evaluators.Add(UWGame.SimSide.DayPhases.Sleep, new List<GoalEvaluator>());
             */

            ResetEvaluators();

        }



        public GoalThink()
        {
        }

        protected override void CreateRegulators()
        {
            base.CreateRegulators();

            arbitrateRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 2, "GoalDoThinkArbitrate");
            arbitrateRegulatorWhileBusy = new Regulator(The.Sim.GameplayRandomGenerator, GameData.Instance.AIConstants.NoOfTimesPerSecondToArbitrateWhileBusy, "GoalDoThinkArbitrateBusy"); // every 3 seconds

        }
        
        /// <summary>
        /// this must be called again whenever the agent changes expedition, allegiance or household
        /// </summary>
        public void ResetEvaluators()
        {
            // restart all arbitration:
            waitingForEvaluator = false;
            evaluatorBeingProcessed = 0;
            The.Sim.WaitingAgents.Remove(entity.ID);

          
            List<AIAgeGroup> childToOldAges = new List<AIAgeGroup>() { AIAgeGroup.Child, AIAgeGroup.YoungAdult, AIAgeGroup.Adult, AIAgeGroup.Old };
            List<AIAgeGroup> youngAdultToOldAges = new List<AIAgeGroup>() { AIAgeGroup.YoungAdult, AIAgeGroup.Adult, AIAgeGroup.Old };
            List<AIAgeGroup> adultToOldAges = new List<AIAgeGroup>() { AIAgeGroup.Adult, AIAgeGroup.Old };
            List<AIAgeGroup> adult = new List<AIAgeGroup>() { AIAgeGroup.Adult };


#if DEBUG ||PROFILE
            if (entity.DebugGoalPlan != null)
            {
                //push the special debug evaluator that cheats and adds user scripted goals 
                AddEvaluator(new DebugJobEvaluator(entity));
                //and don't add any other evaluators
                return;
            }
#endif

            // if the allegiance respects ownership, what about the members that don't? like dogs...
            Allegiances.Allegiance allegiance = entity.Intelligence.Allegiance;
            bool allegianceRespectsOwnership = allegiance.RepresentativeEntityType.IntelligenceType.RespectsOwnership;

            List<EntityGroup> allOwnersOfVehicles = null, allOwnersOfToolsAndWeapons = null;
            List<EntityGroup> householdAndPrivateOwnersOfVehicles = null, householdAndColonyOwners = null;
            List<EntityGroup> colonyOwnedVehicles = null;
            
            OwnerID? ownerOfCarcasses = null; // owner of threatening critters that were killed
            OwnerID? ownerOfProductsID = null;
            
            EntityGroup ownerOfJobs, ownerOfItems;

            if (allegianceRespectsOwnership)
            {
                allOwnersOfVehicles = new List<EntityGroup>();
                allOwnersOfToolsAndWeapons = new List<EntityGroup>();
                householdAndColonyOwners = new List<EntityGroup>();
                colonyOwnedVehicles = new List<EntityGroup>();

                // collect the owner objects:
                // robots do not have private ownership, or households:
                if (personEntity != null)
                {
                    householdAndPrivateOwnersOfVehicles = new List<EntityGroup>();
                    householdAndPrivateOwnersOfVehicles.Add(personEntity.OwnedEntities);
                    householdAndPrivateOwnersOfVehicles.Add(personEntity.Household.OwnedEntities);

                    // household and colony activities:
                    householdAndColonyOwners.Add(personEntity.Household.OwnedEntities);


                    allOwnersOfVehicles.Add(personEntity.OwnedEntities);
                    allOwnersOfVehicles.Add(personEntity.Household.OwnedEntities);

                    allOwnersOfToolsAndWeapons = new List<EntityGroup>();
                    allOwnersOfToolsAndWeapons.Add(personEntity.OwnedEntities);
                    allOwnersOfToolsAndWeapons.Add(personEntity.Household.OwnedEntities);

                }

                Expedition expedition = entityIntelligence.CurrentExpedition;

                allOwnersOfVehicles.Add(expedition.OwnedEntities);
                allOwnersOfToolsAndWeapons.Add(expedition.OwnedEntities);
                householdAndColonyOwners.Add(expedition.OwnedEntities);
                colonyOwnedVehicles.Add(expedition.OwnedEntities);
               
                ownerOfCarcasses = ((IOwner)expedition).ID;
                ownerOfProductsID = ((IOwner)expedition).ID;

                ownerOfJobs = expedition.OwnedEntities;
                ownerOfItems = expedition.OwnedEntities;

            }
            else
            {
                // no ownership is respected
                ownerOfJobs = allegiance.SharedKnowledge.AllKnownEntities;
                ownerOfItems = allegiance.SharedKnowledge.AllKnownEntities;                
            }

          //  EntityGroup foodEntityGroup = GetFoodEntityGroup(entity);


            if (personEntity != null)
            {

                #region personal & household jobs - not yet in use!
                //  AddEvaluator(new EvaluateFindPlaceToEat(entity), null, childToOldAges); //, UWGame.SimSide.DayPhases.Work);

                // fill food stocks:
                AddEvaluator(new EvaluateHaulingJobs(entity, personEntity.Household.OwnedEntities,
                    personEntity.Household.OwnedEntities, householdAndPrivateOwnersOfVehicles), //, false), 
                    Sim.DayPhases.Leisure, childToOldAges);

                // haul personal stuff:
                AddEvaluator(new EvaluateHaulingJobs(entity, personEntity.OwnedEntities,
                   personEntity.OwnedEntities, householdAndPrivateOwnersOfVehicles), //, false), 
                   Sim.DayPhases.Leisure, childToOldAges);


                // do construction and other jobs for the household:
                AddEvaluator(new EvaluateJob(entity, personEntity.Household.OwnedEntities, personEntity.Household.OwnedEntities, householdAndPrivateOwnersOfVehicles, ((IOwner)personEntity.Household).ID),
                    Sim.DayPhases.Leisure, youngAdultToOldAges);

                #endregion


                // cook:
                /*  AddEvaluator(new EvaluateCooking(entity, personEntity.Household.CookingJob, personEntity.Household.Ownership, householdAndPrivateOwnersOfVehicles), 
                      UWGame.SimSide.DayPhases.Leisure, youngAdultToOldAges);*/

                AddEvaluator(new EvaluateLeisureWalk(entity, householdAndColonyOwners), Sim.DayPhases.Leisure, youngAdultToOldAges);


              

              /*  if (entity.EntityType.IntelligenceType.CanHunt != false)
                {
                    AddEvaluator(new EvaluateAttackJobs(entity, Goals.EvaluateAttackJobs.JobTypes.Hunt, entity.Intelligence.Allegiance, ownerOfJobs, ownerOfProductsID, colonyOwnedVehicles, colonyOwnedVehicles),
                      Sim.DayPhases.Work, adult);
                }*/
                


                //*******************

                // add the tasks that adults and the old have in common:
                //  case AIAgeGroup.Adult:
                //  case AIAgeGroup.Old:


                AddEvaluator(new EvaluateFindHome(entity), Sim.DayPhases.Leisure, adultToOldAges);

               
              
                // people
                // TODO: owner group should be a list - person, household, expedition
                AddEvaluator(new EvaluateEat(entity, /*entityIntelligence.CurrentExpedition.OwnedEntities,*/ allOwnersOfVehicles, householdAndColonyOwners));

                Sim.DayPhases phaseToSleepIn = GetSleepPhase();


                if (entity.BiologicalEntity.Needs.NeedsList.ContainsKey("sleep")) //Needs.NeedClass.Sleep))
                {
                    AddEvaluator(new EvaluateSleep(entity, allOwnersOfVehicles, householdAndColonyOwners), phaseToSleepIn);

                }
            }


            // Work phase - only adults!
            //*****************
            // all critters have an expedition. but they do not respect ownership!
            // robots in an allegiance that respects ownership, must do so also.
            // not sure about animals like dogs...
          

            EntityGroup expeditionOwner = entityIntelligence.CurrentExpedition.OwnedEntities;
            // ok to go to work in own vehicle:
            /*
            AddEvaluator(new EvaluateJob(entity, ownerOfJobs, ownerOfItems, allOwnersOfVehicles, ownerOfProductsID),
                Sim.DayPhases.Work, adult);
            */

            if (entity.EntityType.IntelligenceType.CanHaul != false)
            {
                // only haul using colony vehicles:
                AddEvaluator(new EvaluateHaulingJobs(entity, ownerOfJobs, expeditionOwner, colonyOwnedVehicles), //, true),
                    Sim.DayPhases.Work, adult);
            }

           /* if (entity.EntityType.IntelligenceType.IsMobile)
            {
                // testing only...
                AddEvaluator(new EvaluateWander(entity)); //, UWGame.SimSide.DayPhases.Work, UWGame.SimSide.DayPhases.Leisure);
            }*/
           
            // All
            AddEvaluator(new EvaluateTakeFive(entity));

            if (entity.EntityType.IntelligenceType.IsMobile)
            {
                AddEvaluator(new EvaluateReturnHome(entity, allOwnersOfVehicles));
            }
           
            // it is not enough to check intrinsic attacks here, wepons are also possible... so make it designer driven...
            if (entity.EntityType.IntelligenceType.CanAttack != false)
            {
                AddEvaluator(new EvaluateAttackJobs(entity, Goals.EvaluateAttackJobs.JobTypes.Threat, entity.Intelligence.Allegiance, null, ownerOfCarcasses, allOwnersOfToolsAndWeapons, allOwnersOfVehicles), null, youngAdultToOldAges);
                AddEvaluator(new EvaluateAttackJobs(entity, Goals.EvaluateAttackJobs.JobTypes.AssetThreat, entity.Intelligence.Allegiance, null, ownerOfCarcasses, allOwnersOfToolsAndWeapons, allOwnersOfVehicles), null, youngAdultToOldAges);

            }

            if (entityIntelligence.CanEmigrate())
            {
                AddEvaluator(new EvaluateEmigrate(entity), null, adultToOldAges);
            }

            // critters, robots, people
            // skills should filter these jobs...
            if (entity.EntityType.IntelligenceType.CanDoJobs != false)
            {
                AddEvaluator(new EvaluateJob(entity, ownerOfJobs, ownerOfItems, allOwnersOfVehicles, ownerOfProductsID), Sim.DayPhases.Work, adult);
            }

            if (entity.EntityType.IntelligenceType.CanScout != false
                || entity.EntityType.IntelligenceType.CanExamine != false)
            {
                AddEvaluator(new EvaluateScoutingJobs(entity, ownerOfJobs, allOwnersOfVehicles), Sim.DayPhases.Work, youngAdultToOldAges);
            }

            if (entity.EntityType.IntelligenceType.CanHunt != false)
            {
                AddEvaluator(new EvaluateAttackJobs(entity, Goals.EvaluateAttackJobs.JobTypes.Hunt, entity.Intelligence.Allegiance, ownerOfJobs, ownerOfProductsID, colonyOwnedVehicles, colonyOwnedVehicles),
                  Sim.DayPhases.Work, adult);
            }

         /*   if (!allegianceRespectsOwnership) //   entity.AllegianceID != The.Sim.PlaySite.PlayerAllegiance.ID)
            {                
                //Only for non-player controlled allegiances that don't respect ownership!

                AddEvaluator(new EvaluateJob(entity, allegiance.SharedKnowledge.AllKnownEntities,
                                                    allegiance.SharedKnowledge.AllKnownEntities,
                                                    null,
                                                    null));

                AddEvaluator(new EvaluateScoutingJobs(entity,
                                                    allegiance.SharedKnowledge.AllKnownEntities));
            }*/


            // critters
            if (entity.EntityType.Person == null)
            {
                if (entity.BiologicalEntity != null && entity.BiologicalEntity.Needs != null)
                {
                    if (entity.BiologicalEntity.Needs.NeedsList.Any(n => n.Value.NeedType.SleepNeedType != null)) //.ContainsKey("sleep")) //Needs.NeedClass.Sleep))
                    {
                        Sim.DayPhases phaseToSleepIn = GetSleepPhase();

                        AddEvaluator(new EvaluateSleep(entity, allOwnersOfVehicles, null), phaseToSleepIn);
                    }

                    if (entity.BiologicalEntity.Needs.NeedsList.Any(n => n.Value.NeedType.FoodNeedType != null)) //NeedClass == Needs.AINeedClass.Food))
                    {
                        // critters
                        AddEvaluator(new EvaluateEat(entity, 
                           // foodEntityGroup, 
                            null, null));
                    }
                }
            }

            AddEvaluator(new EvaluateChangeThreatStance(entity));

        }

       
        

        private Sim.DayPhases GetSleepPhase()
        {
            // Sleep phase
            Sim.DayPhases phaseToSleepIn;
            if (entity.EntityType.BiologicalType.IsNocturnal)
            {
                phaseToSleepIn = Sim.DayPhases.Work;
            }
            else
            {
                phaseToSleepIn = Sim.DayPhases.Sleep;
            }
            return phaseToSleepIn;
        }

        /// <summary>
        /// these are called when the household splits and the previous ownership is no longer valid!
        /// </summary>
     /*   public void ResetHouseholdAndAgeGroupDependentEvaluators()
        {
            // restart all arbitration:
            waitingForEvaluator = false;
            evaluatorBeingProcessed = 0;

            // Leisure phase - clear old
            evaluators[UWGame.SimSide.DayPhases.Leisure].Clear();


            List<Owner> householdAndPrivateOwnersOfVehicles, householdAndColonyOwners;
            switch (this.entity.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup)
            {
                case AIAgeGroup.Child:
                    List<Owner> noVehicles = new List<Owner>();
                    noVehicles.Add(new Owner(NoVehicles.Instance));

                    // fill food stocks:
                    AddEvaluator(new EvaluateHaulingJobs(entity, personEntity.Household.Ownership,
                        personEntity.Household.Ownership, noVehicles, false), UWGame.SimSide.DayPhases.Leisure);

                    // haul personal stuff:
                    AddEvaluator(new EvaluateHaulingJobs(entity, personEntity.PrivateOwnership,
                       personEntity.PrivateOwnership, noVehicles, false), UWGame.SimSide.DayPhases.Leisure);


                    break;
                case AIAgeGroup.YoungAdult:
                    // can run errands in the daytime too:
                    householdAndPrivateOwnersOfVehicles = new List<Owner>();
                    householdAndPrivateOwnersOfVehicles.Add(personEntity.PrivateOwnership);
                    householdAndPrivateOwnersOfVehicles.Add(personEntity.Household.Ownership);

                    // household and colony activities:
                    householdAndColonyOwners = new List<Owner>();
                    householdAndColonyOwners.Add(UWGame.SimSide.Instance.ColonyOwner);
                    householdAndColonyOwners.Add(personEntity.Household.Ownership);

                    AddEvaluator(new EvaluateLeisureWalk(entity, householdAndColonyOwners), UWGame.SimSide.DayPhases.Leisure);

                    // fill food stocks:
                    // DOESN*T WORK!!! contain item owner in job???
                    AddEvaluator(new EvaluateHaulingJobs(entity, personEntity.Household.Ownership,
                        personEntity.Household.Ownership, householdAndPrivateOwnersOfVehicles, false), UWGame.SimSide.DayPhases.Leisure);
                    //   UWGame.SimSide.DayPhases.Leisure, UWGame.SimSide.DayPhases.Work);

                    // haul personal stuff:
                    AddEvaluator(new EvaluateHaulingJobs(entity, personEntity.PrivateOwnership, 
                       personEntity.PrivateOwnership, householdAndPrivateOwnersOfVehicles, false), UWGame.SimSide.DayPhases.Leisure); //,
                    //UWGame.SimSide.DayPhases.Leisure, UWGame.SimSide.DayPhases.Work);

                    // do construction and other jobs for the household:
                    AddEvaluator(new EvaluateJob(entity, personEntity.Household.Ownership, personEntity.Household.Ownership, householdAndPrivateOwnersOfVehicles, EvaluateJob.JobType.NonColonyWork), UWGame.SimSide.DayPhases.Leisure);

                    // cook:
                    AddEvaluator(new EvaluateCooking(entity, personEntity.Household.CookingJob, personEntity.Household.Ownership, householdAndPrivateOwnersOfVehicles), UWGame.SimSide.DayPhases.Leisure);

                    break;

                case AIAgeGroup.Adult:
                case AIAgeGroup.Old:
                    householdAndPrivateOwnersOfVehicles = new List<Owner>();
                    householdAndPrivateOwnersOfVehicles.Add(personEntity.PrivateOwnership);
                    householdAndPrivateOwnersOfVehicles.Add(personEntity.Household.Ownership);

                    AddEvaluator(new EvaluateFindPlaceToEat(entity), UWGame.SimSide.DayPhases.Leisure);
                    AddEvaluator(new EvaluateFindHome(entity), UWGame.SimSide.DayPhases.Leisure);

                    AddEvaluator(new EvaluateBuildHomeAddon(entity), UWGame.SimSide.DayPhases.Leisure);

                    // household and colony activities:
                    householdAndColonyOwners = new List<Owner>();
                    householdAndColonyOwners.Add(UWGame.SimSide.Instance.ColonyOwner);
                    householdAndColonyOwners.Add(personEntity.Household.Ownership);

                    AddEvaluator(new EvaluateLeisureWalk(entity, householdAndColonyOwners), UWGame.SimSide.DayPhases.Leisure);

                    // do construction and other jobs for the household:
                    AddEvaluator(new EvaluateJob(entity, personEntity.Household.Ownership, personEntity.Household.Ownership, householdAndPrivateOwnersOfVehicles, EvaluateJob.JobType.NonColonyWork), UWGame.SimSide.DayPhases.Leisure);
                    // fill food stocks:
                    AddEvaluator(new EvaluateHaulingJobs(entity, personEntity.Household.Ownership,
                        personEntity.Household.Ownership, householdAndPrivateOwnersOfVehicles, false), UWGame.SimSide.DayPhases.Leisure);
                    // haul personal stuff:
                    AddEvaluator(new EvaluateHaulingJobs(entity, personEntity.PrivateOwnership,
                       personEntity.PrivateOwnership, householdAndPrivateOwnersOfVehicles, false), UWGame.SimSide.DayPhases.Leisure);
                    // cook:
                    AddEvaluator(new EvaluateCooking(entity, personEntity.Household.CookingJob, personEntity.Household.Ownership, householdAndPrivateOwnersOfVehicles), UWGame.SimSide.DayPhases.Leisure);
                    break;

            }
        }
        */

        public int GetIndexOfEvaluator(GoalEvaluator evaluator)
        {
            return evaluators.FindIndex(e => e == evaluator);
        }

        public GoalEvaluator GetEvaluatorFromIndex(int index)
        {            
            return evaluators[index];
        }

        protected GoalEvaluator AddEvaluator(GoalEvaluator evaluator, Sim.DayPhases? phase, List<AIAgeGroup> agegroups = null) //AIAgeGroup? ageGroup1 = null, AIAgeGroup? ageGroup2 = null, AIAgeGroup? ageGroup3 = null)
        {
            evaluators.Add(evaluator);

            if (phase.HasValue)
            {
                switch (phase.Value)
                {
                    case Sim.DayPhases.Leisure:
                        evaluator.ActiveInTime = new Tuple<double, double>(Sim.LeisurePhaseStarts, Sim.SleepPhaseStarts);
                        break;
                    case Sim.DayPhases.Work:
                        evaluator.ActiveInTime = new Tuple<double, double>(Sim.WorkPhaseStarts, Sim.LeisurePhaseStarts);
                        break;
                    case Sim.DayPhases.Sleep:
                        evaluator.ActiveInTime = new Tuple<double, double>(Sim.SleepPhaseStarts, Sim.WorkPhaseStarts);
                        break;
                }
            }

            // dump this? the sets of Owner objects used in the evaluators are discrete, so it is difficult to graduate smoothly between age groups...
            if (agegroups != null && entity.BiologicalEntity != null)
            {
                //evaluator.ActiveInAgeInterval = ;
                double? ageIntervalStart = null;
                double? ageIntervalEnd = null;

                //entity.EntityType.BiologicalType.Castes

                float previousEdge = 0;

                int i = 0;
                foreach (AgeGroupType ageGroupType in entity.BiologicalEntity.CasteType.AgeGroupTypes)
                {                                  
                    
                    if (i < agegroups.Count && ageGroupType.AIAgeGroup == agegroups[i])
                    {
                        if (!ageIntervalStart.HasValue)
                        {
                            ageIntervalStart = previousEdge;
                        }

                        ageIntervalEnd = ageGroupType.Edge;
                        i++;
                        
                    }                  

                    previousEdge = ageGroupType.Edge;
                }

                if (ageIntervalStart.HasValue && ageIntervalEnd.HasValue)
                {
                    evaluator.ActiveInAgeInterval = new Tuple<double, double>(ageIntervalStart.Value, ageIntervalEnd.Value);
                }
            }
            

            return evaluator;
        }

        /*
        protected GoalEvaluator AddEvaluator(GoalEvaluator evaluator, UWGame.SimSide.DayPhases phase1, UWGame.SimSide.DayPhases phase2)
        {
            evaluators[phase1].Add(evaluator);
            evaluators[phase2].Add(evaluator);
            return evaluator;
        }*/

        protected GoalEvaluator AddEvaluator(GoalEvaluator evaluator)
        {
            evaluators.Add(evaluator);

            return evaluator;
        }

        /*  protected GoalEvaluator AddEvaluator(GoalEvaluator evaluator, UWGame.SimSide.DayPhases phase1, UWGame.SimSide.DayPhases phase2, UWGame.SimSide.DayPhases phase3)
          {
              evaluators[phase1].Add(evaluator);
              evaluators[phase2].Add(evaluator);
              evaluators[phase3].Add(evaluator);
              return evaluator;
          }*/


        protected override void Activate()
        {

            bool hasFailedSubgoals = false;

            //TODO: Perhaps move the cleaning up of a subgoal to onexit instead on a job so it does not hang on to it for an extra frame.-  DONE!
            CleanupSubgoals(Status, ref hasFailedSubgoals); 

          /*  if (entity.Name == "Joaquin Lehner")
            {
                int name = 5;
            }*/

            if (arbitrateRegulator.IsReady())
            {               
               
                // NEW: Before arbitrating, make sure all failed and completed subgoals are terminated!
                // They may hold on to jobs, vehicles etc.
                if (entity.ToString().Contains("Lewis"))
                {
                    int problem = 0;
                }

                Arbitrate(ArbitrateMode.NoCurrentGoal);
                Status = Status.Active;
            }
        }



        /// <summary>
        /// why overridden??
        /// </summary>
        /// <param name="elapsed"></param>
        protected override void ProcessWhileActive(GameTime elapsed)
        {
            // should never be called on GoalThink
           // throw new NotImplementedException();
        }


        //  processes the subgoals
        //-----------------------------------------------------------------------------
       // public override Status Process(GameTime elapsed)
        public Status ProcessThink(GameTime elapsed)
        {

            if (entity.ToString().Contains("onlan"))
            {

            }

            
#if DEBUG || PROFILE
            if (entityIntelligence.DisableAI)
            {
                return Status.Inactive;
            }

            if (Kensei.Dev.Options.GetOption("Dev.Debug selected entity") &&
                The.InGameUI.SelectedEntity == entity.EntityID)
            {
                Kensei.Dev.Options.SetOption("Dev.Debug selected entity", false);
            }
#endif

            if (!hasArbitratedWhileBusy)
            {
                // this will Arbitrate if the time is right and the goal is Inactive.
                // skip this step if we have replaced the goal while busy on the previous Update... the new goal will have status Inactive!
                ActivateIfInactive();
            }
            else
            {
                hasArbitratedWhileBusy = false; // reset the flag
            }

            if (entity.ToString().Contains("Millet"))
            {

            }

            Status subgoalStatus = ProcessSubgoals(elapsed);

            
            if (hasArbitratedWhileBusy == false && // don't go inactive if we just changed the top level goal!
                (subgoalStatus == Status.Completed || subgoalStatus == Status.Failed || IsIdle())) // if all done, go Inactive (Arbitrate)
            {
                if (entity.ToString().Contains("onlan"))
                {

                }
                Status = Status.Inactive; // this will trigger Arbitrate() in NoCurrentGoal mode next frame
            }
            else
            {
                ITopLevelGoal topLevelGoal = GetTopLevelGoal();
                if (topLevelGoal != null)
                {
                    topLevelGoal.TimeSpentInTopLevelGoal += elapsed.ElapsedGameTime.TotalSeconds;
                }

              /*  if (Subgoals.Count > 0)
                {   // keep time on the top level goal only:
                    Subgoals.Peek().TimeSpentInTopLevelGoal += elapsed.ElapsedGameTime.TotalSeconds;
                }*/
            }


            // NEW: Remove and terminate goals immediately, instead of on the following frame.
            // this is done to avoid problems with snapshotting completed goals with null job references.
            // also to avoid having to handle messages being sent to completed goals.
            bool hasFailedSubgoal = false;
            CleanupSubgoals(Status, ref hasFailedSubgoal);

            return Status;
        }

        public bool ArbitrateWhileBusy(out ITopLevelGoal newGoal, bool useHighFrequency = false)
        {
            newGoal = null;

            if (Subgoals.Count == 0)
            {
                return false; // not busy
            }

            if ((useHighFrequency && arbitrateRegulator.IsReady())
                || (!useHighFrequency && arbitrateRegulatorWhileBusy.IsReady()))
            {
                
                bool hasNewGoal = entityIntelligence.Brain.Arbitrate(ArbitrateMode.HasCurrentGoal);
                
                if (hasNewGoal)
                {
                 
                    if (entity.Name != null && entity.Name.Contains("eboah")) // (this as GoalAttack != null))
                    {

                    }

                    newGoal = GetTopLevelGoal();

                    hasArbitratedWhileBusy = true;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// only makes sense to call this on GoalThink. but I don't want to expose it outside derived classes...
        /// can be null...
        /// </summary>
        /// <returns></returns>
        protected ITopLevelGoal GetTopLevelGoal()
        {
            if (Subgoals.Count > 0)
            {
                Goal goal = Subgoals.Peek();

                return goal as ITopLevelGoal;
            }

            return null;
        }


        //  this method iterates through each goal option to determine which one has
        //  the highest desirability.
        //-----------------------------------------------------------------------------

        private enum ArbitrateMode { NoCurrentGoal, HasCurrentGoal }
        private bool Arbitrate(ArbitrateMode mode)
        {
          

            if (waitingForEvaluator)
            {
               
                // still waiting...
                return false;
            }


            entityIntelligence.TopScoringJobs.Clear();
            double minimumScoreToConsider;
            if (mode == ArbitrateMode.NoCurrentGoal)
            {
              // double? currentScore = entityIntelligence.GetScore();
                minimumScoreToConsider = 0;//(currentScore.HasValue ? currentScore.Value : 0);

            }
            else
            {
                if (entity.Name != null && entity.Name.Contains("eboah"))
                {
                    int i = 0;
                }

                // if we already have a goal, any new goal must be better by a certain margin before we switch.
                // If the considered goal is a job already taken by others, the score for us must be better by a margin also.
                double? currentScore = entityIntelligence.GetScore();
                minimumScoreToConsider = (currentScore.HasValue ? currentScore.Value : 0);

              /*
                double amountNewGoalMustBeBetterToSwitch;
                amountNewGoalMustBeBetterToSwitch = ScoreInertia(); // TODO: put inertia in Goal.GetScore

                minimumScoreToConsider += amountNewGoalMustBeBetterToSwitch;
                */
            }

          
            GoalEvaluator currentEvaluator;
           
            for (; evaluatorBeingProcessed < evaluators.Count; evaluatorBeingProcessed++)
            {
                currentEvaluator = evaluators[evaluatorBeingProcessed];

                if (minimumScoreToConsider >= 2f * currentEvaluator.Priority)
                {
                    continue; // early bailout.
                }
                

#if DEBUG || PROFILE
                if (entity.Locomotor != null
                    && entity.Locomotor.LeggedLocomotor != null 
                    && entity.Locomotor.LeggedLocomotor.TestWander 
                    && !(currentEvaluator is EvaluateWander || currentEvaluator is EvaluateTakeFive))
                {
                    continue;
                }
#endif

                if (currentEvaluator.IsActive)
                {
                    double score = -1.0;
                    
                    if (currentEvaluator.CalculateDesirability(minimumScoreToConsider, ref score)
                        == GoalEvaluator.CalculateResult.Done)
                    {
                        
                        //iterate through all the evaluators to see which produces the highest score         
                        if (score >= bestScore)
                        {
                           // score > minimumScoreToConsider && (entity.ID == (EntityID)5043 || entity.ID == (EntityID)4814) && mode == ArbitrateMode.NoCurrentGoal && currentEvaluator is EvaluateHaulingJobs

                            if (score > minimumScoreToConsider
                                && (entity.ID == (EntityID)5043 || entity.ID == (EntityID)4814)
                                && mode == ArbitrateMode.NoCurrentGoal
                                && currentEvaluator is EvaluateHaulingJobs) //  && entity.PersonEntity != null && entityIntelligence.Allegiance.AllegianceType == Allegiances.AllegianceType.Player  entity.Name.Contains("Bob") && score > minimumScoreToConsider)
                            {
                               // throw new Exception();
                            }
                                                     
                            bestScore = score;
                            bestEvaluator = currentEvaluator;

                            // Test this: 
                          //  minimumScoreToConsider = bestScore;
                        }

                        double? currentScore = entityIntelligence.GetScore();
                    }
                    else
                    {
                        // pause and wait for a message...
                        waitingForEvaluator = true;
                        The.Sim.AddWaitingAgent(entity, Sim.WaitingFor.Regions);

                        return false;  
                    }
                }
            }

         
            // only for debugging:
#if DEBUG || PROFILE
            SortTopScoringJobsForDebugging();
#endif
            if (entity.Name != null && (entity.Name.Contains("onlan") || entity.Name.Contains("eboah")))         
            {
                int i = 0;
            }

            if (bestEvaluator != null)
            {
                UpdateTimeSpentIdling(bestEvaluator);

                if (bestScore >= minimumScoreToConsider)
                {
                    if (mode == ArbitrateMode.NoCurrentGoal)
                    {        
                       
                        string newAIStateAsString = entity.Name + bestScore + bestEvaluator.ToString();
                        if (bestEvaluator.CanTakeGoal())
                        {

                            if (bestEvaluator.CancelCurrentTakers())
                            {
                                The.Sim.Controller.SaveOrVerifyEntityAIState(newAIStateAsString);

                                bestEvaluator.PreSetGoal();

                                if (bestEvaluator.GetType() != typeof(EvaluateTakeFive))
                                {
                                    RemoveAllSubgoals(); // should only remove Take Five and other IsIdle goals...
                                }
                                
                                bestEvaluator.SetGoal();

                                ResetScoreAndCounter();

                                return true;
                            }
                        }
                    }
                    else if (mode == ArbitrateMode.HasCurrentGoal)
                    {   
                        // SWITCH GOAL
                        if (entity.Name != null && entity.PersonEntity != null) 
                        {
                            int i = 0;
                        }
                        entityIntelligence.GetScore();

                        string newAIStateAsString = entity.Name + bestScore + bestEvaluator.ToString();
                        
                        
                        if (bestEvaluator.CanTakeGoal())
                        {
                            if (bestEvaluator.CancelCurrentTakers())
                            {
                                The.Sim.Controller.SaveOrVerifyEntityAIState(newAIStateAsString);

                                bestEvaluator.PreSetGoal();

                                RemoveAllSubgoals(); // switch goal!
                        
                                bestEvaluator.SetGoal();

                                ResetScoreAndCounter();

                                return true;
                            }
                        }
                    }
                }
            }

            ResetScoreAndCounter();

            return false;
        }

        /// <summary>
        /// contains copied code!!!
        /// </summary>
        public override void RemoveAllSubgoals()
        {
            int noOfSubgoals = Subgoals.Count;
            string subgoalName = "";
            if (noOfSubgoals > 0)
            {
                subgoalName = Subgoals.Peek().ToString();
            }

           
            // NOTE: copied code instead of calling base:
            while (Subgoals.Count > 0)
            {
                Goal subGoal = RemoveFirstSubgoal();

                subGoal.Terminate();
                
            }


            if (noOfSubgoals > 0)
            {
                entity.DebugLog.Add(string.Format("GoalThink.RemoveAllSubgoals, count: {0}, first goal: {1}", noOfSubgoals, subgoalName));
            }

        }


        /// <summary>
        /// InUseBy not being cleared - I believe this error is now fixed!
        /// </summary>
        /// <param name="subGoal"></param>
      /*  private void AssertAllLocksReleased(Goal subGoal)
        {
           
            ITopLevelGoal topLevelGoal = subGoal as ITopLevelGoal;
            if (topLevelGoal != null)
            {
                // assert that no locks are held by this agent.
                foreach (var item in entityIntelligence.CurrentExpedition.OwnedEntities.AllEntities)
                {
                    foreach (var item2 in item.Value)
                    {
                        if (item2 == (EntityID)24023)
                        {

                        }
                        IKnownEntityData entityData;
                        entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(item2, out entityData);
                        //Entity itemEntity = Entity.FindByID(item2);
                        if (entityData != null)
                        {
                            //CurrentExpedition.OwnedEntities: 38
                            // inUseBy: 37
                            EntityID? inUseByExpedition = entityData.GetInUseBy(entityIntelligence.CurrentExpedition.OwnedEntities);
                            EntityID? inUseByAllegiance = entityData.GetInUseBy(entityIntelligence.Allegiance.SharedKnowledge.AllKnownEntities);
                            if (inUseByExpedition == entity.ID
                                || inUseByAllegiance == entity.ID)
                            {
                                System.Diagnostics.Debug.Assert(false, "Forgot to release a lock on an item?");

                                // patch this error so the player can continue - remove the whole function once the error is corrected...
                                entityData.ClearInUseBy(entityIntelligence.CurrentExpedition.OwnedEntities, entity.ID);
                                entityData.ClearInUseBy(entityIntelligence.Allegiance.SharedKnowledge.AllKnownEntities, entity.ID);

                            }

                        }
                    }
                }


            }
        }*/

        public double ScoreTopLevelGoal()
        {
            ITopLevelGoal iTopLevelGoal = GetTopLevelGoal();

            if (iTopLevelGoal != null)
            {
                // added this condition because a failed goal may no longer be in state to score itself
                //- for instance a hauling job will have been cancelled and the item unassigned
                //- this will cause a crash if try to score it.

                Goal topLevelGoal = iTopLevelGoal as Goal;

                if (Status == Goals.Status.Failed
                    || Status == Goals.Status.Completed
                    || topLevelGoal.Status == Goals.Status.Failed // not sure if these are needed now that we are more proactive in cleaning up terminatet goals...
                    || topLevelGoal.Status == Goals.Status.Completed)
                {
                    return 0.0;
                }
                else
                {
                    double score = iTopLevelGoal.ScoreGoal(); // ScoreThisTopLevelGoal();


                    double inertia = ScoreInertia(); 

                    score += inertia;

                    return score;
                }
            }
            else return 0.0;

        }


        /// <summary>
        /// returns a number to add to the score that depends on how long the top level goal has been in effect
        /// </summary>
        /// <returns></returns>
        private double ScoreInertia()
        {          

            double inertia;
            ITopLevelGoal topLevelGoal = GetTopLevelGoal();

            if (topLevelGoal != null) // Added a null check to prevent this crash: http://steamcommunity.com/app/284100/discussions/2/405694115197789476/
            {
                double timeSpentInCurrentGoal = topLevelGoal.TimeSpentInTopLevelGoal; 

                if (timeSpentInCurrentGoal < GameData.Instance.AIConstants.TimeToReachFullGoalSwitchInertia)
                {
                    float inertiaFactor = Common.Clamp((float)timeSpentInCurrentGoal / GameData.Instance.AIConstants.TimeToReachFullGoalSwitchInertia, 0f, 1f);
                    inertiaFactor = inertiaFactor * inertiaFactor; // square it, to make the value increase slowly at first

                    inertia = MathHelper.Lerp(0, GameData.Instance.AIConstants.CurrentGoalInertia, inertiaFactor);
                }
                else
                {
                    inertia = GameData.Instance.AIConstants.CurrentGoalInertia;
                }
            }
            else
            {
                // if there are no goals... let's return 0
                inertia = 0d;
            }

            return inertia;
        }

       


        private void SortTopScoringJobsForDebugging()
        {
            entityIntelligence.TopScoringJobs.Sort((j1, j2) => j2.Score.CompareTo(j1.Score));
        }

      

        /// <summary>
        /// returns a number to add to the score that depends on how long the top level goal has been in effect
        /// </summary>
        /// <returns></returns>
     /*   private double ScoreInertia()
        {
            double amountNewGoalMustBeBetterToSwitch;
            double timeSpentInCurrentGoal = Subgoals.Peek().TimeSpentInGoal;
            if (timeSpentInCurrentGoal < GameData.Instance.AIConstants.TimeToReachFullGoalSwitchInertia)
            {
                float inertiaFactor = Common.Clamp((float)timeSpentInCurrentGoal / GameData.Instance.AIConstants.TimeToReachFullGoalSwitchInertia, 0f, 1f);
                inertiaFactor = inertiaFactor * inertiaFactor; // square it, to make the value increase slowly at first

                amountNewGoalMustBeBetterToSwitch = MathHelper.Lerp(0, GameData.Instance.AIConstants.AmountNewGoalMustBeBetterToSwitch, inertiaFactor);
            }
            else
            {
                amountNewGoalMustBeBetterToSwitch = GameData.Instance.AIConstants.AmountNewGoalMustBeBetterToSwitch;
            }

            return amountNewGoalMustBeBetterToSwitch;
        }*/

   

        private void ResetScoreAndCounter()
        {
            bestEvaluator = null;
            bestScore = 0d;

            evaluatorBeingProcessed = 0;
        }

        long lastUnpausedTimePoint = 0;
        private void UpdateTimeSpentIdling(GoalEvaluator mostDesirable)
        {
           
            if (previousMostDesirable != null)
            {
                if (previousMostDesirable == mostDesirable)                  
                {  
                    long deltaTime = The.Sim.TotalUnPausedGameTime.Ticks - lastUnpausedTimePoint;

                    lastUnpausedTimePoint = The.Sim.TotalUnPausedGameTime.Ticks;

                  
                    if (mostDesirable.IsIdleActivity())
                    {
                        entityIntelligence.Memory.TimeSpentIdling = entityIntelligence.Memory.TimeSpentIdling.Add(TimeSpan.FromTicks(deltaTime));
                    }
                    else
                    {
                        entityIntelligence.Memory.TimeSpentIdling = new TimeSpan(0);
                    }
                }
                else
                {   
                   
                    entityIntelligence.Memory.TimeSpentIdling = new TimeSpan(0);
                }
            }

            previousMostDesirable = mostDesirable;
           
        }

        /// <summary>
        /// made this for non-job goals
        /// where type is ITopLevelGoal
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsSame(Type type) //where type is ITopLevelGoal...
        {
            if (Subgoals.Count > 0)
            {
                return Subgoals.Peek().GetType() == type;              
            }

            return false;
        }

        public bool CanDropRequestedItem(Entity item)
        {
            if (Subgoals.Count > 0)
            {
                return Subgoals.Peek().CanDropRequestedItem(item);
            }

            return false;

        }

        private bool IsIdle()
        {
            if (Subgoals.Count > 0)
            {
                // these are goals that can be interrupted in the middle and cancelled safely.
                if (Subgoals.Peek() is GoalDoTakeFive)// GoalWander;
                {
                    return true;
                }
                else return false;
            }

            return false;
        }

        //---------------------------- notPresent --------------------------------------
        //
        //  returns true if the goal type passed as a parameter is the same as this
        //  goal or any of its subgoals
        //-----------------------------------------------------------------------------
        public bool NotPresent(Type type)
        {
            if (Subgoals.Count > 0)
            {
                return Subgoals.Peek().GetType() != type;
            }

            return true;
        }


        public float GetExertionLevelOfActivity()
        {
            if (Subgoals.Count > 0)
            {
               // PhysicalWork work;
                //work =
                return Subgoals.Peek().GetExertionLevel();

                /*
                switch (work)
                {
                    case PhysicalWork.None:
                        return 1f; // (1f / 4f) * GameData.Instance.Constants.MaximumFoodEnergyDecreaseFactorFromActivity; // 3f;2f;;
                    case PhysicalWork.Light:
                        return (1f / 3f) * GameData.Instance.Constants.MaximumFoodEnergyDecreaseFactorFromActivity; 
                    case PhysicalWork.Medium:
                        return (2f / 3f) * GameData.Instance.Constants.MaximumFoodEnergyDecreaseFactorFromActivity; 
                    case PhysicalWork.Hard:
                        return GameData.Instance.Constants.MaximumFoodEnergyDecreaseFactorFromActivity; 

                }*/
            }

            return 1f;
        }


        public float GetStealthFactorOfActivity()
        {
            float constant;
            if (entity.EntityType.BiologicalType != null)
            {
                constant = entity.BiologicalEntity.PassiveStealthRating;
            }
            else
            {
                constant = GameData.Instance.Constants.DefaultPassiveStealthFactor;
            }

            if (Subgoals.Count > 0)
            {
                StealthFactor factor;
                factor = Subgoals.Peek().GetStealthFactor();

                
                switch (factor)
                {
                    case StealthFactor.None:
                        return constant; 

                    case StealthFactor.NotGood:
                        return constant * 0.5f;

                    case StealthFactor.Bad:
                        return constant * 0.25f;

                    case StealthFactor.ExtremelyBad:
                        return 0f;

                }
            }

            return constant;
        }

        
        /// <summary>
        /// 0 - ?
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public float GetDetectionFactorOfActivity(IDetectable detectable, bool requiresExamineAction) 
        {
            float bestFactorWhenNotLooking = GameData.Instance.Constants.BestDetectionFactorOfActivityWhenNotLooking; //0.2f;

            if (Subgoals.Count > 0)
            {
                DetectionFactor factor;

                float bestDetectionFactor = GameData.Instance.Constants.BestDetectionFactorOfActivityWhenSearching;

                Entity detectableEntity = detectable as Entity;
                if (detectableEntity != null) // && detectableEntity.EntityType.IntelligenceType != null) //entityType.IntelligenceType != null)
                {
                    factor = Subgoals.Peek().GetDetectAgentsFactor(detectableEntity.EntityType, requiresExamineAction);
                }
                else
                {
                    // some resources are only seen when actively searching
                    factor = Subgoals.Peek().GetDetectResourcesFactor(detectable.ResourceType, requiresExamineAction);
                }

                float finalValue = 0f;
                switch (factor)
                {
                    case DetectionFactor.CannotDetect:
                        finalValue = 0f;
                        break;

                    case DetectionFactor.DetectSome:
                        finalValue = MathHelper.Lerp(bestFactorWhenNotLooking, bestDetectionFactor, 0f);
                        break;

                    case DetectionFactor.DetectGood:
                        finalValue = MathHelper.Lerp(bestFactorWhenNotLooking, bestDetectionFactor, 0.5f);
                        break;

                    case DetectionFactor.DetectVeryGood:
                        finalValue = MathHelper.Lerp(bestFactorWhenNotLooking, bestDetectionFactor, 1f);
                        break;
                }

                return finalValue; // Common.Clamp(finalValue, 0f, 1f);
            }
            else
            {
                return 0f; // don't detect anything before AI has started up
            }
        }

       

        public override void Terminate()
        {
            // PLEASE remember to always call base.Terminate to ensure that all subgoals get terminated!!!
            RemoveAllSubgoals();
            base.Terminate();
        }

        public bool SendMessage(Message msg)
        {
            bool wasHandled = HandleMessage(msg);
            if (wasHandled)
            {                
                // NEW: Terminate goals immediately, instead of on the following frame (otherwise Snapshot will cause a crash):
                bool hasFailedSubgoal = false;
                CleanupSubgoals(Status, ref hasFailedSubgoal);
            }

            return wasHandled;
        }

        public override bool HandleMessage(Message message)
        {
            // NEW
            //first, pass the message down the goal hierarchy
            bool handled = ForwardMessageToFrontMostSubgoal(message);

            // always handle these evaluator messages regardless of what the subgoal did to handle them - this is done to prevent bugs where a subgoal (like GoalBeingHit) returns true because it wants to ignore messages:
            switch (message.MessageType)
            {
                case Message.MessageTypes.DistanceFound:
                case Message.MessageTypes.DistanceFoundNoAccess:
                case Message.MessageTypes.BestCropFound:
                    // evaluator can continue now
                    waitingForEvaluator = false; // sometimes, the distance result will have been for a subgoal, and not for the evaluator... but that doesn't hurt that much. 
                                                // worst case is that the evaluators get the same Wait result again...

                    The.Sim.WaitingAgents.Remove(entity.ID);

                    return true;
            }

            if (handled == false)
            {
              
                    switch (message.MessageType)
                    {
                       /* case Message.MessageTypes.DinnerIsReady:
                            // interrupt:
                            //Status = Status.Completed;

                            Entity meal = ((Entity)message.OtherInfo);
                            meal.InUseBy = entity.ID;

                            // go for the meal:
                            //clear any existing goals
                            // RemoveAllSubgoals();

                            // see if we can add the goal as next in line:
                            List<Owner> householdAndPrivateOwnersOfVehicles = new List<Owner>();
                            householdAndPrivateOwnersOfVehicles.Add(personEntity.PrivateOwnership);
                            householdAndPrivateOwnersOfVehicles.Add(personEntity.Household.Ownership);

                            AddSubgoal(new GoalEat(entity, meal.ID, null, householdAndPrivateOwnersOfVehicles));
                            
                            return true; //msg handled
                            */
                      
                        case Message.MessageTypes.OtherAgentRequestsDropItem:
                            // before this message is sent, CanDropRequestedItem has to return true

                            // we can even drop while sleeping or dying, hmm. But then we shouldn't be carrying stuff
                            Entity itemToDrop = ((Entity)message.OtherInfo);

                            // only intelligent agents may drop items:
                            entity.AgentStorage.Uncontain(itemToDrop);

                            List<ActionSets> defaultActionSets;
                            entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.ForceDropsItem, out defaultActionSets); 

                            Goal.FireEventActions(entity, null, defaultActionSets, null);


                            return true;
                        case Message.MessageTypes.Hit:
                            float damageDone = ((float)message.OtherInfo);

                            //If we are being attacked by an agent we want to create threat jobs to defend ourselves.
                            if (message.Sender != null && message.Sender.EntityType.IntelligenceType != null)
                            {
                                entity.Intelligence.Allegiance.ThreatAndCombatJobManager.CreateThreatJobsFromAttackOutOfBand(message.Sender, entity);

                                entity.Intelligence.Memory.RememberAttacker(message.Sender.EntityID);                                
                            }

                            BodyComponent body;                           
                            entity.Find(out body);

                            if (damageDone > GameData.Instance.Constants.DamageAmountFractionCausingHitReaction * body.Body.MaxHitpoints)
                            {
                                if (The.Sim.TotalUnPausedGameTimeInSeconds > 17 &&
                                     entity.ID == (Entities.EntityID)19)
                                {

                                }

                                // is this needed???
                                RemoveAllSubgoals();

                                AddSubgoal(new GoalBeingHit(entity));                            
                            }

                            if (entityIntelligence.Statistics != null && Common.IsGreaterThan(damageDone, 0d))
                            {
                                entity.LogInjuryStatistics(message.Sender);
                            }

                            List<ActionSets> defaultActions;
                            entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.TakingAHit, out defaultActions);

                            Goal.FireEventActions(entity, null, defaultActions);

                            return true;
                        case Message.MessageTypes.HitAndCollapse:
                            RemoveAllSubgoals();
                            OwnerID? ownerOfCarcass = ((OwnerID?)message.OtherInfo);
                            Entity killer = message.Sender;
                            AddSubgoal(new GoalCollapse(entity, ownerOfCarcass, killer.EntityID));
                            AddSubgoal(new GoalIsDying(entity, ownerOfCarcass,true)); // this may never execute if the Collapse goal determines that we're dead.

                            return true;

                        case Message.MessageTypes.AlertToPresence: // alerted by loud attack noises etc.

                            if (message.Sender != null)
                            {                               
                               // entityIntelligence.Allegiance.SharedKnowledge.SeeDetectable(message.Sender, true, false, null, entity);
                                entityIntelligence.Allegiance.SharedKnowledge.SeeDetectableIfRelevant(message.Sender, true, false, null, entity);
                            }

                            return true;

                        case Message.MessageTypes.Interest:
                            // the goals do not need to handle this and return false if they prohibit head turns, it is sufficient to set the property CanReactToInterest
                            Goal frontMostGoal = GetFrontMostGoal();
                            bool canTurnHead = frontMostGoal.CanReactToInterest();

                            if (canTurnHead)
                            {
                                EntityID? interestingEntity;
                                Vector3? interestingLocation;
                                float? interest = GetInterestAndHandleTrigger(message, out interestingEntity, out interestingLocation);

                                if (interest.HasValue)
                                {
                                    /*
                                     if (entityIntelligence.InterestIsHighEnough(message.Sender.EntityID, interest.Value))
                                     { */

                                    // only turn head:
                                    entityIntelligence.SetNewCenterOfAttention(interestingEntity, interestingLocation, interest.Value);
                                    //}
                                }
                            }

                            return true;


                        case Message.MessageTypes.Disembark: // temporary...?
                            RemoveAllSubgoals();
                            AddSubgoal(new GoalExit(entity, true)); // better to use GoalExitVehicle..? Probably not. The container speicific code should be in VehicleContainer.
                            
                            return true; 
                    }               
            }

            return handled;

        }


        /// <summary>
        /// also calls OnEnter if the goal is the first in the queue.
        /// Also if this is the first goal in the queue it will activate GoalThink. 
        /// </summary>
        /// <param name="g"></param>
        public override void AddSubgoal(Goal g)
        {
            if (entity.PersonEntity != null && g is GoalWait /*&& Subgoals.Count > 2 && Subgoals.Peek() is GoalWait*/)
            {

            }
            //add the new goal to the end of the list

            bool firstSubgoal = Subgoals.Count == 0;

            if (ID == GoalID.Invalid)
            {
                //This was happening to goal attack repeatedly. Solve this first then add assert again. Both here and in Compositegoal (Why duplicate places with this code?)
                System.Diagnostics.Debug.Assert(ID != GoalID.Invalid, "Invalid ID. Never add subgoal to Goal with invalid ID.");
                return;
            } 

            Subgoals.Enqueue(g);

            if (firstSubgoal)
            {
                // This was added due to an problem that happend when GoalThink was inactive.
                // And an entity died and added subgoals to GoalThink (GoalIsDying etc.)
                // Then becaouse of GoalThink being inactive it arbitrated and removed subgoals, 
                // this includes removing GoalIsDying and this made the creature avoid dying. 
                // So now when we add an subgoal to an empty GoalThink we will activate it.
                Status = Goals.Status.Active; //We got an goal to process, we are now activated.
                g.EnterIfNew();
            }
        }

        /*
        //Logs the injury in Security Statistics
        private void LogInjuryStatistics(Entity attacker)
        {
            if (entityIntelligence.Stats != null)
            {

                if (entity.PersonEntity != null)
                {

                }

                SecurityStatistics securityStat = (SecurityStatistics)entityIntelligence.Stats.Stats[StatTypes.Security];

                securityStat.AddViolentEvent(entity, "Injured by " + attacker.ToString());
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
            // don't snapshot evaluator state, restart them all post-load

            base.DoSnapshot(sn);

            this.hasArbitratedWhileBusy = sn.DoBool(hasArbitratedWhileBusy); // save flag this so we know if an inactive goal has been placed on the queue and we should not arbitrate again
            this.lastUnpausedTimePoint = sn.DoInt64(lastUnpausedTimePoint);

            
            sn.Ignore(previousMostDesirable);
            sn.Ignore(evaluators);
            sn.Ignore(waitingForEvaluator); // the evaluators have been restarted after post load..
            sn.Ignore(evaluatorBeingProcessed);
            sn.Ignore(bestScore);
            sn.Ignore(bestEvaluator);

            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            // recreate all evaluators post-load:
            // this may have happened already in a call from a subgoal.
            if (evaluators.Count == 0)
            {
                ResetEvaluators();
            }
            

        }

        #endregion
    }


    


    public class DebugGoalPlan
    {
        public DebugGoalPlan(GoalPlanner plan)
        {
            GoalPlanner = plan;
        }
        public GoalPlanner GoalPlanner
        {
            get;
            set;
        }
    }
    public class DebugJobEvaluator : GoalEvaluator, IScoreJob
    {


        public DebugJobEvaluator(Entity entity)
            : base(entity)
        {
        }

        public override bool SetGoal()
        {
            DebugGoalPlan plan = entity.DebugGoalPlan;
            plan.GoalPlanner(entity, this);

            return true;
        }

        /// <summary>
        /// the minimum rating should include Priority, so all comparisons for the sake of culling results should include Priority too.
        /// </summary>
        /// <param name="minimumRatingToConsider"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            result = double.MaxValue;
            return CalculateResult.Done;
        }

        public CalculateResult ScoreThisJob(UWGame.SimSide.Maps.RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, Jobs.Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating,
    ToolParams? toolParams, AttackParams? attackParams, HaulingParams? haulingParams)
        {
            rating = double.MaxValue;
            return CalculateResult.Done;
        }

        public override bool CancelCurrentTakers()
        {
            return true;// throw new NotImplementedException();
        }

        public override bool  CanTakeGoal()
        {
 	        return true;
        }

    }

}
