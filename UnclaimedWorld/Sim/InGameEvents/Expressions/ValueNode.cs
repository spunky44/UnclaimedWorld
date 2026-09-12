using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Expressions
{
    
    public class ValueNode : EvalNode
    {
        /// <summary>
        /// fill in one of the below:
        /// </summary>
        public int? Int;
        public float? Decimal;
        public string String;
        public bool? Bool;
        public Vector2? Location;

       /// <summary>
       /// default is 'Root'
       /// </summary>
        public TargetObject TargetObject;

        // gets a location or a container by invoking this property on the target object:
        public string PropertyKey;

        private PropertyResult? result = null;

        public override PropertyResult? Evaluate(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget) 
        {
            //PropertyResult? result = null;
            // constants:
            if (Int.HasValue)
            {
                result = new PropertyResult() { NumberResult = Int.Value };
            }
            else if (Decimal.HasValue)
            {
                result = new PropertyResult() { NumberResult = Decimal.Value };
            }
            else if (Bool.HasValue)
            {
                result = new PropertyResult() { BoolResult = Bool.Value };
            }
            else if (String != null)
            {
                result = new PropertyResult() { StringResult = String };
            }
            else if (Location != null)
            {
                result = new PropertyResult() { LocationResult = Location.Value };
            }
            else //if (TargetObject != null)
            {
                //dynamic value:

                List<IHasExposedProperties> resultList = null;

                if (TargetObject != null)
                {
                    resultList = TargetObject.GetResult(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                }
                else
                {
                    resultList = TargetObject.GetRootElementAsList();
                }

                if (resultList != null && resultList.Count > 0)
                {
                    IHasExposedProperties targetObject = resultList[0];

                    result = null;
                    if (targetObject != null)
                    {
                        if (PropertyKey != null)
                        {
                            result = targetObject.GetPropertyValue(PropertyKey);

                           /* if (result != null)
                            {
                                return result.Value.LocationResult;
                            }*/
                        }
                    }
                }

            }
            /*
            if (PropertyKey == "estimatedSpawnFrequency")
            {

            }*/
            return result;
        }


        public override string EvaluateConstant()
        {
            if (String != null)
            {
                return String;
            }

            return null;
        }

        public override string ToString()
        {
            if (result.HasValue)
            {
                return result.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}
