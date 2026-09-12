using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.InGameEvents.Conditions
{
    /// <summary>
    /// only works on Entity objects with Intelligence.
    /// </summary>
    public class AgentCondition: FilterCondition
    {
        /// <summary>
        /// same set as in TalkAction
        /// </summary>
        public bool AllowSleeping;
        public bool AllowFighting;
        public bool AllowThreatened;

        /// <summary>
        /// Migrating is true when moving towards the passage point, before leaving the site
        /// </summary>
        public bool AllowEmigrating;
        public bool AllowTravelling;
        public bool AllowUnconscious;

        public override bool IsFulfilled(IHasExposedProperties hasProperties,
           EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            Entity entity = hasProperties as Entity;

            if (entity == null || !IsFulfilled(entity))
            {
                return false;
            }
            

            return true;
        }


        public bool IsFulfilled(Entity entity)
        {
            return TalkAction.TestAgentProperties(entity, AllowSleeping, AllowUnconscious, AllowFighting, AllowThreatened, AllowEmigrating, AllowTravelling);
        }

    }
}
