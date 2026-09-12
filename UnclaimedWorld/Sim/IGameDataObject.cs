using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide
{
    public interface IGameDataObject
    {

        void Initialize();

        void PostDataCompleteInitialize();

    }
}
