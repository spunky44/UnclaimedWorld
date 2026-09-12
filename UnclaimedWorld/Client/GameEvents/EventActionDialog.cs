using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.GameEvents
{
    /// <summary>
    /// a modal user dialog that appears in response to a game event
    /// 
    /// TODO: should have an allegiance target. If not the player, then suppress the dialog...
    /// add a manual condition..?
    /// </summary>
    public class EventActionDialog : EventActionType//: IGameData
    {
        public string Heading;
        public DynamicText DisplayText;

        /// <summary>
        /// for now, the image has to be in the CRT Content folder (rebuild the GUI_CRT_Sprites.xml spritesheet)
        /// </summary>
        public string DisplayImage;


        /// <summary>
        /// if filled, a button will be shown for each of these options.
        /// </summary>
        public DialogOption[] DialogOptions;


        public EventActionDialog(string keyName): base(keyName)
        {

        }

        public EventActionDialog()          
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public void Execute(EventAction eventAction, ref string failReason)
        {
            string substitutedText = DisplayText.GetSubstitutedText(action);

            EventDialog dialog;

            if (DialogOptions != null)
            {
                // show the dialog with custom buttons:
                dialog = ShowAndSaveEventDialog(DisplayImage, Heading, substitutedText, true, DialogOptions);
            }
            else
            {
                // show the dialog with default buttons:
                dialog = ShowAndSaveEventDialog(DisplayImage, Heading, substitutedText, true);
            }

            return true;

           /* dialog.ButtonClicked -= new Action<string, int>(dialog_ButtonClicked);   // makes sure we do not register twice
            dialog.ButtonClicked += new Action<string, int>(dialog_ButtonClicked);
            */
        }

      /*  void dialog_ButtonClicked(string key, int buttonIndex)
        {
            if (this.DialogOptions != null && this.DialogOptions.Length > buttonIndex)
            {
                DialogOption selectedOption = DialogOptions[buttonIndex];

                // fire the actions associated with this button:
                ActionSets actionSets;
                if (GameData.Instance.AllActionSets.TryGetValue(selectedOption.ActionSet, out actionSets))
                {
                    bool isExpired;
                    actionSets.Fire(null, null, out isExpired);
                }
            }
        }*/


        /// <summary>
        /// saves the dialog parameters in the archive, then displays the dialog
        /// </summary>
        /// <param name="displayImage"></param>
        /// <param name="heading"></param>
        /// <param name="displayText"></param>
        /// <param name="modal"></param>
        /// <param name="showOkButton"></param>
        /// <param name="okButtonText"></param>
        /// <param name="okButtonTooltip"></param>
        /// <param name="okButtonWidth"></param>
        /// <param name="showCancelButton"></param>
        /// <param name="cancelButtonText"></param>
        /// <param name="cancelButtonTooltip"></param>
        /// <param name="cancelButtonWidth"></param>
        /// <returns></returns>
        public static EventDialog ShowAndSaveEventDialog(
            string displayImage, 
            string heading, 
            string displayText, 
            bool modal,

            bool showOkButton = true,
            string okButtonText = "OK", 
            string okButtonTooltip = null,
            int? okButtonWidth = null,// EventDialog.DefaultButtonWidth,

            bool showCancelButton = false,
            string cancelButtonText = "CANCEL", 
            string cancelButtonTooltip = null,
            int? cancelButtonWidth = null // EventDialog.DefaultButtonWidth
            )
        {
           
            //Save the eventDialogData and set the currently displayed one to the newest index.
            The.Client.EventDialogsData.Add(new EventDialogData(displayText, heading, displayImage, null));

            EventDialog dialog = ShowEventDialog(modal,
                                                showOkButton,
                                                okButtonText,
                                                okButtonTooltip,
                                                okButtonWidth,
                                                showCancelButton,
                                                cancelButtonText,
                                                cancelButtonText,
                                                cancelButtonWidth);
          
            return dialog;
        }

        public static EventDialog ShowAndSaveEventDialog(
          string displayImage,
          string heading,
          string displayText,
          bool modal,
          DialogOption[] dialogOptions)
        {

            //Save the eventDialogData and set the currently displayed one to the newest index.
            The.Client.EventDialogsData.Add(new EventDialogData(displayText, heading, displayImage, dialogOptions));

            EventDialog dialog = ShowEventDialog();

            return dialog;
        }



        /// <summary>
        /// Displays the latest added EventActionDialog
        /// The player can then cycle through these dialogs.
        /// </summary>
        /// <param name="modal"></param>
        /// <param name="showOkButton"></param>
        /// <param name="okButtonText"></param>
        /// <param name="okButtonTooltip"></param>
        /// <param name="okButtonWidth"></param>
        /// <param name="showCancelButton"></param>
        /// <param name="cancelButtonText"></param>
        /// <param name="cancelButtonTooltip"></param>
        /// <param name="cancelButtonWidth"></param>
        /// <returns></returns>
        public static EventDialog ShowEventDialog(
            bool modal = true,
            bool showOkButton = true,
            string okButtonText = "OK",
            string okButtonTooltip = null,
            int? okButtonWidth = null, // EventDialog.DefaultButtonWidth,

            bool showCancelButton = false,
            string cancelButtonText = "CANCEL",
            string cancelButtonTooltip = null,
            int? cancelButtonWidth = null, //= EventDialog.DefaultButtonWidth,
            
            EventDialog.Mode mode = EventDialog.Mode.NewEvent)
        {
            EventDialog dialog = The.InGameUI.EventDialog;

            //Set the current ID to the latest added dialog
            The.Client.CurrentEventDialogIndex = The.Client.EventDialogsData.Count - 1;

            //Get the data that will be displayed.
            EventDialogData data = The.Client.EventDialogsData[The.Client.CurrentEventDialogIndex];

            // show either the dialog with custom buttons or default buttons
            if (data.DialogOptions != null)
            {
                dialog.ShowImageAndText(data.DisplayImage,
                    data.Header,
                    data.DisplayText,
                    data.DialogOptions, 
                    mode: mode); 
            }
            else
            {
                dialog.ShowImageAndText(
                    data.DisplayImage,
                    data.Header,
                    data.DisplayText,                 
                    showOkButton,
                    okButtonText,
                    okButtonTooltip,
                    okButtonWidth,
                    showCancelButton,
                    cancelButtonText,
                    cancelButtonTooltip,
                    cancelButtonWidth,
                    mode: mode);
            }

            if (/*mode != EventDialog.Mode.EventArchive &&*/ modal) 
            {
                The.Client.SetModal(true);              
            }

            dialog.ShowInScreenSpace(200, 200, modal);
            dialog.Window.CenterWindow();

            return dialog;
        }


       
    }
}
