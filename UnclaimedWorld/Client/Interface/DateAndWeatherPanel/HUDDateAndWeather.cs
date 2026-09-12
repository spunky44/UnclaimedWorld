using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.ClientSide.Interface.HUD_Windows;

namespace UWGame.ClientSide.Interface.DateAndWeatherPanel
{
    public class HUDDateAndWeather: HUDWindow
    {
        
        Label time, /*month,*/ season, year, tempNow, tempAhead, weatherNow, weatherAhead;
        Color headerColor = new Color(0xE2, 0xE2, 0xE2);

        Color weatherNowColor = Common.ColorFromHex("4BE7CF");
        Color weatherLaterColor = Common.ColorFromHex("22B89D");

        UIComponent surface; //the hud window extends beyond the white edge to include glowing air

      //  public const int Width = 368;

        public void SetWeatherNow(string cloudCover, string wind)
        {
            weatherNow.Text = string.Format("{0}/{1}", cloudCover, wind);
        }
        
        public void SetWeatherAhead(string cloudCover, string wind)
        {
            weatherAhead.Text = string.Format("{0}/{1}", cloudCover, wind);
        }

        public void SetTemp(int temp)
        {
            this.tempNow.Text = string.Format("{0}° C", temp.ToString());
        }

        public void SetTime(string timePhrase)
        {
            this.time.Text = timePhrase;
        }

       /* public void SetMonth(int monthNo)
        {
            this.month.Text = "Month " + monthNo.ToString();
        }*/

       /* public void SetYear(double year) //  int yearNo)
        {
            this.year.Text = "Year " + year.ToString("N1"); // yearNo.ToString();
        }*/
        public void SetDate(string date) 
        {
            this.year.Text = date; 
        }

        public void SetSeason(string seasonPhrase)
        {
            this.season.Text = seasonPhrase;
        }

        public HUDDateAndWeather(int xPos, int width) //, int displayWindowWidth)
            : base(width, 72)
        {         
            GUIManager gui = The.InGameUI.gui;
           
            HideOnRightClick = false;
                  
            DisplayWindow.X = xPos;
            DisplayWindow.Y = 4;

            DisplayWindow.Resizable = false; // true;
            DisplayWindow.SetResizableArea(ResizeAreas.Bottom, true);
            DisplayWindow.SetResizableArea(ResizeAreas.Right, true);
            DisplayWindow.SetResizableArea(ResizeAreas.BottomRight, true);

            DisplayWindow.ResizableBorderSize = doubleSpacing;

            DisplayWindow.Resize += new ResizeHandler(DisplayWindow_Resize);

            DisplayWindow.Level = Level.Bottom; //.RockBottom;
            /*    DisplayWindow.SetResizableArea(ResizeAreas.TopLeft, false);
                DisplayWindow.SetResizableArea(ResizeAreas.Left, false);
                DisplayWindow.SetResizableArea(ResizeAreas.Bottom, false);
                DisplayWindow.SetResizableArea(ResizeAreas.BottomLeft, false);
                DisplayWindow.SetResizableArea(ResizeAreas.BottomRight, false);
                */

           
            DisplayWindow.MinHeight = (int)34;
            DisplayWindow.MinWidth = 60;

            DisplayWindow.MaxWidth = width;
            DisplayWindow.MaxHeight = DisplayWindow.Height;
         
            DisplayWindow.Show();

            surface = new UIComponent(gui);
            Add(surface);
            surface.X = 0;
            surface.Y = 0;
            
            //Morten wanted this discplayscreen to be minimized
            DisplayWindow.Height = 34;
          
          /*  Label header = new Label(gui);
            Add(header);
            header.Text = "TIME";
            header.Init(Label.LabelType.PlainPanelSmall);
            header.Color = headerColor; // Color.White;
            header.Position = new Point(16, headerY);
            header.RenderType = RenderType.Overlay;
            */
            time = new Label(gui);
            surface.Add(time);
            time.Text = "Noon";
            time.Init(Label.LabelType.HUDWindow);
            time.Position = new Point(correctedSideMargin, 9);
                    
          /*  month = new Label(gui);
            surface.Add(month);
            month.Text = "Month 1";
            month.Init(Label.LabelType.HUDWindow);
            month.Position = new Point(112, time.Y); // dataBigY);
                   */
  
            season = new Label(gui);
            surface.Add(season);
            season.Text = "Start of spring";
            season.Init(Label.LabelType.HUDWindow);
            season.Position = new Point(112/* 201*/, time.Y);
                    
            year = new Label(gui);
            surface.Add(year);
           // year.Text = "Year 40";
            year.Init(Label.LabelType.HUDWindow);
            year.Position = new Point(220 /*313*/, time.Y); // dataBigY);

            Bar divider = new Bar(gui);
            surface.Add(divider);
            divider.EdgeSize = 1;
            divider.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle("HUD_line_horizontal"));
            divider.Height = 1;
            divider.Width = 352;
            divider.X = correctedSideMargin;
            divider.Y = time.Bottom + 1;

            //*************** weather part **************
                        
           
            // use an image for the degrees symbol:
        /*    Image imDegreeChar = Panel.AddImage(gui, displayWindow, "date_weather_degreechar", 
                new Point(temperatureRecessEdge.X + temperatureRecessEdge.Width - 17, dataY)); // dataBigY));
            imDegreeChar.RenderType = RenderType.Overlay;
          */


            Label lblNow = new Label(gui);
            surface.Add(lblNow);
            lblNow.Text = "WEATHER NOW:";
            lblNow.Init(Label.LabelType.HUDWindow);
            lblNow.NormalColor = weatherNowColor;
            lblNow.Position = new Point(correctedSideMargin, 28);
           
            Label lblAhead = new Label(gui);
            surface.Add(lblAhead);
            lblAhead.Text = "WEATHER AHEAD:";
            lblAhead.Init(Label.LabelType.HUDWindow);
            lblAhead.NormalColor = weatherLaterColor;
            lblAhead.Position = new Point(correctedSideMargin, 46);


            tempNow = new Label(gui);
            surface.Add(tempNow);
            tempNow.Text = "17 C"; // 32° C // ALT 248
            tempNow.Init(Label.LabelType.HUDWindow);
            tempNow.Position = new Point(142, lblNow.Y);
            tempNow.NormalColor = weatherNowColor;

            tempAhead = new Label(gui);
            surface.Add(tempAhead);
            tempAhead.Text = "13 C"; // 32° C // ALT 248
            tempAhead.Init(Label.LabelType.HUDWindow);
            tempAhead.Position = new Point(tempNow.X, lblAhead.Y);
            tempAhead.NormalColor = weatherLaterColor;

            weatherNow = new Label(gui);
            surface.Add(weatherNow);
            weatherNow.Text = "Cloudy/Light breeze"; //"%&/()=?"; // 
            weatherNow.Init(Label.LabelType.HUDWindow);
            weatherNow.Position = new Point(177, lblNow.Y);
            weatherNow.NormalColor = weatherNowColor;

            weatherAhead = new Label(gui);
            surface.Add(weatherAhead);
            weatherAhead.Text = "Cloudy/Windy"; // "'aáéÈèüä"; // 
            weatherAhead.Init(Label.LabelType.HUDWindow);
            weatherAhead.Position = new Point(weatherNow.X, lblAhead.Y);
            weatherAhead.NormalColor = weatherLaterColor;

            UpdateSurface();

            Show();
        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            UpdateSurface();
        }


        private void UpdateSurface()
        {
            surface.Width = DisplayWindow.Width - 6; // 2 * correctedSideMargin;
            surface.Height = DisplayWindow.Height - 9; // 2 * correctedTopMargin;
        }


        public void Show()
        {
            //minimizedWindow.Hide();
          // use F2 to show again...
            //buttonWindow.Show();
        }

        void closeButton_Click(UIComponent sender, EventArgs e)
        {
           
           // buttonWindow.Hide();
        }
    }
}

