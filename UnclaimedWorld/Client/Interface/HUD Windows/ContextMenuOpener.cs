using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;
using InputEventSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    /// <summary>
    /// this is a separate window from Context Menu to get smooth fading when showing/hiding it
    /// </summary>
    public class ContextMenuOpener : HUDWindow
    {
      //  private const int collapsedHeight = 16;
       
        ImageButton btCycle; //, btModify;
        TextButton btExpand;

        UIComponent pnZones;
        Grid grdZones;
        
       // const int yDistanceToMenu = 6;

        public int CycleButtonXPos
        {
            get { return btCycle.X; }

        }

        public ContextMenuOpener()
            : base(96, 60, true, level: Level.RockBottom) // Level.Bottom)
        {
          
            DisplayWindow.DebugTag = "contextMenuOpener";

            this.btExpand = new TextButton(gui);
            Add(btExpand);
            btExpand.Text = "NEW";
            btExpand.Init(TextButton.TextButtonType.HUD); // ImageButtonType.HUDArrowRight);
            btExpand.MouseOver += new MouseOverHandler(bt_MouseOver);
            btExpand.ScaleWidthToFitText();

            btExpand.Y = topMargin;
            btExpand.X = sideMargin; // base.DisplayWindow.Width - btExpand.Width - sideMargin;
            btExpand.DebugTag = "context";
            btExpand.ZOrder = 1f;

            TileSelectionContextMenu.CreateCycleButton(ref btCycle, gui, btExpand.Right + singleSpacing); // base.DisplayWindow.Width - sideMargin - btExpand.Width);
            Add(btCycle);
            btCycle.X = btExpand.Right + singleSpacing;
            btCycle.Click += new ClickHandler(btCycle_Click);
         /*   Add(btModify);
            btModify.X = btCycle.Right + singleSpacing;
            btModify.Click += new ClickHandler(btModify_Click);
            */

          //  base.DisplayWindow.Height = btModify.Bottom + topMargin;
          //  base.DisplayWindow.Width = btModify.Right + sideMargin;

            base.DisplayWindow.Height = btCycle.Bottom + topMargin;
            base.DisplayWindow.Width = btCycle.Right + sideMargin;

           // CreateZoneList();


        }

        /*
        private void CreateZoneList()
        {
            pnZones = new UIComponent(gui);
            pnZones.Width = DisplayWindow.Width;

            pnZones.Y = btModify.Y + btModify.Height + singleSpacing;
            pnZones.HeightResize += new ResizeHandler(pnZones_HeightResize);


            Label lbl = new Label(gui);
            pnZones.Add(lbl);
            lbl.Init(Label.LabelType.HUDWindow);
            lbl.SetText("Zones:");
            lbl.X = sideMargin;



            grdZones = new Grid(gui, ListBoxType.Main, WindowSystem.Label.LabelType.HUDWindow);
            pnZones.Add(grdZones);
            grdZones.FixedItemHeights = true;
            grdZones.Width = pnZones.Width - lbl.Width;
            grdZones.X = lbl.X + lbl.Width + 2;
            grdZones.ScrollBarEnabled = false; // true; // ??
            grdZones.ItemHeight = 14;
            grdZones.CanGrowInHeight = true;
            grdZones.Font = GUIManager.LCDandHUDFontPath;
            // make sure that the panel can expand around the grid!
            grdZones.HeightResize += new ResizeHandler(grdSkills_HeightResize);
        }*/

        void pnZones_HeightResize(UIComponent sender)
        {
            base.DisplayWindow.Height = pnZones.Y + pnZones.Height;
        }

        void grdSkills_HeightResize(UIComponent sender)
        {
            pnZones.Height = grdZones.Y + grdZones.Height + 10; 
        }


        void btModify_Click(UIComponent sender, EventArgs e)
        {
            
        }

        void btCycle_Click(UIComponent sender, EventArgs e)
        {

            CycleEntities(The.InGameUI.SelectedTiles);

        }


        //private static EntityID? selectedID = null;

        public static void CycleEntities(MapArea mapArea)
        {
            EntityID? foundEntity, selectedID = null;
            IKnownEntityData data = null;

            // cycle through entities in this order: Persons, Animals, Structures, Items
            // start cycling in the group that matches the currently selected entity, if any

            if (The.InGameUI.SelectedEntity != null)
            {
                selectedID = The.InGameUI.SelectedEntity.Value;
                The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out data);
            }
            
            //bool hasCycledPersons = false, hasCycledAnimals = false, hasCycledStructures = false, hasCycledItems = false;

            bool returnFirstMatch = false;

            bool foundPersons = false;
            bool foundAnimals = false;
            bool foundStructures = false;
            bool foundItems = false;
            
            while (true)//hasCycledPersons == false || hasCycledAnimals == false || hasCycledStructures == false || hasCycledItems == false)
            {
                if ((/*hasCycledPersons == false &&*/ data == null) ||
                    (data != null && data.EntityType.Person != null))
                {
                    foundEntity = mapArea.CycleKnownEntities(selectedID, The.InGameUI.UIAllegiance.SharedKnowledge,
                        entity => entity.EntityType.Person != null, out foundPersons, returnFirstMatch);

                    if (foundEntity != null)
                    {
                        The.InGameUI.SelectEntity(foundEntity.Value);
                        return;
                    }

                    data = null;
                    //hasCycledPersons = true;
                    returnFirstMatch = true;
                }

                if ((/*hasCycledAnimals == false &&*/ data == null) 
                    || (data != null && data.EntityType.IntelligenceType != null && data.EntityType.Person == null))
                {
                    foundEntity = mapArea.CycleKnownEntities(selectedID, The.InGameUI.UIAllegiance.SharedKnowledge,
                        entity => entity.EntityType.IntelligenceType != null && entity.EntityType.Person == null, out foundAnimals, returnFirstMatch);

                    if (foundEntity != null)
                    {
                        The.InGameUI.SelectEntity(foundEntity.Value);
                        return;
                    }

                    data = null;
                    //hasCycledAnimals = true;
                    returnFirstMatch = true;
                }

                if ((/*hasCycledStructures == false &&*/ data == null)
                    || (data != null && data.EntityType.StructureType != null))
                {
                    foundEntity = mapArea.CycleKnownEntities(selectedID, The.InGameUI.UIAllegiance.SharedKnowledge,
                        entity => entity.EntityType.StructureType != null, out foundStructures, returnFirstMatch);

                    if (foundEntity != null)
                    {
                        The.InGameUI.SelectEntity(foundEntity.Value);
                        return;
                    }

                    data = null;
                    //hasCycledStructures = true;
                    returnFirstMatch = true;
                }

                if ((/*hasCycledItems == false &&*/ data == null)
                    || (data != null && data.EntityType.ItemType != null))
                {
                    foundEntity = mapArea.CycleKnownEntities(selectedID, The.InGameUI.UIAllegiance.SharedKnowledge,
                        entity => entity.EntityType.ItemType != null, out foundItems, returnFirstMatch);

                    if (foundEntity != null)
                    {
                        The.InGameUI.SelectEntity(foundEntity.Value);
                        return;
                    }

                    data = null;
                    //hasCycledItems = true;
                    returnFirstMatch = true;
                }

                // set data to null if we reached the last of the entities in the selected entity's group
                // then test the other groups

                if (foundPersons || foundAnimals || foundStructures || foundItems)
                {
                    returnFirstMatch = true;
                    selectedID = null;
                }
                else
                {
                    return;
                }
            }
        }



        void ViewPort_MouseOut(MouseEventArgs args)
        {
          //  DisplayWindow.Hide();
            /*
            if (!The.InGameUI.ContextMenu.DisplayWindow.CheckCoordinates(args.Position.X, args.Position.Y))
            {
                Hide();
            }*/
        }

        void bt_MouseOver(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
          //  The.InGameUI.ShowContextMenu(DisplayWindow.X + DisplayWindow.Width + 2, DisplayWindow.Y);
            if (btExpand.Enabled)
            {
                The.InGameUI.ShowContextMenu(DisplayWindow.X, DisplayWindow.Y, this.DisplayWindow);
            }
        }


        public void Populate()
        {
            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();

            // if the area is blocked disable the expand button:
            if (mapArea.Count == 0)
            {
                btExpand.Enabled = false;

                //btExpand.MouseOver -= new MouseOverHandler(bt_MouseOver);
            }
            else
            {
                btExpand.Enabled = true;

               // btExpand.MouseOver += new MouseOverHandler(bt_MouseOver);
            }

        }

        

        private void PopulateZoneList()
        {
       /*     List<Zone> allZonesInArea = new List<Zone>();
            The.InGameUI.SelectedTiles.IterateArea(tile =>
            {
                List<Zone> listOfZones = tile.GetListOfZones(The.InGameUI.UIAllegiance);
                if (listOfZones != null)
                {
                    allZonesInArea.AddRange(listOfZones);
                }
            });

            allZonesInArea = allZonesInArea.Distinct().ToList();

            if (allZonesInArea.Count > 0)
            {
                Add(pnZones);

                grdZones.BeginAddingEntries();
                grdZones.Clear();

                //   UIComponent gridItem;
                foreach (var zone in allZonesInArea)
                {
                    //gridItem = new UIComponent(gui);

                    // grdZones.AddEntry(zone.ID, gridItem);

                    grdZones.AddHyperLinkEntry(zone.ID, zone.Name ?? ("#" + zone.ID.ToString()), zone);
                }

                grdZones.EndAddingEntries();

                // base.DisplayWindow.Height = pnZones.Y + pnZones.Height + bottomMargin;

            }
            else
            {
                Remove(pnZones);

                base.DisplayWindow.Height = btModify.Y + btModify.Height;
            }*/
        }


       /* public override void SetPosition(Microsoft.Xna.Framework.Point newPos)
        {
            base.SetPosition(newPos);


            ContextMenu.SetPosition(newPos);
        }*/

        public override void Hide()
        {
            //base.DisplayWindow.Height = collapsedHeight;

            base.Hide();
            //DisplayWindow.Hide();
                        
            The.InGameUI.ContextMenu.Hide();

        }

       
    }
}
