using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Systems;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;
using System.Xml.Serialization;

namespace UWGame.SimSide.InGameEvents.Conditions
{
 
    public enum OperatorType { And, Or }
    public class ConditionFunction : Condition
    {
        public OperatorType Operator;
        public Condition Left;
        public Condition Right;

        public override void Initialize()
        {
            Left.Initialize();
            Right.Initialize();

        }

        public override void PreInitValidate(ref List<string> errors)
        {
            EntityType.ValidateRequiredValue(ref errors, "Left", Left != null);
            EntityType.ValidateRequiredValue(ref errors, "Right", Right != null);

            if (Left != null && Right != null)
            {
                Left.PreInitValidate(ref errors);
                Right.PreInitValidate(ref errors);
            }
        }

        /// <summary>
        /// a triggered area condition will return the triggering entity!
        /// </summary>
        /// <param name="triggeringEntity"></param>
        /// <param name="targetEntity"></param>
        /// <returns></returns>
        public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {

            bool leftIsFulfilled = Left.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            bool rightIsFulfilled = Right.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

            if (Operator == OperatorType.Or)
            {
                return leftIsFulfilled || rightIsFulfilled;
            }
            else if (Operator == OperatorType.And)
            {
                return leftIsFulfilled && rightIsFulfilled;
            }

            return false;
        }       

    }
   
   
}
