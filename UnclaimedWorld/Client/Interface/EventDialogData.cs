using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// for the event archive. contains substituted text and copied fields from EventActionDialog
    /// </summary>
    public class EventDialogData: ISnapshot
    {
        public string DisplayText;
        public string Header;
        public string DisplayImage;

        public DialogOption[] DialogOptions;


        public EventDialogData(string substitutedText, string header, string displayImage, DialogOption[] dialogOptions)
        {
            DisplayText = substitutedText;
            Header = header;
            DisplayImage = displayImage;
            this.DialogOptions = dialogOptions;
        }



        public EventDialogData( /*parameterless*/)
        {
        }

        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.DisplayText = sn.DoString(DisplayText);
            this.Header = sn.DoString(Header);
            this.DisplayImage = sn.DoString(DisplayImage);
            this.DialogOptions = sn.DoArray(DialogOptions);

            return this;
        }




        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (DialogOptions != null)
            {
                foreach (var item in DialogOptions)
                {
                    item.LoadPostProcess(sn);
                }
            }
        }

        #endregion


    }
}
