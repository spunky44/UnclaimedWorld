using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities
{
    public interface IHasCategory<T> where T : ICategoryType
    {
        string Name { get; set; }
        T Category { get; set; }

        
    }
}
