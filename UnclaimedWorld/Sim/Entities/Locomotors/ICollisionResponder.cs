using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Collisions;

namespace UWGame.SimSide.Entities.Locomotors
{
   /* public enum ICollisionResponderID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }*/

    public interface ICollisionResponder //: ILookup<ICollisionResponder, ICollisionResponderID>
    {

        void BeginCollisionHandling();

        void HandleSingleCollision(Collidable<Entity> collidee);

        void EndCollisionHandling(List<Collidable<Entity>> collidees);
    }
}
