using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.InGameEvents.Actions
{
    public class DialogOption: ISnapshot
    {
        //public DialogButton DialogButton;

        public string Text;
        public string Tooltip;

        public int? ButtonWidth;
        
        /// <summary>
        /// fire these when the user chooses this option button.
        /// </summary>
        public string ActionSet;
       // public ActionSets Actions;

        /// <summary>
        /// set this to true if the button should be active even when the dialog is opened again later via the archive feature
        /// </summary>
        public bool ActiveInArchive = false;



        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Text = sn.DoString(Text);
            this.Tooltip = sn.DoString(Tooltip);
            this.ButtonWidth = sn.DoInt32Nullable(ButtonWidth);
            this.ActionSet = sn.DoString(ActionSet);
            this.ActiveInArchive = sn.DoBool(ActiveInArchive);
           

            return this;
        }



        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }


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

        #endregion
    }
}
