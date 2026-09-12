using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.SimSide.Allegiances;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Communication;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide;

namespace UWGame.ClientSide.Interface
{
    public class EventArchivePanel: RosterPanel
    {
        

        // in LCD screen:
        Grid surfaceGrid;
        TextArea area;

        // in CRT screen:
       // Image image;


        /// <summary>
        /// contains the script actions that should fire
        /// </summary>
        DialogOption[] dialogOptions;


        const int maxNoOfButtons = 4;


        /// <summary>
        /// up to 4 buttons are available. Per default, button 0 is OK, button 1 is cancel
        /// </summary>
        private TextButton[] buttons = new TextButton[maxNoOfButtons];

      

        /// <summary>
        /// Previous and Next buttons will always be displayed.
        /// But they should be greyed out (disabled) if we are at the last or first element.
        /// </summary>
        private ImageButton previousButton;
        private ImageButton nextButton;

        private List<TextButton> buttonsOnForm = new List<TextButton>();



        const int itemHeight = 36;
        const int horizPadding = 6;
        const int vertPadding = 4;


        private const int leftButtonXPos = bottomButtonXMargin; 
        private const int rightButtonEdge = 312;


        public EventArchivePanel()
            : base("EVENT ARCHIVE", 487, 486)
        {
            base.HasStatusCRT = true;

            CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, false);


            area = new TextArea(Interface.gui, ListBoxType.LCD);

            area.RenderType = RenderType.CRTAndLCD;
            area.Init(Label.LabelType.LCDNormal);
            area.CanGrowInHeight = true;
            surfaceGrid.AddEntry(area, area);
            area.X = lcdSideMargin;
            area.Y = 45;
            area.Width = surfaceGrid.SurfaceWidth - 2 * lcdSideMargin; 
        
            
            previousButton = new ImageButton(Interface.gui);
            previousButton.InitWithIcon(ImageButtonType.White, "arrowblack_left", false);
            //   previousButton.Height = npHeight;
            // previousButton.Width = npWidth;
            // previousButton.Height = 20;           
          /*  previousButton.Y = crtPlasticEdge.Bottom + 9;
            previousButton.X = 571; // crtPlasticEdge.X;*/
            previousButton.ToolTip = "Previous event";
            //  previousButton.Width = 10;
            previousButton.Click += new ClickHandler(previousButton_Click);
            Window.Add(previousButton);
            PlaceButtonUnderLCD(previousButton, 370);

            nextButton = new ImageButton(Interface.gui);
            //nextButton.Init(ImageButtonType.);
            nextButton.InitWithIcon(ImageButtonType.White, "arrowblack_right", false);
         /*   nextButton.Y = previousButton.Y;
            nextButton.X = 607; // previousButton.Right + 6;*/
            nextButton.ToolTip = "Next event";
            nextButton.Click += new ClickHandler(nextButton_Click);
            Window.Add(nextButton);
            PlaceButtonUnderLCD(nextButton, previousButton.Right + 4);


            for (int i = 0; i < 4; i++)
            {
                TextButton button = new TextButton(Interface.gui);
                button.Init(TextButton.TextButtonType.White);               
               // button.Y = crtPlasticEdge.Bottom + 9;
                buttons[i] = button;
                button.Click += new ClickHandler(button_Click);
                button.EventArgs = new EventDialog.ButtonEventArgs() { Key = null, Index = i };

                PlaceButtonUnderLCD(button);
            }


            InitStatusContentPanel();
        }


        private void InitStatusContentPanel()
        {
            statusContent = The.InGameUI.StatusScreen.GetNewSurfaceContent();

            // add an image control that will hold the dialog image:
            InitStatusImage(statusContent.Width, statusContent.Height);
            

            statusContent.Remove(pnBillboards); //??
        }

      /*  private void RefreshStatusScreen()
        {          
            
            The.InGameUI.StatusScreen.Refresh();

        }*/

       

        void nextButton_Click(UIComponent sender, EventArgs e)
        {
            if (The.Client.EventDialogsData.Count > 0)
            {
                int newIndex = Common.Min(The.Client.EventDialogsData.Count - 1,
                                          The.Client.CurrentEventDialogIndex + 1);

                if (newIndex != The.Client.CurrentEventDialogIndex)
                {
                    DisplayNextOrPreviousEvent(newIndex);
                }
            }
        }

        void btClose_Click(UIComponent sender, EventArgs e)
        {
            Hide();
        }

        void previousButton_Click(UIComponent sender, EventArgs e)
        {
            if (The.Client.EventDialogsData.Count > 0)
            {
                int newIndex = Common.Max(0, The.Client.CurrentEventDialogIndex - 1);

                if (newIndex != The.Client.CurrentEventDialogIndex)
                {
                    DisplayNextOrPreviousEvent(newIndex);
                }
            }
        }

        private void DisplayNextOrPreviousEvent(int newIndex)
        {
            The.Client.CurrentEventDialogIndex = newIndex;

            EventDialogData data = The.Client.EventDialogsData[The.Client.CurrentEventDialogIndex];

            // show either the dialog with custom buttons or default buttons
            if (data.DialogOptions != null)
            {
                ShowImageAndText(data.DisplayImage, data.Header, data.DisplayText, data.DialogOptions); //, mode: this.mode);
            }
            else
            {
                ShowImageAndText(data.DisplayImage, data.Header, data.DisplayText); //, mode: this.mode);
            }
        }



        private void ShowImageAndText(
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
           int? cancelButtonWidth = null
           )
        {
            FillImageAndText(imageName, heading, text);

            this.dialogOptions = null;
            
            RemoveButtons();

            if (showOkButton)
            {
                TextButton okButton = buttons[0];
                EventDialog.SetupButton(Window, buttonsOnForm, okButtonText, okButtonTooltip, okButtonWidth, okButton);
            }

            if (showCancelButton)
            {
                TextButton cancelButton = buttons[1];
                EventDialog.SetupButton(Window, buttonsOnForm, cancelButtonText, cancelButtonTooltip, cancelButtonWidth, cancelButton);
            }

            // SetupModeButtons();

            EventDialog.ArrangeButtons(buttonsOnForm, buttons, leftButtonXPos, rightButtonEdge); // TODO: amke sure buttons don't overlap like they do on the wing game dialog

        }

        private void ShowImageAndText(
                                   string imageName,
                                   string heading,
                                   string text,
                                   DialogOption[] dialogButtons)
        {
            FillImageAndText(imageName, heading, text);

            this.dialogOptions = dialogButtons;
           
           
            EventDialog.SetupButtons(Window, dialogButtons, buttonsOnForm, buttons);

            //SetupModeButtons();

            EventDialog.ArrangeButtons(buttonsOnForm, buttons, leftButtonXPos, rightButtonEdge);


        }


        private void FillImageAndText(string imageName, string heading, string text)
        {
            if (imageName != null)
            {
                //Form.Add(image);
                imStatusBackground.SetSkinLocation(SkinState.Normal,Interface.gui.GUI_CRT_SpriteSheet.GetSourceRectangle(imageName));

               // imStatusBackground.ResizeControlToFitImage(); // ??
            }
            else
            {
                // show default image??
                statusContent.Remove(imStatusBackground);
                //Form.Remove(image);
            }

            imStatusBackground.Texture = Interface.gui.GUI_CRT_SpriteSheet.Texture;

            lblTitle.Text = (heading ?? "").ToUpper(Config.Culture);
            lblTitle.FitToText();


            surfaceGrid.BeginAddingEntries();           

            area.Text = text;

            surfaceGrid.EndAddingEntries();


        }

        private void RemoveButtons()
        {
            buttonsOnForm.Clear();

            foreach (var item in buttons)
            {
                Window.Remove(item);
            }
        }

      
        public override void Show()
        {
            if (The.Client.EventDialogsData.Count > 0)
            {
                //Set the current ID to the latest added dialog
                The.Client.CurrentEventDialogIndex = The.Client.EventDialogsData.Count - 1;

                //Get the data that will be displayed.
                EventDialogData data = The.Client.EventDialogsData[The.Client.CurrentEventDialogIndex];

                // show either the dialog with custom buttons or default buttons
                if (data.DialogOptions != null)
                {
                    ShowImageAndText(data.DisplayImage,
                        data.Header,
                        data.DisplayText,
                        data.DialogOptions);
                }
                else
                {
                    ShowImageAndText(
                        data.DisplayImage,
                        data.Header,
                        data.DisplayText); /*,
                    showOkButton,
                    okButtonText,
                    okButtonTooltip,
                    okButtonWidth,
                    showCancelButton,
                    cancelButtonText,
                    cancelButtonTooltip,
                    cancelButtonWidth);*/
                }

            }


            base.Show(); // calls Refresh
        }



        void button_Click(UIComponent sender, EventArgs e)
        {
            EventDialog.ButtonEventArgs buttonArgs = e as EventDialog.ButtonEventArgs;

            // used on Win game screen
           /* if (ButtonClicked != null)
            {
                ButtonClicked(buttonArgs.Key, buttonArgs.Index);
            }*/


            // used by tutorials etc.
            if (dialogOptions != null)
            {
                DialogOption selectedOption = dialogOptions[buttonArgs.Index];

                if (selectedOption.ActiveInArchive) // || mode != Mode.EventArchive)
                {
                    // fire the actions associated with this button:
                    ActionSets actionSets;
                    if (GameData.Instance.AllActionSets.TryGetValue(selectedOption.ActionSet, out actionSets))
                    {
                        bool isExpired;
                        actionSets.Fire(null, null, null, out isExpired);
                    }
                }
            }

            The.InGameUI.CloseRosterPanel(); // opens Help window. or leave it..?

            /*
            Hide();
            The.InGameUI.HideStatusScreen();*/

        }
    }
}
