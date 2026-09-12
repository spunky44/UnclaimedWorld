using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Overland;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions
{

    

    /// <summary>
    /// represents a text blob that may have placeholders for dynamic content, as well as the property names to populate those.
    /// </summary>
    public class DynamicText
    {
        /// <summary>
        /// this can contain placeholders like {0}, {1} etc.
        /// </summary>
        public string Text;

        /// <summary>
        /// same as above, but dynamic
        /// </summary>
        public EvalNode EvalText;

              
        /// <summary>
        /// these are intelligent so they can handle a returned Entity, Date etc. and format them with
        /// special substitution, like people (w. profession icon), items...
        /// </summary>
        public SubstituteValue[] SubstitutionValues;


        public string GetSubstitutedText(EventAction eventAction)
        {
            return SubstituteTextVariables(eventAction);
        }

        private string SubstituteTextVariables(EventAction eventAction)
        {
            if (string.IsNullOrEmpty(Text) && EvalText == null)
                return null;

            string textToSubstitute;

            if (Text != null)
            {
                textToSubstitute = Text;
            }
            else
            {
                PropertyResult? propertyResult = EvalText.Evaluate(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);
                if (propertyResult.HasValue)
                {
                    textToSubstitute = propertyResult.Value.StringResult ?? "";
                }
                else return null;
            }

           // string[] results;
            Dictionary<string, string> results;

          /*  if (this.PropertyNames != null) // perhaps delete and migrate this.
            {               

               // results = new string[PropertyNames.Count];
                results = new Dictionary<string, string>(PropertyNames.Count);

                Site site = The.Sim.PlaySite; 

                PropertyResult? propertyResult;

                IHasExposedProperties dataSource = site;

               // int counter = 0;
                string stringResult;
                foreach (var item in PropertyNames)
                {
                    stringResult = "";
                    // get the value:
                    propertyResult = dataSource.GetPropertyValue(item.Value);

                    if (propertyResult.HasValue)
                    {
                        stringResult = propertyResult.Value.StringResult ?? "";
                    }

                    results[item.Key] = stringResult;
                  //  results[counter] = stringResult;
                   // counter++;
                }

            }
            else*/
            if (SubstitutionValues != null)
            {
                results = new Dictionary<string, string>(SubstitutionValues.Length); // new string[SubstitutionValues.Count];
                foreach (var item in SubstitutionValues)
                {
                    results.Add(item.Placeholder, item.Evaluate(eventAction));

                }

                /*
                results = new Dictionary<string, string>(SubstitutionValues.Count); // new string[SubstitutionValues.Count];
                string stringResult;
                PropertyResult? propertyResult;
             
                // TODO: would be nice if these could handle non-string objects like People, ItemTypes or Dates and give them nice standard formatting with colors, icons and so on.
                // but the dates are formatted differently in the scenarios...
                foreach (var item in SubstitutionValues)
                {
                    stringResult = "";
                    // get the value:
                    propertyResult = item.Value.Evaluate(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);

                    if (propertyResult.HasValue)
                    {
                        stringResult = propertyResult.Value.ToString();
                    }

                    results[item.Key] = stringResult;                  
                }*/
            }
            else
            {
                return textToSubstitute;
            }

            // substitute:          
            foreach (var item in results)
            {
                textToSubstitute = textToSubstitute.Replace(item.Key, item.Value);
            }

            return textToSubstitute;

            //return string.Format(textToSubstitute, results);


        }

        public void Validate(ref List<string> listOfErrors)
        {


        }
    }
}
