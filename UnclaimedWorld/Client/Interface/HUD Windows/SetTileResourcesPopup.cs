using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.Maps.MapEditor;
using Microsoft.Xna.Framework;

using UWGame.SimSide.Resources;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
   
    public class SetTileResourcesPopup : HUDPopup
    {
        Label lblHeader;
        Spinner spMeanModifier;/*, spStandardDevModifier*///, spAbsoluteMean, spAbsoluteStandardDev;

        TextBox tbAbsoluteMin, tbAbsoluteMax;

       // Resource resourceData;

        SimSide.Resources.ResourceType resourceType;

        public SetTileResourcesPopup()
            : base(200, 160)
        {
            DisplayWindow.Hide();

            int yPos = 30;

            int distance = 64;
            int leftMargin = 10;

            int x = leftMargin;

            lblHeader = new Label(gui);
            Add(lblHeader);
            lblHeader.Init(Label.LabelType.HUDWindow);
            lblHeader.Position = new Microsoft.Xna.Framework.Point(x, 5);

            Label lbl = new Label(gui);
            Add(lbl);
            lbl.Text = "Modifier: ";
            lbl.Init(Label.LabelType.HUDWindow);
            lbl.Position = new Microsoft.Xna.Framework.Point(x, yPos);

            x += distance;

            spMeanModifier = new Spinner(gui);
            Add(spMeanModifier);
            spMeanModifier.Init(Spinner.SpinnerType.HUD);
            spMeanModifier.Position = new Microsoft.Xna.Framework.Point(x, yPos);
            spMeanModifier.NoOfDigits = 3;
            spMeanModifier.Width = 60;

            x += distance;

        /*    spStandardDevModifier = new Spinner(gui);
            Add(spStandardDevModifier);
            spStandardDevModifier.Init(Spinner.SpinnerType.LCD);
            spStandardDevModifier.Position = new Microsoft.Xna.Framework.Point(x, yPos);
            spStandardDevModifier.NoOfDigits = 3;
            */

            yPos += 30;
            x = leftMargin;

            lbl = new Label(gui);
            Add(lbl);
            lbl.Text = "Min/max:";
            lbl.Init(Label.LabelType.HUDWindow);
            lbl.Position = new Microsoft.Xna.Framework.Point(x, yPos);

            x += distance;

            tbAbsoluteMin = new TextBox(gui);
            Add(tbAbsoluteMin);
            tbAbsoluteMin.Width = 55;
            tbAbsoluteMin.Position = new Point(x, yPos);
            
            tbAbsoluteMin.IsEditable = true;
            tbAbsoluteMin.IsNumericBox = true;
            tbAbsoluteMin.Init(TextBox.TextBoxType.HUD);

      /*      spAbsoluteMean = new Spinner(gui);
            Add(spAbsoluteMean);
            spAbsoluteMean.Init(Spinner.SpinnerType.LCD);
            spAbsoluteMean.Position = new Microsoft.Xna.Framework.Point(x, yPos);
            spAbsoluteMean.NoOfDigits = 4;
            */

            x += tbAbsoluteMin.Width + 8;

            tbAbsoluteMax = new TextBox(gui);
            Add(tbAbsoluteMax);
            tbAbsoluteMax.Width = 55;
            tbAbsoluteMax.Position = new Point(x, yPos);
            tbAbsoluteMax.Height = 27;
            tbAbsoluteMax.IsEditable = true;
            tbAbsoluteMax.IsNumericBox = true;
            tbAbsoluteMax.Init(TextBox.TextBoxType.HUD);

           /* spAbsoluteStandardDev = new Spinner(gui);
            Add(spAbsoluteStandardDev);
            spAbsoluteStandardDev.Init(Spinner.SpinnerType.LCD);
            spAbsoluteStandardDev.Position = new Microsoft.Xna.Framework.Point(x, yPos);
            spAbsoluteStandardDev.NoOfDigits = 4;
            */


            yPos += 30;

            x = leftMargin;

            TextButton btSave = new TextButton(gui);
            Add(btSave);
            btSave.Text = "Save";
            btSave.Init(TextButton.TextButtonType.HUD);
            btSave.Position = new Microsoft.Xna.Framework.Point(leftMargin, yPos);
            btSave.Click += new ClickHandler(btSave_Click);
           // btSave.Height = 20;
            btSave.ScaleWidthToFitText();

            btSave.DebugTag = "HUDhover";

            x += (btSave.Width + 8);

            TextButton btClear = new TextButton(gui);
            Add(btClear);
            btClear.Text = "Clear";
            btClear.Init(TextButton.TextButtonType.HUD);
            btClear.Position = new Microsoft.Xna.Framework.Point(x, yPos);
            btClear.Click += new ClickHandler(btClear_Click);
          //  btClear.Height = 20;
            btClear.ScaleWidthToFitText();

            x += (btClear.Width + 8);

            TextButton btClose = new TextButton(gui);
            Add(btClose);
            btClose.Text = "Close";
            btClose.Init(TextButton.TextButtonType.HUD);
            btClose.Position = new Microsoft.Xna.Framework.Point(x, yPos);
            btClose.Click += new ClickHandler(btClose_Click);
            //btClose.Height = 20;
            btClose.ScaleWidthToFitText();

        }

        void btClose_Click(UIComponent sender, EventArgs e)
        {
            DisplayWindow.Hide();
        }

        void btClear_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.SetTileResources.ClearResource(resourceType);
        }

        void btSave_Click(UIComponent sender, EventArgs e)
        {
            int? min = tbAbsoluteMin.GetNumberAsInt();
            int? max = tbAbsoluteMax.GetNumberAsInt();

            if (min.HasValue && max.HasValue)
            {
                if (min.Value > max.Value) // invalid input
                {
                    return;
                }
            }

            // save the changes
            The.InGameUI.SetTileResources.SaveResourceChanges(
                resourceType,
                min, max, spMeanModifier.Count //(0.01f * (float)spMeanModifier.Count)
                );

            DisplayWindow.Hide();
        }

       /* public void Save(Resources.ResourceType resourceType)//, Resource resourceData)
        {


        }*/

        public void Fill(UWGame.SimSide.Resources.ResourceType resourceType, Resource resourceData)
        {
            this.resourceType = resourceType;

            lblHeader.Text = resourceType.Name;

           /* if (resourceData != null)
            {
                this.resourceData = resourceData;
            }
            else
            {
                this.resourceData = new Resource();
            }*/

            // show percentages - 100 is default
            spMeanModifier.Count = (resourceData != null && resourceData.Modifier.HasValue ? resourceData.Modifier.Value : 100); //(int)(100f * (resourceData != null && resourceData.Modifier.HasValue ? resourceData.Modifier.Value : 1));

            //spStandardDevModifier.Count = (int)(100f * (this.resourceData.StandardDeviationModifier.HasValue ? this.resourceData.StandardDeviationModifier.Value : 100));

            tbAbsoluteMin.Text = (resourceData != null && resourceData.MinResourceItems.HasValue ? resourceData.MinResourceItems.Value.ToString("G") : "");
            tbAbsoluteMax.Text = (resourceData != null && resourceData.MaxResourceItems.HasValue ? resourceData.MaxResourceItems.Value.ToString("G") : "");

            // show the amounts in units of 1000:
          //  spAbsoluteMean.Count = (int)(1000f * (this.resourceData.AbsoluteMean.HasValue ? this.resourceData.AbsoluteMean.Value : 1000));

           // spAbsoluteStandardDev.Count = (int)(1000f * (this.resourceData.AbsoluteStandardDeviation.HasValue ? this.resourceData.AbsoluteStandardDeviation.Value : 1000));
        }

        //public void Save(Resourc)


    }
}
