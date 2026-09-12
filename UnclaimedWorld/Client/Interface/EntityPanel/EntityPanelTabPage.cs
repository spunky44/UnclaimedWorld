using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface.EntityPanel
{
    public class EntityPanelTabPage//: Panel
    {
        public bool HasCRT = false;

      //  public bool LCDOnly = false;

        protected GUIManager gui;

        public UIComponent lcdContent;
        protected Grid lcdContentGrid;

        public Panel tabPanel;

        public string Title = "";

        protected EntityPanel entityPanel;

        protected const int captionWidth = 100;
        protected const int lineHeight = 18;

        protected const int leftMargin = 10;

        public EntityPanelTabPage(GUIManager gui, EntityPanel entityPanel, bool hasCRT, UIComponent fullLCD, UIComponent halfLCD)
        {
            HasCRT = hasCRT;
            this.gui = gui;
            this.entityPanel = entityPanel;

            if (fullLCD != null)
            {
                lcdContent = new UIComponent(gui);
                lcdContent.RenderType = RenderType.CRTAndLCD;

                if (HasCRT)
                {                    
                    lcdContent.Width = halfLCD.Width;
                    lcdContent.Height = halfLCD.Height;
                }
                else
                {
                    lcdContent.Width = fullLCD.Width;
                    lcdContent.Height = fullLCD.Height;
                }

                lcdContentGrid = CreateOuterGridForContent(gui, lcdContent);
            }
        }

        /*public TabPage(CommonInterface intf, Point pos, Vector2 dimensions): base(intf, pos, dimensions,
            //new Point(Interface.Instance.leftExpanded, 0), new Vector2(Interface.Instance.expandedInterfaceWidth,
            //            Interface.Instance.expandedInterfaceHeight)
            Level.Middle)
        {

        }*/

        //public virtual void ChangeLCD

        public virtual void Refresh()
        {           
        }

        public virtual void Clear()
        {
            lcdContentGrid.Clear();
        }

        /// <summary>
        /// Creates a panel with margins for adding content. best placed in an outer grid.
        /// Important! Vertical margins are not possible - any y position will get reset to 0 if this panel is placed in a grid!!!
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="horizMargin"></param>
        /// <param name="lcdSurface"></param>
        /// <returns></returns>
        public static UIComponent CreatePanelWithMargins(GUIManager gui, int horizMargin, UIComponent lcdSurface)
        {
            UIComponent panel = new UIComponent(gui);
            panel.Width = FullLCDPanel.GetContentWidthFromLCDSurface(lcdSurface) - 2 * horizMargin; // lcdContentGrid.Width - Grid.ScrollBarAndGapMain - 2 * leftMargin;
            panel.X = horizMargin;
            panel.RenderType = RenderType.CRTAndLCD;

            return panel;
        }


        public void AddCaptionAndLabel(UIComponent pnPanel, string caption, ref Label lblValue, int xPos, ref int yPos)
        {
            Label lblCaption = new Label(gui);
            pnPanel.Add(lblCaption);
            lblCaption.Text = caption;
            lblCaption.Init(Label.LabelType.LCDNormal);
            lblCaption.Position = new Point(xPos, yPos);

            lblValue = new Label(gui);
            pnPanel.Add(lblValue);
            //  lblValue.Text = caption;
            lblValue.Init(Label.LabelType.LCDNormal);
            lblValue.Position = new Point(xPos + captionWidth, yPos);
            lblValue.Width = 280;


            yPos += lineHeight;
        }

        public static Grid CreateOuterGridForContent(GUIManager gui, UIComponent lcdSurface)
        {
            int gridTopMargin = 4;

            Grid outerGrid = new Grid(gui, ListBoxType.Main, Label.LabelType.LCDNormal);
            outerGrid.FixedItemHeights = false;
            outerGrid.RenderType = RenderType.CRTAndLCD;
            lcdSurface.Add(outerGrid);
            outerGrid.HMargin = 0; //!!!
            outerGrid.VMargin = 0;
            outerGrid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            outerGrid.Width = lcdSurface.Width;
            outerGrid.Height = lcdSurface.Height - gridTopMargin;
            outerGrid.ItemHeight = 26;//22; 
            outerGrid.Position = new Point(0, gridTopMargin);

            return outerGrid;
        }
    }
}
