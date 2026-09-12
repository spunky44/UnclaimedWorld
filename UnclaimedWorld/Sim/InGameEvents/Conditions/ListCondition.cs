using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.InGameEvents.Conditions
{
    /// <summary>
    /// a condition that can be invoked on a list of elements
    /// </summary>
    public class ListCondition
    {
     
        public int? CountEqual;

        /// <summary>
        /// inclusive
        /// </summary>
        //public float? CountMinimum;


        public EvalNode CountMinimum;
        //public float? CountMaximum;

        /// <summary>
        /// less than or equal
        /// </summary>
        public EvalNode CountMaximum;


        public bool IsFulfilled(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, List<IHasExposedProperties> list)
        {

            int count = 0;
            if (list != null)
            {   // bools
                count = list.Count;
            }

            if (CountEqual.HasValue)
            {
                return count == CountEqual.Value;
            }
            else if (CountMinimum != null)
            {
                float? minNumber = GetNumber(triggeringEntity, targetEntity, polledEventSource, dynamicTarget, CountMinimum);
                if (minNumber.HasValue && count >= minNumber.Value)
                {
                    if (CountMaximum != null)
                    {
                        float? maxNumber = GetNumber(triggeringEntity, targetEntity, polledEventSource, dynamicTarget, CountMaximum);

                        if (maxNumber.HasValue)
                        {
                            return Common.IsLessThanOrEqual(count, maxNumber.Value);
                        }
                    }
                 
                    return true; // default???

                }
            }
            else if (CountMaximum != null)
            {
                float? maxNumber = GetNumber(triggeringEntity, targetEntity, polledEventSource, dynamicTarget, CountMaximum);

                 if (maxNumber.HasValue)
                 {
                     return Common.IsLessThanOrEqual(count, maxNumber.Value);
                 }               
            }

            return false;
        }


        private float? GetNumber(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, EvalNode evalNode)
        {
            PropertyResult? maxResult = evalNode.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            if (maxResult.HasValue) // && maxResult.Value.NumberResult.HasValue)
            {
                return maxResult.Value.NumberResult;
            }

            return null;

        }

    }
}
