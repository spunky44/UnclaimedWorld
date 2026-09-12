using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities // TODO DECOUPLE - IANimatedModel, IDrawnAsGroundSPrite, IHasIcon should all move to CLientSide
{
    public interface IHasIcon
    {
        string IconSpriteName { get; set; } 

    }
}
