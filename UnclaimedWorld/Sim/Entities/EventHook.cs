using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Actions;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// These constants correspond to events during the entity's lifecycle. They can be used for firing scripted events.
    /// For agents, there is an additional set of event hooks below.
    /// since these represent points in the program it is OK to make them a compiled enum type.
    /// </summary>
    public enum EntityEventHooks
    {
        Created, 
        Completed, // not implemented!
        Destroyed, ToBeDestroyed, ComeOnline
    }


    /// <summary>
    /// These event hooks are specific for agents. since these represent points in the program it is OK to make them a compiled enum type
    /// </summary>
    public enum AgentActionHooks
    {
        StartProducing,
        CompletedProducing,  // Does not fire when using Build. target is either the actingOnEntity or the output
        CompletedConstructing, // only remarks.. only fires when using the Build menu -  target is always the output  TODO: better to fire this AND Producing? less chance of mistakes...
        StartConstructing,
        StartHarvesting, CompletedHarvesting, // only remarks
        MissedAnAttackOnAnEnemy, MissedAnAttackOnPrey, StartedEffect, EndedEffect,
        TakingAHit, HitEnemy, HitPrey, KilledEnemy, KilledPrey, Fleeing, 
        DiedOnPlaySite, KilledInCombat, Dying, GoingToSleep, 
        Eating, SwitchedToPlayerAllegiance, DisembarkedNewPlayerAllegianceMember,
        DecidedToLeaveAllegiance, // when decision is taken
        StartsToLeaveAllegiance, // when the agent starts to leave
        LeavesSiteForNewAllegiance,  // when the agent leaves the site
        ForceDropsItem // when we drop an item requested by another agent
    }

    public interface IHook
    {
        string TypeKey { get; }

        int ExecutionOrder { get; }

    }

    /// <summary>
    /// I made the connection into a class to make it easier for mods to update/add/delete them without interfering with the rest of the EntityType
    /// </summary>
    public class AgentActionHook : IGameData, IHook
    {
        /// <summary>
        /// EntityType key
        /// </summary>
        [XmlElement(ElementName = "EntityTypeKey")]
        public string TypeKey { get; set; }

        public int ExecutionOrder
        {
            get;
            set;
        }

        public AgentActionHooks Hook;

        /// <summary>
        /// refers to ActionSets
        /// </summary>
        public string ActionSetsKey;


        public string KeyName
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }

        public void PreInitValidate(ref List<string> errors)
        {
           
        }

        public void Initialize()
        {
          
        }

        public void PostInitValidate(ref List<string> errors)
        {
          
        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }


    public class EntityEventHook : IGameData, IHook
    {
        [XmlElement(ElementName = "EntityTypeKey")]
        public string TypeKey { get; set; }

        public int ExecutionOrder
        {
            get;
            set;
        }

        public EntityEventHooks Hook;

        /// <summary>
        /// refers to ActionSets
        /// </summary>
        public string ActionSetsKey;


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

        public void PreInitValidate(ref List<string> errors)
        {

        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> errors)
        {

        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }

   /* public enum AttackTypeActionHooks
    {
       MissedAnAttackOnAnEnemy, MissedAnAttackOnPrey, HitEnemy, HitPrey, KilledEnemy, KilledPrey
    }*/

  

    public class AttackTypeActionHook : IGameData, IHook
    {
       // public string AttackTypeKey;

        /// <summary>
        /// AttackType key
        /// </summary>
        [XmlElement(ElementName = "AttackTypeKey")]
        public string TypeKey { get; set; }

        public int ExecutionOrder
        {
            get;
            set;
        }

        /// <summary>
        /// only a subset is used...
        /// </summary>
        public AgentActionHooks Hook;

        /// <summary>
        /// refers to ActionSets
        /// </summary>
        public string ActionSetsKey;


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
        public void PreInitValidate(ref List<string> errors)
        {
            switch (Hook)
            {
                case AgentActionHooks.HitEnemy:
                case AgentActionHooks.HitPrey:
                case AgentActionHooks.KilledEnemy:
                case AgentActionHooks.KilledPrey:
                case AgentActionHooks.MissedAnAttackOnAnEnemy:
                case AgentActionHooks.MissedAnAttackOnPrey:
                    return;
                default:
                    EntityType.CreateValidationError(ref errors, Hook.ToString() + " is not a valid value for an attack type hook event");
                    return;

            }
        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> errors)
        {

        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }

    public class ProcessTypeActionHook : IGameData, IHook
    {
        [XmlElement(ElementName = "ProcessTypeKey")]
        public string TypeKey { get; set; }

        public int ExecutionOrder
        {
            get;
            set;
        }

        /// <summary>
        /// only a subset is used...
        /// </summary>
        public AgentActionHooks Hook;

        /// <summary>
        /// refers to ActionSets
        /// </summary>
        public string ActionSetsKey;


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
        public void PreInitValidate(ref List<string> errors)
        {
            switch (Hook)
            {
                case AgentActionHooks.StartProducing:
                case AgentActionHooks.CompletedProducing:
               /* case AgentActionHooks.StartConstructing:
                case AgentActionHooks.CompletedConstructing:*/
                    return;
                default:
                    EntityType.CreateValidationError(ref errors, Hook.ToString() + " is not a valid value for a process type hook event");
                    return;

            }
        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> errors)
        {

        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }

    public class EffectTypeActionHook : IGameData, IHook
    {
        [XmlElement(ElementName = "EffectTypeKey")]
        public string TypeKey { get; set; }

        public int ExecutionOrder
        {
            get;
            set;
        }

        /// <summary>
        /// only a subset is used...
        /// </summary>
        public AgentActionHooks Hook;

        /// <summary>
        /// refers to ActionSets
        /// </summary>
        public string ActionSetsKey;


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
        public void PreInitValidate(ref List<string> errors)
        {
            switch (Hook)
            {
                case AgentActionHooks.StartedEffect:
                case AgentActionHooks.EndedEffect:
                    return;
                default:
                    EntityType.CreateValidationError(ref errors, Hook.ToString() + " is not a valid value for an effect type hook event");
                    return;

            }
        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> errors)
        {

        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }


    public class DetectEntityTypeHook : IGameData, IHook
    {
        [XmlElement(ElementName = "EntityTypeKey")]
        public string TypeKey { get; set; }

        public int ExecutionOrder
        {
            get;
            set;
        }

        public string DetectedEntityKey;

        /// <summary>
        /// refers to ActionSets
        /// </summary>
        public string ActionSetsKey;


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

        public void PreInitValidate(ref List<string> errors)
        {
          
        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> errors)
        {

        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }

    public class DetectResourceTypeHook : IGameData, IHook
    {
        [XmlElement(ElementName = "ResourceTypeKey")]
        public string TypeKey { get; set; }

        public int ExecutionOrder
        {
            get;
            set;
        }

        public string DetectedResourceKey;

        /// <summary>
        /// refers to ActionSets
        /// </summary>
        public string ActionSetsKey;


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

        public void PreInitValidate(ref List<string> errors)
        {

        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> errors)
        {

        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }
}
