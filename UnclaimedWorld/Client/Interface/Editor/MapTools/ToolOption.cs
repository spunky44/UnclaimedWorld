using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Editor.MapTools
{
    public abstract class ToolOption: UIComponent
    {
        public abstract int Order { get; }



        public ToolOption(GUIManager gui): base(gui)
        {


        }

    }
}
