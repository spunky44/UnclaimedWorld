using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.ClientSide.Hints;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.Control;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Screens.Loading
{
    /// <summary>
    /// is a window
    /// extend this with data driven hints, that cycle. Store the progress in a file..?
    /// </summary>
    public class HintPanel: HUDWindow //Window
    {
        TextArea taHint;

       // Window window;

        public HintPanel(CommonInterface intf)
            : base(intf, 600, 80)  //base(intf, intf.gui.ScreenWidth - 200, 90) 
        { 
            GUIManager gui = intf.gui;
           /*
            window = new Window(gui);
            window.X = 100;
            window.Y = 100;


            window.Width = gui.ScreenWidth - 200;
            window.Height = 90;
            window.Show();
            */
          /*  Width = gui.ScreenWidth - 200;
            Height = 90;*/
            

            taHint = new TextArea(gui, ListBoxType.HUDAndLCD); // .LCD);
            taHint.Init(Label.LabelType.HUDWindow); // LCDNormal);
            Add(taHint);
            taHint.X = correctedSideMargin;
            taHint.Y = correctedTopMargin;
            taHint.CanGrowInHeight = false;
            taHint.ScrollBarEnabled = false;
            taHint.Width = DisplayWindow.Width - 2 * taHint.X;
            taHint.Height = DisplayWindow.Height - correctedTopMargin - correctedBottomMargin; // bottomMargin;           
            taHint.DebugTag = "taHint";
           // taHint.Text = GetHintToDisplay();

        }


        public void ShowHint(Controller controller)
        {
            Hint hint = null;
            if (GameData.Instance.AllHints.Count > 0)
            {
                List<Hint> possibleHints = new List<Hint>();
                foreach (var item in GameData.Instance.AllHints)
                {
                    if (!controller.Progress.DisplayedHints.Contains(item.KeyName))
                    {
                        possibleHints.Add(item);
                    }
                }

                if (possibleHints.Count > 0)
                {
                    var ordered = possibleHints.OrderByDescending(h => h.Priority).ToList();

                    hint = ordered[0];

                    controller.Progress.DisplayedHints.Add(hint.KeyName);

                    try
                    {
                        controller.Progress.Write();
                    }
                    catch(Exception)
                    {

                    }
                }
                else
                {
                    hint = Common.GetRandomListMember(GameData.Instance.AllHints, controller.RandomGenerator);
                }
            }
           
            if (hint != null)
            {
                taHint.Text = hint.Text;
            }
        }


    }
}
