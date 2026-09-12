using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.SimSide;

namespace UWGame.ClientSide.Interface
{
    public class CounterPanel: Panel
    {
        Label lblCredits;
       
        Label lblMembers;
       
        //Label lblPopCap;

        public CounterPanel(int xPos)
            : base(The.InGameUI, null, new Microsoft.Xna.Framework.Point(xPos, 0),
             new Vector2(104, 59), WindowSystem.Level.Middle, PanelType.Counters)
        {
            Window.Show();

            int buttonYPos = 6;
            int buttonDistance = 4;

            ImageButton btCredits = new ImageButton(Interface.gui);
            btCredits.InitWithIcon(ImageButtonType.Counter, "counter_button_creditsIcon", true);
            Window.Add(btCredits);
            btCredits.X = 7;
            btCredits.Y = buttonYPos;
            btCredits.Enabled = false;

            ImageButton btMembers = new ImageButton(Interface.gui);
            btMembers.InitWithIcon(ImageButtonType.Counter, "counter_button_popIcon", true);
            Window.Add(btMembers);
            btMembers.X = btCredits.Right + buttonDistance;
            btMembers.Y = buttonYPos;
            btMembers.Enabled = false;

            int y = 28;

            Label.LabelType labelType = Label.LabelType.PlainPanelNormal;
           
            lblCredits = new Label(Interface.gui);
            Window.Add(lblCredits);
            lblCredits.Init(labelType);          
            lblCredits.FitToText();                
            lblCredits.Y = y;
            lblCredits.ToolTip = "Available trade credits";


            lblMembers = new Label(Interface.gui);
            Window.Add(lblMembers);
            lblMembers.Init(labelType);          
            lblMembers.Y = y;
            lblMembers.ToolTip = "Current members of the colony";
            //lblMembers.ToolTip = ""; 

          /*  lblPopCap = new Label(Interface.gui);
            Window.Add(lblPopCap);
            lblPopCap.Init(labelType);
            lblPopCap.Y = y;
            lblPopCap.ToolTip = "Maximum members - this is a fixed limit";
            */

            Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_top");
            Image dirtTop = AddImage(Interface.gui, Window, rect, new Point(50, -35));

            //Panel.AddDirtOnTopEdge(Interface.gui, Form, 0, false);

            RefreshLabels();

        }

        private void RefreshLabels()
        {
            decimal credits = The.InGameUI.UIAllegiance.TradeCredits ?? 0;

            lblCredits.Text = Common.MoneyAsString(credits, true); 
            lblCredits.FitToText();
            Window.CenterHorizontally(27, lblCredits);   

           /* lblMembers.Text = The.InGameUI.UIAllegiance.GetNoOfPersons().ToString();
            lblMembers.FitToText();
            Window.CenterHorizontally(68, lblMembers);                 
            */
            int members = The.InGameUI.UIAllegiance.GetNoOfPersons();
            lblMembers.Text = members.ToString() + "/" + GameData.Instance.Constants.PopulationCap;
            lblMembers.FitToText();
            Window.CenterHorizontally(68, lblMembers);

            StringBuilder text = new StringBuilder();
            Common.AppendHeaderOnLightBG(text, "Colony members");
            Common.Append(text, "Shows the current members and the maximum we can accept. (The current members do not want the community to grow too large, making their votes count less)");
            Common.AppendLine(text);
            Common.AppendDivider(text);
            Common.Append(text, "Current: ");
            Common.Append(text, members.ToString(), true);
            Common.AppendLine(text);
            Common.Append(text, "Maximum: ");
            Common.Append(text, GameData.Instance.Constants.PopulationCap.ToString(), true);
            lblMembers.ToolTip = text.ToString();

            if (The.InGameUI.UIAllegiance.IsOverPopulationCap())
            {
                lblMembers.NormalColor = UIComponent.errorColor;
            }
            else
            {
                lblMembers.NormalColor = lblMembers.GetNormalColorForType();
            }

        }

        public override void Refresh()
        {
            
            RefreshLabels();
        }
    }
}
