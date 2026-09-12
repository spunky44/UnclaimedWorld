using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using UWGame.Control.Commands;
using UWGame.SimSide.Commands;
using UWGame.SimSide;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Snapshots;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Entities.Containers;
using UWGame.ClientSide.Interface.Controls;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    /// <summary>
    /// has buttons for the actions that can be taken when the entity or expedition is selected.
    /// Perhaps this could be merged with TileSelectionContextMenu
    /// </summary>
    public class HUDEntityContextMenu : HUDWindow
    {
        TextButton tbSalvage, tbPackingDown, tbHunt, tbMoveExpedition, tbSetStockpile, tbSetTradeOffers, tbUpgrade, tbDiscard, tbClaim;

        DataTypeButton btSalvageProcess;


        Grid grdSpecialActions;

        /// <summary>
        /// the context (item that was activated) can be an entity or an expedition: 
        /// </summary>
        EntityID? entityID;
        Expedition expedition;

        #region Child windows

        StockpileWindow stockpileWindow;

        public UpgradeWindow UpgradeWindow;

        #endregion


        const string stockpileTooltip = "Choose the types of items that can be stored in this structure";
        const string stockpileBrokenTooltip = "The structure is broken and is unusable for stockpiling.";

        const string offeredForTradeTooltip = "Choose the types of items that can be offered for trade in this structure";

        const string upgradeTooltip = "Choose improvements for the structure";


        public HUDEntityContextMenu()
            : base(240 /* 204*/, 220, true, false, false, "HUD_window_base", false) // autohide seems buggy! stay open // true)
        {
            tbSalvage = new TextButton(gui);
            tbSalvage.Text = "BEGIN"; // "SALVAGE";
            tbSalvage.ToolTip = "Salvage the object: When breaking this apart, some parts will be retrieved, some will be lost. See the process tooltip for more info.";
            tbSalvage.Init(TextButton.TextButtonType.HUDSalvage);
            tbSalvage.Click += new ClickHandler(tbSalvage_Click);
            tbSalvage.ScaleWidthToFitText();

            tbPackingDown = new TextButton(gui);
            tbPackingDown.Text = "BEGIN"; // "SALVAGE";
            tbPackingDown.ToolTip = "Disassemble the object: All its parts will be retrieved. See the process tooltip for more info.";
            tbPackingDown.Init(TextButton.TextButtonType.HUDPackingDown);
            tbPackingDown.Click += new ClickHandler(tbSalvage_Click);
            tbPackingDown.ScaleWidthToFitText();


           // btSalvageProcess = new DataTypeButton(gui, DataTypeTooltip.InfoToShow.Production, , , true);


            tbHunt = new TextButton(gui);
            tbHunt.Text = "HUNT";
            tbHunt.ToolTip = "Hunt this animal";
            tbHunt.Init(TextButton.TextButtonType.HUDHunt);
            tbHunt.Click += new ClickHandler(tbHunt_Click);
            tbHunt.ScaleWidthToFitText();

            tbSetStockpile = new TextButton(gui);
            tbSetStockpile.Text = "STOCKPILE";
            tbSetStockpile.ToolTip = stockpileTooltip;
            tbSetStockpile.Init(TextButton.TextButtonType.HUDStockpile);
            tbSetStockpile.Click += new ClickHandler(tbSetStockpile_Click);
            tbSetStockpile.ScaleWidthToFitText();

            tbSetTradeOffers = new TextButton(gui);
            tbSetTradeOffers.Text = "TRADE";
            tbSetTradeOffers.ToolTip = offeredForTradeTooltip;
            tbSetTradeOffers.Init(TextButton.TextButtonType.HUDStockpile);
            tbSetTradeOffers.Click += tbSetTradeOffers_Click;
            tbSetTradeOffers.ScaleWidthToFitText();

            tbUpgrade = new TextButton(gui);
            tbUpgrade.Text = "UPGRADE";
            tbUpgrade.ToolTip = upgradeTooltip;
            tbUpgrade.Init(TextButton.TextButtonType.HUDUpgrade);
            tbUpgrade.Click += tbUpgrade_Click;
            tbUpgrade.ScaleWidthToFitText();


            tbClaim = new TextButton(gui);
            tbClaim.Text = "CLAIM";
            // tbSetStockpile.ToolTip = "Claim this item for the c";
            tbClaim.Init(TextButton.TextButtonType.HUDClaim);
            tbClaim.Click += new ClickHandler(tbClaim_Click);
            tbClaim.ScaleWidthToFitText();

            tbDiscard = new TextButton(gui);
            tbDiscard.Text = "DISCARD";
            tbDiscard.ToolTip = stockpileTooltip;
            tbDiscard.Init(TextButton.TextButtonType.HUDDiscard);
            tbDiscard.Click += new ClickHandler(tbDiscard_Click);
            tbDiscard.ScaleWidthToFitText();
            /*
            tbClaim = new ImageButton(gui);
            tbClaim.Init(ImageButtonType.HUDClaim);
            tbClaim.Click += new ClickHandler(tbClaim_Click);

            tbDiscard = new ImageButton(gui);
            tbDiscard.Init(ImageButtonType.HUDDiscard);
            tbDiscard.Click += new ClickHandler(tbDiscard_Click);*/




            tbMoveExpedition = new TextButton(gui);
            tbMoveExpedition.Text = "MOVE CAMP";
            tbMoveExpedition.ToolTip = "Click on terrain to designate a new spot for the camp";
            tbMoveExpedition.Init(TextButton.TextButtonType.HUDStockpile);
            tbMoveExpedition.Click += new ClickHandler(tbExpedition_Click);
            tbMoveExpedition.Y = topMargin;
            tbMoveExpedition.X = sideMargin;
            tbMoveExpedition.ScaleWidthToFitText();



            grdSpecialActions = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
            grdSpecialActions.FixedItemHeights = true;
            grdSpecialActions.RenderType = RenderType.Normal; // RenderType.CRTAndLCD;
           // Add(grdSpecialActions);
            grdSpecialActions.HMargin = 5; // !!!
            grdSpecialActions.VMargin = 5; // !!!
            grdSpecialActions.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grdSpecialActions.Width = DisplayWindow.ViewPort.Width - 2 * sideMargin; // listSurface.Width;
            // grdSpecialActions.Height = listSurface.Height;
            grdSpecialActions.ItemHeight = 26;
            grdSpecialActions.Position = new Point(0, 0);
            grdSpecialActions.CanGrowInHeight = true;
            grdSpecialActions.ScrollBarEnabled = false;

            //  CreateMenuGrid(out grdSpecialActions, tbHunt.Bottom + singleSpacing,false);



            stockpileWindow = new StockpileWindow();

            UpgradeWindow = new UpgradeWindow();
        }

        void tbUpgrade_Click(UIComponent sender, EventArgs e)
        {
            IKnownEntityData entityData;

            if (entityID.HasValue
                && !GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityData)))
            {
                HideChildWindows();

                UpgradeWindow.ShowOnPlayfield(TileSelectionContextMenu.GetXPositionOfChildWindow(DisplayWindow), base.DisplayWindow.Y);

                UpgradeWindow.Fill(entityData.EntityID, true);
            }
        }

        void tbSetTradeOffers_Click(UIComponent sender, EventArgs e)
        {
            IKnownEntityData entityData;

            if (entityID.HasValue
                && !GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityData)))
            {
                HideChildWindows();

                stockpileWindow.ShowOnPlayfield(TileSelectionContextMenu.GetXPositionOfChildWindow(DisplayWindow), base.DisplayWindow.Y);

                stockpileWindow.FillFromStructure(entityData.EntityID, Stockpile.TypesOfStockpiles.OfferedForTrade);
            }
        }

        void tbSetStockpile_Click(UIComponent sender, EventArgs e)
        {
            IKnownEntityData entityData;

            if (entityID.HasValue
                && !GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityData)))
            {
                HideChildWindows();

                stockpileWindow.ShowOnPlayfield(TileSelectionContextMenu.GetXPositionOfChildWindow(DisplayWindow), base.DisplayWindow.Y);

                stockpileWindow.FillFromStructure(entityData.EntityID, Stockpile.TypesOfStockpiles.Normal);
            }

        }

        private void HideChildWindows()
        {
            stockpileWindow.Hide();
            UpgradeWindow.Hide();
        }

        void tbClaim_Click(UIComponent sender, EventArgs e)
        {
            if (entityID.HasValue == false)
            {
                return;
            }

            IKnownEntityData entityData;
            if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityData)))
            {
                if (EntityListWindow.CanBeClaimed(entityData)) // validate!
                {

                    // claim the item for the main? expedition:
                    Expedition expedition = The.InGameUI.GetExpedition();
                    if (expedition != null)
                    {
                        Command claimCommand = new Claim(entityID.Value, The.InGameUI.UIAllegiance.ID, ((IOwner)expedition).ID, true);

                        The.Client.Controller.StoreAndExecuteCommand(claimCommand);
                    }
                }
                else
                {
                    Refresh();
                }
            }
            else
            {
                HandleInvalidEntity();
            }
        }

        private void HandleInvalidEntity()
        {
            Hide();

        }

        public void OnClaimEntity()
        {
            RefreshEntityContent();

            Hide();
        }

        void tbDiscard_Click(UIComponent sender, EventArgs e)
        {
            if (entityID.HasValue == false)
            {
                return;
            }

            IKnownEntityData entityData;
            if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityData)))
            {
                bool canBeSalvaged, canBeDiscarded;
                EntityListWindow.CanBeSalvagedOrDiscarded(entityData, out canBeSalvaged, out canBeDiscarded); // validate!

                if (canBeDiscarded)
                {
                    Command discardCommand = new Discard(entityID.Value, The.InGameUI.UIAllegiance.ID, true);

                    The.Client.Controller.StoreAndExecuteCommand(discardCommand);
                }
                else
                {
                    Refresh();
                }
            }
            else
            {
                HandleInvalidEntity();
            }
        }



        public void OnDiscardEntity()
        {
            RefreshEntityContent();

            Hide();
        }

        void tbExpedition_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.PlaceExpeditionCenter;

            Hide();
        }

        void tbHunt_Click(UIComponent sender, EventArgs e)
        {
            if (entityID.HasValue)
            {
                SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;

                IKnownEntityData creatureToHunt;
                if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entityID.Value, out creatureToHunt)))
                {
                    if (creatureToHunt.CanBeHunted(The.InGameUI.UIAllegiance))
                    {

                        Expedition expedition = The.InGameUI.GetExpedition();
                        // or this? 
                        // Expedition expedition = The.Map.GetClosestExpedition(creatureToHunt.Location);

                        if (expedition != null)
                        {
                            Command huntCommand = new Hunt(entityID.Value, The.InGameUI.UIAllegiance.ID, expedition.OwnedEntities.ID, true);
                            The.Client.Controller.StoreAndExecuteCommand(huntCommand);
                        }

                    }
                }
                else
                {
                    HandleInvalidEntity();
                }

            }
        }



        public void OnHuntCreature()
        {
            // play a sound:
            The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);

            Hide();
        }


        public bool IsShowingEntity(EntityID entityID)
        {
            return this.entityID == entityID;
        }

        public bool IsShowingExpedition(Expedition expedition)
        {
            return this.expedition != null && this.expedition == expedition;
        }

        void tbSalvage_Click(UIComponent sender, EventArgs e)
        {
            if (entityID.HasValue)
            {
                TrySalvage(entityID.Value);
            }
        }

        public static void TrySalvage(EntityID entityID)
        {
            SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
            IKnownEntityData entity;

            if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entityID, out entity)))
            {
                IOwner ownerOfEntity;
                LookUpOwners.ResolveEntityOwner(entity, out ownerOfEntity);

                if (ownerOfEntity != null
                    && ownerOfEntity.Allegiance == The.InGameUI.UIAllegiance // must be player owned
                    && entity.EntityType.CanBeSalvagedDirectly()
                    && !Salvage.SalvageJobExists(entity))
                {
                    Command salvageCommand;

                    // salvage any upgrades first - the evaluator needs to check this also.
                    // the manager will handle this.
                    if (entity.ContainedUpgrades != null)
                    {
                        foreach (var item in entity.ContainedUpgrades)
                        {
                             IKnownEntityData upgradeEntity;
                             if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(item.Value, out upgradeEntity)))
                             {
                                 if (upgradeEntity.EntityType.CanBeSalvagedDirectly()
                                     && !Salvage.SalvageJobExists(upgradeEntity))
                                 {
                                     salvageCommand = new Salvage(item.Value, The.InGameUI.UIAllegiance.ID, true);
                                     The.Client.Controller.StoreAndExecuteCommand(salvageCommand);
                                 }
                             }
                        }
                    }

                    // disable all upgrades, 
                    // (so salvage jobs will be created and not reattempted while salvaging the host)
                    if (ownerOfEntity.OwnedEntities.Upgrades != null)
                    {
                        Dictionary<UpgradeCategory, EntityType> dict;
                        if (ownerOfEntity.OwnedEntities.Upgrades.TryGetValue(entityID, out dict))
                        {
                            List<SetUpgrade> commands = new List<SetUpgrade>();
                            foreach (var item in dict)
                            {
                                SetUpgrade setUpgrade = new SetUpgrade(entityID, The.InGameUI.UIAllegiance.ID, ownerOfEntity.OwnedEntities.ID, true, item.Key, null);
                                commands.Add(setUpgrade); // needed to avoid iterator crash                                
                            }

                            foreach (var item in commands)
                            {
                                The.Client.Controller.StoreAndExecuteCommand(item);
                            }
                        }
                    }
                    
                    salvageCommand = new Salvage(entityID, The.InGameUI.UIAllegiance.ID, true);
                    The.Client.Controller.StoreAndExecuteCommand(salvageCommand);
                }

            }

        }



        public void OnSalvageEntity()
        {
            // play a sound:         
            The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);

            Hide();
        }

        /*
          EntityID? entityID;
        Expedition expedition;
         * */
        public void ShowOnPlayfield(int screenPosX, int screenPosY, bool modal, Expedition expedition)
        {
            if (RefreshExpeditionContent() == false)
            {
                return;
            }
            this.expedition = expedition;
            entityID = null;

            base.ShowOnPlayfield(screenPosX, screenPosY);//, modal);
        }

        public void ShowOnPlayfield(int screenPosX, int screenPosY, bool modal, EntityID entityID)
        {
            this.entityID = entityID;
            expedition = null;
            if (RefreshEntityContent() == false)
            {
                return;
            }

            base.ShowOnPlayfield(screenPosX, screenPosY);
        }

        public bool RefreshExpeditionContent()
        {
            Remove(tbSalvage);
            Remove(tbPackingDown);
            Remove(tbHunt);
            Remove(tbDiscard);
            Remove(tbClaim);
            if (btSalvageProcess != null)
            {
                Remove(btSalvageProcess);
            }
            grdSpecialActions.Clear();

            Add(tbMoveExpedition);

            DisplayWindow.Height = tbMoveExpedition.Height + singleSpacing * 2;

            return true;
        }

        private bool RefreshEntityContent()
        {
            bool canBeSalvaged, canBeHunted, /*hasEnabledSpecialActions,*/ hasSpecialActions;
            bool canBeDiscarded;
            bool canBeClaimed;
            bool canSetStockpile;
            bool canSetTradeOffers;
            bool canUpgrade;
            IKnownEntityData entityData;

            if (EntityHasActions(out canBeSalvaged, out canBeHunted, out canBeDiscarded, out canBeClaimed, out hasSpecialActions, /*out hasEnabledSpecialActions,*/ out canSetStockpile, out canSetTradeOffers, out canUpgrade, out entityData, this.entityID)
                == false)
            {
                Remove(tbMoveExpedition);
                Remove(tbSalvage);
                Remove(tbPackingDown);
                Remove(tbHunt);
                Remove(tbDiscard);
                Remove(tbClaim);
                Remove(tbSetStockpile);
                Remove(tbSetTradeOffers);
                Remove(tbUpgrade);
                grdSpecialActions.Clear();
                return false;

            }

            int height = topMargin - singleSpacing;

            Remove(tbMoveExpedition);


            if (canBeSalvaged)
            {
                int yPos = height + singleSpacing;
                var process = entityData.EntityType.NonLivingType.SalvageProcessType;
                CreateSalvageProcessButton(process, yPos);

                Add(btSalvageProcess);

                TextButton salvageButton;
                if (process.IsSalvageWithoutWaste)
                {
                    salvageButton = tbPackingDown;
                    Remove(tbSalvage);
                }
                else
                {
                    salvageButton = tbSalvage;
                    Remove(tbPackingDown);
                }

                Add(salvageButton); 
                salvageButton.Y = yPos;
                salvageButton.X = btSalvageProcess.Right + singleSpacing; // sideMargin;
                height += salvageButton.Height;               
            }
            else
            {
                Remove(tbSalvage);
                Remove(tbPackingDown);

                if (btSalvageProcess != null)
                {
                    Remove(btSalvageProcess);
                }
            }

            if (canBeHunted)
            {
                Add(tbHunt);
                tbHunt.Y = height + singleSpacing;
                tbHunt.X = sideMargin;
                height += tbHunt.Height;
            }
            else
            {
                Remove(tbHunt);
            }

            if (canSetStockpile)
            {
                Add(tbSetStockpile);
                tbSetStockpile.Y = height + singleSpacing;
                tbSetStockpile.X = sideMargin;
                height += tbSetStockpile.Height;

                if (Entity.IsFunctional(entityData))
                {
                    tbSetStockpile.Enabled = true;
                    tbSetStockpile.ToolTip = stockpileTooltip;
                }
                else
                {
                    // broken...
                    tbSetStockpile.Enabled = false;
                    tbSetStockpile.ToolTip = stockpileBrokenTooltip;
                }
            }
            else
            {
                Remove(tbSetStockpile);
            }

            if (canSetTradeOffers)
            {
                Add(tbSetTradeOffers);
                tbSetTradeOffers.Y = height + singleSpacing;
                tbSetTradeOffers.X = sideMargin;
                height += tbSetTradeOffers.Height;

                if (Entity.IsFunctional(entityData))
                {
                    tbSetTradeOffers.Enabled = true;
                    tbSetTradeOffers.ToolTip = offeredForTradeTooltip;
                }
                else
                {
                    // broken...
                    tbSetTradeOffers.Enabled = false;
                    tbSetTradeOffers.ToolTip = stockpileBrokenTooltip;
                }
            }
            else
            {
                Remove(tbSetTradeOffers);
            }

            if (canUpgrade)
            {
                Add(tbUpgrade);
                tbUpgrade.Y = height + singleSpacing;
                tbUpgrade.X = sideMargin;
                height += tbUpgrade.Height;

                if (Entity.IsFunctional(entityData))
                {
                    tbUpgrade.Enabled = true;
                    tbUpgrade.ToolTip = "UPGRADE. View or set the possible upgrades.";
                }
                else
                {
                    // broken...
                    tbUpgrade.Enabled = false;
                    tbUpgrade.ToolTip = "UPGRADE. Cannot upgrade a broken structure.";
                }
            }
            else
            {
                Remove(tbUpgrade);
            }

            if (canBeDiscarded)
            {
                Add(tbDiscard);
                tbDiscard.Y = height + singleSpacing;
                tbDiscard.X = sideMargin;
                height += tbDiscard.Height;
                if (entityData.EntityType.StructureType != null)
                {
                    tbDiscard.Text = "ABANDON";
                    tbDiscard.ToolTip = "ABANDON. Stop using this structure";
                }
                else
                {
                    tbDiscard.Text = "DISCARD";
                    tbDiscard.ToolTip = "DISCARD. Exclude this item from the colony's possessions.";
                }
                tbDiscard.ScaleWidthToFitText();
            }
            else
            {
                Remove(tbDiscard);
            }

            if (canBeClaimed)
            {
                Add(tbClaim);
                tbClaim.Y = height + singleSpacing;
                tbClaim.X = sideMargin;
                height += tbClaim.Height;

                if (entityData.EntityType.StructureType != null)
                {
                    tbClaim.Text = "CLAIM";
                    tbClaim.ToolTip = "CLAIM. Start using this structure";
                }
                else
                {
                    tbClaim.Text = "CLAIM";
                    tbClaim.ToolTip = "CLAIM. Include this item in the colony's possessions";
                }

                tbClaim.ScaleWidthToFitText();
            }
            else
            {
                Remove(tbClaim);
            }

            PopulateSpecialActionGrid(hasSpecialActions /* hasEnabledSpecialActions*/, entityData);

            if (hasSpecialActions) // hasEnabledSpecialActions)
            {
                Add(grdSpecialActions);

                grdSpecialActions.Y = height + singleSpacing;

                // is this two line necessary ??
                /*  grdSpecialActions.Parent.Y = 0;  
                  grdSpecialActions.Parent.Parent.Y = 0;
                  */
                /*
                foreach (var item in grdSpecialActions.Entries)
                {                                                      
                    height += item.Height;
                }
                height += 12;
                grdSpecialActions.Height = height; // what does this do?? does the grid get its own viewport w. scrollbar???
                 * */

                height += grdSpecialActions.Height; // ??
            }
            else
            {
                Remove(grdSpecialActions);
            }

            //ReArrangeButtons();
            DisplayWindow.Height = height + singleSpacing * 2;
            return true;
        }


        private void CreateSalvageProcessButton(ProcessType salvageProcess, int yPos)
        {
            if (btSalvageProcess == null || btSalvageProcess.processType != salvageProcess)
            {
                if (btSalvageProcess != null)
                {
                    // remove the old button first:
                    Remove(btSalvageProcess);
                }

                btSalvageProcess = new DataTypeButton(The.InGameUI.gui, DataSheet.InfoToShow.Production, salvageProcess, null, true);
                btSalvageProcess.Init(TextButton.TextButtonType.HUDToolTipWhite);
              //  btSalvageProcess.ID = UIComponent.DataControlID.Caption;
                btSalvageProcess.IsRoot = true;
                btSalvageProcess.Text = salvageProcess.Name; // usePluralName ? entityType.PluralName : entityType.Name;
              //  Add(btSalvageProcess);
                btSalvageProcess.TextAlignment = TextButton.TextAlign.Left;
                btSalvageProcess.Width = 110; // quantityX - captionX;
                btSalvageProcess.X = sideMargin; // captionX;         
                btSalvageProcess.Y = yPos;
            }
        }

        /// <summary>
        /// "Lower gas bomb"
        /// </summary>
        public void PopulateSpecialActionGrid(bool hasSpecialActions, IKnownEntityData entityData)
        {

            UIComponent itemRow;

            EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);


            grdSpecialActions.BeginAddingEntries();

            if (!hasSpecialActions || entityData == null || !entityData.IsCompleted())
            {
                grdSpecialActions.Clear();
            }
            else
            {
                var specialActionsToDisplay = The.InGameUI.UIAllegiance.SharedKnowledge.GetSpecialActionsForDisplay(entityData); // Entity.GetSpecialActionsForDisplay(entityData); // append locks too
                if (specialActionsToDisplay != null)
                {
                    foreach (var processType in specialActionsToDisplay) 
                    {
                        if (!grdSpecialActions.TryGetEntry(processType, out itemRow))
                        {
                            itemRow = AddItemRow(processType);
                        }

                        UpdateItemRow(itemRow, entityData, processType, owner);
                    }
                }

                grdSpecialActions.DeleteEntries<ProcessType>(e => specialActionsToDisplay != null && specialActionsToDisplay.Contains(e)); //.AvailableSpecialActions.Contains(e.KeyName)); // .EntityType.SpecialActionTypes.Contains(e));

            }

            grdSpecialActions.Sort(p => p.OrderByTag1, Grid.Sorting.Ascending);

            grdSpecialActions.EndAddingEntries();

        }

        private void UpdateItemRow(UIComponent itemRow, IKnownEntityData entityData, ProcessType processType, EntityGroup owner) // bool canBuildNow) // ProductionAvailability availability)
        {
            TextButton tbBuild = (TextButton)itemRow.FindChildById(UIComponent.DataControlID.CurrentOrders);
                        
            List<Job> jobs = owner.OtherJobs;

            if (SpecialAction.ActionJobExists(entityData, processType, jobs)) // see that this job was not added already
            {
                tbBuild.ToolTip = "This task is ongoing. Use the task panel to view or cancel it";
                tbBuild.Enabled = false;
                return;
            }

            bool isEnabled = The.InGameUI.UIAllegiance.SharedKnowledge.SpecialActionIsAvailable(entityData, processType); // entityData.AvailableSharedSpecialActions.Contains(processType);

            if (!isEnabled)
            {
                tbBuild.ToolTip = "Not available at this time.";
                tbBuild.Enabled = false;
                return;
            }

            bool hasTools, hasInputs, hasSkills, hasResources, hasSpecialSite, hasPolicy; //, needsImmovableInput;
            //  EntityType immovableInput;
            // int productionLimit;
            int? noOfMissingInputTypes;
            int? noOfAvailableInputTypes;
            // int? outputBatchAmount;
            int maxAmountThatCanBeProduced;
            EntityType needsImmovableInput;


            bool canProduce = InventoryPanel.HasAllInputsAndToolsForProcess(processType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced,
                    out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out needsImmovableInput, null); // allAvailableItems);


            // Color color = GetBuildingAvailabilityColor(availability);

            // tbCaption.Color = color;

            // find build button in row and disable / hide it
            // let's hide it...
            //     ImageButton tbBuild = (ImageButton)itemRow.FindChildById(UIComponent.DataControlID.CurrentOrders);
            if (canProduce == false) // availability != ProductionAvailability.CanBuildNow)
            {
                if (hasPolicy == false)
                {
                    tbBuild.ToolTip = "We need to adopt a policy first.";
                }
                else
                {
                    tbBuild.ToolTip = "We don't have all the needed materials or tools to begin this"; //"We lack one material or tool type to begin this";  //execute  // change in the second place as well
                }

                tbBuild.Enabled = false;
            }
            else
            {
                tbBuild.ToolTip = "Click to begin"; //execute
                tbBuild.Enabled = true;
            }

        }

        protected UIComponent CreateTooltipAndActionButton(EntityType entityType, ProcessType processType, string actionLabel, EventArgs eventArgs, int menuWidth, out TextButton btAction)
        {
            UIComponent item = new UIComponent(gui);

            btAction = new TextButton(gui);
            btAction.Init(WindowSystem.TextButton.TextButtonType.HUDToolTipWhite);
            btAction.ID = UIComponent.DataControlID.CurrentOrders;
            btAction.EventArgs = eventArgs;
            btAction.Text = actionLabel;
            btAction.TextAlignment = TextButton.TextAlign.Center;
            btAction.ScaleWidthToFitText();
            btAction.X = menuWidth - btAction.Width - 20;
            item.Add(btAction);

            DataTypeButton tbCaption;
            if (entityType != null)
            {
                tbCaption = new DataTypeButton(gui, DataSheet.InfoToShow.Production, entityType, null, true);
                tbCaption.Text = entityType.Name;
            }
            else
            {
                tbCaption = new DataTypeButton(gui, DataSheet.InfoToShow.Production, processType, null, true);
                tbCaption.Text = processType.Name;
            }

            tbCaption.Init(TextButton.TextButtonType.HUDToolTipWhite);
            tbCaption.ID = UIComponent.DataControlID.Caption;
            tbCaption.IsRoot = true;
            item.Add(tbCaption);
            tbCaption.TextAlignment = TextButton.TextAlign.Left;
            tbCaption.Width = menuWidth - btAction.Width - (menuWidth - btAction.Width - btAction.X) - singleSpacing; // doubleSpacing; 
            tbCaption.X = 2; // captionX;
            tbCaption.SideToAnchorOn = InGameInterface.AnchorSide.Left;
            return item;
        }

        private UIComponent AddItemRow(ProcessType processType) //, EventArgs eventArgs)
        {

            ActionButtonEventArgs eventArgs = new ActionButtonEventArgs(processType); 

            TextButton actionButton;
            UIComponent item = CreateTooltipAndActionButton(null, processType, processType.SpecialActionCaption ?? "BEGIN", eventArgs, DisplayWindow.Width, out actionButton); //MP was: "Execute"

            actionButton.Click += new ClickHandler(specialAction_Click);
            actionButton.ID = UIComponent.DataControlID.CurrentOrders;

            /*
            if (actionButton.Enabled == false)
            {
                actionButton.ChangeSkinState(SkinState.Disabled); // why is this hack needed???
            }*/


            grdSpecialActions.AddEntry(processType, item);
            item.OrderByTag1 = processType.SortOrder; //processType.Name;

            return item;
        }

        void specialAction_Click(UIComponent sender, EventArgs e)
        {
            if (entityID.HasValue)
            {
                ActionButtonEventArgs processEventArgs = e as ActionButtonEventArgs;

                SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
                IKnownEntityData entity;

                if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entityID.Value, out entity)))
                {
                    if (TryToCreateActionJob(entity, processEventArgs.ProcessType))
                    {
                        // play a sound:
                        The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);
                        //  TextButton actionButton = (TextButton)sender;
                        //  actionButton.Pressed = true;

                        /*
                        // hide/disable alternative action:

                        if (processEventArgs.ProcessType.UsesAnchor())
                        {
                            // disable special action(s) on the anchor to prevent re-clicking:
                            // this should be repeated when the process finishes to account for event spawns
                            Entity.DisableSpecialActionsUsingAnchor(entity); 
                        }
                        */
                    }
                }

            }

            Refresh();

        }


        private const int maxNumberOfMissingInputsToDisplayProcessesWithout = 1;

        /*  private static ProductionAvailability GetActionAvailability(ProcessType processType, EntityGroup owner)
          {
              bool canProduce;
              bool hasInputs;
              bool hasTools;
              int maxAmountThatCanBeProduced;
              int? noOfMissingInputTypes;
              int? noOfAvailableInputTypes;
              bool hasSkills;
              bool needsImmovableInput;
              EntityType immovableInput;

              canProduce = InventoryPanel.HasAllInputsAndToolsForProcess(
                  processType,
                  owner,
                  out hasInputs,
                  out hasTools,
                  out maxAmountThatCanBeProduced,
                  out noOfMissingInputTypes,
                  out noOfAvailableInputTypes,
                  out hasSkills,
                  out immovableInput, // needsImmovableInput,
                  null);

              needsImmovableInput = immovableInput != null;

              return GetAvailability(hasInputs, hasTools, noOfMissingInputTypes, noOfAvailableInputTypes, hasSkills, needsImmovableInput);

         
          }*/

        /*
        private static ProductionAvailability GetAvailability(bool hasInputs, bool hasTools, int? noOfMissingInputTypes, int? noOfAvailableInputTypes, bool hasSkills, bool needsImmovableInput)
        {
            if (hasInputs && hasTools && hasSkills) // && !needsImmovableInput)
            {
                return ProductionAvailability.CanBuildNow;
            }
            else if ((noOfMissingInputTypes ?? 0) <= maxNumberOfMissingInputsToDisplayProcessesWithout
              //  && noOfAvailableInputTypes > 0 // don't include items with zero available inputs in the lookahead
              )
            {
                return ProductionAvailability.MissingOneInputType;
            }
            else
            {
                return ProductionAvailability.FurtherAway;
            }
        }
*/




        private static bool TryToCreateActionJob(IKnownEntityData entity, ProcessType processType)
        {
            EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);

            if (owner == null)
                return false;

            List<Job> jobs = owner.OtherJobs;

            if (The.InGameUI.UIAllegiance.SharedKnowledge.SpecialActionIsAvailable(entity, processType) //entity.AvailableSharedSpecialActions.Contains(processType)
                && !SpecialAction.ActionJobExists(entity, processType, jobs)) // see that this job was not added already
            {
                Command command = new SpecialAction(entity.EntityID, The.InGameUI.UIAllegiance.ID, owner.ID, true, processType.KeyName);

                The.Client.Controller.StoreAndExecuteCommand(command);

                return true;

            }

            return false;
        }



        /// <summary>
        /// periodic refresh. acion buttons can be out of date in between refreshes. so each action should check if it is valid before / during executing...
        /// </summary>
        public override void Refresh()
        {
            if (entityID.HasValue == true)
            {
                RefreshEntityContent();
            }
            else if (expedition != null)
            {
                RefreshExpeditionContent();
            }

            base.Refresh();
        }

        public static bool EntityHasActions(out bool canBeSalvaged, out bool canBeHunted,
            out bool canBeDiscarded, out bool canBeClaimed, out bool hasSpecialActions, /*out bool hasEnabledSpecialActions,*/ out bool canSetStockpile, out bool canSetTradeOffers, out bool canUpgrade, 
            out IKnownEntityData entityData, EntityID? entityID) //, out ProcessType salvageProcess)
        {
            canBeSalvaged = false;
            canBeHunted = false;
            canBeDiscarded = false;
            hasSpecialActions = false;
            //hasEnabledSpecialActions = false;
            canSetStockpile = false;
            entityData = null;
            canBeClaimed = false;
            canSetTradeOffers = false;
            canUpgrade = false;

            //salvageProcess = null;

            if (entityID.HasValue)
            {
                SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;

                if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entityID.Value, out entityData)))
                {
                    canBeHunted = entityData.CanBeHunted(The.InGameUI.UIAllegiance);

                    if (The.InGameUI.UIOwner.HasValue)
                    {
                        canSetStockpile = entityData.CanSetStockpileSettings(The.InGameUI.UIOwner.Value);
                        canSetTradeOffers = entityData.CanSetTradeOfferSettings(The.InGameUI.UIOwner.Value);
                        canUpgrade = entityData.CanBeUpgraded(The.InGameUI.UIOwner.Value);
                    }

                    if (The.InGameUI.UIAllegiance.SharedKnowledge.HasSpecialActionsForDisplay(entityData)) //entityData.EntityType.SpecialActionTypes != null && entityData.EntityType.SpecialActionTypes.Count > 0)
                    {
                        hasSpecialActions = true;
                    }
                    /*
                    if (The.InGameUI.UIAllegiance.SharedKnowledge.HasAvailableSpecialActions(entityData))  //  entityData.AvailableSpecialActions != null && entityData.AvailableSpecialActions.Count > 0) 
                    {
                        hasEnabledSpecialActions = true;
                    }*/

                    EntityListWindow.GetAllowedActions(entityData, out canBeSalvaged, out canBeDiscarded, out canBeClaimed); //, out salvageProcess);
                }
            }

            return canBeHunted || canBeSalvaged || hasSpecialActions || canBeDiscarded || canBeClaimed || canSetTradeOffers || canSetStockpile || canUpgrade;
        }

        public override void Hide()
        {
            this.expedition = null;
            this.entityID = null;

            //TEMP: Check if all the windows are reset as the mouse will be outside the next time we open them...

            //  this.DisplayWindow.isMouseOver = false;
            base.Hide();
        }
    }
}
