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
using System.Diagnostics;


namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// all these file browser panels should work the same - on save/load, expose the filename in a property, then invoke an event.
    /// DO NOT put loading/saving code inside these classes!
    /// </summary>
    public class MapEditorSaveLoadPanel : Panel
    {

        public enum SaveOrLoad { Save, Load }

        private SaveOrLoad saveOrLoad;
        
        Box display; //, edges;
        LCDScreen lcdScreen;
        UIComponent lcdSurface;

        Grid grid;

        TextBox tbMapName;
     //   Label lblMessages;

        Label lblFolderPath;

        TextArea taMessages;

        public event EventHandler SaveOrLoadClick;
        public event EventHandler CancelClick;

        //public string MapDataXmlPath;

        string folderPath;

        public DirectoryInfo SelectedMapFolder;


        public MapEditorSaveLoadPanel(SaveOrLoad saveOrLoad, CommonInterface intf, Point position) :
            base(intf, saveOrLoad == SaveOrLoad.Save? "SAVE MAP": "LOAD MAP", position, 
                 new Vector2(440, 560), Level.Dialogs)
        {
            this.saveOrLoad = saveOrLoad;
                      
            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 80, new Point(MarginX, MarginTop), out display, out lcdSurface, ref lcdScreen);

           
            taMessages = new TextArea(intf.gui, ListBoxType.LCD);
            taMessages.RenderType = RenderType.CRTAndLCD;
            lcdSurface.Add(taMessages);
            taMessages.Init(Label.LabelType.LCDNormal);
            taMessages.Width = lcdSurface.Width;
            taMessages.CanGrowInHeight = false; // true;
            taMessages.ScrollBarEnabled = false;
            taMessages.Height = 42;
            DisplayPrompt();     

            lblFolderPath = new Label(intf.gui);
            lcdSurface.Add(lblFolderPath);
            lblFolderPath.Init(Label.LabelType.LCDNormal);
            lblFolderPath.Y = taMessages.Bottom;
            lblFolderPath.TooltipWidth = 300;
            lblFolderPath.TooltipExpires = false;

            /*
            lblMessages = new Label(intf.gui);
            lcdSurface.Add(lblMessages);
            lblMessages.Text = (saveOrLoad == SaveOrLoad.Save? "Select an existing map file to overwrite or enter a new name in the box below."
                : "Select a map to load."); 
            lblMessages.Init(Label.LabelType.LCDNormal);
            */

            int gridStartY = lblFolderPath.Bottom + 6;

            grid = FullLCDPanel.AddGridWithFixedItemHeights(intf.gui, lcdSurface, gridStartY); // 30);
            if (saveOrLoad == SaveOrLoad.Save)
            {
                grid.SelectedChanged += new SelectionChangedHandler(terrainGrid_SelectedChanged);
            }

            TextButton btCommit = new TextButton(Interface.gui);
            Window.Add(btCommit); // add first!!! sets defaults!
            btCommit.Init(TextButton.TextButtonType.White);     
            btCommit.Text = (saveOrLoad == SaveOrLoad.Save ? "SAVE" : "LOAD");
            btCommit.ToolTip = (saveOrLoad == SaveOrLoad.Save ? "Saves the map data." : "Loads a new map.");
            btCommit.Click += new ClickHandler(btSaveLoad_Click);
            btCommit.ScaleWidthToFitText();
            PlaceRightButtonUnderLCD(btCommit);


            TextButton btCancel = new TextButton(Interface.gui);
            Window.Add(btCancel); // add first!!! sets defaults!
            btCancel.Init(TextButton.TextButtonType.White);         
            btCancel.Text = "CANCEL";
            btCancel.ToolTip = "Cancels and closes the dialog.";
            btCancel.ScaleWidthToFitText();
            btCancel.Click += new ClickHandler(btCancel_Click);
            PlaceLeftButtonUnderLCD(btCancel);

            if (saveOrLoad == SaveOrLoad.Save)
            {
                tbMapName = new TextBox(Interface.gui);
                Window.Add(tbMapName);
                tbMapName.Position = new Point(display.X, display.Bottom + 6);
                tbMapName.Width = display.Width;
                tbMapName.Height = 27;
                tbMapName.IsEditable = true;
                tbMapName.DebugTag = "editorMapName";
            }


            AddDefaultDirt();
          
          //  PopulateFileList();
        }

        private void DisplayPrompt()
        {
            taMessages.Text = (this.saveOrLoad == SaveOrLoad.Save ? "Select an existing map file to overwrite or enter a new name in the box below."
               : "Select a map to load.");
        }

        private string ShortenPath(string path, int maxLength)
        {
            int pathLength = path.Length;

            string[] parts;
            parts = path.Split('\\');

            int startIndex = (parts.Length - 1) / 2;
            int index = startIndex;

            string output = "";
            output = string.Join("\\", parts, 0, parts.Length);

            decimal step = 0;
            int lean = 1;

            while (output.Length >= maxLength && index != 0 && index != -1)
            {
                parts[index] = "...";

                output = string.Join("\\", parts, 0, parts.Length);

                step = step + 0.5M;
                lean = lean * -1;

                index = startIndex + ((int)step * lean);
            }

            return output;
        }

        private void DisplayFolderPath(string path)
        {
            string shortenedPath = ShortenPath(path, 55);

            string text = "Path: " + shortenedPath;
           /* if (lblFolderPath.GetTextWidth(text) > lcdSurface.Width)
            {

            }*/

            lblFolderPath.Text = text;
            lblFolderPath.ToolTip = path;
        }

      //  public static List<string> GetListOfMapFolders()
        public static List<DirectoryInfo> GetListOfMapFolders()
        { 
            // under Visual studio, take the files from data instead of bin:
            // TODO: save the whole path in scenario start up params, and use it when saving!
            string baseFolderPath = GetDefaultPath();

            return GetListOfMapFolders(baseFolderPath);
        }

        public static List<DirectoryInfo> GetListOfMapFolders(string baseFolderPath)
        {
           // List<string> listOfMapFolders = new List<string>();
            List<DirectoryInfo> listOfMapFolders = new List<DirectoryInfo>();
                       

            /*
#if RELEASE
            folderPath = Config.GetDataFolderPath(Config.DataType.RGMap);
#else
            folderPath = "..//..//..//" + Config.mapsFolder; // GetDataFolderPath(Config.DataType.RGMap);
#endif*/
           
            string[] mapFolders = System.IO.Directory.GetDirectories(baseFolderPath, "*", System.IO.SearchOption.TopDirectoryOnly);

           
            foreach (string folder in mapFolders) // mapFolders)
            {
                string[] mapDataFiles = System.IO.Directory.GetFiles(folder, "MapData.xml");

                // check: does the folder contain a map file?
                if (mapDataFiles.Length > 0)
                {
                    var dir = new DirectoryInfo(folder);

                    listOfMapFolders.Add(dir);
                    // save the name of the folder only (which is also the name of the map)

                   // listOfMapFolders.Add(dir.Name);
                }

            }

            return listOfMapFolders;
        }

        private static string GetDefaultPath()
        {
            string folderPath;
            if (Debugger.IsAttached)
            {
                folderPath = "..//..//..//" + Config.mapsFolder; // GetDataFolderPath(Config.DataType.RGMap);
            }
            else
            {
                folderPath = Config.GetDataFolderPath(Config.DataType.RGMap);
            }

            folderPath = System.IO.Path.GetFullPath(folderPath);
            return folderPath;
        }

        private void PopulateFileList()
        {
          
            List<DirectoryInfo> listOfMapFolders = GetListOfMapFolders(folderPath);
           // List<string> listOfMapFolders = GetListOfMapFolders();

           

            grid.BeginAddingEntries();
            grid.Clear();

            foreach (var map in listOfMapFolders)
            {
                //grid.AddEntry(map.Name, map.Name);
                grid.AddEntry(map, map.Name);
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


       
        void btSaveLoad_Click(UIComponent sender, EventArgs e)
        {            
            if (saveOrLoad == SaveOrLoad.Save)
            {                
                try
                {
                    string fullFolderPath = System.IO.Path.Combine(folderPath, tbMapName.Text);
                    string mapDataXmlPath = MapManager.ComposeMapDataXmlFilePathFromFolderPath(fullFolderPath);  //.ComposeMapDataXmlFilePath(tbMapName.Text);

                    if (System.IO.File.Exists(mapDataXmlPath))
                    {
                        MapData mapData = MapLoader.LoadMapData(mapDataXmlPath, tbMapName.Text);

                        The.Map.SaveMap(fullFolderPath, mapData, false);
                    }
                    else
                    {                      
                        //create a new map data object...
                        MapData mapData = new MapData();
                    
                        mapData.Name = tbMapName.Text;
                        mapData.FolderName = tbMapName.Text;

                        The.Map.SaveMap(fullFolderPath, mapData, true);
                    }

                    taMessages.Text = "The map was saved.";

                    PopulateFileList();
                }
                catch (Exception ex)
                {
                    taMessages.Text = "An error occurred: " + ex.Message;
                }
            }
            else
            {                
                object key;
                if (grid.GetSelectedKey(out key))
                {
                    SelectedMapFolder = (DirectoryInfo)key;
                    //MapDataXmlPath = key.ToString(); 

                }
                else
                {
                    SelectedMapFolder = null;
                    //MapDataXmlPath = null;
                }

            }

            if (SaveOrLoadClick != null)
            {
                SaveOrLoadClick.Invoke(sender, e);
            }
        }

       

        public override void ShowDialog(bool modal)
        {
            base.ShowDialog(modal);

            DisplayPrompt(); // overwrite any error msg

            folderPath = GetDefaultPath();

            DisplayFolderPath(folderPath);


            PopulateFileList();
        }

        

        void btCancel_Click(UIComponent sender, EventArgs e)
        {
          //  Form.Hide();

            Hide();

            if (CancelClick != null)
            {
                CancelClick.Invoke(sender, e);
            }
        }


        void terrainGrid_SelectedChanged(UIComponent sender)
        {
          //  selectedEntityType = null;

            Grid clickedGrid = sender as Grid;

            if (clickedGrid != null)
            {
                object key;
                if (clickedGrid.GetSelectedKey(out key))
                {
                    string name = ((DirectoryInfo)key).Name; // System.IO.Path.GetDirectoryName((string)key);
                    tbMapName.Text = name; // key.ToString();
                }
            }

        }


    }
}
