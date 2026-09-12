using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface
{
    public class TabButtonPanel //: Window
    {
       // public delegate void TabButtonHandler(int buttonIndex);
        public delegate void TabButtonHandler(object buttonArgument);
        public event TabButtonHandler TabButtonEvent;

        public bool Visible = true;

        public Window tabPanel;
        Window tabPanelEdge;
        RadioGroup buttonGroup;
        GUIManager gui;
        //int tabWidth;

        // vertical layout:
        public const int verticalWidth = 94; //103; //90; 
        public const int verticalOverlap = 16;
        const int buttonYSpacing = 59; //54; 

       // int tabHeight; 
        Point pos;

        // horizontal layout:
      //  public const int horizontalHeight = 70; //96;
        const int buttonXSpacing = 69; //68; //54; 

        
        private const int metalPosX = 12; 
        private const int metalPosY = 12; //5; 

        private const int markingsTop = 10; //7; //19;

        /// <summary>
        /// distance from metal edge to markings start
        /// </summary> 
        private const int markingsLeft = 6; //4; //12;

        private Image imBottomDirt;

        Box metal;

       // Dictionary<ImageButton, int> tabButtons = new Dictionary<ImageButton, int>();
        Dictionary<TabButtonContainer, object> tabButtons = new Dictionary<TabButtonContainer, object>();
        Dictionary<ImageButton, TabButtonContainer> tabButtonContainers = new Dictionary<ImageButton, TabButtonContainer>();
        List<TabButtonContainer> listOfTabButtons = new List<TabButtonContainer>();
       
       // ImageButton btPersonnel, btItems, btVehicles, bt;

        public enum Layout { Horizontal, Vertical }

        private Layout layout;

        public TabButtonPanel(/*out Window tabPanel, out Window tabPanelEdge, out RadioGroup buttonGroup,*/
            GUIManager gui, /*int tabSize,*/ Point pos, Layout layout, Window formToLevelWith)
        {
            this.gui = gui;
            this.layout = layout;

            // put tab buttons here


            /*   tabPanel = CreatePanelWindow(intf.gui,
                   new Point(Form.X, Form.Y - tabHeight),
                   new Vector2(tabWidth, tabHeight), Form.Level, PanelOptions.SteelAndDust);
               */

            //********
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("tabsbase"); //"help_panel"); 

            tabPanel = new Window(gui);
            // Sequence matters for skins!!!
            tabPanel.Skin = rect; // gui.GUISpriteSheet.SourceRectangle("event_displaypanel"); 
            tabPanel.CornerSize = 15; // 7;

            // add textures:
            Panel.AddSteelTexture(gui, tabPanel);
            Panel.AddDust(gui, tabPanel);

            
            tabPanel.Margin = 0; // 7;                   


            tabPanel.Position = pos; //new Point(Form.X, Form.Y - tabHeight + overlap); // dimensions.Top);

            
            /*
            if (layout == Layout.Horizontal)
            {
                tabPanel.WindowSize = new Vector2(tabSize, horizontalHeight); //, "Main Panel", Color.White, Color.Black, "tahoma", 1f, false, false, false, false, Form.BorderStyle.None, Form.Style.Default));
            }
            else
            {
                tabPanel.WindowSize = new Vector2(verticalWidth + verticalOverlap, tabSize);
            }*/
            
            tabPanel.Show(); //Make it visible
            tabPanel.Resizable = false;
            tabPanel.IsMovable = false;
            tabPanel.HasCloseButton = false;
            tabPanel.Level = formToLevelWith.Level; // Level.Bottom;
            tabPanel.ZOrder = 0;

            tabPanel.HasCRTOrLCDComponents = false;
            tabPanel.HasOverlayComponents = false;


            if (layout == Layout.Horizontal)
            {
                // do the ribbed edge:
                rect = gui.GUISpriteSheet.GetSourceRectangle("tabribs_right"); //"help_panel"); 

                tabPanelEdge = new Window(gui);
                // Sequence matters for skins!!!
                tabPanelEdge.Skin = rect; // gui.GUISpriteSheet.SourceRectangle("event_displaypanel"); 
                tabPanelEdge.CornerSize = 2; //1; // 7;

                tabPanelEdge.Margin = 0; // 7;    

                tabPanelEdge.Position = new Point(tabPanel.X + tabPanel.Width, tabPanel.Y); // dimensions.Top);
                tabPanelEdge.WindowSize = new Vector2(rect.Width, rect.Height); //, "Main Panel", Color.White, Color.Black, "tahoma", 1f, false, false, false, false, Form.BorderStyle.None, Form.Style.Default));
            
                tabPanelEdge.Show(); //Make it visible
                tabPanelEdge.Resizable = false;
                tabPanelEdge.IsMovable = false;
                tabPanelEdge.HasCloseButton = false;
                tabPanelEdge.Level = tabPanel.Level;
                tabPanelEdge.ZOrder = 0;

                tabPanelEdge.HasCRTOrLCDComponents = false;
                tabPanelEdge.HasOverlayComponents = false;

                ExpandedPanel.AddDirtOnLeftEdge(gui, tabPanelEdge);
                ExpandedPanel.AddDirtOnBottomEdge(gui, tabPanelEdge);
            }
                     
            //*********

            // not as important as SetSize()
            if (layout == Layout.Horizontal)
            {
                metal = Panel.AddMetalPlate(gui, tabPanel, new Point(metalPosX, metalPosY), new Point(20, 20)); //tabSize - 2 * metalPosX, horizontalHeight - 2 * metalPosY));
            }
            else
            {   // vertical
                metal = Panel.AddMetalPlate(gui, tabPanel, new Point(metalPosX, metalPosY), new Point(20, 20)); //78, tabSize - 2 * metalPosY));
            }

            ExpandedPanel.AddDirtOnLeftEdge(gui, tabPanel);
            imBottomDirt = ExpandedPanel.AddDirtOnBottomEdge(gui, tabPanel);

            SetSize();

            buttonGroup = new RadioGroup(gui);
            tabPanel.Add(buttonGroup);


            
/*

            btPersonnel = MainControlPanel.AddHorizontalMetalPanelButton(gui, tabPanel, buttonLeft, buttonTop, 1, 2, "PERSONNEL");
            btPersonnel.Click += new ClickHandler(personnel_Click);
            buttonGroup.Add(btPersonnel);
            btPersonnel.IsChecked = true;
        //    btPersonnel.DebugTag = "horizMetalButton";

            //  column1.ToolTip = "Shows the counters in the first column.";

            btItems = MainControlPanel.AddHorizontalMetalPanelButton(gui, tabPanel, buttonLeft + buttonXSpacing, buttonTop, 1, 1, "SUPPLIES");
            btItems.Click += new ClickHandler(items_Click);
            buttonGroup.Add(btItems);
            //   column2.ToolTip = "Shows the counters in the second column.";

            btAnimals = MainControlPanel.AddHorizontalMetalPanelButton(gui, tabPanel, buttonLeft + 2 * (buttonXSpacing), buttonTop, 4, 1, "ANIMALS");
            btAnimals.Click += new ClickHandler(animals_Click);
            buttonGroup.Add(btAnimals);

            //    column3.ToolTip = "Shows the counters in the third column.";
            btVehicles = MainControlPanel.AddHorizontalMetalPanelButton(gui, tabPanel, buttonLeft + 3 * (buttonXSpacing), buttonTop, 4, 2, "VEHICLES");
            btVehicles.Click += new ClickHandler(vehicles_Click);
            buttonGroup.Add(btVehicles);
            */

            //topPanel.Hide();

        }

        public int X
        {
            get { return tabPanel.X; }
            set 
            { 
                tabPanel.X = value;
                PlacePanelEdge();
            }
        }

        public int Y
        {
            get { return tabPanel.Y; }
            set
            {
                tabPanel.Y = value;
                PlacePanelEdge();
            }
        }

        public void SetSize()
        {

            if (layout == Layout.Horizontal)
            {

                int width = 2 * (metalPosX + markingsLeft) + listOfTabButtons.Count * buttonXSpacing;
                int metalHeight = buttonYSpacing + markingsTop - 4; 
                int height = metalHeight + 2 * metalPosY - 2; // -2 to fit with the edge graphics...
                tabPanel.WindowSize = new Vector2(width, height);   //horizontalHeight); //, "Main Panel", Color.White, Color.Black, "tahoma", 1f, false, false, false, false, Form.BorderStyle.None, Form.Style.Default));
            
                metal.Width = width - 2 * metalPosX;
                metal.Height = metalHeight; // horizontalHeight - metalPosY; 

                PlacePanelEdge();
            }
            else
            { // vertical layout
                int height = 2 * (metalPosY + markingsTop) + tabButtons.Count * buttonYSpacing - 12;
                tabPanel.WindowSize = new Vector2(verticalWidth + verticalOverlap, height);

                metal.Width = buttonXSpacing + 2 * markingsLeft; // 78;
                metal.Height = height - 2 * metalPosY;

                if (imBottomDirt != null)
                {
                    imBottomDirt.Y = 260; // height - imBottomDirt.Height;
                }
            }

            
        }

        private void PlacePanelEdge()
        {
            if (tabPanelEdge != null)
            {
                tabPanelEdge.Height = tabPanel.Height;

                tabPanelEdge.X = tabPanel.X + tabPanel.Width;
                tabPanelEdge.Y = tabPanel.Y;
            }
        }

        public List<TabButtonContainer> GetListOfButtons()
        {
            return listOfTabButtons;
        }

        public TabButtonContainer GetButton(int index)
        {
            return listOfTabButtons[index];
        }

        public object GetButtonArgument(TabButtonContainer button)
        {
            return tabButtons[button];
        }


        public TabButtonContainer GetButtonContainer(object buttonArgument)
        {
            foreach (KeyValuePair<TabButtonContainer, object> kvp in tabButtons)
            {
                if (kvp.Value == buttonArgument)
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        

       /* public void SetChecked(int index)
        {
            
        }*/

        /// <summary>
        /// Markings flavour 2 is for long text!
        /// </summary>
        /// <param name="text"></param>
        /// <param name="toolTip"></param>
        /// <param name="index"></param>
        /// <param name="markingsFlavour"></param>
        /// <returns></returns>
        public TabButtonContainer AddButton(string text, string toolTip, object buttonArgument, int markingsFlavour)
        {
            // TODO: flavours
           /* ImageButton bt;

            if (layout == Layout.Horizontal)
            {
                bt = MainControlPanel.AddHorizontalMetalPanelButton(gui, tabPanel, metalPosX + buttonLeft + tabButtons.Count * buttonXSpacing,
                    metalPosY + buttonTop, (tabButtons.Count % 4) + 1, markingsFlavour, text);
            }
            else
            {
                bt = MainControlPanel.AddHorizontalMetalPanelButton(gui, tabPanel, metalPosX + buttonLeft,
                    metalPosY + buttonTop + tabButtons.Count * buttonYSpacing, (tabButtons.Count % 4) + 1, markingsFlavour, text);
          
            }*/

            TabButtonContainer bt;

            if (layout == Layout.Horizontal)
            {
                bt = new TabButtonContainer();
                bt.AddHorizontalMetalPanelButton(gui, tabPanel, metalPosX + markingsLeft + tabButtons.Count * buttonXSpacing,
                    metalPosY + markingsTop, (tabButtons.Count % 4) + 1, markingsFlavour, text);
            }
            else
            {
                bt = new TabButtonContainer();
                bt.AddHorizontalMetalPanelButton(gui, tabPanel, metalPosX + markingsLeft,
                    metalPosY + markingsTop + tabButtons.Count * buttonYSpacing, (tabButtons.Count % 4) + 1, markingsFlavour, text);

            }
                
            bt.button.Click += new ClickHandler(bt_Click);
            buttonGroup.Add(bt.button);
            tabButtons.Add(bt, buttonArgument);
            listOfTabButtons.Add(bt);
            tabButtonContainers.Add(bt.button, bt); // XXX only used for getting the container from a clicked button...

            // resize to fit the new button:
            SetSize();

            return bt;

        }

        public void Clear()
        {
            foreach (TabButtonContainer button in listOfTabButtons)
	        {               
                tabPanel.Remove(button.button);
                tabPanel.Remove(button.label);
                tabPanel.Remove(button.markings);
                // also remove the labels...
            }

            tabButtons.Clear();
            buttonGroup.Clear();
            listOfTabButtons.Clear();
            tabButtonContainers.Clear();
           
        }

        void bt_Click(UIComponent sender, EventArgs e)
        {
            if (TabButtonEvent != null)
            {
                //int buttonIndex = tabButtons[(ImageButton)sender]; 
                object buttonArg = tabButtons[tabButtonContainers[(ImageButton)sender]];
                TabButtonEvent.Invoke(buttonArg);
               // TabButtonEvent.Invoke(buttonIndex);
            }
        }

        public void Hide()
        {
            tabPanel.Hide();
            if (tabPanelEdge != null)
            {
                tabPanelEdge.Hide();
            }
        }

        public void Show()
        {
            if (Visible)
            {
                tabPanel.Show();
                if (tabPanelEdge != null)
                {
                    tabPanelEdge.Show();
                }
            }
        }
    }
}
