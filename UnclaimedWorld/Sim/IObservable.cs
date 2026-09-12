using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide
{
    public delegate void ValueChanged();
    interface IObservable
    {        
        event ValueChanged ValueChangedEvent;

    }
}
