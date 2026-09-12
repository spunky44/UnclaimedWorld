using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface;

namespace GameStateManagement
{
    
   
    public class TimelinePanel : HUDWindow
    {
        const int minHeight = 26;
        const int defaultHeight = 260;
      //  const int pagerHeight = 48;

        
        TextArea text;

        public const int Width = 360;

        public TimelinePanel(CommonInterface intf) // GUIManager gui) //int xPos, int yPos, int width, int height)
            : base(intf, Width, 500, true, false) // width, height, true, false, false, "HUD_window_base", false)
        {
            base.HideOnRightClick = false;

            DisplayWindow.CenterWindow();
            /*
            DisplayWindow.X = xPos;
            DisplayWindow.Y = 200;*/

            DisplayWindow.Resizable = false; // true;
         
            DisplayWindow.Level = Level.Bottom; //.RockBottom;
       
           
           // DisplayWindow.MinWidth = 120;
         //   DisplayWindow.MaxWidth = maxDisplayWindowWidth;
            
            DisplayWindow.Show();



            //PlacePageButtons();
            text = new TextArea(gui, ListBoxType.HUDAndLCD);
            DisplayWindow.Add(text);
            text.Init(Label.LabelType.HUDWindow); // TextArea.TextAreaType.HUD);
            text.CanGrowInHeight = false;

            text.Width = DisplayWindow.Width - 24;
            text.Height = DisplayWindow.Height - 24;
            text.X = 12;
            text.Y = 12;

            //    http://en.wikipedia.org/wiki/Antheia
            text.Text = "TIMELINE: \n \n2034 \nTelescopes detect animal life on Tau Ceti b, a nearby extrasolar planet. Plans for an exploration mission are drawn up, but are soon abandoned because of the immense cost. \n \n \n2080 \nA comet strikes the Atlantic Ocean, killing around 500 million in the subsequent natural disasters. \nAfter rebuilding, Earth's nations agree to prevent a possible extermination of humankind in the future. They decide to preserve the human species by starting a colony on Tau Ceti b, which has been dubbed Antheia. \n \n \n2080-2129 \nEarth's nations collaborate to construct a starship and develop new technologies such as hibernation and self-replicating machines. \n \n \n2130 \nBuilding of the starship Exigence is complete. The Earth celebrates its space pioneers: 2100 of the best and brightest have been chosen. They board the ship and enter hibernation as they begin a century long journey. \n \n \n2130-2238 \nTo eliminate risks of natural or man-made disasters, Earth gradually develops a high surveillance society. The Solar system is intensely monitored as well as each individual's thoughts and actions. The concept of labor loses its meaning as robots fulfil people's needs so they can live a life of leisure. \n \n \n2238 \nThe Exigence reaches Antheia. However, news of its arrival is received with indifference on Earth. During the long voyage the mission has lost its relevance and Earth's populations are immersed in virtual worlds. The pioneers realize that they are alone in facing the tasks ahead. \n \n \n \nThe PRECOL mission: \nAn exploration mission that took place before the main landing operation. A select group of 60 specialists examined the planet to find the most suitable place for the first colony. After they decided on a location, the 2040 remaining colonists were awakened to come down from orbit. But during the landing, the ground team was caught in the swarming event of a highly dangerous predator species. They were overwhelmed and either lost or scattered in the surroundings. Meanwhile, the arriving colonists were forced to land on a separate continent.";
            
        }
              
      

    }
}
