using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowSystem
{
    /// <summary>
    /// container for the tab items and the top row of buttons that access the tab items
    /// </summary>
    public class TabControl: UIComponent
    {
        private RadioGroup rgAccessButtons;

        public List<TabPage> TabPages = new List<TabPage>();

        public event Action<TabPage> NewPageSelected;

        public TabPage DisplayedTabPage
        {
            get;
            private set;
        }

        /// <summary>
        /// sits below the access buttons, scrolls the displayed tab item which can grow higher than this
        /// </summary>
        Grid grdSurface;

        const int accessHeight = 36;

        Box headerBanner;

        private Dictionary<TabPage, ICanBeChecked> accessButtons = new Dictionary<TabPage,ICanBeChecked>();

        /// <summary>
        /// creates a TabControl that fills the surface except for an optional bottom margin
        /// </summary>
        /// <param name="guiManager"></param>
        /// <param name="surface"></param>
        /// <param name="bottomMargin"></param>
        public TabControl(GUIManager guiManager, UIComponent surface, int bottomMargin = 0): base(guiManager)
        {
            this.DebugTag = "tabControl";

            headerBanner = new Box(guiManager);
            headerBanner.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("basic_header_big"));
            headerBanner.Height = accessHeight;
            headerBanner.Width = surface.Width;
            headerBanner.CornerSize = 18;
            Add(headerBanner);


            rgAccessButtons = new RadioGroup(guiManager);
            headerBanner.Add(rgAccessButtons);
            rgAccessButtons.ButtonMargin = 6;
          //  rgAccessButtons.Y = topMargin;
            rgAccessButtons.DebugTag = "rgAccessButtons";
            rgAccessButtons.NewMemberChecked += rgAccessButtons_NewMemberChecked;

            grdSurface = new Grid(guiManager, ListBoxType.LCD, Label.LabelType.LCDNormal);
            grdSurface.FixedItemHeights = false;
            grdSurface.RenderType = RenderType.Normal; 
           // surface.Add(grdSurface);
            Add(grdSurface);
            grdSurface.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grdSurface.Width = surface.Width;
          /*  grdSurface.Position = new Point(0, rgAccessButtons.Bottom); // topMargin);
            grdSurface.Height = surface.Height - grdSurface.Y - bottomMargin;    */      
            grdSurface.CanHaveFocus = false; // ?? canHaveFocus;

            this.Width = surface.Width;
            this.Height = surface.Height - bottomMargin;

            surface.Add(this);

            SetHeight();            
        }

        void rgAccessButtons_NewMemberChecked(ICanBeChecked arg1, EventArgs arg2)
        {
            TabPage page = (TabPage)((UIComponent)arg1).Tag1;
            SelectTab(page);

            if (NewPageSelected != null)
            {
                NewPageSelected.Invoke(page);
            }
        }

        private void SetHeight()
        {
            grdSurface.Position = new Point(0, headerBanner.Bottom + 6); 
            grdSurface.Height = Height - grdSurface.Y;
        }

        public void SelectTab(TabPage tabItem)
        {
            if (DisplayedTabPage != null)
            {
                grdSurface.RemoveEntry(DisplayedTabPage);
            }

            DisplayedTabPage = tabItem;
            grdSurface.AddEntry(tabItem, tabItem);


            rgAccessButtons.SelectMember(accessButtons[tabItem]);
        }

        public void AddTabPage(TabPage tabPage, string caption, string tooltip)
        {
            TabPages.Add(tabPage);
            tabPage.Width = grdSurface.Width;

            RadioButton rbAccess = new RadioButton(guiManager);
            rbAccess.Init(CheckBoxType.LCDRadio);
            rbAccess.Text = caption;
            rbAccess.ToolTip = tooltip;
            rbAccess.FitToText();
            rbAccess.DebugTag = "rbAccess";
            rbAccess.Tag1 = tabPage;

            rgAccessButtons.Add(rbAccess, true, true);

            rgAccessButtons.Height = rbAccess.Height;
            rgAccessButtons.Width = rbAccess.Right;

            headerBanner.CenterChildVertically(rgAccessButtons);

            accessButtons.Add(tabPage, rbAccess);

            if (DisplayedTabPage == null)
            {
                SelectTab(tabPage);
            }

            SetHeight();            
        }
       

        public TabPage CreateTabItem(string caption, string tooltip)
        {
            TabPage tabPage = new TabPage(guiManager);

            AddTabPage(tabPage, caption, tooltip);


            return tabPage;
        }


       

        public void AddTabItem(string caption, string tooltip, TabPage tabItem)
        {
            
            RadioButton rbAccess = new RadioButton(guiManager);
            rbAccess.Init(CheckBoxType.LCD);
            rbAccess.Text = caption;
            rbAccess.ToolTip = tooltip;
            
            rgAccessButtons.Add(rbAccess, true, true);

            rgAccessButtons.Height = rbAccess.Height;
            rgAccessButtons.Width = rbAccess.Right;

        }


    }
}
