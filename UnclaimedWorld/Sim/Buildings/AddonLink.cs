using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Buildings
{
    public enum Size { Small, Big }
    public class AddonLink
    {
        public IAddon Addon;
        public int SlotIndex;
        public Size Size;
    }
}
