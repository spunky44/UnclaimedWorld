using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class ToolGrid
    {
       // public Label Header;
        public UIComponent HeaderRow;

        public Grid Grid;

        public int ScrollPosition = 0;

        public ImageButton btScrollDown, btScrollUp;

        public bool ScrollButtonsAreShown = true;

    }
}
