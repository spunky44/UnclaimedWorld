using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.PropertyPresentation
{
    public class StringSource
    {
        public string StaticString;

        public string PropertyName;

        /// <summary>
        /// shorthand for retuning EntityType name like "Human", SkillType name like "Construction" etc.
        /// </summary>
        public bool UseDefaultName;

        public string GetValue(IHasExposedProperties hasExposedProperties, IHasExposedProperties parent)
        {
            if (StaticString != null)
            {
                return StaticString;
            }
            else if (PropertyName != null)
            {
                PropertyResult? propertyResult = hasExposedProperties.GetPropertyValue(PropertyName, The.InGameUI.UIAllegiance.SharedKnowledge, parent);

                if (propertyResult.HasValue == true
                    && propertyResult.Value.StringResult != null)
                {
                    return propertyResult.Value.StringResult;
                }

            }
            else if (UseDefaultName == true)
            {
                return hasExposedProperties.GetDefaultCaption(null);                
            }

            return null;
        }

    }
}
