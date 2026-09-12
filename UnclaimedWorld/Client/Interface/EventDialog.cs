using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide;

namespace UWGame.ClientSide.Interface
{

  /*  public class DialogButton
    {
        public string Text;
        public string Tooltip;

        public int? Width;
    }*/

    /// <summary>
    /// a modal LCD dialog with text and an image
    /// </summary>
    public class EventDialog: Panel
    {
       
       // Label lblHeader; 
        
        // in LCD screen:
        TextArea area;

        // in CRT screen:
        Image image;

        const int width = 786;
        const int height = 308; // 260; 

        Box display, /*edges,*/ crtPlasticEdge;
        LCDScreen lcdScreen;
        UIComponent lcdSurface;

        CRTScreen crtScreen;

        Grid surfaceGrid;

        const int titleHeight = 30;


        /// <summary>
        /// this event will not work from the archive, or via next/previous buttons.
        /// </summary>
        public event Action<string, int> ButtonClicked;

        /// <summary>
        /// contains the script actions that should fire
        /// </summary>
        DialogOption[] dialogOptions;


        const int maxNoOfButtons = 4;

      
        /// <summary>
        /// up to 4 buttons are available. Per default, button 0 is OK, button 1 is cancel
        /// </summary>
        private TextButton[] buttons = new TextButton[maxNoOfButtons];

      
     //   TextButton btClose;

      
       /* private ImageButton previousButton;
        private ImageButton nextButton;*/

        private List<TextButton> buttonsOnForm = new List<TextButton>();

       
        /// <summary>
        /// OLD:
        /// The dialog has 3 modes. The mode is selected when the dialog is first displayed and does NOT change when next/prev buttons are used.
        ///      
        /// 
        /// We stay in NewEvent mode when next/previous buttons are clicked, so the close button does not appear in this situation.
        /// </summary>
        public enum Mode { NewEvent, /*EventArchive,*/ OtherDialog }

        /// <summary>
        /// the mode is the same even when pressing next and previous!
        /// This is so the close button does not appear when pressing previous on a new event. We don't want to give the user the ability to dismiss the event dialog.
        /// </summary>
        private Mode mode = Mode.NewEvent;


        public string Text
        {
            set
            {
                area.Text = value;
            }
        }

       

        public string Image
        {
            set
            {
                image.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle(value));
                image.ResizeControlToFitImage();
            }
        }

        
        public EventDialog(CommonInterface intf):
            base(intf, null, Point.Zero, new Vector2(width, height), Level.EventDialog, PanelType.EventDialog)
        {
                    
            FullLCDPanel.AddLCDPanel(intf, Window, new Point(14, 46), 465 /* 477 - 2 * lcdMargin*/, 215 /*height - 29 - 2 * lcdMargin*/,               
                out display, out lcdSurface, ref lcdScreen);
            
            CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, false);

            area = new TextArea(intf.gui, ListBoxType.LCD);         
            area.RenderType = RenderType.CRTAndLCD;
            area.Init(Label.LabelType.LCDNormal);         
            area.CanGrowInHeight = true;
            surfaceGrid.AddEntry(area, area);
            area.X = lcdSideMargin;
            area.Y = 45;
            area.Width = surfaceGrid.SurfaceWidth - 2 * lcdSideMargin; // 196; // triggers BreakText that requires font to be set
        
           
            Window.Level = Level.EventDialog;

            //*** CRT ****
            int crtWidth = 279;
            int crtHeight = 216; // 218;

            image = new WindowSystem.Image(intf.gui);
           // Window.Add(image);
            image.RenderType = RenderType.CRTAndLCD;
            image.ScaleImageToSizeOfControl = true;
            image.Width = crtWidth;
            image.Height = crtHeight;
            image.X = 496;
            image.Y = 33; 

         
            StatusScreen.AddCRTPlasticFrame(intf.gui, Window, new Point(image.X, image.Y), crtWidth, crtHeight, out crtPlasticEdge);                      

            crtScreen = intf.DisplayPanelRenderer.AddCRT(image, 
                            new Point(
                                image.AbsolutePosition.X,
                                image.AbsolutePosition.Y),
                                crtWidth,
                                crtHeight, 
                                Window.Level, Window,
                                ReflectionToUse.Small, true);

            for (int i = 0; i < 4; i++)
            {
                TextButton button = new TextButton(intf.gui);
                button.Init(TextButton.TextButtonType.White);
             //   button.Height = 20;
                button.Y = crtPlasticEdge.Bottom + 9;
                buttons[i] = button;
                button.Click += new ClickHandler(button_Click);
                button.EventArgs = new ButtonEventArgs() { Key = null, Index = i };
            }

         
                
            
         //   RosterPanel.AddDirtOnLeftEdge(Interface.gui, Form);
         //   RosterPanel.AddDirtOnBottomEdge(Interface.gui, Form);

            RosterPanel.AddDirtOnIrregularTopEdge(Interface.gui, Window);

            AddDirtOnBottomEdge(Interface.gui, Window);
        }

        private void AddDirtOnBottomEdge(GUIManager gui, Window Form)
        {
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
            //Image dirtBottom = AddImage(gui, Form, rect, new Point(445, 0));
            Image dirtBottom = AddImage(gui, Form, rect, new Point(445, Form.Height - rect.Height));
            dirtBottom.RenderType = RenderType.Overlay;

        }

       
       

        private bool IsNewestEvent()
        {
            if (The.Client.CurrentEventDialogIndex == The.Client.EventDialogsData.Count - 1)
                return true;

            return false;
        }

      

        void button_Click(UIComponent sender, EventArgs e)
        {
            ButtonEventArgs buttonArgs = e as ButtonEventArgs;

            // used on Win game screen
            if (ButtonClicked != null)
            {               
                ButtonClicked(buttonArgs.Key, buttonArgs.Index);
            }

           
            // used by tutorials etc.
            if (dialogOptions != null)
            {
                DialogOption selectedOption = dialogOptions[buttonArgs.Index];

               /* if (selectedOption.ActiveInArchive || mode != Mode.EventArchive)
                {*/
                    // fire the actions associated with this button:
                    ActionSets actionSets;
                    if (GameData.Instance.AllActionSets.TryGetValue(selectedOption.ActionSet, out actionSets))
                    {
                        bool isExpired;
                        actionSets.Fire(null, null, null, out isExpired);
                    }
               // }
            }


            Hide(); 

        }

       /* private void SetDefaultButtonValues()
        {
            buttonsOnForm.Clear();

            TextButton okButton = buttons[0];
            ((ButtonEventArgs)okButton.EventArgs).Key = "ok";
            okButton.Text = "OK";
            okButton.Width = 87;
            okButton.Y = crtPlasticEdge.Bottom;
          
            AddButton(okButton);

            TextButton cancelButton = buttons[1];
            ((ButtonEventArgs)cancelButton.EventArgs).Key = "cancel";
            cancelButton.Text = "CANCEL";
            cancelButton.Width = 87;
            cancelButton.Y = crtPlasticEdge.Bottom;
          
            AddButton(cancelButton);


            // remove the others:
            for (int i = 2; i < maxNoOfButtons; i++)
            {
                Form.Remove(buttons[i]);
            }
            
            ArrangeButtons();
        }*/

        private static void AddButton(Window Form, List<TextButton> buttonsOnForm, TextButton okButton)
        {
            Form.Add(okButton);
            buttonsOnForm.Add(okButton);
        }

        /// <summary>
        /// evenly spaces the buttons on the form.
        /// </summary>
        public static void ArrangeButtons(List<TextButton> buttonsOnForm, TextButton[] buttons, int leftXPos, int rightEdge)
        {
            // perhaps generalize this method so it can evenly space other elements too..

            int noOfButtonsOnForm = buttonsOnForm.Count; 

            if (noOfButtonsOnForm > 0)
            {
                TextButton leftMostButton = buttonsOnForm[0];

                leftMostButton.X = leftXPos; // 451; // display.X;
                    

                if (noOfButtonsOnForm > 1)
                {
                    TextButton rightmostButton = buttons[noOfButtonsOnForm - 1];
                    rightmostButton.X = rightEdge /* 756*/ - rightmostButton.Width;  // right align
                    

                    if (noOfButtonsOnForm > 2)
                    {
                        // evenly space any other buttons:
                        int totalButtonWidth = buttonsOnForm.Sum(bt => bt.Width);

                        int spacingToUse = (rightmostButton.Right - leftMostButton.X - totalButtonWidth) / (noOfButtonsOnForm - 1); 

                        int lastRightEdge = buttons[0].Right;

                        for (int i = 1; i < buttonsOnForm.Count - 1; i++)
                        {
                            TextButton button = buttonsOnForm[i];
                            button.X = lastRightEdge + spacingToUse;

                            lastRightEdge = button.Right;
                        }
                    }
                }
            }
        }

     

        public class ButtonEventArgs: EventArgs
        {
            public string Key;
            public int Index;
        }

      /*  private void OK_Click(UIComponent sender, EventArgs e)
        {
            Hide();
            if (OnOKClicked != null)
            {
                OnOKClicked.Invoke(sender, e);
            }

            if (ButtonClicked != null)
            {
                ButtonEventArgs eventArgs = (ButtonEventArgs)e;
                ButtonClicked(eventArgs.Key, eventArgs.Index);
            }
        }

        private void Cancel_Click(UIComponent sender, EventArgs e)
        {
            Hide();
            if (OnCancelClicked != null)
            {
                OnCancelClicked.Invoke(sender, e);
            }

            if (ButtonClicked != null)
            {
                ButtonClicked(1);
            }
        }*/

        public void SetButtonText(int index, string text)
        {
            buttons[index].Text = text;
            buttons[index].ScaleWidthToFitText();
        }


        private const int leftButtonXPos = 451;
        private const int rightButtonEdge = 756;

       // public const int DefaultButtonWidth = 87;

        public void ShowImageAndText(
            string imageName, 
            string heading, 
            string text,         
            bool showOkButton = true,
            string okButtonText = "OK", //null,
            string okButtonTooltip = null,
            int? okButtonWidth = null, // = DefaultButtonWidth,     
   
            bool showCancelButton = false,
            string cancelButtonText = "CANCEL",
            string cancelButtonTooltip = null,
            int? cancelButtonWidth = null, // = DefaultButtonWidth,
            Mode mode = Mode.NewEvent
            )
        {
            FillImageAndText(imageName, heading, text);

            this.dialogOptions = null;
            this.mode = mode;

            RemoveButtons();

            if (showOkButton)
            {            
                TextButton okButton = buttons[0];
                SetupButton(Window, buttonsOnForm, okButtonText, okButtonTooltip, okButtonWidth, okButton);              
            }

            if (showCancelButton)
            {
                TextButton cancelButton = buttons[1];
                SetupButton(Window, buttonsOnForm, cancelButtonText, cancelButtonTooltip, cancelButtonWidth, cancelButton);   
            }

           // SetupModeButtons();

            ArrangeButtons(buttonsOnForm, buttons, leftButtonXPos, rightButtonEdge); // TODO: amke sure buttons don't overlap like they do on the wing game dialog

        }

        public void ShowImageAndText(
                                   string imageName,
                                   string heading,
                                   string text,
                                   DialogOption[] dialogButtons,
                                   Mode mode = Mode.NewEvent)
        {
            FillImageAndText(imageName, heading, text);

            this.dialogOptions = dialogButtons;
            this.mode = mode;

            // clear all handlers registered by ActionSets
            if (ButtonClicked != null)
            {
                foreach (Delegate d in ButtonClicked.GetInvocationList())
                {
                    ButtonClicked -= (Action<string, int>)d;
                }
            }

            SetupButtons(Window, dialogButtons, buttonsOnForm, buttons);

            //SetupModeButtons();

            ArrangeButtons(buttonsOnForm, buttons, leftButtonXPos, rightButtonEdge);


        }

        public static void SetupButtons(Window Form, DialogOption[] dialogButtons, List<TextButton> buttonsOnForm, TextButton[] buttons)
        {
            buttonsOnForm.Clear();

            for (int i = 0; i < dialogButtons.Length; i++)
            {
                DialogOption dialogButton = dialogButtons[i];
                TextButton textButton = buttons[i];

                textButton.Text = dialogButton.Text;
                textButton.ToolTip = dialogButton.Tooltip;
                if (dialogButton.ButtonWidth.HasValue)
                {
                    textButton.Width = dialogButton.ButtonWidth.Value;
                }
                else
                {
                    textButton.ScaleWidthToFitText();
                }

                Form.Add(textButton);
                buttonsOnForm.Add(textButton);
            }

            for (int i = dialogButtons.Length; i < buttons.Length; i++)
            {
                // hide the unused buttons:
                Form.Remove(buttons[i]);
            }
        }

      /*  private void SetupModeButtons() 
        {            

            switch (mode)
            {
                case Mode.NewEvent:
                    Form.Add(previousButton);
                    Form.Add(nextButton);
                    Form.Remove(btClose);                   
                    break;

                case Mode.EventArchive:
                    Form.Add(previousButton);
                    Form.Add(nextButton);
                    Form.Add(btClose);
                    break;

                case Mode.OtherDialog:
                    Form.Remove(previousButton);
                    Form.Remove(nextButton);
                    Form.Remove(btClose);
                   
                    break;
            }
                      
        }*/



        private void FillImageAndText(string imageName, string heading, string text)
        {
            if (imageName != null)
            {
                Window.Add(image);
                image.SetSkinLocation(SkinState.Normal,Interface.gui.GUI_CRT_SpriteSheet.GetSourceRectangle(imageName));
            }
            else
            {
               
                // TODO: this has a displacement bug. it should be fixed so we can show events without any image...
                
                //If you remove "image" from Window then it gets displaced if we add it we have a black image on the right place
                //display default black image
                 Window.Add(image);          
           //    Window.Remove(image);
            }

            image.Texture = Interface.gui.GUI_CRT_SpriteSheet.Texture;
            

            surfaceGrid.BeginAddingEntries();

            lblTitle.Text = (heading ?? "").ToUpper(Config.Culture);
            lblTitle.FitToText();

            area.Text = text;

            surfaceGrid.EndAddingEntries();
        }   
  


        public static void SetupButton(Window form, List<TextButton> buttonsOnForm, string text, string tooltip, int? width, TextButton button)
        {
            AddButton(form, buttonsOnForm, button);

            if (text != null)
            {
                button.Text = text;
            }

            button.ToolTip = tooltip;
            /*
            if (tooltip != null)
            {
                button.ToolTip = tooltip;
            }
            else
            {
                button.ToolTip = null;
            }*/

            if (width.HasValue == true)
            {
                button.Width = width.Value;
            }
            else
            {
                button.ScaleWidthToFitText(); 
            }
        }



        private void RemoveButtons()
        {
            buttonsOnForm.Clear();

            foreach (var item in buttons)
            {
                Window.Remove(item);
            }
        }

      /*  public void ShowText(string text)
        {
            Remove(image);

            area.Width = minWidth - 2 * doubleSpacing;
            area.Text = text;
            area.Y = doubleSpacing;

            //scale window after text (with a minimum)
            Form.Height = Math.Max(minHeight, area.Height + 2 * doubleSpacing);
                       
        }*/


       
        public override void Hide()
        {
           

            base.Hide();

           // The.InGameUI.poolOfEventDialogs.Retire(this);

            //Retire();

        }

       
    }
}
