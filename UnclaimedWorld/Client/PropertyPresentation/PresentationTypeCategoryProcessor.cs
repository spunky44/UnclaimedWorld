using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide;
using UWGame.Client.Interface;
using UWGame.SimSide.Entities;
using UWGame.SimSide;
using WindowSystem;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.PropertyPresentation
{
    /// <summary>
    /// Each Category is showed in a collapsable panel. Inside the panel, there can be Groups, marked with indentation, borders etc.
    /// </summary>
    class PresentationTypeCategoryProcessor
    {
        private static List<IHasExposedProperties> hasPropertiesList = new List<IHasExposedProperties>();

        private static List<LeafNode> emptyPropertyPresentations = new List<LeafNode>();
        private static Dictionary<object, object> currentKeys = new Dictionary<object, object>();


        /// <summary>
        /// tests whether the keyed entry component would have any data to show.
        /// </summary>
        /// <param name="categoryToProcess"></param>
        /// <param name="hasExposedProperties"></param>
        /// <param name="populatable"></param>
        /// <returns></returns>
        public static bool KeyedEntryComponentHasDataToShow(
            PresentationTypeCategory categoryToProcess,
            IHasExposedProperties hasExposedProperties,
            Func<string, string, float?, bool> canShowData
            )
        {
            Dictionary<object, PresentationData> processedData = ProcessCategoryData(categoryToProcess, hasExposedProperties);
            if (processedData.Count == 0)
            {
                return false;// no data was found for keyed Entry component
            }

            foreach (var presentationData in processedData)
            {
                if (canShowData(presentationData.Value.Term, presentationData.Value.IconName, presentationData.Value.NormalizedValue))
                //  KeyedEntryComponentCanShowData(categoryToProcess, canShowData /* populatable*/, propertyPresentationData.Value))
                {
                    return true;
                }
            }

            return false;
        }



        public static bool DisplayCategory(
          PresentationTypeCategory categoryToProcess,
          IHasExposedProperties hasExposedProperties,
          IKeyedEntryComponent populatable,
          ref int? numberOfItems)
        {

            populatable.BeginAddingEntries();

            foreach (var node in categoryToProcess.Nodes)
            {
                //string entryKey;
                node.Display(categoryToProcess, hasExposedProperties, null, populatable, false, ref currentKeys, ref numberOfItems);
            }

            CleanupAndSortEntries(populatable, categoryToProcess.PrimarySortingOfItems, categoryToProcess.SecondarySortingOfItems, currentKeys);


            populatable.EndAddingEntries();


            if (currentKeys != null && currentKeys.Count > 0)
            {
                // numberOfItems = currentKeys.Count;

                currentKeys.Clear();

                return true;
            }
            else return false;

        }

        public static void CleanupAndSortEntries(IKeyedEntryComponent populatable, Sorting primarySorting, Sorting secondarySorting, Dictionary<object, object> entryKeys)
        {
            CleanupUnusedEntries(populatable, entryKeys);

            populatable.CapNoOfEntries();

            Sort(populatable, primarySorting, secondarySorting);
        }

        /*  private static void Sort(IKeyedEntryComponent populatable, Sorting primarySorting, Sorting secondarySorting)
          {
              WindowSystem.Grid.Sorting sortingDir = WindowSystem.Grid.Sorting.Ascending;

              if (primarySorting != null)
              {
                  sortingDir = primarySorting.SortingDirection ?? WindowSystem.Grid.Sorting.Ascending;


                  if (primarySorting.SortingMethod == SortingMethod.NumberResult
                      || primarySorting.SortingMethod == SortingMethod.NumberResultMiddleDistance)
                  {
                      Func<float, float> transformation = null;

                      switch (primarySorting.SortingMethod)
                      {
                          case SortingMethod.NumberResultMiddleDistance:
                              transformation = GetMiddleDistanceForValue;
                              break;
                      }

                      //Sort by float value
                      populatable.Sort(sortingDir, true, transformation);
                  }
                  else
                  {
                      //sort by ordering numbers               
                      populatable.Sort(sortingDir, false);
                  }
              }
              else
              {
                  //sort by ordering numbers
                  // (orderByTag2 is now the primary sorting value!)
                  populatable.Sort(sortingDir, false);
              }
          }*/

        private static void Sort(IKeyedEntryComponent populatable, Sorting primarySorting, Sorting secondarySorting)
        {
            if (populatable.Entries.Count > 0)
            {
                List<UIComponent> entries = populatable.Entries;
                Func<UIComponent, object> primarySelector;

                Grid.Sorting primarySortingDir;

                if (primarySorting == null)
                {
                    //sort by ordering numbers by default:
                    primarySelector = Sorting.GetSelector(SortingMethod.StaticSortOrder);
                    primarySortingDir = Grid.Sorting.Ascending;
                }
                else
                {
                    primarySelector = primarySorting.GetSelector();
                    primarySortingDir = primarySorting.SortingDirection ?? Grid.Sorting.Ascending;
                }


                if (secondarySorting != null)
                {
                    Func<UIComponent, object> secondarySelector = secondarySorting.GetSelector();

                    Grid.Sort(primarySelector, primarySortingDir,
                        secondarySelector, secondarySorting.SortingDirection ?? Grid.Sorting.Ascending, ref entries, null);

                }
                else
                {
                    Grid.Sort(primarySelector, primarySortingDir, ref entries, null);
                }

                populatable.Entries = entries; // hmm.
                populatable.EndAddingEntries(); // RefreshEntries();
            }
        }




        /* private static void Sort(
            Func<UIComponent, object> keySelector1, Sorting sorting1, 
            Func<UIComponent, object> keySelector2, Sorting sorting2, 
            ref List<UIComponent> entriesToSort, RefreshFunction refreshFunction)
         {
             
             IOrderedEnumerable<UIComponent> sortResult1, sortResult2;
             if (sorting1 == Sorting.Descending)
             {
                 sortResult1 = entriesToSort.OrderByDescending(keySelector1);
             }
             else
             {
                 sortResult1 = entriesToSort.OrderBy(keySelector1);
             }

             if (sorting2 == Sorting.Descending)
             {
                 sortResult2 = sortResult1.ThenByDescending(keySelector2);
             }
             else
             {
                 sortResult2 = sortResult1.ThenBy(keySelector2);
             }

             entriesToSort = sortResult2.ToList();

             if (refreshFunction != null)
             {
                 refreshFunction.Invoke();
             }
         }*/

        private static void CleanupUnusedEntries(IKeyedEntryComponent entryComponent, Dictionary<object, object> entryKeys)
        {
            // for each entry, test that its key exists in the data set. otherwise remove it.    
            for (int index = 0; index < entryComponent.Count; index++)
            {
                object currentComponentKey = entryComponent.GetKeyFromIndex(index);

                if (entryKeys == null || !entryKeys.ContainsKey(currentComponentKey))
                {
                    if (entryComponent.TryRemoveEntry(currentComponentKey))
                    {
                        index--;
                    }
                }
            }
        }

        private static void RemoveEmptyPresentations(Dictionary<LeafNode, List<PresentationData>> processedData)
        {
            foreach (var propertyPresentation in emptyPropertyPresentations)
            {
                processedData.Remove(propertyPresentation);
            }
            emptyPropertyPresentations.Clear();
        }

       /* public static bool KeyedEntryComponentCanShowData(
            PresentationTypeCategory categoryToProcess,
            Func<string, string, float?, bool> canShowData,
            PresentationData presentationData)
        {
           
            bool canShowDataResult = canShowData(presentationData.Term, presentationData.IconName, presentationData.NormalizedValue); // entryComponent.CanProcessEntryData(presentationData.GetTerm(), presentationData.GetIconName());
            if (canShowDataResult)
            {
                return true;
            }
            // }

            return false;
        }*/


     

        public static void DetermineEntryKey(string caption, Presentation presentation,
                                                    IHasExposedProperties hasExposedProperties, out string entryKey)
        {

            if (presentation.PresentationTypeKey == "storageCapacityPresentation")
            {

            }

            hasExposedProperties.GetDefaultKey(out entryKey);
            entryKey += caption + presentation.PropertyNameForValue + presentation.PresentationTypeKey; //Does this formula work for all entries?

        }

      /*  public static EntityID? GetEntityID(Presentation presentation, IHasExposedProperties hasExposedProperties)
        {
           
            EntityID? clickableEntity;//We can only add hyperlinks for properties with an entityID
            if (presentation.MakePropertyClickable)
            {
                clickableEntity = hasExposedProperties.GetEntityID();//If this does not return null the property will be clickable
            }
            else
            {
                clickableEntity = null;
            }

            return clickableEntity;
        }*/



        private static float GetInvertedValue(float value)//for values between 1 - 0
        {
            return (1.0f - value);
        }

        /*  private static void AddUpdateEntry(
             IKeyedEntryComponent entryComponent,
             PresentationData entryData,
             string entryKey,
             EntityID? clickablePropertyEntityID ,
             PresentationTypeCategory presentationTypeCategory,
             int? orderingNumber )
         {
             UIComponent existingEntry;

             float? propertyResult = entryData.Result.Value.NumberResult;


             if (entryComponent.TryGetEntry(entryKey, out existingEntry))
             {
                 entryComponent.UpdateEntry(existingEntry, entryData.Term, entryData.Caption, entryData.GetTermTooltip(), entryData.GetIconName(), entryData.Color, propertyResult);

                 currentKeys.Add(entryKey, entryKey);
             }
             else
             {
                 if(clickablePropertyEntityID.HasValue == false) //If we have an ID then the property should be clickable
                 {
                     if (entryComponent.AddEntry(entryKey, entryData.Term, entryData.Caption, entryData.GetTermTooltip(), entryData.GetIconName(), entryData.Color, orderingNumber, propertyResult))
                     {
                         currentKeys.Add(entryKey, entryKey);
                     }
                 }
                 else
                 {
                     if (entryComponent.AddEntry(entryKey, entryData.Term, entryData.Caption, entryData.GetTermTooltip(), entryData.GetIconName(),
                         entryData.Color, orderingNumber, propertyResult, true, (uint)entryData.ClickablePropertyEntityID.Value))
                     {
                         currentKeys.Add(entryKey, entryKey);
                     }
                 }
             }

         }*/


        private static Dictionary<object, PresentationData> ProcessCategoryData(PresentationTypeCategory categoryToProcess,
                                                                                        IHasExposedProperties hasExposedProperties)
        {
            //Dictionary<LeafNode, List<PresentationData>> processedData = new Dictionary<LeafNode, List<PresentationData>>();
            Dictionary<object, PresentationData> processedData = new Dictionary<object, PresentationData>();
            foreach (var node in categoryToProcess.Nodes)
            {
                LeafNode singleNode = node as LeafNode;
                if (singleNode != null)
                {
                    ProcessLeaf(hasExposedProperties, singleNode, processedData);
                }
                else
                {
                    GroupNode groupNode = node as GroupNode;
                    ProcessGroup(hasExposedProperties, groupNode, processedData);
                }
            }

            return processedData;
        }

        private static void ProcessGroup(
            IHasExposedProperties hasExposedProperties,
            GroupNode groupNode,
            // Dictionary<LeafNode, List<PresentationData>> processedData
             Dictionary<object, PresentationData> processedData
             )
        {
            if (groupNode.DynamicList != null) // propertyPresentation.ChildPresentation != null)
            {
                // processes child properties like bodyparts, parts 
                List<IHasExposedProperties> hasPropertiesList = new List<IHasExposedProperties>();

                hasExposedProperties.GetChildren(groupNode.DynamicList.HasPropertiesList, ref hasPropertiesList, groupNode.DynamicList.Filter, null, null, null, null);


                foreach (var item in hasPropertiesList)
                {
                    //use item.KeyName ?
                    ProcessSingleObject(item, hasExposedProperties, item.KeyName, processedData, groupNode.DynamicList.Presentation);
                }

            }
            else
            {
                foreach (var node in groupNode.Nodes)
                {
                    LeafNode singleNode = node as LeafNode;
                    if (singleNode != null)
                    {
                        ProcessLeaf(hasExposedProperties, singleNode, processedData);
                    }
                    else
                    {
                        GroupNode nestedGroupNode = node as GroupNode;
                        ProcessGroup(hasExposedProperties, nestedGroupNode, processedData);
                    }
                }
            }
        }

        private static void ProcessLeaf(
            IHasExposedProperties hasExposedProperties,
            LeafNode propertyPresentation,
            Dictionary<object, PresentationData> processedData
             )
        {

            // single properties
            ProcessSingleObject(hasExposedProperties, null, propertyPresentation, processedData, propertyPresentation.Presentation); // .ParentPresentation.Presentations);

        }

        /// <summary>
        /// Retrieves all the information the object has for the presentations
        /// </summary>
        private static void ProcessSingleObject(IHasExposedProperties hasExposedProperties,
                                                IHasExposedProperties parent,
                                                object key,
                                                Dictionary<object, PresentationData> processedData,
                                                Presentation presentation)
        {
            Color? iconColor;
            Color? termColor;
            string propertyTerm;
            string propertyIconName;
            string caption;
            string keyNameToCheckFor;
            string entryKey;
            string propertyTermTooltip;
            TooltipSettings /*string*/ valueTooltipSettings;
            string captionTooltip;
            PropertyResult? propertyResult;
            bool showBar;
            float? normalizedValue1;
            float? normalizedValue2;
            int? barWidth;
            EntityID? entityID;
            bool? isSeenDirectly;
            bool makeClickable;
            bool showEntityTypeButton;

            PresentationType presentationType = PresentationTypeCategoryProcessor.GetPresentationTypeFromData(presentation);

            PresentationNode.GetDataToDisplay(hasExposedProperties, parent, key, processedData,
                presentation, presentationType, out iconColor, out termColor, out propertyTerm, out propertyIconName,
                out caption, out keyNameToCheckFor, out entryKey, 
                out propertyTermTooltip, out captionTooltip, out valueTooltipSettings,
                out propertyResult, out showBar, out normalizedValue1, out normalizedValue2, out barWidth,
                out entityID, out makeClickable, out showEntityTypeButton, 
                out isSeenDirectly);

            /* GetDataToDisplay(hasExposedProperties, key, processedData, 
             presentation, out propertyColor, out propertyTerm, out propertyIconName, out caption, out keyNameToCheckFor, out lineKey, out propertyTermTooltip, out propertyResult, out clickablePropertyEntityID);
        */
        }

        // merge with PresentationNode.GetDataToDisplay() !!!
        /*   public static void GetDataToDisplay(
               IHasExposedProperties hasExposedProperties, 
               object key,           
               Dictionary<object, PresentationData> processedData, 
               Presentation presentation,
               out Color? propertyColor, out string propertyTerm, out string propertyIconName, out string caption, 
               out string keyNameToCheckFor, out string lineKey, 
               out string propertyTermTooltip, 
               out PropertyResult? propertyResult, out EntityID? clickablePropertyEntityID)
           {
               propertyTermTooltip = null;
               propertyColor = null;
               propertyTerm = null;
               propertyIconName = null;
               caption = null;
               keyNameToCheckFor = null;
               lineKey = null;
               clickablePropertyEntityID = null;

                propertyResult = hasExposedProperties.GetPropertyValue(presentation.PropertyName, The.InGameUI.UIAllegiance.SharedKnowledge);
               if (propertyResult.HasValue == true)//Did we recieve a result from the object with exposed properties?
               {
                 

                   keyNameToCheckFor = DetermineKeyName(presentation, propertyResult.Value, hasExposedProperties);//Get 

                   clickablePropertyEntityID = DetermineClickableEntityID(presentation, hasExposedProperties);

                   //caption should only be set to null when we have a list of things//Is this still true? do we need to be able to set it to null in other cases?
                   PresentationType presentationType = GetPresentationTypeFromData(presentation);

                   //this code assumes that our presentation type either uses an icon or a term, never both

                   float? propertyValue = propertyResult.Value.NumberResult;
                   if (presentationType != null)
                   {
                       propertyIconName = presentationType.GetIcon(propertyValue.Value, keyNameToCheckFor);

                       propertyTerm = presentationType.GetTerm(propertyValue.Value, keyNameToCheckFor);

                       propertyTermTooltip = presentationType.GetTermTooltip(propertyValue.Value, keyNameToCheckFor);
                   }
                   else
                   {
                       propertyTerm = propertyResult.Value.StringResult;
                       //If we are not using a presentation type, then that means that we already have the resulting text, currently this only works the term and not for icons
                   }

  #if DEBUG || PROFILE
                   if (propertyTerm != null)
                   {
                       if (Kensei.Dev.Options.GetOption("Dev.Show property values"))
                       {
                           propertyTerm = propertyResult.Value.NumberResult.Value + " " + propertyTerm;//Show the property value in debug mode
                       }
                   }
  #endif

                   if (propertyIconName != null)
                   {
                       propertyColor = presentationType.GetColor(propertyValue.Value, keyNameToCheckFor);

                       //DetermineEntryKeyForIcon(out lineKey, presentation, propertyIconName);
                   }

                   if (propertyTerm != null || propertyIconName != null)
                   {
                       DetermineCaption(ref caption, propertyTerm, presentation, hasExposedProperties);
                       //new: use the same method for generating a key for a text and an icon entry, the keyed component decides what to do with the data
                       DetermineEntryKey(caption, presentation, hasExposedProperties, out lineKey);

                       //We have data for this presentation! Add it
                       processedData.Add(key, new PresentationData(caption, propertyTerm, propertyIconName, propertyResult, normalizedValue, lineKey, clickablePropertyEntityID, propertyColor, propertyTermTooltip));
                     
                   }
               }
           }*/

        public static PresentationType GetPresentationTypeFromData(Presentation presentation)
        {
            if (presentation.PresentationTypeKey != null)
            {
                return GameData.Instance.AllPresentationTypes[presentation.PresentationTypeKey];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// This will only make a difference for type dependent presentation types, this key will determine which thresholds are used
        /// </summary>
        /// <param name="presentation"></param>
        /// <param name="result"></param>
        /// <param name="hasExposedProperties"></param>
        /// <returns></returns>
        public static string DetermineKeyName(Presentation presentation, PropertyResult result, IHasExposedProperties hasExposedProperties)
        {
            if (presentation.KeyNameForTypeDependentPresentationToUse == Presentation.KeyNameForTypeDependentPresentation.Custom) // UsePropertyKeyNameForTypeDependentPresentation)
            {
                return result.PropertyKeyName; // Custom
            }
            else
            {
                return hasExposedProperties.KeyName; // Standard
            }

        }
    }
}

