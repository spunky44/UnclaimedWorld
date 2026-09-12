using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;

namespace UWGame.ClientSide.PropertyPresentation
{
    /// <summary>
    /// optional formatting that can be applied to text terms and/or float values. 
    /// Uses .NET string formatting methods.
    /// </summary>
    public class TextFormatting
    {

        /// <summary>
        /// to easily scale a number, enter the factor here
        /// </summary>
        public float? NumberFactor;
        

        /// <summary>
        /// "F0" - no decimals
        /// "F2" - 2 decimals
        /// 
        /// SecondsToInGameDays - converts seconds to ingame days string
        /// </summary>
        public string NumberFormatString;


        /// <summary>
        /// 
        /// {0} means term, {1} means formatted number value
        /// </summary>
        public string TextWithPlaceholders;


        public string GetFormattedText(float? value, string term)
        {
            string valueString = null;

            if (value.HasValue)
            {
              
                if (NumberFactor.HasValue)
                {
                    value *= NumberFactor.Value;
                }

                if (NumberFormatString != null)
                {
                    switch (NumberFormatString)
                    {
                        case "SecondsToInGameDays":
                            if (Common.IsGreaterThan(value.Value, 0f)) // only accept positive numbers.
                            {
                                DateAndTime.TimeDateYear timeStruct = DateAndTime.GetSecondsToIngameDays(value.Value);

                                valueString = timeStruct.ToIntervalString();
                            }
                            break;

                        default:
                            valueString = value.Value.ToString(NumberFormatString, Config.Culture);
                            break;
                    }
                }
            }

            string result = "";

            if (TextWithPlaceholders != null)
            {
                string termToUse = term ?? "";
                result = string.Format(TextWithPlaceholders, termToUse, valueString ?? ""); // never null.
            }
            else
            {
                result = term ?? valueString; // can be null.
            }

            return result;
        }
    }
}
