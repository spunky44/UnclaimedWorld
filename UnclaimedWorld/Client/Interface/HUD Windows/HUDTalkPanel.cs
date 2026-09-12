using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    
    /// <summary>
    /// the expanding part of the talk panel  
    /// </summary>
    public class HUDTalkPanel : HUDWindow
    {
        const int minHeight = 26;
        const int defaultHeight = 260;
      //  const int pagerHeight = 48;

        
        
        /// <summary>
        /// the list that has the log messages - it gets a scrollbar automatically when there are more lines than can be displayed with its specified height
        /// </summary>
        Grid grid;
       // ListBox list;

    //    List<TextButton> pagerButtons = new List<TextButton>();


        List<Log.TalkEvent> logdata;
    //    int totalPages;

        /// <summary>
        /// 1 is the page no. with the newest messages
        /// is never 0
        /// 
        /// </summary>
       // int currentPage = 1;


     //   int thisPageButtonIndex;

       
        /// <summary>
        /// cycle between these to visually group conversation messages
        /// </summary>
        private static Color[] conversationColors = new[] { Color.White, Color.Turquoise, Color.LightBlue, Color.LightSkyBlue, Color.LightCyan, Color.MediumAquamarine, Color.LightSeaGreen };


        public HUDTalkPanel(int xPos, int yPos, int width, int height)
            : base(width, height, true, false, false, "HUD_window_base", false)
        {
            base.HideOnRightClick = false;

            
            DisplayWindow.X = xPos;
            DisplayWindow.Y = yPos;

            DisplayWindow.Resizable = false; // true;
         
            DisplayWindow.Level = Level.Bottom; //.RockBottom;
        /*    DisplayWindow.SetResizableArea(ResizeAreas.TopLeft, false);
            DisplayWindow.SetResizableArea(ResizeAreas.Left, false);
            DisplayWindow.SetResizableArea(ResizeAreas.Bottom, false);
            DisplayWindow.SetResizableArea(ResizeAreas.BottomLeft, false);
            DisplayWindow.SetResizableArea(ResizeAreas.BottomRight, false);
            */

           
           // DisplayWindow.MinWidth = 120;
         //   DisplayWindow.MaxWidth = maxDisplayWindowWidth;
            
            DisplayWindow.Show();



            //PlacePageButtons();


            grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow); // .Comm);
            //list.RenderType = RenderType.CRTAndLCD;          
            DisplayWindow.Add(grid);
            grid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grid.Color = Color.White;
          //  grid.Width = DisplayWindow.Width - 2 * singleSpacing;
            //grid.ItemHeight = 20;  
            grid.FixedItemHeights = false;
          //  grid.FixedItemHeights = false; // line breaks???
            SetListWidth();
            SetListHeight();
            grid.Position = new Point(10, 10); //new Point(display.X + 10, display.Y + 10);
            grid.ZOrder = 1.0f;
            grid.InsertNewRows = Grid.NewRowsInsertion.First;


            maxEntries = entriesPerMessage * The.Sim.Controller.Options.TalkLogMessagesToShow;
        }

        private void SetListHeight()
        {
            grid.Height = DisplayWindow.Height - 2 * grid.Y - 20; // -pagerHeight - messageGridYPos; //- 20;

            // adjust pager buttons too:
          /*  foreach (var item in pagerButtons)
            {
                item.Y = grid.Bottom + singleSpacing;
            }*/
        }

        private void SetListWidth()
        {
            grid.Width = DisplayWindow.Width - 2 * grid.X - 20; 
                        
        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            SetListHeight();

            SetListWidth();
        }

        /// <summary>
        /// we store these for quick re-use. should there be a limit... for 1000 perhaps?
        /// </summary>
      //  private Dictionary<uint, UIComponent> messageRowCache = new Dictionary<uint, UIComponent>();

        const int maxRowsToStore = 1000;

        /// <summary>
        /// adds 2 entries to the grid
        /// </summary>
        /// <param name="talkEvent"></param>
        private void AddMessage(Log.TalkEvent talkEvent)
        {
            
            string text = talkEvent.Line;

           
            
            Entity speaker = Entity.FindByID(talkEvent.SpokenBy);
            if (speaker != null)
            {

                Color? color = GetConversationColor(talkEvent);

                // add from the top
                UIComponent lineRow = grid.AddEntry(talkEvent.ID, text, true, color);
                lineRow.Tag1 = talkEvent.ID;

                UIComponent speakerRow = grid.AddEntry(speaker.EntityID + " " + talkEvent.ID, speaker.ToLink(true), false, color);
                speakerRow.Tag1 = talkEvent.ID;
            }  
            
        }


        private static Color? GetConversationColor(Log.TalkEvent talkEvent)
        {
            ulong messageGroupNo = 0;
            if (talkEvent.MessageGroupNo.HasValue) // Conversation != null)
            {
                messageGroupNo = talkEvent.MessageGroupNo.Value; // talkEvent.Conversation.ID;
            }

            return conversationColors[messageGroupNo % (ulong)conversationColors.Length];

        }

        /// <summary>
        /// removes 2 entries from the grid
        /// </summary>
        /// <param name="newEvent"></param>
        private void RemoveOldestMessage()
        {
            grid.RemoveEntry(grid.GetKeyFromIndex(grid.Entries.Count - 1));

            if (grid.Entries.Count > 0)
            {
                grid.RemoveEntry(grid.GetKeyFromIndex(grid.Entries.Count - 1));
            }

        }
        

        /// <summary>
        /// get all the log messages matching the filters
        /// </summary>
        private void GetAllData()
        {
           
            logdata = The.Client.Log.TalkEvents;

            //totalPages = logdata.Count / The.Sim.ScreenManager.UserSettings.LogMessagesPerPage;

        }


        const int noOfPageButtonsToEachSide = 2;

        const int pagerXPos = 210;

      /*  private void PlacePageButtons()
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


            }

        }*/
        /*
        void btPager_Click(UIComponent sender, EventArgs e)
        {
            int pageNo = (int)(sender.Tag1);

            
            if (pageNo != currentPage)
            {
                ((TextButton)sender).IsChecked = false; // the state is set elsewhere.

                currentPage = pageNo;

                Repopulate(pageNo);
            }

        }
        */
        int xPosOfCurrentPageButton;

        const int pageButtonSpacing = 4;
        const int spacingToEndButtons = 12;

      /*  private void RefreshPagerButtons()
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
            btCurrent.ScaleToFitText();
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
                    b.ScaleToFitText();
                    b.X = previousXpos - pageButtonSpacing - b.Width;
                    b.Tag1 = pageNo;

                    previousXpos = b.X;
                }
            
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
                b.ScaleToFitText();
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
                    b.ScaleToFitText();
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
                b.ScaleToFitText();
                b.Tag1 = totalPages;
                b.X = previousXpos + spacingToEndButtons; // pageButtonSpacing;
            }
          // btCurrent.X = 


        }
*/
    /*    private void Repopulate(int pageToShow = 1)
        {
            grid.BeginAddingEntries();

            grid.Clear();

            // the last (newest) log item appears on page 1

            bool isAtEnd = grid.ScrollBarIsAtEnd();

            // subtract one from page no.
            int startIndex = (totalPages - pageToShow + 1) * The.Sim.ScreenManager.UserSettings.LogMessagesPerPage;
            int endIndex = Common.Min(logdata.Count - 1, startIndex + The.Sim.ScreenManager.UserSettings.LogMessagesPerPage);

            TalkEvent e;
            for (int i = startIndex; i < endIndex; i++)
            {
                e = logdata[i];
                AddRow(e);
            }

            grid.EndAddingEntries();

            if (isAtEnd)
            {
                grid.ScrollToIndex(0); 
            }
            
             

            //RefreshPagerButtons();

        }*/


        /// <summary>
        /// when the user pages or changes the filters, we need to repopulate... spread this operation over multiple cycles perhaps.
        /// </summary>
      /*  private void Repopulate(int pageToShow = 1)
        {
            grid.BeginAddingEntries();

            grid.Clear();

            // the last (newest) log item appears on page 1

            
            // subtract one from page no.
            int startIndex = (totalPages - pageToShow + 1) * The.Sim.ScreenManager.UserSettings.LogMessagesPerPage;
            int endIndex = Common.Min(logdata.Count - 1, startIndex + The.Sim.ScreenManager.UserSettings.LogMessagesPerPage);
            
            TalkEvent e;
            for (int i = startIndex; i < endIndex; i++)
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

            //RefreshPagerButtons();

        }*/

        private const int entriesPerMessage = 2;

        private int maxEntries;

        /// <summary>
        /// continually add new messages at the bottom as they appear. if over the limit, also remove from the top
        /// </summary>
        public override void Refresh()
        {   
            GetAllData();

            bool scrollbarIsAtTop = false;
            if (grid.Entries.Count == 0 || grid.ScrollBarIsAtTop())
            {
                scrollbarIsAtTop = true;
            }

            int firstIndex = GetFirstIndexOfNewMessagesToShow();

            TalkEvent lastAddedEvent = null;

            if (firstIndex != -1)
            {
                grid.BeginAddingEntries();


                for (int i = firstIndex; i < logdata.Count; i++)
                {
                    lastAddedEvent = logdata[i];
                    AddMessage(lastAddedEvent);
                }

                // now remove the oldest messages:
                while (grid.Entries.Count > maxEntries) 
                {
                    // remove both the speaker row and the line row:
                    RemoveOldestMessage();

                    //  grid.RemoveEntry(grid.Entries[0].Tag1);
                }                    
               

                grid.EndAddingEntries();

                // if scroll position was at the top
                // scroll to the top:
                if (scrollbarIsAtTop && grid.Entries.Count > 0)
                {
                    grid.ScrollToIndex(0); //grid.Entries.Count - 1);
                }
            }


            if (lastAddedEvent != null)
            {
                The.InGameUI.TalkPanel.ShowSpeaker(lastAddedEvent);
            }            
        }

        private int GetFirstIndexOfNewMessagesToShow()
        {
            int firstIndex = -1; // means no new messages

            uint newestShownMessageID;

            if (grid.Count > 0)
            {
                newestShownMessageID = (uint)(grid.Entries[0].Tag1);
                
                TalkEvent e, lastEvent;

                // scan the data, stop when we reach an event that is already displayed:
                for (int i = logdata.Count - 1; i >= 0; i--)
             //   for (int i = 0; i < logdata.Count; i++)
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

       

    }
}
