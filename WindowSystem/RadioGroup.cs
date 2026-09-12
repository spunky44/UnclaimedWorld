#region File Description
//-----------------------------------------------------------------------------
// File:      RadioGroup.cs
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
using System.Collections.Generic;
#endregion

namespace WindowSystem
{
    /// <summary>
    /// Contains multiple RadioButton controls, and handles the exclusively
    /// checked behavior. A radio button on it's own is simply a checkbox with
    /// a different default appearence.
    /// 
    /// DOES NOT HANDLE PROGRAMMATICAL CHANGES TO ISCHECKED! Call SelectMember to get deselect logic!
    /// 
    /// I have now made it possible to use this class as an abstract logic only group instead of as a parent control. Gives more flexibility.
    /// </summary>
    public class RadioGroup : UIComponent
    {
        #region Fields
        private bool firstButtonClicked;

        public int ButtonMargin = 0;

        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public RadioGroup(GUIManager guiManager)
            : base(guiManager)
        {
            this.firstButtonClicked = false;
            CanHaveFocus = false;
        }
        #endregion

        public event Action<ICanBeChecked, EventArgs> NewMemberChecked;

        public event Action<EventArgs> UnChecked;

        /// <summary>
        /// always filled
        /// </summary>
        private List<ICanBeChecked> abstractMembers = new List<ICanBeChecked>();

        /// <summary>
        /// Overridden to prevent any control except ICanBeChecked from being
        /// added.
        /// </summary>
        /// <param name="control">Control to add.</param>
        public override int Add(UIComponent control)
        {
            Debug.Assert(false);

            return 0;
        }

        /// <summary>
        /// Overloaded to prevent any control except ICanBeChecked from being
        /// added.
        /// 
        /// supply false to use as an abstract grouping object instead of a parent control.
        /// </summary>
        /// <param name="control">RadioButton to add.</param>
        public virtual void Add(ICanBeChecked control, bool addAsControl = true, bool setHorizPosition = false)
        {
            // Set event handler and add control
            control.Click += new ClickHandler(OnClick);

            if (addAsControl)
            {
                UIComponent uic = (UIComponent)control;
                base.Add(uic);

                if (setHorizPosition)
                {
                    uic.X = GetNextXPos();
                }
            }
           
            Util.AddToList(ref abstractMembers, control);
        
        }


        public int GetNextXPos()
        {
           // int lastRight = 0;
            if (abstractMembers.Count > 0)
            {
                return ((UIComponent)abstractMembers[abstractMembers.Count - 1]).Right + ButtonMargin;
            }

            return 0;
        }


        public void Clear()
        {
            base.Controls.Clear();
        }

        /// <summary>
        /// for programmatic access
        /// </summary>
        /// <param name="member"></param>
        public void SelectMember(ICanBeChecked member)
        {
            member.IsChecked = true;

            foreach (ICanBeChecked control in abstractMembers) 
            {
                if (control != member)
                {                   
                    control.IsChecked = false;
                }
            }
        }

        public RadioButton GetSelected()
        {
            /*if (Controls.Count > 0)
            {
                UIComponent radio = Controls.Find(u => ((RadioButton)u).IsChecked);
                if (radio != null)
                {
                    return (RadioButton)radio;
                }

                return null;

            }*/
            
            if (abstractMembers.Count > 0)
            {
                ICanBeChecked radio = abstractMembers.Find(u => u.IsChecked);
                if (radio != null)
                {
                    return (RadioButton)radio;
                }

                return null;
            }

            else return null;
        }

        #region Event Handlers

        /// <summary>
        /// When a child is checked, uncheck all other children.
        /// </summary>
        /// <param name="sender">Clicked RadioButton</param>
        protected void OnClick(UIComponent sender, EventArgs e)
        {
            ICanBeChecked canBeChecked = (ICanBeChecked)sender;

            bool previouslyNoneWasChecked = false;
           

            bool newSelectionWasMade = false;


            bool anyButtonIsChecked = false;

            // Uncheck every other checkbox
            foreach (ICanBeChecked control in abstractMembers) // Controls)
            {
                if (control != canBeChecked)
                {
                    if (control.IsChecked)
                    {
                        newSelectionWasMade = true;     
                    }

                    control.IsChecked = false;
                }
            }

            if (canBeChecked.IsChecked 
                && newSelectionWasMade == false 
                && canBeChecked.CheckedMode == CheckedModes.SwitchCheckedStateOnClick)
            {
                previouslyNoneWasChecked = true;
            }

            foreach (ICanBeChecked control in abstractMembers) 
            {
                if (control.IsChecked)
                {
                    anyButtonIsChecked = true;                    
                }
            }

            // Ensure that only one checkbox is checked at any one time.
            // Also ensure first click gets through! Lars: not sure why this code is here...
            if (this.firstButtonClicked && !canBeChecked.IsChecked)
            {
                // checkBox.IsChecked = true;
            }
            else
            {
                this.firstButtonClicked = true;
            }

            // if a new radiobutton has been checked, fire an event 
            // (clients cannot use OnClick, the radio group is not in a valid state since they may receive it before this class does)
            if (newSelectionWasMade || previouslyNoneWasChecked)
            {
                if (NewMemberChecked != null)
                {
                    NewMemberChecked.Invoke(canBeChecked, this.EventArgs); // e);
                }
            }

            if (!anyButtonIsChecked && UnChecked != null)
            {
                UnChecked.Invoke(this.EventArgs);
            }


        }
        #endregion
    }
}