using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using WindowSystem;
using UWGame.ClientSide.Interface;


namespace UWGame.Client.MainMenu.LoadReplay
{
    /// <summary>
    /// all these file browser panels should work the same - on save/load, expose the filename in a property, then invoke an event.
    /// DO NOT put loading/saving code inside these classes!
    /// </summary>
    public class LoadReplayPanel : Panel
    {
        Box display; //, edges;
        LCDScreen lcdScreen;
        UIComponent lcdSurface;

        Grid grid;

        TextBox tbReplayPauseTime;
        Label lblMessages;

        public event EventHandler CancelClick;
        public event EventHandler LoadClick;

        private Label folderPathLabel;
        private string replayFolderPath;

        /// <summary>
        /// here is the dialog result
        /// </summary>
        public string SelectedLoadReplayPath
        {
            get;
            private set;
        }

        public float? TimeToPauseReplay
        {
            get;
            private set;
        }



        public LoadReplayPanel(CommonInterface intf, Point position) :
            base(intf, "LOAD REPLAY", position,
                 new Vector2(440, 560), Level.Dialogs)
        {
            /*  Label title = new Label(intf.gui);
              Form.Add(title);
              title.Text = ("LOAD REPLAY");
              title.X = MarginX;
              title.Y = MarginY;
              title.Init(Label.LabelType.PlainPanelHeader);
              */

            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 160, new Point(MarginX, 60), out display, /*out edges,*/ out lcdSurface, ref lcdScreen);

            lblMessages = new Label(intf.gui);
            lcdSurface.Add(lblMessages);
            lblMessages.Text = ("Select a replay to load.");

            lblMessages.Init(Label.LabelType.LCDNormal);

            grid = FullLCDPanel.AddGridWithFixedItemHeights(intf.gui, lcdSurface, 30);

            folderPathLabel = new Label(intf.gui);
            folderPathLabel.Init(Label.LabelType.LCDNormal);

            InitButtons();


            PopulateFileList();

            Label lblPause = new Label(intf.gui);
            Window.Add(lblPause);
            lblPause.Init(Label.LabelType.PlainPanelNormal);
            lblPause.Text = "Time (s) to pause:";
            lblPause.FitToText();
            lblPause.X = display.X;
            lblPause.Y = display.Bottom + DoubleSpacing;

            tbReplayPauseTime = new TextBox(intf.gui);
            Window.Add(tbReplayPauseTime);
            tbReplayPauseTime.Position = new Point(lblPause.Right + DoubleSpacing, display.Bottom + DoubleSpacing);
            tbReplayPauseTime.Width = display.Width - tbReplayPauseTime.X;
            tbReplayPauseTime.Height = 27;
            tbReplayPauseTime.IsEditable = true;
            tbReplayPauseTime.DebugTag = "replayPanelPauseTime";
        }

        

        private void PopulateFileList()
        {
            replayFolderPath = Config.GetDataFolderPath(Config.DataType.Replays);
            List<DirectoryInfo> directories;
            List<string> listOfReplayFiles = Recorder.GetListOfReplayFolders(replayFolderPath, out directories);

            grid.BeginAddingEntries();
            grid.Clear();

            grid.AddEntry("folderPath", folderPathLabel);
            folderPathLabel.Text = replayFolderPath + " :";
            foreach (string replayFile in listOfReplayFiles)
            {
                grid.AddEntry(replayFile, replayFile);
            }

            grid.EndAddingEntries();
        }

        void btClose_Click(UIComponent sender, EventArgs e)
        {
            Window.Hide();

            if (CancelClick != null)
            {
                CancelClick.Invoke(sender, e);
            }
        }


        private void InitButtons()
        {
            Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
            Panel.AddImage(Interface.gui, Window, rect, new Point(40, 30));



            /////////////////////////////////
            /*  Box brown = new Box(Interface.gui);
              Form.Add(brown);
              rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_darkblue"); //"basic_brown");
              brown.SetSkinLocation(SkinState.Normal,rect);
              brown.CornerSize = 3;
              brown.Position = new Point(display.X, Form.Height - 40 - MarginY);//edges.Y + edges.Height + 10 + 70);
              brown.Width = display.Width;
              brown.Height = 40;
              */

            TextButton btCommit = new TextButton(Interface.gui);
            Window.Add(btCommit); // add first!!! sets defaults!
            btCommit.Init(TextButton.TextButtonType.White);
            //btCommit.Position = new Point(brown.X + 5, brown.Y - 2);
           
            btCommit.Text = ("LOAD");
            btCommit.ToolTip = ("Loads a replay.");
            btCommit.Click += new ClickHandler(buttonLoad_Click);
            btCommit.ScaleWidthToFitText();
            PlaceRightButtonUnderLCD(btCommit);

            TextButton btCancel = new TextButton(Interface.gui);
            Window.Add(btCancel); // add first!!! sets defaults!
            btCancel.Init(TextButton.TextButtonType.White);
            //btCancel.Position = new Point(btCommit.X + btCommit.Width + 5, brown.Y - 2);
            PlaceLeftButtonUnderLCD(btCancel);
            btCancel.Text = "CANCEL";
            btCancel.ToolTip = "Cancels and closes the dialog.";
            btCancel.ScaleWidthToFitText();
            btCancel.Click += new ClickHandler(btCancel_Click);
        }

        void buttonLoad_Click(UIComponent sender, EventArgs e)
        {
            object key;
            if (grid.GetSelectedKey(out key))
            {

                string replayFolder = key.ToString();

                SelectedLoadReplayPath = replayFolderPath + "\\" + replayFolder;
                TimeToPauseReplay = tbReplayPauseTime.GetNumber();

                if (LoadClick != null)
                    LoadClick.Invoke(this, null);

            }

        }

        public override void ShowDialog(bool modal)
        {

            base.ShowDialog(modal);

            TimeToPauseReplay = null;
            SelectedLoadReplayPath = null;

            PopulateFileList();
        }

        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            Window.Hide();

            if (CancelClick != null)
            {
                CancelClick.Invoke(sender, e);
            }
        }

        public override void Update(GameTime elapsed)
        {
            base.Update(elapsed);

        }
    }
}