using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide.Interface.Controls;
using InputEventSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    /// <summary>
    /// necessary to add this to all CommonInterface classes...
    /// </summary>
    public class Tooltip : HUDWindow
    {

        TextArea area;

        //  private double timePassed = 0f;

        public Window SpawningWindow;

        const int width = 200;
        const int height = 80;

        private double timePassed = 0f;

        private const double timeBeforeAppearing = 0.4;

        private const double timeBeforeDisappearing = 7; //mp was: 3.6  but did not have time to read.



        public string Text
        {
            set
            {
                area.Text = value;

              /*  area.FitToLongestEntry();
*/
                DisplayWindow.Width = area.Width + 2 * correctedSideMargin;

                DisplayWindow.Height = area.Height + correctedTopMargin;
                DisplayWindow.CenterChildVertically(area);
            }
        }

        /// <summary>
        /// never take focus
        /// </summary>
        public Tooltip(CommonInterface intf) //int screenWidth, int screenHeight, int screenX)
            : base(width, height, true, false, false, "HUD_windowHint_base", intf: intf)
        {

            UpdateWhileHidden = true;

            base.DisplayWindow.CanHaveFocus = false;

            area = new TextArea(gui, ListBoxType.HUDAndLCD);
            //area.LabelType = Label.LabelType.LCDNormal;
            area.Font = GUIManager.LCDandHUDFont;
            area.Color = Color.Black; // Color.White;
            //  panel.Add(area);
            Add(area); // add calls initialize, that sets font...
            area.X = correctedSideMargin;
            area.Y = correctedTopMargin;
            SetAreaWidth();
            area.Height = DisplayWindow.Height - 2 * topMargin; // 126;
            //area.RenderType = RenderType.CRTAndLCD; // RenderType.Normal; 
            area.ZOrder = 1f;
            DisplayWindow.Level = Level.Tooltip;

            area.CanHaveFocus = false;

            gui.ShowTooltip += new GUIManager.ShowTooltipHandler(gui_ShowTooltip);
            gui.HideTooltip += new GUIManager.ShowTooltipHandler(gui_HideTooltip);
            gui.WindowClosed += new GUIManager.WindowClosedHandler(gui_WindowClosed);

            //  base.DisplayWindow.ViewPort.MouseOut +=new MouseOutHandler(ViewPort_MouseOut);  

        }

        private void SetAreaWidth()
        {
            area.Width = DisplayWindow.Width - 2 * sideMargin; // 196; // triggers BreakText that requires font to be set
        }


        public int Width
        {
            get
            {
                return DisplayWindow.Width;
            }

            set
            {
                DisplayWindow.Width = value;
                SetAreaWidth();
            }
        }


        /* void ViewPort_MouseOut(InputEventSystem.MouseEventArgs args)
         {            
             Hide();            
         }*/

        void gui_ShowTooltip(UIComponent sender)
        {
            StartCountdownToShow(sender);

        }

        void gui_WindowClosed(Window sender)
        {
            // make sure there are no orphan tooltips
            if (SpawningWindow == sender)
            {
                //HideTooltip(tooltipAnchor, this);
                Hide();
            }
        }

        /// <summary>
        /// gets called on every ui element...
        /// </summary>
        /// <param name="sender"></param>
        void gui_HideTooltip(UIComponent sender)
        {
            if (sender.ToolTip == null)
                return;

           // HideTooltip(tooltipAnchor, this);
            this.Hide();

        }

        public void Destroy()
        {
            gui.ShowTooltip -= new GUIManager.ShowTooltipHandler(gui_ShowTooltip);
            gui.HideTooltip -= new GUIManager.ShowTooltipHandler(gui_HideTooltip);

        }

        public override void Update(GameTime elapsed)
        {
            base.Update(elapsed);

            HandleUpdate(elapsed, ref timePassed, tooltipAnchor, this);

        }

        public override void Refresh()
        {
            base.Refresh();

            if (DisplayWindow.IsVisibleAndActive == true)
            {
                Text = (string)tooltipAnchor.ToolTip;
            }
        }

        /// <summary>
        /// called each frame.
        /// shows the tooltip if it is time to appear
        /// </summary>
        /// <param name="elapsed"></param>
        /// <param name="timePassed"></param>
        /// <param name="tooltipAnchor"></param>
        public static void HandleUpdate(GameTime elapsed, ref double timePassed, UIComponent tooltipAnchor, HUDWindow tooltip)
        {
            if (tooltipAnchor != null)
            {               

                timePassed += elapsed.ElapsedGameTime.TotalSeconds;

                InputData inputData = tooltipAnchor.guiManager.InputData; // The.Client.ScreenManager.InputData;


                if (tooltip.DisplayWindow.IsVisibleAndActive == false
                    && timePassed > timeBeforeAppearing)
                {

                    if (tooltipAnchor != null)
                    {

                        if (tooltipAnchor.CheckCoordinates(inputData.mouseX, inputData.mouseY))
                        {

                            Tooltip standardTooltip = tooltip as Tooltip;
                            if (standardTooltip != null) //!string.IsNullOrEmpty(sender.ToolTip))
                            {
                                ShowTooltip(inputData, tooltipAnchor, standardTooltip);
                            }
                            else
                            {
                                // EntityTypeTooltip tooltip = GetTooltip((EntityType)sender.ToolTip);
                                DataSheet entityTypeTooltip = tooltip as DataSheet;

                                ShowDataTypeTooltip(tooltipAnchor, entityTypeTooltip);

                            }
                        }
                        else
                        {
                            // the mouse was moved away before the tooltip could appear. make sure to hide & retire it:
                           // HideTooltip(tooltipAnchor, tooltip);
                            tooltip.Hide();
                        }
                    }

                    // tooltipAnchor = null;
                }
                else 
                {
                    // make the yellow tooltip disappear after a while:
                    Tooltip standardTooltip = tooltip as Tooltip;
                    if (standardTooltip != null
                        && standardTooltip.tooltipAnchor.TooltipExpires == true
                        && tooltip.DisplayWindow.IsVisibleAndActive == true
                        && timePassed > timeBeforeDisappearing)
                    {
                       /* if (tooltipAnchor.CheckCoordinates(inputData.mouseX, inputData.mouseY))
                        {
                            standardTooltip.mousePosWhenTimedOut = new Point(inputData.mouseX, inputData.mouseY);

                        }*/

                        tooltip.Hide();

                    }
                }
            }
        }

      

        private static void ShowTooltip(InputData inputData, UIComponent tooltipAnchor, Tooltip standardTooltip)
        {
            tooltipAnchor.NotifyTooltipShown();

            int x, y;
            standardTooltip.Width = tooltipAnchor.TooltipWidth;

            standardTooltip.Text = (string)tooltipAnchor.ToolTip;
            
            // NEW: anchor to mouse cursor at the far end
            x = inputData.mouseX + 24;
            y = inputData.mouseY + 24;

            // if the tooltip is offscreen, fall back on old anchoring:

            Rectangle tooltipBoundsToTest = new Rectangle(x, y, standardTooltip.DisplayWindow.Width, standardTooltip.DisplayWindow.Height);

            Rectangle drawableArea = new Rectangle(0, 0, standardTooltip.gui.ScreenWidth, standardTooltip.gui.ScreenHeight);
            if (!drawableArea.Contains(tooltipBoundsToTest))
                //!standardTooltip.gui.Game.GraphicsDevice.Viewport.Bounds.Contains(tooltipBoundsToTest))
            {

                standardTooltip.Interface.SelectAnchorPoint(tooltipAnchor, standardTooltip.DisplayWindow, null, standardTooltip.DisplayWindow.Height, false,
                   0,
                    out x, out y, -4);
            }

            standardTooltip.ShowInScreenSpace(x, y);          
        }

        private static void ShowDataTypeTooltip(UIComponent tooltipAnchor, DataSheet entityTypeTooltip)
        {
            int x, y;
            DataTypeButton button = tooltipAnchor as DataTypeButton;


            // is the DataTypeButton button already showing another tooltip (spawned by clicking)?
            DataSheet otherTooltip = The.InGameUI.GetAnySpawnedTooltip(button);
            if (otherTooltip != null)
            {
                // cancel:
                entityTypeTooltip.Hide(); // stays open if pinned!!!
                return;
            }

            /*
            DataTypeTooltip otherTooltip = The.InGameUI.EntityTypeTooltipsStack.Find(tt => tt.SpawningControl == button);
            if (otherTooltip != null
                || The.InGameUI.PinnedDataTypeTooltips.Any(d => d.SpawningControl == button))
            {
                // cancel:
                entityTypeTooltip.Hide();
                return;
            }*/

           
            if (button.IsRoot)
            {
                // close other line of tooltips:
                The.InGameUI.CloseAllEntityTooltips();
            }


            DataTypeButtonEventArgs typeArgs = tooltipAnchor.EventArgs as DataTypeButtonEventArgs;

            The.InGameUI.SelectAnchorPoint(tooltipAnchor, entityTypeTooltip.DisplayWindow, button.SideToAnchorOn, DataSheet.ExpandedHeight, true, DataSheet.GetAnchorPointYOffset(), out x, out y);
            if (typeArgs != null)
            {
                entityTypeTooltip.InitAndShow(tooltipAnchor, typeArgs.Owner, typeArgs.UseUIOwner, x, y);
            }
        }



        int screenPosX, screenPosY;
        UIComponent tooltipAnchor;


        public bool IsShowingTooltipForComponent(UIComponent component)
        {
            if (tooltipAnchor == component) // if (tooltip.DisplayWindow.IsVisibleAndActive
            {
                return true;
            }
            else return false;
        }


        public void StartCountdownToShow(UIComponent sender)
        {
            // only show if the mouse has moved since the tooltip last timed out:
            /*   if (mousePosWhenTimedOut != null)
               {
                   MouseState mouseState = The.InGameUI.input.MouseState;
                   if (
                       mousePosWhenTimedOut.Value.X == mouseState.X
                       && mousePosWhenTimedOut.Value.Y == mouseState.Y)
                   {
                       return;
                   }
               }*/

            this.tooltipAnchor = sender;

            UIComponent parentWindow = sender.FindParentOfType(typeof(Window));
            if (parentWindow != null)
            {
                this.SpawningWindow = (Window)parentWindow;
            }

            timePassed = 0f;
        }

        /*  public override void ShowInScreenSpace(int screenPosX, int screenPosY)
          {
              // don't show yet, just start the countdown to show the tooltip
              //tooltipAnchor = 
              //base.ShowInScreenSpace(screenPosX, screenPosY);

              timePassed = 0f;

          }*/

        public override void Hide()
        {
            base.Hide();

            if (tooltipAnchor != null)
            {
                tooltipAnchor.NotifyTooltipHidden();
                tooltipAnchor = null;
            }

            timePassed = 0f;
        }
    }
}
