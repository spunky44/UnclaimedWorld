using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework.Content;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using System.Diagnostics;

namespace UWGame.SimSide.InGameEvents.Actions
{
    public enum ActionSetsToFire { FirstValid, RandomValid, AllValid }

    /// <summary>
    /// these actions can be fired from the global condition poller, or be pushed from/by entities/behaviours/processes from their various lifetime hooks
    /// 
    /// the nested objects make randomization easier...
    /// </summary>
    [DebuggerDisplay("{KeyName}")]
    public class ActionSets: IGameData
    {
        /// <summary>
        /// place the description here, then it will be preserved for player modding when serialized to xml.
        /// </summary>
        public string Comments;

        /// <summary>
        /// if filled, it sets the chance that this whole set will fire. After that, static conditions and random chances on the individual sets are evaluated.
        /// 0 - 1
        /// 
        /// </summary>
        public float? ChanceToFire;
        public EvalNode DynamicChanceToFire;

        /// <summary>
        /// set this to true to suppress talk/speak when spawning farm plots and fish traps at the beginning
        /// </summary>
        public bool SupressWhenSpawning = false;

        /// <summary>
        /// controls how many actionSets are fired. Either the first valid, a random valid or all valid 
        /// 
        /// Random valid: all actionSets are evaluated for static conditions and RandomChance - after that, one is selected (by random)
        /// </summary>
        public ActionSetsToFire FireMode = ActionSetsToFire.FirstValid;

        public ActionSetType[] SetsOfActions;

        /// <summary>
        /// Actions should fire on each of these... the currently handled item will be available via DynamicTarget...
        /// </summary>
        public TargetObject ActionTargets; 
     

        // needed..? as a shorthand for single actions?
      //  public string EventActionKey;
        


        public string KeyName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }

        /// <summary>
        /// triggering entity is an agent (always..? traps..?) but can be null!
        /// targetEntity can be an inanimate entity
        /// </summary>
        /// <param name="triggeringEntity"></param>
        /// <param name="laterFiringWasAdded"></param>
        public void Fire(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, out bool isExpired, bool isSpawning = false)
        {
          
            isExpired = false;

            if (isSpawning && SupressWhenSpawning)
            {
                isExpired = false;
                return;
            }

            if (DynamicChanceToFire != null)
            {
                EntityID? triggeringEntityID = null;
                if(triggeringEntity != null)
                {
                    triggeringEntityID = triggeringEntity.ID;
                }
                PropertyResult? value = DynamicChanceToFire.Evaluate(triggeringEntityID, targetEntity, polledEventSource, null); // dynamicTarget); //??
                if (value.HasValue)
                {
                    ChanceToFire = value.Value.NumberResult;
                }
            }

            if (ChanceToFire.HasValue && !Common.IsEqual(ChanceToFire.Value, 1f))
            {
                if (The.Sim.GameplayRandomGenerator.NextDouble("") > ChanceToFire.Value) // was: > ChanceToFire.Value)
                {
                    // don't fire
                    isExpired = false;

                    return;
                }
            }

            List<IHasExposedProperties> invocationList;
            if (ActionTargets != null)
            {
                invocationList = ActionTargets.GetResult(
                    triggeringEntity != null ? triggeringEntity.ID : (EntityID?)null,
                    targetEntity, polledEventSource, null);

                if (invocationList != null)
                {
                    foreach (var item in invocationList)
                    {
                        FireSingle(triggeringEntity, targetEntity, polledEventSource, item, ref isExpired);

                        if (isExpired)
                            break;
                    }
                }

            }
            else
            {
                FireSingle(triggeringEntity, targetEntity, polledEventSource, null, ref isExpired);
            }


            if (The.Sim == null) // the game can have ended here
            {
                return;
            }
         
            // check after firing to see if the action sets are all expired:
            isExpired = SetsOfActions.All(a => The.Sim.PlaySite.EventManager.IsExpended(a));               
          
        }


        public void TestFire(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
           /* bool isExpired = false;
            FireSingle(triggeringEntity, targetEntity, polledEventSource, dynamicTarget, ref isExpired, skipConditions: true, firemode: ActionSetsToFire.AllValid);
            */
           
                List<Exception> exceptions = null;
            foreach (var actionSetType in SetsOfActions)
            {
               
                try
                {
                    // create a data object for each action set type that will be fired:
                    ActionSetData actionSetData = new ActionSetData(actionSetType);

                    actionSetData.Fire(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                }
                catch(Exception e)
                {
                    Common.AddToList(ref exceptions, new Exception(Environment.NewLine + "Error in ActionSetType " + actionSetType.KeyName + ": " + Environment.NewLine, e));

                    /*
                    Common.AddToList(ref exceptions, e);
                              
                    throw new Exception("Error in ActionSetType " + actionSetType.KeyName + ": ", e);*/
                }
            }
            // a48ad170-4002-4f17-b8c1-2eb9d9815ed5
            if (exceptions != null)
            {
                AggregateException aggregate = new AggregateException(exceptions.ToArray());

                throw aggregate;

            }
        }

        private void FireSingle(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref bool isExpired)
        {
            List<ActionSetType> validActionSets = null;

           
            foreach (var actionSetType in SetsOfActions)
            {
                // has this ActionSet been fired before? look up the type:

                if (!The.Sim.PlaySite.EventManager.IsExpended(actionSetType) // TOOD: this won't work for othersites...
                        && actionSetType.ConditionsAreFulfilled(triggeringEntity, targetEntity, polledEventSource, dynamicTarget))
                {

                    switch (FireMode)
                    {
                        case ActionSetsToFire.AllValid:
                            {
                                // create a data object for each action set type that will be fired:
                                ActionSetData actionSetData = new ActionSetData(actionSetType);

                                actionSetData.Fire(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                                break; // go on with the rest
                            }
                        case ActionSetsToFire.FirstValid:
                            {
                                // create a data object for each action set type that will be fired:
                                ActionSetData actionSetData = new ActionSetData(actionSetType);

                                actionSetData.Fire(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

                                if (The.Sim != null) // the game can have ended here
                                {
                                    isExpired = SetsOfActions.All(a => The.Sim.PlaySite.EventManager.IsExpended(a));
                                }
                                else isExpired = true;

                                return; // skip the rest
                            }
                        case ActionSetsToFire.RandomValid:
                            {
                                // gather, don't fire yet
                                Common.AddToList(ref validActionSets, actionSetType);

                                break;
                            }
                    }
                }

            }

            if (FireMode == ActionSetsToFire.RandomValid)
            {
                if (validActionSets != null)
                {
                    ActionSetType randomActionSet = Common.GetRandomListMember(validActionSets, The.Sim.GameplayRandomGenerator);
                    ActionSetData actionSet = new ActionSetData(randomActionSet);

                    actionSet.Fire(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                }
            }

        }

        public void ExtractNestedGameData(ref List<string> duplicateKeyErrors)
        {
            foreach (var item2 in this.SetsOfActions)
            {
                item2.ExtractNestedGameData(ref duplicateKeyErrors);

                
            }

        }

        public void LoadContent(ContentManager content)
        {
            if (SetsOfActions != null)
            {
                foreach (var item in SetsOfActions)
                {
                    item.LoadContent(content);
                }
            }
        }


        public void PreInitValidate(ref List<string> errors)
        {
            foreach (var actionSet in SetsOfActions)
            {
                actionSet.PreInitValidate(ref errors);
            }
        }

        private bool isInitialized = false;

        public void Initialize()
        {
            if (isInitialized)
                return;

            foreach (var actionSet in SetsOfActions)
            {
                actionSet.Initialize();
            }


            isInitialized = true;
        }

        public void PostInitValidate(ref List<string> errors)
        {
            foreach (var actionSet in SetsOfActions)
            {
                actionSet.PostInitValidate(ref errors);
            }
        }

        public void PostLoadContentValidate(List<string> listOfErrors)
        {
            foreach (var actionSet in SetsOfActions)
            {
                actionSet.PostLoadContentValidate(ref listOfErrors);
            }

        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (SetsOfActions != null)
            {
                foreach (var item in SetsOfActions)
                {
                    item.PostDataCompleteValidate(ref listOfErrors);
                }
            }

        }

    }
}
