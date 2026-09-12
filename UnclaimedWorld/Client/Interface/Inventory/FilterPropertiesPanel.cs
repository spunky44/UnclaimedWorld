using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Inventory
{
    /// <summary>
    /// contains the drop down with filters to select from, the filter buttons and delete all filters button
    /// </summary>
    public class FilterPropertiesPanel: UIComponent
    {
        ComboBox cbFilter;
        HorizontalList hzlFilters;
        Grid grdHrzFilterContainer;
        ImageButton ibRemoveAllFilters;//, ibExpand;

        TextBox tbSearch;
        ImageButton ibSearch;

      //  TextButton tbExpand;

        const string addFilterPromptKey = "ADDFILTER";


        public event Action FiltersChanged;

        FilterPropertySettings FilterPropertySettings;
      //  Dictionary<string, FilterSetting> ActiveFilterSettings;


        public FilterPropertiesPanel(GUIManager gui, bool makeRoomForExpandButton): base(gui)
        {           

            int yPos = 0;
            if (makeRoomForExpandButton)
            {               
                yPos = 32; // tbExpand.Bottom + 6;
                this.Height = 100; 
            }
            else
            {
                this.Height = 72;
            }
           

            cbFilter = new ComboBox(gui, ListBoxType.LCDCombo, false);
           // filterPanel.AddContent(cbFilter);
            Add(cbFilter);
            cbFilter.Init(ComboBoxTypes.LCD);
            cbFilter.X = 4;
            cbFilter.Y = yPos; // ibExpand.Bottom + 6;
            cbFilter.Width = 185;
            PopulateFilterSettingsCombo();
            cbFilter.SelectedIndex = 0;
            cbFilter.SelectionChanged += cbFilter_SelectionChanged;
            cbFilter.ToolTip = "Select filter";
            cbFilter.DebugTag = "cbFilter";


            grdHrzFilterContainer = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            grdHrzFilterContainer.FixedItemHeights = false;
            grdHrzFilterContainer.RenderType = RenderType.Normal;
            Add(grdHrzFilterContainer); // filterPanel.AddContent(grdHrzFilterContainer);
            grdHrzFilterContainer.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grdHrzFilterContainer.Width = 278; //310;
            grdHrzFilterContainer.Height = 75;
            grdHrzFilterContainer.Position = new Point(222, 0);  //new Point(190, 0); 
            grdHrzFilterContainer.CanHaveFocus = true;
            grdHrzFilterContainer.ScrollBarEnabled = true;
            grdHrzFilterContainer.CanGrowInHeight = false;
            grdHrzFilterContainer.DebugTag = "filterGrid";

            grdHrzFilterContainer.BeginAddingEntries();
            
            this.hzlFilters = new HorizontalList(gui);
            hzlFilters.CenterItemsVertically = true;
            grdHrzFilterContainer.AddEntry(hzlFilters, hzlFilters);
            hzlFilters.HorizontalSpacing = 6;
            hzlFilters.Y = 5;
            hzlFilters.MaxWidth = grdHrzFilterContainer.Width - 10;
            hzlFilters.X = 0;
            hzlFilters.RefreshEntries();

            grdHrzFilterContainer.EndAddingEntries();


            ibRemoveAllFilters = new ImageButton(gui);
            Add(ibRemoveAllFilters); // filterPanel.AddContent(ibRemoveAllFilters);
            ibRemoveAllFilters.InitWithIcon(ImageButtonType.LCD, "HUD_icon_trash", false);
            ibRemoveAllFilters.Click += tbRemoveAllFilters_Click;
            ibRemoveAllFilters.ToolTip = "Remove all filters";
            ibRemoveAllFilters.X = 190; // 158;
           /* if (makeRoomForExpandButton)
            {*/
                ibRemoveAllFilters.Y = 0;
           /* }
            else
            {
                ibRemoveAllFilters.Y = cbFilter.Bottom + 7;
            }*/
            ibRemoveAllFilters.Height = 30;
            ibRemoveAllFilters.Width = 30;
            ibRemoveAllFilters.Visible = false;
            ibRemoveAllFilters.SetIconTint(GameData.Instance.GUIConstants.sidePanelTextColor);
            ibRemoveAllFilters.RecalculateIconPosition();

            tbSearch = new TextBox(gui);
            tbSearch.Init(TextBox.TextBoxType.LCD);
            tbSearch.IsEditable = true;
            Add(tbSearch);
            tbSearch.Width = 142; // cbFilter.Width;
            tbSearch.X = cbFilter.X;
            tbSearch.Y = cbFilter.Bottom;
            tbSearch.GetFocus += tbSearch_GetFocus;
            tbSearch.LoseFocus += tbSearch_LoseFocus;

            ibSearch = new ImageButton(gui);
            Add(ibSearch); // filterPanel.AddContent(ibRemoveAllFilters);
            ibSearch.InitWithIcon(ImageButtonType.LCD, "HUD_icon_search", true);
            ibSearch.Height = 30;
            ibSearch.Width = 30;
            ibSearch.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;          
            ibSearch.X = tbSearch.Right; // 158;
            ibSearch.Y = tbSearch.Y - 4;
            ibSearch.Click += ibSearch_Click;
            ibSearch.SetIconTint(GameData.Instance.GUIConstants.sidePanelTextColor);
          
            UpdateSearchFilterButton();

        }

        void tbSearch_LoseFocus() 
        {
            The.Client.EnableKeyboardShortcuts = true;
        }

        void tbSearch_GetFocus() //UIComponent sender, EventArgs e)
        {
            The.Client.EnableKeyboardShortcuts = false;
        }

        void ibSearch_Click(UIComponent sender, EventArgs e)
        {
            if (ibSearch.IsChecked)
            {
                FilterPropertySettings.EnableSearchText(tbSearch.Text);
            }
            else
            {
                FilterPropertySettings.DisableSearchText();
            }

            UpdateSearchFilterButton();
            UpdateRemoveAllFiltersButton();

            FireFiltersChangedEvent();
        }


        private void UpdateSearchFilterButton()
        {
            if (ibSearch.IsChecked)
            {
                ibSearch.ToolTip = "Click to deactivate the text filter";
                tbSearch.Enabled = false;
                tbSearch.ToolTip = string.Format("Search term: '{0}' The search text cannot be changed when the search button is active.", tbSearch.Text);
            }
            else
            {
                ibSearch.ToolTip = "Click to activate the text filter using the text in the search field";
                tbSearch.Enabled = true;
                tbSearch.ToolTip = "";
            }

        }

        public void Fill(FilterPropertySettings settings)
        {
            this.FilterPropertySettings = settings;

            Populate();

        }

        private void Populate()
        {
            hzlFilters.Clear();

            foreach (var filter in FilterPropertySettings.ActiveFilterSettings)
            {
                AddFilterSettingToHzlList(filter.Value);
            }

            ibSearch.IsChecked = FilterPropertySettings.SearchTextActive;

            tbSearch.Text = FilterPropertySettings.GetSearchText();

            UpdateSearchFilterButton();

            UpdateRemoveAllFiltersButton();
        }


     

        private void tbRemoveAllFilters_Click(UIComponent sender, EventArgs e)
        {
           // hzlFilters.Clear();
            this.FilterPropertySettings.RemoveAllFilterSettings();
          
            FilterPropertySettings.DisableSearchText();
 
            /* tbSearch.Text = "";
            ibSearch.IsChecked = false;
            */
            Populate();

            /*
            UpdateRemoveAllFiltersButton();
            */
            FireFiltersChangedEvent();

            //Refresh();
        }

        private void cbFilter_SelectionChanged(UIComponent sender)
        {
            ComboBox cb = (ComboBox)sender;

            if (!cb.SelectedKey.Equals(addFilterPromptKey))
            {
                FilterSetting filter = (FilterSetting)cb.SelectedKey;

                if (FilterPropertySettings.AddFilterSetting(filter))
                {
                    AddFilterSettingToHzlList(filter);
                }

                cb.SelectedIndex = 0;

                hzlFilters.Sort(Grid.Sorting.Ascending, true);
                hzlFilters.RefreshEntries();

                UpdateRemoveAllFiltersButton();

                FireFiltersChangedEvent();
            }
        }

        

        private void UpdateRemoveAllFiltersButton()
        {
            // only update this when filters are changed
            if (/*The.InGameUI.InventorySettings.*/ FilterPropertySettings.ActiveFilterSettings.Count > 0 || FilterPropertySettings.SearchTextActive)
            {
                ibRemoveAllFilters.Visible = true;
            }
            else
            {
                ibRemoveAllFilters.Visible = false;
            }
        }

        private void FireFiltersChangedEvent()
        {
            if (FiltersChanged != null)
            {
                FiltersChanged.Invoke(); // should call InventoryPanel.Refresh()
            }
        }

        private void AddFilterSettingToHzlList(FilterSetting filter) // object key, string displayName)
        {
            string text = filter.GetDisplayString();
            TextButton btFilter = InventoryPanel.CreateTextButton(this.guiManager, 190, 5, 0, text + "  X ", filter_Click);
            btFilter.ScaleToFitText();
            btFilter.Height = 30;
            btFilter.ID = UIComponent.DataControlID.Filter;
            btFilter.Tag1 = filter; // "Filter tag used: " + filter.FilterSettingType.KeyName; // ???
            btFilter.ToolTip = "Click to remove"; // text;
            btFilter.OrderByTag1 = (float)btFilter.Width;

            hzlFilters.AddEntry(filter, btFilter);
        }

        private void PopulateFilterSettingsCombo()
        {
            var filters = Enum.GetValues(typeof(StaticFilterSettings));

            cbFilter.AddEntry(addFilterPromptKey, "Add Filter:");

            foreach (var item in GameData.Instance.GUIConstants.ProductionFilterSettings)
            {
                FilterSettingType filterType = GameData.Instance.AllFilterSettingTypes[item];
                FilterSetting filterSetting = new FilterSetting(filterType);
                cbFilter.AddEntry(filterSetting, filterSetting.GetDisplayString());
            }
        }

        private void filter_Click(UIComponent sender, EventArgs e)
        {
            //Removes the clicked filter from the active Filters/Categories            

            // string deleteFilter = ((TextButton)sender).Tag1.ToString();
            FilterSetting filter = (FilterSetting)(((TextButton)sender).Tag1);

            hzlFilters.RemoveEntry(filter);

            FilterPropertySettings.RemoveFilterSetting(filter); //.ActiveFilterSettings.Remove(deleteFilter);


            hzlFilters.Sort(Grid.Sorting.Ascending, true);
            hzlFilters.RefreshEntries();

            UpdateRemoveAllFiltersButton();

            FireFiltersChangedEvent();
        }
    }
}
