using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide
{
    public interface IGameDataCollection
    {
         void PostDataCompleteInitialize();
         void PostDataCompleteValidate(Dictionary<string, List<string>> allPostInitValidationErrors);
         void PreDataCompleteValidate(Dictionary<string, List<string>> allPostInitValidationErrors);

         int Order { get; }

         IGameData Get(string key);
    }
}
