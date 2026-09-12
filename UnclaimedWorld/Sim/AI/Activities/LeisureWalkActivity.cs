using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
namespace UWGame.SimSide.AI.Activities
{
    public class LeisureWalkActivity: GroupMoveActivity
    {
        
        public LeisureWalkActivity(List<Activity> addToList, Vector3 destination) // Point destination)
            : base(addToList, destination)
        {
            
        }
    }
}
