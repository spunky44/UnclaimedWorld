using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using UWGame.Client.Interface;
using UWGame.ClientSide;
using WindowSystem;
using UWGame.SimSide.XmlCollections;

namespace UWGame.ClientSide.PropertyPresentation
{

    public class PresentationType : IGameData
    {
        public string KeyName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }


        /// <summary>
        /// use the type dependent presentation when we want to use different thresholds based on a key, for example an entity type keyName like entity:person
        /// </summary>
        public TypeDependentPresentation TypeDependentPresentation;

        /// <summary>
        /// this is the default if there are no matches in TypeDependentPresentation
        /// </summary>
        public NumberThresholdPresentation NumberThresholdPresentation;

        public TextFormatting ValueTextFormatting;
        public TextFormatting ValueTooltipTextFormatting;
        public TextFormatting CaptionTextFormatting;
        public TextFormatting CaptionTooltipTextFormatting;

        public bool RightAdjustValue;

        public int ValueRightPadding = 0;

        /// <summary>
        /// fill this in order to scale the number result and present it as a bar.
        /// </summary>
        public BarPresentation BarPresentation;

        /// <summary>
        /// fill this to show the entity type button
        /// </summary>
        public EntityTypePresentation EntityTypePresentation;

        /// <summary>
        /// if present, will show the term as an icon sprite and tooltip
        /// </summary>
        public IconPresentation IconPresentation;
        
        public const string MissingTooltipNote = "No tooltip available.";

        public string GetTermTooltip(float value1, float? value2, string typeKey)
        {

            Threshold threshold = GetThreshold(value1, value2, typeKey);

            if ((threshold == null || threshold.UseValueTooltipFormatting ) && ValueTooltipTextFormatting != null)
            {
                string termToUse = null;
                if (threshold != null)
                {
                    termToUse = threshold.TermTooltip ?? MissingTooltipNote;
                    //return threshold.TermTooltip; 
                }

                return ValueTooltipTextFormatting.GetFormattedText(value1, termToUse);
            }
            else if (threshold != null)
            {
                return threshold.TermTooltip ?? MissingTooltipNote;
            }
            else
            {
                return null;
            }

        }

       /* public string GetTermTooltip(string value, string typeKey)
        {
            if (IconPresentation != null)
            {
                return termValue;
            }
            else return null;

        }*/

      /*  public string GetCaptionTooltip(float value, string typeKey)
        {
            Threshold threshold = GetThreshold(value, typeKey);

            if (threshold != null)
            {
                return threshold.CaptionTooltip;
            }
            else
            {
                return null;
            }

        }*/


        public string GetValueTerm(float value1, float? value2, string typeKey) // "leftArm", "entity:Human", 
        {
            Threshold threshold = GetThreshold(value1, value2, typeKey);

            if ((threshold == null || threshold.UseValueTextFormatting == true) && ValueTextFormatting != null)
            {
                string termToUse = null;
                if (threshold != null)
                {
                    termToUse = threshold.Term;
                }

                float valueToUse = GetValueToUse(value1, value2);

                return ValueTextFormatting.GetFormattedText(valueToUse, termToUse);
            }
            else if (threshold != null)
            {
                return threshold.Term;
            }
            else
            {
                return null;
            }

        }

        public string GetIcon(float value1, float? value2, string typeKey)
        {
            Threshold threshold = GetThreshold(value1, value2, typeKey);

            if (threshold != null)
            {
                return threshold.Icon;
            }
            else
            {
                return null;
            }
        }

        public string GetIcon(string value, string typeKey)
        {
            if (IconPresentation != null)
            {
                return value;
            }
            else return null;
        }

        public string GetTerm(string value, string typeKey) 
        {
            if (IconPresentation == null)
            {
                return value;
            }
            else return null;
        }

        public void GetCustomPresentation(List<string> multiResult, out string propertyTerm, out Color? propertyColor, out string propertyIconName, out string propertyTermTooltip, string typeKey)
        {
            propertyColor = null;
            propertyTerm = null;
            propertyIconName = null;
            propertyTermTooltip = null;

            if (multiResult.Count == 2)
            {
                if (IconPresentation != null)
                {
                    propertyIconName = multiResult[0];                   
                }
                else
                {
                    propertyTerm = multiResult[0];
                }

                propertyTermTooltip = multiResult[1];
            }
           
        }

       

        public Color? GetIconColor(float value1, float? value2, string typeKey)
        {
            Threshold threshold = GetThreshold(value1, value2, typeKey);

            if (threshold != null)
            {
                return threshold.IconTint;
            }
            else
            {                
                return null;
            }

        }

        public Color? GetTermColor(float value1, float? value2, string typeKey)
        {
            Threshold threshold = GetThreshold(value1, value2, typeKey);

            if (threshold != null)
            {
                return threshold.TermTint;
            }
            else
            {
                return null;
            }

        }

        public float? GetNormalizedValue(float value)
        {
            if (BarPresentation != null) 
            {
                float normalizedValue = Common.Clamp(value / BarPresentation.MaxValue, 0f, 1f);

                // don't show the full bar:
                if (BarPresentation.SuppressIfMaximumValue && Common.IsEqual(normalizedValue, 1f))
                {
                    return null;
                }
                else
                {
                    return normalizedValue;
                }
            }

            return null;
        }

        public Threshold GetThreshold(float value1, float? value2, string typeKey)
        {
            Threshold[] thresholds = null;
            
            // select the more specific first
            if (typeKey != null && TypeDependentPresentation != null)
            {
               /* if (typeKey == null)
                {
                    throw new Exception("object with exposed properties missing key name!");
                }*/

                TypeDependentPresentation.ThresholdsByType.TryGetValue(typeKey, out thresholds);

                 /*
                if (TypeDependentPresentation.ThresholdsByType.TryGetValue(typeKey, out thresholds) == false)
                {
                    return null;
                }*/
            }

            // new: fall back on default:
            if (thresholds == null)
            {
                if (NumberThresholdPresentation != null)
                {
                    thresholds = NumberThresholdPresentation.Thresholds;
                }
                else
                {
                    return null;
                }
            }

            float valueToUse = GetValueToUse(value1, value2);

            if (thresholds != null)
            {
                int index;
                Threshold threshold = Common.GetStairStepIndex(valueToUse, thresholds, out index);
                return threshold;
            }
            else return null;
        }

        private float GetValueToUse(float value1, float? value2)
        {
            float valueToUse;
            if (value2 == null)
            {
                valueToUse = value1;
            }
            else
            {
                switch (NumberThresholdPresentation.NumberSource)
                {
                    case NumberSource.First:
                        valueToUse = value1;
                        break;

                    case NumberSource.Second:
                        valueToUse = value2.Value;
                        break;

                    case NumberSource.Difference:
                        valueToUse = value2.Value - value1;
                        break;

                    default: valueToUse = value1;
                        break;
                }
            }
            return valueToUse;
        }


        public void PreInitValidate(ref List<string> errors)
        {

        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> errors)
        {

        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }


    public class Sorting
    {
        public Grid.Sorting? SortingDirection;

        /// <summary>
        /// if true, the items are sorted by their computed (result) float value. 
        /// </summary>
        // public bool SortByValue = false;
        public SortingMethod SortingMethod = SortingMethod.StaticSortOrder;

        public static Func<UIComponent, object> GetSelector(SortingMethod method)
        {
            Func<UIComponent, object> primarySelector = null;

            switch (method)
            {
                case SortingMethod.StaticSortOrder:
                    primarySelector = i => i.OrderByTag2;
                    break;

                case SortingMethod.NumberResult:
                    primarySelector = i => i.OrderByTag1 ?? 0f;
                    break;

                case SortingMethod.NumberResultMiddleDistance:
                    primarySelector = i => GetMiddleDistanceForValue((float)(i.OrderByTag1 ?? 0.5f)); // 0.5 is a neutral value we can use as default..
                    break;
            }

            return primarySelector;
        }

        public Func<UIComponent, object> GetSelector() //Sorting primarySorting)
        {
            return GetSelector(SortingMethod);
        }

        private static float GetMiddleDistanceForValue(float value)//for values between 1 - 0, gives a value of how far from 0.5 the value is
        {
            return Math.Abs(value - 0.5f);
        }
    }

   /* public class NumberedPresentation
    {
        public string ValueType = "";
        public Threshold[] Thresholds;
    }*/

    public class TypeDependentPresentation 
    {
        public SerializableDictionary<string, Threshold[]> ThresholdsByType;
    }

    public enum NumberSource { First, Second, Difference }
    public class NumberThresholdPresentation 
    {
        public NumberSource NumberSource = NumberSource.First;
        public Threshold[] Thresholds;      
    }

    public class IconPresentation
    {        
    }

    public class EntityTypePresentation
    {
    }

    public class BarPresentation
    {
        public float MaxValue = 1f;

      /*  public Color? BarColor; // = new Color(73,172,163);

        public string Tooltip;*/

        public int? Width;

        /// <summary>
        /// defines whether the bar should be shown when the value is at maximum.
        /// </summary>
        public bool SuppressIfMaximumValue = false; // true;
    }
  
}
