using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.SimSide.Entities;
using UWGame.Control;

namespace UWGame.ClientSide.Interface
{
    public class FullLCDPanel
    {
       
        public delegate bool TypeIsRepresented(object type);
        public delegate void SetCollapsedSummary(CollapsablePanel cp, object type);

       
     /*  
        // the grey silver part:
        private const int silverBandSizeLeft = 12;
        private const int silverBandSizeRight = 13;
        private const int silverBandSizeTop = 12; //15;
        private const int silverBandSizeBottom = 11;*/

      /*  private const int edgeSizeLeft = 0;
        private const int edgeSizeRight = 0;
        private const int edgeSizeTop = 0;
        private const int edgeSizeBottom = 0;
        */


       
        private const int lcdPaddingLeft = 12;
        private const int lcdPaddingRight = 12;
        private const int lcdPaddingTop = 12; // 0; 
        private const int lcdPaddingBottom = 12; // 0;


        /// <summary>
        /// Surface is the area in the lcd screen to add controls to. 
        /// Edges are drawn with overlay!!!
        /// </summary>
        /// <param name="window"></param>
        /// <param name="heightFraction"></param>
        /// <param name="margin"></param>
        /// <param name="display"></param>
        /// <param name="edges"></param>
        /// <param name="surface"></param>
        /// <param name="lcdScreen"></param>
        /// <param name="roomForScroller"></param>
   /*     public static void AddLCDPanelFitWindow(CommonInterface intf, Window window, float heightFraction, Point? margin,
            out Box display, out Box edges, out UIComponent surface, ref LCDScreen lcdScreen, bool roomForScroller)
        {
            int xMarginToUse;
            Point position;
            if (margin.HasValue)
            {
                xMarginToUse = margin.Value.X;
                position = new Point(margin.Value.X, margin.Value.Y);
            }
            else
            {
                xMarginToUse = window.Margin;
                position = new Point(0, 0); //new Vector2(10, 10);
            }


            int width = window.Width - 2 * xMarginToUse - (roomForScroller ? Grid.ScrollBarAndGapMain - xMarginToUse : 0);//(roomForScroller? 40 : 0);
            int height = (int)(window.Height * heightFraction);
            AddLCDPanel(intf, window, position, width, height, out display, out edges, out surface, ref lcdScreen); //, PanelType.Green);
        }*/

        public static Bar AddLCDDividerLine(GUIManager gui, int yPos, int xMargin, UIComponent lcdSurface)
        {
          //  return AddLCDLine(gui, new Point(xMargin, yPos), lcdSurface.Width - 2 * xMargin - edgeSizeLeft - edgeSizeRight - Grid.ScrollBarAndGapMain, lcdSurface);
            return AddLCDLine(gui, new Point(xMargin, yPos), lcdSurface.Width - 2 * xMargin - Grid.ScrollBarAndGapMain, lcdSurface);
        }


        /// <summary>
        /// A surface extends all the way to the right to accomodate a scroll bar. 
        /// This method gives the content width by subtracting the right margins.
        /// </summary>
        /// <param name="lcdSurface"></param>
        /// <returns></returns>
        public static int GetContentWidthFromLCDSurface(UIComponent lcdSurface)
        {
          //  return lcdSurface.Width - edgeSizeRight - silverBandSizeRight - Grid.ScrollBarAndGapMain;
            return lcdSurface.Width - lcdPaddingRight - Grid.ScrollBarAndGapMain;
        }

        public static Bar AddLCDLine(GUIManager gui, Point pos, int width, UIComponent lcdSurface)
        {
            Bar underline = new Bar(gui);
            lcdSurface.Add(underline);
            underline.Position = pos;
            underline.EdgeSize = 6;
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("lcd_line");
            underline.SetSkinLocation(SkinState.Normal,rect);
            underline.Width = width; // crtContent.Width - 2 * leftMargin;
            underline.Height = rect.Height;
            underline.RenderType = RenderType.CRTAndLCD;

            return underline;
        }

        public static Bar AddLCDLineThin(GUIManager gui, Point pos, int width, UIComponent lcdSurface)
        {
            Bar underline = new Bar(gui);
            lcdSurface.Add(underline);
            underline.Position = pos;
            underline.EdgeSize = 1;
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("lcd_thinline");
            underline.SetSkinLocation(SkinState.Normal,rect);
            underline.Width = width; // crtContent.Width - 2 * leftMargin;
            underline.Height = rect.Height;
            underline.RenderType = RenderType.CRTAndLCD;

            return underline;
        }

        /// <summary>      
        /// </summary>
        /// <param name="intf"></param>
        /// <param name="window"></param>
        /// <param name="bottomMargin"></param>
        /// <param name="margin"></param>
        /// <param name="display"></param>
        /// <param name="edges"></param>      
        /// <param name="lcdScreen"></param>
        /// <param name="roomForScroller"></param>
        public static void AddLCDPanelFitWindowWithBottomMargin(CommonInterface intf, Window window, int bottomMargin, Point? margin,
            out Box display, out UIComponent surface, ref LCDScreen lcdScreen, int? width = null, int? rightMargin = null)
        {
            int leftMarginToUse, topMarginToUse;
            Point position;

            if (margin.HasValue)
            {
                leftMarginToUse = margin.Value.X;
                topMarginToUse = margin.Value.Y;
                position = new Point(margin.Value.X, margin.Value.Y);
            }
            else
            {
                leftMarginToUse = window.Margin;
                topMarginToUse = 0;
                position = new Point(0, 0); 
            }
            

           // int widthToUse = width ?? GetLCDPanelFullWidth(window, leftMarginToUse);
            int widthToUse;

            if (width != null)
            {
                widthToUse = width.Value;
            }
            else
            {
                if (rightMargin.HasValue)
                {
                    widthToUse = window.Width - leftMarginToUse - rightMargin.Value;
                }
                else
                {
                    widthToUse = window.Width - 2 * leftMarginToUse;
                }
            }

            int height = (int)(window.Height - bottomMargin - topMarginToUse);
            AddLCDPanel(intf, window, position, widthToUse, height, out display, out surface, ref lcdScreen); 
        }

      /*  public static int GetLCDPanelFullWidth(Window window, int xMarginToUse)
        {
          //  int width = window.Width - 2 * xMarginToUse - (roomForScroller ? Grid.ScrollBarAndGapMain - xMarginToUse : 0);//(roomForScroller? 40 : 0);

            int width = window.Width - 2 * xMarginToUse;

            return width;
        }*/

        /// <summary>
        /// call this to resize an existing lcd screen according to a certain content width
        /// </summary>
        public static void SetFullsizeLCDScreenDimensionsFromContent(Window window, Box display, LCDScreen lcdScreen, int margin, int surfaceWidth, int? surfaceHeight = null)
        {
           
            display.Width = surfaceWidth + lcdPaddingLeft + lcdPaddingRight;

            if (surfaceHeight.HasValue)
            {
                display.Height = surfaceHeight.Value + lcdPaddingTop + lcdPaddingBottom;
            }

            Rectangle rect = new Rectangle(display.X, display.Y, display.Width, display.Height);

            lcdScreen.SetDimensions(rect);

            window.Width = display.Width + 2 * margin;

           
        }

        /// <summary>
        /// Cannot be called before DisplayPanelRenderer.LoadContent()!
        /// 
        /// the display and lcdscreen have the same dimensions. the surface is a bit smaller, so the content does not interfere with the border graphics
        /// </summary>
        /// <param name="intf"></param>
        /// <param name="window"></param>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="display"></param>
        /// <param name="edges"></param>
        /// <param name="surface"></param>
        /// <param name="lcdScreen"></param>
        public static void AddLCDPanel(CommonInterface intf, Window window, Point position, int width, int height,
            out Box display, out UIComponent surface, ref LCDScreen lcdScreen) 
        {
        
            Rectangle rect;

            window.HasCRTOrLCDComponents = true;
            window.HasOverlayComponents = true;

            // display edge and 'screen'
            display = new Box(intf.gui); 
            window.Add(display);
            display.DebugTag = "lcdBackground";
            display.RenderType = RenderType.CRTAndLCD;
            rect = intf.gui.GUISpriteSheet.GetSourceRectangle("basic_display_panel");
            display.SetSkinLocation(SkinState.Normal,rect);
            display.CornerSize = 12; // 28;

           
            display.Position = new Point(position.X, position.Y);
            display.Width = width;
            display.Height = height; 
          
                     
            // the surface starts where the borders end:
            surface = new UIComponent(intf.gui);
            surface.Position = new Point(display.X + lcdPaddingLeft, display.Y + lcdPaddingTop);
            surface.Width = width - lcdPaddingLeft - lcdPaddingRight; 
            surface.Height = display.Height - lcdPaddingTop - lcdPaddingBottom; 
            surface.RenderType = RenderType.CRTAndLCD;
            window.Add(surface);

            
            lcdScreen = intf.DisplayPanelRenderer.AddLCD(                   
                    display, window.Level, window, true); //, LCDScreen.ReflectionToUse.Circular);
            
        }

        /// <summary>
        /// adds a grid that fills the surface, with a scrollbar enabled if the grid contents overflow the height of the surface.
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="lcdSurface"></param>
        /// <param name="gridTopMargin"></param>
        /// <returns></returns>
        public static Grid AddGridWithFixedItemHeights(GUIManager gui, UIComponent lcdSurface, int gridTopMargin, int gridBottomMargin = 0)
        {
            Grid grid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            grid.FixedItemHeights = true;
            grid.RenderType = RenderType.CRTAndLCD;
            lcdSurface.Add(grid);
            grid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grid.Width = lcdSurface.Width;
            grid.Height = lcdSurface.Height - gridTopMargin - gridBottomMargin;
            grid.ItemHeight = 22;//22; 
            grid.Position = new Point(0, gridTopMargin);

            grid.CanGrowInHeight = false; // shows the scrollbar
            grid.ScrollBarEnabled = true;

            grid.Selectability = Grid.SelectabilityOptions.Single;

            return grid;
        }

        public static Grid AddTreeGrid(GUIManager gui, CollapsablePanel.PanelType panelType) //, UIComponent lcdSurface)
        {
            Grid grid = new Grid(gui, ListBoxType.LCD /*ListBoxType.Main*/, Label.LabelType.LCDNormal);
            grid.FixedItemHeights = false;
            grid.RenderType = RenderType.CRTAndLCD;          
            grid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            //grid.Width = lcdSurface.Width;
            int gridTopMargin = 10; // 30;
            //grid.Height = lcdSurface.Height - gridTopMargin;
            grid.ItemHeight = (panelType == CollapsablePanel.PanelType.DropDownBig? 26: 22); //22; 
            grid.Position = new Point(0, gridTopMargin);
            grid.CanGrowInHeight = true; // ??

            return grid;
        }

        public static void PopulateCategoryGrid<T, TE, CATTYPE>(GUIManager gui, Grid outerGrid, CollapsablePanel.PanelType panelType, Label.LabelType labelType,
          //  TypeIsRepresented typeIsRepresented,
            SetCollapsedSummary setCollapsedSummary,
            ClickHandler clickHandler,
             Dictionary<T, TE> dictionary)
            where CATTYPE : ICategoryType
            where T : IHasCategory<CATTYPE>//, IGameData 
        {
            outerGrid.BeginAddingEntries();


            CollapsablePanel cpCategory;
            Grid categoryGrid = null;
            UIComponent item = null;

            List<Grid> categoryGrids = new List<Grid>();

            UIComponent valueLabel;
            int xValueColumn = 195;

            string value;

            // this stores the panels when Summary has been reset.
            List<CollapsablePanel> initializedPanels = new List<CollapsablePanel>();

            // outer level is an item "category"
            // add nested grids for each, containing item types...
            // foreach (KeyValuePair<T, List<TE>> kvp in dictionary)
            foreach (KeyValuePair<T, TE> kvp in dictionary)
            {
                // see if the category is represented:
                if (outerGrid.TryGetEntry(kvp.Key.Category, out item)) // cpCategory))
                {
                    cpCategory = (CollapsablePanel)item;
                    if (!initializedPanels.Contains(cpCategory))
                    {
                        cpCategory.Summary = ""; // clear the summary of the data of the previous entity!
                        initializedPanels.Add(cpCategory);
                    }

                    categoryGrids.Clear();
                    cpCategory.ExpandedPanel.FindChildOfType<Grid>(null, ref categoryGrids);
                    categoryGrid = categoryGrids[0];
                }
                else
                {
                    // create the category node
                    if (panelType == CollapsablePanel.PanelType.DropDownBig)
                    {
                        cpCategory = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownBig);
                        cpCategory.HeadingYPos = 4;
                        cpCategory.CollapsedHeight = outerGrid.ItemHeight;
                        outerGrid.AddEntry(kvp.Key.Category, cpCategory);
                        cpCategory.Init(); //CollapsablePanel.PanelType.DropDown);
                    }
                    else
                    {
                        cpCategory = new CollapsablePanel(gui, panelType); // CollapsablePanel.PanelType.Node);
                        cpCategory.HeadingYPos = 4;
                        cpCategory.CollapsedHeight = outerGrid.ItemHeight;
                        outerGrid.AddEntry(kvp.Key.Category, cpCategory);
                        cpCategory.Init(); //CollapsablePanel.PanelType.Node);

                        IHasIcon hasIcon = kvp.Key.Category as IHasIcon;
                        if (hasIcon != null && hasIcon.IconSpriteName != null)
                        {
                            cpCategory.SetIcon(hasIcon.IconSpriteName);
                        }                        
                    }

                    cpCategory.Title = kvp.Key.Category.Name;

                    cpCategory.Width = outerGrid.Width;


                    categoryGrid = new Grid(gui, ListBoxType.LCD /*ListBoxType.Main*/, labelType);
                    categoryGrid.IsOuterGrid = false;

                    // categoryGrid.Position = new Point(
                    // categoryGrid.DebugTag = "categoryGrid";

                    categoryGrid.FixedItemHeights = true; // false;
                    categoryGrid.Width = cpCategory.Width; // make grid fill the collapsable panel
                    cpCategory.AddContent(categoryGrid); // .ExpandedPanel.Add(categoryGrid);

                    categoryGrid.ScrollBarEnabled = false;
                    categoryGrid.ItemHeight = 22;
                    categoryGrid.CanGrowInHeight = true;
                    categoryGrid.Font = GUIManager.LCDandHUDBodyFontPath;


                   // cpCategory.IsExpanded = true; // DEBUGGING!

                    // categoryGrid.BeginAddingEntries();
                    // touchedCategoryGrids.Add(categoryGrid);
                }


                categoryGrid.BeginAddingEntries();

                value = kvp.Value.ToString();

                // see if the item is represented:   
                if (categoryGrid.TryGetEntry(kvp.Key, out item)) // grdSkills.TryGetItem(kvp.Key.SkillCategory, out item)) // cpCategory))
                {
                    // update the item value label
                    valueLabel = item.FindChildById("value");
                    if (valueLabel != null)
                    {   
                        // formatting: "HackSaw       2"

                        Label lblValue = (Label)valueLabel;
                        lblValue.Text = value;
                        cpCategory.RightJustifyLabel(lblValue);
                    }
                    else
                    {
                        // formatting: "HackSaw 2"

                        valueLabel = item.FindChildById("captionAndValue");
                        Label lblValue = (Label)valueLabel;
                        lblValue.Text = kvp.Key.Name + " " + value;
                       // cpCategory.RightJustifyLabel(lblValue);
                    }
                }
                else
                {
                    // add the item
                    IHasIcon hasIcon = kvp.Key as IHasIcon;
                    if (hasIcon != null)
                    {
                        categoryGrid.AddEntryRightJustifyValue(kvp.Key, gui.GUISpriteSheet.GetSourceRectangle(hasIcon.IconSpriteName), 34, 60, kvp.Key.Name, cpCategory.GetPaddingRight(), value);                    
                    }
                    else
                    {
                        if (clickHandler != null)
                        {
                            categoryGrid.AddEntryAndButton(kvp.Key, null, kvp.Key.Name, cpCategory.GetPaddingRight(), value, clickHandler, new IGameDataButtonEventArgs(kvp.Key));
                        }
                        else 
                        {
                            categoryGrid.AddEntryRightJustifyValue(kvp.Key, null, 0, 34, kvp.Key.Name, cpCategory.GetPaddingRight(), value);
                        }
                    }
                    

                    /*
                    if (panelType == CollapsablePanel.PanelType.DropDown)
                    {
                        categoryGrid.AddEntryRightJustifyValue(kvp.Key, null, kvp.Key.Name, cpCategory.GetPaddingRight(), value);
                        //categoryGrid.AddEntry(kvp.Key, kvp.Key.Name, xValueColumn, value);
                    }
                    else
                    {
                        categoryGrid.AddEntry(kvp.Key, 34, kvp.Key.Name, xValueColumn, value);
                    }*/
                }

                //**********
                if (setCollapsedSummary != null)
                {
                    setCollapsedSummary(cpCategory, value);
                }
                //***********
            }


            Cleanup<T, TE, CATTYPE>(outerGrid, dictionary, null, null);

            outerGrid.EndAddingEntries();
        }

/*
        public static void PopulateGatherResourcesGrid<T, TE, CATTYPE>(GUIManager gui, Grid outerGrid, 
            TypeIsRepresented typeIsRepresented,
            SetCollapsedSummary setCollapsedSummary,
            ClickHandler clickHandler,
             Dictionary<T, TE> dictionary)
            where CATTYPE : ICategoryType
            where T : IHasCategory<CATTYPE>//, IGameData 
        {
            outerGrid.BeginAddingEntries();


            CollapsablePanel cpCategory;
            Grid categoryGrid = null;
            UIComponent item = null;

            List<Grid> categoryGrids = new List<Grid>();

            UIComponent valueLabel;
            int xValueColumn = 195;

            string value;

            // this stores the panels when Summary has been reset.
            List<CollapsablePanel> initializedPanels = new List<CollapsablePanel>();

            // outer level is an item "category"
            // add nested grids for each, containing item types...
            // foreach (KeyValuePair<T, List<TE>> kvp in dictionary)
            foreach (KeyValuePair<T, TE> kvp in dictionary)
            {
                // see if the category is represented:
                if (outerGrid.TryGetItem(kvp.Key.Category, out item)) // cpCategory))
                {
                    cpCategory = (CollapsablePanel)item;
                    if (!initializedPanels.Contains(cpCategory))
                    {
                        cpCategory.Summary = ""; // clear the summary of the data of the previous entity!
                        initializedPanels.Add(cpCategory);
                    }

                    categoryGrids.Clear();
                    cpCategory.ExpandedPanel.FindChildOfType<Grid>(null, categoryGrids);
                    categoryGrid = categoryGrids[0];
                }
                else
                {
                    // create the category node
                    
                    cpCategory = new CollapsablePanel(gui, CollapsablePanel.PanelType.HUD); // CollapsablePanel.PanelType.Node);
                    cpCategory.HeadingYPos = 4;
                    cpCategory.CollapsedHeight = outerGrid.ItemHeight;
                    outerGrid.AddEntry(kvp.Key.Category, cpCategory);
                    cpCategory.Init(); //CollapsablePanel.PanelType.Node);
                                                
                    

                    cpCategory.Title = kvp.Key.Category.Name;

                    cpCategory.Width = outerGrid.Width;


                    categoryGrid = new Grid(gui, ListBoxType.Main, WindowSystem.Label.LabelType.HUDWindow);
                    categoryGrid.IsOuterGrid = false;

                    // categoryGrid.Position = new Point(
                    // categoryGrid.DebugTag = "categoryGrid";

                    categoryGrid.FixedItemHeights = true; // false;
                    categoryGrid.Width = cpCategory.Width; // make grid fill the collapsable panel
                    cpCategory.AddContent(categoryGrid); // .ExpandedPanel.Add(categoryGrid);

                    categoryGrid.ScrollBarEnabled = false;
                    categoryGrid.ItemHeight = 22;
                    categoryGrid.ResizeToFit = true;
                    categoryGrid.Font = GUIManager.LCDInterfaceFontPath;


                    // cpCategory.IsExpanded = true; // DEBUGGING!

                    // categoryGrid.BeginAddingEntries();
                    // touchedCategoryGrids.Add(categoryGrid);
                }


                categoryGrid.BeginAddingEntries();

                value = kvp.Value.ToString();

                // see if the item is represented:   
                if (categoryGrid.TryGetItem(kvp.Key, out item)) // grdSkills.TryGetItem(kvp.Key.SkillCategory, out item)) // cpCategory))
                {
                    // update the item value label
                    valueLabel = item.FindChildById("value1");
                    if (valueLabel != null)
                    {
                        // formatting: "HackSaw       2"

                        Label lblValue = (Label)valueLabel;
                        lblValue.Text = value;
                        cpCategory.RightJustifyLabel(lblValue);
                    }

                   
                }
                else
                {
                    // add the item                   
                    if (clickHandler != null)
                    {
                        categoryGrid.AddEntryWithCaptionTwoValuesAndButton(kvp.Key, null, kvp.Key.Name, cpCategory.GetPaddingRight(), value, , ImageButtonType.HUDArrowRight, clickHandler, new IGameDataButtonEventArgs(kvp.Key));
                        //categoryGrid.AddEntryAndButton(kvp.Key, null, kvp.Key.Name, cpCategory.GetPaddingRight(), value, clickHandler, new IGameDataButtonEventArgs(kvp.Key));
                    }                                   

                }

                //**********
                if (setCollapsedSummary != null)
                {
                    setCollapsedSummary(cpCategory, value);
                }
                //***********
            }

                       

            Cleanup<T, TE, CATTYPE>(outerGrid, typeIsRepresented, dictionary, categoryGrids);

            outerGrid.EndAddingEntries();
        }
        */

         

        /// <summary>
        /// removes unused category and item rows in grids organized with collapsable categories
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TE"></typeparam>
        /// <typeparam name="CATTYPE"></typeparam>
        /// <param name="outerGrid"></param>
        /// <param name="amountDictionary"></param>
        /// <param name="categoryGrids"></param>
        public static void Cleanup<T, TE, CATTYPE>(Grid outerGrid, Dictionary<T, TE> amountDictionary, Dictionary<T, T> typeDictionary, Predicate<T> itemExists, Predicate<CATTYPE> categoryExists = null)
            where T : IHasCategory<CATTYPE>
            where CATTYPE : ICategoryType
        {
            // now remove unused instances and categories!
            // clean up: ***************************************************************
            List<Grid> categoryGrids = new List<Grid>();
            List<T> instancesToRemove = new List<T>();
            List<ICategoryType> categoriesToRemove = new List<ICategoryType>();
            T key;
            ICategoryType currentCategory = null;

            foreach (UIComponent gridItem in outerGrid.Entries)
            {
                categoryGrids.Clear();
                ((CollapsablePanel)gridItem).ExpandedPanel.FindChildOfType<Grid>(null, ref categoryGrids);

                Grid catGrid = categoryGrids[0];

                instancesToRemove.Clear();
                currentCategory = null;

                foreach (KeyValuePair<object, UIComponent> kvp in catGrid.EntriesByKey)
                {
                    key = (T)kvp.Key;

                    if ((itemExists != null && !itemExists(key))
                        || (amountDictionary != null && !amountDictionary.ContainsKey(key))
                        || (typeDictionary != null && !typeDictionary.ContainsKey(key)))
                    {
                        // can't remove while iterating...
                        instancesToRemove.Add(key);

                    }                   
                }

                // remove the unused items:
                foreach (T instanceToRemove in instancesToRemove)
                {
                    //currentCategory = skillToRemove.Category;
                    currentCategory = (ICategoryType)instanceToRemove.Category;
                    catGrid.RemoveEntry(instanceToRemove);
                }

                if (currentCategory != null &&
                    ((categoryExists != null && !categoryExists((CATTYPE)gridItem.Tag1)) &&
                    catGrid.EntriesByKey.Count == 0))
                {
                    // remove the categoryGrid panel too
                    /*if (currentCategory != null)
                    {*/
                        categoriesToRemove.Add(currentCategory);
                   // }
                }


            }
            // Now we can remove the unused categories:
            foreach (ICategoryType categoryToRemove in categoriesToRemove)
            {
                outerGrid.RemoveEntry(categoryToRemove);
            }



            // necessary! resize the inner grids now:
            categoryGrids.Clear();
            outerGrid.FindChildOfType<Grid>(null, ref categoryGrids);
            foreach (Grid grid in categoryGrids)
            {
                grid.EndAddingEntries();
            }
        }

      /*  public static void Cleanup<T>(Grid outerGrid, List<T> listOfItems)
        {
            object key;
            for (int i = outerGrid.EntriesByKey.Count - 1; i >= 0; i--)
            {
               // key = outerGrid.entr

            }

        }*/

        

     /*   private static void RightJustifyLabel(CollapsablePanel.PanelType type, Label lblTitleSummary, int width)
        {
            if (type == WindowSystem.CollapsablePanel.PanelType.DropDown)
            {
                lblTitleSummary.X = width - CollapsablePanel.headingSummaryRightPaddingDropDown - lblTitleSummary.TextWidth;
            }
            else
            {
                lblTitleSummary.X = width - CollapsablePanel.headingSummaryRightPaddingNode - lblTitleSummary.TextWidth;
            }
        }
        */

        /// <summary>
        /// 2 columns of grids!!! - Not currently working...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TE"></typeparam>
        /// <typeparam name="CATTYPE"></typeparam>
        /// <param name="gui"></param>
        /// <param name="outerGrid1"></param>
        /// <param name="outerGrid2"></param>
        /// <param name="xValueColumn"></param>
        /// <param name="TypeIsRepresented"></param>
        /// <param name="setCollapsedSummary"></param>
        /// <param name="dictionary"></param>
        public static void PopulateCategoryGrid<T, TE, CATTYPE>(GUIManager gui, Grid outerGrid1, Grid outerGrid2, int xValueColumn,
            TypeIsRepresented TypeIsRepresented,
            SetCollapsedSummary setCollapsedSummary,
             Dictionary<T, TE> dictionary)
            where CATTYPE : ICategoryType
            where T : IHasCategory<CATTYPE>
        {
            outerGrid1.BeginAddingEntries();
            outerGrid2.BeginAddingEntries();

            CollapsablePanel cpCategory;
            Grid categoryGrid = null;
            UIComponent item = null;

            List<Grid> categoryGrids = new List<Grid>();

            UIComponent valueLabel;
            //int xValueColumn = 200;

            string value;

            List<CollapsablePanel> initializedPanels = new List<CollapsablePanel>();

            Grid currentGrid = outerGrid1;

            // outer level is an item "category"
            // add nested grids for each, containing item types...
            // foreach (KeyValuePair<T, List<TE>> kvp in dictionary)
            foreach (KeyValuePair<T, TE> kvp in dictionary)
            {
                // see if the category is represented:
                if (outerGrid1.TryGetEntry(kvp.Key.Category, out item)) // cpCategory))
                {
                    currentGrid = outerGrid1;
                    cpCategory = (CollapsablePanel)item;
                    if (!initializedPanels.Contains(cpCategory))
                    {
                        cpCategory.Summary = ""; // clear the summary of the data of the previous entity!
                        initializedPanels.Add(cpCategory);
                    }

                    categoryGrids.Clear();
                    cpCategory.ExpandedPanel.FindChildOfType<Grid>(null, ref categoryGrids);
                    categoryGrid = categoryGrids[0];
                }
                else if (outerGrid2.TryGetEntry(kvp.Key.Category, out item)) // cpCategory))
                {
                    currentGrid = outerGrid2;
                    cpCategory = (CollapsablePanel)item;
                    if (!initializedPanels.Contains(cpCategory))
                    {
                        cpCategory.Summary = ""; // clear the summary of the data of the previous entity!
                        initializedPanels.Add(cpCategory);
                    }

                    categoryGrids.Clear();
                    cpCategory.ExpandedPanel.FindChildOfType<Grid>(null, ref categoryGrids);
                    categoryGrid = categoryGrids[0];
                }
                else
                {
                    // create the category node
                    cpCategory = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownBig);
                    cpCategory.HeadingYPos = 4;
                    cpCategory.CollapsedHeight = currentGrid.ItemHeight;
                    currentGrid.AddEntry(kvp.Key.Category, cpCategory);
                    cpCategory.Init(); //CollapsablePanel.PanelType.DropDown);
                    cpCategory.Title = kvp.Key.Category.Name;

                    cpCategory.Width = currentGrid.Width;


                    categoryGrid = new Grid(gui, ListBoxType.LCD /*ListBoxType.Main*/, Label.LabelType.LCDNormal);
                    categoryGrid.IsOuterGrid = false;

                    // categoryGrid.Position = new Point(
                    // categoryGrid.DebugTag = "categoryGrid";

                    categoryGrid.FixedItemHeights = true; // false;
                    categoryGrid.Width = cpCategory.Width; // make grid fill the collapsable panel
                    cpCategory.AddContent(categoryGrid); // .ExpandedPanel.Add(categoryGrid);

                    categoryGrid.ScrollBarEnabled = false;
                    categoryGrid.ItemHeight = 22;
                    categoryGrid.CanGrowInHeight = true;
                    categoryGrid.Font = GUIManager.LCDandHUDBodyFontPath;

                    // categoryGrid.BeginAddingEntries();
                    // touchedCategoryGrids.Add(categoryGrid);
                }


                categoryGrid.BeginAddingEntries();

                value = kvp.Value.ToString();

                // see if the skill is represented:   
                if (categoryGrid.TryGetEntry(kvp.Key, out item)) // grdSkills.TryGetItem(kvp.Key.SkillCategory, out item)) // cpCategory))
                {
                    // update the item value label
                    valueLabel = item.FindChildById("value");
                    ((Label)valueLabel).Text = value;
                }
                else
                {
                    // add the item
                    categoryGrid.AddEntry(kvp.Key, kvp.Key.Name, xValueColumn, value);
                }

                //**********
                setCollapsedSummary(cpCategory, value);
                //***********

                // alternate between the two grids:
                if (currentGrid == outerGrid1)
                {
                    currentGrid = outerGrid2;
                }
                else currentGrid = outerGrid1;
            }

            // now remove unused skills and categories!
            // clean up:
            List<T> itemsToRemove = new List<T>();
            List<ICategoryType> categoriesToRemove = new List<ICategoryType>();
            T key;
            ICategoryType currentCategory = null;

            currentGrid = outerGrid1;

            for (int i = 0; i < 2; i++)
            {
                foreach (UIComponent gridItem in currentGrid.Entries)
                {
                    categoryGrids.Clear();
                    ((CollapsablePanel)gridItem).ExpandedPanel.FindChildOfType<Grid>(null, ref categoryGrids);

                    Grid catGrid = categoryGrids[0];

                    itemsToRemove.Clear();
                    currentCategory = null;

                    foreach (KeyValuePair<object, UIComponent> kvp in catGrid.EntriesByKey)
                    {
                        key = (T)kvp.Key;
                        if (!TypeIsRepresented(key)) // !entityPanel.SelectedEntity.PersonEntity.Skills.ContainsKey(key))
                        {
                            // can't remove while iterating...
                            itemsToRemove.Add(key);
                        }
                    }

                    // remove the unused items:
                    foreach (T itemToRemove in itemsToRemove)
                    {
                        //currentCategory = skillToRemove.Category;
                        currentCategory = (ICategoryType)itemToRemove.Category;
                        catGrid.RemoveEntry(itemToRemove);
                    }

                    if (currentCategory != null && catGrid.EntriesByKey.Count == 0)
                    {
                        // remove the categoryGrid panel too
                        if (currentCategory != null)
                        {
                            categoriesToRemove.Add(currentCategory);
                        }
                    }
                }

                // Now we can remove the unused categories:
                foreach (ICategoryType categoryToRemove in categoriesToRemove)
                {
                    currentGrid.RemoveEntry(categoryToRemove);
                }



                // necessary! resize the inner grids now:
                categoryGrids.Clear();
                currentGrid.FindChildOfType<Grid>(null, ref categoryGrids);
                foreach (Grid grid in categoryGrids)
                {
                    grid.EndAddingEntries();
                }

                categoriesToRemove.Clear();
                currentGrid = outerGrid2; // clean the 2nd grid
            }

            // balance the grids so they have the same number of items:
            int difference = Math.Abs(outerGrid1.Entries.Count - outerGrid2.Entries.Count);
            if (difference > 1)
            {
                categoriesToRemove.Clear();
                int entriesMoved = 0;

                if (outerGrid1.Entries.Count > outerGrid2.Entries.Count)
                {
                    foreach (KeyValuePair<object, UIComponent> kvp in outerGrid1.EntriesByKey)
                    {
                        categoriesToRemove.Add((ICategoryType)kvp.Key);
                        difference--;
                        //entriesMoved++;
                        if (difference < 2)
                        {
                            break;
                        }
                    }

                    UIComponent cPanel;
                    foreach (ICategoryType categoryToRemove in categoriesToRemove)
                    {
                        // not currently working:
                       /* cPanel = outerGrid1.RemoveEntry(categoryToRemove);
                        outerGrid2.AddEntry(categoryToRemove, cPanel);*/
                    }

                }
                else
                {
                    foreach (KeyValuePair<object, UIComponent> kvp in outerGrid2.EntriesByKey)
                    {
                        categoriesToRemove.Add((ICategoryType)kvp.Key);
                        difference--;
                        //entriesMoved++;
                        if (difference < 2)
                        {
                            break;
                        }
                    }

                    UIComponent cPanel;
                    foreach (ICategoryType categoryToRemove in categoriesToRemove)
                    {
                        // not currently working:
                       /*cPanel = outerGrid2.RemoveEntry(categoryToRemove);
                        outerGrid1.AddEntry(categoryToRemove, cPanel);*/
                    }


                }
            }




            outerGrid1.EndAddingEntries();
            outerGrid2.EndAddingEntries();
        }


      /*  static void surface_MouseOut(InputEventSystem.MouseEventArgs args)
        {
            Interface.Instance.SetFingerCursor();
        }

        static void surface_MouseOver(InputEventSystem.MouseEventArgs args)
        {
            Interface.Instance.SetLCDCursor();
        }*/
    }
}
