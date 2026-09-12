using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Interface.Layout;
using WindowSystem;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data
{
    public class TutorialLoader
    {
        public static List<HelpTopic> Init()
        {
            List<HelpTopic> list = new List<HelpTopic>();
            #region Tutorial Island
            /////////////////////////Tutorial Island***************************************************///////////////////////////////
            #region tutorial 1
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial1",
                Name = "GUIDE #1 - Use the PPU",

                FlowElements = new LayoutElement[]
                     {


                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel(" GUIDE #1: ACTION ZONES",HelpTopic.ColorHeader) +"\n \n -By holding the left mouse button and dragging, you define a zone. \n -When you have defined a zone, you assign an action by selecting 'NEW', and choosing an action from the zone menu. \n \n"}
                         },
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + "\n \n -See archived messages by clicking this button at the right side of the screen:"}
                         },

                        new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_eventArchiveButton"}
                         }
  
                     }
            });
            #endregion
            #region Tutorial 2
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial2",
                Name = "GUIDE #2 SCOUT",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #2: SCOUT",HelpTopic.ColorHeader) + "\n \n -Scout the area north of the wreck by dragging a zone with the left mouse button, select NEW and click SCOUT. \n \n"}
                         },

                        new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_2b_scout"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + "\n \n -Pause/Unpause by clicking P or Space."}
                         }
                     }

            });
            #endregion
            #region Tutorial 2 a

            list.Add(new HelpTopic()
            {
                KeyName = "tutorial2a",
                Name = "GUIDE #2a: NAVIGATING THE MAP",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #2a: NAVIGATING THE MAP",HelpTopic.ColorHeader) + "\n \n -Hold and drag the right mouse button to navigate around the map. \n -You can also use WASD keys. \n \n"}
                         },
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////", HelpTopic.ColorItem) + "\n \n -See archived messages by clicking this button at the right side of the screen:"}
                         },

                        new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_eventArchiveButton"}
                         }


                     }

            });
            #endregion
            #region Tutorial 3

            list.Add(new HelpTopic()
            {
                KeyName = "tutorial3",
                Name = "GUIDE #3 SALVAGE WRECK",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #3 SALVAGE WRECK",HelpTopic.ColorHeader) + "\n \n -To designate a structure for salvaging, click the white 'Expand' arrow on the marker window. \n -Select 'SALVAGE' from the menu. \n \n"}
                         },
                        new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_3b_salvage"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + "\n \n -Pause/unpause by pressing P or Space. \nSpeed up time by clicking 2X or 4X in the top left corner of the screen."}
                         }, //
                        new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_3c_setSpeed"}
                         },
                     }

            });
            #endregion
            #region Tutorial 4
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial4",
                Name = "GUIDE #4 BUILD ROPE BRIDGE",

                FlowElements = new LayoutElement[]
                     {
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #4 BUILD ROPE BRIDGE",HelpTopic.ColorHeader) + " \n \n -Click the white arrow button next to the word Gorge, then click BEGIN to indicate to your group that the building task can begin. \n \n"}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_4a_buildBridge"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + " \n \n -Hide menus and icons by right-clicking on the game area. Display them again by left-clicking."}
                         },

                     }

            });
            #endregion
            #region Tutorial 5 (Commented out)


            /*               list.Add(new HelpTopic()
                           {
                               KeyName = "tutorial5",
                               Name = "GUIDE #5 SCOUT AREA (NORTH OF BRIDGE)",

                                FlowElements = new LayoutElement[]
                                {
                                    new LayoutElement()
                                    {
                                        Image = new ImageElement() { Image = "productionChain_watergun"}
                                    },
                                    new LayoutElement()
                                    {
                                        Text = new TextElement() { Text = " \nGUIDE #5 SCOUT AREA (NORTH OF BRIDGE) \n \nDrag a zone at the edge of the shrouded area. Select 'NEW' then 'SCOUT'."}
                                    }
                                }  

                           });
           */
            #endregion
            #region Tutorial 5
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial5",
                Name = "GUIDE #5 EXAMINE AREA TO FIND FOOD",

                FlowElements = new LayoutElement[]
                     {
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #5 EXAMINE AREA TO FIND FOOD",HelpTopic.ColorHeader) + "\n \n -Drag a zone as indicated in the image. Select 'NEW' then 'EXAMINE' \n -A camp member will then examine the area for hidden resources such as food. \n -If you suspect that some resources may have been missed, make a new zone and select EXAMINE again."}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_6_examine"}
                         },

                     }

            });

            #endregion
            #region Tutorial 6 a

            list.Add(new HelpTopic()
            {
                KeyName = "tutorial6a",
                Name = "GUIDE #6a GATHER 3 X COMMON OIL TUBERS",

                FlowElements = new LayoutElement[]
                     {
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #6a GATHER 3 X OIL TUBERS",HelpTopic.ColorHeader) + "\n \n -Drag a zone in the area where 'common oil tubers' were found. \n -Select 'NEW' then 'GATHER'. Find the new resource 'Common oil tubers' in the list and drag the adjacent slider to order 3 X Common oil tubers. \n -Click OK. \n \n"}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_7b_gatherOilTubers"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + "\n \n -If there's an insufficient number of oil tubers, make a new EXAMINE zone to search the area again and/or make a bigger GATHER zone."}
                         }
                     }

            });

            #endregion
            #region Tutorial 7
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial7",
                Name = "GUIDE #7 MOVE CAMP",

                FlowElements = new LayoutElement[]
                     {
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #7 MOVE CAMP",HelpTopic.ColorHeader) + "\n \n -The location of your colony is indicated with the marker window 'CAMP'. \n -This is where your group resides and stores supplies. \n -To move your camp: Click the arrow next to 'CAMP', select MOVE CAMP and click in the area north of the bridge. \n -This will tell your camp members to start moving supplies to the new site. \n \n"}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_7a1_moveCampMenu"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n \n"} //line break between images
                         },


                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_7a2_moveCampTarget"}
                         },
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + " \n \n -Moving your camp to an area with resources means you spend less time hauling."}
                         },


  

                     }
            });

            #endregion
            #region Tutorial 7 a
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial7a",
                Name = "GUIDE #7a GATHER MATERIALS: 3 X FIREWOOD, 1 X STONE",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #7a GATHER MATERIALS: 3 X FIREWOOD, 1 X STONE", HelpTopic.ColorHeader) + " \n \n -Drag a zone in the area north of the gorge. \n -Select 'NEW' then 'GATHER'. Find the materials in the list and drag the adjacent slider to order 3 X Firewood and 1 X Stone. Click OK. \n \n"}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_gather1Stone3Firewood"}
                         },
                        new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + " \n \n -If there's an insufficient number of resources, make a bigger GATHER zone."}
                         },




                     }
            });
            #endregion
            #region Tutorial 8

            list.Add(new HelpTopic()
            {
                KeyName = "tutorial8",
                Name = "GUIDE #8: BUILD FIREPLACE",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #8: BUILD CAMPFIRE",HelpTopic.ColorHeader) + " \n \n -To build a structure, open the Production Manager:"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_inventoryButton"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n -Find 'Campfire' and click 'BUILD' (the hammer button): \n"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_8_buildCampfireMenu_Small"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n -Hover over an open area in the terrain view with the ghosted structure image until it changes color, indicating that there's room in that spot. \n -Place the campfire by left-clicking in the terrain view."}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_8_buildCampfirePlacement"}
                         },

                     }

            });
            #endregion
            #region Tutorial 9
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial9",
                Name = "GUIDE #9: PRODUCE FOOD (3 X MASHED OIL TUBERS)",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #9: PRODUCE FOOD (3 X MASHED OIL TUBERS)",HelpTopic.ColorHeader) + " \n \n -Open the Production Manager by clicking this button found at the right side of the screen:"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_inventoryButton"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n -In the Production Manager, open the Prepared Food section and drag the slider that sits next to Mashed oil tubers to order 3 rounds of Mashed oil tubers. \n "}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_9_produceMashedOilTubers"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + " \n \n -Monitor tasks by opening the Task Manager (click this button:)"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_taskManagerButton"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n -For each task listed, expand the entry by clicking the arrow button to get more details about materials, tools and manpower for that particular task."}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_9_taskManagerExpand"}
                         },


                     }
            });
            #endregion
            #region Tutorial 10
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial10",
                Name = "GUIDE #10: SCOUT TO FIND ACCESS TO TREES",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #10: SCOUT TO FIND ACCESS TO TREES",HelpTopic.ColorHeader) + " \n \n -Make a SCOUT zone as shown in the image to explore the area north of the creek. Once the zone disappears, make another SCOUT zone further north, then another until the area is fully scouted."}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_10_scoutNorthCreek"}
                         }
                     }

            });
            #endregion
            #region Tutorial 11
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial11",
                Name = "GUIDE #11: CANCEL ALL ACTION ZONES",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #11: CANCEL ALL ACTION ZONES",HelpTopic.ColorHeader) + " \n \n -Cancel a zone by clicking the white arrow on the zone marker:"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_11_zoneMarker"} 
                         },

                        new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \nthen click the garbage can icon: (Cancel all of your zones this way)"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_11_cancelScout"} 
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + " \n \n -Many zones can also be cancelled in the Task Manager:"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_taskManagerButton"}
                         },
                     }
            });

            #endregion
            #region Tutorial 12 a
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial12a",
                Name = "GUIDE #12a: MAKE 3 SPEARS",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #12a: MAKE 3 SPEARS (Gather 3 X Flint, 3 X Water cane stems",HelpTopic.ColorHeader) + " \n \n -To make spears you need Flint and Water cane stems. \n -Locate water cane and make a GATHER zone. Drag the slider to order 3 X Water cane stems."}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_12a_gatherWatercane"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n -Locate flint, make a GATHER zone and drag the slider to order 3 X Flint."}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_12a_gatherFlint"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + "\n \n -If you see an insufficient number of resources, try making a larger GATHER zone or use EXAMINE again."}
                         },

                     }

            });
            #endregion
            #region Tutorial 12 c //NOT used in tut, simplified.

            list.Add(new HelpTopic()
            {
                KeyName = "tutorial12c", // NA was b
                Name = "GUIDE #12b: MAKE 3 SPEARS",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #12b: MAKE 3 SPEARS (Make 3 spearheads)",HelpTopic.ColorHeader) + " \n \n -Open the Production Manager by clicking this button:"}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_inventoryButton"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n -In the Production Manager, open the MATERIALS/COMPONENTS section and drag the slider that sits next to the word 'Spearhead (flint)' to order 3."}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_12b_orderSpearheads"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + "\n \n -If an Item type name is written in white or black it means your colony owns it. If it's YELLOW, it means you don't have it."}
                         },

                     }

            });
            #endregion
            #region Tutorial 12 b

            list.Add(new HelpTopic()
            {
                KeyName = "tutorial12b",
                Name = "GUIDE #12b: MAKE 3 SPEARS",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #12b: MAKE 3 SPEARS:",HelpTopic.ColorHeader) + " \n \n -In the Production Manager, open the WEAPONS section and drag the slider that sits next to the word 'Spear (flint-tipped)' to order 3."}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_12c_orderSpears"}
                         },
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text =  " \n-Click the item name Spear(flint-tipped) to open its pop-up. Click its Pin-button to keep it open as you RMB-click to close all other windows. Drag and place it where you want and resize it as needed. That way, you can keep track of the progress."}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_12d_SpearProdHUD"}
                         },
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + "\n \n -It takes time to make flint tools. Speed up time by clicking 4X in the top left corner of the screen. In the meantime, look at the info in the item's DATA section:"}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_12d_SpearDATAHUD"}
                         },


                     }

            });
            #endregion
            #region Tutorial 13

            list.Add(new HelpTopic()
            {
                KeyName = "tutorial13",
                Name = "GUIDE #13: SCOUT FOR BUSH DRAGONS",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #13: SCOUT FOR BUSH DRAGONS",HelpTopic.ColorHeader) + " \n \n -Make a scout zone as indicated in the image."}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_13_scoutBushDragons"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + "\n \n -If no bush dragon is found after scouting, make another SCOUT zone further up."}
                         },

                     }
            });
            #endregion
            #region Tutorial 14
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial14",
                Name = "GUIDE #14: KILL 3 BUSH DRAGONS",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #14: KILL 3 BUSH DRAGONS",HelpTopic.ColorHeader) + " \n \n Attack the bush dragons by dragging a zone into the bush dragon's territory (the zone should be around 10 X 10 tiles). Then select ATTACK and set the slider to 3 attackers as shown in the image:"}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_14_patrolBushDragons"}
                         },
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = " \n The ATTACK zone assigns armed camp members to engage any dangerous animal that enters the zone. If any of the 3 bush dragons escape, make another ATTACK zone on top of them and cancel the first one. Repeat until all 3 dragons are dead! \n \n"}
                         },
                         new LayoutElement() 
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + "\n \n The ATTACK zone will be automatically cancelled after a while when there are no more live bush dragons inside the zone."}
                         }
                     }
            });
            #endregion
            #region Tutorial 15
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial15",
                Name = "GUIDE #15: MOVE CAMP, BUILD SIGNAL PYRE",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #15: MOVE CAMP, BUILD SIGNAL PYRE",HelpTopic.ColorHeader) + " \n \n -Move camp by clicking the white arrow next to CAMP, select MOVE CAMP and click in the newly discovered area close to some trees. \n \n -GATHER 3 X Firewood and 1 X Spoak branches by making a GATHER zone. \n \n-Reopen the GATHER window, click on the word 'Spoak branches'. In the popup that appears, display PRODUCTION info by selecting the 'cogwheels' icon (as seen in the image below). \n-Scroll down or resize the window to see the section 'USED IN'. \n-Find 'Signal Pyre', click it and pin the pop-up window by clicking the 'Pin button'. \n \n-When the materials have been gathered, a Hammer button (Build) will appear in the 'Signal Pyre' window. Click the Hammer button and place the structure somewhere near the camp. \n \n"}
                         },

                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_15_signalPyreProdInfo"}
                         },

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + " \n \n -Remember to cancel any remaining ATTACK zones and other unimportant tasks to free up your workforce."}
                         },


                     }

            });
            #endregion
            #region Tutorial 16
            list.Add(new HelpTopic()
            {
                KeyName = "tutorial16",
                Name = "GUIDE #16: LIGHT SIGNAL PYRE",

                FlowElements = new LayoutElement[]
                     {

                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("GUIDE #16: LIGHT SIGNAL PYRE",HelpTopic.ColorHeader) + " \n \n -Select the signal pyre, click the arrow and select BEGIN \n \n"}
                         },
                         new LayoutElement()
                         {
                             Text = new TextElement() { Text = Label.ToLabel("HINTS:////////////////////////////////////////////////",HelpTopic.ColorItem) + " \n \n -To select a structure or entity, it can sometimes be necessary to drag a selection zone around it,  then repeatedly click the Cycle Entity button to select entities in the zone one after another:"}
                         },
                         new LayoutElement()
                         {
                             Image = new ImageElement() { Image = "tut_16_signalPyreCycleButton"}
                         },
                     }

            });
            #endregion
            #endregion

            return list;
        }

    }
}
