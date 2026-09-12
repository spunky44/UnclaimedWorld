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
#endregion

namespace WindowSystem
{

    /// <summary>   
    /// Arranges components on a horizontal line, wraps around to the next line if MaxWidth is set
    /// </summary>
    public class HorizontalList : UIComponent, IKeyedEntryComponent
    {
        #region Default Properties
        private static int defaultWidth = 200;
        private static int defaultHeight = 150;
        private static int defaultHMargin = 5;
        private static int defaultVMargin = 0; // 2;
        private static string defaultFont = GUIManager.LCDandHUDBodyFontPath;
        private static Rectangle defaultSkin = new Rectangle(84, 41, 25, 25);

        private bool isAddingEntries = false;

        private int? MaxNumberOfEntries;
        //private bool SortEntries;

        public WindowSystem.Label.AnimationMode AnimateOnCRTScreen = WindowSystem.Label.AnimationMode.None;

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

        private UIComponent surface;
        private UIComponent viewPort;

        List<UIComponent> entries;
        public List<UIComponent> Entries
        {
            get { return entries; }
            set { entries = value; }
        }

        private Dictionary<object, UIComponent> entriesByKey;

        private SpriteFont font;
        private string fontFileName;

        private List<Icon> poolOfIcons;

        private int hMargin;
        private int vMargin;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of entries in the listbox.
        /// </summary>
        public int Count
        {
            get { return this.Entries.Count; }
        }

        /// <summary>
        /// Sets the font to use for listbox entries.
        /// </summary>
        /// <value>Must not be a valid path.</value>
        public string Font
        {
            set
            {
                this.fontFileName = value;
                this.font = GUIManager.ContentManager.Load<SpriteFont>(value);

                RefreshEntries();
            }
        }

        Color color;
        /// <summary>
        /// Get/Set the text colour.
        /// </summary>
        public Color Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                RefreshEntries();
            }
        }

        public bool CenterItemsVertically
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the horizontal padding.
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
        /// The vertical padding in the listbox.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public int VMargin
        {
            get { return this.vMargin; }
            set
            {
                Debug.Assert(value >= 0);
                this.vMargin = value;
                RefreshMargins();
            }
        }

        protected SpriteFont SpriteFont
        {
            get { return this.font; }
        }

        #endregion

       
        public override int? MaxHeight
        {
            get
            {
                return base.MaxHeight;
            }
            set
            {
                base.MaxHeight = value;

                this.surface.MaxHeight = value; // couple these properties...
            }
        }

        public override int MinHeight
        {
            get
            {
                return base.MinHeight;
            }
            set
            {
                base.MinHeight = value;

                this.surface.MinHeight = value; // couple these properties...
            }
        }


        #region Constructors
        ///<summary>
        ///Constructor.
        ///</summary>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public HorizontalList(GUIManager guiManager, int? maxNumberOfEntries = null)
            : base(guiManager)
        {

            MaxNumberOfEntries = maxNumberOfEntries;
            Entries = new List<UIComponent>();
            entriesByKey = new Dictionary<object, UIComponent>();

            poolOfIcons = new List<Icon>();

            #region Create Child Controls

            surface = new UIComponent(guiManager);
            //  this.surface.DebugTag = "ListSurface";
            viewPort = new UIComponent(guiManager);

            this.viewPort.Add(this.surface);
            Add(this.viewPort);

            #endregion

            #region Set Properties
            this.surface.CanHaveFocus = false;
            this.viewPort.CanHaveFocus = false;

            #endregion

            #region Set Default Properties
            Width = defaultWidth;
            Height = defaultHeight;
            HMargin = defaultHMargin;
            VMargin = defaultVMargin;

            #endregion

            surface.Height = 0;
        }

        #endregion

        /// <summary>
        /// Load default font.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
                Font = defaultFont;

            base.LoadGraphicsContent(loadAllContent);
        }

        /// <summary>
        /// Update controls to respect new margins.
        /// </summary>
        protected void RefreshMargins()
        {
            this.viewPort.X = 0;
            this.viewPort.Width = Width;

            this.viewPort.Y = this.vMargin;
            this.viewPort.Height = Height - (this.vMargin * 2);


            SetSurfaceWidth(viewPort, surface);

        }

        public static void SetSurfaceWidth(UIComponent viewPort, UIComponent surface)
        {
            /*if (type == ListBoxType.Main)
            {
                surface.Width = viewPort.Width - 28; //65; // make sure we cut off the contents before it reaches the silver edge area. Only adjust for the right side!
            }
            else if (type == ListBoxType.Comm)
            {
                surface.Width = viewPort.Width - 10; //20;
            }
            else*/
            {
                surface.Width = viewPort.Width;
            }
        }

        /// <summary>
        /// spacing between items
        /// </summary>
        public int HorizontalSpacing = 0;

        /// <summary>
        /// Refresh control to take new entries into account.      
        /// </summary>
        public void RefreshEntries()
        {
            int x = 0;
            int y = 0;

            int height = 0;
            int width = 0;

            int noOfRows = 1;

            bool isLastItem = false;
            int currentEntryNo = 0;
           
            foreach (UIComponent component in this.Entries)
            {               
                currentEntryNo++;
            

                if (currentEntryNo == Entries.Count)
                {
                    isLastItem = true;
                }

                Label label = component as Label;
                if (label != null)
                {
                    if (this.font != null)
                    {
                        label.Font = this.font;

                        label.Height = this.font.LineSpacing;
                    }
                    else
                    {
                        label.Height = 15; //??
                    }

                    label.NormalColor = this.color; // new

                    label.Width = this.surface.Width;
                }
                
                component.X = x;

                x = ComputeItemXPosition(x, component, !isLastItem);

             //   if (!isLastItem)
             //   {
                    // wrap-around:
                    if (MaxWidth.HasValue && x > MaxWidth) /* x + component.Width > MaxWidth)/*/
                    {
                        x = 0;
                        y += component.Height;

                        width = MaxWidth.Value;

                        noOfRows++;
                    }
           //     }               

                if (x > width)
                {
                    width = x;
                }

                // auto-resize height?
                if (component.Height > height)
                    height = component.Height;

              //  x = ComputeItemXPosition(x, component, currentEntryNo < Entries.Count);              
            }

            Width = width;
            height += y;
            surface.Height = height; // +(this.vMargin * 2);

            int newHeight = this.surface.Height + (this.vMargin * 2);
            if (newHeight != Height)
            {
                Height = newHeight;
            }

            x = 0;
            y = 0;
           
            int currentRow = 0;

            int rowHeight = Height / noOfRows;

            currentEntryNo = 0;
            isLastItem = false;

            // after we have the dimensions, we can position the entries vertically.
            foreach (UIComponent component in this.Entries)
            {
                currentEntryNo++;              

                if (currentEntryNo == Entries.Count)
                {
                    isLastItem = true;
                }

                if (CenterItemsVertically)
                {                   
                    component.CenterThisVertically((int)((currentRow + 0.5f) * rowHeight));
                }
                else
                {
                    component.Y = currentRow * rowHeight;
                }

                component.X = x;
                
                x = ComputeItemXPosition(x, component, !isLastItem);

              //  if (!isLastItem) // Oliver: I removed this because it wasnt wrapping the last item
              //  {    

                if (MaxWidth.HasValue && x > MaxWidth)
                {
                    x = 0;
                    currentRow++;

                    component.X = x;
                    component.Y = currentRow * component.Height;

                    x = component.X + component.Width;
                   // component.Bottom;
                    Height = component.Bottom;// this.entries[this.entries.Count - 1].Bottom;
                    this.surface.Height = component.Bottom;// this.entries[this.entries.Count - 1].Bottom;
                }              
              //  }                   
            }
                       
            this.surface.Redraw();
        }

        private int ComputeItemXPosition(int x, UIComponent component, bool addHorizontalSpacing)
        {
            int spacing = HorizontalSpacing;

            FillableBar bar = component as FillableBar;
            if (bar != null)
            {
                spacing += 4; // HACK: extra spacing...               
            }           

            x += component.Width;

            if (addHorizontalSpacing)
                x += spacing;

            return x;
        }

        public void BeginAddingEntries()
        {
            isAddingEntries = true;
        }

        public void EndAddingEntries()
        {
            isAddingEntries = false;
            RefreshEntries();
        }

       
        public void Sort(Grid.Sorting sortType, bool useFirstTag, Func<float, float> transformation = null)
        {
            if (useFirstTag == true)
            {
                if (transformation != null)
                {
                    Grid.Sort(i => transformation((float)i.OrderByTag1), sortType, ref entries, null);

                }
                else
                {
                    Grid.Sort(i => (float)i.OrderByTag1, sortType, ref entries, null);
                }
            }
            else
            {
                Grid.Sort(i => (int)i.OrderByTag2, sortType, ref entries, null); // SortOrder gets assigned to this property

            }
        }

        public void CapNoOfEntries()
        {
            if (MaxNumberOfEntries.HasValue)
            {
                int index;
                while (Entries.Count > MaxNumberOfEntries.Value)
                {
                    index = Entries.Count - 1;
                    TryRemoveEntry((string)Entries[index].Tag1);
                }
            }
        }

       
        /// <summary>
        /// Clears all entries from listbox.
        /// </summary>
        public void Clear()
        {
            // why not call RemoveEntry on all entries..?

            foreach (UIComponent entry in this.Entries)
            {
                surface.Remove(entry);

                Icon icon = entry as Icon;
                if (icon != null)
                {
                    ReturnEntryToPool(icon);
                }
            }

            Entries.Clear();
            entriesByKey.Clear();
            RefreshEntries();
        }

        /// <summary>
        /// Finds the index of the supplied UIComponent.
        /// </summary>
        /// <param name="label">Search key.</param>
        /// <returns>Index of entry, or -1 if it was not found.</returns>
        protected int FindIndex(UIComponent component)
        {
            for (int i = 0; i < this.Entries.Count; i++)
            {
                if (this.Entries[i] == component)
                {
                    return i;
                }
            }

            return -1;
        }

        #region Event Handlers



        /// <summary>
        /// Resize child controls.
        /// </summary>
        /// <param name="sender">Resizing control.</param>
        protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

            RefreshMargins();
            //RefreshEntries();
        }
        #endregion

        public void StartAddingData()
        {
            BeginAddingEntries();
        }

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
        /// removes items from a grid that do not match the predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TE"></typeparam>
        /// <param name="grid"></param>
        /// <param name="dictionary"></param>
        public void DeleteEntries<T>(Predicate<T> exists)
        {
            Grid.DeleteEntries<T>(this, exists);
        }

        public void RemoveEntry(object key)
        {
            UIComponent item = entriesByKey[key];

            RemoveEntry(key, item);
        }

        public void RemoveEntry(object key, UIComponent entry) // int entryIndex)
        {
            UIComponent entryToRemove = entriesByKey[key];
            surface.Remove(entryToRemove);
            entriesByKey.Remove(key);
            Entries.Remove(entry); // RemoveAt(entryIndex);

            Icon icon = entryToRemove as Icon;
            if (icon != null)
            {
                ReturnEntryToPool(icon);
            }
        }

        private void ReturnEntryToPool(Icon entry) // UIComponent entry)
        {
            poolOfIcons.Add(entry);//save the removed icon for later use
        }

        public bool TryGetEntry(object key, out UIComponent entry)
        {
            return entriesByKey.TryGetValue(key, out entry);
        }
        /// <summary>
        /// Adds a string as an entry in the listbox.
        /// </summary>
        /// <param name="text">Text of the new entry.</param>
        public void AddEntry(object key, UIComponent newEntry)
        {
          
            newEntry.CanHaveFocus = this.CanHaveFocus; // NEW: inherit focus ability. same as Grid.AddEntry

            Entries.Add(newEntry);
            surface.Add(newEntry);
            entriesByKey.Add(key, newEntry);
            newEntry.Tag1 = key;

            // newEntry.CanHaveFocus = false;
            //newEntry.DebugTag = "horizIcon";

            if (!isAddingEntries)
            {
                RefreshEntries();
            }
        }


        public UIComponent AddGroup(object key, bool hasBorder, int? marginLeft = null)
        {
            throw new Exception("Groups are not supported by horizontal list.");

            // return null;           
        }

        public bool CanProcessEntryData(string entryTerm, string entryIconName, float? normalizedValue)
        {
            return CanProcessEntryDataStatic(entryTerm, entryIconName, normalizedValue);

        }

        public static bool CanProcessEntryDataStatic(string entryTerm, string entryIconName, float? normalizedValue)
        {
            if (entryIconName == null && normalizedValue == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public UIComponent AddEntry(object key, string entryTerm, string caption, //string entryTermTooltip, string captionTooltip,
            Action<UIComponent, bool> tooltipDisplayedCallback, string tooltipActivationProperty, bool? disableExpiry, int? tooltipWidth,
            string entryIconName, Color? iconColor, Color? termColor, int? orderingNumber,
            float? entryValue,
            bool showBar = false, float? normalizedValue1 = null, float? normalizedValue2 = null, int? barWidth = null,
            bool clickable = false, uint? entityID = null,
            UIComponent cutsomComponent = null, // Func<UIComponent> createCustomControlCallback = null,
            int? height = null, int? paddingLeft = null, bool showIconBackground = false)
        {
            UIComponent newEntry = null;
            if (CanProcessEntryData(entryTerm, entryIconName, normalizedValue1) == false)
            {
                return null;
            }

            if (entryIconName != null)
            {
                if (poolOfIcons.Count > 0)
                {
                    newEntry = poolOfIcons[poolOfIcons.Count - 1];
                    poolOfIcons.RemoveAt(poolOfIcons.Count - 1);
                }
                else
                {
                    newEntry = new Icon(GUIManager);
                }
            }
            else if (showBar || normalizedValue1.HasValue)
            {
                // add bar for normalized float:
                newEntry = new FillableBar(GUIManager, FillableBar.FillableBarType.ProgressBar) { ShowMaxValueLabelAtEnd = false };
                newEntry.ID = DataControlID.HorizontalConnector;
                newEntry.Width = 20; // 30;               
                // newEntry.Y = 4;
                newEntry.Height = 12; // 16; // <- this makes it 12 pixels tall? // 12;
            }


            AddEntry(key, newEntry);
            newEntry.Tag2 = entityID; // NEW: we need this for tooltip callbacks (grid only?)

            if (orderingNumber.HasValue == true)
            {
                newEntry.OrderByTag2 = orderingNumber.Value;
            }
            else
            {
                newEntry.OrderByTag2 = -1;
            }

            //  UpdateEntry(newEntry, entryTerm, caption, entryTermTooltip, captionTooltip, entryIconName, entryColor, entryValue, normalizedValue);

            return newEntry;
        }

        public bool AddSeparator(object key, int? orderingNumber, float? entryValue)
        {
            return false;
        }

        public bool AddSubHeader(string subHeaderName, int? orderingNumber, float? entryValue)
        {
            return false;
        }


        public void UpdateEntry(UIComponent existingEntry, string entryTerm, string caption, string entryTermTooltip, string captionTooltip, //string tooltipActivationCallback,
            string entryIconName, Color? iconColor, Color? termColor, float? entryValue, float? normalizedValue1, float? normalizedValue2 = null, int? paddingLeft = null, int? paddingRight = null, bool showIconBackground = false, bool showFaded = false, bool rightAdjustTerm = false) //, float? valueToSortBy = null)
        {

            Icon iconEntry = existingEntry as Icon;
            if (iconEntry != null)
            {
                if (entryIconName == null)
                {
                    return; //??
                }

                Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle(entryIconName);
               // iconEntry.SetSkinLocation(SkinState.Normal,rect);

                iconEntry.SetSkinLocations(rect, Icon.UIType.HUD); // not always HUD...!

              //  iconEntry.SetSkinLocation(SkinState.Hover, rect, Color.Gray, Color.Gray);

                iconEntry.ResizeControlToFitImage();
                
                if (iconColor.HasValue)
                {
                    if (showFaded)
                    {
                        iconEntry.Color = Color.Gray;
                        
                    }
                    else
                    {
                        iconEntry.Color = iconColor.Value;
                        
                    }                    
                }
                else
                {
                    iconEntry.Color = null;
                }

                iconEntry.ToolTip = entryTermTooltip;
                if (entryTermTooltip == null || entryTermTooltip == "" || entryTermTooltip == "No tooltip available.")
                {
                    iconEntry.ToolTip = entryTerm;
                }              
            }

            FillableBar bar = existingEntry as FillableBar;
            if (bar != null)
            {
                if (normalizedValue1 == null)
                    return;

                bar.MaxValue = 20;
                bar.Value = (int)(normalizedValue1 * bar.MaxValue);
                bar.ToolTip = entryTermTooltip;
                if (iconColor.HasValue)
                {
                    bar.BarColor = iconColor.Value;
                }
                else if (termColor.HasValue) // optional..?
                {
                    bar.BarColor = termColor.Value;
                }

                if (showFaded)
                {
                    bar.Enabled = false;                                                    
                }
                else
                {
                    bar.Enabled = true;                 
                }
            }

            if (entryValue.HasValue)
            {
                existingEntry.OrderByTag1 = entryValue.Value;
            }

            if (surface.Height < existingEntry.Height)
            {
                surface.Height = existingEntry.Height;
            }

            return;
        }

        public object GetKeyFromIndex(int index)
        {
            return Entries[index].Tag1;
        }
    }
}