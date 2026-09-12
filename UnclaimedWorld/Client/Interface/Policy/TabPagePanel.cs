using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Policy
{
    public abstract class TabPagePanel: TabPage
    {
        public TabPagePanel(TabControl parent)
            : base(parent.guiManager)
        {

        }


        public virtual void Refresh()
        {

        }

    }
}
