using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Interface.Layout;
using WindowSystem;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data
{
    public class TutorialLoader
    {
        public static List<HelpTopic> Init()
        {
            List<HelpTopic> list = new List<HelpTopic>();

         
            #region "tutorialRubberScenario5_1"
            list.Add(new HelpTopic()
            {
                KeyName = "tutorialRubberScenario5_1",
                Name = "GUIDE #1 - Hire a barge",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #1 - HIRE A BARGE",HelpTopic.ColorHeader) +"\n \nFirst, click the MISSIONS button:"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "comm_button_out"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n-Click 'NEW RUN' \n-then click the Globe button next to START:"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tutRubber_startRun"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n-Click the Eden Plains button, \n-then SELECT Eden Plains:"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tutRubber_selectEdenPlains"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n \nBack on the CREATE NEW RUN screen, \n-click the globe button next to DESTINATION. \n-Click Headway, \n-then SELECT Headway \n \n-Now select Barge (Hired) from the drop-down menu next to TRANSPORT. \n \n-Next, select ADD ACTION, \n-then click BUY \n-Find 'Extrusion machine components' and drag the slider to order 1. \n-Find 'Sulfur powder' and order all of it. \n-Click OK. \n \nBack on the CREATE NEW RUN screen, \n-click ADD ACTION \n-Click EMBARK \n-Find the chemists called Neson in the personnel list and check as many of them as are willing to join. \n-Click 'Start Run'"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tutRubber_createRun"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n \nOn the MISSIONS screen, you can see ETA (estimated time of arrival) for the Barge. \n \nYou can also click the WORLD MAP screen to see how far the barge has come on its journey:"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tutRubber_bargeMap"}
                         },

                     }
            });
            #endregion
            #region "tutorialRubberScenario5_2"
            list.Add(new HelpTopic()
            {
                KeyName = "tutorialRubberScenario5_2",
                Name = "GUIDE #2 - Build and upgrade workshop. Produce Rubber parts.",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #2a - BUILD GENERAL WORKSHOP",HelpTopic.ColorHeader) }
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tutRubber_polymerWorkshopTooltip"}
                         },

                         new LayoutElement()
                         {
                           //  Text = new TextElement() { Text = " \n-Open the Production Manager \n-Under MATERIALS/COMPONENTS, find 'Extrusion machine' and click the name to open the production info \nScroll down to USED IN: and click 'Polymer workshop'. The missing materials, tool and skill will be indicated with yellow text. Acquire these resources. \n-When all of it is acquired, find 'Polymer workshop' under the STORAGE/PRODUCTION section. A BUILD button willl appear, allowing you to place the Polymer workshop."}
                             Text = new TextElement() { Text = " \n-Open the Production Manager \n-Under STORAGE/PRODUCTION, find 'Workshop building' and click the name to open the Production pop-up. Click the 'Pin' button to keep this window open and place it where you want. \n-The missing materials, tool and skill will be indicated with yellow text. Acquire these resources. \n(If 'Workshop building' does not appear in the Production Manager, use the 'ATTAINABLE' or the 'ENCYCLOPEDIA' buttons at the top of the Production panel.) \n-When all the needed input is acquired, a BUILD button will appear and you can place the workshop."}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #2b - UPGRADE WORKSHOP",HelpTopic.ColorHeader) }
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tutRubber_upgradeWorkshop"}
                         },

                         new LayoutElement()
                         {
                           //  Text = new TextElement() { Text = " \n-Open the Production Manager \n-Under MATERIALS/COMPONENTS, find 'Extrusion machine' and click the name to open the production info \nScroll down to USED IN: and click 'Polymer workshop'. The missing materials, tool and skill will be indicated with yellow text. Acquire these resources. \n-When all of it is acquired, find 'Polymer workshop' under the STORAGE/PRODUCTION section. A BUILD button willl appear, allowing you to place the Polymer workshop."}
                             Text = new TextElement() { Text = " \n-Select the workshop building. From the action menu, click 'UPGRADE'. Expand the 'WORKSHOPS' category by clicking it. Then click the checkbox for 'Polymer workshop'. This will start the upgrade task as soon as the materials are ready. Click the name to open the production info. The missing materials, tool and skill will be indicated with yellow text. Acquire these resources so the upgrade can be completed."}
                         },
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #2c - PRODUCE RUBBER PARTS",HelpTopic.ColorHeader) }
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tutRubber_rubberPartsTooltip"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n-Open the Production Manager \n-Under MATERIALS/COMPONENTS, find 'Marshcot sap' and click the name. Scroll down to the section 'USED IN' and click 'Rubber parts'. Click the 'Pin' button to keep this window open and place it where you want. \n -The needed materials, tool and skill for making a batch of Rubber parts will be indicated with yellow text. Acquire these resources. (The marshcot sap is found in the swamp by using the EXAMINE command.) \n-When you have got all of it, a slider will appear and you can order a batch of Rubber parts."}
                         },       


                     }
            });
            #endregion

            #region "tutorialRubberScenario5_3"
            list.Add(new HelpTopic()
            {
                KeyName = "tutorialRubberScenario5_3",
                Name = "GUIDE #3 - Sell Rubber parts",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #3 - SELL RUBBER PARTS",HelpTopic.ColorHeader) }
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tutRubber_portRubberParts"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = "When you have made several batches of Rubber parts, it is time to start selling them. \n-Click on the port \n-Click TRADE \n-Find 'Rubber parts' and check the box. Click OK. \n-The townspeople will then place any Rubber parts you have in the trade area of the port. Once this has happened, \n-Click the MISSIONS button:"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "comm_button_out"}
                         },                     


                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n-Order a barge FROM Zenig Station TO Headway. Use the interface to sell all your Rubber parts and buy a supply of Sulfur powder. Buy whatever else that helps to increase the town's ratings towards the goal which was presented in the beginning of the game (find this by flipping through the Event archive). \n \nThis concludes the tutorial! Good luck! \n \nHINT: The barge costs credits, but this price is deducted from the value of your goods. So it is possible to pay for a barge without having any credits as long as your trade goods have sufficient value."}
                         },

     

                     }
            });
            #endregion



            return list;
        }

    }
}
