using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Interface.Controls;
using UWGame.SimSide;

namespace UWGame.ClientSide.PropertyPresentation
{
    [XmlInclude(typeof(GroupNode))]    
    [XmlInclude(typeof(LeafNode))]    
    public abstract class PresentationNode
    {
        /// <summary>
        /// this number determines where this PropertyPresentation and its children will be placed in the container,
        /// the PropertyPresentation with the lowest number will be placed first
        /// </summary>
        public int? SortOrder;


        /// <summary>
        /// used for grid entry key
        /// </summary>
        [XmlIgnore]
        public int Index;

        public bool SetCountAsSummary = false;


        protected const int IndentedMemberIconPaddingLeft = 19; // only normal entries in a group with a group header are considered indented
        protected const int IndentedMemberTextPaddingLeft = 20;
        protected const int NormalIconPaddingLeft = 5;
        protected const int NormalTextPaddingLeft = 8;


        public abstract void Display(PresentationTypeCategory categoryToProcess,
                                     IHasExposedProperties hasExposedProperties,
                                     IHasExposedProperties parent,
                                     IKeyedEntryComponent populatable,
                                     bool isIndented, // only normal entries in a group with a group header are indented                                
                                     ref Dictionary<object, object> entryKeys, // this dict will gather all the present keys, for cleanup purposes
                                     ref int? numberOfItems); 
       


        protected bool DisplayEntry(PresentationTypeCategory categoryToProcess, 
                                    IHasExposedProperties hasExposedProperties,
                                    IHasExposedProperties parent, 
                                    IKeyedEntryComponent populatable, 
                                    Presentation presentation, 
                                    int? sortOrder,                                
                                    ref Dictionary<object, object> entryKeys, 
                                    int? height = null, int? 
                                    iconPaddingLeft = null, int? textPaddingLeft = null, bool alwaysAdd = false, bool showIconBackground = false)
        {
            Color? iconColor;
            Color? termColor;
            string term;
            string icon;
            string caption;
            string captionTooltip;
            string keyNameToCheckFor; // used for lookup in TypeDependentPresentation
            string entryKey;
            string propertyTermTooltip;
            TooltipSettings valueTooltipSettings;
            PropertyResult? propertyResult;
            EntityID? entityID; // there is no ID for IHasExposedProperties, and no way of getting to the "parent"... 
            bool makeClickable; // needs entityID to be filled
            float? normalizedValue1;
            float? normalizedValue2;
            int? barWidth;
            bool? isSeenDirectly;
            bool showBar;
            bool showEntityTypeButton;

            /*if (hasExposedProperties is Entity && ((Entity)hasExposedProperties).Name.Contains("Charles") )
            {

            }*/

            // can be null:
            PresentationType presentationType = PresentationTypeCategoryProcessor.GetPresentationTypeFromData(presentation);   
            

            GetDataToDisplay(hasExposedProperties, parent, null, null, presentation, presentationType,
                out iconColor, out termColor, out term, out icon, out caption, out keyNameToCheckFor, out entryKey, 
                out propertyTermTooltip, out captionTooltip, out valueTooltipSettings,
                out propertyResult, out showBar, out normalizedValue1, out normalizedValue2, out barWidth, out entityID, out makeClickable, out showEntityTypeButton, out isSeenDirectly);


            if (/*showBar 
                ||*/ alwaysAdd // set true for headers...
                || populatable.CanProcessEntryData(term, icon, normalizedValue1))            
            {              

               
                int? paddingLeftToUse = null;
                if (icon != null)
                {
                    paddingLeftToUse = iconPaddingLeft;
                }
                else
                {
                    paddingLeftToUse = textPaddingLeft;
                }

                bool rightAdjustTerm = false;
                int? paddingRight = null;
                if (presentationType != null)
                {
                    rightAdjustTerm = presentationType.RightAdjustValue;
                    paddingRight = presentationType.ValueRightPadding;
                }

                AddUpdateEntry(populatable, term, caption, propertyTermTooltip, captionTooltip, valueTooltipSettings, icon, iconColor, termColor, entryKey, propertyResult, showBar, showEntityTypeButton, normalizedValue1, normalizedValue2, barWidth, entityID, makeClickable, 
                    sortOrder, height, paddingLeftToUse, paddingRight, showIconBackground,
                    isSeenDirectly != true, rightAdjustTerm);

                // save the key for this entry.
                Common.AddToDictionary(ref entryKeys, (object)entryKey, (object)entryKey);

                return true;
            }

            return false;
        }


        public UIComponent AddEntityTypeButton(GUIManager gui, string typeKey)
        {
            EntityType entityType;
            if (GameData.Instance.AllEntityTypes.TryGetValue(typeKey, out entityType))
            {
                DataTypeButton button = new DataTypeButton(gui, UWGame.ClientSide.Interface.HUD_Windows.DataSheet.InfoToShow.Production, entityType,
                  null,
                  true);
                button.Init(TextButton.TextButtonType.LCDToolTipBlack);
                button.ID = UIComponent.DataControlID.Status;
                button.IsRoot = true;
                button.Text = entityType.Name; // caption;
                //item.Add(button);
                button.TextAlignment = TextButton.TextAlign.Left;
                button.DebugTag = "cropType";
                button.Width = 125; 

                return button;
            }

            return null;
        }

        private void AddUpdateEntry(IKeyedEntryComponent entryComponent,
                                          string term, string caption,                                        
                                          string termTooltip, string captionTooltip, 
                                          TooltipSettings valueTooltipSettings,
                                          string icon, Color? iconColor, Color? termColor,
                                          string entryKey,
                                          PropertyResult? result,
                                          bool showBar, bool showEntityTypeButton,
                                          float? normalizedValue1, float? normalizedValue2, int? barWidth,
                                          EntityID? entityID,
                                          bool makeClickable,
                                         // PresentationTypeCategory presentationTypeCategory,
                                          int? orderingNumber,
                                          int? height = null,
                                          int? paddingLeft = null, int? paddingRight = null, 
                                          bool showIconBackground = false, 
                                          bool showFaded = false,
                                          bool rightAdjustTerm = false)
        {
            
            float? propertyResult = null;
            if (result != null)
            {
                propertyResult = result.Value.NumberResult;
            }

            if (entryKey.Contains("2progressprogressStatusIcons"))
            {
               // ((Grid)entryComponent).DebugTag = "sleepGrid";
            }

            UIComponent entry;
            if (!entryComponent.TryGetEntry(entryKey, out entry))
            {
                uint? entityIDToUse = null;
                if (entityID.HasValue)
                {
                    entityIDToUse = (uint)entityID.Value;
                }

                string tooltipActivationProperty = null;
                bool? disableExpiry = null;
                int? tooltipWidth = null;

                if (valueTooltipSettings != null)
                {
                    tooltipActivationProperty = valueTooltipSettings.TooltipActivationProperty;
                    disableExpiry = valueTooltipSettings.DisableExpiry;
                    tooltipWidth = valueTooltipSettings.TooltipWidth;
                }

                //Func<UIComponent> addEntityTypeButton = null;
                UIComponent customComponent = null;
                if (showEntityTypeButton)
                {
                    customComponent = AddEntityTypeButton(((UIComponent)entryComponent).guiManager, term);
                    //addEntityTypeButton = AddEntityTypeButton;
                }

                entry = entryComponent.AddEntry(entryKey, term, caption,
                    tooltipDisplayedCallback, 
                    tooltipActivationProperty, disableExpiry, tooltipWidth,
                    icon, iconColor, termColor, orderingNumber, propertyResult, showBar, normalizedValue1, normalizedValue2, barWidth, makeClickable, entityIDToUse,
                    customComponent, //addEntityTypeButton, 
                    height, paddingLeft, showIconBackground);
              
            }

            entryComponent.UpdateEntry(entry, term, caption, termTooltip, captionTooltip,/* tooltipActivationProperty,*/ icon, iconColor, termColor, propertyResult,
                normalizedValue1, normalizedValue2, paddingLeft, paddingRight, showIconBackground, showFaded: showFaded, rightAdjustTerm: rightAdjustTerm); 
               

        }


        void tooltipDisplayedCallback(UIComponent sender, bool value)
        {
            // get the IHasExposedProperties object to call
            if (sender.Tag2 != null)
            {
                // call the property with false

                Tuple<uint, string> callbackProperty = sender.Tag2 as Tuple<uint, string>;
                if (callbackProperty != null)
                {
                    EntityID entityID = (EntityID)callbackProperty.Item1;

                    Entity entity = Entity.FindByID(entityID);
                    if (entity != null)
                    {
                        entity.SetPropertyValue(callbackProperty.Item2, new PropertyResult() { BoolResult = value });
                    }
                }
            }
        }

        public static void GetDataToDisplay(
            IHasExposedProperties hasExposedProperties,            
            IHasExposedProperties parent,
            object key, // can be null
            Dictionary<object, PresentationData> processedData, // can be null

            Presentation presentation,
            PresentationType presentationType,
            out Color? iconColor, out Color? termColor, out string propertyTerm, out string propertyIconName, out string caption, 
            out string keyNameToCheckFor, out string entryKey, 
            out string propertyTermTooltip, out string captionTooltip, out TooltipSettings valueTooltipSetting, // out string tooltipActivationProperty,
            out PropertyResult? propertyResult,
            out bool showBar, out float? normalizedValue1, out float? normalizedValue2, out int? barWidth, 
            out EntityID? entityID,
            out bool makeClickable,
            out bool showTypeButton,
            out bool? isSeenDirectly)
        {
            propertyTermTooltip = null;
            iconColor = null;
            termColor = null;
            propertyTerm = null;
            propertyIconName = null;
            caption = null;
            keyNameToCheckFor = null;
            entryKey = null;
            entityID = null;
            propertyResult = null;
            normalizedValue1 = null;
            normalizedValue2 = null;
            barWidth = null;
            isSeenDirectly = null;
            captionTooltip = null;
            showTypeButton = false;
            showBar = false;
            makeClickable = presentation.MakePropertyClickable;
            entityID = hasExposedProperties.GetEntityID();

            if (presentationType != null)
            {
                if (presentationType.BarPresentation != null)
                {
                    showBar = true;
                    barWidth = presentationType.BarPresentation.Width;
                }

                if (presentationType.EntityTypePresentation != null)
                {
                    showTypeButton = true;
                }
            }
          
           // entityID = PresentationTypeCategoryProcessor.GetEntityID(presentation, hasExposedProperties);

            
            isSeenDirectly = hasExposedProperties.GetIsSeenDirectly(); //The.InGameUI.UIAllegiance.SharedKnowledge);
                    

            // property is optional; without it we can display a static line of text (as a heading).
            if (presentation.PropertyNameForValue != null)
            {
                propertyResult = hasExposedProperties.GetPropertyValue(presentation.PropertyNameForValue, The.InGameUI.UIAllegiance.SharedKnowledge, parent);
               
                if (propertyResult.HasValue == true) //Did we recieve a result from the object with exposed properties?
                {                    
                    keyNameToCheckFor = PresentationTypeCategoryProcessor.DetermineKeyName(presentation, propertyResult.Value, hasExposedProperties);
                                        

                    float? numberResult = propertyResult.Value.NumberResult; //null;
                    Pair<float, float> numberPairResult = propertyResult.Value.NumberPairResult;
                    string stringResult = propertyResult.Value.StringResult;
                    List<string> multiResult =  propertyResult.Value.MultiResults;
                    DateAndTime.TimeDateYear? dateResult = propertyResult.Value.DateResult;

                    if (numberResult != null)
                    {
                        if (presentationType != null)
                        {
                            propertyIconName = presentationType.GetIcon(numberResult.Value, null, keyNameToCheckFor);
                            propertyTerm = presentationType.GetValueTerm(numberResult.Value, null, keyNameToCheckFor);
                            propertyTermTooltip = presentationType.GetTermTooltip(numberResult.Value, null, keyNameToCheckFor);
                            iconColor = presentationType.GetIconColor(numberResult.Value, null, keyNameToCheckFor);
                            termColor = presentationType.GetTermColor(numberResult.Value, null, keyNameToCheckFor);

                            normalizedValue1 = presentationType.GetNormalizedValue(numberResult.Value);                            
                        }
                        else
                        {
                            propertyTerm = numberResult.Value.ToString(Config.Culture);
                        }
                    }                    
                    else if (stringResult != null) // NEW
                    {
                        if (presentationType != null)
                        {
                            propertyIconName = presentationType.GetIcon(stringResult, keyNameToCheckFor);
                            propertyTerm = presentationType.GetTerm(stringResult, keyNameToCheckFor);

                        }
                        else
                        {
                            propertyTerm = stringResult; 
                        }

                    }
                    else if (dateResult != null)
                    {
                        propertyTerm = dateResult.ToString(); 
                    }
                    else if (numberPairResult != null)
                    {
                        // can only be displayed with an indicatorBar...
                        if (presentationType != null)
                        {
                            propertyIconName = presentationType.GetIcon(numberPairResult.First, numberPairResult.Second, keyNameToCheckFor);
                            propertyTerm = presentationType.GetValueTerm(numberPairResult.First, numberPairResult.Second, keyNameToCheckFor);
                            propertyTermTooltip = presentationType.GetTermTooltip(numberPairResult.First, numberPairResult.Second, keyNameToCheckFor);
                            iconColor = presentationType.GetIconColor(numberPairResult.First, numberPairResult.Second, keyNameToCheckFor);
                            termColor = presentationType.GetTermColor(numberPairResult.First, numberPairResult.Second, keyNameToCheckFor);

                            normalizedValue1 = presentationType.GetNormalizedValue(numberPairResult.First);
                            normalizedValue2 = presentationType.GetNormalizedValue(numberPairResult.Second);
                        }

                    }

                    /*else if (multiResult != null) // profession icon name + tooltip. yikes...
                    {                         
                        
                     * if (presentationType != null)
                        {
                            presentationType.GetCustomPresentation(multiResult, out propertyTerm, out propertyColor, out propertyIconName, out propertyTermTooltip, keyNameToCheckFor);                            

                        }
                    }*/

#if DEBUG || PROFILE
                    if (propertyTerm != null && numberResult != null)
                    {
                        if (Kensei.Dev.Options.GetOption("Dev.Show property values"))
                        {
                            propertyTerm = propertyResult.Value.NumberResult.Value + " " + propertyTerm;//Show the property value in debug mode
                        }
                    }
#endif
                             
                }
               
            }
            else if (hasExposedProperties != null)
            {
               // hasExposedProperties.
            }

            if (presentation.Caption != null)
            {
                caption = presentation.Caption.GetValue(hasExposedProperties, parent);

                // apply formatting:
                if (presentationType != null && presentationType.CaptionTextFormatting != null)
                {
                    caption = presentationType.CaptionTextFormatting.GetFormattedText(null, caption);
                }

                if (showBar)
                {
                    if (normalizedValue1.HasValue)
                    {
                        caption += ": ";
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(propertyTerm) && !string.IsNullOrEmpty(caption)) //Only add the colon if we want a caption and if we actually have a value to show
                    {
                        caption += ": ";
                    }
                }
            }

           // PresentationTypeCategoryProcessor.DetermineCaption(ref caption, propertyTerm, presentation, hasExposedProperties);

            // we can now look up the tooltip value separately:
            if (presentation.CaptionTooltip != null)
            {
                captionTooltip = presentation.CaptionTooltip.GetValue(hasExposedProperties, parent);

                // apply formatting:
                if (presentationType != null && presentationType.CaptionTooltipTextFormatting != null)
                {
                    captionTooltip = presentationType.CaptionTooltipTextFormatting.GetFormattedText(null, captionTooltip);
                }
            }
            
            captionTooltip = captionTooltip ?? PresentationType.MissingTooltipNote; // "No tooltip available."; // presentation.CaptionTooltip;

            valueTooltipSetting = presentation.ValueTooltipSettings;

            if (presentation.ValueTooltip != null)
            {
                propertyTermTooltip = presentation.ValueTooltip.GetValue(hasExposedProperties, parent);

                // apply formatting:
                if (presentationType != null && presentationType.ValueTooltipTextFormatting != null)
                {
                    propertyTermTooltip = presentationType.ValueTooltipTextFormatting.GetFormattedText(null, propertyTermTooltip);
                }
            }

           

            if (isSeenDirectly == false && propertyTermTooltip != null && propertyTermTooltip != PresentationType.MissingTooltipNote)
            {
                propertyTermTooltip += "\n ( last known status! )";
            }

            propertyTermTooltip = propertyTermTooltip ?? PresentationType.MissingTooltipNote;

           
            //new: use the same method for generating a key for a text and an icon entry, the keyed component decides what to do with the data
            PresentationTypeCategoryProcessor.DetermineEntryKey(caption, presentation, hasExposedProperties, out entryKey);


            if (key != null && processedData != null)
            {
                if (propertyTerm != null || propertyIconName != null || normalizedValue1.HasValue) // not sure why this line is needed..
                {
                    //We have data for this presentation! Add it
                    processedData.Add(key, new PresentationData(caption, propertyTerm, propertyIconName, propertyResult, normalizedValue1, entryKey, entityID, iconColor, propertyTermTooltip));
                }
            }
        }


        public abstract void PreInitValidate(ref List<string> listOfErrors);
        
    }
}
