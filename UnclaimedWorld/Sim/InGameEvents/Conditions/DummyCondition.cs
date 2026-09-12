using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Systems;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.SimSide.InGameEvents.Conditions
{
    /// <summary>
    /// Is alkways true but also tests the base flags   
    /// </summary>
    public class DummyCondition : ConditionValue
    {
       

        public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            if (base.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget))
            {
                return true;
            }
            else return false;
        }


    }
}
