using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.InGameEvents.Conditions
{
    public class PropertyCondition: FilterCondition
    {
        
        /// <summary>
        /// the property to get
        /// </summary>
        public string PropertyKey;


        //Only one of these values will be filled out
        public string ConstantStringEqual;

        public EvalNode StringEqual;
        public EvalNode StringNotEqual;
      
        public EvalNode NumberMinimumInclusive;
        public EvalNode NumberMinimumNotInclusive;

        public EvalNode NumberMaximumNotInclusive;

        public EvalNode NumberEqual;
        public EvalNode NumberNotEqual;

      //  public float? ConstantNumberEqual;

        public bool? BoolValue;

        /// <summary>
        /// if this is set True, the condition is True if the property is Null
        /// if this is set False, the condition is True if the property is not Null
        /// </summary>
        public bool? IsNull; // = false;


        /// <summary>
        /// fill in both to test location within a distance
        /// </summary>
        public float? LocationDistance;
        public EvalNode DynamicLocation;


        private bool FulfillsCondition(PropertyResult result, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            
            if (result.BoolResult.HasValue)
            {   // bools
                if (BoolValue.HasValue)
                {
                    return result.BoolResult.Value == BoolValue.Value;
                }
            }
            else if (result.NumberResult.HasValue)
            {
                // numbers
                float number = result.NumberResult.Value;

                if (NumberEqual != null) //ConstantNumberEqual.HasValue)
                {
                    float? equal = GetNumberCompareValue(NumberEqual, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                    if (equal.HasValue)
                    {
                        if (Common.IsEqual(number, equal.Value))
                        {
                            return true;
                        }
                    }
                    else return false;

                   // return Common.IsEqual(ConstantNumberEqual.Value, number);
                }
                else if (NumberNotEqual != null) 
                {
                    float? equal = GetNumberCompareValue(NumberNotEqual, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                    if (equal.HasValue)
                    {
                        if (!Common.IsEqual(number, equal.Value))
                        {
                            return true;
                        }
                    }
                    else return false;

                    // return Common.IsEqual(ConstantNumberEqual.Value, number);
                }
                else if (NumberMinimumInclusive != null) // NumberMinimum.HasValue)
                {
                    float? min = GetNumberCompareValue(NumberMinimumInclusive, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                    if (min.HasValue)
                    {
                        if (Common.IsGreaterThanOrEqual(number, min.Value))
                        {
                            if (NumberMaximumNotInclusive != null)
                            {
                                float? max = GetNumberCompareValue(NumberMaximumNotInclusive, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

                                if (max.HasValue)
                                {
                                    return Common.IsLessThan(number, max.Value); //return !Common.IsGreaterThan(number, max.Value); // error here?
                                }
                                else return false;
                            }

                            return true;
                        }
                    }
                    else return false;
                }
                else if (NumberMinimumNotInclusive != null) // #MINCHANGE
                {
                    float? min = GetNumberCompareValue(NumberMinimumNotInclusive, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                    if (min.HasValue)
                    {
                        if (Common.IsGreaterThan(number, min.Value))
                        {
                            if (NumberMaximumNotInclusive != null)
                            {
                                float? max = GetNumberCompareValue(NumberMaximumNotInclusive, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

                                if (max.HasValue)
                                {
                                    return Common.IsLessThan(number, max.Value);
                                    //return !Common.IsGreaterThan(number, max.Value);  // error here?
                                }
                                else return false;
                            }

                            return true;
                        }
                    }
                    else return false;
                }
                else if (NumberMaximumNotInclusive != null)
                {
                    float? max = GetNumberCompareValue(NumberMaximumNotInclusive, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

                    if (max.HasValue)
                    {
                        return Common.IsLessThan(number, max.Value); //!Common.IsGreaterThan(number, max.Value);
                    }
                    else return false;                   
                }
            }
            else if (!string.IsNullOrEmpty(result.StringResult) && (StringEqual != null || ConstantStringEqual != null))
            {
                return result.StringResult == GetStringCompareValue(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            }
            else if (StringNotEqual != null)
            {
                string stringToCompareWith = null;
                PropertyResult? dynamicStringResult = StringNotEqual.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                if (dynamicStringResult != null)
                {
                    stringToCompareWith = dynamicStringResult.Value.StringResult;
                }

                return result.StringResult != stringToCompareWith;
            }
            else if (result.LocationResult.HasValue && DynamicLocation != null)
            {
                Vector2? location = GetLocationCompareValue(DynamicLocation, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                float distance = LocationDistance ?? 0f;
                if (location == null) //bso
                {
                    return false;
                }
                return Common.DistanceOctile(result.LocationResult.Value, location.Value) < distance;
            }

            if (IsNull == true)
            {

            }

            return false;
        }

        public override bool IsFulfilled(IHasExposedProperties hasProperties, 
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget) 
        {
            PropertyResult? result = null;
            if (hasProperties != null)
            {
                result = hasProperties.GetPropertyValue(PropertyKey);
               // result = The.Sim.Site.GetPropertyValue(PropertyKey);
            }

            if (IsNull.HasValue)
            {
                if (result.HasValue)
                {
                    return !IsNull.Value;
                }
                else
                {
                    return IsNull.Value;
                }
            }

            if (result.HasValue)
            {
                return FulfillsCondition(result.Value, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);                              
            }
           /* else if(IsNull.HasValue)
            {
                return IsNull.Value;
            }*/

            return false;
        }


        public string GetStringCompareValue(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            if (StringEqual != null)
            {
                PropertyResult? dynamicStringResult = StringEqual.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                if (dynamicStringResult != null)
                {
                    return dynamicStringResult.Value.StringResult;
                }

                return null;
            }          
            else return ConstantStringEqual;
        }

        public float? GetNumberCompareValue(EvalNode evalNode, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            if (evalNode != null)
            {
                PropertyResult? dynamicResult = evalNode.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                if (dynamicResult != null)
                {
                    return dynamicResult.Value.NumberResult;
                }

                return null;
            }
            else return null;

        }

        public Vector2? GetLocationCompareValue(EvalNode evalNode, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            if (evalNode != null)
            {
                PropertyResult? dynamicResult = evalNode.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                if (dynamicResult != null)
                {
                    if (dynamicResult.Value.LocationResult.HasValue)
                    {
                        return dynamicResult.Value.LocationResult.Value;
                    }
                }

                return null;
            }
            else return null;

        }

    }
}
