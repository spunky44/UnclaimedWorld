using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;

namespace UWGame.SimSide.Jobs
{
    public interface IRequiresWeapon
    {
        bool WeaponsAreAvailable { get; set; }

        ItemType.TaskType TaskType { get; }
    }
}
