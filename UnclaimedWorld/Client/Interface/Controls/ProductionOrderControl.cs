using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Processes;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Controls
{
    public class ProductionOrderControl: UIComponent
    {
        public const string btPadlockTooltip = "Switch to standing order mode.";
        public const string btPadlockEnabledTooltip = "Standing order mode. In this mode, production will start and continue whenever the inventory is below the slider value. \nClick to switch back to direct order mode.";

        private const string tbBuildToolTip = "Build: Click the button, then place the structure on the terrain"; //Click to build the selected structure.
        private const string lblMaxOrderToolTip = "Maximum number of structures we can build";
        private const string lblImmovableToolTip = "{0} must first be selected, then the item can be built from the action menu";

        public const string orderSpamWarning = "Ordering many single items can take a while to produce. Try to look for ways to produce in larger batches, as this cuts down on the production time.";
     
        EntityType entityType;

        StockButton btStockAvailable, btStockUnavailable;

        ImageButton btStandingOrder;

        Label lblMaxOrder;

        /// <summary>
        /// canb be null
        /// </summary>
        ImageButton btBuild;
        HorizontalList hzAttainable;
        HorizontalList hzNotAttainable;

        /// <summary>
        /// can be null
        /// </summary>
        FillableBar fillableBar;

        Icon icWarning;

        bool isFirstUpdate = true;

        bool isSliderBeingDragged = false;


        public enum UILayout { HUD, LCD }

        public ProductionOrderControl(EntityType entityType, ProductionTargetEventArgs eventArgs, GUIManager gui, UILayout uiLayout,
                int productionColumnX,               
              Action<UIComponent, EventArgs> tbItems_Click,         
              Action<UIComponent, EventArgs> btPadlock_Click)
            : base(gui)
        {
            this.entityType = entityType;

            base.Width = 224;
            base.Height = 25;


            btStockAvailable = new StockButton(guiManager, entityType);
          //  tbAvailableItems.ID = UIComponent.DataControlID.Available;
            Add(btStockAvailable);
            btStockAvailable.Position = new Point(0, 0);
            btStockAvailable.Click += new ClickHandler(tbItems_Click);
            CenterChildVertically(btStockAvailable);

            btStockUnavailable = new StockButton(guiManager, entityType);
           // tbUnavailableItems.ID = UIComponent.DataControlID.Unavailable;
            Add(btStockUnavailable);
            btStockUnavailable.Position = new Point(39 /* unavailableX*/, 0);
            btStockUnavailable.Click += new ClickHandler(tbItems_Click);
            CenterChildVertically(btStockUnavailable);
            

            //  ProductionMode productionMode = The.Sim.Controller.Options.ProductionMode;

            hzNotAttainable = new HorizontalList(guiManager);
            Add(hzNotAttainable);
          //  hzNotAttainable.ID = UIComponent.DataControlID.NotAttainableIcons;
            hzNotAttainable.X = productionColumnX;
            hzNotAttainable.Height = 21;
            CenterChildVertically(hzNotAttainable);

            hzAttainable = new HorizontalList(guiManager);
            Add(hzAttainable);
         //   hzAttainable.ID = UIComponent.DataControlID.AttainableIcons;
            hzAttainable.X = productionColumnX;
            hzAttainable.Height = 21;
            CenterChildVertically(hzAttainable);

            if (entityType.StructureType == null)
            {
                FillableBar.FillableBarType barType = (uiLayout == UILayout.LCD ? FillableBar.FillableBarType.LCDSliderWhite : FillableBar.FillableBarType.HUDSliderWhite);
                fillableBar = new FillableBar(guiManager, barType, false, true,
                    GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay);

                Add(fillableBar);
              //  fillableBar.ID = UIComponent.DataControlID.CurrentOrders;

                fillableBar.Width = 130; // 125;
                fillableBar.X = productionColumnX;
                fillableBar.Y = 5;
                fillableBar.SliderTooltip = "Drag slider to specify amount to produce.";
                fillableBar.ButtonTooltip = "Click or hold the mouse button to change the amount to produce.";

                fillableBar.EventArgs = eventArgs;
                fillableBar.SliderMouseDown += new EventHandler(fillableBar_SliderMouseDown);
                fillableBar.SliderMouseUp += new EventHandler(fillableBar_SliderMouseUp);
                fillableBar.ShowNotches = true;

                /*
                fillableBar.MaxValue = GameData.Instance.GUIConstants.UnlimitedStandingOrderValue; // 99;            
                fillableBar.MaxSliderValueSymbol = "...";
                fillableBar.MaxSliderValueTooltip = "No limit";
                */

                icWarning = new Icon(guiManager);
                Add(icWarning);
              //  icWarning.ID = UIComponent.DataControlID.Warning;
                icWarning.ToolTip = orderSpamWarning;
                icWarning.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_status_exclamation"), Color.Red, Color.Red);
                icWarning.ResizeControlToFitImage();
                icWarning.X = fillableBar.Right - 7; // the tracking button is closer to the right in category view! // 5;
                icWarning.Y = 2;

            }
            else
            {
                fillableBar = null;

                btBuild = new ImageButton(guiManager);
                Add(btBuild);
                btBuild.CheckedMode = CheckedModes.CannotBeChecked;
                if (uiLayout == UILayout.LCD)
                {
                    btBuild.InitWithIcon(ImageButtonType.LCD, "basic_icon_hammer", false);
                }
                else
                {
                    btBuild.InitWithIcon(ImageButtonType.HUD, "basic_icon_hammer_white", false);
                }
                btBuild.Position = new Point(productionColumnX + 10, 0);
                btBuild.EventArgs = eventArgs;
               // tbBuild.ID = UIComponent.DataControlID.Build;
                btBuild.Click += build_Click; // new ClickHandler(build_Click); // build_Click;
                btBuild.ToolTip = tbBuildToolTip;
                btBuild.DebugTag = "Build Debug";
                //  tbBuild.MouseOver += InventoryPanel_MouseOver;
                // tbBuild.MouseOut += InventoryPanel_MouseOver;
                CenterChildVertically(btBuild);


                lblMaxOrder = new Label(guiManager);
                Add(lblMaxOrder);
                if (uiLayout == UILayout.LCD)
                {
                    lblMaxOrder.Init(Label.LabelType.LCDNormal);
                }
                else
                {
                    lblMaxOrder.Init(Label.LabelType.HUDWindow);
                }
                lblMaxOrder.X = btBuild.Right + 16;
               // lblMaxOrder.ID = UIComponent.DataControlID.MaxOrders;
                CenterChildVertically(lblMaxOrder);
                lblMaxOrder.ToolTip = lblMaxOrderToolTip;
                // lblMaxOrder.MouseOver += InventoryPanel_MouseOver;
                // lblMaxOrder.MouseOut += InventoryPanel_MouseOver;
            }


            if (GameData.Instance.GUIConstants.EnableStandingOrders)
            {
                btStandingOrder = new ImageButton(guiManager);
                Add(btStandingOrder);
                btStandingOrder.Init(ImageButtonType.LCDPadlockWhite);
                btStandingOrder.Position = new Point(3, 0);
                btStandingOrder.EventArgs = eventArgs;
                btStandingOrder.Click += new ClickHandler(btPadlock_Click); // btPadlock_Click;
                btStandingOrder.Visible = true;
                btStandingOrder.ToolTip = btPadlockTooltip;
               // btPadlock.ID = UIComponent.DataControlID.StandingOrderModePadlock;
                CenterChildVertically(btStandingOrder);
                // btPadlock.MouseOut += tbStanding_MouseOut;

                btStandingOrder.X = productionColumnX - 16;

                /*
                ImageButton btPadlock = new ImageButton(Interface.gui);

                if (!The.Sim.Controller.Options.AlwaysShowPadlockButton)
                {
                    UIComponent standingHotspot = new UIComponent(Interface.gui);
                    item.Add(standingHotspot);
                    standingHotspot.Position = new Point(itemTypeProductionTargetColumnX - 16, 0); 
                    standingHotspot.Width = 16;
                    standingHotspot.Height = item.Height;
                    standingHotspot.MouseOver += standingHotspot_MouseOver;
                    standingHotspot.MouseOut += standingHotspot_MouseOut;
                    standingHotspot.EventArgs = eventArgs;
                    standingHotspot.ID = UIComponent.DataControlID.StandingOrderMode;

                    standingHotspot.Add(btPadlock); 
                }
                else
                {
                    item.Add(btPadlock);
                    btPadlock.X = itemTypeProductionTargetColumnX - 16;                        
                }
              
             
                btPadlock.Init(ImageButtonType.LCDPadlockWhite);
                btPadlock.Position = new Point(3, 0); //tbTracking.Position = new Point(itemTypeProductionTargetColumnX + 125 + 20, -5); 
                btPadlock.EventArgs = eventArgs;
                //  tbTracking.ID = UIComponent.DataControlID.Track;
                btPadlock.Click += btPadlock_Click;
                btPadlock.Visible = false;
                btPadlock.ToolTip = btPadlockTooltip;
               // tbStanding.Width = 36;
                item.CenterChildVertically(btPadlock);
                btPadlock.MouseOut += tbStanding_MouseOut;
                */

            }

        }


        //void fillableBar_SliderMouseUp(object sender, EventArgs e)
        void fillableBar_SliderMouseUp(object sender, EventArgs e)
        {
            FillableBar slider = sender as FillableBar;
            EntityType entityType = ((ProductionTargetEventArgs)e).Item;
          
           /* ImageButton btStandingOrder = null;
            if (GameData.Instance.GUIConstants.EnableStandingOrders)
            {               
                btStandingOrder = (ImageButton)slider.Parent.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock);
            }*/

            ExpeditionID? expeditionID = The.InGameUI.UIExpedition;

            if (expeditionID.HasValue)
            {

                if (btStandingOrder != null && btStandingOrder.IsChecked)
                {
                    int value;
                    if (slider.Value == GameData.Instance.GUIConstants.UnlimitedStandingOrderValue)
                    {
                        value = Sim.HasNoLimitValue;
                    }
                    else
                    {
                        value = slider.Value;
                    }


                    SetStandingOrder setStandingOrder = new SetStandingOrder(expeditionID.Value, entityType.KeyName, value, true);
                    The.Client.Controller.StoreAndExecuteCommand(setStandingOrder);
                }
                else
                {
                    int newCount = slider.Value;

                    // if we change the meaning of Stock order we can remove this conversion:
                    int jobAmount = GetNoOfJobsFromOutputAmount(((ProductionTargetEventArgs)e).OutputBatchAmount, newCount);

                    SetProduction setProduction = new SetProduction(expeditionID.Value, entityType.KeyName, jobAmount, true);
                    The.Client.Controller.StoreAndExecuteCommand(setProduction);
                }
            }
        }

        private int GetNoOfJobsFromOutputAmount(int? outputBatchAmount, int outputAmount)
        {
            return outputAmount / (outputBatchAmount ?? 1);
        }

        public static void SetPadlockButtonState(bool hasOrder, ImageButton btStandingOrder)
        {
            if (hasOrder)
            {
                btStandingOrder.IsChecked = true;
                btStandingOrder.ToolTip = btPadlockEnabledTooltip;
                //  btStandingOrder.Visible = true;
            }
            else
            {
                btStandingOrder.IsChecked = false;
                btStandingOrder.ToolTip = btPadlockTooltip;
                // btStandingOrder.Visible = false;
            }
        }

        public static void Padlock_Click(UIComponent sender, EventArgs e)
        {
            Expedition expedition = The.InGameUI.GetExpedition(); // The.Sim.PlaySite.GetFirstPlayerExpedition();
            if (expedition == null)
                return;


            ProductionTargetEventArgs prodArgs = e as ProductionTargetEventArgs;

            ImageButton btPadlock = sender as ImageButton;
            if (btPadlock.IsChecked)
            {
                btPadlock.ToolTip = btPadlockEnabledTooltip;

                // NEW: clear direct order 
                SetStandingOrder setStandingOrder = new SetStandingOrder(expedition.ID, prodArgs.Item.KeyName, 0, true);
                The.Client.Controller.StoreAndExecuteCommand(setStandingOrder);
            }
            else
            {
                btPadlock.ToolTip = btPadlockTooltip;

                // NEW: clear standing order 
                SetProduction setProduction = new SetProduction(expedition.ID, prodArgs.Item.KeyName, 0, true);
                // SetStandingOrder setStandingOrder = new SetStandingOrder(expedition.ID, prodArgs.Item.KeyName, 0, true);
                The.Client.Controller.StoreAndExecuteCommand(setProduction);

            }

            // UpdateItemRow(sender.Parent, prodArgs.Item, owner);
        }

        //  public static TextButton latestSelectedBuildButton;
        private static void build_Click(UIComponent sender, EventArgs e)
        {
            EntityType selectedStructureType = ((ProductionTargetEventArgs)e).Item;

            The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.Build;
            Entity entity = new Entity(selectedStructureType, true);

            //   UWGame.SimSide.Instance.buildingBeingPlaced.Structure.SetupQuadVertices(); 

            entity.NonLivingEntity.Progress = 0f;

            entity.Initialize(The.Sim.PlaySite, The.Sim.PlaySite.PlayerAllegiance);
            entity.InitializeModelAndOnScreenFunctionality();

            entity.Renderable.SetOverlayRendering(true);

            /* if (latestSelectedBuildButton != null) // why was this needed?
             {
                 latestSelectedBuildButton.IsChecked = false;
             }
             latestSelectedBuildButton = (sender as TextButton);
             */

            The.InGameUI.EntitiesBeingPlaced.Add(new InGameInterface.EntityPosition() { Entity = entity, Position = Vector2.Zero });

        }

        public void UpdateOrders(EntityGroup owner, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems, out int noOfAvailableItems)
        {
            List<EntityID> listOfEntities;
            List<EntityID> listOfAvailableEntities;
            List<EntityID> listOfUnavailableEntities;

           
            int noOfIncompleteItems, noOfEntitiesUsedAsParts, noOfItemsOnOtherSite, noOfItemsOwnedByOthers, noOfAvailableItemsIncludingIntrinsic;
            noOfAvailableItems = InventoryPanel.GetNoOfAvailableEntities(owner.AllEntities, owner, entityType, out noOfIncompleteItems,
                                                                             out noOfEntitiesUsedAsParts, out noOfItemsOnOtherSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic,
                                                                             out listOfEntities, out listOfAvailableEntities, out listOfUnavailableEntities, allAvailableItems);

            UpdateItemRowAvailableStockButton(listOfAvailableEntities, noOfAvailableItems);

            UpdateItemRowUnavailableStockButton(listOfUnavailableEntities, noOfIncompleteItems, noOfEntitiesUsedAsParts, noOfItemsOwnedByOthers, noOfItemsOnOtherSite);


            bool hasTools, hasInputs, hasSkills, hasResources, hasSpecialSite, hasPolicy, needsImmovableInput;
            EntityType immovableInput;
            int productionLimit;
            int? noOfMissingInputTypes;
            int? noOfAvailableInputTypes;
            int? outputBatchAmount;
            ProcessType process;
            bool canProduce = InventoryPanel.GetBestProcessForDisplay(entityType, owner, out hasInputs, out hasTools, out productionLimit,
                out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out immovableInput, out outputBatchAmount, out process, 
                The.InGameUI.InventorySettings.IncludeSalvageProcesses,
                p => p.IsSalvageProcess == false && !p.IsPartOfProductionChainButCannotOrderFromInventory(),
                allAvailableItems);

            //always returns a process, even if we cannot produce now!

            needsImmovableInput = immovableInput != null;
            //  bool canBuildNow = productionLimit > 0;


          //  bool processCanBeOrderedFromInventory = false;

            Dictionary<ProcessType, AttainableInfo> attainableInfo = null; //  AttainableInfo attainableInfo = null;
            if (!canProduce) // !canBuildNow)
            {
                attainableInfo = The.InGameUI.InventorySettings.GetAttainableInfo(entityType);
            }

            ProcessType.ProductionUI productionMethod;
            if (process != null)
            {
                productionMethod = process.GetProductionUI();
              //  processCanBeOrderedFromInventory = process.IsSalvageProcess == false && !process.IsPartOfProductionChainButCannotOrderFromInventory();
            }
            else
            {
                productionMethod = ProcessType.ProductionUI.None; 
            }



            if (productionMethod == ProcessType.ProductionUI.Build) // entityType.StructureType != null)
            {
                UpdateItemRowStructureType(process, allAvailableItems, attainableInfo, noOfIncompleteItems, /*needsImmovableInput,*/ immovableInput, productionLimit,
                    hasTools, hasInputs, hasSkills, hasResources); //, productionMethod); // processCanBeOrderedFromInventory);

            }
            else
            {
                UpdateItemRowNormalProduction(owner, allAvailableItems, attainableInfo, entityType, process, hasTools, hasInputs, hasSkills, hasResources, productionLimit, outputBatchAmount, productionMethod); // processCanBeOrderedFromInventory);
            }


            switch (The.InGameUI.InventorySettings.SortingSettings.SortedBy)
            {
                case InventorySettings.SortColumns.Name:
                    SetOrderBy(entityType.PluralName); //(Parent.Tag1 as EntityType).PluralName);
                    break;

                case InventorySettings.SortColumns.CanProduce:
                    if (canProduce) // canBuildNow)
                    {
                        if (productionMethod == ProcessType.ProductionUI.Slider   //!= ProcessType.ProductionUI.None) // processCanBeOrderedFromInventory)
                            || productionMethod == ProcessType.ProductionUI.Build)
                        {
                            // let's order by the max number on the slider scale:
                            SetOrderBy(10000 * (outputBatchAmount ?? 1) * productionLimit);
                            //itemRow.OrderByTag1 = 10 * (outputBatchAmount ?? 1) * productionLimit; 
                        }
                        else
                        {
                            Parent.OrderByTag1 = 100;
                        }
                    }
                    else if (attainableInfo != null)
                    {
                        if (attainableInfo.Any(a => a.Value.IsProducable))
                        {
                            SetOrderBy(2); 
                        }
                        else
                        {
                            SetOrderBy(1);
                        }
                    }
                    else
                    {
                        SetOrderBy(0);
                    }
                    // itemRow.OrderByTag1 = productionLimit > 0 ? 0 : 1;
                    break;

                case InventorySettings.SortColumns.InStock:
                    {
                        int sumToOrderBy = GetSumToOrderBy(noOfIncompleteItems, noOfEntitiesUsedAsParts, noOfAvailableItems, noOfItemsOnOtherSite);
                        SetOrderBy(sumToOrderBy);

                        break;
                    }
            }

            isFirstUpdate = false;
        }

        private int GetSumToOrderBy(int noOfIncompleteItems, int noOfEntitiesUsedAsParts, int noOfAvailableItems, int noOfItemsOffSite) //, int noOfItemsOnOtherSite)
        {
            int sumToOrderBy = 100 * noOfAvailableItems + 10 * (noOfEntitiesUsedAsParts + noOfItemsOffSite) + noOfIncompleteItems;
            return sumToOrderBy;
        }

        private void SetOrderBy(object order)
        {
            Parent.OrderByTag1 = order;
        }

        private void UpdateItemRowAvailableStockButton(List<EntityID> listOfEntities, int noOfAvailableItems)
        {
            btStockAvailable.UpdateStockButton(noOfAvailableItems, null, null, null, null, listOfEntities, false);


            /*
            UIComponent itemComponent;
            itemComponent = itemRow.FindChildById(UIComponent.DataControlID.Available); // Stock);
            if (itemComponent != null)
            {
                StockButton tbStock = (StockButton)itemComponent;
                tbStock.UpdateStockButton(noOfAvailableItems, null, null, null, null, listOfEntities);
            }*/

        }

      

        private void UpdateItemRowUnavailableStockButton(List<EntityID> listOfEntities, int noOfIncompleteItems, int noOfEntitiesUsedAsParts, int noOfItemsOwnedByOthers, int noOfOffSiteItems)
        {
            btStockUnavailable.UpdateStockButton(null, noOfIncompleteItems, noOfEntitiesUsedAsParts, noOfOffSiteItems, noOfItemsOwnedByOthers, listOfEntities, true);

            /*
            UIComponent itemComponent;
            itemComponent = itemRow.FindChildById(UIComponent.DataControlID.Unavailable); 
            if (itemComponent != null)
            {
                StockButton tbStock = (StockButton)itemComponent;
                tbStock.UpdateStockButton(null, noOfIncompleteItems, noOfEntitiesUsedAsParts, noOfOffSiteItems, noOfItemsOwnedByOthers, listOfEntities);

            }*/
        }

        private void UpdateItemRowStructureType(ProcessType process, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems, Dictionary<ProcessType, AttainableInfo> attainableInfo, int noOfIncompleteItems,
           EntityType immovableInput, int productionLimit, bool hasTools, bool hasInputs, bool hasSkills, bool hasResources) //, ProcessType.ProductionUI processProductionMethod) //bool canOrderFromInventory)
        {

            if (GameData.Instance.GUIConstants.EnableStandingOrders)
            {
                /* UIComponent hotspot = itemRow.FindChildById(UIComponent.DataControlID.StandingOrderModeHotspot);
                 ImageButton btStandingOrder = hotspot.Controls[0] as ImageButton;
                 */

              //  ImageButton btStandingOrder = (ImageButton)itemRow.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock);
                btStandingOrder.Visible = false;

                //  hotspot.Visible = false;
            }

            bool canBuildNow = productionLimit > 0;

           // Label lblMaxOrder = itemRow.FindChildById(UIComponent.DataControlID.MaxOrders) as Label;

            int maxOrder = Common.ClampBottom(productionLimit - noOfIncompleteItems, 0); // exclude structures already ordered/being built
            lblMaxOrder.Text = maxOrder.ToString();

            if (maxOrder == 0)
            {
                lblMaxOrder.Visible = false;
            }
            else
            {
                lblMaxOrder.Visible = true;
            }


           /* if (processProductionMethod == ProcessType.ProductionUI.Build) // canOrderFromInventory)
            {*/
                if (maxOrder > 0)
                {
                    btBuild.Visible = true;
                }
                else
                {
                    btBuild.Visible = false;
                }
          /*  }
            else
            {
                btBuild.Visible = false;
            }*/

            if (btBuild.Visible == false)
            {
                // the build button is hidden for special actions

              /*  if (processProductionMethod == ProcessType.ProductionUI.None // canOrderFromInventory == false 
                    && canBuildNow == true)
                {
                    // special action. processes that are initiated outside the inventory.
                    // give information about giving orders here
                    UpdateNonInventoryOrders(allAvailableItems, process, hzAttainable, hzNotAttainable);
                }
                else
                {*/
                    // cannot produce now. show either green or red icons:
                    UpdateAttainable(attainableInfo, hzAttainable, hzNotAttainable,
                        hasTools, hasInputs, hasSkills, hasResources, canBuildNow, process);

               // }

            }
            else
            {
                hzAttainable.Visible = false;
                hzNotAttainable.Visible = false;
            }

        }


        public static void UpdateAttainable(Dictionary<ProcessType, AttainableInfo> attainableInfos, HorizontalList hzAttainable, HorizontalList hzNotAttainable,
          bool hasTools, bool hasInputs, bool hasSkills, bool hasResources, bool canProduceNow, ProcessType anyProcess) // these flags refer to the process!
        {
            KeyValuePair<ProcessType, AttainableInfo> producableInfo = default(KeyValuePair<ProcessType, AttainableInfo>); // = null;
            if (attainableInfos != null)
            {
                // choose a process here.. later, we may want to get ALL the producable processes for display
                // NEW: select the matching process if possible
                producableInfo = attainableInfos.FirstOrDefault(a => a.Value.IsProducable && (anyProcess== null || a.Key == anyProcess)); 

                if (producableInfo.Key == null) // == null)
                {
                    producableInfo = attainableInfos.First();
                }
            }

            ProcessType attainableProcess = null;
            AttainableInfo attainableInfo = null;

            if (producableInfo.Key != null)
            {
                attainableProcess = producableInfo.Key;
                attainableInfo = producableInfo.Value;

            }


            if (attainableProcess != null)
            {
                hzAttainable.Visible = true;
                hzNotAttainable.Visible = false;

                PopulateAttainable(attainableProcess, attainableInfo,
                                        hzAttainable, hasTools, hasInputs, hasSkills, hasResources, anyProcess, attainableInfos.Count); // display list of green icons


            }
            else
            {
                hzAttainable.Visible = false;
                hzNotAttainable.Visible = true;

                AttainableInfo nonAttainableInfo = null;
                int total = 0;
                if (attainableInfos != null)
                {
                    total = attainableInfos.Count;

                    KeyValuePair<ProcessType, AttainableInfo> nonAttainableInfoPair;
                    nonAttainableInfoPair = attainableInfos.FirstOrDefault(a => true); // pick any process...
                    if (nonAttainableInfoPair.Value != null)
                    {
                        nonAttainableInfo = nonAttainableInfoPair.Value;
                    }
                }

                PopulateNotAttainableIcons(hzNotAttainable, nonAttainableInfo, total); // display list of red icons

            }
        }


        /// <summary>
        /// displays an item that is attainable, but cannot be produced now. If the process type is available, we can show icons for which prerequisites are missing
        /// 
        /// </summary>
        /// <param name="attainableInfo"></param>
        /// <param name="list"></param>
        /// <param name="hasTools"></param>
        /// <param name="hasInputs"></param>
        /// <param name="hasSkills"></param>
        /// <param name="hasResources"></param>
        /// <param name="canProduce"></param>
        /// <param name="processType"></param>
        private static void PopulateAttainable(ProcessType producableProcess, AttainableInfo producableProcessInfo,
            //Dictionary<ProcessType, AttainableInfo> attainableInfo, 
            HorizontalList list, bool hasTools, bool hasInputs, bool hasSkills, bool hasResources, ProcessType processType, int totalProcesses)
        {

            bool processFlagsAreValid = false;
            if (processType != null && processType == producableProcess) // ????????????? processtype is always filled
            {
                processFlagsAreValid = true; // ????

            }


            /*  if (producableProcess.IsSalvageProcess)
              {                   
                  // show the green salvage icon
                  // only show the icon when directly salvagable? Would also remove the loop problem...
                  canSalvage = true;
              }
              else*/
            /*if (processType != null) 
            {
                // use deduction to determine what is needed. (Accurate enough?)
                if (processType.IsHarvesting)
                {
                    harvestable = true;
                }

                // true for harvest/gather processes...
                // show an icon with a tooltip that directs to either the special action menu or the gather window.
               // canGatherOrUseSpecialAction = true;
            }*/


            list.BeginAddingEntries();

            string text = "";

            UIComponent icon;

            Color attainableColor = GameData.Instance.GUIConstants.AttainableColor;

            if (producableProcess.IsSalvageProcess) //canSalvage)
            {

                icon = AddOrGetIcon(IconKeys.Salvage, list, "lcd_icon_recycleArrows", attainableColor);

                /*  string input = ".";
                
                  if (producableProcess.InputsByType != null && producableProcess.InputsByType.Count > 0)
                  {
                      EntityType inputType = producableProcess.InputsByType.First().Key;

                      // only show the hint if we own the item/structure input..
                      Availability availability;
                      if (The.InGameUI.InventorySettings.AllAvailableItems.TryGetValue(inputType, out availability)
                          && availability.NoOfAvailableItems > 0)
                      {
                          input = ", for example: " + inputType.Name + ".";
                      }
                  }*/

                text = "Attainable from salvaging items or structures"; // +input; // the items or structures may not exist (loops are possible)

                icon.ToolTip = text;

                list.TryRemoveEntry(IconKeys.NoProcess);
                list.TryRemoveEntry(IconKeys.NoSkill);
                list.TryRemoveEntry(IconKeys.NoResource);
                list.TryRemoveEntry(IconKeys.NoInput);
                list.TryRemoveEntry(IconKeys.NoTool);
                list.TryRemoveEntry(IconKeys.NoPolicy);

                list.EndAddingEntries();

                return; // don't show the rest.
            }
            else
            {
                list.TryRemoveEntry(IconKeys.Salvage);
            }


            if (!processFlagsAreValid)
            {
                // can't show flags

                list.TryRemoveEntry(IconKeys.NoProcess);
                list.TryRemoveEntry(IconKeys.NoSkill);
                list.TryRemoveEntry(IconKeys.NoResource);
                list.TryRemoveEntry(IconKeys.NoInput);
                list.TryRemoveEntry(IconKeys.NoTool);
                list.TryRemoveEntry(IconKeys.NoPolicy);

                list.TryRemoveEntry(IconKeys.Salvage);

                list.EndAddingEntries();

                return;
            }


            // now show icons for a regular process:

            /*  if (harvestable) // !hasResources)
              {
                  icon = AddOrGetIcon(IconKeys.NoResource, list, "lcd_icon_gather", attainableColor);

                  text = "Attainable, but needs to be harvested from a resource with the GATHER action. Examine the tooltip to see the resource.";               

                  icon.ToolTip = text;

              }
              else
              {
                  list.TryRemoveEntry(IconKeys.NoResource);
              }*/

            /*
            if (canGatherOrUseSpecialAction)
            {
                icon = AttainableInfo.AddOrGetIcon(IconKeys.NoResource, list, "lcd_icon_gather", attainableColor);

                if (harvestable)
                {
                    text = "Attainable, but needs to be harvested from a resource with the GATHER action. Examine the tooltip to see the resource.";
                }
                else
                {
                    text = "Attainable, but needs a special action to be performed. Examine the tooltip to determine how.";
                }

                icon.ToolTip = text;

            }
            else
            {
                list.TryRemoveEntry(IconKeys.NoResource);
            }*/

            if (!hasInputs)
            {
                icon = AddOrGetIcon(IconKeys.NoInput, list, "lcd_icon_stockpile", attainableColor);

                text = "Attainable, but inputs are needed. Examine the tooltip to determine what is missing."; //"  The following inputs (materials/ingredients) are needed, but are also not attainable: \n";

                /* delim = "";
                 foreach (var item in UnavailableInputs)
                 {
                     text += delim + item.PluralName;
                     delim = " \n";
                 }*/

                icon.ToolTip = text;
            }
            else
            {
                list.TryRemoveEntry(IconKeys.NoInput);

            }

            if (!hasTools)
            {
                icon = AddOrGetIcon(IconKeys.NoTool, list, "lcd_icon_tool", attainableColor);

                int toolCounter = 0;
                text = "Attainable, but some tools are needed. Examine the tooltip to determine what is missing."; //"Not attainable. One of these tools is needed, but they are not attainable either: \n";

                /*string toolDelim = "";
                foreach (var item in UnavailableTools)
                {
                    if (toolCounter == 3)
                    {
                        text += "...";
                        break;
                    }

                    text += toolDelim + item.Name;
                    toolDelim = ", ";

                    toolCounter++;
                }*/

                icon.ToolTip = text;
            }
            else
            {
                list.TryRemoveEntry(IconKeys.NoTool);
            }
            // }

            list.EndAddingEntries();

            /* 
                 if (UnavailableSkill != null)
                 {
                     string sprite;
                   
                     sprite = "lcd_icon_person";                   

                     icon = AddOrGetIcon(IconKeys.NoSkill, list, sprite);

                     text = "Not attainable. No one has the needed skill: " + UnavailableSkill.Name;
                     icon.ToolTip = text;
                 }
                 else
                 {
                     list.TryRemoveEntry(IconKeys.NoSkill);
                     //text = "Currently, there is no way of getting the materials needed to produce this. Exploration may help. \n \n";            
                 }

                */

        }




        public static UIComponent AddOrGetIcon(IconKeys iconKey, HorizontalList list, string sprite, Color color)
        {
            UIComponent icon;
            if (!list.TryGetEntry(iconKey, out icon))
            {
                icon = new Icon(list.guiManager);

                Icon iconAsIcon = icon as Icon;

                list.AddEntry(iconKey, icon);

                Rectangle rect = list.guiManager.GUISpriteSheet.GetSourceRectangle(sprite);
                iconAsIcon.SetSkinLocation(SkinState.Normal,rect, color, color);

                iconAsIcon.ResizeControlToFitImage();
            }

            return icon;
        }


        public static UIComponent AddOrGetIcon(TierOrAreaType areaTier, HorizontalList list, Color color)
        {
            UIComponent icon;
            if (!list.TryGetEntry(IconKeys.NoPolicy, out icon))
            {
                icon = new Icon(list.guiManager);

                Icon iconAsIcon = icon as Icon;

                list.AddEntry(IconKeys.NoPolicy, icon);

                Rectangle rect = list.guiManager.GUISpriteSheet.GetSourceRectangle(areaTier.Icon);
                iconAsIcon.SetSkinLocation(SkinState.Normal,rect, color, color);

                iconAsIcon.ResizeControlToFitImage();
            }

            return icon;
        }

        private static void PopulateNotAttainableIcons(HorizontalList list, AttainableInfo info, int noOfProcesses) //, bool includesSalvageProcesses)
        {
            list.BeginAddingEntries();

            string text = "";
            string delim = "";

            UIComponent icon;

            Color unattainableColor = GameData.Instance.GUIConstants.UnattainableColor;

            if (noOfProcesses > 1) // ProcessCount > 1)
            {
                icon = AddOrGetIcon(IconKeys.MultipleProcesses, list, "lcd_icon_asterisk", unattainableColor);
                icon.ToolTip = "There is more than one way of producing this item, but none of them are attainable.";

                list.TryRemoveEntry(IconKeys.NoProcess);
                list.TryRemoveEntry(IconKeys.NoSkill);
                list.TryRemoveEntry(IconKeys.NoResource);
                list.TryRemoveEntry(IconKeys.NoInput);
                list.TryRemoveEntry(IconKeys.NoTool);

                list.EndAddingEntries();

                return; // don't show the rest.
            }
            else
            {
                list.TryRemoveEntry(IconKeys.MultipleProcesses);
            }

            if (info == null) // NoProcess == true)
            {
                icon = AddOrGetIcon(IconKeys.NoProcess, list, "lcd_icon_noEntry", unattainableColor);

                if (The.InGameUI.InventorySettings.IncludeSalvageProcesses) //  includesSalvageProcesses)
                {
                    text = "Not attainable. We have no way of producing this.";
                }
                else
                {
                    text = "Not attainable. We have no way of producing this (HOWEVER: There may/may not be salvage options available!)"; //mp: i added (There may/may not be salvage options available) because it depends on the Salvage filter button.
                }

                icon.ToolTip = text;

                list.TryRemoveEntry(IconKeys.NoSkill);
                list.TryRemoveEntry(IconKeys.NoResource);
                list.TryRemoveEntry(IconKeys.NoInput);
                list.TryRemoveEntry(IconKeys.NoTool);

                list.EndAddingEntries();

                return; // don't show the rest.
            }
            else
            {
                list.TryRemoveEntry(IconKeys.NoProcess);
            }
            /*  else if (UnavailableInputs != null || UnavailableResource != null || UnavailableSkill != null || UnavailableTools != null)
              {*/
            //  list.TryRemoveEntry(IconKeys.NoProcess);



            if (info.UnavailableSkill != null)
            {
                string sprite;

                sprite = "lcd_icon_person";


                icon = AddOrGetIcon(IconKeys.NoSkill, list, sprite, unattainableColor);

                //  text = "No one has the SKILLS needed to produce this.";
                text = "Not attainable. No one has the needed skill: " + info.UnavailableSkill.Name;
                icon.ToolTip = text;
            }
            else
            {
                list.TryRemoveEntry(IconKeys.NoSkill);
            }


            if (info.UnavailableResource != null)
            {
                icon = AddOrGetIcon(IconKeys.NoResource, list, "lcd_icon_gather", unattainableColor);
                text = "Not attainable. The following resource is needed, but has not been discovered (exploration may help): " + info.UnavailableResource.Name;
                icon.ToolTip = text;
            }
            else
            {
                list.TryRemoveEntry(IconKeys.NoResource);
            }

            if (info.UnavailablePolicy != null)
            {
                icon = AddOrGetIcon(info.UnavailablePolicy, list, unattainableColor);
                text = "Not attainable. " + info.UnavailablePolicy.GetNotAvailableTooltip(); // The following policy needs to be enacted first: " + info.UnavailablePolicy.ToString();
                icon.ToolTip = text;
            }
            else
            {
                list.TryRemoveEntry(IconKeys.NoPolicy);
            }


            if (info.UnavailableSpecialSite != null)
            {
                icon = AddOrGetIcon(IconKeys.NoSpecialEntity, list, "lcd_icon_star", unattainableColor);
                text = "Not attainable. The following special site is needed, but has not been discovered (exploration may help): " + info.UnavailableSpecialSite.Name;
                icon.ToolTip = text;
            }
            else
            {
                list.TryRemoveEntry(IconKeys.NoSpecialEntity);
            }

            if (info.UnavailableInputs != null)
            {
                icon = AddOrGetIcon(IconKeys.NoInput, list, "lcd_icon_stockpile", unattainableColor);

                text = "Not attainable. The following inputs (materials/ingredients) are needed, but are also not attainable: \n";

                delim = "";
                foreach (var item in info.UnavailableInputs)
                {
                    text += delim + item.PluralName;
                    delim = " \n";
                }

                icon.ToolTip = text;
            }
            else
            {
                list.TryRemoveEntry(IconKeys.NoInput);

            }

            if (info.UnavailableTools != null)
            {
                icon = AddOrGetIcon(IconKeys.NoTool, list, "lcd_icon_tool", unattainableColor);

                int toolCounter = 0;
                text = "Not attainable. One of these tools is needed, but they are not attainable either: \n";
                string toolDelim = "";
                foreach (var item in info.UnavailableTools)
                {
                    if (toolCounter == 3)
                    {
                        text += "...";
                        break;
                    }

                    text += toolDelim + item.Name;
                    toolDelim = ", ";

                    toolCounter++;
                }

                icon.ToolTip = text;
            }
            else
            {
                list.TryRemoveEntry(IconKeys.NoTool);
            }
            /* }
             else
             {
                 list.TryRemoveEntry(IconKeys.NoProcess);                
             }*/

            list.EndAddingEntries();

        }

        private void UpdateItemRowNormalProduction(EntityGroup owner, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems, Dictionary<ProcessType, AttainableInfo> attainableInfo, EntityType entityType, ProcessType processType,
           bool hasTools, bool hasInputs, bool hasSkills, bool hasResources, int productionLimit, int? outputBatchAmount, ProcessType.ProductionUI processProductionMethod) // bool canOrderFromInventory)
        {

            // first update (Fill):
            // set the check button state according to orders

            // later updates:
            // set fillable bar according to check button


            // first see if button was checked/unchecked this session:
            // standing order mode:
            // has checked standing order in this session 
            
            bool canProduceNow = productionLimit > 0;

         
            if (btBuild != null)
            {
                btBuild.Visible = false;
            }

            /*
            ImageButton btStandingOrder = null;
            if (GameData.Instance.GUIConstants.EnableStandingOrders)
            {              
                btStandingOrder = (ImageButton)itemRow.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock);
            }*/

            UWGame.SimSide.Expeditions.ProductionOrder stockTarget;
            if (owner != null)
            {
                bool showWarning = false;

                if (processProductionMethod == ProcessType.ProductionUI.Build //processProductionMethod != ProcessType.ProductionUI.None) // canOrderFromInventory)
                    || processProductionMethod == ProcessType.ProductionUI.Slider)
                {
                    if (GameData.Instance.GUIConstants.EnableStandingOrders)
                    {
                        btStandingOrder.Visible = true;

                        // make sure the hotspot is visible, but setting the state changes the padlock child button too.
                        /*   bool padlockIsVisible = btStandingOrder.Visible;                   
                           btStandingOrder.Visible = padlockIsVisible; // don't change the state
                           */

                        if (owner.ProductionOrders.Orders.TryGetValue(entityType, out stockTarget)) // all Items have an entry.
                        {
                           /* if (isFirstUpdate)
                            {*/
                                SetPadlockButtonState(stockTarget.AmountToKeepInStore.HasValue, btStandingOrder);
                           // }


                            if (btStandingOrder.IsChecked)
                            {
                                UpdateStandingOrderProduction(productionLimit, outputBatchAmount, canProduceNow, stockTarget, fillableBar, btStandingOrder);

                            }
                            else
                            {
                                UpdateDirectProduction(owner, productionLimit, outputBatchAmount, canProduceNow, stockTarget, ref showWarning, fillableBar, btStandingOrder);
                            }

                        }
                        else
                        {
                            fillableBar.Visible = false; // process is gather or other which cannot yet be given from the inventory panel

                            btStandingOrder.Visible = false;
                        }
                    }
                    else
                    {
                        // tutorial etc.
                        //btStandingOrder.Visible = false; 

                        if (owner.ProductionOrders.Orders.TryGetValue(entityType, out stockTarget)) // all Items have an entry.
                        {
                            UpdateDirectProduction(owner, productionLimit, outputBatchAmount, canProduceNow, stockTarget, ref showWarning, fillableBar, null);
                        }
                    }
                }
                else
                {
                    if (fillableBar != null)
                    {
                        fillableBar.Visible = false; // process is gather or other which cannot yet be given from the inventory panel
                    }

                    if (GameData.Instance.GUIConstants.EnableStandingOrders && btStandingOrder != null)
                    {
                        btStandingOrder.Visible = false;
                    }
                }

               /* HorizontalList hzNotAttainable = itemRow.FindChildById(UIComponent.DataControlID.NotAttainableIcons) as HorizontalList;
                HorizontalList hzAttainable = itemRow.FindChildById(UIComponent.DataControlID.AttainableIcons) as HorizontalList;
                */

                if (fillableBar == null || fillableBar.Visible == false)
                {
                    // the slider control is hidden for salvage, gather and special actions

                  
                    if (processProductionMethod == ProcessType.ProductionUI.Other /*None*/ && canProduceNow == true)
                    {
                        // salvage, gather, special action. processes that are initiated outside the inventory.
                        // give information about giving orders here
                        UpdateNonInventoryOrders(allAvailableItems, processType, hzAttainable, hzNotAttainable);
                    }
                    else
                    {
                        // cannot produce now. show either green or red icons:
                        UpdateAttainable(attainableInfo, hzAttainable, hzNotAttainable,
                            hasTools, hasInputs, hasSkills, hasResources, canProduceNow, processType);

                    }
                }
                else
                {
                    hzAttainable.Visible = false;
                    hzNotAttainable.Visible = false;
                }


               // UIComponent warning = itemRow.FindChildById(UIComponent.DataControlID.Warning, true);
                if (icWarning != null)
                {
                    if (showWarning) //canOrderFromInventory && OrderedItemsRequireWarning(expedition, currentOrder))
                    {
                        icWarning.Visible = true;
                    }
                    else
                    {
                        icWarning.Visible = false;
                    }
                }
            }
        }


        //  public const int maxStandingOrder = 50;

       /* private bool IsDraggingSlider(FillableBar fillableBar)
        {
            return fillableBar == sliderBeingDragged;
        }*/


        private void UpdateStandingOrderProduction(int productionLimit, int? outputBatchAmount, bool canProduceNow, ProductionOrder stockTarget, FillableBar fillableBar, ImageButton btStandingOrder)
        {
            fillableBar.Visible = true; // can always set orders

            int currentOrder = stockTarget.AmountToKeepInStore ?? 0;

            fillableBar.ColorAllControls = GameData.Instance.GUIConstants.StandingOrderTint; // Color.Cornsilk;
            btStandingOrder.NormalColor = GameData.Instance.GUIConstants.StandingOrderTint;

            // don't limit the slider based on inputs. use a fixed maximum, like 50...

            bool sliderValuesWereChanged = false;

            fillableBar.StepSize = 1;

          //  fillableBar.MaxValue = GameData.Instance.GUIConstants.UnlimitedStandingOrderValue; // 99;            
            fillableBar.MaxSliderValueSymbol = "...";
            fillableBar.MaxSliderValueTooltip = "No limit";
            fillableBar.ShowMaxValueLabelAtEnd = false;
              

            if (fillableBar.MaxValue != GameData.Instance.GUIConstants.UnlimitedStandingOrderValue) // MaxStandingOrder)
            {
                fillableBar.MaxValue = GameData.Instance.GUIConstants.UnlimitedStandingOrderValue; // MaxStandingOrder;
                sliderValuesWereChanged = true;
            }


            if (isSliderBeingDragged == false) // !IsDraggingSlider(fillableBar)) // don't change the slider that the user is currently dragging
            {
                if (fillableBar.Value != currentOrder)
                {
                    // NEW
                    int value;
                    if (currentOrder == Sim.HasNoLimitValue)
                    {
                        value = GameData.Instance.GUIConstants.UnlimitedStockpileValue;
                    }
                    else
                    {
                        value = currentOrder;
                    }
                    

                    fillableBar.Value = value; // currentOrder;
                    sliderValuesWereChanged = true;
                }
            }


            if (sliderValuesWereChanged)
            {
                fillableBar.UpdateSliderPosition();
            }
        }

        private void UpdateDirectProduction(EntityGroup owner, int productionLimit, int? outputBatchAmount, bool canProduceNow, UWGame.SimSide.Expeditions.ProductionOrder stockTarget, ref bool showWarning, FillableBar fillableBar, ImageButton btStandingOrder)
        {
            int currentOrder = stockTarget.ProductionJobsToComplete ?? 0;

            if (OrderedItemsRequireWarning(owner, currentOrder))
            {
                showWarning = true;
                //warning.Visible = true;
            }

            fillableBar.ColorAllControls = UIComponent.LCDTint;
            if (btStandingOrder != null)
            {
                btStandingOrder.NormalColor = UIComponent.LCDTint;
            }

            fillableBar.MaxSliderValueSymbol = null;
            fillableBar.MaxSliderValueTooltip = null;
            fillableBar.ShowMaxValueLabelAtEnd = true;

            // update the slider:  
            bool sliderValuesWereChanged = false;

            ((ProductionTargetEventArgs)fillableBar.EventArgs).OutputBatchAmount = outputBatchAmount; // for converting back to Jobs...

            int totalProductionLimit = productionLimit;
            int totalCurrentOrder = currentOrder;
            if (outputBatchAmount.HasValue)
            {
                totalProductionLimit *= outputBatchAmount.Value;
                totalCurrentOrder *= outputBatchAmount.Value;

                fillableBar.StepSize = outputBatchAmount.Value; // set the minimum slider increments to the same as the batch size
            }
            else
            {
                fillableBar.StepSize = 1;
            }

            if (isSliderBeingDragged == false) // !IsDraggingSlider(fillableBar)) // don't change the slider that the user is currently dragging
            {
                // show/limit the slider based on available inputs:
                if (canProduceNow == false) // productionLimit == 0)
                {
                    // cannot produce NOW
                    fillableBar.Visible = false;

                }
                else
                {
                    fillableBar.Visible = true;

                    /*  hzAttainable.Visible = false;
                      hzNotAttainable.Visible = false;*/

                    if (fillableBar.MaxValue != totalProductionLimit)
                    {
                        fillableBar.MaxValue = totalProductionLimit;
                        sliderValuesWereChanged = true;
                    }

                    if (fillableBar.Value != totalCurrentOrder)
                    {
                        fillableBar.Value = totalCurrentOrder;
                        sliderValuesWereChanged = true;
                    }
                }
            }


            if (sliderValuesWereChanged)
            {
                fillableBar.UpdateSliderPosition();
            }

        }


        private void fillableBar_SliderMouseDown(object sender, EventArgs e)
        {
            // don't refresh the slider position once the user has touched it:
            isSliderBeingDragged = true; // (FillableBar)sender;

        }


        public void ResetSliderBeingDragged()
        {
            isSliderBeingDragged = false;
        }

        private void UpdateNonInventoryOrders(Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems, ProcessType processType, HorizontalList hzAttainable, HorizontalList hzNotAttainable)
        {
            hzAttainable.Visible = true;
            hzNotAttainable.Visible = false;

            hzAttainable.BeginAddingEntries();

            hzAttainable.TryRemoveEntry(IconKeys.NoProcess);
            hzAttainable.TryRemoveEntry(IconKeys.NoSkill);
            hzAttainable.TryRemoveEntry(IconKeys.NoResource);
            hzAttainable.TryRemoveEntry(IconKeys.NoPolicy);
            hzAttainable.TryRemoveEntry(IconKeys.NoInput);
            hzAttainable.TryRemoveEntry(IconKeys.NoTool);
            hzAttainable.TryRemoveEntry(IconKeys.Upgrade);
            hzAttainable.TryRemoveEntry(IconKeys.Pseudo);

            Color color = UIComponent.LCDNormal; // GameData.Instance.GUIConstants.AttainableColor;
            UIComponent icon;
            string text = "";

            if (processType.IsSalvageProcess)
            {
                // show the salvage icon

                icon = AddOrGetIcon(IconKeys.Salvage, hzAttainable, "lcd_icon_recycleArrows", color);

                string input = ".";

                if (processType.InputsByType != null && processType.InputsByType.Count > 0)
                {
                    EntityType inputType = processType.InputsByType.First().Key;

                    // only show the hint if we own the item/structure input..
                    InventoryPanel.Availability availability;
                    if (allAvailableItems.TryGetValue(inputType, out availability)
                        && availability.NoOfAvailableItems > 0)
                    {
                        input = ", for example: " + inputType.Name + ".";
                    }
                }

                text = "Attainable from salvaging items or structures" + input; // the items or structures may not exist (loops are possible)

                icon.ToolTip = text;
            }
            else
            {
                hzAttainable.TryRemoveEntry(IconKeys.Salvage);
            }

            if (processType.IsGathering)
            {
                // show the gather icon
                icon = AddOrGetIcon(IconKeys.NoResource, hzAttainable, "lcd_icon_gather", color);
                text = "Attainable, but needs to be harvested from a resource with the GATHER action. Examine the tooltip to see the resource.";


                icon.ToolTip = text;
            }
            else
            {
                hzAttainable.TryRemoveEntry(IconKeys.NoResource);
            }

            /*   AreaTier policy;
               if (!expedition.Policy.CanUseProcess(processType, out policy))
               {
                   icon = AddOrGetIcon(policy, hzAttainable, color);
                   text = "Attainable, but needs to be harvested from a resource with the GATHER action. Examine the tooltip to see the resource.";
                
                   icon.ToolTip = text;
               }
               else
               {
                   hzAttainable.TryRemoveEntry(IconKeys.NoPolicy);
               }*/


            if (processType.IsUpgrade)
            {
                icon = AddOrGetIcon(IconKeys.Upgrade, hzAttainable, "lcd_icon_uparrow", color);
                text = "This is an upgrade and it is constructed with the UPGRADE action. Examine the tooltip to see the objects that can be upgraded.";

                icon.ToolTip = text;
            }
            else
            {
                hzAttainable.TryRemoveEntry(IconKeys.Upgrade);
            }


            if (processType.IsPseudoProcess)
            {
                icon = AddOrGetIcon(IconKeys.Pseudo, hzAttainable, "lcd_icon_pseudo", color);
                text = "This is a pseudo process. Each pseudo process yields its product in a special way. Examine the tooltip to find out how to produce the item";

                icon.ToolTip = text;
            }
            else
            {
                hzAttainable.TryRemoveEntry(IconKeys.Pseudo);
            }


            if (processType.IsSpecialActionType)
            {
                // show the special site (star) icon

                icon = AddOrGetIcon(IconKeys.NoSpecialEntity, hzAttainable, "lcd_icon_special", color);
                text = string.Format(lblImmovableToolTip, processType.ActingOnType.Name);  //"Attainable, but needs a special action to be performed. Examine the tooltip to determine how.";

                icon.ToolTip = text;
            }
            else
            {
                hzAttainable.TryRemoveEntry(IconKeys.NoSpecialEntity);
            }


            hzAttainable.EndAddingEntries();
        }


        public static bool OrderedItemsRequireWarning(EntityGroup entityGroup, int orderedJobs)
        {
            if (orderedJobs > GameData.Instance.GUIConstants.OrderedJobsWithSameOutputToTriggerWarning)
            {
                //int totalJobs = expedition.OwnedEntities.coun 0;
                // expedition.OwnedEntities.ProductionJobs 
                if (TotalJobsRequireWarning(entityGroup))
                {
                    return true;
                }

            }

            return false;
        }

        public static bool TotalJobsRequireWarning(EntityGroup entityGroup) // Expedition expedition)
        {
            //if (expedition.OwnedEntities.TotalDirectOrderProductionJobs > GetMaximumJobsBeforeWarning(expedition))
            if (entityGroup.ProductionOrders.TotalDirectOrders > EntityGroup.GetMaximumJobsBeforeWarning(entityGroup.Parent))
            {
                return true;
            }

            return false;
        }


    }
}
