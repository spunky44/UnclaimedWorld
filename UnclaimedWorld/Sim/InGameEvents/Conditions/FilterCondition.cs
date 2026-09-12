using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Conditions
{
    /// <summary>
    /// a condition that is invoked on an IHasExposedProperties object
    /// </summary>
    [XmlInclude(typeof(PropertyCondition))]
    [XmlInclude(typeof(AgentCondition))]
    [XmlInclude(typeof(FilterConditionFunction))]
    public abstract class FilterCondition
    {
        public abstract bool IsFulfilled(IHasExposedProperties hasProperties,
           EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget); 
      
    }


    public class FilterConditionFunction: FilterCondition
    {
        public OperatorType Operator;

        public FilterCondition Left;
        public FilterCondition Right;

        
        public override bool IsFulfilled(IHasExposedProperties hasProperties,
           EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            bool leftIsFulfilled = Left.IsFulfilled(hasProperties, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            
            if (Operator == OperatorType.Or)
            {
                if (leftIsFulfilled)
                {
                    return true;
                }
                else
                {
                    return Right.IsFulfilled(hasProperties, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                }
            }
            else if (Operator == OperatorType.And)
            {
                if (leftIsFulfilled)
                {
                    return Right.IsFulfilled(hasProperties, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                }
                else return false;
            }

            return false;
        }

    }
}
