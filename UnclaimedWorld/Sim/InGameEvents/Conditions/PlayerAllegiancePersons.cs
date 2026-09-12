using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Conditions
{
    /// <summary>
    /// Will now return the number of persons in the player's allegiance
    /// NEW: only on-site members are counted by default!
    /// 
    /// </summary>
    public class PlayerAllegiancePersons : ConditionValue
    {
        /// <summary>
        /// put in the interval here. the limits can be omitted on either end...
        /// </summary>
        public int? MinMembers;
        public int? MaxMembers;

        /// <summary>
        /// an optional condition that each member must fulfill in order to count
        /// </summary>
        public AgentCondition AgentCondition;


        public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            if (base.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget))
            {
                return IsFulfilled();
            }
            else return false;
        }

        public bool IsFulfilled()
        {
            int siteMembers;
            Allegiance allegiance;

            allegiance = The.Sim.PlaySite.PlayerAllegiance;
         

            if (allegiance == null)
            {
                siteMembers = 0; // consider 0 members if the allegiance is gone..?
            }
            else
            {
                if (AgentCondition != null)
                {
                    siteMembers = allegiance.GetNoOfPersons(e => AgentCondition.IsFulfilled(e)); // //GetNoOfPersons();  
                }
                else
                {
                    siteMembers = allegiance.GetNoOfPersons(null); // //GetNoOfPersons();  
                }               
            }

            if (MinMembers.HasValue)
            {
                if (siteMembers < MinMembers.Value)
                    return false;
            }

            if (MaxMembers.HasValue)
            {
                if (siteMembers > MaxMembers.Value)
                    return false;
            }

            return true;
        }

    }
}
