using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;
using UWGame.ClientSide.Renderables;
namespace UWGame.SimSide
{
    /// <summary>
    /// used in sorting billboards, light sources etc.
    /// </summary>
    public interface ILocatable: IComparable
    {
        Vector3 Location { get; }
        
        Renderable AsRenderable { get; }

        RenderAsBillboard AsRenderAsBillboard { get; }


    }
}
