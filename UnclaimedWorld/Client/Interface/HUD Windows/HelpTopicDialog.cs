using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Interface.Layout;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class HelpTopicDialog: HUDWindow
    {
      //  TextArea area;
       // Image image;


        const int minWidth = 420;
        const int minHeight = 400;

        UIComponent listSurface;

        Grid surfaceGrid;
        const int surfaceHeight = 354;

     /*   public string Text
        {
            set
            {
                area.Text = value;
            }
        }*/

       /* public string Image
        {
            set
            {
                image.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle(value));
                image.ResizeControlToFitImage();
            }
        }*/

       

        public HelpTopicDialog(HelpTopic helpTopic)
            : base(minWidth, minHeight, true, true, true)
        {
            DisplayWindow.SetResizableArea(ResizeAreas.Top, true);
            DisplayWindow.SetResizableArea(ResizeAreas.Bottom, true);

            DisplayWindow.MinHeight = 40; // minHeight; // 200;
            DisplayWindow.ResizableBorderSize = 6;
            DisplayWindow.Resize += DisplayWindow_Resize;


            HideOnRightClick = false;


            CreateSurfaceWithScrollbar(out surfaceGrid, out listSurface, surfaceHeight, TitleBarHeight, doubleSpacing, false);
                      
            DisplayWindow.Level = Level.Dialogs;

            Populate(helpTopic.FlowElements);

            SetVerticalPositions();

        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            SetVerticalPositions();
        }

        private void SetVerticalPositions()
        {

           // listSurface.Height = surfaceHeight.HasValue ? surfaceHeight.Value : DisplayWindow.ViewPort.Height - listSurface.Y;
           listSurface.Height = DisplayWindow.ViewPort.Height - listSurface.Y - 10;
           surfaceGrid.Height = listSurface.Height;
            
        }

        void bt_Click(UIComponent sender, EventArgs e)
        {
            Hide();
        }

        const int borderWidth = 2;

        public void Populate(LayoutElement[] flowLayoutElements)
        {
            surfaceGrid.BeginAddingEntries();

            surfaceGrid.Clear();

            foreach (var item in flowLayoutElements)
            {
                if (item.Text != null)
                {                   

                    TextArea area = new TextArea(gui, ListBoxType.HUDAndLCD);
                    area.Init(Label.LabelType.HUDWindow);

                    area.CanGrowInHeight = true; // don't scroll or clamp
                    surfaceGrid.AddEntry(area, area); // add calls initialize, that sets font...

                    area.Text = item.Text.Text;
                }
                else if (item.Image != null)
                {
                    UIComponent imageWithBorder = new UIComponent(gui);

                    Image image = new WindowSystem.Image(gui);
//                    surfaceGrid.AddEntry(image, image);
                    imageWithBorder.Add(image);
                    image.X = borderWidth;
                    image.Y = borderWidth;
                    image.CanHaveFocus = false;

                    image.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle(item.Image.Image));
                    image.Texture = gui.GUISpriteSheet.Texture;
                    image.ResizeControlToFitImage();
                                       
                    
                   // surfaceGrid.CenterChildHorizontallyInViewport(image);
                    //image.X -= listSurface.X / 2;
                   // DisplayWindow.CenterChildHorizontally(image);

                    Box border = new Box(gui);
                    border.SetSkinLocation(0, gui.GUISpriteSheet.GetSourceRectangle("HUD_border"));
                    border.CornerSize = 3;
                    imageWithBorder.Add(border);
                    border.Width = image.Width + 2 * borderWidth;
                    border.Height = image.Height + 2 * borderWidth;

                    imageWithBorder.Width = border.Width;
                    imageWithBorder.Height = border.Height;

                    surfaceGrid.AddEntry(image, imageWithBorder);

                    // align the image and text nicely 
                    surfaceGrid.CenterChildHorizontallyInViewport(imageWithBorder);
                    imageWithBorder.X -= listSurface.X / 2;
                }
            }


            surfaceGrid.EndAddingEntries();

            // HACK: do the final line breaking on the text areas here. 
            // It caused a stack overflow when I tried to override the UIComponent.Width property in TextArea...
            foreach (var item in surfaceGrid.Entries) 
            {
                TextArea ta = item as TextArea;
                if (ta != null)
                    ta.RefreshText();
            }
        }


      /*  public void ShowImageAndText(string imageName, string text)
        {
            image.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle(imageName));
            image.Texture = gui.GUISpriteSheet.Texture;
            image.ResizeControlToFitImage();

            // align the image and text nicely
            DisplayWindow.CenterChildHorizontally(image);

            //scale window after image (with a minimum)
          //  DisplayWindow.Width = Math.Max(minWidth, image.Width + 2 * doubleSpacing);
           
           // area.Width = DisplayWindow.Width - 2 * doubleSpacing;
            area.Text = text;
            
        }*/

      /*  public void ShowText(string text)
        {
            Remove(image);

          //  area.Width = minWidth - 2 * doubleSpacing;
            area.Text = text;
           
            //scale window after text (with a minimum)
           // DisplayWindow.Height = Math.Max(minHeight, area.Height + 2 * doubleSpacing);                       
        }*/

        public override void Hide()
        {
            DisplayWindow.Hide();            
        }

       
    }
}
