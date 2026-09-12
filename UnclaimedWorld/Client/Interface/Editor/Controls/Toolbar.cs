using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.ClientSide.Interface.Editor.MapTools;
using UWGame.SimSide.Maps;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Editor.Controls
{
    /// <summary>
    /// can this be shared between resources, entities, soil/vegetation?
    /// 
    /// controls with tool options: sharpness, shape - belong in this class too?
    /// init with the available options
    /// Resources: min/max and factor
    /// 
    /// save brushes...
    /// 
    /// decouple, so map operations give a callback to the parent...
    /// </summary>
    public class Toolbar : UIComponent
    {
        public enum PaintTools { None, Pencil, Brush, Eraser }

        Paintbrush paintBrush = new Paintbrush();
        Pencil pencil = new Pencil();
        Eraser eraser = new Eraser();

        HorizontalList hzButtons;

        IEditorPanel parent;

      //  PaintTools selectedPaintTool;

        MapTool selectedTool;

        public enum Resolution { Subtile, Tile }

        Resolution resolution;

        #region Options

        RadiusOption radiusOption;
        AlphaOption alphaOption;

        #endregion


        Grid grdOptions;

        public Toolbar(IEditorPanel parent, Resolution resolution)
            : base(((RosterPanel)parent).Window.guiManager)
        {
            this.resolution = resolution;
            GUIManager gui = ((RosterPanel)parent).Window.guiManager;

            base.Width = 224;
           
            this.parent = parent;

            hzButtons = new HorizontalList(guiManager);          
            Add(hzButtons);           
            hzButtons.X = 6;
            hzButtons.Height = 36;
           // CenterChildVertically(hzNotAttainable);

            RadioGroup rg = new RadioGroup(gui);
            rg.NewMemberChecked += rgNewToolSelected;
            rg.UnChecked += rg_UnChecked;

            ImageButton bt;
            
            bt = new ImageButton(gui);
            bt.InitWithIcon(ImageButtonType.LCD, "basic_icon_category", true); // Init(ImageButtonType.GraphButton);
            hzButtons.AddEntry(pencil /* PaintTools.Pencil*/, bt);
         //   bt.Click += bt_PencilClick;
            rg.Add(bt, false);
            bt.ToolTip = "Pencil. This tool draws with a sharp edge.";


            bt = new ImageButton(gui);
            bt.InitWithIcon(ImageButtonType.LCD, "basic_icon_category", true); //.Init(ImageButtonType.GraphButton);
            hzButtons.AddEntry(paintBrush /* PaintTools.Brush*/, bt);
           // bt.Click += btBrush_Click;
            rg.Add(bt, false);
            bt.ToolTip = "Paintbrush. This tool draws with a soft edge.";

            bt = new ImageButton(gui);
            bt.InitWithIcon(ImageButtonType.LCD, "basic_icon_category", true); //Init(ImageButtonType.GraphButton);
            hzButtons.AddEntry(eraser /* PaintTools.Eraser*/, bt);
            // bt.Click += btBrush_Click;
            rg.Add(bt, false);
            bt.ToolTip = "Eraser. This tool removes/erases.";



            // add tool options to a grid
           // grdOptions = RosterPanel.CreateOuterGridForCollapsableLists(The.InGameUI.gui, this);

            // don't include a scrollbar here...
            grdOptions = new Grid(gui, ListBoxType.LCD /* ListBoxType.Main*/, Label.LabelType.CRTBigGlow);
            grdOptions.FixedItemHeights = false;
            grdOptions.RenderType = RenderType.CRTAndLCD;
            grdOptions.CanGrowInHeight = true;
            Add(grdOptions);
            grdOptions.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grdOptions.Width = Width;
           // outerGrid.Height = lcdSurface.Height - gridTopMargin - bottomMargin;         
            grdOptions.Position = new Point(0, hzButtons.Bottom + 6);

            int ypos = hzButtons.Bottom + 6;

            radiusOption = new RadiusOption(gui);
            alphaOption = new AlphaOption(gui);


            base.Height = grdOptions.Bottom;



          //  The.MapUI.LeftMouseReleasedInMap += MapUI_LeftMouseReleasedInMap;
          //  The.MapUI.LeftMouseWorldPosDragInMap += MapUI_LeftMouseTileDragInMap;
            The.MapUI.LeftMouseDownInMap += MapUI_LeftMouseDownInMap;
            The.MapUI.LeftMouseReleasedInMap += MapUI_LeftMouseReleasedInMap;

            if (resolution == Resolution.Subtile)
            {
                The.MapUI.LeftMouseSubtileDragInMap += MapUI_LeftMouseSubtileDragInMap;
            }
            else
            {
                The.MapUI.LeftMouseTileDragInMap += MapUI_LeftMouseTileDragInMap;
            }
        }

        void MapUI_LeftMouseReleasedInMap(Vector3 obj)
        {
            The.MapUI.ResetDragging();
        }

      //  UIComponent sizeOption;


      /*  private UIComponent CreateToolOptionRow()
        {
            UIComponent row = new UIComponent(this.guiManager);
            row.Height = 26;
            row.Width = Width;

            return row;
        }*/


        void MapUI_LeftMouseTileDragInMap(TilePos obj)
        {
            UseTool(obj);
        }

        void MapUI_LeftMouseSubtileDragInMap(SubtilePos obj)
        {
            UseTool(obj);
        }

        void MapUI_LeftMouseDownInMap(Vector3 location)
        {
            if (resolution == Resolution.Subtile)
            {
                UseTool(MapManager.WorldPosToSubtilePos(location));
            }
            else
            {
                UseTool(MapManager.WorldPosToTilePos(location));
            }
        }

        void rg_UnChecked(EventArgs obj)
        {
            The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.None;
        }


        private void UseTool(TilePos tilePos)
        {
            // draw on tiles only
            // get affected tiles from tool size
            // compute the change for each
            // send to parent
            if (selectedTool != null)
            {
                var affected = selectedTool.GetAffectedTiles(tilePos);
                parent.AffectMap(affected);

            }

            /*
            switch (selectedPaintTool)
            {
                case PaintTools.Brush:
                    PaintBrushDrag();

                    break;
            }*/

        }

        private void UseTool(SubtilePos subtilePos)
        {
            // draw on tiles only
            // get affected tiles from tool size
            // compute the change for each
            // send to parent
            if (selectedTool != null)
            {
                var affected = selectedTool.GetAffectedSubtiles(subtilePos);
                parent.AffectMap(affected);

            }

            /*
            switch (selectedPaintTool)
            {
                case PaintTools.Brush:
                    PaintBrushDrag();

                    break;
            }*/

        }

      
        void rgNewToolSelected(ICanBeChecked arg1, EventArgs arg2)
        {
            // when selecting a tool, show the options
            // also remove the zone overlay.
            // also select a default tool when a soil type is selected

            // pull or push option settings???

            // Radius
            // Alpha

            ImageButton bt = arg1 as ImageButton;
            //  PaintTools tool = (PaintTools)bt.Tag1;
            MapTool tool = (MapTool)bt.Tag1;

            selectedTool = tool;

            grdOptions.BeginAddingEntries();

            // get the options and their settings from the tool:
            IHasRadiusOption hasRadius = tool as IHasRadiusOption;
            if (hasRadius != null)
            {
                radiusOption.Set(hasRadius.RadiusSetting);
            }
            AddOrRemoveOption(hasRadius, radiusOption);

            IHasAlphaOption hasAlpha = tool as IHasAlphaOption;
            if (hasAlpha != null)
            {
                alphaOption.Set(hasAlpha.AlphaSetting);
            }
            AddOrRemoveOption(hasAlpha, alphaOption);


            grdOptions.Sort(u => ((ToolOption)u).Order, Grid.Sorting.Ascending);

            grdOptions.EndAddingEntries();
            
                      
            //  selectedPaintTool = tool;

            The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.EditorTool;

            Height = grdOptions.Bottom;
        }


        private void AddOrRemoveOption(object hasOption, ToolOption option)
        {
            if (hasOption != null)
            {
                if (!grdOptions.EntriesByKey.ContainsKey(option))
                {
                    grdOptions.AddEntry(option, option);
                }
            }
            else
            {
                grdOptions.RemoveEntry(option);
            }
        }

      

    }
}
