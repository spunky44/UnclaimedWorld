#region File Description
//-----------------------------------------------------------------------------
// File:      ListBox.cs
// Namespace: WindowSystem
// Author:    Aaron MacDougall
//-----------------------------------------------------------------------------
#endregion

#region License
//-----------------------------------------------------------------------------
// Copyright (c) 2007, Aaron MacDougall
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of Aaron MacDougall nor the names of its contributors may
//   be used to endorse or promote products derived from this software without
//   specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using InputEventSystem;
using System.Linq;
#endregion

namespace WindowSystem
{



    /// <summary>
    /// A graphical listbox control. Uses a scrollbar to allow more entries
    /// than can fit on control. Entries are actually children to a
    /// DrawableUIComponent, which acts as a viewport, only allowing some to
    /// be seen at one time. Scrollbar is only shown if required.
    /// </summary>
    public class Grid : UIComponent, IKeyedEntryComponent
    {
        public enum SelectabilityOptions { None, Single, Multiple }

        public enum NewRowsInsertion { First, Last }

        /// <summary>
        /// we use this to control direction instead of sorting all the time
        /// </summary>
        public NewRowsInsertion InsertNewRows = NewRowsInsertion.Last;


        private bool isOuterGrid = true;

        /// <summary>
        /// Set to false if the grid shouldn't accommodate a scroll bar by making surface and viewport slimmer
        /// </summary>
        public bool IsOuterGrid
        {
            get
            {
                return isOuterGrid;
            }
            set
            {
                isOuterGrid = value;
                if (!isOuterGrid)
                {
                    // don't allow nested grids to swallow 
                    CanReceiveMouseWheelEvents = false;
                }

                RefreshMargins(); // affects room for scrollbar or not
            }
        }


        private ListBoxType type;

        private Label.LabelType labelType;

        private const int itemContentJustifyX = 2;
        private const int itemContentJustifyY = 2;


        #region Default Properties
        private static int defaultWidth = 200;
        private static int defaultHeight = 150;
        private static int defaultHMargin = 0; //5;
        private static int defaultVMargin = 0; //2;
        private static string defaultFont = "Content/Fonts/DefaultFont";
        private static Rectangle defaultSkin = new Rectangle(84, 41, 25, 25);

        public const int ScrollBarAndGapMain = 25;
        public const int GapAndScrollbarComm = 20;
        public const int GapAndScrollbarLCD = 18;
        public const int GapAndScrollbarHUD = 14;

        private bool isAddingEntries = false;


        public int GapAndScrollBar;

        /// <summary>
        /// Sets the default control width.
        /// </summary>
        /// <value>Must be greater than 0.</value>
        public static int DefaultWidth
        {
            set
            {
                Debug.Assert(value > 0);
                defaultWidth = value;
            }
        }

        /// <summary>
        /// Sets the default control height.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public static int DefaultHeight
        {
            set
            {
                Debug.Assert(value > 0);
                defaultHeight = value;
            }
        }

        /// <summary>
        /// Sets the default horizontal padding.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public static int DefaultHMargin
        {
            set
            {
                Debug.Assert(value >= 0);
                defaultHMargin = value;
            }
        }

        /// <summary>
        /// Sets the default vertical padding.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public static int DefaultVMargin
        {
            set
            {
                Debug.Assert(value >= 0);
                defaultVMargin = value;
            }
        }

        /// <summary>
        /// Sets the default text font.
        /// </summary>
        /// <value>Must be a non-empty string.</value>
        public static string DefaultFont
        {
            set
            {
                Debug.Assert(value != null);
                Debug.Assert(value.Length > 0);
                defaultFont = value;
            }
        }

        /// <summary>
        /// Sets the default skin.
        /// </summary>
        public static Rectangle DefaultSkin
        {
            set { defaultSkin = value; }
        }
        #endregion

        #region Fields

        /// <summary>
        /// optional background image
        /// </summary>
        private Box background;

        private int? backgroundRightMargin;

        /// <summary>
        /// surface and viewport will have the same width, but surface can be higher than viewport - the surface's position then decides how much of it is visible through the viewport
        /// </summary>
        public UIComponent surface;
        private UIComponent viewPort;
        private ScrollBar scrollBar;

        private List<UIComponent> entries;
        private Dictionary<object, UIComponent> entriesByKey;
        //private Dictionary<UIComponent, object> keysByEntry;
        private SpriteFont font;
        private string fontFileName;
        private UIComponent selectedItem;
        private int selectedIndex;
        private int hMargin;

        private int topMargin;
        private int bottomMargin;

        private int itemHeight = 15;
        private bool canGrowInHeight;

        private Box selectionBox;

        private SelectabilityOptions selectability;
        public SelectabilityOptions Selectability
        {
            get { return selectability; }
            set
            {
                selectability = value;
                if (selectability == SelectabilityOptions.None)
                {
                    // don't take mouse clicks
                    CanHaveFocus = false;
                }
                else
                {
                    CanHaveFocus = true;
                }
            }
        }



        /// <summary>
        /// if false, the items will have their height resize event hooked up and be able to expand/contract the grid.
        /// </summary>
        public bool FixedItemHeights = true;

        private bool scrollBarEnabled = true;
        #endregion

        #region Properties

        public UIComponent SelectedItem
        {
            get { return selectedItem; }
        }

        /// <summary>
        /// returns the surface width that is available for content - that is, up till the scrollbar
        /// </summary>
        public int SurfaceWidth
        {
            get
            {
                return surface.Width;
            }
        }

        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                if (base.Width != value)
                {
                   
                    base.Width = value;

                    RefreshMargins();
                }
            }
        }

        public int ItemHeight
        {
            get { return this.itemHeight; }
            set
            {
                if (value != itemHeight)
                {
                    this.itemHeight = value;
                    RefreshEntries();
                }

            }
        }

      /*  Label lblCapMessage;
        public string CapMessage
        {
            set
            {
                if (lblCapMessage == null)
                {
                    lblCapMessage = new Label(guiManager);
                    lblCapMessage.Init(labelType);

                }

                lblCapMessage.Text = value;
                lblCapMessage.FitToText();
            }

            get
            {
                if (lblCapMessage != null)
                {
                    return lblCapMessage.Text;
                }
                else return null;
            }
        }*/

        /// <summary>
        /// the spacing between rows. the default is 0
        /// most useful when each row has a border, or if the controls in the row have borders (like buttons)
        /// </summary>
        public int RowSpacing
        {
            get;
            set;
        }

        public ScrollBar ScrollBar
        {
            get { return scrollBar; }
        }

        /// <summary>
        /// grow, clamp or show a scrollbar together with CanGrowInHeight when the entries overflow the height     
        /// </summary>
        public bool ScrollBarEnabled
        {
            get { return scrollBarEnabled; }
            set
            {
                scrollBarEnabled = value;
                if (value == false)
                {
                    if (scrollBar != null)
                    {
                        // leave the reference...
                        Remove(scrollBar);
                    }
                }

                RefreshMargins(); // affects room for scrollbar or not
            }

        }


        /// <summary>
        /// grow, clamp or show a scrollbar together with ScrollbarEnabled when the entries overflow the height     
        /// </summary>
        public bool CanGrowInHeight
        {
            get { return this.canGrowInHeight; }
            set { this.canGrowInHeight = value; }
        }

        /// <summary>
        /// Gets the number of entries in the listbox.
        /// </summary>
        public int Count
        {
            get { return entries.Count; }
        }

        /// <summary>
        /// Get/Set the currently selected index.
        /// </summary>
        /// <value>Selection index, or -1 for no selection.</value>
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (value >= 0 && value < entries.Count)
                    Select(entries[value], value);
                else if (value == -1)
                    Select(null, -1);
            }
        }

        /// <summary>
        /// invoked when the grid surface changes height, not the grid itself
        /// </summary>
        public event ResizeHandler SurfaceHeightResize;
     

        /// <summary>
        /// Sets the font to use for listbox entries.
        /// It is important to set this to get correct scroll step
        /// </summary>
        /// <value>Must not be a valid path.</value>
        public string Font
        {
            get
            {
                return fontFileName;
            }
            set
            {
                this.fontFileName = value;
                this.font = GUIManager.ContentManager.Load<SpriteFont>(value);
                this.scrollBar.ScrollStep = this.font.LineSpacing;
                RefreshEntries();
            }
        }

        Color? color;
        /// <summary>
        /// Get/Set the text colour.
        /// </summary>
        public Color? Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                RefreshEntries();
            }
        }


        /// <summary>
        /// Sets the horizontal padding on both sides.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public int HMargin
        {
            get { return this.hMargin; }
            set
            {
                Debug.Assert(value >= 0);
                this.hMargin = value;
                RefreshMargins();
            }
        }

        /// <summary>
        /// The vertical padding in the grid.
        /// Creates space above and below the list of rows
        /// </summary>
        /// <value>Must be at least 0.</value>
        public int VMargin
        {
            get { return this.topMargin; }
            set
            {
                Debug.Assert(value >= 0);
                
                this.topMargin = value;
                this.bottomMargin = value;

                RefreshMargins();
            }
        }


        public int BottomMargin
        {
            get
            {
                return bottomMargin;
            }
            set
            {
                Debug.Assert(value >= 0);
               
                bottomMargin = value;

                RefreshMargins();
            }
        }

        public int TopMargin
        {
            get
            {
                return topMargin;
            }
            set
            {
                Debug.Assert(value >= 0);

                topMargin = value;

                RefreshMargins();
            }
        }

        /// <summary>
        /// Sets the control skin.
        /// </summary>
        public Rectangle Skin
        {
            set
            {
                this.background.SetSkinLocation(SkinState.Normal,value);
            }
        }

        protected SpriteFont SpriteFont
        {
            get { return this.font; }
        }

        #endregion

       /* public void SetBackground(string sprite, int cornerSize)
        {
            if (background == null)
            {
                background = new Box(guiManager);
                background.CornerSize = cornerSize;

                Rectangle rect = GUIManager.GUISpriteSheet.GetSourceRectangle(sprite);
                background.SetSkinLocation(SkinState.Normal,rect);

                Add(background);
            }
        }*/


        private void ResizeBackground()
        {
            if (background != null)
            {
                background.Height = Height;

                background.Width = SurfaceWidth - (backgroundRightMargin ?? 0); // Width;
            }
        }

        #region Events
        public event SelectionChangedHandler SelectedChanged;
        #endregion


        /// <summary>
        /// to add grid entries (items), call AddEntry instead!!!
        /// </summary>
        /// <param name="control"></param>
        public override int Add(UIComponent control)
        {
            return base.Add(control);
        }

        #region Constructors



        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public Grid(GUIManager guiManager, ListBoxType type, Label.LabelType labelType,
            string backgroundSprite = null, int? backgroundCornerSize = null, int? backgroundRightMargin = null)
            : base(guiManager)
        {
            this.type = type;
            this.labelType = labelType;

            this.entries = new List<UIComponent>();
            this.entriesByKey = new Dictionary<object, UIComponent>();
            //keysByEntry = new Dictionary<UIComponent, object>();
            this.selectedIndex = -1;
            this.canGrowInHeight = false;
            this.Selectability = SelectabilityOptions.None;
            this.CanReceiveMouseWheelEvents = true;

            #region Create Child Controls
            /* if (type == ListBoxType.Default)
            {
                this.box = new Box(guiManager);
                Add(this.box);
            }*/
            Rectangle rect;

            this.surface = new UIComponent(guiManager);
           // this.surface.HeightResize += surface_HeightResize;
            this.viewPort = new UIComponent(guiManager);
            //   this.viewPort.IsLCDSurface = true;

            if (backgroundSprite != null)
            {
                background = new Box(guiManager);
                background.CornerSize = backgroundCornerSize.Value;
                this.backgroundRightMargin = backgroundRightMargin;

                rect = GUIManager.GUISpriteSheet.GetSourceRectangle(backgroundSprite);
                background.SetSkinLocation(SkinState.Normal,rect);
               
                Add(background); // add under viewport..?
            }

            this.selectionBox = new Box(guiManager);
            rect = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_selection");
            this.selectionBox.SetSkinLocation(SkinState.Normal,rect);
            selectionBox.CornerSize = 3;
            selectionBox.CanHaveFocus = false;

            if (type == ListBoxType.Comm)
            {
                // avoid clipping!!!           
                GapAndScrollBar = GapAndScrollbarComm;
                this.scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.CommRoller);

            }
            else if (type == ListBoxType.Main)
            {
                GapAndScrollBar = ScrollBarAndGapMain;
                this.scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.MainRoller);
            }
            else if (type == ListBoxType.HUDAndLCD)
            { 
                GapAndScrollBar = GapAndScrollbarHUD;
                this.scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.HUD);
            }
            else if (type == ListBoxType.LCD)
            {
                GapAndScrollBar = GapAndScrollbarLCD;
                this.scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.LCD);
            }          
            else
            {
                this.scrollBar = new ScrollBar(guiManager);
            }

            this.scrollBar.DebugTag = "ScrollBarDebug";

            this.viewPort.Add(this.surface);
            Add(this.viewPort);


            //NEW: always show scrollbar:
            Add(this.scrollBar);
            #endregion



            #region Set Properties
            this.surface.CanHaveFocus = false;
            this.viewPort.CanHaveFocus = false;



            //this.scrollBar.Y = 1;
            #endregion

            #region Set Default Properties
            Width = defaultWidth;
            Height = defaultHeight;
            HMargin = defaultHMargin;
            VMargin = defaultVMargin;

            /* if (type == ListBoxType.Default)
             {
                 Skin = defaultSkin;
             }*/

            #endregion

            #region Event Handlers
            this.scrollBar.Scroll += new ScrollHandler(OnScroll);
            // Scrollbar doesn't need keyboard, so hand control over to this
            //this.scrollBar.KeyDown += new KeyDownHandler(OnKeyDown);

            #endregion
        }

       /* void surface_HeightResize(UIComponent sender)
        {
            if (SurfaceHeightResize != null)
            {
                SurfaceHeightResize.Invoke(this);
            }
        }*/


        #endregion



        /// <summary>
        /// Clean up scrollbar in case it hasn't been added.
        /// </summary>
        public override void CleanUp()
        {
            scrollBar.CleanUp();
            base.CleanUp();
        }

        /// <summary>
        /// Load default font.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            /* if (loadAllContent)
                 Font = defaultFont;
             */
            base.LoadGraphicsContent(loadAllContent);
        }

        public bool GetSelectedKey(out object key)
        {
            return GetKey(SelectedItem, out key);
        }

        public bool GetKey(UIComponent item, out object key)
        {
            foreach (KeyValuePair<object, UIComponent> kvp in entriesByKey)
            {
                if (kvp.Value == item)
                {
                    key = kvp.Key;
                    return true;
                }
            }
            key = null;
            return false;
            /*if (keysByEntry.TryGetValue(item, out key))
            {
                return true;
            }
            else
            {
                key = null;
                return false;
            }*/
        }

        /// <summary>
        /// orders the grid entries by a single value (most often the Tag on each item)
        /// </summary>
        /// <param name="keySelector"></param>
        public void Sort(Func<UIComponent, object> keySelector, Sorting sorting = Sorting.Descending)
        {
            Sort(keySelector, sorting, ref entries, RefreshEntries);
        }

        /// <summary>
        /// orders a list of entries by a single value (most often the Tag on each item)
        /// </summary>
        /// <param name="keySelector"></param>
        public static void Sort(Func<UIComponent, object> keySelector, Sorting sorting, ref List<UIComponent> entriesToSort, RefreshFunction refreshFunction)
        {
            if (sorting == Sorting.Descending)
            {
                entriesToSort = entriesToSort.OrderByDescending(keySelector).ToList();
            }
            else
            {
                entriesToSort = entriesToSort.OrderBy(keySelector).ToList();
            }

            if (refreshFunction != null)
            {
                refreshFunction.Invoke();
            }
        }

        public enum Sorting { Ascending, Descending }
        public delegate void RefreshFunction();
        /// <summary>
        /// orders the grid entries by multiple values
        /// </summary>
        /// <param name="keySelector1"></param>
        /// <param name="keySelector2"></param>
        public void Sort(
            Func<UIComponent, object> keySelector1, Sorting sorting1, 
            Func<UIComponent, object> keySelector2,
            Sorting sorting2)
        {
            Sort(keySelector1, sorting1, keySelector2, sorting2, ref entries, RefreshEntries);
        }

        public static void Sort(
            Func<UIComponent, object> keySelector1, Sorting sorting1, Func<UIComponent, object> keySelector2, Sorting sorting2, ref List<UIComponent> entriesToSort, RefreshFunction refreshFunction)
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
        }

        public void Sort(Grid.Sorting sortType, bool useFirstTag, Func<float, float> transformation = null)
        {
            if (useFirstTag == true)
            {
                if (transformation != null)
                {
                    Sort(i => transformation((float)i.OrderByTag1), sortType); // intended for a number result
                }
                else
                {
                    Sort(i => (float)i.OrderByTag1, sortType); // intended for a number result
                }
            }
            else
            {
                Sort(i => (int)i.OrderByTag2, sortType); // SortOrder gets assigned to this property
            }
        }

       

        /// <summary>
        /// Update controls to respect new margins.
        /// </summary>
        protected void RefreshMargins()
        {
           
            this.viewPort.X = this.hMargin;
            this.viewPort.Y = this.topMargin;
            this.viewPort.Height = Height - (topMargin + bottomMargin); 

            if (IsOuterGrid && scrollBarEnabled)
            {   // make room for scroll bar etc:
                this.viewPort.Width = Width - (this.hMargin * 2) - GapAndScrollBar;
                ListBox.SetSurfaceWidthForScrollbar(type, viewPort, surface);
            }
            else
            {
                this.viewPort.Width = Width - (this.hMargin * 2);
                surface.Width = viewPort.Width;
            }

            //this.surface.Width = this.viewPort.Width;
            if (scrollBarEnabled)
            {
                this.scrollBar.Viewable = this.viewPort.Height;
            }
        }

        public void SetContentWidth(int contentWidth)
        {
            Width = contentWidth + GapAndScrollBar;

            /*
            surface.Width = contentWidth;
            viewPort.Width = contentWidth;*/

        }

        /// <summary>
        /// Refresh control to take new entries into account.
        /// </summary>
        public void RefreshEntries()
        {
            if (isAddingEntries)
            {
                return;
            }

            int y = 0;

           
            foreach (UIComponent item in this.entries)
            {
                item.Y = y;


                // items can expand/contract:
                if (FixedItemHeights)
                {
                    item.Height = ItemHeight;
                }

                y += item.Height;
                y += RowSpacing;

               
                if (item.X == 0)
                {
                    item.Width = this.surface.Width;
                }
                else
                {
                    // clamp the width?
                    //  item.Width = Math.Min(item.Right, surface.Width);
                }
            }

            bool surfaceHeightWasChanged = false;
            if (this.surface.Height != y)
            {
                this.surface.Height = y;
                surfaceHeightWasChanged = true;
            }

            if (scrollBarEnabled)
            {
                this.scrollBar.MaximumValue = this.surface.Height;
            }

            if (this.canGrowInHeight)
            {
                int newHeight = this.surface.Height + (topMargin + bottomMargin); // this.topMargin * 2);
                
                if (newHeight != Height)
                {
                    Height = newHeight;
                }
            }
            else if (y > this.viewPort.Height)
            {
                if (scrollBarEnabled)
                {
                    // OLD: Always show groove:
                    scrollBar.ShowKnob = true;
                    //Add(this.scrollBar);

                    scrollBar.Visible = true;
                }
            }
            else
            {
                if (scrollBarEnabled)
                {
                    // OLD: Always show groove:
                    scrollBar.ShowKnob = false;

                    // NEW: hide the scrollbar completely
                    scrollBar.Visible = false;

                    //Remove(this.scrollBar);
                }
            }

            this.surface.Redraw();

            if (surfaceHeightWasChanged)
            {
                if (SurfaceHeightResize != null)
                {
                    SurfaceHeightResize.Invoke(this);
                }
            }
        }

        /// <summary>
        /// call this to skip recomputing of row positions, dimensions etc while updating of the grid is ongoing
        /// </summary>
        public void BeginAddingEntries()
        {
            isAddingEntries = true;
        }

        public void EndAddingEntries()
        {
            isAddingEntries = false;
            RefreshEntries();
        }

       /* public void TryRemoveEntry(object key)
        {
            UIComponent item;
            if (TryGetEntry(key, out item))
            {
                RemoveEntry(key, item);
            }
        }*/

        public bool TryRemoveEntry(object key)
        {
            if (entriesByKey.ContainsKey(key))
            {
                RemoveEntry(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Call RemoveEntry instead!  this will not update the key collections properly.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public override bool Remove(UIComponent control)
        {
            return base.Remove(control);
        }


        public void RemoveEntry(object key)
        {
            UIComponent item = entriesByKey[key];
           
            RemoveEntry(key, item);
        }


        public void RemoveEntry(object key, UIComponent item)
        {
            this.entries.Remove(item);
            this.surface.Remove(item);

            /*if (keysByEntry.ContainsKey(item))//Do we ever have two of the same UI components in the grid? If so this could screw it up
            {
                keysByEntry.Remove(item);
            }*/
            if (key != null)
            {
                entriesByKey.Remove(key);
            }

            if (!FixedItemHeights)
            {
                // hook up the height! resize event so the grid can handle expands / resizes:
                // (avoid infinite recursion)
                item.HeightResize -= new ResizeHandler(item_Resize);
            }

            RefreshEntries();

        }


        public UIComponent AddEntry(object key, string entry, bool useLineBreaks = false, Color? entryColor = null, int? leftMargin = null) //string text1, string text2)
        {
            // NEW: Always use an item
            UIComponent item = new UIComponent(guiManager);

            Color? colorToUse = entryColor ?? color;

            if (useLineBreaks)
            {
                TextArea textArea = new TextArea(guiManager, type);
                textArea.HMargin = 0;
                textArea.VMargin = 0;
                textArea.CanGrowInHeight = true;
                textArea.Font = font; // fontFileName; // necessary to know the font size to break text correctly
                if (colorToUse.HasValue)
                {
                    textArea.Color = colorToUse.Value;
                }
                textArea.Text = entry;

                item.Add(textArea);
                //JustifyItemContent(abel);
            }
            else
            {
                Label label = new Label(guiManager);
                label.Text = entry;

                InitLabel(label);

                item.Add(label);
                JustifyItemContent(label);

                label.X = leftMargin ?? 0;

                if (colorToUse.HasValue)
                {
                    label.NormalColor = colorToUse.Value;
                }
            }

            AddEntry(key, item);

            return item;
        }

        void InitLabel(Label label)
        {
            label.Init(labelType);
            label.ID = DataControlID.Status;
        }

        public Hyperlink AddHyperLinkEntry(object key, string entry, uint entityID, int marginX = 0)
        {
            // NEW: Always use an item
            UIComponent item = new UIComponent(guiManager);
            return AddHyperLinkEntry(key, entry, entityID, item);
        }

        public Hyperlink AddHyperLinkEntry(object key, string entry, uint entityID, UIComponent newEntry, int marginX = 0) 
        {
            Hyperlink hyperlink = new Hyperlink(guiManager);
            hyperlink.Text = entry;
          
            hyperlink.TargetEntityID = entityID;
            hyperlink.RenderType = base.RenderType;
            newEntry.Add(hyperlink);
            JustifyItemContent(hyperlink);
            hyperlink.X = marginX;

            AddEntry(key, newEntry);

            return hyperlink;
        }

        public void AddEntry(object key, string entry1, int xPositionColumn2, string entry2)
        {
            AddEntry(key, null, entry1, xPositionColumn2, entry2);
        }

        public UIComponent AddEntryRightJustifyValue(object key, Rectangle? iconRect, int? iconXPos, int? leadingTextXPos, string entry1, int paddingRight, 
            string entry2, string valueTooltip = null, string captionTooltip = null)
        {
            //containingPanel.RightJustifyLabel();
            // int valuePosX = lblTitleSummary.X = Width - CollapsablePanel.headingSummaryRightPaddingDropDown - lblTitleSummary.TextWidth;

            UIComponent item = new UIComponent(guiManager);

            if (iconRect.HasValue)
            {
                Image icon = new Image(guiManager);
                //rect = The.GameEngine.GUISpriteSheet.SourceRectangle(itemTypeInCategory.IconSpriteName); //SpriteName);
                icon.SetSkinLocation(SkinState.Normal,iconRect.Value);
                icon.Position = new Point(0, 0);
                icon.ResizeControlToFitImage();
                item.Add(icon);

                if (iconXPos.HasValue)
                {
                    icon.X += iconXPos.Value;
                }
            }

            Label label = new Label(guiManager);
            label.Text = entry1;
            label.ToolTip = captionTooltip;
            label.Init(labelType);
            item.Add(label);
            if (leadingTextXPos.HasValue)
            {
                label.X += leadingTextXPos.Value;
            }
            JustifyItemContent(label);

            label = new Label(guiManager);
            label.Text = entry2;
            label.Init(labelType);
            label.ID = DataControlID.Value;


            int valuePosX = label.X = Width - paddingRight - label.TextWidth;

            label.X = valuePosX;
            label.Y += itemContentJustifyY;
            label.Name = "value";
            label.ToolTip = valueTooltip;

            item.Add(label);

            AddEntry(key, item);

            return item;
        }

        public void AddEntryWithCaptionTwoValuesAndButton(object key, int? leadingTextXPos, string caption, int paddingRight, string value1, string value2, ImageButtonType imageButtonType, ClickHandler clickHandler, EventArgs eventArgs)
        {
            UIComponent item = new UIComponent(guiManager);

            int xPos;
            int width;

            Label label = new Label(guiManager);
            label.Text = caption;
            label.Init(labelType);
            item.Add(label);
            if (leadingTextXPos.HasValue)
            {
                label.X += leadingTextXPos.Value;
            }
            JustifyItemContent(label);
            width = label.TextWidth;

            label = new Label(guiManager);
            label.Text = value1;
            label.Init(labelType);
            label.Name = "value1";
            item.Add(label);


            label.X = width + 6;

            xPos = label.X;

            /*  if (leadingTextXPos.HasValue)
              {
                  label.X += leadingTextXPos.Value;
              }*/
            //   JustifyItemContent(label);


            label = new Label(guiManager);
            label.Text = value2;
            label.Init(labelType);
            label.Name = "value2";
            item.Add(label);
            label.X = 100;

            //  JustifyItemContent(label);

            /*  Label valueLabel = new Label(guiManager);
              valueLabel.Text = entry2;
              valueLabel.Init(labelType);

              int valuePosX = //valueLabel.X = Width - paddingRight - valueLabel.TextWidth;

              valueLabel.X = valuePosX;
              valueLabel.Y += itemContentJustifyY;
              valueLabel.ID = "value";
            
              item.Add(valueLabel);
              */

            int buttonPosX = label.X + label.TextWidth + 4; //Width - paddingRight - label.TextWidth;

            ImageButton button = new ImageButton(guiManager);
            button.X = buttonPosX;
            button.Click += clickHandler;
            button.Name = "button";
            item.Add(button);
            button.EventArgs = eventArgs;
            button.Init(imageButtonType);

            AddEntry(key, item);
        }


        public void AddEntryAndButton(object key, int? leadingTextXPos, string entry1, int paddingRight, string entry2, ClickHandler clickHandler, EventArgs eventArgs)
        {
            UIComponent item = new UIComponent(guiManager);


            Label label = new Label(guiManager);
            label.Text = entry1 + " " + entry2;
            label.Init(labelType);
            label.Name = "captionAndValue";
            item.Add(label);
            if (leadingTextXPos.HasValue)
            {
                label.X += leadingTextXPos.Value;
            }
            JustifyItemContent(label);

            /*  Label valueLabel = new Label(guiManager);
              valueLabel.Text = entry2;
              valueLabel.Init(labelType);

              int valuePosX = //valueLabel.X = Width - paddingRight - valueLabel.TextWidth;

              valueLabel.X = valuePosX;
              valueLabel.Y += itemContentJustifyY;
              valueLabel.ID = "value";
            
              item.Add(valueLabel);
              */

            int buttonPosX = Width - paddingRight - label.TextWidth;

            ImageButton button = new ImageButton(guiManager);
            button.X = buttonPosX;
            button.Click += clickHandler; // new ClickHandler(button_Click);
            item.Add(button);
            button.EventArgs = eventArgs;
            button.Init(ImageButtonType.HUDArrowRight);

            AddEntry(key, item);
        }

        public void AddEntry(object key, int? xPosColumn1, string entry1, int xPositionColumn2, string entry2)
        {
            UIComponent item = new UIComponent(guiManager);

            Label label = new Label(guiManager);
            label.Text = entry1;
            label.Init(labelType);
            item.Add(label);
            if (xPosColumn1.HasValue)
            {
                label.X += xPosColumn1.Value;
            }
            JustifyItemContent(label);

            label = new Label(guiManager);
            label.Text = entry2;
            label.Init(labelType);
            label.X = xPositionColumn2;
            label.Y += itemContentJustifyY;
            label.Name = "value";


            item.Add(label);

            AddEntry(key, item);
        }

        private void JustifyItemContent(UIComponent content)
        {
            //  content.Y = content.Y + itemContentJustifyY;
            // content.X = content.X + itemContentJustifyX;

        }

        public void AddEntryWithIcon(object key, Rectangle iconRect, int margin1, string entry1, int margin2, string entry2)
        {
            UIComponent item = new UIComponent(guiManager);

            Image icon = new Image(guiManager);
            //rect = The.GameEngine.GUISpriteSheet.SourceRectangle(itemTypeInCategory.IconSpriteName); //SpriteName);
            icon.SetSkinLocation(SkinState.Normal,iconRect);
            icon.Position = new Point(0, 0);
            icon.ResizeControlToFitImage();
            item.Add(icon);

            Label label = new Label(guiManager);
            label.Text = entry1;
            label.Init(labelType);
            label.X = margin1;
            item.Add(label);
            label.Y += itemContentJustifyY;


            label = new Label(guiManager);
            label.Text = entry2;
            label.Init(labelType);
            label.X = margin2;
            label.Y += itemContentJustifyY;


            item.Add(label);

            AddEntry(key, item);
        }

        /// <summary>
        /// Adds an item, containing a number of column controls. It is best to add a grid row before adding its controls, then they will be sized correctly.
        /// Height gets set on the items here - If FixedItemHeights is True, else the item will be auto-sized by its contents.
        /// Width and Font is set here.
        /// Can also be a CollapsablePanel.
        /// </summary>
        /// <param name="text">Text of the new entry.</param>
        public void AddEntry(object key, UIComponent item, int? index = null) // UIComponent addAfterThis = null) 
        {
            // Very important: the item mustn't take focus.
            // Grid will handle clicks.
            if (selectability != SelectabilityOptions.None)
            {
                item.CanHaveFocus = false;
            }
            else
            {
                item.CanHaveFocus = this.CanHaveFocus; // NEW! inherit this setting
            }

            // let the grid handle mouse wheel events
            //item.CanReceiveMouseWheelEvents = false; //??

            InitNestedGrids(item);

            item.Tag1 = key; // identifies the row component

            // Add new entry
            if (index == null)
            {
                if (InsertNewRows == NewRowsInsertion.Last)
                {
                    this.entries.Add(item);
                }
                else
                {
                    this.entries.Insert(0, item);
                }

                this.surface.Add(item);
            }
            else
            {
                // entries.IndexOf(addAfterThis);
                this.entries.Insert(index.Value, item);

                this.surface.Insert(item, index.Value);
            }

            if (key != null)
            {
                this.entriesByKey.Add(key, item);
                //keysByEntry.Add(item,key);
            }

            Label label;
            Hyperlink hyperLink;


            item.RenderType = this.RenderType;

            // good, necessary??? done in RefreshEntries
            // item.Width = surface.Width;

            //   item.Height = this.font.LineSpacing;
            if (FixedItemHeights)
            {
                item.Height = ItemHeight;
            }


            int maxHeight = item.Height;

            // inherit fonts and color...
            foreach (UIComponent itemControl in item.Controls)
            {

                if (FixedItemHeights)
                {
                    itemControl.Height = ItemHeight;
                }
                else
                {
                    maxHeight = Math.Max(maxHeight, itemControl.Height);
                }

                label = itemControl as Label;
                if (label != null)
                {
                    if (font != null)
                    {
                        label.Font = font;
                    }

                    if (color != null)
                    {
                        label.NormalColor = color.Value;
                    }
                }
                else
                {
                    hyperLink = itemControl as Hyperlink;
                    if (hyperLink != null)
                    {
                        if (font != null)
                        {
                            hyperLink.Font = font;
                        }

                        if (color != null)
                        {
                            hyperLink.LabelColor = color.Value;
                        }
                    }

                }


            }

            if (!FixedItemHeights)
            {
                // size the row item by its contained controls:
                item.Height = maxHeight;

                // hook up the height! resize event so the grid can handle expands / resizes:
                // (avoid infinite recursion)
                item.HeightResize += new ResizeHandler(item_Resize);
            }

            RefreshEntries();

        }


        private static void InitNestedGrids(UIComponent item)
        {
            item.CanReceiveMouseWheelEvents = false; // grid entries should never receive this event.
            Grid grid = item as Grid;
            if (grid != null)
            {
                grid.IsOuterGrid = false; 
            }

            // check any children too:
            List<Grid> childGrids = null;
            item.FindChildOfType<Grid>(null, ref childGrids);
            if (childGrids != null)
            {
                foreach (var childGrid in childGrids)
                {
                    childGrid.CanReceiveMouseWheelEvents = false; // nested grids should not swallow the mouse wheel event
                    
                    childGrid.IsOuterGrid = false; // mark this property also.
                }
            }

            // now do the listboxes...
            List<ListBox> childListBoxes = null;
            item.FindChildOfType<ListBox>(null, ref childListBoxes);
            if (childListBoxes != null)
            {
                foreach (var childListBox in childListBoxes)
                {
                    childListBox.CanReceiveMouseWheelEvents = false; // nested grids should not swallow the mouse wheel event
                }
            }
        }

        /// <summary>
        /// removes items from a grid that do not match the predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TE"></typeparam>
        /// <param name="grid"></param>
        /// <param name="dictionary"></param>
        public void DeleteEntries<T>(Predicate<T> exists)
        {
            DeleteEntries<T>(this, exists);
        }

        public static void DeleteEntries<T>(IKeyedEntryComponent entryComponent, Predicate<T> exists)
        {
            T key;
            UIComponent item;

            for (int i = entryComponent.Entries.Count - 1; i >= 0; i--)
            {
                item = entryComponent.Entries[i];
                key = (T)item.Tag1;

                if (!exists(key))
                {
                    entryComponent.RemoveEntry(item.Tag1, item);
                }
            }
        }

        public static void DeleteEntriesWithMixedKeyTypes(IKeyedEntryComponent entryComponent, Predicate<object> exists)
        {
            object key;
            UIComponent item;

            for (int i = entryComponent.Entries.Count - 1; i >= 0; i--)
            {
                item = entryComponent.Entries[i];
                key = item.Tag1;

                if (!exists(key))
                {
                    entryComponent.RemoveEntry(item.Tag1, item);
                }
            }
        }

        public void DeleteEntriesWithNullableKey<T>(Predicate<T> exists) where T : class
        {
            T key;
            UIComponent item;

            for (int i = Entries.Count - 1; i >= 0; i--)
            {
                item = Entries[i];
                key = item.Tag1 as T;

                if (!exists(key))
                {
                    RemoveEntry(item.Tag1, item);
                }
            }
        }

        void item_Resize(UIComponent sender)
        {
            /*  UIComponent item;
              GetItem(sender, out item);

              item.Height = Math.Max(itemHeight, sender.Height);
              */

            // move all the entries so there is room:
            RefreshEntries();
        }

        public List<UIComponent> Entries
        {
            get { return entries; }
            set { entries = value; }
        }


        /// <summary>
        /// unsorted! iterate the Entries collection for correct sorting order. Use Tag to retireve each item's key.
        /// </summary>
        public Dictionary<object, UIComponent> EntriesByKey
        {
            get { return entriesByKey; }
        }


        public int GetIndex(UIComponent entry)
        {
            return Entries.IndexOf(entry);
        }

        public int GetIndexByKey(object key)
        {
            return Entries.IndexOf(entriesByKey[key]);
        }

        /// <summary>
        /// Clears all entries from the grid. Warning! This destroys user clicks!
        /// </summary>
        public void Clear()
        {
            foreach (UIComponent entry in this.entries)
                this.surface.Remove(entry);

            this.entries.Clear();
            this.entriesByKey.Clear();
            //keysByEntry.Clear();

            RefreshEntries();
        }

        public void Deselect()
        {


        }

        /// <summary>
        /// Retrieves the text from the current selection.
        /// </summary>
        /// <returns>Text from current selection.</returns>
        /*    public string GetSelectedText()
            {
                int index = SelectedIndex;

                if (index == -1)
                    return null;
                else
                    return this.entries[index].Text;
            }
            */
        /// <summary>
        /// Selects a specified item. Changes the item to the selection
        /// colour, and scrolls to show entry if necessary. 
        /// Deselects any items in child and sibling grids too!
        /// Also invokes
        /// SelectionChanged event.
        /// </summary>
        /// <param name="label">Label to select.</param>
        /// <param name="index">Index of selected item.</param>
        protected void Select(UIComponent item, int index)
        {
            SelectInternalNoEvent(item, index);
            if (SelectedChanged != null)
                SelectedChanged.Invoke(this);
        }

        public void PublicSetSelectionNoEvent(string itemString)
        {
            foreach (KeyValuePair<Object, UIComponent> pair in entriesByKey)
            {
                string key = pair.Key as string;
                if (key != null)
                {
                    if (key == itemString)
                    {
                        selectedItem = pair.Value;
                        SelectInternalNoEvent(pair.Value, 0);
                        return;
                    }
                }
            }

        }

        private void SelectInternalNoEvent(UIComponent item, int index)
        {
            if (index == -1)
            {
                if (this.selectedItem != null)
                    this.selectedItem.Remove(selectionBox);

                this.selectedItem = item;
                this.selectedIndex = index;

                List<Grid> childGrids = null; // new List<Grid>();
                this.FindChildOfType(this, ref childGrids);
                if (childGrids != null)
                {
                    foreach (Grid child in childGrids)
                    {
                        child.SelectedIndex = -1;
                    }
                }
            }
            else
            {
                if (item != this.selectedItem)
                {
                    // Deselect current item
                    if (this.selectedItem != null)
                        this.selectedItem.Remove(selectionBox);

                    // deselect items in sibling grids:
                    if (item != null)
                    {
                        DeselectItemsInSiblingGrids();
                    }

                    // Select new item
                    this.selectedItem = item;
                    this.selectedIndex = index;
                }

                // add the selectionBox always
                this.selectedItem.Insert(selectionBox, 0); // insert gray box at the bottom.
                selectionBox.Width = selectedItem.Width - hMargin; // !!!
                selectionBox.Height = selectedItem.Height;

                // Automatically scroll to selected item
                ScrollToItem(this.selectedItem);

                /*   if (-(this.selectedItem.Y + this.selectedItem.Height) <
                       (this.surface.Y - this.viewPort.Height)
                       ) // Scroll down
                   {
                       this.surface.Y = (this.viewPort.Height - (this.selectedItem.Y + this.selectedItem.Height));

                       this.scrollBar.Value = -this.surface.Y;

                       viewPort.Redraw();
                   }
                   else if (-this.selectedItem.Y > this.surface.Y) // Scroll up
                   {
                       this.surface.Y = -this.selectedItem.Y;
                       this.scrollBar.Value = -this.surface.Y;
                       this.viewPort.Redraw();
                   }*/
            }
        }

        private void DeselectItemsInSiblingGrids()
        {
            UIComponent outerGrid = FindParentOfType(typeof(Grid));
            if (outerGrid != null)
            {
                List<Grid> siblingGrids = null; // new List<Grid>();
                outerGrid.FindChildOfType(this, ref siblingGrids);
                if (siblingGrids != null)
                {
                    foreach (Grid sibling in siblingGrids)
                    {
                        sibling.SelectedIndex = -1;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the index of the supplied label.
        /// </summary>
        /// <param name="label">Search key.</param>
        /// <returns>Index of entry, or -1 if it was not found.</returns>
        /*   protected int FindIndex(Label label)
           {
               for (int i = 0; i < this.entries.Count; i++)
               {
                   if (this.entries[i] == label)
                       return i;
               }

               return -1;
           }*/

        /// <summary>
        /// Tries to get the grid item with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGetEntry(object key, out UIComponent item)
        {
            return entriesByKey.TryGetValue(key, out item);
        }

        /// <summary>
        /// Used by mouse up and down to check if an entry should be selected.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void CheckMouseSelect(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                // Go through each label and check if it should be selected
                int index = -1;
                foreach (UIComponent item in this.entries)
                {
                    index++;
                    if (item.CheckCoordinates(args.Position.X, args.Position.Y))
                    {
                        Select(item, index);
                        break;
                    }
                }
            }
        }


        public void CenterChildHorizontallyInViewport(UIComponent childControl)
        {
            childControl.X = (viewPort.Width - childControl.Width) / 2;
        }

        public void ScrollToIndex(int index) // UIComponent itemToScrollTo)
        {
            if (index != -1 && entries.Count > 0)
            {
                UIComponent item = entries[index];

                ScrollToItem(item);
            }
        }

        public void ScrollToItem(UIComponent itemToScrollTo)
        {
            // Automatically scroll to selected item
            if (-(itemToScrollTo.Y + itemToScrollTo.Height) <
                (this.surface.Y - this.viewPort.Height)
                ) // Scroll down
            {
                this.surface.Y = (this.viewPort.Height - (itemToScrollTo.Y + itemToScrollTo.Height));

                this.scrollBar.Value = -this.surface.Y;

                viewPort.Redraw();
            }
            else if (-itemToScrollTo.Y > this.surface.Y) // Scroll up
            {
                this.surface.Y = -itemToScrollTo.Y;
                this.scrollBar.Value = -this.surface.Y;
                this.viewPort.Redraw();
            }

        }

        public bool ScrollBarIsAtEnd()
        {
            return scrollBar.IsAtEnd();
        }

        public bool ScrollBarIsAtTop()
        {
            return scrollBar.IsAtTop();
        }

        #region Event Handlers
        /// <summary>
        /// Scrolls listbox view to the position supplied by the scrollbar.
        /// </summary>
        /// <param name="position">New scroll position.</param>
        protected void OnScroll(int position)
        {
            this.surface.Y = -position;
            this.viewPort.Redraw();
        }

        /// <summary>
        /// Up and down keys are used to move selection through the list.
        /// </summary>
        /// <param name="args">Key event arguments.</param>
        /*protected override void OnKeyDown(KeyEventArgs args)
        {
            base.OnKeyDown(args);

            if (this.selectedIndex != -1)
            {
                if (args.Key == Keys.Up)
                {
                    if (this.selectedIndex > 0)
                        Select(this.entries[this.selectedIndex - 1], this.selectedIndex - 1);
                }
                else if (args.Key == Keys.Down)
                {
                    if (this.selectedIndex < this.entries.Count - 1)
                        Select(this.entries[this.selectedIndex + 1], this.selectedIndex + 1);
                }
            }
        }*/

        /// <summary>
        /// Select the entry under the mouse if there is one. Used in regular
        /// list boxes.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseDown(MouseEventArgs args)
        {
            base.OnMouseDown(args);

            if (this.Selectability != SelectabilityOptions.None) // && !this.resizeToFit)
                CheckMouseSelect(args);
        }

        /// <summary>
        /// copied from ListBox
        /// </summary>
        /// <param name="wheelChange"></param>
        protected override void OnMouseWheelChanged(int wheelChange)
        {
            base.OnMouseWheelChanged(wheelChange);

            scrollBar.Value -= wheelChange;
        }

        /// <summary>
        /// Select the entry under the mouse if there is one. Used for combo
        /// boxes.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        /*   protected override void OnMouseUp(MouseEventArgs args)
           {
               base.OnMouseUp(args);

               if (this.resizeToFit)
                   CheckMouseSelect(args);
           }*/

        /// <summary>
        /// Resize child controls.
        /// </summary>
        /// <param name="sender">Resizing control.</param>
        protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

            /* if (type == ListBoxType.Default)
             {
                 this.box.Width = Width;
                 this.box.Height = Height;
             }*/

            if (scrollBarEnabled)
            {
                this.scrollBar.X = Width - this.scrollBar.Width; // -ViewPortAndScrollbarGap;

                this.scrollBar.Height = Height;
            }

            RefreshMargins();
            RefreshEntries();

            ResizeBackground();
        }

        /*
           if (propertyResult != null)
           {   // it seems wrong that the result is changed, it should only be the sort value that gets changed by this.
               if (presentationTypeCategory.SortingValueConversionMethod.HasValue == true)
               {
                   switch (presentationTypeCategory.SortingValueConversionMethod.Value)
                   {
                       case SortingValue.MiddleDistance:
                           {
                               propertyResult = PresentationTypeCategoryProcessor.GetMiddleDistanceForValue(propertyResult.Value);
                               break;
                           }
                   }
               }
           }
           */

       

      
        /* public bool TryGetEntry(object key, out UIComponent entry)
         {
             return entriesByKey.TryGetValue(key, out entry);
         }*/

        public bool CanProcessEntryData(string entryTerm, string entryIconName, float? normalizedValue)
        {
            if (entryTerm != null || normalizedValue != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

       /* public bool AddEntry(object key, string entryTerm, string entryTermTooltip, string entryIconName, Color? entryColor, int? orderingNumber, float? entryValue, bool clickable = false, uint? entityID = null, int? height = null)
        {
            UIComponent newEntry = null;

            if (CanProcessEntryData(entryTerm, entryIconName) == true)
            {
                
                if (clickable == false)
                {
                    newEntry = new Label(guiManager);
                    InitLabel((Label)newEntry);
                    AddEntry(key, newEntry, null);
                    UpdateEntry(newEntry, entryTerm, entryTermTooltip, entryIconName, entryColor, entryValue);
                }
                else
                {
                    newEntry = new UIComponent(guiManager);
                    AddHyperLinkEntry(key, entryTerm, entityID.Value, newEntry);
                    UpdateEntry(newEntry, entryTerm, entryTermTooltip, entryIconName, entryColor, entryValue);
                }

                newEntry.OrderByTag2 = orderingNumber ?? 0;
                return true;
            }
           
            return false;
        }*/

        /// <summary>
        /// from IKeyedEntryComponent
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entryTerm"></param>
        /// <param name="termTooltip"></param>
        /// <param name="entryIconName"></param>
        /// <param name="iconColor"></param>
        /// <param name="orderingNumber"></param>
        /// <param name="entryValue"></param>
        /// <param name="clickable"></param>
        /// <param name="entityID"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public UIComponent AddEntry(object key,
            string term, string caption,
           // string termTooltip, string captionTooltip,
            Action<UIComponent, bool> tooltipDisplayedCallback, string tooltipActivationProperty, bool? disableExpiry, int? tooltipWidth,
            string entryIconName, Color? iconColor, Color? termColor, 
            int? orderingNumber,
            float? entryValue,
            bool showBar = false, float? normalizedValue1 = null, float? normalizedValue2 = null, int? barWidth = null,
            bool clickable = false, uint? entityID = null,
            UIComponent customComponent = null, // Func<UIComponent> addCustomComponent = null,
            int? height = null,
            int? paddingLeft = null, 
            bool useIconBackground = false)
        {

            UIComponent newEntry = null;

            if (CanProcessEntryData(term ?? caption, entryIconName, normalizedValue1) == true)
            {
                newEntry = new UIComponent(guiManager);
                newEntry.OrderByTag2 = orderingNumber ?? 0;

                Hyperlink hyperlinkCaption = null;
                Label lblCaption = null;
                if (caption != null)
                {         
                 
                    if (clickable == false)
                    {
                        lblCaption = new Label(guiManager);
                        lblCaption.Init(labelType);
                        lblCaption.ID = DataControlID.Caption;
                        newEntry.Add(lblCaption);
                    }
                    else
                    {
                        hyperlinkCaption = new Hyperlink(guiManager);
                        hyperlinkCaption.Text = caption;
                        hyperlinkCaption.TargetEntityID = entityID;
                        hyperlinkCaption.RenderType = base.RenderType;
                        hyperlinkCaption.NormalColor = Label.LCDNormal;
                        newEntry.Add(hyperlinkCaption);
                        hyperlinkCaption.ID = DataControlID.Caption;
                    }
                }

                Icon circleIcon = null;
                if (useIconBackground && entryIconName != null)
                {
                    circleIcon = new Icon(GUIManager);
                    circleIcon.ID = DataControlID.StatusIconBackground;
                    Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_icon_circleBG");
                    circleIcon.SetSkinLocation(SkinState.Normal,rect);
                    circleIcon.ResizeControlToFitImage();

                    newEntry.Add(circleIcon);                
                }

                Icon icon = null;
                if (entryIconName != null)
                {
                    icon = new Icon(GUIManager);
                    icon.ID = DataControlID.StatusIcon;
                    newEntry.Add(icon);
                }                

                Label label = null;
                Hyperlink hyperlink = null;
                if (term != null || normalizedValue1 != null || showBar == true)
                {
                    if (showBar == true || normalizedValue1.HasValue)
                    {
                        FillableBar bar;
                        if (normalizedValue2 == null)
                        {
                            // add bar for normalized float:
                            bar = new FillableBar(GUIManager, FillableBar.FillableBarType.ProgressBar) { ShowMaxValueLabelAtEnd = false };
                        }
                        else
                        {
                            bar = new FillableBar(GUIManager, FillableBar.FillableBarType.LCDIndicator) { ShowMaxValueLabelAtEnd = false };
                        }

                        bar.ID = DataControlID.Status;
                        bar.Width = barWidth ?? 40; // 20; // 30;         
                        bar.MaxValue = 40;                       
                       // bar.Height = 12; 
                        newEntry.Add(bar);

                        SetTermTooltipSettings(tooltipActivationProperty, disableExpiry, tooltipWidth, entityID, tooltipDisplayedCallback, bar);
                    }
                    else if (customComponent != null)
                    {
                        newEntry.Add(customComponent); //addCustomComponent.Invoke());
                    }
                    else if (clickable == false || hyperlinkCaption != null)  // add label or hyperlink for the term
                    {
                        label = new Label(guiManager);
                        label.Init(labelType);
                        label.ID = DataControlID.Status;
                        newEntry.Add(label);
                        
                        SetTermTooltipSettings(tooltipActivationProperty, disableExpiry, tooltipWidth, entityID, tooltipDisplayedCallback, label);
                    }                                     
                    else
                    {                                             
                        hyperlink = new Hyperlink(guiManager);
                        hyperlink.Text = term;
                        hyperlink.TargetEntityID = entityID;
                        hyperlink.RenderType = base.RenderType;
                        newEntry.Add(hyperlink);        
                        hyperlink.ID = DataControlID.Status;
                    }
                }
                               
               
                AddEntry(key, newEntry, null);                

                if (height.HasValue)
                {
                    // use a fixed height
                    newEntry.Height = height.Value;
                }
                /*  otherwise size the entry item by its content (done automatically be the Grid) - whoops, no content has been set, that gets done in Update */


                if (circleIcon != null)
                {
                    newEntry.CenterChildVertically(circleIcon);
                }

            }

            return newEntry;

            //return false;
        }

        private static void SetTermTooltipSettings(string tooltipActivationProperty, bool? disableExpiry, int? tooltipWidth, uint? entityID, Action<UIComponent, bool> tooltipDisplayedCallback, UIComponent control)
        {
            control.TooltipDisplayChange += tooltipDisplayedCallback;

            if (disableExpiry.HasValue)
            {
                control.TooltipExpires = !disableExpiry.Value;
            }
            if (tooltipWidth.HasValue)
            {
                control.TooltipWidth = tooltipWidth.Value;
            }
            if (entityID.HasValue && tooltipActivationProperty != null)
            {
                control.Tag2 = new Tuple<uint, string>(entityID.Value, tooltipActivationProperty); // NEW: we need this for tooltip callbacks...
            }
        }

       

        public UIComponent AddGroup(object key, bool hasBorder, int? marginLeft = null)
        {                        
            Grid grdGroup;
            if (hasBorder)
            {
                grdGroup = new Grid(guiManager, this.type, this.labelType, "basic_dropdown_BG_tiny", 5, 0);
            }
            else
            {
                grdGroup = new Grid(guiManager, this.type, this.labelType);
            }

            grdGroup.FixedItemHeights = false;
            grdGroup.RenderType = RenderType.CRTAndLCD;
          //  innerPanel.AddContent(contentGrid);
            grdGroup.Font = GUIManager.LCDandHUDBodyFontPath;
          //  contentGrid.Width = 260;
            grdGroup.Width = Width - (marginLeft ?? 0);
            grdGroup.Height = 0; // 30; 
            grdGroup.X = marginLeft ?? 0;
          //  contentGrid.Y = 50;
         //   grdGroup.Y = -4;
            grdGroup.IsOuterGrid = false;
            grdGroup.CanGrowInHeight = true;
            grdGroup.ScrollBarEnabled = false;
            grdGroup.Selectability = Grid.SelectabilityOptions.None;
          //  grdGroup.HMargin = 4; // this displaces the grid entries to the right, but not the background image
            
            AddEntry(key, grdGroup);

            return grdGroup;
        }

        /// <summary>
        /// sets icon, caption and term
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="term"></param>
        /// <param name="caption"></param>
        /// <param name="termTooltip"></param>
        /// <param name="iconName"></param>
        /// <param name="iconColor"></param>
        /// <param name="entryValue"></param>
        /// <param name="paddingLeft"></param>
        public void UpdateEntry(UIComponent entry, string term, string caption, 
            string termTooltip, string captionTooltip, //Action<UIComponent> tooltipDisplayedCallback, // string showTooltipSetProperty,
            string iconName, Color? iconColor, Color? termColor,
            float? entryValue, float? normalizedValue1, float? normalizedValue2, 
            int? paddingLeft, int? paddingRight = null, bool showIconBackground = false, bool showFaded = false, bool rightAdjustTerm = false) //, float? valueToSortBy = null)
        {
            if (entryValue.HasValue)
            {
                entry.OrderByTag1 = entryValue.Value;
            }
            else
            {
                entry.OrderByTag1 = -1.0f;
            }


            // icon:
            UIComponent iconEntry = entry.FindChildById(DataControlID.StatusIcon);
            UIComponent iconBackground = entry.FindChildById(DataControlID.StatusIconBackground);
            if (iconEntry != null)
            {
                Icon icIcon = iconEntry as Icon;

                if (iconName != null)
                {
                    Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle(iconName);
                    icIcon.SetSkinLocation(SkinState.Normal,rect);
                    icIcon.ResizeControlToFitImage();
                    icIcon.Visible = true;
                   // icIcon.X = paddingLeft ?? 0;

                    icIcon.Color = iconColor;

                    if (showIconBackground && iconBackground != null)
                    {
                        iconBackground.Visible = true;

                      //  iconBackground.CenterChildHorizontally(icIcon);

                    }

                }
                else
                {
                    icIcon.Visible = false;

                    if (iconBackground != null)
                    {
                        iconBackground.Visible = false;
                    }
                }
            }


            // caption:
            UIComponent captionControl = entry.FindChildById(DataControlID.Caption); 
            if (captionControl != null)
            {              
              //  Label lblCaption = captionControl as Label;
              //  lblCaption.Text = caption;
              //  lblCaption.ToolTip = captionTooltip;        
                          
                Label lblTextEntry = captionControl as Label;
                if (lblTextEntry != null)
                {                    
                    lblTextEntry.Text = caption ?? "";                    
                }

                Hyperlink hyperLinkEntry = captionControl as Hyperlink;
                if (hyperLinkEntry != null)
                {
                    hyperLinkEntry.Text = caption;
                }
                captionControl.ToolTip = captionTooltip;                
            }
                       
            // term:
            UIComponent termComponent = entry.FindChildById(DataControlID.Status); 
            if (termComponent != null)
            {
                // term can be either a label or a link
                Label lblTextEntry = termComponent as Label;
                if (lblTextEntry != null)
                {
                    lblTextEntry.Text = term; 
                   
                    if (termColor.HasValue) //iconColor.HasValue)
                    {
                        lblTextEntry.NormalColor = termColor.Value; // iconColor.Value; 
                    }
                }

                Hyperlink hyperLinkEntry = termComponent as Hyperlink;
                if (hyperLinkEntry != null)
                {
                    hyperLinkEntry.Text = term;       
                }

                FillableBar bar = termComponent as FillableBar;
                if (bar != null)
                {
                    if (normalizedValue1 == null)
                    {
                        bar.Visible = false;
                       
                    }
                    else
                    {
                        bar.Visible = true;
                                                
                        // the max value is not related to the BarPresentation!
                        bar.Value = (int)(normalizedValue1 * bar.MaxValue);

                        if (normalizedValue2.HasValue)
                        {
                            // set slider value
                            bar.SliderValue = (int)(normalizedValue2 * bar.MaxValue);

                            bar.UpdateSliderPosition();
                        }

                        if (termColor.HasValue) // iconColor.HasValue)
                        {
                            bar.Color = termColor.Value; // iconColor.Value;
                            //bar.BarColor = entryColor.Value;
                        }
                    }
                }

                if (termTooltip != null && termTooltip.Contains("Well rested"))
                {

                }

                termComponent.ToolTip = termTooltip;
               // termComponent.TooltipDisplayed += tooltipDisplayedCallback;
               // termComponent.TooltipDisplayed = showTooltipSetProperty;
              
            }          
                       

            // rearrange horizontal only:
            // icon, caption, term
            int xPos = paddingLeft ?? 0; 
            if (iconEntry != null && iconEntry.Visible)
            {
                
                if (iconBackground != null && showIconBackground)
                {
                    iconBackground.X = xPos + 1; // -3;
                   // iconEntry.X = iconBackground.X;

                    iconBackground.CenterChildHorizontally(iconEntry);
                    iconEntry.X += iconBackground.X;

                    xPos = Math.Max(iconEntry.Right, iconBackground.Right);
                }
                else
                {
                    iconEntry.X = xPos;
                    xPos = iconEntry.Right;
                }
            }
         
            if (captionControl != null)
            {
                captionControl.X = xPos;
                xPos = captionControl.Right;


              /*  if (rightAdjustTerm)
                {
                    if (termComponent != null)
                    {
                        captionControl.MaxWidth = termComponent.X - captionControl.X - 4; // NEW
                    }
                    else
                    {
                        captionControl.MaxWidth = 200;
                    }
                }*/
              
            }            

            if (termComponent != null)
            {
                if (rightAdjustTerm)
                {
                    termComponent.AlignRight(SurfaceWidth - paddingRight ?? 0);

                    if (captionControl != null)
                    {
                        // NEW: truncate the caption:
                        captionControl.MaxWidth = termComponent.X - captionControl.X - 4;
                    }
                }
                else
                {
                    termComponent.X = xPos;
                }
            }




            // center children, now that content and height has been set:
            if (captionControl != null)
            {
                entry.CenterChildVertically(captionControl);
            }

            if (iconEntry != null)
            {
                entry.CenterChildVertically(iconEntry);
            }

        /*    if (circleIcon != null)
            {
                entry.CenterChildVertically(circleIcon);
            }*/

            if (termComponent != null)
            {
                entry.CenterChildVertically(termComponent);            
            }
        }

        public bool AddSeparator(object key, int? orderingNumber, float? entryValue)
        {
            Label separator = new Label(guiManager);
            AddEntry(key, separator, null);
            separator.OrderByTag2 = orderingNumber.Value;
            separator.OrderByTag1 = entryValue.Value;

            return true;
        }

        public bool AddSubHeader(string subHeaderName, int? orderingNumber, float? entryValue)
        {
            //the sub header name is used as both key and text
            Label subHeader = new Label(guiManager);
            subHeader.Text = subHeaderName;
            subHeader.OrderByTag2 = orderingNumber.Value;
            subHeader.OrderByTag1 = entryValue.Value;
            float dinner = (float)subHeader.OrderByTag1;

            InitLabel(subHeader);
            AddEntry(subHeaderName, subHeader, null);

            return true;
        }

        public object GetKeyFromIndex(int index)
        {
            object key;
            GetKey(entries[index], out key);

            return key;

            /*
            string stringKey = key as string;
            if (stringKey != null)
            {
                return stringKey;//if we have a key which is not a string then this will be null. Will this be a problem? Lars: Yes, it is unnecessary - object should be a key
            }
            else
            {
                return stringKey;
            }*/
        }

      /*  public object GetKeyFromIndex(int index)
        {
            object key;
            GetKey(entries[index], out key);

            return key;

        }*/

        /// <summary>
        /// removes from the end, to be used after sorting
        /// </summary>
        /// <param name="maxItems"></param>
        public bool CapNoOfEntries(int maxItems)
        {
            int itemsToRemove = Entries.Count - maxItems;

            bool wasCapped = false;

            while(itemsToRemove > 0)
            {
                RemoveEntry(entries[entries.Count - 1].Tag1);
                itemsToRemove--;

                wasCapped = true;
            }

            return wasCapped;
        }


        public void CapNoOfEntries()
        {
        }

        #endregion
    }
}