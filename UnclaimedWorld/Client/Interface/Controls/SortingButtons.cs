using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Controls
{
    public class SortingButtons<T> : RadioGroup where T : struct,  IComparable, IFormattable, IConvertible // the closest constraint to enum we can get..
    {
      //  private Dictionary<int, ICanBeChecked> buttons;
        private Dictionary<T, ICanBeChecked> buttons = new Dictionary<T,ICanBeChecked>();

        public event Action SortClicked;


        public SortingSettings<T> Settings;

        public SortingButtons(GUIManager gui): base(gui)
        {
            Height = 50;
        }

     /*   public void Add(ICanBeChecked control, int sortIndex)
        {
            buttons.Add(sortIndex, control);

            ((UIComponent)control).Click += tbSort_Click;

            base.Add(control, true);
        }*/


        public void Fill(SortingSettings<T> settings) //T index, Grid.Sorting order)
        {
            Settings = settings;

          

            ICanBeChecked bt = buttons[settings.SortedBy];
            bt.IsChecked = true;

            SetSortOrderIcons(bt, settings.SortOrder);
                  
        }

        public void EnableButton(T sortIndex, bool enable)
        {
            ((UIComponent)buttons[sortIndex]).Enabled = enable;
        }

        public void AddTextButton(int width, string text, T sortIndex, string tooltip = null)
        {
            int x = 0;

            if (buttons.Count > 0)
            {
                x = ((UIComponent)buttons.Last().Value).Right;
            }

            CreateTextButton(x, width, text, sortIndex, tooltip);

        }

        public void CreateTextButton(int x, int width, string text, T sortIndex, string tooltip = null)
        {
            TextButton tb = new TextButton(guiManager);
            tb.Width = width;
            tb.Init(TextButton.TextButtonType.LCDSortingArrows);
            tb.CheckedMode = CheckedModes.CanBeChecked;
            tb.Y = 0;     
            tb.Text = text;
            tb.X = x;
            tb.Width = width;
            tb.Height = 30;
            tb.OrderByTag1 = null;
            tb.Click += tbSort_Click;

            InitButton(tb, text, tooltip);

            tb.Tag1 = sortIndex; // InventorySettings.SortColumns.Name;
            buttons.Add(sortIndex, tb);

        }

       /* private TextButton CreateSortingTextButton(int x, int width, string text)
        {
           

            return tb;
        }*/

        private void InitButton(ICanBeChecked button, string text, string tooltip)
        {
            string tooltipToUse;
            if (tooltip != null)
            {
                tooltipToUse = tooltip;
            }
            else
            {
                tooltipToUse = text;
            }

            ((UIComponent)button).ToolTip = "Sort by:" + tooltipToUse;

            base.Add(button, true);
        }

        /// <summary>
        /// not possible to have 2 icons on a button...
        /// </summary>
        /// <param name="x"></param>
        /// <param name="width"></param>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        /// <param name="sortIndex"></param>
        public void CreateImageButton(int x, int width, /*string icon,*/ string text, T sortIndex)
        {           
            ImageButton tb = new ImageButton(guiManager);
            tb.Width = width;
            tb.Init(ImageButtonType.LCDSortingArrows);
            tb.CheckedMode = CheckedModes.CanBeChecked;
            tb.Click += tbSort_Click;
            tb.Y = 0;
            tb.X = x;
            tb.Width = width;
           // tb.Height = 30;
            tb.OrderByTag1 = null;
            InitButton(tb, text, null);

            tb.Tag1 = sortIndex; // InventorySettings.SortColumns.Name;
            buttons.Add(sortIndex, tb);

        }

       
        private void tbSort_Click(UIComponent sender, EventArgs e)
        {
            ICanBeChecked tb = sender as ICanBeChecked;
           

           // InventorySettings.SortColumns clickedColumn = (InventorySettings.SortColumns)sender.Tag1;
            T clickedColumn = (T)sender.Tag1;

            if (EqualityComparer<T>.Default.Equals(clickedColumn, Settings.SortedBy))
               // clickedColumn == (T)Settings.SortedBy)
            {
                // clicked the same column
                if (Settings.SortOrder == Grid.Sorting.Descending)
                {
                    Settings.SortOrder = Grid.Sorting.Ascending;
                }
                else if (Settings.SortOrder == Grid.Sorting.Ascending)
                {
                    Settings.SortOrder = Grid.Sorting.Descending;
                }
            }
            else
            {
                // set to default
                Settings.SetDefaultSortOrder();
                Settings.SortedBy = clickedColumn;
            }

            SetSortOrderIcons(tb, Settings.SortOrder);


            if (SortClicked != null)
            {
                SortClicked.Invoke();
            }

        }

        public void SetSortOrderIcons(ICanBeChecked clickedTextButton, Grid.Sorting order) // bool name, bool instock, bool canproduce, bool tracked)
        {
            UpdateSortDirectionIcon((UIComponent)clickedTextButton,
                order, true);

            foreach (var item in Controls)
            {
                if (item != clickedTextButton)
                {
                    UpdateSortDirectionIcon(item, null, false);
                }
            }

            /*   Icon icon;
               if (clickedTextButton != null)
               {
                   icon = clickedTextButton.Controls[2] as Icon; // HACK!!!
               }
               else
               {
                   icon = clickedImageButton.Controls[0] as Icon;
               }

               icon.CurrentSkin = The.InGameUI.InventorySettings.SortOrder == Grid.Sorting.Ascending ? 1 : 0;

               tbSortName.Controls[2].Visible = name;
               tbSortTracked.Controls[0].Visible = tracked;
               tbSortInStock.Controls[0].Visible = instock;
               tbSortCanProduce.Controls[0].Visible = canproduce;

               tbSortName.Pressed = name;
               tbSortTracked.Pressed = tracked;
               tbSortInStock.Pressed = instock;
               tbSortCanProduce.Pressed = canproduce;*/
        }

        private void UpdateSortDirectionIcon(UIComponent button, Grid.Sorting? order, bool showDirection)
        {
            Icon icon;
            TextButton tb = button as TextButton;
            ICanBeChecked canBeChecked = button as ICanBeChecked;

            if (tb != null)
            {
                icon = tb.Controls[2] as Icon; // HACK!!!
            }
            else
            {
                ImageButton ib = button as ImageButton;
                icon = ib.Controls[0] as Icon; // HACK!!!
            }

            if (showDirection)
            {
                icon.CurrentSkin = order.Value == Grid.Sorting.Ascending ? 1 : 0;
                icon.Visible = true;

                // canBeChecked.IsChecked = true;
            }
            else
            {
                icon.Visible = false;

                //  canBeChecked.IsChecked = false;
            }

        }

    }
}
