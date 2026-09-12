using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.PropertyPresentation
{
    /// <summary>
    /// the properties inside a group will be displayed indented, possibly with a border around them
    /// </summary>
    public class GroupNode: PresentationNode
    {
       
       // public Sorting Sorting; // never used..?

        public bool HasBorder;

        /// <summary>
        /// set to true if horizontal connector lines should be shown to the GroupNodes inside this.
        /// </summary>
        public bool ShowConnectors;

        /// <summary>
        /// always stays at top
        /// </summary>
        public GroupHeader GroupHeader;


        /// <summary>
        /// static option
        /// </summary>
        public PresentationNode[] Nodes;

        public DynamicList DynamicList;

        /// <summary>
        /// no indentation for outermost groups
        /// </summary>
        [XmlIgnore]
        public bool IsOuterGroup = false;


       

        const int headingHeight = 25;

        const int nestedGroupIndentation = 14;


        public enum HeaderType { None, Static, Dynamic }

        public override void Display(PresentationTypeCategory categoryToProcess,
                                     IHasExposedProperties hasExposedProperties,
                                     IHasExposedProperties parent,
                                     IKeyedEntryComponent populatable,
                                     bool isIndented,
                                 //    out string entryKey,
                                     ref Dictionary<object, object> entryKeys,
                                     ref int? numberOfItems)
        {
            
            // start a new dict for this groups members. Each GroupNode manages its own cleanup.
            Dictionary<object, object> nestedEntryKeys = null;

            //return;

            // add/retrieve the container
           

            UIComponent container; // a panel..? or delete this.
            IKeyedEntryComponent entryContainer = null;

            if (!populatable.TryGetEntry(this, out container))
            {
              
                // the container will be removed immediately in parent cleanup if no members are added to it... so this could be optimized, but we have no lookahead...
                container = populatable.AddGroup(this, HasBorder, IsOuterGroup? 0 : nestedGroupIndentation); 

                container.OrderByTag2 = SortOrder ?? 0;
            }

            entryContainer = (IKeyedEntryComponent)container;
            Grid containerAsGrid = entryContainer as Grid;

            entryContainer.BeginAddingEntries();

            // add the heading first, if it exists:
            HeaderType headerType = HeaderType.None; // false;
            bool? dynamicHeaderIsDisplayed = null;
            bool indentMembers = false;
            if (this.GroupHeader != null)
            {
                
                headerType = GroupHeader.GetHeaderType();

              
                bool isDisplayed = DisplayEntry(categoryToProcess, hasExposedProperties, parent, entryContainer,
                        GroupHeader.Presentation, 0, ref nestedEntryKeys, headingHeight, NormalIconPaddingLeft, NormalTextPaddingLeft, 
                        headerType == HeaderType.Static, true);

                if (headerType == HeaderType.Dynamic)
                {
                    if (isDisplayed)
                    {
                        dynamicHeaderIsDisplayed = true; // a dynamic header that isn't displayed should suppress the whole group.
                    }
                    else
                    {

                    }
                }


                indentMembers = true; // as an alternative, this flag could be set in the Presentation object..

                // layout fixes:
                if (containerAsGrid != null) // hacky..
                {
                    containerAsGrid.TopMargin = 1;
                    containerAsGrid.BottomMargin = 1;
                }
            }


            List<string> childGroupsWithHeaders = null;
            if (this.DynamicList != null)
            {
                // populate from a dynamic list and a filter: 
                List<IHasExposedProperties> hasPropertiesList = new List<IHasExposedProperties>();

                hasExposedProperties.GetChildren(DynamicList.HasPropertiesList, ref hasPropertiesList, DynamicList.Filter, null, null, null, null, The.InGameUI.UIAllegiance.SharedKnowledge);

                if (SetCountAsSummary)
                    numberOfItems = hasPropertiesList.Count;

                int order = 1;                               

                // only indent these if there is a group header in the same grid:
                foreach (var item in hasPropertiesList)
                {
                    // the dynamic list cannot render as groups, headers etc., hmmm...
                   // string entryKey;
                    DisplayEntry(categoryToProcess, 
                        item, // child
                        hasExposedProperties, // this is the parent of the children
                        entryContainer,
                        DynamicList.Presentation, order, /*out entryKey,*/ ref nestedEntryKeys, 
                        iconPaddingLeft: IndentedMemberIconPaddingLeft,
                        textPaddingLeft: IndentedMemberTextPaddingLeft);

                    order++; //??     
                    
                }

                if (hasPropertiesList.Count > 0)
                {
                    // layout fixes:
                    if (containerAsGrid != null) // hacky..
                    {
                        containerAsGrid.BottomMargin = 5; // this is needed because single items are slim

                        if (this.GroupHeader == null)
                        {
                            containerAsGrid.TopMargin = 4; // compensate for no group header
                        }
                    }
                }
            }
            else if (Nodes != null)
            {
                if (SetCountAsSummary)
                    numberOfItems = Nodes.Length;

                // populate from static list:
                // only indent these if there is a group header in the same grid:
                foreach (var item in Nodes)
                {
                  
                    item.Display(categoryToProcess, hasExposedProperties, null, entryContainer, indentMembers, ref nestedEntryKeys, ref numberOfItems);
                }
            }

            PresentationTypeCategoryProcessor.CleanupAndSortEntries(entryContainer, null, null, nestedEntryKeys);

            entryContainer.EndAddingEntries();

            // add this grid to the parent dict of keys, but only if there are entries in it!
            if (nestedEntryKeys != null && 
                ((headerType == HeaderType.None && nestedEntryKeys.Count > 0) 
                || (headerType == HeaderType.Dynamic && dynamicHeaderIsDisplayed == true /*&& nestedEntryKeys.Count > 1*/)// an optional (not static) header that was not displayed, should suppres all the members too.
                || (headerType == HeaderType.Static && nestedEntryKeys.Count > 1)) // a static header by itself does not count
                )
            {
                Common.AddToDictionary(ref entryKeys, this, this); // use the group node as key..? // entryContainer, entryContainer);

                DisplayConnectors(containerAsGrid, childGroupsWithHeaders);
            }


        }

        public override void PreInitValidate(ref List<string> listOfErrors)
        {
            if (GroupHeader != null)
            {
                GroupHeader.PreInitValidate(ref listOfErrors);
            }

            if (Nodes != null)
            {
                foreach (var item in Nodes)
                {
                    item.PreInitValidate(ref listOfErrors);
                }
            }
        }

        /// <summary>
        /// this is grid-specific, so will not put it in the IKeyed... interface
        /// </summary>
        /// <param name="container"></param>
        private void DisplayConnectors(Grid container, List<string> childGroupsWithHeaders)
        {
            if (this.ShowConnectors)
            {
                // vertical line:
                int height = container.Height;

                int startY = headingHeight - 4;

                // don't look into nested grids:
                UIComponent verticalLine = container.FindChildById(UIComponent.DataControlID.Connector, true);

                int vertXPos = IsOuterGroup ? 9 : 15;


                if (verticalLine == null)
                {
                    Image verticalLineImage = new Image(The.InGameUI.gui);                   
                    verticalLineImage.SetSkinLocation(SkinState.Normal,The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("lcd_connectionline_vertical"));
                    verticalLineImage.Position = new Point(vertXPos, startY);
                    verticalLineImage.ScaleImageToSizeOfControl = true;
                    verticalLineImage.ID = UIComponent.DataControlID.Connector;
                    container.Add(verticalLineImage);
                   // verticalLineImage.ZOrder = 1f;

                    verticalLine = verticalLineImage;
                }
                // the vertical connector is sized after the horizontal connectors.


                // horizontal connectors:
                List<UIComponent> connectors = null;
                container.FindChildrenById(UIComponent.DataControlID.HorizontalConnector, ref connectors, true);

           
                // find child header rows, add (or re-use) a horizontal connector to each:
                int? lastHorizConnectorYPos = null;
                foreach (var item in container.Entries) //ByKey)
                {
                    object key = item.Tag1;
                    GroupNode childGroup = key /*item.Key*/ as GroupNode;
                     if (childGroup != null
                         && childGroup.GroupHeader != null)
                     {
                         // find a connector:
                         Image horizLineImage;
                         if (connectors != null && connectors.Count > 0)
                         {
                             horizLineImage = (Image)connectors[0];
                             connectors.RemoveAt(0);
                         }
                         else
                         {
                             horizLineImage = new Image(The.InGameUI.gui);
                             horizLineImage.SetSkinLocation(SkinState.Normal,The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("lcd_connectionline_horizontal"));                             
                             horizLineImage.ScaleImageToSizeOfControl = true;
                             horizLineImage.Width = 13;
                             horizLineImage.ID = UIComponent.DataControlID.HorizontalConnector;
                             container.Add(horizLineImage);
                         }

                         lastHorizConnectorYPos = item.Y + 9; // item.Value.Y + 9;
                         horizLineImage.Position = new Point(vertXPos, lastHorizConnectorYPos.Value);
                     }                    
                }

                // make sure the vertical line is connected to the last group header below it. If there are no group headers, then extend the line to the last row.
                if (lastHorizConnectorYPos.HasValue)
                {
                    verticalLine.Height = lastHorizConnectorYPos.Value - startY;
                }
                else
                {
                    UIComponent lastRow = container.Entries[container.Count - 1];
                    int lastRowPos = lastRow.Y + (int)(0.85f * lastRow.Height);
                    verticalLine.Height = lastRowPos - startY;     
                }
               

                // cleanup unused connectors:
                if (connectors != null && connectors.Count > 0)
                {
                    foreach (var item in connectors)
                    {
                        container.Remove(item);
                    }
                }

            }
        }
        
    }


    

    public class DynamicList
    {
        /// <summary>
        /// Dynamic option: key to the list of static functions that return a list of objects with exposed properties.
        /// can be used instead of the static array!
        /// </summary>
        public string HasPropertiesList;
        public PropertyCondition Filter;// body part condition

        /// <summary>
        /// for child /dynamic population only
        /// </summary>
        public Presentation Presentation;

    }

    /// <summary>
    /// will have less indentation and bigger padding than group members
    /// </summary>
    public class GroupHeader
    {
        public Presentation Presentation;

        
     //   public bool AlwaysDisplayed = false;

        /// <summary>
        /// returns if this is a static text item
        /// </summary>
        /// <returns></returns>
     /*   public GroupNode.HeaderType GetHeaderType()
        {
            if (Presentation.CaptionSource == PropertyPresentation.Presentation.CaptionType.Static
                && Presentation.CaptionData != null
                && Presentation.PropertyNameForValue == null)
            {
                return GroupNode.HeaderType.Static;
            }
            else return GroupNode.HeaderType.Dynamic;
        }*/

        /// <summary>
        /// returns if this is a static text item
        /// </summary>
        /// <returns></returns>
        public GroupNode.HeaderType GetHeaderType()
        {
            if (Presentation.Caption != null
                && Presentation.Caption.StaticString != null
                && Presentation.PropertyNameForValue == null)
            {
                return GroupNode.HeaderType.Static;
            }
            else return GroupNode.HeaderType.Dynamic;
        }

        public void PreInitValidate(ref List<string> listOfErrors)
        {
            Presentation.PreInitValidate(ref listOfErrors);
        }
    }
}
