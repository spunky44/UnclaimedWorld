using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Resources;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class GatherResourcePopup: HUDPopup
    {
        FillableBar fillableBar;

        ResourceType resourceType;

        public GatherResourcePopup()
            : base(200, 160)
        {
            DisplayWindow.Hide();
            
            fillableBar = new FillableBar(gui, FillableBar.FillableBarType.HUDSlider, false, true, 0.5f, 0.1f) 
            { 
                Position = new Point(10, 20),
                Width = 100
            };

            Add(fillableBar);

            fillableBar.SliderMouseUp += new EventHandler(fillableBar_SliderMouseUp);

          //  fillableBar.SliderMouseUp += new InputEventSystem.MouseUpHandler(fillableBar_MouseUp);
        }

        void fillableBar_SliderMouseUp(object sender, EventArgs e)
        {
            base.DisplayWindow.Hide();

            // save the changes
            The.InGameUI.ContextMenu.ZoneGatherResourcesWindow.SaveJobChanges(resourceType, fillableBar.Value);
        }

       



        public void Fill(ResourceType resourceType, int currentOrders, int max)
        {
            this.resourceType = resourceType;

            fillableBar.Value = currentOrders;
            fillableBar.MaxValue = max;
            fillableBar.UpdateSliderPosition();
           
        }
    }
}
