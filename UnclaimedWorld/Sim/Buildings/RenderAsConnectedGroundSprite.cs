using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables; // TODO DECOUPLE

namespace UWGame.SimSide.Buildings
{
    public class RenderAsConnectedGroundSprite : RenderAsBase
    {
        public bool IsRenderedAsConnection = false;

        public RenderAsConnectedGroundSprite(Entity parent, Renderable renderable):base(parent, renderable)  
        {
        }

        public RenderAsConnectedGroundSprite(RenderAsConnectedGroundSprite original, Renderable renderable)
            : base(null, renderable)
        {
            
        }
    }
}
