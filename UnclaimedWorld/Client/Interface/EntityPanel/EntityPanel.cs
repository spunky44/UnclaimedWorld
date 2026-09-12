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
//using Microsoft.Xna.Framework.Storage;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide;

namespace UWGame.ClientSide.Interface.EntityPanel
{
    /// <summary>
    /// Full screen display of entity info.
    /// not currently used
    /// </summary>
    public class EntityPanel : ExpandedPanel
    {
       
        public TabButtonPanel tabButtonPanel; 

      //  private enum TabPages { Main, Household, Health, History, CombatHistory }

      //  private List<EntityPanelTabPage> AllTabPages = new List<EntityPanelTabPage>();


        private SkillsAndAttributes SkillsAndAttributes;
        private History History;
        private CombatHistory CombatHistory;

        private int? tabPanelY;

        public EntityPanelTabPage CurrentTab;

       // public bool IsDisplayed = false;

        
        UIComponent lcdSurfaceFull;
        UIComponent lcdSurfaceHalf;

        EntityCRTContent entityCRTContent;

        private Window HalfPanel, FullPanel;

        private Entity selectedEntity;
        public Entity SelectedEntity
        {
            // NEW!
            get { return selectedEntity; }
            set 
            { 
                selectedEntity = value;

               /* if (intf.displayedExpandedPanel != null && displayedExpandedPanel == SidePanelEntity.ExpandedPanel)
                {*/
                if (MainPanelIsVisible())
                {
                    RefreshTabPanel();
                }

               // }
                //}
               
            }

        }

        Label lblFullPanelTitle;

        private TabButtonPanel.Layout tabLayout;

        

        public EntityPanel(CommonInterface intf, FramedCRT framedCRT, Point pos, Vector2 dimensions, Level level, bool includeCables, TabButtonPanel.Layout tabLayout, int? tabXPos, int? tabYPos) //, bool shrinkLCD)
            : base("", intf, pos, dimensions, level, includeCables) //"ENTITY")
        {
            HasCRT = true;

            this.tabLayout = tabLayout;
            
            entityCRTContent = new EntityCRTContent(intf.gui, this, framedCRT);

            // close the gap because we have a problem with the tab panel when it goes over the top edge of the half-height lcd panel.
            int crtAndLcdGap = 0; // 4;
            
            // we have two panels (windows) Half and Full. Full is equal to Form. We only show one at a time.
            float halfPanelHeight;
          /*  if (shrinkLCD)
            {
                halfPanelHeight = 176; // TEASER HACK - make smaller for sentry gun combat history
            }
            else
            {*/
                // Use this line on load expedition screen:
                halfPanelHeight = dimensions.Y - framedCRT.DisplayWindow.Height - crtAndLcdGap;
          //  }
            
           /* HalfPanel = CreatePanelWindow(intf.gui,
                new Point(pos.X, framedCRT.DisplayWindow.Y + framedCRT.DisplayWindow.Height + crtAndLcdGap),
                new Vector2(dimensions.X, halfPanelHeight), 
                Level.Middle);*/

            HalfPanel.Hide();
            
            FullPanel = Window;

            //AddIndentedTopTitle(intf.gui, FullPanel, "", out lblFullPanelTitle);
            lblFullPanelTitle.Width = 300;

            Window.Position = pos; // new Point(Interface.Instance.leftExpanded, Interface.Instance.framedCRT.DisplayWindow.Y + Interface.Instance.framedCRT.DisplayWindow.Height);
            Window.Width = (int)dimensions.X;
            Window.Height = (int)dimensions.Y; // Interface.Instance.expandedInterfaceHeight - Interface.Instance.framedCRT.DisplayWindow.Height;

            InitTabPanel(tabXPos, tabYPos);

            InitMainPanels();

            CurrentTab = SkillsAndAttributes;
            //ChangeTab(this.SkillsAndAttributes);

           


       /*     TextButton expand;
            Label label;
            yPos = intf.topMargin;

            foreach (KeyValuePair<ItemType, List<Item>> listOfItemTypes in game.ColonyOwner.OwningBody.Items)
            {
                expand = new xWinFormsLib.TextButton("Expand", new Vector2(expandColumnX, yPos), "[+]", 20, Color.White, intf.InterfaceFont, Form.Style.Default);
                expand.OnPress += new EventHandler(ExpandStockItem_OnPress);
                expand.EventArgs = new ItemButtonEventArgs(listOfItemTypes.Key);
                Form.Add(expand);

                label = new Label("lblItemType", new Vector2(descriptionColumnX, yPos), listOfItemTypes.Key.Name, Label.Alignment.Left, null, Color.Black, intf.InterfaceFont);

                Form.Add(label);
                yPos += 20;
            }*/

        }

        private bool MainPanelIsVisible()
        {
            return HalfPanel.Visible || FullPanel.Visible;
        }
        /*

        public override void Update(GameTime elapsed)
        {
            if (updateRegulator.IsReady())
            {
                Refresh();
            }
            base.Update(elapsed);
        }*/
                
        /// <summary>
        /// this method displays/hides the relevant panels for the new tab.
        /// it doesn't call refresh on it.
        /// </summary>
        /// <param name="newTab"></param>
        private void ChangeTab(EntityPanelTabPage newTab)
        {
            /*if (CurrentTab != newTab)
            {*/
                if (CurrentTab != null && CurrentTab.tabPanel != null)
                {
                    CurrentTab.tabPanel.Hide();
                }

                bool crtWasDisplayed = false;
                if (CurrentTab != null && CurrentTab.HasCRT)
                {
                    crtWasDisplayed = true;
                }

                // exchange tabs:
                CurrentTab = newTab;

                if (CurrentTab.tabPanel != null)
                {
                    FullPanel.Hide();
                    HalfPanel.Hide();

                    CurrentTab.tabPanel.Show();
                }
                else
                {   // lcd content only
                    // change the lcd content only:      
                    if (CurrentTab.HasCRT)
                    {
                        FullPanel.Hide();
                        HalfPanel.Show();

                        lcdSurfaceHalf.Controls.Clear();
                        lcdSurfaceHalf.Add(CurrentTab.lcdContent);
                    }
                    else 
                    {
                        FullPanel.Show();
                        HalfPanel.Hide();

                        lblFullPanelTitle.Text = CurrentTab.Title;

                        lcdSurfaceFull.Controls.Clear();
                        lcdSurfaceFull.Add(CurrentTab.lcdContent);
                    }
                }
          

                //CurrentTab.Refresh();

                if (CurrentTab.HasCRT)
                {
                    entityCRTContent.Show(!crtWasDisplayed);
                }
                else
                {
                    entityCRTContent.Hide();
                }

                if (tabButtonPanel.tabPanel.Visible)
                {
                    tabButtonPanel.tabPanel.BringToTop();
                }

          //  }
           
        }

        private void RefreshTabPanel()
        {
           // AllTabPages.Clear();
            tabButtonPanel.Clear();

            if (SelectedEntity != null)
            {

                if (SelectedEntity.Intelligence != null && SelectedEntity.Locomotor.LeggedLocomotor != null)
                {
                    tabButtonPanel.AddButton("STATS", "", SkillsAndAttributes, 3); // (int)TabPages.Main, 3);
                }

                if (SelectedEntity.PersonEntity != null)
                {
                    tabButtonPanel.AddButton("HOUSEHOLD", "", History, 2);//(int)TabPages.Household, 2);
                }

                if (SelectedEntity.BiologicalEntity != null)
                {
                    tabButtonPanel.AddButton("HEALTH", "", History, 1); // (int)TabPages.Health, 1);
                }

                if (SelectedEntity.Intelligence != null &&
                    SelectedEntity.EntityType.IntelligenceType.AttackTypes != null &&
                    SelectedEntity.EntityType.IntelligenceType.AttackTypes.Count > 0)
                {
                    tabButtonPanel.AddButton("COMBAT", "", CombatHistory, 4); // (int)TabPages.History, 4);
                }

                if (SelectedEntity.Intelligence != null)
                {
                    tabButtonPanel.AddButton("HISTORY", "", History, 4); //(int)TabPages.History, 4);
                }

                // ImageButton currentButton = tabButtonPanel.GetButton(CurrentTab);
                TabButtonContainer currentButton = tabButtonPanel.GetButtonContainer(CurrentTab);

                if (tabButtonPanel.GetListOfButtons().Count == 0)
                {
                    tabButtonPanel.Hide();
                }
                else
                {
                    tabButtonPanel.SetSize();
                    SetVerticalTabPanelPosition();

                    tabButtonPanel.Show();



                    if (currentButton != null)
                    {   // 'reselect' the current button
                        currentButton.button.IsChecked = true;
                    }
                    else
                    {   // select the top one...

                        currentButton = tabButtonPanel.GetButton(0);
                        currentButton.button.IsChecked = true;

                        EntityPanelTabPage firstTab = (EntityPanelTabPage)tabButtonPanel.GetButtonArgument(currentButton);
                        if (CurrentTab != firstTab)
                        {
                            ChangeTab(firstTab); // don't change tabs if it is already shown...
                        }
                    }
                }
            }
            else
            {
                tabButtonPanel.Hide();
            }



        }

        private void InitMainPanels()
        {
            Box display, edges;
            LCDScreen lcdScreen = null;

            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(Interface, FullPanel, MarginX /*80*/, new Point(MarginX, MarginX + TitleHeight), 
                out display, /*out edges,*/ out lcdSurfaceFull, ref lcdScreen);

            AddDirtOnEdges(Interface.gui, FullPanel);

            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(Interface, HalfPanel, MarginX /*80*/, new Point(MarginX, MarginX + TitleHeight),
                out display, /*out edges,*/ out lcdSurfaceHalf, ref lcdScreen);

            AddDirtOnEdges(Interface.gui, HalfPanel);


            SkillsAndAttributes = new SkillsAndAttributes(Interface.gui, this, lcdSurfaceFull, lcdSurfaceHalf);
            History = new History(Interface.gui, this, lcdSurfaceFull, lcdSurfaceHalf);
            CombatHistory = new CombatHistory(Interface.gui, this, lcdSurfaceFull, lcdSurfaceHalf);

        }

        private void SetVerticalTabPanelPosition()
        {

            int tabY;
            if (tabPanelY.HasValue)
            {               
                tabY = tabPanelY.Value;
            }
            else
            {
                tabY = HalfPanel.Y;
            }


            if (tabY + tabButtonPanel.tabPanel.Height > HalfPanel.Y + HalfPanel.Height)
            {   // don't go below the edge...
                tabButtonPanel.tabPanel.Y = HalfPanel.Y + HalfPanel.Height - tabButtonPanel.tabPanel.Height;
            }
            else
            {
                tabButtonPanel.tabPanel.Y = tabY;
            }
        }

        private void InitTabPanel(int? tabXPos, int? tabYPos)
        {
            int tabSize = 270; //260;
            int overlap = 0; //8;
            int tabX, tabY;
          /*  if (tabLayout == TabButtonPanel.Layout.Vertical)
            {*/
            tabPanelY = tabYPos;

            


            if (tabXPos.HasValue)
            {
                tabX = tabXPos.Value;
            }
            else
            {
                tabX = Window.X - TabButtonPanel.verticalWidth + overlap;
            }

            tabButtonPanel = new TabButtonPanel(Interface.gui, //tabSize,
                new Point(tabX, HalfPanel.Y),
                TabButtonPanel.Layout.Vertical, Window);
         

            Image bigSplotch = AddImage(Interface.gui, tabButtonPanel.tabPanel, "basic_dirt_bigsplotch", new Point(80, 0));
            
            
          /*  tabButtonPanel.AddButton("HOUSEHOLD", "", (int)TabPages.Household, 2);
            tabButtonPanel.AddButton("HEALTH", "", (int)TabPages.Health, 1);
            tabButtonPanel.AddButton("HISTORY", "", (int)TabPages.History, 4);
            */

            tabButtonPanel.TabButtonEvent += new TabButtonPanel.TabButtonHandler(topPanel_TabButtonEvent);

            

            tabButtonPanel.Hide();
            
        }

        void topPanel_TabButtonEvent(object buttonArgument) //int buttonIndex)
        {
            ChangeTab((EntityPanelTabPage) buttonArgument);
            /*
            switch ((TabPages) buttonIndex)
            {
                case TabPages.Main:
                    ChangeTab(SkillsAndAttributes);
                    break;
                case TabPages.Household:
                    //ChangeTab(
                    break;
                case TabPages.History:
                    ChangeTab(History);
                    break;
                case TabPages.Health:

                default:
                break;
            }*/

            if (SelectedEntity != null)
            {
                entityCRTContent.Refresh(SelectedEntity);
                CurrentTab.Refresh();
            }
            else
            {
                CurrentTab.Clear();
            }
        }

                

     /*   void bt_Click(UIComponent sender, EventArgs e)
        {
            framedCRT.Switch();
        }

        void btN_Click(UIComponent sender, EventArgs e)
        {
            framedCRT.PlayInterference();
        }

        void btNo_Click(UIComponent sender, EventArgs e)
        {
            framedCRT.PlayNoReception();
        }*/

        public override void Show()
        {
           // framedCRT.Show();
                  
            //Now display the fresh data on the CRT screen:
            //ChangeTab(this.SkillsAndAttributes);
                 
            base.Show(); // calls Refresh()!
            ChangeTab(CurrentTab);
            
            // important that we are shown last to overlap!!!
            tabButtonPanel.Show();

            RefreshTabPanel();
        }

        public override void Hide()
        {
          //  framedCRT.Hide();
            tabButtonPanel.Hide();
            HalfPanel.Hide();
            FullPanel.Hide();

            base.Hide();
        }

        
              
       
        

        /// <summary>
        /// Setup the display depending on what is being shown...
        /// </summary>
        public override void Refresh()
        {
           // Entity entity = Interface.Instance.SelectedEntity;

            if (SelectedEntity != null)
            {
                entityCRTContent.Refresh(SelectedEntity);
                CurrentTab.Refresh();

                //RefreshTabPanel(); // we won't be refreshing the tab buttons regularly, only when the entity changes.
            }
            else
            {
                CurrentTab.Clear();
            }

            base.Refresh();
        }


        
        public override void DrawContent(Window sender, SpriteBatch formSpriteBatch)
        {
            if (SelectedEntity != null)
            {
                //if (displayRegulator.IsReady())
                //displayRegulator = new Regulator(intf.Game.ScreenManager.Random, 2);

            }

            /*  yPos = topMargin;
              foreach (KeyValuePair<ItemType, List<Item>> listOfItemTypes in ColonyOwner.OwningBody.Items)
              {
                  //  formSpriteBatch.DrawString(InterfaceFont, listOfItemTypes.Key.Name, new Vector2(descriptionColumnX, yPos), Color.Black);
                  formSpriteBatch.DrawString(InterfaceFont, listOfItemTypes.Value.Count.ToString(), new Vector2(quantityColumnX, yPos), Color.Black);

                  yPos += 20;
              }*/
        }
    }
}
