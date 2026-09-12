using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Renderables
{
    interface IUpdatable
    {
        double? GetUpdateInterval();

        void Update(GameTime gameTime);

    }
}
