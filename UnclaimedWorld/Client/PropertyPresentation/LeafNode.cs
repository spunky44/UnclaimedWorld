using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using WindowSystem;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.ClientSide.PropertyPresentation
{
    public class LeafNode : PresentationNode
    {       
        
       // public string SubHeader;

        public Presentation Presentation;
        

        public override void Display(PresentationTypeCategory categoryToProcess,
                                     IHasExposedProperties hasExposedProperties, 
                                     IHasExposedProperties parent,
                                     IKeyedEntryComponent populatable,
                                     bool isIndented,
                                     ref Dictionary<object, object> entryKeys,
                                     ref int? numberOfItems)
        {
           // string entryKey;
           // base.Display(categoryToProcess, hasExposedProperties, populatable, isIndented, out entryKey, ref entryKeys);

          
            DisplayEntry(categoryToProcess, hasExposedProperties, parent, populatable,
                Presentation, SortOrder, ref entryKeys,
                iconPaddingLeft: isIndented? IndentedMemberIconPaddingLeft : NormalIconPaddingLeft, 
                textPaddingLeft: isIndented? IndentedMemberTextPaddingLeft : NormalTextPaddingLeft);        

        }



        public override void PreInitValidate(ref List<string> listOfErrors)
        {
            Presentation.PreInitValidate(ref listOfErrors);
        }


      
        /*

        private void AddUpdateGridEntries(
            PresentationTypeCategory categoryToProcess,
            IKeyedEntryComponent entryComponent,
            KeyValuePair<LeafNode, List<PresentationData>> propertyPresentationData
            )
        {
            if (propertyPresentationData.Key.DividingLineBefore == true)
            {
                string separatorKey = categoryToProcess.Name + propertyPresentationData.Key.Index + "before";
                UIComponent entry;
                if (entryComponent.TryGetEntry(separatorKey, out entry) == false)
                {
                    if (entryComponent.AddSeparator(separatorKey, propertyPresentationData.Key.SortOrder, 3.0f))
                    {
                       // currentKeys.Add(separatorKey, separatorKey);
                    }
                }
                else
                {
                  //  currentKeys.Add(separatorKey, separatorKey);
                }
            }

            if (propertyPresentationData.Key.SubHeader != null)
            {
                string entryKey = propertyPresentationData.Key.SubHeader;

                UIComponent entry;
                if (entryComponent.TryGetEntry(entryKey, out entry) == false)
                {
                    if (entryComponent.AddSubHeader(entryKey, propertyPresentationData.Key.SortOrder, 200.0f))
                    {
                      //  currentKeys.Add(entryKey, entryKey); // NUTRITION
                    }
                }
                else
                {
                   // currentKeys.Add(entryKey, entryKey);
                }
            }

           
            AddUpdateEntry(entryComponent, presentationData, propertyPresentationData.Key.SubHeader + presentationData.Key,
                    presentationData.ClickablePropertyEntityID, categoryToProcess, propertyPresentationData.Key.SortOrder);
            

            if (propertyPresentationData.Key.DividingLineAfter == true)
            {
                string separatorKey = categoryToProcess.Name + propertyPresentationData.Key.Index + "after";
                UIComponent entry;
                if (entryComponent.TryGetEntry(separatorKey, out entry) == false)
                {
                    if (entryComponent.AddSeparator(separatorKey, propertyPresentationData.Key.SortOrder, -2.0f))
                    {
                      //  currentKeys.Add(separatorKey, separatorKey);
                    }
                }
                else
                {
                  //  currentKeys.Add(separatorKey, separatorKey);
                }
            }
        }*/
    }
}
