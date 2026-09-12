using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.ClientSide.Log;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    
    /// <summary>
    /// the expanding part of the event log panel
    /// the panel only updates in real time when the first page is shown.
    /// </summary>
    public class HUDLog : HUDWindow
    {
        const int minHeight = 26;
        const int defaultHeight = 80;
        const int pagerHeight = 32;

        
        
        /// <summary>
        /// the list that has the log messages - it gets a scrollbar automatically when there are more lines than can be displayed with its specified height
        /// </summary>
        Grid grid;
       // ListBox list;

        List<TextButton> pagerButtons = new List<TextButton>();


        List<Log.Event> logdata;
        int totalPages;

        /// <summary>
        /// 1 is the page no. with the newest messages
        /// is never 0
        /// 
        /// </summary>
        int currentPage = 1;

        /// <summary>
        /// the panel only updates in real time when the first page is shown.
        /// </summary>
    //    bool isOnFirstPage = true;


        int thisPageButtonIndex;

        int messageGridYPos;

        public const int DefaultWidth = 404;

        public HUDLog(int xPos,int width)
            : base(width, defaultHeight, true, false, false, "HUD_window_base", false)
        {
            base.HideOnRightClick = false;

            messageGridYPos = doubleSpacing;

            DisplayWindow.X = xPos-2;
            DisplayWindow.Y = The.Sim.Controller.DrawArea.Height /* .GraphicsDevice.Viewport.Height */ - defaultHeight - 10;

            DisplayWindow.Resizable = false; // true;
            DisplayWindow.SetResizableArea(ResizeAreas.Top, true);
            DisplayWindow.SetResizableArea(ResizeAreas.Right, true);
            DisplayWindow.SetResizableArea(ResizeAreas.TopRight, true);

            DisplayWindow.ResizableBorderSize = messageGridYPos;

            DisplayWindow.Level = Level.Bottom; //.RockBottom;
        /*    DisplayWindow.SetResizableArea(ResizeAreas.TopLeft, false);
            DisplayWindow.SetResizableArea(ResizeAreas.Left, false);
            DisplayWindow.SetResizableArea(ResizeAreas.Bottom, false);
            DisplayWindow.SetResizableArea(ResizeAreas.BottomLeft, false);
            DisplayWindow.SetResizableArea(ResizeAreas.BottomRight, false);
            */

            DisplayWindow.MinHeight = (int)minHeight;
            DisplayWindow.MinWidth = 60;
           // DisplayWindow.MinWidth = 120;
         //   DisplayWindow.MaxWidth = maxDisplayWindowWidth;
            
            DisplayWindow.Resize += new ResizeHandler(DisplayWindow_Resize);

            DisplayWindow.Show();
            
      /*      list = new ListBox(gui, ListBoxType.HUD); // .Comm);
            //list.RenderType = RenderType.CRTAndLCD;          
            DisplayWindow.Add(list);
            list.Font = GUIManager.LCDandHUDFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            list.Color = Color.Black;
            list.Width = DisplayWindow.Width - 2 * singleSpacing;
            SetListHeight();
            list.Position = new Point(singleSpacing, singleSpacing); //new Point(display.X + 10, display.Y + 10);
            list.ZOrder = 1.0f;
            */
          

            PlacePageButtons();

            

            grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow); // .Comm);
            //list.RenderType = RenderType.CRTAndLCD;          
            DisplayWindow.Add(grid);
            grid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grid.Color = Color.White;
          //  grid.Width = DisplayWindow.Width - 2 * singleSpacing;
            grid.ItemHeight = 20;  
          //  grid.FixedItemHeights = false; // line breaks???
            SetListWidth();
            SetListHeight();
            grid.Position = new Point(singleSpacing + 2, messageGridYPos); //new Point(display.X + 10, display.Y + 10);
            grid.ZOrder = 1.0f;
            grid.Parent.Y -= 6;
            grid.ScrollBar.Position = new Point(grid.Width-28,0);   
        }

        int logMessagesPerPage;

        private void SetListHeight()
        {
            grid.Height = DisplayWindow.Height - pagerHeight - messageGridYPos; //- 20;

          
            // adjust pager buttons too:
            foreach (var item in pagerButtons)
            {
                item.Y = grid.Bottom + singleSpacing;
            }


            int newMessagesPerPage = Math.Max(The.Sim.Controller.Options.MinimumLogMessagesPerPage, (grid.Height - 20) / grid.ItemHeight);

            if (newMessagesPerPage != logMessagesPerPage)// NEW
            {
                logMessagesPerPage = newMessagesPerPage;

                GetAllData();

                if (currentPage > totalPages)
                {
                    currentPage = Common.ClampBottom(totalPages, 1);
                }

                int startIndex;
                int endIndex;
                GetMessageIndicesToShow(currentPage, out startIndex, out endIndex);
          
                Repopulate(startIndex, endIndex); 
            }
        }

        private void SetListWidth()
        {
            grid.Width = DisplayWindow.Width - 2 * grid.X; // singleSpacing;
                        
        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            SetListHeight();

            SetListWidth();
        }

        /// <summary>
        /// we store these for quick re-use. should there be a limit... for 1000 perhaps?
        /// </summary>
        private Dictionary<uint, UIComponent> messageRowCache = new Dictionary<uint, UIComponent>();

        const int maxRowsToStore = 1000;

        private void AddRow(Log.Event newEvent)
        {
            UIComponent row;

            if (!messageRowCache.TryGetValue(newEvent.ID, out row))
            {

                string text = FormatLogEntryText(newEvent);

                row = grid.AddEntry(newEvent.ID, text); //, true);

                if (messageRowCache.Count < maxRowsToStore)
                {
                    messageRowCache.Add(newEvent.ID, row);
                }
            }
            else
            {
                grid.AddEntry(newEvent.ID, row);
            }
            
        }

        public static string FormatLogEntryText(Log.Event newEvent, bool includeTime = true)
        {
            

            string text = "";

            if (includeTime)
            {
                string time = newEvent.Time.ToShortTimeString();
                text = time + " ";
            }

            if (newEvent.Entity != null)
            {
                text = string.Format("{0}{1} {2}", text, newEvent.EntityLink, newEvent.Text);
            }
            else
            {
                text = string.Format("{0}{1}", text, newEvent.Text);
            }

            return text;
        }

        

        /// <summary>
        /// get all the log messages matching the filters
        /// </summary>
        private void GetAllData()
        {           
            logdata = The.Client.Log.Events;

            totalPages = (int)Math.Ceiling((float)logdata.Count / logMessagesPerPage);
         //   totalPages = (logdata.Count + logMessagesPerPage - 1) / logMessagesPerPage;
         
        }


        const int noOfPageButtonsToEachSide = 2;

        const int pagerXPos = 210;

        private void PlacePageButtons()
        {
            xPosOfCurrentPageButton = DisplayWindow.Width / 2; // keep centering the current page button, that way the user does not have to move his mouse when paging 1 page at a time

            thisPageButtonIndex = noOfPageButtonsToEachSide + 1; // 'this' page button sits in the middle

            int noOfPagerButtons = noOfPageButtonsToEachSide * 2 + 1 + 2;
            //int currentXPos = pagerXPos;
            for (int i = 0; i < noOfPagerButtons; i++)
            {
                TextButton bt = new TextButton(The.InGameUI.gui);

                if (i == thisPageButtonIndex)
                {
                    bt.Init(TextButton.TextButtonType.HUDHasState); // only the center button will show checked status
                }
                else
                {
                    bt.Init(TextButton.TextButtonType.HUD); //HasState);
                }

                pagerButtons.Add(bt);
                bt.Visible = false;

              //  bt.Y = grid.Bottom + singleSpacing;

                Add(bt);
                bt.Click += new ClickHandler(btPager_Click);
             
            //    bt.Width = 
            //    bt.X = currentXPos;

              //  currentXPos = bt.
            }            

        }

        void btPager_Click(UIComponent sender, EventArgs e)
        {
            int pageNo = (int)(sender.Tag1);

            
            if (pageNo != currentPage)
            {
                ((TextButton)sender).IsChecked = false; // the state is set elsewhere.

                currentPage = pageNo;

                int startIndex;
                int endIndex;
                GetMessageIndicesToShow(currentPage, out startIndex, out endIndex);
          
                Repopulate(startIndex, endIndex);
            }

        }

        int xPosOfCurrentPageButton;

        const int pageButtonSpacing = 4;
        const int spacingToEndButtons = 12;

        private void RefreshPagerButtons()
        {
            foreach (var bt in pagerButtons)
            {
                bt.Visible = false;
                bt.IsChecked = false;

                //Remove(bt);
            }

            if (totalPages == 1)
            {
                return;
            }

            int leftMostPageButton = currentPage - noOfPageButtonsToEachSide;

            leftMostPageButton = Common.ClampBottom(leftMostPageButton, 1);

            bool showPage1AndGap = leftMostPageButton > 2; // the gap is (...)

            int noOfLeftAdjacentButtons = currentPage - leftMostPageButton; // max = noOfPageButtonsToEachSide


            int rightMostPageButton = currentPage + noOfPageButtonsToEachSide;
            rightMostPageButton = Common.ClampTop(rightMostPageButton, totalPages);

            bool showLastPageAndGap = rightMostPageButton < totalPages - 2;

                        

          //  int thisPageButtonIndex = noOfPageButtonsToEachSide + 1 + 1; // 'this' page button
            TextButton btCurrent = pagerButtons[thisPageButtonIndex];
            btCurrent.Text = currentPage.ToString();
            btCurrent.IsChecked = true; // the only button that will ever be checked!
            btCurrent.ScaleWidthToFitText();
            btCurrent.X = xPosOfCurrentPageButton - btCurrent.Width / 2; // center the button at its position
            btCurrent.Visible = true;
            btCurrent.Tag1 = currentPage;

            TextButton b;
            int pageNo = currentPage - 1;
            int previousXpos = btCurrent.X;

            

            // go left from center
            for (int i = thisPageButtonIndex - 1; i >= 0; i--)
            {                          
                b = pagerButtons[i];

                //thisPageButtonIndex--;
                if (pageNo >= leftMostPageButton)
                {
                    b.Visible = true;

                    b.Text = pageNo.ToString();
                    b.ScaleWidthToFitText();
                    b.X = previousXpos - pageButtonSpacing - b.Width;
                    b.Tag1 = pageNo;

                    previousXpos = b.X;
                }
              /*  else if (pageNo == 1)
                {
                    b.Visible = true;
                }*/
                else
                {
                    b.Visible = false;
                }

                pageNo--;
            }

            // set the button to the first page
            if (leftMostPageButton > 1)
            {
                b = pagerButtons[0];
                b.Visible = true;
                b.Text = "1";
                b.ScaleWidthToFitText();
                b.Tag1 = 1;
                b.X = previousXpos - spacingToEndButtons - b.Width;
            }
 
            // go right from center
            pageNo = currentPage + 1;
            previousXpos = btCurrent.Right;
           
            for (int i = thisPageButtonIndex + 1; i < pagerButtons.Count; i++)
            {
                b = pagerButtons[i];

                if (pageNo <= rightMostPageButton)
                {
                    b.Visible = true;

                    b.Text = pageNo.ToString();
                    b.ScaleWidthToFitText();
                    b.X = previousXpos + pageButtonSpacing;
                    b.Tag1 = pageNo;

                    previousXpos = b.Right;
                }              
                else
                {
                    b.Visible = false;
                }

                pageNo++;

            }


            // set the button to the last page
            if (rightMostPageButton < totalPages)
            {
                b = pagerButtons[pagerButtons.Count - 1];
                b.Visible = true;
                b.Text = totalPages.ToString();
                b.ScaleWidthToFitText();
                b.Tag1 = totalPages;
                b.X = previousXpos + spacingToEndButtons; // pageButtonSpacing;
            }
          // btCurrent.X = 


        }

        /// <summary>
        /// when the user pages or changes the filters, we need to repopulate... 
        /// </summary>
        private void Repopulate(int startIndex, int endIndex)
        {
            // the last (newest) log item appears on page 1

          /*  int startIndex;
            int endIndex;
            GetMessageIndicesToShow(pageToShow, out startIndex, out endIndex);
            */

            grid.BeginAddingEntries();

            grid.Clear();

            if (endIndex - startIndex >= 0 && logdata.Count > 0)
            {

                Event e;
                for (int i = startIndex; i <= endIndex; i++)
                {
                    e = logdata[i];
                    AddRow(e);
                }

                grid.EndAddingEntries();

                if (currentPage == 1)
                {
                    //scroll to end
                    grid.ScrollToIndex(grid.Entries.Count - 1);

                }
                else
                {
                    grid.ScrollToIndex(0); // else scroll to top??
                }
            }
            else
            {
                grid.EndAddingEntries();
            }

            RefreshPagerButtons();

        }

        private void GetMessageIndicesToShow(int pageToShow, out int startIndex, out int endIndex)
        {
            // subtract one from page no.      
            if (totalPages > 0)
            {
                startIndex = (totalPages - pageToShow) * logMessagesPerPage; // +1;
             //   startIndex = (totalPages - pageToShow) * (logMessagesPerPage + 1);
                endIndex = Common.Min(logdata.Count - 1, startIndex + logMessagesPerPage - 1);
                
            }
            else
            {
                startIndex = 0;
                endIndex = 0;
            }
        }

        /// <summary>
        /// on page one, continually add new messages at the bottom as they appear. if over the page limit, also remove from the top
        /// </summary>
     /*   private void RefreshFirstPage()
        {
            int previousNoOfPages = totalPages;

            GetAllData();

            bool scrollbarIsAtEnd = false;
            if (grid.Entries.Count == 0 || grid.ScrollBarIsAtEnd())
            {
                scrollbarIsAtEnd = true;
            }

            int firstIndex = GetFirstIndexOfNewMessagesToShow();

            if (firstIndex != -1)
            {
                grid.BeginAddingEntries();


                for (int i = firstIndex; i < logdata.Count; i++)
                {
                    AddRow(logdata[i]);
                }

                // now remove the oldest messages:
                if (grid.Entries.Count > logMessagesPerPage) // The.Sim.Controller.Options.LogMessagesPerPage)
                {
                    while (grid.Entries.Count > logMessagesPerPage) // The.Sim.Controller.Options.LogMessagesPerPage)
                    {
                        grid.RemoveEntry(grid.Entries[0].Tag1);
                    }
                    
                }

                grid.EndAddingEntries();

                // if scroll position was at the end
                // scroll to the new end:
                if (scrollbarIsAtEnd && grid.Entries.Count > 0)
                {
                    grid.ScrollToIndex(grid.Entries.Count - 1);
                }
            }

            if (previousNoOfPages != totalPages)
            {
                RefreshPagerButtons();
            }
        }*/

        private int GetFirstIndexOfNewMessagesToShow()
        {
            int firstIndex = -1; // means no new messages

            uint newestShownMessageID;

            if (grid.Count > 0)
            {
                newestShownMessageID = (uint)(grid.Entries[grid.Count - 1].Tag1);
                
                Event e, lastEvent;

                // scan backwards in the data, stop when we reach an event that is already displayed:
                for (int i = logdata.Count - 1; i >= 0; i--)
                {
                    e = logdata[i];

                    if (e.ID <= newestShownMessageID)
                    {
                        firstIndex = i + 1; // get the previous message we looked at
                        if (firstIndex < logdata.Count) // see that it is within range
                        {
                            return firstIndex;
                        }
                        else return -1;
                    }

                    lastEvent = e;                    
                }
            }
            else
            {
                // repopulate...
                firstIndex = 0;
            }

            return firstIndex;
        }

        public override void Refresh()
        {
            // see if there are new log messages that should be added to the list which fit the filter and page settings:
            if (currentPage == 1) // isOnFirstPage)
            {
                int currentNoOfMessagesOnFirstPage = grid.Entries.Count;

                GetAllData();

                int startIndex;
                int endIndex;
                GetMessageIndicesToShow(currentPage, out startIndex, out endIndex);
          
                int newNoOfMessages = endIndex - startIndex + 1;
                if (currentNoOfMessagesOnFirstPage != newNoOfMessages)
                {
                    Repopulate(startIndex, endIndex);
                }

              //  RefreshFirstPage();                

            }

        }

    }
}
