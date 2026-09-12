 #define SAVE_LOAD

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using System.IO;
using UWGame.SimSide.Maps;
using System.Xml.Serialization;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide.Interface.LCD;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Scenarios;
using System.IO.Compression;
using GameStateManagement;
using UWGame.ClientSide.MainMenu.Scenario;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// all these file browser panels should work the same - on save/load, expose the filename in a property, then invoke an event.
    /// DO NOT put loading/saving code inside these classes!
    /// </summary>
    public class SaveLoadGamePanel : Panel
    {
        public enum SaveOrLoad { Save, Load }
        public SaveOrLoad SaveOrLoadValue;
        Box display;  
        LCDScreen lcdScreen;
        UIComponent lcdSurface;
        Grid grid;
        TextBox tbFileName;

        TextArea taNote;
        ErrorsAndMessages output;
              
        public event EventHandler SaveOrLoadClick;
        public event EventHandler CancelClick;


        const int itemHeight = 112;

        /// <summary>
        /// here is the dialog result
        /// </summary>
        public string SelectedSaveGamePath
        {
            get;
            private set;
        }

        public SaveLoadGamePanel(SaveOrLoad saveOrLoad, CommonInterface intf, Point position) :
            base(intf, "SAVE / LOAD", position, new Vector2(822 /*792*/, 620), Level.Menu) //.Dialogs)
                                 //  base(intf, position, new Vector2(440, 560), Level.Dialogs)
        {
            SaveOrLoadValue = saveOrLoad;

            RosterPanel.CreateRosterStyleLCDPanel(intf, Window, out display, out lcdSurface, ref lcdScreen);

            int textMargin = 1;

            grid = FullLCDPanel.AddGridWithFixedItemHeights(intf.gui, lcdSurface, 58); // 30);
            grid.ItemHeight = itemHeight;
            grid.Selectability = Grid.SelectabilityOptions.None;


            taNote = new TextArea(intf.gui, ListBoxType.LCD);
            taNote.RenderType = RenderType.CRTAndLCD;
            taNote.Init(Label.LabelType.LCDNormal);
            taNote.CanGrowInHeight = true;
            lcdSurface.Add(taNote);
            taNote.X = textMargin;
            taNote.Y = 2;
            taNote.Width = lcdSurface.Width - 4;            
            if (SaveOrLoadValue == SaveOrLoad.Save)
            {
                taNote.Text = string.Format("Save files may become obsolete when they are older than the current version {0} of the game. To load those, you must change to the corresponding version of the game using the Steam library list. For more info, go to the Unclaimed World forum on Steam.", UnclaimedWorld.GetVersionAsString()); //was: Saving typically takes 20 seconds. \n     mp unnecessary instruction : Select an existing save file to overwrite or enter a new name in the box below.
                
            }
            else
            {
                taNote.Text = string.Format("Select a game to load. \nSave files with a version number lower than the current version {0} of the game may not work. To load those, you must change to the corresponding version of the game using the Steam library list. For more info, go to the Unclaimed World forum on Steam.", UnclaimedWorld.GetVersionAsString()); //was  Loading can take up to 100 seconds.
            }

            /* output.ShowMessage(SaveOrLoadValue == SaveOrLoad.Save ? "Select an existing save file to overwrite or enter a new name in the box below."
                : "Select a game to load.");*/
            output = new ErrorsAndMessages(lcdSurface, intf.gui, textMargin, taNote.Bottom); // 2);
         

            InitButtons();          

           // ExpandedPanel.AddDirtOnEdges(Interface.gui, Form);
        }

       


        // GUI //
        private void InitButtons()
        {           
            Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
            Panel.AddImage(Interface.gui, Window, rect, new Point(40, 30));

          /*  Box brown = new Box(Interface.gui);
            Form.Add(brown);
            rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_darkblue");  
            brown.SetSkinLocation(SkinState.Normal,rect);
            brown.CornerSize = 3;
            brown.Position = new Point(display.X, Form.Height - 40 - MarginY); 
            brown.Width = display.Width;
            brown.Height = 40;*/

           // int startX = display.X + 5;

            TextButton btCancel = new TextButton(Interface.gui);
            Window.Add(btCancel); // add first!!! sets defaults!
            btCancel.Init(TextButton.TextButtonType.White);
            PlaceLeftButtonUnderLCD(btCancel);
           // btCancel.Position = new Point(display.X - 2, Form.Height - 36 - MarginY); 
            btCancel.Text = "CANCEL";
            btCancel.ToolTip = "Cancels and closes the dialog.";
            btCancel.Click += new ClickHandler(btCancel_Click);
            btCancel.ScaleWidthToFitText();
          
            if (SaveOrLoadValue == SaveOrLoad.Save)
            {

                TextButton btSave = new TextButton(Interface.gui);
                Window.Add(btSave); // add first!!! sets defaults!
                btSave.Init(TextButton.TextButtonType.White);
                btSave.Position = new Point(300, btCancel.Y);
                btSave.Text = "NEW SAVE"; 
                btSave.ToolTip = "Saves the game in a new file."; 
                btSave.Click += new ClickHandler(btSaveNew_Click);              
                btSave.ScaleWidthToFitText();

                tbFileName = new TextBox(Interface.gui);
                Window.Add(tbFileName);
                tbFileName.Position = new Point(btSave.Right + DoubleSpacing, display.Y + display.Height + 10);               
                tbFileName.Width = display.Width - tbFileName.X; // display.Width;
                tbFileName.Height = 27;
                tbFileName.IsEditable = true;
                tbFileName.CenterThisVertically(btSave.Y + btSave.Height / 2);

            }


           
        }


        const int selectButtonWidth = 80;

       // const int thumbnailPanelWidth = 135;
       // const int descriptionPanelWidth = 446;
      //  const int selectPanelWidth = 148;

        const int horizPadding = 8;
        const int vertPadding = 5;
        int itemPadding = SingleSpacing;

        private void AddItemRow(string fullPath, long fileLength, SnapshotHeader header)
        {
            Image thumbnail;
            UIComponent item;

            item = new UIComponent(Interface.gui);

            LCDInnerPanel thumbnailPanel = new LCDInnerPanel(Interface.gui, SelectScenarioPanel.ThumbnailPanelWidth, false); // lcdSurfaceOptions.Width);
            item.Add(thumbnailPanel.Panel);

            Scenario scenario = null;
            if (header.StartGameParams.StartScenarioParams != null) // we allow DebugScenarios also
            {
                scenario = header.StartGameParams.StartScenarioParams.Scenario;
            }

            string name = Path.GetFileNameWithoutExtension(fullPath);

            int thumbnailWidth = itemHeight - 2 * itemPadding;
            if (scenario != null && !string.IsNullOrEmpty(scenario.ThumbnailImage))
            {
                thumbnail = new Image(Interface.gui);
                thumbnailPanel.AddContentSetFullWidth(thumbnail); // .Panel.Add(thumbnail);

                Rectangle? rect;
                if (!Interface.gui.GUISpriteSheet.TryGetSourceRectangle(scenario.ThumbnailImage, out rect))
                {
                    thumbnail.Texture = Interface.gui.ContentManager.Load<Texture2D>(scenario.ThumbnailImage);
                    //   thumbnail.SetSkinLocation(SkinState.Normal,thumbnail.Texture.Bounds);
                }
                else
                {
                    thumbnail.SetSkinLocation(SkinState.Normal,rect.Value);
                    thumbnail.Texture = Interface.gui.GUISpriteSheet.Texture;
                }
                //   Rectangle rect = intf.gui.GUI_SpriteSheet.GetSourceRectangle(scenario.Image); //???


                // item.Add(thumbnail);

                //  thumbnail.SetSkinLocation(SkinState.Normal,rect);
                //  thumbnail.Texture = Interface.gui.GUISpriteSheet.Texture;              
                thumbnail.Position = new Point(itemPadding, itemPadding);
                thumbnail.Height = thumbnailWidth;
                thumbnail.Width = thumbnailWidth;
                //icon.ResizeControlToFitImage();
            }

            LCDInnerPanel descriptionPanel = new LCDInnerPanel(Interface.gui, SelectScenarioPanel.DescriptionPanelWidth, false); // lcdSurfaceOptions.Width);
            item.Add(descriptionPanel.Panel);
            descriptionPanel.Panel.X = thumbnailPanel.Panel.Right - 2; // collapse borders
            descriptionPanel.HorizontalContentPadding = horizPadding;
            descriptionPanel.VerticalContentPadding = vertPadding;

            int lineSpacing = 4;

            Label lblName = new Label(Interface.gui);
            descriptionPanel.AddContent(lblName);
            lblName.Init(Label.LabelType.LCDHeadingBlue); // LCDSmallHeadingBanner);
            lblName.Text = name; // scenario.Name.ToUpper(Config.Culture);

            Label lblScenario = new Label(Interface.gui);
            descriptionPanel.Panel.Add(lblScenario);
            descriptionPanel.AddContentSetFullWidth(lblScenario);
            lblScenario.Init(Label.LabelType.LCDNormal);
            lblScenario.Y = lblName.Bottom + lineSpacing;
            lblScenario.Text = (scenario != null ? "Scenario: " + scenario.DisplayName : "(Missing)");  //(scenario != null? "Scenario: " + scenario.Name : ""); 

            Label lblTimestamp = new Label(Interface.gui); 
            descriptionPanel.Panel.Add(lblTimestamp);
            descriptionPanel.AddContentSetFullWidth(lblTimestamp);
            lblTimestamp.Init(Label.LabelType.LCDNormal);
            lblTimestamp.Y = lblScenario.Bottom + lineSpacing;          
            lblTimestamp.Text = "Date: " + header.Timestamp.ToString(Config.Culture); 

            Label lblVersion = new Label(Interface.gui);
            descriptionPanel.Panel.Add(lblVersion);
            descriptionPanel.AddContentSetFullWidth(lblVersion);
            lblVersion.Init(Label.LabelType.LCDNormal);
            lblVersion.Y = lblTimestamp.Bottom + lineSpacing;
            lblVersion.Text = "Version: " + header.ProgramVersion.ToString();

            Label lblSize = new Label(Interface.gui);
            descriptionPanel.Panel.Add(lblSize);
            descriptionPanel.AddContentSetFullWidth(lblSize);
            lblSize.Init(Label.LabelType.LCDNormal);
            lblSize.Y = lblVersion.Bottom + lineSpacing;
            lblSize.Text = "File size: " + fileLength / 1000000 + " mb";

            

            LCDInnerPanel selectPanel = new LCDInnerPanel(Interface.gui, SelectScenarioPanel.SelectPanelWidth, false); // lcdSurfaceOptions.Width);
            item.Add(selectPanel.Panel);
            selectPanel.Panel.X = descriptionPanel.Panel.Right - 2;


            TextButton tbSelect = new TextButton(Interface.gui);
            selectPanel.Panel.Add(tbSelect);
            tbSelect.Init(TextButton.TextButtonType.LCD); //TextButton.TextButtonType.LCD);       
            tbSelect.Text = (SaveOrLoadValue == SaveOrLoad.Save ? "SAVE" : "LOAD");
            tbSelect.ToolTip = (SaveOrLoadValue == SaveOrLoad.Save ? "Saves the current game and overwrites this file." : "Loads the game.");

            tbSelect.ScaleWidthToFitText();
            tbSelect.X = selectPanel.Panel.Width - tbSelect.Width - selectPanel.HorizontalContentPadding;
            tbSelect.Y = selectPanel.Panel.Height - tbSelect.Height; // -selectPanel.VerticalContentPadding;
            tbSelect.Tag1 = fullPath;
            if (SaveOrLoadValue == SaveOrLoad.Save)
            {
                tbSelect.Click += new ClickHandler(btSave_Click);
            }
            else
            {
                tbSelect.Click += new ClickHandler(btLoad_Click);
            }

         //   tbSelect.EventArgs = new ScenarioEventArgs() { Scenario = header };
        //    tbSelect.Click += new ClickHandler(tbSelect_Click);

            TextButton tbDelete = new TextButton(Interface.gui);
            selectPanel.Panel.Add(tbDelete);
            tbDelete.Init(TextButton.TextButtonType.LCD); //TextButton.TextButtonType.LCD);       
            tbDelete.Text = "DELETE";
            tbDelete.ToolTip = "Deletes this saved game file";
            tbDelete.ScaleWidthToFitText();
            tbDelete.X = 0;
            tbDelete.Y = tbSelect.Y; 
            tbDelete.Tag1 = fullPath;
            tbDelete.Click += new ClickHandler(tbDelete_Click);

            grid.AddEntry(header, item);

        }

        void tbDelete_Click(UIComponent sender, EventArgs e)
        {
            string savegamePath = (string)sender.Tag1;

            try
            {
                System.IO.File.Delete(savegamePath);               
            }
            catch (Exception ex)
            {
                output.ShowError("Could not delete the file. Message: " + ex.Message);
            }

            PopulateFileList();
        }

       
        
        public override void ShowDialog(bool modal)
        {
            base.ShowDialog(modal);

            SelectedSaveGamePath = null;

       /*     Form.Position = new Point((int)(The.MapUI.mapWindowWidth / 2f + Form.Width * (SaveOrLoadValue == SaveOrLoad.Save ? .1f : -1.1f)),
                The.MapUI.mapWindowHeight / 2 - Form.Height / 2);
            */

            PopulateFileList();

           // grid.PublicSetSelectionNoEvent(persistentSaveFileName); //revive the previously selected item (LoadGameFilePath), if any

        }


        // FILE PICKING //
       /* void OnSelectionChanged(UIComponent sender)
        {
            Grid clickedGrid = sender as Grid;

            if (clickedGrid != null)
            {
                persistentSaveFileName = GetSaveFileNameFromGridSelection();
                if (tbFileName != null)
                    tbFileName.Text = persistentSaveFileName;
            }
        }*/

       /* string GetSaveFileNameFromGridSelection()
        {
            object key;
            if (grid.GetSelectedKey(out key))
                return (key as string) ?? "";

            return "";
        }*/

        public static List<string> GetListOfSavedGamePaths()
        {
            string folderPath = Config.GetDataFolderPath(Config.DataType.SaveGames);

            if (!Directory.Exists(folderPath))
            {
                DirectoryInfo directory = Directory.CreateDirectory(folderPath);
            }

            List<string> listOfSavedGames = new List<string>();
    
           
            string[] mapDataFiles = System.IO.Directory.GetFiles(folderPath, "*.*");

            foreach (string fullPath in mapDataFiles)
            {               
                listOfSavedGames.Add(fullPath);
            }

            return listOfSavedGames;
        }


        private List<Tuple<string, long, SnapshotHeader>> GetSnapshotHeaders(List<string> savegamePaths)
        {
            SnapshotHeader header;
            List<Tuple<string, long, SnapshotHeader>> headers = new List<Tuple<string, long, SnapshotHeader>>();

            foreach (var path in savegamePaths)
            {
                FileInfo fileInfo = new FileInfo(path);
                long fileLength = fileInfo.Length;

                // read in the save header with the name of the scenario etc.

                FileStream stream = File.Open(path, FileMode.Open);
                GZipStream cmp = new GZipStream(stream, CompressionMode.Decompress);
                BufferedStream buffStrm = new BufferedStream(cmp, 65536);


                using (BinaryReader reader = new BinaryReader(buffStrm))
                {
                    try
                    {
                        header = The.Snapshotter.LoadHeader(reader);

                        headers.Add(new Tuple<string, long, SnapshotHeader>(path, fileLength, header));
                    }
                    catch (Exception)
                    {
                        // ignore corrupt files...
                        
                        // just make sure the flag is reset:
                        Snapshotter.IsSnapshotting = false;
                    }
                }                            
                
            }

            return headers;
        }


        private void PopulateFileList()
        {
            List<string> listOfSavedGames = GetListOfSavedGamePaths();
            List<Tuple<string, long, SnapshotHeader>> headers = GetSnapshotHeaders(listOfSavedGames);

            grid.BeginAddingEntries();
            grid.Clear();

          /*  foreach (string fileName in listOfSavedGames)
            {
                grid.AddEntry(fileName, fileName);
            }*/

            foreach (var item in headers)
            {
                AddItemRow(item.Item1, item.Item2, item.Item3);
            }

            grid.EndAddingEntries();
        }

       
      
        private void btLoad_Click(UIComponent sender, EventArgs e)
        {            
            string fullFilePath = sender.Tag1.ToString();
            Console.WriteLine(sender.Tag1.ToString());
            if (File.Exists(fullFilePath))
            {
                SelectedSaveGamePath = fullFilePath;

                if (SaveOrLoadClick != null)
                    SaveOrLoadClick.Invoke(this, null); // this will trigger a lot of code execution. don't try-catch this.


            }
            else
            {
                output.ShowError("File not found.");              
            }
       
        }
 
        private void btSave_Click(UIComponent sender, EventArgs e)
        {
          //  persistentSaveFileName = tbFileName.Text;

            string filePath = sender.Tag1.ToString();
            
         
            Save(filePath);

        }

        string saveFileFullPath = null;
        private void btSaveNew_Click(UIComponent sender, EventArgs e)
        {   
            if (string.IsNullOrEmpty(tbFileName.Text))
            {
                output.ShowError("Please enter a name for the new save file.");
                return;
            }

            string folderPath = Config.GetDataFolderPath(Config.DataType.SaveGames);

            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            saveFileFullPath = null;

            try
            {
                saveFileFullPath = Config.GetDataFolderPath(Config.DataType.SaveGames, folderPath, tbFileName.Text);
                saveFileFullPath += ".sav";
            }
            catch (Exception ex)
            {
                // this will catch illegal characters in the file name exception
                output.ShowError("Error occurred: " + ex.Message);

                return;
            }

            // see if the file already exists:
            if (System.IO.File.Exists(saveFileFullPath))
            {
                HandleFileAlreadyExists();
            }
            else
            {
                Save(saveFileFullPath);
            }

        }

        private void HandleFileAlreadyExists()
        {
            // show a popup message to the user.

            The.InGameUI.MessageBox.ShowMessage("There is already a save file with that name. Overwrite?", "FILE EXISTS", buttonOptions: MessageBox.ButtonOptions.OKAndCancel);

            The.InGameUI.MessageBox.OKClick += new EventHandler(MessageBoxOverwriteSaveFile_OKClick);
        }

        void MessageBoxOverwriteSaveFile_OKClick(object sender, EventArgs e)
        {
            The.InGameUI.MessageBox.OKClick -= new EventHandler(MessageBoxOverwriteSaveFile_OKClick);

            Save(saveFileFullPath);
        }

        private void Save(string fullFilePath)
        {
            output.ShowMessage("Saving. Please wait"); // this will not show up...

            SelectedSaveGamePath = fullFilePath;
                     
          //  PopulateFileList();          

            Window.Hide();
   
            // we try-catch only the disk operations, not the rest of the code (it may hide bugs) RELEASE only??
            
            if (SaveOrLoadClick != null)
            {
                try 
                {                  
                    SaveOrLoadClick.Invoke(this, null); // this will trigger a lot of code execution. 

                }
                catch(UWGame.SimSide.Sim.FileOpenException ex) // handle all the strange exceptions that the file system can come up with, and display them to the user. Don't handle snapshot exceptions.
                {
                    HandleSaveException(ex);
                }
               /* catch(System.IO.IOException ex) 
                {
                    HandleSaveException(ex);
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    HandleSaveException(ex);
                }*/

            }
        }

        private void HandleSaveException(Exception ex)
        {
            Window.Show();

            output.ShowError(ex.Message);
        }

        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            Window.Hide();

            if (CancelClick != null)
                CancelClick.Invoke(this, null);
        }

        private void Load(string loadFileName)
        {
            

        }

      


    }
}
