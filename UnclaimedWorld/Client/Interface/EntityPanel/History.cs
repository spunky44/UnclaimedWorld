using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;

namespace UWGame.ClientSide.Interface.EntityPanel
{
    public class History : EntityPanelTabPage
    {

        public History(GUIManager gui, EntityPanel entityPanel, UIComponent fullLCD, UIComponent halfLCD)
            : base(gui, entityPanel, false, fullLCD, halfLCD)            
        {
            Title = "HISTORY";
        }
    }
}
