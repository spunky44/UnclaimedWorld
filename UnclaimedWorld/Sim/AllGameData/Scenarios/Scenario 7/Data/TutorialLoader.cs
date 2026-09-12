using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Interface.Layout;
using WindowSystem;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_7.Data
{
    public class TutorialLoader
    {
        public static List<HelpTopic> Init()
        {
            List<HelpTopic> list = new List<HelpTopic>();

         
            #region "tutorialMiningScenario7"
            list.Add(new HelpTopic()
            {
                KeyName = "tutorialMiningScenario7",
                Name = "ADVICE - Providing metals for Duke's Landing",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("ADVICE - PROVIDING METALS FOR DUKE's LANDING",HelpTopic.ColorHeader) +" \n \n-Locate the scandium deposit in the volcanic region to the southwest by using EXAMINE. Build a mine on it. \n \n-Extract scandium ore and set up the refiner. \n \n-See how much scandium is requested by Duke's Landing. \n(Open the World Map screen, Select 'Duke's Landing' and click the 'Willing to buy' button.) \n \n -Refine only the requested amount of scandium. Place the scandium in the trade area of the big Helipad building. \n \n-Order trade runs from Duke's Landing and sell scandium in return for weapons, provisions and what else your expedition needs. \n \n-Once you are sufficiently armed, move north into the muckroot biome, eliminate the swarmers and find the terbium deposit to start extracting / refining terbium."}
                         },
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("IMPORTANT NOTE:////////////////////////////////////////////////", HelpTopic.ColorItem) + "\n \n Duke's Landing only offers a limited amount of weapons and tools. Once you have bought the items on offer there will be NO additional items offered for sale, so use them wisely."}
                         },


                     }
            });
            #endregion




            return list;
        }

    }
}
