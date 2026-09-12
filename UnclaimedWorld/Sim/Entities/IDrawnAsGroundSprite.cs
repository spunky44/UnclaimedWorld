using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;
using UWGame.ClientSide.Map;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Entities
{
    interface IDrawnAsGroundSprite
    {
        void CopyQuadToVertexBuffer(VertexGroundFeature[] featureVertices, ref int index, Renderable.AdditionalEffect? overridingEffectID = null);
    }
}
