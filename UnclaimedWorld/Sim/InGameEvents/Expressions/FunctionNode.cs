using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.InGameEvents.Expressions
{
    public enum ExpressionOperator { Plus, Minus, Multiply, Divide, ClampTop, ClampBottom }
    public class FunctionNode : EvalNode
    {
        /// <summary>
        /// binary operators
        /// </summary>
        public ExpressionOperator Operator;
        public EvalNode Left;
        public EvalNode Right;

        private PropertyResult? result = null;

        public override PropertyResult? Evaluate(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget) 
        {
            PropertyResult? leftResult = Left.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            PropertyResult? rightResult = Right.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            float? leftNumberValue = null;
            Vector2? leftLocationValue = null;
            string leftStringValue = null;

            GetValues(leftResult, ref leftNumberValue, ref leftLocationValue, ref leftStringValue);

            float? rightNumberValue = null;
            Vector2? rightLocationValue = null;
            string rightStringValue = null;

            GetValues(rightResult, ref rightNumberValue, ref rightLocationValue, ref rightStringValue);

            float? numberResult = null;
            Vector2? locationResult = null;
            string stringResult = null;

            switch (Operator)
            {
                case ExpressionOperator.Plus:
                    {
                        if (leftNumberValue.HasValue && rightNumberValue.HasValue)
                        {
                            numberResult = leftNumberValue + rightNumberValue;
                        }
                        else if (leftStringValue != null && rightStringValue != null)
                        {
                            stringResult = leftStringValue + rightStringValue;
                        }
                        else if (leftLocationValue != null && rightLocationValue != null)
                        {
                            locationResult = leftLocationValue.Value + rightLocationValue.Value;
                        }

                        break;
                    }
                case ExpressionOperator.Minus:
                    {
                        if (leftNumberValue.HasValue && rightNumberValue.HasValue)
                        {
                            numberResult = leftNumberValue - rightNumberValue;
                        }
                        else if (leftStringValue != null && rightStringValue != null)
                        {
                            stringResult = leftStringValue.Replace(rightStringValue, ""); // lol
                        }
                        else if (leftLocationValue != null && rightLocationValue != null)
                        {
                            locationResult = leftLocationValue.Value - rightLocationValue.Value;
                        }
                        break;
                    }
                case ExpressionOperator.Multiply:
                    {
                        if (leftNumberValue.HasValue && rightNumberValue.HasValue)
                        {
                            numberResult = leftNumberValue * rightNumberValue;
                        }       

                        break;
                    }
                case ExpressionOperator.Divide:
                    {
                        if (leftNumberValue.HasValue && rightNumberValue.HasValue)
                        {
                            numberResult = leftNumberValue / rightNumberValue;
                        }

                        break;
                    }
                case ExpressionOperator.ClampTop:
                    {
                        if (leftNumberValue.HasValue && rightNumberValue.HasValue)
                        {
                            numberResult = Common.ClampTop(leftNumberValue.Value, rightNumberValue.Value);
                        }

                        break;
                    }
                case ExpressionOperator.ClampBottom:
                    {
                        if (leftNumberValue.HasValue && rightNumberValue.HasValue)
                        {
                            numberResult = Common.ClampBottom(leftNumberValue.Value, rightNumberValue.Value);
                        }

                        break;
                    }  
            }

            //PropertyResult? finalResult = null;
            if (numberResult.HasValue)
            {
                result = new PropertyResult() { NumberResult = numberResult };
            }
            else if (stringResult != null)
            {
                result = new PropertyResult() { StringResult = stringResult };
            }
            else if (locationResult != null)
            {
                result = new PropertyResult() { LocationResult = locationResult };
            }

            return result;
        }

        public static void GetValues(PropertyResult? leftResult, ref float? numberValue, ref Vector2? locationValue, ref string stringResult)
        {
            if (leftResult != null)
            {
                if (leftResult.Value.NumberResult.HasValue)
                {
                    numberValue = leftResult.Value.NumberResult.Value;
                }
                else if (leftResult.Value.LocationResult.HasValue)
                {
                    locationValue = leftResult.Value.LocationResult.Value;
                }
                else if (leftResult.Value.StringResult != null)
                {
                    stringResult = leftResult.Value.StringResult;
                }
            }
        }

        public override string ToString()
        {
            if (result.HasValue)
            {
                return result.ToString();
            }
            else
            {
                return Left.ToString() + " " + Operator.ToString() +  " " + Right.ToString();
            }
        }
    }
}
