using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems
{
    /// <summary>
    /// includes the object, its location and a pointer to its node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PointTreeDweller<T>
    {
        public PointQuadTreeNode<T> ContainingNode;
        public Pair<T, Vector2> ObjectAndPosition;


    }
}
