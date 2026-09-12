using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.PropertyPresentation
{
    /// <summary>
    /// groups data source + presentation type to use
    /// caption + value is displayed
    ///    
    /// </summary>
    public class Presentation
    {
       
        /// <summary>
        /// This displays the term after Caption:
        /// 
        /// NOTE: When set in the Group Header, it will supress the whole panel if no result
        /// 
        /// key to the list of static functions that return one value (float?)
        /// </summary>
        public string PropertyNameForValue;

        /// <summary>
        /// function to use for the tooltip for the value label/icon
        /// optional. tooltip can be defined in Threshold also
        /// </summary>
      //  public string PropertyNameForValueTooltip;


        public StringSource Caption;
        public StringSource CaptionTooltip;

        /// <summary>
        /// function to use for the tooltip for the value label/icon
        /// optional. tooltip can be defined in Threshold also
        /// </summary>
        public StringSource ValueTooltip;

        public TooltipSettings ValueTooltipSettings;

      
        /// <summary>
        /// key to a presentation type
        /// </summary>
        public string PresentationTypeKey;


        public enum KeyNameForTypeDependentPresentation { Standard, Custom }

        /// <summary>
        /// determines if we use the IHasExposedProperty key name for lookup amongst the presentation types or if we use a programmer defined key that is returned as part of the result.
        ///        
        /// </summary>
        public KeyNameForTypeDependentPresentation KeyNameForTypeDependentPresentationToUse;
    
        /// <summary>
        /// This will make the property clickable (hyperlinks for text). 
        /// 
        /// Only works for classes with an entityID.
        /// We can only use this for properties which do not change status
        /// , because the keys that we use to add the property to a grid need to be static
        /// </summary>
        public bool MakePropertyClickable = false;

        public void PreInitValidate(ref List<string> listOfErrors)
        {
          /*  switch(CaptionSource)
            {
                case CaptionType.Static:
                  

                    break;

                case CaptionType.Default:
                    if (CaptionData != null)
                    {
                        EntityType.CreateValidationError(ref listOfErrors, "With Default caption source, CaptionData must not be filled");
                    }

                    break;                    

            }*/



        }
       
    }
}
