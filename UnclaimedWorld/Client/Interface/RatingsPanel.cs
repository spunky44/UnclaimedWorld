using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.SimSide.Allegiances.Statistics;

namespace UWGame.ClientSide.Interface
{
    public class RatingsPanel: Panel
    {
        Label lblSecurity;
        Label lblFoodSupply;
        Label lblComfort;

       /* bool foodRatingIsDirty = true;
        bool securityRatingIsDirty = true;
        bool comfortRatingIsDirty = true;
        */

        Image imSecurityProgress, imFoodProgress, imComfortProgress;

        public RatingsPanel(): base(The.InGameUI, null, new Microsoft.Xna.Framework.Point(325, 0),
             new Vector2(150, 69), WindowSystem.Level.Middle, PanelType.Ratings)
        {
            Window.Show();

            int buttonYPos = 3;
            int buttonDistance = 4;

            ImageButton btNutrition = new ImageButton(Interface.gui);
            btNutrition.Init(ImageButtonType.FoodRating);
            Window.Add(btNutrition);
            btNutrition.X = 16;
            btNutrition.Y = buttonYPos;
            btNutrition.Click += btNutrition_Click;
            btNutrition.ToolTip = "Click to see the nutrition graph";
           // btNutrition.Enabled = false;

            ImageButton btSecurity = new ImageButton(Interface.gui);
            btSecurity.Init(ImageButtonType.SecurityRating);
            Window.Add(btSecurity);
            btSecurity.X = btNutrition.Right + buttonDistance;
            btSecurity.Y = buttonYPos;
            btSecurity.Click += btSecurity_Click;
            btSecurity.ToolTip = "Click to see the security graph";
          //  btSecurity.Enabled = false;

            ImageButton btComfort = new ImageButton(Interface.gui);
            btComfort.Init(ImageButtonType.ComfortRating);
            Window.Add(btComfort);
            btComfort.X = btSecurity.Right + buttonDistance;
            btComfort.Y = buttonYPos;
            btComfort.Click += btComfort_Click;
            btComfort.ToolTip = "Click to see the comfort graph";
          //  btComfort.Enabled = false;

            int labelYPos = 31;

            Label.LabelType labelType = Label.LabelType.PlainPanelNormal; // not LCD...

            int tooltipWidth = 280;

            lblFoodSupply = new Label(Interface.gui);
            Window.Add(lblFoodSupply);
            lblFoodSupply.Init(labelType);
            lblFoodSupply.Text = The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(RatingTypes.Food).ToString();
            lblFoodSupply.FitToText();
            lblFoodSupply.Y = labelYPos;
            lblFoodSupply.ToolTip = " - "; // triggers tooltip on hover
            lblFoodSupply.TooltipRequested += lblFoodSupply_TooltipRequested;
            lblFoodSupply.TooltipExpires = false;
            lblFoodSupply.TooltipWidth = tooltipWidth;
         //   lblFoodSupply.TooltipDisplayChange += lblFoodSupply_TooltipDisplayed;
            // when the tooltip is shown, set a flag in statistics to generate the text also.

            lblSecurity = new Label(Interface.gui);
            Window.Add(lblSecurity);
            lblSecurity.Init(labelType);
            lblSecurity.Text = The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(RatingTypes.Security).ToString();
            lblSecurity.FitToText();
            lblSecurity.Y = labelYPos;
            lblSecurity.ToolTip = " - "; // triggers tooltip on hover
            lblSecurity.TooltipRequested += lblSecurity_TooltipRequested;
            lblSecurity.TooltipExpires = false;
            lblSecurity.TooltipWidth = tooltipWidth;
         //   lblSecurity.TooltipDisplayChange += lblSecurity_TooltipDisplayed;
          
            lblComfort = new Label(Interface.gui);
            Window.Add(lblComfort);
            lblComfort.Init(labelType);
            lblComfort.Text = The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(RatingTypes.Comfort).ToString();
            lblComfort.FitToText();
            lblComfort.Y = labelYPos;
            lblComfort.ToolTip = " - "; // triggers tooltip on hover
            lblComfort.TooltipRequested += lblComfort_TooltipRequested;
            lblComfort.TooltipExpires = false;
            lblComfort.TooltipWidth = tooltipWidth;
          //  lblComfort.TooltipDisplayChange += lblComfort_TooltipDisplayed;
         
            // little arrows
            int iconYPos = 46;

            imSecurityProgress = new Image(Interface.gui);
            imSecurityProgress.ScaleImageToSizeOfControl = false;
            imSecurityProgress.Y = iconYPos;
            Window.Add(imSecurityProgress);

            imFoodProgress = new Image(Interface.gui);
            imFoodProgress.ScaleImageToSizeOfControl = false;
            imFoodProgress.Y = iconYPos;
            Window.Add(imFoodProgress);

            imComfortProgress = new Image(Interface.gui);
            imComfortProgress.ScaleImageToSizeOfControl = false;
            imComfortProgress.Y = iconYPos;
            Window.Add(imComfortProgress);

            Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
            Image dirtLeft = AddImage(Interface.gui, Window, rect, new Point(0, 0));
           // dirtLeft.RenderType = RenderType.Overlay;

            // NEW: always compose breakdowns for player allegiance stats.
            RequestRatingsBreakdown(RatingTypes.Food, true);
            RequestRatingsBreakdown(RatingTypes.Comfort, true);
            RequestRatingsBreakdown(RatingTypes.Security, true);

            RefreshContent();
           
        }

        private void OpenGraphRosterPanel(Graphs graphType)
        {
            The.InGameUI.GraphPanel.SelectGraphType(graphType);

            The.InGameUI.ChangeRosterPanel(The.InGameUI.GraphPanel);
            
        }


        void btComfort_Click(UIComponent sender, EventArgs e)
        {
            OpenGraphRosterPanel(Graphs.ComfortRating);
        }

        void btSecurity_Click(UIComponent sender, EventArgs e)
        {
            OpenGraphRosterPanel(Graphs.SecurityRating);
        }

        void btNutrition_Click(UIComponent sender, EventArgs e)
        {
            OpenGraphRosterPanel(Graphs.FoodRating);
        }

       /* void lblSecurity_TooltipDisplayed(UIComponent obj)
        {
            
        }*/

      /*  void lblComfort_TooltipHidden(UIComponent sender)
        {
            RequestRatingsBreakdown(StatTypes.Comfort, false);
        }*/

        void lblComfort_TooltipDisplayed(UIComponent sender, bool value)
        {
            RequestRatingsBreakdown(RatingTypes.Comfort, value);
        }

      /*  void lblSecurity_TooltipHidden(UIComponent sender)
        {
            RequestRatingsBreakdown(StatTypes.Security, false);
        }*/

        void lblSecurity_TooltipDisplayed(UIComponent sender, bool value)
        {
            RequestRatingsBreakdown(RatingTypes.Security, value);
        }

      /*  void lblFoodSupply_TooltipHidden(UIComponent sender)
        {
            RequestRatingsBreakdown(StatTypes.Food, false);
        }*/

        void lblFoodSupply_TooltipDisplayed(UIComponent sender, bool value)
        {
            RequestRatingsBreakdown(RatingTypes.Food, value);
        }

        void lblComfort_TooltipRequested(UIComponent sender)
        {
            RefreshRatingTooltip(RatingTypes.Comfort, lblComfort); //, ref comfortRatingIsDirty);
        }

        void lblSecurity_TooltipRequested(UIComponent sender)
        {
            RefreshRatingTooltip(RatingTypes.Security, lblSecurity); //, ref securityRatingIsDirty);
        }

        void lblFoodSupply_TooltipRequested(UIComponent sender)
        {
            RefreshRatingTooltip(RatingTypes.Food, lblFoodSupply); //, ref foodRatingIsDirty);                         
         
        }

       

        public override void Refresh()
        {
            RefreshContent();
        }

        private void RefreshContent()
        {
            int distance = 39;
            
            int centerOnPos = 35;

            RefreshRating(RatingTypes.Food, lblFoodSupply, imFoodProgress, centerOnPos); //, ref foodRatingIsDirty);

            centerOnPos += distance;
            RefreshRating(RatingTypes.Security, lblSecurity, imSecurityProgress, centerOnPos); //, ref securityRatingIsDirty);      

            centerOnPos += distance;
            RefreshRating(RatingTypes.Comfort, lblComfort, imComfortProgress, centerOnPos); //, ref comfortRatingIsDirty);  
           
        }

        private void RequestRatingsBreakdown(RatingTypes statType, bool value)
        {
            Rating hasRatingStat = The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(statType);
            
            hasRatingStat.ToggleComposeBreakdown(value);
        }

        private void RefreshRatingTooltip(RatingTypes statType, Label label) //, ref bool isDirty)
        {
           /* if (isDirty)
            {*/
            Rating hasRatingStat = The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(statType);

                label.ToolTip = hasRatingStat.GetRatingsBreakdown();

            /*
                int latestIndex = hasRatingStat.GetLatestDataIndex();
                ((IHasRating)stat).ComposeRatingBreakdown(latestIndex, out rating, tooltip);
                label.ToolTip = tooltip.ToString();
            */
               /* isDirty = false;
            }*/
        }

        private void RefreshRating(RatingTypes statType, Label label, Image image, int centerOnXPos) //, ref bool ratingIsDirty)
        {
            Statistic stat = The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(statType);
            double value = stat.GetLatestValue();
            label.Text = Common.PercentageToString(value); // stat.ToString();
            label.FitToText();
            Window.CenterHorizontally(centerOnXPos, label);
                     

            double change = stat.GetChange();

            string sprite;
            if (Common.IsZero(change))
            {
                sprite = "ratings_arrow_horizontal";
            }
            else if (Common.IsGreaterThan(change, 0d))
            {
                sprite = "ratings_arrow_up";
               // ratingIsDirty = true;
            }
            else
            {
                sprite = "ratings_arrow_down";
              //  ratingIsDirty = true;
            }


            image.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle(sprite)); 
            image.ResizeControlToFitImage();

            Window.CenterHorizontally(centerOnXPos, image);
         
        }


    }
}
