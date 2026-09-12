using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.Control.Commands;
using UWGame.SimSide;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Policies;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Policy
{
    public class WeaponsPage: TabPagePanel
    {

        Grid grid;

     //   TextButton btOK;

        Expedition expedition;


        const int allowVerminX = 300;
        const int itemTypeIconColumnX = 10;
        const int typeX = 22;
        const int bulletsX = 200;

        int gridHeaderY = 40;

        public WeaponsPage(TabControl parent)
            : base(parent)
        {
            GUIManager gui = parent.guiManager;

            parent.AddTabPage(this, "WEAPONS POLICY", "Set/view weapon policies");

            Label lblAmmoUsageTitle = new Label(guiManager);
            lblAmmoUsageTitle.Init(Label.LabelType.LCDBigHeaderBanner);
            Add(lblAmmoUsageTitle);
            lblAmmoUsageTitle.X = 0;
            lblAmmoUsageTitle.Y = 0;
            lblAmmoUsageTitle.Text = "AMMO USAGE";
            lblAmmoUsageTitle.ToolTip = "Specify what the ammunition may be used for";
            lblAmmoUsageTitle.Width = Width;

            grid = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.HUDWindow);
            grid.IsOuterGrid = true; // false;

            // categoryGrid.Position = new Point(
            // categoryGrid.DebugTag = "categoryGrid";
            grid.X = 0;
            grid.Y = gridHeaderY + 26;
            grid.FixedItemHeights = true; // false;
            grid.Width = this.Width;
            grid.ScrollBarEnabled = true;
            grid.ItemHeight = 27; // 22;
            grid.CanGrowInHeight = true; // true; // false; // true;            
            grid.Font = GUIManager.LCDandHUDBodyFontPath;
           // grid.Height = 136; // 40; // 160
            Add(grid);


            CreateGridHeader();

          /*  btOK = new TextButton(gui);
            Add(btOK);
            btOK.Text = "OK";
            btOK.Init(TextButton.TextButtonType.HUD);
            btOK.Click += btOK_Click;
            btOK.Width = 72;
            // btOK.Y = DisplayWindow.Height - btOK.Height - doubleSpacing;
            btOK.X = 100;
            */

            SetHeight();
        }

        private void SetHeight()
        {
           /* btOK.Y = grid.Bottom + 12;

            Height = btOK.Bottom + 12;*/

            Height = grid.Bottom + 6;
        }

        string availableTooltip = "The amount of ammunition available";
        string useVerminTooltip = "Select whether we allow this ammunition type to be used against vermin, or if it can only be used against threats and other targets.";

        private void CreateGridHeader()
        {
            /*  Label lblHeader = new Label(gui);
              Add(lblHeader);
              lblHeader.Init(Label.LabelType.HUDWindow);
              lblHeader.Text = "Resource"; 
              lblHeader.FitToText();
              lblHeader.X = resourceTextX;
              lblHeader.Y = gridHeaderY;
              */

            /*   lblHeader = new Label(gui);
               Add(lblHeader);
               lblHeader.Init(Label.LabelType.HUDWindow);
               lblHeader.Text = "In stock";
               lblHeader.FitToText();
               lblHeader.X = inStockX;
               lblHeader.Y = gridHeaderY;
               */
            Label lblType = new Label(guiManager);
            Add(lblType);
            lblType.Init(Label.LabelType.LCDHeadingBrown);
            lblType.Text = "Type";
            lblType.ToolTip = "The ammunition type";
            lblType.FitToText();
            lblType.X = typeX;
            lblType.Y = gridHeaderY;


            Label lblHeader = new Label(guiManager);
            Add(lblHeader);
            lblHeader.Init(Label.LabelType.LCDHeadingBlue);
            lblHeader.Text = "Owned";
            lblHeader.ToolTip = availableTooltip;
            lblHeader.FitToText();
            lblHeader.X = bulletsX;
            lblHeader.Y = gridHeaderY;

            Label lblUseAgainstVermin = new Label(guiManager);
            Add(lblUseAgainstVermin);
            lblUseAgainstVermin.Init(Label.LabelType.LCDHeadingRed);
            lblUseAgainstVermin.ToolTip = useVerminTooltip;
            lblUseAgainstVermin.Text = "Use against vermin";
            lblUseAgainstVermin.FitToText();
            lblUseAgainstVermin.X = allowVerminX;
            lblUseAgainstVermin.Y = gridHeaderY;
        }

        void btOK_Click(UIComponent sender, EventArgs e)
        {           
           
            foreach (var row in grid.EntriesByKey)
            {
                CheckBox cb;
                row.Value.FindChildById(UIComponent.DataControlID.Selector, out cb);               

                EntityType entityType = (EntityType)row.Key;

                SetUseAgainstVermin(expedition, cb.IsChecked, entityType);
            }

        }

        private static void SetUseAgainstVermin(Expedition expedition, bool newValue, EntityType entityType)
        {
            ExpeditionPolicy policy = expedition.Policy;

            bool oldOrder = policy.GetAllowAmmoForVermin(entityType);

            if (oldOrder != newValue)
            {
                Command command;

                command = new AllowAmmoForVermin(expedition.ID, entityType, newValue, true);

                The.Client.Controller.StoreAndExecuteCommand(command);
            }
        }

      

        private UIComponent AddRow(EntityType entityType, OwnerAmmoOfType ownedAmmo, EntityGroup owner)
        {

            // EventArgs eventArgs = new HarvestJobsButtonEventArgs(entityType, resourcesAndJobs);

            //GuiManager gui = guima

            UIComponent item = new UIComponent(guiManager);
            grid.AddEntry(entityType, item);

            item.OrderByTag1 = entityType.PluralName;
            
            Image icon = InventoryPanel.AddEntityTypeIcon(entityType, item, itemTypeIconColumnX);

            DataTypeButton tbCaption = new DataTypeButton(guiManager, DataSheet.InfoToShow.Data, entityType, owner.ID, false);
            tbCaption.Init(TextButton.TextButtonType.LCDToolTipBlack);
            tbCaption.ID = UIComponent.DataControlID.Caption;
            tbCaption.IsRoot = true;
            tbCaption.Text = entityType.PluralName;
            item.Add(tbCaption);
            tbCaption.TextAlignment = TextButton.TextAlign.Left;
            tbCaption.Width = 160; // 105; 
            tbCaption.X = typeX;
            tbCaption.DebugTag = "entityTypeButton";

           // int produceColumnX = orderedX - 11;

            Label lblBullets = new Label(guiManager);
            lblBullets.Init(Label.LabelType.LCDNormal);
            item.Add(lblBullets);
            lblBullets.X = bulletsX;
            lblBullets.ID = DataControlID.Available;
            lblBullets.ToolTip = availableTooltip;
            lblBullets.Text = "0";
            lblBullets.FitToText();
            item.CenterChildVertically(lblBullets);

           /* StockButton btStockAvailable = new StockButton(guiManager, entityType);
            //  tbAvailableItems.ID = UIComponent.DataControlID.Available;
            Add(btStockAvailable);
            btStockAvailable.Position = new Point(0, 0);
            btStockAvailable.Click += new ClickHandler(tbItems_Click);
            CenterChildVertically(btStockAvailable);
            btStockAvailable.ID = DataControlID.Available;
           
            */

            CheckBox cbAllowVermin = new CheckBox(guiManager);
            item.Add(cbAllowVermin);
            cbAllowVermin.Init(CheckBoxType.LCDNoLabel);
            cbAllowVermin.SwitchStateOnClick = true;
            cbAllowVermin.X = allowVerminX;
            cbAllowVermin.ID = DataControlID.Selector;
            cbAllowVermin.ToolTip = useVerminTooltip; 
            cbAllowVermin.Click += cbAllowVermin_Click;
            cbAllowVermin.Tag1 = entityType;

            item.OrderByTag2 = entityType.PluralName; //We want to order the items by name


            return item;

        }

        void cbAllowVermin_Click(UIComponent sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            EntityType entityType = (EntityType)cb.Tag1;

            SetUseAgainstVermin(expedition, cb.IsChecked, entityType);
        }

        public override void Refresh()
        {
            expedition = The.InGameUI.GetExpedition();

            Populate();
        }

        /// <summary>
        /// grid without categories
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="outerGrid"></param>
        /// <param name="setCollapsedSummary"></param>
        /// <param name="clickHandler"></param>
        /// <param name="dictionary"></param>
        private void Populate()
        {
            UIComponent item = null;


            EntityGroup owner = expedition.OwnedEntities; // .UIExpedition.owner mapArea.GetOwner();

            if (owner == null)
            {
                // oops, what should happen here..? clear the grid?
                grid.Clear();
                return;
            }


            var data = GetAmmoToDisplay(owner);

            int currentNoOfCategories = grid.Entries.Count;

            grid.BeginAddingEntries();


            foreach (var ammo in data) //  var entityType in The.InGameUI.UIAllegiance.RepresentativeEntityType.IntelligenceType.PreyTypes)
            {
                if (!grid.TryGetEntry(ammo.Key, out item))
                {
                    item = AddRow(ammo.Key, ammo.Value, owner);
                }

                UpdateRow(item, ammo.Key, ammo.Value, owner);
            }
           

            // grid.DeleteEntries<EntityType>(e => e.CanBeHuntedBy(The.InGameUI.UIAllegiance));

            grid.DeleteEntries<EntityType>(e => data.Any(t => t.Key == e));
            grid.Sort(i => i.OrderByTag1, Grid.Sorting.Ascending);

            grid.EndAddingEntries();

            SetHeight();
        }

        /// <summary>
        /// get ammo that we either own or have a setting for.
        /// Ammo that we once owned bu have since run out of should also be included
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        private Dictionary<EntityType, OwnerAmmoOfType> GetAmmoToDisplay(EntityGroup owner)
        {
            //List<EntityType> result; // = new List<Tuple<EntityType, bool>>();

            return owner.AmmoItems;

            /*
            foreach (var item in owner.AmmoItems)
            {
                if (habitats == null || !habitats.Contains(item))
                {
                    result.Add(new Tuple<EntityType, bool>(item, false));
                }
            }

            return result;*/
        }

        private void UpdateRow(UIComponent item, EntityType entityType, OwnerAmmoOfType ownedAmmo, EntityGroup owner)
        {
                       

            List<EntityID> listOfEntities;
            List<EntityID> listOfAvailableEntities;
            List<EntityID> listOfUnavailableEntities;


            int noOfIncompleteItems, noOfEntitiesUsedAsParts, noOfItemsOnOtherSite, noOfItemsOwnedByOthers, noOfAvailableItemsIncludingIntrinsic;
            int noOfAvailableItems = InventoryPanel.GetNoOfAvailableEntities(owner.AllEntities, owner, entityType, out noOfIncompleteItems,
                                                                             out noOfEntitiesUsedAsParts, out noOfItemsOnOtherSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic,
                                                                             out listOfEntities, out listOfAvailableEntities, out listOfUnavailableEntities, null); // allAvailableItems);

            DataTypeButton tbCaption = (DataTypeButton)item.FindChildById(UIComponent.DataControlID.Caption);

            tbCaption.SetAvailableStatusColor(noOfAvailableItems > 0);

            Label lblBullets;
            item.FindChildById(UIComponent.DataControlID.Available, out lblBullets);

            lblBullets.Text = ownedAmmo.TotalRounds.ToString();
            lblBullets.FitToText();

            CheckBox cbAllowVermin;
            item.FindChildById(DataControlID.Selector, out cbAllowVermin);
            cbAllowVermin.IsChecked = expedition.Policy.GetAllowAmmoForVermin(entityType);

          /*  StockButton btStockAvailable;
            item.FindChildById(UIComponent.DataControlID.Available, out btStockAvailable);


            btStockAvailable.UpdateStockButton(noOfAvailableItems, null, null, null, null, listOfEntities, false);
            */

        }
    }
}
