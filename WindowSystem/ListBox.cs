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
    public interface IHasText
    {
        SpriteFont Font
        {
            set;            
            
        }

        Color NormalColor
        {
            set;

        }

        void Init(Label.LabelType labelType);

        RenderType RenderType { set; }

       /*  public virtual RenderType RenderType
        {
            get 
            {
                return renderType;
            }
            set 
            {*/
            
    }

    #region Delegates
    /// <summary>
    /// When a listbox selection changes.
    /// </summary>
    /// <param name="sender">Selected control.</param>
    public delegate void SelectionChangedHandler(UIComponent sender);
    #endregion

    public enum ListBoxType { Comm, Main, HUDAndLCD, LCD, LCDCombo}

    /// <summary>
    /// A graphical listbox control. Uses a scrollbar to allow more entries
    /// than can fit on control. Entries are actually children to a
    /// DrawableUIComponent, which acts as a viewport, only allowing some to
    /// be seen at one time. Scrollbar is only shown if required.
    /// 
    /// NOTE: The listbox does not maintain keys for its entries - only the raw text labels!
    /// If keys are needed, use a Grid or a ComboBox
    /// </summary>
    public class ListBox : UIComponent, IHasText
    {
        private ListBoxType type;

        protected Label.LabelType labelType;

        #region Default Properties
        private static int defaultWidth = 200;
        private static int defaultHeight = 150;
        private static int defaultHMargin = 0; // 5;
        private static int defaultVMargin = 2;
        //private static string defaultFont = GUIManager.LCDandHUDFontPath; //"Content/Fonts/DefaultFont";
        private static SpriteFont defaultFont = GUIManager.LCDandHUDFont;
        private static Rectangle defaultSkin = new Rectangle(84, 41, 25, 25);

        private bool isAddingEntries = false;
        public int scrollBarXOffset = 0;

        public WindowSystem.Label.AnimationMode AnimateOnCRTScreen = WindowSystem.Label.AnimationMode.None; // false;
        
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
      /*  public static string DefaultFont
        {
            set
            {
                Debug.Assert(value != null);
                Debug.Assert(value.Length > 0);
                defaultFont = value;
            }
        }*/

        /// <summary>
        /// Sets the default skin.
        /// </summary>
        public static Rectangle DefaultSkin
        {
            set { defaultSkin = value; }
        }
        #endregion

        #region Fields
       // private Box box;
        private UIComponent surface;
        private Box /*UIComponent*/ viewPort;
        private ScrollBar scrollBar;
        private List<Label> entries;
        private SpriteFont font;

        private Bar highlightBar;

        private Label selectedLabel;
        private int selectedIndex;
        private int hMargin;
        private int vMargin;

        private int entryPaddingHorizontal;
        private int entryPaddingVertical;

        /// <summary>
        /// how far out does the highlight extend from the label entry
        /// </summary>
        private int highlightPadding;

        private bool canGrowInHeight;

        private bool scrollBarEnabled = true;

     //   private bool fitWidthToEntries = false;

     
        #endregion

        #region Properties
        public ScrollBar ScrollBar
        {
            get { return scrollBar;  }
        }

        // TODO?
     /*   public bool FitWidthToEntries
        {
            set
            {
                if (fitWidthToEntries != value)
                {
                    fitWidthToEntries = value;

                    RefreshEntries();
                }
            }
        }*/
       

        /// <summary>
        /// grow, clamp or show a scrollbar when the entries overflow the height    
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
            }

        }


        

        /// <summary>
        /// Set this before setting the Height or Text properties
        /// grow, clamp or show a scrollbar when the entries overflow the height    
        /// </summary>
        public bool CanGrowInHeight
        {
            get
            {
                return this.canGrowInHeight;
            }
            set
            {
                if (canGrowInHeight != value)
                {

                    this.canGrowInHeight = value;

                    scrollBar.Visible = !this.canGrowInHeight;

                    SetGapForScrollbar();

                    RefreshMargins();
                }
            }
        }


        /// <summary>
        /// Gets the number of entries in the listbox.
        /// </summary>
        public int Count
        {
            get { return this.entries.Count; }
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

        public Label SelectedText
        {
            get 
            {
                if (selectedIndex == -1)
                {
                    return null;
                }
                else
                {
                    return entries[selectedIndex];
                }
            }
            set
            {
                Select(value, FindIndex(value));
              
            }
        }

      

        /// <summary>
        /// Sets the font to use for listbox entries.
        /// </summary>
        /// <value>Must not be a valid path.</value>
        public SpriteFont Font
        {
            set
            {
                this.font = value; // GUIManager.ContentManager.Load<SpriteFont>(value);
                this.scrollBar.ScrollStep = this.font.LineSpacing;
                RefreshEntries();
            }
        }
      /*  public string Font
        {
            set
            {               
                this.font = GUIManager.ContentManager.Load<SpriteFont>(value);
                this.scrollBar.ScrollStep = this.font.LineSpacing;
                RefreshEntries();
            }
        }*/

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

        public Color NormalColor
        {
            set
            {
                Color = value;
            }
        }

/*
        private Label.LabelType labelType = Label.LabelType.LCDNormal;
        public Label.LabelType LabelType
        {
            set
            {               
                RefreshEntries();
            }
        }*/

        public void Init(Label.LabelType type)
        {
            Rectangle rect;
            switch (this.type)
            {
                case ListBoxType.LCDCombo:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_dropdown_BG_small");
                  //  box.SetSkinLocation(SkinState.Normal,rect);
                    viewPort.SetSkinLocation(SkinState.Normal,rect);

                    entryPaddingHorizontal = 9;
                    entryPaddingVertical = 4;
                    highlightPadding = 6;

                    InitHoverBackground("basic_dropdown_highlight", 8);

                   

                    break;

            }


            if (this.labelType == type)
            {
                return;
            }

            this.labelType = type;

            Label.ApplyTextFormat(this, type);

            //FitToText();
            RefreshEntries();
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

        /// <summary>
        /// Sets the control skin.
        /// </summary>
      /*  public Rectangle Skin
        {
            set { 
                this.box.SetSkinLocation(SkinState.Normal,value); }
        }*/

        protected SpriteFont SpriteFont
        {
            get { return this.font; }
        }

        public const int GapAndScrollbarMain = 25;
        public const int GapAndScrollbarComm = 13; //5; //20;
        public const int GapAndScrollbarLCDHUD = 13; //5; //20;

        public int GapAndScrollBar;

        #endregion

        #region Events - used to communicate with ComboBox which is not a subclass
        public event SelectionChangedHandler SelectedChanged;
        public event SelectionChangedHandler SelectedSame;

        public event Action<UIComponent> MouseSelected;
        #endregion

       

        #region Constructors

       
         
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public ListBox(GUIManager guiManager, ListBoxType type)
            : base(guiManager)
        {
            this.type = type;

            this.entries = new List<Label>();
            this.selectedIndex = -1;
            this.canGrowInHeight = false;

            this.CanReceiveMouseWheelEvents = true;

            #region Create Child Controls
          

            this.surface = new UIComponent(guiManager);
         
         //   this.viewPort = new UIComponent(guiManager);
            this.viewPort = new Box(guiManager);


            SetGapForScrollbar();

            if (type == ListBoxType.Comm)
            {
               
                this.scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.CommRoller);           
            }
            else if (type == ListBoxType.Main)
            {               
                this.scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.MainRoller);           
            }
            else if (type == ListBoxType.HUDAndLCD)
            {
                this.scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.HUD);
            }
            else if (type == ListBoxType.LCD)
            {
                this.scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.LCD);
            }
           /* else if (type == ListBoxType.LCDCombo)
            {
                // #COMBO
                this.scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.LCD);
            }*/
            else
            {
                this.scrollBar = new ScrollBar(guiManager);
            }

            // make viewport a SkinnedComponent..
          //  box = new Box(guiManager);
         //   this.viewPort.Add(this.box);

            this.viewPort.Add(this.surface);
            Add(this.viewPort);
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

         
          /*  if (type == ListBoxType.Default)
            {
                Skin = defaultSkin; // white background...
            }*/

            #endregion

            #region Event Handlers
            
            this.scrollBar.Scroll += new ScrollHandler(OnScroll);
            // Scrollbar doesn't need keyboard, so hand control over to this
            //this.scrollBar.KeyDown += new KeyDownHandler(OnKeyDown);


            #endregion
        }
        #endregion

        public void SetDebugTagOnScrollbar(string tag)
        {
            scrollBar.DebugTag = tag;
        }

        private void SetGapForScrollbar()
        {
            if (scrollBarEnabled == false || canGrowInHeight == true)
            {
                GapAndScrollBar = 0;
            }
            else
            {
                if (type == ListBoxType.Comm)
                {
                    GapAndScrollBar = GapAndScrollbarComm;
                }
                else if (type == ListBoxType.Main)
                {
                    GapAndScrollBar = GapAndScrollbarMain;
                }
                else if (type == ListBoxType.LCD || type == ListBoxType.HUDAndLCD) // NEW
                {
                    GapAndScrollBar = GapAndScrollbarLCDHUD;
                }
                else
                {
                    GapAndScrollBar = 0;
                }
            }
        }

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
            if (loadAllContent)
                Font = defaultFont;

            base.LoadGraphicsContent(loadAllContent);
        }

        /// <summary>
        /// Update controls to respect new margins.
        /// </summary>
        protected void RefreshMargins()
        {           
            this.viewPort.X = this.hMargin;
            
          //  this.viewPort.X = 0;

            if (scrollBarEnabled)
            {
              //  this.viewPort.Width = Width - GapAndScrollBar;
                this.viewPort.Width = Width - (this.hMargin * 2) - GapAndScrollBar;
           
                SetSurfaceWidthForScrollbar(type, viewPort, surface); // this will cut off some of the right side area when not HUD type, hmmm...
            }
            else
            {
              //  this.viewPort.Width = Width;
                this.viewPort.Width = Width - (this.hMargin * 2);
           
                surface.Width = viewPort.Width;                
            }

            SetHighlightWidth();

            this.viewPort.Y = this.vMargin;       
            this.viewPort.Height = Height - (this.vMargin * 2);

      //      this.box.Width = viewPort.Width;
      //      this.box.Height = viewPort.Height;
           
       
            if (scrollBarEnabled)
            {
                this.scrollBar.Viewable = this.viewPort.Height;
            }
        }


        public void FitToLongestEntry()
        {
           // int width = 0;
            foreach (var item in entries)
            {
              //  if (item.TextWidth > width)
               //     width = item.TextWidth;
            }
           // this.Width = width;
        }
        
       /* private void SetSurfaceWidthAndMargin()
        {
            switch (type)
            {               
                case ListBoxType.LCDCombo:
                    surface.Width = viewPort.Width - 2 * lcdComboPadding;
                    surface.X = lcdComboPadding;
                    break;
                default:
                    surface.Width = viewPort.Width;
                    break;

            }

        }*/

        public static void SetSurfaceWidthForScrollbar(ListBoxType type, UIComponent viewPort, UIComponent surface)
        {
            switch (type)
            {
                case ListBoxType.Main:
                     surface.Width = viewPort.Width - 28; //65; // make sure we cut off the contents before it reaches the silver edge area. Only adjust for the right side!
                    break;
                case ListBoxType.Comm:
                    surface.Width = viewPort.Width - 10; //20;
                    break;            
             /*   case ListBoxType.LCD:
                    surface.Width = viewPort.Width - 10;
                    break;*/
                default:
                    surface.Width = viewPort.Width;
                    break;

            }
           
        }

        /// <summary>
        /// Refresh control to take new entries into account.
        /// </summary>
        public void RefreshEntries()
        {
            int y = entryPaddingVertical; //0;

            foreach (Label label in this.entries)
            {
                label.Y = y;
                label.X = entryPaddingHorizontal;

               // label.Init(labelType);


                if (this.font != null)
                {
                    label.Font = this.font;

                    label.Height = this.font.LineSpacing;
                }
                else
                {
                    label.Height = 15;
                }

                label.NormalColor = this.color; // NEW
              //  label.Color = this.color; // OLD

                y += label.Height;

                    
                label.Width = this.surface.Width - 2 * entryPaddingHorizontal;
               
            }

            this.surface.Height = y + entryPaddingVertical;

           

            if (scrollBarEnabled)
            {
                this.scrollBar.MaximumValue = this.surface.Height;
            }

            if (this.canGrowInHeight)
            {
                int newHeight = this.surface.Height + (this.vMargin * 2);
                if (newHeight != Height)
                    Height = newHeight;
            }
            else if (y > this.viewPort.Height)
            {
              
                if (scrollBarEnabled)
                {  
                    // Always show groove:
                    scrollBar.ShowKnob = true;
                               
                    scrollBar.Visible = true;
                }
            }
            else
            {
                if (scrollBarEnabled)
                {
                    // Always show groove:
                    scrollBar.ShowKnob = false;

                    scrollBar.Visible = false;
                }

            }

            this.surface.Redraw();
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


        /// <summary>
        /// Adds a string as an entry in the listbox.
        /// </summary>
        /// <param name="text">Text of the new entry.</param>
        public Label AddEntry(string text)
        {
            // Create a new label
            Label newEntry = new Label(GUIManager);
            if (AnimateOnCRTScreen == Label.AnimationMode.Line)
            {
                newEntry.SetLineAnimationMode(entries.Count);
            }
            else
            {
                newEntry.AnimateOnCRTScreen = AnimateOnCRTScreen;
            }

            newEntry.Init(labelType);

            newEntry.Text = text;

            newEntry.DebugTag = "comboEntry";
            
            // Add new entry
            this.entries.Add(newEntry);
            this.surface.Add(newEntry);

            if (!isAddingEntries)
            {
                RefreshEntries();
            }

            return newEntry;
        }

        /// <summary>
        /// Clears all entries from listbox.
        /// </summary>
        public void Clear()
        {
            foreach (Label entry in this.entries)
                this.surface.Remove(entry);
            this.entries.Clear();
            RefreshEntries();
        }

        /// <summary>
        /// Retrieves the text from the current selection.
        /// </summary>
        /// <returns>Text from current selection.</returns>
        public string GetSelectedText()
        {
            int index = SelectedIndex;

            if (index == -1)
                return null;
            else
                return this.entries[index].Text;
        }

        /// <summary>
        /// Selects a specified label. Changes the label to the selection
        /// colour, and scrolls to show entry if necessary Also invokes
        /// SelectionChanged event.
        /// </summary>
        /// <param name="label">Label to select.</param>
        /// <param name="index">Index of selected item.</param>
        protected void Select(Label label, int index)
        {
            if (index == -1)
            {
                if (this.selectedLabel != null)
                {
                    this.selectedLabel.Color = Color.Black;

                    this.selectedLabel = null; // NEW - deselect
                }

                this.selectedIndex = index;
            }
            else
            {
                if (label != this.selectedLabel)
                {
                    // Deselect current item
                    if (this.selectedLabel != null)
                        this.selectedLabel.Color = Color.Black;
                    // Select new item
                    this.selectedLabel = label;
                    this.selectedLabel.Color = Color.Gray;// Color.Blue;
                    this.selectedIndex = index;


                    if (SelectedChanged != null)
                        SelectedChanged.Invoke(this);
                }
                else
                {
                    if (SelectedSame != null)
                        SelectedSame.Invoke(this);
                }

                // Automatically scroll to selected item
                if (-(this.selectedLabel.Y + this.selectedLabel.Height) <
                    (this.surface.Y - this.viewPort.Height)
                    ) // Scroll down
                {
                    this.surface.Y = (this.viewPort.Height -
                        (this.selectedLabel.Y + this.selectedLabel.Height));
                    this.scrollBar.Value = -this.surface.Y;
                    viewPort.Redraw();
                }
                else if (-this.selectedLabel.Y > this.surface.Y) // Scroll up
                {
                    this.surface.Y = -this.selectedLabel.Y;
                    this.scrollBar.Value = -this.surface.Y;
                    this.viewPort.Redraw();
                }
            }
        }

        /// <summary>
        /// Finds the index of the supplied label.
        /// </summary>
        /// <param name="label">Search key.</param>
        /// <returns>Index of entry, or -1 if it was not found.</returns>
        protected int FindIndex(Label label)
        {
            for (int i = 0; i < this.entries.Count; i++)
            {
                if (this.entries[i] == label)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Used by mouse up and down to check if an entry should be selected.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void CheckMouseSelect(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                /*
                if (scrollBar.CheckCoordinates(args.Position.X, args.Position.Y))
                {
                    UIComponent result;
                    float zTemp = 0.0f;

                    // Go through each child asking them if they can have focus
                    foreach (UIComponent control in scrollBar.Controls)
                    {
                        // Keep track of highest z-order
                        if (control.ZOrder >= zTemp)
                        {
                            UIComponent child = control.CheckFocus(args.Position.X, args.Position.Y, TestMode.Focus);
                            if (child != null)
                            {

                                // Set result and update highest z-order
                                result = child;
                                zTemp = control.ZOrder;
                            }
                        }

                    }

                    if (result != null)
                    {
                        result.
                    }
                }*/


                Label mousedEntry;
                int labelIndex;
                if (GetMousedEntry(args, out labelIndex, out mousedEntry))
                {
                    Select(mousedEntry, labelIndex);

                    if (MouseSelected != null)
                        MouseSelected.Invoke(this);
                }

                // Go through each label and check if it should be selected
             /*   int index = -1;
                foreach (Label label in this.entries)
                {
                    index++;
                    if (label.CheckCoordinates(args.Position.X, args.Position.Y))
                    {
                        Select(label, index);
                        break;
                    }
                }*/
            }
        }


      
        protected bool GetMousedEntry(MouseEventArgs args, out int index, out Label mousedLabel)
        {                
            // Go through each label and check if it should be selected
            mousedLabel = null;
            index = -1;
            foreach (Label label in this.entries)
            {
                index++;
                if (label.CheckCoordinates(args.Position.X, args.Position.Y))
                {
                    mousedLabel = label;
                    return true;

                }
            }

            return false;
        }

      

        private void InitHoverBackground(string backgroundSprite, int spriteEdgeSize)
        {
            highlightBar = new Bar(guiManager);
            highlightBar.EdgeSize = spriteEdgeSize;
            Rectangle rect = GUIManager.GUISpriteSheet.GetSourceRectangle(backgroundSprite);
            highlightBar.SetSkinLocation(SkinState.Normal, rect);
            highlightBar.Height = rect.Height;

            SetHighlightWidth();

            highlightBar.X = entryPaddingHorizontal - highlightPadding;
        }

        private void SetHighlightWidth()
        {
            if (highlightBar != null)
            {
                highlightBar.Width = surface.Width - 2 * entryPaddingHorizontal + 2 * highlightPadding;
            }
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


     /*   protected override void OnMouseOver(MouseEventArgs args)
        {
            base.OnMouseOver(args);

            //the hover effect is set here, and not in each label, because setting CanHaveFocus on the label seems to make the mouse down event not reach the list box...
            if (highlightBar != null)
            {
                Label mousedEntry;
                int labelIndex;

                if (GetMousedEntry(args, out labelIndex, out mousedEntry))
                {
                    surface.Remove(highlightBar);

                    surface.Add(highlightBar);

                    highlightBar.Y = mousedEntry.Y;
                }
            }

        }*/

        protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
        {
            base.OnMouseOut(sender, args);

            if (highlightBar != null)
            {
                surface.Remove(highlightBar);
            }
        }

        /// <summary>
        /// we will only receive this event if the listbox has been given focus explicitly, since clicking the list box closes it...
        /// </summary>
        /// <param name="args"></param>
        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);

            if (highlightBar != null)
            {
                Label mousedEntry;
                int labelIndex;

                if (GetMousedEntry(args, out labelIndex, out mousedEntry))
                {
                    // show highligt:
                    ShowHighlight(mousedEntry);
                }
            }
        }


        protected override void OnMouseWheelChanged(int wheelChange)
        {
            base.OnMouseWheelChanged(wheelChange);

            scrollBar.Value -= wheelChange;
        }

        private void ShowHighlight(Label mousedEntry)
        {
            surface.Remove(highlightBar);
            surface.Remove(mousedEntry);

            surface.Add(highlightBar);
            surface.Add(mousedEntry);

            switch (type)
            {
                case ListBoxType.LCDCombo:
                    highlightBar.Y = mousedEntry.Y - 4;
                  //  highlightBar.X = mousedEntry.X - 10;

                    break;

                default:
                    highlightBar.Y = mousedEntry.Y;
                    break;
            }
            
        }

        /// <summary>
        /// Select the entry under the mouse if there is one. Used in regular
        /// list boxes.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseDown(MouseEventArgs args)
        {
            base.OnMouseDown(args);

            if (!this.canGrowInHeight)
                CheckMouseSelect(args);
        }

        /// <summary>
        /// Select the entry under the mouse if there is one. Used for combo
        /// boxes.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseUp(MouseEventArgs args)
        {
            base.OnMouseUp(args);

            if (this.canGrowInHeight)
                CheckMouseSelect(args);
        }

        public void RemoveEntry(Label item)
        {
            this.entries.Remove(item);            
        }

        /// <summary>
        /// Resize child controls.
        /// </summary>
        /// <param name="sender">Resizing control.</param>
        protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

          /*  if (type == ListBoxType.Default)
            {
                this.box.Width = Width;
                this.box.Height = Height;
            }*/

            if (scrollBarEnabled)
            {
                this.scrollBar.X = Width - this.scrollBar.Width + scrollBarXOffset;

                this.scrollBar.Height = Height;
            }

            RefreshMargins();
            RefreshEntries();
        }
        #endregion
    }
}