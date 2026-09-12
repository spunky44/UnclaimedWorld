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

namespace UWGame.ClientSide.MainMenu.Credits
{
    
   
    public class CreditsPanel : HUDWindow
    {
        const int minHeight = 26;
        const int defaultHeight = 260;
      //  const int pagerHeight = 48;

        
        TextArea text;
     

        public CreditsPanel(CommonInterface intf) // GUIManager gui) //int xPos, int yPos, int width, int height)
            : base(intf, 400, 600, true, true) // width, height, true, false, false, "HUD_window_base", false)
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
            text.X = 12;
            text.Y = TitleBarHeight; // 12;
            text.Width = DisplayWindow.Width - 24;
            text.Height = DisplayWindow.Height - text.Y - 12; // -24;
           

            // needs a space before newline character
            text.Text = "MADE BY REFACTORED GAMES \n/////////////////////////// \n \nLEAD PROGRAMMING: Lars Pedersen \n \nART DIRECTION: Morten Pedersen \n \nPROJECT MANAGEMENT ON PROTOTYPE: \nHans von Knut Skovfoged \n \nPROGRAMMING: Mark Lorenzen \n \nJUNIOR PROGRAMMER: Andreas Broqvist \n \n3D MODEL PIPELINE: Tobias Jacobsen \n \nINTERNS// \n \nMUSIC: Martin Hasseldam, Jesper Lundager \n \nPROGRAMMING: Finn Axel Simon Broman, Martin Juul Petersen, Jakob Sigvard, Olivér Árnits \n \nSCRIPTING: Thor-Bjørn Böhme, Benjamin Størup Olsen \n \n3D ART: Anchelika Skjødt \n \nGAME DESIGN: Stefan Ort Mortensen, Nicklas Andersen, Aleksander Fjellvang \n \nLEVEL DESIGN: Matteo Martinelli \n \nSOUND DESIGN: Morten Skouboe \n \nIN-HOUSE TESTING: Lukasz Maliglowka, Daniel Voss, Tommy Christensen \n \nTHANK YOU: Lau Korsgaard (GAME DESIGN), Chris Correia (VFX), Calvin Riedy, Samuel Blantz, Bjarke Larsen (WRITING), Carl Trägårdh (3D), Leo Claxton (SOUND), Gavin 'DrTssha' Craig and Daniel Pena \n \n";

       //     text.AddEntry("REFACTORED GAMES ARE:");
         //   text.AddEntry("");


            /*
            grid = new Grid(gui, ListBoxType.HUD, Label.LabelType.HUDWindow); // .Comm);
            //list.RenderType = RenderType.CRTAndLCD;          
            DisplayWindow.Add(grid);
            grid.Font = GUIManager.LCDandHUDFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grid.Color = Color.White;
          //  grid.Width = DisplayWindow.Width - 2 * singleSpacing;
            //grid.ItemHeight = 20;  
            grid.FixedItemHeights = false;
          //  grid.FixedItemHeights = false; // line breaks???
            SetListWidth();
            SetListHeight();
            grid.Position = new Point(10, 10); //new Point(display.X + 10, display.Y + 10);
            grid.ZOrder = 1.0f;
            grid.InsertNewRows = Grid.NewRowsInsertion.First;
           */
        }

        /*
        private void SetListHeight()
        {
            grid.Height = DisplayWindow.Height - 2 * grid.Y - 20; // -pagerHeight - messageGridYPos; //- 20;

        }

        private void SetListWidth()
        {
            grid.Width = DisplayWindow.Width - 2 * grid.X - 20; 
                        
        }*/

      
      

    }
}
