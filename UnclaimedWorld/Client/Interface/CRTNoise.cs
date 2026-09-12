using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface
{
    public class CRTNoise
    {
        // move noise stuff to new class, to be resued in Status Screen.
        public Animation2D turnOn, turnOff, switchChannel, switchChannelBlackFrame, interference, NoReception;
        
      /*  private static CRTNoise instance;
        public static CRTNoise Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CRTNoise();
                }
                return instance;
            }
        }*/


        public CRTNoise(GUIManager gui)
        {
            turnOn = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.17f, false);
            turnOn.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame1")));
            turnOn.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame2")) /*{ Color = Animation2D.halfTransp }*/);
            turnOn.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame3")) /*{ Color = Animation2D.halfTransp }*/);
            turnOn.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame3")) { Color = Animation2D.transp });

            turnOff = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.1f, false);
            turnOff.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame3")) { Color = Animation2D.halfTransp });
            turnOff.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame2")) { Color = Animation2D.halfTransp });
            turnOff.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame1")));
           // turnOff.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("CRT-turnon-frame1")) { Color = new Color(0.5f, 0.5f, 0.5f, 1f) }); // xna 3
            turnOff.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame1")) { Color = Color.FromNonPremultiplied(new Vector4(0.5f, 0.5f, 0.5f, 1f)) });
            turnOff.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame1")) { Color = Animation2D.black }); // black


            switchChannel = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, false);

            /* xna 3
            switchChannelBlackFrame = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, false);
            switchChannelBlackFrame.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame1")) { Color = new Color(0f, 0f, 0f, 1f) });

            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame1")));
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame2")));
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame3")));
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame4")) { Color = new Color(1f, 1f, 1f, 0.67f) });
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame5")) { Color = new Color(1f, 1f, 1f, 0.5f) });
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame6")) { Color = new Color(1f, 1f, 1f, 0.27f) });
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame6")) { Color = new Color(1f, 1f, 1f, 0f) });

            interference = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, false);
            interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame4")) { Color = new Color(1f, 1f, 1f, 0.67f) });
            interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame5")) { Color = new Color(1f, 1f, 1f, 0.5f) });
            interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame6")) { Color = new Color(1f, 1f, 1f, 0.27f) });
            interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame6")) { Color = new Color(1f, 1f, 1f, 0f) });
            */

            // xna 4
            switchChannelBlackFrame = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, false);
            switchChannelBlackFrame.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame1")) { Color = Color.FromNonPremultiplied(new Vector4(0f, 0f, 0f, 1f)) });

            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame1")));
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame2")));
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame3")));
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame4")) { Color = new Color(0.67f, 0.67f, 0.67f, 0.67f) });
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame5")) { Color = new Color(0.5f, 0.5f, 0.5f, 0.5f) });
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6")) { Color = new Color(0.27f, 0.27f, 0.27f, 0.27f) });
            switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6")) { Color = new Color(0f, 0f, 0f, 0f) });

            interference = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, false);
            interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame4")) { Color = new Color(0.67f, 0.67f, 0.67f, 0.67f) });
            interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame5")) { Color = new Color(0.5f, 0.5f, 0.5f, 0.5f) });
            interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6")) { Color = new Color(0.27f, 0.27f, 0.27f, 0.27f) });
            interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6")) { Color = new Color(0f, 0f, 0f, 0f) });

            NoReception = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.05f, true);
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame1")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame1")));
            //  NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("no_reception_frame1")){ Color = new Color(0f, 0f, 0f, 1f) });
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame2")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame2")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame3")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame3")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame4")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame4")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame5")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame5")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame6")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame6")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7")));
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7")));
         //   NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("no_reception_frame7")) { Color = new Color(0f, 0f, 0f, 1f) }); // xna 3
            NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7")) { Color = Color.FromNonPremultiplied(new Vector4(0f, 0f, 0f, 1f)) });
            
            
        }

    }
}
