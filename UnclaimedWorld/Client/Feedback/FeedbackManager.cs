using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Systems;

namespace UWGame.ClientSide.Feedback
{
    /// <summary>
    /// Feedback collections - use SleepyUpdater to enable timeout/cleanup. Not used by Sim.
    /// </summary>
    public class FeedbackManager
    {     

        /// <summary>
        /// data is only maintained for the UIAllegiance, for the purpose of player feedback.
        /// 
        /// cleanup:
        /// job: on Destroy
        /// entity: on Destroy Entity and Destroy MemoryFact
        ///  
        /// (if UI allegiance is ever switched while playing, these collections should be cleared)
        /// </summary>
        private Dictionary<JobID, AccessibilityFeedback> jobAccessibility = new Dictionary<JobID, AccessibilityFeedback>();
        private Dictionary<EntityID, AccessibilityFeedback> entityAccessibility = new Dictionary<EntityID, AccessibilityFeedback>();

        SleepyUpdater<AccessibilityFeedback> accessibilityUpdater = new SleepyUpdater<AccessibilityFeedback>(Module.Client);
        SleepyUpdater<ReplenishFeedback> replenishUpdater = new SleepyUpdater<ReplenishFeedback>(Module.Client);

        Dictionary<ExpeditionID, Dictionary<EntityType, ReplenishFeedback>> toolReplenish = new Dictionary<ExpeditionID, Dictionary<EntityType, ReplenishFeedback>>();
       // Dictionary<EntityType, bool> toolReplenishAvailableStates = new Dictionary<EntityType, bool>();

        private Regulator replenishAlertRegulator;



        public FeedbackManager()
        {
            replenishAlertRegulator = new Regulator(The.Client.ClientRandomGenerator /*The.Sim.GameplayRandomGenerator*/, 1d / GameData.Instance.GUIConstants.TimeBetweenReplenishAlerts, "FeedbackManager"); // "Expedition replenish");
      
        }


        public void Update(GameTime gameTime)
        {
            accessibilityUpdater.Update(gameTime);

            replenishUpdater.Update(gameTime);
        }


        #region Replenish

        /// <summary>
        /// call this from AI evaluators to give player feedback without extra overhead
        /// </summary>
        /// <param name="toolType"></param>
        /// <param name="status"></param>
        public void SetToolReplenishStatusOwnsItem(Expedition expedition, EntityType toolType, bool ownsItem, float? requiredAmount) // ReplenishStatus status)
        {
            // owns item can be true, but itemAvailable false, but not the other way
            // remove if itemAvailable is true
            ReplenishFeedback feedback = GetExistingReplenishFeedbackOrCreateNew(expedition, toolType);
            feedback.SetOwnsItem(ownsItem, requiredAmount);
          

            if (!ownsItem)
            {               
                if (replenishAlertRegulator.IsReady())
                {
                    The.Client.LogOutOfFuel(toolType, feedback.MinimumAmountRequired); // show an alert as well
                }
            }

        }

        public void SetToolReplenishStatusItemIsAvailable(Expedition expedition, EntityType toolType, bool itemIsAvailable, float? requiredAmount) // ReplenishStatus status)
        {
            // owns item can be true, but itemAvailable false, but not the other way
           
            // remove if itemIsAvailable is true
            if (itemIsAvailable)
            {
                ReplenishFeedback feedback = GetToolReplenishFeedback(expedition, toolType);
                if (feedback != null)
                {
                    DestroyReplenishFeedback(feedback);
                }
            }
            else
            {
                ReplenishFeedback feedback = GetExistingReplenishFeedbackOrCreateNew(expedition, toolType);
                feedback.SetItemIsAvailable(itemIsAvailable, requiredAmount);

                if (replenishAlertRegulator.IsReady())
                {
                    The.Client.LogOutOfFuel(toolType, feedback.MinimumAmountRequired); // show an alert as well
                }
            }

        }

        /* OLD:
        public void SetToolReplenishStatus(Expedition expedition, EntityType toolType, bool ownsItem) // ReplenishStatus status)
        {
          
            // remove if ownsItem is true
            if (ownsItem)
            {
                ReplenishFeedback feedback = GetToolReplenishFeedback(expedition, toolType);
                if (feedback != null)
                {
                    DestroyReplenishFeedback(feedback);
                }
            }
            else
            {
                if (replenishAlertRegulator.IsReady())
                {
                    The.Client.LogOutOfFuel(toolType); // show an alert as well
                }

                ReplenishFeedback feedback = GetExistingReplenishFeedbackOrCreateNew(expedition, toolType);
                feedback.RefreshExpiry(); // (ownsItem);
            }
           
        }*/

        /// <summary>
        /// New, with two flags
        /// </summary>
        /// <param name="expedition"></param>
        /// <param name="toolType"></param>
        /// <returns></returns>
        public bool GetToolReplenishStatus(Expedition expedition, EntityType toolType, out float? minimumRequiredAmount)
        {
            ReplenishFeedback feedback = GetToolReplenishFeedback(expedition, toolType);

            if (feedback != null)
            {
                if (feedback.OwnsItem == false || feedback.ItemIsAvailable == false)
                {
                    minimumRequiredAmount = feedback.MinimumAmountRequired;
                    return false;
                }
            }

            minimumRequiredAmount = null;
            return true;  // true is default...
        }

        // OLD:
        /*public bool GetToolReplenishStatus(Expedition expedition, EntityType toolType)
        {
            ReplenishFeedback feedback = GetToolReplenishFeedback(expedition, toolType);
            
            if (feedback != null)
            {
                return false; // feedback.OwnsItem;
            }

            return true;  // true is default...
        }*/

        private ReplenishFeedback GetToolReplenishFeedback(Expedition expedition, EntityType toolType)
        {
            Dictionary<EntityType, ReplenishFeedback> states;
            if (toolReplenish.TryGetValue(expedition.ID, out states)) // ownsItem))
            {
                ReplenishFeedback feedback; 
                if (states.TryGetValue(toolType, out feedback))
                {
                    return feedback;
                }
            }
            
            return null; 

           
            /*bool ownsItem; // 
            if (toolReplenishAvailableStates.TryGetValue(toolType, out ownsItem))
            {
                return ownsItem;
            }
            else return true; // true is default...*/
        }

        private ReplenishFeedback GetExistingReplenishFeedbackOrCreateNew(Expedition expedition, EntityType toolType) //, bool createNew)
        {
            ReplenishFeedback feedback = GetToolReplenishFeedback(expedition, toolType);

            if (feedback == null)
            {
               /* if (createNew) // no reason to create the object if the flag is not remarkable...
                {*/

                feedback = new ReplenishFeedback(expedition, toolType);
                replenishUpdater.Add(feedback);
                Common.AddToNestedDictionary(toolReplenish, expedition.ID, toolType, feedback);
                  
               // }
            }

            return feedback;
        }



        #endregion

        public void GetFeedback(JobID jobID, out bool isInAccessible, out bool isBlockedByThreat, out bool isBlockedDueToBoldStanceRequired, out bool tooFarFromExpedition, out bool huntingNotFeasible, out bool areaNotCleared)
        {
            AccessibilityFeedback accessibility;
            if (jobAccessibility.TryGetValue(jobID, out accessibility))
            {
                isInAccessible = accessibility.IsInaccessible;
                isBlockedByThreat = accessibility.IsBlockedByThreat;
                isBlockedDueToBoldStanceRequired = accessibility.IsBlockedDueToBoldStanceRequired;
                tooFarFromExpedition = accessibility.TooFarFromExpedition;
                huntingNotFeasible = accessibility.HuntingJobNotFeasible;
                areaNotCleared = accessibility.AreaNotCleared;


                return;
            }

            isBlockedDueToBoldStanceRequired = false;
            isInAccessible = false;
            isBlockedByThreat = false;
            tooFarFromExpedition = false;
            huntingNotFeasible = false;
            areaNotCleared = false;
        }

        public void GetFeedback(EntityID entityID, out bool isInAccessible, out bool isBlockedByThreat, out bool isBlockedByBoldStance)
        {
            AccessibilityFeedback accessibility;
            if (entityAccessibility.TryGetValue(entityID, out accessibility))
            {
                isInAccessible = accessibility.IsInaccessible;
                isBlockedByThreat = accessibility.IsBlockedByThreat;

                isBlockedByBoldStance = accessibility.IsBlockedDueToBoldStanceRequired;

                return;
            }

            isInAccessible = false;
            isBlockedByThreat = false;

            isBlockedByBoldStance = false;
        }

        public bool GetIsInaccessible(JobID jobID)
        {
            AccessibilityFeedback accessibility;
            if (jobAccessibility.TryGetValue(jobID, out accessibility))
            {
                return accessibility.IsInaccessible;
            }

            return false;
        }

        public bool GetIsBlockedByThreat(JobID jobID)
        {
            AccessibilityFeedback accessibility;
            if (jobAccessibility.TryGetValue(jobID, out accessibility))
            {
                return accessibility.IsBlockedByThreat;
            }

            return false;
        }

        public void SetJobInaccessible(Job job, IHasEntityGroup ownerOfJob, bool isInaccessible)
        {
            if (isInaccessible == true && job.TakenBy.Count > 0)
            {
                return; // Do not set if someone is doing the job. Then it is accessible.
            }

            AccessibilityFeedback accessibility = GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, isInaccessible == true);

            if (accessibility != null)
            {
                accessibility.SetIsInaccessible(ownerOfJob, isInaccessible);
            }
        }

        public void SetHuntingJobNotFeasible(Job job, IHasEntityGroup ownerOfJob, bool value)
        {
            if (value == true && job.TakenBy.Count > 0)
            {
                return; // Do not set if someone is doing the job. Then it is accessible.
            }

            AccessibilityFeedback accessibility = GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, value == true);
            if (accessibility != null)
            {
                accessibility.SetHuntingJobNotFeasible(ownerOfJob, value);
            }

        }

        public void SetAreaNotCleared(Job job, IHasEntityGroup ownerOfJob, bool value)
        {
            AccessibilityFeedback accessibility = GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, value == true);
            if (accessibility != null)
            {
                accessibility.SetAreaNotCleared(value);
            }

        }

        public void SetJobTooFarFromExpedition(Job job, IHasEntityGroup ownerOfJob, bool value)
        {
            if (value == true && job.TakenBy.Count > 0)
            {
                return; // Do not set if someone is doing the job. Then it is accessible.
            }

            AccessibilityFeedback accessibility = GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, value == true);
            if (accessibility != null)
            {
                accessibility.SetTooFarFromExpedition(value);
            }

        }

        public void SetJobBlockedByBoldStance(Job job, IHasEntityGroup ownerOfJob, bool isBlocked)//TEMP Name
        {

            if (isBlocked == true && job.TakenBy.Count > 0)
            {
                return; // Do not set if someone is doing the job. Then it is accessible.
            }

            AccessibilityFeedback accessability = GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, isBlocked == true);
            if (accessability != null)
            {
                accessability.SetBlockedByBoldStance(ownerOfJob, isBlocked);
            }

        }



        public void SetJobBlockedByThreat(Job job, IHasEntityGroup ownerOfJob, bool isBlocked)
        {
            if (isBlocked == true && job.TakenBy.Count > 0)
            {
                return; // Do not set if someone is doing the job. Then it is accessible.
            }

            AccessibilityFeedback accessibility = GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, isBlocked == true);

            if (accessibility != null)
            {
                accessibility.SetBlockedByThreat(ownerOfJob, isBlocked);
            }

        }

        public void DestroyReplenishFeedback(ReplenishFeedback feedback)
        {
            replenishUpdater.Remove(feedback);
            Common.RemoveFromNestedDictionary(toolReplenish, feedback.Expedition, feedback.EntityType);            
        }

        public void DestroyAccessibility(AccessibilityFeedback accessibility)
        {
            accessibilityUpdater.Remove(accessibility);

            if (accessibility.JobID.HasValue)
            {
                jobAccessibility.Remove(accessibility.JobID.Value);
            }
            else if (accessibility.EntityID.HasValue)
            {
                entityAccessibility.Remove(accessibility.EntityID.Value);
            }

        }

        public void DestroyAccessibility(JobID jobID)
        {
            AccessibilityFeedback accessibility;
            if (jobAccessibility.TryGetValue(jobID, out accessibility))
            {
                jobAccessibility.Remove(jobID);
                accessibilityUpdater.Remove(accessibility);
            }
        }

        public void DestroyAccessibility(EntityID entityID)
        {
            AccessibilityFeedback accessibility;
            if (entityAccessibility.TryGetValue(entityID, out accessibility))
            {
                entityAccessibility.Remove(entityID);
                accessibilityUpdater.Remove(accessibility);
            }
        }

        public void HandleDestroyedEntity(EntityID entityID)
        {
            //if the entity is destroyed, and no memory facts exist, remove accessibility data:
            if (The.InGameUI.UIAllegiance == null
                || !The.InGameUI.UIAllegiance.SharedKnowledge.MemoryFacts.ContainsKey(entityID))
            {
                DestroyAccessibility(entityID);
            }
        }

        private AccessibilityFeedback GetExistingJobAccessibilityOrCreateNew(Job job, IHasEntityGroup ownerOfJob, bool createNew)
        {
            if (ownerOfJob.Allegiance == The.InGameUI.UIAllegiance)
            {
                AccessibilityFeedback accessibility = null;

                if (!jobAccessibility.TryGetValue(job.ID, out accessibility))
                {
                    if (createNew)
                    {
                        accessibility = new AccessibilityFeedback(job.ID);
                        accessibilityUpdater.Add(accessibility);
                        jobAccessibility.Add(job.ID, accessibility);
                    }
                }


                return accessibility;
            }

            return null;
        }


        private AccessibilityFeedback GetExistingEntityAccessibilityOrCreateNew(IKnownEntityData entityData, bool createNew)
        {

            AccessibilityFeedback accessibility;

            if (!entityAccessibility.TryGetValue(entityData.EntityID, out accessibility))
            {
                if (createNew) // no reason to create the object if the flag is not remarkable...
                {
                    accessibility = new AccessibilityFeedback(entityData.EntityID);
                    accessibilityUpdater.Add(accessibility);
                    entityAccessibility.Add(entityData.EntityID, accessibility);
                }
            }

            return accessibility;

        }

        public void SetEntityInaccessible(Allegiance agentAllegiance, IKnownEntityData entityData, bool isInaccessible)
        {
            if (agentAllegiance != The.InGameUI.UIAllegiance)
                return;

            if (entityData.EntityType.IntelligenceType != null)
            {
                return;//We do not want to do this on agents.
            }

            AccessibilityFeedback accessibility = GetExistingEntityAccessibilityOrCreateNew(entityData, isInaccessible == true);

            if (accessibility != null)
            {
                // Lars: using the owner to give client feedback is hacky and ugly. But adding a parameter would be a huge amount of work due to the widespread use of GetDistance...
                IOwner owner;
                if (LookUpOwners.ResolveEntityOwner(entityData, out owner)
                    && owner != null)
                {
                    IHasEntityGroup hasEntityGroup = owner as IHasEntityGroup; // #### HACK
                    if (hasEntityGroup != null)
                    {
                        accessibility.SetIsInaccessible(hasEntityGroup, isInaccessible);

                        if (isInaccessible == false)
                        {
                            // clear the threat flag also:
                            accessibility.SetBlockedByThreat(hasEntityGroup, isInaccessible);
                        }
                    }
                }
            }
        }

        public void SetEntityBlockedByThreat(Allegiance agentAllegiance, IKnownEntityData entityData, bool blockedByThreat)
        {
            if (agentAllegiance != The.InGameUI.UIAllegiance)
                return;

            if (entityData.EntityType.BiologicalType != null)
            {
                return; //We do not want to do this on living things.
            }

            if (entityData.EntityType.IntelligenceType != null)
            {
                return; //We do not want to do this on agents.
            }


            AccessibilityFeedback accessibility = GetExistingEntityAccessibilityOrCreateNew(entityData, blockedByThreat == true);

            if (accessibility != null)
            {
                EntityGroup entityOwner;
                if (LookUpOwners.ResolveEntityOwner(entityData, out entityOwner)
                    && entityOwner != null)
                {
                    accessibility.SetBlockedByThreat(entityOwner.Parent, blockedByThreat);
                }
            }
        }

    }
}
