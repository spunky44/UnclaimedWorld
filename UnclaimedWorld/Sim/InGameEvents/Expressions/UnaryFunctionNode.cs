using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances.Statistics;

namespace UWGame.SimSide.InGameEvents.Expressions
{
    public enum UnaryExpressionOperator 
    { 
        Not,
        DateFromRelativeDays, DateFromAbsoluteDays, DateFromRelativeSeconds, DateFromAbsoluteSeconds, DateToAbsoluteString, /*DateToRelativeString,*/ DateToJournalString, DateToRelativeSeconds, DateToRelativeDays,
        ComfortRatingToString, FoodRatingToString, SecurityRatingToString, ClampToWithinZeroAndOne
    }

    /// <summary>
    /// useful???
    /// </summary>
    public class UnaryFunctionNode : EvalNode
    {
        /// <summary>
        /// binary operators
        /// </summary>
        public UnaryExpressionOperator Operator;
        public EvalNode Operand;
      
        private PropertyResult? result = null;

        public override PropertyResult? Evaluate(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget) 
        {
            PropertyResult? result = Operand.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

            if (result == null)
                return null;

           /* float? leftNumberValue = null;
            Vector2? leftLocationValue = null;
            string leftStringValue = null;
            */

            bool? operandBoolResult = null;           

            operandBoolResult = result.Value.BoolResult;
            

          //  FunctionNode.GetValues(result, ref leftNumberValue, ref leftLocationValue, ref leftStringValue);
                      
            float? numberResult = null;
            Vector2? locationResult = null;
            string stringResult = null;
            bool? boolResult = null;
            DateAndTime.TimeDateYear? timeDateYearResult = null;

            switch (Operator)
            {
                case UnaryExpressionOperator.ClampToWithinZeroAndOne:
                    {
                        if (result.Value.NumberResult.HasValue)
                        {
                            numberResult = Common.Clamp(result.Value.NumberResult.Value, 0f, 1f);
                        } 

                        break;
                    }
                case UnaryExpressionOperator.Not:
                    {
                        if (operandBoolResult.HasValue)
                        {
                            boolResult = !operandBoolResult.Value;
                        }                       

                        break;
                    }     
                case UnaryExpressionOperator.DateFromRelativeDays:
                    {
                        if (result.Value.NumberResult.HasValue)
                        {
                            timeDateYearResult = new DateAndTime.TimeDateYear(The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays + result.Value.NumberResult.Value);
                        }

                       // string tostring = timeDateYearResult.ToString();

                        break;
                    }
                case UnaryExpressionOperator.DateFromAbsoluteDays:
                    {
                        if (result.Value.NumberResult.HasValue)
                        {
                            timeDateYearResult = new DateAndTime.TimeDateYear(result.Value.NumberResult.Value);
                        }

                        break;
                    }
                case UnaryExpressionOperator.DateFromRelativeSeconds:
                    {
                        if (result.Value.NumberResult.HasValue)
                        {
                            timeDateYearResult = new DateAndTime.TimeDateYear(The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays + result.Value.NumberResult.Value / DateAndTime.secondsPerDay);
                        }

                        break;
                    }
                case UnaryExpressionOperator.DateFromAbsoluteSeconds:
                    {
                        if (result.Value.NumberResult.HasValue)
                        {
                            timeDateYearResult = new DateAndTime.TimeDateYear(result.Value.NumberResult.Value / DateAndTime.secondsPerDay);
                        }

                        break;
                    }
                case UnaryExpressionOperator.DateToAbsoluteString:
                    {
                        if (result.Value.DateResult.HasValue)
                        {
                            stringResult = result.Value.DateResult.Value.ToString();
                        }

                        break;
                    }
              /*  case UnaryExpressionOperator.DateToRelativeString:
                    {
                        if (result.Value.DateResult.HasValue)
                        {
                            stringResult = result.Value.DateResult.Value.ToIntervalString();
                        }

                        break;
                    }*/
                case UnaryExpressionOperator.DateToJournalString:
                    {
                        if (result.Value.DateResult.HasValue)
                        {
                            stringResult = result.Value.DateResult.Value.GetDateForJournal();
                        }

                        break;
                    }
                case UnaryExpressionOperator.DateToRelativeSeconds:
                    {
                        if (result.Value.DateResult.HasValue)
                        {
                            numberResult = (float)result.Value.DateResult.Value.ToRelativeSeconds();
                        }

                        break;
                    }
                case UnaryExpressionOperator.DateToRelativeDays:
                    {
                        if (result.Value.DateResult.HasValue)
                        {
                            numberResult = (float)result.Value.DateResult.Value.ToRelativeDays();
                        }

                        break;
                    }       
                case UnaryExpressionOperator.ComfortRatingToString:
                    {
                        if (result.Value.NumberResult.HasValue)
                        {
                            stringResult = FormatRating(result, RatingTypes.Comfort);
                        }

                        break;
                    }
                case UnaryExpressionOperator.FoodRatingToString:
                    {
                        if (result.Value.NumberResult.HasValue)
                        {
                            stringResult = FormatRating(result, RatingTypes.Food);
                        }

                        break;
                    }
                case UnaryExpressionOperator.SecurityRatingToString:
                    {
                        if (result.Value.NumberResult.HasValue)
                        {
                            stringResult = FormatRating(result, RatingTypes.Security);
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
            else if (boolResult != null)
            {
                result = new PropertyResult() { BoolResult = boolResult };
            }
            else if (timeDateYearResult != null)
            {
                result = new PropertyResult() { DateResult = timeDateYearResult };
            }

            return result;
        }

        private static string FormatRating(PropertyResult? result, RatingTypes ratingType)
        {
            float rating = result.Value.NumberResult.Value;
            StringBuilder text = new StringBuilder();
            Statistic.AppendRatingsTypeToStringAndIcon(text, ratingType);
            text.Append(" ");
            Common.AppendPercentage(text, rating, false, null);

            return text.ToString();
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
