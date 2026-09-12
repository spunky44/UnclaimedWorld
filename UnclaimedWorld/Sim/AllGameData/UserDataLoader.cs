using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.AllGameData
{
    /// <summary>
    /// for user scenarios. 
    /// this class can only deserialize...
    /// </summary>
    public class UserDataLoader: DataLoader
    {
        public UserDataLoader(): base(Config.DataType.UserScenarios, 0.1f)
        {

        }

    }
}
