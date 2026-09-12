using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Systems;
using UWGame.SimSide.AllGameData;
using System.Xml.Serialization;

namespace UWGame.SimSide.InGameEvents.Conditions
{
   
    [XmlInclude(typeof(AreaCondition))]
    [XmlInclude(typeof(PlayerAllegiancePersons))]
    [XmlInclude(typeof(CustomCondition))]
    [XmlInclude(typeof(DummyCondition))]
    public abstract class ConditionValue: Condition
    {
       

        /// <summary>
        /// TODO: replace with AgentCondition
        /// short hand functions for player allegiance only... placed here to reduce the text density...
        /// </summary>
        public bool? AllowWhileAllPlayerMembersAreSleepingOrCollapsed;
        public bool? AllowWhilePlayerMemberIsFighting;
        public bool? AllowWhilePlayerThreatened;


        public override void Initialize()
        {
            
        }

        public override void PreInitValidate(ref List<string> errors)
        {
          
        }

        /// <summary>
        /// some global event triggers (AreaTrigger) will return the triggering entity if fulfilled
        /// </summary>
        /// <param name="triggeringEntity"></param>
        /// <param name="targetEntity"></param>
        /// <returns></returns>
        public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            if (AllowWhileAllPlayerMembersAreSleepingOrCollapsed == false && The.Sim.PlaySite.EventManager.PlayerAllegianceIsSleepingOrCollapsed())
            {
                return false;
            }

            if (AllowWhilePlayerMemberIsFighting == false && The.Sim.PlaySite.EventManager.PlayerAllegianceIsAttacking())
            {
                return false;
            }

            if (AllowWhilePlayerThreatened == false && The.Sim.PlaySite.EventManager.PlayerAllegianceIsUnderThreat())
            {
                return false;
            }


            return true;
        }

        
         
        /// <summary>
        /// only called for Global polled conditions
        /// </summary>
        /// <returns></returns>
        public virtual double? GetUpdateInterval()
        {
            return null;
        }
    }
}
