using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// this class monitors and orchestrates ingame save/loads
    /// </summary>
    public class SaveLoadMessageBox: MessageBox
    {
        public enum Mode { None, Save, LoadAfterSave, Load }

        Thread saveThread;
        Mode mode = Mode.None;
     //   bool isSaving = false; // true during save+load
        bool saveThreadFinished = false;
        string savePath;
        TimeSpan startedTimeStamp;


        public SaveLoadMessageBox(CommonInterface intf): base(intf)
        {

        }

        public void StartLoad(bool isLoadAfterSave)
        {
            SetStartTimestamp();

            if (isLoadAfterSave)
            {
                mode = Mode.LoadAfterSave;
            }
            else
            {
                mode = Mode.Load;
            }
        }

        private void SetStartTimestamp()
        {
            startedTimeStamp = The.Client.GameTime.TotalGameTime; // The.Sim.GameTime.TotalGameTime; 
        }

        public void StartSave(string savePath)
        {
            /*
             start saving in a background thread. 
             * show a modal message box with no buttons
             * in Update, do a busy wait until the thread has finished
             * then continue with load... 
             */

            this.savePath = savePath; // saveDialog.SelectedSaveGamePath;
            // The.Sim.DoSave(path);

            if (mode != Mode.Save) // isSaving == false)
            {
                //startedTimeStamp = The.Sim.GameTime.TotalGameTime; // .TotalUnPausedGameTime;
                SetStartTimestamp();

                saveThreadFinished = false;

                saveThread = new System.Threading.Thread(SaveInThread);
                saveThread.IsBackground = true;
                saveThread.Start();
                mode = Mode.Save;
               // isSaving = true;


                ShowMessage("Saving. Please wait...", null, true, MessageBox.ButtonOptions.None); // gets removed after load when client is recreated

            }
            //  LoadGameDirectly(path);
        }

       /* public override void ShowMessage(string message) //, bool modal = true, MessageBox.ButtonOptions buttonOptions = ButtonOptions.OK)
        {
            base.ShowMessage(message, true, MessageBox.ButtonOptions.None);
        }*/

        private void SaveInThread()
        {
            The.Sim.DoSave(savePath); // path);

            saveThreadFinished = true;
        }

        /// <summary>
        /// Updates will stop while the Sim and Client are loaded. After that, this instance will have been replaced too
        /// </summary>
        /// <param name="elapsed"></param>
        public override void Update(GameTime elapsed) // uses Client game time
        {
            base.Update(elapsed);

            if (mode == Mode.Save) // isSaving)
            {
                UpdateProgress(elapsed);

                if (saveThreadFinished)
                {
                    if (saveThread != null
                        && saveThread.IsAlive)
                    {
                        if (Thread.CurrentThread != saveThread)
                        {
                            saveThread.Join();
                        }
                    }

                    // cannot dispose a Form from another thread, so do it now:
                   /* Kensei.Dev.Options.Destroy();


                    // run this in a thread:
                    The.Sim.LoadSavedGame(savePath); // this messagebox will not be drawn/updated shortly after this call
                    */
                    LoadGameDirectlyInGame(savePath, true);

                    mode = Mode.None;
                   // isSaving = false;
                }
            }
            else if (mode == Mode.Load || mode == Mode.LoadAfterSave)
            {
                UpdateProgress(elapsed);


            }
        }

        private void UpdateProgress(GameTime elapsed)
        {
            TimeSpan timeSinceSaveStarted = elapsed.TotalGameTime - startedTimeStamp;
            string msg = "";
            switch(mode)
            {
                case Mode.Save:
                    msg = "Saving (1/2). Please wait... ";
                    break;

                case Mode.LoadAfterSave:
                    msg = "Saving (2/2). Please wait... ";
                    break;

                case Mode.Load:
                    msg = "Loading. Please wait... ";
                    break;

            }           

            int bars = Common.ClampBottom((int)timeSinceSaveStarted.TotalSeconds, 0);
            Text = msg + new string('|', bars); //+ timeSinceSaveStarted.TotalSeconds.ToString();
        }


        public static void LoadGameDirectlyInGame(string savePath, bool isLoadAfterSave)
        {
            // cannot dispose a Form from another thread, so do it now:
            Kensei.Dev.Options.Destroy();


            The.IngameLoadScreen.Interface.SaveLoadMessageBox.StartLoad(isLoadAfterSave);
           
            // run this in a thread:
            The.Sim.LoadSavedGame(savePath); // this messagebox will not be drawn/updated shortly after this call
        }

    }
}
