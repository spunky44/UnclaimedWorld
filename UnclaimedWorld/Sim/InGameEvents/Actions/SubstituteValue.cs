using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Skills;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Overland;
using WindowSystem;

namespace UWGame.SimSide.InGameEvents.Actions
{
    public enum FormattingOptions { EarthDate, PlanetDate, BothDates, NameWithSkill, NameOnly }

    /// <summary>
    /// the source of the text that will be replacing the placeholders
    /// </summary>
    public class SubstituteValue
    {
        public string Placeholder;

        /// <summary>
        /// fill in one of the below
        /// </summary>
        public EvalNode Property;

        /// <summary>
        /// shorthand for the above..
        /// </summary>
        public string PropertyName;

        /// <summary>
        /// can handle entity, list of entity...
        /// </summary>
        public TargetObject TargetObject;

        public FormattingOptions? Formatting;

        /// <summary>
        /// allow named constants
        /// </summary>
        public string Color;


        public string Evaluate(EventAction eventAction)
        {
            if (TargetObject != null)
            {
                List<IHasExposedProperties> result = TargetObject.GetResult(eventAction);

                string delim = "";
                StringBuilder text = new StringBuilder();
                foreach (var item in result)
                {
                    Entity entity = item as Entity;
                                       
                    if (entity != null)
                    {
                        FormatEntity(text, entity, Formatting, Color);
                    }
                    else
                    {
                        //item. // ??
                    }

                    text.Append(delim);

                    delim = ", ";
                }

                return text.ToString();

            }
            else if (Property != null)
            {
                string stringResult = "";
                // get the value:
                PropertyResult? propertyResult = Property.Evaluate(eventAction);

                stringResult = FormatPropertyResult(propertyResult);

                return stringResult;
            }
            else if (PropertyName != null)
            {
                Site site = The.Sim.PlaySite;
                PropertyResult? propertyResult;
                IHasExposedProperties dataSource = site;

                propertyResult = dataSource.GetPropertyValue(PropertyName);

                string stringResult = FormatPropertyResult(propertyResult);

                return stringResult;
            }

            return null;
        }

        /// <summary>
        /// creates a parsable string with profession icon and colored label for a person
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string FormatEntity(Entity entity)
        {
            StringBuilder text = new StringBuilder();
            FormatEntity(text, entity);

            return text.ToString();
        }

        /// <summary>
        /// creates a parsable string with profession icon and colored label for a person
        /// </summary>
        /// <param name="name"></param>
        /// <param name="profession"></param>
        /// <param name="isInAllegiance"></param>
        /// <returns></returns>
        public static string FormatEntity(string name, ProfessionType profession, bool isInAllegiance)
        {
            StringBuilder text = new StringBuilder();
            FormatEntity(text, name, profession, isInAllegiance);

            return text.ToString();
        }

        /// <summary>
        ///  creates a parsable string with profession icon and colored label for a person
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string FormatAllegianceMember(Entity entity)
        {
            return SubstituteValue.FormatEntity(entity.GetDisplayName(), entity.Intelligence.Profession, true);
        }

        /// <summary>
        /// creates a parsable string with profession icon and colored label for a person
        /// </summary>
        /// <param name="text"></param>
        /// <param name="entity"></param>
        /// <param name="formatting"></param>
        /// <param name="color"></param>
        public static void FormatEntity(StringBuilder text, Entity entity, FormattingOptions? formatting = null, string color = null)
        {
           
            string name = entity.GetDisplayName();

            ProfessionType profession = null;
            if (entity.EntityType.IntelligenceType != null)
            {
                profession = entity.Intelligence.Profession;
            }

            FormatEntity(text, name, profession, entity.Intelligence.Allegiance == The.InGameUI.UIAllegiance,
                formatting, color);
        }

        public static void FormatEntity(StringBuilder text, string name, ProfessionType profession, bool isInAllegiance, FormattingOptions? formatting = null, string color = null)
        {
            string colorToUse = null;
           
            if (formatting != FormattingOptions.NameOnly)
            {                   
                if (profession != null)
                {
                    text.Append(Icon.ToIcon(profession.Icon, UIComponent.LCDNormalHex));
                }
            }

            if (isInAllegiance)
            {
                colorToUse = HelpTopic.ColorMember;
            }
           

            colorToUse = color ?? colorToUse; // ?? UIComponent.LCDNormalHex;

            if (colorToUse != null)
            {
                text.Append(Label.ToLabel(name, colorToUse));
            }
            else
            {
                text.Append(name);
            }
        }

        private string FormatPropertyResult(PropertyResult? propertyResult)
        {
            string stringResult = null;
            if (propertyResult.HasValue)
            {                
                DateAndTime.TimeDateYear? date =  propertyResult.Value.DateResult;
                
                if (date.HasValue)
                {
                    stringResult = FormatDate(date, Formatting);

                    return stringResult;                   

                }
                else
                {
                    stringResult = propertyResult.Value.ToString();

                    if (Color != null)
                    {
                        stringResult = Label.ToLabel(stringResult, Color);
                    }
                }    
            }

            return stringResult;
        }

        public static string FormatDate(DateAndTime.TimeDateYear? date, FormattingOptions? formatting)
        {
            string stringResult = null;
            string color = null;

            if (formatting == FormattingOptions.BothDates)
            {
                // don't color the earth date:
                stringResult = date.Value.ToString();
                stringResult = Label.ToLabel(stringResult, HelpTopic.ColorDate);
                stringResult += string.Format("({0})", date.Value.GetEarthDate());

                // stringResult = date.Value.GetDateForJournal(); 
            }
            else
            {

                color = HelpTopic.ColorDate;
                if (formatting == FormattingOptions.EarthDate)
                {
                    stringResult = date.Value.GetEarthDate();
                }
                else
                {
                    stringResult = date.Value.ToString();
                }

                stringResult = Label.ToLabel(stringResult, color);
            }

            return stringResult;
        }
    }

}
