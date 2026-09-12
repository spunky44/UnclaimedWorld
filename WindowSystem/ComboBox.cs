#region File Description
//-----------------------------------------------------------------------------
// File:      ComboBox.cs
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using InputEventSystem;
using System.Collections.Generic;
#endregion

namespace WindowSystem
{

    public enum ComboBoxTypes { LCD, Default }

    /// <summary>
    /// A graphical combobox control.
    /// </summary>
    /// 
    /// <remarks>
    /// Contains a textbox and a listbox, 
    /// 
    /// OLD: as well as an Icon object, faking an
    /// ImageButton. The reason is that they share most of the same code, and
    /// without any mouse event handling, it's easier to predict state changes
    /// for when list box is open (has focus).
    /// </remarks>
    public class ComboBox : UIComponent
    {
        #region Default Properties
        private static int defaultWidth = 200;
        private static int defaultHeight = 20;
        private static Rectangle defaultButtonSkin = new Rectangle(138, 5, 20, 20);
        private static Rectangle defaultButtonHoverSkin = new Rectangle(159, 5, 20, 20);
        private static Rectangle defaultButtonPressedSkin = new Rectangle(180, 5, 20, 20);

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
        /// <value>Must be greater than 0.</value>
        public static int DefaultHeight
        {
            set
            {
                Debug.Assert(value > 0);
                defaultHeight = value;
            }
        }

        /// <summary>
        /// Sets the default skin of the ComboBox button.
        /// </summary>
        public static Rectangle DefaultButtonSkin
        {
            set { defaultButtonSkin = value; }
        }

        /// <summary>
        /// Sets the default hover skin of the ComboBox button.
        /// </summary>
        public static Rectangle DefaultButtonHoverSkin
        {
            set { defaultButtonHoverSkin = value; }
        }

        /// <summary>
        /// Sets the default pressed skin of the ComboBox button.
        /// </summary>
        public static Rectangle DefaultButtonPressedSkin
        {
            set { defaultButtonPressedSkin = value; }
        }
        #endregion

        #region Fields
        // used when in edit mode:
        private TextBox textBox;

        // used when in no-edit mode:
        TextButton headerbox;


      //  private Icon button;
        private ImageButton button;

      //  private Window listWindow;

        /// <summary>
        /// the listbox controls cannot receive clicks the normal way since they are not in the window control tree.
        /// The listbox is given focus manually, and a list item is selected from the mouse coords.
        /// 
        /// We need to make this float at the top, it cannot be clipped to LCD panels
        /// </summary>
        private ListBox listBox;
        private bool isListBoxOpen;

        private Dictionary<object, Label> entriesByKey;
        private object selectedKey;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current text.
        /// </summary>
     /*   public string SelectedText
        {
            get { return headerbox.Text; GetTextBoxOrButton(). this.textBox.Text; }
        }*/

        /// <summary>
        /// Get/Set the selected index.
        /// </summary>
        public int SelectedIndex
        {
            get { return listBox.SelectedIndex; }
            set
            {
                this.listBox.SelectedIndex = value;
                if (this.listBox.SelectedIndex == -1)
                {
                    if (this.textBox != null)
                    {
                        this.textBox.Text = "";
                    }
                    else if (this.headerbox != null)
                    {
                        this.headerbox.Text = "";
                    }

                    selectedKey = null;
                }
                else
                {
                    UpdateSelectedKey();
                }
            }
        }

        private void UpdateSelectedKey()
        {
            // not pretty, but...

            Label selectedLabel = listBox.SelectedText;
            foreach (var item in entriesByKey)
            {
                if (item.Value == selectedLabel)
                {
                    selectedKey = item.Key;
                    break;
                }
            }
        }


        public object SelectedKey
        {
            get
            {
                return selectedKey;

               /* Label selectedLabel = listBox.SelectedText;

                if (selectedLabel != null)
                {
                    return entriesByKey.Values
                }*/
            }
            set
            {
                selectedKey = value;
                Label label = entriesByKey[value];

                listBox.SelectedText = label;
            }
        }


        public Dictionary<object, Label> EntriesByKey
        {
            get { return entriesByKey; }
        }


       /* public int GetIndex(Label entry)
        {
            return Entries.IndexOf(entry);
        }

        public int GetIndexByKey(object key)
        {
            return Entries.IndexOf(entriesByKey[key]);
        }
        */


        /// <summary>
        /// Get/Set whether the text can be modified.
        /// </summary>
     /*   public bool IsEditable
        {
            get { return textBox.IsEditable; }
            set { this.textBox.IsEditable = value; }
        }*/

        /// <summary>
        /// Sets the skin of the ComboBox skin.
        /// </summary>
        public Rectangle ButtonSkin
        {
            set { this.button.SetSkinLocation(SkinState.Normal,value); }
        }

        /// <summary>
        /// Sets the hover skin of the ComboBox skin.
        /// </summary>
        public Rectangle ButtonHoverSkin
        {
            set { this.button.SetSkinLocation(1, value); }
        }

        /// <summary>
        /// Sets the pressed skin of the ComboBox skin.
        /// </summary>
        public Rectangle ButtonPressedSkin
        {
            set { this.button.SetSkinLocation(2, value); }
        }
        #endregion

        #region Events
        public event SelectionChangedHandler SelectionChanged;
        #endregion

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                if (base.Enabled != value)
                {
                    base.Enabled = value;

                    button.Enabled = value;

                    GetTextBoxOrButton().Enabled = value;

                    if (value == false)
                    {
                        GetTextBoxOrButton().DebugTag = "disabledcombo";
                    }
                    else
                    {
                        GetTextBoxOrButton().DebugTag = "";
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                return listBox.Count;
            }
        }

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public ComboBox(GUIManager guiManager, ListBoxType type, bool isEditable)
            : base(guiManager)
        {
            this.isListBoxOpen = false;
            entriesByKey = new Dictionary<object, Label>();

            //IsEditable = isEditable;

            #region Create Child Controls

            if (isEditable)
            {
                this.textBox = new TextBox(guiManager);
            }
            else
            {
                this.headerbox = new TextButton(guiManager);
                headerbox.DebugTag = "headerBox";
            }

          //  this.button = new Icon(guiManager);
            this.button = new ImageButton(guiManager);
            this.listBox = new ListBox(guiManager, type); 
            listBox.DebugTag = "combolist";
            listBox.SetDebugTagOnScrollbar("comboListScrollbar");
            
            GetTextBoxOrButton().Move += HeaderButton_Move;
            //listBox.Level = WindowSystem.Level.Middle; // TODO!!!


            #endregion

            #region Add Child Controls

            Add(GetTextBoxOrButton());
            
            Add(this.button);
            #endregion

            #region Set Properties

            // #COMBO
           /* this.listBox.CanGrowInHeight = false;
            this.listBox.ScrollBarEnabled = true;
            this.listBox.Height = 100; // should be a max height instead...
            this.listBox.GapAndScrollBar = 30;
            this.listBox.scrollBarXOffset = -15;

            this.listBox.DebugTag = "comboList";
            */

            this.listBox.CanGrowInHeight = true;
            this.listBox.ScrollBarEnabled = false;
             
            #endregion

            #region Set Default Properties
            this.Width = defaultWidth;
            this.Height = defaultHeight;
            ButtonSkin = defaultButtonSkin;
            ButtonHoverSkin = defaultButtonHoverSkin;
            ButtonPressedSkin = defaultButtonPressedSkin;
            #endregion

            #region Event Handlers
          /*  this.button.MouseOver += new MouseOverHandler(OnButtonMouseOver);
            this.button.MouseOut += new MouseOutHandler(OnButtonMouseOut);*/

          //  this.button.MouseDown += new MouseDownHandler(OnButtonMouseDown);
            this.button.Click +=new ClickHandler(headerbox_Click);
            this.button.LoseFocus += new LoseFocusHandler(OnButtonLoseFocus);

            //we need these to communicate with listbox in a decoupled manner. probably not oustide the class.
            this.listBox.SelectedChanged += new SelectionChangedHandler(OnSelectionChanged);
            this.listBox.SelectedSame += new SelectionChangedHandler(listBox_SelectedSame);
            this.listBox.LoseFocus += new LoseFocusHandler(OnListBoxLoseFocus);
            this.listBox.MouseSelected += listBox_MouseSelected;
            #endregion
        }

        void HeaderButton_Move(UIComponent sender)
        {
            // fires when the user scrolls the surface.
            // lets the listbox follow the scrolled button if shown
            if (isListBoxOpen)
            {
                SetListboxDimensions();
            }
        }

       

      
        #endregion

        void listBox_SelectedSame(UIComponent sender)
        {
          //  CloseListBox(true);
        }

        void listBox_MouseSelected(UIComponent obj)
        {
            CloseListBox(true);
        }

        private UIComponent GetTextBoxOrButton()
        {
            if (textBox != null)
            {
                return textBox;
            }

            if (headerbox != null)
            {
                return headerbox;
            }

            return null;
        }

        ComboBoxTypes type;

        public void Init(ComboBoxTypes type)
        {
            this.type = type;

            switch (type)
            {
                case ComboBoxTypes.LCD:
                    if (textBox != null)
                    {
                        textBox.Init(TextBox.TextBoxType.LCDCombo);

                        button.Init(ImageButtonType.LCDArrowDownNew);
                   
                        Height = textBox.Height;
                    }
                    else
                    {
                        headerbox.Init(TextButton.TextButtonType.LCDCombo);                     
                     //   this.headerbox.MouseDown += new MouseDownHandler(OnButtonMouseDown);
                        this.headerbox.Click += new ClickHandler(headerbox_Click);

                        this.headerbox.LoseFocus += new LoseFocusHandler(OnButtonLoseFocus);
                       
                        // no button...
                        Remove(button);

                        Height = headerbox.Height;
                    }

                    RenderType = WindowSystem.RenderType.CRTAndLCD;
               
                    listBox.Init(Label.LabelType.LCDComboBoxItem);

                  //  listBox.RenderType = WindowSystem.RenderType.Normal;
                    //listWindow.HasCRTOrLCDComponents = true;

                    break;

                case ComboBoxTypes.Default:
                    // Set Default Properties
                    this.Width = defaultWidth;
                    this.Height = defaultHeight;
                    ButtonSkin = defaultButtonSkin;
                    ButtonHoverSkin = defaultButtonHoverSkin;
                    ButtonPressedSkin = defaultButtonPressedSkin;         

                    break;

            }
        }

       

        /// <summary>
        /// Performs necessary cleanup operations when control is removed from
        /// a parent, or when it is no longer needed.
        /// </summary>
        /// <remarks>
        /// This method is necessary to ensure references to object are
        /// removed, allowing the object to be cleared by the garbage
        /// collector.
        /// 
        /// As such, all event handlers to outside objects such as
        /// InputEventSystem should be removed. All controls that have been
        /// added will automatically be taken care of, but any UIComponent
        /// controls that have not been added, should have their CleanUp()
        /// methods called from this method.
        /// </remarks>
        public override void CleanUp()
        {
            this.listBox.CleanUp();

            base.CleanUp();
        }


        public void BeginAddingEntries()
        {
            listBox.BeginAddingEntries();            
        }

        public void EndAddingEntries()
        {
            listBox.EndAddingEntries();            
        }



        /// <summary>
        /// Adds a string as an entry in the listbox.
        /// the string must be unique, otherwise an exception will be thrown... use the overload if you really want to add identical options.
        /// </summary>
        /// <param name="text">Text of the new entry.</param>
        public void AddEntry(string text)
        {
            // Add entry to child listbox
            Label label = this.listBox.AddEntry(text);

            entriesByKey.Add(text, label);
        }


        public void AddEntry(object key, string text)
        {
            // Add entry to child listbox
            Label label = this.listBox.AddEntry(text);

           // label.ToolTip = "TEST";

            entriesByKey.Add(key, label);
        }


      /*  public void FitToLongestEntry()
        {
            listBox.

            foreach (var item in entrie)
            {
                
            }

        }*/

      
        

        /// <summary>
        /// Clears all entries from listbox.
        /// </summary>
        public void Clear()
        {
            this.listBox.Clear();

            entriesByKey.Clear();

            selectedKey = null;

            SetText("");
        }

        /// <summary>
        /// Closes listbox.
        /// </summary>
        protected void CloseListBox(bool resetHeaderBox)
        {
            if (this.isListBoxOpen)
            {
                GUIManager.Remove(this.listBox);
                //listWindow.Hide();

                // Check if mouse is over button
              /*  if (this.button.CheckCoordinates(inputData.mouseX, inputData.mouseY))
                    this.button.CurrentSkinState = SkinState.Hover;
                else
                    this.button.CurrentSkinState = SkinState.Normal;
                */

                this.isListBoxOpen = false;

                if (resetHeaderBox && headerbox != null)
                {
                    headerbox.IsChecked = false;
                }
            }
        }

        #region Event Handlers

        /// <summary>
        /// Close listbox and update textbox, and invoke SelectionChanged
        /// event.
        /// </summary>
        /// <param name="sender">Selected control.</param>
        protected void OnSelectionChanged(UIComponent sender)
        {
            // Update text
            string text = this.listBox.GetSelectedText();
            SetText(text);

            UpdateSelectedKey();

            // gets called during programmatic access too. don't close the listbox unless the user is making changes:
          //  CloseListBox(true);

            if (SelectionChanged != null)
                SelectionChanged.Invoke(this);
        }

        public override string ToolTip
        {
            get
            {
                if (textBox != null)
                {
                    return textBox.ToolTip;
                }
                else if (headerbox != null)
                {
                    return headerbox.ToolTip;
                }
                else return null;
            }
            set
            {
                if (textBox != null)
                {
                    textBox.ToolTip = value;
                }
                else if (headerbox != null)
                {
                    headerbox.ToolTip = value;
                }
            }
        }

        private void SetText(string text)
        {
            if (text != null)
            {
                if (textBox != null)
                {
                    this.textBox.Text = text;
                }

                if (headerbox != null)
                {
                    headerbox.Text = text;
                }
            }
        }

       
        protected void OnButtonMouseOver(MouseEventArgs args)
        {
            if (!this.isListBoxOpen)
                this.button.CurrentSkinState = SkinState.Hover;
        }

        protected void OnButtonMouseOut(MouseEventArgs args)
        {
            if (!this.isListBoxOpen)
                this.button.CurrentSkinState = SkinState.Normal;
        }

        /// <summary>
        /// Open listbox when button is pressed.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
     /*   protected void OnButtonMouseDown(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                if (!this.isListBoxOpen && this.listBox.Count > 0)
                {
                    SetListboxDimensions();                

                    GUIManager.Add(this.listBox);
                    guiManager.SetFocus(listBox); // this makes the text button not receive the MouseUp, which causes problems with its skin...

                 
                    this.isListBoxOpen = true;
                }
                else
                    CloseListBox(false);
            }
        }*/

        void headerbox_Click(UIComponent sender, EventArgs e)
        {
            if (!this.isListBoxOpen && this.listBox.Count > 0)
            {
                SetListboxDimensions();

                // necessary to be on a level above this:
                listBox.Level = (Level)(this.Level + 1); //WindowSystem.Level.ComboBoxList; //WindowSystem.Level.Tooltip; // this.Level; // // this.Level;

                GUIManager.Add(this.listBox);

                // set focus to the listbox so it can show the highlight via OnMouseMove (this requires focus!)
                // the call would cancel a MouseUp event:
                guiManager.SetFocus(listBox); //<-- had to use Click instead of MouseDown, otherwise this call makes the text button (headerbox) not receive the following MouseUp, which again causes problems with its skin/pressed state...


                this.isListBoxOpen = true;
            }
            else
                CloseListBox(false);

        }

        private void SetListboxDimensions()
        {
            switch (type)
            {
                case ComboBoxTypes.LCD:
                    this.listBox.X = AbsolutePosition.X + 7;
                    this.listBox.Y = AbsolutePosition.Y + Height - 7;

                    this.listBox.Width = Width - 2 * 7;

                    break;
                default:
                    this.listBox.X = AbsolutePosition.X;
                    this.listBox.Y = AbsolutePosition.Y + Height - 1;

                    this.listBox.Width = Width;
                    break;
            }
        }



        /// <summary>
        /// Close listbox when it loses focus.
        /// </summary>
        protected void OnListBoxLoseFocus()
        {
            // only close if the box does not have focus - in that case OnMouseDown will be called right after, and that will close the list box.
            if (GUIManager.GetFocus() != this.headerbox)
                CloseListBox(true);
        }

        /// <summary>
        /// Close listbox when it loses focus.
        /// </summary>
        protected void OnButtonLoseFocus()
        {
            // Only close if not listbox
            if (GUIManager.GetFocus() != this.listBox)
                CloseListBox(true);
        }

        /// <summary>
        /// Resize child controls.
        /// </summary>
        /// <param name="sender">Resizing control.</param>
        protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

            if (type != ComboBoxTypes.LCD)
            {
                this.button.Width = Height;
                this.button.Height = Height;

                this.button.X = Width - this.button.Width;
                this.textBox.Width = Width - this.button.Width;
                this.textBox.Height = Height;
            }
            else
            {
                GetTextBoxOrButton().Width = Width;
                GetTextBoxOrButton().Height = Height;

            }          
        }
        #endregion
    }
}