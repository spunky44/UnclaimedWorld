using System;
using System.Collections.Generic;
using System.Text;
using WindowSystem;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
////using Microsoft.Xna.Framework.Storage;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Trees;
using GameStateManagement;
using UWGame.ClientSide.Interface;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances;
using UWGame.Control;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.ClientSide.Interface.Editor.Controls;
using UWGame.ClientSide.Interface.Editor;
using UWGame.ClientSide.Interface.Editor.MapTools;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Soil;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// paint tiles and terrain
    /// </summary>
    public class SidePanelEditorSoil : RosterPanel, IEditorPanel 
    {
        Grid outerGrid;
                
        Label lblStatusInfo1, lblStatusInfo2;

        CollapsablePanel cpSoil, cpVegetation;
        Grid grdSoil, grdVegetation; 
        
        Toolbar tools;
        
        RenderedTerrainType SelectedType;

        public SidePanelEditorSoil()
            : base(The.InGameUI.sidePanelFullHeight, true) //, 150)
        {
            
            int itemHeight = ItemHeight;

            outerGrid = CreateOuterGridForCollapsableLists(The.InGameUI.gui, lcdSurface, 150);

            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Soil", itemHeight, out cpSoil, out grdSoil);
            grdSoil.SelectedChanged += new SelectionChangedHandler(grdSoil_SelectedChanged);

            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Vegetation", itemHeight, out cpVegetation, out grdVegetation);
            grdVegetation.SelectedChanged += new SelectionChangedHandler(grdVegetation_SelectedChanged);
          

            PopulateGrid(); // only do this once...

            InitStatusContentPanel();

            // keep this fixed at the bottom:
            tools = new Toolbar(this, Toolbar.Resolution.Subtile);
            lcdSurface.Add(tools); // Window.Add(tools);
            tools.Y = outerGrid.Bottom + 12; //lcdSurface.Bottom + 12; // lcdScreen.DisplayBox.Bottom + 12;


          //  TextButton btClear;          

          /*  AddBottomButtonInSequence(null, out btClear, "CLEAR!", "Clears the contents of the tile you click on.");
            btClear.Click += new ClickHandler(btClear_Click);*/
     
        }

        public void AffectMap(List<MapTool.TileAndChange> affectedSubtiles)
        {
        }

        public void AffectMap(List<MapTool.SubTileAndChange> affectedSubtiles)
        {
            // get the selected soil/veg type

            // foreach subtile:
            // if it has a terrain, add to it
            // otherwise add to the tile's terrain with 1/9 the value

            // normalize???

            foreach (var item in affectedSubtiles)
            {
                Terrain terrain = The.Map.GetTerrain(item.SubtilePos.ToPoint());

                float valueToAdd;
                if (terrain.IsSubtileTerrain())
                {
                    valueToAdd = item.Change;
                }
                else
                {
                    valueToAdd = item.Change / 9f;
                }

                if (SelectedType is SoilComponentType)
                {
                    terrain.AddSoilComponent((SoilComponentType)SelectedType, valueToAdd);
                    terrain.NormalizeSoil();
                }
                else
                {

                    terrain.AddVegetation((LowVegetationType)SelectedType, valueToAdd);
                    terrain.NormalizeVegetation();
                }

                The.Client.Renderer.terrainSlicedMap.Redraw(MapManager.SubTileToWorldPos3(item.SubtilePos));
            }

            

        }



        void cbDrawCoords_Click(UIComponent sender, EventArgs e)
        {
          /*  if (cbDrawCoords.IsChecked)
            {
                cbDrawResources.IsChecked = false;
            }*/
        }

        void cbDrawResources_Click(UIComponent sender, EventArgs e)
        {
          /*  if (cbDrawResources.IsChecked)
            {
                cbDrawCoords.IsChecked = false;
            }*/
        }

       

        void saveDialog_SaveOrLoadClick(object sender, EventArgs e)
        {

        }

      /*  void loadDialog_SaveOrLoadClick(object sender, EventArgs e)
        {         
           // The.Sim.LoadEditorMap(loadDialog.MapDataXmlPath);

            if (The.Sim.PlaySite != null)
            {
                MapLoader mapLoader = new MapLoader(loadDialog.MapDataXmlPath);

                // TODO: this won't work because the editor site and allegiance is not destroyed and recreated.
                // so it crashes with out of bounds when going from a small map to a larger map.
                // right now, only possible to load edit from main menu

                // TODO: need to cycle this:
                bool result;
                do
                {
                    result = mapLoader.QueueLoad();
                }
                while (result == false);
               // The.Map.LoadMap(loadDialog.MapDataXmlPath);

                The.Sim.PostLoadMap();
            }
        }*/

       


        void btClear_Click(UIComponent sender, EventArgs e)
        {
            intface.InterfaceMode = InGameInterface.InterfaceState.EditorClearTile;
        }

      


        private void InitStatusContentPanel()
        {
            statusContent = The.InGameUI.StatusScreen.GetNewSurfaceContent();

            Game game = The.Sim.Controller.Game;
            GUIManager gui = The.InGameUI.gui;

            InitStatusImage();

            InitBillboardPanel();

            /*
            imStatusBackground = new Image(gui);            
            statusContent.Add(imStatusBackground);
            imStatusBackground.Position = new Point(0, 2);
            imStatusBackground.ResizeToFit();
            imStatusBackground.RenderType = RenderType.CRTAndLCD;
            imStatusBackground.Alpha = 0.7f; // 0.35f;// use for background for text!
            */

            InitStatusCRTHeader(gui, statusContent, statusTextX, out lblStatusHeading, out crtUnderline);

            /*
            lblStatusHeading = new Label(gui);
            statusContent.Add(lblStatusHeading);
            lblStatusHeading.Position = new Point(statusTextX, 14);
       //     lblStatusHeading.Text = "A. NKBELE MBUTU";
            lblStatusHeading.Init(Label.LabelType.CRTNormal); // Label.LabelType.CRTGlow);
            lblStatusHeading.Width = statusTextWidth;
            lblStatusHeading.Height = 24;
            lblStatusHeading.DebugTag = "CRTStatusHeader";
            */

            lblStatusInfo1 = new Label(gui);
            statusContent.Add(lblStatusInfo1);
            lblStatusInfo1.Position = new Point(statusTextX, 80);
            //   lblStatusInfo1.Text = "MALE";
            lblStatusInfo1.Init(Label.LabelType.CRTSmall);
            lblStatusInfo1.Width = statusTextWidth;

            lblStatusInfo2 = new Label(gui);    
            statusContent.Add(lblStatusInfo2);
            lblStatusInfo2.Position = new Point(statusTextX, 100);
            //   lblStatusInfo2.Text = "AGE: 34";
            lblStatusInfo2.Init(Label.LabelType.CRTSmall);
            lblStatusInfo2.Width = statusTextWidth;

        }

        private void RefreshStatusScreen()
        {
           /* if (SelectedType  != null)
            {
                if (SelectedEntityType.RenderableTypeMode != null
                    && SelectedEntityType.RenderableTypeMode.DefaultClientState != null
                    && SelectedEntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType != null)
                {
                    //DisplayScaledBillboardImage(statusContent, imStatusBackground, selectedEntityType, StatusScreen.HeightOfStatusImage, true);
                    ShowBillboardPanel();

                    CreateAndPlaceBillboards(intface.gui, pnBillboards, SelectedEntityType, statusBillboardPanelCenter,
                        StatusScreen.HeightOfStatusImage, true); //true);

                    SetHeaderText(SelectedEntityType.Name.ToUpper(Config.Culture));

                    // lblStatusInfo1.Text = "WORK REQ.: " + selectedEntityType.StructureType.ManSecondsOfWorkNeeded.ToString();
                }
            }*/

        }



        private void PopulateGrid()
        {
            /*outerGrid.BeginAddingEntries();
            outerGrid.Clear();

            Image icon;
            TextButton expand;
            Label label;
           
            CollapsablePanel cpCategory;
            Grid categoryGrid = null;
            UIComponent item;
            Rectangle clampedRect;*/


            // AllTerrainFeatureTypes

            outerGrid.BeginAddingEntries();
            grdSoil.BeginAddingEntries();

            foreach (var terrainType in GameData.Instance.AllSoilComponentTypes)
            {
                grdSoil.AddEntry(terrainType.Value, terrainType.Value.Name);
                
            }

            grdSoil.EndAddingEntries();

            grdVegetation.BeginAddingEntries();
            foreach (var vegType in GameData.Instance.AllLowVegetationTypes)
            {               
                grdVegetation.AddEntry(vegType.Value, vegType.Value.Name);
            }

            grdVegetation.EndAddingEntries();

            outerGrid.EndAddingEntries();
        }




        private static char[] stopChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '_' };

        void grdSoil_SelectedChanged(UIComponent sender)
        {
           // HandleUserSelectedPaintType(sender);

            object key;
            if (grdSoil.GetSelectedKey(out key))
            {
                SelectedType = (RenderedTerrainType)key;
            }

        }

        void grdVegetation_SelectedChanged(UIComponent sender)
        {
            object key;
            if (grdVegetation.GetSelectedKey(out key))
            {
                SelectedType = (RenderedTerrainType)key;
            }
        }

              
       
       
        public override void Hide()
        {
            base.Hide();
        }

        public override void Refresh()
        {

        }
    }
}
